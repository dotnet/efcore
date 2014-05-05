// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity;
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
            ;

            modelBuilder
                .Entity<Product>()
                .Key(e => e.ProductID)
                .Properties(ps => ps.Property(c => c.ProductName))
                .StorageName("Products");
            ;

            modelBuilder
                .Entity<Order>()
                .Key(o => o.OrderID)
                .Properties(ps =>
                    {
                        ps.Property(c => c.CustomerID);
                        ps.Property(c => c.OrderDate);
                    })
                .StorageName("Orders");
            ;

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

            return model;
        }

        public abstract ImmutableDbContextOptions Configuration { get; }
    }
}
