// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class NotificationEntitiesSqlServerTest
        : NotificationEntitiesTestBase<NotificationEntitiesSqlServerTest.NotificationEntitiesSqlServerFixture>
    {
        public NotificationEntitiesSqlServerTest(NotificationEntitiesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesSqlServerFixture : NotificationEntitiesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
