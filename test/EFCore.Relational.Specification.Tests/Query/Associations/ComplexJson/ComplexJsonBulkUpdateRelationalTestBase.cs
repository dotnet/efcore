// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public abstract class ComplexJsonBulkUpdateRelationalTestBase<TFixture> : ComplexPropertiesBulkUpdateTestBase<TFixture>
    where TFixture : ComplexJsonRelationalFixtureBase, new()
{
    public ComplexJsonBulkUpdateRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
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

    // #36679: non-constant inline array/list translation
    public override Task Update_collection_referencing_the_original_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_collection_referencing_the_original_collection);

    // #36722: support ExecuteUpdate inside structural collections mapped to JSON
    public override Task Update_inside_structural_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_inside_structural_collection);

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
