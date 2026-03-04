// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class RelationshipDiscoveryConventionTest
{
    [ConditionalFact]
    public void Entity_type_is_not_discovered_if_ignored()
    {
        var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();
        entityBuilder.ModelBuilder.Ignore(typeof(OneToManyPrincipal).FullName, ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetProperties());
        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Equal(entityBuilder.Metadata.ClrType, entityBuilder.Metadata.Model.GetEntityTypes().Single().ClrType);
    }

    [ConditionalFact]
    public void Entity_type_is_not_discovered_if_navigation_is_ignored()
    {
        var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();
        entityBuilder.Ignore(OneToManyDependent.NavigationProperty.Name, ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetProperties());
        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Equal(entityBuilder.Metadata.ClrType, entityBuilder.Metadata.Model.GetEntityTypes().Single().ClrType);
    }

    [ConditionalFact]
    public void One_to_one_bidirectional_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), OneToOneDependent.NavigationProperty.Name, unique: true);
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_is_upgraded_to_one_to_one_bidirectional()
    {
        var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
        var dependentEntityBuilder =
            principalEntityBuilder.ModelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);

        principalEntityBuilder.HasRelationship(
                dependentEntityBuilder.Metadata, OneToOnePrincipal.NavigationProperty, null, ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.Convention);

        Assert.Same(dependentEntityBuilder, RunConvention(dependentEntityBuilder));

        VerifyRelationship(
            dependentEntityBuilder.Metadata.GetNavigations().Single(), OneToOnePrincipal.NavigationProperty.Name, unique: true);
        Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Two_one_to_many_unidirectional_are_upgraded_to_one_to_one_bidirectional()
    {
        var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
        var dependentEntityBuilder =
            principalEntityBuilder.ModelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);

        principalEntityBuilder.HasRelationship(
                dependentEntityBuilder.Metadata, OneToOnePrincipal.NavigationProperty, null, ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.Convention);

        dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, OneToOneDependent.NavigationProperty, null, ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.Convention);

        Assert.Same(dependentEntityBuilder, RunConvention(dependentEntityBuilder));

        VerifyRelationship(
            dependentEntityBuilder.Metadata.GetNavigations().Single(), OneToOnePrincipal.NavigationProperty.Name, unique: true);
        Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_is_not_upgraded_to_one_to_one_bidirectional_if_higher_source()
    {
        var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
        var dependentEntityBuilder =
            principalEntityBuilder.ModelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);

        principalEntityBuilder.HasRelationship(
                dependentEntityBuilder.Metadata, OneToOnePrincipal.NavigationProperty, null, ConfigurationSource.Explicit)
            .IsUnique(false, ConfigurationSource.Convention);

        Assert.Same(dependentEntityBuilder, RunConvention(dependentEntityBuilder));
        VerifyRelationship(principalEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
        VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
        Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>(OneToManyPrincipal.IgnoreNavigation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), null, unique: false);
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_is_upgraded_to_one_to_many_bidirectional()
    {
        var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
        var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(
            typeof(OneToManyDependent), ConfigurationSource.Convention);

        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, null, OneToManyPrincipal.NavigationProperty, ConfigurationSource.Convention);

        Assert.Same(dependentEntityBuilder, RunConvention(dependentEntityBuilder));

        VerifyRelationship(
            dependentEntityBuilder.Metadata.GetNavigations().Single(), OneToManyPrincipal.NavigationProperty.Name, unique: false);
        Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_is_not_upgraded_to_one_to_many_bidirectional_if_higher_source()
    {
        var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
        var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(
            typeof(OneToManyDependent), ConfigurationSource.Convention);

        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, null, OneToManyPrincipal.NavigationProperty, ConfigurationSource.Explicit);

        Assert.Same(dependentEntityBuilder, RunConvention(dependentEntityBuilder));

        VerifyRelationship(principalEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
        VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
        Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void One_to_many_bidirectional_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), OneToManyPrincipal.NavigationProperty.Name, unique: false);
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Many_to_one_unidirectional_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>(OneToManyDependent.IgnoreNavigation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), null, unique: false);
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Many_to_one_unidirectional_is_upgraded_to_many_to_one_bidirectional()
    {
        var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
        var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(
            typeof(OneToManyDependent), ConfigurationSource.Convention);

        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, OneToManyDependent.NavigationProperty, null, ConfigurationSource.Convention);

        Assert.Same(principalEntityBuilder, RunConvention(principalEntityBuilder));

        VerifyRelationship(
            principalEntityBuilder.Metadata.GetNavigations().Single(), OneToManyDependent.NavigationProperty.Name, unique: false);
        Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Many_to_one_unidirectional_is_not_upgraded_to_many_to_one_bidirectional_if_higher_source()
    {
        var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
        var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(
            typeof(OneToManyDependent), ConfigurationSource.Convention);

        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, OneToManyDependent.NavigationProperty, null, ConfigurationSource.Explicit);

        Assert.Same(principalEntityBuilder, RunConvention(principalEntityBuilder));

        VerifyRelationship(principalEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
        VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
        Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Many_to_one_bidirectional_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), OneToManyDependent.NavigationProperty.Name, unique: false);
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Many_to_many_skip_navigations_are_discovered_if_self_join()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManySelf = modelBuilder.Entity(typeof(ManyToManySelf), ConfigurationSource.Convention);

        manyToManySelf.PrimaryKey(new[] { nameof(ManyToManySelf.Id) }, ConfigurationSource.Convention);

        RunConvention(manyToManySelf);

        Assert.Equal(2, manyToManySelf.Metadata.GetSkipNavigations().Count());
        var navigationOnManyToManyFirst = manyToManySelf.Metadata.GetSkipNavigations().First();
        var navigationOnManyToManySecond = manyToManySelf.Metadata.GetSkipNavigations().Last();
        Assert.Equal(nameof(ManyToManySelf.ManyToManySelf1), navigationOnManyToManyFirst.Name);
        Assert.Equal(nameof(ManyToManySelf.ManyToManySelf2), navigationOnManyToManySecond.Name);
        Assert.Same(navigationOnManyToManyFirst.Inverse, navigationOnManyToManySecond);
        Assert.Same(navigationOnManyToManySecond.Inverse, navigationOnManyToManyFirst);
    }

    [ConditionalFact]
    public void Many_to_many_skip_navigations_are_not_discovered_if_relationship_should_be_on_ancestors()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var derivedManyToManyFirst = modelBuilder.Entity(typeof(DerivedManyToManyFirst), ConfigurationSource.Convention);
        var derivedManyToManySecond = modelBuilder.Entity(typeof(DerivedManyToManySecond), ConfigurationSource.Convention);

        derivedManyToManyFirst.PrimaryKey(new[] { nameof(DerivedManyToManyFirst.Id) }, ConfigurationSource.Convention);
        derivedManyToManySecond.PrimaryKey(new[] { nameof(DerivedManyToManySecond.Id) }, ConfigurationSource.Convention);

        RunConvention(derivedManyToManyFirst);

        Assert.Empty(derivedManyToManyFirst.Metadata.GetSkipNavigations());
        Assert.Empty(derivedManyToManySecond.Metadata.GetSkipNavigations());
    }

    [ConditionalFact]
    public void Many_to_many_bidirectional_sets_up_skip_navigations()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManyFirst = modelBuilder.Entity(typeof(ManyToManyFirst), ConfigurationSource.Convention);
        var manyToManySecond = modelBuilder.Entity(typeof(ManyToManySecond), ConfigurationSource.Convention);

        manyToManyFirst.PrimaryKey(new[] { nameof(ManyToManyFirst.Id) }, ConfigurationSource.Convention);
        manyToManySecond.PrimaryKey(new[] { nameof(ManyToManySecond.Id) }, ConfigurationSource.Convention);

        RunConvention(manyToManyFirst);

        var navigationOnManyToManyFirst = manyToManyFirst.Metadata.GetSkipNavigations().Single();
        var navigationOnManyToManySecond = manyToManySecond.Metadata.GetSkipNavigations().Single();
        Assert.Equal("ManyToManySeconds", navigationOnManyToManyFirst.Name);
        Assert.Equal("ManyToManyFirsts", navigationOnManyToManySecond.Name);
        Assert.Same(navigationOnManyToManyFirst.Inverse, navigationOnManyToManySecond);
        Assert.Same(navigationOnManyToManySecond.Inverse, navigationOnManyToManyFirst);
    }

    [ConditionalFact]
    public void Ambiguous_navigations_are_not_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Existing_relationship_is_removed_if_ambiguous()
    {
        var entityBuilderFirst =
            CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
        var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(
            typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

        entityBuilderFirst.HasRelationship(
            entityBuilderSecond.Metadata, MultipleNavigationsFirst.CollectionNavigationProperty, null, ConfigurationSource.Convention);

        Assert.Same(entityBuilderFirst, RunConvention(entityBuilderFirst));

        Assert.Empty(entityBuilderFirst.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilderFirst.Metadata.GetNavigations());
        Assert.Empty(entityBuilderSecond.Metadata.GetNavigations());
        Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Existing_relationship_removes_ambiguity_if_higher_source()
    {
        var entityBuilderFirst =
            CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
        var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(
            typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

        entityBuilderFirst.HasRelationship(
            entityBuilderSecond.Metadata, MultipleNavigationsFirst.CollectionNavigationProperty, null,
            ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilderFirst, RunConvention(entityBuilderFirst));

        VerifyRelationship(
            entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.CollectionNavigationProperty.Name), null, unique: false,
            singleRelationship: false);
        VerifyRelationship(
            entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.NonCollectionNavigationProperty.Name),
            nameof(MultipleNavigationsSecond.MultipleNavigationsFirst), unique: true, singleRelationship: false);
        Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigations_are_not_discovered_if_ambiguous_inverse()
    {
        var entityBuilderFirst = CreateInternalEntityBuilder<MultipleNavigationsSecond>(
            MultipleNavigationsSecond.IgnoreCollectionNavigation);
        var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(
            typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

        Assert.Same(entityBuilderFirst, RunConvention(entityBuilderFirst));

        Assert.Empty(entityBuilderFirst.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilderFirst.Metadata.GetNavigations());
        Assert.Empty(entityBuilderSecond.Metadata.GetNavigations());
        Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Existing_relationship_is_removed_if_ambiguous_inverse()
    {
        var entityBuilderFirst =
            CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
        var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(
            typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

        entityBuilderFirst.HasRelationship(
            entityBuilderSecond.Metadata, MultipleNavigationsFirst.CollectionNavigationProperty, null, ConfigurationSource.Convention);

        Assert.Same(entityBuilderSecond, RunConvention(entityBuilderSecond));

        Assert.Empty(entityBuilderFirst.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilderFirst.Metadata.GetNavigations());
        Assert.Empty(entityBuilderSecond.Metadata.GetNavigations());
        Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());

        var logEntry = ListLoggerFactory.Log[0];
        Assert.Equal(LogLevel.Debug, logEntry.Level);
        Assert.Equal(
            CoreResources.LogMultipleNavigationProperties(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(MultipleNavigationsSecond),
                nameof(MultipleNavigationsFirst),
                "{'MultipleNavigationsFirst'}",
                "{'MultipleNavigationsSecond', 'MultipleNavigationsSeconds'}"), logEntry.Message);
    }

    [ConditionalFact]
    public void Existing_relationship_removes_ambiguity_in_inverse_if_higher_source()
    {
        var entityBuilderFirst =
            CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
        var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(
            typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

        entityBuilderFirst.HasRelationship(
            entityBuilderSecond.Metadata, MultipleNavigationsFirst.CollectionNavigationProperty, null,
            ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilderSecond, RunConvention(entityBuilderSecond));

        VerifyRelationship(
            entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.CollectionNavigationProperty.Name), null, unique: false,
            singleRelationship: false);
        VerifyRelationship(
            entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.NonCollectionNavigationProperty.Name),
            nameof(MultipleNavigationsSecond.MultipleNavigationsFirst), unique: true, singleRelationship: false);
        Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Multiple_navigations_to_same_entity_type_are_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<MultipleNavigationsFirst>(
            MultipleNavigationsSecond.IgnoreCollectionNavigation, MultipleNavigationsSecond.IgnoreNonCollectionNavigation);
        entityBuilder.ModelBuilder.Entity(typeof(MultipleNavigationsSecond), ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var model = (IReadOnlyModel)entityBuilder.Metadata.Model;
        Assert.Equal(2, model.GetEntityTypes().Count());
        var firstEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(MultipleNavigationsFirst));
        var secondEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(MultipleNavigationsSecond));

        Assert.Equal(2, firstEntityType.GetProperties().Count());
        Assert.Single(firstEntityType.GetKeys());
        var firstFK = firstEntityType.GetForeignKeys().Single();
        Assert.False(firstFK.IsRequired);
        Assert.False(firstFK.IsUnique);
        Assert.Equal(
            new[]
            {
                MultipleNavigationsFirst.NonCollectionNavigationProperty.Name,
                MultipleNavigationsFirst.CollectionNavigationProperty.Name
            },
            firstEntityType.GetNavigations().Select(n => n.Name));

        Assert.Equal(2, secondEntityType.GetProperties().Count());
        Assert.Single(secondEntityType.GetKeys());
        var secondFK = firstEntityType.GetForeignKeys().Single();
        Assert.False(secondFK.IsRequired);
        Assert.False(secondFK.IsUnique);
        Assert.Empty(secondEntityType.GetNavigations());
    }

    [ConditionalFact]
    public void Navigations_to_base_and_derived_are_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            Base.IgnoreBaseNavigation,
            DerivedOne.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var baseFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
        var derivedFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
        Assert.Empty(baseFk.FindNavigationsTo(entityBuilder.Metadata));
        Assert.Empty(derivedFk.FindNavigationsTo(entityBuilder.Metadata));
        Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigations_to_base_and_derived_are_discovered_if_inverse_from_base()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            DerivedOne.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var baseFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
        var derivedFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
        Assert.Equal(nameof(Base.BaseNavigation), baseFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Empty(derivedFk.FindNavigationsTo(entityBuilder.Metadata));
        Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigations_to_derived_and_base_are_discovered_if_inverse_from_base()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation,
            DerivedTwo.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedTwo), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var baseFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
        var derivedFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedTwo)).ForeignKey;
        Assert.Equal(nameof(Base.BaseNavigation), baseFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Empty(derivedFk.FindNavigationsTo(entityBuilder.Metadata));
        Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigations_to_base_and_derived_are_discovered_if_inverse_from_derived()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            Base.IgnoreBaseNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var baseFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
        var derivedFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
        Assert.Empty(baseFk.FindNavigationsTo(entityBuilder.Metadata));
        Assert.Equal(nameof(DerivedOne.DerivedNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_pairs_to_base_and_derived_are_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>();
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);
        var derivedBuilderTwo = modelBuilder.Entity(typeof(DerivedTwo), ConfigurationSource.Explicit);
        derivedBuilderTwo.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var baseFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
        var derivedFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
        Assert.Equal(nameof(Base.BaseNavigation), baseFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Equal(nameof(DerivedOne.DerivedNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Equal(3, entityBuilder.Metadata.GetNavigations().Count());
        Assert.Equal(4, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_to_base_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));
        Cleanup(modelBuilder);

        VerifyRelationship(
            entityBuilder.Metadata.FindNavigation(nameof(NavigationsToBaseAndDerived.Base)),
            expectedInverseName: nameof(Base.BaseNavigation), unique: true);
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Existing_navigation_to_derived_is_promoted()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation,
            DerivedOne.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        derivedBuilder.HasRelationship(entityBuilder.Metadata, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifyRelationship(
            entityBuilder.Metadata.FindNavigation(nameof(NavigationsToBaseAndDerived.Base)),
            expectedInverseName: nameof(Base.BaseNavigation), unique: true);
        Assert.Empty(derivedBuilder.Metadata.GetDeclaredNavigations());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Existing_navigation_from_derived_is_promoted()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation,
            DerivedOne.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        derivedBuilder.HasRelationship(entityBuilder.Metadata, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

        Assert.Same(baseBuilder, RunConvention(baseBuilder));

        VerifyRelationship(
            baseBuilder.Metadata.FindNavigation(nameof(Base.BaseNavigation)),
            expectedInverseName: nameof(NavigationsToBaseAndDerived.Base), unique: true);
        Assert.Empty(derivedBuilder.Metadata.GetDeclaredNavigations());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_from_derived_is_not_discovered_if_ambiguous()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreBaseNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(derivedBuilder, RunConvention(derivedBuilder));

        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(derivedBuilder.Metadata.GetNavigations());
        Assert.Empty(derivedBuilder.Metadata.GetForeignKeys());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Existing_relationship_to_base_removes_ambiguity()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreBaseNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        baseBuilder.HasRelationship(entityBuilder.Metadata, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

        Assert.Same(derivedBuilder, RunConvention(derivedBuilder));

        VerifyRelationship(
            baseBuilder.Metadata.GetNavigations().Single(), expectedInverseName: null, unique: false, singleRelationship: false);
        VerifyRelationship(
            entityBuilder.Metadata.GetNavigations().Single(), nameof(DerivedOne.DerivedNavigation), unique: true,
            singleRelationship: false);
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_to_derived_is_not_discovered_if_inverse_ambiguous()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreBaseNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(derivedBuilder.Metadata.GetNavigations());
        Assert.Empty(derivedBuilder.Metadata.GetForeignKeys());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Existing_relationship_to_base_removes_ambiguity_in_derived_inverse()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreBaseNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        baseBuilder.HasRelationship(entityBuilder.Metadata, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifyRelationship(
            baseBuilder.Metadata.GetNavigations().Single(), expectedInverseName: null, unique: false, singleRelationship: false);
        VerifyRelationship(
            entityBuilder.Metadata.GetNavigations().Single(), nameof(DerivedOne.DerivedNavigation), unique: true,
            singleRelationship: false);
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_to_derived_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreBaseNavigation,
            Base.IgnoreBaseNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var derivedFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
        Assert.Equal(nameof(DerivedOne.DerivedNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Single(entityBuilder.Metadata.GetNavigations());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_to_derived_is_discovered_if_inverse_inherited()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreBaseNavigation,
            DerivedOne.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
        derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var derivedFk = entityBuilder.Metadata.GetNavigations()
            .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
        Assert.Equal(nameof(Base.BaseNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Single(entityBuilder.Metadata.GetNavigations());
        Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_to_base_is_not_discovered_if_base_ignored()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation,
            DerivedOne.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        modelBuilder.Ignore(typeof(Base), ConfigurationSource.Explicit);
        modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Navigation_to_derived_is_discovered_if_base_ignored()
    {
        var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
            NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
            DerivedOne.IgnoreDerivedNavigation);
        var modelBuilder = entityBuilder.ModelBuilder;
        modelBuilder.Ignore(typeof(Base), ConfigurationSource.Explicit);
        modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var derivedFk = entityBuilder.Metadata.FindNavigation(nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
        Assert.Equal(nameof(DerivedOne.BaseNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
        Assert.Single(entityBuilder.Metadata.GetNavigations());
        Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
    }

    [ConditionalFact]
    public void Does_not_throw_on_shadow_entity_types()
    {
        var entityBuilder = new InternalModelBuilder(new Model())
            .Entity("Shadow", ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));
    }

    [ConditionalFact]
    public void Bidirectional_ambiguous_cardinality_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<AmbiguousCardinalityOne>();

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var model = (IReadOnlyModel)entityBuilder.Metadata.Model;
        Assert.Equal(2, model.GetEntityTypes().Count());
        var firstEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityOne));
        var secondEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityTwo));

        var fk = firstEntityType.GetNavigations().Single().ForeignKey;
        Assert.Same(fk, secondEntityType.GetNavigations().Single().ForeignKey);
        Assert.True(fk.IsUnique);
    }

    [ConditionalFact]
    public void Unidirectional_ambiguous_cardinality_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<AmbiguousCardinalityOne>(
            AmbiguousCardinalityTwo.IgnoreNavigation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        var model = (IReadOnlyModel)entityBuilder.Metadata.Model;
        Assert.Equal(2, model.GetEntityTypes().Count());
        var firstEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityOne));
        var secondEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityTwo));

        var fk = firstEntityType.GetNavigations().Single().ForeignKey;
        Assert.Empty(secondEntityType.GetNavigations());
        Assert.False(fk.IsUnique);
    }

    [ConditionalFact]
    public void One_to_one_bidirectional_self_ref_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), nameof(SelfRef.SelfRef2), unique: true);
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_self_ref_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>(
            SelfRef.IgnoreNavigation2, SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: null, unique: false);
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_self_ref_is_upgraded_to_one_to_one_bidirectional()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

        entityBuilder.HasRelationship(entityBuilder.Metadata, nameof(SelfRef.SelfRef1), null, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), nameof(SelfRef.SelfRef2), unique: true);
    }

    [ConditionalFact]
    public void One_to_many_unidirectional_self_ref_is_not_upgraded_to_one_to_one_bidirectional_if_higher_source()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

        entityBuilder.HasRelationship(entityBuilder.Metadata, nameof(SelfRef.SelfRef1), null, ConfigurationSource.Explicit)
            .IsUnique(false, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifySelfRef(
            entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: null, unique: false,
            singleRelationship: false);
        VerifySelfRef(
            entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef2)), expectedInverseName: null, unique: false,
            singleRelationship: false);
    }

    [ConditionalFact]
    public void Ambiguous_self_ref_is_not_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation4);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Empty(entityBuilder.Metadata.GetProperties());
    }

    [ConditionalFact]
    public void Existing_unidirectional_self_ref_is_removed_if_ambiguous()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation4);

        entityBuilder.HasRelationship(entityBuilder.Metadata, nameof(SelfRef.SelfRef1), null, ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Empty(entityBuilder.Metadata.GetProperties());
    }

    [ConditionalFact]
    public void Existing_unidirectional_self_ref_removes_ambiguity_if_higher_source()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation4);

        entityBuilder.HasRelationship(entityBuilder.Metadata, nameof(SelfRef.SelfRef1), null, ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifySelfRef(
            entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: null, unique: false,
            singleRelationship: false);
        VerifySelfRef(
            entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef2)), expectedInverseName: nameof(SelfRef.SelfRef3),
            unique: false, singleRelationship: false);
    }

    [ConditionalFact]
    public void Existing_bidirectional_self_ref_is_removed_if_ambiguous()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>();

        entityBuilder.HasRelationship(
            entityBuilder.Metadata, nameof(SelfRef.SelfRef1), nameof(SelfRef.SelfRef3), ConfigurationSource.Convention);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Empty(entityBuilder.Metadata.GetProperties());

        var logEntry = ListLoggerFactory.Log[0];
        Assert.Equal(LogLevel.Debug, logEntry.Level);
        Assert.Equal(
            CoreResources.LogMultipleNavigationProperties(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(SelfRef), nameof(SelfRef), "{'SelfRef1'}", "{'SelfRef2', 'SelfRef3', 'SelfRef4'}"), logEntry.Message);
    }

    [ConditionalFact]
    public void Existing_bidirectional_self_ref_removes_ambiguity_if_higher_source()
    {
        var entityBuilder = CreateInternalEntityBuilder<SelfRef>();

        entityBuilder.HasRelationship(
            entityBuilder.Metadata, nameof(SelfRef.SelfRef1), nameof(SelfRef.SelfRef3), ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        VerifySelfRef(
            entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: nameof(SelfRef.SelfRef3),
            unique: false, singleRelationship: false);
        VerifySelfRef(
            entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef2)), expectedInverseName: nameof(SelfRef.SelfRef4),
            unique: false, singleRelationship: false);
    }

    [ConditionalFact]
    public void Navigation_to_abstract_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<AbstractClass>();

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        IReadOnlyModel model = entityBuilder.Metadata.Model;
        var entityType = model.GetEntityTypes().Single();

        Assert.Equal(2, entityType.GetProperties().Count());
        Assert.Single(entityType.GetKeys());

        var fk = entityType.GetForeignKeys().Single();
        Assert.False(fk.IsUnique);
        Assert.True(fk.PrincipalEntityType.ClrType.IsAbstract);
        Assert.Single(entityType.GetNavigations());
    }

    [ConditionalFact]
    public void Collection_navigation_without_setter_is_discovered()
    {
        var entityBuilder = CreateInternalEntityBuilder<ReadOnlyCollectionNavigationEntity>();

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        IReadOnlyModel model = entityBuilder.Metadata.Model;
        Assert.NotNull(model.FindEntityType(typeof(EntityWithNoValidNavigations)));
        var entityType = entityBuilder.Metadata;

        Assert.Equal(ReadOnlyCollectionNavigationEntity.NavigationProperty.Name, entityType.GetNavigations().First().Name);
    }

    [ConditionalFact]
    public void Does_not_discover_nonNavigation_properties()
    {
        var entityBuilder = CreateInternalEntityBuilder<EntityWithNoValidNavigations>();

        Assert.Same(entityBuilder, RunConvention(entityBuilder));

        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(entityBuilder.Metadata.GetNavigations());
        Assert.Empty(entityBuilder.Metadata.GetProperties());
    }

    public ListLoggerFactory ListLoggerFactory { get; }
        = new(l => l == DbLoggerCategory.Model.Name);

    private static IMemberClassifier CreateMemberClassifier()
        => new MemberClassifier(
            TestServiceFactory.Instance.Create<InMemoryTypeMappingSource>(),
            TestServiceFactory.Instance.Create<IParameterBindingFactories>());

    private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
    {
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
        var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
            ListLoggerFactory,
            options,
            new DiagnosticListener("Fake"),
            new TestLoggingDefinitions(),
            new NullDbContextLogger());
        return modelLogger;
    }

    private class EntityWithNoValidNavigations
    {
        public int Id { get; set; }

        public object Object { get; set; }

        public static OneToManyDependent Static { get; set; }

        public OneToManyDependent WriteOnly
        {
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        // ReSharper disable once UnusedParameter.Local
        public OneToManyDependent this[int index]
        {
            get => null;
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        public MyStruct Struct { get; set; }
        public IInterface Interface { get; set; }
    }

    private struct MyStruct
    {
        public int Id { get; set; }
    }

    private interface IInterface
    {
        int Id { get; set; }
    }

    private abstract class AbstractClass
    {
        public int Id { get; set; }
        public AbstractClass Abstract { get; set; }
    }

    private class ReadOnlyCollectionNavigationEntity
    {
        public static readonly PropertyInfo NavigationProperty =
            typeof(ReadOnlyCollectionNavigationEntity).GetProperty("CollectionNavigation", BindingFlags.Public | BindingFlags.Instance);

        public int Id { get; set; }

        public ICollection<EntityWithNoValidNavigations> CollectionNavigation { get; } = new List<EntityWithNoValidNavigations>();
    }

    private static void VerifyRelationship(
        Navigation navigation,
        string expectedInverseName,
        bool unique,
        bool singleRelationship = true)
    {
        IReadOnlyForeignKey fk = navigation.ForeignKey;
        Assert.Equal(expectedInverseName, navigation.Inverse?.Name);
        Assert.Equal(unique, fk.IsUnique);
        Assert.NotSame(fk.Properties.Single(), fk.PrincipalKey.Properties.Single());
        Assert.NotEqual(fk.PrincipalToDependent?.Name, fk.DependentToPrincipal?.Name);

        if (singleRelationship)
        {
            var principalEntityType = fk.PrincipalEntityType;
            Assert.Single(principalEntityType.GetDeclaredProperties());
            Assert.Single(principalEntityType.GetKeys());
            Assert.Empty(principalEntityType.GetDeclaredForeignKeys());
            if ((expectedInverseName == null)
                && navigation.IsOnDependent)
            {
                Assert.Empty(principalEntityType.GetNavigations());
            }

            var dependentEntityType = fk.DeclaringEntityType;
            Assert.Single(dependentEntityType.GetDeclaredProperties());
            Assert.Equal(principalEntityType.IsAssignableFrom(dependentEntityType) ? 1 : 0, dependentEntityType.GetKeys().Count());
            if ((expectedInverseName == null)
                && !navigation.IsOnDependent)
            {
                Assert.Empty(dependentEntityType.GetNavigations());
            }
        }
    }

    private static void VerifySelfRef(
        Navigation navigation,
        string expectedInverseName,
        bool unique,
        bool singleRelationship = true)
    {
        IReadOnlyForeignKey fk = navigation.ForeignKey;
        Assert.Single(fk.DeclaringEntityType.Model.GetEntityTypes());
        Assert.Equal(expectedInverseName, navigation.Inverse?.Name);
        Assert.Equal(unique, fk.IsUnique);
        Assert.NotSame(fk.Properties.Single(), fk.PrincipalKey.Properties.Single());
        Assert.NotEqual(fk.PrincipalToDependent?.Name, fk.DependentToPrincipal?.Name);

        var entityType = fk.DeclaringEntityType;
        if (singleRelationship)
        {
            Assert.Single(entityType.GetKeys());
            Assert.Single(entityType.GetForeignKeys());
            Assert.Equal(2, entityType.GetProperties().Count());
            Assert.Equal(expectedInverseName == null ? 1 : 2, entityType.GetNavigations().Count());
        }
        else
        {
            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Equal(2, entityType.GetForeignKeys().Count());
            Assert.Equal(4, entityType.GetProperties().Count());
        }
    }

    private InternalEntityTypeBuilder RunConvention(InternalEntityTypeBuilder entityBuilder)
    {
        var context = new ConventionContext<IConventionEntityTypeBuilder>(entityBuilder.Metadata.Model.ConventionDispatcher);
        CreateRelationshipDiscoveryConvention().ProcessEntityTypeAdded(entityBuilder, context);
        return context.ShouldStopProcessing() ? (InternalEntityTypeBuilder)context.Result : entityBuilder;
    }

    private RelationshipDiscoveryConvention CreateRelationshipDiscoveryConvention()
        => new(CreateDependencies());

    private void Cleanup(InternalModelBuilder modelBuilder)
        => new ModelCleanupConvention(CreateDependencies())
            .ProcessModelFinalizing(
                modelBuilder,
                new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher));

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>()
            with
            {
                Logger = CreateLogger()
            };

    private InternalModelBuilder CreateInternalModeBuilder(params Action<IConventionEntityTypeBuilder>[] onEntityAdded)
    {
        var conventions = new ConventionSet();
        if (onEntityAdded != null)
        {
            conventions.EntityTypeAddedConventions.Add(new TestModelChangeListener(onEntityAdded));

            var relationshipDiscoveryConvention = CreateRelationshipDiscoveryConvention();
            conventions.EntityTypeBaseTypeChangedConventions.Add(relationshipDiscoveryConvention);
            conventions.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);
            conventions.NavigationAddedConventions.Add(relationshipDiscoveryConvention);
            conventions.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);
        }

        return new Model(conventions).Builder;
    }

    private InternalEntityTypeBuilder CreateInternalEntityBuilder<T>(params Action<IConventionEntityTypeBuilder>[] onEntityAdded)
    {
        var modelBuilder = CreateInternalModeBuilder(onEntityAdded);
        var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.DataAnnotation);

        return entityBuilder;
    }

    private class TestModelChangeListener(Action<IConventionEntityTypeBuilder>[] onEntityAdded) : IEntityTypeAddedConvention
    {
        private readonly Action<IConventionEntityTypeBuilder>[] _onEntityAdded = onEntityAdded;

        public void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            foreach (var action in _onEntityAdded)
            {
                action(entityTypeBuilder);
            }
        }
    }

    private class OneToOnePrincipal
    {
        public static readonly PropertyInfo NavigationProperty =
            typeof(OneToOnePrincipal).GetProperty("OneToOneDependent", BindingFlags.Public | BindingFlags.Instance);

        public int Id { get; set; }
        public OneToOneDependent OneToOneDependent { get; set; }

        public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(OneToOnePrincipal))
            {
                entityTypeBuilder.Ignore(nameof(OneToOneDependent), ConfigurationSource.DataAnnotation);
            }
        }
    }

    private class OneToOneDependent
    {
        public static readonly PropertyInfo NavigationProperty =
            typeof(OneToOneDependent).GetProperty("OneToOnePrincipal", BindingFlags.Public | BindingFlags.Instance);

        public int Id { get; set; }
        public OneToOnePrincipal OneToOnePrincipal { get; set; }

        public static void IgnoreNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(OneToOneDependent))
            {
                entityTypeBuilder.Ignore(nameof(OneToOnePrincipal), fromDataAnnotation: true);
            }
        }
    }

    private class OneToManyPrincipal
    {
        public static readonly PropertyInfo NavigationProperty =
            typeof(OneToManyPrincipal).GetProperty("OneToManyDependents", BindingFlags.Public | BindingFlags.Instance);

        public int Id { get; set; }

        public IEnumerable<OneToManyDependent> OneToManyDependents { get; set; }

        public static void IgnoreNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(OneToManyPrincipal))
            {
                entityTypeBuilder.Ignore(NavigationProperty.Name, fromDataAnnotation: true);
            }
        }
    }

    private class OneToManyDependent
    {
        public static readonly PropertyInfo NavigationProperty =
            typeof(OneToManyDependent).GetProperty("OneToManyPrincipal", BindingFlags.Public | BindingFlags.Instance);

        public int Id { get; set; }

        public OneToManyPrincipal OneToManyPrincipal { get; set; }

        public static void IgnoreNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(OneToManyDependent))
            {
                entityTypeBuilder.Ignore(NavigationProperty.Name, fromDataAnnotation: true);
            }
        }
    }

    private class ManyToManyFirst
    {
        public int Id { get; set; }
        public IEnumerable<ManyToManySecond> ManyToManySeconds { get; set; }
    }

    private class ManyToManySecond
    {
        public int Id { get; set; }
        public IEnumerable<ManyToManyFirst> ManyToManyFirsts { get; set; }
    }

    private class ManyToManySelf
    {
        public int Id { get; set; }
        public IEnumerable<ManyToManySelf> ManyToManySelf1 { get; set; }
        public IEnumerable<ManyToManySelf> ManyToManySelf2 { get; set; }
    }

    private class DerivedManyToManyFirst : ManyToManyFirst
    {
        public string Name { get; set; }
    }

    private class DerivedManyToManySecond : ManyToManySecond
    {
        public string Name { get; set; }
    }

    private class MultipleNavigationsFirst
    {
        public static readonly PropertyInfo CollectionNavigationProperty =
            typeof(MultipleNavigationsFirst).GetProperty("MultipleNavigationsSeconds", BindingFlags.Public | BindingFlags.Instance);

        public static readonly PropertyInfo NonCollectionNavigationProperty =
            typeof(MultipleNavigationsFirst).GetProperty("MultipleNavigationsSecond", BindingFlags.Public | BindingFlags.Instance);

        public int Id { get; set; }

        public IEnumerable<MultipleNavigationsSecond> MultipleNavigationsSeconds { get; set; }
        public MultipleNavigationsSecond MultipleNavigationsSecond { get; set; }
    }

    private class MultipleNavigationsSecond
    {
        public int Id { get; set; }

        public IEnumerable<MultipleNavigationsFirst> MultipleNavigationsFirsts { get; set; }
        public MultipleNavigationsFirst MultipleNavigationsFirst { get; set; }

        public static void IgnoreCollectionNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(MultipleNavigationsSecond))
            {
                entityTypeBuilder.Ignore(nameof(MultipleNavigationsFirsts), fromDataAnnotation: true);
            }
        }

        public static void IgnoreNonCollectionNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(MultipleNavigationsSecond))
            {
                entityTypeBuilder.Ignore(nameof(MultipleNavigationsFirst), fromDataAnnotation: true);
            }
        }
    }

    private class NavigationsToBaseAndDerived
    {
        public int Id { get; set; }

        public DerivedOne DerivedOne { get; set; }
        public DerivedTwo DerivedTwo { get; set; }
        public Base Base { get; set; }

        public static void IgnoreDerivedOneNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(NavigationsToBaseAndDerived))
            {
                entityTypeBuilder.Ignore(nameof(DerivedOne), fromDataAnnotation: true);
            }
        }

        public static void IgnoreDerivedTwoNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(NavigationsToBaseAndDerived))
            {
                entityTypeBuilder.Ignore(nameof(DerivedTwo), fromDataAnnotation: true);
            }
        }

        public static void IgnoreBaseNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(NavigationsToBaseAndDerived))
            {
                entityTypeBuilder.Ignore(nameof(Base), fromDataAnnotation: true);
            }
        }
    }

    private class Base
    {
        public int Id { get; set; }

        public NavigationsToBaseAndDerived BaseNavigation { get; set; }

        public static void IgnoreBaseNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(Base))
            {
                entityTypeBuilder.Ignore(nameof(BaseNavigation), fromDataAnnotation: true);
            }
        }
    }

    private class DerivedOne : Base
    {
        public NavigationsToBaseAndDerived DerivedNavigation { get; set; }

        public static void IgnoreDerivedNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(DerivedOne))
            {
                entityTypeBuilder.Ignore(nameof(DerivedNavigation), fromDataAnnotation: true);
            }
        }
    }

    private class DerivedTwo : Base
    {
        public NavigationsToBaseAndDerived DerivedNavigation { get; set; }

        public static void IgnoreDerivedNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(DerivedTwo))
            {
                entityTypeBuilder.Ignore(nameof(DerivedNavigation), fromDataAnnotation: true);
            }
        }
    }

    private class SelfRef
    {
        public int Id { get; set; }
        public SelfRef SelfRef1 { get; set; }
        public SelfRef SelfRef2 { get; set; }
        public IEnumerable<SelfRef> SelfRef3 { get; set; }
        public IEnumerable<SelfRef> SelfRef4 { get; set; }
        public int SelfRefId { get; set; }

        public static void IgnoreNavigation2(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(SelfRef))
            {
                entityTypeBuilder.Ignore(nameof(SelfRef2), fromDataAnnotation: true);
            }
        }

        public static void IgnoreNavigation3(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(SelfRef))
            {
                entityTypeBuilder.Ignore(nameof(SelfRef3), fromDataAnnotation: true);
            }
        }

        public static void IgnoreNavigation4(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(SelfRef))
            {
                entityTypeBuilder.Ignore(nameof(SelfRef4), fromDataAnnotation: true);
            }
        }
    }

    public class AmbiguousCardinalityOne : IEnumerable<AmbiguousCardinalityOne>
    {
        public int Id { get; set; }
        public AmbiguousCardinalityTwo AmbiguousCardinalityTwo { get; set; }

        public static void IgnoreNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(AmbiguousCardinalityOne))
            {
                entityTypeBuilder.Ignore(nameof(AmbiguousCardinalityTwo), fromDataAnnotation: true);
            }
        }

        public IEnumerator<AmbiguousCardinalityOne> GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public class AmbiguousCardinalityTwo : IEnumerable<AmbiguousCardinalityTwo>
    {
        public int Id { get; set; }
        public AmbiguousCardinalityOne AmbiguousCardinalityOne { get; set; }

        public static void IgnoreNavigation(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.ClrType == typeof(AmbiguousCardinalityTwo))
            {
                entityTypeBuilder.Ignore(nameof(AmbiguousCardinalityOne), fromDataAnnotation: true);
            }
        }

        public IEnumerator<AmbiguousCardinalityTwo> GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
