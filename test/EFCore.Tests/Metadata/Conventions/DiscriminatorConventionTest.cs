// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class DiscriminatorConventionTest
    {
        [ConditionalFact]
        public void Sets_discriminator_for_two_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            RunConvention(entityTypeBuilder, null);

            Assert.Null(((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Null(entityTypeBuilder.Metadata.GetDiscriminatorValue());

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            Assert.Same(entityTypeBuilder, entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation));

            RunConvention(entityTypeBuilder, null);

            var discriminator = ((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty();

            Assert.NotNull(discriminator);
            Assert.Same(discriminator, ((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.GetDiscriminatorValue());

            Assert.NotNull(entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
            RunConvention(entityTypeBuilder, baseTypeBuilder.Metadata);
            Assert.Null(((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Null(((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty());
        }

        [ConditionalFact]
        public void Sets_discriminator_for_three_level_hierarchy()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            RunConvention(entityTypeBuilder, null);

            Assert.Null(((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Null(entityTypeBuilder.Metadata.GetDiscriminatorValue());

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
            Assert.Same(entityTypeBuilder, entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation));

            RunConvention(entityTypeBuilder, null);

            var derivedTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(DerivedEntity), ConfigurationSource.Explicit);
            Assert.Same(derivedTypeBuilder, derivedTypeBuilder.HasBaseType(entityTypeBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.Same(
                derivedTypeBuilder.Metadata,
                entityTypeBuilder.ModelBuilder.Entity(typeof(DerivedEntity).FullName, ConfigurationSource.Convention).Metadata);

            RunConvention(entityTypeBuilder, null);

            var discriminator = ((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty();
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, ((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Same(discriminator, ((IEntityType)derivedTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.GetDiscriminatorValue());

            entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation);

            RunConvention(entityTypeBuilder, baseTypeBuilder.Metadata);

            Assert.Null(((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            discriminator = ((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty();
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, ((IEntityType)derivedTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Uses_explicit_discriminator_if_compatible()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.DataAnnotation);
            entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation);
            new EntityTypeBuilder(baseTypeBuilder.Metadata).HasDiscriminator("T", typeof(string));

            RunConvention(entityTypeBuilder, null);

            var discriminator = ((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty();
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, ((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Equal("T", discriminator.Name);
            Assert.Equal(typeof(string), discriminator.ClrType);
            Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Does_nothing_if_explicit_discriminator_is_not_compatible()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.DataAnnotation);
            entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation);
            new EntityTypeBuilder(baseTypeBuilder.Metadata).HasDiscriminator("T", typeof(int));

            RunConvention(entityTypeBuilder, null);

            var discriminator = ((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty();
            Assert.NotNull(discriminator);
            Assert.Same(discriminator, ((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Equal("T", discriminator.Name);
            Assert.Equal(typeof(int), discriminator.ClrType);
            Assert.Null(baseTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Null(entityTypeBuilder.Metadata.GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Does_nothing_if_explicit_discriminator_set_on_derived_type()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

            new EntityTypeBuilder(entityTypeBuilder.Metadata).HasDiscriminator("T", typeof(string));

            var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Convention);
            entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.Convention);

            RunConvention(entityTypeBuilder, null);

            Assert.Null(((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Null(((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Null(baseTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Null(entityTypeBuilder.Metadata.GetDiscriminatorValue());

            entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation);

            RunConvention(entityTypeBuilder, baseTypeBuilder.Metadata);

            Assert.Null(((IEntityType)baseTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.NotNull(((IEntityType)entityTypeBuilder.Metadata).GetDiscriminatorProperty());
            Assert.Null(baseTypeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Null(entityTypeBuilder.Metadata.GetDiscriminatorValue());
        }

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            var context = new ConventionContext<IConventionEntityType>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);
            new DiscriminatorConvention(CreateDependencies())
                .ProcessEntityTypeBaseTypeChanged(
                    entityTypeBuilder, entityTypeBuilder.Metadata.BaseType, oldBaseType, context);
            Assert.False(context.ShouldStopProcessing());
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

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
            var modelBuilder = (InternalModelBuilder)
                InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }
    }
}
