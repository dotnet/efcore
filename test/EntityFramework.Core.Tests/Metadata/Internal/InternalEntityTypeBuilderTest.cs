// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalEntityTypeBuilderTest
    {
        [Fact]
        public void Relationship_returns_same_instance_for_same_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.Explicit);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder,
                dependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    dependentEntityBuilder,
                    null,
                    null,
                    new[]
                    {
                        dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                        dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                    },
                    key.Metadata.Properties,
                    ConfigurationSource.Convention));
        }

        [Fact]
        public void Relationship_returns_same_instance_for_same_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                null,
                null,
                ConfigurationSource.Explicit);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder,
                dependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    dependentEntityBuilder,
                    Order.CustomerProperty.Name,
                    Customer.OrdersProperty.Name,
                    null,
                    null,
                    ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_add_relationship_if_principal_type_ignored_at_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Convention);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
                null,
                null,
                ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_add_relationship_if_dependent_type_ignored_at_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore(typeof(Order), ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
                null,
                null,
                ConfigurationSource.Explicit));
        }

        [Fact]
        public void Can_add_relationship_if_navigation_to_dependent_ignored_at_lower_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));

            Assert.NotNull(dependentEntityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
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

            Assert.NotNull(principalEntityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
                Order.CustomerProperty.Name,
                null,
                ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Relationship_configures_facets_with_the_specified_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.Explicit,
                isUnique: false,
                isRequired: false,
                deleteBehavior: DeleteBehavior.Cascade,
                strictPrincipal: true);

            Assert.Null(relationshipBuilder.ForeignKey(new[] { Order.IdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));
            var shadowId = principalEntityBuilder.Property("ShadowId", typeof(int), ConfigurationSource.Convention).Metadata;
            Assert.Null(relationshipBuilder.PrincipalKey(new[] { shadowId.Name, Customer.UniqueProperty.Name }, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.Unique(true, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.Required(true, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.DeleteBehavior(DeleteBehavior.None, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.DependentToPrincipal(null, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.PrincipalToDependent(null, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Relationship_throws_when_base_has_conflicting_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);
            var basePrincipalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var baseDependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            principalEntityBuilder.BaseType(basePrincipalEntityBuilder.Metadata, ConfigurationSource.Explicit);
            dependentEntityBuilder.BaseType(baseDependentEntityBuilder.Metadata, ConfigurationSource.Explicit);

            baseDependentEntityBuilder.Relationship(
                basePrincipalEntityBuilder,
                baseDependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                null,
                null,
                ConfigurationSource.Convention);

            Assert.Null(dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                Order.CustomerProperty.Name,
                null,
                null,
                null,
                ConfigurationSource.Convention));

            Assert.Equal(Strings.DuplicateNavigation(Order.CustomerProperty.Name, typeof(SpecialOrder).Name),
                Assert.Throws<InvalidOperationException>(() =>
                    dependentEntityBuilder.Relationship(
                        principalEntityBuilder,
                        dependentEntityBuilder,
                        Order.CustomerProperty.Name,
                        null,
                        null,
                        null,
                        ConfigurationSource.Explicit)).Message);

            Assert.Null(dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                Customer.OrdersProperty.Name,
                null,
                null,
                ConfigurationSource.Convention));

            Assert.Equal(Strings.DuplicateNavigation(Customer.OrdersProperty.Name, typeof(SpecialCustomer).Name),
                Assert.Throws<InvalidOperationException>(() =>
                    dependentEntityBuilder.Relationship(
                        principalEntityBuilder,
                        dependentEntityBuilder,
                        null,
                        Customer.OrdersProperty.Name,
                        null,
                        null,
                        ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Relationship_throws_when_foreign_key_properties_cannot_be_nullable()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(Strings.ForeignKeyCannotBeOptional("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() =>
                    dependentEntityBuilder.Relationship(
                        principalEntityBuilder,
                        dependentEntityBuilder,
                        null,
                        null,
                        new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
                        null,
                        ConfigurationSource.Explicit,
                        isRequired: false)).Message);
        }

        [Fact]
        public void Can_only_remove_lower_or_equal_source_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationshipBuilder);

            Assert.Null(dependentEntityBuilder.RemoveRelationship(relationshipBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.DataAnnotation, dependentEntityBuilder.RemoveRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(
                new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
                dependentEntityBuilder.Metadata.Properties.Select(p => p.Name));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Removing_relationship_removes_unused_contained_shadow_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = dependentEntityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    shadowProperty.Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);

            Assert.Equal(ConfigurationSource.Convention, dependentEntityBuilder.RemoveRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Same(Order.CustomerIdProperty.Name, dependentEntityBuilder.Metadata.Properties.Single().Name);
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
        {
            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.PrimaryKey(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.Index(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.ForeignKey(
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
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = dependentEntityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);
            Assert.NotNull(shadowConfig(dependentEntityBuilder, shadowProperty.Metadata));

            var relationshipBuilder = dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    shadowProperty.Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);

            Assert.Equal(ConfigurationSource.Convention, dependentEntityBuilder.RemoveRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, dependentEntityBuilder.Metadata.Properties.Count(p => p.Name == shadowProperty.Metadata.Name));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys().Where(foreignKey => foreignKey.Properties.SequenceEqual(relationshipBuilder.Metadata.Properties)));
        }

        [Fact]
        public void Index_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var indexBuilder = entityBuilder.Index(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit);

            Assert.NotNull(indexBuilder);
            Assert.Same(indexBuilder, entityBuilder.Index(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Index_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var indexBuilder = entityBuilder.Index(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(indexBuilder);
            Assert.Same(indexBuilder, entityBuilder.Index(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));
        }

        [Fact]
        public void Index_returns_null_for_ignored_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

            Assert.Null(entityBuilder.Index(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Index_returns_null_for_ignored_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Null(entityBuilder.Index(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_only_remove_lower_or_equal_source_index()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var index = entityBuilder.Index(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.NotNull(index);

            Assert.Null(entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.DataAnnotation, entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.Properties.Single().Name);
            Assert.Empty(entityBuilder.Metadata.Indexes);
        }

        [Fact]
        public void Removing_index_removes_unused_contained_shadow_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);

            var index = entityBuilder.Index(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(index);

            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.Properties.Single().Name);
            Assert.Empty(entityBuilder.Metadata.Indexes);
        }

        [Fact]
        public void Removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere()
        {
            Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.PrimaryKey(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.Index(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.ForeignKey(
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

            var index = entityBuilder.Index(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(index);

            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveIndex(index.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, entityBuilder.Metadata.Properties.Count(p => p.Name == shadowProperty.Metadata.Name));
            Assert.Empty(entityBuilder.Metadata.Indexes.Where(i => i.Properties.SequenceEqual(index.Metadata.Properties)));
        }

        [Fact]
        public void Key_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);

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
            Assert.Same(keyBuilder, entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Key_throws_for_property_names_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(Strings.NoClrProperty(Customer.UniqueProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    entityBuilder.Key(new[] { Customer.UniqueProperty.Name }, ConfigurationSource.Convention)).Message);
        }

        [Fact]
        public void Key_throws_for_derived_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var keyBuilder = entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Equal(Strings.DerivedEntityTypeKey(typeof(SpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    derivedEntityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation)).Message);
        }

        [Fact]
        public void Key_throws_for_property_names_for_shadow_entity_type_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order).Name, ConfigurationSource.Explicit);

            Assert.Equal(Strings.PropertyNotFound(Order.IdProperty.Name, typeof(Order).Name),
                Assert.Throws<ModelItemNotFoundException>(() =>
                    entityBuilder.Key(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention)).Message);
        }

        [Fact]
        public void Key_works_for_property_names_for_shadow_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.CustomerIdProperty.Name, Order.CustomerIdProperty.PropertyType, ConfigurationSource.Convention);

            Assert.NotNull(entityBuilder.Key(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetKeys().Single().Properties.Single().Name);
        }

        [Fact]
        public void Key_returns_null_for_ignored_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit);

            Assert.Null(entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Key_returns_null_for_ignored_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Null(entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_only_remove_lower_or_equal_source_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var key = entityBuilder.Key(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.NotNull(key);

            Assert.Null(entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.DataAnnotation, entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.Properties.Single().Name);
            Assert.Empty(entityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void Removing_key_removes_unused_contained_shadow_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property("Shadow", typeof(Guid), ConfigurationSource.Convention);

            var key = entityBuilder.Key(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(key);

            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.Properties.Single().Name);
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
                (entityBuilder, property) => entityBuilder.Index(new[] { property.Name }, ConfigurationSource.Convention));

            Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.ForeignKey(
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
            Assert.NotNull(shadowConfig(entityBuilder, shadowProperty.Metadata));

            var key = entityBuilder.Key(new[] { Order.CustomerIdProperty.Name, shadowProperty.Metadata.Name }, ConfigurationSource.Convention);
            Assert.NotNull(key);

            Assert.Equal(ConfigurationSource.Convention, entityBuilder.RemoveKey(key.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, entityBuilder.Metadata.Properties.Count(p => p.Name == shadowProperty.Metadata.Name));
            Assert.Empty(entityBuilder.Metadata.GetKeys().Where(foreignKey => foreignKey.Properties.SequenceEqual(key.Metadata.Properties)));
        }

        [Fact]
        public void Removing_key_removes_referencing_foreign_key_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.DataAnnotation);

            Assert.Null(principalEntityBuilder.RemoveKey(key.Metadata, ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.DataAnnotation, principalEntityBuilder.RemoveKey(key.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(
                new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
                dependentEntityBuilder.Metadata.Properties.Select(p => p.Name));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(principalEntityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void PrimaryKey_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

            Assert.Same(entityBuilder.Metadata.GetPrimaryKey(), entityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void PrimaryKey_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.Same(entityBuilder.Metadata.GetPrimaryKey(), entityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void PrimaryKey_throws_for_property_names_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(Strings.NoClrProperty(Customer.UniqueProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    entityBuilder.PrimaryKey(new[] { Customer.UniqueProperty.Name }, ConfigurationSource.Convention)).Message);
        }

        [Fact]
        public void PrimaryKey_throws_for_property_names_for_shadow_entity_type_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order).Name, ConfigurationSource.Explicit);

            Assert.Equal(Strings.PropertyNotFound(Order.IdProperty.Name, typeof(Order).Name),
                Assert.Throws<ModelItemNotFoundException>(() =>
                    entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name }, ConfigurationSource.Convention)).Message);
        }

        [Fact]
        public void PrimaryKey_throws_for_derived_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var keyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Equal(Strings.DerivedEntityTypeKey(typeof(SpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    derivedEntityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation)).Message);
        }

        [Fact]
        public void PrimaryKey_works_for_property_names_for_shadow_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.CustomerIdProperty.Name, Order.CustomerIdProperty.PropertyType, ConfigurationSource.Convention);

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));

            Assert.Equal(Order.CustomerIdProperty.Name, entityBuilder.Metadata.GetPrimaryKey().Properties.Single().Name);
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

            var simpleKeyBuilder2 = entityBuilder.Key(new[] { Order.IdProperty }, ConfigurationSource.Explicit);
            Assert.Same(simpleKeyBuilder, simpleKeyBuilder2);

            var compositeKeyBuilder2 = entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Convention);
            Assert.NotNull(compositeKeyBuilder2);
            Assert.NotEqual(compositeKeyBuilder, compositeKeyBuilder2);
            Assert.Same(compositeKeyBuilder2.Metadata, entityBuilder.Metadata.GetPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));
            Assert.Same(compositeKeyBuilder2.Metadata, entityBuilder.Metadata.GetPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
        }

        [Fact]
        public void Can_only_override_existing_primary_key_explicitly()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;
            entityType.SetPrimaryKey(new[] { entityType.GetOrAddProperty(Order.IdProperty), entityType.GetOrAddProperty(Order.CustomerIdProperty) });

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));

            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit));

            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);
        }

        [Fact]
        public void Changing_primary_key_removes_previously_referenced_primary_key_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                keyBuilder.Metadata.Properties,
                ConfigurationSource.DataAnnotation);

            keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Same(keyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey);
            Assert.Same(keyBuilder.Metadata, principalEntityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void Changing_primary_key_removes_previously_referenced_key_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var keyBuilder = principalEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                keyBuilder.Metadata.Properties,
                ConfigurationSource.DataAnnotation);

            keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Same(keyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single().PrincipalKey);
            Assert.Same(keyBuilder.Metadata, principalEntityBuilder.Metadata.GetKeys().Single());
        }

        [Fact]
        public void Changing_primary_key_does_not_remove_previously_explicitly_referenced_key()
        {
            var modelBuilder = CreateModelBuilder();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var existingKeyBuilder = principalEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                existingKeyBuilder.Metadata.Properties,
                ConfigurationSource.Explicit);

            var keyBuilder = principalEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

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
            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty.Name, typeof(Order), ConfigurationSource.Explicit));
        }

        [Fact]
        public void Property_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var propertyBuilder = entityBuilder.Property(Order.IdProperty.Name, typeof(Order), ConfigurationSource.DataAnnotation);

            Assert.NotNull(propertyBuilder);
            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Property_throws_for_derived_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var propertyBuilder = entityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.Convention);

            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Equal(Strings.DuplicateProperty(Order.IdProperty.Name, typeof(SpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    derivedEntityBuilder.Property(Order.IdProperty.Name, typeof(int), ConfigurationSource.DataAnnotation)).Message);
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
            Assert.Null(entityBuilder.Property(Order.IdProperty.Name, typeof(Order), ConfigurationSource.DataAnnotation));

            Assert.NotNull(entityBuilder.Property(Order.IdProperty.Name, typeof(Order), ConfigurationSource.Explicit));
        }

        [Fact]
        public void Cannot_ignore_higher_source_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            Assert.NotNull(entityBuilder.Property(Order.IdProperty.Name, typeof(Order), ConfigurationSource.DataAnnotation));
            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.NotNull(entityType.FindProperty(Order.IdProperty.Name));

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.Null(entityType.FindProperty(Order.IdProperty.Name));

            Assert.NotNull(entityBuilder.Property(Order.IdProperty.Name, typeof(Order), ConfigurationSource.Explicit));
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
            property.IsShadowProperty = false;

            Assert.Same(property, entityBuilder.Property(Order.IdProperty.Name, ConfigurationSource.Convention).Metadata);

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));
            Assert.Null(entityType.FindProperty(Order.IdProperty.Name));
        }

        [Fact]
        public void Can_ignore_inherited_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.BaseType(typeof(Order), ConfigurationSource.DataAnnotation);

            Assert.NotNull(entityBuilder.Property(Order.IdProperty.Name, typeof(Order), ConfigurationSource.DataAnnotation));
            Assert.False(derivedEntityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.NotNull(entityBuilder.Metadata.FindProperty(Order.IdProperty.Name));

            Assert.True(derivedEntityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.Null(entityBuilder.Metadata.FindProperty(Order.IdProperty.Name));
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_foreign_key_preserving_the_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder =
                dependentEntityBuilder.Relationship(
                    principalEntityBuilder,
                    dependentEntityBuilder,
                    null,
                    null,
                    new[]
                    {
                        dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                        dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                    },
                    key.Metadata.Properties,
                    ConfigurationSource.DataAnnotation,
                    isUnique: true,
                    isRequired: true,
                    deleteBehavior: DeleteBehavior.Cascade);
            var fk = relationshipBuilder.Metadata;

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(dependentEntityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
            var newFk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Same(fk.DeclaringEntityType, newFk.DeclaringEntityType);
            Assert.Same(fk.PrincipalEntityType, newFk.PrincipalEntityType);
            Assert.Null(newFk.PrincipalToDependent);
            Assert.Null(newFk.DependentToPrincipal);
            Assert.NotEqual(fk.Properties, newFk.DeclaringEntityType.Properties);
            Assert.Same(fk.PrincipalKey, newFk.PrincipalKey);
            Assert.True(newFk.IsUnique);
            Assert.True(newFk.IsRequired);
            Assert.Equal(DeleteBehavior.Cascade, newFk.DeleteBehavior);

            relationshipBuilder = dependentEntityBuilder.Relationship(newFk, true, ConfigurationSource.Convention);
            var shadowId = principalEntityBuilder.Property("ShadowId", typeof(int), ConfigurationSource.Convention).Metadata;
            Assert.Null(relationshipBuilder.PrincipalKey(new[] { shadowId.Name, Customer.UniqueProperty.Name }, ConfigurationSource.Convention));
            Assert.Null(relationshipBuilder.Unique(false, ConfigurationSource.Convention));
            Assert.Null(relationshipBuilder.Required(false, ConfigurationSource.Convention));
            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.Convention));
            Assert.Null(relationshipBuilder.DependentToPrincipal(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.Null(relationshipBuilder.PrincipalToDependent(Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention));
        }

        [Fact]
        public void Cannot_ignore_property_that_is_part_of_higher_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                null,
                null,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.DataAnnotation);

            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Convention));

            Assert.NotEmpty(dependentEntityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.NotEmpty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_index()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.Index(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.True(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(entityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.Empty(entityBuilder.Metadata.Indexes);
        }

        [Fact]
        public void Cannot_ignore_property_that_is_part_of_same_or_higher_source_index()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.Index(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));

            Assert.False(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.NotEmpty(entityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.NotEmpty(entityBuilder.Metadata.Indexes);
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.True(entityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(entityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
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
                dependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                new[]
                {
                    dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                    dependentEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                },
                key.Metadata.Properties,
                ConfigurationSource.Convention).Metadata;

            Assert.True(principalEntityBuilder.Ignore(Customer.UniqueProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.Empty(principalEntityBuilder.Metadata.Properties.Where(p => p.Name == Customer.UniqueProperty.Name));
            var newFk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Same(fk.DeclaringEntityType, newFk.DeclaringEntityType);
            Assert.Same(fk.PrincipalEntityType, newFk.PrincipalEntityType);
            Assert.Equal(new[] { Order.CustomerIdProperty.Name, newFk.Properties.Single().Name, Order.CustomerUniqueProperty.Name },
                dependentType.Properties.Select(p => p.Name));
            Assert.Same(principalType.GetKeys().Single(), newFk.PrincipalKey);
            Assert.Equal(new[] { Customer.IdProperty.Name, newFk.PrincipalKey.Properties.Single().Name },
                principalType.Properties.Select(p => p.Name));
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

            Assert.NotEmpty(entityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.NotEmpty(entityBuilder.Metadata.GetKeys());
        }

        [Fact]
        public void Navigation_returns_same_value()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.True(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.Same(foreignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.False(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(dependentEntityBuilder.CanAddOrReplaceNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));

            Assert.True(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.CanAddOrReplaceNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Same(foreignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Navigation_throws_for_derived_type()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            var derivedPrincipalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Convention);
            derivedPrincipalEntityBuilder.BaseType(principalEntityBuilder.Metadata, ConfigurationSource.Convention);
            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention);

            Assert.True(derivedDependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.Equal(Strings.NavigationOnWrongEntityType(Order.CustomerProperty.Name, typeof(SpecialOrder).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    derivedDependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation)).Message);

            Assert.True(derivedPrincipalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.Equal(Strings.NavigationOnWrongEntityType(Customer.OrdersProperty.Name, typeof(SpecialCustomer).FullName, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    derivedPrincipalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation)).Message);

            dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation);
            Assert.False(derivedDependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(derivedDependentEntityBuilder.CanAddOrReplaceNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));

            principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation);
            Assert.False(derivedPrincipalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));
            Assert.True(derivedPrincipalEntityBuilder.CanAddOrReplaceNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Equal(Strings.DuplicateNavigation(Order.CustomerProperty.Name, typeof(SpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    derivedDependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation)).Message);

            Assert.Equal(Strings.DuplicateNavigation(Customer.OrdersProperty.Name, typeof(SpecialCustomer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    derivedPrincipalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation)).Message);
        }

        [Fact]
        public void Can_override_lower_or_equal_source_conflicting_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var conflictingForeignKeyBuilder = dependentEntityBuilder.ForeignKey(
                typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.Same(conflictingForeignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, conflictingForeignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.Same(conflictingForeignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, conflictingForeignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.Null(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.Null(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));

            Assert.Same(conflictingForeignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
            Assert.Same(conflictingForeignKeyBuilder.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);

            Assert.Same(foreignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            Assert.Same(foreignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
            Assert.Same(foreignKeyBuilder.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);
            Assert.Same(foreignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
        }

        [Fact]
        public void Can_ignore_lower_or_equal_source_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.Same(foreignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());

            foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.Null(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.Null(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            Assert.NotNull(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Explicit));
            Assert.NotNull(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit));
        }

        [Fact]
        public void Cannot_ignore_higher_source_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.True(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));
            Assert.Same(foreignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Explicit));
            Assert.Same(foreignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit));
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));

            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.NotNull(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));

            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit));
            Assert.Null(dependentEntityBuilder.Navigation(null, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_ignore_existing_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var property1 = dependentEntityBuilder.Metadata.AddProperty(Order.CustomerIdProperty.Name, typeof(int));
            property1.IsShadowProperty = false;
            var property2 = dependentEntityBuilder.Metadata.AddProperty(Order.CustomerUniqueProperty.Name, typeof(Guid));
            property2.IsShadowProperty = false;
            var foreignKey = dependentEntityBuilder.Metadata.AddForeignKey(
                new[]
                {
                    property1,
                    property2
                },
                principalEntityBuilder.Metadata.GetPrimaryKey(),
                principalEntityBuilder.Metadata);
            var navigationToPrincipal = dependentEntityBuilder.Metadata.AddNavigation(Order.CustomerProperty.Name, foreignKey, pointsToPrincipal: true);
            var navigationToDependent = principalEntityBuilder.Metadata.AddNavigation(Customer.OrdersProperty.Name, foreignKey, pointsToPrincipal: false);

            Assert.Same(foreignKey, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKey, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention).Metadata);
            Assert.Same(foreignKey, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKey, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention).Metadata);

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
        }

        [Fact]
        public void Can_ignore_inherited_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder,
                dependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                ConfigurationSource.DataAnnotation);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention);

            Assert.False(derivedDependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.Equal(Order.CustomerProperty.Name, dependentEntityBuilder.Metadata.Navigations.Single().Name);

            derivedDependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit);
            Assert.Empty(dependentEntityBuilder.Metadata.Navigations);
        }

        [Fact]
        public void Cannot_add_navigations_to_higher_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.Null(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.Null(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
        }

        [Fact]
        public void Can_only_override_existing_conflicting_navigations_explicitly()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));
            Assert.Same(foreignKeyBuilder, dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            var newForeignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.Null(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, newForeignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.Same(foreignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit));

            Assert.Equal(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.Equal(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata.DependentToPrincipal.Name);
            Assert.Null(newForeignKeyBuilder.Metadata.PrincipalToDependent);
            Assert.Null(newForeignKeyBuilder.Metadata.DependentToPrincipal);

            Assert.Same(newForeignKeyBuilder, principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, newForeignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit));
        }

        [Fact]
        public void Navigation_to_principal_does_not_change_uniqueness_for_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            foreignKeyBuilder.Unique(false, ConfigurationSource.Convention);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique.Value);

            foreignKeyBuilder = foreignKeyBuilder.DependentToPrincipal("Customer", ConfigurationSource.Convention);
            Assert.NotNull(foreignKeyBuilder.Metadata.DependentToPrincipal);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique.Value);
        }

        [Fact]
        public void Navigation_to_dependent_changes_uniqueness_for_relationship_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);

            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            Assert.Null(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.DataAnnotation);
            Assert.Equal(Customer.NotCollectionOrdersProperty.Name, foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.True(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.AmbiguousOrderProperty.Name, ConfigurationSource.DataAnnotation);
            Assert.Equal(Customer.AmbiguousOrderProperty.Name, foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.True(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation);
            Assert.Equal(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent(Customer.AmbiguousOrderProperty.Name, ConfigurationSource.DataAnnotation);
            Assert.Equal(Customer.AmbiguousOrderProperty.Name, foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);
        }

        [Fact]
        public void Navigation_to_dependent_does_not_change_uniqueness_for_relationship_of_higher_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            Assert.Null(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.Unique(false, ConfigurationSource.DataAnnotation);
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            Assert.Null(foreignKeyBuilder.PrincipalToDependent(Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention));
            Assert.False(foreignKeyBuilder.Metadata.IsUnique);

            foreignKeyBuilder = foreignKeyBuilder.PrincipalToDependent("Orders", ConfigurationSource.Convention);
            Assert.Equal("Orders", foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
        }

        [Fact]
        public void Relationship_returns_same_instance_for_clr_types()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Convention, true, true);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder, customerEntityBuilder.Relationship(customerEntityBuilder.Metadata, orderEntityBuilder.Metadata, Order.CustomerProperty.Name, null, ConfigurationSource.Convention, true, true));
        }

        [Fact]
        public void Relationship_returns_same_instance_for_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(customerEntityBuilder.Metadata, orderEntityBuilder.Metadata, null, Customer.OrdersProperty.Name, ConfigurationSource.Explicit, false, true);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder, customerEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation, false, true));
        }

        [Fact]
        public void Relationship_does_not_return_same_instance_if_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true);

            Assert.NotNull(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, customerEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true));
        }

        [Fact]
        public void Can_ignore_lower_source_dependent_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            Assert.NotNull(principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true));

            Assert.True(modelBuilder.Ignore(typeof(Order), ConfigurationSource.Explicit));

            Assert.Equal(typeof(Customer).FullName, modelBuilder.Metadata.EntityTypes.Single().Name);
            Assert.True(modelBuilder.Ignore(typeof(Order), ConfigurationSource.Convention));
            Assert.Null(principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true));

            Assert.NotNull(principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Explicit, true));
        }

        [Fact]
        public void Can_ignore_lower_source_principal_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.NotNull(principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true));

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

            Assert.Equal(typeof(Order).FullName, modelBuilder.Metadata.EntityTypes.Single().Name);
            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Convention));
            Assert.Null(principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true));
            Assert.NotNull(principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Explicit, true));
        }

        [Fact]
        public void Can_ignore_lower_source_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.OrdersProperty.Name, ConfigurationSource.Convention, false));

            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.Null(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.OrdersProperty.Name, ConfigurationSource.Convention, false));
        }

        [Fact]
        public void Can_ignore_lower_source_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Convention, false));

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));

            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.Null(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Convention, false));
        }

        [Fact]
        public void Cannot_add_navigation_to_principal_if_conflicting_navigation_is_higher_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.NotNull(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.OrdersProperty.Name, ConfigurationSource.Explicit, false, true));

            Assert.Null(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, Customer.OrdersProperty.Name, ConfigurationSource.Convention, false, true));

            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Null(fk.DependentToPrincipal);
            Assert.Equal(Customer.OrdersProperty.Name, fk.PrincipalToDependent.Name);
        }

        [Fact]
        public void Cannot_add_navigation_to_dependent_if_conflicting_navigation_is_higher_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.NotNull(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Explicit, false, true));

            Assert.Null(dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, Customer.OrdersProperty.Name, ConfigurationSource.Convention, false, true));

            Assert.Empty(principalEntityBuilder.Metadata.GetForeignKeys());
            var fk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Null(fk.PrincipalToDependent);
            Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
        }

        [Fact]
        public void Dependent_conflicting_relationship_is_not_removed_if_principal_conflicting_relationship_cannot_be_removed()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var orderRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.DataAnnotation, true, true);
            Assert.NotNull(orderRelationship);
            var customerRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention, true, true);
            Assert.NotNull(customerRelationship);

            Assert.Null(orderEntityBuilder.Relationship(typeof(Order), typeof(Customer), Customer.NotCollectionOrdersProperty.Name, Order.CustomerProperty.Name, ConfigurationSource.Convention, true, true));

            Assert.Null(orderRelationship.Metadata.PrincipalToDependent);
            Assert.Equal(Order.CustomerProperty.Name, orderRelationship.Metadata.DependentToPrincipal.Name);
            Assert.Equal(Customer.NotCollectionOrdersProperty.Name, customerRelationship.Metadata.PrincipalToDependent.Name);
            Assert.Null(customerRelationship.Metadata.DependentToPrincipal);
        }

        [Fact]
        public void Principal_conflicting_relationship_is_not_removed_if_dependent_conflicting_relationship_cannot_be_removed()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var orderRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Convention, true);
            Assert.NotNull(orderRelationship);
            var customerRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.DataAnnotation, true);
            Assert.NotNull(customerRelationship);

            Assert.Null(orderEntityBuilder.Relationship(typeof(Order), typeof(Customer), Customer.NotCollectionOrdersProperty.Name, Order.CustomerProperty.Name, ConfigurationSource.Convention, true));

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
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var fkProperty = customerEntityBuilder.Property(Customer.IdProperty, ConfigurationSource.Explicit).Metadata;
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var pkProperty1 = orderEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Explicit).Metadata;
            var pkProperty2 = orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Explicit).Metadata;

            var orderRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Convention, true);
            Assert.NotNull(orderRelationship);
            var customerRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Convention, true);
            Assert.NotNull(customerRelationship);
            var fkRelationship = orderEntityBuilder.Relationship(orderEntityBuilder, customerEntityBuilder, null, null, new[] { fkProperty }, new[] { pkProperty1 }, ConfigurationSource.DataAnnotation, true);
            Assert.NotNull(fkRelationship);

            Assert.Null(orderEntityBuilder.Relationship(orderEntityBuilder, customerEntityBuilder, Customer.NotCollectionOrdersProperty.Name, Order.CustomerProperty.Name, new[] { fkProperty }, new[] { pkProperty2 }, ConfigurationSource.Convention, true));

            Assert.Same(customerRelationship.Metadata, customerEntityBuilder.Metadata.Navigations.Single().ForeignKey);
            Assert.Same(orderRelationship.Metadata, orderEntityBuilder.Metadata.Navigations.Single().ForeignKey);
            Assert.Same(pkProperty1, customerEntityBuilder.Metadata.GetForeignKeys().Single(fk => fk.Properties.Any(p => p == fkProperty)).PrincipalKey.Properties.Single());
            Assert.Equal(1, orderEntityBuilder.Metadata.GetKeys().Count());
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            Assert.Same(derivedEntityBuilder,
                derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.False(modelBuilder.Ignore(entityBuilder.Metadata.Name, ConfigurationSource.Convention));
            Assert.Same(derivedEntityBuilder,
                derivedEntityBuilder.BaseType((Type)null, ConfigurationSource.DataAnnotation));
            Assert.Same(derivedEntityBuilder,
                derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.Explicit));
            Assert.Null(derivedEntityBuilder.BaseType((Type)null, ConfigurationSource.Convention));
            Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
        }

        [Fact]
        public void Can_only_override_existing_base_type_explicitly()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.Metadata.BaseType = entityBuilder.Metadata;

            Assert.Same(derivedEntityBuilder, derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedEntityBuilder.BaseType((EntityType)null, ConfigurationSource.Convention));
            Assert.Same(derivedEntityBuilder, derivedEntityBuilder.BaseType((EntityType)null, ConfigurationSource.Explicit));
            Assert.Null(derivedEntityBuilder.Metadata.BaseType);
        }

        [Fact]
        public void Can_only_set_base_type_if_duplicate_properties_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation);

            Assert.Null(derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, derivedEntityBuilder.Metadata.GetDeclaredProperties().Count());

            Assert.Same(derivedEntityBuilder,
                derivedEntityBuilder.BaseType(entityBuilder.Metadata.Name, ConfigurationSource.DataAnnotation));
            Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, entityBuilder.Metadata.GetDeclaredProperties().Count());
            Assert.Equal(0, derivedEntityBuilder.Metadata.GetDeclaredProperties().Count());
        }

        [Fact]
        public void Can_only_set_base_type_if_keys_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            derivedEntityBuilder.Key(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Null(derivedEntityBuilder.BaseType(entityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, derivedEntityBuilder.Metadata.GetDeclaredKeys().Count());

            Assert.Same(derivedEntityBuilder,
                derivedEntityBuilder.BaseType(typeof(Order), ConfigurationSource.DataAnnotation));
            Assert.Same(entityBuilder.Metadata, derivedEntityBuilder.Metadata.BaseType);
            Assert.Equal(0, derivedEntityBuilder.Metadata.GetDeclaredKeys().Count());
        }

        [Fact]
        public void Can_only_set_base_type_if_relationship_with_conflicting_navigation_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            var derivedIdProperty = derivedDependentEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention).Metadata;

            derivedDependentEntityBuilder.Relationship(
                principalEntityBuilder,
                derivedDependentEntityBuilder,
                Order.CustomerProperty.Name,
                null,
                new[] { derivedIdProperty },
                null,
                ConfigurationSource.DataAnnotation);

            Assert.Null(derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());

            Assert.Same(derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, dependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());
            Assert.Equal(0, derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());
        }

        [Fact]
        public void Can_only_set_base_type_if_relationship_with_conflicting_navigation_of_lower_or_equal_source_on_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            var derivedIdProperty = derivedDependentEntityBuilder.Property(Order.IdProperty, ConfigurationSource.Convention).Metadata;

            derivedDependentEntityBuilder.Relationship(
                principalEntityBuilder,
                derivedDependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.SpecialOrdersProperty.Name,
                new[] { derivedIdProperty },
                null,
                ConfigurationSource.DataAnnotation);

            Assert.Null(derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, dependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());

            Assert.Same(derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(0, dependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());
            Assert.Equal(1, derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Count());
        }

        [Fact]
        public void Can_only_set_base_type_if_relationship_with_conflicting_foreign_key_of_lower_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var principalShadowProp = principalEntityBuilder.Property("ShadowId", typeof(int), ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            var derivedIdProperty = derivedDependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;

            derivedDependentEntityBuilder.Relationship(
                principalEntityBuilder,
                derivedDependentEntityBuilder,
                Order.CustomerProperty.Name,
                null,
                new[] { derivedIdProperty },
                new[] { principalShadowProp.Metadata },
                ConfigurationSource.DataAnnotation);

            Assert.Null(derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());

            Assert.Same(derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, dependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());
            Assert.Equal(0, derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());
        }

        [Fact]
        public void Can_only_set_base_type_if_relationship_with_conflicting_foreign_key_of_lower_or_equal_source_on_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var principalShadowProp = principalEntityBuilder.Property("ShadowId", typeof(int), ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation);

            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);
            var derivedIdProperty = derivedDependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;

            derivedDependentEntityBuilder.Relationship(
                principalEntityBuilder,
                derivedDependentEntityBuilder,
                null,
                Customer.SpecialOrdersProperty.Name,
                new[] { derivedIdProperty },
                new[] { principalShadowProp.Metadata },
                ConfigurationSource.DataAnnotation);

            Assert.Null(derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Null(derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(1, derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());

            Assert.Same(derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation));
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Equal(0, dependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());
            Assert.Equal(1, derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Count());
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

            derivedPrincipalEntityBuilder.Relationship(
                derivedPrincipalEntityBuilder,
                dependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.OrdersProperty.Name,
                null,
                new[] { derivedIdProperty },
                ConfigurationSource.Convention);
            Assert.Equal(1, derivedPrincipalEntityBuilder.Metadata.GetDeclaredKeys().Count());

            Assert.Same(derivedPrincipalEntityBuilder,
                derivedPrincipalEntityBuilder.BaseType(principalEntityBuilder.Metadata, ConfigurationSource.Convention));

            Assert.Equal(1, principalEntityBuilder.Metadata.GetDeclaredKeys().Count());
            Assert.Equal(0, derivedPrincipalEntityBuilder.Metadata.GetDeclaredKeys().Count());
            Assert.Equal(0, derivedPrincipalEntityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Equal(0, principalEntityBuilder.Metadata.FindReferencingForeignKeys().Count());
            var fk = derivedPrincipalEntityBuilder.Metadata.FindReferencingForeignKeys().Single();
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
                derivedDependentEntityBuilder,
                Order.CustomerProperty.Name,
                Customer.SpecialOrdersProperty.Name,
                new[] { derivedIdProperty },
                null,
                ConfigurationSource.DataAnnotation);

            Assert.Same(derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.Convention));
            Assert.Equal(0, dependentEntityBuilder.Metadata.GetForeignKeys().Count());
            Assert.Equal(0, dependentEntityBuilder.Metadata.GetDeclaredProperties().Count());
            var fk = derivedDependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
            Assert.Equal(Customer.SpecialOrdersProperty.Name, fk.PrincipalToDependent.Name);
            Assert.Equal(Order.IdProperty.Name, fk.Properties.Single().Name);
        }

        [Fact]
        public void Setting_base_type_runs_conventions_on_other_derived_types()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new CoreConventionSetBuilder().CreateConventionSet());
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit);
            principalEntityBuilder.Ignore(Customer.EnumerableOrdersProperty.Name, ConfigurationSource.Explicit);
            principalEntityBuilder.Ignore(Customer.NotCollectionOrdersProperty.Name, ConfigurationSource.Explicit);
            principalEntityBuilder.Ignore(Customer.AmbiguousOrderProperty.Name, ConfigurationSource.Explicit);

            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var derivedDependentEntityBuilder = modelBuilder.Entity(typeof(SpecialOrder), ConfigurationSource.Explicit);
            Assert.NotNull(derivedDependentEntityBuilder.BaseType((Type)null, ConfigurationSource.DataAnnotation));
            var otherDerivedDependentEntityBuilder = modelBuilder.Entity(typeof(BackOrder), ConfigurationSource.Explicit);
            Assert.NotNull(otherDerivedDependentEntityBuilder.BaseType((Type)null, ConfigurationSource.DataAnnotation));

            var fk = dependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.Equal(Order.CustomerIdProperty.Name, fk.Properties.Single().Name);

            var derivedFk = derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, derivedFk.DependentToPrincipal.Name);
            Assert.Equal(Customer.SpecialOrdersProperty.Name, derivedFk.PrincipalToDependent.Name);
            Assert.Equal(Order.CustomerIdProperty.Name, derivedFk.Properties.Single().Name);

            var otherDerivedFk = otherDerivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, otherDerivedFk.DependentToPrincipal.Name);
            Assert.Null(otherDerivedFk.PrincipalToDependent);
            Assert.Equal(Order.CustomerIdProperty.Name, otherDerivedFk.Properties.Single().Name);

            Assert.Same(derivedDependentEntityBuilder,
                derivedDependentEntityBuilder.BaseType(dependentEntityBuilder.Metadata, ConfigurationSource.DataAnnotation));

            Assert.Equal(0, dependentEntityBuilder.Metadata.GetForeignKeys().Count());

            derivedFk = derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, derivedFk.DependentToPrincipal.Name);
            Assert.Equal(Customer.SpecialOrdersProperty.Name, derivedFk.PrincipalToDependent.Name);
            Assert.Equal(Order.CustomerIdProperty.Name, derivedFk.Properties.Single().Name);

            otherDerivedFk = otherDerivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single();
            Assert.Equal(Order.CustomerProperty.Name, otherDerivedFk.DependentToPrincipal.Name);
            Assert.Null(otherDerivedFk.PrincipalToDependent);
            Assert.Equal(Order.CustomerIdProperty.Name, otherDerivedFk.Properties.Single().Name);
        }

        private InternalModelBuilder CreateModelBuilder() => new InternalModelBuilder(new Model(), new ConventionSet());

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
        }

        private class SpecialOrder : Order, IEnumerable<Order>
        {
            public IEnumerator<Order> GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class BackOrder : Order
        {
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo UniqueProperty = typeof(Customer).GetProperty("Unique");
            public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty("Orders");
            public static readonly PropertyInfo EnumerableOrdersProperty = typeof(Customer).GetProperty("EnumerableOrders");
            public static readonly PropertyInfo NotCollectionOrdersProperty = typeof(Customer).GetProperty("NotCollectionOrders");
            public static readonly PropertyInfo SpecialOrdersProperty = typeof(Customer).GetProperty("SpecialOrders");
            public static readonly PropertyInfo AmbiguousOrderProperty = typeof(Customer).GetProperty("AmbiguousOrder");

            public int Id { get; set; }
            public Guid Unique { get; set; }
            public ICollection<Order> Orders { get; set; }
            public ICollection<SpecialOrder> SpecialOrders { get; set; }
            public SpecialOrder AmbiguousOrder { get; set; }
            public IEnumerable<Order> EnumerableOrders { get; set; }
            public Order NotCollectionOrders { get; set; }
        }

        private class SpecialCustomer : Customer
        {
        }
    }
}
