// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class MusicStoreSqliteTest(MusicStoreSqliteTest.MusicStoreSqliteFixture fixture) : MusicStoreTestBase<MusicStoreSqliteTest.MusicStoreSqliteFixture>(fixture)
{
    public class MusicStoreSqliteFixture : MusicStoreFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
