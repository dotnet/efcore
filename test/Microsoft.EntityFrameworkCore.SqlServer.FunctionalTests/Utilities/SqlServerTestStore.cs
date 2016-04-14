// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.FunctionalTests;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities
{
    public class SqlServerTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 90;

#if NETSTANDARDAPP1_5
        private static string BaseDirectory => AppContext.BaseDirectory;
#else
        private static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
#endif

        public static SqlServerTestStore GetOrCreateShared(string name, Action initializeDatabase)
            => new SqlServerTestStore(name).CreateShared(initializeDatabase);

        /// <summary>
        ///     A non-transactional, transient, isolated test database. Use this in the case
        ///     where transactions are not appropriate.
        /// </summary>
        public static Task<SqlServerTestStore> CreateScratchAsync(bool createDatabase = true)
            => new SqlServerTestStore(GetScratchDbName()).CreateTransientAsync(createDatabase);

        public static SqlServerTestStore CreateScratch(bool createDatabase = true)
            => new SqlServerTestStore(GetScratchDbName()).CreateTransient(createDatabase);

        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private readonly string _name;
        private string _connectionString;
        private bool _deleteDatabase;

        public override string ConnectionString => _connectionString;

        // Use async static factory method
        private SqlServerTestStore(string name)
        {
            _name = name;
        }

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

        private SqlServerTestStore CreateShared(Action initializeDatabase)
        {
            CreateShared(typeof(SqlServerTestStore).Name + _name, initializeDatabase);

            _connectionString = CreateConnectionString(_name);
            _connection = new SqlConnection(_connectionString);

            _connection.Open();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        public static void CreateDatabase(string name, string scriptPath = null, bool nonMasterScript = false, bool recreateIfAlreadyExists = false)
        {
            using (var master = new SqlConnection(CreateConnectionString("master", multipleActiveResultSets: false)))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;

                    var exists = DatabaseExists(name);
                    if (exists && (recreateIfAlreadyExists || !TablesExist(name)))
                    {
                        // if scriptPath is non-null assume that the script will handle dropping DB
                        if (scriptPath == null
                            || nonMasterScript)
                        {
                            command.CommandText = GetDeleteDatabaseSql(name);

                            command.ExecuteNonQuery();
                        }
                    }

                    if (!exists || recreateIfAlreadyExists)
                    {
                        if (scriptPath == null
                            || nonMasterScript)
                        {
                            command.CommandText = $@"CREATE DATABASE [{name}]";

                            command.ExecuteNonQuery();

                            using (var newConnection = new SqlConnection(CreateConnectionString(name)))
                            {
                                WaitForExists(newConnection);
                            }
                        }

                        if (scriptPath != null)
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

                            if (nonMasterScript)
                            {
                                using (var newConnection = new SqlConnection(CreateConnectionString(name)))
                                {
                                    newConnection.Open();
                                    using (var nonMasterCommand = newConnection.CreateCommand())
                                    {
                                        ExecuteScript(scriptPath, nonMasterCommand);
                                    }
                                }
                            }
                            else
                            {
                                ExecuteScript(scriptPath, command);
                            }
                        }
                    }
                }
            }
        }

        private static void ExecuteScript(string scriptPath, SqlCommand scriptCommand)
        {
            var script = File.ReadAllText(scriptPath);
            foreach (var batch in new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                .Split(script).Where(b => !string.IsNullOrEmpty(b)))
            {
                scriptCommand.CommandText = batch;

                scriptCommand.ExecuteNonQuery();
            }
        }

        private static async Task WaitForExistsAsync(SqlConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    await connection.OpenAsync();

                    connection.Close();

                    return;
                }
                catch (SqlException e)
                {
                    if (++retryCount >= 30
                        || (e.Number != 233 && e.Number != -2 && e.Number != 4060))
                    {
                        throw;
                    }

                    SqlConnection.ClearPool(connection);

                    Thread.Sleep(100);
                }
            }
        }

        private static void WaitForExists(SqlConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    connection.Open();

                    connection.Close();

                    return;
                }
                catch (SqlException e)
                {
                    if (++retryCount >= 30
                        || (e.Number != 233 && e.Number != -2 && e.Number != 4060))
                    {
                        throw;
                    }

                    SqlConnection.ClearPool(connection);

                    Thread.Sleep(100);
                }
            }
        }

        private async Task<SqlServerTestStore> CreateTransientAsync(bool createDatabase)
        {
            _connectionString = CreateConnectionString(_name);
            _connection = new SqlConnection(_connectionString);

            if (createDatabase)
            {
                using (var master = new SqlConnection(CreateConnectionString("master")))
                {
                    await master.OpenAsync();
                    using (var command = master.CreateCommand())
                    {
                        command.CommandTimeout = CommandTimeout;
                        command.CommandText = $"{Environment.NewLine}CREATE DATABASE [{_name}]";

                        await command.ExecuteNonQueryAsync();

                        await WaitForExistsAsync(_connection);
                    }
                }
                await _connection.OpenAsync();
            }

            _deleteDatabase = true;
            return this;
        }

        private SqlServerTestStore CreateTransient(bool createDatabase)
        {
            _connectionString = CreateConnectionString(_name);
            _connection = new SqlConnection(_connectionString);

            if (createDatabase)
            {
                using (var master = new SqlConnection(CreateConnectionString("master")))
                {
                    master.Open();
                    using (var command = master.CreateCommand())
                    {
                        command.CommandTimeout = CommandTimeout;
                        command.CommandText = $"{Environment.NewLine}CREATE DATABASE [{_name}]";

                        command.ExecuteNonQuery();

                        WaitForExists(_connection);
                    }
                }
                _connection.Open();
            }

            _deleteDatabase = true;
            return this;
        }

        private static bool DatabaseExists(string name)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = $@"SELECT COUNT(*) FROM sys.databases WHERE name = N'{name}'";

                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        private static bool TablesExist(string name)
        {
            using (var connection = new SqlConnection(CreateConnectionString(name)))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = @"SELECT COUNT(*) FROM information_schema.tables";

                    var result = (int)command.ExecuteScalar() > 0;

                    connection.Close();

                    SqlConnection.ClearAllPools();

                    return result;
                }
            }
        }

        private static bool DatabaseFilesExist(string name)
        {
            var userFolder = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
            return userFolder != null
                   && (File.Exists(Path.Combine(userFolder, name + ".mdf"))
                       || File.Exists(Path.Combine(userFolder, name + "_log.ldf")));
        }

        private async Task DeleteDatabaseAsync(string name)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                await master.OpenAsync();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout; // Query will take a few seconds if (and only if) there are active connections

                    // SET SINGLE_USER will close any open connections that would prevent the drop
                    command.CommandText
                        = string.Format(@"IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                          BEGIN
                                              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                              DROP DATABASE [{0}];
                                          END", name);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private void DeleteDatabase(string name)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout; // Query will take a few seconds if (and only if) there are active connections

                    // SET SINGLE_USER will close any open connections that would prevent the drop
                    command.CommandText = GetDeleteDatabaseSql(name);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static string GetDeleteDatabaseSql(string name)
            => string.Format(@"IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                          BEGIN
                                              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                              DROP DATABASE [{0}];
                                          END", name);

        public override DbConnection Connection => _connection;

        public override DbTransaction Transaction => _transaction;

        public async Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return (T)await command.ExecuteScalarAsync(cancellationToken);
            }
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var results = Enumerable.Empty<T>();

                    while (await dataReader.ReadAsync())
                    {
                        try
                        {
                            results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                        }
                        catch (NotImplementedException)
                        {
                            // TODO remove workaround for mono limitation.
                            results = results.Concat(new[] { (T)dataReader.GetValue(0) });
                        }
                    }

                    return results;
                }
            }
        }

        private DbCommand CreateCommand(string commandText, object[] parameters)
        {
            var command = _connection.CreateCommand();

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }

            return command;
        }

        public override void Dispose()
        {
            _transaction?.Dispose();

            _connection.Dispose();

            if (_deleteDatabase)
            {
                DeleteDatabase(_name);
            }
        }

        public static string CreateConnectionString(string name)
            => CreateConnectionString(name, new Random().Next(0, 2) == 1);

        private static string CreateConnectionString(string name, bool multipleActiveResultSets)
            => new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                MultipleActiveResultSets = multipleActiveResultSets,
                InitialCatalog = name
            }.ConnectionString;
    }
}
