// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class FindInMemoryTest : FindTestBase<FindInMemoryTest.FindInMemoryFixture>
{
    protected FindInMemoryTest(FindInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class FindInMemoryTestSet : FindInMemoryTest
    {
        public FindInMemoryTestSet(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindInMemoryTestContext : FindInMemoryTest
    {
        public FindInMemoryTestContext(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindInMemoryTestNonGeneric : FindInMemoryTest
    {
        public FindInMemoryTestNonGeneric(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindInMemoryFixture : FindFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
