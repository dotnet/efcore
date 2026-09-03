// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsStructuralEqualityRelationalTestBase<TFixture> : OwnedNavigationsStructuralEqualityTestBase<TFixture>
    where TFixture : OwnedNavigationsRelationalFixtureBase, new()
{
    public OwnedNavigationsStructuralEqualityRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Contains

    public override async Task Contains_with_inline()
    {
        // The given key 'Property: RootEntity.RequiredRelated#RelatedType.NestedCollection#NestedType.__synthesizedOrdinal (no field, int) Shadow Required PK AfterSave:Throw ValueGenerated.OnAdd' was not present in the dictionary.
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
        // No backing field could be found for property 'RootEntity.RelatedCollection#RelatedType.RootEntityId' and the property does not have a getter.
        await Assert.ThrowsAsync<InvalidOperationException>(base.Contains_with_nested_and_composed_operators);

        AssertSql();
    }

    #endregion Contains

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
