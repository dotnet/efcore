// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindJoinQuerySqliteTest : NorthwindJoinQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindJoinQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task SelectMany_with_client_eval(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_with_client_eval(async))).Message);

    public override async Task SelectMany_with_client_eval_with_collection_shaper(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_with_client_eval_with_collection_shaper(async)))
            .Message);

    public override async Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.SelectMany_with_client_eval_with_collection_shaper_ignored(async))).Message);

    public override async Task SelectMany_with_selecting_outer_entity(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_with_selecting_outer_entity(async))).Message);

    public override async Task SelectMany_with_selecting_outer_element(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_with_selecting_outer_element(async))).Message);

    public override async Task SelectMany_with_selecting_outer_entity_column_and_inner_column(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.SelectMany_with_selecting_outer_entity_column_and_inner_column(async))).Message);

    public override async Task Take_in_collection_projection_with_FirstOrDefault_on_top_level(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Take_in_collection_projection_with_FirstOrDefault_on_top_level(async))).Message);

    public override async Task Join_local_string_closure_is_cached_correctly(bool async)
    {
        await base.Join_local_string_closure_is_cached_correctly(async);

        AssertSql(
            """
@p1='1' (DbType = String)
@p2='2' (DbType = String)

SELECT "e"."EmployeeID"
FROM "Employees" AS "e"
INNER JOIN (SELECT @p1 AS "Value" UNION ALL VALUES (@p2)) AS "p" ON "e"."EmployeeID" = unicode("p"."Value")
""",
            //
            """
@p1='3' (DbType = String)

SELECT "e"."EmployeeID"
FROM "Employees" AS "e"
INNER JOIN (SELECT @p1 AS "Value") AS "p" ON "e"."EmployeeID" = unicode("p"."Value")
""");
    }

    public override async Task Join_local_bytes_closure_is_cached_correctly(bool async)
    {
        var ids = new byte[] { 1, 2 };
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID);

        ids = new byte[] { 3 };
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID);

        AssertSql(
            """
@p1='1'
@p2='2'

SELECT "e"."EmployeeID"
FROM "Employees" AS "e"
INNER JOIN (SELECT @p1 AS "Value" UNION ALL VALUES (@p2)) AS "p" ON "e"."EmployeeID" = "p"."Value"
""",
            //
            """
@p1='3'

SELECT "e"."EmployeeID"
FROM "Employees" AS "e"
INNER JOIN (SELECT @p1 AS "Value") AS "p" ON "e"."EmployeeID" = "p"."Value"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
