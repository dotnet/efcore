// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDatabaseExtensionsTest
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var database = new InMemoryDatabaseFacade(
                TestHelpers.Instance.CreateContext(),
                Mock.Of<IInMemoryDataStoreCreator>(),
                Mock.Of<IInMemoryConnection>(),
                new LoggerFactory());

            Assert.Same(database, database.AsInMemory());
        }

        [Fact]
        public void Throws_when_non_relational_provider_is_in_use()
        {
            var database = new ConcreteDatabase(
                TestHelpers.Instance.CreateContext(),
                Mock.Of<IDataStoreCreator>(),
                Mock.Of<IDataStoreConnection>(),
                new LoggerFactory());

            Assert.Equal(
                Strings.InMemoryNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsInMemory()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(
                DbContext context,
                IDataStoreCreator dataStoreCreator,
                IDataStoreConnection connection,
                ILoggerFactory loggerFactory)
                : base(context, dataStoreCreator, connection, loggerFactory)
            {
            }
        }
    }
}
