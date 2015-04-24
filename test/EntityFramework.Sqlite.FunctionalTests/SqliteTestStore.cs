// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SqliteTestStore : RelationalTestStore
    {
        private static int _scratchCount;

        public static SqliteTestStore GetOrCreateShared(string name, Action initializeDatabase) =>
            new SqliteTestStore(name).CreateShared(initializeDatabase);

        public static SqliteTestStore CreateScratch() =>
            new SqliteTestStore("scratch-" + Interlocked.Increment(ref _scratchCount)).CreateTransient();

        private SqliteConnection _connection;
        private SqliteTransaction _transaction;
        private readonly string _name;
        private bool _deleteDatabase;

        public SqliteTestStore(string name)
        {
            _name = name;
        }

        private SqliteTestStore CreateShared(Action initializeDatabase)
        {
            CreateShared(typeof(SqliteTestStore).Name + _name, initializeDatabase);

            _connection = new SqliteConnection(CreateConnectionString(_name));

            _connection.Open();
            _transaction = _connection.BeginTransaction();

            return this;
        }

        private SqliteTestStore CreateTransient()
        {
            _connection = new SqliteConnection(CreateConnectionString(_name));

            _connection.Open();

            _deleteDatabase = true;

            return this;
        }

        public override DbConnection Connection => _connection;
        public override DbTransaction Transaction => _transaction;

        public override void Dispose()
        {
            Transaction?.Dispose();

            string path = null;
            if (_deleteDatabase)
            {
                _connection.Open();
                path = _connection.DataSource;
            }

            _connection.Dispose();

            if (path != null)
            {
                File.Delete(path);
            }
        }

        public static string CreateConnectionString(string name) =>
            new SqliteConnectionStringBuilder
                {
                    DataSource = name + ".db"
                }
            .ToString();
    }
}
