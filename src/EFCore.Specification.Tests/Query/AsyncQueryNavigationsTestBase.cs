// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AsyncQueryNavigationsTestBase<TFixture> : AsyncQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {

        protected AsyncQueryNavigationsTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual async Task Include_with_multiple_optional_navigations()
        {
            await AssertQuery<OrderDetail>(
                ods => ods
                    .Include(od => od.Order.Customer)
                    .Where(od => od.Order.Customer.City == "London"),
                entryCount: 164);
        }

        [ConditionalFact]
        public virtual async Task Multiple_include_with_multiple_optional_navigations()
        {
            await AssertQuery<OrderDetail>(
                ods => ods
                    .Include(od => od.Order.Customer)
                    .Include(od => od.Product)
                    .Where(od => od.Order.Customer.City == "London"),
                entryCount: 221);
        }
    }
}
