// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ForeignKeyPropertyDiscoveryConventionTest
    {
        private readonly InternalModelBuilder _model = BuildModel();

        [Fact]
        public void Does_not_override_explicit_foreign_key_created_using_given_property()
        {
            DependentType.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);
            var fkProperty = DependentType.Property("No!No!", typeof(int), ConfigurationSource.Convention).Metadata;

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Explicit)
                .HasForeignKey(new[] { fkProperty }, ConfigurationSource.Explicit);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            Assert.Same(relationshipBuilder, convention.Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.GetForeignKeys().Single());
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_override_explicit_composite_foreign_key_created_using_given_properties()
        {
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);
            var fkProperty1 = DependentTypeWithCompositeKey.Property("No!No!", typeof(int), ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property("No!No!2", typeof(string), ConfigurationSource.Convention);
            fkProperty2.IsRequired(true, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Explicit)
                .HasForeignKey(new[] { fkProperty1.Metadata, fkProperty2.Metadata }, ConfigurationSource.Explicit);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            Assert.Same(relationshipBuilder, convention.Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single());
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
            Assert.False(fk.IsUnique);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Returns_same_builder_if_no_matching_clr_properties_found()
        {
            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            relationshipBuilder = convention.Apply(relationshipBuilder);

            var fk = relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.GetForeignKeys().Single());
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.False(fk.IsUnique);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_navigation_plus_PK_name_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_navigation_plus_PK_name_properties()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropNameProperty, ConfigurationSource.Convention);
            fkProperty2.IsRequired(true, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
            Assert.False(fk.IsUnique);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_navigation_plus_Id_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_principal_type_plus_PK_name_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_principal_type_plus_Id_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_principal_type_plus_PK_name_properties()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            fkProperty2.IsRequired(true, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_PK_name_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_Id_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.IDProperty, ConfigurationSource.Convention).Metadata;

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_PK_name_properties()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);
            fkProperty2.IsRequired(true, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_dependent_PK_for_unique_FK_set_by_higher_source_than_convention()
        {
            var fkProperty = DependentType.Metadata.FindPrimaryKey().Properties.Single();
            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                "InverseReferenceNav",
                ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.DataAnnotation)
                .DependentEntityType(DependentType, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_PK_name_property_of_different_type()
        {
            var fkProperty = DependentType.Property("PeeKay", typeof(string), ConfigurationSource.Explicit).Metadata;
            DependentType.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.NotSame(fkProperty, fk.Properties.Single());
            Assert.Null(((ForeignKey)fk).GetForeignKeyPropertiesConfigurationSource());

            Assert.Equal(1, Log.Count);
            Assert.Equal(LogLevel.Information, Log[0].Level);
            Assert.Equal(CoreStrings.LogIncompatibleMatchingForeignKeyProperties.GenerateMessage(
                "{'PeeKay' : string}", "{'PeeKay' : int}"), Log[0].Message);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_dependent_PK_for_non_unique_FK()
        {
            DependentType.PrimaryKey(new[] { DependentEntity.PrincipalEntityPeEKaYProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = DependentType.Relationship(PrincipalType, ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            Assert.Same(relationshipBuilder, convention.Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.GetForeignKeys().Single());
            Assert.Equal(PrincipalType.Metadata.DisplayName() + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.False(fk.IsUnique);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_non_nullable_dependent_PK_for_optional_unique_FK()
        {
            var fkProperty = DependentType.Metadata.FindPrimaryKey().Properties.Single();

            var relationshipBuilder = DependentType.Relationship(PrincipalType, ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.DataAnnotation)
                .IsRequired(false, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            Assert.Same(relationshipBuilder, convention.Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.NotSame(fkProperty, fk.Properties.Single());
            Assert.Equal("PrincipalEntity" + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.True(fk.IsUnique);
            Assert.False(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_dependent_PK_for_self_ref()
        {
            var relationshipBuilder = PrincipalType.Relationship(PrincipalType, ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            Assert.Same(relationshipBuilder, convention.Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, PrincipalType.Metadata.GetForeignKeys().Single());
            Assert.Equal(PrincipalType.Metadata.DisplayName() + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.True(fk.IsUnique);
            Assert.False(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Matches_composite_dependent_PK_for_unique_FK()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[0];
            var fkProperty2 = DependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[1];

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                "InverseReferenceNav", ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.DataAnnotation)
                .DependentEntityType(DependentTypeWithCompositeKey, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(PrincipalTypeWithCompositeKey.Metadata.FindPrimaryKey(), fk.PrincipalKey);
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_composite_dependent_PK_for_unique_FK_on_derived_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var principalTypeWithCompositeKey = modelBuilder.Entity(typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Explicit);
            principalTypeWithCompositeKey.PrimaryKey(new[] { PrincipalEntityWithCompositeKey.IdProperty, PrincipalEntityWithCompositeKey.NameProperty }, ConfigurationSource.Explicit);
            principalTypeWithCompositeKey.Property(PrincipalEntityWithCompositeKey.NameProperty, ConfigurationSource.Explicit).IsRequired(true, ConfigurationSource.Explicit);

            var dependentTypeWithCompositeKeyBase = modelBuilder.Entity(typeof(DependentCompositeBase), ConfigurationSource.Explicit);
            var dependentTypeWithCompositeKey = modelBuilder.Entity(typeof(DependentEntityWithCompositeKey), ConfigurationSource.Explicit);
            dependentTypeWithCompositeKey.HasBaseType(dependentTypeWithCompositeKeyBase.Metadata, ConfigurationSource.Explicit);
            dependentTypeWithCompositeKeyBase.PrimaryKey(new[] { nameof(DependentEntityWithCompositeKey.NotId), nameof(DependentEntityWithCompositeKey.NotName) }, ConfigurationSource.Explicit);

            var relationshipBuilder = dependentTypeWithCompositeKey.Relationship(
                principalTypeWithCompositeKey,
                "NavProp",
                "InverseReferenceNav", ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.DataAnnotation)
                .DependentEntityType(dependentTypeWithCompositeKey, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);

            var fk = dependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.True(fk.IsUnique);
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Same(principalTypeWithCompositeKey.Metadata.FindPrimaryKey(), fk.PrincipalKey);
            Assert.NotEqual(dependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[0], fk.Properties[0]);
            Assert.NotEqual(dependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[1], fk.Properties[1]);
            Assert.Equal("NavProp" + CompositePrimaryKey[0].Name + "1", fk.Properties[0].Name);
            Assert.Equal("NavProp" + CompositePrimaryKey[1].Name + "1", fk.Properties[1].Name);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_composite_dependent_PK_for_non_unique_FK()
        {
            DependentTypeWithCompositeKey.PrimaryKey(
                new[] { DependentEntityWithCompositeKey.NavPropIdProperty, DependentEntityWithCompositeKey.NavPropNameProperty },
                ConfigurationSource.Explicit);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            relationshipBuilder = convention.Apply(relationshipBuilder);

            var fk = relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single());
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Equal("NavProp" + CompositePrimaryKey[0].Name + "1", fk.Properties[0].Name);
            Assert.Equal("NavProp" + CompositePrimaryKey[1].Name + "1", fk.Properties[1].Name);
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
            Assert.False(fk.IsUnique);
            Assert.False(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_composite_dependent_PK_for_unique_FK_if_count_mismatched()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[0];
            DependentTypeWithCompositeKey.PrimaryKey(new[] { fkProperty1.Name }, ConfigurationSource.Explicit);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(PrincipalTypeWithCompositeKey, ConfigurationSource.Convention)
                .HasPrincipalKey(PrincipalTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties, ConfigurationSource.DataAnnotation)
                .IsUnique(true, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            Assert.Same(relationshipBuilder, convention.Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single());
            Assert.Equal(2, fk.Properties.Count);
            Assert.NotSame(fkProperty1, fk.Properties[0]);
            Assert.NotSame(fkProperty1, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
            Assert.True(fk.IsUnique);
            Assert.False(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_composite_dependent_PK_for_unique_FK_if_order_mismatched()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[0];
            var fkProperty2 = DependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[1];
            DependentTypeWithCompositeKey.PrimaryKey(new[] { fkProperty2.Name, fkProperty1.Name }, ConfigurationSource.Explicit);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey, "NavProp", "InverseReferenceNav", ConfigurationSource.Convention)
                .HasPrincipalKey(PrincipalTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties, ConfigurationSource.DataAnnotation)
                .IsUnique(true, ConfigurationSource.DataAnnotation);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            relationshipBuilder = convention.Apply(relationshipBuilder);

            var fk = relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single());
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Equal(2, fk.Properties.Count);
            Assert.NotSame(fkProperty1, fk.Properties[0]);
            Assert.NotSame(fkProperty2, fk.Properties[1]);
            Assert.NotSame(fkProperty1, fk.Properties[1]);
            Assert.NotSame(fkProperty2, fk.Properties[0]);
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
            Assert.True(fk.IsUnique);
            Assert.False(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_properties_with_different_base_names()
        {
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey, "NavProp", null, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            relationshipBuilder = convention.Apply(relationshipBuilder);

            var fk = relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single());
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_match_if_a_foreign_key_on_the_best_candidate_property_already_exists()
        {
            var fkProperty = DependentType.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.HasForeignKey(
                PrincipalType, new[] { fkProperty }, ConfigurationSource.Convention);

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);

            relationshipBuilder = DependentType.Relationship(
                PrincipalType, "SomeNav", null, ConfigurationSource.Convention);

            Assert.Equal(nameof(PrincipalEntity) + nameof(PrincipalEntity.PeeKay), relationshipBuilder.Metadata.Properties.Single().Name);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);

            var newFk = newRelationshipBuilder.Metadata;
            Assert.Equal(2, DependentType.Metadata.GetForeignKeys().Count());
            Assert.Equal("SomeNav" + nameof(PrincipalEntity.PeeKay), newFk.Properties.Single().Name);
            Assert.Null(newFk.GetForeignKeyPropertiesConfigurationSource());

            Assert.Equal(
                CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                    nameof(DependentEntity),
                    nameof(PrincipalEntity),
                    nameof(DependentEntity) + ".SomeNav",
                    nameof(PrincipalEntity),
                    "{'" + nameof(DependentEntity.SomeNavPeEKaY) + "'}"),
                Assert.Throws<InvalidOperationException>(() =>
                    convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder)).Message);
        }

        [Fact]
        public void Logs_warning_if_foreign_key_property_names_are_order_dependent()
        {
            var relationshipBuilder = DependentType.Relationship(
                PrincipalType, (string)null, null, ConfigurationSource.Convention);

            var otherRelationshipBuilder = DependentType.Relationship(
                PrincipalType, (string)null, null, ConfigurationSource.Convention);

            Assert.Equal(nameof(PrincipalEntity) + nameof(PrincipalEntity.PeeKay),
                relationshipBuilder.Metadata.Properties.Single().Name);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.Null(newRelationshipBuilder.Metadata.GetForeignKeyPropertiesConfigurationSource());

            newRelationshipBuilder = convention.Apply(otherRelationshipBuilder);
            Assert.Equal(2, DependentType.Metadata.GetForeignKeys().Count());
            Assert.Null(newRelationshipBuilder.Metadata.GetForeignKeyPropertiesConfigurationSource());

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);

            Assert.Equal(1, Log.Count);
            Assert.Equal(LogLevel.Warning, Log[0].Level);
            Assert.Equal(CoreStrings.LogConflictingShadowForeignKeys.GenerateMessage(
                nameof(DependentEntity), nameof(PrincipalEntity), nameof(DependentEntity)), Log[0].Message);
        }

        [Fact]
        public void Inverts_if_principal_entity_type_can_have_non_pk_fk_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention).Metadata;

            var relationshipBuilder = PrincipalType.Relationship(DependentType, ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);
            Assert.Same(DependentType.Metadata, newRelationshipBuilder.Metadata.DeclaringEntityType);

            newRelationshipBuilder = convention.Apply(newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_invert_if_dependent_entity_type_can_have_non_pk_fk_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention).Metadata;

            var relationshipBuilder = DependentType.Relationship(PrincipalType, ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.Same(DependentType.Metadata, newRelationshipBuilder.Metadata.DeclaringEntityType);

            newRelationshipBuilder = convention.Apply(newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.GetForeignKeys().Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(DependentType.Metadata, fk.DeclaringEntityType);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_invert_if_both_entity_types_can_have_non_pk_fk_property()
        {
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);
            PrincipalType.Property(PrincipalEntity.DependentEntityKayPeeProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(PrincipalType, ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.Same(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)newRelationshipBuilder.Metadata;
            Assert.Same(DependentType.Metadata, fk.DeclaringEntityType);
            Assert.Same(PrincipalType.Metadata.FindPrimaryKey(), fk.PrincipalKey);
            Assert.True(fk.IsUnique);
            Assert.False(fk.IsRequired);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_not_invert_if_principal_entity_type_is_defining_the_dependent_entity_type()
        {
            PrincipalType.Property(nameof(PrincipalEntity.DependentEntityKayPee), ConfigurationSource.Convention);
            PrincipalType.Metadata.Model.RemoveEntityType(typeof(DependentEntity));
            var relationshipBuilder = PrincipalType.Owns(
                typeof(DependentEntity), nameof(PrincipalEntity.InverseReferenceNav), ConfigurationSource.Convention);
            var dependentTypeBuilder = relationshipBuilder.Metadata.DeclaringEntityType.Builder;
            dependentTypeBuilder.PrimaryKey(new[] { nameof(DependentEntity.KayPee) }, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.Same(dependentTypeBuilder.Metadata, newRelationshipBuilder.Metadata.DeclaringEntityType);

            newRelationshipBuilder = convention.Apply(newRelationshipBuilder);

            var fk = (IForeignKey)dependentTypeBuilder.Metadata.GetForeignKeys().Single();
            Assert.Same(dependentTypeBuilder.Metadata, fk.DeclaringEntityType);
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
            Assert.True(fk.IsUnique);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Does_nothing_if_matching_shadow_property_added()
        {
            var relationshipBuilder = DependentType.Relationship(PrincipalType, "SomeNav", null, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            relationshipBuilder = convention.Apply(relationshipBuilder);

            var fk = relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.GetForeignKeys().Single());
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.False(fk.IsUnique);

            var property = DependentType.Property("SomeNavId", typeof(int?), ConfigurationSource.Convention);

            Assert.Same(property, convention.Apply(property));
            Assert.Same(fk, DependentType.Metadata.GetForeignKeys().Single());
            Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.False(fk.IsUnique);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Sets_foreign_key_if_matching_non_shadow_property_added()
        {
            var relationshipBuilder = DependentType.Relationship(PrincipalType, "SomeNav", null, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            relationshipBuilder = convention.Apply(relationshipBuilder);

            var fk = relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.GetForeignKeys().Single());
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.True(fk.Properties.Single().IsShadowProperty);
            Assert.False(fk.IsUnique);

            var property = DependentType.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);

            Assert.Same(property, convention.Apply(property));

            var newFk = DependentType.Metadata.GetForeignKeys().Single();
            Assert.NotSame(fk, newFk);
            Assert.Equal(property.Metadata, newFk.Properties.Single());

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Inverts_and_sets_foreign_key_if_matching_non_shadow_property_added_on_principal_type()
        {
            var relationshipBuilder = PrincipalType
                .Relationship(DependentType, "InverseReferenceNav", "SomeNav", ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.Convention);

            var fk = relationshipBuilder.Metadata;
            Assert.Same(fk, PrincipalType.Metadata.GetForeignKeys().Single());
            Assert.True(fk.Properties.Single().IsShadowProperty);
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Same(fk.DeclaringEntityType, PrincipalType.Metadata);
            Assert.Same(fk.PrincipalEntityType, DependentType.Metadata);
            Assert.True(fk.IsUnique);

            var property = DependentType.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            Assert.Same(property, convention.Apply(property));
            Assert.Same(property, convention.Apply(property));

            var newFk = DependentType.Metadata.GetForeignKeys().Single();
            Assert.NotSame(fk, newFk);
            Assert.Equal(property.Metadata, newFk.Properties.Single());
            Assert.Same(newFk.DeclaringEntityType, fk.PrincipalEntityType);
            Assert.Same(newFk.PrincipalEntityType, fk.DeclaringEntityType);
            Assert.True(newFk.IsUnique);

            convention.Apply(relationshipBuilder.Metadata.DeclaringEntityType.Model.Builder);
        }

        [Fact]
        public void Throws_on_ambiguous_relationship()
        {
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention);

            var convention = CreateForeignKeyPropertyDiscoveryConvention();
            var newRelationshipBuilder = convention.Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var otherRelationshipBuilder = DependentType.Relationship(
                PrincipalType,
                (string)null,
                null,
                ConfigurationSource.Convention);

            var otherNewRelationshipBuilder = convention.Apply(otherRelationshipBuilder);
            Assert.Same(otherRelationshipBuilder, otherNewRelationshipBuilder);

            Assert.Equal(
                CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                    nameof(DependentEntity) + ".SomeNav",
                    nameof(PrincipalEntity),
                    nameof(DependentEntity),
                    nameof(PrincipalEntity),
                    "{'" + nameof(DependentEntity.PrincipalEntityPeEKaY) + "'}"),
                Assert.Throws<InvalidOperationException>(() => convention.Apply(DependentType.Metadata.Model.Builder)).Message);

            newRelationshipBuilder.Metadata.UpdateForeignKeyPropertiesConfigurationSource(ConfigurationSource.Explicit);

            convention.Apply(DependentType.Metadata.Model.Builder);
        }

        private static InternalModelBuilder BuildModel()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var principalType = modelBuilder.Entity(typeof(PrincipalEntity), ConfigurationSource.Explicit);
            principalType.PrimaryKey(new[] { nameof(PrincipalEntity.PeeKay) }, ConfigurationSource.Explicit);

            var dependentType = modelBuilder.Entity(typeof(DependentEntity), ConfigurationSource.Explicit);
            dependentType.PrimaryKey(new[] { nameof(DependentEntity.KayPee) }, ConfigurationSource.Explicit);

            var principalTypeWithCompositeKey = modelBuilder.Entity(typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Explicit);
            principalTypeWithCompositeKey.PrimaryKey(new[] { PrincipalEntityWithCompositeKey.IdProperty, PrincipalEntityWithCompositeKey.NameProperty }, ConfigurationSource.Explicit);
            principalTypeWithCompositeKey.Property(PrincipalEntityWithCompositeKey.NameProperty, ConfigurationSource.Explicit).IsRequired(true, ConfigurationSource.Explicit);

            var dependentTypeWithCompositeKey = modelBuilder.Entity(typeof(DependentEntityWithCompositeKey), ConfigurationSource.Explicit);
            dependentTypeWithCompositeKey.PrimaryKey(new[] { nameof(DependentEntityWithCompositeKey.NotId), nameof(DependentEntityWithCompositeKey.NotName) }, ConfigurationSource.Explicit);

            return modelBuilder;
        }

        private ForeignKeyPropertyDiscoveryConvention CreateForeignKeyPropertyDiscoveryConvention()
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
            return new ForeignKeyPropertyDiscoveryConvention(new DiagnosticsLogger<DbLoggerCategory.Model>(
                new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Name),
                options,
                new DiagnosticListener("Fake")));
        }

        public List<(LogLevel Level, EventId Id, string Message)> Log { get; }
            = new List<(LogLevel, EventId, string)>();

        private Property PrimaryKey => PrincipalType.Metadata.FindPrimaryKey().Properties.Single();

        private InternalEntityTypeBuilder PrincipalType => _model.Entity(typeof(PrincipalEntity), ConfigurationSource.Convention);

        private InternalEntityTypeBuilder DependentType => _model.Entity(typeof(DependentEntity), ConfigurationSource.Convention);

        private IReadOnlyList<Property> CompositePrimaryKey => PrincipalTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties;

        private InternalEntityTypeBuilder PrincipalTypeWithCompositeKey => _model.Entity(typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Convention);

        private InternalEntityTypeBuilder DependentTypeWithCompositeKey => _model.Entity(typeof(DependentEntityWithCompositeKey), ConfigurationSource.Convention);

        private class PrincipalEntity
        {
            public static readonly PropertyInfo DependentEntityKayPeeProperty = typeof(PrincipalEntity).GetProperty("DependentEntityKayPee");

            public int PeeKay { get; set; }
            public int? DependentEntityKayPee { get; set; }
            public IEnumerable<DependentEntity> InverseNav { get; set; }
            public DependentEntity InverseReferenceNav { get; set; }
            public PrincipalEntity SelfRef { get; set; }
        }

        private class DependentEntity
        {
            public static readonly PropertyInfo SomeNavIDProperty = typeof(DependentEntity).GetProperty("SomeNavID");
            public static readonly PropertyInfo SomeNavPeEKaYProperty = typeof(DependentEntity).GetProperty("SomeNavPeEKaY");
            public static readonly PropertyInfo PrincipalEntityIDProperty = typeof(DependentEntity).GetProperty("PrincipalEntityID");
            public static readonly PropertyInfo PrincipalEntityPeEKaYProperty = typeof(DependentEntity).GetProperty("PrincipalEntityPeEKaY");
            public static readonly PropertyInfo IDProperty = typeof(DependentEntity).GetProperty("ID");
            public static readonly PropertyInfo PeEKaYProperty = typeof(DependentEntity).GetProperty("PeEKaY");

            public int KayPee { get; set; }
            public int SomeNavID { get; set; }
            public int SomeNavPeEKaY { get; set; }
            public int PrincipalEntityID { get; set; }
            public int PrincipalEntityPeEKaY { get; set; }
            public int ID { get; set; }
            public int PeEKaY { get; set; }

            public PrincipalEntity SomeNav { get; set; }
            public PrincipalEntity SomeOtherNav { get; set; }
        }

        private class PrincipalEntityWithCompositeKey
        {
            public static readonly PropertyInfo IdProperty = typeof(PrincipalEntityWithCompositeKey).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(PrincipalEntityWithCompositeKey).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<DependentEntityWithCompositeKey> InverseNav { get; set; }
            public DependentEntityWithCompositeKey InverseReferenceNav { get; set; }
        }

        private class DependentCompositeBase
        {
            public int NotId { get; set; }
            public string NotName { get; set; }
        }

        private class DependentEntityWithCompositeKey : DependentCompositeBase
        {
            public static readonly PropertyInfo NavPropIdProperty = typeof(DependentEntityWithCompositeKey).GetProperty("NavPropId");
            public static readonly PropertyInfo NavPropNameProperty = typeof(DependentEntityWithCompositeKey).GetProperty("NavPropName");
            public static readonly PropertyInfo PrincipalEntityWithCompositeKeyIdProperty = typeof(DependentEntityWithCompositeKey).GetProperty("PrincipalEntityWithCompositeKeyId");
            public static readonly PropertyInfo PrincipalEntityWithCompositeKeyNameProperty = typeof(DependentEntityWithCompositeKey).GetProperty("PrincipalEntityWithCompositeKeyName");
            public static readonly PropertyInfo IdProperty = typeof(DependentEntityWithCompositeKey).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(DependentEntityWithCompositeKey).GetProperty("Name");

            public int NavPropId { get; set; }
            public string NavPropName { get; set; }
            public int PrincipalEntityWithCompositeKeyId { get; set; }
            public string PrincipalEntityWithCompositeKeyName { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }

            public PrincipalEntityWithCompositeKey NavProp { get; set; }
        }
    }
}
