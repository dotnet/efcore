// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests.TestModels.NorthwindSproc;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class NorthwindSprocQueryRelationalFixture : NorthwindQueryFixtureBase
    {
        public override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.Entity<CustomerOrderHistory>().Key(coh => coh.ProductName);
            model.Entity<MostExpensiveProduct>().Key(mep => mep.TenMostExpensiveProducts);
        }
    }
}
