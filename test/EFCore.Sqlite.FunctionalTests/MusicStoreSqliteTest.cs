// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class MusicStoreSqliteTest : MusicStoreTestBase<MusicStoreSqliteTest.MusicStoreSqliteFixture>
    {
        public MusicStoreSqliteTest(MusicStoreSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class MusicStoreSqliteFixture : MusicStoreFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }
}
