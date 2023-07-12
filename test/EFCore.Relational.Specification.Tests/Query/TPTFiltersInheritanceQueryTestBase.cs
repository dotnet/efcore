// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class TPTFiltersInheritanceQueryTestBase<TFixture> : FiltersInheritanceQueryTestBase<TFixture>
    where TFixture : TPTInheritanceQueryFixture, new()
{
    public TPTFiltersInheritanceQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }
}
