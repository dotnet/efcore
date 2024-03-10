// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPTRelationshipsQueryTestBase<TFixture> : InheritanceRelationshipsQueryRelationalTestBase<TFixture>
    where TFixture : TPTRelationshipsQueryRelationalFixture, new()
{
    protected TPTRelationshipsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }
}
