// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal
{
    public class DiscriminatorConventionTest
    {
        [Fact]
        public void Creates_discriminator_property_and_sets_discriminator_value_for_entityType()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();
            var discriminatorProperty = entityTypeBuilder.Metadata.Cosmos().DiscriminatorProperty;

            Assert.NotNull(discriminatorProperty);
            Assert.Equal("Discriminator", discriminatorProperty.Name);
            Assert.Equal(typeof(string), discriminatorProperty.ClrType);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
        }

        [Fact]
        public void Sets_discriminator_for_two_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            var discriminator = entityTypeBuilder.Metadata.Cosmos().DiscriminatorProperty;

            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseTypeBuilder.Metadata.Cosmos().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Cosmos().DiscriminatorValue);

            Assert.NotNull(entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
            Assert.NotSame(baseTypeBuilder.Metadata.Cosmos().DiscriminatorProperty,
                           entityTypeBuilder.Metadata.Cosmos().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
        }

        [Fact]
        public void Sets_discriminator_for_three_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();
            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            var derivedTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(DerivedEntity), ConfigurationSource.Explicit);

            var baseDiscriminator = baseTypeBuilder.Metadata.Cosmos().DiscriminatorProperty;
            var discriminator = entityTypeBuilder.Metadata.Cosmos().DiscriminatorProperty;
            var derivedDiscriminator = derivedTypeBuilder.Metadata.Cosmos().DiscriminatorProperty;

            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseDiscriminator);
            Assert.Same(discriminator, derivedDiscriminator);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.Cosmos().DiscriminatorValue);

            Assert.NotNull(entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
            Assert.NotSame(baseTypeBuilder.Metadata.Cosmos().DiscriminatorProperty,
                           entityTypeBuilder.Metadata.Cosmos().DiscriminatorProperty);
            Assert.Same(entityTypeBuilder.Metadata.Cosmos().DiscriminatorProperty,
                           derivedTypeBuilder.Metadata.Cosmos().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.Cosmos().DiscriminatorValue);
        }

        private class EntityBase
        {
        }

        private class Entity : EntityBase
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class DerivedEntity : Entity
        {
        }

        private static InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }
    }
}
