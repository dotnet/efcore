// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerTestDatabase : RelationalTestStore, IDbCommandExecutor
    {
        public const int CommandTimeout = 30;
        private const string DefaultDatabaseName = "Microsoft.Data.SqlServer.FunctionalTest";
        private const string NorthwindDatabaseName = "Northwind";

        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, AsyncLock> _creationLocks
            = new ConcurrentDictionary<string, AsyncLock>();

        private static int _scratchCount;

        /// <summary>
        ///     A transactional test database, pre-populated with Northwind schema/data
        /// </summary>
        public static Task<SqlServerTestDatabase> Northwind()
        {
            return new SqlServerTestDatabase()
                .CreateShared(NorthwindDatabaseName, () => CreateDatabaseIfNotExistsAsync(NorthwindDatabaseName, scriptPath: @"..\..\Northwind.sql")); // relative from bin/<config>
        }

        public static string NorthwindConnectionString
        {
            get { return CreateConnectionString(NorthwindDatabaseName); }
        }

        /// <summary>
        ///     The default empty transactional test database.
        /// </summary>
        public static Task<SqlServerTestDatabase> Default()
        {
            return new SqlServerTestDatabase()
                .CreateShared(DefaultDatabaseName, () => CreateDatabaseIfNotExistsAsync(DefaultDatabaseName));
        }

        public static string DefaultConnectionString
        {
            get { return CreateConnectionString(DefaultDatabaseName); }
        }

        public static Task<SqlServerTestDatabase> Named(string name, Func<Task> initializeDatabase)
        {
            return new SqlServerTestDatabase()
                .CreateShared(name, initializeDatabase);
        }

        /// <summary>
        ///     A non-transactional, transient, isolated test database. Use this in the case
        ///     where transactions are not appropriate.
        /// </summary>
        public static Task<SqlServerTestDatabase> Scratch(bool createDatabase = true)
        {
            return new SqlServerTestDatabase()
                .CreateScratch("Microsoft.Data.SqlServer.Scratch_" + Interlocked.Increment(ref _scratchCount), createDatabase);
        }

        private SqlConnection _connection;
        private SqlTransaction _transaction;

        private SqlServerTestDatabase()
        {
            // Use async static factory method
        }

        private async Task<SqlServerTestDatabase> CreateShared(string name, Func<Task> initializeDatabase)
        {
            if (!_createdDatabases.Contains(name))
            {
                var asyncLock
                    = _creationLocks.GetOrAdd(name, new AsyncLock());

                using (await asyncLock.LockAsync())
                {
                    if (!_createdDatabases.Contains(name))
                    {
                        await initializeDatabase();

                        _createdDatabases.Add(name);

                        AsyncLock _;
                        _creationLocks.TryRemove(name, out _);
                    }
                }
            }

            _connection = new SqlConnection(CreateConnectionString(name));

            await _connection.OpenAsync();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        private static async Task CreateDatabaseIfNotExistsAsync(string name, string scriptPath = null)
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
                        }
                        else
                        {
                            // HACK: Probe for script file as current dir
                            // is different between k build and VS run.

                            if (!File.Exists(scriptPath))
                            {
                                var kAppBase = Environment.GetEnvironmentVariable("k_appbase");

                                if (kAppBase != null)
                                {
                                    scriptPath = Path.Combine(kAppBase, Path.GetFileName(scriptPath));
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

        private async Task<SqlServerTestDatabase> CreateScratch(string name, bool createDatabase)
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

                    if (createDatabase)
                    {
                        command.CommandText += string.Format("{0}CREATE DATABASE [{1}]", Environment.NewLine, name);
                    }

                    await command.ExecuteNonQueryAsync();
                }
            }

            _connection = new SqlConnection(CreateConnectionString(name));

            if (createDatabase)
            {
                await _connection.OpenAsync();
            }

            return this;
        }

        public override DbConnection Connection
        {
            get { return _connection; }
        }

        public SqlTransaction Transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
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
                command.Parameters.AddWithValue("p" + i, parameters[i]);
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
        }

        public static string CreateConnectionString(string name)
        {
            return new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\v11.0",
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
