// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class DesignTimeSqlServerTest : DesignTimeTestBase<DesignTimeSqlServerTest.DesignTimeSqlServerFixture>
    {
        public DesignTimeSqlServerTest(DesignTimeSqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override Assembly ProviderAssembly
            => typeof(SqlServerDesignTimeServices).Assembly;

        public class DesignTimeSqlServerFixture : DesignTimeFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }
}
