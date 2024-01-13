// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Query;

public class FromSqlQuerySqliteTest : FromSqlQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public FromSqlQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task FromSqlRaw_queryable_composed(bool async)
    {
        await base.FromSqlRaw_queryable_composed(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM (
    SELECT * FROM "Customers"
) AS "c"
WHERE "c"."ContactName" IS NOT NULL AND instr("c"."ContactName", 'z') > 0
""");
    }

    public override async Task<string> FromSqlRaw_queryable_with_parameters_and_closure(bool async)
    {
        var queryString = await base.FromSqlRaw_queryable_with_parameters_and_closure(async);

        Assert.Equal(
            """
            .param set p0 'London'
            .param set @__contactTitle_1 'Sales Representative'

            SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
            FROM (
                SELECT * FROM "Customers" WHERE "City" = @p0
            ) AS "c"
            WHERE "c"."ContactTitle" = @__contactTitle_1
            """,
            queryString, ignoreLineEndingDifferences: true);

        return queryString;
    }

    public override Task Bad_data_error_handling_invalid_cast_key(bool async)
        // Not supported on SQLite
        => Task.CompletedTask;

    public override Task Bad_data_error_handling_invalid_cast(bool async)
        // Not supported on SQLite
        => Task.CompletedTask;

    public override Task Bad_data_error_handling_invalid_cast_projection(bool async)
        // Not supported on SQLite
        => Task.CompletedTask;

    public override Task Bad_data_error_handling_invalid_cast_no_tracking(bool async)
        // Not supported on SQLite
        => Task.CompletedTask;

    public override async Task FromSqlRaw_composed_with_common_table_expression(bool async)
    {
        await base.FromSqlRaw_composed_with_common_table_expression(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM (
    WITH "Customers2" AS (
        SELECT * FROM "Customers"
    )
    SELECT * FROM "Customers2"
) AS "c"
WHERE "c"."ContactName" IS NOT NULL AND instr("c"."ContactName", 'z') > 0
""");
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new SqliteParameter { ParameterName = name, Value = value };

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
