// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class OverzealousInitializationSqliteTest
        : OverzealousInitializationTestBase<OverzealousInitializationSqliteTest.OverzealousInitializationSqliteFixture>
    {
        public OverzealousInitializationSqliteTest(OverzealousInitializationSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class OverzealousInitializationSqliteFixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }
}
