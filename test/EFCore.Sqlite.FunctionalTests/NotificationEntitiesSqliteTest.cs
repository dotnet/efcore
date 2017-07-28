// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public class NotificationEntitiesSqliteTest: NotificationEntitiesTestBase<NotificationEntitiesSqliteTest.NotificationEntitiesSqliteFixture>
    {
        public NotificationEntitiesSqliteTest(NotificationEntitiesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesSqliteFixture : NotificationEntitiesFixtureBase
        {
            protected override ITestStoreFactory<TestStore> TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
