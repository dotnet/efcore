// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class ComplexNavigationsSharedTypeQuerySqliteTest(ComplexNavigationsSharedTypeQuerySqliteFixture fixture)
    : ComplexNavigationsSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQuerySqliteFixture>(fixture)
{
    public override Task GroupJoin_client_method_in_OrderBy(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.GroupJoin_client_method_in_OrderBy(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.ComplexNavigationsQueryTestBase<Microsoft.EntityFrameworkCore.Query.ComplexNavigationsSharedTypeQuerySqliteFixture>",
                "ClientMethodNullableInt"));

    public override async Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async))).Message);

    public override async Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Join_with_result_selector_returning_queryable_throws_validation_error(async))).Message);

    public override async Task Let_let_contains_from_outer_let(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Let_let_contains_from_outer_let(async))).Message);

    public override async Task Prune_does_not_throw_null_ref(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Prune_does_not_throw_null_ref(async))).Message);

    [ConditionalTheory(Skip = "Issue#26104")]
    public override Task GroupBy_aggregate_where_required_relationship(bool async)
        => base.GroupBy_aggregate_where_required_relationship(async);

    [ConditionalTheory(Skip = "Issue#26104")]
    public override Task GroupBy_aggregate_where_required_relationship_2(bool async)
        => base.GroupBy_aggregate_where_required_relationship_2(async);

    public override async Task GroupJoin_with_subquery_on_inner(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.GroupJoin_with_subquery_on_inner(async))).Message);

    public override async Task GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(async))).Message);

    public override async Task Correlated_projection_with_first(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_projection_with_first(async))).Message);

    public override async Task Multiple_select_many_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Multiple_select_many_in_projection(async))).Message);

    public override async Task Single_select_many_in_projection_with_take(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Single_select_many_in_projection_with_take(async))).Message);

    [ConditionalTheory(Skip = "issue #32559")]
    public override Task Max_in_multi_level_nested_subquery(bool async)
        => base.Max_in_multi_level_nested_subquery(async);
}
