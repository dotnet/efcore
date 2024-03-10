// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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
SELECT "m"."CustomerID", "m"."Address", "m"."City", "m"."CompanyName", "m"."ContactName", "m"."ContactTitle", "m"."Country", "m"."Fax", "m"."Phone", "m"."PostalCode", "m"."Region"
FROM (
    SELECT * FROM "Customers"
) AS "m"
WHERE "m"."ContactName" IS NOT NULL AND instr("m"."ContactName", 'z') > 0
""");
    }

    public override async Task<string> FromSqlRaw_queryable_with_parameters_and_closure(bool async)
    {
        var queryString = await base.FromSqlRaw_queryable_with_parameters_and_closure(async);

        Assert.Equal(
            @".param set p0 'London'
.param set @__contactTitle_1 'Sales Representative'

SELECT ""m"".""CustomerID"", ""m"".""Address"", ""m"".""City"", ""m"".""CompanyName"", ""m"".""ContactName"", ""m"".""ContactTitle"", ""m"".""Country"", ""m"".""Fax"", ""m"".""Phone"", ""m"".""PostalCode"", ""m"".""Region""
FROM (
    SELECT * FROM ""Customers"" WHERE ""City"" = @p0
) AS ""m""
WHERE ""m"".""ContactTitle"" = @__contactTitle_1", queryString, ignoreLineEndingDifferences: true);

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
SELECT "m"."CustomerID", "m"."Address", "m"."City", "m"."CompanyName", "m"."ContactName", "m"."ContactTitle", "m"."Country", "m"."Fax", "m"."Phone", "m"."PostalCode", "m"."Region"
FROM (
    WITH "Customers2" AS (
        SELECT * FROM "Customers"
    )
    SELECT * FROM "Customers2"
) AS "m"
WHERE "m"."ContactName" IS NOT NULL AND instr("m"."ContactName", 'z') > 0
""");
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new SqliteParameter { ParameterName = name, Value = value };

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
