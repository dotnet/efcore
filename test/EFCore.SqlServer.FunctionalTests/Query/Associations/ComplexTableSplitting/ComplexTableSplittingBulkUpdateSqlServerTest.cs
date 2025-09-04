// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingBulkUpdateSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingBulkUpdateRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    #region Delete

    public override async Task Delete_entity_with_associations()
    {
        await base.Delete_entity_with_associations();

        AssertSql(
            """
@deletableEntity_Name='?' (Size = 4000)

DELETE FROM [r]
FROM [RootEntity] AS [r]
WHERE [r].[Name] = @deletableEntity_Name
""");
    }

    public override async Task Delete_required_association()
    {
        await base.Delete_required_association();

        AssertSql();
    }

    public override async Task Delete_optional_association()
    {
        await base.Delete_optional_association();

        AssertSql();
    }

    #endregion Delete

    #region Update properties

    public override async Task Update_property_inside_association()
    {
        await base.Update_property_inside_association();

        AssertExecuteUpdateSql(
            """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated_String] = @p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_property_inside_association_with_special_chars()
    {
        await base.Update_property_inside_association_with_special_chars();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[RequiredRelated_String] = N'{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }'
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_String] = N'{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
    }

    public override async Task Update_property_inside_nested()
    {
        await base.Update_property_inside_nested();

        AssertExecuteUpdateSql(
            """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated_RequiredNested_String] = @p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_property_on_projected_association()
    {
        await base.Update_property_on_projected_association();

        AssertExecuteUpdateSql(
            """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated_String] = @p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_property_on_projected_association_with_OrderBy_Skip()
    {
        await base.Update_property_on_projected_association_with_OrderBy_Skip();

        AssertExecuteUpdateSql();
    }

    #endregion Update properties

    #region Update association

    public override async Task Update_association_to_parameter()
    {
        await base.Update_association_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Name='?' (Size = 4000)
@complex_type_p_String='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated_Id] = @complex_type_p_Id,
    [r].[RequiredRelated_Int] = @complex_type_p_Int,
    [r].[RequiredRelated_Name] = @complex_type_p_Name,
    [r].[RequiredRelated_String] = @complex_type_p_String,
    [r].[RequiredRelated_OptionalNested_Id] = @complex_type_p_Id,
    [r].[RequiredRelated_OptionalNested_Int] = @complex_type_p_Int,
    [r].[RequiredRelated_OptionalNested_Name] = @complex_type_p_Name,
    [r].[RequiredRelated_OptionalNested_String] = @complex_type_p_String,
    [r].[RequiredRelated_RequiredNested_Id] = @complex_type_p_Id,
    [r].[RequiredRelated_RequiredNested_Int] = @complex_type_p_Int,
    [r].[RequiredRelated_RequiredNested_Name] = @complex_type_p_Name,
    [r].[RequiredRelated_RequiredNested_String] = @complex_type_p_String
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_association_to_parameter()
    {
        await base.Update_nested_association_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Name='?' (Size = 4000)
@complex_type_p_String='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated_RequiredNested_Id] = @complex_type_p_Id,
    [r].[RequiredRelated_RequiredNested_Int] = @complex_type_p_Int,
    [r].[RequiredRelated_RequiredNested_Name] = @complex_type_p_Name,
    [r].[RequiredRelated_RequiredNested_String] = @complex_type_p_String
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_another_association()
    {
        await base.Update_association_to_another_association();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated_Id] = [r].[RequiredRelated_Id],
    [r].[OptionalRelated_Int] = [r].[RequiredRelated_Int],
    [r].[OptionalRelated_Name] = [r].[RequiredRelated_Name],
    [r].[OptionalRelated_String] = [r].[RequiredRelated_String],
    [r].[OptionalRelated_OptionalNested_Id] = [r].[OptionalRelated_OptionalNested_Id],
    [r].[OptionalRelated_OptionalNested_Int] = [r].[OptionalRelated_OptionalNested_Int],
    [r].[OptionalRelated_OptionalNested_Name] = [r].[OptionalRelated_OptionalNested_Name],
    [r].[OptionalRelated_OptionalNested_String] = [r].[OptionalRelated_OptionalNested_String],
    [r].[OptionalRelated_RequiredNested_Id] = [r].[OptionalRelated_RequiredNested_Id],
    [r].[OptionalRelated_RequiredNested_Int] = [r].[OptionalRelated_RequiredNested_Int],
    [r].[OptionalRelated_RequiredNested_Name] = [r].[OptionalRelated_RequiredNested_Name],
    [r].[OptionalRelated_RequiredNested_String] = [r].[OptionalRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_association_to_another_nested_association()
    {
        await base.Update_nested_association_to_another_nested_association();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[RequiredRelated_OptionalNested_Id] = [r].[RequiredRelated_RequiredNested_Id],
    [r].[RequiredRelated_OptionalNested_Int] = [r].[RequiredRelated_RequiredNested_Int],
    [r].[RequiredRelated_OptionalNested_Name] = [r].[RequiredRelated_RequiredNested_Name],
    [r].[RequiredRelated_OptionalNested_String] = [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_inline()
    {
        await base.Update_association_to_inline();

        AssertExecuteUpdateSql(
            """
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Name='?' (Size = 4000)
@complex_type_p_String='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated_Id] = @complex_type_p_Id,
    [r].[RequiredRelated_Int] = @complex_type_p_Int,
    [r].[RequiredRelated_Name] = @complex_type_p_Name,
    [r].[RequiredRelated_String] = @complex_type_p_String,
    [r].[RequiredRelated_OptionalNested_Id] = @complex_type_p_Id,
    [r].[RequiredRelated_OptionalNested_Int] = @complex_type_p_Int,
    [r].[RequiredRelated_OptionalNested_Name] = @complex_type_p_Name,
    [r].[RequiredRelated_OptionalNested_String] = @complex_type_p_String,
    [r].[RequiredRelated_RequiredNested_Id] = @complex_type_p_Id,
    [r].[RequiredRelated_RequiredNested_Int] = @complex_type_p_Int,
    [r].[RequiredRelated_RequiredNested_Name] = @complex_type_p_Name,
    [r].[RequiredRelated_RequiredNested_String] = @complex_type_p_String
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_inline_with_lambda()
    {
        await base.Update_association_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[RequiredRelated_Id] = 0,
    [r].[RequiredRelated_Int] = 70,
    [r].[RequiredRelated_Name] = N'Updated related name',
    [r].[RequiredRelated_String] = N'Updated related string',
    [r].[RequiredRelated_OptionalNested_Id] = NULL,
    [r].[RequiredRelated_OptionalNested_Int] = NULL,
    [r].[RequiredRelated_OptionalNested_Name] = NULL,
    [r].[RequiredRelated_OptionalNested_String] = NULL,
    [r].[RequiredRelated_RequiredNested_Id] = 0,
    [r].[RequiredRelated_RequiredNested_Int] = 80,
    [r].[RequiredRelated_RequiredNested_Name] = N'Updated nested name',
    [r].[RequiredRelated_RequiredNested_String] = N'Updated nested string'
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_association_to_inline_with_lambda()
    {
        await base.Update_nested_association_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[RequiredRelated_RequiredNested_Id] = 0,
    [r].[RequiredRelated_RequiredNested_Int] = 80,
    [r].[RequiredRelated_RequiredNested_Name] = N'Updated nested name',
    [r].[RequiredRelated_RequiredNested_String] = N'Updated nested string'
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_null()
    {
        await base.Update_association_to_null();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated_Id] = NULL,
    [r].[OptionalRelated_Int] = NULL,
    [r].[OptionalRelated_Name] = NULL,
    [r].[OptionalRelated_String] = NULL,
    [r].[OptionalRelated_OptionalNested_Id] = NULL,
    [r].[OptionalRelated_OptionalNested_Int] = NULL,
    [r].[OptionalRelated_OptionalNested_Name] = NULL,
    [r].[OptionalRelated_OptionalNested_String] = NULL,
    [r].[OptionalRelated_RequiredNested_Id] = NULL,
    [r].[OptionalRelated_RequiredNested_Int] = NULL,
    [r].[OptionalRelated_RequiredNested_Name] = NULL,
    [r].[OptionalRelated_RequiredNested_String] = NULL
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_null_with_lambda()
    {
        await base.Update_association_to_null_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated_Id] = NULL,
    [r].[OptionalRelated_Int] = NULL,
    [r].[OptionalRelated_Name] = NULL,
    [r].[OptionalRelated_String] = NULL,
    [r].[OptionalRelated_OptionalNested_Id] = NULL,
    [r].[OptionalRelated_OptionalNested_Int] = NULL,
    [r].[OptionalRelated_OptionalNested_Name] = NULL,
    [r].[OptionalRelated_OptionalNested_String] = NULL,
    [r].[OptionalRelated_RequiredNested_Id] = NULL,
    [r].[OptionalRelated_RequiredNested_Int] = NULL,
    [r].[OptionalRelated_RequiredNested_Name] = NULL,
    [r].[OptionalRelated_RequiredNested_String] = NULL
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_null_parameter()
    {
        await base.Update_association_to_null_parameter();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated_Id] = NULL,
    [r].[OptionalRelated_Int] = NULL,
    [r].[OptionalRelated_Name] = NULL,
    [r].[OptionalRelated_String] = NULL,
    [r].[OptionalRelated_OptionalNested_Id] = NULL,
    [r].[OptionalRelated_OptionalNested_Int] = NULL,
    [r].[OptionalRelated_OptionalNested_Name] = NULL,
    [r].[OptionalRelated_OptionalNested_String] = NULL,
    [r].[OptionalRelated_RequiredNested_Id] = NULL,
    [r].[OptionalRelated_RequiredNested_Int] = NULL,
    [r].[OptionalRelated_RequiredNested_Name] = NULL,
    [r].[OptionalRelated_RequiredNested_String] = NULL
FROM [RootEntity] AS [r]
""");
    }

    #endregion Update association

    #region Update collection

    public override async Task Update_collection_to_parameter()
    {
        await base.Update_collection_to_parameter();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_nested_collection_to_parameter()
    {
        await base.Update_nested_collection_to_parameter();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_nested_collection_to_inline_with_lambda()
    {
        await base.Update_nested_collection_to_inline_with_lambda();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_collection_referencing_the_original_collection()
    {
        await base.Update_collection_referencing_the_original_collection();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_nested_collection_to_another_nested_collection()
    {
        await base.Update_nested_collection_to_another_nested_collection();

        AssertExecuteUpdateSql();
    }

    #endregion Update collection

    #region Multiple updates

    public override async Task Update_multiple_properties_inside_same_association()
    {
        await base.Update_multiple_properties_inside_same_association();

        AssertExecuteUpdateSql(
            """
@p='?' (Size = 4000)
@p0='?' (DbType = Int32)

UPDATE [r]
SET [r].[RequiredRelated_String] = @p,
    [r].[RequiredRelated_Int] = @p0
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_multiple_properties_inside_associations_and_on_entity_type()
    {
        await base.Update_multiple_properties_inside_associations_and_on_entity_type();

        AssertExecuteUpdateSql(
            """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[Name] = [r].[Name] + N'Modified',
    [r].[RequiredRelated_String] = [r].[OptionalRelated_String],
    [r].[OptionalRelated_RequiredNested_String] = @p
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated_Id] IS NOT NULL
""");
    }

    public override async Task Update_multiple_projected_associations_via_anonymous_type()
    {
        await base.Update_multiple_projected_associations_via_anonymous_type();

        AssertExecuteUpdateSql(
            """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated_String] = [r].[OptionalRelated_String],
    [r].[OptionalRelated_String] = @p
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated_Id] IS NOT NULL
""");
    }

    #endregion Multiple updates

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
