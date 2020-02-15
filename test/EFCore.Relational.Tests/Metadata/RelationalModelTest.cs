// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalModelTest
    {
        [ConditionalFact]
        public void Can_use_relational_model_with_tables()
        {
            var model = CreateTestModel();

            Assert.Equal(6, model.GetEntityTypes().Count());
            Assert.Equal(2, model.GetTables().Count());
            Assert.Empty(model.GetViews());

            var orderType = model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetTableMappings().Single();
            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.CustomerId), nameof(Order.OrderDate), nameof(Order.OrderId) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersTable = orderMapping.Table;
            Assert.Same(ordersTable, model.FindTable(ordersTable.Name, ordersTable.Schema));
            Assert.Equal(
                new[] { "OrderDetails.BillingAddress#Address", "OrderDetails.ShippingAddress#Address", nameof(Order), nameof(OrderDetails) },
                ordersTable.EntityTypeMappings.Select(m => m.EntityType.DisplayName()));
            Assert.Equal(new[] {
                    nameof(Order.CustomerId),
                    "Details_BillingAddress_City",
                    "Details_BillingAddress_Street",
                    "Details_ShippingAddress_City",
                    "Details_ShippingAddress_Street",
                    nameof(Order.OrderDate),
                    nameof(Order.OrderId)
            },
                ordersTable.Columns.Select(m => m.Name));
            Assert.Equal("Order", ordersTable.Name);
            Assert.Null(ordersTable.Schema);
            Assert.True(ordersTable.IsMigratable);
            Assert.True(ordersTable.IsSplit);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));
            Assert.False(orderDate.IsColumnNullable());

            var orderDateMapping = orderDate.GetTableColumnMappings().Single();
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.TableMapping);

            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            Assert.Same(ordersTable, orderDetailsType.GetTableMappings().Single().Table);
            Assert.Equal(ordersTable.GetReferencingInternalForeignKeys(orderType), ordersTable.GetInternalForeignKeys(orderDetailsType));

            var orderDetailsDate = orderDetailsType.FindProperty(nameof(OrderDetails.OrderDate));
            Assert.True(orderDetailsDate.IsColumnNullable());

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersTable.FindColumn("OrderDate"));
            Assert.Equal(new[] { orderDate, orderDetailsDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));
            Assert.Equal("OrderDate", orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.Type);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersTable, orderDateColumn.Table);

            var customerType = model.FindEntityType(typeof(Customer));
            var customerTable = customerType.GetTableMappings().Single().Table;
            Assert.Equal("Customer", customerTable.Name);

            var specialCustomerType = model.FindEntityType(typeof(SpecialCustomer));
            Assert.Same(customerTable, specialCustomerType.GetTableMappings().Single().Table);
        }

        [ConditionalFact]
        public void Can_use_relational_model_with_views()
        {
            var model = CreateTestModel(mapToViews: true);

            Assert.Equal(6, model.GetEntityTypes().Count());
            Assert.Equal(2, model.GetViews().Count());
            Assert.Empty(model.GetTables());

            var orderType = model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetViewMappings().Single();
            Assert.Null(orderType.GetTableMappings());
            Assert.Same(orderType.GetViewMappings(), orderType.GetViewOrTableMappings());
            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.CustomerId), nameof(Order.OrderDate), nameof(Order.OrderId) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersView = orderMapping.View;
            Assert.Same(ordersView, model.FindView(ordersView.Name, ordersView.Schema));
            Assert.Equal(
                new[] { "OrderDetails.BillingAddress#Address", "OrderDetails.ShippingAddress#Address", nameof(Order), nameof(OrderDetails) },
                ordersView.EntityTypeMappings.Select(m => m.EntityType.DisplayName()));
            Assert.Equal(new[] {
                    nameof(Order.CustomerId),
                    "Details_BillingAddress_City",
                    "Details_BillingAddress_Street",
                    "Details_ShippingAddress_City",
                    "Details_ShippingAddress_Street",
                    nameof(Order.OrderDate),
                    nameof(Order.OrderId)
            },
                ordersView.Columns.Select(m => m.Name));
            Assert.Equal("OrderView", ordersView.Name);
            Assert.Null(ordersView.Schema);
            Assert.Null(ordersView.ViewDefinition);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));
            Assert.False(orderDate.IsColumnNullable());

            var orderDateMapping = orderDate.GetViewColumnMappings().Single();
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.ViewMapping);

            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            Assert.Same(ordersView, orderDetailsType.GetViewMappings().Single().View);
            Assert.Equal(ordersView.GetReferencingInternalForeignKeys(orderType), ordersView.GetInternalForeignKeys(orderDetailsType));

            var orderDetailsDate = orderDetailsType.FindProperty(nameof(OrderDetails.OrderDate));
            Assert.True(orderDetailsDate.IsColumnNullable());

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersView.FindColumn("OrderDate"));
            Assert.Equal(new[] { orderDate, orderDetailsDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));
            Assert.Equal("OrderDate", orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.Type);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersView, orderDateColumn.Table);

            var customerType = model.FindEntityType(typeof(Customer));
            var customerView = customerType.GetViewMappings().Single().Table;
            Assert.Equal("CustomerView", customerView.Name);

            var specialCustomerType = model.FindEntityType(typeof(SpecialCustomer));
            Assert.Same(customerView, specialCustomerType.GetViewMappings().Single().Table);
        }

        private IModel CreateTestModel(bool mapToViews = false)
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Entity<Customer>(cb =>
            {
                if (mapToViews)
                {
                    cb.ToView("CustomerView");
                }
            });
            modelBuilder.Entity<SpecialCustomer>();
            modelBuilder.Entity<Order>(ob =>
            {
                ob.Property(od => od.OrderDate).HasColumnName("OrderDate");
                ob.OwnsOne(o => o.Details, odb =>
                {
                    odb.Property(od => od.OrderDate).HasColumnName("OrderDate");
                    odb.OwnsOne(od => od.BillingAddress);
                    odb.OwnsOne(od => od.ShippingAddress);
                });

                if (mapToViews)
                {
                    ob.ToView("OrderView");
                }
            });

            var model = modelBuilder.FinalizeModel();
            return model;
        }

        protected virtual ModelBuilder CreateConventionModelBuilder() => RelationalTestHelpers.Instance.CreateConventionBuilder();

        private enum MyEnum : ulong
        {
            Sun,
            Mon,
            Tue
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public short SomeShort { get; set; }
            public MyEnum EnumValue { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }

        private class SpecialCustomer : Customer
        {
            public string Speciality { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public DateTime OrderDate { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public DateTime OrderDate { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
            public Address BillingAddress { get; set; }
            public Address ShippingAddress { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
        }
    }
}
