// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class MappingQueryFixtureBase
    {
        protected abstract string DatabaseSchema { get;  }

        protected Model CreateModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    e.Key(c => c.CustomerID);
                    e.Property(c => c.CompanyName2).Metadata.Relational().Column = "Broken";
                    e.Metadata.Relational().Table = "Broken";
                    if (!string.IsNullOrEmpty(DatabaseSchema))
                    {
                        e.Metadata.Relational().Schema = "wrong";
                    }
                });

            modelBuilder.Entity<MappingQueryTestBase.MappedEmployee>(e =>
                {
                    e.Key(em => em.EmployeeID);
                    e.Property(em => em.City2).Metadata.Relational().Column = "City";
                    e.Metadata.Relational().Table = "Employees";
                    e.Metadata.Relational().Schema = DatabaseSchema;
                });

            modelBuilder.Entity<MappingQueryTestBase.MappedOrder>(e =>
                {
                    e.Key(o => o.OrderID);
                    e.Property(em => em.ShipVia2).Metadata.Relational().Column = "ShipVia";
                    e.Metadata.Relational().Table = "Orders";
                    e.Metadata.Relational().Schema = DatabaseSchema;
                });

            OnModelCreating(modelBuilder);

            return modelBuilder.Model;
        }

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
