// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class MusicStoreSqlServerTest : MusicStoreTestBase<MusicStoreSqlServerTest.MusicStoreSqlServerFixture>
{
    public MusicStoreSqlServerTest(MusicStoreSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class MusicStoreSqlServerFixture : MusicStoreFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
