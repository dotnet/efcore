// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.NorthwindSproc;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindSprocQueryRelationalFixture : NorthwindQueryFixtureBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CustomerOrderHistory>().HasKey(coh => coh.ProductName);
            modelBuilder.Entity<MostExpensiveProduct>().HasKey(mep => mep.TenMostExpensiveProducts);
        }
    }
}
