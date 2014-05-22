// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Northwind;

namespace Microsoft.Data.FunctionalTests
{
    public abstract class NorthwindQueryFixtureBase
    {
        protected static Model CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.CustomerID)
                .Properties(ps =>
                {
                    ps.Property(c => c.CompanyName);
                    ps.Property(c => c.ContactName);
                    ps.Property(c => c.ContactTitle);
                    ps.Property(c => c.Address);
                    ps.Property(c => c.City);
                    ps.Property(c => c.Region);
                    ps.Property(c => c.PostalCode);
                    ps.Property(c => c.Country);
                    ps.Property(c => c.Phone);
                    ps.Property(c => c.Fax);
                })
                .StorageName("Customers");

            modelBuilder
                .Entity<Employee>()
                .Key(e => e.EmployeeID)
                .Properties(ps => ps.Property(c => c.City))
                .StorageName("Employees");

            modelBuilder
                .Entity<Product>()
                .Key(e => e.ProductID)
                .Properties(ps => ps.Property(c => c.ProductName))
                .StorageName("Products");

            modelBuilder
                .Entity<Order>()
                .Key(o => o.OrderID)
                .Properties(ps =>
                {
                    ps.Property(c => c.CustomerID);
                    ps.Property(c => c.OrderDate);
                })
                .StorageName("Orders");

            modelBuilder
                .Entity<OrderDetail>()
                .Key(od => new { od.OrderID, od.ProductID })
                .Properties(ps =>
                {
                    ps.Property(c => c.UnitPrice);
                    ps.Property(c => c.Quantity);
                    ps.Property(c => c.Discount);
                })
                .StorageName("Order Details");

            // TODO: Use FAPIS when avail.
            var productType = model.GetEntityType(typeof(Product));
            var orderDetailType = model.GetEntityType(typeof(OrderDetail));

            var productIdFk
                = orderDetailType.AddForeignKey(productType.GetKey(), orderDetailType.GetProperty("ProductID"));

            orderDetailType.AddNavigation(new Navigation(productIdFk, "Product"));
            productType.AddNavigation(new Navigation(productIdFk, "OrderDetails"));

            return model;
        }
    }
}
