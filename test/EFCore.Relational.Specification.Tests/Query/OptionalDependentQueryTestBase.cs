// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OptionalDependentQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : OptionalDependentQueryFixtureBase, new()
{
    protected OptionalDependentQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_projection_entity_with_all_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>(),
            entryCount: 35);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_projection_entity_with_some_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>(),
            entryCount: 59);

    [ConditionalTheory(Skip = "issue #30589")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_optional_dependent_with_all_optional_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>().Where(x => x.Json == null),
            entryCount: 35);

    [ConditionalTheory(Skip = "issue #30589")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_optional_dependent_with_all_optional_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>().Where(x => x.Json != null),
            entryCount: 35);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_optional_dependent_with_some_required_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>().Where(x => x.Json == null),
            entryCount: 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_optional_dependent_with_some_required_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>().Where(x => x.Json != null),
            entryCount: 59);

    [ConditionalTheory(Skip = "issue #30589")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_all_optional_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>().Where(x => x.Json.OpNested1 == null),
            entryCount: 59);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>().Where(x => x.Json.OpNested2 != null),
            entryCount: 23);

    [ConditionalTheory(Skip = "issue #30589")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_some_required_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>().Where(x => x.Json.ReqNested1 == null),
            entryCount: 59);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_some_required_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>().Where(x => x.Json.ReqNested2 != null),
            entryCount: 59);
}
