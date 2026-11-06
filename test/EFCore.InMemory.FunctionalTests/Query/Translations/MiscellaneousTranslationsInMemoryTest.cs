// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class MiscellaneousTranslationsInMemoryTest(BasicTypesQueryInMemoryFixture fixture)
    : MiscellaneousTranslationsTestBase<BasicTypesQueryInMemoryFixture>(fixture)
{
    // The in-memory provider evaluates the comparer itself, so these queries run and honor its ordering.
    public override Task StringComparer_Compare()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => StringComparer.OrdinalIgnoreCase.Compare(c.String, "seattle") == 0));

    public override async Task Custom_Comparer_Compare()
    {
        var reversed = Comparer<int>.Create((x, y) => y.CompareTo(x));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => reversed.Compare(c.Int, 8) > 0));

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(c => reversed.Compare(c.Int, 8)).ThenBy(c => c.Id),
            assertOrder: true);
    }

    public override Task Default_Comparer_Compare()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => Comparer<int>.Default.Compare(c.Int, 8) > 0));

    public override Task Nullable_Compare()
        => AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(c => Nullable.Compare(c.Int, 8) == 0));
}
