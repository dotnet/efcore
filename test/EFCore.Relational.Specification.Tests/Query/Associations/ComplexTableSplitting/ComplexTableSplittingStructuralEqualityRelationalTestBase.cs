// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public abstract class
    ComplexTableSplittingStructuralEqualityRelationalTestBase<TFixture> : ComplexPropertiesStructuralEqualityTestBase<TFixture>
    where TFixture : ComplexTableSplittingRelationalFixtureBase, new()
{
    public ComplexTableSplittingStructuralEqualityRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Collections are not supported with table splitting, only JSON
    public override Task Nested_collection_with_parameter()
        => AssertTranslationFailed(() => base.Nested_collection_with_parameter());

    // Collections are not supported with table splitting, only JSON
    public override Task Nested_collection_with_inline()
        => AssertTranslationFailed(() => base.Nested_collection_with_inline());

    // Collections are not supported with table splitting, only JSON
    public override Task Two_nested_collections()
        => AssertTranslationFailed(() => base.Two_nested_collections());

    #region Contains

    public override async Task Contains_with_inline()
    {
        // Collections are not supported with table splitting, only JSON
        await AssertTranslationFailed(base.Contains_with_inline);

        AssertSql();
    }

    public override async Task Contains_with_parameter()
    {
        // Collections are not supported with table splitting, only JSON
        await AssertTranslationFailed(base.Contains_with_parameter);

        AssertSql();
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        // Collections are not supported with table splitting, only JSON
        // Note that the exception is correct, since the collections in the test data are null for table splitting
        await Assert.ThrowsAsync<InvalidOperationException>(base.Contains_with_operators_composed_on_the_collection);

        AssertSql();
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        // Collections are not supported with table splitting, only JSON
        // Note that the exception is correct, since the collections in the test data are null for table splitting
        await Assert.ThrowsAsync<InvalidOperationException>(base.Contains_with_nested_and_composed_operators);

        AssertSql();
    }

    #endregion Contains

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
