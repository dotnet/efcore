// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class Ef6GroupByInMemoryTest : Ef6GroupByTestBase<Ef6GroupByInMemoryTest.Ef6GroupByInMemoryFixture>
{
    public Ef6GroupByInMemoryTest(Ef6GroupByInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }

    public class Ef6GroupByInMemoryFixture : Ef6GroupByFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
