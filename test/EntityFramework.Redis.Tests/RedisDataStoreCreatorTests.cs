// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;

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
            Assert.False(creator.EnsureCreated(model));
        }

        [Fact]
        public void Ensure_creation_async()
        {
            var model = Mock.Of<IModel>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();
            Assert.False(creator.EnsureCreatedAsync(model).Result);
        }

        [Fact]
        public void Ensure_deletion()
        {
            var model = Mock.Of<IModel>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            Assert.True(creator.EnsureDeleted(model));
        }

        [Fact]
        public void Ensure_deletion_async()
        {
            var model = Mock.Of<IModel>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            Assert.True(creator.EnsureDeletedAsync(model).Result);
        }
    }
}
