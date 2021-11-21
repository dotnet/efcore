// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class Ef6GroupByInMemoryTest : Ef6GroupByTestBase<Ef6GroupByInMemoryTest.Ef6GroupByInMemoryFixture>
{
    public Ef6GroupByInMemoryTest(Ef6GroupByInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }

    public override Task Average_Grouped_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<ProductForLinq>()
                  group p by p.Category
                  into g
                  select new { Category = g.Key, AveragePrice = g.Average(p => p.UnitPrice) });

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_doesnt_produce_a_groupby_statement(bool async)
        => await base.Grouping_by_all_columns_doesnt_produce_a_groupby_statement(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_2(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_2(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_3(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_3(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_4(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_4(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_5(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_5(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_6(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_6(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_7(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_7(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_8(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_8(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_9(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_9(async);

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_10(bool async)
        => await base.Grouping_by_all_columns_with_aggregate_function_works_10(async);

    public class Ef6GroupByInMemoryFixture : Ef6GroupByFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
