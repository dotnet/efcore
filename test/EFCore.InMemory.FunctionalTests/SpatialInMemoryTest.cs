// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SpatialInMemoryTest(SpatialInMemoryFixture fixture) : SpatialTestBase<SpatialInMemoryFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }
}
