// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class NorthwindQueryFixtureBase
    {
        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>();

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
                    e.Ignore(p => p.UnitsOnOrder);
                });

            modelBuilder.Entity<Order>(e =>
                {
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
                });

            modelBuilder.Entity<OrderDetail>(e => { e.Key(od => new { od.OrderID, od.ProductID }); });
        }

        public abstract NorthwindContext CreateContext();
    }
}
