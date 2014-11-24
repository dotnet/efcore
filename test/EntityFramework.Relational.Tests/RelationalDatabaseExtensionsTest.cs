// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
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
                new DbContextService<IModel>(() => null),
                Mock.Of<DataStoreCreator>(),
                Mock.Of<DataStoreConnection>(),
                new LoggerFactory());

            Assert.Same(database, database.AsRelational());
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
                Strings.RelationalNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsRelational()).Message);
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

        private class ConcreteRelationalDatabase : RelationalDatabase
        {
            public ConcreteRelationalDatabase(
                DbContextService<IModel> model,
                DataStoreCreator dataStoreCreator,
                DataStoreConnection connection,
                ILoggerFactory loggerFactory)
                : base(model, dataStoreCreator, connection, loggerFactory)
            {
            }
        }
    }
}
