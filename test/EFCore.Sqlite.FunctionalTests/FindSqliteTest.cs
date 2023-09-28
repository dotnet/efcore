// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class FindSqliteTest : FindTestBase<FindSqliteTest.FindSqliteFixture>
{
    protected FindSqliteTest(FindSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class FindSqliteTestSet : FindSqliteTest
    {
        public FindSqliteTestSet(FindSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindSqliteTestContext : FindSqliteTest
    {
        public FindSqliteTestContext(FindSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindSqliteTestNonGeneric : FindSqliteTest
    {
        public FindSqliteTestNonGeneric(FindSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindSqliteFixture : FindFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
