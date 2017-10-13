// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncFromSqlSprocQueryOracleTest : NorthwindQueryOracleFixture<NoopModelCustomizer>
    {
        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_with_parameter()
        {
            using (var context = CreateContext())
            {
                var parameters = new OracleParameter[]
                {
                    new OracleParameter
                    {
                        ParameterName = ":p0",
                        Value = "ALFKI"
                    },
                    new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output)
                };

                var actual = await context
                    .Set<CustomerOrderHistory>()
                    .FromSql(CustomerOrderHistorySproc, parameters)
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
        public virtual async Task From_sql_queryable_stored_procedure()
        {
            using (var context = CreateContext())
            {
                var parameter =
                    new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output);

                var actual = await context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, parameter)
                    .ToArrayAsync();

                Assert.Equal(10, actual.Length);

                Assert.True(
                    actual.Any(
                        mep =>
                            mep.TenMostExpensiveProducts == "Côte de Blaye"
                            && mep.UnitPrice == 263.50m));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_composed()
        {
            using (var context = CreateContext())
            {
                var parameter =
                    new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output);

                var actual = await context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, parameter)
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
                var parameters = new OracleParameter[]
                {
                    new OracleParameter
                    {
                        ParameterName = ":p0",
                        Value = "ALFKI"
                    },
                    new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output)
                };

                var actual = await context
                    .Set<CustomerOrderHistory>()
                    .FromSql(CustomerOrderHistorySproc, parameters)
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
                var parameter =
                    new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output);

                var actual = await context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, parameter)
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
                var parameter =
                    new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output);

                Assert.Equal(
                    45.60m,
                    await context.Set<MostExpensiveProduct>()
                        .FromSql(TenMostExpensiveProductsSproc, parameter)
                        .MinAsync(mep => mep.UnitPrice));
            }
        }

        [Fact]
        public virtual async Task From_sql_queryable_stored_procedure_with_include_throws()
        {
            using (var context = CreateContext())
            {
                var parameter =
                    new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output);

                Assert.Equal(
                    RelationalStrings.StoredProcedureIncludeNotSupported,
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        async () =>
                            await context.Set<Product>()
                                .FromSql("SelectStoredProcedure", parameter)
                                .Include(p => p.OrderDetails)
                                .ToArrayAsync()
                    )).Message);
            }
        }

        private string TenMostExpensiveProductsSproc
            => "BEGIN \"Ten Most Expensive Products\"(:cur); END;";

        private string CustomerOrderHistorySproc => "BEGIN \"CustOrderHist\"(:p0, :cur); END;";
    }
}
