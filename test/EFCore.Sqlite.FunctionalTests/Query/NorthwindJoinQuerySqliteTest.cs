// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

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
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_client_eval(async))).Message);

    public override async Task SelectMany_with_client_eval_with_collection_shaper(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_client_eval_with_collection_shaper(async))).Message);

    public override async Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_client_eval_with_collection_shaper_ignored(async))).Message);

    public override async Task SelectMany_with_selecting_outer_entity(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_selecting_outer_entity(async))).Message);

    public override async Task SelectMany_with_selecting_outer_element(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_selecting_outer_element(async))).Message);

    public override async Task SelectMany_with_selecting_outer_entity_column_and_inner_column(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_selecting_outer_entity_column_and_inner_column(async))).Message);

    public override async Task Take_in_collection_projection_with_FirstOrDefault_on_top_level(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Take_in_collection_projection_with_FirstOrDefault_on_top_level(async))).Message);

    public override async Task GroupJoin_as_final_operator(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.GroupJoin_as_final_operator(async))).Message);

    public override async Task Unflattened_GroupJoin_composed(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Unflattened_GroupJoin_composed(async))).Message);

    public override async Task Unflattened_GroupJoin_composed_2(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Unflattened_GroupJoin_composed_2(async))).Message);

    public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.GroupJoin_SelectMany_subquery_with_filter_orderby(async))).Message);

    public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(async))).Message);
}
