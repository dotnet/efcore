// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlSprocQuerySqlServerTest : FromSqlSprocQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public FromSqlSprocQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override async Task<Exception> From_sql_queryable_stored_procedure_with_include_throws(bool async)
            => AssertSqlException(await base.From_sql_queryable_stored_procedure_with_include_throws(async));

        private static Exception AssertSqlException(Exception exception)
        {
            Assert.IsType<SqlException>(exception);
            Assert.Equal(102, ((SqlException)exception).Number);

            return exception;
        }

        public override async Task From_sql_queryable_stored_procedure(bool async)
        {
            await base.From_sql_queryable_stored_procedure(async);

            AssertSql("[dbo].[Ten Most Expensive Products]");
        }

        public override async Task From_sql_queryable_stored_procedure_projection(bool async)
        {
            await base.From_sql_queryable_stored_procedure_projection(async);

            AssertSql("[dbo].[Ten Most Expensive Products]");
        }

        public override async Task From_sql_queryable_stored_procedure_with_parameter(bool async)
        {
            await base.From_sql_queryable_stored_procedure_with_parameter(async);

            AssertSql(
                @"p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0");
        }

        public override async Task From_sql_queryable_stored_procedure_re_projection_on_client(bool async)
        {
            await base.From_sql_queryable_stored_procedure_re_projection_on_client(async);

            AssertSql("[dbo].[Ten Most Expensive Products]");
        }

        public override async Task<Exception> From_sql_queryable_stored_procedure_re_projection(bool async)
            => AssertSqlException(await base.From_sql_queryable_stored_procedure_re_projection(async));

        public override async Task From_sql_queryable_stored_procedure_composed_on_client(bool async)
        {
            await base.From_sql_queryable_stored_procedure_composed_on_client(async);

            AssertSql("[dbo].[Ten Most Expensive Products]");
        }

        public override async Task<Exception> From_sql_queryable_stored_procedure_composed(bool async)
            => AssertSqlException(await base.From_sql_queryable_stored_procedure_composed(async));

        public override async Task From_sql_queryable_stored_procedure_with_parameter_composed_on_client(bool async)
        {
            await base.From_sql_queryable_stored_procedure_with_parameter_composed_on_client(async);

            AssertSql(
                @"p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0");
        }

        public override async Task<Exception> From_sql_queryable_stored_procedure_with_parameter_composed(bool async)
            => AssertSqlException(await base.From_sql_queryable_stored_procedure_with_parameter_composed(async));

        public override async Task From_sql_queryable_stored_procedure_take_on_client(bool async)
        {
            await base.From_sql_queryable_stored_procedure_take_on_client(async);

            AssertSql("[dbo].[Ten Most Expensive Products]");
        }

        public override async Task<Exception> From_sql_queryable_stored_procedure_take(bool async)
            => AssertSqlException(await base.From_sql_queryable_stored_procedure_take(async));

        public override async Task From_sql_queryable_stored_procedure_min_on_client(bool async)
        {
            await base.From_sql_queryable_stored_procedure_min_on_client(async);

            AssertSql("[dbo].[Ten Most Expensive Products]");
        }

        public override async Task<Exception> From_sql_queryable_stored_procedure_min(bool async)
            => AssertSqlException(await base.From_sql_queryable_stored_procedure_min(async));

        public override async Task<Exception> From_sql_queryable_with_multiple_stored_procedures(bool async)
            => AssertSqlException(await base.From_sql_queryable_with_multiple_stored_procedures(async));

        public override async Task<Exception> From_sql_queryable_stored_procedure_and_select(bool async)
            => AssertSqlException(await base.From_sql_queryable_stored_procedure_and_select(async));

        public override async Task<Exception> From_sql_queryable_select_and_stored_procedure(bool async)
            => AssertSqlException(await base.From_sql_queryable_select_and_stored_procedure(async));

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override string TenMostExpensiveProductsSproc => "[dbo].[Ten Most Expensive Products]";

        protected override string CustomerOrderHistorySproc => "[dbo].[CustOrderHist] @CustomerID = {0}";
    }
}
