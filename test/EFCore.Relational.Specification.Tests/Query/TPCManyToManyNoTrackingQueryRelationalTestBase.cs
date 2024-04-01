// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPCManyToManyNoTrackingQueryRelationalTestBase<TFixture> : ManyToManyNoTrackingQueryRelationalTestBase<TFixture>
    where TFixture : TPCManyToManyQueryRelationalFixture, new()
{
    protected TPCManyToManyNoTrackingQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }
}
