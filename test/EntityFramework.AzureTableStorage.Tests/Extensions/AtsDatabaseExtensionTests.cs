// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Extensions
{
    public class AtsDatabaseExtensionTests
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var database = new AtsDatabase(
                new ContextService<IModel>(() => null),
                Mock.Of<AtsDataStoreCreator>(),
                Mock.Of<AtsConnection>(),
                new LoggerFactory());

            Assert.Same(database, database.AsAzureTableStorage());
        }

        [Fact]
        public void Throws_when_non_ats_provider_is_in_use()
        {
            var database = new ConcreteDatabase(
                new ContextService<IModel>(() => null),
                Mock.Of<DataStoreCreator>(),
                Mock.Of<DataStoreConnection>(),
                new LoggerFactory());

            Assert.Equal(
                Strings.AtsDatabaseNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsAzureTableStorage()).Message);
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(
                ContextService<IModel> model,
                DataStoreCreator dataStoreCreator,
                DataStoreConnection connection,
                ILoggerFactory loggerFactory)
                : base(model, dataStoreCreator, connection, loggerFactory)
            {
            }
        }
    }
}
