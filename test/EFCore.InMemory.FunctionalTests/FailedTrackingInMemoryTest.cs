// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class FailedTrackingInMemoryTest(FailedTrackingInMemoryTest.FailedTrackingInMemoryFixture fixture)
    : FailedTrackingTestBase<FailedTrackingInMemoryTest.FailedTrackingInMemoryFixture>(fixture)
{
    public class FailedTrackingInMemoryFixture : FailedTrackingFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
