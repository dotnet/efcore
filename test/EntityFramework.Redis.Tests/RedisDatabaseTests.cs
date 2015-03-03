// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisDatabaseTests
    {
        [Fact]
        public void Delegates_to_datastore_creator()
        {
            var model = Mock.Of<IModel>();
            var connection = Mock.Of<RedisConnection>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            var database = new RedisDatabase(new DbContextService<IModel>(() => model),
                creator,
                connection,
                new LoggerFactory());

            Assert.True(database.EnsureCreated());

            Assert.True(database.EnsureDeleted());

            Assert.Same(connection, database.Connection);
        }

        // TODO: add some tests for other methods in RedisDatabase
    }
}
