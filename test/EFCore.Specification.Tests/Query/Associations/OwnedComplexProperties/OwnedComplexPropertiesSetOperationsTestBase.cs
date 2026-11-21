// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public abstract class OwnedComplexPropertiesSetOperationsTestBase<TFixture>(TFixture fixture)
    : AssociationsSetOperationsTestBase<TFixture>(fixture)
    where TFixture : OwnedComplexPropertiesFixtureBase, new()
{
    [Fact]
    public virtual Task Union_over_associate_collections()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.AssociateCollection.Where(r => r.Int == 8)
                .Union(e.AssociateCollection.Where(r => r.String == "foo"))
                .Count()
            == 2));

    [Fact]
    public virtual Task Union_associates_then_project_nested_associate()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.Id < 3).Select(e => e.RequiredAssociate)
                .Union(ss.Set<RootEntity>().Where(e => e.Id > 4).Select(e => e.RequiredAssociate))
                .Select(a => a.OptionalNestedAssociate),
            elementSorter: e => e?.Id);

    [Fact]
    public virtual Task Union_associates_then_filter_on_nested_associate()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.Id < 3).Select(e => e.RequiredAssociate)
                .Union(ss.Set<RootEntity>().Where(e => e.Id > 4).Select(e => e.RequiredAssociate))
                .Where(a => a.OptionalNestedAssociate!.Int == 8)
                .Select(a => a.Id));
}
