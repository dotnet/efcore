// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ManyToManyTrackingSqlServerTest(ManyToManyTrackingSqlServerTest.ManyToManyTrackingSqlServerFixture fixture)
    : ManyToManyTrackingSqlServerTestBase<ManyToManyTrackingSqlServerTest.ManyToManyTrackingSqlServerFixture>(fixture)
{
    public class ManyToManyTrackingSqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
        protected override string StoreName
            => "ManyToManyTrackingSqlServerTest";
    }
}
