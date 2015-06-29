// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalEntityBuilderTest
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
        public void Can_add_relationship_if_principal_type_ignored_at_same_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
                null,
                null,
                ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_add_relationship_if_dependent_type_ignored_at_same_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore(typeof(Order), ConfigurationSource.DataAnnotation);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
                null,
                null,
                ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_add_relationship_if_navigation_to_dependent_ignored_at_same_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.NotNull(dependentEntityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
                null,
                Customer.OrdersProperty.Name,
                ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_add_relationship_if_navigation_to_principal_ignored_at_same_or_equal_source()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.NotNull(principalEntityBuilder.Relationship(
                typeof(Customer),
                typeof(Order),
                Order.CustomerProperty.Name,
                null,
                ConfigurationSource.DataAnnotation));
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
            var shadowProperty = dependentEntityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);

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
                    new[] { entityBuilder.Property(typeof(int), "Shadow2", ConfigurationSource.Convention).Metadata.Name, property.Name },
                    ConfigurationSource.Convention));

            Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(
                (entityBuilder, property) => entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Explicit));
        }

        private void Test_removing_relationship_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = dependentEntityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);
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
            var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);

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
                (entityBuilder, property) => entityBuilder.Property(property.ClrType, property.Name, ConfigurationSource.Explicit));
        }

        private void Test_removing_index_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);
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

            var keyBuilder = entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Key_throws_for_clr_properties_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(Strings.NoClrProperty(Customer.UniqueProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    entityBuilder.Key(new[] { Customer.UniqueProperty }, ConfigurationSource.DataAnnotation)).Message);
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
            entityBuilder.Property(Order.CustomerIdProperty.PropertyType, Order.CustomerIdProperty.Name, ConfigurationSource.Convention);

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
            var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);

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
                            entityBuilder.Property(typeof(int), "Shadow2", ConfigurationSource.Convention).Metadata.Name,
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
                (entityBuilder, property) => entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Explicit));
        }

        private void Test_removing_key_does_not_remove_contained_shadow_properties_if_referenced_elsewhere(Func<InternalEntityTypeBuilder, Property, object> shadowConfig)
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var shadowProperty = entityBuilder.Property(typeof(Guid), "Shadow", ConfigurationSource.Convention);
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
                ConfigurationSource.Convention);
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
        }

        [Fact]
        public void PrimaryKey_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void PrimaryKey_throws_for_clr_properties_if_they_do_not_exist()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.Equal(Strings.NoClrProperty(Customer.UniqueProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    entityBuilder.PrimaryKey(new[] { Customer.UniqueProperty }, ConfigurationSource.DataAnnotation)).Message);
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
        public void PrimaryKey_works_for_property_names_for_shadow_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.CustomerIdProperty.PropertyType, Order.CustomerIdProperty.Name, ConfigurationSource.Convention);

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
        public void Can_only_override_lower_source_key_using_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            var originalKeyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Convention);
            var newKeyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            Assert.NotNull(newKeyBuilder);
            Assert.NotEqual(originalKeyBuilder, newKeyBuilder);
            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);

            var originalKeyBuilder2 = entityBuilder.PrimaryKey(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit);
            Assert.NotNull(originalKeyBuilder2);
            Assert.NotEqual(originalKeyBuilder, originalKeyBuilder2);

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));
            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Can_only_override_lower_source_key_using_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            var originalKeyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);
            var newKeyBuilder = entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(newKeyBuilder);
            Assert.NotEqual(originalKeyBuilder, newKeyBuilder);
            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);

            var originalKeyBuilder2 = entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit);
            Assert.NotNull(originalKeyBuilder2);
            Assert.NotEqual(originalKeyBuilder, originalKeyBuilder2);

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation));
            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Can_only_override_existing_key_explicitly_using_clr_properties()
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
        public void Can_only_override_existing_key_explicitly_using_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;
            entityType.SetPrimaryKey(new[] { entityType.GetOrAddProperty(Order.IdProperty), entityType.GetOrAddProperty(Order.CustomerIdProperty) });

            Assert.Null(entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation));

            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());

            Assert.NotNull(entityBuilder.PrimaryKey(new[] { Order.IdProperty.Name }, ConfigurationSource.Explicit));

            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);
        }

        [Fact]
        public void Property_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var propertyBuilder = entityBuilder.Property(Order.IdProperty, ConfigurationSource.Explicit);

            Assert.NotNull(propertyBuilder);
            Assert.Same(propertyBuilder, entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Explicit));
        }

        [Fact]
        public void Property_returns_same_instance_for_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var propertyBuilder = entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.NotNull(propertyBuilder);
            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation));
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
            Assert.Null(entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.Equal(Strings.PropertyIgnoredExplicitly(Order.IdProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Cannot_ignore_same_or_higher_source_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.NotNull(entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.NotNull(entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.NotNull(entityType.FindProperty(Order.IdProperty.Name));
        }

        [Fact]
        public void Cannot_ignore_existing_property()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var entityType = entityBuilder.Metadata;
            var property = entityType.AddProperty(Order.IdProperty.Name, typeof(int));
            Assert.Same(property, entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Convention).Metadata);

            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.Same(property, entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Convention).Metadata);
            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(entityType.FindProperty(Order.IdProperty.Name));

            Assert.Equal(Strings.PropertyAddedExplicitly(Order.IdProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_foreign_key_preserving_the_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var key = principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

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

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.Empty(dependentEntityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
            var newFk = dependentEntityBuilder.Metadata.GetForeignKeys().Single();
            Assert.Same(fk.EntityType, newFk.EntityType);
            Assert.Same(fk.PrincipalEntityType, newFk.PrincipalEntityType);
            Assert.NotEqual(fk.Properties, newFk.EntityType.Properties);
            Assert.Same(fk.PrincipalKey, newFk.PrincipalKey);
            Assert.Equal(Order.CustomerProperty.Name, newFk.DependentToPrincipal.Name);
            Assert.Equal(Customer.OrdersProperty.Name, newFk.PrincipalToDependent.Name);
        }

        [Fact]
        public void Cannot_ignore_property_that_is_part_of_same_or_higher_source_foreign_key()
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

            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerIdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.NotEmpty(dependentEntityBuilder.Metadata.Properties.Where(p => p.Name == Order.CustomerIdProperty.Name));
            Assert.NotEmpty(dependentEntityBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Can_ignore_property_that_is_part_of_lower_source_index()
        {
            var modelBuilder = CreateModelBuilder();
            var entityType = modelBuilder.Metadata.AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityTypeBuilder(entityType, modelBuilder);

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
            Assert.Same(fk.EntityType, newFk.EntityType);
            Assert.Same(fk.PrincipalEntityType, newFk.PrincipalEntityType);
            Assert.Equal(fk.Properties, newFk.EntityType.Properties);
            Assert.NotSame(fk.PrincipalKey, newFk.PrincipalKey);
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
            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.False(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));

            Assert.True(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_override_same_or_lower_source_conflicting_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var conflictingForeignKeyBuilder = dependentEntityBuilder.ForeignKey(
                typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, conflictingForeignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, conflictingForeignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.False(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.False(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));

            Assert.Same(conflictingForeignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
            Assert.Same(conflictingForeignKeyBuilder.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);

            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            Assert.Same(foreignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name).ForeignKey);
            Assert.Same(foreignKeyBuilder.Metadata, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name).ForeignKey);
            Assert.Same(foreignKeyBuilder.Metadata, dependentEntityBuilder.Metadata.GetForeignKeys().Single());
        }

        [Fact]
        public void Can_ignore_same_or_lower_source_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());

            foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.False(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));

            Assert.Equal(Strings.NavigationIgnoredExplicitly(Order.CustomerProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Explicit)).Message);

            Assert.Equal(Strings.NavigationIgnoredExplicitly(Customer.OrdersProperty.Name, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Cannot_ignore_same_or_higher_source_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.True(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.False(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.True(dependentEntityBuilder.CanAddNavigation(Order.CustomerProperty.Name, ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.CanAddNavigation(Customer.OrdersProperty.Name, ConfigurationSource.Explicit));
            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.DataAnnotation));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));

            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Convention));
            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Convention));
            Assert.False(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.NotNull(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));

            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit));
            Assert.False(dependentEntityBuilder.Navigation(null, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Cannot_ignore_existing_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKey = dependentEntityBuilder.Metadata.AddForeignKey(
                new[]
                    {
                        dependentEntityBuilder.Metadata.GetOrAddProperty(Order.CustomerIdProperty.Name, typeof(int)),
                        dependentEntityBuilder.Metadata.GetOrAddProperty(Order.CustomerUniqueProperty.Name, typeof(Guid))
                    },
                principalEntityBuilder.Metadata.GetPrimaryKey());
            var navigationToPrincipal = dependentEntityBuilder.Metadata.AddNavigation(Order.CustomerProperty.Name, foreignKey, pointsToPrincipal: true);
            var navigationToDependent = principalEntityBuilder.Metadata.AddNavigation(Customer.OrdersProperty.Name, foreignKey, pointsToPrincipal: false);
            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKey, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKey, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));

            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKey, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKey, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));
            Assert.False(dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.False(principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.Same(navigationToPrincipal, dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Same(navigationToDependent, principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));

            Assert.Equal(Strings.NavigationAddedExplicitly(Order.CustomerProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    dependentEntityBuilder.Ignore(Order.CustomerProperty.Name, ConfigurationSource.Explicit)).Message);
            Assert.Equal(Strings.NavigationAddedExplicitly(Customer.OrdersProperty.Name, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    principalEntityBuilder.Ignore(Customer.OrdersProperty.Name, ConfigurationSource.Explicit)).Message);

            Assert.NotNull(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.NotNull(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));

            Assert.True(dependentEntityBuilder.Navigation(null, foreignKey, pointsToPrincipal: true, configurationSource: ConfigurationSource.Explicit));
            Assert.True(principalEntityBuilder.Navigation(null, foreignKey, pointsToPrincipal: false, configurationSource: ConfigurationSource.Explicit));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
        }

        [Fact]
        public void Cannot_add_navigations_to_higher_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.False(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));
            Assert.False(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));

            Assert.Null(dependentEntityBuilder.Metadata.FindNavigation(Order.CustomerProperty.Name));
            Assert.Null(principalEntityBuilder.Metadata.FindNavigation(Customer.OrdersProperty.Name));
        }

        [Fact]
        public void Navigation_not_configured_if_it_conflicts_with_an_existing_one()
        {
            var modelBuilder = CreateModelBuilder();
            var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            principalEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var foreignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);
            Assert.True(principalEntityBuilder.Navigation(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: false, configurationSource: ConfigurationSource.Convention));
            Assert.True(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.DataAnnotation));
            var newForeignKeyBuilder = dependentEntityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.IdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.False(dependentEntityBuilder.Navigation(Order.CustomerProperty.Name, newForeignKeyBuilder.Metadata, pointsToPrincipal: true, configurationSource: ConfigurationSource.Convention));

            Assert.Equal(Customer.OrdersProperty.Name, foreignKeyBuilder.Metadata.PrincipalToDependent.Name);
            Assert.Equal(Order.CustomerProperty.Name, foreignKeyBuilder.Metadata.DependentToPrincipal.Name);

            Assert.Null(newForeignKeyBuilder.Metadata.PrincipalToDependent);
            Assert.Null(newForeignKeyBuilder.Metadata.DependentToPrincipal);
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
            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Explicit, true)).Message);
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
            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    principalEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Explicit, true)).Message);
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
            Assert.Equal(Strings.NavigationIgnoredExplicitly(Customer.OrdersProperty.Name, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, Customer.OrdersProperty.Name, ConfigurationSource.Explicit, false)).Message);
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
            Assert.Equal(Strings.NavigationIgnoredExplicitly(Order.CustomerProperty.Name, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    dependentEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Explicit, false)).Message);
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
            var customerRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, "NotCollectionOrders", ConfigurationSource.Convention, true, true);
            Assert.NotNull(customerRelationship);

            Assert.Null(orderEntityBuilder.Relationship(typeof(Order), typeof(Customer), "NotCollectionOrders", Order.CustomerProperty.Name, ConfigurationSource.Convention, true, true));

            Assert.Null(orderRelationship.Metadata.PrincipalToDependent);
            Assert.Equal(Order.CustomerProperty.Name, orderRelationship.Metadata.DependentToPrincipal.Name);
            Assert.Equal("NotCollectionOrders", customerRelationship.Metadata.PrincipalToDependent.Name);
            Assert.Null(customerRelationship.Metadata.DependentToPrincipal);
        }

        [Fact]
        public void Principal_conflicting_relationship_is_not_removed_if_dependent_conflicting_relationship_cannot_be_removed()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var orderRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), Order.CustomerProperty.Name, null, ConfigurationSource.Convention, true, true);
            Assert.NotNull(orderRelationship);
            var customerRelationship = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, "NotCollectionOrders", ConfigurationSource.DataAnnotation, true, true);
            Assert.NotNull(customerRelationship);

            Assert.Null(orderEntityBuilder.Relationship(typeof(Order), typeof(Customer), "NotCollectionOrders", Order.CustomerProperty.Name, ConfigurationSource.Convention, true, true));

            Assert.Null(orderRelationship.Metadata.PrincipalToDependent);
            Assert.Equal(Order.CustomerProperty.Name, orderRelationship.Metadata.DependentToPrincipal.Name);
            Assert.Equal("NotCollectionOrders", customerRelationship.Metadata.PrincipalToDependent.Name);
            Assert.Null(customerRelationship.Metadata.DependentToPrincipal);
        }

        private InternalModelBuilder CreateModelBuilder()
        {
            return new InternalModelBuilder(new Model(), new ConventionSet());
        }

        private class Order
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");
            public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty("Customer");

            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Guid CustomerUnique { get; set; }
            public Customer Customer { get; set; }

            public Order OrderCustomer { get; set; }
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");
            public static readonly PropertyInfo UniqueProperty = typeof(Customer).GetProperty("Unique");
            public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty("Orders");

            public int Id { get; set; }
            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
            public ICollection<Order> Orders { get; set; }

            public IEnumerable<Order> EnumerableOrders { get; set; }
            public Order NotCollectionOrders { get; set; }
        }
    }
}
