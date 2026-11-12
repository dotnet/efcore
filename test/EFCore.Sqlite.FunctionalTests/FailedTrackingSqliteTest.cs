// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class FailedTrackingSqliteTest(FailedTrackingSqliteTest.FailedTrackingSqliteFixture fixture)
    : FailedTrackingTestBase<FailedTrackingSqliteTest.FailedTrackingSqliteFixture>(fixture)
{
    public class FailedTrackingSqliteFixture : FailedTrackingFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
