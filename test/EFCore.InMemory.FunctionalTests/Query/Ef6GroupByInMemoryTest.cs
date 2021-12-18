// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    public class Ef6GroupByInMemoryFixture : Ef6GroupByFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
