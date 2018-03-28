// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalRelationshipBuilderTest
    {
        [Fact]
        public void Facets_are_configured_with_the_specified_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder, ConfigurationSource.Convention);

            var fk = relationshipBuilder.Metadata;
            Assert.Equal(ConfigurationSource.Convention, fk.GetConfigurationSource());
            Assert.Null(fk.GetForeignKeyPropertiesConfigurationSource());
            Assert.Null(fk.GetPrincipalKeyConfigurationSource());
            Assert.Null(fk.GetPrincipalEndConfigurationSource());
            Assert.Null(fk.GetDependentToPrincipalConfigurationSource());
            Assert.Null(fk.GetPrincipalToDependentConfigurationSource());
            Assert.Null(fk.GetIsRequiredConfigurationSource());
            Assert.Null(fk.GetIsOwnershipConfigurationSource());
            Assert.Null(fk.GetIsUniqueConfigurationSource());
            Assert.Null(fk.GetDeleteBehaviorConfigurationSource());

            relationshipBuilder = relationshipBuilder.PrincipalEntityType(principalEntityBuilder, ConfigurationSource.Explicit)
                .HasPrincipalKey(key.Metadata.Properties, ConfigurationSource.Explicit)
                .DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Explicit)
                .PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Explicit)
                .IsUnique(false, ConfigurationSource.Explicit)
                .IsRequired(false, ConfigurationSource.Explicit)
                .IsOwnership(false, ConfigurationSource.Explicit)
                .DeleteBehavior(DeleteBehavior.Cascade, ConfigurationSource.Explicit)
                .HasForeignKey(
                    new[]
                    {
                        dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                        dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                    }, ConfigurationSource.Explicit);

            Assert.Null(relationshipBuilder.HasForeignKey(new[] { Order.IdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));
            var shadowId = principalEntityBuilder.Property("ShadowId", typeof(int), ConfigurationSource.Convention).Metadata;
            Assert.Null(relationshipBuilder.HasPrincipalKey(new[] { shadowId.Name, Customer.UniqueProperty.Name }, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.IsUnique(true, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.IsRequired(true, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.IsOwnership(true, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.DeleteBehavior(DeleteBehavior.ClientSetNull, ConfigurationSource.DataAnnotation));
            Assert.Null(
                relationshipBuilder.DependentEntityType(
                    relationshipBuilder.Metadata.PrincipalEntityType, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.DependentToPrincipal((string)null, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.PrincipalToDependent((string)null, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Existing_facets_are_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKey = dependentEntityBuilder.Relationship(
                principalEntityBuilder, ConfigurationSource.Explicit).Metadata;

            foreignKey.UpdateForeignKeyPropertiesConfigurationSource(ConfigurationSource.Explicit);
            foreignKey.UpdatePrincipalKeyConfigurationSource(ConfigurationSource.Explicit);
            foreignKey.UpdatePrincipalEndConfigurationSource(ConfigurationSource.Explicit);

            foreignKey.HasDependentToPrincipal(Order.CustomerProperty);
            foreignKey.HasPrincipalToDependent(Customer.OrdersProperty);
            foreignKey.IsUnique = false;
            foreignKey.IsRequired = false;
            foreignKey.IsOwnership = false;
            foreignKey.DeleteBehavior = DeleteBehavior.Cascade;

            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetForeignKeyPropertiesConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalKeyConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalEndConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetDependentToPrincipalConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalToDependentConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsUniqueConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsRequiredConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsOwnershipConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetDeleteBehaviorConfigurationSource());
        }

        [Fact]
        public void Read_only_facets_are_configured_explicitly_by_default()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var customerKeyBuilder = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKey = orderEntityBuilder.Metadata.AddForeignKey(
                new[]
                {
                    orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                customerKeyBuilder.Metadata,
                customerEntityBuilder.Metadata);

            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetForeignKeyPropertiesConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalKeyConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetPrincipalEndConfigurationSource());
        }

        [Fact]
        public void ForeignKey_returns_same_instance_for_same_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder
                .Relationship(customerEntityBuilder, ConfigurationSource.Convention)
                .HasForeignKey(new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(
                relationshipBuilder, orderEntityBuilder
                    .Relationship(customerEntityBuilder, ConfigurationSource.Convention)
                    .HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation)
                    .HasPrincipalKey(relationshipBuilder.Metadata.PrincipalKey.Properties, ConfigurationSource.Convention));
        }

        [Fact]
        public void ForeignKey_creates_new_relationship_if_conflicting_properties_configured_with_lower_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder
                .Relationship(customerEntityBuilder, ConfigurationSource.DataAnnotation)
                .HasForeignKey(new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(relationshipBuilder);
            Assert.NotSame(
                relationshipBuilder, orderEntityBuilder
                    .Relationship(customerEntityBuilder, ConfigurationSource.DataAnnotation)
                    .HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));

            Assert.Equal(2, orderEntityBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
        public void ForeignKey_overrides_incompatible_lower_or_equal_source_required()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention);
            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);

            var nullableId = orderEntityBuilder.Property("NullableId", typeof(int?), ConfigurationSource.Explicit);
            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { nullableId.Metadata.Name }, ConfigurationSource.Convention);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(
                new[] { nullableId.Metadata.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            Assert.Null(relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(
                new[] { nullableId.Metadata.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(
                new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));
        }

        [Fact]
        public void ForeignKey_overrides_incompatible_lower_or_equal_source_principal_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention);
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

            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation);
            Assert.NotEqual(
                new[] { Customer.IdProperty.Name },
                relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { Order.CustomerUniqueProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));
        }

        [Fact]
        public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_non_shadow()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention)
                .HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { "ShadowCustomerId" }, ConfigurationSource.Convention);

            var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
            Assert.NotNull(shadowProperty);
            Assert.True(((IProperty)shadowProperty).IsShadowProperty);
            Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
        }

        [Fact]
        public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_properties_count_mismatch()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention)
                .HasForeignKey(new[] { "ShadowCustomerId", "ShadowCustomerUnique" }, ConfigurationSource.Convention);

            var shadowProperty1 = relationshipBuilder.Metadata.Properties.First();
            Assert.True(shadowProperty1.IsShadowProperty);
            Assert.Equal("ShadowCustomerId", shadowProperty1.Name);

            var shadowProperty2 = relationshipBuilder.Metadata.Properties.Last();
            Assert.True(shadowProperty2.IsShadowProperty);
            Assert.Equal("ShadowCustomerUnique", shadowProperty2.Name);

            Assert.Null(customerEntityBuilder.Metadata.FindPrimaryKey());
            Assert.Equal(2, relationshipBuilder.Metadata.PrincipalKey.Properties.Count);
        }

        [Fact]
        public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_shadow()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention)
                .HasForeignKey(new[] { "ShadowCustomerId" }, ConfigurationSource.Convention);

            var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
            Assert.True(shadowProperty.IsShadowProperty);
            Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
        }

        [Fact]
        public void PrincipalKey_does_not_return_same_instance_for_same_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = customerEntityBuilder.Relationship(orderEntityBuilder, ConfigurationSource.DataAnnotation)
                .HasPrincipalKey(new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.NotSame(
                relationshipBuilder, customerEntityBuilder.Relationship(orderEntityBuilder, ConfigurationSource.DataAnnotation)
                    .HasPrincipalKey(new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation));

            Assert.Equal(2, customerEntityBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
        public void PrincipalKey_overrides_incompatible_lower_or_equal_source_dependent_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention)
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

        [Fact]
        public void Can_only_override_lower_or_equal_source_Unique()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention);
            Assert.False(relationshipBuilder.Metadata.IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
            Assert.True(relationshipBuilder.Metadata.IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.DataAnnotation);
            Assert.False(relationshipBuilder.Metadata.IsUnique);

            Assert.Null(relationshipBuilder.IsUnique(true, ConfigurationSource.Convention));
            Assert.False(relationshipBuilder.Metadata.IsUnique);
        }

        [Fact]
        public void Can_only_override_existing_Unique_value_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var customerKeyBuilder = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKey = orderEntityBuilder.Metadata.AddForeignKey(
                new[]
                {
                    orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                customerKeyBuilder.Metadata,
                customerEntityBuilder.Metadata);
            foreignKey.IsUnique = true;

            Assert.Equal(ConfigurationSource.Explicit, foreignKey.GetIsUniqueConfigurationSource());

            var relationshipBuilder = orderEntityBuilder
                .HasForeignKey(customerEntityBuilder, foreignKey.Properties, ConfigurationSource.Convention);
            Assert.NotSame(foreignKey, relationshipBuilder.Metadata);

            relationshipBuilder = relationshipBuilder
                .HasPrincipalKey(customerKeyBuilder.Metadata.Properties, ConfigurationSource.Convention);
            Assert.Same(foreignKey, relationshipBuilder.Metadata);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            Assert.Null(relationshipBuilder.IsUnique(false, ConfigurationSource.Convention));
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.Explicit);
            Assert.NotNull(relationshipBuilder);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
        }

        [Fact]
        public void Unique_overrides_incompatible_lower_or_equal_source_principalToDependent()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, nameof(Order.Customer), nameof(Customer.NotCollectionOrders), ConfigurationSource.DataAnnotation);
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

        [Fact]
        public void Can_only_override_lower_or_equal_source_Required()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention);
            Assert.False(relationshipBuilder.Metadata.IsRequired);

            relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
            Assert.True(relationshipBuilder.Metadata.IsRequired);

            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);
            Assert.False(relationshipBuilder.Metadata.IsRequired);

            Assert.Null(relationshipBuilder.IsRequired(true, ConfigurationSource.Convention));
            Assert.False(relationshipBuilder.Metadata.IsRequired);
        }

        [Fact]
        public void Can_only_override_existing_Required_value_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var pk = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit).Metadata;
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var customerIdProperty = orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;
            var customerUniqueProperty = orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata;
            var fk = orderEntityBuilder.Metadata.AddForeignKey(
                new[] { customerIdProperty, customerUniqueProperty },
                pk,
                customerEntityBuilder.Metadata);
            fk.IsRequired = true;

            Assert.Equal(ConfigurationSource.Explicit, fk.GetIsRequiredConfigurationSource());

            var relationshipBuilder = orderEntityBuilder.HasForeignKey(customerEntityBuilder, fk.Properties, ConfigurationSource.Explicit);
            Assert.Null(relationshipBuilder.IsRequired(false, ConfigurationSource.Convention));
            Assert.True(fk.IsRequired);
            Assert.False(customerIdProperty.IsNullable);
            Assert.False(customerUniqueProperty.IsNullable);

            relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.False(customerIdProperty.IsNullable);
            Assert.False(customerUniqueProperty.IsNullable);

            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.Explicit);
            Assert.NotNull(relationshipBuilder);
            fk = relationshipBuilder.Metadata;
            Assert.False(fk.IsRequired);
            Assert.False(customerIdProperty.IsNullable);
            Assert.True(customerUniqueProperty.IsNullable);
            Assert.Same(customerIdProperty, fk.Properties[0]);
            Assert.Same(customerUniqueProperty, fk.Properties[1]);
        }

        [Fact]
        public void Required_overrides_incompatible_lower_or_equal_source_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention);
            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);
            Assert.True(relationshipBuilder.Metadata.IsRequired);
            Assert.Equal(
                new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
            Assert.True(relationshipBuilder.Metadata.IsRequired);
            Assert.Equal(
                new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            Assert.Null(relationshipBuilder.IsRequired(false, ConfigurationSource.Convention));
            Assert.True(relationshipBuilder.Metadata.IsRequired);
            Assert.Equal(
                new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);
            Assert.False(relationshipBuilder.Metadata.IsRequired);
            Assert.NotEqual(
                new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_Ownership()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention);
            Assert.False(relationshipBuilder.Metadata.IsOwnership);

            relationshipBuilder = relationshipBuilder.IsOwnership(true, ConfigurationSource.Convention);
            Assert.True(relationshipBuilder.Metadata.IsOwnership);

            relationshipBuilder = relationshipBuilder.IsOwnership(false, ConfigurationSource.DataAnnotation);
            Assert.False(relationshipBuilder.Metadata.IsOwnership);

            Assert.Null(relationshipBuilder.IsOwnership(true, ConfigurationSource.Convention));
            Assert.False(relationshipBuilder.Metadata.IsOwnership);
        }

        [Fact]
        public void Can_only_invert_lower_or_equal_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder
                .Relationship(customerEntityBuilder, ConfigurationSource.Convention);

            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.RelatedEntityTypes(
                relationshipBuilder.Metadata.DeclaringEntityType,
                relationshipBuilder.Metadata.PrincipalEntityType,
                ConfigurationSource.DataAnnotation);
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.HasPrincipalKey(
                    orderEntityBuilder.Metadata.GetKeys().Single().Properties,
                    ConfigurationSource.Convention)
                .HasForeignKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Null(relationshipBuilder.DependentEntityType(relationshipBuilder.Metadata.PrincipalEntityType, ConfigurationSource.DataAnnotation));

            Assert.Equal(
                CoreStrings.EntityTypesNotInRelationship(
                    relationshipBuilder.Metadata.PrincipalEntityType.DisplayName(),
                    relationshipBuilder.Metadata.PrincipalEntityType.DisplayName(),
                    relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                    relationshipBuilder.Metadata.PrincipalEntityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => relationshipBuilder.DependentEntityType(relationshipBuilder.Metadata.PrincipalEntityType, ConfigurationSource.Explicit)).Message);

            Assert.Null(relationshipBuilder.PrincipalEntityType(relationshipBuilder.Metadata.DeclaringEntityType, ConfigurationSource.DataAnnotation));

            Assert.Equal(
                CoreStrings.EntityTypesNotInRelationship(
                    relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                    relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                    relationshipBuilder.Metadata.DeclaringEntityType.DisplayName(),
                    relationshipBuilder.Metadata.PrincipalEntityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => relationshipBuilder.PrincipalEntityType(relationshipBuilder.Metadata.DeclaringEntityType, ConfigurationSource.Explicit)).Message);

            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.PrincipalEntityType);

            Assert.Null(
                relationshipBuilder.RelatedEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType,
                    ConfigurationSource.Convention));
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.RelatedEntityTypes(
                relationshipBuilder.Metadata.DeclaringEntityType,
                relationshipBuilder.Metadata.PrincipalEntityType,
                ConfigurationSource.DataAnnotation);
            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
        }

        [Fact]
        public void Can_invert_one_to_many()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder
                .Relationship(customerEntityBuilder, ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.DataAnnotation);

            relationshipBuilder = relationshipBuilder.RelatedEntityTypes(
                relationshipBuilder.Metadata.DeclaringEntityType,
                relationshipBuilder.Metadata.PrincipalEntityType,
                ConfigurationSource.Convention);

            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.PrincipalEntityType);
        }

        [Fact]
        public void Can_lift_self_referencing_relationships()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var specialOrderEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            specialOrderEntityBuilder.HasBaseType(orderEntityBuilder.Metadata, ConfigurationSource.Explicit);
            var relationshipBuilder = specialOrderEntityBuilder
                .Relationship(specialOrderEntityBuilder, ConfigurationSource.Convention)
                .PrincipalToDependent(nameof(SpecialOrder.SpecialOrder), ConfigurationSource.Explicit);

            relationshipBuilder = relationshipBuilder.PrincipalEntityType(
                orderEntityBuilder.Metadata, ConfigurationSource.DataAnnotation);

            Assert.Equal(1, specialOrderEntityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Same(specialOrderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.PrincipalEntityType);
            Assert.Null(relationshipBuilder.Metadata.DependentToPrincipal);
            Assert.Equal(nameof(SpecialOrder.SpecialOrder), relationshipBuilder.Metadata.PrincipalToDependent.Name);
        }

        [Fact]
        public void Can_add_navigations_to_higher_source_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            var relationship = foreignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Convention);
            relationship = relationship.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Convention);

            Assert.Same(relationship.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
            Assert.Same(relationship.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);
        }

        [Fact]
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
                principalEntityBuilder.Metadata);
            existingForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);
            existingForeignKey.HasDependentToPrincipal(Order.CustomerProperty);
            Assert.Equal(ConfigurationSource.Explicit, existingForeignKey.GetDependentToPrincipalConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, existingForeignKey.GetPrincipalToDependentConfigurationSource());

            var newForeignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            newForeignKeyBuilder = newForeignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Convention);
            Assert.Same(existingForeignKey, newForeignKeyBuilder.Metadata);

            newForeignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            newForeignKeyBuilder = newForeignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Explicit);
            Assert.Same(existingForeignKey, newForeignKeyBuilder.Metadata);

            Assert.Equal(Customer.OrdersProperty.Name, newForeignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.Equal(Order.CustomerProperty.Name, newForeignKeyBuilder.Metadata.DependentToPrincipal.Name);
        }

        [Fact]
        public void Can_override_lower_or_equal_source_conflicting_navigation()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var conflictingForeignKeyBuilder = dependentEntityBuilder.HasForeignKey(
                typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            conflictingForeignKeyBuilder = conflictingForeignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation);
            conflictingForeignKeyBuilder = conflictingForeignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation);

            var foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.Same(conflictingForeignKeyBuilder, foreignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.Same(conflictingForeignKeyBuilder, foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Convention));

            Assert.Same(conflictingForeignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
            Assert.Same(conflictingForeignKeyBuilder.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);

            var newForeignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
            newForeignKeyBuilder = newForeignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation);
            newForeignKeyBuilder = newForeignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Equal(Order.CustomerIdProperty.Name, newForeignKeyBuilder.Metadata.Properties.Single().Name);
            Assert.Same(newForeignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
            Assert.Same(newForeignKeyBuilder.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);
            Assert.Same(newForeignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
        }

        [Fact]
        public void Navigation_to_principal_creates_new_relationship_if_conflicting_navigation_configured_with_lower_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = customerEntityBuilder
                .Relationship(orderEntityBuilder, ConfigurationSource.Explicit)
                .PrincipalToDependent(nameof(Order.Customer), ConfigurationSource.Convention);

            Assert.NotNull(relationshipBuilder);
            Assert.NotSame(
                relationshipBuilder, orderEntityBuilder
                    .Relationship(customerEntityBuilder, ConfigurationSource.DataAnnotation)
                    .DependentToPrincipal(nameof(Order.Customer), ConfigurationSource.DataAnnotation));

            Assert.Equal(1, customerEntityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Equal(1, orderEntityBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
        public void Navigation_to_principal_does_not_change_uniqueness_for_relationship()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            foreignKeyBuilder.IsUnique(false, ConfigurationSource.Convention);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.DependentToPrincipal(nameof(Order.Customer), ConfigurationSource.Convention);
            Assert.NotNull(foreignKeyBuilder.Metadata.DependentToPrincipal);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);
        }

        [Fact]
        public void Navigation_to_dependent_creates_new_relationship_if_conflicting_navigation_configured_with_lower_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder
                .Relationship(customerEntityBuilder, ConfigurationSource.DataAnnotation)
                .PrincipalToDependent(nameof(Customer.Orders), ConfigurationSource.Convention);

            Assert.NotNull(relationshipBuilder);
            Assert.NotSame(
                relationshipBuilder, orderEntityBuilder
                    .Relationship(customerEntityBuilder, ConfigurationSource.DataAnnotation)
                    .PrincipalToDependent(nameof(Customer.Orders), ConfigurationSource.DataAnnotation));
            Assert.Equal(2, orderEntityBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
        public void Navigation_to_dependent_changes_uniqueness_for_relationship_of_lower_or_equal_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);

            var foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(nameof(Customer.NotCollectionOrders), ConfigurationSource.DataAnnotation);
            Assert.Equal(nameof(Customer.NotCollectionOrders), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.True(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(nameof(Customer.AmbiguousOrder), ConfigurationSource.DataAnnotation);
            Assert.Equal(nameof(Customer.AmbiguousOrder), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.True(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(nameof(Customer.Orders), ConfigurationSource.DataAnnotation);
            Assert.Equal(nameof(Customer.Orders), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(nameof(Customer.AmbiguousOrder), ConfigurationSource.DataAnnotation);
            Assert.Equal(nameof(Customer.AmbiguousOrder), foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);
        }

        [Fact]
        public void Navigation_to_dependent_does_not_change_uniqueness_for_relationship_of_higher_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.IsUnique(false, ConfigurationSource.DataAnnotation);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            Assert.Null(foreignKeyBuilder.PrincipalToDependent(nameof(Customer.NotCollectionOrders), ConfigurationSource.Convention));
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent("Orders", ConfigurationSource.Convention);
            Assert.Equal("Orders", foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
        }

        private InternalModelBuilder CreateInternalModelBuilder() => new InternalModelBuilder(new Model());

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

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
}
