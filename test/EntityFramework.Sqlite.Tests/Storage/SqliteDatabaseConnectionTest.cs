// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests.TestUtilities;
using Microsoft.Data.Entity.TestUtilities.FakeProvider;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Storage
{
    public class SqliteDatabaseConnectionTest
    {
        [Fact]
        public void Enables_foreign_keys_when_connection_opened()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(new FakeDbConnection("Data Source=Fake"));

            var log = new List<Tuple<LogLevel, string>>();

            var loggerFactory = new ListLoggerFactory(log, s => s == typeof(RelationalCommand).FullName);

            var connection = new SqliteDatabaseConnection(
                new RelationalCommandBuilderFactory(
                    loggerFactory,
                    new SqliteTypeMapper()),
                optionsBuilder.Options,
                loggerFactory);

            connection.Open();

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("PRAGMA foreign_keys=ON;", log[0].Item2);

            connection.Close();
            connection.Open();

            Assert.Equal(2, log.Count);
            Assert.Equal(LogLevel.Verbose, log[1].Item1);
            Assert.Equal("PRAGMA foreign_keys=ON;", log[1].Item2);
        }

        [Fact]
        public void Only_enables_foreign_keys_the_first_time_a_connection_is_opened()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(new FakeDbConnection("Data Source=Fake"));

            var log = new List<Tuple<LogLevel, string>>();

            var loggerFactory = new ListLoggerFactory(log, s => s == typeof(RelationalCommand).FullName);

            var connection = new SqliteDatabaseConnection(
                new RelationalCommandBuilderFactory(
                    loggerFactory,
                    new SqliteTypeMapper()),
                optionsBuilder.Options,
                loggerFactory);

            connection.Open();

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Verbose, log[0].Item1);
            Assert.Equal("PRAGMA foreign_keys=ON;", log[0].Item2);

            connection.Open();

            Assert.Equal(1, log.Count);
        }
    }
}
