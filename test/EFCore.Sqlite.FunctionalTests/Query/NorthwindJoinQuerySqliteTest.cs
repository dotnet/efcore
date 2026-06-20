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
        // SQLite parameterizes the string closure as a single value ("12"/"3"), so no EmployeeID matches.
        var ids = "12";
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID,
            assertEmpty: true);

        ids = "3";
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID,
            assertEmpty: true);
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
    }
}
