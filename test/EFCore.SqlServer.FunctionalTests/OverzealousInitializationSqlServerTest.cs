// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class OverzealousInitializationSqlServerTest
        : OverzealousInitializationTestBase<OverzealousInitializationSqlServerTest.OverzealousInitializationSqlServerFixture>
    {
        public OverzealousInitializationSqlServerTest(OverzealousInitializationSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class OverzealousInitializationSqlServerFixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }
}
