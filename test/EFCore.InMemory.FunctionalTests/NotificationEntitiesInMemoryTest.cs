// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class NotificationEntitiesInMemoryTest : NotificationEntitiesTestBase<
    NotificationEntitiesInMemoryTest.NotificationEntitiesInMemoryFixture>
{
    public NotificationEntitiesInMemoryTest(NotificationEntitiesInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class NotificationEntitiesInMemoryFixture : NotificationEntitiesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
