﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
