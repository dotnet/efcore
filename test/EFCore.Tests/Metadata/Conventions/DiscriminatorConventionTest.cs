// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class DiscriminatorConventionTest
{
    [ConditionalFact]
    public void Sets_discriminator_for_two_level_hierarchy()
    {
        var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

        RunConvention(entityTypeBuilder, null);

        Assert.Null(((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Null(entityTypeBuilder.Metadata.GetDiscriminatorValue());

        var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Explicit);
        Assert.Same(entityTypeBuilder, entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.DataAnnotation));

        RunConvention(entityTypeBuilder, null);

        var discriminator = ((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty();

        Assert.NotNull(discriminator);
        Assert.Same(discriminator, ((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.GetDiscriminatorValue());

        Assert.NotNull(entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
        RunConvention(entityTypeBuilder, baseTypeBuilder.Metadata);
        Assert.Null(((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Null(((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty());
    }

    [ConditionalFact]
    public void Sets_discriminator_for_three_level_hierarchy()
    {
        var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

        RunConvention(entityTypeBuilder, null);

        Assert.Null(((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty());
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

        var discriminator = ((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty();
        Assert.NotNull(discriminator);
        Assert.Same(discriminator, ((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Same(discriminator, ((IReadOnlyEntityType)derivedTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Equal(typeof(EntityBase).Name, baseTypeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(typeof(Entity).Name, entityTypeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(typeof(DerivedEntity).Name, derivedTypeBuilder.Metadata.GetDiscriminatorValue());

        entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation);

        RunConvention(entityTypeBuilder, baseTypeBuilder.Metadata);

        Assert.Null(((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
        discriminator = ((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty();
        Assert.NotNull(discriminator);
        Assert.Same(discriminator, ((IReadOnlyEntityType)derivedTypeBuilder.Metadata).FindDiscriminatorProperty());
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

        var discriminator = ((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty();
        Assert.NotNull(discriminator);
        Assert.Same(discriminator, ((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
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

        var discriminator = ((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty();
        Assert.NotNull(discriminator);
        Assert.Same(discriminator, ((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Equal("T", discriminator.Name);
        Assert.Equal(typeof(int), discriminator.ClrType);
        Assert.Null(baseTypeBuilder.Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.Null(entityTypeBuilder.Metadata[CoreAnnotationNames.DiscriminatorValue]);
    }

    [ConditionalFact]
    public void Does_nothing_if_explicit_discriminator_set_on_derived_type()
    {
        var entityTypeBuilder = CreateInternalEntityTypeBuilder<Entity>();

        new EntityTypeBuilder(entityTypeBuilder.Metadata).HasDiscriminator("T", typeof(int));

        var baseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(typeof(EntityBase), ConfigurationSource.Convention);
        entityTypeBuilder.HasBaseType(baseTypeBuilder.Metadata, ConfigurationSource.Convention);

        RunConvention(entityTypeBuilder, null);

        Assert.Null(((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Null(((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.Null(baseTypeBuilder.Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.Null(entityTypeBuilder.Metadata[CoreAnnotationNames.DiscriminatorValue]);

        entityTypeBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation);

        RunConvention(entityTypeBuilder, baseTypeBuilder.Metadata);

        Assert.Null(((IReadOnlyEntityType)baseTypeBuilder.Metadata).FindDiscriminatorProperty());
        Assert.NotNull(((IReadOnlyEntityType)entityTypeBuilder.Metadata).FindDiscriminatorProperty());
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

    private class EntityBase;

    private class Entity : EntityBase
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private class DerivedEntity : Entity;

    private static InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
    {
        var modelBuilder = (InternalModelBuilder)
            InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

        return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
    }
}
