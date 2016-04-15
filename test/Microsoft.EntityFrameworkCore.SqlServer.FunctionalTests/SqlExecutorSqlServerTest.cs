// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class SqlExecutorSqlServerTest : SqlExecutorTestBase<NorthwindQuerySqlServerFixture>
    {
        public override void Executes_stored_procedure()
        {
            base.Executes_stored_procedure();

            Assert.Equal(
                "[dbo].[Ten Most Expensive Products]",
                Sql);
        }

        public override void Executes_stored_procedure_with_parameter()
        {
            base.Executes_stored_procedure_with_parameter();

            Assert.Equal(
                @"@CustomerID: ALFKI

[dbo].[CustOrderHist] @CustomerID",
                Sql);
        }

        public override void Executes_stored_procedure_with_generated_parameter()
        {
            base.Executes_stored_procedure_with_generated_parameter();

            Assert.Equal(
                @"@p0: ALFKI

[dbo].[CustOrderHist] @CustomerID = @p0",
                Sql);
        }

        public SqlExecutorSqlServerTest(NorthwindQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new SqlParameter
            {
                ParameterName = name,
                Value = value
            };

        protected override string TenMostExpensiveProductsSproc => "[dbo].[Ten Most Expensive Products]";

        protected override string CustomerOrderHistorySproc => "[dbo].[CustOrderHist] @CustomerID";

        protected override string CustomerOrderHistoryWithGeneratedParameterSproc => "[dbo].[CustOrderHist] @CustomerID = {0}";

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
