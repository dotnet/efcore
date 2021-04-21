// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTRelationshipsQueryTestBase<TFixture> : InheritanceRelationshipsQueryRelationalTestBase<TFixture>
        where TFixture : TPTRelationshipsQueryRelationalFixture, new()
    {
        protected TPTRelationshipsQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }
    }
}
