// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class MigrationsDatabaseExtensionsTest
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new ConcreteMigrationsEnabledDatabase(configurationMock.Object, new LoggerFactory());

            Assert.Same(database, database.AsMigrationsEnabled());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new ConcreteDatabase(configurationMock.Object, new LoggerFactory());

            Assert.Equal(
                Strings.MigrationsNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsMigrationsEnabled()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(DbContextConfiguration configuration, ILoggerFactory loggerFactory)
                : base(configuration, loggerFactory)
            {
            }
        }

        private class ConcreteMigrationsEnabledDatabase : MigrationsEnabledDatabase
        {
            public ConcreteMigrationsEnabledDatabase(DbContextConfiguration configuration, ILoggerFactory loggerFactory)
                : base(configuration, loggerFactory)
            {
            }
        }
    }
}
