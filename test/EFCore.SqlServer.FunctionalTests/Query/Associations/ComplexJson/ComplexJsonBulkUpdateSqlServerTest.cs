// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonBulkUpdateSqlServerTest(
    ComplexJsonSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexJsonBulkUpdateRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
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

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [RequiredRelated].modify('$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_property_inside_association_with_special_chars()
    {
        await base.Update_property_inside_association_with_special_chars();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredRelated].modify('$.String', N'{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }')
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredRelated], '$.String' RETURNING nvarchar(max)) = N'{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.String', N'{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }')
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredRelated], '$.String') = N'{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
        }
    }

    public override async Task Update_property_inside_nested()
    {
        await base.Update_property_inside_nested();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [RequiredRelated].modify('$.RequiredNested.String', @p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.RequiredNested.String', @p)
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_property_on_projected_association()
    {
        await base.Update_property_on_projected_association();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [RequiredRelated].modify('$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
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
@complex_type_p='?' (Size = 201)

UPDATE [r]
SET [r].[RequiredRelated] = @complex_type_p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_association_to_parameter()
    {
        await base.Update_nested_association_to_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@complex_type_p='?' (Size = 79)

UPDATE [r]
SET [RequiredRelated].modify('$.RequiredNested', @complex_type_p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@complex_type_p='?' (Size = 79)

UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.RequiredNested', JSON_QUERY(@complex_type_p))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_association_to_another_association()
    {
        await base.Update_association_to_another_association();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated] = [r].[RequiredRelated]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_association_to_another_nested_association()
    {
        await base.Update_nested_association_to_another_nested_association();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredRelated].modify('$.OptionalNested', JSON_QUERY([r].[RequiredRelated], '$.RequiredNested'))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.OptionalNested', JSON_QUERY([r].[RequiredRelated], '$.RequiredNested'))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_association_to_inline()
    {
        await base.Update_association_to_inline();

        AssertExecuteUpdateSql(
            """
@complex_type_p='?' (Size = 222)

UPDATE [r]
SET [r].[RequiredRelated] = @complex_type_p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_inline_with_lambda()
    {
        await base.Update_association_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[RequiredRelated] = '{"Id":0,"Int":70,"Name":"Updated related name","String":"Updated related string","NestedCollection":[],"OptionalNested":null,"RequiredNested":{"Id":0,"Int":80,"Name":"Updated nested name","String":"Updated nested string"}}'
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_association_to_inline_with_lambda()
    {
        await base.Update_nested_association_to_inline_with_lambda();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredRelated].modify('$.RequiredNested', CAST('{"Id":0,"Int":80,"Name":"Updated nested name","String":"Updated nested string"}' AS json))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.RequiredNested', JSON_QUERY('{"Id":0,"Int":80,"Name":"Updated nested name","String":"Updated nested string"}'))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_association_to_null()
    {
        await base.Update_association_to_null();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated] = NULL
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_null_with_lambda()
    {
        await base.Update_association_to_null_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated] = NULL
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_association_to_null_parameter()
    {
        await base.Update_association_to_null_parameter();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalRelated] = NULL
FROM [RootEntity] AS [r]
""");
    }

    #endregion Update association

    #region Update collection

    public override async Task Update_collection_to_parameter()
    {
        await base.Update_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='?' (Size = 411)

UPDATE [r]
SET [r].[RelatedCollection] = @complex_type_p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_collection_to_parameter()
    {
        await base.Update_nested_collection_to_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@complex_type_p='?' (Size = 165)

UPDATE [r]
SET [RequiredRelated].modify('$.NestedCollection', @complex_type_p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@complex_type_p='?' (Size = 165)

UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.NestedCollection', JSON_QUERY(@complex_type_p))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_nested_collection_to_inline_with_lambda()
    {
        await base.Update_nested_collection_to_inline_with_lambda();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredRelated].modify('$.NestedCollection', CAST('[{"Id":0,"Int":80,"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":0,"Int":81,"Name":"Updated nested name2","String":"Updated nested string2"}]' AS json))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.NestedCollection', JSON_QUERY('[{"Id":0,"Int":80,"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":0,"Int":81,"Name":"Updated nested name2","String":"Updated nested string2"}]'))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_nested_collection_to_another_nested_collection()
    {
        await base.Update_nested_collection_to_another_nested_collection();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredRelated].modify('$.NestedCollection', JSON_QUERY([r].[OptionalRelated], '$.NestedCollection'))
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NOT NULL
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.NestedCollection', JSON_QUERY([r].[OptionalRelated], '$.NestedCollection'))
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NOT NULL
""");
        }
    }

    public override async Task Update_collection_referencing_the_original_collection()
    {
        await base.Update_collection_referencing_the_original_collection();

        AssertExecuteUpdateSql();
    }

    #endregion Update collection

    #region Multiple updates

    public override async Task Update_multiple_properties_inside_same_association()
    {
        await base.Update_multiple_properties_inside_same_association();

        // Note that since two properties within the same JSON column are updated, SQL Server 2025 modify
        // is not used (it only supports modifying a single property)
        AssertExecuteUpdateSql(
            """
@p='?' (Size = 4000)
@p0='?' (DbType = Int32)

UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY(JSON_MODIFY([r].[RequiredRelated], '$.String', @p), '$.Int', @p0)
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_multiple_properties_inside_associations_and_on_entity_type()
    {
        await base.Update_multiple_properties_inside_associations_and_on_entity_type();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[Name] = [r].[Name] + N'Modified',
    [RequiredRelated].modify('$.String', JSON_VALUE([r].[OptionalRelated], '$.String' RETURNING nvarchar(max))),
    [OptionalRelated].modify('$.RequiredNested.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NOT NULL
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[Name] = [r].[Name] + N'Modified',
    [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.String', JSON_VALUE([r].[OptionalRelated], '$.String')),
    [r].[OptionalRelated] = JSON_MODIFY([r].[OptionalRelated], '$.RequiredNested.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NOT NULL
""");
        }
    }

    public override async Task Update_multiple_projected_associations_via_anonymous_type()
    {
        await base.Update_multiple_projected_associations_via_anonymous_type();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [RequiredRelated].modify('$.String', JSON_VALUE([r].[OptionalRelated], '$.String' RETURNING nvarchar(max))),
    [OptionalRelated].modify('$.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NOT NULL
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='?' (Size = 4000)

UPDATE [r]
SET [r].[RequiredRelated] = JSON_MODIFY([r].[RequiredRelated], '$.String', JSON_VALUE([r].[OptionalRelated], '$.String')),
    [r].[OptionalRelated] = JSON_MODIFY([r].[OptionalRelated], '$.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NOT NULL
""");
        }
    }

    #endregion Multiple updates

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
