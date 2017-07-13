// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable SuggestBaseTypeForParameter
namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqlServerTestStore : RelationalTestStore<SqlConnection>
    {
        private const string NorthwindName = "Northwind";
        public static readonly string NorthwindConnectionString = CreateConnectionString(NorthwindName);
        public const int CommandTimeout = 600;

        private static string BaseDirectory => AppContext.BaseDirectory;

        public static SqlServerTestStore GetNorthwindStore()
            => GetOrCreateShared(
                NorthwindName,
                Path.Combine(Path.GetDirectoryName(typeof(SqlServerTestStore).GetTypeInfo().Assembly.Location),
                    "Northwind.sql"));

        public static SqlServerTestStore GetOrCreateShared(string name, Action initializeDatabase)
            => new SqlServerTestStore(name).InitializeShared(s => initializeDatabase?.Invoke());

        public static SqlServerTestStore GetOrCreateShared(string name, string scriptPath)
            => new SqlServerTestStore(name, cleanDatabase: false, scriptPath: scriptPath).InitializeShared(s => s.ExecuteScript(scriptPath));

        public static SqlServerTestStore GetOrCreateShared(string name)
            => new SqlServerTestStore(name);

        public static SqlServerTestStore GetOrCreateSharedScript(string name, string scriptPath)
            => new SqlServerTestStore(name, cleanDatabase: false, scriptPath: scriptPath);

        public static SqlServerTestStore Create(string name, bool deleteDatabase = false)
            => new SqlServerTestStore(name).CreateTransient(true, deleteDatabase);

        public static SqlServerTestStore CreateScratch(bool createDatabase = true, bool useFileName = false)
            => new SqlServerTestStore(GetScratchDbName(), useFileName: useFileName).CreateTransient(createDatabase, true);

        private static string GetScratchDbName()
        {
            string name;
            do
            {
                name = "Scratch_" + Guid.NewGuid();
            }
            while (DatabaseExists(name)
                   || DatabaseFilesExist(name));

            return name;
        }

        private readonly string _fileName;
        private readonly bool _cleanDatabase;
        private readonly string _scriptPath;
        private bool _deleteDatabase;

        private SqlServerTestStore(
            string name,
            bool useFileName = false,
            bool cleanDatabase = true,
            string scriptPath = null)
            : base(name)
        {
            if (useFileName)
            {
                _fileName = Path.Combine(BaseDirectory, name + ".mdf");
            }

            _cleanDatabase = cleanDatabase;
            _scriptPath = scriptPath;
        }

        public override TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => InitializeShared(s =>
                {
                    if (_scriptPath != null)
                    {
                        s.ExecuteScript(_scriptPath);
                    }
                    else
                    {
                        using (var context = createContext())
                        {
                            context.Database.EnsureCreated();
                            seed(context);
                        }
                    }
                });

        private SqlServerTestStore InitializeShared(Action<SqlServerTestStore> initializeDatabase)
        {
            ConnectionString = CreateConnectionString(Name, _fileName);
            Connection = new SqlConnection(ConnectionString);

            GlobalTestStoreIndex.CreateShared(typeof(SqlServerTestStore).Name + Name, () =>
                {
                    if (CreateDatabase())
                    {
                        initializeDatabase?.Invoke(this);
                    }
                });

            return this;
        }

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseSqlServer(Connection, b => b.ApplyConfiguration().CommandTimeout(CommandTimeout));

        private bool CreateDatabase()
        {
            using (var master = new SqlConnection(CreateConnectionString("master", false)))
            {
                if (DatabaseExists(Name))
                {
                    if (!_cleanDatabase)
                    {
                        return false;
                    }

                    using (var context = new DbContext(AddProviderOptions(new DbContextOptionsBuilder()).Options))
                    {
                        context.Database.EnsureClean();
                    }
                }
                else
                {
                    ExecuteNonQuery(master, GetCreateDatabaseStatement(Name, _fileName));
                    WaitForExists(Connection);
                }
            }

            return true;
        }

        public void ExecuteScript(string scriptPath)
        {
            // HACK: Probe for script file as current dir
            // is different between k build and VS run.
            if (File.Exists(@"..\..\" + scriptPath))
            {
                //executing in VS - so path is relative to bin\<config> dir
                scriptPath = @"..\..\" + scriptPath;
            }
            else
            {
                scriptPath = Path.Combine(BaseDirectory, scriptPath);
            }

            var script = File.ReadAllText(scriptPath);
            Execute(Connection, command =>
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
        {
            if (TestEnvironment.IsSqlAzure)
            {
                new TestSqlServerRetryingExecutionStrategy().Execute(connection, WaitForExistsImplementation);
            }
            else
            {
                WaitForExistsImplementation(connection);
            }
        }

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

        private SqlServerTestStore CreateTransient(bool createDatabase, bool deleteDatabase)
        {
            ConnectionString = CreateConnectionString(Name, _fileName);
            Connection = new SqlConnection(ConnectionString);

            if (createDatabase)
            {
                CreateDatabase();

                OpenConnection();
            }
            else if (DatabaseExists(Name))
            {
                DeleteDatabase(Name);
            }

            _deleteDatabase = deleteDatabase;
            return this;
        }

        private static string GetCreateDatabaseStatement(string name, string fileName)
        {
            var result = $"CREATE DATABASE [{name}]";

            if (TestEnvironment.IsSqlAzure)
            {
                var elasticGroupName = TestEnvironment.ElasticPoolName;
                result += Environment.NewLine +
                          (string.IsNullOrEmpty(elasticGroupName)
                              ? " ( Edition = 'basic' )"
                              : $" ( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = {elasticGroupName} ) )");
            }
            else
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    var logFileName = Path.ChangeExtension(fileName, ".ldf");
                    result += Environment.NewLine +
                              $" ON (NAME = '{name}', FILENAME = '{fileName}')" +
                              $" LOG ON (NAME = '{name}_log', FILENAME = '{logFileName}')";
                }
            }
            return result;
        }

        private static bool DatabaseExists(string name)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                return ExecuteScalar<int>(master, $@"SELECT COUNT(*) FROM sys.databases WHERE name = N'{name}'") > 0;
            }
        }

        private static bool DatabaseFilesExist(string name)
        {
            var userFolder = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
            return userFolder != null
                   && (File.Exists(Path.Combine(userFolder, name + ".mdf"))
                       || File.Exists(Path.Combine(userFolder, name + "_log.ldf")));
        }

        private static void DeleteDatabase(string name)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                ExecuteNonQuery(master, GetDeleteDatabaseSql(name));

                SqlConnection.ClearAllPools();
            }
        }

        private static string GetDeleteDatabaseSql(string name)
            // SET SINGLE_USER will close any open connections that would prevent the drop
            => string.Format(@"IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                          BEGIN
                                              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                              DROP DATABASE [{0}];
                                          END", name);

        public override void OpenConnection()
        {
            if (TestEnvironment.IsSqlAzure)
            {
                new TestSqlServerRetryingExecutionStrategy().Execute(Connection, connection => connection.Open());
            }
            else
            {
                Connection.Open();
            }
        }

        public override Task OpenConnectionAsync()
            => TestEnvironment.IsSqlAzure
                ? new TestSqlServerRetryingExecutionStrategy().ExecuteAsync(Connection, connection => connection.OpenAsync())
                : Connection.OpenAsync();

        public T ExecuteScalar<T>(string sql, params object[] parameters)
            => ExecuteScalar<T>(Connection, sql, parameters);

        private static T ExecuteScalar<T>(SqlConnection connection, string sql, params object[] parameters)
            => Execute(connection, command => (T)command.ExecuteScalar(), sql, false, parameters);

        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
            => ExecuteScalarAsync<T>(Connection, sql, parameters);

        private static Task<T> ExecuteScalarAsync<T>(SqlConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, async command => (T)await command.ExecuteScalarAsync(), sql, false, parameters);

        public int ExecuteNonQuery(string sql, params object[] parameters)
            => ExecuteNonQuery(Connection, sql, parameters);

        private static int ExecuteNonQuery(SqlConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
            => ExecuteNonQueryAsync(Connection, sql, parameters);

        private static Task<int> ExecuteNonQueryAsync(SqlConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, parameters);

        public IEnumerable<T> Query<T>(string sql, params object[] parameters)
            => Query<T>(Connection, sql, parameters);

        private static IEnumerable<T> Query<T>(SqlConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command =>
                {
                    using (var dataReader = command.ExecuteReader())
                    {
                        var results = Enumerable.Empty<T>();
                        while (dataReader.Read())
                        {
                            results = results.Concat(new[] { dataReader.GetFieldValue<T>(0) });
                        }
                        return results;
                    }
                }, sql, false, parameters);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
            => QueryAsync<T>(Connection, sql, parameters);

        private static Task<IEnumerable<T>> QueryAsync<T>(SqlConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(connection, async command =>
                {
                    using (var dataReader = await command.ExecuteReaderAsync())
                    {
                        var results = Enumerable.Empty<T>();
                        while (await dataReader.ReadAsync())
                        {
                            results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                        }
                        return results;
                    }
                }, sql, false, parameters);

        private static T Execute<T>(
            SqlConnection connection, Func<DbCommand, T> execute, string sql,
            bool useTransaction = false, object[] parameters = null)
            => TestEnvironment.IsSqlAzure
                ? new TestSqlServerRetryingExecutionStrategy().Execute(new { connection, execute, sql, useTransaction, parameters },
                    state => ExecuteCommand(state.connection, state.execute, state.sql, state.useTransaction, state.parameters))
                : ExecuteCommand(connection, execute, sql, useTransaction, parameters);

        private static T ExecuteCommand<T>(
            SqlConnection connection, Func<DbCommand, T> execute, string sql, bool useTransaction, object[] parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            connection.Open();
            try
            {
                using (var transaction = useTransaction ? connection.BeginTransaction() : null)
                {
                    T result;
                    using (var command = CreateCommand(connection, sql, parameters))
                    {
                        command.Transaction = transaction;
                        result = execute(command);
                    }
                    transaction?.Commit();

                    return result;
                }
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
            SqlConnection connection, Func<DbCommand, Task<T>> executeAsync, string sql,
            bool useTransaction = false, IReadOnlyList<object> parameters = null)
            => TestEnvironment.IsSqlAzure
                ? new TestSqlServerRetryingExecutionStrategy().ExecuteAsync(
                    new { connection, executeAsync, sql, useTransaction, parameters },
                    state => ExecuteCommandAsync(state.connection, state.executeAsync, state.sql, state.useTransaction, state.parameters))
                : ExecuteCommandAsync(connection, executeAsync, sql, useTransaction, parameters);

        private static async Task<T> ExecuteCommandAsync<T>(
            SqlConnection connection, Func<DbCommand, Task<T>> executeAsync, string sql, bool useTransaction, IReadOnlyList<object> parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            await connection.OpenAsync();
            try
            {
                using (var transaction = useTransaction ? connection.BeginTransaction() : null)
                {
                    T result;
                    using (var command = CreateCommand(connection, sql, parameters))
                    {
                        result = await executeAsync(command);
                    }
                    transaction?.Commit();

                    return result;
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private static DbCommand CreateCommand(
            SqlConnection connection, string commandText, IReadOnlyList<object> parameters = null)
        {
            var command = connection.CreateCommand();

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

            if (_deleteDatabase)
            {
                DeleteDatabase(Name);
            }
        }

        public static string CreateConnectionString(string name)
            => CreateConnectionString(name, null, true); // Force MARS until #9074 is fixed

        public static string CreateConnectionString(string name, string fileName)
            => CreateConnectionString(name, fileName, true); // Force MARS until #9074 is fixed

        private static string CreateConnectionString(string name, bool multipleActiveResultSets)
            => CreateConnectionString(name, null, multipleActiveResultSets);

        private static string CreateConnectionString(string name, string fileName, bool multipleActiveResultSets)
        {
            var builder = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                MultipleActiveResultSets = multipleActiveResultSets,
                InitialCatalog = name
            };
            if (fileName != null)
            {
                builder.AttachDBFilename = fileName;
            }

            return builder.ToString();
        }
    }
}
