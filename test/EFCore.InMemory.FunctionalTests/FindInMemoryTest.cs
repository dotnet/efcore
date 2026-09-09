// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class FindInMemoryTest(FindInMemoryTest.FindInMemoryFixture fixture)
    : FindTestBase<FindInMemoryTest.FindInMemoryFixture>(fixture)
{
    public class FindInMemoryTestSet(FindInMemoryFixture fixture) : FindInMemoryTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindInMemoryTestContext(FindInMemoryFixture fixture) : FindInMemoryTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindInMemoryTestNonGeneric(FindInMemoryFixture fixture) : FindInMemoryTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindInMemoryFixture : FindFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
