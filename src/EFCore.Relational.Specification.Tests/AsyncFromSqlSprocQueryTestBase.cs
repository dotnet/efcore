// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.NorthwindSproc;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class AsyncFromSqlSprocQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure()
        {
            using (var context = CreateContext())
            {
                var actual = await context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc)
                    .ToArrayAsync();

                Assert.Equal(10, actual.Length);

                Assert.True(actual.Any(
                    mep =>
                        mep.TenMostExpensiveProducts == "Côte de Blaye"
                        && mep.UnitPrice == 263.50m));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_with_parameter()
        {
            using (var context = CreateContext())
            {
                var actual = await context
                    .Set<CustomerOrderHistory>()
                    .FromSql(CustomerOrderHistorySproc, "ALFKI")
                    .ToArrayAsync();

                Assert.Equal(11, actual.Length);

                Assert.True(
                    actual.Any(
                        coh =>
                            coh.ProductName == "Aniseed Syrup"
                            && coh.Total == 6));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_composed()
        {
            using (var context = CreateContext())
            {
                var actual = await context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc)
                    .Where(mep => mep.TenMostExpensiveProducts.Contains("C"))
                    .OrderBy(mep => mep.UnitPrice)
                    .ToArrayAsync();

                Assert.Equal(4, actual.Length);
                Assert.Equal(46.00m, actual.First().UnitPrice);
                Assert.Equal(263.50m, actual.Last().UnitPrice);
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_with_parameter_composed()
        {
            using (var context = CreateContext())
            {
                var actual = await context
                    .Set<CustomerOrderHistory>()
                    .FromSql(CustomerOrderHistorySproc, "ALFKI")
                    .Where(coh => coh.ProductName.Contains("C"))
                    .OrderBy(coh => coh.Total)
                    .ToArrayAsync();

                Assert.Equal(2, actual.Length);
                Assert.Equal(15, actual.First().Total);
                Assert.Equal(21, actual.Last().Total);
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_take()
        {
            using (var context = CreateContext())
            {
                var actual = await context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc)
                    .OrderByDescending(mep => mep.UnitPrice)
                    .Take(2)
                    .ToArrayAsync();

                Assert.Equal(2, actual.Length);
                Assert.Equal(263.50m, actual.First().UnitPrice);
                Assert.Equal(123.79m, actual.Last().UnitPrice);
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_min()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    45.60m,
                    await context.Set<MostExpensiveProduct>()
                        .FromSql(TenMostExpensiveProductsSproc)
                        .MinAsync(mep => mep.UnitPrice));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_with_include_throws()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    RelationalStrings.StoredProcedureIncludeNotSupported,
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        async () =>
                            await context.Set<Product>()
                                .FromSql("SelectStoredProcedure")
                                .Include(p => p.OrderDetails)
                                .ToArrayAsync()
                    )).Message);
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected AsyncFromSqlSprocQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected abstract string TenMostExpensiveProductsSproc { get; }

        protected abstract string CustomerOrderHistorySproc { get; }
    }
}
