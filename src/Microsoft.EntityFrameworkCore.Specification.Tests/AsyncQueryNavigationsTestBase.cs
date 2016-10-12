// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class AsyncQueryNavigationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
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

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected AsyncQueryNavigationsTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
            => await AssertQuery(query, query, assertOrder, entryCount, asserter);

        protected async Task AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    await efQuery(context.Set<TItem>()).ToArrayAsync(),
                    assertOrder,
                    asserter);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }
    }
}
