// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsBulkUpdateSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsBulkUpdateRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    // FK constraint failures
    public override Task Delete_entity_with_associations()
        => Assert.ThrowsAsync<SqlException>(base.Delete_entity_with_associations);

    public override Task Delete_required_associate()
        => Assert.ThrowsAsync<SqlException>(base.Delete_required_associate);

    // SQL generation produces invalid object name for these scenarios
    public override Task Delete_optional_associate()
        => Assert.ThrowsAsync<SqlException>(base.Delete_optional_associate);

    public override Task Update_property_inside_associate()
        => Assert.ThrowsAsync<SqlException>(base.Update_property_inside_associate);

    public override Task Update_property_inside_associate_with_special_chars()
        => Assert.ThrowsAsync<SqlException>(base.Update_property_inside_associate_with_special_chars);

    public override Task Update_property_on_projected_associate()
        => Assert.ThrowsAsync<SqlException>(base.Update_property_on_projected_associate);

    public override async Task Update_property_on_projected_associate_with_OrderBy_Skip()
        => await Assert.ThrowsAnyAsync<Exception>(base.Update_property_on_projected_associate_with_OrderBy_Skip);

    public override Task Update_multiple_properties_inside_same_associate()
        => Assert.ThrowsAsync<SqlException>(base.Update_multiple_properties_inside_same_associate);

    public override Task Update_primitive_collection_to_constant()
        => Assert.ThrowsAsync<SqlException>(base.Update_primitive_collection_to_constant);

    public override Task Update_primitive_collection_to_parameter()
        => Assert.ThrowsAsync<SqlException>(base.Update_primitive_collection_to_parameter);

    // Translation not yet supported for navigation-mapped associations
    public override Task Update_associate_to_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_associate_to_parameter);

    public override Task Update_associate_to_inline()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_associate_to_inline);

    public override Task Update_associate_to_inline_with_lambda()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_associate_to_inline_with_lambda);

    public override Task Update_associate_to_another_associate()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_associate_to_another_associate);

    public override Task Update_associate_to_null()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_associate_to_null);

    public override Task Update_associate_to_null_with_lambda()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_associate_to_null_with_lambda);

    public override Task Update_associate_to_null_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_associate_to_null_parameter);

    public override Task Update_nested_associate_to_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_associate_to_parameter);

    public override Task Update_nested_associate_to_inline_with_lambda()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_associate_to_inline_with_lambda);

    public override Task Update_nested_associate_to_another_nested_associate()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_associate_to_another_nested_associate);

    public override Task Update_nested_collection_to_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_collection_to_parameter);

    public override Task Update_nested_collection_to_inline_with_lambda()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_collection_to_inline_with_lambda);

    public override Task Update_nested_collection_to_another_nested_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_nested_collection_to_another_nested_collection);

    public override Task Update_collection_to_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_collection_to_parameter);

    public override Task Update_collection_referencing_the_original_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_collection_referencing_the_original_collection);

    public override Task Update_primitive_collection_to_another_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_primitive_collection_to_another_collection);

    public override Task Update_inside_structural_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_inside_structural_collection);

    public override Task Update_multiple_properties_inside_associates_and_on_entity_type()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_multiple_properties_inside_associates_and_on_entity_type);

    public override Task Update_multiple_projected_associates_via_anonymous_type()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Update_multiple_projected_associates_via_anonymous_type);
}
