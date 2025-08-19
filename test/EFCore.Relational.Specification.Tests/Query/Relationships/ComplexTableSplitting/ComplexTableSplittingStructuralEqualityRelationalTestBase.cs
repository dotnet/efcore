// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

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

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
