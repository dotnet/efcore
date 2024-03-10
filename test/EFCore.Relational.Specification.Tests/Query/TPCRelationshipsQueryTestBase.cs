﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPCRelationshipsQueryTestBase<TFixture> : InheritanceRelationshipsQueryRelationalTestBase<TFixture>
    where TFixture : TPCRelationshipsQueryRelationalFixture, new()
{
    protected TPCRelationshipsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }
}
