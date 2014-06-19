// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.SQLite;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class TestDatabase : IDisposable
    {
        private readonly Data.SQLite.SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;

        public TestDatabase(string connectionString)
        {
            _connection = new Data.SQLite.SQLiteConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public Data.SQLite.SQLiteConnection Connection
        {
            get { return _connection; }
        }

        public static TestDatabase Northwind()
        {
            return new TestDatabase("Filename=northwind.db");
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }
    }
}
