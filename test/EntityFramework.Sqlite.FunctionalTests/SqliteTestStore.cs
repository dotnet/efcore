// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SqliteTestStore : RelationalTestStore
    {
        private static int _scratchCount;

        public static SqliteTestStore GetOrCreateShared(string name, Action initializeDatabase) =>
            new SqliteTestStore(name).CreateShared(initializeDatabase);

        public static SqliteTestStore CreateScratch(bool sharedCache = false)
        {
            string name;
            do
            {
                name = "scratch-" + Interlocked.Increment(ref _scratchCount);
            }
            while (File.Exists(name + ".db"));

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

        private SqliteTestStore CreateShared(Action initializeDatabase)
        {
            CreateShared(typeof(SqliteTestStore).Name + _name, initializeDatabase);

            _connection = new SqliteConnection(CreateConnectionString(_name));

            _connection.Open();
            _transaction = _connection.BeginTransaction();

            return this;
        }

        private SqliteTestStore CreateTransient(bool sharedCache)
        {
            _connection = new SqliteConnection(CreateConnectionString(_name, sharedCache));

            _connection.Open();

            return this.AsTransient();
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

            if (_deleteDatabase)
            {
                var fileName = _name + ".db";
                try
                {
                    // TODO figure out why some tests cannot delete db files
                    File.Delete(fileName);
                }
                catch (IOException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            base.Dispose();
        }

        public static string CreateConnectionString(string name, bool sharedCache = false) =>
            new SqliteConnectionStringBuilder
            {
                DataSource = name + ".db",
                CacheMode = sharedCache ? CacheMode.Shared : CacheMode.Private
            }
                .ToString();
    }
}
