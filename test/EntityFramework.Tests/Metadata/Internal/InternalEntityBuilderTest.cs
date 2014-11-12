// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalEntityBuilderTest
    {
        [Fact]
        public void ForeignKey_returns_same_instance_for_clr_properties()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKeyBuilder = entityBuilder.ForeignKey(typeof(Customer), new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.Explicit);

            Assert.NotNull(foreignKeyBuilder);
            Assert.Same(foreignKeyBuilder, entityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void ForeignKey_returns_same_instance_for_property_names()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            entityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Explicit);
            entityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Explicit);

            var foreignKeyBuilder = entityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(foreignKeyBuilder);
            Assert.Same(foreignKeyBuilder, entityBuilder.ForeignKey(typeof(Customer), new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.Explicit));
        }

        [Fact]
        public void ForeignKey_returns_null_for_clr_properties_if_entity_type_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKeyBuilder = entityBuilder.ForeignKey(typeof(Customer), new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.Convention);

            Assert.Null(foreignKeyBuilder);
        }

        [Fact]
        public void ForeignKey_returns_null_for_property_names_if_entity_type_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.Explicit);
            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKeyBuilder = entityBuilder.ForeignKey(typeof(Customer).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention);

            Assert.Null(foreignKeyBuilder);
        }

        [Fact]
        public void Index_returns_same_instance_for_clr_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));

            var indexBuilder = entityBuilder.Index(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit);

            Assert.NotNull(indexBuilder);
            Assert.Same(indexBuilder, entityBuilder.Index(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Index_returns_same_instance_for_property_names()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));
            entityType.GetOrAddProperty(Order.IdProperty);
            entityType.GetOrAddProperty(Order.CustomerIdProperty);

            var indexBuilder = entityBuilder.Index(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(indexBuilder);
            Assert.Same(indexBuilder, entityBuilder.Index(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit));
        }

        [Fact]
        public void Key_returns_same_instance_for_clr_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));

            var keyBuilder = entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void Key_returns_same_instance_for_property_names()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));
            entityType.GetOrAddProperty(Order.IdProperty);
            entityType.GetOrAddProperty(Order.CustomerIdProperty);

            var keyBuilder = entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(keyBuilder);
            Assert.Same(keyBuilder, entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_only_override_lower_source_key_using_clr_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));
            entityType.GetOrAddProperty(Order.IdProperty);
            entityType.GetOrAddProperty(Order.CustomerIdProperty);

            var originalKeyBuilder = entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Convention);
            var newKeyBuilder = entityBuilder.Key(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            Assert.NotNull(newKeyBuilder);
            Assert.NotEqual(originalKeyBuilder, newKeyBuilder);
            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);

            var originalKeyBuilder2 = entityBuilder.Key(new[] { Order.IdProperty, Order.CustomerIdProperty }, ConfigurationSource.Explicit);
            Assert.NotNull(originalKeyBuilder2);
            Assert.NotEqual(originalKeyBuilder, originalKeyBuilder2);

            Assert.Null(entityBuilder.Key(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));
            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Can_only_override_lower_source_key_using_property_names()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));
            entityType.GetOrAddProperty(Order.IdProperty);
            entityType.GetOrAddProperty(Order.CustomerIdProperty);

            var originalKeyBuilder = entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Convention);
            var newKeyBuilder = entityBuilder.Key(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(newKeyBuilder);
            Assert.NotEqual(originalKeyBuilder, newKeyBuilder);
            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);

            var originalKeyBuilder2 = entityBuilder.Key(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, ConfigurationSource.Explicit);
            Assert.NotNull(originalKeyBuilder2);
            Assert.NotEqual(originalKeyBuilder, originalKeyBuilder2);

            Assert.Null(entityBuilder.Key(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation));
            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Can_only_override_existing_key_explicitly_using_clr_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));
            entityType.SetPrimaryKey(new[] { entityType.GetOrAddProperty(Order.IdProperty), entityType.GetOrAddProperty(Order.CustomerIdProperty) });

            Assert.Null(entityBuilder.Key(new[] { Order.IdProperty }, ConfigurationSource.DataAnnotation));

            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());

            Assert.NotNull(entityBuilder.Key(new[] { Order.IdProperty }, ConfigurationSource.Explicit));

            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);
        }

        [Fact]
        public void Can_only_override_existing_key_explicitly_using_property_names()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));
            entityType.SetPrimaryKey(new[] { entityType.GetOrAddProperty(Order.IdProperty), entityType.GetOrAddProperty(Order.CustomerIdProperty) });

            Assert.Null(entityBuilder.Key(new[] { Order.IdProperty.Name }, ConfigurationSource.DataAnnotation));

            Assert.Equal(new[] { Order.IdProperty.Name, Order.CustomerIdProperty.Name }, entityType.GetPrimaryKey().Properties.Select(p => p.Name).ToArray());

            Assert.NotNull(entityBuilder.Key(new[] { Order.IdProperty.Name }, ConfigurationSource.Explicit));

            Assert.Equal(Order.IdProperty.Name, entityType.GetPrimaryKey().Properties.Single().Name);
        }

        [Fact]
        public void Property_returns_same_instance_for_clr_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));

            var propertyBuilder = entityBuilder.Property(Order.IdProperty, ConfigurationSource.Explicit);

            Assert.NotNull(propertyBuilder);
            Assert.Same(propertyBuilder, entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Explicit));
        }

        [Fact]
        public void Property_returns_same_instance_for_property_names()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));

            var propertyBuilder = entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.NotNull(propertyBuilder);
            Assert.Same(propertyBuilder, entityBuilder.Property(Order.IdProperty, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_ignore_lower_source_property()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));

            Assert.Null(entityType.TryGetProperty(Order.IdProperty.Name));
            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));
            Assert.Null(entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Explicit));
        }

        [Fact]
        public void Cannot_ignore_higher_source_property()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.NotNull(entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Convention));
            Assert.NotNull(entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Convention));

            Assert.NotNull(entityType.TryGetProperty(Order.IdProperty.Name));
        }

        [Fact]
        public void Can_only_ignore_existing_property_explicitly()
        {
            var entityType = new Model().AddEntityType(typeof(Order));
            var property = entityType.AddProperty(Order.IdProperty.Name, typeof(int));
            var entityBuilder = new InternalEntityBuilder(entityType, new InternalModelBuilder(new Model(), null));
            Assert.Same(property, entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Convention).Metadata);

            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));

            Assert.Same(property, entityBuilder.Property(typeof(Order), Order.IdProperty.Name, ConfigurationSource.Convention).Metadata);
            Assert.False(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.DataAnnotation));
            Assert.NotNull(entityType.TryGetProperty(Order.IdProperty.Name));

            Assert.True(entityBuilder.Ignore(Order.IdProperty.Name, ConfigurationSource.Explicit));
        }

        [Fact]
        public void BuildRelationship_returns_same_instance_for_clr_types()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.BuildRelationship(typeof(Customer), typeof(Order), null, null, /*oneToOne:*/ true, ConfigurationSource.Convention);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder, customerEntityBuilder.BuildRelationship(customerEntityBuilder.Metadata, orderEntityBuilder.Metadata, null, null, /*oneToOne:*/ true, ConfigurationSource.Convention));
        }

        [Fact]
        public void BuildRelationship_returns_same_instance_for_entity_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.BuildRelationship(customerEntityBuilder.Metadata, orderEntityBuilder.Metadata, null, null, /*oneToOne:*/ true, ConfigurationSource.Explicit);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder, customerEntityBuilder.BuildRelationship(typeof(Customer), typeof(Order), null, null, /*oneToOne:*/ true, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void BuildRelationship_returns_null_for_clr_types_if_dependent_entity_type_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            modelBuilder.IgnoreEntity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = customerEntityBuilder.BuildRelationship(typeof(Customer), typeof(Order), null, null, /*oneToOne:*/ true, ConfigurationSource.Convention);

            Assert.Null(relationshipBuilder);
        }

        [Fact]
        public void BuildRelationship_returns_null_for_clr_types_if_principal_entity_type_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.BuildRelationship(typeof(Customer), typeof(Order), null, null, /*oneToOne:*/ true, ConfigurationSource.Convention);

            Assert.Null(relationshipBuilder);
        }

        [Fact]
        public void ReplaceForeignKey_returns_same_instance_same_entity()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.BuildRelationship(typeof(Customer), typeof(Order), null, null, /*oneToOne:*/ true, ConfigurationSource.DataAnnotation);
            var newRelationshipBuilder = orderEntityBuilder.ReplaceForeignKey(relationshipBuilder, new Property[0], new Property[0], ConfigurationSource.Explicit);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(newRelationshipBuilder, orderEntityBuilder.BuildRelationship(typeof(Customer), typeof(Order), null, null, /*oneToOne:*/ true, ConfigurationSource.Explicit));
        }

        [Fact]
        public void ReplaceForeignKey_returns_same_instance_different_entity()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.Key(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.BuildRelationship(typeof(Customer), typeof(Order), null, null, /*oneToOne:*/ true, ConfigurationSource.Convention);
            var newRelationshipBuilder = orderEntityBuilder.ReplaceForeignKey(relationshipBuilder.Invert(), relationshipBuilder.Metadata.ReferencedProperties, relationshipBuilder.Metadata.Properties, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(newRelationshipBuilder, orderEntityBuilder.BuildRelationship(typeof(Order), typeof(Customer), null, null, /*oneToOne:*/ true, ConfigurationSource.Explicit));
        }

        private class Order
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");

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
