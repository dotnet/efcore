// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public abstract class ComplexPropertiesPrimitiveCollectionTestBase<TFixture>(TFixture fixture)
    : AssociationsPrimitiveCollectionTestBase<TFixture>(fixture)
    where TFixture : ComplexPropertiesFixtureBase, new()
{
    [Fact]
    public virtual Task Any_predicate_on_GroupBy_First()
        => AssertQuery(ss => ss.Set<RootEntity>()
            .GroupBy(e => e.Id % 2)
            .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
            .Where(e => e.RequiredAssociate.Ints.Any(i => i == 2)));
}
