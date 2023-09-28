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
            entryCount: 36);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_projection_entity_with_some_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>(),
            entryCount: 60);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_optional_dependent_with_all_optional_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>().Where(x => x.Json == null),
            entryCount: 1);

    [ConditionalTheory]
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
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_optional_dependent_with_some_required_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>().Where(x => x.Json != null),
            entryCount: 59);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_all_optional_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>().Where(x => x.Json.OpNav1 == null),
            entryCount: 10);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntityAllOptional>().Where(x => x.Json.OpNav2 != null),
            entryCount: 23);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_some_required_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>().Where(x => x.Json.ReqNav1 == null),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_nested_optional_dependent_with_some_required_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OptionalDependentEntitySomeRequired>().Where(x => x.Json.ReqNav2 != null),
            entryCount: 59);
}
