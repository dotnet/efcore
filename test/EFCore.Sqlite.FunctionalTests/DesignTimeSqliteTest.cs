// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class DesignTimeSqliteTest : DesignTimeTestBase<DesignTimeSqliteTest.DesignTimeSqliteFixture>
    {
        public DesignTimeSqliteTest(DesignTimeSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override Assembly ProviderAssembly
            => typeof(SqliteDesignTimeServices).Assembly;

        public class DesignTimeSqliteFixture : DesignTimeFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }
}
