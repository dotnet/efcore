// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.Tests
{
    public class SQLiteDatabaseExtensionsTest
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new SQLiteDatabase(configurationMock.Object, new LoggerFactory());

            Assert.Same(database, database.AsSQLite());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new ConcreteDatabase(configurationMock.Object, new LoggerFactory());

            Assert.Equal(
                Strings.SQLiteNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsSQLite()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(DbContextConfiguration configuration, ILoggerFactory loggerFactory)
                : base(configuration, loggerFactory)
            {
            }
        }
    }
}
