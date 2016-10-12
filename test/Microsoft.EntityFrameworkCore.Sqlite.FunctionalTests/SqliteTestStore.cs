// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class SqliteTestStore : RelationalTestStore
    {
        private static int _scratchCount;

        public static SqliteTestStore GetOrCreateShared(string name, bool useTransaction, bool sharedCache, Action initializeDatabase = null) =>
            new SqliteTestStore(name).CreateShared(initializeDatabase, useTransaction, sharedCache);

        public static SqliteTestStore GetOrCreateShared(string name, Action initializeDatabase = null) =>
            GetOrCreateShared(name, true, false, initializeDatabase);

#if NETCOREAPP1_1
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
        private readonly string _name;
        private bool _deleteDatabase;
        public const int CommandTimeout = 30;

        private SqliteTestStore(string name)
        {
            _name = name;
        }

        public override string ConnectionString => Connection.ConnectionString;

        private SqliteTestStore CreateShared(Action initializeDatabase, bool openConnection, bool sharedCache)
        {
            CreateShared(typeof(SqliteTestStore).Name + _name, initializeDatabase);

            CreateConnection(sharedCache);

            if (openConnection)
            {
                OpenConnection();
            }

            return this;
        }

        private SqliteTestStore CreateTransient(bool sharedCache)
        {
            CreateConnection(sharedCache);
            OpenConnection();

            _deleteDatabase = true;
            return this;
        }

        private void CreateConnection(bool sharedCache = false)
        {
            _connection = new SqliteConnection(CreateConnectionString(_name, sharedCache));

            OpenConnection();
        }

        public override void OpenConnection()
        {
            _connection.Open();

            var command = _connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys=ON;";
            command.ExecuteNonQuery();
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

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("@p" + i, parameters[i]);
            }

            return command;
        }

        public override DbConnection Connection => _connection;
        public override DbTransaction Transaction => null;

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
