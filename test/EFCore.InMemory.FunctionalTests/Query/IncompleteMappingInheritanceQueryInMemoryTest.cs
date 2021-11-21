// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Query;

public class IncompleteMappingInheritanceQueryInMemoryTest : InheritanceQueryTestBase<IncompleteMappingInheritanceQueryInMemoryFixture>
{
    public IncompleteMappingInheritanceQueryInMemoryTest(IncompleteMappingInheritanceQueryInMemoryFixture fixture)
        : base(fixture)
    {
    }

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
