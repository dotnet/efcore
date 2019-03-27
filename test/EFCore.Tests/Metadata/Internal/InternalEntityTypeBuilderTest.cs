// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalEntityTypeBuilderTest
    {
        [Fact]
        public void Relationship_returns_same_instance_for_same_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Explicit)
                .DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Explicit)
                .PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Explicit);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(
                relationshipBuilder,
                dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention)
                    .DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Convention)
                    .PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_add_relationship_if_principal_entity_has_no_PK()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                null,
                Customer.OrdersProperty.Name,
                ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            var pkProperty = relationshipBuilder.Metadata.PrincipalKey.Properties.Single();
            Assert.Equal("TempId", pkProperty.Name);
            Assert.True(pkProperty.IsShadowProperty);
            var fkProperty = relationshipBuilder.Metadata.Properties.Single();
            Assert.Equal(nameof(Customer) + pkProperty.Name, fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
        }

        [Fact]
        public void Can_add_relationship_if_principal_entity_PK_name_contains_principal_entity_name()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.Property("CustomerId", typeof(string), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { "CustomerId" }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                (string)null,
                null,
                ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            var pkProperty = relationshipBuilder.Metadata.PrincipalKey.Properties.Single();
            Assert.Equal("CustomerId", pkProperty.Name);
            Assert.True(pkProperty.IsShadowProperty);
            var fkProperty = relationshipBuilder.Metadata.Properties.Single();
            Assert.Equal("CustomerId1", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
        }

        [Fact]
        public void Can_add_relationship_if_principal_entity_PK_name_contains_principal_navigation_name()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.Property("CustomerId", typeof(string), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { "CustomerId" }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.Ignore("CustomerId", ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                nameof(Order.Customer),
                null,
                ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            var pkProperty = relationshipBuilder.Metadata.PrincipalKey.Properties.Single();
            Assert.Equal("CustomerId", pkProperty.Name);
            Assert.True(pkProperty.IsShadowProperty);
            var fkProperty = relationshipBuilder.Metadata.Properties.Single();
            Assert.Equal("CustomerId1", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
        }

        [Fact]
        public void Can_add_relationship_if_navigation_to_dependent_ignored_at_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));

            Assert.NotNull(
                dependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    null,
                    Customer.OrdersProperty.Name,
                    ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_add_relationship_if_navigation_to_principal_ignored_at_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));

            Assert.NotNull(
                dependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    Order.CustomerProperty.Name,
                    null,
                    ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_non_shadow()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.HasForeignKey(
                customerEntityBuilder.Metadata.Name,
                new[] { "ShadowCustomerId" },
                ConfigurationSource.Convention);

            var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
            Assert.NotNull(shadowProperty);
            Assert.True(((IProperty)shadowProperty).IsShadowProperty);
            Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
        }

        [Fact]
        public void ForeignKey_creates_shadow_properties_if_principal_type_does_not_have_primary_key()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.HasForeignKey(customerEntityBuilder.Metadata.Name, new[] { "ShadowCustomerId" }, ConfigurationSource.Convention);

            var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
            Assert.True(shadowProperty.IsShadowProperty);
            Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());

            Assert.Null(customerEntityBuilder.Metadata.FindPrimaryKey());
            Assert.Equal(1, relationshipBuilder.Metadata.PrincipalKey.Properties.Count);
        }

        [Fact]
        public void ForeignKey_does_not_create_shadow_properties_if_corresponding_principal_key_properties_has_different_count()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.Property("ShadowPrimaryKey", typeof(int), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(
                new List<string>
                {
                    "ShadowPrimaryKey"
                }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.NoPropertyType("ShadowCustomerId", nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () => orderEntityBuilder.HasForeignKey(
                        customerEntityBuilder.Metadata.Name,
                        new[] { "ShadowCustomerId", "ShadowCustomerUnique" },
                        ConfigurationSource.Convention)).Message);
        }

        [Fact]
        public void ForeignKey_creates_shadow_properties_if_corresponding_principal_key_property_is_shadow()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.Property("ShadowPrimaryKey", typeof(int), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(
                new List<string>
                {
                    "ShadowPrimaryKey"
                }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.HasForeignKey(
                customerEntityBuilder.Metadata.Name,
                new[] { "ShadowCustomerId" },
                ConfigurationSource.Convention);

            var shadowProperty = orderEntityBuilder.Metadata.FindProperty("ShadowCustomerId");
            Assert.NotNull(shadowProperty);
            Assert.True(((IProperty)shadowProperty).IsShadowProperty);
            Assert.Equal(shadowProperty, relationshipBuilder.Metadata.Properties.First());
        }

        [Fact]
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
                derivedEntityBuilder.HasForeignKey(
                        principalEntityBuilder.Metadata.Name,
                        new[] { Order.IdProperty.Name },
                        primaryKey,
                        ConfigurationSource.DataAnnotation)
                    .IsUnique(true, ConfigurationSource.DataAnnotation));

            entityBuilder.HasForeignKey(
                principalEntityBuilder.Metadata.Name,
                new[] { Order.IdProperty.Name },
                ConfigurationSource.Convention);

            Assert.Equal(2, derivedEntityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Equal(1, entityBuilder.Metadata.GetForeignKeys().Count());

            var foreignKeyBuilder = entityBuilder.HasForeignKey(
                principalEntityBuilder.Metadata.Name,
                new[] { Order.IdProperty.Name },
                primaryKey,
                ConfigurationSource.Convention);

            Assert.Equal(2, entityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredForeignKeys());
            Assert.Equal(ConfigurationSource.DataAnnotation, foreignKeyBuilder.Metadata.GetConfigurationSource());
            Assert.True(foreignKeyBuilder.Metadata.IsUnique);
        }

        [Fact]
        public void ForeignKey_returns_inherited_foreign_key_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var principalKey = principalEntityBuilder.HasKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit).Metadata;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Convention);
            dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder.Metadata.Name,
                new[] { Order.IdProperty.Name },
                principalKey,
                ConfigurationSource.Convention);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention);

            var relationshipBuilder = derivedDependentEntityBuilder.HasForeignKey(
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

        [Fact]
        public void ForeignKey_matches_existing_foreign_key_if_same_or_no_principal_key_specified_or_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var primaryKey = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention).Metadata;
            var shadowKeyPropety = principalEntityBuilder.Property("ShadowId", typeof(int), ConfigurationSource.Convention);
            var alternateKey = principalEntityBuilder.HasKey(new[] { shadowKeyPropety.Metadata.Name }, ConfigurationSource.Convention).Metadata;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Convention);

            var fk1 = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder.Metadata.Name,
                new[] { Order.IdProperty.Name },
                ConfigurationSource.DataAnnotation).Metadata;
            var newFk1 = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder.Metadata.Name,
                new[] { Order.IdProperty.Name },
                alternateKey,
                ConfigurationSource.Explicit).Metadata;

            var fk2 = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder.Metadata.Name,
                new[] { Order.IdProperty.Name },
                ConfigurationSource.DataAnnotation).Metadata;
            var newFk2 = dependentEntityBuilder.HasForeignKey(
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

        [Fact]
        public void Promotes_derived_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);

            dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                ConfigurationSource.DataAnnotation);

            var basePrincipalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var baseDependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            principalEntityBuilder.HasBaseType(basePrincipalEntityBuilder.Metadata, ConfigurationSource.Explicit);
            dependentEntityBuilder.HasBaseType(baseDependentEntityBuilder.Metadata, ConfigurationSource.Explicit);

            Assert.Empty(baseDependentEntityBuilder.Metadata.GetForeignKeys());

            var relationship = baseDependentEntityBuilder.Relationship(
                basePrincipalEntityBuilder,
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

        [Fact]
        public void Returns_inherited_relationship_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);
            var basePrincipalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var baseDependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            principalEntityBuilder.HasBaseType(basePrincipalEntityBuilder.Metadata, ConfigurationSource.Explicit);
            dependentEntityBuilder.HasBaseType(baseDependentEntityBuilder.Metadata, ConfigurationSource.Explicit);

            baseDependentEntityBuilder.Relationship(
                basePrincipalEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                ConfigurationSource.DataAnnotation);

            Assert.Null(
                dependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    Order.CustomerProperty.Name,
                    null,
                    ConfigurationSource.Convention));

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                Order.CustomerProperty.Name,
                null,
                ConfigurationSource.DataAnnotation);

            Assert.Same(baseDependentEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
            Assert.Null(relationshipBuilder.Metadata.PrincipalToDependent);
            Assert.Equal(Order.CustomerProperty.Name, relationshipBuilder.Metadata.DependentToPrincipal.Name);
            Assert.Empty(principalEntityBuilder.Metadata.GetNavigations());
            Assert.Same(relationshipBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
        }

        [Fact]
        public void Does_not_add_index_on_foreign_key_properties_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                    { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
                ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);

            Assert.Empty(dependentEntityBuilder.Metadata.GetIndexes());
        }

        [Fact]
        public void Can_create_foreign_key_on_mix_of_inherited_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityTypeBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityTypeBuilder.PrimaryKey(
                new List<PropertyInfo>
                {
                    Customer.IdProperty
                }, ConfigurationSource.Explicit);

            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.Property(SpecialOrder.SpecialtyProperty, ConfigurationSource.Convention);
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            var relationshipBuilder = derivedEntityBuilder
                .HasForeignKey(
                    customerEntityTypeBuilder.Metadata.Name,
                    new[] { Order.IdProperty.Name, SpecialOrder.SpecialtyProperty.Name },
                    ConfigurationSource.DataAnnotation);

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Same(derivedEntityBuilder.Metadata.GetDeclaredForeignKeys().Single(), relationshipBuilder.Metadata);
            Assert.Collection(
                relationshipBuilder.Metadata.Properties,
                t1 => Assert.Same(entityBuilder.Metadata, t1.DeclaringEntityType),
                t2 => Assert.Same(derivedEntityBuilder.Metadata, t2.DeclaringEntityType));
        }

        [Fact]
        public void Can_only_remove_lower_or_equal_source_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationshipBuilder);

            Assert.Null(dependentEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.DataAnnotation, dependentEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(
                new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
                dependentEntityBuilder.Metadata.GetProperties().Select(p => p.Name));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Removing_relationship_removes_unused_contained_shadow_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = dependentEntityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    shadowProperty.Metadata
                },
                ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);

            Assert.Equal(ConfigurationSource.Convention, dependentEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Same(Order.CustomerIdProperty.Name, dependentEntityBuilder.Metadata.GetProperties().Single().Name);
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Removing_relationship_removes_unused_conventional_index()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                    { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
                ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);

            Assert.Equal(ConfigurationSource.Convention, dependentEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Empty(dependentEntityBuilder.Metadata.GetIndexes());
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact] // TODO: Add test if the index is being used by another FK when support for multiple FK on same set of properties is added
        public void Removing_relationship_does_not_remove_conventional_index_if_in_use()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                    { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
                ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            dependentEntityBuilder.HasIndex(new[] { Order.CustomerIdProperty }, ConfigurationSource.Explicit);

            Assert.Equal(ConfigurationSource.Convention, dependentEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, dependentEntityBuilder.Metadata.GetIndexes().Count());
            Assert.Equal(Order.CustomerIdProperty.Name, dependentEntityBuilder.Metadata.GetIndexes().First().Properties.First().Name);
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
        {
            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.PrimaryKey(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.HasIndex(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.HasForeignKey(
                    typeof(Customer).FullName,
                    new[] { entityBuilder.Property("Shadow2", typeof(int), ConfigurationSource.Convention).Metadata.Name, property.Name },
                    ConfigurationSource.Convention));

            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Explicit));
        }

        private void Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = dependentEntityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);
            Assert.NotNull(shadowConfig(dependentEntityBuilder, shadowProperty.Metadata));

            var relationshipBuilder = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    shadowProperty.Metadata
                },
                ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);

            Assert.Equal(ConfigurationSource.Convention, dependentEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, dependentEntityBuilder.Metadata.GetProperties().Count(p => p.Name == shadowProperty.Metadata.Name));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys().Where(foreignKey => foreignKey.Properties.SequenceEqual(relationshipBuilder.Metadata.Properties)));
        }

        [Fact]
        public void Index_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit);

            Assert.NotNull(indexBuilder);
            Assert.Same(indexBuilder, entityBuilder.HasIndex(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Index_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(indexBuilder);
            Assert.Same(indexBuilder, entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));
        }

        [Fact]
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

        [Fact]
        public void Can_promote_index_to_base_with_facets()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
            derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
            derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation).IsUnique(true, ConfigurationSource.Convention);

            var indexBuilder = entityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention);
            Assert.Same(indexBuilder.Metadata.Properties.Single(), entityBuilder.Metadata.FindProperty(Order.IdProperty.Name));
            Assert.Same(indexBuilder.Metadata, entityBuilder.Metadata.FindIndex(indexBuilder.Metadata.Properties.Single()));
            Assert.True(indexBuilder.Metadata.IsUnique);
            Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredIndexes());
        }

        [Fact]
        public void Can_configure_inherited_index()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Convention);
            entityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.Explicit);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            var indexBuilder = derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder.Metadata.GetIndexes().Single(), indexBuilder.Metadata);
            Assert.True(indexBuilder.IsUnique(true, ConfigurationSource.Convention));
            Assert.True(indexBuilder.Metadata.IsUnique);
        }

        [Fact]
        public void Can_create_index_on_inherited_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Convention);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            var indexBuilder = derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.Empty(entityBuilder.Metadata.GetIndexes());
            Assert.Same(derivedEntityBuilder.Metadata.GetDeclaredIndexes().Single(), indexBuilder.Metadata);
            Assert.Same(entityBuilder.Metadata, indexBuilder.Metadata.Properties.First().DeclaringEntityType);
        }

        [Fact]
        public void Can_create_index_on_mix_of_inherited_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Convention);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.Property(SpecialOrder.SpecialtyProperty.Name, typeof(string), ConfigurationSource.Convention);
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            var indexBuilder = derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name, SpecialOrder.SpecialtyProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.Empty(entityBuilder.Metadata.GetIndexes());
            Assert.Same(derivedEntityBuilder.Metadata.GetDeclaredIndexes().Single(), indexBuilder.Metadata);
            Assert.Collection(
                indexBuilder.Metadata.Properties,
                t1 => Assert.Same(entityBuilder.Metadata, t1.DeclaringEntityType),
                t2 => Assert.Same(derivedEntityBuilder.Metadata, t2.DeclaringEntityType));
        }

        [Fact]
        public void Index_returns_null_for_ignored_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

            Assert.Null(entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Index_returns_null_for_ignored_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Null(entityBuilder.HasIndex(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_only_remove_lower_or_equal_source_index()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var index = entityBuilder.HasIndex(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.NotNull(index);

            Assert.Null(entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.DataAnnotation, entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
            Assert.Empty(entityBuilder.Metadata.GetIndexes());
        }

        [Fact]
        public void Removing_index_removes_unused_contained_shadow_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);

            var index = entityBuilder.HasIndex(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(index);

            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
            Assert.Empty(entityBuilder.Metadata.GetIndexes());
        }

        [Fact]
        public void Removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
        {
            Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.PrimaryKey(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.HasIndex(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.HasForeignKey(
                    typeof(Customer).FullName,
                    new[] { Order.CustomerIdProperty.Name, property.Name },
                    ConfigurationSource.Convention));

            Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.Property(((IProperty)property).Name, property.ClrType, ConfigurationSource.Explicit));
        }

        private void Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);
            Assert.NotNull(shadowConfig(entityBuilder, shadowProperty.Metadata));

            var index = entityBuilder.HasIndex(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(index);

            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, entityBuilder.Metadata.GetProperties().Count(p => p.Name == shadowProperty.Metadata.Name));
            Assert.Empty(entityBuilder.Metadata.GetIndexes().Where(i => i.Properties.SequenceEqual(index.Metadata.Properties)));
        }

        [Fact]
        public void Key_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.HasKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void Key_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.HasKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Key_sets_properties_to_required()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            entityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).IsRequired(false, ConfigurationSource.DataAnnotation);

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.Convention));

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));

            Assert.False(entityBuilder.Metadata.FindProperty(Order.CustomerUniqueProperty).IsNullable);

            Assert.False(
                entityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention)
                    .IsRequired(false, ConfigurationSource.Convention));
            Assert.True(
                entityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention)
                    .IsRequired(false, ConfigurationSource.DataAnnotation));

            Assert.True(entityBuilder.Metadata.FindProperty(Order.CustomerUniqueProperty).IsNullable);
            Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
        }

        [Fact]
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

        [Fact]
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
                        derivedEntityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation)).Message);
        }

        [Fact]
        public void Key_throws_if_conflicting_with_derived_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Explicit);
            var idProperty = dependentEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention).Metadata;
            idProperty.ValueGenerated = ValueGenerated.OnAdd;

            derivedDependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    Order.CustomerProperty.Name,
                    nameof(Customer.SpecialOrders),
                    ConfigurationSource.Explicit)
                .HasForeignKey(new[] { idProperty }, ConfigurationSource.Explicit);

            Assert.Null(dependentEntityBuilder.HasKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));

            Assert.Equal(
                CoreStrings.KeyPropertyInForeignKey(Order.IdProperty.Name, nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => dependentEntityBuilder.HasKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Key_throws_for_property_names_for_shadow_entity_type_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order).Name, ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.NoPropertyType(Order.IdProperty.Name, nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        entityBuilder.HasKey(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention)).Message);
        }

        [Fact]
        public void Key_works_for_property_names_for_shadow_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.CustomerIdProperty.Name, Order.CustomerIdProperty.PropertyType, ConfigurationSource.Convention);

            Assert.NotNull(entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetKeys().Single().Properties.Single().Name);
        }

        [Fact]
        public void Key_returns_null_for_ignored_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

            Assert.Null(entityBuilder.HasKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Key_returns_null_for_ignored_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Null(entityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_only_remove_lower_or_equal_source_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var key = entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.NotNull(key);

            Assert.Null(entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.DataAnnotation, entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
            Assert.Empty(entityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void Removing_key_removes_unused_contained_shadow_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);

            var key = entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(key);

            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetProperties().Single().Name);
            Assert.Empty(entityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void Removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
        {
            Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.PrimaryKey(
                    new[]
                    {
                        entityBuilder.Property("Shadow2", typeof(int), ConfigurationSource.Convention).Metadata.Name,
                        property.Name
                    }, ConfigurationSource.Convention));

            Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.HasIndex(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.HasForeignKey(
                    typeof(Customer).FullName,
                    new[] { Order.CustomerIdProperty.Name, property.Name },
                    ConfigurationSource.Convention));

            Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Explicit));
        }

        private void Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);

            var key = entityBuilder.HasKey(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(key);

            Assert.NotNull(shadowConfig(entityBuilder, shadowProperty.Metadata));
            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, entityBuilder.Metadata.GetProperties().Count(p => p.Name == shadowProperty.Metadata.Name));
            Assert.Empty(entityBuilder.Metadata.GetKeys().Where(foreignKey => foreignKey.Properties.SequenceEqual(key.Metadata.Properties)));
        }

        [Fact]
        public void PrimaryKey_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

            Assert.Same(entityBuilder.Metadata.FindPrimaryKey(), entityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void PrimaryKey_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.HasKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.Same(entityBuilder.Metadata.FindPrimaryKey(), entityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void PrimaryKey_throws_for_property_names_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.NoPropertyType(Customer.UniqueProperty.Name, nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        entityBuilder.PrimaryKey(new[] { Customer.UniqueProperty.Name }, ConfigurationSource.Convention)).Message);
        }

        [Fact]
        public void PrimaryKey_throws_for_property_names_for_shadow_entity_type_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order).Name, ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.NoPropertyType(Order.IdProperty.Name, nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention)).Message);
        }

        [Fact]
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
                        derivedEntityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation)).Message);
        }

        [Fact]
        public void PrimaryKey_works_for_property_names_for_shadow_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.CustomerIdProperty.Name, Order.CustomerIdProperty.PropertyType, ConfigurationSource.Convention);

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.FindPrimaryKey().Properties.Single().Name);
        }

        [Fact]
        public void PrimaryKey_returns_null_for_ignored_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void PrimaryKey_returns_null_for_ignored_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_primary_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            var compositeKeyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Convention);
            var simpleKeyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Convention);

            Assert.NotNull(simpleKeyBuilder);
            Assert.NotEqual(compositeKeyBuilder, simpleKeyBuilder);
            Assert.Equal(Order.IdProperty.Name, entityType.GetKeys().Single().Properties.Single().Name);

            var simpleKeyBuilder2 = entityBuilder.HasKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);
            Assert.Same(simpleKeyBuilder, simpleKeyBuilder2);

            var compositeKeyBuilder2 = entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Convention);
            Assert.NotNull(compositeKeyBuilder2);
            Assert.NotEqual(compositeKeyBuilder, compositeKeyBuilder2);
            Assert.Same(compositeKeyBuilder2.Metadata, entityBuilder.Metadata.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));
            Assert.Same(compositeKeyBuilder2.Metadata, entityBuilder.Metadata.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
        }

        [Fact]
        public void Can_only_override_existing_primary_key_explicitly()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            Assert.Null(entityType.GetPrimaryKeyConfigurationSource());

            entityType.SetPrimaryKey(new[] { entityType.GetOrAddProperty(Order.IdProperty), entityType.GetOrAddProperty(Order.CustomerIdProperty) });

            Assert.Equal(ConfigurationSource.Explicit, entityType.GetPrimaryKeyConfigurationSource());

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));

            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.FindPrimaryKey().Properties.Select(p => p.Name).ToArray());

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit));

            Assert.Equal(Order.IdProperty.Name, entityType.FindPrimaryKey().Properties.Single().Name);
        }

        [Fact]
        public void Changing_primary_key_removes_previously_referenced_primary_key_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            var fkProperty1 = dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;
            var fkProperty2 = dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata;

            dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.DataAnnotation)
                .HasPrincipalKey(keyBuilder.Metadata.Properties, ConfigurationSource.DataAnnotation)
                .HasForeignKey(new[] { fkProperty1, fkProperty2 }, ConfigurationSource.Explicit);

            keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Same(keyBuilder.Metadata, principalEntityBuilder.Metadata.FindPrimaryKey());
            var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Equal(new[] { fkProperty1, fkProperty2 }, fk.Properties);
        }

        [Fact]
        public void Changing_primary_key_removes_previously_referenced_key_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.DataAnnotation)
                .HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation);

            Assert.Null(principalEntityBuilder.Metadata.FindPrimaryKey());
            Assert.NotEqual(nameof(Customer.Id), dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey.Properties.First().Name);

            var keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey);
            Assert.Same(keyBuilder.Metadata, principalEntityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void Changing_primary_key_does_not_remove_previously_explicitly_referenced_key()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var existingKeyBuilder = principalEntityBuilder.HasKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Explicit)
                .HasPrincipalKey(existingKeyBuilder.Metadata.Properties, ConfigurationSource.Explicit);

            var keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);

            Assert.Same(existingKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey);
            Assert.Equal(2, principalEntityBuilder.Metadata.GetKeys().Count());
            Assert.Contains(keyBuilder.Metadata, principalEntityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void Property_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var propertyBuilder = entityBuilder.Property(Order.IdProperty, ConfigurationSource.Explicit);

            Assert.NotNull(propertyBuilder);
            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Explicit));
        }

        [Fact]
        public void Property_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var propertyBuilder = entityBuilder.Property(Order.IdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.NotNull(propertyBuilder);
            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Property_returns_same_instance_if_type_matches()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var propertyBuilder = entityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation);
            Assert.NotNull(propertyBuilder);

            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.DataAnnotation, typeConfigurationSource: ConfigurationSource.DataAnnotation));

            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Convention, typeConfigurationSource: null));

            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty.Name, null, ConfigurationSource.Convention, typeConfigurationSource: ConfigurationSource.Convention));

            Assert.Null(entityBuilder.Property(Order.IdProperty.Name, typeof(string), ConfigurationSource.Convention, typeConfigurationSource: ConfigurationSource.Convention));

            Assert.Equal(new[] { propertyBuilder.Metadata }, entityBuilder.GetActualProperties(new[] { propertyBuilder.Metadata }, null));
        }

        [Fact]
        public void Property_throws_for_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Explicit)
                .DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Explicit)
                .PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.PropertyCalledOnNavigation(nameof(Order.Customer), nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () => dependentEntityBuilder
                        .Property(Order.CustomerProperty, ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Cannot_add_shadow_property_without_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.NoPropertyType("Shadow", nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => entityBuilder.Property("Shadow", ConfigurationSource.DataAnnotation)).Message);
        }

        [Fact]
        public void Can_promote_property_to_base()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
            var derivedProperty = derivedEntityBuilder.Property("byte", typeof(int), ConfigurationSource.DataAnnotation);
            derivedProperty.IsConcurrencyToken(true, ConfigurationSource.Convention);
            derivedProperty.HasMaxLength(1, ConfigurationSource.Explicit);
            var derivedEntityBuilder2 = modelBuilder.Entity(typeof(BackOrder), ConfigurationSource.Convention);
            derivedEntityBuilder2.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);
            var derivedProperty2 = derivedEntityBuilder2.Property("byte", typeof(byte), ConfigurationSource.Convention);
            derivedProperty2.HasMaxLength(2, ConfigurationSource.Convention);

            var propertyBuilder = entityBuilder.Property("byte", typeof(int), ConfigurationSource.Convention);
            Assert.Same(propertyBuilder.Metadata, entityBuilder.Metadata.FindProperty("byte"));
            Assert.False(entityBuilder.Ignore("byte", ConfigurationSource.Convention));
            Assert.Empty(derivedEntityBuilder.Metadata.GetDeclaredProperties());
            Assert.Empty(derivedEntityBuilder2.Metadata.GetDeclaredProperties());
            Assert.Equal(typeof(int), propertyBuilder.Metadata.ClrType);
            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
            Assert.Equal(1, propertyBuilder.Metadata.GetMaxLength());
        }

        [Fact]
        public void Can_configure_inherited_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(nameof(SpecialOrder.Specialty), typeof(int), ConfigurationSource.Explicit)
                .IsConcurrencyToken(false, ConfigurationSource.Convention);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            var propertyBuilder = derivedEntityBuilder.Property(nameof(SpecialOrder.Specialty), typeof(int), ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder.Metadata.FindProperty(nameof(SpecialOrder.Specialty)), propertyBuilder.Metadata);
            Assert.True(propertyBuilder.IsConcurrencyToken(true, ConfigurationSource.Convention));
            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
            Assert.Same(typeof(int), propertyBuilder.Metadata.ClrType);

            Assert.Null(derivedEntityBuilder.Property(nameof(SpecialOrder.Specialty), typeof(string), ConfigurationSource.DataAnnotation));

            Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
            Assert.NotNull(entityBuilder.PrimaryKey(new[] { propertyBuilder.Metadata.Name }, ConfigurationSource.Explicit));
            propertyBuilder = derivedEntityBuilder.Property(nameof(SpecialOrder.Specialty), typeof(string), ConfigurationSource.Explicit);

            Assert.Same(typeof(string), propertyBuilder.Metadata.ClrType);
            Assert.Same(entityBuilder.Metadata, propertyBuilder.Metadata.DeclaringEntityType);
            Assert.NotNull(entityBuilder.Metadata.FindPrimaryKey());
        }

        [Fact]
        public void Can_reuniquify_temporary_properties_with_same_names()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(nameof(Customer), ConfigurationSource.Explicit);
            var principalKey = principalEntityBuilder.HasKey(
                new[]
                {
                    principalEntityBuilder.Property("Id", typeof(int), ConfigurationSource.Explicit).Metadata,
                    principalEntityBuilder.Property("AlternateId", typeof(int), ConfigurationSource.Explicit).Metadata
                }, ConfigurationSource.Explicit).Metadata;
            var dependentEntityBuilder = modelBuilder.Entity(nameof(Order), ConfigurationSource.Explicit);
            var foreignKey = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                {
                    dependentEntityBuilder.Property("AlternateId", typeof(int), ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property("AlternateId1", typeof(int), ConfigurationSource.Convention).Metadata
                },
                principalKey, ConfigurationSource.Convention).Metadata;

            Assert.True(dependentEntityBuilder.ShouldReuniquifyTemporaryProperties(
                foreignKey.Properties, principalKey.Properties, true, ""));

            var newFkProperties = foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>) null, ConfigurationSource.Convention)
                .Metadata.Properties;

            Assert.Equal("CustomerId", newFkProperties[0].Name);
            Assert.Equal("CustomerAlternateId", newFkProperties[1].Name);
            Assert.Equal(2, dependentEntityBuilder.Metadata.GetProperties().Count());
        }

        [Fact]
        public void Can_reuniquify_temporary_properties_with_same_names_different_types_in_two_passes()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(nameof(Customer), ConfigurationSource.Explicit);
            var principalKey = principalEntityBuilder.HasKey(
                new[]
                {
                    principalEntityBuilder.Property("Id", typeof(int), ConfigurationSource.Explicit).Metadata,
                    principalEntityBuilder.Property("AlternateId", typeof(Guid), ConfigurationSource.Explicit).Metadata
                }, ConfigurationSource.Explicit).Metadata;
            var dependentEntityBuilder = modelBuilder.Entity(nameof(Order), ConfigurationSource.Explicit);
            var foreignKey = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                {
                    dependentEntityBuilder.Property("AlternateId", typeof(int), ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property("Id", typeof(Guid), ConfigurationSource.Convention).Metadata
                },
                principalKey, ConfigurationSource.Convention).Metadata;

            Assert.True(dependentEntityBuilder.ShouldReuniquifyTemporaryProperties(
                foreignKey.Properties, principalKey.Properties, true, ""));

            var newFkProperties = foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention)
                .Metadata.Properties;

            Assert.Equal("CustomerId", newFkProperties[0].Name);
            Assert.Equal("CustomerAlternateId", newFkProperties[1].Name);
            Assert.Equal(2, dependentEntityBuilder.Metadata.GetProperties().Count());
        }

        [Fact]
        public void Can_reuniquify_temporary_properties_avoiding_unmapped_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(nameof(Customer), ConfigurationSource.Explicit);
            var principalKey = principalEntityBuilder.HasKey(
                new[]
                {
                    principalEntityBuilder.Property("Id", typeof(int), ConfigurationSource.Explicit).Metadata,
                    principalEntityBuilder.Property("Unique", typeof(int), ConfigurationSource.Explicit).Metadata
                }, ConfigurationSource.Explicit).Metadata;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKey = dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                {
                    dependentEntityBuilder.Property("Id1", typeof(int), ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property("Id2", typeof(int), ConfigurationSource.Convention).Metadata
                },
                principalKey, ConfigurationSource.Convention).Metadata;

            Assert.True(dependentEntityBuilder.ShouldReuniquifyTemporaryProperties(
                foreignKey.Properties, principalKey.Properties, true, "Customer"));

            var newFkProperties = foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention)
                .Metadata.Properties;

            Assert.Equal("CustomerId1", newFkProperties[0].Name);
            Assert.Equal("CustomerUnique1", newFkProperties[1].Name);
            Assert.Equal(2, dependentEntityBuilder.Metadata.GetProperties().Count());
        }

        [Fact]
        public void Can_ignore_same_or_lower_source_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));

            Assert.Null(entityType.FindProperty(Order.IdProperty.Name));
            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));
            Assert.Null(entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.DataAnnotation));

            Assert.NotNull(entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Explicit));
        }

        [Fact]
        public void Cannot_ignore_higher_source_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            Assert.NotNull(entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.DataAnnotation));
            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.NotNull(entityType.FindProperty(Order.IdProperty.Name));

            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(entityType.FindProperty(Order.IdProperty.Name));

            Assert.NotNull(entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Explicit));
            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(entityType.FindProperty(Order.IdProperty.Name));

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));
            Assert.Null(entityType.FindProperty(Order.IdProperty.Name));
        }

        [Fact]
        public void Can_ignore_existing_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;
            var property = entityType.AddProperty(Order.IdProperty.Name, typeof(int));

            Assert.Same(property, entityBuilder.Property(Order.IdProperty.Name, ConfigurationSource.Convention).Metadata);

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));
            Assert.Null(entityType.FindProperty(Order.IdProperty.Name));
        }

        [Theory]
        [InlineData(ConfigurationSource.Explicit, ConfigurationSource.Explicit)]
        [InlineData(ConfigurationSource.Explicit, ConfigurationSource.DataAnnotation)]
        [InlineData(ConfigurationSource.Explicit, ConfigurationSource.Convention)]
        [InlineData(ConfigurationSource.DataAnnotation, ConfigurationSource.Explicit)]
        [InlineData(ConfigurationSource.DataAnnotation, ConfigurationSource.DataAnnotation)]
        [InlineData(ConfigurationSource.DataAnnotation, ConfigurationSource.Convention)]
        [InlineData(ConfigurationSource.Convention, ConfigurationSource.Explicit)]
        [InlineData(ConfigurationSource.Convention, ConfigurationSource.DataAnnotation)]
        [InlineData(ConfigurationSource.Convention, ConfigurationSource.Convention)]
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
        {
            VerifyIgnoreMember(
                ignoredOnType, ignoreConfigurationSource, addConfigurationSource, ignoredFirst, setBaseFirst,
                et => et.Metadata.FindProperty(Order.CustomerIdProperty.Name) != null,
                et => et.Property(Order.CustomerIdProperty, addConfigurationSource) != null,
                et => et.Property(Order.CustomerIdProperty, ignoreConfigurationSource) != null,
                Order.CustomerIdProperty.Name);
        }

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
            modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            if (setBaseFirst)
            {
                ConfigureOrdersHierarchy(modelBuilder);
            }

            var ignoredEntityTypeBuilder = modelBuilder.Entity(ignoredOnType, ConfigurationSource.Convention);
            var addedEntityTypeBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            Assert.False(findMember(addedEntityTypeBuilder));

            var exceptionExpected = ignoredOnType == typeof(ExtraSpecialOrder);
            var expectedAdded = exceptionExpected
                                || (addConfigurationSource.Overrides(ignoreConfigurationSource)
                                    && (ignoreConfigurationSource != ConfigurationSource.Explicit
                                        || ignoredOnType != typeof(SpecialOrder)
                                        || ignoredFirst));
            var expectedIgnored = (ignoredOnType != typeof(SpecialOrder)
                                   || !expectedAdded)
                                  && !exceptionExpected;

            if (ignoredFirst)
            {
                Assert.True(ignoredEntityTypeBuilder.Ignore(memberToIgnore, ignoreConfigurationSource));
                Assert.Equal(expectedAdded || (!setBaseFirst && ignoredOnType != typeof(SpecialOrder)), addMember(addedEntityTypeBuilder));
            }
            else
            {
                Assert.True(addMember(addedEntityTypeBuilder));
                if (exceptionExpected
                    && ignoreConfigurationSource == ConfigurationSource.Explicit
                    && setBaseFirst)
                {
                    Assert.Equal(
                        CoreStrings.InheritedPropertyCannotBeIgnored(
                            memberToIgnore, typeof(ExtraSpecialOrder).ShortDisplayName(), typeof(SpecialOrder).ShortDisplayName()),
                        Assert.Throws<InvalidOperationException>(
                            () => ignoredEntityTypeBuilder.Ignore(memberToIgnore, ignoreConfigurationSource)).Message);
                    return;
                }

                Assert.Equal(
                    expectedIgnored
                    || (!setBaseFirst && (ignoreConfigurationSource == ConfigurationSource.Explicit || ignoredOnType != typeof(SpecialOrder))),
                    ignoredEntityTypeBuilder.Ignore(memberToIgnore, ignoreConfigurationSource));
            }

            if (!setBaseFirst)
            {
                ConfigureOrdersHierarchy(modelBuilder);
            }

            var validationConvention = new IgnoredMembersValidationConvention();
            if (exceptionExpected)
            {
                Assert.Equal(
                    CoreStrings.InheritedPropertyCannotBeIgnored(
                        memberToIgnore, typeof(ExtraSpecialOrder).ShortDisplayName(), typeof(SpecialOrder).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => validationConvention.Apply(modelBuilder)).Message);

                Assert.True(unignoreMember(ignoredEntityTypeBuilder));
            }

            validationConvention.Apply(modelBuilder);

            var modelValidator = InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<IModelValidator>();

            modelValidator.Validate(modelBuilder.Metadata);

            Assert.Equal(expectedIgnored, ignoredEntityTypeBuilder.Metadata.FindDeclaredIgnoredMemberConfigurationSource(memberToIgnore) == ignoreConfigurationSource);
            Assert.Equal(expectedAdded, findMember(addedEntityTypeBuilder));
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

        [Fact]
        public void Can_ignore_service_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.ServiceProperty(Order.ContextProperty, ConfigurationSource.Explicit);

            Assert.False(entityBuilder.Ignore(nameof(Order.Context), ConfigurationSource.DataAnnotation));

            Assert.True(entityBuilder.Ignore(nameof(Order.Context), ConfigurationSource.Explicit));
            Assert.Empty(entityBuilder.Metadata.GetServiceProperties());
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_foreign_key_preserving_the_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder =
                dependentEntityBuilder.Relationship(principalEntityBuilder, (string)null, null, ConfigurationSource.DataAnnotation)
                    .HasForeignKey(
                        new[]
                        {
                            dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                            dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                        }, ConfigurationSource.DataAnnotation)
                    .HasPrincipalKey(key.Metadata.Properties, ConfigurationSource.DataAnnotation)
                    .IsUnique(true, ConfigurationSource.DataAnnotation)
                    .IsRequired(true, ConfigurationSource.DataAnnotation)
                    .DeleteBehavior(DeleteBehavior.Cascade, ConfigurationSource.DataAnnotation);
            var fk = relationshipBuilder.Metadata;

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

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

            Assert.NotNull(dependentEntityBuilder.HasForeignKey(principalEntityBuilder, newFk.Properties, ConfigurationSource.Convention));

            Assert.NotNull(dependentEntityBuilder.Metadata.GetForeignKeys().Where(foreignKey => foreignKey != newFk));
        }

        [Fact]
        public void Cannot_ignore_property_that_is_part_of_higher_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                ConfigurationSource.DataAnnotation);

            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Convention));

            Assert.NotEmpty(dependentEntityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.NotEmpty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_index()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.True(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.Empty(entityBuilder.Metadata.GetIndexes());
        }

        [Fact]
        public void Cannot_ignore_property_that_is_part_of_same_or_higher_source_index()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.HasIndex(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

            Assert.False(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.NotEmpty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.NotEmpty(entityBuilder.Metadata.GetIndexes());
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.True(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.Empty(entityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_principal_key_preserving_the_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var principalType = principalEntityBuilder.Metadata;
            var dependentType = dependentEntityBuilder.Metadata;

            var fk = dependentEntityBuilder.Relationship(
                    principalEntityBuilder,
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

            Assert.True(principalEntityBuilder.Ignore(Customer.UniqueProperty.Name, ConfigurationSource.DataAnnotation));

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

        [Fact]
        public void Cannot_ignore_property_that_is_part_of_same_or_higher_source_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

            Assert.False(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.NotEmpty(entityBuilder.Metadata.GetProperties().Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.NotEmpty(entityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void Navigation_returns_same_value()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.DataAnnotation);

            Assert.True(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Convention));

            foreignKeyBuilder = foreignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.False(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(dependentEntityBuilder.CanAddOrReplaceNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Convention));

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.False(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.CanAddOrReplaceNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            var newForeignKeyBuilder = dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention)
                .PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Convention);
            Assert.Same(foreignKeyBuilder, newForeignKeyBuilder);
            newForeignKeyBuilder = principalEntityBuilder.Relationship(dependentEntityBuilder, ConfigurationSource.Convention)
                .PrincipalToDependent(Order.CustomerProperty.Name, ConfigurationSource.Convention);
            Assert.Same(foreignKeyBuilder, newForeignKeyBuilder);

            Assert.Same(foreignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(dependentEntityBuilder.Metadata.GetKeys());
            Assert.Same(foreignKeyBuilder.Metadata.PrincipalKey, principalEntityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void Can_ignore_lower_or_equal_source_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            foreignKeyBuilder = foreignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation);
            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation);
            Assert.NotNull(foreignKeyBuilder);

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());

            foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.Null(foreignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.Null(foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));

            foreignKeyBuilder = foreignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Explicit);
            Assert.NotNull(foreignKeyBuilder);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Explicit);
            Assert.NotNull(foreignKeyBuilder);
        }

        [Fact]
        public void Cannot_ignore_higher_source_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.False(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.True(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));

            foreignKeyBuilder = foreignKeyBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Explicit);
            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Explicit);

            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.NotNull(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));

            Assert.Same(foreignKeyBuilder, foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));
            Assert.Null(foreignKeyBuilder.DependentToPrincipal((string)null, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_ignore_existing_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var property1 = dependentEntityBuilder.Metadata.AddProperty(Order.CustomerIdProperty.Name, typeof(int));
            var property2 = dependentEntityBuilder.Metadata.AddProperty(Order.CustomerUniqueProperty.Name, typeof(Guid?));
            var foreignKey = dependentEntityBuilder.Metadata.AddForeignKey(
                new[]
                {
                    property1,
                    property2
                },
                principalEntityBuilder.Metadata.FindPrimaryKey(),
                principalEntityBuilder.Metadata);

            var navigationToPrincipal = foreignKey.HasDependentToPrincipal(Order.CustomerProperty);
            var navigationToDependent = foreignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var relationship = dependentEntityBuilder.HasForeignKey(principalEntityBuilder, foreignKey.Properties, ConfigurationSource.Convention)
                .DependentToPrincipal(navigationToPrincipal.Name, ConfigurationSource.Convention)
                .PrincipalToDependent(navigationToDependent.Name, ConfigurationSource.Convention);
            Assert.Same(foreignKey, relationship.Metadata);

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
        }

        [Theory]
        [InlineData(ConfigurationSource.Explicit, ConfigurationSource.Explicit)]
        [InlineData(ConfigurationSource.Explicit, ConfigurationSource.DataAnnotation)]
        [InlineData(ConfigurationSource.Explicit, ConfigurationSource.Convention)]
        [InlineData(ConfigurationSource.DataAnnotation, ConfigurationSource.Explicit)]
        [InlineData(ConfigurationSource.DataAnnotation, ConfigurationSource.DataAnnotation)]
        [InlineData(ConfigurationSource.DataAnnotation, ConfigurationSource.Convention)]
        [InlineData(ConfigurationSource.Convention, ConfigurationSource.Explicit)]
        [InlineData(ConfigurationSource.Convention, ConfigurationSource.DataAnnotation)]
        [InlineData(ConfigurationSource.Convention, ConfigurationSource.Convention)]
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
                et => et.Relationship(
                          et.ModelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit),
                          Order.CustomerProperty.Name,
                          Customer.OrdersProperty.Name,
                          addConfigurationSource) != null,
                et => et.Relationship(
                          et.ModelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit),
                          Order.CustomerProperty.Name,
                          Customer.OrdersProperty.Name,
                          ignoreConfigurationSource) != null,
                Order.CustomerProperty.Name);

        [Fact]
        public void Can_merge_with_intrahierarchal_relationship_of_higher_source()
        {
            var modelBuilder = CreateModelBuilder();
            var baseEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
            derivedEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);

            baseEntityBuilder.Relationship(derivedEntityBuilder, ConfigurationSource.Explicit)
                .DependentToPrincipal(nameof(Customer.SpecialCustomer), ConfigurationSource.Explicit);

            var derivedRelationship = derivedEntityBuilder.Relationship(
                baseEntityBuilder,
                nameof(SpecialCustomer.Customer),
                nameof(Customer.SpecialCustomer),
                ConfigurationSource.Convention);

            Assert.NotNull(derivedRelationship);

            var baseNavigation = baseEntityBuilder.Metadata.GetNavigations().Single();
            Assert.Equal(nameof(Customer.SpecialCustomer), baseNavigation.Name);
            Assert.Equal(nameof(SpecialCustomer.Customer), baseNavigation.FindInverse()?.Name);
        }

        [Fact]
        public void Relationship_does_not_return_same_instance_if_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention);

            Assert.NotNull(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention));
            Assert.Equal(2, orderEntityBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
        public void Can_ignore_lower_source_weak_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.NotNull(dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention));

            Assert.True(modelBuilder.Ignore(typeof(Order), ConfigurationSource.Explicit));

            Assert.Equal(typeof(Customer).FullName, modelBuilder.Metadata.GetEntityTypes().Single().Name);
        }

        [Fact]
        public void Can_ignore_lower_source_principal_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.NotNull(dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention));

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

            Assert.Equal(typeof(Order).FullName, modelBuilder.Metadata.GetEntityTypes().Single().Name);
        }

        [Fact]
        public void Can_ignore_lower_source_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(dependentEntityBuilder.Relationship(principalEntityBuilder, null, Customer.OrdersProperty.Name, ConfigurationSource.Convention));

            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.Null(dependentEntityBuilder.Relationship(principalEntityBuilder, null, Customer.OrdersProperty.Name, ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_ignore_lower_source_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(dependentEntityBuilder.Relationship(principalEntityBuilder, Order.CustomerProperty, null, ConfigurationSource.Convention));

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.Null(dependentEntityBuilder.Relationship(principalEntityBuilder, Order.CustomerProperty, null, ConfigurationSource.Convention));
        }

        [Fact]
        public void Cannot_add_navigation_to_principal_if_null_navigation_is_higher_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.Relationship(principalEntityBuilder, null, Customer.OrdersProperty.Name, ConfigurationSource.Explicit);

            Assert.Null(dependentEntityBuilder.Relationship(principalEntityBuilder, Order.CustomerProperty.Name, Customer.OrdersProperty.Name, ConfigurationSource.Convention));

            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Null(fk.DependentToPrincipal?.Name);
            Assert.Equal(Customer.OrdersProperty.Name, fk.PrincipalToDependent.Name);
        }

        [Fact]
        public void Cannot_add_navigation_to_dependent_if_null_navigation_is_higher_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.Relationship(principalEntityBuilder, Order.CustomerProperty.Name, null, ConfigurationSource.Explicit);

            Assert.Null(dependentEntityBuilder.Relationship(principalEntityBuilder, Order.CustomerProperty, Customer.OrdersProperty, ConfigurationSource.Convention));

            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Null(fk.PrincipalToDependent?.Name);
            Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
        }

        [Fact]
        public void Dependent_conflicting_relationship_is_not_removed_if_principal_conflicting_relationship_cannot_be_removed()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var orderRelationship = dependentEntityBuilder.Relationship(principalEntityBuilder, Order.CustomerProperty.Name, null, ConfigurationSource.DataAnnotation);
            Assert.NotNull(orderRelationship);
            var customerRelationship = dependentEntityBuilder.Relationship(principalEntityBuilder, null, Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention)
                .RelatedEntityTypes(principalEntityBuilder.Metadata, dependentEntityBuilder.Metadata, ConfigurationSource.Convention);
            Assert.NotNull(customerRelationship);

            Assert.Null(principalEntityBuilder.Relationship(dependentEntityBuilder, Customer.NotCollectionOrdersProperty.Name, Order.CustomerProperty.Name, ConfigurationSource.Convention));

            var orderFk = dependentEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            var customerFk = principalEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            Assert.Null(orderFk.PrincipalToDependent);
            Assert.Equal(Order.CustomerProperty.Name, orderFk.DependentToPrincipal.Name);
            Assert.Equal(Customer.NotCollectionOrdersProperty.Name, customerFk.PrincipalToDependent.Name);
            Assert.Null(customerFk.DependentToPrincipal);
        }

        [Fact]
        public void Principal_conflicting_relationship_is_not_removed_if_dependent_conflicting_relationship_cannot_be_removed()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var orderRelationship = dependentEntityBuilder.Relationship(principalEntityBuilder, Order.CustomerProperty, null, ConfigurationSource.Convention);
            Assert.NotNull(orderRelationship);
            var customerRelationship = dependentEntityBuilder.Relationship(principalEntityBuilder, null, Customer.NotCollectionOrdersProperty, ConfigurationSource.DataAnnotation)
                .RelatedEntityTypes(principalEntityBuilder.Metadata, dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation);
            Assert.NotNull(customerRelationship);

            Assert.Null(dependentEntityBuilder.Relationship(principalEntityBuilder, Customer.NotCollectionOrdersProperty, Order.CustomerProperty, ConfigurationSource.Convention));

            Assert.Null(orderRelationship.Metadata.PrincipalToDependent);
            Assert.Equal(Order.CustomerProperty.Name, orderRelationship.Metadata.DependentToPrincipal.Name);
            Assert.Equal(Customer.NotCollectionOrdersProperty.Name, customerRelationship.Metadata.PrincipalToDependent.Name);
            Assert.Null(customerRelationship.Metadata.DependentToPrincipal);
        }

        [Fact]
        public void Conflicting_navigations_are_not_removed_if_conflicting_fk_cannot_be_removed()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var fkProperty = orderEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Explicit).Metadata;
            var key = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit).Metadata;

            var orderRelationship = orderEntityBuilder.Relationship(
                customerEntityBuilder, Order.CustomerProperty.Name, null, ConfigurationSource.Convention);
            Assert.NotNull(orderRelationship);
            var customerRelationship = orderEntityBuilder.Relationship(
                customerEntityBuilder, null, Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention);
            Assert.NotNull(customerRelationship);
            var fkRelationship = orderEntityBuilder.HasForeignKey(
                    customerEntityBuilder, new[] { fkProperty }, key, ConfigurationSource.DataAnnotation)
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

            orderEntityBuilder.Relationship(customerEntityBuilder, ConfigurationSource.Convention)
                .DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Convention)
                .PrincipalToDependent(Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention)
                .HasForeignKey(new[] { fkProperty }, ConfigurationSource.Convention)
                .HasPrincipalKey(key.Properties, ConfigurationSource.Convention);

            var navigationFk = customerEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            Assert.Same(navigationFk, orderEntityBuilder.Metadata.GetNavigations().Single().ForeignKey);
            Assert.NotSame(navigationFk, fkRelationship.Metadata);
            Assert.NotNull(fkRelationship.Metadata.Builder);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            Assert.Same(
                derivedEntityBuilder,
                derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.False(modelBuilder.Ignore(entityBuilder.Metadata.Name, ConfigurationSource.Convention));
            Assert.Same(
                derivedEntityBuilder,
                derivedEntityBuilder.HasBaseType((Type)null, ConfigurationSource.DataAnnotation));
            Assert.Same(
                derivedEntityBuilder,
                derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Explicit));
            Assert.Null(derivedEntityBuilder.HasBaseType((string)null, ConfigurationSource.Convention));
            Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
        }

        [Fact]
        public void Can_only_override_existing_base_type_explicitly()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.Metadata.HasBaseType(entityBuilder.Metadata);

            Assert.Same(derivedEntityBuilder, derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedEntityBuilder.HasBaseType((EntityType)null, ConfigurationSource.Convention));
            Assert.Same(derivedEntityBuilder, derivedEntityBuilder.HasBaseType((EntityType)null, ConfigurationSource.Explicit));
            Assert.Null(derivedEntityBuilder.Metadata.BaseType);
        }

        [Fact]
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
            Assert.Equal(1, entityBuilder.Metadata.GetDeclaredProperties().Count());
            Assert.Equal(0, derivedEntityBuilder.Metadata.GetDeclaredProperties().Count());
        }

        [Fact]
        public void Can_only_set_base_type_if_keys_of_data_annotation_or_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.HasKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            Assert.Null(derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, derivedEntityBuilder.Metadata.GetDeclaredKeys().Count());

            Assert.Same(
                derivedEntityBuilder,
                derivedEntityBuilder.HasBaseType(typeof(Order), ConfigurationSource.Explicit));
            Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
            Assert.Equal(0, derivedEntityBuilder.Metadata.GetDeclaredKeys().Count());
        }

        [Fact]
        public void Can_only_set_base_type_if_relationship_with_conflicting_navigation_of_data_annotation_or_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder, Order.CustomerProperty.Name, null, ConfigurationSource.Explicit);

            var derivedPrincipalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit)
                .HasBaseType((string)null, ConfigurationSource.Explicit);
            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedDependentEntityBuilder.Relationship(
                derivedPrincipalEntityBuilder, Order.CustomerProperty.Name, null, ConfigurationSource.Explicit);

            Assert.Null(derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());

            Assert.Same(
                derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Explicit));
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, dependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());
            Assert.Equal(0, derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());
        }

        [Fact]
        public void Can_set_base_type_if_relationship_with_conflicting_foreign_key_of_data_annotation_or_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedDependentEntityBuilder.HasForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit);

            Assert.Same(
                derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, dependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());
            Assert.Equal(0, derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());
        }

        [Fact]
        public void Setting_base_type_preserves_index()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
            derivedEntityBuilder.HasIndex(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation).IsUnique(true, ConfigurationSource.Convention);
            Assert.Equal(1, derivedEntityBuilder.Metadata.GetDeclaredIndexes().Count());

            Assert.Same(
                derivedEntityBuilder,
                derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Convention));

            Assert.Equal(1, derivedEntityBuilder.Metadata.GetDeclaredIndexes().Count());
            Assert.True(derivedEntityBuilder.Metadata.GetDeclaredIndexes().First().IsUnique);
        }

        [Fact]
        public void Setting_base_type_preserves_non_conflicting_referencing_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var derivedPrincipalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Convention);
            var derivedIdProperty = derivedPrincipalEntityBuilder.Property(Customer.IdProperty, ConfigurationSource.Convention).Metadata;

            dependentEntityBuilder.Relationship(
                    derivedPrincipalEntityBuilder,
                    Order.CustomerProperty.Name,
                    Customer.OrdersProperty.Name,
                    ConfigurationSource.Convention)
                .HasPrincipalKey(new[] { derivedIdProperty }, ConfigurationSource.Convention);
            Assert.Equal(1, derivedPrincipalEntityBuilder.Metadata.GetDeclaredKeys().Count());

            Assert.Same(
                derivedPrincipalEntityBuilder,
                derivedPrincipalEntityBuilder.HasBaseType(principalEntityBuilder.Metadata, ConfigurationSource.Convention));

            Assert.Equal(1, principalEntityBuilder.Metadata.GetDeclaredKeys().Count());
            Assert.Equal(0, derivedPrincipalEntityBuilder.Metadata.GetDeclaredKeys().Count());
            Assert.Equal(0, derivedPrincipalEntityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Equal(0, principalEntityBuilder.Metadata.GetReferencingForeignKeys().Count());
            var fk = derivedPrincipalEntityBuilder.Metadata.GetReferencingForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
            Assert.Equal(Customer.OrdersProperty.Name, fk.PrincipalToDependent.Name);
        }

        [Fact]
        public void Setting_base_type_preserves_non_conflicting_relationship_on_duplicate_foreign_key_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            var derivedIdProperty = derivedDependentEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention).Metadata;

            derivedDependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    Order.CustomerProperty.Name,
                    Customer.SpecialOrdersProperty.Name,
                    ConfigurationSource.DataAnnotation)
                .HasForeignKey(new[] { derivedIdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Same(
                derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Equal(0, dependentEntityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Equal(0, dependentEntityBuilder.Metadata.GetDeclaredProperties().Count());
            var fk = derivedDependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
            Assert.Equal(Customer.SpecialOrdersProperty.Name, fk.PrincipalToDependent.Name);
            Assert.Equal(Order.IdProperty.Name, fk.Properties.Single().Name);
        }

        [Fact]
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

            derivedDependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    Order.CustomerProperty.Name,
                    Customer.SpecialOrdersProperty.Name,
                    ConfigurationSource.Explicit)
                .HasForeignKey(new[] { derivedIdProperty }, ConfigurationSource.Explicit);

            Assert.Null(derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Same(
                derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.HasBaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Explicit));
            Assert.Equal(0, dependentEntityBuilder.Metadata.GetForeignKeys().Count());
            var fk = derivedDependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
            Assert.Equal(Customer.SpecialOrdersProperty.Name, fk.PrincipalToDependent.Name);
            Assert.NotEqual(Order.IdProperty.Name, fk.Properties.Single().Name);
            Assert.Equal(1, dependentEntityBuilder.Metadata.GetDeclaredProperties().Count());
        }

        [Fact]
        public void Setting_base_type_removes_duplicate_service_properties()
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
                derivedEntityBuilder.HasBaseType(entityBuilder.Metadata, ConfigurationSource.Explicit));

            Assert.Equal(1, entityBuilder.Metadata.GetServiceProperties().Count());
            Assert.Equal(1, derivedEntityBuilder.Metadata.GetServiceProperties().Count());
        }

        private InternalModelBuilder CreateModelBuilder() => new InternalModelBuilder(new Model());

        private class Order
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");
            public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty("Customer");
            public static readonly PropertyInfo ContextProperty = typeof(Order).GetProperty(nameof(Context));

            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Guid? CustomerUnique { get; set; }
            public Customer Customer { get; set; }
            public DbContext Context { get; set; }
        }

        private class SpecialOrder : Order, IEnumerable<Order>
        {
            public static readonly PropertyInfo SpecialtyProperty = typeof(SpecialOrder).GetProperty("Specialty");

            public IEnumerator<Order> GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public string Specialty { get; set; }
        }

        private class ExtraSpecialOrder : SpecialOrder
        {
        }

        private class BackOrder : Order
        {
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo UniqueProperty = typeof(Customer).GetProperty("Unique");
            public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty("Orders");
            public static readonly PropertyInfo NotCollectionOrdersProperty = typeof(Customer).GetProperty("NotCollectionOrders");
            public static readonly PropertyInfo SpecialOrdersProperty = typeof(Customer).GetProperty("SpecialOrders");

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
    }
}
