// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public abstract class
    OwnedTableSplittingStructuralEqualityRelationalTestBase<TFixture> : OwnedNavigationsStructuralEqualityTestBase<TFixture>
    where TFixture : OwnedTableSplittingRelationalFixtureBase, new()
{
    public OwnedTableSplittingStructuralEqualityRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Contains

    public override async Task Contains_with_inline()
    {
        // No backing field could be found for property 'RootEntity.RequiredRelated#RelatedType.NestedCollection#NestedType.RelatedTypeRootEntityId' and the property does not have a getter.
        await Assert.ThrowsAsync<InvalidOperationException>(base.Contains_with_inline);

        AssertSql();
    }

    public override async Task Contains_with_parameter()
    {
        // No backing field could be found for property 'RootEntity.RequiredRelated#RelatedType.NestedCollection#NestedType.RelatedTypeRootEntityId' and the property does not have a getter.
        await Assert.ThrowsAsync<InvalidOperationException>(base.Contains_with_parameter);

        AssertSql();
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        // No backing field could be found for property 'RootEntity.RequiredRelated#RelatedType.NestedCollection#NestedType.RelatedTypeRootEntityId' and the property does not have a getter.
        await Assert.ThrowsAsync<InvalidOperationException>(base.Contains_with_operators_composed_on_the_collection);

        AssertSql();
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        // No backing field could be found for property 'RootEntity.RequiredRelated#RelatedType.NestedCollection#NestedType.RelatedTypeRootEntityId' and the property does not have a getter.
        await Assert.ThrowsAsync<InvalidOperationException>(base.Contains_with_nested_and_composed_operators);

        AssertSql();
    }

    #endregion Contains

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
