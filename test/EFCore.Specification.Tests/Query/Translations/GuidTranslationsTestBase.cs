// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class GuidTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task New_with_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Guid == new Guid("DF36F493-463F-4123-83F9-6B135DEEB7BA")));

    [ConditionalFact]
    public virtual async Task New_with_parameter()
    {
        var guid = "DF36F493-463F-4123-83F9-6B135DEEB7BA";

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Guid == new Guid(guid)));
    }

    [ConditionalFact]
    public virtual Task ToString_projection()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Select(b => b.Guid.ToString()),
            elementAsserter: (e, a) => Assert.Equal(e.ToLower(), a.ToLower()));

    [ConditionalFact]
    public virtual Task NewGuid()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(od => Guid.NewGuid() != default));
}
