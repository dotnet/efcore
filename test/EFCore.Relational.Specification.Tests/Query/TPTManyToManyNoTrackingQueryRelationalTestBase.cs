// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPTManyToManyNoTrackingQueryRelationalTestBase<TFixture> : ManyToManyNoTrackingQueryRelationalTestBase<TFixture>
    where TFixture : TPTManyToManyQueryRelationalFixture, new()
{
    protected TPTManyToManyNoTrackingQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }
}
