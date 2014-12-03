// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class NorthwindQueryFixtureBase
    {
        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(e =>
                {
                    e.Ignore(c => c.IsLondon);
                });

            modelBuilder.Entity<Employee>(e =>
                {
                    e.Ignore(em => em.Address);
                    e.Ignore(em => em.BirthDate);
                    e.Ignore(em => em.Extension);
                    e.Ignore(em => em.HireDate);
                    e.Ignore(em => em.HomePhone);
                    e.Ignore(em => em.LastName);
                    e.Ignore(em => em.Notes);
                    e.Ignore(em => em.Photo);
                    e.Ignore(em => em.PhotoPath);
                    e.Ignore(em => em.PostalCode);
                    e.Ignore(em => em.Region);
                    e.Ignore(em => em.TitleOfCourtesy);
                });

            modelBuilder.Entity<Product>(e =>
                {
                    e.Ignore(p => p.CategoryID);
                    e.Ignore(p => p.QuantityPerUnit);
                    e.Ignore(p => p.ReorderLevel);
                    e.Ignore(p => p.UnitPrice);
                    e.Ignore(p => p.UnitsInStock);
                    e.Ignore(p => p.UnitsOnOrder);
                });

            modelBuilder.Entity<Order>(e =>
                {
                    e.Ignore(o => o.EmployeeID);
                    e.Ignore(o => o.Freight);
                    e.Ignore(o => o.RequiredDate);
                    e.Ignore(o => o.ShipAddress);
                    e.Ignore(o => o.ShipCity);
                    e.Ignore(o => o.ShipCountry);
                    e.Ignore(o => o.ShipName);
                    e.Ignore(o => o.ShipPostalCode);
                    e.Ignore(o => o.ShipRegion);
                    e.Ignore(o => o.ShipVia);
                    e.Ignore(o => o.ShippedDate);
                    e.ForeignKey<Customer>(o => o.CustomerID);
                });

            modelBuilder.Entity<OrderDetail>(e =>
                {
                    e.Key(od => new { od.OrderID, od.ProductID });
                    e.ForeignKey<Product>(od => od.ProductID);
                    e.ForeignKey<Order>(od => od.OrderID);
                });

            // TODO: Use FAPIS when avail.
            var model = modelBuilder.Model;
            var productType = model.GetEntityType(typeof(Product));
            var customerType = model.GetEntityType(typeof(Customer));
            var orderType = model.GetEntityType(typeof(Order));
            var orderDetailType = model.GetEntityType(typeof(OrderDetail));

            var customerIdFk = orderType.ForeignKeys.Single();
            var productIdFk = orderDetailType.ForeignKeys.Single(fk => fk.ReferencedEntityType == productType);
            var orderIdFk = orderDetailType.ForeignKeys.Single(fk => fk.ReferencedEntityType == orderType);

            productType.AddNavigation("OrderDetails", productIdFk, pointsToPrincipal: false);
            customerType.AddNavigation("Orders", customerIdFk, pointsToPrincipal: false);
            orderType.AddNavigation("Customer", customerIdFk, pointsToPrincipal: true);
            orderType.AddNavigation("OrderDetails", orderIdFk, pointsToPrincipal: false);
            orderDetailType.AddNavigation("Product", productIdFk, pointsToPrincipal: true);
            orderDetailType.AddNavigation("Order", orderIdFk, pointsToPrincipal: true);
        }

        public abstract NorthwindContext CreateContext();
    }
}
