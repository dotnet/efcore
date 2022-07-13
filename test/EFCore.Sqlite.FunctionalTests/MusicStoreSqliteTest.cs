// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

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
