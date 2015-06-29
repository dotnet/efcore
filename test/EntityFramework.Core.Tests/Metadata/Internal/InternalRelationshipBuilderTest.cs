// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Internal
{
    public class InternalRelationshipBuilderTest
    {
        [Fact]
        public void ForeignKey_returns_same_instance_if_no_navigations()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            orderEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true)
                .ForeignKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder, customerEntityBuilder.Relationship(typeof(Order), typeof(Customer), null, null, ConfigurationSource.Convention, true, true)
                .ForeignKey(typeof(Order), new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void PrincipalKey_does_not_return_same_instance_if_no_navigations_or_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            orderEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.DataAnnotation, true, true)
                .PrincipalKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder,
                relationshipBuilder.PrincipalKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation));
            Assert.Null(relationshipBuilder.PrincipalKey(typeof(Order), new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention));

            Assert.Equal(1, customerEntityBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
        public void Can_only_override_lower_source_Unique()
        {
            var modelBuilder = CreateInternalModelBuilder();
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
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true);
            Assert.Null(relationshipBuilder.Metadata.IsRequired);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            relationshipBuilder = relationshipBuilder.Required(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            relationshipBuilder = relationshipBuilder.Required(false, ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationshipBuilder);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            Assert.Null(relationshipBuilder.Required(true, ConfigurationSource.Convention));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
        }

        [Fact]
        public void Can_only_override_existing_Required_value_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var customerIdProperty = orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;
            var customerUniqueProperty = orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata;
            customerUniqueProperty.IsNullable = false;

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true)
                .ForeignKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            Assert.Null(relationshipBuilder.Required(false, ConfigurationSource.Convention));
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Null(customerIdProperty.IsNullable);
            Assert.False(customerUniqueProperty.IsNullable.Value);

            relationshipBuilder = relationshipBuilder.Required(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.False(customerIdProperty.IsNullable.Value);
            Assert.False(customerUniqueProperty.IsNullable.Value);

            relationshipBuilder = relationshipBuilder.Required(false, ConfigurationSource.Explicit);
            Assert.NotNull(relationshipBuilder);
            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.False(fk.IsRequired);
            Assert.False(customerIdProperty.IsNullable.Value);
            Assert.False(customerUniqueProperty.IsNullable.Value);
            Assert.True(fk.Properties[0].IsNullable);
            Assert.True(fk.Properties[1].IsNullable);
        }

        [Fact]
        public void Can_only_invert_same_or_lower_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder
                .Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, isUnique: true);

            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.EntityType);

            relationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.Convention);
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.EntityType);

            relationshipBuilder = relationshipBuilder.PrincipalKey(
                orderEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Convention).Metadata.Properties,
                ConfigurationSource.Convention);

            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.Convention));
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.EntityType);

            relationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation);
            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.EntityType);

            relationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation);
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.EntityType);

            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.Convention));
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.EntityType);

            relationshipBuilder = relationshipBuilder.ForeignKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.DataAnnotation));
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.EntityType);
        }

        private InternalModelBuilder CreateInternalModelBuilder()
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
