// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.EntitySplitting;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class EntitySplittingQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : EntitySplittingQueryFixtureBase, new()
{
    public EntitySplittingQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_entity_which_is_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<SplitEntityOne>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_entity_which_is_split_selecting_only_main_properties(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<SplitEntityOne>().Select(
                e => new
                {
                    e.Id,
                    e.SharedValue,
                    e.Value
                }));
}
