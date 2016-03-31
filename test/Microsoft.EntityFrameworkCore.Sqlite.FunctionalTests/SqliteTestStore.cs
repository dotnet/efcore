// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.FunctionalTests;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class SqliteTestStore : RelationalTestStore
    {
        private static int _scratchCount;

        public static SqliteTestStore GetOrCreateShared(string name, Action initializeDatabase = null) =>
            new SqliteTestStore(name).CreateShared(initializeDatabase);

#if NETSTANDARDAPP1_5
        private static string BaseDirectory => AppContext.BaseDirectory;
#else
        private static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
#endif

        public static SqliteTestStore CreateScratch(bool sharedCache = false)
        {
            string name;
            do
            {
                name = "scratch-" + Interlocked.Increment(ref _scratchCount);
            }
            while (File.Exists(name + ".db")
                   || File.Exists(Path.Combine(BaseDirectory, name + ".db")));

            return new SqliteTestStore(name).CreateTransient(sharedCache);
        }

        private SqliteConnection _connection;
        private SqliteTransaction _transaction;
        private readonly string _name;
        private bool _deleteDatabase;
        public const int CommandTimeout = 30;

        private SqliteTestStore(string name)
        {
            _name = name;
        }

        public override string ConnectionString => Connection.ConnectionString;

        private SqliteTestStore CreateShared(Action initializeDatabase)
        {
            CreateShared(typeof(SqliteTestStore).Name + _name, initializeDatabase);

            CreateAndOpenConnection();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        private SqliteTestStore CreateTransient(bool sharedCache)
        {
            CreateAndOpenConnection(sharedCache);

            return AsTransient();
        }

        private void CreateAndOpenConnection(bool sharedCache = false)
        {
            _connection = new SqliteConnection(CreateConnectionString(_name, sharedCache));

            _connection.Open();

            var command = _connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys=ON;";
            command.ExecuteNonQuery();
        }

        public SqliteTestStore AsTransient()
        {
            _deleteDatabase = true;
            return this;
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQuery();
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
                command.Parameters.AddWithValue("@p" + i, parameters[i]);
            }

            return command;
        }

        public override DbConnection Connection => _connection;
        public override DbTransaction Transaction => _transaction;

        public override void Dispose()
        {
            Transaction?.Dispose();
            Connection?.Dispose();
            base.Dispose();

            if (_deleteDatabase)
            {
                File.Delete(_name + ".db");
            }
        }

        public static string CreateConnectionString(string name, bool sharedCache = false) =>
            new SqliteConnectionStringBuilder
            {
                DataSource = name + ".db",
                Cache = sharedCache ? SqliteCacheMode.Shared : SqliteCacheMode.Private
            }.ToString();
    }
}
