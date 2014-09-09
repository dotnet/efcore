// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Query;
using Microsoft.Data.Entity.Services;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests.Query
{
    public class RedisQueryTests
    {
        [Fact]
        public void Constructor_stores_EntityType_and_produces_empty_SelectedProperties()
        {
            var entityType = QueryTestType.EntityType();

            var redisQuery = new RedisQuery(entityType);

            Assert.Equal(entityType, redisQuery.EntityType);
            Assert.Empty(redisQuery.SelectedProperties);
        }

        [Fact]
        public void AddProperty_updates_SelectedProperties_and_subsequent_GetProjectionIndex_is_correct()
        {
            // add 1 property
            var entityType = QueryTestType.EntityType();
            var redisQuery = new RedisQuery(entityType);
            var someValueProperty = entityType.GetProperty("SomeValue");

            redisQuery.AddProperty(someValueProperty);

            Assert.True((new List<IProperty> { someValueProperty }).SequenceEqual(redisQuery.SelectedProperties));
            Assert.Equal(0, redisQuery.GetProjectionIndex(someValueProperty));

            // add a different property
            var idProperty = entityType.GetProperty("Id");

            redisQuery.AddProperty(idProperty);

            Assert.True((new List<IProperty> { someValueProperty, idProperty }).SequenceEqual(redisQuery.SelectedProperties));
            Assert.Equal(0, redisQuery.GetProjectionIndex(someValueProperty));
            Assert.Equal(1, redisQuery.GetProjectionIndex(idProperty));

            // add the 1st property again - adds to end of list
            redisQuery.AddProperty(someValueProperty);

            Assert.True((new List<IProperty> { someValueProperty, idProperty, someValueProperty }).SequenceEqual(redisQuery.SelectedProperties));

            // Note: GetProjectionIndex(someValueProperty) returns the _first_ index at which that property is returned
            Assert.Equal(0, redisQuery.GetProjectionIndex(someValueProperty));
            Assert.Equal(1, redisQuery.GetProjectionIndex(idProperty));
        }

        [Fact]
        public void GetValueReaders_returns_ObjectArrayReaders_over_an_enumerable_of_object_arrays()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(configurationMock.Object);
            var materializationStrategyMock = new Mock<IQueryBuffer>();
            var stateManagerMockMock = new Mock<StateManager>();

            var redisQueryContextMock
                = new Mock<RedisQueryContext>(
                    QueryTestType.Model(),
                    NullLogger.Instance,
                    materializationStrategyMock.Object,
                    stateManagerMockMock.Object,
                    redisDatabaseMock.Object);

            var resultsFromDatabase = new List<object[]>
                {
                    new object[] { 1, "SomeValue1" },
                    new object[] { 2, "SomeValue2" }
                };
            redisQueryContextMock.Setup(m => m.GetResultsFromRedis(It.IsAny<RedisQuery>())).Returns(resultsFromDatabase);
            var entityType = QueryTestType.EntityType();
            var redisQuery = new RedisQuery(entityType);
            
            var readers = redisQuery.GetValueReaders(redisQueryContextMock.Object);

            Assert.Equal(2, readers.Count());
            var i = 1;
            foreach (var reader in readers)
            {
                Assert.Equal(i, reader.ReadValue<int>(0));
                Assert.Equal("SomeValue" + i, reader.ReadValue<string>(1));
                i++;
            }
        }
    }
}
