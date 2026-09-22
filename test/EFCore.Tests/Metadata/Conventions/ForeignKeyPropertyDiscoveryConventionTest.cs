// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class ForeignKeyPropertyDiscoveryConventionTest
{
    private readonly InternalModelBuilder _model = BuildModel();

    [ConditionalFact]
    public void Does_not_override_explicit_foreign_key_created_using_given_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        dependentTypeBuilder.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);
        var fkProperty = dependentTypeBuilder.Property(typeof(int), "No!No!", ConfigurationSource.Convention).Metadata;

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Explicit)
            .HasForeignKey(new[] { fkProperty }, ConfigurationSource.Explicit);

        RunConvention(relationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fk, dependentTypeBuilder.Metadata.GetForeignKeys().Single());
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.False(fk.IsUnique);
        Assert.True(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_override_explicit_composite_foreign_key_created_using_given_properties()
    {
        var dependentTypeBuilder = DependentTypeWithCompositeKey.Builder;
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.NavPropNameProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);
        var fkProperty1 = dependentTypeBuilder.Property(typeof(int), "No!No!", ConfigurationSource.Convention);
        var fkProperty2 = dependentTypeBuilder.Property(typeof(string), "No!No!2", ConfigurationSource.Convention);
        fkProperty2.IsRequired(true, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Explicit)
            .HasForeignKey(new[] { fkProperty1.Metadata, fkProperty2.Metadata }, ConfigurationSource.Explicit);

        RunConvention(relationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fk, DependentTypeWithCompositeKey.GetForeignKeys().Single());
        Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
        Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
        Assert.False(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Returns_same_builder_if_no_matching_clr_properties_found()
    {
        var relationshipBuilder = DependentType.Builder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = relationshipBuilder.Metadata;
        Assert.Same(fk, DependentType.GetForeignKeys().Single());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.False(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_navigation_plus_PK_name_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention).Metadata;
        dependentTypeBuilder.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.False(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_navigation_plus_PK_name_properties()
    {
        var dependentTypeBuilder = DependentTypeWithCompositeKey.Builder;
        var fkProperty1 = dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
        var fkProperty2 = dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.NavPropNameProperty, ConfigurationSource.Convention);
        fkProperty2.IsRequired(true, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalTypeWithCompositeKey,
            "NavProp",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentTypeWithCompositeKey.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
        Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
        Assert.False(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_navigation_plus_Id_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention).Metadata;
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.False(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_principal_type_plus_PK_name_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention)
            .Metadata;
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_principal_type_plus_Id_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention)
            .Metadata;
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_principal_type_plus_PK_name_properties()
    {
        var dependentTypeBuilder = DependentTypeWithCompositeKey.Builder;
        var fkProperty1 = dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
        var fkProperty2 = dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
        fkProperty2.IsRequired(true, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalTypeWithCompositeKey,
            "NavProp",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentTypeWithCompositeKey.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
        Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_PK_name_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_key_Id_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.PrimaryKey(new[] { DependentEntity.IDProperty }, ConfigurationSource.Explicit)
            .Metadata.Properties.Single();

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation)
            .DependentEntityType(DependentType, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.True(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_non_key_Id_property()
    {
        var relationshipBuilder = DependentType.Builder.HasRelationship(
                PrincipalType,
                "SomeNav",
                null,
                ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        Assert.NotEqual(DependentEntity.IDProperty.Name, DependentType.FindPrimaryKey().Properties.Single().Name);

        var fk = (IReadOnlyForeignKey)newRelationshipBuilder.Metadata;
        Assert.Same(fk, DependentType.GetForeignKeys().Single());
        Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.True(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_PK_name_properties()
    {
        var dependentTypeBuilder = DependentTypeWithCompositeKey.Builder;
        dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(
                DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention)
            .IsRequired(true, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalTypeWithCompositeKey,
            nameof(DependentEntityWithCompositeKey.NavProp),
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentTypeWithCompositeKey.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Equal("NavProp" + CompositePrimaryKey[0].Name + "1", fk.Properties[0].Name);
        Assert.Equal("NavProp" + CompositePrimaryKey[1].Name + "1", fk.Properties[1].Name);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_PK_name_properties_if_subset_of_dependent_PK_and_contains_id()
    {
        var dependentTypeBuilder = DependentTypeWithCompositeKey.Builder;
        var pkProperty1 = dependentTypeBuilder.Property(
                DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention)
            .Metadata;
        var pkProperty2 = dependentTypeBuilder.Property(
                DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention)
            .IsRequired(true, ConfigurationSource.Convention)
            .Metadata;
        var pkProperty3 = dependentTypeBuilder.Property(
                DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention)
            .Metadata;

        dependentTypeBuilder.PrimaryKey(new[] { pkProperty1, pkProperty2, pkProperty3 }, ConfigurationSource.Explicit);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalTypeWithCompositeKey,
            nameof(DependentEntityWithCompositeKey.NavProp),
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentTypeWithCompositeKey.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Equal("NavProp" + CompositePrimaryKey[0].Name + "1", fk.Properties[0].Name);
        Assert.Equal("NavProp" + CompositePrimaryKey[1].Name + "1", fk.Properties[1].Name);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_PK_name_properties_if_subset_of_dependent_PK()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var pkProperty = dependentTypeBuilder.Property(
                DependentEntity.IDProperty, ConfigurationSource.Convention)
            .IsRequired(true, ConfigurationSource.Convention)
            .Metadata;
        var fkProperty = dependentTypeBuilder.Property(
                DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention)
            .Metadata;

        dependentTypeBuilder.PrimaryKey(new[] { pkProperty, fkProperty }, ConfigurationSource.Explicit);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            nameof(DependentEntity.SomeNav),
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties[0]);
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties[0]);

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_dependent_PK_for_unique_FK_set_by_higher_source_than_convention()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = DependentType.FindPrimaryKey().Properties.Single();
        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
                PrincipalType,
                "SomeNav",
                "InverseReferenceNav",
                ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation)
            .DependentEntityType(DependentType, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_principal_type_plus_PK_name_property_of_different_type()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.Property(typeof(string), "PrincipalEntityPeeKay", ConfigurationSource.Explicit).Metadata;
        dependentTypeBuilder.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.NotSame(fkProperty, fk.Properties.Single());
        Assert.Null(((ForeignKey)fk).GetPropertiesConfigurationSource());

        var logEntry = ListLoggerFactory.Log.Single();
        Assert.Equal(LogLevel.Debug, logEntry.Level);
        Assert.Equal(
            CoreResources.LogIncompatibleMatchingForeignKeyProperties(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(DependentEntity) + "." + nameof(DependentEntity.SomeNav),
                nameof(PrincipalEntity),
                "{'PrincipalEntityPeeKay' : string}",
                "{'PeeKay' : int}"),
            logEntry.Message);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_dependent_PK_for_non_unique_FK()
    {
        var dependentTypeBuilder = DependentType.Builder;
        dependentTypeBuilder.PrimaryKey(new[] { DependentEntity.PrincipalEntityPeEKaYProperty }, ConfigurationSource.Explicit);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(PrincipalType, ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fk, DependentType.GetForeignKeys().Single());
        Assert.Equal(PrincipalType.DisplayName() + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.False(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_non_nullable_dependent_PK_for_optional_unique_FK()
    {
        var fkProperty = DependentType.FindPrimaryKey().Properties.Single();

        var relationshipBuilder = DependentType.Builder.HasRelationship(PrincipalType, ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation)
            .IsRequired(false, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.NotSame(fkProperty, fk.Properties.Single());
        Assert.Equal("PrincipalEntity" + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_dependent_PK_for_self_ref()
    {
        var relationshipBuilder = PrincipalType.Builder.HasRelationship(PrincipalType, ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fk, PrincipalType.GetForeignKeys().Single());
        Assert.Equal(PrincipalType.DisplayName() + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_for_convention_identifying_FK()
    {
        var derivedType = PrincipalType.Builder.ModelBuilder.Entity(typeof(DerivedPrincipalEntity), ConfigurationSource.Convention);
        derivedType.HasBaseType(PrincipalType, ConfigurationSource.Convention);

        PrincipalType.Builder.Property(typeof(int), nameof(PrincipalEntity.PrincipalEntityId), ConfigurationSource.Convention);
        var relationshipBuilder = derivedType.HasRelationship(
                PrincipalType, PrincipalType.FindPrimaryKey().Properties, ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Equal(nameof(PrincipalEntity.PeeKay), fk.Properties.Single().Name);
        Assert.Same(fk, derivedType.Metadata.GetForeignKeys().Single());
        Assert.True(fk.IsUnique);
        Assert.True(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Matches_composite_dependent_PK_for_unique_FK()
    {
        var dependentTypeBuilder = DependentTypeWithCompositeKey.Builder;
        var fkProperty1 = DependentTypeWithCompositeKey.FindPrimaryKey().Properties[0];
        var fkProperty2 = DependentTypeWithCompositeKey.FindPrimaryKey().Properties[1];

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                "InverseReferenceNav", ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation)
            .DependentEntityType(DependentTypeWithCompositeKey, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentTypeWithCompositeKey.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty1, fk.Properties[0]);
        Assert.Same(fkProperty2, fk.Properties[1]);
        Assert.Same(PrincipalTypeWithCompositeKey.FindPrimaryKey(), fk.PrincipalKey);
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_composite_dependent_PK_for_unique_FK_on_derived_type()
    {
        var modelBuilder = new InternalModelBuilder(new Model());

        var principalTypeWithCompositeKey = modelBuilder.Entity(typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Explicit);
        principalTypeWithCompositeKey.PrimaryKey(
            new[] { PrincipalEntityWithCompositeKey.IdProperty, PrincipalEntityWithCompositeKey.NameProperty },
            ConfigurationSource.Explicit);
        principalTypeWithCompositeKey.Property(PrincipalEntityWithCompositeKey.NameProperty, ConfigurationSource.Explicit)
            .IsRequired(true, ConfigurationSource.Explicit);

        var dependentTypeWithCompositeKeyBase = modelBuilder.Entity(typeof(DependentCompositeBase), ConfigurationSource.Explicit);
        var dependentTypeWithCompositeKey = modelBuilder.Entity(typeof(DependentEntityWithCompositeKey), ConfigurationSource.Explicit);
        dependentTypeWithCompositeKey.HasBaseType(dependentTypeWithCompositeKeyBase.Metadata, ConfigurationSource.Explicit);
        dependentTypeWithCompositeKeyBase.PrimaryKey(
            new[] { nameof(DependentEntityWithCompositeKey.NotId), nameof(DependentEntityWithCompositeKey.NotName) },
            ConfigurationSource.Explicit);

        var relationshipBuilder = dependentTypeWithCompositeKey.HasRelationship(
                principalTypeWithCompositeKey.Metadata,
                "NavProp",
                "InverseReferenceNav", ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.DataAnnotation)
            .DependentEntityType(dependentTypeWithCompositeKey.Metadata, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);

        var fk = dependentTypeWithCompositeKey.Metadata.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.True(fk.IsUnique);
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Same(principalTypeWithCompositeKey.Metadata.FindPrimaryKey(), fk.PrincipalKey);
        Assert.NotEqual(dependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[0], fk.Properties[0]);
        Assert.NotEqual(dependentTypeWithCompositeKey.Metadata.FindPrimaryKey().Properties[1], fk.Properties[1]);
        Assert.Equal("NavProp" + CompositePrimaryKey[0].Name + "1", fk.Properties[0].Name);
        Assert.Equal("NavProp" + CompositePrimaryKey[1].Name + "1", fk.Properties[1].Name);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_composite_dependent_PK_for_non_unique_FK()
    {
        DependentTypeWithCompositeKey.Builder.PrimaryKey(
            new[] { DependentEntityWithCompositeKey.NavPropIdProperty, DependentEntityWithCompositeKey.NavPropNameProperty },
            ConfigurationSource.Explicit);

        var relationshipBuilder = DependentTypeWithCompositeKey.Builder.HasRelationship(
                PrincipalTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = relationshipBuilder.Metadata;
        Assert.Same(fk, DependentTypeWithCompositeKey.GetForeignKeys().Single());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Equal("NavProp" + CompositePrimaryKey[0].Name + "1", fk.Properties[0].Name);
        Assert.Equal("NavProp" + CompositePrimaryKey[1].Name + "1", fk.Properties[1].Name);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
        Assert.False(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_composite_dependent_PK_for_unique_FK_if_count_mismatched()
    {
        var fkProperty1 = DependentTypeWithCompositeKey.FindPrimaryKey().Properties[0];
        DependentTypeWithCompositeKey.Builder.PrimaryKey(new[] { fkProperty1.Name }, ConfigurationSource.Explicit);

        var relationshipBuilder = DependentTypeWithCompositeKey.Builder
            .HasRelationship(PrincipalTypeWithCompositeKey, ConfigurationSource.Convention)
            .HasPrincipalKey(PrincipalTypeWithCompositeKey.FindPrimaryKey().Properties, ConfigurationSource.DataAnnotation)
            .IsUnique(true, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fk, DependentTypeWithCompositeKey.GetForeignKeys().Single());
        Assert.Equal(2, fk.Properties.Count);
        Assert.NotSame(fkProperty1, fk.Properties[0]);
        Assert.NotSame(fkProperty1, fk.Properties[1]);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_composite_dependent_PK_for_unique_FK_if_order_mismatched()
    {
        var fkProperty1 = DependentTypeWithCompositeKey.FindPrimaryKey().Properties[0];
        var fkProperty2 = DependentTypeWithCompositeKey.FindPrimaryKey().Properties[1];
        DependentTypeWithCompositeKey.Builder.PrimaryKey(new[] { fkProperty2.Name, fkProperty1.Name }, ConfigurationSource.Explicit);

        var relationshipBuilder = DependentTypeWithCompositeKey.Builder.HasRelationship(
                PrincipalTypeWithCompositeKey, "NavProp", "InverseReferenceNav", ConfigurationSource.Convention)
            .HasPrincipalKey(PrincipalTypeWithCompositeKey.FindPrimaryKey().Properties, ConfigurationSource.DataAnnotation)
            .IsUnique(true, ConfigurationSource.DataAnnotation);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = relationshipBuilder.Metadata;
        Assert.Same(fk, DependentTypeWithCompositeKey.GetForeignKeys().Single());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Equal(2, fk.Properties.Count);
        Assert.NotSame(fkProperty1, fk.Properties[0]);
        Assert.NotSame(fkProperty2, fk.Properties[1]);
        Assert.NotSame(fkProperty1, fk.Properties[1]);
        Assert.NotSame(fkProperty2, fk.Properties[0]);
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_properties_with_different_base_names()
    {
        var dependentTypeBuilder = DependentTypeWithCompositeKey.Builder;
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(
            DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalTypeWithCompositeKey, "NavProp", null, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = relationshipBuilder.Metadata;
        Assert.Same(fk, DependentTypeWithCompositeKey.GetForeignKeys().Single());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Same(CompositePrimaryKey[0], fk.PrincipalKey.Properties[0]);
        Assert.Same(CompositePrimaryKey[1], fk.PrincipalKey.Properties[1]);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_match_if_a_foreign_key_on_the_best_candidate_property_already_exists()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention).Metadata;
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(PrincipalType, new[] { fkProperty }, ConfigurationSource.Convention);

        var newRelationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType, "SomeNav", null, ConfigurationSource.Convention);

        Assert.Equal(
            "SomeNav" + nameof(PrincipalEntity.PeeKay),
            newRelationshipBuilder.Metadata.Properties.Single().Name);

        newRelationshipBuilder = RunConvention(newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.False(fk.IsUnique);

        var newFk = newRelationshipBuilder.Metadata;
        Assert.Equal(2, DependentType.GetForeignKeys().Count());
        Assert.Equal("SomeNav" + nameof(PrincipalEntity.PeeKay), newFk.Properties.Single().Name);
        Assert.Null(newFk.GetPropertiesConfigurationSource());

        Assert.Equal(
            CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                nameof(DependentEntity),
                nameof(PrincipalEntity),
                nameof(DependentEntity) + ".SomeNav",
                nameof(PrincipalEntity),
                "{'" + nameof(DependentEntity.SomeNavPeEKaY) + "'}"),
            Assert.Throws<InvalidOperationException>(ValidateModel).Message);
    }

    [ConditionalFact]
    public void Does_not_match_if_a_foreign_key_on_the_best_candidate_property_already_configured_explicitly()
    {
        var dependentTypeBuilder = DependentType.Builder;
        var fkProperty = dependentTypeBuilder.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention).Metadata;
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

        var derivedTypeBuilder = _model.Entity(typeof(DerivedPrincipalEntity), ConfigurationSource.Convention);
        derivedTypeBuilder.HasBaseType(PrincipalType, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder
            .HasRelationship(derivedTypeBuilder.Metadata, new[] { fkProperty }, ConfigurationSource.Explicit);
        var compositeRelationshipBuilder = dependentTypeBuilder
            .HasRelationship(PrincipalTypeWithCompositeKey, new[] { fkProperty }, ConfigurationSource.Explicit);

        var newRelationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType, "SomeNav", null, ConfigurationSource.Convention);

        Assert.Equal(
            "SomeNav" + nameof(PrincipalEntity.PeeKay),
            newRelationshipBuilder.Metadata.Properties.Single().Name);

        newRelationshipBuilder = RunConvention(newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.False(fk.IsUnique);

        var newFk = newRelationshipBuilder.Metadata;
        Assert.Equal(3, DependentType.GetForeignKeys().Count());
        Assert.Equal("SomeNav" + nameof(PrincipalEntity.PeeKay), newFk.Properties.Single().Name);
        Assert.Null(newFk.GetPropertiesConfigurationSource());

        ValidateModel();
    }

    [ConditionalFact]
    public void Logs_warning_if_foreign_key_property_names_are_order_dependent()
    {
        var relationshipBuilder = DependentType.Builder.HasRelationship(
            PrincipalType, (string)null, null, ConfigurationSource.Convention);

        var otherRelationshipBuilder = DependentType.Builder.HasRelationship(
            PrincipalType, (string)null, null, ConfigurationSource.Convention);

        Assert.Equal(
            nameof(PrincipalEntity) + nameof(PrincipalEntity.PeeKay),
            relationshipBuilder.Metadata.Properties.Single().Name);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);
        Assert.Null(newRelationshipBuilder.Metadata.GetPropertiesConfigurationSource());

        newRelationshipBuilder = RunConvention(otherRelationshipBuilder);
        Assert.Same(otherRelationshipBuilder, newRelationshipBuilder);
        Assert.Equal(2, DependentType.GetForeignKeys().Count());
        Assert.Null(newRelationshipBuilder.Metadata.GetPropertiesConfigurationSource());

        ValidateModel();

        var logEntry = ListLoggerFactory.Log.Single();
        Assert.Equal(LogLevel.Warning, logEntry.Level);
        Assert.Equal(
            CoreResources.LogConflictingShadowForeignKeys(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(DependentEntity), nameof(PrincipalEntity), nameof(DependentEntity)), logEntry.Message);
    }

    [ConditionalFact]
    public void Inverts_if_principal_entity_type_can_have_non_pk_fk_property()
    {
        var fkProperty = DependentType.Builder.Property(
            DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention).Metadata;

        var relationshipBuilder = PrincipalType.Builder.HasRelationship(DependentType, ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.NotSame(relationshipBuilder, newRelationshipBuilder);
        Assert.Same(DependentType, newRelationshipBuilder.Metadata.DeclaringEntityType);

        newRelationshipBuilder = RunConvention(newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_invert_if_weak_entity_type_can_have_non_pk_fk_property()
    {
        var fkProperty = DependentType.Builder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention)
            .Metadata;

        var relationshipBuilder = DependentType.Builder.HasRelationship(PrincipalType, ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);
        Assert.Same(DependentType, newRelationshipBuilder.Metadata.DeclaringEntityType);

        newRelationshipBuilder = RunConvention(newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(DependentType, fk.DeclaringEntityType);
        Assert.Same(fkProperty, fk.Properties.Single());
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.True(fk.IsUnique);
        Assert.True(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_invert_if_both_entity_types_can_have_non_pk_fk_property()
    {
        var dependentTypeBuilder = DependentType.Builder;
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
        PrincipalType.Builder.Property(PrincipalEntity.DependentEntityKayPeeProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(PrincipalType, ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)newRelationshipBuilder.Metadata;
        Assert.Same(dependentTypeBuilder.Metadata, fk.DeclaringEntityType);
        Assert.Same(PrincipalType.FindPrimaryKey(), fk.PrincipalKey);
        Assert.True(fk.IsUnique);
        Assert.False(fk.IsRequired);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_not_invert_if_principal_entity_type_owns_the_weak_entity_type()
    {
        PrincipalType.Builder.Property(nameof(PrincipalEntity.DependentEntityKayPee), ConfigurationSource.Convention);
        PrincipalType.Model.RemoveEntityType(typeof(DependentEntity));
        var relationshipBuilder = PrincipalType.Builder.HasOwnership(
            typeof(DependentEntity), nameof(PrincipalEntity.InverseReferenceNav), ConfigurationSource.Convention);
        var dependentTypeBuilder = relationshipBuilder.Metadata.DeclaringEntityType.Builder;
        dependentTypeBuilder.PrimaryKey(new[] { nameof(DependentEntity.KayPee) }, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);
        Assert.Same(dependentTypeBuilder.Metadata, newRelationshipBuilder.Metadata.DeclaringEntityType);

        newRelationshipBuilder = RunConvention(newRelationshipBuilder);

        var fk = (IReadOnlyForeignKey)dependentTypeBuilder.Metadata.GetForeignKeys().Single();
        Assert.Same(dependentTypeBuilder.Metadata, fk.DeclaringEntityType);
        Assert.Same(fk, newRelationshipBuilder.Metadata);
        Assert.Same(PrimaryKey, fk.PrincipalKey.Properties.Single());
        Assert.True(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Does_nothing_if_matching_shadow_property_added()
    {
        var relationshipBuilder = DependentType.Builder.HasRelationship(PrincipalType, "SomeNav", null, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = relationshipBuilder.Metadata;
        Assert.Same(fk, DependentType.GetForeignKeys().Single());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.False(fk.IsUnique);

        var property = DependentType.Builder.Property(typeof(int?), "SomeNavId", ConfigurationSource.Convention);

        Assert.Same(property, RunConvention(property));

        Assert.Same(fk, DependentType.GetForeignKeys().Single());
        Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.False(fk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Sets_foreign_key_if_matching_non_shadow_property_added()
    {
        var relationshipBuilder = DependentType.Builder.HasRelationship(PrincipalType, "SomeNav", null, ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var fk = (IConventionForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fk, DependentType.GetForeignKeys().Single());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
        Assert.True(fk.Properties.Single().IsShadowProperty());
        Assert.False(fk.IsUnique);

        var property = DependentType.Builder.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);

        Assert.Same(property, RunConvention(property));

        var newFk = DependentType.GetForeignKeys().Single();
        Assert.Same(fk, newFk);
        Assert.Equal(property.Metadata, newFk.Properties.Single());

        ValidateModel();
    }

    [ConditionalFact]
    public void Inverts_and_sets_foreign_key_if_matching_non_shadow_property_added_on_principal_type()
    {
        var relationshipBuilder = PrincipalType.Builder
            .HasRelationship(DependentType, "InverseReferenceNav", "SomeNav", ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.Convention);

        var fk = (IConventionForeignKey)relationshipBuilder.Metadata;
        Assert.Same(fk, PrincipalType.GetForeignKeys().Single());
        Assert.True(fk.Properties.Single().IsShadowProperty());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Same(fk.DeclaringEntityType, PrincipalType);
        Assert.Same(fk.PrincipalEntityType, DependentType);
        Assert.True(fk.IsUnique);

        var property = DependentType.Builder.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);

        Assert.Same(property, RunConvention(property));
        Assert.Same(property, RunConvention(property));

        var newFk = DependentType.GetForeignKeys().Single();
        Assert.NotSame(fk, newFk);
        Assert.Equal(property.Metadata, newFk.Properties.Single());
        Assert.Same(newFk.DeclaringEntityType, fk.PrincipalEntityType);
        Assert.Same(newFk.PrincipalEntityType, fk.DeclaringEntityType);
        Assert.True(newFk.IsUnique);

        ValidateModel();
    }

    [ConditionalFact]
    public void Throws_on_ambiguous_relationship()
    {
        var dependentTypeBuilder = DependentType.Builder;
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
        dependentTypeBuilder.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

        var relationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            "SomeNav",
            null,
            ConfigurationSource.Convention);

        var newRelationshipBuilder = RunConvention(relationshipBuilder);
        Assert.Same(relationshipBuilder, newRelationshipBuilder);

        var otherRelationshipBuilder = dependentTypeBuilder.HasRelationship(
            PrincipalType,
            (string)null,
            null,
            ConfigurationSource.Convention);

        var otherNewRelationshipBuilder = RunConvention(otherRelationshipBuilder);
        Assert.Same(otherRelationshipBuilder, otherNewRelationshipBuilder);

        Assert.Equal(
            CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                nameof(DependentEntity) + ".SomeNav",
                nameof(PrincipalEntity),
                nameof(DependentEntity),
                nameof(PrincipalEntity),
                "{'" + nameof(DependentEntity.PrincipalEntityPeEKaY) + "'}"),
            Assert.Throws<InvalidOperationException>(() => ValidateModel()).Message);

        newRelationshipBuilder.Metadata.UpdatePropertiesConfigurationSource(ConfigurationSource.Explicit);

        ValidateModel();
    }

    private static InternalModelBuilder BuildModel()
    {
        var modelBuilder = new InternalModelBuilder(new Model());

        var principalType = modelBuilder.Entity(typeof(PrincipalEntity), ConfigurationSource.Explicit);
        principalType.PrimaryKey(new[] { nameof(PrincipalEntity.PeeKay) }, ConfigurationSource.Explicit);

        var dependentType = modelBuilder.Entity(typeof(DependentEntity), ConfigurationSource.Explicit);
        dependentType.PrimaryKey(new[] { nameof(DependentEntity.KayPee) }, ConfigurationSource.Explicit);

        var principalTypeWithCompositeKey = modelBuilder.Entity(typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Explicit);
        principalTypeWithCompositeKey.PrimaryKey(
            new[] { PrincipalEntityWithCompositeKey.IdProperty, PrincipalEntityWithCompositeKey.NameProperty },
            ConfigurationSource.Explicit);
        principalTypeWithCompositeKey.Property(PrincipalEntityWithCompositeKey.NameProperty, ConfigurationSource.Explicit)
            .IsRequired(true, ConfigurationSource.Explicit);

        var dependentTypeWithCompositeKey = modelBuilder.Entity(typeof(DependentEntityWithCompositeKey), ConfigurationSource.Explicit);
        dependentTypeWithCompositeKey.PrimaryKey(
            new[] { nameof(DependentEntityWithCompositeKey.NotId), nameof(DependentEntityWithCompositeKey.NotName) },
            ConfigurationSource.Explicit);

        return modelBuilder;
    }

    private InternalForeignKeyBuilder RunConvention(InternalForeignKeyBuilder relationshipBuilder)
    {
        var convention = CreateForeignKeyPropertyDiscoveryConvention();
        var context = new ConventionContext<IConventionForeignKeyBuilder>(
            relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);
        convention.ProcessForeignKeyAdded(relationshipBuilder, context);
        if (context.ShouldStopProcessing())
        {
            return (InternalForeignKeyBuilder)context.Result;
        }

        return relationshipBuilder;
    }

    private InternalPropertyBuilder RunConvention(InternalPropertyBuilder propertyBuilder)
    {
        var convention = CreateForeignKeyPropertyDiscoveryConvention();
        var context = new ConventionContext<IConventionPropertyBuilder>(
            propertyBuilder.Metadata.DeclaringType.Model.ConventionDispatcher);
        convention.ProcessPropertyAdded(propertyBuilder, context);
        if (context.ShouldStopProcessing())
        {
            return (InternalPropertyBuilder)context.Result;
        }

        return propertyBuilder;
    }

    private void ValidateModel()
    {
        var convention = CreateForeignKeyPropertyDiscoveryConvention();
        convention.ProcessModelFinalizing(_model, new ConventionContext<IConventionModelBuilder>(_model.Metadata.ConventionDispatcher));
    }

    private ForeignKeyPropertyDiscoveryConvention CreateForeignKeyPropertyDiscoveryConvention()
    {
        ListLoggerFactory.Clear();
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
        return new ForeignKeyPropertyDiscoveryConvention(CreateDependencies());
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>()
            with
            {
                Logger = CreateLogger()
            };

    private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
    {
        ListLoggerFactory.Clear();
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

    public ListLoggerFactory ListLoggerFactory { get; }
        = new(l => l == DbLoggerCategory.Model.Name);

    private Property PrimaryKey
        => PrincipalType.FindPrimaryKey().Properties.Single();

    private EntityType PrincipalType
        => _model.Entity(typeof(PrincipalEntity), ConfigurationSource.Convention).Metadata;

    private EntityType DependentType
        => _model.Entity(typeof(DependentEntity), ConfigurationSource.Convention).Metadata;

    private IReadOnlyList<Property> CompositePrimaryKey
        => PrincipalTypeWithCompositeKey.FindPrimaryKey().Properties;

    private EntityType PrincipalTypeWithCompositeKey
        => _model.Entity(
            typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Convention).Metadata;

    private EntityType DependentTypeWithCompositeKey
        => _model.Entity(
            typeof(DependentEntityWithCompositeKey), ConfigurationSource.Convention).Metadata;

    private class PrincipalEntity
    {
        public static readonly PropertyInfo DependentEntityKayPeeProperty =
            typeof(PrincipalEntity).GetProperty("DependentEntityKayPee");

        public int PrincipalEntityId { get; set; }
        public int PeeKay { get; set; }
        public int? DependentEntityKayPee { get; set; }
        public IEnumerable<DependentEntity> InverseNav { get; set; }
        public DependentEntity InverseReferenceNav { get; set; }
        public PrincipalEntity SelfRef { get; set; }
    }

    private class DerivedPrincipalEntity : PrincipalEntity;

    private class DependentEntity
    {
        public static readonly PropertyInfo SomeNavIDProperty = typeof(DependentEntity).GetProperty("SomeNavID");
        public static readonly PropertyInfo SomeNavPeEKaYProperty = typeof(DependentEntity).GetProperty("SomeNavPeEKaY");
        public static readonly PropertyInfo PrincipalEntityIDProperty = typeof(DependentEntity).GetProperty("PrincipalEntityID");

        public static readonly PropertyInfo PrincipalEntityPeEKaYProperty =
            typeof(DependentEntity).GetProperty("PrincipalEntityPeEKaY");

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

        public static readonly PropertyInfo PrincipalEntityWithCompositeKeyIdProperty =
            typeof(DependentEntityWithCompositeKey).GetProperty("PrincipalEntityWithCompositeKeyId");

        public static readonly PropertyInfo PrincipalEntityWithCompositeKeyNameProperty =
            typeof(DependentEntityWithCompositeKey).GetProperty("PrincipalEntityWithCompositeKeyName");

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
