// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Conventions.Internal
{
    public class DiscriminatorConventionTest
    {
        [Fact]
        public void Creates_discriminator_property_and_sets_discriminator_value_for_entityType()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();
            var discriminatorProperty = entityTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty;

            Assert.NotNull(discriminatorProperty);
            Assert.Equal("Discriminator", discriminatorProperty.Name);
            Assert.Equal(typeof(string), discriminatorProperty.ClrType);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
        }

        [Fact]
        public void Sets_discriminator_for_two_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            var discriminator = entityTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty;

            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);

            Assert.NotNull(entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
            Assert.NotSame(baseTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty,
                           entityTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
        }

        [Fact]
        public void Sets_discriminator_for_three_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();
            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            var derivedTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(DerivedEntity), ConfigurationSource.Explicit);

            var baseDiscriminator = baseTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty;
            var discriminator = entityTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty;
            var derivedDiscriminator = derivedTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty;

            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseDiscriminator);
            Assert.Same(discriminator, derivedDiscriminator);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);

            Assert.NotNull(entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
            Assert.NotSame(baseTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty,
                           entityTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty);
            Assert.Same(entityTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty,
                           derivedTypeBuilder.Metadata.CosmosSql().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.CosmosSql().DiscriminatorValue);
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
            var modelBuilder = CosmosSqlTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }
    }
}
