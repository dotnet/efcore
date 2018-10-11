// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlSprocQuerySqlServerTest : FromSqlSprocQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public FromSqlSprocQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void From_sql_queryable_stored_procedure()
        {
            base.From_sql_queryable_stored_procedure();

            Assert.Equal(
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_projection()
        {
            base.From_sql_queryable_stored_procedure_projection();

            Assert.Equal(
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_with_parameter()
        {
            base.From_sql_queryable_stored_procedure_with_parameter();

            Assert.Equal(
                @"@p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void From_sql_queryable_stored_procedure_reprojection()
        {
            base.From_sql_queryable_stored_procedure_reprojection();

            Assert.Equal(
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_composed()
        {
            base.From_sql_queryable_stored_procedure_composed();

            Assert.Equal(
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_with_parameter_composed()
        {
            base.From_sql_queryable_stored_procedure_with_parameter_composed();

            Assert.Equal(
                @"@p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void From_sql_queryable_stored_procedure_take()
        {
            base.From_sql_queryable_stored_procedure_take();

            Assert.Equal(
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_min()
        {
            base.From_sql_queryable_stored_procedure_min();

            Assert.Equal(
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_with_multiple_stored_procedures()
        {
            base.From_sql_queryable_with_multiple_stored_procedures();

            Assert.StartsWith(
                "[dbo].[Ten Most Expensive Products]" + _eol +
                _eol +
                "[dbo].[Ten Most Expensive Products]" + _eol +
                _eol +
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_and_select()
        {
            base.From_sql_queryable_stored_procedure_and_select();

            Assert.StartsWith(
                "[dbo].[Ten Most Expensive Products]" + _eol +
                _eol +
                "SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]" + _eol +
                "FROM (" + _eol +
                @"    SELECT * FROM ""Products""" + _eol +
                ") AS [p]" + _eol +
                _eol +
                "SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]" + _eol +
                "FROM (" + _eol +
                @"    SELECT * FROM ""Products""" + _eol +
                ") AS [p]",
                Sql);
        }

        public override void From_sql_queryable_select_and_stored_procedure()
        {
            base.From_sql_queryable_select_and_stored_procedure();

            Assert.StartsWith(
                "SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]" + _eol +
                "FROM (" + _eol +
                @"    SELECT * FROM ""Products""" + _eol +
                ") AS [p]" + _eol +
                _eol +
                "[dbo].[Ten Most Expensive Products]" + _eol +
                _eol +
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        protected override string TenMostExpensiveProductsSproc => "[dbo].[Ten Most Expensive Products]";
        protected override string CustomerOrderHistorySproc => "[dbo].[CustOrderHist] @CustomerID = {0}";

        private static readonly string _eol = Environment.NewLine;

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;
    }
}
