// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsBulkUpdateSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsBulkUpdateRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    // FK constraint failures
    public override Task Delete_entity_with_associations()
        => Assert.ThrowsAsync<SqlException>(base.Delete_entity_with_associations);

    public override Task Delete_required_associate()
        => Assert.ThrowsAsync<SqlException>(base.Delete_required_associate);

    public override Task Delete_optional_associate()
        => Assert.ThrowsAsync<SqlException>(base.Delete_optional_associate);

    public override async Task Update_property_inside_associate()
    {
        await base.Update_property_inside_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated' (Size = 4000)

SET NOCOUNT OFF;
UPDATE [a]
SET [a].[String] = @p
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
""");
    }

    public override async Task Update_property_inside_associate_with_special_chars()
    {
        await base.Update_property_inside_associate_with_special_chars();

        AssertExecuteUpdateSql(
            """
SET NOCOUNT OFF;
UPDATE [a0]
SET [a0].[String] = N'{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }'
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
WHERE [a].[String] = N'{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
    }

    public override async Task Update_property_on_projected_associate()
    {
        await base.Update_property_on_projected_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated' (Size = 4000)

SET NOCOUNT OFF;
UPDATE [a]
SET [a].[String] = @p
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
""");
    }

    public override Task Update_property_on_projected_associate_with_OrderBy_Skip()
        => Assert.ThrowsAsync<EqualException>(base.Update_property_on_projected_associate_with_OrderBy_Skip);

    public override async Task Update_multiple_properties_inside_same_associate()
    {
        await base.Update_multiple_properties_inside_same_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated' (Size = 4000)
@p1='20'

SET NOCOUNT OFF;
UPDATE [a]
SET [a].[String] = @p,
    [a].[Int] = @p1
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
""");
    }

    public override async Task Update_primitive_collection_to_constant()
    {
        await base.Update_primitive_collection_to_constant();

        AssertExecuteUpdateSql(
            """
SET NOCOUNT OFF;
UPDATE [a]
SET [a].[Ints] = N'[1,2,4]'
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
""");
    }

    public override async Task Update_primitive_collection_to_parameter()
    {
        await base.Update_primitive_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@ints='[1,2,4]' (Size = 4000)

SET NOCOUNT OFF;
UPDATE [a]
SET [a].[Ints] = @ints
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
""");
    }

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

    public override async Task Update_property_inside_nested_associate()
    {
        await base.Update_property_inside_nested_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated' (Size = 4000)

SET NOCOUNT OFF;
UPDATE [n]
SET [n].[String] = @p
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
""");
    }

    public override async Task Update_associate_with_null_required_property()
    {
        await base.Update_associate_with_null_required_property();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_required_nested_associate_to_null()
    {
        await base.Update_required_nested_associate_to_null();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_inside_primitive_collection()
    {
        await base.Update_inside_primitive_collection();

        AssertExecuteUpdateSql(
            """
@p='99'

SET NOCOUNT OFF;
UPDATE [a0]
SET [a0].[Ints] = JSON_MODIFY([a0].[Ints], '$[1]', @p)
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([a].[Ints]) AS [i]) >= 2
""");
    }
}
