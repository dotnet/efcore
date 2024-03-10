// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class FromSqlSprocQuerySqlServerTest : FromSqlSprocQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public FromSqlSprocQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public override async Task From_sql_queryable_stored_procedure(bool async)
    {
        await base.From_sql_queryable_stored_procedure(async);

        AssertSql(
            """
[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_with_tag(bool async)
    {
        await base.From_sql_queryable_stored_procedure_with_tag(async);

        AssertSql(
            """
-- Stored Procedure

[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_with_tags(bool async)
    {
        await base.From_sql_queryable_stored_procedure_with_tags(async);

        AssertSql(
            """
-- One
-- Two
-- Three

[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_with_caller_info_tag(bool async)
    {
        await base.From_sql_queryable_stored_procedure_with_caller_info_tag(async);

        AssertSql(
            """
-- File: SampleFileName:13

[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_with_caller_info_tag_and_other_tags(bool async)
    {
        await base.From_sql_queryable_stored_procedure_with_caller_info_tag_and_other_tags(async);

        AssertSql(
            """
-- Before
-- File: SampleFileName:13
-- After

[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_with_parameter(bool async)
    {
        await base.From_sql_queryable_stored_procedure_with_parameter(async);

        AssertSql(
            """
p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0
""");
    }

    public override async Task From_sql_queryable_stored_procedure_re_projection_on_client(bool async)
    {
        await base.From_sql_queryable_stored_procedure_re_projection_on_client(async);

        AssertSql(
            """
[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_composed_on_client(bool async)
    {
        await base.From_sql_queryable_stored_procedure_composed_on_client(async);

        AssertSql(
            """
[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_with_parameter_composed_on_client(bool async)
    {
        await base.From_sql_queryable_stored_procedure_with_parameter_composed_on_client(async);

        AssertSql(
            """
p0='ALFKI' (Size = 4000)

[dbo].[CustOrderHist] @CustomerID = @p0
""");
    }

    public override async Task From_sql_queryable_stored_procedure_take_on_client(bool async)
    {
        await base.From_sql_queryable_stored_procedure_take_on_client(async);

        AssertSql(
            """
[dbo].[Ten Most Expensive Products]
""");
    }

    public override async Task From_sql_queryable_stored_procedure_min_on_client(bool async)
    {
        await base.From_sql_queryable_stored_procedure_min_on_client(async);

        AssertSql(
            """
[dbo].[Ten Most Expensive Products]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override string TenMostExpensiveProductsSproc
        => "[dbo].[Ten Most Expensive Products]";

    protected override string CustomerOrderHistorySproc
        => "[dbo].[CustOrderHist] @CustomerID = {0}";
}
