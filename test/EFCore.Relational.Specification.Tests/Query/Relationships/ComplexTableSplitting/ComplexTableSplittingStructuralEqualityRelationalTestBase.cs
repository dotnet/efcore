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

    public override Task Two_related(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Two_related(async));

    public override Task Two_nested(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Two_nested(async));

    public override Task Not_equals(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Not_equals(async));

    public override Task Related_with_inline_null(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_inline_null(async));

    public override Task Related_with_parameter_null(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_parameter_null(async));

    public override Task Nested_with_inline_null(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_inline_null(async));

    public override Task Two_nested_collections(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Two_nested_collections(async));

    // Collection equality with owned collections is not supported
    public override Task Nested_collection_with_inline(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_inline(async));

    // Collection equality with owned collections is not supported
    public override Task Nested_collection_with_parameter(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_parameter(async));

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
