﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPTGearsOfWarQueryRelationalTestBase<TFixture> : GearsOfWarQueryRelationalTestBase<TFixture>
    where TFixture : TPTGearsOfWarQueryRelationalFixture, new()
{
    protected TPTGearsOfWarQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override Task Project_discriminator_columns(bool async)
        => AssertUnableToTranslateEFProperty(() => base.Project_discriminator_columns(async));
}
