// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlSprocQueryOracleTest : NorthwindQueryOracleFixture<NoopModelCustomizer>
    {
        [Fact]
        public virtual void From_sql_queryable_stored_procedure()
        {
            using (var context = CreateContext())
            {
                var actual = context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                    .ToArray();

                Assert.Equal(10, actual.Length);

                Assert.True(
                    actual.Any(
                        mep =>
                            mep.TenMostExpensiveProducts == "Côte de Blaye"
                            && mep.UnitPrice == 263.50m));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_projection()
        {
            using (var context = CreateContext())
            {
                var actual = context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                    .Select(mep => mep.TenMostExpensiveProducts)
                    .ToArray();

                Assert.Equal(10, actual.Length);
                Assert.True(actual.Any(r => r == "Côte de Blaye"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_reprojection()
        {
            using (var context = CreateContext())
            {
                var actual = context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                    .Select(
                        mep =>
                            new MostExpensiveProduct
                            {
                                TenMostExpensiveProducts = "Foo",
                                UnitPrice = mep.UnitPrice
                            })
                    .ToArray();

                Assert.Equal(10, actual.Length);
                Assert.True(actual.All(mep => mep.TenMostExpensiveProducts == "Foo"));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_with_parameter()
        {
            using (var context = CreateContext())
            {
                var actual = context
                    .Set<CustomerOrderHistory>()
                    .FromSql(CustomerOrderHistorySproc, ParametersQuery)
                    .ToArray();

                Assert.Equal(11, actual.Length);

                Assert.True(
                    actual.Any(
                        coh =>
                            coh.ProductName == "Aniseed Syrup"
                            && coh.Total == 6));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_composed()
        {
            using (var context = CreateContext())
            {
                var actual = context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                    .Where(mep => mep.TenMostExpensiveProducts.Contains("C"))
                    .OrderBy(mep => mep.UnitPrice)
                    .ToArray();

                Assert.Equal(4, actual.Length);
                Assert.Equal(46.00m, actual.First().UnitPrice);
                Assert.Equal(263.50m, actual.Last().UnitPrice);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_with_parameter_composed()
        {
            using (var context = CreateContext())
            {
                var actual = context
                    .Set<CustomerOrderHistory>()
                    .FromSql(CustomerOrderHistorySproc, ParametersQuery)
                    .Where(coh => coh.ProductName.Contains("C"))
                    .OrderBy(coh => coh.Total)
                    .ToArray();

                Assert.Equal(2, actual.Length);
                Assert.Equal(15, actual.First().Total);
                Assert.Equal(21, actual.Last().Total);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_take()
        {
            using (var context = CreateContext())
            {
                var actual = context
                    .Set<MostExpensiveProduct>()
                    .FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                    .OrderByDescending(mep => mep.UnitPrice)
                    .Take(2)
                    .ToArray();

                Assert.Equal(2, actual.Length);
                Assert.Equal(263.50m, actual.First().UnitPrice);
                Assert.Equal(123.79m, actual.Last().UnitPrice);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_min()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    45.60m,
                    context.Set<MostExpensiveProduct>()
                        .FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                        .Min(mep => mep.UnitPrice));
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_with_include_throws()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    RelationalStrings.StoredProcedureIncludeNotSupported,
                    Assert.Throws<InvalidOperationException>(
                        () => context.Set<Product>()
                            .FromSql("SelectStoredProcedure", ParametersQuery[1])
                            .Include(p => p.OrderDetails)
                            .ToArray()
                    ).Message);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_with_multiple_stored_procedures()
        {
            using (var context = CreateContext())
            {
                var actual
                    = (from a in context.Set<MostExpensiveProduct>().FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                       from b in context.Set<MostExpensiveProduct>().FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                       where a.TenMostExpensiveProducts == b.TenMostExpensiveProducts
                       select new { a, b })
                    .ToArray();

                Assert.Equal(10, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_stored_procedure_and_select()
        {
            using (var context = CreateContext())
            {
                var actual
                    = (from mep in context.Set<MostExpensiveProduct>().FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                       from p in context.Set<Product>().FromSql("SELECT * FROM \"Products\"")
                       where mep.TenMostExpensiveProducts == p.ProductName
                       select new { mep, p })
                    .ToArray();

                Assert.Equal(10, actual.Length);
            }
        }

        [Fact]
        public virtual void From_sql_queryable_select_and_stored_procedure()
        {
            using (var context = CreateContext())
            {
                var actual
                    = (from p in context.Set<Product>().FromSql("SELECT * FROM \"Products\"")
                       from mep in context.Set<MostExpensiveProduct>().FromSql(TenMostExpensiveProductsSproc, ParametersQuery[1])
                       where mep.TenMostExpensiveProducts == p.ProductName
                       select new { mep, p })
                    .ToArray();

                Assert.Equal(10, actual.Length);
            }
        }

        protected OracleParameter[] ParametersQuery
            => new[]
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


        protected string TenMostExpensiveProductsSproc
            => "BEGIN \"Ten Most Expensive Products\"(:cur); END;";

        protected string CustomerOrderHistorySproc => "BEGIN \"CustOrderHist\"(:p0, :cur); END;";
    }
}
