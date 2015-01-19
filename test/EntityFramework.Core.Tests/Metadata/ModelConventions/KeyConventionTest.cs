// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
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
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();

            Assert.Same(entityBuilder, new KeyConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.Null(key);
        }

        [Fact]
        public void ConfigureKey_handles_multiple_key_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();
            var convention = new Mock<KeyConvention> { CallBase = true };
            convention.Protected().Setup<IEnumerable<Property>>("DiscoverKeyProperties", ItExpr.IsAny<EntityType>())
                .Returns<EntityType>(t => t.Properties);

            Assert.Same(entityBuilder, convention.Object.Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
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
            var entityBuilder = CreateInternalEntityBuilder<EntityWithId>();

            Assert.Same(entityBuilder, new KeyConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
            Assert.Equal(true, key.Properties.Single().GenerateValueOnAdd);
        }

        private class EntityWithTypeId
        {
            public int EntityWithTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithTypeId>();

            Assert.Same(entityBuilder, new KeyConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "EntityWithTypeIdId" }, key.Properties.Select(p => p.Name));
            Assert.Equal(true, key.Properties.Single().GenerateValueOnAdd);
        }

        private class EntityWithIdAndTypeId
        {
            public int Id { get; set; }
            public int EntityWithIdAndTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_prefers_id_over_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithIdAndTypeId>();

            Assert.Same(entityBuilder, new KeyConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
            Assert.Equal(true, key.Properties.Single().GenerateValueOnAdd);
        }

        private class EntityWithMultipleIds
        {
            public int ID { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_throws_when_multiple_ids()
        {
            var entityType = CreateInternalEntityBuilder<EntityWithMultipleIds>();
            var convention = new KeyConvention();

            var ex = Assert.Throws<InvalidOperationException>(() => convention.Apply(entityType));

            Assert.Equal(
                Strings.MultiplePropertiesMatchedAsKeys("ID", typeof(EntityWithMultipleIds).FullName),
                ex.Message);
        }

        private class EntityWithGenericKey<T>
        {
            public T Id { get; set; }
        }

        [Fact]
        public void ConfigureKeyProperty_sets_generation_strategy_only_when_guid_or_common_integer()
        {
            ConfigureKeyProperty_generation_strategy<Guid>();
            ConfigureKeyProperty_generation_strategy<long>();
            ConfigureKeyProperty_generation_strategy<int>();
            ConfigureKeyProperty_generation_strategy<short>();
            ConfigureKeyProperty_generation_strategy<byte>();
            ConfigureKeyProperty_generation_strategy<long?>();
            ConfigureKeyProperty_generation_strategy<int?>();
            ConfigureKeyProperty_generation_strategy<short?>();
            ConfigureKeyProperty_generation_strategy<byte?>();
            ConfigureKeyProperty_generation_strategy<string>();
            ConfigureKeyProperty_generation_strategy<Enum1>();
            ConfigureKeyProperty_generation_strategy<Enum1?>();
            ConfigureKeyProperty_generation_strategy<bool>();
            ConfigureKeyProperty_generation_strategy<bool?>();
            ConfigureKeyProperty_generation_strategy<sbyte>();
            ConfigureKeyProperty_generation_strategy<uint>();
            ConfigureKeyProperty_generation_strategy<ulong>();
            ConfigureKeyProperty_generation_strategy<ushort>();
            ConfigureKeyProperty_generation_strategy<decimal>();
            ConfigureKeyProperty_generation_strategy<float>();
            ConfigureKeyProperty_generation_strategy<DateTime>();
        }

        private void ConfigureKeyProperty_generation_strategy<T>()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithGenericKey<T>>();

            Assert.Same(entityBuilder, new KeyConvention().Apply(entityBuilder));

            var property = entityBuilder.Metadata.TryGetProperty("Id");
            Assert.NotNull(property);
            Assert.True(property.GenerateValueOnAdd.Value);
        }

        private enum Enum1
        {
        }

        [Fact]
        public void ConfigureKeyProperty_does_not_override_generation_strategy_when_configured_explicitly()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithGenericKey<Guid>>();
            var property = entityBuilder.Metadata.TryGetProperty("Id");
            property.GenerateValueOnAdd = false;

            Assert.Same(entityBuilder, new KeyConvention().Apply(entityBuilder));

            Assert.Equal(false, property.GenerateValueOnAdd);
        }

        private static InternalEntityBuilder CreateInternalEntityBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionsDispatcher());
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.Convention);

            new PropertiesConvention().Apply(entityBuilder);

            return entityBuilder;
        }
    }
}
