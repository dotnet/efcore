// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class LazyLoadProxySqliteTest : LazyLoadProxyTestBase<LazyLoadProxySqliteTest.LoadSqliteFixture>
    {
        public LazyLoadProxySqliteTest(LoadSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class LoadSqliteFixture : LoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
