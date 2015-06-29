// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class FromSqlSprocQuerySqlServerTest : FromSqlSprocQueryTestBase<NorthwindSprocQuerySqlServerFixture>
    {
        public override void From_sql_queryable_stored_procedure()
        {
            base.From_sql_queryable_stored_procedure();

            Assert.Equal(
                @"[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_projection()
        {
            base.From_sql_queryable_stored_procedure_projection();

            Assert.Equal(
                @"[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_with_parameter()
        {
            base.From_sql_queryable_stored_procedure_with_parameter();

            Assert.Equal(
                @"@p0: ALFKI

[dbo].[CustOrderHist] @CustomerID = @p0",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_composed()
        {
            base.From_sql_queryable_stored_procedure_composed();

            Assert.Equal(
                @"[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_with_parameter_composed()
        {
            base.From_sql_queryable_stored_procedure_with_parameter_composed();

            Assert.Equal(
                @"@p0: ALFKI

[dbo].[CustOrderHist] @CustomerID = @p0",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_take()
        {
            base.From_sql_queryable_stored_procedure_take();

            Assert.Equal(
                @"[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_min()
        {
            base.From_sql_queryable_stored_procedure_min();

            Assert.Equal(
                @"[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public FromSqlSprocQuerySqlServerTest(NorthwindSprocQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override string TenMostExpensiveProductsSproc => "[dbo].[Ten Most Expensive Products]";

        protected override string CustomerOrderHistorySproc => "[dbo].[CustOrderHist] @CustomerID = {0}";

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
