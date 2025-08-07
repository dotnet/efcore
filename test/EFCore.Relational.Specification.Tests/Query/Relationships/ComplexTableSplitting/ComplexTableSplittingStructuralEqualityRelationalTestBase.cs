// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public abstract class ComplexTableSplittingStructuralEqualityRelationalTestBase<TFixture> : ComplexPropertiesStructuralEqualityTestBase<TFixture>
    where TFixture : ComplexTableSplittingRelationalFixtureBase, new()
{
    public ComplexTableSplittingStructuralEqualityRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // TODO: All the tests below rely on access OptionalRelated, but optional complex properties not yet supported (#31376)

    public override Task Two_related()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Two_related());

    public override Task Two_nested()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Two_nested());

    public override Task Not_equals()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Not_equals());

    public override Task Related_with_inline_null()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_inline_null());

    public override Task Related_with_parameter_null()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_parameter_null());

    public override Task Nested_with_inline_null()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_inline_null());

    public override Task Two_nested_collections()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Two_nested_collections());

    // Collection equality with owned collections is not supported
    public override Task Nested_collection_with_inline()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_inline());

    // Collection equality with owned collections is not supported
    public override Task Nested_collection_with_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_parameter());

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
