// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsCollectionsSharedQueryTypeRelationalTestBase<TFixture> : ComplexNavigationsCollectionsSharedTypeQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase, new()
    {
        protected ComplexNavigationsCollectionsSharedQueryTypeRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }
    }
}
