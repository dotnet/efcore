// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationshipDiscoveryConventionTest
    {
        [Fact]
        public void Entity_type_is_not_discovered_if_ignored()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();
            entityBuilder.ModelBuilder.Ignore(typeof(OneToManyPrincipal).FullName, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetProperties());
            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Equal(entityBuilder.Metadata.ClrType, entityBuilder.Metadata.Model.GetEntityTypes().Single().ClrType);
        }

        [Fact]
        public void Entity_type_is_not_discovered_if_navigation_is_ignored()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();
            entityBuilder.Ignore(OneToManyDependent.NavigationProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetProperties());
            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Equal(entityBuilder.Metadata.ClrType, entityBuilder.Metadata.Model.GetEntityTypes().Single().ClrType);
        }

        [Fact]
        public void One_to_one_bidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), OneToOneDependent.NavigationProperty.Name, unique: true);
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void One_to_many_unidirectional_is_upgraded_to_one_to_one_bidirectional()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);

            principalEntityBuilder.Relationship(dependentEntityBuilder, OneToOnePrincipal.NavigationProperty, null, ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.Convention);

            Assert.Same(dependentEntityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(dependentEntityBuilder));

            VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), OneToOnePrincipal.NavigationProperty.Name, unique: true);
            Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Two_one_to_many_unidirectional_are_upgraded_to_one_to_one_bidirectional()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);

            principalEntityBuilder.Relationship(dependentEntityBuilder, OneToOnePrincipal.NavigationProperty, null, ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.Convention);

            dependentEntityBuilder.Relationship(principalEntityBuilder, OneToOneDependent.NavigationProperty, null, ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.Convention);

            Assert.Same(dependentEntityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(dependentEntityBuilder));

            VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), OneToOnePrincipal.NavigationProperty.Name, unique: true);
            Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void One_to_many_unidirectional_is_not_upgraded_to_one_to_one_bidirectional_if_higher_source()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);

            principalEntityBuilder.Relationship(dependentEntityBuilder, OneToOnePrincipal.NavigationProperty, null, ConfigurationSource.Explicit)
                .IsUnique(false, ConfigurationSource.Convention);

            Assert.Same(dependentEntityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(dependentEntityBuilder));
            VerifyRelationship(principalEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
            VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
            Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void One_to_many_unidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>(OneToManyPrincipal.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), null, unique: false);
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void One_to_many_unidirectional_is_upgraded_to_one_to_many_bidirectional()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
            var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(typeof(OneToManyDependent), ConfigurationSource.Convention);

            dependentEntityBuilder.Relationship(principalEntityBuilder, null, OneToManyPrincipal.NavigationProperty, ConfigurationSource.Convention);

            Assert.Same(dependentEntityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(dependentEntityBuilder));

            VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), OneToManyPrincipal.NavigationProperty.Name, unique: false);
            Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void One_to_many_unidirectional_is_not_upgraded_to_one_to_many_bidirectional_if_higher_source()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
            var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(typeof(OneToManyDependent), ConfigurationSource.Convention);

            dependentEntityBuilder.Relationship(principalEntityBuilder, null, OneToManyPrincipal.NavigationProperty, ConfigurationSource.Explicit);

            Assert.Same(dependentEntityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(dependentEntityBuilder));

            VerifyRelationship(principalEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
            VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
            Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void One_to_many_bidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), OneToManyPrincipal.NavigationProperty.Name, unique: false);
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Many_to_one_unidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>(OneToManyDependent.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), null, unique: false);
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Many_to_one_unidirectional_is_upgraded_to_many_to_one_bidirectional()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
            var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(typeof(OneToManyDependent), ConfigurationSource.Convention);

            dependentEntityBuilder.Relationship(principalEntityBuilder, OneToManyDependent.NavigationProperty, null, ConfigurationSource.Convention);

            Assert.Same(principalEntityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(principalEntityBuilder));

            VerifyRelationship(principalEntityBuilder.Metadata.GetNavigations().Single(), OneToManyDependent.NavigationProperty.Name, unique: false);
            Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Many_to_one_unidirectional_is_not_upgraded_to_many_to_one_bidirectional_if_higher_source()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();
            var dependentEntityBuilder = principalEntityBuilder.ModelBuilder.Entity(typeof(OneToManyDependent), ConfigurationSource.Convention);

            dependentEntityBuilder.Relationship(principalEntityBuilder, OneToManyDependent.NavigationProperty, null, ConfigurationSource.Explicit);

            Assert.Same(principalEntityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(principalEntityBuilder));

            VerifyRelationship(principalEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
            VerifyRelationship(dependentEntityBuilder.Metadata.GetNavigations().Single(), null, unique: false, singleRelationship: false);
            Assert.Equal(2, principalEntityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Many_to_one_bidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), OneToManyDependent.NavigationProperty.Name, unique: false);
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Many_to_many_bidirectional_is_not_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<ManyToManyFirst>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));
            new ModelCleanupConvention().Apply(entityBuilder.ModelBuilder);

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Equal(1, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Ambiguous_navigations_are_not_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Existing_relationship_is_removed_if_ambiguous()
        {
            var entityBuilderFirst = CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
            var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

            entityBuilderFirst.Relationship(entityBuilderSecond, MultipleNavigationsFirst.CollectionNavigationProperty, null, ConfigurationSource.Convention);

            Assert.Same(entityBuilderFirst, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilderFirst));

            Assert.Empty(entityBuilderFirst.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilderFirst.Metadata.GetNavigations());
            Assert.Empty(entityBuilderSecond.Metadata.GetNavigations());
            Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Existing_relationship_removes_ambiguity_if_higher_source()
        {
            var entityBuilderFirst = CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
            var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

            entityBuilderFirst.Relationship(entityBuilderSecond, MultipleNavigationsFirst.CollectionNavigationProperty, null, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilderFirst, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilderFirst));

            VerifyRelationship(entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.CollectionNavigationProperty.Name), null, unique: false, singleRelationship: false);
            VerifyRelationship(entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.NonCollectionNavigationProperty.Name), nameof(MultipleNavigationsSecond.MultipleNavigationsFirst), unique: true, singleRelationship: false);
            Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigations_are_not_discovered_if_ambiguous_inverse()
        {
            var entityBuilderFirst = CreateInternalEntityBuilder<MultipleNavigationsSecond>(
                MultipleNavigationsSecond.IgnoreCollectionNavigation);
            var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

            Assert.Same(entityBuilderFirst, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilderFirst));

            Assert.Empty(entityBuilderFirst.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilderFirst.Metadata.GetNavigations());
            Assert.Empty(entityBuilderSecond.Metadata.GetNavigations());
            Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Existing_relationship_is_removed_if_ambiguous_inverse()
        {
            var entityBuilderFirst = CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
            var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

            entityBuilderFirst.Relationship(entityBuilderSecond, MultipleNavigationsFirst.CollectionNavigationProperty, null, ConfigurationSource.Convention);

            Assert.Same(entityBuilderSecond, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilderSecond));

            Assert.Empty(entityBuilderFirst.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilderFirst.Metadata.GetNavigations());
            Assert.Empty(entityBuilderSecond.Metadata.GetNavigations());
            Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());

            Assert.Equal(LogLevel.Information, Log[0].Level);
            Assert.Equal(CoreStrings.LogMultipleNavigationProperties.GenerateMessage(
                nameof(MultipleNavigationsSecond),
                nameof(MultipleNavigationsFirst),
                "{'MultipleNavigationsFirst'}" ,
                "{'MultipleNavigationsSecond', 'MultipleNavigationsSeconds'}"), Log[0].Message);
        }

        [Fact]
        public void Existing_relationship_removes_ambiguity_in_inverse_if_higher_source()
        {
            var entityBuilderFirst = CreateInternalEntityBuilder<MultipleNavigationsFirst>(MultipleNavigationsSecond.IgnoreCollectionNavigation);
            var entityBuilderSecond = entityBuilderFirst.ModelBuilder.Entity(typeof(MultipleNavigationsSecond), ConfigurationSource.Convention);

            entityBuilderFirst.Relationship(entityBuilderSecond, MultipleNavigationsFirst.CollectionNavigationProperty, null, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilderSecond, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilderSecond));

            VerifyRelationship(entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.CollectionNavigationProperty.Name), null, unique: false, singleRelationship: false);
            VerifyRelationship(entityBuilderFirst.Metadata.FindNavigation(MultipleNavigationsFirst.NonCollectionNavigationProperty.Name), nameof(MultipleNavigationsSecond.MultipleNavigationsFirst), unique: true, singleRelationship: false);
            Assert.Equal(2, entityBuilderFirst.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Multiple_navigations_to_same_entity_type_are_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<MultipleNavigationsFirst>(
                MultipleNavigationsSecond.IgnoreCollectionNavigation, MultipleNavigationsSecond.IgnoreNonCollectionNavigation);
            entityBuilder.ModelBuilder.Entity(typeof(MultipleNavigationsSecond), ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var model = (IModel)entityBuilder.Metadata.Model;
            Assert.Equal(2, model.GetEntityTypes().Count());
            var firstEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(MultipleNavigationsFirst));
            var secondEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(MultipleNavigationsSecond));

            Assert.Equal(2, firstEntityType.GetProperties().Count());
            Assert.Equal(1, firstEntityType.GetKeys().Count());
            var firstFK = firstEntityType.GetForeignKeys().Single();
            Assert.False(firstFK.IsRequired);
            Assert.False(firstFK.IsUnique);
            Assert.Equal(
                new[] { MultipleNavigationsFirst.NonCollectionNavigationProperty.Name, MultipleNavigationsFirst.CollectionNavigationProperty.Name },
                firstEntityType.GetNavigations().Select(n => n.Name));

            Assert.Equal(2, secondEntityType.GetProperties().Count());
            Assert.Equal(1, secondEntityType.GetKeys().Count());
            var secondFK = firstEntityType.GetForeignKeys().Single();
            Assert.False(secondFK.IsRequired);
            Assert.False(secondFK.IsUnique);
            Assert.Empty(secondEntityType.GetNavigations());
        }

        [Fact]
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

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var baseFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
            var derivedFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
            Assert.Empty(baseFk.FindNavigationsTo(entityBuilder.Metadata));
            Assert.Empty(derivedFk.FindNavigationsTo(entityBuilder.Metadata));
            Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigations_to_base_and_derived_are_discovered_if_inverse_from_base()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                DerivedOne.IgnoreDerivedNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var baseFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
            var derivedFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
            Assert.Equal(nameof(Base.BaseNavigation), baseFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Empty(derivedFk.FindNavigationsTo(entityBuilder.Metadata));
            Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigations_to_derived_and_base_are_discovered_if_inverse_from_base()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation,
                DerivedTwo.IgnoreDerivedNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedTwo), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var baseFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
            var derivedFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedTwo)).ForeignKey;
            Assert.Equal(nameof(Base.BaseNavigation), baseFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Empty(derivedFk.FindNavigationsTo(entityBuilder.Metadata));
            Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigations_to_base_and_derived_are_discovered_if_inverse_from_derived()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                Base.IgnoreBaseNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var baseFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
            var derivedFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
            Assert.Empty(baseFk.FindNavigationsTo(entityBuilder.Metadata));
            Assert.Equal(nameof(DerivedOne.DerivedNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Equal(2, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigation_pairs_to_base_and_derived_are_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>();
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);
            var derivedBuilderTwo = modelBuilder.Entity(typeof(DerivedTwo), ConfigurationSource.Explicit);
            derivedBuilderTwo.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var baseFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.Base)).ForeignKey;
            var derivedFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
            Assert.Equal(nameof(Base.BaseNavigation), baseFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Equal(nameof(DerivedOne.DerivedNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Equal(3, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(4, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigation_to_base_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));
            new ModelCleanupConvention().Apply(modelBuilder);

            VerifyRelationship(
                entityBuilder.Metadata.FindNavigation(nameof(NavigationsToBaseAndDerived.Base)),
                expectedInverseName: nameof(Base.BaseNavigation), unique: true);
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
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

            derivedBuilder.Relationship(entityBuilder, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifyRelationship(
                entityBuilder.Metadata.FindNavigation(nameof(NavigationsToBaseAndDerived.Base)),
                expectedInverseName: nameof(Base.BaseNavigation), unique: true);
            Assert.Empty(derivedBuilder.Metadata.GetDeclaredNavigations());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
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

            derivedBuilder.Relationship(entityBuilder, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

            Assert.Same(baseBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(baseBuilder));

            VerifyRelationship(
                baseBuilder.Metadata.FindNavigation(nameof(Base.BaseNavigation)),
                expectedInverseName: nameof(NavigationsToBaseAndDerived.Base), unique: true);
            Assert.Empty(derivedBuilder.Metadata.GetDeclaredNavigations());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigation_from_derived_is_not_discovered_if_ambiguous()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                NavigationsToBaseAndDerived.IgnoreBaseNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(derivedBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(derivedBuilder));

            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(derivedBuilder.Metadata.GetNavigations());
            Assert.Empty(derivedBuilder.Metadata.GetForeignKeys());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Existing_relationship_to_base_removes_ambiguity()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                NavigationsToBaseAndDerived.IgnoreBaseNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            baseBuilder.Relationship(entityBuilder, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

            Assert.Same(derivedBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(derivedBuilder));

            VerifyRelationship(baseBuilder.Metadata.GetNavigations().Single(), expectedInverseName: null, unique: false, singleRelationship: false);
            VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), nameof(DerivedOne.DerivedNavigation), unique: true, singleRelationship: false);
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigation_to_derived_is_not_discovered_if_inverse_ambiguous()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                NavigationsToBaseAndDerived.IgnoreBaseNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(derivedBuilder.Metadata.GetNavigations());
            Assert.Empty(derivedBuilder.Metadata.GetForeignKeys());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Existing_relationship_to_base_removes_ambiguity_in_derived_inverse()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                NavigationsToBaseAndDerived.IgnoreBaseNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            var baseBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var derivedBuilder = modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);
            derivedBuilder.HasBaseType(baseBuilder.Metadata, ConfigurationSource.Convention);

            baseBuilder.Relationship(entityBuilder, nameof(Base.BaseNavigation), null, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifyRelationship(baseBuilder.Metadata.GetNavigations().Single(), expectedInverseName: null, unique: false, singleRelationship: false);
            VerifyRelationship(entityBuilder.Metadata.GetNavigations().Single(), nameof(DerivedOne.DerivedNavigation), unique: true, singleRelationship: false);
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
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

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var derivedFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
            Assert.Equal(nameof(DerivedOne.DerivedNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Equal(1, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
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

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var derivedFk = entityBuilder.Metadata.GetNavigations()
                .Single(n => n.Name == nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
            Assert.Equal(nameof(Base.BaseNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Equal(1, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(3, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigation_to_base_is_not_discovered_if_base_ignored()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                NavigationsToBaseAndDerived.IgnoreDerivedOneNavigation,
                DerivedOne.IgnoreDerivedNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            modelBuilder.Ignore(typeof(Base), ConfigurationSource.Explicit);
            modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigation_to_derived_is_discovered_if_base_ignored()
        {
            var entityBuilder = CreateInternalEntityBuilder<NavigationsToBaseAndDerived>(
                NavigationsToBaseAndDerived.IgnoreDerivedTwoNavigation,
                DerivedOne.IgnoreDerivedNavigation);
            var modelBuilder = entityBuilder.ModelBuilder;
            modelBuilder.Ignore(typeof(Base), ConfigurationSource.Explicit);
            modelBuilder.Entity(typeof(DerivedOne), ConfigurationSource.Explicit);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var derivedFk = entityBuilder.Metadata.FindNavigation(nameof(NavigationsToBaseAndDerived.DerivedOne)).ForeignKey;
            Assert.Equal(nameof(DerivedOne.BaseNavigation), derivedFk.FindNavigationsTo(entityBuilder.Metadata).Single().Name);
            Assert.Equal(1, entityBuilder.Metadata.GetNavigations().Count());
            Assert.Equal(2, entityBuilder.Metadata.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Does_not_throw_on_shadow_entity_types()
        {
            var entityBuilder = new InternalModelBuilder(new Model())
                .Entity("Shadow", ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));
        }

        [Fact]
        public void Bidirectional_ambiguous_cardinality_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<AmbiguousCardinalityOne>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var model = (IModel)entityBuilder.Metadata.Model;
            Assert.Equal(2, model.GetEntityTypes().Count());
            var firstEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityOne));
            var secondEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityTwo));

            var fk = firstEntityType.GetNavigations().Single().ForeignKey;
            Assert.Same(fk, secondEntityType.GetNavigations().Single().ForeignKey);
            Assert.True(fk.IsUnique);
        }

        [Fact]
        public void Unidirectional_ambiguous_cardinality_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<AmbiguousCardinalityOne>(
                AmbiguousCardinalityTwo.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            var model = (IModel)entityBuilder.Metadata.Model;
            Assert.Equal(2, model.GetEntityTypes().Count());
            var firstEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityOne));
            var secondEntityType = model.GetEntityTypes().Single(e => e.ClrType == typeof(AmbiguousCardinalityTwo));

            var fk = firstEntityType.GetNavigations().Single().ForeignKey;
            Assert.Empty(secondEntityType.GetNavigations());
            Assert.False(fk.IsUnique);
        }

        [Fact]
        public void One_to_one_bidirectional_self_ref_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), nameof(SelfRef.SelfRef2), unique: true);
        }

        [Fact]
        public void One_to_many_unidirectional_self_ref_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation2, SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: null, unique: false);
        }

        [Fact]
        public void One_to_many_unidirectional_self_ref_is_upgraded_to_one_to_one_bidirectional()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

            entityBuilder.Relationship(entityBuilder, nameof(SelfRef.SelfRef1), null, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), nameof(SelfRef.SelfRef2), unique: true);
        }

        [Fact]
        public void One_to_many_unidirectional_self_ref_is_not_upgraded_to_one_to_one_bidirectional_if_higher_source()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation3, SelfRef.IgnoreNavigation4);

            entityBuilder.Relationship(entityBuilder, nameof(SelfRef.SelfRef1), null, ConfigurationSource.Explicit)
                .IsUnique(false, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: null, unique: false, singleRelationship: false);
            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef2)), expectedInverseName: null, unique: false, singleRelationship: false);
        }

        [Fact]
        public void Ambiguous_self_ref_is_not_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation4);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Empty(entityBuilder.Metadata.GetProperties());
        }

        [Fact]
        public void Existing_unidirectional_self_ref_is_removed_if_ambiguous()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation4);

            entityBuilder.Relationship(entityBuilder, nameof(SelfRef.SelfRef1), null, ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Empty(entityBuilder.Metadata.GetProperties());
        }

        [Fact]
        public void Existing_unidirectional_self_ref_removes_ambiguity_if_higher_source()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(SelfRef.IgnoreNavigation4);

            entityBuilder.Relationship(entityBuilder, nameof(SelfRef.SelfRef1), null, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: null, unique: false, singleRelationship: false);
            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef2)), expectedInverseName: nameof(SelfRef.SelfRef3), unique: false, singleRelationship: false);
        }

        [Fact]
        public void Existing_bidirectional_self_ref_is_removed_if_ambiguous()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>();

            entityBuilder.Relationship(entityBuilder, nameof(SelfRef.SelfRef1), nameof(SelfRef.SelfRef3), ConfigurationSource.Convention);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Empty(entityBuilder.Metadata.GetProperties());

            Assert.Equal(LogLevel.Information, Log[0].Level);
            Assert.Equal(CoreStrings.LogMultipleNavigationProperties.GenerateMessage(
                nameof(SelfRef), nameof(SelfRef), "{'SelfRef1'}", "{'SelfRef2', 'SelfRef3', 'SelfRef4'}"), Log[0].Message);
        }

        [Fact]
        public void Existing_bidirectional_self_ref_removes_ambiguity_if_higher_source()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>();

            entityBuilder.Relationship(entityBuilder, nameof(SelfRef.SelfRef1), nameof(SelfRef.SelfRef3), ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef1)), expectedInverseName: nameof(SelfRef.SelfRef3), unique: false, singleRelationship: false);
            VerifySelfRef(entityBuilder.Metadata.FindNavigation(nameof(SelfRef.SelfRef2)), expectedInverseName: nameof(SelfRef.SelfRef4), unique: false, singleRelationship: false);
        }

        [Fact]
        public void Navigation_to_abstract_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<AbstractClass>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            IModel model = entityBuilder.Metadata.Model;
            var entityType = model.GetEntityTypes().Single();

            Assert.Equal(2, entityType.GetProperties().Count());
            Assert.Equal(1, entityType.GetKeys().Count());

            var fk = entityType.GetForeignKeys().Single();
            Assert.False(fk.IsUnique);
            Assert.True(fk.PrincipalEntityType.ClrType.GetTypeInfo().IsAbstract);
            Assert.Equal(1, entityType.GetNavigations().Count());
        }

        [Fact]
        public void Collection_navigation_without_setter_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<ReadOnlyCollectionNavigationEntity>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            IModel model = entityBuilder.Metadata.Model;
            Assert.NotNull(model.FindEntityType(typeof(EntityWithNoValidNavigations)));
            var entityType = entityBuilder.Metadata;

            Assert.Equal(ReadOnlyCollectionNavigationEntity.NavigationProperty.Name, entityType.GetNavigations().First().Name);
        }

        [Fact]
        public void Does_not_discover_nonNavigation_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoValidNavigations>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger()).Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.GetNavigations());
            Assert.Empty(entityBuilder.Metadata.GetProperties());
        }
        public List<(LogLevel Level, EventId Id, string Message)> Log { get; }
            = new List<(LogLevel, EventId, string)>();

        private static CoreTypeMapper CreateTypeMapper()
            => TestServiceFactory.Instance.Create<CoreTypeMapper>();

        private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
            var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
                new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Name),
                options,
                new DiagnosticListener("Fake"));
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

            public OneToManyDependent this[int index]
            {
                get { return null; }
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
            Navigation navigation, string expectedInverseName, bool unique, bool singleRelationship = true)
        {
            IForeignKey fk = navigation.ForeignKey;
            Assert.Equal(expectedInverseName, navigation.FindInverse()?.Name);
            Assert.Equal(unique, fk.IsUnique);
            Assert.NotSame(fk.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.NotEqual(fk.PrincipalToDependent?.Name, fk.DependentToPrincipal?.Name);

            if (singleRelationship)
            {
                var principalEntityType = fk.PrincipalEntityType;
                Assert.Equal(1, principalEntityType.GetDeclaredProperties().Count());
                Assert.Equal(1, principalEntityType.GetKeys().Count());
                Assert.Empty(principalEntityType.GetDeclaredForeignKeys());
                if ((expectedInverseName == null)
                    && navigation.IsDependentToPrincipal())
                {
                    Assert.Empty(principalEntityType.GetNavigations());
                }

                var dependentEntityType = fk.DeclaringEntityType;
                Assert.Equal(1, dependentEntityType.GetDeclaredProperties().Count());
                Assert.Equal(principalEntityType.IsAssignableFrom(dependentEntityType) ? 1 : 0, dependentEntityType.GetKeys().Count());
                if ((expectedInverseName == null)
                    && !navigation.IsDependentToPrincipal())
                {
                    Assert.Empty(dependentEntityType.GetNavigations());
                }
            }
        }

        private static void VerifySelfRef(
            Navigation navigation, string expectedInverseName, bool unique, bool singleRelationship = true)
        {
            IForeignKey fk = navigation.ForeignKey;
            Assert.Equal(1, fk.DeclaringEntityType.Model.GetEntityTypes().Count());
            Assert.Equal(expectedInverseName, navigation.FindInverse()?.Name);
            Assert.Equal(unique, fk.IsUnique);
            Assert.NotSame(fk.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.NotEqual(fk.PrincipalToDependent?.Name, fk.DependentToPrincipal?.Name);

            var entityType = fk.DeclaringEntityType;
            if (singleRelationship)
            {
                Assert.Equal(1, entityType.GetKeys().Count());
                Assert.Equal(1, entityType.GetForeignKeys().Count());
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

        private InternalEntityTypeBuilder CreateInternalEntityBuilder<T>(params Action<InternalEntityTypeBuilder>[] onEntityAdded)
        {
            var conventions = new ConventionSet();
            if (onEntityAdded != null)
            {
                conventions.EntityTypeAddedConventions.Add(new TestModelChangeListener(onEntityAdded));

                var relationshipDiscoveryConvention = new RelationshipDiscoveryConvention(CreateTypeMapper(), CreateLogger());
                conventions.BaseEntityTypeChangedConventions.Add(relationshipDiscoveryConvention);
                conventions.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);
                conventions.NavigationAddedConventions.Add(relationshipDiscoveryConvention);
                conventions.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);
            }
            var modelBuilder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.DataAnnotation);

            return entityBuilder;
        }

        private class TestModelChangeListener : IEntityTypeAddedConvention
        {
            private readonly Action<InternalEntityTypeBuilder>[] _onEntityAdded;

            public TestModelChangeListener(Action<InternalEntityTypeBuilder>[] onEntityAdded)
            {
                _onEntityAdded = onEntityAdded;
            }

            public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
            {
                foreach (var action in _onEntityAdded)
                {
                    action(entityTypeBuilder);
                }

                return entityTypeBuilder;
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

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(OneToOneDependent))
                {
                    entityTypeBuilder.Ignore(nameof(OneToOnePrincipal), ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class OneToManyPrincipal
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyPrincipal).GetProperty("OneToManyDependents", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }

            public IEnumerable<OneToManyDependent> OneToManyDependents { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(OneToManyPrincipal))
                {
                    entityTypeBuilder.Ignore(NavigationProperty.Name, ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class OneToManyDependent
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyDependent).GetProperty("OneToManyPrincipal", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }

            public OneToManyPrincipal OneToManyPrincipal { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(OneToManyDependent))
                {
                    entityTypeBuilder.Ignore(NavigationProperty.Name, ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class ManyToManyFirst
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyPrincipal).GetProperty("ManyToManySeconds", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }
            public IEnumerable<ManyToManySecond> ManyToManySeconds { get; set; }
        }

        private class ManyToManySecond
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyPrincipal).GetProperty("ManyToManyFirsts", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }
            public IEnumerable<ManyToManyFirst> ManyToManyFirsts { get; set; }
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

            public static void IgnoreCollectionNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(MultipleNavigationsSecond))
                {
                    entityTypeBuilder.Ignore(nameof(MultipleNavigationsFirsts), ConfigurationSource.DataAnnotation);
                }
            }

            public static void IgnoreNonCollectionNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(MultipleNavigationsSecond))
                {
                    entityTypeBuilder.Ignore(nameof(MultipleNavigationsFirst), ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class NavigationsToBaseAndDerived
        {
            public int Id { get; set; }

            public DerivedOne DerivedOne { get; set; }
            public DerivedTwo DerivedTwo { get; set; }
            public Base Base { get; set; }

            public static void IgnoreDerivedOneNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(NavigationsToBaseAndDerived))
                {
                    entityTypeBuilder.Ignore(nameof(DerivedOne), ConfigurationSource.DataAnnotation);
                }
            }

            public static void IgnoreDerivedTwoNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(NavigationsToBaseAndDerived))
                {
                    entityTypeBuilder.Ignore(nameof(DerivedTwo), ConfigurationSource.DataAnnotation);
                }
            }

            public static void IgnoreBaseNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(NavigationsToBaseAndDerived))
                {
                    entityTypeBuilder.Ignore(nameof(Base), ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class Base
        {
            public int Id { get; set; }

            public NavigationsToBaseAndDerived BaseNavigation { get; set; }

            public static void IgnoreBaseNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(Base))
                {
                    entityTypeBuilder.Ignore(nameof(BaseNavigation), ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class DerivedOne : Base
        {
            public NavigationsToBaseAndDerived DerivedNavigation { get; set; }

            public static void IgnoreDerivedNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(DerivedOne))
                {
                    entityTypeBuilder.Ignore(nameof(DerivedNavigation), ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class DerivedTwo : Base
        {
            public NavigationsToBaseAndDerived DerivedNavigation { get; set; }

            public static void IgnoreDerivedNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(DerivedTwo))
                {
                    entityTypeBuilder.Ignore(nameof(DerivedNavigation), ConfigurationSource.DataAnnotation);
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

            public static void IgnoreNavigation2(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(SelfRef))
                {
                    entityTypeBuilder.Ignore(nameof(SelfRef2), ConfigurationSource.DataAnnotation);
                }
            }

            public static void IgnoreNavigation3(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(SelfRef))
                {
                    entityTypeBuilder.Ignore(nameof(SelfRef3), ConfigurationSource.DataAnnotation);
                }
            }

            public static void IgnoreNavigation4(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(SelfRef))
                {
                    entityTypeBuilder.Ignore(nameof(SelfRef4), ConfigurationSource.DataAnnotation);
                }
            }
        }

        public class AmbiguousCardinalityOne : IEnumerable<AmbiguousCardinalityOne>
        {
            public int Id { get; set; }
            public AmbiguousCardinalityTwo AmbiguousCardinalityTwo { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(AmbiguousCardinalityOne))
                {
                    entityTypeBuilder.Ignore(nameof(AmbiguousCardinalityTwo), ConfigurationSource.DataAnnotation);
                }
            }

            public IEnumerator<AmbiguousCardinalityOne> GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class AmbiguousCardinalityTwo : IEnumerable<AmbiguousCardinalityTwo>
        {
            public int Id { get; set; }
            public AmbiguousCardinalityOne AmbiguousCardinalityOne { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(AmbiguousCardinalityTwo))
                {
                    entityTypeBuilder.Ignore(nameof(AmbiguousCardinalityOne), ConfigurationSource.DataAnnotation);
                }
            }

            public IEnumerator<AmbiguousCardinalityTwo> GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
