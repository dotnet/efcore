// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyTrackingSqlServerTest
    : ManyToManyTrackingSqlServerTestBase<ManyToManyTrackingSqlServerTest.ManyToManyTrackingSqlServerFixture>
{
    public ManyToManyTrackingSqlServerTest(ManyToManyTrackingSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class ManyToManyTrackingSqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
    }
}
