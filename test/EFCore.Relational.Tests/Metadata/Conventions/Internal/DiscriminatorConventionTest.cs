﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class DiscriminatorConventionTest
    {
        [Fact]
        public void Sets_discriminator_for_two_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: null));

            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorValue);

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            Assert.Same(entityTypeBuilder, entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: null));

            var discriminator = entityTypeBuilder.Metadata.Relational().DiscriminatorProperty;

            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Relational().DiscriminatorValue);

            Assert.NotNull(entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: baseTypeBuilder.Metadata));
            Assert.Null(baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorProperty);
        }

        [Fact]
        public void Sets_discriminator_for_three_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: null));

            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorValue);

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            Assert.Same(entityTypeBuilder, entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: null));

            var derivedTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(DerivedEntity), ConfigurationSource.Explicit);
            Assert.Same(derivedTypeBuilder, derivedTypeBuilder.HasBaseType(entityTypeBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.Same(derivedTypeBuilder.Metadata, entityTypeBuilder.ModelBuilder.Entity(typeof(DerivedEntity).FullName, ConfigurationSource.Convention).Metadata);

            Assert.True(new DiscriminatorConvention().Apply(derivedTypeBuilder, oldBaseType: null));

            var discriminator = entityTypeBuilder.Metadata.Relational().DiscriminatorProperty;
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Same(discriminator, derivedTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.Relational().DiscriminatorValue);

            entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation);
            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: baseTypeBuilder.Metadata));

            Assert.Null(baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            discriminator = entityTypeBuilder.Metadata.Relational().DiscriminatorProperty;
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, derivedTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.Relational().DiscriminatorValue);
        }

        [Fact]
        public void Uses_explicit_discriminator_if_compatible()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.DataAnnotation);
            entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation);
            baseTypeBuilder.Relational(ConfigurationSource.Explicit).HasDiscriminator("T", typeof(string));

            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: null));

            var discriminator = entityTypeBuilder.Metadata.Relational().DiscriminatorProperty;
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Equal("T", discriminator.Name);
            Assert.Equal(typeof(string), discriminator.ClrType);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.Relational().DiscriminatorValue);
        }

        [Fact]
        public void Does_nothing_if_explicit_discriminator_is_not_compatible()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.DataAnnotation);
            entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation);
            baseTypeBuilder.Relational(ConfigurationSource.Explicit).HasDiscriminator("T", typeof(int));

            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: null));

            var discriminator = entityTypeBuilder.Metadata.Relational().DiscriminatorProperty;
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Equal("T", discriminator.Name);
            Assert.Equal(typeof(int), discriminator.ClrType);
            Assert.Null(baseTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorValue);
        }

        [Fact]
        public void Does_nothing_if_explicit_discriminator_set_on_derived_type()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            entityTypeBuilder.Relational(ConfigurationSource.Explicit).HasDiscriminator("T", typeof(string));

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Convention);
            entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.Convention);

            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: null));

            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Null(baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Null(baseTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorValue);

            entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation);
            Assert.True(new DiscriminatorConvention().Apply(entityTypeBuilder, oldBaseType: baseTypeBuilder.Metadata));

            Assert.Null(baseTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.NotNull(entityTypeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Null(baseTypeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Null(entityTypeBuilder.Metadata.Relational().DiscriminatorValue);
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
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }
    }
}
