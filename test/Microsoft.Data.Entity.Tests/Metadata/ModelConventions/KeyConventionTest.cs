// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class KeyConventionTest
    {
        private class EntityWithNoId
        {
            public string Name { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        [Fact]
        public void ConfigureKey_is_noop_when_zero_key_properties()
        {
            var entityType = CreateEntityType<EntityWithNoId>();

            new KeyConvention().Apply(entityType);

            var key = entityType.TryGetKey();
            Assert.Null(key);
        }

        [Fact]
        public void ConfigureKey_handles_multiple_key_properties()
        {
            var entityType = CreateEntityType<EntityWithNoId>();
            var convention = new Mock<KeyConvention>() { CallBase = true };
            convention.Protected().Setup<IEnumerable<Property>>("DiscoverKeyProperties", ItExpr.IsAny<EntityType>())
                .Returns<EntityType>(t => t.Properties);

            convention.Object.Apply(entityType);

            var key = entityType.TryGetKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "ModifiedDate", "Name" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithId
        {
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_id()
        {
            var entityType = CreateEntityType<EntityWithId>();

            new KeyConvention().Apply(entityType);

            var key = entityType.TryGetKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
            Assert.Equal(ValueGenerationStrategy.StoreIdentity, key.Properties.Single().ValueGenerationStrategy);
        }

        private class EntityWithTypeId
        {
            public int EntityWithTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_type_id()
        {
            var entityType = CreateEntityType<EntityWithTypeId>();

            new KeyConvention().Apply(entityType);

            var key = entityType.TryGetKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "EntityWithTypeIdId" }, key.Properties.Select(p => p.Name));
            Assert.Equal(ValueGenerationStrategy.StoreIdentity, key.Properties.Single().ValueGenerationStrategy);
        }

        private class EntityWithIdAndTypeId
        {
            public int Id { get; set; }
            public int EntityWithIdAndTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_prefers_id_over_type_id()
        {
            var entityType = CreateEntityType<EntityWithIdAndTypeId>();

            new KeyConvention().Apply(entityType);

            var key = entityType.TryGetKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
            Assert.Equal(ValueGenerationStrategy.StoreIdentity, key.Properties.Single().ValueGenerationStrategy);
        }

        private class EntityWithMultipleIds
        {
            public int ID { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_throws_when_multiple_ids()
        {
            var entityType = CreateEntityType<EntityWithMultipleIds>();
            var convention = new KeyConvention();

            var ex = Assert.Throws<InvalidOperationException>(() => convention.Apply(entityType));

            Assert.Equal(
                Strings.FormatMultiplePropertiesMatchedAsKeys("ID", typeof(EntityWithMultipleIds).Name),
                ex.Message);
        }

        private class EntityWithGuidKey
        {
            public Guid Id { get; set; }
        }

        [Fact]
        public void ConfigureKeyProperty_sets_generation_strategy_when_guid()
        {
            var entityType = CreateEntityType<EntityWithGuidKey>();

            new KeyConvention().Apply(entityType);

            var property = entityType.TryGetProperty("Id");
            Assert.NotNull(property);
            Assert.Equal(ValueGenerationStrategy.Client, property.ValueGenerationStrategy);
        }

        private static EntityType CreateEntityType<T>()
        {
            var entityType = new EntityType(typeof(T));
            new PropertiesConvention().Apply(entityType);

            return entityType;
        }
    }
}
