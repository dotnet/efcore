// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class InternalForeignKeyBuilderTest
{
    [ConditionalFact]
    public void Facets_are_configured_with_the_specified_source()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var key = principalEntityBuilder.PrimaryKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, ConfigurationSource.Convention);

        var fk = relationshipBuilder.Metadata;
        Assert.Equal(ConfigurationSource.Convention, fk.GetConfigurationSource());
        Assert.Null(fk.GetPropertiesConfigurationSource());
        Assert.Null(fk.GetPrincipalKeyConfigurationSource());
        Assert.Null(fk.GetPrincipalEndConfigurationSource());
        Assert.Null(fk.GetDependentToPrincipalConfigurationSource());
        Assert.Null(fk.GetPrincipalToDependentConfigurationSource());
        Assert.Null(fk.GetIsRequiredConfigurationSource());
        Assert.Null(fk.GetIsRequiredDependentConfigurationSource());
        Assert.Null(fk.GetIsOwnershipConfigurationSource());
        Assert.Null(fk.GetIsUniqueConfigurationSource());
        Assert.Null(fk.GetDeleteBehaviorConfigurationSource());

        relationshipBuilder = relationshipBuilder.PrincipalEntityType(principalEntityBuilder.Metadata, ConfigurationSource.Explicit)
            .HasPrincipalKey(key.Metadata.Properties, ConfigurationSource.Explicit).HasNavigation(
                Order.CustomerProperty.Name,
                pointsToPrincipal: true,
                ConfigurationSource.Explicit).HasNavigation(
                Customer.OrdersProperty.Name,
                pointsToPrincipal: false,
                ConfigurationSource.Explicit)
            .IsUnique(false, ConfigurationSource.Explicit)
            .IsRequired(false, ConfigurationSource.Explicit)
            .IsRequiredDependent(false, ConfigurationSource.Explicit)
            .IsOwnership(false, ConfigurationSource.Explicit)
            .OnDelete(DeleteBehavior.Cascade, ConfigurationSource.Explicit)
            .HasForeignKey(
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                }, ConfigurationSource.Explicit);

        Assert.Null(
            relationshipBuilder.HasForeignKey(
                new[] { Order.IdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));
        var shadowId = principalEntityBuilder.Property(typeof(int), "ShadowId", ConfigurationSource.Convention).Metadata;
        Assert.Null(
            relationshipBuilder.HasPrincipalKey(
                new[] { shadowId.Name, Customer.UniqueProperty.Name }, ConfigurationSource.DataAnnotation));
        Assert.Null(relationshipBuilder.IsUnique(true, ConfigurationSource.DataAnnotation));
        Assert.Null(relationshipBuilder.IsRequired(true, ConfigurationSource.DataAnnotation));
        Assert.Null(relationshipBuilder.IsRequiredDependent(true, ConfigurationSource.DataAnnotation));
        Assert.Null(relationshipBuilder.IsOwnership(true, ConfigurationSource.DataAnnotation));
        Assert.Null(relationshipBuilder.OnDelete(DeleteBehavior.ClientSetNull, ConfigurationSource.DataAnnotation));
        Assert.Null(
            relationshipBuilder.DependentEntityType(
                relationshipBuilder.Metadata.PrincipalEntityType, ConfigurationSource.DataAnnotation));
        Assert.Null(
            relationshipBuilder.HasNavigation(
                (string)null,
                pointsToPrincipal: true,
                ConfigurationSource.DataAnnotation));
        Assert.Null(
            relationshipBuilder.HasNavigation(
                (string)null,
                pointsToPrincipal: false,
                ConfigurationSource.DataAnnotation));
    }

    [ConditionalFact]
    public void Existing_facets_are_configured_explicitly()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var foreignKey = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata, ConfigurationSource.Explicit).Metadata;

        foreignKey.UpdatePropertiesConfigurationSource(ConfigurationSource.Explicit);
        foreignKey.UpdatePrincipalKeyConfigurationSource(ConfigurationSource.Explicit);
        foreignKey.UpdatePrincipalEndConfigurationSource(ConfigurationSource.Explicit);

        foreignKey.SetDependentToPrincipal(Order.CustomerProperty, ConfigurationSource.Explicit);
        foreignKey.SetPrincipalToDependent(Customer.OrdersProperty, ConfigurationSource.Explicit);
        foreignKey.IsUnique = false;
        foreignKey.IsRequired = false;
        foreignKey.IsRequiredDependent = false;
        foreignKey.IsOwnership = false;
        foreignKey.DeleteBehavior = DeleteBehavior.Cascade;

        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPropertiesConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalKeyConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalEndConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetDependentToPrincipalConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalToDependentConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsUniqueConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsRequiredConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsRequiredDependentConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsOwnershipConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetDeleteBehaviorConfigurationSource());
    }

    [ConditionalFact]
    public void Read_only_facets_are_configured_explicitly_by_default()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var customerKeyBuilder = customerEntityBuilder.PrimaryKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var foreignKey = orderEntityBuilder.Metadata.AddForeignKey(
            new[]
            {
                orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
            },
            customerKeyBuilder.Metadata,
            customerEntityBuilder.Metadata,
            ConfigurationSource.Explicit,
            ConfigurationSource.Explicit);

        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPropertiesConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalKeyConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalEndConfigurationSource());
    }

    [ConditionalFact]
    public void ForeignKey_returns_same_instance_for_same_properties()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder
            .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention)
            .HasForeignKey(
                new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

        Assert.NotNull(relationshipBuilder);
        Assert.Same(
            relationshipBuilder, orderEntityBuilder
                .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention)
                .HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation)
                .HasPrincipalKey(relationshipBuilder.Metadata.PrincipalKey.Properties, ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void ForeignKey_uses_same_relationship_if_conflicting_properties_configured_with_lower_source()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder
            .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.DataAnnotation)
            .HasForeignKey(new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

        Assert.NotNull(relationshipBuilder);
        Assert.Same(
            relationshipBuilder, orderEntityBuilder
                .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.DataAnnotation)
                .HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));

        Assert.Single(orderEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void ForeignKey_can_be_set_independently_from_requiredness()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);
        relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);

        var nullableId = orderEntityBuilder.Property(typeof(int?), "NullableId", ConfigurationSource.Explicit);
        relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { nullableId.Metadata.Name }, ConfigurationSource.Convention);
        Assert.False(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsRequired);
        Assert.Equal(
            new[] { nullableId.Metadata.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention);
        Assert.False(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsRequired);
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));
    }

    [ConditionalFact]
    public void ForeignKey_overrides_incompatible_lower_or_equal_source_principal_key()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);
        relationshipBuilder = relationshipBuilder.HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

        relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention);
        Assert.Equal(
            new[] { Customer.IdProperty.Name },
            relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        Assert.Null(relationshipBuilder.HasForeignKey(new[] { Order.CustomerUniqueProperty }, ConfigurationSource.Convention));
        Assert.Equal(
            new[] { Customer.IdProperty.Name },
            relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        relationshipBuilder = relationshipBuilder.HasForeignKey(
            new[] { Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation);
        Assert.NotEqual(
            new[] { Customer.IdProperty.Name },
            relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
        Assert.Equal(
            new[] { Order.CustomerUniqueProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));
    }

    [ConditionalFact]
    public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_non_shadow()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention)
            .HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

        relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { "ShadowCustomerId" }, ConfigurationSource.Convention);

        var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
        Assert.NotNull(shadowProperty);
        Assert.True(shadowProperty.IsShadowProperty());
        Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
    }

    [ConditionalFact]
    public void ForeignKey_creates_shadow_properties_if_principal_entity_has_no_PK()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention)
            .HasForeignKey(new[] { "ShadowCustomerId", "ShadowCustomerUnique" }, ConfigurationSource.Convention);

        var shadowProperty1 = relationshipBuilder.Metadata.Properties.First();
        Assert.True(shadowProperty1.IsShadowProperty());
        Assert.Equal("ShadowCustomerId", shadowProperty1.Name);

        var shadowProperty2 = relationshipBuilder.Metadata.Properties.Last();
        Assert.True(shadowProperty2.IsShadowProperty());
        Assert.Equal("ShadowCustomerUnique", shadowProperty2.Name);

        Assert.Null(customerEntityBuilder.Metadata.FindPrimaryKey());
        Assert.Equal(2, relationshipBuilder.Metadata.PrincipalKey.Properties.Count);
    }

    [ConditionalFact]
    public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_shadow()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention)
            .HasForeignKey(new[] { "ShadowCustomerId" }, ConfigurationSource.Convention);

        var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
        Assert.True(shadowProperty.IsShadowProperty());
        Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
    }

    [ConditionalFact]
    public void PrincipalKey_does_not_return_same_instance_for_same_properties()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = customerEntityBuilder.HasRelationship(orderEntityBuilder.Metadata, ConfigurationSource.DataAnnotation)
            .HasPrincipalKey(
                new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

        Assert.NotNull(relationshipBuilder);
        Assert.NotSame(
            relationshipBuilder, customerEntityBuilder.HasRelationship(orderEntityBuilder.Metadata, ConfigurationSource.DataAnnotation)
                .HasPrincipalKey(
                    new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation));

        Assert.Equal(2, customerEntityBuilder.Metadata.GetForeignKeys().Count());
    }

    [ConditionalFact]
    public void PrincipalKey_overrides_incompatible_lower_or_equal_source_dependent_properties()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention)
            .HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation)
            .HasPrincipalKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
        Assert.Equal(
            new[] { Customer.IdProperty.Name, Customer.UniqueProperty.Name },
            relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        Assert.Null(relationshipBuilder.HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention));
        Assert.Equal(
            new[] { Customer.IdProperty.Name, Customer.UniqueProperty.Name },
            relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        relationshipBuilder = relationshipBuilder.HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);
        Assert.Equal(
            new[] { Customer.IdProperty.Name },
            relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
        Assert.NotEqual(
            new[] { Order.CustomerUniqueProperty.Name, Order.CustomerUniqueProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_Unique()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);
        Assert.False(relationshipBuilder.Metadata.IsUnique);

        relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
        Assert.True(relationshipBuilder.Metadata.IsUnique);

        relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.DataAnnotation);
        Assert.False(relationshipBuilder.Metadata.IsUnique);

        Assert.Null(relationshipBuilder.IsUnique(true, ConfigurationSource.Convention));
        Assert.False(relationshipBuilder.Metadata.IsUnique);
    }

    [ConditionalFact]
    public void Can_only_override_existing_Unique_value_explicitly()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var customerKeyBuilder = customerEntityBuilder.PrimaryKey(
            new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var foreignKey = orderEntityBuilder.Metadata.AddForeignKey(
            new[]
            {
                orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
            },
            customerKeyBuilder.Metadata,
            customerEntityBuilder.Metadata,
            ConfigurationSource.Explicit,
            ConfigurationSource.Explicit);
        foreignKey.IsUnique = true;

        Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsUniqueConfigurationSource());

        var relationshipBuilder = orderEntityBuilder
            .HasRelationship(customerEntityBuilder.Metadata, foreignKey.Properties, ConfigurationSource.Convention);
        Assert.Same(foreignKey, relationshipBuilder.Metadata);

        relationshipBuilder = relationshipBuilder
            .HasPrincipalKey(customerKeyBuilder.Metadata.Properties, ConfigurationSource.Convention);
        Assert.Same(foreignKey, relationshipBuilder.Metadata);
        Assert.True(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsUnique);

        Assert.Null(relationshipBuilder.IsUnique(false, ConfigurationSource.Convention));
        Assert.True(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsUnique);

        relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
        Assert.NotNull(relationshipBuilder);
        Assert.True(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsUnique);

        relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.Explicit);
        Assert.NotNull(relationshipBuilder);
        Assert.False(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsUnique);
    }

    [ConditionalFact]
    public void Unique_overrides_incompatible_lower_or_equal_source_principalToDependent()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata, nameof(Order.Customer), nameof(Customer.NotCollectionOrders),
            ConfigurationSource.DataAnnotation);
        Assert.True(relationshipBuilder.Metadata.IsUnique);

        relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
        Assert.True(relationshipBuilder.Metadata.IsUnique);
        Assert.NotNull(relationshipBuilder.Metadata.PrincipalToDependent);

        Assert.Null(relationshipBuilder.IsUnique(false, ConfigurationSource.Convention));
        Assert.True(relationshipBuilder.Metadata.IsUnique);
        Assert.NotNull(relationshipBuilder.Metadata.PrincipalToDependent);

        relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.DataAnnotation);
        Assert.False(relationshipBuilder.Metadata.IsUnique);
        Assert.Null(relationshipBuilder.Metadata.PrincipalToDependent);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_Required()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);
        Assert.False(relationshipBuilder.Metadata.IsRequired);

        relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
        Assert.True(relationshipBuilder.Metadata.IsRequired);

        relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);
        Assert.False(relationshipBuilder.Metadata.IsRequired);

        Assert.Null(relationshipBuilder.IsRequired(true, ConfigurationSource.Convention));
        Assert.False(relationshipBuilder.Metadata.IsRequired);
    }

    [ConditionalFact]
    public void Can_set_Required_independently_from_nullability()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var pk = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit)
            .Metadata;
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var customerIdProperty = orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;
        var customerUniqueProperty = orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata;
        var fk = orderEntityBuilder.Metadata.AddForeignKey(
            new[] { customerIdProperty, customerUniqueProperty },
            pk,
            customerEntityBuilder.Metadata,
            ConfigurationSource.Explicit,
            ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata, fk.Properties, ConfigurationSource.Explicit);
        relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.Convention);
        Assert.False(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsRequired);
        Assert.False(customerIdProperty.IsNullable);
        Assert.True(customerUniqueProperty.IsNullable);

        relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
        Assert.True(((IReadOnlyForeignKey)relationshipBuilder.Metadata).IsRequired);
        Assert.False(customerIdProperty.IsNullable);
        Assert.True(customerUniqueProperty.IsNullable);

        relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.Explicit);
        Assert.NotNull(relationshipBuilder);
        fk = relationshipBuilder.Metadata;
        Assert.False(fk.IsRequired);
        Assert.False(customerIdProperty.IsNullable);
        Assert.True(customerUniqueProperty.IsNullable);
        Assert.Same(customerIdProperty, fk.Properties[0]);
        Assert.Same(customerUniqueProperty, fk.Properties[1]);
    }

    [ConditionalFact]
    public void Can_set_Required_false_on_non_nullable_properties()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);
        relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);
        Assert.False(relationshipBuilder.Metadata.IsRequired);
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
        Assert.True(relationshipBuilder.Metadata.IsRequired);
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.Convention);
        Assert.False(relationshipBuilder.Metadata.IsRequired);
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));

        relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);
        Assert.False(relationshipBuilder.Metadata.IsRequired);
        Assert.Equal(
            new[] { Order.CustomerIdProperty.Name },
            relationshipBuilder.Metadata.Properties.Select(p => p.Name));
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_RequiredDependent()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);
        Assert.False(relationshipBuilder.Metadata.IsRequiredDependent);

        relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
        relationshipBuilder = relationshipBuilder.IsRequiredDependent(true, ConfigurationSource.Convention);
        Assert.True(relationshipBuilder.Metadata.IsRequiredDependent);

        relationshipBuilder = relationshipBuilder.IsRequiredDependent(false, ConfigurationSource.DataAnnotation);
        Assert.False(relationshipBuilder.Metadata.IsRequiredDependent);

        Assert.Null(relationshipBuilder.IsRequiredDependent(true, ConfigurationSource.Convention));
        Assert.False(relationshipBuilder.Metadata.IsRequiredDependent);
    }

    [ConditionalFact]
    public void IsRequiredDependent_throws_when_ambiguous()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);

        relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);

        Assert.Equal(
            CoreStrings.AmbiguousEndRequiredDependent(
                "{'CustomerTempId'}",
                nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    relationshipBuilder.IsRequiredDependent(true, ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_Ownership()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit, shouldBeOwned: true);

        var relationshipBuilder = orderEntityBuilder.HasRelationship(
            customerEntityBuilder.Metadata, null, nameof(Customer.Orders), ConfigurationSource.Convention);
        Assert.False(relationshipBuilder.Metadata.IsOwnership);

        relationshipBuilder = relationshipBuilder.IsOwnership(true, ConfigurationSource.Convention);
        Assert.True(relationshipBuilder.Metadata.IsOwnership);

        relationshipBuilder = relationshipBuilder.IsOwnership(false, ConfigurationSource.DataAnnotation);
        Assert.False(relationshipBuilder.Metadata.IsOwnership);

        Assert.Null(relationshipBuilder.IsOwnership(true, ConfigurationSource.Convention));
        Assert.False(relationshipBuilder.Metadata.IsOwnership);
    }

    [ConditionalFact]
    public void HasRelationship_throws_when_incompatible_navigations()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.PrincipalEndIncompatibleNavigations(
                "Customer.Orders",
                "Order.Customer",
                nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    customerEntityBuilder.HasRelationship(
                        orderEntityBuilder.Metadata, nameof(Customer.Orders), nameof(Order.Customer), ConfigurationSource.Convention,
                        setTargetAsPrincipal: true)).Message);
    }

    [ConditionalFact]
    public void Can_only_invert_lower_or_equal_source()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder
            .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

        relationshipBuilder = relationshipBuilder.HasEntityTypes(
            relationshipBuilder.Metadata.DeclaringEntityType,
            relationshipBuilder.Metadata.PrincipalEntityType,
            ConfigurationSource.DataAnnotation);
        Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

        relationshipBuilder = relationshipBuilder.HasPrincipalKey(
                orderEntityBuilder.Metadata.GetKeys().Single().Properties,
                ConfigurationSource.Convention)
            .HasForeignKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

        Assert.Null(
            relationshipBuilder.DependentEntityType(
                relationshipBuilder.Metadata.PrincipalEntityType, ConfigurationSource.DataAnnotation));

        Assert.Equal(
            CoreStrings.EntityTypesNotInRelationship(
                relationshipBuilder.Metadata.PrincipalEntityType.DisplayName(),
                relationshipBuilder.Metadata.PrincipalEntityType.DisplayName(),
                relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                relationshipBuilder.Metadata.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => relationshipBuilder.DependentEntityType(
                    relationshipBuilder.Metadata.PrincipalEntityType, ConfigurationSource.Explicit)).Message);

        Assert.Null(
            relationshipBuilder.PrincipalEntityType(
                relationshipBuilder.Metadata.DeclaringEntityType, ConfigurationSource.DataAnnotation));

        Assert.Equal(
            CoreStrings.EntityTypesNotInRelationship(
                relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                relationshipBuilder.Metadata.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => relationshipBuilder.PrincipalEntityType(
                    relationshipBuilder.Metadata.DeclaringEntityType, ConfigurationSource.Explicit)).Message);

        Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
        Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.PrincipalEntityType);

        Assert.Null(
            relationshipBuilder.HasEntityTypes(
                relationshipBuilder.Metadata.DeclaringEntityType,
                relationshipBuilder.Metadata.PrincipalEntityType,
                ConfigurationSource.Convention));
        Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

        relationshipBuilder = relationshipBuilder.HasEntityTypes(
            relationshipBuilder.Metadata.DeclaringEntityType,
            relationshipBuilder.Metadata.PrincipalEntityType,
            ConfigurationSource.DataAnnotation);
        Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
    }

    [ConditionalFact]
    public void Can_invert_one_to_many()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder
            .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.DataAnnotation);

        relationshipBuilder = relationshipBuilder.HasEntityTypes(
            relationshipBuilder.Metadata.DeclaringEntityType,
            relationshipBuilder.Metadata.PrincipalEntityType,
            ConfigurationSource.Convention);

        Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
        Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.PrincipalEntityType);
    }

    [ConditionalFact]
    public void Inverting_to_keyless_throws()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit)
            .HasNoKey(ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder
            .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.Convention);

        Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

        Assert.Equal(
            CoreStrings.PrincipalKeylessType(
                nameof(Order), nameof(Customer), nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    relationshipBuilder.HasEntityTypes(
                        relationshipBuilder.Metadata.DeclaringEntityType,
                        relationshipBuilder.Metadata.PrincipalEntityType,
                        ConfigurationSource.DataAnnotation)).Message);
    }

    [ConditionalFact]
    public void Can_lift_self_referencing_relationships()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var specialOrderEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        specialOrderEntityBuilder.HasBaseType(orderEntityBuilder.Metadata, ConfigurationSource.Explicit);
        var relationshipBuilder = specialOrderEntityBuilder
            .HasRelationship(specialOrderEntityBuilder.Metadata, ConfigurationSource.Convention).HasNavigation(
                nameof(SpecialOrder.SpecialOrder),
                pointsToPrincipal: false,
                ConfigurationSource.Explicit);

        relationshipBuilder = relationshipBuilder.PrincipalEntityType(
            orderEntityBuilder.Metadata, ConfigurationSource.DataAnnotation);

        Assert.Single(specialOrderEntityBuilder.Metadata.GetForeignKeys());
        Assert.Same(specialOrderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
        Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.PrincipalEntityType);
        Assert.Null(relationshipBuilder.Metadata.DependentToPrincipal);
        Assert.Equal(nameof(SpecialOrder.SpecialOrder), relationshipBuilder.Metadata.PrincipalToDependent.Name);
    }

    [ConditionalFact]
    public void Can_add_navigations_to_higher_source_foreign_key()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var foreignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.DataAnnotation);

        var relationship = foreignKeyBuilder.HasNavigation(
            Order.CustomerProperty.Name,
            pointsToPrincipal: true,
            ConfigurationSource.Convention);
        relationship = relationship.HasNavigation(
            Customer.OrdersProperty.Name,
            pointsToPrincipal: false,
            ConfigurationSource.Convention);

        Assert.Same(relationship.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
        Assert.Same(relationship.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);
    }

    [ConditionalFact]
    public void Can_only_override_existing_conflicting_navigations_explicitly()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        var existingForeignKey = dependentEntityBuilder.Metadata.AddForeignKey(
            new[]
            {
                dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Explicit).Metadata,
                dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Explicit).Metadata
            },
            principalEntityBuilder.Metadata.FindPrimaryKey(),
            principalEntityBuilder.Metadata,
            ConfigurationSource.Explicit,
            ConfigurationSource.Explicit);
        existingForeignKey.SetPrincipalToDependent(Customer.OrdersProperty, ConfigurationSource.Explicit);
        existingForeignKey.SetDependentToPrincipal(Order.CustomerProperty, ConfigurationSource.Explicit);
        Assert.Equal(ConfigurationSource.Explicit, existingForeignKey.GetDependentToPrincipalConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, existingForeignKey.GetPrincipalToDependentConfigurationSource());

        var newForeignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.Convention);

        newForeignKeyBuilder = newForeignKeyBuilder.HasNavigation(
            Order.CustomerProperty.Name,
            pointsToPrincipal: true,
            ConfigurationSource.Convention);
        Assert.Same(existingForeignKey, newForeignKeyBuilder.Metadata);

        newForeignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.Convention);
        newForeignKeyBuilder = newForeignKeyBuilder.HasNavigation(
            Customer.OrdersProperty.Name,
            pointsToPrincipal: false,
            ConfigurationSource.Explicit);
        Assert.Same(existingForeignKey, newForeignKeyBuilder.Metadata);

        Assert.Equal(Customer.OrdersProperty.Name, newForeignKeyBuilder.Metadata.PrincipalToDependent.Name);
        Assert.Equal(Order.CustomerProperty.Name, newForeignKeyBuilder.Metadata.DependentToPrincipal.Name);
    }

    [ConditionalFact]
    public void Can_override_lower_or_equal_source_conflicting_navigation()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var conflictingForeignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.DataAnnotation);
        conflictingForeignKeyBuilder = conflictingForeignKeyBuilder.HasNavigation(
            Order.CustomerProperty.Name,
            pointsToPrincipal: true,
            ConfigurationSource.DataAnnotation);
        conflictingForeignKeyBuilder = conflictingForeignKeyBuilder.HasNavigation(
            Customer.OrdersProperty.Name,
            pointsToPrincipal: false,
            ConfigurationSource.DataAnnotation);

        var foreignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.Convention);

        Assert.Same(
            conflictingForeignKeyBuilder,
            foreignKeyBuilder.HasNavigation(
                Order.CustomerProperty.Name,
                pointsToPrincipal: true,
                ConfigurationSource.Convention));
        Assert.Same(
            conflictingForeignKeyBuilder,
            foreignKeyBuilder.HasNavigation(
                Customer.OrdersProperty.Name,
                pointsToPrincipal: false,
                ConfigurationSource.Convention));

        Assert.Same(
            conflictingForeignKeyBuilder.Metadata,
            dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
        Assert.Same(
            conflictingForeignKeyBuilder.Metadata,
            principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);

        var newForeignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
        newForeignKeyBuilder = newForeignKeyBuilder.HasNavigation(
            Order.CustomerProperty.Name,
            pointsToPrincipal: true,
            ConfigurationSource.DataAnnotation);
        newForeignKeyBuilder = newForeignKeyBuilder.HasNavigation(
            Customer.OrdersProperty.Name,
            pointsToPrincipal: false,
            ConfigurationSource.DataAnnotation);

        Assert.Equal(Order.CustomerIdProperty.Name, newForeignKeyBuilder.Metadata.Properties.Single().Name);
        Assert.Same(
            newForeignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
        Assert.Same(
            newForeignKeyBuilder.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);
        Assert.Same(newForeignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
    }

    [ConditionalFact]
    public void Navigation_to_principal_uses_same_relationship_if_conflicting_navigation_configured_with_lower_source()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = customerEntityBuilder
            .HasRelationship(orderEntityBuilder.Metadata, ConfigurationSource.Explicit).HasNavigation(
                nameof(Order.Customer),
                pointsToPrincipal: false,
                ConfigurationSource.Convention);

        Assert.NotNull(relationshipBuilder);
        Assert.Same(
            relationshipBuilder, orderEntityBuilder
                .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.DataAnnotation).HasNavigation(
                    nameof(Order.Customer),
                    pointsToPrincipal: true,
                    ConfigurationSource.DataAnnotation));

        Assert.Single(customerEntityBuilder.Metadata.GetForeignKeys());
        Assert.Empty(orderEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Navigation_to_principal_does_not_change_uniqueness_for_relationship()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var foreignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.Convention);
        foreignKeyBuilder.IsUnique(false, ConfigurationSource.Convention);
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            nameof(Order.Customer),
            pointsToPrincipal: true,
            ConfigurationSource.Convention);
        Assert.NotNull(foreignKeyBuilder.Metadata.DependentToPrincipal);
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);
    }

    [ConditionalFact]
    public void Navigation_to_dependent_uses_same_relationship_if_conflicting_navigation_configured_with_lower_source()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = orderEntityBuilder
            .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.DataAnnotation).HasNavigation(
                nameof(Customer.Orders),
                pointsToPrincipal: false,
                ConfigurationSource.Convention);

        Assert.NotNull(relationshipBuilder);
        Assert.Same(
            relationshipBuilder, orderEntityBuilder
                .HasRelationship(customerEntityBuilder.Metadata, ConfigurationSource.DataAnnotation).HasNavigation(
                    nameof(Customer.Orders),
                    pointsToPrincipal: false,
                    ConfigurationSource.DataAnnotation));
        Assert.Single(orderEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Navigation_to_dependent_changes_uniqueness_for_relationship_of_lower_or_equal_source()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);

        var foreignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.Convention);
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            nameof(Customer.NotCollectionOrders),
            pointsToPrincipal: false,
            ConfigurationSource.DataAnnotation);
        Assert.Equal(nameof(Customer.NotCollectionOrders), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
        Assert.True(foreignKeyBuilder.Metadata.IsUnique);

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            nameof(Customer.AmbiguousOrder),
            pointsToPrincipal: false,
            ConfigurationSource.DataAnnotation);
        Assert.Equal(nameof(Customer.AmbiguousOrder), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
        Assert.True(foreignKeyBuilder.Metadata.IsUnique);

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            nameof(Customer.Orders),
            pointsToPrincipal: false,
            ConfigurationSource.DataAnnotation);
        Assert.Equal(nameof(Customer.Orders), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            nameof(Customer.AmbiguousOrder),
            pointsToPrincipal: false,
            ConfigurationSource.DataAnnotation);
        Assert.Equal(nameof(Customer.AmbiguousOrder), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);
    }

    [ConditionalFact]
    public void Navigation_to_dependent_does_not_change_uniqueness_for_relationship_of_higher_source()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var foreignKeyBuilder = dependentEntityBuilder.HasRelationship(
            typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
            ConfigurationSource.Convention);
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);

        foreignKeyBuilder = foreignKeyBuilder.IsUnique(false, ConfigurationSource.DataAnnotation);
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);

        Assert.Null(
            foreignKeyBuilder.HasNavigation(
                nameof(Customer.NotCollectionOrders),
                pointsToPrincipal: false,
                ConfigurationSource.Convention));
        Assert.False(foreignKeyBuilder.Metadata.IsUnique);

        foreignKeyBuilder = foreignKeyBuilder.HasNavigation(
            "Orders",
            pointsToPrincipal: false,
            ConfigurationSource.Convention);
        Assert.Equal("Orders", foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
    }

    private InternalModelBuilder CreateInternalModelBuilder()
        => new(new Model());

    private class Order
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
        public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
        public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");
        public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty("Customer");

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Guid? CustomerUnique { get; set; }
        public Customer Customer { get; set; }

        public Order OrderCustomer { get; set; }
        public SpecialOrder SpecialOrder { get; set; }
    }

    private class SpecialOrder : Order, IEnumerable<Order>
    {
        public IEnumerator<Order> GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public string Specialty { get; set; }
    }

    private class Customer
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
        public static readonly PropertyInfo UniqueProperty = typeof(Customer).GetProperty("Unique");
        public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty("Orders");

        public int Id { get; set; }
        public Guid Unique { get; set; }
        public string Name { get; set; }
        public string Mane { get; set; }
        public ICollection<Order> Orders { get; set; }
        public SpecialOrder AmbiguousOrder { get; set; }
        public IEnumerable<Order> EnumerableOrders { get; set; }
        public ICollection<SpecialOrder> SpecialOrders { get; set; }
        public Order NotCollectionOrders { get; set; }
    }
}
