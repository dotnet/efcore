// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class InternalEntityTypeBuilderTest
{
    [ConditionalFact]
    public void Relationship_returns_same_instance_for_same_navigations()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Explicit)
            .HasNavigation(
                Order.CustomerProperty.Name,
                pointsToPrincipal: true,
                ConfigurationSource.Explicit).HasNavigation(
                Customer.OrdersProperty.Name,
                pointsToPrincipal: false,
                ConfigurationSource.Explicit);

        Assert.NotNull(relationshipBuilder);
        Assert.Same(
            relationshipBuilder,
            dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention).HasNavigation(
                Order.CustomerProperty.Name,
                pointsToPrincipal: true,
                ConfigurationSource.Convention).HasNavigation(
                Customer.OrdersProperty.Name,
                pointsToPrincipal: false,
                ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Can_add_relationship_if_principal_entity_has_no_PK()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            null,
            Customer.OrdersProperty.Name,
            ConfigurationSource.DataAnnotation);

        Assert.NotNull(relationshipBuilder);
        var pkProperty = relationshipBuilder.Metadata.PrincipalKey.Properties.Single();
        Assert.Equal("TempId", pkProperty.Name);
        Assert.True(pkProperty.IsShadowProperty());
        var fkProperty = relationshipBuilder.Metadata.Properties.Single();
        Assert.Equal(nameof(Customer) + pkProperty.Name, fkProperty.Name);
        Assert.True(fkProperty.IsShadowProperty());
    }

    [ConditionalFact]
    public void Collection_navigation_to_principal_throws()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.PrincipalEndIncompatibleNavigations(
                nameof(Customer) + "." + nameof(Customer.Orders), nameof(Order), nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    principalEntityBuilder.HasRelationship(
                        dependentEntityBuilder.Metadata,
                        Customer.OrdersProperty,
                        ConfigurationSource.DataAnnotation,
                        targetIsPrincipal: true)).Message);
    }

    [ConditionalFact]
    public void Can_add_relationship_if_principal_entity_PK_name_contains_principal_entity_name()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.Property(typeof(string), "CustomerId", ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { "CustomerId" }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            (string)null,
            null,
            ConfigurationSource.DataAnnotation);

        Assert.NotNull(relationshipBuilder);
        var pkProperty = relationshipBuilder.Metadata.PrincipalKey.Properties.Single();
        Assert.Equal("CustomerId", pkProperty.Name);
        Assert.True(pkProperty.IsShadowProperty());
        var fkProperty = relationshipBuilder.Metadata.Properties.Single();
        Assert.Equal("CustomerId1", fkProperty.Name);
        Assert.True(fkProperty.IsShadowProperty());
    }

    [ConditionalFact]
    public void Can_add_relationship_if_principal_entity_PK_name_contains_principal_navigation_name()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.Property(typeof(string), "CustomerId", ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { "CustomerId" }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        dependentEntityBuilder.Ignore("CustomerId", ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            nameof(Order.Customer),
            null,
            ConfigurationSource.DataAnnotation);

        Assert.NotNull(relationshipBuilder);
        var pkProperty = relationshipBuilder.Metadata.PrincipalKey.Properties.Single();
        Assert.Equal("CustomerId", pkProperty.Name);
        Assert.True(pkProperty.IsShadowProperty());
        var fkProperty = relationshipBuilder.Metadata.Properties.Single();
        Assert.Equal("CustomerId1", fkProperty.Name);
        Assert.True(fkProperty.IsShadowProperty());
    }

    [ConditionalFact]
    public void Can_add_relationship_if_navigation_to_dependent_ignored_at_lower_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        Assert.NotNull(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));

        Assert.NotNull(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                null,
                Customer.OrdersProperty.Name,
                ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Can_add_relationship_if_navigation_to_principal_ignored_at_lower_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        Assert.NotNull(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));

        Assert.NotNull(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                Order.CustomerProperty.Name,
                null,
                ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Can_add_relationship_on_property_bag()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity("Count", ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity("Value", ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Explicit)
            .HasNavigation(
                "Count",
                pointsToPrincipal: true,
                ConfigurationSource.Explicit)
            .HasNavigation(
                "Values",
                pointsToPrincipal: false,
                ConfigurationSource.Explicit);

        var fk = relationshipBuilder.Metadata;
        Assert.Same(dependentEntityBuilder.Metadata.FindIndexerPropertyInfo(), fk.DependentToPrincipal.PropertyInfo);
        Assert.Same(principalEntityBuilder.Metadata.FindIndexerPropertyInfo(), fk.PrincipalToDependent.PropertyInfo);

        var skipNavigationBuilder = principalEntityBuilder.HasSkipNavigation(
            MemberIdentity.Create("Keys"),
            dependentEntityBuilder.Metadata,
            ConfigurationSource.Explicit);

        Assert.Same(dependentEntityBuilder.Metadata.FindIndexerPropertyInfo(), skipNavigationBuilder.Metadata.PropertyInfo);
    }

    [ConditionalFact]
    public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_non_shadow()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata.Name,
            new[] { "ShadowCustomerId" },
            ConfigurationSource.Convention);

        var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
        Assert.NotNull(shadowProperty);
        Assert.True(shadowProperty.IsShadowProperty());
        Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
    }

    [ConditionalFact]
    public void ForeignKey_creates_shadow_properties_if_principal_type_does_not_have_primary_key()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata.Name, new[] { "ShadowCustomerId" }, ConfigurationSource.Convention);

        var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
        Assert.True(shadowProperty.IsShadowProperty());
        Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());

        Assert.Null(customerEntityBuilder.Metadata.FindPrimaryKey());
        Assert.Equal(1, relationshipBuilder.Metadata.PrincipalKey.Properties.Count);
    }

    [ConditionalFact]
    public void ForeignKey_does_not_create_shadow_properties_if_corresponding_principal_key_properties_has_different_count()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        customerEntityBuilder.Property(typeof(int), "ShadowPrimaryKey", ConfigurationSource.Explicit);
        customerEntityBuilder.PrimaryKey(
            new List<string> { "ShadowPrimaryKey" }, ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NoPropertyType("ShadowCustomerId", nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () => orderEntityBuilder.HasRelationship(
                    customerEntityBuilder.Metadata.Name,
                    new[] { "ShadowCustomerId", "ShadowCustomerUnique" },
                    ConfigurationSource.Convention)).Message);
    }

    [ConditionalFact]
    public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_shadow()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        customerEntityBuilder.Property(typeof(int), "ShadowPrimaryKey", ConfigurationSource.Explicit);
        customerEntityBuilder.PrimaryKey(
            new List<string> { "ShadowPrimaryKey" }, ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata.Name,
            new[] { "ShadowCustomerId" },
            ConfigurationSource.Convention);

        var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
        Assert.NotNull(shadowProperty);
        Assert.True(shadowProperty.IsShadowProperty());
        Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
    }

    [ConditionalFact]
    public void ForeignKey_promotes_derived_foreign_key_of_lower_or_equal_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var primaryKey = principalEntityBuilder.PrimaryKey(new[] { nameof(Customer.Id) }, ConfigurationSource.Convention).Metadata;
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        Assert.NotNull(
            derivedEntityBuilder.HasRelationship(
                    principalEntityBuilder.Metadata.Name,
                    new[] { Order.IdProperty.Name },
                    primaryKey,
                    ConfigurationSource.DataAnnotation)
                .IsUnique(true, ConfigurationSource.DataAnnotation));

        entityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            ConfigurationSource.Convention);

        Assert.Single(derivedEntityBuilder.Metadata.GetForeignKeys());
        Assert.Single(entityBuilder.Metadata.GetForeignKeys());

        var foreignKeyBuilder = entityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            primaryKey,
            ConfigurationSource.Convention);

        Assert.Single(entityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredForeignKeys());
        Assert.Equal(ConfigurationSource.DataAnnotation, foreignKeyBuilder.Metadata.GetConfigurationSource());
        Assert.True(foreignKeyBuilder.Metadata.IsUnique);
    }

    [ConditionalFact]
    public void ForeignKey_returns_inherited_foreign_key_of_lower_or_equal_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var principalKey = principalEntityBuilder.HasKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit).Metadata;
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        dependentEntityBuilder.Property(typeof(int), Order.IdProperty.Name, ConfigurationSource.Convention);
        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            principalKey,
            ConfigurationSource.Convention);

        var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention);

        var relationshipBuilder = derivedDependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            principalKey,
            ConfigurationSource.DataAnnotation);

        Assert.Same(derivedDependentEntityBuilder.Metadata.GetForeignKeys().Single(), relationshipBuilder.Metadata);
        Assert.Same(dependentEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

        relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
        Assert.True(relationshipBuilder.Metadata.IsUnique);
        Assert.Null(
            relationshipBuilder.HasForeignKey(
                new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
                ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void ForeignKey_matches_existing_foreign_key_if_same_or_no_principal_key_specified_or_lower_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var primaryKey = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention).Metadata;
        var shadowKeyProperty = principalEntityBuilder.Property(typeof(int), "ShadowId", ConfigurationSource.Convention);
        var alternateKey = principalEntityBuilder.HasKey(new[] { shadowKeyProperty.Metadata.Name }, ConfigurationSource.Convention)
            .Metadata;
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        dependentEntityBuilder.Property(typeof(int), Order.IdProperty.Name, ConfigurationSource.Convention);

        var fk1 = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            ConfigurationSource.DataAnnotation).Metadata;
        var newFk1 = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            alternateKey,
            ConfigurationSource.Explicit).Metadata;

        var fk2 = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            ConfigurationSource.DataAnnotation).Metadata;
        var newFk2 = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata.Name,
            new[] { Order.IdProperty.Name },
            primaryKey,
            ConfigurationSource.Explicit).Metadata;

        Assert.NotSame(fk1, newFk1);
        Assert.Same(fk1, fk2);
        Assert.Same(fk1, newFk2);
        Assert.NotSame(newFk1, fk2);
        Assert.NotSame(newFk1, newFk2);
        Assert.Equal(2, dependentEntityBuilder.Metadata.GetForeignKeys().Count());
    }

    [ConditionalFact]
    public void Promotes_derived_relationship()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);

        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            Order.CustomerProperty.Name,
            Customer.OrdersProperty.Name,
            ConfigurationSource.DataAnnotation);

        var basePrincipalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var baseDependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        principalEntityBuilder.HasBaseType(basePrincipalEntityBuilder.Metadata, ConfigurationSource.Explicit);
        dependentEntityBuilder.HasBaseType(baseDependentEntityBuilder.Metadata, ConfigurationSource.Explicit);

        Assert.Empty(baseDependentEntityBuilder.Metadata.GetForeignKeys());

        var relationship = baseDependentEntityBuilder.HasRelationship(
            basePrincipalEntityBuilder.Metadata,
            Order.CustomerProperty.Name,
            Customer.OrdersProperty.Name,
            ConfigurationSource.Convention);

        Assert.Same(relationship.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
        Assert.Same(relationship.Metadata, principalEntityBuilder.Metadata.GetNavigations().Single().ForeignKey);
        Assert.Same(relationship.Metadata, dependentEntityBuilder.Metadata.GetNavigations().Single().ForeignKey);
        Assert.Empty(dependentEntityBuilder.Metadata.GetDeclaredForeignKeys());
        Assert.Empty(dependentEntityBuilder.Metadata.GetKeys());
        Assert.Same(relationship.Metadata.PrincipalKey, principalEntityBuilder.Metadata.GetKeys().Single());
    }

    [ConditionalFact]
    public void Returns_inherited_relationship_of_lower_or_equal_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);
        var basePrincipalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var baseDependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        principalEntityBuilder.HasBaseType(basePrincipalEntityBuilder.Metadata, ConfigurationSource.Explicit);
        dependentEntityBuilder.HasBaseType(baseDependentEntityBuilder.Metadata, ConfigurationSource.Explicit);

        baseDependentEntityBuilder.HasRelationship(
            basePrincipalEntityBuilder.Metadata,
            Order.CustomerProperty.Name,
            Customer.OrdersProperty.Name,
            ConfigurationSource.DataAnnotation);

        Assert.Null(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                Order.CustomerProperty.Name,
                null,
                ConfigurationSource.Convention));

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            Order.CustomerProperty.Name,
            null,
            ConfigurationSource.DataAnnotation);

        Assert.Same(baseDependentEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
        Assert.Null(relationshipBuilder.Metadata.PrincipalToDependent);
        Assert.Equal(Order.CustomerProperty.Name, relationshipBuilder.Metadata.DependentToPrincipal.Name);
        Assert.Empty(principalEntityBuilder.Metadata.GetNavigations());
        Assert.Same(relationshipBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
    }

    [ConditionalFact]
    public void Does_not_add_index_on_foreign_key_properties_by_convention()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.Convention);
        Assert.NotNull(relationshipBuilder);

        Assert.Empty(dependentEntityBuilder.Metadata.GetIndexes());
    }

    [ConditionalFact]
    public void Can_create_foreign_key_on_mix_of_inherited_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityTypeBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        customerEntityTypeBuilder.PrimaryKey(
            new List<PropertyInfo> { Customer.IdProperty }, ConfigurationSource.Explicit);

        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.Property(SpecialOrder.SpecialtyProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

        var relationshipBuilder = derivedEntityBuilder
            .HasRelationship(
                customerEntityTypeBuilder.Metadata.Name,
                new[] { Order.IdProperty.Name, SpecialOrder.SpecialtyProperty.Name },
                ConfigurationSource.DataAnnotation);

        Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
        Assert.Same(derivedEntityBuilder.Metadata.GetDeclaredForeignKeys().Single(), relationshipBuilder.Metadata);
        Assert.Collection(
            relationshipBuilder.Metadata.Properties,
            t1 => Assert.Same(entityBuilder.Metadata, t1.DeclaringType),
            t2 => Assert.Same(derivedEntityBuilder.Metadata, t2.DeclaringType));
    }

    [ConditionalFact]
    public void Can_only_remove_lower_or_equal_source_relationship()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[]
            {
                dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
            },
            ConfigurationSource.DataAnnotation);
        Assert.NotNull(relationshipBuilder);

        Assert.Null(dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.Convention));
        Assert.NotNull(dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            dependentEntityBuilder.Metadata.GetProperties().Select(p => p.Name));
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Removing_a_relationship_from_the_wrong_entity_type_throws()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.DataAnnotation);
        Assert.NotNull(relationshipBuilder);

        Assert.Equal(
            CoreStrings.ForeignKeyWrongType(
                "{'" + Order.CustomerIdProperty.Name + "'}", "{'TempId'}", nameof(Customer), nameof(Customer), nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                    () =>
                        Assert.Null(
                            principalEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation)))
                .Message);
    }

    [ConditionalFact]
    public void Removing_relationship_removes_unused_contained_shadow_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var shadowProperty = dependentEntityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[]
            {
                dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                shadowProperty.Metadata
            },
            ConfigurationSource.Convention);
        Assert.NotNull(relationshipBuilder);

        Assert.NotNull(dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Same(Order.CustomerIdProperty.Name, dependentEntityBuilder.Metadata.GetProperties().Single().Name);
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Removing_relationship_removes_unused_conventional_index()
    {
        var modelBuilder = CreateConventionalModelBuilder();
        modelBuilder.Ignore(typeof(SpecialOrder), ConfigurationSource.Explicit);
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var derivedPrincipalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.DataAnnotation);
        Assert.NotNull(relationshipBuilder);

        var relationshipBuilder2 = dependentEntityBuilder.HasRelationship(
            derivedPrincipalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.DataAnnotation);
        Assert.NotNull(relationshipBuilder2);
        Assert.NotSame(relationshipBuilder, relationshipBuilder2);
        Assert.Single(dependentEntityBuilder.Metadata.GetIndexes());

        Assert.NotNull(
            dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Single(dependentEntityBuilder.Metadata.GetIndexes());
        Assert.Single(dependentEntityBuilder.Metadata.GetForeignKeys());

        Assert.NotNull(
            dependentEntityBuilder.HasNoRelationship(relationshipBuilder2.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Empty(dependentEntityBuilder.Metadata.GetIndexes());
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Removing_relationship_does_not_remove_conventional_index_if_in_use()
    {
        var modelBuilder = CreateConventionalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.Convention);
        Assert.NotNull(relationshipBuilder);
        dependentEntityBuilder.HasIndex(new[] { Order.CustomerIdProperty }, ConfigurationSource.Explicit);

        Assert.NotNull(dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Single(dependentEntityBuilder.Metadata.GetIndexes());
        Assert.Equal(Order.CustomerIdProperty.Name, dependentEntityBuilder.Metadata.GetIndexes().First().Properties.First().Name);
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
    {
        Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.PrimaryKey(new[] { property.Name }, ConfigurationSource.Convention));

        Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.HasIndex(new[] { property.Name }, ConfigurationSource.Convention));

        Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.HasRelationship(
                typeof(Customer).FullName,
                new[] { entityBuilder.Property(typeof(int), "Shadow2", ConfigurationSource.Convention).Metadata.Name, property.Name },
                ConfigurationSource.Convention));

        Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Explicit));
    }

    private void Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
        Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var shadowProperty = dependentEntityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);
        Assert.NotNull(shadowConfig(dependentEntityBuilder, shadowProperty.Metadata));

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[]
            {
                dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                shadowProperty.Metadata
            },
            ConfigurationSource.Convention);
        Assert.NotNull(relationshipBuilder);

        Assert.NotNull(dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(1, dependentEntityBuilder.Metadata.GetProperties().Count(p => p.Name == shadowProperty.Metadata.Name));
        Assert.Empty(
            dependentEntityBuilder.Metadata.GetForeignKeys().Where(
                foreignKey => foreignKey.Properties.SequenceEqual(relationshipBuilder.Metadata.Properties)));
    }

    [ConditionalFact]
    public void Index_returns_same_instance_for_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit);

        Assert.NotNull(indexBuilder);
        Assert.Same(
            indexBuilder,
            entityBuilder.HasIndex(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Index_returns_same_instance_for_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var indexBuilder = entityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder);
        Assert.Same(
            indexBuilder, entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));
    }

    [ConditionalFact]
    public void Index_returns_same_instance_if_asked_for_twice()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var indexBuilder = entityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder);
        Assert.Same(
            indexBuilder,
            entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));
    }

    [ConditionalFact]
    public void Named_index_returns_same_instance_for_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var indexBuilder = entityBuilder.HasIndex(
            new[] { Order.IdProperty, Order.CustomerIdProperty }, "TestIndex", ConfigurationSource.Explicit);

        Assert.NotNull(indexBuilder);
        Assert.Same(
            indexBuilder,
            entityBuilder.HasIndex(
                new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, "TestIndex", ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Named_index_returns_same_instance_for_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var indexBuilder = entityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, "TestIndex", ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder);
        Assert.Same(
            indexBuilder,
            entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, "TestIndex", ConfigurationSource.Explicit));
    }

    [ConditionalFact]
    public void Named_index_returns_same_instance_if_asked_for_twice()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var indexBuilder = entityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, "TestIndex", ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder);
        Assert.Same(
            indexBuilder,
            entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, "TestIndex", ConfigurationSource.Explicit));
    }

    [ConditionalFact]
    public void Named_index_throws_if_try_to_create_a_new_different_index_with_same_name_on_same_type()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var indexBuilder = entityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, "NamedIndex", ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder);
        Assert.Equal(
            CoreStrings.DuplicateNamedIndex("NamedIndex", "{'CustomerId', 'Id'}", typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () => entityBuilder
                    .HasIndex(
                        new[] { Order.CustomerIdProperty, Order.IdProperty }, "NamedIndex", ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Named_index_throws_if_try_to_create_a_new_different_index_with_same_name_on_derived_type()
    {
        var modelBuilder = CreateModelBuilder();
        var baseEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention);

        var indexBuilder = baseEntityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, "NamedIndex", ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder);
        Assert.Equal(
            CoreStrings.DuplicateNamedIndex("NamedIndex", "{'CustomerId', 'Id'}", typeof(SpecialOrder).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () => derivedEntityBuilder.HasIndex(
                    new[] { Order.CustomerIdProperty, Order.IdProperty }, "NamedIndex", ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Named_index_throws_if_try_to_create_a_new_different_index_with_same_name_on_base_type()
    {
        var modelBuilder = CreateModelBuilder();
        var baseEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention);

        var indexBuilder = derivedEntityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, "NamedIndex", ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder);
        Assert.Equal(
            CoreStrings.DuplicateNamedIndex("NamedIndex", "{'CustomerId', 'Id'}", typeof(Order).Name, typeof(SpecialOrder).Name),
            Assert.Throws<InvalidOperationException>(
                () => baseEntityBuilder.HasIndex(
                    new[] { Order.CustomerIdProperty, Order.IdProperty }, "NamedIndex", ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Can_promote_index_to_base()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation);

        var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention);
        Assert.Same(indexBuilder.Metadata.Properties.Single(), entityBuilder.Metadata.FindProperty(Order.IdProperty.Name));
        Assert.Same(indexBuilder.Metadata, entityBuilder.Metadata.FindIndex(indexBuilder.Metadata.Properties.Single()));
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredIndexes());
    }

    [ConditionalFact]
    public void Can_promote_named_index_to_base()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, "IndexToPromote", ConfigurationSource.DataAnnotation);

        var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty.Name }, "IndexToPromote", ConfigurationSource.Convention);
        Assert.Same(indexBuilder.Metadata.Properties.Single(), entityBuilder.Metadata.FindProperty(Order.IdProperty.Name));
        Assert.Same(indexBuilder.Metadata, entityBuilder.Metadata.FindIndex(indexBuilder.Metadata.Name));
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredIndexes());
    }

    [ConditionalFact]
    public void Can_promote_index_to_base_with_facets()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation)
            .IsUnique(true, ConfigurationSource.Convention);

        var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention);
        Assert.Same(indexBuilder.Metadata.Properties.Single(), entityBuilder.Metadata.FindProperty(Order.IdProperty.Name));
        Assert.Same(indexBuilder.Metadata, entityBuilder.Metadata.FindIndex(indexBuilder.Metadata.Properties.Single()));
        Assert.True(indexBuilder.Metadata.IsUnique);
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredIndexes());
    }

    [ConditionalFact]
    public void Can_promote_named_index_to_base_with_facets()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, "IndexToPromote", ConfigurationSource.DataAnnotation)
            .IsUnique(true, ConfigurationSource.Convention);

        var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty.Name }, "IndexToPromote", ConfigurationSource.Convention);
        Assert.Same(indexBuilder.Metadata.Properties.Single(), entityBuilder.Metadata.FindProperty(Order.IdProperty.Name));
        Assert.Same(indexBuilder.Metadata, entityBuilder.Metadata.FindIndex(indexBuilder.Metadata.Name));
        Assert.True(indexBuilder.Metadata.IsUnique);
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredIndexes());
    }

    [ConditionalFact]
    public void Can_configure_inherited_index()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(typeof(int), Order.IdProperty.Name, ConfigurationSource.Convention);
        entityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.Explicit);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

        var indexBuilder = derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder.Metadata.GetIndexes().Single(), indexBuilder.Metadata);
        Assert.NotNull(indexBuilder.IsUnique(true, ConfigurationSource.Convention));
        Assert.True(indexBuilder.Metadata.IsUnique);
    }

    [ConditionalFact]
    public void Can_create_index_on_inherited_property()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(typeof(int), Order.IdProperty.Name, ConfigurationSource.Convention);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

        var indexBuilder = derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation);

        Assert.Empty(entityBuilder.Metadata.GetIndexes());
        Assert.Same(derivedEntityBuilder.Metadata.GetDeclaredIndexes().Single(), indexBuilder.Metadata);
        Assert.Same(entityBuilder.Metadata, indexBuilder.Metadata.Properties.First().DeclaringType);
    }

    [ConditionalFact]
    public void Can_create_index_on_mix_of_inherited_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(typeof(int), Order.IdProperty.Name, ConfigurationSource.Convention);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.Property(typeof(string), SpecialOrder.SpecialtyProperty.Name, ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

        var indexBuilder = derivedEntityBuilder.HasIndex(
            new[] { Order.IdProperty.Name, SpecialOrder.SpecialtyProperty.Name }, ConfigurationSource.DataAnnotation);

        Assert.Empty(entityBuilder.Metadata.GetIndexes());
        Assert.Same(derivedEntityBuilder.Metadata.GetDeclaredIndexes().Single(), indexBuilder.Metadata);
        Assert.Collection(
            indexBuilder.Metadata.Properties,
            t1 => Assert.Same(entityBuilder.Metadata, t1.DeclaringType),
            t2 => Assert.Same(derivedEntityBuilder.Metadata, t2.DeclaringType));
    }

    [ConditionalFact]
    public void Index_returns_null_for_ignored_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

        Assert.Null(entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Index_returns_null_for_ignored_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

        Assert.Null(
            entityBuilder.HasIndex(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Can_only_remove_lower_or_equal_source_index()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Explicit)
            .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var index = entityBuilder.HasIndex(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
        Assert.NotNull(index);

        Assert.Null(entityBuilder.HasNoIndex(index.Metadata, ConfigurationSource.Convention));
        Assert.NotNull(entityBuilder.HasNoIndex(index.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
        Assert.Empty(entityBuilder.Metadata.GetIndexes());
    }

    [ConditionalFact]
    public void Removing_index_removes_unused_contained_shadow_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);

        var index = entityBuilder.HasIndex(
            new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
        Assert.NotNull(index);

        Assert.NotNull(entityBuilder.HasNoIndex(index.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
        Assert.Empty(entityBuilder.Metadata.GetIndexes());
    }

    [ConditionalFact]
    public void Removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
    {
        Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.PrimaryKey(new[] { property.Name }, ConfigurationSource.Convention));

        Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.HasIndex(new[] { property.Name }, ConfigurationSource.Convention));

        Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.HasRelationship(
                typeof(Customer).FullName,
                new[] { Order.CustomerIdProperty.Name, property.Name },
                ConfigurationSource.Convention));

        Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.Property(
                property.ClrType, ((IReadOnlyProperty)property).Name, ConfigurationSource.Explicit));
    }

    private void Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
        Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Explicit)
            .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);
        Assert.NotNull(shadowConfig(entityBuilder, shadowProperty.Metadata));

        var index = entityBuilder.HasIndex(
            new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
        Assert.NotNull(index);

        Assert.NotNull(entityBuilder.HasNoIndex(index.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(1, entityBuilder.Metadata.GetProperties().Count(p => p.Name == shadowProperty.Metadata.Name));
        Assert.Empty(entityBuilder.Metadata.GetIndexes().Where(i => i.Properties.SequenceEqual(index.Metadata.Properties)));
    }

    [ConditionalFact]
    public void Key_returns_same_instance_for_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.HasKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);

        Assert.NotNull(keyBuilder);
        Assert.Same(
            keyBuilder,
            entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Key_returns_same_instance_for_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.PrimaryKey(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

        Assert.NotNull(keyBuilder);
        Assert.Same(
            keyBuilder, entityBuilder.HasKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Key_sets_properties_to_required()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        entityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention)
            .IsRequired(false, ConfigurationSource.DataAnnotation);

        Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.Convention));

        Assert.NotNull(
            entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));

        Assert.False(((IReadOnlyEntityType)entityBuilder.Metadata).FindProperty(Order.CustomerUniqueProperty).IsNullable);

        Assert.Null(
            entityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention)
                .IsRequired(false, ConfigurationSource.Convention));
        Assert.NotNull(
            entityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention)
                .IsRequired(false, ConfigurationSource.DataAnnotation));

        Assert.True(((IReadOnlyEntityType)entityBuilder.Metadata).FindProperty(Order.CustomerUniqueProperty).IsNullable);
        Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
    }

    [ConditionalFact]
    public void Key_throws_for_property_names_if_they_do_not_exist()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NoPropertyType(Customer.UniqueProperty.Name, nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    entityBuilder.HasKey(new[] { Customer.UniqueProperty.Name }, ConfigurationSource.Convention)).Message);
    }

    [ConditionalFact]
    public void Key_throws_for_derived_type()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Equal(
            CoreStrings.DerivedEntityTypeKey(typeof(SpecialOrder).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () =>
                    derivedEntityBuilder.HasKey(
                        new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation)).Message);
    }

    [ConditionalFact]
    public void Key_throws_for_derived_type_before_HasBase()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasKey(new[] { nameof(SpecialOrder.Specialty) }, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.DerivedEntityCannotHaveKeys(typeof(SpecialOrder).Name),
            Assert.Throws<InvalidOperationException>(
                () => derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Key_throws_for_property_names_for_shared_entity_type_if_they_do_not_exist()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order).Name, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NoPropertyType(Order.IdProperty.Name, nameof(Order) + " (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(
                () => entityBuilder.HasKey(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention)).Message);
    }

    [ConditionalFact]
    public void Key_works_for_property_names_for_shadow_entity_type()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(Order.CustomerIdProperty.PropertyType, Order.CustomerIdProperty.Name, ConfigurationSource.Convention);

        Assert.NotNull(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

        Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetKeys().Single().Properties.Single().Name);
    }

    [ConditionalFact]
    public void Key_returns_null_for_ignored_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

        Assert.Null(entityBuilder.HasKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Key_returns_null_for_ignored_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

        Assert.Null(
            entityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Can_only_remove_lower_or_equal_source_key()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var key = entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
        Assert.NotNull(key);

        Assert.Null(entityBuilder.HasNoKey(key.Metadata, ConfigurationSource.Convention));
        Assert.NotNull(entityBuilder.HasNoKey(key.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
        Assert.Empty(entityBuilder.Metadata.GetKeys());
    }

    [ConditionalFact]
    public void Removing_key_removes_unused_contained_shadow_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);

        var key = entityBuilder.HasKey(
            new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
        Assert.NotNull(key);

        Assert.NotNull(entityBuilder.HasNoKey(key.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
        Assert.Empty(entityBuilder.Metadata.GetKeys());
    }

    [ConditionalFact]
    public void Removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
    {
        Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.PrimaryKey(
                new[] { entityBuilder.Property(typeof(int), "Shadow2", ConfigurationSource.Convention).Metadata.Name, property.Name },
                ConfigurationSource.Convention));

        Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.HasIndex(new[] { property.Name }, ConfigurationSource.Convention));

        Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.HasRelationship(
                typeof(Customer).FullName,
                new[] { Order.CustomerIdProperty.Name, property.Name },
                ConfigurationSource.Convention));

        Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
            (entityBuilder, property) => entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Explicit));
    }

    private void Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
        Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Explicit)
            .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);

        var key = entityBuilder.HasKey(
            new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
        Assert.NotNull(key);

        Assert.NotNull(shadowConfig(entityBuilder, shadowProperty.Metadata));
        Assert.NotNull(entityBuilder.HasNoKey(key.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Equal(1, entityBuilder.Metadata.GetProperties().Count(p => p.Name == shadowProperty.Metadata.Name));
        Assert.Empty(
            entityBuilder.Metadata.GetKeys().Where(foreignKey => foreignKey.Properties.SequenceEqual(key.Metadata.Properties)));
    }

    [ConditionalFact]
    public void HasNoKey_can_override_lower_or_equal_source_key()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var entityType = entityBuilder.Metadata;

        Assert.False(entityType.IsKeyless);

        Assert.NotNull(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        Assert.False(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.Convention, entityType.GetIsKeylessConfigurationSource());

        Assert.NotNull(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation));
        Assert.False(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.DataAnnotation, entityType.GetIsKeylessConfigurationSource());

        Assert.NotNull(entityBuilder.HasNoKey(ConfigurationSource.DataAnnotation));
        Assert.True(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.DataAnnotation, entityType.GetIsKeylessConfigurationSource());
        Assert.Empty(entityType.GetKeys());

        Assert.Null(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        Assert.Empty(entityType.GetKeys());

        Assert.NotNull(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit));
        Assert.NotEmpty(entityType.GetKeys());

        Assert.Null(entityBuilder.HasNoKey(ConfigurationSource.DataAnnotation));
        Assert.False(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.Explicit, entityType.GetIsKeylessConfigurationSource());
        Assert.NotEmpty(entityType.GetKeys());

        Assert.Equal(
            CoreStrings.KeylessTypeExistingKey(nameof(Order), "{'CustomerId'}"),
            Assert.Throws<InvalidOperationException>(
                () => entityBuilder.HasNoKey(ConfigurationSource.Explicit)).Message);
        Assert.NotEmpty(entityType.GetKeys());
    }

    [ConditionalFact]
    public void HasKey_can_override_lower_or_equal_source_HasNoKey()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var entityType = entityBuilder.Metadata;

        Assert.NotNull(entityBuilder.HasNoKey(ConfigurationSource.Convention));
        Assert.True(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.Convention, entityType.GetIsKeylessConfigurationSource());

        Assert.NotNull(entityBuilder.HasNoKey(ConfigurationSource.DataAnnotation));
        Assert.True(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.DataAnnotation, entityType.GetIsKeylessConfigurationSource());

        Assert.NotNull(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation));
        Assert.False(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.DataAnnotation, entityType.GetIsKeylessConfigurationSource());
        Assert.NotEmpty(entityType.GetKeys());

        var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);
        Assert.NotNull(entityBuilder.HasKey(new[] { shadowProperty.Metadata.Name }, ConfigurationSource.Convention));

        Assert.Null(entityBuilder.HasNoKey(ConfigurationSource.Convention));
        Assert.Equal(2, entityType.GetKeys().Count());

        Assert.NotNull(entityBuilder.HasNoKey(ConfigurationSource.Explicit));
        Assert.Empty(entityType.GetKeys());

        Assert.Null(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation));
        Assert.True(entityType.IsKeyless);
        Assert.Equal(ConfigurationSource.Explicit, entityType.GetIsKeylessConfigurationSource());
        Assert.Empty(entityType.GetKeys());

        Assert.NotNull(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit));
    }

    [ConditionalFact]
    public void PrimaryKey_returns_same_instance_for_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.PrimaryKey(
            new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);

        Assert.NotNull(keyBuilder);
        Assert.Same(
            keyBuilder,
            entityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

        Assert.Same(entityBuilder.Metadata.FindPrimaryKey(), entityBuilder.Metadata.GetKeys().Single());
    }

    [ConditionalFact]
    public void PrimaryKey_returns_same_instance_for_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.HasKey(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

        Assert.NotNull(keyBuilder);
        Assert.Same(
            keyBuilder,
            entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

        Assert.Same(entityBuilder.Metadata.FindPrimaryKey(), entityBuilder.Metadata.GetKeys().Single());
    }

    [ConditionalFact]
    public void PrimaryKey_throws_for_property_names_if_they_do_not_exist()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NoPropertyType(Customer.UniqueProperty.Name, nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () => entityBuilder.PrimaryKey(new[] { Customer.UniqueProperty.Name }, ConfigurationSource.Convention)).Message);
    }

    [ConditionalFact]
    public void PrimaryKey_throws_for_property_names_for_shared_entity_type_if_they_do_not_exist()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order).Name, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NoPropertyType(Order.IdProperty.Name, nameof(Order) + " (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(
                () => entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention)).Message);
    }

    [ConditionalFact]
    public void PrimaryKey_throws_for_derived_type()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Equal(
            CoreStrings.DerivedEntityTypeKey(typeof(SpecialOrder).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () =>
                    derivedEntityBuilder.PrimaryKey(
                        new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation)).Message);
    }

    [ConditionalFact]
    public void PrimaryKey_works_for_property_names_for_shadow_entity_type()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(Order.CustomerIdProperty.PropertyType, Order.CustomerIdProperty.Name, ConfigurationSource.Convention);

        Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

        Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.FindPrimaryKey().Properties.Single().Name);
    }

    [ConditionalFact]
    public void PrimaryKey_returns_null_for_ignored_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

        Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void PrimaryKey_returns_null_for_ignored_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

        Assert.Null(
            entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_primary_key()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var entityType = entityBuilder.Metadata;

        var compositeKeyBuilder = entityBuilder.PrimaryKey(
            new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Convention);
        var simpleKeyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Convention);

        Assert.NotNull(simpleKeyBuilder);
        Assert.NotEqual(compositeKeyBuilder, simpleKeyBuilder);
        Assert.Equal(Order.IdProperty.Name, entityType.GetKeys().Single().Properties.Single().Name);

        var simpleKeyBuilder2 = entityBuilder.HasKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);
        Assert.Same(simpleKeyBuilder, simpleKeyBuilder2);

        var compositeKeyBuilder2 = entityBuilder.PrimaryKey(
            new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Convention);
        Assert.NotNull(compositeKeyBuilder2);
        Assert.NotEqual(compositeKeyBuilder, compositeKeyBuilder2);
        Assert.Same(compositeKeyBuilder2.Metadata, entityBuilder.Metadata.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());

        Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

        Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));
        Assert.Same(compositeKeyBuilder2.Metadata, entityBuilder.Metadata.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());
    }

    [ConditionalFact]
    public void Can_only_override_existing_primary_key_explicitly()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var entityType = entityBuilder.Metadata;

        Assert.Null(entityType.GetPrimaryKeyConfigurationSource());

        entityType.SetPrimaryKey(
            new[]
            {
                entityType.AddProperty(Order.IdProperty, ConfigurationSource.Explicit),
                entityType.AddProperty(Order.CustomerIdProperty, ConfigurationSource.Explicit)
            },
            ConfigurationSource.Explicit);

        Assert.Equal(ConfigurationSource.Explicit, entityType.GetPrimaryKeyConfigurationSource());

        Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));

        Assert.Equal(
            new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name },
            entityType.FindPrimaryKey().Properties.Select(p => p.Name).ToArray());

        Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit));

        Assert.Equal(Order.IdProperty.Name, entityType.FindPrimaryKey().Properties.Single().Name);
    }

    [ConditionalFact]
    public void Changing_primary_key_removes_previously_referenced_primary_key_of_lower_or_equal_source()
    {
        var modelBuilder = CreateModelBuilder();
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var keyBuilder = principalEntityBuilder.PrimaryKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
        var fkProperty1 = dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;
        var fkProperty2 = dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata;

        dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.DataAnnotation)
            .HasPrincipalKey(keyBuilder.Metadata.Properties, ConfigurationSource.DataAnnotation)
            .HasForeignKey(new[] { fkProperty1, fkProperty2 }, ConfigurationSource.Explicit);

        keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

        Assert.Same(keyBuilder.Metadata, principalEntityBuilder.Metadata.FindPrimaryKey());
        var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
        Assert.Equal(new[] { fkProperty1, fkProperty2 }, fk.Properties);
    }

    [ConditionalFact]
    public void Changing_primary_key_removes_previously_referenced_key_of_lower_or_equal_source()
    {
        var modelBuilder = CreateModelBuilder();
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.DataAnnotation)
            .HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation);

        Assert.Null(principalEntityBuilder.Metadata.FindPrimaryKey());
        Assert.NotEqual(
            nameof(Customer.Id), dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey.Properties.First().Name);

        var keyBuilder = principalEntityBuilder.PrimaryKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);

        Assert.Same(keyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey);
        Assert.Same(keyBuilder.Metadata, principalEntityBuilder.Metadata.GetKeys().Single());
    }

    [ConditionalFact]
    public void Changing_primary_key_does_not_remove_previously_explicitly_referenced_key()
    {
        var modelBuilder = CreateModelBuilder();
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var existingKeyBuilder = principalEntityBuilder.HasKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
        dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Explicit)
            .HasPrincipalKey(existingKeyBuilder.Metadata.Properties, ConfigurationSource.Explicit);

        var keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);

        Assert.Same(existingKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey);
        Assert.Equal(2, principalEntityBuilder.Metadata.GetKeys().Count());
        Assert.Contains(keyBuilder.Metadata, principalEntityBuilder.Metadata.GetKeys());
    }

    [ConditionalFact]
    public void Property_returns_same_instance_for_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var propertyBuilder = entityBuilder.Property(Order.IdProperty, ConfigurationSource.Explicit);

        Assert.NotNull(propertyBuilder);
        Assert.Same(propertyBuilder, entityBuilder.Property(typeof(int), Order.IdProperty.Name, ConfigurationSource.Explicit));
    }

    [ConditionalFact]
    public void Property_returns_same_instance_for_property_names()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var propertyBuilder = entityBuilder.Property(Order.IdProperty.Name, ConfigurationSource.DataAnnotation);

        Assert.NotNull(propertyBuilder);
        Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Property_returns_same_instance_if_type_matches()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var propertyBuilder = entityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation);
        Assert.NotNull(propertyBuilder);

        Assert.Same(
            propertyBuilder,
            entityBuilder.Property(
                typeof(int), Order.IdProperty.Name, typeConfigurationSource: ConfigurationSource.DataAnnotation,
                configurationSource: ConfigurationSource.DataAnnotation));

        Assert.Same(
            propertyBuilder,
            entityBuilder.Property(
                typeof(int), Order.IdProperty.Name, typeConfigurationSource: null,
                configurationSource: ConfigurationSource.Convention));

        Assert.Same(
            propertyBuilder,
            entityBuilder.Property(
                Order.IdProperty.Name, ConfigurationSource.Convention));

        Assert.Null(
            entityBuilder.Property(
                typeof(string), Order.IdProperty.Name, typeConfigurationSource: ConfigurationSource.Convention,
                configurationSource: ConfigurationSource.Convention));

        Assert.Equal(new[] { propertyBuilder.Metadata }, entityBuilder.GetActualProperties(new[] { propertyBuilder.Metadata }, null));
    }

    [ConditionalFact]
    public void Can_add_indexed_property()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(IndexedClass), ConfigurationSource.Explicit);

        var propertyBuilder = entityBuilder.IndexerProperty(
            typeof(string), IndexedClass.IndexerPropertyName, ConfigurationSource.DataAnnotation);

        Assert.NotNull(propertyBuilder);
    }

    [ConditionalFact]
    public void Property_returns_same_instance_for_existing_index_property()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(IndexedClass), ConfigurationSource.Explicit);

        var propertyBuilder = entityBuilder.IndexerProperty(
            typeof(string), IndexedClass.IndexerPropertyName, ConfigurationSource.DataAnnotation);

        Assert.NotNull(propertyBuilder);
        Assert.Same(
            propertyBuilder,
            entityBuilder.Property(typeof(string), IndexedClass.IndexerPropertyName, ConfigurationSource.Convention));

        Assert.Same(
            propertyBuilder,
            entityBuilder.Property(IndexedClass.IndexerPropertyName, ConfigurationSource.Convention));

        Assert.Null(entityBuilder.Property(typeof(int), IndexedClass.IndexerPropertyName, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Property_removes_existing_index_property_for_higher_source_if_type_mismatch()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(IndexedClass), ConfigurationSource.Explicit);

        var propertyBuilder = entityBuilder.IndexerProperty(
            typeof(string), IndexedClass.IndexerPropertyName, ConfigurationSource.Convention);

        Assert.NotNull(propertyBuilder);
        Assert.True(propertyBuilder.Metadata.IsIndexerProperty());
        Assert.False(propertyBuilder.Metadata.IsShadowProperty());

        var replacedPropertyBuilder = entityBuilder.Property(
            typeof(int), IndexedClass.IndexerPropertyName, ConfigurationSource.DataAnnotation);

        Assert.NotNull(replacedPropertyBuilder);
        Assert.NotSame(propertyBuilder, replacedPropertyBuilder);
        Assert.True(replacedPropertyBuilder.Metadata.IsIndexerProperty());
        Assert.False(replacedPropertyBuilder.Metadata.IsShadowProperty());
    }

    [ConditionalFact]
    public void Indexer_property_removes_existing_shadow_property_for_higher_source()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(IndexedClass), ConfigurationSource.Explicit);

        var shadowPropertyBuilder = entityBuilder.Property(
            typeof(int), IndexedClass.IndexerPropertyName, ConfigurationSource.Convention);

        Assert.NotNull(shadowPropertyBuilder);
        Assert.True(shadowPropertyBuilder.Metadata.IsShadowProperty());

        var replacedPropertyBuilder = entityBuilder.IndexerProperty(
            typeof(string), IndexedClass.IndexerPropertyName, ConfigurationSource.DataAnnotation);

        Assert.NotNull(replacedPropertyBuilder);
        Assert.NotSame(shadowPropertyBuilder, replacedPropertyBuilder);
        Assert.True(replacedPropertyBuilder.Metadata.IsIndexerProperty());
        Assert.False(replacedPropertyBuilder.Metadata.IsShadowProperty());
    }

    [ConditionalFact]
    public void Indexer_property_throws_when_entityType_is_not_indexer()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NonIndexerEntityType(IndexedClass.IndexerPropertyName, nameof(Order), typeof(string).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => entityBuilder.IndexerProperty(
                    typeof(string), IndexedClass.IndexerPropertyName, ConfigurationSource.Convention)).Message);
    }

    [ConditionalFact]
    public void Property_throws_for_navigation()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Explicit).HasNavigation(
            Order.CustomerProperty.Name,
            pointsToPrincipal: true,
            ConfigurationSource.Explicit).HasNavigation(
            Customer.OrdersProperty.Name,
            pointsToPrincipal: false,
            ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(
                nameof(Order.Customer), nameof(Order), nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () => dependentEntityBuilder
                    .Property(Order.CustomerProperty, ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Cannot_add_shadow_property_without_type()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NoPropertyType("Shadow", nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => entityBuilder.Property("Shadow", ConfigurationSource.DataAnnotation))
                .Message);
    }

    [ConditionalFact]
    public void Can_promote_property_to_base()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
        var derivedProperty = derivedEntityBuilder.Property(typeof(int), "byte", ConfigurationSource.DataAnnotation);
        derivedProperty.IsConcurrencyToken(true, ConfigurationSource.Convention);
        derivedProperty.HasMaxLength(1, ConfigurationSource.Explicit);
        var derivedEntityBuilder2 = modelBuilder.Entity(typeof(BackOrder), ConfigurationSource.Convention);
        derivedEntityBuilder2.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
        var derivedProperty2 = derivedEntityBuilder2.Property(typeof(byte), "byte", ConfigurationSource.Convention);
        derivedProperty2.HasMaxLength(2, ConfigurationSource.Convention);

        var propertyBuilder = entityBuilder.Property(typeof(int), "byte", ConfigurationSource.Convention);
        Assert.Same(propertyBuilder.Metadata, entityBuilder.Metadata.FindProperty("byte"));
        Assert.Null(entityBuilder.Ignore("byte", ConfigurationSource.Convention));
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredProperties());
        Assert.Empty(derivedEntityBuilder2.Metadata.GetDeclaredProperties());
        Assert.Equal(typeof(int), propertyBuilder.Metadata.ClrType);
        Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        Assert.Equal(1, propertyBuilder.Metadata.GetMaxLength());
    }

    [ConditionalFact]
    public void Can_configure_inherited_property()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(typeof(int), nameof(SpecialOrder.Specialty), ConfigurationSource.Explicit)
            .IsConcurrencyToken(false, ConfigurationSource.Convention);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

        var propertyBuilder = derivedEntityBuilder.Property(
            typeof(int), nameof(SpecialOrder.Specialty), ConfigurationSource.DataAnnotation);

        Assert.Same(entityBuilder.Metadata.FindProperty(nameof(SpecialOrder.Specialty)), propertyBuilder.Metadata);
        Assert.NotNull(propertyBuilder.IsConcurrencyToken(true, ConfigurationSource.Convention));
        Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        Assert.Same(typeof(int), propertyBuilder.Metadata.ClrType);

        Assert.Null(derivedEntityBuilder.Property(typeof(string), nameof(SpecialOrder.Specialty), ConfigurationSource.DataAnnotation));

        Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
        Assert.NotNull(entityBuilder.PrimaryKey(new[] { propertyBuilder.Metadata.Name }, ConfigurationSource.Explicit));
        propertyBuilder = derivedEntityBuilder.Property(typeof(string), nameof(SpecialOrder.Specialty), ConfigurationSource.Explicit);

        Assert.Same(typeof(string), propertyBuilder.Metadata.ClrType);
        Assert.Same(entityBuilder.Metadata, propertyBuilder.Metadata.DeclaringType);
        Assert.NotNull(entityBuilder.Metadata.FindPrimaryKey());
    }

    [ConditionalFact]
    public void Can_reuniquify_temporary_properties_with_same_names()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(nameof(Customer), ConfigurationSource.Explicit);
        var principalKey = principalEntityBuilder.HasKey(
            new[]
            {
                principalEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit).Metadata,
                principalEntityBuilder.Property(typeof(int), "AlternateId", ConfigurationSource.Explicit).Metadata
            }, ConfigurationSource.Explicit).Metadata;
        var dependentEntityBuilder = modelBuilder.Entity(nameof(Order), ConfigurationSource.Explicit);
        var foreignKey = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[]
            {
                dependentEntityBuilder.Property(typeof(int), "AlternateId", ConfigurationSource.Convention).Metadata,
                dependentEntityBuilder.Property(typeof(int), "AlternateId1", ConfigurationSource.Convention).Metadata
            },
            principalKey, ConfigurationSource.Convention).Metadata;

        Assert.True(dependentEntityBuilder.ShouldReuniquifyTemporaryProperties(foreignKey));

        var newFkProperties = foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention)
            .Metadata.Properties;

        Assert.Equal("CustomerId", newFkProperties[0].Name);
        Assert.Equal("CustomerAlternateId", newFkProperties[1].Name);
        Assert.Equal(2, dependentEntityBuilder.Metadata.GetProperties().Count());
    }

    [ConditionalFact]
    public void Can_reuniquify_temporary_properties_with_same_names_different_types_in_two_passes()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(nameof(Customer), ConfigurationSource.Explicit);
        var principalKey = principalEntityBuilder.HasKey(
            new[]
            {
                principalEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit).Metadata,
                principalEntityBuilder.Property(typeof(Guid), "AlternateId", ConfigurationSource.Explicit).Metadata
            }, ConfigurationSource.Explicit).Metadata;
        var dependentEntityBuilder = modelBuilder.Entity(nameof(Order), ConfigurationSource.Explicit);
        var foreignKey = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[]
            {
                dependentEntityBuilder.Property(typeof(int), "AlternateId", ConfigurationSource.Convention).Metadata,
                dependentEntityBuilder.Property(typeof(Guid), "Id", ConfigurationSource.Convention).Metadata
            },
            principalKey, ConfigurationSource.Convention).Metadata;

        Assert.True(dependentEntityBuilder.ShouldReuniquifyTemporaryProperties(foreignKey));

        var newFkProperties = foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention)
            .Metadata.Properties;

        Assert.Equal("CustomerId", newFkProperties[0].Name);
        Assert.Equal("CustomerAlternateId", newFkProperties[1].Name);
        Assert.Equal(2, dependentEntityBuilder.Metadata.GetProperties().Count());
    }

    [ConditionalFact]
    public void Can_reuniquify_temporary_properties_avoiding_unmapped_clr_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(nameof(Customer), ConfigurationSource.Explicit);
        var principalKey = principalEntityBuilder.HasKey(
            new[]
            {
                principalEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit).Metadata,
                principalEntityBuilder.Property(typeof(int), "Unique", ConfigurationSource.Explicit).Metadata
            }, ConfigurationSource.Explicit).Metadata;
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var foreignKey = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[]
            {
                dependentEntityBuilder.Property(typeof(int), "Id1", ConfigurationSource.Convention).Metadata,
                dependentEntityBuilder.Property(typeof(int), "Id2", ConfigurationSource.Convention).Metadata
            },
            principalKey, ConfigurationSource.Convention).Metadata;

        Assert.True(dependentEntityBuilder.ShouldReuniquifyTemporaryProperties(foreignKey));

        var newFkProperties = foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention)
            .Metadata.Properties;

        Assert.Equal("CustomerId1", newFkProperties[0].Name);
        Assert.Equal("CustomerUnique1", newFkProperties[1].Name);
        Assert.Equal(2, dependentEntityBuilder.Metadata.GetProperties().Count());
    }

    [ConditionalTheory]
    [MemberData(
        nameof(DataGenerator.GetCombinations),
        [new[] { typeof(ConfigurationSource), typeof(ConfigurationSource) }],
        MemberType = typeof(DataGenerator))]
    public void Can_ignore_property_in_hierarchy(ConfigurationSource ignoreSource, ConfigurationSource addSource)
    {
        VerifyIgnoreProperty(typeof(Order), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreProperty(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreProperty(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreProperty(typeof(Order), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);
        VerifyIgnoreProperty(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);
        VerifyIgnoreProperty(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);

        VerifyIgnoreProperty(typeof(Order), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreProperty(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreProperty(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreProperty(typeof(Order), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
        VerifyIgnoreProperty(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
        VerifyIgnoreProperty(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
    }

    private void VerifyIgnoreProperty(
        Type ignoredOnType,
        ConfigurationSource ignoreConfigurationSource,
        ConfigurationSource addConfigurationSource,
        bool ignoredFirst,
        bool setBaseFirst)
        => VerifyIgnoreMember(
            ignoredOnType, ignoreConfigurationSource, addConfigurationSource, ignoredFirst, setBaseFirst,
            et => et.Metadata.FindProperty(Order.CustomerIdProperty.Name) != null,
            et => et.Property(Order.CustomerIdProperty, addConfigurationSource) != null,
            et => et.Property(Order.CustomerIdProperty, ignoreConfigurationSource) != null,
            Order.CustomerIdProperty.Name);

    private void VerifyIgnoreMember(
        Type ignoredOnType,
        ConfigurationSource ignoreConfigurationSource,
        ConfigurationSource addConfigurationSource,
        bool ignoredFirst,
        bool setBaseFirst,
        Func<InternalEntityTypeBuilder, bool> findMember,
        Func<InternalEntityTypeBuilder, bool> addMember,
        Func<InternalEntityTypeBuilder, bool> unignoreMember,
        string memberToIgnore)
    {
        var modelBuilder = CreateModelBuilder();
        var customerTypeBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
        customerTypeBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
        var productTypeBuilder = modelBuilder.Entity(typeof(Product), ConfigurationSource.Convention);
        productTypeBuilder.PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

        if (setBaseFirst)
        {
            ConfigureOrdersHierarchy(modelBuilder);
        }

        var ignoredEntityTypeBuilder = modelBuilder.Entity(ignoredOnType, ConfigurationSource.Convention);
        var addedEntityTypeBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

        var exceptionExpected = ignoredOnType == typeof(ExtraSpecialOrder)
            && (ignoreConfigurationSource == ConfigurationSource.Explicit
                || (!ignoredFirst && setBaseFirst));

        var expectedAdded = ignoredOnType == typeof(ExtraSpecialOrder)
            || (addConfigurationSource.Overrides(ignoreConfigurationSource)
                && (ignoreConfigurationSource != addConfigurationSource
                    || (ignoreConfigurationSource == ConfigurationSource.Explicit
                        && (ignoredFirst || ignoredOnType != typeof(SpecialOrder)))));

        var expectedIgnored = (ignoredOnType != typeof(SpecialOrder)
                || !expectedAdded)
            && !exceptionExpected;

        if (ignoredFirst)
        {
            Assert.NotNull(ignoredEntityTypeBuilder.Ignore(memberToIgnore, ignoreConfigurationSource));
            Assert.Equal(
                expectedAdded || (!setBaseFirst && ignoredOnType != typeof(SpecialOrder)), addMember(addedEntityTypeBuilder));
        }
        else
        {
            Assert.True(addMember(addedEntityTypeBuilder));
            if (exceptionExpected
                && setBaseFirst)
            {
                Assert.Equal(
                    CoreStrings.InheritedPropertyCannotBeIgnored(
                        memberToIgnore, typeof(ExtraSpecialOrder).ShortDisplayName(),
                        typeof(SpecialOrder).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(
                        () => ignoredEntityTypeBuilder.Ignore(memberToIgnore, ignoreConfigurationSource)).Message);
                return;
            }

            Assert.Equal(
                expectedIgnored
                || (!setBaseFirst
                    && (ignoreConfigurationSource == ConfigurationSource.Explicit || ignoredOnType != typeof(SpecialOrder))),
                ignoredEntityTypeBuilder.Ignore(memberToIgnore, ignoreConfigurationSource) != null);
        }

        if (!setBaseFirst)
        {
            ConfigureOrdersHierarchy(modelBuilder);
        }

        var modelValidator = InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<IModelValidator>();

        if (exceptionExpected)
        {
            Assert.Equal(
                CoreStrings.InheritedPropertyCannotBeIgnored(
                    memberToIgnore,
                    typeof(ExtraSpecialOrder).ShortDisplayName(),
                    typeof(SpecialOrder).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => modelValidator.Validate(
                        modelBuilder.Metadata,
                        new TestLogger<DbLoggerCategory.Model.Validation, TestLoggingDefinitions>())).Message);

            Assert.True(unignoreMember(ignoredEntityTypeBuilder));
        }
        else
        {
            Assert.Equal(expectedAdded, findMember(addedEntityTypeBuilder));
            Assert.Equal(
                expectedIgnored,
                ignoredEntityTypeBuilder.Metadata.FindDeclaredIgnoredConfigurationSource(memberToIgnore)
                == ignoreConfigurationSource);
        }
    }

    private void ConfigureOrdersHierarchy(InternalModelBuilder modelBuilder)
    {
        modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit)
            .PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);
        var entityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);
        entityBuilder.HasBaseType(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(ExtraSpecialOrder), ConfigurationSource.Explicit);
        derivedEntityBuilder.HasBaseType(typeof(SpecialOrder), ConfigurationSource.Explicit);
    }

    [ConditionalFact]
    public void Can_ignore_service_property()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.ServiceProperty(Order.ContextProperty, ConfigurationSource.Explicit);

        Assert.Null(entityBuilder.Ignore(nameof(Order.Context), ConfigurationSource.DataAnnotation));

        Assert.NotNull(entityBuilder.Ignore(nameof(Order.Context), ConfigurationSource.Explicit));
        Assert.Empty(entityBuilder.Metadata.GetServiceProperties());
    }

    [ConditionalFact]
    public void Can_ignore_property_that_is_part_of_lower_source_foreign_key_preserving_the_relationship()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var key = principalEntityBuilder.PrimaryKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder =
            dependentEntityBuilder.HasRelationship(
                    principalEntityBuilder.Metadata, (string)null, null, ConfigurationSource.DataAnnotation)
                .HasForeignKey(
                    new[]
                    {
                        dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                        dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                    }, ConfigurationSource.DataAnnotation)
                .HasPrincipalKey(key.Metadata.Properties, ConfigurationSource.DataAnnotation)
                .IsUnique(true, ConfigurationSource.DataAnnotation)
                .IsRequired(true, ConfigurationSource.DataAnnotation)
                .OnDelete(DeleteBehavior.Cascade, ConfigurationSource.DataAnnotation);
        var fk = relationshipBuilder.Metadata;

        Assert.NotNull(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

        Assert.Empty(dependentEntityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
        var newFk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
        Assert.Same(fk.DeclaringEntityType, newFk.DeclaringEntityType);
        Assert.Same(fk.PrincipalEntityType, newFk.PrincipalEntityType);
        Assert.Null(newFk.PrincipalToDependent);
        Assert.Null(newFk.DependentToPrincipal);
        Assert.NotEqual(fk.Properties, newFk.DeclaringEntityType.GetProperties());
        Assert.Same(fk.PrincipalKey, newFk.PrincipalKey);
        Assert.True(newFk.IsUnique);
        Assert.True(newFk.IsRequired);
        Assert.Equal(DeleteBehavior.Cascade, newFk.DeleteBehavior);

        Assert.NotNull(
            dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, newFk.Properties, ConfigurationSource.Convention));

        Assert.NotNull(dependentEntityBuilder.Metadata.GetForeignKeys().Where(foreignKey => foreignKey != newFk));
    }

    [ConditionalFact]
    public void Cannot_ignore_property_that_is_part_of_higher_source_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[]
            {
                dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
            },
            ConfigurationSource.DataAnnotation);

        Assert.Null(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Convention));

        Assert.NotEmpty(dependentEntityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
        Assert.NotEmpty(dependentEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Can_ignore_property_that_is_part_of_lower_source_index()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.NotNull(
            entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

        Assert.NotNull(entityBuilder.Ignore(nameof(Order.CustomerId), ConfigurationSource.Explicit));

        Assert.Empty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == nameof(Order.CustomerId)));
        Assert.Empty(entityBuilder.Metadata.GetIndexes());
        Assert.Null(logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_property_that_was_explicitly_mapped()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit)!;

        Assert.NotNull(entityBuilder.Property(nameof(Order.CustomerId), ConfigurationSource.Explicit));
        Assert.NotNull(entityBuilder.Ignore(nameof(Order.CustomerId), ConfigurationSource.Explicit));

        Assert.Empty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == nameof(Order.CustomerId)));

        Assert.Equal(
            CoreResources.LogMappedPropertyIgnored(logger).GenerateMessage(nameof(Order), nameof(Order.CustomerId)),
            logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_navigation_that_is_part_of_lower_source_index()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit)!;

        entityBuilder.HasRelationship(
            entityBuilder.ModelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit)!.Metadata,
            Order.CustomerProperty,
            Customer.OrdersProperty,
            ConfigurationSource.DataAnnotation);

        Assert.NotNull(entityBuilder.Navigation(nameof(Order.Customer)));
        Assert.NotNull(entityBuilder.Ignore(nameof(Order.Customer), ConfigurationSource.Explicit));

        Assert.Empty(entityBuilder.Metadata.GetNavigations().Where(p => p.Name == nameof(Order.Customer)));

        Assert.Null(logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_navigation_that_was_explicitly_mapped()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit)!;

        entityBuilder.HasRelationship(
            entityBuilder.ModelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit)!.Metadata,
            Order.CustomerProperty,
            Customer.OrdersProperty,
            ConfigurationSource.Explicit);

        Assert.NotNull(entityBuilder.Navigation(nameof(Order.Customer)));
        Assert.NotNull(entityBuilder.Ignore(nameof(Order.Customer), ConfigurationSource.Explicit));

        Assert.Empty(entityBuilder.Metadata.GetNavigations().Where(p => p.Name == nameof(Order.Customer)));

        Assert.Equal(
            CoreResources.LogMappedNavigationIgnored(logger).GenerateMessage(nameof(Order), nameof(Order.Customer)),
            logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_skip_navigation_that_is_part_of_lower_source_index()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit)!;

        entityBuilder.HasSkipNavigation(
            MemberIdentity.Create(Order.ProductsProperty),
            entityBuilder.ModelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit)!.Metadata,
            ConfigurationSource.DataAnnotation);

        Assert.NotNull(entityBuilder.Navigation(nameof(Order.Products)));
        Assert.NotNull(entityBuilder.Ignore(nameof(Order.Products), ConfigurationSource.Explicit));

        Assert.Empty(entityBuilder.Metadata.GetSkipNavigations().Where(p => p.Name == nameof(Order.Products)));

        Assert.Null(logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_skip_navigation_that_was_explicitly_mapped()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit)!;

        entityBuilder.HasSkipNavigation(
            MemberIdentity.Create(Order.ProductsProperty),
            entityBuilder.ModelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit)!.Metadata,
            ConfigurationSource.Explicit);

        Assert.NotNull(entityBuilder.Navigation(nameof(Order.Products)));
        Assert.NotNull(entityBuilder.Ignore(nameof(Order.Products), ConfigurationSource.Explicit));

        Assert.Empty(entityBuilder.Metadata.GetSkipNavigations().Where(p => p.Name == nameof(Order.Products)));

        Assert.Equal(
            CoreResources.LogMappedNavigationIgnored(logger).GenerateMessage(nameof(Order), nameof(Order.Products)),
            logger.Message);
    }

    [ConditionalFact]
    public void Cannot_ignore_property_that_is_part_of_same_or_higher_source_index()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.NotNull(entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

        Assert.Null(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation));

        Assert.NotEmpty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
        Assert.NotEmpty(entityBuilder.Metadata.GetIndexes());
    }

    [ConditionalFact]
    public void Can_ignore_property_that_is_part_of_lower_source_key()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.NotNull(
            entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

        Assert.NotNull(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

        Assert.Empty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
        Assert.Empty(entityBuilder.Metadata.GetKeys());
    }

    [ConditionalFact]
    public void Can_ignore_property_that_is_part_of_lower_source_principal_key_preserving_the_relationship()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var key = principalEntityBuilder.PrimaryKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var principalType = principalEntityBuilder.Metadata;
        var dependentType = dependentEntityBuilder.Metadata;

        var fk = dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                ConfigurationSource.Convention)
            .HasForeignKey(
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                }, ConfigurationSource.Convention)
            .HasPrincipalKey(key.Metadata.Properties, ConfigurationSource.Convention)
            .Metadata;

        Assert.NotNull(principalEntityBuilder.Ignore(Customer.UniqueProperty.Name, ConfigurationSource.DataAnnotation));

        Assert.Empty(principalEntityBuilder.Metadata.GetProperties().Where(p => p.Name == Customer.UniqueProperty.Name));
        var newFk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
        Assert.Same(fk.DeclaringEntityType, newFk.DeclaringEntityType);
        Assert.Same(fk.PrincipalEntityType, newFk.PrincipalEntityType);
        Assert.Same(principalType.GetKeys().Single(), newFk.PrincipalKey);
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name, newFk.Properties.Single().Name, Order.CustomerUniqueProperty.Name },
            dependentType.GetProperties().Select(p => p.Name));
        Assert.Equal(
            new[] { Customer.IdProperty.Name, newFk.PrincipalKey.Properties.Single().Name },
            principalType.GetProperties().Select(p => p.Name));
        Assert.Equal(Order.CustomerProperty.Name, newFk.DependentToPrincipal.Name);
        Assert.Equal(Customer.OrdersProperty.Name, newFk.PrincipalToDependent.Name);
    }

    [ConditionalFact]
    public void Cannot_ignore_property_that_is_part_of_same_or_higher_source_key()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

        Assert.Null(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation));

        Assert.NotEmpty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
        Assert.NotEmpty(entityBuilder.Metadata.GetKeys());
    }

    [ConditionalFact]
    public void Navigation_returns_same_value()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var foreignKeyBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, ConfigurationSource.DataAnnotation);

        Assert.True(
            dependentEntityBuilder.CanHaveNavigation(
                Order.CustomerProperty.Name, Order.CustomerProperty.GetMemberType(), ConfigurationSource.Convention));

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            Order.CustomerProperty.Name,
            pointsToPrincipal: true,
            ConfigurationSource.DataAnnotation);

        Assert.True(
            dependentEntityBuilder.CanHaveNavigation(
                Order.CustomerProperty.Name, Order.CustomerProperty.GetMemberType(), ConfigurationSource.Explicit));
        Assert.True(
            principalEntityBuilder.CanHaveNavigation(
                Customer.OrdersProperty.Name, Customer.OrdersProperty.GetMemberType(), ConfigurationSource.Convention));

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            Customer.OrdersProperty.Name,
            pointsToPrincipal: false,
            ConfigurationSource.DataAnnotation);

        Assert.True(
            principalEntityBuilder.CanHaveNavigation(
                Customer.OrdersProperty.Name, Customer.OrdersProperty.GetMemberType(), ConfigurationSource.Explicit));

        var newForeignKeyBuilder = dependentEntityBuilder
            .HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention).HasNavigation(
                Customer.OrdersProperty.Name,
                pointsToPrincipal: false,
                ConfigurationSource.Convention);
        Assert.Same(foreignKeyBuilder, newForeignKeyBuilder);
        newForeignKeyBuilder = principalEntityBuilder.HasRelationship(dependentEntityBuilder.Metadata, ConfigurationSource.Convention)
            .HasNavigation(
                Order.CustomerProperty.Name,
                pointsToPrincipal: false,
                ConfigurationSource.Convention);
        Assert.Same(foreignKeyBuilder, newForeignKeyBuilder);

        Assert.Same(foreignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
        Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(dependentEntityBuilder.Metadata.GetKeys());
        Assert.Same(foreignKeyBuilder.Metadata.PrincipalKey, principalEntityBuilder.Metadata.GetKeys().Single());
    }

    [ConditionalTheory]
    [MemberData(
        nameof(DataGenerator.GetCombinations),
        [new[] { typeof(ConfigurationSource), typeof(ConfigurationSource) }],
        MemberType = typeof(DataGenerator))]
    public void Can_ignore_navigation_in_hierarchy(ConfigurationSource ignoreSource, ConfigurationSource addSource)
    {
        VerifyIgnoreNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);
        VerifyIgnoreNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);
        VerifyIgnoreNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);

        VerifyIgnoreNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
        VerifyIgnoreNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
        VerifyIgnoreNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
    }

    private void VerifyIgnoreNavigation(
        Type ignoredOnType,
        ConfigurationSource ignoreConfigurationSource,
        ConfigurationSource addConfigurationSource,
        bool ignoredFirst,
        bool setBaseFirst)
        => VerifyIgnoreMember(
            ignoredOnType, ignoreConfigurationSource, addConfigurationSource, ignoredFirst, setBaseFirst,
            et => et.Metadata.FindNavigation(Order.CustomerProperty.Name) != null,
            et => et.HasRelationship(
                    et.ModelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit).Metadata,
                    Order.CustomerProperty,
                    Customer.OrdersProperty,
                    addConfigurationSource)
                != null,
            et => et.HasRelationship(
                    et.ModelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit).Metadata,
                    Order.CustomerProperty,
                    Customer.OrdersProperty,
                    ignoreConfigurationSource)
                != null,
            Order.CustomerProperty.Name);

    [ConditionalFact]
    public void Can_merge_with_intrahierarchical_relationship_of_higher_source()
    {
        var modelBuilder = CreateModelBuilder();
        var baseEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
        derivedEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);

        baseEntityBuilder.HasRelationship(derivedEntityBuilder.Metadata, ConfigurationSource.Explicit).HasNavigation(
            nameof(Customer.SpecialCustomer),
            pointsToPrincipal: true,
            ConfigurationSource.Explicit);

        var derivedRelationship = derivedEntityBuilder.HasRelationship(
            baseEntityBuilder.Metadata,
            nameof(SpecialCustomer.Customer),
            nameof(Customer.SpecialCustomer),
            ConfigurationSource.Convention);

        Assert.NotNull(derivedRelationship);

        var baseNavigation = baseEntityBuilder.Metadata.GetNavigations().Single();
        Assert.Equal(nameof(Customer.SpecialCustomer), baseNavigation.Name);
        Assert.Equal(nameof(SpecialCustomer.Customer), baseNavigation.Inverse?.Name);
    }

    [ConditionalFact]
    public void Relationship_does_not_return_same_instance_if_no_navigations()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);

        Assert.NotNull(relationshipBuilder);
        Assert.NotSame(
            relationshipBuilder, orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention));
        Assert.Equal(2, orderEntityBuilder.Metadata.GetForeignKeys().Count());
    }

    [ConditionalTheory]
    [MemberData(
        nameof(DataGenerator.GetCombinations),
        [new[] { typeof(ConfigurationSource), typeof(ConfigurationSource) }],
        MemberType = typeof(DataGenerator))]
    public void Can_ignore_skip_navigation_in_hierarchy(ConfigurationSource ignoreSource, ConfigurationSource addSource)
    {
        VerifyIgnoreSkipNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreSkipNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreSkipNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: true);
        VerifyIgnoreSkipNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);
        VerifyIgnoreSkipNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);
        VerifyIgnoreSkipNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: true);

        VerifyIgnoreSkipNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreSkipNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreSkipNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: true, setBaseFirst: false);
        VerifyIgnoreSkipNavigation(typeof(Order), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
        VerifyIgnoreSkipNavigation(typeof(SpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
        VerifyIgnoreSkipNavigation(typeof(ExtraSpecialOrder), ignoreSource, addSource, ignoredFirst: false, setBaseFirst: false);
    }

    private void VerifyIgnoreSkipNavigation(
        Type ignoredOnType,
        ConfigurationSource ignoreConfigurationSource,
        ConfigurationSource addConfigurationSource,
        bool ignoredFirst,
        bool setBaseFirst)
        => VerifyIgnoreMember(
            ignoredOnType, ignoreConfigurationSource, addConfigurationSource, ignoredFirst, setBaseFirst,
            et => et.Metadata.FindSkipNavigation(nameof(Order.Products)) != null,
            et => et.HasSkipNavigation(
                    MemberIdentity.Create(Order.ProductsProperty),
                    et.ModelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit).Metadata,
                    addConfigurationSource)
                != null,
            et => et.HasSkipNavigation(
                    MemberIdentity.Create(Order.ProductsProperty),
                    et.ModelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit).Metadata,
                    ignoreConfigurationSource)
                != null,
            nameof(Order.Products));

    [ConditionalTheory]
    [MemberData(
        nameof(DataGenerator.GetCombinations),
        [
            new[] { typeof(ConfigurationSource), typeof(ConfigurationSource), typeof(MemberType), typeof(MemberType), typeof(bool) }
        ],
        MemberType = typeof(DataGenerator))]
    public void Can_override_members_in_hierarchy(
        ConfigurationSource firstSource,
        ConfigurationSource secondSource,
        MemberType firstMemberType,
        MemberType secondMemberType,
        bool setBaseFirst)
    {
        VerifyOverrideMembers(typeof(Order), firstSource, secondSource, firstMemberType, secondMemberType, setBaseFirst);
        VerifyOverrideMembers(typeof(SpecialOrder), firstSource, secondSource, firstMemberType, secondMemberType, setBaseFirst);
        VerifyOverrideMembers(typeof(ExtraSpecialOrder), firstSource, secondSource, firstMemberType, secondMemberType, setBaseFirst);
    }

    private void VerifyOverrideMembers(
        Type firstType,
        ConfigurationSource firstSource,
        ConfigurationSource secondSource,
        MemberType firstMemberType,
        MemberType secondMemberType,
        bool setBaseFirst)
    {
        var modelBuilder = CreateModelBuilder();
        var productTypeBuilder = modelBuilder.Entity(typeof(Product), ConfigurationSource.Convention);
        productTypeBuilder.PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

        if (setBaseFirst)
        {
            ConfigureOrdersHierarchy(modelBuilder);
        }

        var firstEntityTypeBuilder = modelBuilder.Entity(firstType, ConfigurationSource.Convention);
        var secondEntityTypeBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

        Assert.True(ConfigureMember(firstEntityTypeBuilder, firstMemberType, firstSource));

        if ((!setBaseFirst && firstEntityTypeBuilder != secondEntityTypeBuilder)
            || firstSource != ConfigurationSource.Explicit
            || secondSource != ConfigurationSource.Explicit
            || firstMemberType == secondMemberType)
        {
            Assert.Equal(
                (!setBaseFirst && firstEntityTypeBuilder != secondEntityTypeBuilder)
                || firstMemberType == secondMemberType
                || secondSource.Overrides(firstSource),
                ConfigureMember(secondEntityTypeBuilder, secondMemberType, secondSource));
        }
        else
        {
            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation(
                    nameof(Order.Products), nameof(SpecialOrder), firstEntityTypeBuilder.Metadata.DisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => ConfigureMember(secondEntityTypeBuilder, secondMemberType, secondSource)).Message);

            return;
        }

        if (!setBaseFirst)
        {
            if (firstEntityTypeBuilder == secondEntityTypeBuilder
                || firstSource != ConfigurationSource.Explicit
                || secondSource != ConfigurationSource.Explicit
                || firstMemberType == secondMemberType)
            {
                ConfigureOrdersHierarchy(modelBuilder);
            }
            else
            {
                if (firstType == typeof(Order))
                {
                    Assert.Equal(
                        CoreStrings.DuplicatePropertiesOnBase(
                            nameof(SpecialOrder), nameof(Order),
                            nameof(SpecialOrder), nameof(Order.Products), nameof(Order), nameof(Order.Products)),
                        Assert.Throws<InvalidOperationException>(
                            () => ConfigureOrdersHierarchy(modelBuilder)).Message);
                }
                else
                {
                    Assert.Equal(
                        CoreStrings.DuplicatePropertiesOnBase(
                            nameof(ExtraSpecialOrder), nameof(SpecialOrder),
                            nameof(ExtraSpecialOrder), nameof(Order.Products), nameof(SpecialOrder), nameof(Order.Products)),
                        Assert.Throws<InvalidOperationException>(
                            () => ConfigureOrdersHierarchy(modelBuilder)).Message);
                }

                return;
            }
        }

        var leastDerivedType = firstEntityTypeBuilder.Metadata.LeastDerivedType(secondEntityTypeBuilder.Metadata);
        var shouldSecondWin = secondSource.Overrides(firstSource)
            && (secondSource != firstSource
                || setBaseFirst
                || secondEntityTypeBuilder.Metadata == leastDerivedType);

        var expectedDeclaringType = firstMemberType == secondMemberType
            ? leastDerivedType
            : shouldSecondWin
                ? secondEntityTypeBuilder.Metadata
                : firstEntityTypeBuilder.Metadata;

        var expectedMemberType = shouldSecondWin
            ? secondMemberType
            : firstMemberType;

        AssertDeclaringType(
            modelBuilder.Entity(typeof(ExtraSpecialOrder), ConfigurationSource.Convention),
            expectedDeclaringType,
            expectedMemberType);
    }

    private bool ConfigureMember(
        InternalEntityTypeBuilder entityTypeBuilder,
        MemberType memberType,
        ConfigurationSource configurationSource)
    {
        switch (memberType)
        {
            case MemberType.Property:
                return entityTypeBuilder.Property(Order.ProductsProperty, configurationSource) != null;
            case MemberType.ComplexProperty:
                return entityTypeBuilder.ComplexProperty(
                        Order.ProductsProperty, complexTypeName: null, collection: true, configurationSource)
                    != null;
            case MemberType.ServiceProperty:
                return entityTypeBuilder.ServiceProperty(Order.ProductsProperty, configurationSource) != null;
            case MemberType.Navigation:
                return entityTypeBuilder.HasRelationship(
                        entityTypeBuilder.ModelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit).Metadata,
                        Order.ProductsProperty,
                        null,
                        configurationSource)
                    != null;
            case MemberType.SkipNavigation:
                return entityTypeBuilder.HasSkipNavigation(
                        MemberIdentity.Create(Order.ProductsProperty),
                        entityTypeBuilder.ModelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit).Metadata,
                        configurationSource)
                    != null;
        }

        return false;
    }

    private void AssertDeclaringType(
        InternalEntityTypeBuilder entityTypeBuilder,
        EntityType expectedDeclaringType,
        MemberType memberType)
    {
        Assert.Same(
            memberType == MemberType.Property ? expectedDeclaringType : null,
            GetDeclaringType(entityTypeBuilder, MemberType.Property));
        Assert.Same(
            memberType == MemberType.ComplexProperty ? expectedDeclaringType : null,
            GetDeclaringType(entityTypeBuilder, MemberType.ComplexProperty));
        Assert.Same(
            memberType == MemberType.ServiceProperty ? expectedDeclaringType : null,
            GetDeclaringType(entityTypeBuilder, MemberType.ServiceProperty));
        Assert.Same(
            memberType == MemberType.Navigation ? expectedDeclaringType : null,
            GetDeclaringType(entityTypeBuilder, MemberType.Navigation));
        Assert.Same(
            memberType == MemberType.SkipNavigation ? expectedDeclaringType : null,
            GetDeclaringType(entityTypeBuilder, MemberType.SkipNavigation));
    }

    private EntityType GetDeclaringType(
        InternalEntityTypeBuilder entityTypeBuilder,
        MemberType memberType)
    {
        switch (memberType)
        {
            case MemberType.Property:
                return (EntityType)entityTypeBuilder.Metadata.FindProperty(nameof(Order.Products))?.DeclaringType;
            case MemberType.ComplexProperty:
                return (EntityType)entityTypeBuilder.Metadata.FindComplexProperty(nameof(Order.Products))?.DeclaringType;
            case MemberType.ServiceProperty:
                return entityTypeBuilder.Metadata.FindServiceProperty(nameof(Order.Products))?.DeclaringEntityType;
            case MemberType.Navigation:
                return entityTypeBuilder.Metadata.FindNavigation(nameof(Order.Products))?.DeclaringEntityType;
            case MemberType.SkipNavigation:
                return entityTypeBuilder.Metadata.FindSkipNavigation(nameof(Order.Products))?.DeclaringEntityType;
        }

        return null;
    }

    [ConditionalFact]
    public void Can_ignore_lower_source_weak_entity_type()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        Assert.NotNull(dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention));

        Assert.NotNull(modelBuilder.Ignore(typeof(Order), ConfigurationSource.Explicit));

        Assert.Equal(typeof(Customer).FullName, modelBuilder.Metadata.GetEntityTypes().Single().Name);
    }

    [ConditionalFact]
    public void Can_ignore_lower_source_principal_entity_type()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        Assert.NotNull(dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention));

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

        Assert.Equal(typeof(Order).FullName, modelBuilder.Metadata.GetEntityTypes().Single().Name);
    }

    [ConditionalFact]
    public void Can_ignore_lower_source_navigation_to_dependent()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.NotNull(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, null, Customer.OrdersProperty.Name, ConfigurationSource.Convention));

        Assert.NotNull(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
        Assert.NotNull(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
        Assert.Null(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, null, Customer.OrdersProperty.Name, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Can_ignore_lower_source_navigation_to_principal()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.NotNull(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, Order.CustomerProperty, null, ConfigurationSource.Convention));

        Assert.NotNull(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));

        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
        Assert.NotNull(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
        Assert.Null(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, Order.CustomerProperty, null, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Cannot_add_navigation_to_principal_if_null_navigation_is_higher_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, null, Customer.OrdersProperty.Name, ConfigurationSource.Explicit);

        Assert.Null(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, Order.CustomerProperty.Name, Customer.OrdersProperty.Name,
                ConfigurationSource.Convention));

        Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
        var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
        Assert.Null(fk.DependentToPrincipal?.Name);
        Assert.Equal(Customer.OrdersProperty.Name, fk.PrincipalToDependent.Name);
    }

    [ConditionalFact]
    public void Cannot_add_navigation_to_dependent_if_null_navigation_is_higher_source()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, Order.CustomerProperty.Name, null, ConfigurationSource.Explicit);

        Assert.Null(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, Order.CustomerProperty, Customer.OrdersProperty, ConfigurationSource.Convention));

        Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
        var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
        Assert.Null(fk.PrincipalToDependent?.Name);
        Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
    }

    [ConditionalFact]
    public void Dependent_conflicting_relationship_is_not_removed_if_principal_conflicting_relationship_cannot_be_removed()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var orderRelationship = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, Order.CustomerProperty.Name, null, ConfigurationSource.DataAnnotation);
        Assert.NotNull(orderRelationship);
        var customerRelationship = dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, null, Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention)
            .HasEntityTypes(principalEntityBuilder.Metadata, dependentEntityBuilder.Metadata, ConfigurationSource.Convention);
        Assert.NotNull(customerRelationship);

        Assert.Null(
            principalEntityBuilder.HasRelationship(
                dependentEntityBuilder.Metadata, Customer.NotCollectionOrdersProperty.Name, Order.CustomerProperty.Name,
                ConfigurationSource.Convention));

        var orderFk = dependentEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
        var customerFk = principalEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
        Assert.Null(orderFk.PrincipalToDependent);
        Assert.Equal(Order.CustomerProperty.Name, orderFk.DependentToPrincipal.Name);
        Assert.Equal(Customer.NotCollectionOrdersProperty.Name, customerFk.PrincipalToDependent.Name);
        Assert.Null(customerFk.DependentToPrincipal);
    }

    [ConditionalFact]
    public void Principal_conflicting_relationship_is_not_removed_if_dependent_conflicting_relationship_cannot_be_removed()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var orderRelationship = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, Order.CustomerProperty, null, ConfigurationSource.Convention);
        Assert.NotNull(orderRelationship);
        var customerRelationship = dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, null, Customer.NotCollectionOrdersProperty, ConfigurationSource.DataAnnotation)
            .HasEntityTypes(principalEntityBuilder.Metadata, dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation);
        Assert.NotNull(customerRelationship);

        Assert.Null(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, Customer.NotCollectionOrdersProperty, Order.CustomerProperty,
                ConfigurationSource.Convention));

        Assert.Null(orderRelationship.Metadata.PrincipalToDependent);
        Assert.Equal(Order.CustomerProperty.Name, orderRelationship.Metadata.DependentToPrincipal.Name);
        Assert.Equal(Customer.NotCollectionOrdersProperty.Name, customerRelationship.Metadata.PrincipalToDependent.Name);
        Assert.Null(customerRelationship.Metadata.DependentToPrincipal);
    }

    [ConditionalFact]
    public void Conflicting_navigations_are_not_removed_if_conflicting_fk_cannot_be_removed()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var fkProperty = orderEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Explicit).Metadata;
        var key = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit).Metadata;

        var orderRelationship = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata, Order.CustomerProperty.Name, null, ConfigurationSource.Convention);
        Assert.NotNull(orderRelationship);
        var customerRelationship = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata, null, Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention);
        Assert.NotNull(customerRelationship);
        var fkRelationship = orderEntityBuilder.HasRelationship(
                customerEntityBuilder.Metadata, new[] { fkProperty }, key, ConfigurationSource.DataAnnotation)
            .IsUnique(false, ConfigurationSource.DataAnnotation);
        Assert.NotNull(fkRelationship);
        Assert.Same(
            fkRelationship.Metadata, orderEntityBuilder.Metadata.GetForeignKeys()
                .Single(fk => fk.DependentToPrincipal == null && fk.PrincipalToDependent == null));
        var fk1 = orderEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey;
        Assert.NotSame(fkRelationship.Metadata, fk1);
        var fk2 = customerEntityBuilder.Metadata.FindNavigation(Customer.NotCollectionOrdersProperty.Name).ForeignKey;
        Assert.NotSame(fkRelationship.Metadata, fk2);
        Assert.NotSame(fk1, fk2);

        orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention).HasNavigation(
                Order.CustomerProperty.Name,
                pointsToPrincipal: true,
                ConfigurationSource.Convention).HasNavigation(
                Customer.NotCollectionOrdersProperty.Name,
                pointsToPrincipal: false,
                ConfigurationSource.Convention)
            .HasForeignKey(new[] { fkProperty }, ConfigurationSource.Convention)
            ?.HasPrincipalKey(key.Properties, ConfigurationSource.Convention);

        var navigationFk = customerEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
        Assert.Same(navigationFk, orderEntityBuilder.Metadata.GetNavigations().Single().ForeignKey);
        Assert.NotSame(navigationFk, fkRelationship.Metadata);
        Assert.NotNull(fkRelationship.Metadata.Builder);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_base_type()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.DataAnnotation));
        Assert.Null(modelBuilder.Ignore(entityBuilder.Metadata.Name, ConfigurationSource.Convention));
        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Explicit));
        Assert.Null(derivedEntityBuilder.HasBaseType((string)null, ConfigurationSource.Convention));
        Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
    }

    [ConditionalFact]
    public void Can_only_override_existing_base_type_explicitly()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.Metadata.SetBaseType(entityBuilder.Metadata, ConfigurationSource.Explicit);

        Assert.Same(derivedEntityBuilder, derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention));
        Assert.Null(derivedEntityBuilder.HasBaseType((EntityType)null, ConfigurationSource.Convention));
        Assert.Same(derivedEntityBuilder, derivedEntityBuilder.HasBaseType((EntityType)null, ConfigurationSource.Explicit));
        Assert.Null(derivedEntityBuilder.Metadata.BaseType);
    }

    [ConditionalFact]
    public void Can_set_base_type_if_duplicate_properties_of_higher_source()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation);

        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata.Name, ConfigurationSource.Convention));
        Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
        Assert.Single(entityBuilder.Metadata.GetDeclaredProperties());
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredProperties());
    }

    [ConditionalFact]
    public void Can_only_set_base_type_if_keys_of_data_annotation_or_lower_source()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.HasKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

        Assert.Null(derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention));
        Assert.Null(derivedEntityBuilder.Metadata.BaseType);
        Assert.Single(derivedEntityBuilder.Metadata.GetDeclaredKeys());

        entityBuilder.HasKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation);
        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType(typeof(Order), ConfigurationSource.Explicit));
        Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredKeys());
    }

    [ConditionalFact]
    public void Cannot_set_base_type_for_relationship_with_explicit_conflicting_incompatible_navigations()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        Assert.NotNull(
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, Order.CustomerProperty.Name, null, ConfigurationSource.Explicit));

        var derivedPrincipalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit)
            .HasBaseType((string)null, ConfigurationSource.Explicit);
        var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        Assert.NotNull(
            derivedDependentEntityBuilder.HasRelationship(
                derivedPrincipalEntityBuilder.Metadata, Order.CustomerProperty.Name, null, ConfigurationSource.Explicit));

        Assert.Null(derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
        Assert.Null(derivedDependentEntityBuilder.Metadata.BaseType);
        Assert.Single(derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations());

        Assert.Equal(
            CoreStrings.DuplicatePropertiesOnBase(
                nameof(SpecialOrder), nameof(Order),
                nameof(SpecialOrder), nameof(Order.Customer), nameof(Order), nameof(Order.Customer)),
            Assert.Throws<InvalidOperationException>(
                    () => derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Explicit))
                .Message);
    }

    [ConditionalFact]
    public void Can_set_base_type_for_relationship_with_explicit_conflicting_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit);

        var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedDependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit);

        Assert.Same(
            derivedDependentEntityBuilder,
            derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
        Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
        Assert.Single(dependentEntityBuilder.Metadata.GetDeclaredForeignKeys());
        Assert.Empty(derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys());
    }

    [ConditionalFact]
    public void Setting_base_type_preserves_index()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
        derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation)
            .IsUnique(true, ConfigurationSource.Convention);
        Assert.Single(derivedEntityBuilder.Metadata.GetDeclaredIndexes());

        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention));

        Assert.Single(derivedEntityBuilder.Metadata.GetDeclaredIndexes());
        Assert.True(derivedEntityBuilder.Metadata.GetDeclaredIndexes().First().IsUnique);
    }

    [ConditionalFact]
    public void Setting_base_type_preserves_non_conflicting_referencing_relationship()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var derivedPrincipalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Convention);
        var derivedIdProperty = derivedPrincipalEntityBuilder.Property(Customer.IdProperty, ConfigurationSource.Convention).Metadata;

        dependentEntityBuilder.HasRelationship(
                derivedPrincipalEntityBuilder.Metadata,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                ConfigurationSource.Convention)
            .HasPrincipalKey(new[] { derivedIdProperty }, ConfigurationSource.Convention);
        Assert.Single(derivedPrincipalEntityBuilder.Metadata.GetDeclaredKeys());

        Assert.Same(
            derivedPrincipalEntityBuilder,
            derivedPrincipalEntityBuilder.HasBaseType(principalEntityBuilder.Metadata, ConfigurationSource.Convention));

        Assert.Single(principalEntityBuilder.Metadata.GetDeclaredKeys());
        Assert.Empty(derivedPrincipalEntityBuilder.Metadata.GetDeclaredKeys());
        Assert.Empty(derivedPrincipalEntityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(principalEntityBuilder.Metadata.GetReferencingForeignKeys());
        var fk = derivedPrincipalEntityBuilder.Metadata.GetReferencingForeignKeys().Single();
        Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
        Assert.Equal(Customer.OrdersProperty.Name, fk.PrincipalToDependent.Name);
    }

    [ConditionalFact]
    public void Setting_base_type_preserves_non_conflicting_relationship_on_duplicate_foreign_key_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        var derivedIdProperty = derivedDependentEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention).Metadata;

        derivedDependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                Order.CustomerProperty.Name,
                Customer.SpecialOrdersProperty.Name,
                ConfigurationSource.DataAnnotation)
            .HasForeignKey(new[] { derivedIdProperty }, ConfigurationSource.DataAnnotation);

        Assert.Same(
            derivedDependentEntityBuilder,
            derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(dependentEntityBuilder.Metadata.GetDeclaredProperties());
        var fk = derivedDependentEntityBuilder.Metadata.GetForeignKeys().Single();
        Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
        Assert.Equal(Customer.SpecialOrdersProperty.Name, fk.PrincipalToDependent.Name);
        Assert.Equal(Order.IdProperty.Name, fk.Properties.Single().Name);
    }

    [ConditionalFact]
    public void Setting_base_type_fixes_relationship_conflicting_with_PK()
    {
        var modelBuilder = CreateModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        dependentEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);
        var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedDependentEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Convention);
        var derivedIdProperty = derivedDependentEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention).Metadata;

        derivedDependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                Order.CustomerProperty.Name,
                Customer.SpecialOrdersProperty.Name,
                ConfigurationSource.Explicit)
            .HasForeignKey(new[] { derivedIdProperty }, ConfigurationSource.Convention);

        Assert.Same(
            derivedDependentEntityBuilder,
            derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation));
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        var fk = derivedDependentEntityBuilder.Metadata.GetForeignKeys().Single();
        Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
        Assert.Equal(Customer.SpecialOrdersProperty.Name, fk.PrincipalToDependent.Name);
        Assert.NotEqual(Order.IdProperty.Name, fk.Properties.Single().Name);
        Assert.Single(dependentEntityBuilder.Metadata.GetDeclaredProperties());
    }

    [ConditionalFact]
    public void Setting_base_type_removes_duplicate_derived_service_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);
        entityBuilder.ServiceProperty(Order.ContextProperty, ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Convention);
        derivedEntityBuilder.ServiceProperty(Order.ContextProperty, ConfigurationSource.Convention);

        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention));

        Assert.Single(entityBuilder.Metadata.GetServiceProperties());
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredServiceProperties());
    }

    [ConditionalFact]
    public void Setting_base_type_preserves_duplicate_base_service_properties()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);
        entityBuilder.ServiceProperty(Order.ContextProperty, ConfigurationSource.Explicit);
        var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
        derivedEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Convention);
        derivedEntityBuilder.ServiceProperty(Order.ContextProperty, ConfigurationSource.Explicit);

        Assert.Same(
            derivedEntityBuilder,
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Single(entityBuilder.Metadata.GetServiceProperties());
        Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredServiceProperties());
    }

    [ConditionalFact]
    public void Can_access_discriminator()
    {
        IConventionEntityTypeBuilder typeBuilder = CreateModelBuilder().Entity(typeof(Order), ConfigurationSource.Convention);

        Assert.NotNull(typeBuilder.HasDiscriminator());
        Assert.Equal("Discriminator", typeBuilder.Metadata.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), typeBuilder.Metadata.FindDiscriminatorProperty().ClrType);

        Assert.NotNull(typeBuilder.HasNoDiscriminator());
        Assert.Null(typeBuilder.Metadata.FindDiscriminatorProperty());
        Assert.Empty(typeBuilder.Metadata.GetProperties());

        Assert.NotNull(typeBuilder.HasDiscriminator("Splod", typeof(int?)));
        Assert.Equal("Splod", typeBuilder.Metadata.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(int?), typeBuilder.Metadata.FindDiscriminatorProperty().ClrType);
        Assert.Equal("Splod", typeBuilder.Metadata.GetProperties().Single().Name);

        Assert.NotNull(typeBuilder.HasDiscriminator(Order.CustomerUniqueProperty, fromDataAnnotation: true));
        Assert.Equal(Order.CustomerUniqueProperty.Name, typeBuilder.Metadata.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(Guid?), typeBuilder.Metadata.FindDiscriminatorProperty().ClrType);
        Assert.Equal(Order.CustomerUniqueProperty.Name, typeBuilder.Metadata.GetProperties().Single().Name);

        Assert.Null(typeBuilder.HasDiscriminator("Splew", typeof(int?)));
        Assert.Equal(Order.CustomerUniqueProperty.Name, typeBuilder.Metadata.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(Guid?), typeBuilder.Metadata.FindDiscriminatorProperty().ClrType);
        Assert.Equal(Order.CustomerUniqueProperty.Name, typeBuilder.Metadata.GetProperties().Single().Name);

        Assert.NotNull(typeBuilder.HasDiscriminator(typeof(int), fromDataAnnotation: true));
        Assert.Null(typeBuilder.HasDiscriminator(typeof(long)));
        Assert.Equal("Discriminator", typeBuilder.Metadata.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(int), typeBuilder.Metadata.FindDiscriminatorProperty().ClrType);

        Assert.Null(typeBuilder.HasNoDiscriminator());
    }

    [ConditionalFact]
    public void Discriminator_is_not_set_if_ignored()
    {
        IConventionEntityTypeBuilder typeBuilder = CreateModelBuilder().Entity(typeof(Order), ConfigurationSource.Convention);
        typeBuilder.Ignore("Splod", true);

        Assert.NotNull(typeBuilder.HasDiscriminator("Splew", typeof(string)));
        Assert.Equal("Splew", typeBuilder.Metadata.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), typeBuilder.Metadata.FindDiscriminatorProperty().ClrType);

        Assert.Null(typeBuilder.HasDiscriminator("Splod", typeof(int?)));
        Assert.Equal("Splew", typeBuilder.Metadata.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), typeBuilder.Metadata.FindDiscriminatorProperty().ClrType);
    }

    [ConditionalFact]
    public void Discriminator_is_not_set_if_default_ignored()
    {
        IConventionEntityTypeBuilder typeBuilder = CreateModelBuilder().Entity(typeof(Order), ConfigurationSource.Convention);
        typeBuilder.Ignore("Discriminator", true);

        Assert.Null(typeBuilder.HasDiscriminator());
        Assert.Empty(typeBuilder.Metadata.GetProperties());
    }

    [ConditionalFact]
    public void Can_access_discriminator_value()
    {
        IConventionEntityTypeBuilder typeBuilder = CreateModelBuilder().Entity("Splot", ConfigurationSource.Convention);
        var derivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splod");
        derivedTypeBuilder.HasBaseType(typeBuilder.Metadata, fromDataAnnotation: true);
        var otherDerivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splow");

        Assert.NotNull(typeBuilder.HasDiscriminator());
        Assert.Single(typeBuilder.Metadata.GetDeclaredProperties());
        Assert.Empty(derivedTypeBuilder.Metadata.GetDeclaredProperties());

        var discriminatorBuilder = typeBuilder.HasDiscriminator("Splowed", typeof(int?));
        Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
        Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 2));
        Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 3));

        Assert.Same(typeBuilder.Metadata, otherDerivedTypeBuilder.Metadata.BaseType);
        Assert.Equal(1, typeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(
            2, typeBuilder.ModelBuilder.Entity("Splow")
                .Metadata.GetDiscriminatorValue());
        Assert.Equal(
            3, typeBuilder.ModelBuilder.Entity("Splod")
                .Metadata.GetDiscriminatorValue());
        Assert.Same(typeBuilder.Metadata, typeBuilder.ModelBuilder.Metadata.FindEntityType("Splow").BaseType);

        discriminatorBuilder = typeBuilder.HasDiscriminator(fromDataAnnotation: true);
        Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, 4, fromDataAnnotation: true));
        Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 5, fromDataAnnotation: true));
        Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 6, fromDataAnnotation: true));
        Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(
            5, typeBuilder.ModelBuilder.Entity("Splow")
                .Metadata.GetDiscriminatorValue());
        Assert.Equal(
            6, typeBuilder.ModelBuilder.Entity("Splod")
                .Metadata.GetDiscriminatorValue());

        discriminatorBuilder = typeBuilder.HasDiscriminator();
        Assert.Null(discriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
        Assert.Null(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 2));
        Assert.Null(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 3));
        Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(
            5, typeBuilder.ModelBuilder.Entity("Splow")
                .Metadata.GetDiscriminatorValue());
        Assert.Equal(
            6, typeBuilder.ModelBuilder.Entity("Splod")
                .Metadata.GetDiscriminatorValue());

        Assert.NotNull(typeBuilder.HasNoDiscriminator(fromDataAnnotation: true));
        Assert.Null(typeBuilder.Metadata.FindDiscriminatorProperty());
        Assert.Null(typeBuilder.Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.Empty(typeBuilder.Metadata.GetProperties());
    }

    [ConditionalFact]
    public void Changing_discriminator_type_removes_values()
    {
        IConventionEntityTypeBuilder typeBuilder = CreateModelBuilder().Entity("Splot", ConfigurationSource.Convention);
        var derivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splod");
        derivedTypeBuilder.HasBaseType(typeBuilder.Metadata, fromDataAnnotation: true);
        var otherDerivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splow");

        Assert.NotNull(typeBuilder.HasDiscriminator());
        Assert.Single(typeBuilder.Metadata.GetDeclaredProperties());
        Assert.Empty(derivedTypeBuilder.Metadata.GetDeclaredProperties());

        var discriminatorBuilder = typeBuilder.HasDiscriminator("Splowed", typeof(int));
        Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
        Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 2));
        Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 3));

        discriminatorBuilder = typeBuilder.HasDiscriminator("Splowed", typeof(string));
        Assert.Null(typeBuilder.Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.Null(
            typeBuilder.ModelBuilder.Entity("Splow")
                .Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.Null(
            typeBuilder.ModelBuilder.Entity("Splod")
                .Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, "4"));
        Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, "5"));
        Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, "6"));

        discriminatorBuilder = typeBuilder.HasDiscriminator("Splotted", typeof(string));

        Assert.NotNull(discriminatorBuilder);
        Assert.Equal("4", typeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(
            "5", typeBuilder.ModelBuilder.Entity("Splow")
                .Metadata.GetDiscriminatorValue());
        Assert.Equal(
            "6", typeBuilder.ModelBuilder.Entity("Splod")
                .Metadata.GetDiscriminatorValue());

        discriminatorBuilder = typeBuilder.HasDiscriminator(typeof(int));

        Assert.NotNull(discriminatorBuilder);
        Assert.Null(typeBuilder.Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.Null(
            typeBuilder.ModelBuilder.Entity("Splow")
                .Metadata[CoreAnnotationNames.DiscriminatorValue]);
        Assert.Null(
            typeBuilder.ModelBuilder.Entity("Splod")
                .Metadata[CoreAnnotationNames.DiscriminatorValue]);
    }

    [ConditionalFact]
    public void Can_access_discriminator_value_generic()
    {
        IConventionEntityTypeBuilder typeBuilder = CreateModelBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

        var discriminatorBuilder = new DiscriminatorBuilder<int?>(
            (DiscriminatorBuilder)typeBuilder.HasDiscriminator(Splot.SplowedProperty));
        Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splot), 1));
        Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splow), 2));
        Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splod), 3));

        var splow = typeBuilder.ModelBuilder.Entity(typeof(Splow)).Metadata;
        var splod = typeBuilder.ModelBuilder.Entity(typeof(Splod)).Metadata;
        Assert.Equal(1, typeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(2, splow.GetDiscriminatorValue());
        Assert.Equal(
            3, typeBuilder.ModelBuilder.Entity(typeof(Splod))
                .Metadata.GetDiscriminatorValue());

        discriminatorBuilder = new DiscriminatorBuilder<int?>(
            (DiscriminatorBuilder)typeBuilder.HasDiscriminator(fromDataAnnotation: true));
        Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splot), 4));
        Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splow), 5));
        Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splod), 6));
        Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(5, splow.GetDiscriminatorValue());
        Assert.Equal(6, splod.GetDiscriminatorValue());

        var conventionDiscriminatorBuilder = typeBuilder.HasDiscriminator();
        Assert.Null(conventionDiscriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
        Assert.Null(conventionDiscriminatorBuilder.HasValue(splow, 2));
        Assert.Null(conventionDiscriminatorBuilder.HasValue(splod, 3));
        Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
        Assert.Equal(5, splow.GetDiscriminatorValue());
        Assert.Equal(6, splod.GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void DiscriminatorValue_throws_if_base_cannot_be_set()
    {
        IConventionModelBuilder modelBuilder = CreateModelBuilder();
        var typeBuilder = modelBuilder.Entity("Splot");
        var nonDerivedTypeBuilder = modelBuilder.Entity("Splow");
        nonDerivedTypeBuilder.HasBaseType(modelBuilder.Entity("Splod").Metadata, true);

        var discriminatorBuilder = typeBuilder.HasDiscriminator();
        Assert.Equal(
            CoreStrings.DiscriminatorEntityTypeNotDerived("Splow (Dictionary<string, object>)", "Splot (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(
                () => discriminatorBuilder.HasValue(nonDerivedTypeBuilder.Metadata, "1")).Message);
    }

    private static TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions> CreateTestLogger()
        => new() { EnabledFor = LogLevel.Warning };

    private InternalModelBuilder CreateModelBuilder(Model model = null)
        => new(model ?? new Model());

    private InternalModelBuilder CreateConventionalModelBuilder()
        => (InternalModelBuilder)InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

    public enum MemberType
    {
        Property,
        ComplexProperty,
        ServiceProperty,
        Navigation,
        SkipNavigation
    }

    private class Order
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty(nameof(Id));
        public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty(nameof(CustomerId));
        public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty(nameof(CustomerUnique));
        public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty(nameof(Customer));
        public static readonly PropertyInfo ContextProperty = typeof(Order).GetProperty(nameof(Context));
        public static readonly PropertyInfo ProductsProperty = typeof(Order).GetProperty(nameof(Products));

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Guid? CustomerUnique { get; set; }
        public Customer Customer { get; set; }
        public DbContext Context { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    private class SpecialOrder : Order, IEnumerable<Order>
    {
        public static readonly PropertyInfo SpecialtyProperty = typeof(SpecialOrder).GetProperty("Specialty");

        public IEnumerator<Order> GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public string Specialty { get; set; }
    }

    private class ExtraSpecialOrder : SpecialOrder;

    private class BackOrder : Order;

    private class Customer
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty(nameof(Id));
        public static readonly PropertyInfo UniqueProperty = typeof(Customer).GetProperty(nameof(Unique));
        public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty(nameof(Orders));
        public static readonly PropertyInfo NotCollectionOrdersProperty = typeof(Customer).GetProperty(nameof(NotCollectionOrders));
        public static readonly PropertyInfo SpecialOrdersProperty = typeof(Customer).GetProperty(nameof(SpecialOrders));

        public int Id { get; set; }
        public Guid Unique { get; set; }
        public ICollection<Order> Orders { get; set; }
        public Order NotCollectionOrders { get; set; }
        public ICollection<SpecialOrder> SpecialOrders { get; set; }
        internal SpecialCustomer SpecialCustomer { get; set; }
    }

    private class SpecialCustomer : Customer
    {
        internal Customer Customer { get; set; }
    }

    private class OrderProduct
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(OrderProduct).GetProperty(nameof(OrderId));
        public static readonly PropertyInfo ProductIdProperty = typeof(OrderProduct).GetProperty(nameof(ProductId));

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

    private class Product
    {
        public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty(nameof(Id));

        public int Id { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }

    private class SpecialProduct : Product;

    private class ExtraSpecialProduct : SpecialProduct;

    private class Splot
    {
        public static readonly PropertyInfo SplowedProperty = typeof(Splot).GetProperty("Splowed");

        public int? Splowed { get; set; }
    }

    private class Splow : Splot;

    private class Splod : Splow;

    private class IndexedClass
    {
        public static readonly string IndexerPropertyName = "Indexer";

        public object this[string name]
        {
            get => null;
            set { }
        }
    }
}
