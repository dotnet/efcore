// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.FunctionalTests;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerTestStore : RelationalTestStore, IDbCommandExecutor
    {
        public const int CommandTimeout = 30;

        private static int _scratchCount;

        public static Task<SqlServerTestStore> GetOrCreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            return new SqlServerTestStore(name).CreateSharedAsync(initializeDatabase);
        }

        /// <summary>
        ///     A non-transactional, transient, isolated test database. Use this in the case
        ///     where transactions are not appropriate.
        /// </summary>
        public static Task<SqlServerTestStore> CreateScratchAsync(bool createDatabase = true)
        {
            var name = "Microsoft.Data.SqlServer.Scratch_" + Interlocked.Increment(ref _scratchCount);
            return new SqlServerTestStore(name).CreateTransientAsync(createDatabase);
        }

        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private readonly string _name;
        private bool _deleteDatabase;

        // Use async static factory method
        private SqlServerTestStore(string name)
        {
            _name = name;
        }

        private async Task<SqlServerTestStore> CreateSharedAsync(Func<Task> initializeDatabase)
        {
            await CreateSharedAsync(typeof(SqlServerTestStore).Name + _name, initializeDatabase);

            _connection = new SqlConnection(CreateConnectionString(_name));

            await _connection.OpenAsync();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        public static async Task CreateDatabaseIfNotExistsAsync(string name, string scriptPath = null)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                await master.OpenAsync();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText
                        = string.Format(@"SELECT COUNT(*) FROM sys.databases WHERE name = N'{0}'", name);

                    var exists = (int)await command.ExecuteScalarAsync() > 0;

                    if (!exists)
                    {
                        if (scriptPath == null)
                        {
                            command.CommandText = string.Format(@"CREATE DATABASE [{0}]", name);

                            await command.ExecuteNonQueryAsync();

                            using (var newConnection = new SqlConnection(CreateConnectionString(name)))
                            {
                                await WaitForExistsAsync(newConnection);
                            }
                        }
                        else
                        {
                            // HACK: Probe for script file as current dir
                            // is different between k build and VS run.

                            if (!File.Exists(scriptPath))
                            {
                                var appBase = Environment.GetEnvironmentVariable("KRE_APPBASE");

                                if (appBase != null)
                                {
                                    scriptPath = Path.Combine(appBase, Path.GetFileName(scriptPath));
                                }
                            }

                            var script = File.ReadAllText(scriptPath);

                            foreach (var batch
                                in new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline)
                                    .Split(script))
                            {
                                command.CommandText = batch;

                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
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

        private async Task<SqlServerTestStore> CreateTransientAsync(bool createDatabase)
        {
            await DeleteDatabaseAsync(_name);

            _connection = new SqlConnection(CreateConnectionString(_name));

            if (createDatabase)
            {
                using (var master = new SqlConnection(CreateConnectionString("master")))
                {
                    await master.OpenAsync();
                    using (var command = master.CreateCommand())
                    {
                        command.CommandText = string.Format("{0}CREATE DATABASE [{1}]", Environment.NewLine, _name);

                        await command.ExecuteNonQueryAsync();

                        await WaitForExistsAsync(_connection);
                    }
                }
                await _connection.OpenAsync();
            }

            _deleteDatabase = createDatabase;
            return this;
        }

        private async Task DeleteDatabaseAsync(string name)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                await master.OpenAsync().WithCurrentCulture();

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

                    var userFolder = Environment.GetEnvironmentVariable("USERPROFILE");
                    try
                    {
                        File.Delete(Path.Combine(userFolder, name + ".mdf"));
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        File.Delete(Path.Combine(userFolder, name + "_log.ldf"));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public override DbConnection Connection
        {
            get { return _connection; }
        }

        public override DbTransaction Transaction
        {
            get { return _transaction; }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return (T)await command.ExecuteScalarAsync(cancellationToken);
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
                        results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
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
                ((SqlParameterCollection)command.Parameters).AddWithValue("p" + i, parameters[i]);
            }

            return command;
        }

        public override void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }

            _connection.Dispose();

            if (_deleteDatabase)
            {
                DeleteDatabaseAsync(_name).Wait();
            }
        }

        public static string CreateConnectionString(string name)
        {
            return new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\MSSQLLocalDB",
                    // TODO: Currently nested queries are run while processing the results of outer queries
                    // This either requires MARS or creation of a new connection for each query. Currently using
                    // MARS since cloning connections is known to be problematic.
                    MultipleActiveResultSets = true,
                    InitialCatalog = name,
                    IntegratedSecurity = true,
                    ConnectTimeout = 30
                }.ConnectionString;
        }
    }
}
