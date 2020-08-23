// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SqlExecutorSqlServerTest : SqlExecutorTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public SqlExecutorSqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Executes_stored_procedure()
        {
            base.Executes_stored_procedure();

            AssertSql("[dbo].[Ten Most Expensive Products]");
        }

        public override void Executes_stored_procedure_with_parameter()
        {
            base.Executes_stored_procedure_with_parameter();

            AssertSql(
                @"@CustomerID='ALFKI' (Nullable = false) (Size = 5)

[dbo].[CustOrderHist] @CustomerID");
        }

        public override void Executes_stored_procedure_with_generated_parameter()
        {
            base.Executes_stored_procedure_with_generated_parameter();

            AssertSql(
                @"@p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0");
        }

        public override void Query_with_parameters()
        {
            base.Query_with_parameters();

            AssertSql(
                @"@p0='London' (Size = 4000)
@p1='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1");
        }

        public override void Query_with_dbParameter_with_name()
        {
            base.Query_with_dbParameter_with_name();

            AssertSql(
                @"@city='London' (Nullable = false) (Size = 6)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city");
        }

        public override void Query_with_positional_dbParameter_with_name()
        {
            base.Query_with_positional_dbParameter_with_name();

            AssertSql(
                @"@city='London' (Nullable = false) (Size = 6)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city");
        }

        public override void Query_with_positional_dbParameter_without_name()
        {
            base.Query_with_positional_dbParameter_without_name();

            AssertSql(
                @"@p0='London' (Nullable = false) (Size = 6)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @p0");
        }

        public override void Query_with_dbParameters_mixed()
        {
            base.Query_with_dbParameters_mixed();

            AssertSql(
                @"@p0='London' (Size = 4000)
@contactTitle='Sales Representative' (Nullable = false) (Size = 20)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @contactTitle",
                //
                @"@city='London' (Nullable = false) (Size = 6)
@p0='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city AND ""ContactTitle"" = @p0");
        }

        public override void Query_with_parameters_interpolated()
        {
            base.Query_with_parameters_interpolated();

            AssertSql(
                @"@p0='London' (Size = 4000)
@p1='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1");
        }

        public override void Query_with_DbParameters_interpolated()
        {
            base.Query_with_DbParameters_interpolated();

            AssertSql(
                @"city='London' (Nullable = false) (Size = 6)
contactTitle='Sales Representative' (Nullable = false) (Size = 20)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city AND ""ContactTitle"" = @contactTitle");
        }

        public override async Task Query_with_parameters_async()
        {
            await base.Query_with_parameters_async();

            AssertSql(
                @"@p0='London' (Size = 4000)
@p1='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1");
        }

        public override async Task Query_with_parameters_interpolated_async()
        {
            await base.Query_with_parameters_interpolated_async();

            AssertSql(
                @"@p0='London' (Size = 4000)
@p1='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1");
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new SqlParameter { ParameterName = name, Value = value };

        protected override string TenMostExpensiveProductsSproc
            => "[dbo].[Ten Most Expensive Products]";

        protected override string CustomerOrderHistorySproc
            => "[dbo].[CustOrderHist] @CustomerID";

        protected override string CustomerOrderHistoryWithGeneratedParameterSproc
            => "[dbo].[CustOrderHist] @CustomerID = {0}";

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
