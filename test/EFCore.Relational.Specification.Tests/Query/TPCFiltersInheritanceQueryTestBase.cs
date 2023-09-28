// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class TPCFiltersInheritanceQueryTestBase<TFixture> : FiltersInheritanceQueryTestBase<TFixture>
    where TFixture : TPCInheritanceQueryFixture, new()
{
    public TPCFiltersInheritanceQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }
}
