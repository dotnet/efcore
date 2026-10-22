// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class SqlExecutorSqlServerTest : SqlExecutorTestBase<NorthwindQuerySqlServerFixture<SqlExecutorModelCustomizer>>
{
    public SqlExecutorSqlServerTest(NorthwindQuerySqlServerFixture<SqlExecutorModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Executes_stored_procedure(bool async)
    {
        await base.Executes_stored_procedure(async);

        AssertSql("[dbo].[Ten Most Expensive Products]");
    }

    public override async Task Executes_stored_procedure_with_parameter(bool async)
    {
        await base.Executes_stored_procedure_with_parameter(async);

        AssertSql(
            """
@CustomerID='ALFKI' (Nullable = false) (Size = 5)

[dbo].[CustOrderHist] @CustomerID
""");
    }

    public override async Task Executes_stored_procedure_with_generated_parameter(bool async)
    {
        await base.Executes_stored_procedure_with_generated_parameter(async);

        AssertSql(
            """
@p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0
""");
    }

    public override async Task Query_with_parameters(bool async)
    {
        await base.Query_with_parameters(async);

        AssertSql(
            """
@p0='London' (Size = 4000)
@p1='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task Query_with_dbParameter_with_name(bool async)
    {
        await base.Query_with_dbParameter_with_name(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @city
""");
    }

    public override async Task Query_with_positional_dbParameter_with_name(bool async)
    {
        await base.Query_with_positional_dbParameter_with_name(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @city
""");
    }

    public override async Task Query_with_positional_dbParameter_without_name(bool async)
    {
        await base.Query_with_positional_dbParameter_without_name(async);

        AssertSql(
            """
@p0='London' (Nullable = false) (Size = 6)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @p0
""");
    }

    public override async Task Query_with_dbParameters_mixed(bool async)
    {
        await base.Query_with_dbParameters_mixed(async);

        AssertSql(
            """
@p0='London' (Size = 4000)
@contactTitle='Sales Representative' (Nullable = false) (Size = 20)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @contactTitle
""",
            //
            """
@city='London' (Nullable = false) (Size = 6)
@p0='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @city AND "ContactTitle" = @p0
""");
    }

    public override async Task Query_with_parameters_interpolated(bool async)
    {
        await base.Query_with_parameters_interpolated(async);

        AssertSql(
            """
@p0='London' (Size = 4000)
@p1='Sales Representative' (Size = 4000)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task Query_with_DbParameters_interpolated(bool async)
    {
        await base.Query_with_DbParameters_interpolated(async);

        AssertSql(
            """
city='London' (Nullable = false) (Size = 6)
contactTitle='Sales Representative' (Nullable = false) (Size = 20)

SELECT COUNT(*) FROM "Customers" WHERE "City" = @city AND "ContactTitle" = @contactTitle
""");
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
