// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NorthwindBulkUpdatesSqliteTest : NorthwindBulkUpdatesTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindBulkUpdatesSqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Where_delete(bool async)
    {
        await base.Where_delete(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE ""o"".""OrderID"" < 10300");
    }

    public override async Task Where_delete_parameter(bool async)
    {
        await base.Where_delete_parameter(async);

        AssertSql(
            @"@__quantity_0='1' (Nullable = true) (DbType = Int16)

DELETE FROM ""Order Details"" AS ""o""
WHERE ""o"".""Quantity"" = @__quantity_0",
                //
                @"DELETE FROM ""Order Details"" AS ""o""
WHERE 0");
    }

    public override async Task Where_delete_OrderBy(bool async)
    {
        await base.Where_delete_OrderBy(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Order Details"" AS ""o0""
    WHERE ""o0"".""OrderID"" < 10300 AND ""o0"".""OrderID"" = ""o"".""OrderID"" AND ""o0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Where_delete_OrderBy_Skip(bool async)
    {
        await base.Where_delete_OrderBy_Skip(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        ORDER BY ""o0"".""OrderID""
        LIMIT -1 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Where_delete_OrderBy_Take(bool async)
    {
        await base.Where_delete_OrderBy_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        ORDER BY ""o0"".""OrderID""
        LIMIT @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Where_delete_OrderBy_Skip_Take(bool async)
    {
        await base.Where_delete_OrderBy_Skip_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        ORDER BY ""o0"".""OrderID""
        LIMIT @__p_0 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Where_delete_Skip(bool async)
    {
        await base.Where_delete_Skip(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        LIMIT -1 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Where_delete_Take(bool async)
    {
        await base.Where_delete_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        LIMIT @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Where_delete_Skip_Take(bool async)
    {
        await base.Where_delete_Skip_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        LIMIT @__p_0 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
