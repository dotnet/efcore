// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

/// <summary>
///     Contains tests exercising complex type support in inheritance scenarios.
/// </summary>
/// <remarks>
///     These are split out into their own class because (a) some providers don't support complex typeS (e.g. Cosmos, InMemory),
///     and (b) some providers have multiple mapping options for them (relational can map them via table splitting or JSON), and
///     so can extend this test class twice.
/// </remarks>
public abstract class InheritanceComplexTypesQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : InheritanceQueryFixtureBase, new()
{
    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_complex_type_property_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Coke>().Where(d => d.ChildComplexType!.Int == 10));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_complex_type_property_on_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Drink>().Where(d => d.ParentComplexType!.Int == 8));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Coke>().Where(d => d.ChildComplexType!.Nested!.NestedInt == 58));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_nested_complex_type_property_on_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Drink>().Where(d => d.ParentComplexType!.Nested!.NestedInt == 50));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_complex_type_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Coke>().Select(d => d.ChildComplexType));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_complex_type_on_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Drink>().Select(d => d.ParentComplexType));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_nested_complex_type_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Coke>().Select(d => d.ChildComplexType!.Nested));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_nested_complex_type_on_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Drink>().Select(d => d.ParentComplexType!.Nested));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_over_complex_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Drink>().Where(d => d.ComplexTypeCollection.Count(c => c.Int > 59) == 2),
            ss => ss.Set<Drink>().Where(d => d.ComplexTypeCollection != null && d.ComplexTypeCollection.Count(c => c.Int > 59) == 2));
}
