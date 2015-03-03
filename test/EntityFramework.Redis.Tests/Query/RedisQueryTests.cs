// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Query;
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
    }
}
