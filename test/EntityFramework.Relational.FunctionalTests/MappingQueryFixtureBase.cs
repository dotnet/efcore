// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class MappingQueryFixtureBase
    {
        protected Model CreateModel()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    e.Key(c => c.CustomerID);
                    e.Property(c => c.CompanyName2).ForRelational(c => c.Column("Broken"));
                    e.ForRelational(t => t.Table("Broken", "wrong"));
                });

            modelBuilder.Entity<MappingQueryTestBase.MappedEmployee>(e =>
                {
                    e.Key(em => em.EmployeeID);
                    e.Property(em => em.City2).ForRelational(c => c.Column("City"));
                    e.ForRelational(t => t.Table("Employees", "dbo"));
                });

            modelBuilder.Entity<MappingQueryTestBase.MappedOrder>(e =>
                {
                    e.Key(o => o.OrderID);
                    e.Property(em => em.ShipVia2).ForRelational(c => c.Column("ShipVia"));
                    e.ForRelational(t => t.Table("Orders", "dbo"));
                });

            OnModelCreating(modelBuilder);

            return model;
        }

        protected virtual void OnModelCreating(BasicModelBuilder modelBuilder)
        {
        }
    }
}
