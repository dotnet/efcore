// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class GuidTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task New_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Guid == new Guid("DF36F493-463F-4123-83F9-6B135DEEB7BA")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task New_with_parameter(bool async)
    {
        var guid = "DF36F493-463F-4123-83F9-6B135DEEB7BA";

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Guid == new Guid(guid)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToString_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.Guid.ToString()),
            elementAsserter: (e, a) => Assert.Equal(e.ToLower(), a.ToLower()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task NewGuid(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(od => Guid.NewGuid() != default));
}
