// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class NotificationEntitiesSqliteTest(NotificationEntitiesSqliteTest.NotificationEntitiesSqliteFixture fixture) : NotificationEntitiesTestBase<
    NotificationEntitiesSqliteTest.NotificationEntitiesSqliteFixture>(fixture)
{
    public class NotificationEntitiesSqliteFixture : NotificationEntitiesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
