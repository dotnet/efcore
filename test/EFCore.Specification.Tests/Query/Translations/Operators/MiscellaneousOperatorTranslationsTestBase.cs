// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class MiscellaneousOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual async Task Conditional()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int == 8 ? b.String : "Foo") == "Seattle"));

    [ConditionalFact]
    public virtual async Task Coalesce()
        => await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(b => (b.String ?? "Unknown") == "Seattle"));
}
