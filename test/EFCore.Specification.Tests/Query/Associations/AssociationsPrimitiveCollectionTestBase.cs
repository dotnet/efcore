// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsPrimitiveCollectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Count()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Ints.Count == 3));

    [ConditionalFact]
    public virtual Task Index()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Ints[0] == 1));

    [ConditionalFact]
    public virtual Task Contains()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Ints.Contains(3)));

    [ConditionalFact]
    public virtual Task Any_predicate()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Ints.Any(i => i == 2)));

    [ConditionalFact]
    public virtual Task Nested_Count()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate.Ints.Count == 3));

    [ConditionalFact]
    public virtual Task Select_Sum()
        => AssertQuery(ss => ss.Set<RootEntity>().Select(e => e.RequiredAssociate.Ints.Sum()).Where(sum => sum >= 6));
}
