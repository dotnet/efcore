// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Conventions
{
    public class KeyDiscoveryConventionTest
    {
        private class EntityWithNoId
        {
            public string Name { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        [Fact]
        public void Primary_key_is_not_set_when_zero_key_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);
        }

        [Fact]
        public void Composite_primary_key_is_set_when_multiple_key_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();
            var convention = new Mock<KeyDiscoveryConvention> { CallBase = true };
            convention.Setup(c => c.DiscoverKeyProperties(It.IsAny<EntityType>()))
                .Returns<EntityType>(t => t.Properties.ToList());

            Assert.Same(entityBuilder, convention.Object.Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
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

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithTypeId
        {
            public int EntityWithTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithTypeId>();

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "EntityWithTypeIdId" }, key.Properties.Select(p => p.Name));
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

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithMultipleIds
        {
            public int ID { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_does_not_discover_key_when_multiple_ids()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithMultipleIds>();

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);
        }

        private static InternalEntityTypeBuilder CreateInternalEntityBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.Convention);

            new PropertyDiscoveryConvention().Apply(entityBuilder);

            return entityBuilder;
        }
    }
}
