// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis.Query;
using Microsoft.Data.Entity.Services;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests.Query
{
    public class RedisQueryContextTests
    {
        [Fact]
        public void Can_construct_RedisQueryContext()
        {
            var model = QueryTestType.Model();
            var logger = NullLogger.Instance;
            var configurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(configurationMock.Object);
            var stateManagerMock = new Mock<StateManager>();
            var stateManager = stateManagerMock.Object;

            var redisQueryContext = new RedisQueryContext(model, logger, stateManager, redisDatabaseMock.Object);

            Assert.Equal(model, redisQueryContext.Model);
            Assert.Equal(logger, redisQueryContext.Logger);
        }

        [Fact]
        public void GetResultsFromRedis_EntityType_calls_RedisDatabase_GetMaterializedResults()
        {
            var model = QueryTestType.Model();
            var logger = NullLogger.Instance;
            var configurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(configurationMock.Object);
            var stateManagerMock = new Mock<StateManager>();
            var redisQueryContext = new RedisQueryContext(model, logger, stateManagerMock.Object, redisDatabaseMock.Object);
            var entityType = QueryTestType.EntityType();
            var redisQuery = new RedisQuery(entityType);

            redisQueryContext.GetResultsFromRedis<QueryTestType>(entityType);

            redisDatabaseMock.Verify(m => m.GetMaterializedResults<QueryTestType>(entityType), Times.Once);
            redisDatabaseMock.Verify(m => m.GetResults(redisQuery), Times.Never);
        }

        [Fact]
        public void GetResultsFromRedis_RedisQuery_calls_RedisDatabase_GetResults()
        {
            var model = QueryTestType.Model();
            var logger = NullLogger.Instance;
            var configurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(configurationMock.Object);
            var stateManagerMock = new Mock<StateManager>();
            var redisQueryContext = new RedisQueryContext(model, logger, stateManagerMock.Object, redisDatabaseMock.Object);
            var entityType = QueryTestType.EntityType();
            var redisQuery = new RedisQuery(entityType);

            redisQueryContext.GetResultsFromRedis(redisQuery);

            redisDatabaseMock.Verify(m => m.GetResults(redisQuery), Times.Once);
            redisDatabaseMock.Verify(m => m.GetMaterializedResults<QueryTestType>(entityType), Times.Never);
        }
    }
}
