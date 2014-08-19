// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Northwind;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class NorthwindQueryFixtureBase
    {
        protected static Model CreateModel()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.CustomerID);
                    b.Property(c => c.CompanyName);
                    b.Property(c => c.ContactName);
                    b.Property(c => c.ContactTitle);
                    b.Property(c => c.Address);
                    b.Property(c => c.City);
                    b.Property(c => c.Region);
                    b.Property(c => c.PostalCode);
                    b.Property(c => c.Country);
                    b.Property(c => c.Phone);
                    b.Property(c => c.Fax);
                });

            modelBuilder.Entity<Employee>(b =>
                {
                    b.Key(e => e.EmployeeID);
                    b.Property(e => e.City);
                    b.Property(e => e.Country);
                    b.Property(e => e.FirstName);
                    b.Property(e => e.ReportsTo);
                    b.Property<string>("Title", shadowProperty: true);
                });

            modelBuilder.Entity<Product>(b =>
                {
                    b.Key(e => e.ProductID);
                    b.Property(c => c.ProductName);
                });

            modelBuilder.Entity<Order>(ps =>
                {
                    ps.Key(o => o.OrderID);
                    ps.Property(o => o.CustomerID);
                    ps.Property(o => o.OrderDate);
                });

            modelBuilder.Entity<OrderDetail>(b =>
                {
                    b.Key(od => new { od.OrderID, od.ProductID });
                    b.Property(od => od.UnitPrice);
                    b.Property(od => od.Quantity);
                    b.Property(od => od.Discount);
                    b.ForeignKey<Product>(od => od.ProductID);
                });

            // TODO: Use FAPIS when avail.
            var productType = model.GetEntityType(typeof(Product));
            var orderDetailType = model.GetEntityType(typeof(OrderDetail));

            var productIdFk = orderDetailType.ForeignKeys.Single();
            orderDetailType.AddNavigation(new Navigation(productIdFk, "Product", pointsToPrincipal: true));
            productType.AddNavigation(new Navigation(productIdFk, "OrderDetails", pointsToPrincipal: false));

            return model;
        }
    }
}
