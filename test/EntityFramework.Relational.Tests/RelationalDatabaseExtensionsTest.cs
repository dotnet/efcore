// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalDatabaseExtensionsTest
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var database = new ConcreteRelationalDatabase(
                TestHelpers.Instance.CreateContext(),
                Mock.Of<RelationalDataStoreCreator>(),
                Mock.Of<RelationalConnection>(),
                Mock.Of<Migrator>(),
                new LoggerFactory());

            Assert.Same(database, database.AsRelational());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var database = new ConcreteDatabase(
                TestHelpers.Instance.CreateContext(),
                Mock.Of<RelationalDataStoreCreator>(),
                Mock.Of<RelationalConnection>(),
                new LoggerFactory());

            Assert.Equal(
                Strings.RelationalNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsRelational()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(
                DbContext context,
                RelationalDataStoreCreator dataStoreCreator,
                RelationalConnection connection,
                ILoggerFactory loggerFactory)
                : base(context, dataStoreCreator, connection, loggerFactory)
            {
            }
        }

        private class ConcreteRelationalDatabase : RelationalDatabase
        {
            public ConcreteRelationalDatabase(
                DbContext context,
                RelationalDataStoreCreator dataStoreCreator,
                RelationalConnection connection,
                Migrator migrator,
                ILoggerFactory loggerFactory)
                : base(context, dataStoreCreator, connection, migrator, loggerFactory)
            {
            }
        }
    }
}
