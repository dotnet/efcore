// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class
    ComplexNavigationsCollectionsQueryRelationalTestBase<TFixture> : ComplexNavigationsCollectionsQueryTestBase<TFixture>
    where TFixture : ComplexNavigationsQueryFixtureBase, new()
{
    protected ComplexNavigationsCollectionsQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }
}
