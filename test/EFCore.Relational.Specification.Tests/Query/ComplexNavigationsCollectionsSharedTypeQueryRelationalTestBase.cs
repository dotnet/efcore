// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class
    ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<TFixture> : ComplexNavigationsCollectionsSharedTypeQueryTestBase<
        TFixture>
    where TFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase, new()
{
    protected ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override async Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
        => Assert.Equal(
            RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async))).Message);
}
