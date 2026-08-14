// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public abstract class ComplexTableSplittingBulkUpdateRelationalTestBase<TFixture> : ComplexPropertiesBulkUpdateTestBase<TFixture>
    where TFixture : ComplexTableSplittingRelationalFixtureBase, new()
{
    public ComplexTableSplittingBulkUpdateRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // #36678 - ExecuteDelete on complex type
    public override Task Delete_required_associate()
        => AssertTranslationFailedWithDetails(RelationalStrings.ExecuteDeleteOnNonEntityType, base.Delete_required_associate);

    // #36678 - ExecuteDelete on complex type
    public override Task Delete_optional_associate()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Delete_optional_associate);

    // #36336
    public override Task Update_property_on_projected_associate_with_OrderBy_Skip()
        => AssertTranslationFailedWithDetails(
            RelationalStrings.ExecuteUpdateSubqueryNotSupportedOverComplexTypes("RootEntity.RequiredAssociate#AssociateType"),
            base.Update_property_on_projected_associate_with_OrderBy_Skip);

    #region Update collection

    // Collections are not supported with table splitting, only JSON
    public override async Task Update_collection_to_parameter()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(base.Update_collection_to_parameter);

        AssertExecuteUpdateSql();
    }

    // Collections are not supported with table splitting, only JSON
    public override async Task Update_nested_collection_to_parameter()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_collection_to_parameter);

        AssertExecuteUpdateSql();
    }

    // Collections are not supported with table splitting, only JSON
    public override async Task Update_nested_collection_to_inline_with_lambda()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_collection_to_inline_with_lambda);

        AssertExecuteUpdateSql();
    }

    // Collections are not supported with table splitting, only JSON
    public override async Task Update_collection_referencing_the_original_collection()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(base.Update_collection_referencing_the_original_collection);

        AssertExecuteUpdateSql();
    }

    // Collections are not supported with table splitting, only JSON
    public override async Task Update_nested_collection_to_another_nested_collection()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_collection_to_another_nested_collection);

        AssertExecuteUpdateSql();
    }

    // Collections are not supported with table splitting, only JSON
    public override async Task Update_inside_structural_collection()
    {
        await Assert.ThrowsAsync<NullReferenceException>(base.Update_inside_structural_collection);

        AssertExecuteUpdateSql();
    }

    #endregion Update collection

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
