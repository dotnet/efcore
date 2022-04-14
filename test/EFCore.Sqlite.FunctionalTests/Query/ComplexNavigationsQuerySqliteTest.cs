// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsQuerySqliteTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQuerySqliteFixture>
{
    public ComplexNavigationsQuerySqliteTest(ComplexNavigationsQuerySqliteFixture fixture)
        : base(fixture)
    {
    }

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

    public override async Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Join_with_result_selector_returning_queryable_throws_validation_error(async))).Message);

    public override async Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async))).Message);

    public override Task GroupJoin_client_method_in_OrderBy(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.GroupJoin_client_method_in_OrderBy(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.ComplexNavigationsQueryTestBase<Microsoft.EntityFrameworkCore.Query.ComplexNavigationsQuerySqliteFixture>",
                "ClientMethodNullableInt"));
}
