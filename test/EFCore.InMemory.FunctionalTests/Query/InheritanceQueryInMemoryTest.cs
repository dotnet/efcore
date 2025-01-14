// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class InheritanceQueryInMemoryTest(InheritanceQueryInMemoryFixture fixture)
    : InheritanceQueryTestBase<InheritanceQueryInMemoryFixture>(fixture)
{
    public override async Task Can_query_all_animal_views(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Can_query_all_animal_views(async))).Message;

        Assert.Equal(
            CoreStrings.TranslationFailed(
                @"DbSet<Bird>()
    .Select(b => InheritanceQueryInMemoryFixture.MaterializeView(b))
    .OrderBy(a => a.CountryId)"),
            message,
            ignoreLineEndingDifferences: true);
    }

    protected override bool EnforcesFkConstraints
        => false;
}
