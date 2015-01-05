// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations
{
    public class MigrationsDatabaseExtensionsTest
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var database = new ConcreteMigrationsEnabledDatabase(
                new DbContextService<IModel>(() => null),
                Mock.Of<DataStoreCreator>(),
                Mock.Of<DataStoreConnection>(),
                Mock.Of<Migrator>(),
                new LoggerFactory());

            Assert.Same(database, database.AsMigrationsEnabled());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var database = new ConcreteDatabase(
                new DbContextService<IModel>(() => null),
                Mock.Of<DataStoreCreator>(),
                Mock.Of<DataStoreConnection>(),
                new LoggerFactory());

            Assert.Equal(
                Strings.MigrationsNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsMigrationsEnabled()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(
                DbContextService<IModel> model,
                DataStoreCreator dataStoreCreator,
                DataStoreConnection connection,
                ILoggerFactory loggerFactory)
                : base(model, dataStoreCreator, connection, loggerFactory)
            {
            }
        }

        private class ConcreteMigrationsEnabledDatabase : MigrationsEnabledDatabase
        {
            public ConcreteMigrationsEnabledDatabase(
                DbContextService<IModel> model,
                DataStoreCreator dataStoreCreator,
                DataStoreConnection connection,
                Migrator migrator,
                ILoggerFactory loggerFactory)
                : base(model, dataStoreCreator, connection, migrator, loggerFactory)
            {
            }
        }
    }
}
