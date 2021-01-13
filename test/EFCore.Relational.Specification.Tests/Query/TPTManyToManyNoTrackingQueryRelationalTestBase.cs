// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTManyToManyNoTrackingQueryRelationalTestBase<TFixture> : ManyToManyNoTrackingQueryRelationalTestBase<TFixture>
        where TFixture : TPTManyToManyQueryRelationalFixture, new()
    {
        protected TPTManyToManyNoTrackingQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }
    }
}
