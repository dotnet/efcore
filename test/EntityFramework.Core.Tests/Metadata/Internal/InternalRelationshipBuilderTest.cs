// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Internal
{
    public class InternalRelationshipBuilderTest
    {
        [Fact]
        public void ForeignKey_returns_same_instance_if_no_navigations()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionsDispatcher());
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            orderEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true)
                .ForeignKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder, customerEntityBuilder.Relationship(typeof(Order), typeof(Customer), null, null, ConfigurationSource.Convention, true, true)
                .ForeignKey(typeof(Order), new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));
            Assert.Null(customerEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, false, true)
                .ForeignKey(typeof(Order).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention));
        }

        [Fact]
        public void ReferencedKey_does_not_return_same_instance_if_no_navigations_or_foreign_key()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionsDispatcher());
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            orderEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.DataAnnotation, true, true)
                .ReferencedKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder,
                relationshipBuilder.ReferencedKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation));
            Assert.NotSame(relationshipBuilder, customerEntityBuilder.Relationship(typeof(Order), typeof(Customer), null, null, ConfigurationSource.DataAnnotation, true, true)
                .ReferencedKey(typeof(Order).FullName, new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.Convention));
            Assert.Null(relationshipBuilder.ReferencedKey(typeof(Order), new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention));

            Assert.Equal(2, customerEntityBuilder.Metadata.ForeignKeys.Count);
        }

        [Fact]
        public void Can_only_override_lower_source_Unique()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionsDispatcher());
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true);
            Assert.True(relationshipBuilder.Metadata.IsUnique.Value);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.Unique(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.Unique(false, ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationshipBuilder);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            Assert.Null(relationshipBuilder.Unique(true, ConfigurationSource.Convention));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
        }

        [Fact]
        public void Can_only_override_existing_Unique_value_explicitly()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionsDispatcher());
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var customerKeyBuilder = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKey = orderEntityBuilder.Metadata.AddForeignKey(
                new[]
                    {
                        orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                        orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                    },
                customerKeyBuilder.Metadata);
            foreignKey.IsUnique = true;

            var relationshipBuilder = orderEntityBuilder.Relationship(foreignKey, true, ConfigurationSource.Convention);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            Assert.Null(relationshipBuilder.Unique(false, ConfigurationSource.Convention));
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.Unique(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.Unique(false, ConfigurationSource.Explicit);
            Assert.NotNull(relationshipBuilder);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
        }

        [Fact]
        public void Can_only_override_lower_source_Required()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionsDispatcher());
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true);
            Assert.Null(relationshipBuilder.Metadata.IsRequired);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            Assert.True(relationshipBuilder.Required(true, ConfigurationSource.Convention));
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            Assert.True(relationshipBuilder.Required(false, ConfigurationSource.DataAnnotation));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            Assert.False(relationshipBuilder.Required(true, ConfigurationSource.Convention));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
        }

        [Fact]
        public void Can_only_override_existing_Required_value_explicitly()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionsDispatcher());
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var customerIdProperty = orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;
            var customerUniqueProperty = orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata;
            customerUniqueProperty.IsNullable = false;

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true)
                .ForeignKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            Assert.False(relationshipBuilder.Required(false, ConfigurationSource.Convention));
            Assert.Null(customerIdProperty.IsNullable);
            Assert.False(customerUniqueProperty.IsNullable.Value);

            Assert.True(relationshipBuilder.Required(true, ConfigurationSource.Convention));
            Assert.False(customerIdProperty.IsNullable.Value);
            Assert.False(customerUniqueProperty.IsNullable.Value);

            Assert.True(relationshipBuilder.Required(false, ConfigurationSource.Explicit));
            Assert.False(customerIdProperty.IsNullable.Value);
            Assert.True(customerUniqueProperty.IsNullable.Value);
        }

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
