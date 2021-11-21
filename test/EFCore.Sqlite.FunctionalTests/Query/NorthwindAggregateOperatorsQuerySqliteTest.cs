// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindAggregateOperatorsQuerySqliteTest : NorthwindAggregateOperatorsQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindAggregateOperatorsQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Sum_with_division_on_decimal(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Sum", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Sum_with_division_on_decimal(async)))
            .Message);

    public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Sum", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Sum_with_division_on_decimal_no_significant_digits(async)))
            .Message);

    public override async Task Average_with_division_on_decimal(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_with_division_on_decimal(async)))
            .Message);

    public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_with_division_on_decimal_no_significant_digits(async)))
            .Message);

    public override async Task Average_over_max_subquery_is_client_eval(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_over_max_subquery_is_client_eval(async)))
            .Message);

    public override async Task Average_over_nested_subquery_is_client_eval(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_over_nested_subquery_is_client_eval(async)))
            .Message);

    public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Multiple_collection_navigation_with_FirstOrDefault_chained(async))).Message);

    public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
        // Aggregates. Issue #15937.
        => await AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));

    public override async Task Contains_with_local_tuple_array_closure(bool async)
        => await AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));
}
