// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Framework.Logging;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisDataStoreCreatorTests
    {
        [Fact]
        public void Ensure_creation()
        {
            var model = Mock.Of<IModel>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();
            Assert.True(creator.EnsureCreated(model));
        }

        [Fact]
        public void Ensure_creation_async()
        {
            var model = Mock.Of<IModel>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();
            Assert.True(creator.EnsureCreatedAsync(model).Result);
        }

        [Fact]
        public void Ensure_deletion()
        {
            var model = Mock.Of<IModel>();
            var redisDataStoreMock = new Mock<RedisDataStore>(Mock.Of<StateManager>(),
                new DbContextService<IModel>(() => model),
                Mock.Of<EntityKeyFactorySource>(),
                Mock.Of<EntityMaterializerSource>(),
                Mock.Of<ClrCollectionAccessorSource>(),
                Mock.Of<ClrPropertySetterSource>(),
                Mock.Of<RedisConnection>(),
                new LoggerFactory());
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInstance<RedisDataStore>(redisDataStoreMock.Object);
            redisDataStoreMock.Setup(s => s.FlushDatabase());
            var contextServices = TestHelpers.CreateContextServices(serviceCollection);
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            Assert.True(creator.EnsureDeleted(model));
            redisDataStoreMock.Verify(m => m.FlushDatabase(), Times.Once);
        }

        [Fact]
        public void Ensure_deletion_async()
        {
            var model = Mock.Of<IModel>();
            var redisDataStoreMock = new Mock<RedisDataStore>(Mock.Of<StateManager>(),
                new DbContextService<IModel>(() => model),
                Mock.Of<EntityKeyFactorySource>(),
                Mock.Of<EntityMaterializerSource>(),
                Mock.Of<ClrCollectionAccessorSource>(),
                Mock.Of<ClrPropertySetterSource>(),
                Mock.Of<RedisConnection>(),
                new LoggerFactory());
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInstance<RedisDataStore>(redisDataStoreMock.Object);
            redisDataStoreMock.Setup(s => s.FlushDatabaseAsync(CancellationToken.None)).Returns(Task.FromResult(true));
            var contextServices = TestHelpers.CreateContextServices(serviceCollection);
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            Assert.True(creator.EnsureDeletedAsync(model).Result);
            redisDataStoreMock.Verify(m => m.FlushDatabaseAsync(CancellationToken.None), Times.Once);
        }
    }
}
