// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

#pragma warning disable IDE0022 // Use block body for methods
// ReSharper disable SuggestBaseTypeForParameter
namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqlServerTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 300;

        private static string CurrentDirectory
            => Environment.CurrentDirectory;

        public static SqlServerTestStore GetNorthwindStore()
            => (SqlServerTestStore)SqlServerNorthwindTestStoreFactory.Instance
                .GetOrCreate(SqlServerNorthwindTestStoreFactory.Name).Initialize(null, (Func<DbContext>)null);

        public static SqlServerTestStore GetOrCreate(string name)
            => new(name);

        public static SqlServerTestStore GetOrCreateInitialized(string name)
            => new SqlServerTestStore(name).InitializeSqlServer(null, (Func<DbContext>)null, null);

        public static SqlServerTestStore GetOrCreate(string name, string scriptPath, bool? multipleActiveResultSets = null)
            => new(name, scriptPath: scriptPath, multipleActiveResultSets: multipleActiveResultSets);

        public static SqlServerTestStore Create(string name, bool useFileName = false)
            => new(name, useFileName, shared: false);

        public static SqlServerTestStore CreateInitialized(string name, bool useFileName = false, bool? multipleActiveResultSets = null)
            => new SqlServerTestStore(name, useFileName, shared: false, multipleActiveResultSets: multipleActiveResultSets)
                .InitializeSqlServer(null, (Func<DbContext>)null, null);

        private readonly string _fileName;
        private readonly string _scriptPath;

        private SqlServerTestStore(
            string name,
            bool useFileName = false,
            bool? multipleActiveResultSets = null,
            string scriptPath = null,
            bool shared = true)
            : base(name, shared)
        {
            if (useFileName)
            {
                _fileName = Path.Combine(CurrentDirectory, name + ".mdf");
            }

            if (scriptPath != null)
            {
                _scriptPath = Path.Combine(Path.GetDirectoryName(typeof(SqlServerTestStore).Assembly.Location), scriptPath);
            }

            ConnectionString = CreateConnectionString(Name, _fileName, multipleActiveResultSets);
            Connection = new SqlConnection(ConnectionString);
        }

        public SqlServerTestStore InitializeSqlServer(
            IServiceProvider serviceProvider,
            Func<DbContext> createContext,
            Action<DbContext> seed)
            => (SqlServerTestStore)Initialize(serviceProvider, createContext, seed);

        public SqlServerTestStore InitializeSqlServer(
            IServiceProvider serviceProvider,
            Func<SqlServerTestStore, DbContext> createContext,
            Action<DbContext> seed)
            => InitializeSqlServer(serviceProvider, () => createContext(this), seed);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
        {
            if (CreateDatabase(clean))
            {
                if (_scriptPath != null)
                {
                    ExecuteScript(_scriptPath);
                }
                else
                {
                    using var context = createContext();
                    context.Database.EnsureCreatedResiliently();
                    seed?.Invoke(context);
                }
            }
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder
                .UseSqlServer(Connection, b => b.ApplyConfiguration())
                .ConfigureWarnings(b => b.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));

        private bool CreateDatabase(Action<DbContext> clean)
        {
            using (var master = new SqlConnection(CreateConnectionString("master", fileName: null, multipleActiveResultSets: false)))
            {
                if (ExecuteScalar<int>(master, $"SELECT COUNT(*) FROM sys.databases WHERE name = N'{Name}'") > 0)
                {
                    // Only reseed scripted databases during CI runs
                    if (_scriptPath != null
                        && !TestEnvironment.IsCI)
                    {
                        return false;
                    }

                    if (_fileName == null)
                    {
                        using var context = new DbContext(
                            AddProviderOptions(
                                    new DbContextOptionsBuilder()
                                        .EnableServiceProviderCaching(false))
                                .Options);
                        clean?.Invoke(context);
                        Clean(context);
                        return true;
                    }

                    // Delete the database to ensure it's recreated with the correct file path
                    DeleteDatabase();
                }

                ExecuteNonQuery(master, GetCreateDatabaseStatement(Name, _fileName));
                WaitForExists((SqlConnection)Connection);
            }

            return true;
        }

        public override void Clean(DbContext context)
            => context.Database.EnsureClean();

        public void ExecuteScript(string scriptPath)
        {
            var script = File.ReadAllText(scriptPath);
            Execute(
                Connection, command =>
                {
                    foreach (var batch in
                        new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                            .Split(script).Where(b => !string.IsNullOrEmpty(b)))
                    {
                        command.CommandText = batch;
                        command.ExecuteNonQuery();
                    }

                    return 0;
                }, "");
        }

        private static void WaitForExists(SqlConnection connection)
            => new TestSqlServerRetryingExecutionStrategy().Execute(connection, WaitForExistsImplementation);

        private static void WaitForExistsImplementation(SqlConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }

                    SqlConnection.ClearPool(connection);

                    connection.Open();
                    connection.Close();
                    return;
                }
                catch (SqlException e)
                {
                    if (++retryCount >= 30
                        || e.Number != 233 && e.Number != -2 && e.Number != 4060 && e.Number != 1832 && e.Number != 5120)
                    {
                        throw;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private static string GetCreateDatabaseStatement(string name, string fileName)
        {
            var result = $"CREATE DATABASE [{name}]";

            if (TestEnvironment.IsSqlAzure)
            {
                var elasticGroupName = TestEnvironment.ElasticPoolName;
                result += Environment.NewLine
                    + (string.IsNullOrEmpty(elasticGroupName)
                        ? " ( Edition = 'basic' )"
                        : $" ( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = {elasticGroupName} ) )");
            }
            else
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    var logFileName = Path.ChangeExtension(fileName, ".ldf");
                    result += Environment.NewLine
                        + $" ON (NAME = '{name}', FILENAME = '{fileName}')"
                        + $" LOG ON (NAME = '{name}_log', FILENAME = '{logFileName}')";
                }
            }

            return result;
        }

        public void DeleteDatabase()
        {
            using var master = new SqlConnection(CreateConnectionString("master"));
            ExecuteNonQuery(
                master, string.Format(
                    @"IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                          BEGIN
                                              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                              DROP DATABASE [{0}];
                                          END", Name));

            SqlConnection.ClearAllPools();
        }

        public override void OpenConnection()
            => new TestSqlServerRetryingExecutionStrategy().Execute(Connection, connection => connection.Open());

        public override Task OpenConnectionAsync()
            => new TestSqlServerRetryingExecutionStrategy().ExecuteAsync(Connection, connection => connection.OpenAsync());

        public T ExecuteScalar<T>(string sql, params object[] parameters)
            => ExecuteScalar<T>(Connection, sql, parameters);

        private static T ExecuteScalar<T>(DbConnection connection, string sql, params object[] parameters)
            => Execute(connection, command => (T)command.ExecuteScalar(), sql, false, parameters);

        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
            => ExecuteScalarAsync<T>(Connection, sql, parameters);

        private static Task<T> ExecuteScalarAsync<T>(DbConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, async command => (T)await command.ExecuteScalarAsync(), sql, false, parameters);

        public int ExecuteNonQuery(string sql, params object[] parameters)
            => ExecuteNonQuery(Connection, sql, parameters);

        private static int ExecuteNonQuery(DbConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
            => ExecuteNonQueryAsync(Connection, sql, parameters);

        private static Task<int> ExecuteNonQueryAsync(DbConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, parameters);

        public IEnumerable<T> Query<T>(string sql, params object[] parameters)
            => Query<T>(Connection, sql, parameters);

        private static IEnumerable<T> Query<T>(DbConnection connection, string sql, object[] parameters = null)
            => Execute(
                connection, command =>
                {
                    using var dataReader = command.ExecuteReader();
                    var results = Enumerable.Empty<T>();
                    while (dataReader.Read())
                    {
                        results = results.Concat(new[] { dataReader.GetFieldValue<T>(0) });
                    }

                    return results;
                }, sql, false, parameters);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
            => QueryAsync<T>(Connection, sql, parameters);

        private static Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(
                connection, async command =>
                {
                    using var dataReader = await command.ExecuteReaderAsync();
                    var results = Enumerable.Empty<T>();
                    while (await dataReader.ReadAsync())
                    {
                        results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                    }

                    return results;
                }, sql, false, parameters);

        private static T Execute<T>(
            DbConnection connection,
            Func<DbCommand, T> execute,
            string sql,
            bool useTransaction = false,
            object[] parameters = null)
            => new TestSqlServerRetryingExecutionStrategy().Execute(
                new
                {
                    connection,
                    execute,
                    sql,
                    useTransaction,
                    parameters
                },
                state => ExecuteCommand(state.connection, state.execute, state.sql, state.useTransaction, state.parameters));

        private static T ExecuteCommand<T>(
            DbConnection connection,
            Func<DbCommand, T> execute,
            string sql,
            bool useTransaction,
            object[] parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }

            connection.Open();
            try
            {
                using var transaction = useTransaction ? connection.BeginTransaction() : null;
                T result;
                using (var command = CreateCommand(connection, sql, parameters))
                {
                    command.Transaction = transaction;
                    result = execute(command);
                }

                transaction?.Commit();

                return result;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private static Task<T> ExecuteAsync<T>(
            DbConnection connection,
            Func<DbCommand, Task<T>> executeAsync,
            string sql,
            bool useTransaction = false,
            IReadOnlyList<object> parameters = null)
            => new TestSqlServerRetryingExecutionStrategy().ExecuteAsync(
                new
                {
                    connection,
                    executeAsync,
                    sql,
                    useTransaction,
                    parameters
                },
                state => ExecuteCommandAsync(state.connection, state.executeAsync, state.sql, state.useTransaction, state.parameters));

        private static async Task<T> ExecuteCommandAsync<T>(
            DbConnection connection,
            Func<DbCommand, Task<T>> executeAsync,
            string sql,
            bool useTransaction,
            IReadOnlyList<object> parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                await connection.CloseAsync();
            }

            await connection.OpenAsync();
            try
            {
                using var transaction = useTransaction ? await connection.BeginTransactionAsync() : null;
                T result;
                using (var command = CreateCommand(connection, sql, parameters))
                {
                    result = await executeAsync(command);
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                return result;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static DbCommand CreateCommand(
            DbConnection connection,
            string commandText,
            IReadOnlyList<object> parameters = null)
        {
            var command = (SqlCommand)connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    command.Parameters.AddWithValue("p" + i, parameters[i]);
                }
            }

            return command;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_fileName != null // Clean up the database using a local file, as it might get deleted later
                || (TestEnvironment.IsSqlAzure && !Shared))
            {
                DeleteDatabase();
            }
        }

        public static string CreateConnectionString(string name, string fileName = null, bool? multipleActiveResultSets = null)
        {
            var builder = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                MultipleActiveResultSets = multipleActiveResultSets ?? Random.Shared.Next(0, 2) == 1, InitialCatalog = name
            };
            if (fileName != null)
            {
                builder.AttachDBFilename = fileName;
            }

            return builder.ToString();
        }
    }
}
