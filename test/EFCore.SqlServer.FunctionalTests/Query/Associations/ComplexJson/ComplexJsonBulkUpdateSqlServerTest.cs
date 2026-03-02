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
@deletableEntity_Name='Root3_With_different_values' (Size = 4000)

DELETE FROM [r]
FROM [RootEntity] AS [r]
WHERE [r].[Name] = @deletableEntity_Name
""");
    }

    public override async Task Delete_required_associate()
    {
        await base.Delete_required_associate();

        AssertSql();
    }

    public override async Task Delete_optional_associate()
    {
        await base.Delete_optional_associate();

        AssertSql();
    }

    #endregion Delete

    #region Update properties

    public override async Task Update_property_inside_associate()
    {
        await base.Update_property_inside_associate();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [RequiredAssociate].modify('$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_property_inside_associate_with_special_chars()
    {
        await base.Update_property_inside_associate_with_special_chars();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredAssociate].modify('$.String', N'{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }')
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.String' RETURNING nvarchar(max)) = N'{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.String', N'{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }')
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.String') = N'{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
        }
    }

    public override async Task Update_property_inside_nested_associate()
    {
        await base.Update_property_inside_nested_associate();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [RequiredAssociate].modify('$.RequiredNestedAssociate.String', @p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.RequiredNestedAssociate.String', @p)
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_property_on_projected_associate()
    {
        await base.Update_property_on_projected_associate();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [RequiredAssociate].modify('$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.String', @p)
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_property_on_projected_associate_with_OrderBy_Skip()
    {
        await base.Update_property_on_projected_associate_with_OrderBy_Skip();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_associate_with_null_required_property()
    {
        await base.Update_associate_with_null_required_property();

        AssertExecuteUpdateSql();
    }

    #endregion Update properties

    #region Update association

    public override async Task Update_associate_to_parameter()
    {
        await base.Update_associate_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='{"Id":1000,"Int":80,"Ints":[1,2,3],"Name":"Updated associate name","String":"Updated nested string","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,3],"Name":"Updated nested name","String":"Updated nested string"}}' (Size = 277)

UPDATE [r]
SET [r].[RequiredAssociate] = @complex_type_p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_associate_to_parameter()
    {
        await base.Update_nested_associate_to_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@complex_type_p='{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}' (Size = 97)

UPDATE [r]
SET [RequiredAssociate].modify('$.RequiredNestedAssociate', @complex_type_p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@complex_type_p='{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}' (Size = 97)

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.RequiredNestedAssociate', JSON_QUERY(@complex_type_p))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_associate_to_another_associate()
    {
        await base.Update_associate_to_another_associate();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalAssociate] = [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_nested_associate_to_another_nested_associate()
    {
        await base.Update_nested_associate_to_another_nested_associate();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredAssociate].modify('$.OptionalNestedAssociate', JSON_QUERY([r].[RequiredAssociate], '$.RequiredNestedAssociate'))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.OptionalNestedAssociate', JSON_QUERY([r].[RequiredAssociate], '$.RequiredNestedAssociate'))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_associate_to_inline()
    {
        await base.Update_associate_to_inline();

        AssertExecuteUpdateSql(
            """
@complex_type_p='{"Id":1000,"Int":70,"Ints":[1,2,4],"Name":"Updated associate name","String":"Updated associate string","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}}' (Size = 280)

UPDATE [r]
SET [r].[RequiredAssociate] = @complex_type_p
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_associate_to_inline_with_lambda()
    {
        await base.Update_associate_to_inline_with_lambda();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = CAST('{"Id":1000,"Int":70,"Ints":[1,2,4],"Name":"Updated associate name","String":"Updated associate string","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}}' AS json)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = '{"Id":1000,"Int":70,"Ints":[1,2,4],"Name":"Updated associate name","String":"Updated associate string","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}}'
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_nested_associate_to_inline_with_lambda()
    {
        await base.Update_nested_associate_to_inline_with_lambda();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredAssociate].modify('$.RequiredNestedAssociate', CAST('{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}' AS json))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.RequiredNestedAssociate', JSON_QUERY('{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}'))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_associate_to_null()
    {
        await base.Update_associate_to_null();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalAssociate] = NULL
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_associate_to_null_with_lambda()
    {
        await base.Update_associate_to_null_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalAssociate] = NULL
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_associate_to_null_parameter()
    {
        await base.Update_associate_to_null_parameter();

        AssertExecuteUpdateSql(
            """
UPDATE [r]
SET [r].[OptionalAssociate] = NULL
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_required_nested_associate_to_null()
    {
        await base.Update_required_nested_associate_to_null();

        AssertExecuteUpdateSql();
    }

    #endregion Update association

    #region Update collection

    public override async Task Update_collection_to_parameter()
    {
        await base.Update_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated associate name1","String":"Updated associate string1","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"}},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated associate name2","String":"Updated associate string2","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}}]' (Size = 571)

UPDATE [r]
SET [r].[AssociateCollection] = @complex_type_p
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
@complex_type_p='[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}]' (Size = 201)

UPDATE [r]
SET [RequiredAssociate].modify('$.NestedCollection', @complex_type_p)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@complex_type_p='[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}]' (Size = 201)

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.NestedCollection', JSON_QUERY(@complex_type_p))
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
SET [RequiredAssociate].modify('$.NestedCollection', CAST('[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}]' AS json))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.NestedCollection', JSON_QUERY('[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}]'))
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
SET [RequiredAssociate].modify('$.NestedCollection', JSON_QUERY([r].[OptionalAssociate], '$.NestedCollection'))
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate] IS NOT NULL
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.NestedCollection', JSON_QUERY([r].[OptionalAssociate], '$.NestedCollection'))
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate] IS NOT NULL
""");
        }
    }

    public override async Task Update_collection_referencing_the_original_collection()
    {
        await base.Update_collection_referencing_the_original_collection();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_inside_structural_collection()
    {
        await base.Update_inside_structural_collection();

        AssertExecuteUpdateSql();
    }

    #endregion Update collection

    #region Update primitive collection

    public override async Task Update_primitive_collection_to_constant()
    {
        await base.Update_primitive_collection_to_constant();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredAssociate].modify('$.Ints', CAST('[1,2,4]' AS json))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.Ints', JSON_QUERY(N'[1,2,4]'))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_primitive_collection_to_parameter()
    {
        await base.Update_primitive_collection_to_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@ints='[1,2,4]' (Size = 7)

UPDATE [r]
SET [RequiredAssociate].modify('$.Ints', @ints)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@ints='[1,2,4]' (Size = 4000)

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.Ints', JSON_QUERY(@ints))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_primitive_collection_to_another_collection()
    {
        await base.Update_primitive_collection_to_another_collection();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [RequiredAssociate].modify('$.OptionalNestedAssociate.Ints', JSON_QUERY([r].[RequiredAssociate], '$.RequiredNestedAssociate.Ints'))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.OptionalNestedAssociate.Ints', JSON_QUERY([r].[RequiredAssociate], '$.RequiredNestedAssociate.Ints'))
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Update_inside_primitive_collection()
    {
        await base.Update_inside_primitive_collection();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='99'

UPDATE [r]
SET [RequiredAssociate].modify('$.Ints[1]', @p)
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.Ints')) AS [i]) >= 2
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='99'

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.Ints[1]', @p)
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.Ints')) AS [i]) >= 2
""");
        }
    }

    #endregion Update primitive collection

    #region Multiple updates

    public override async Task Update_multiple_properties_inside_same_associate()
    {
        await base.Update_multiple_properties_inside_same_associate();

        // Note that since two properties within the same JSON column are updated, SQL Server 2025 modify
        // is not used (it only supports modifying a single property)
        AssertExecuteUpdateSql(
            """
@p='foo_updated' (Size = 4000)
@p1='20'

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY(JSON_MODIFY([r].[RequiredAssociate], '$.String', @p), '$.Int', @p1)
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Update_multiple_properties_inside_associates_and_on_entity_type()
    {
        await base.Update_multiple_properties_inside_associates_and_on_entity_type();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [r].[Name] = [r].[Name] + N'Modified',
    [RequiredAssociate].modify('$.String', JSON_VALUE([r].[OptionalAssociate], '$.String' RETURNING nvarchar(max))),
    [OptionalAssociate].modify('$.RequiredNestedAssociate.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate] IS NOT NULL
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [r].[Name] = [r].[Name] + N'Modified',
    [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.String', JSON_VALUE([r].[OptionalAssociate], '$.String')),
    [r].[OptionalAssociate] = JSON_MODIFY([r].[OptionalAssociate], '$.RequiredNestedAssociate.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate] IS NOT NULL
""");
        }
    }

    public override async Task Update_multiple_projected_associates_via_anonymous_type()
    {
        await base.Update_multiple_projected_associates_via_anonymous_type();

        if (Fixture.UsingJsonType)
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [RequiredAssociate].modify('$.String', JSON_VALUE([r].[OptionalAssociate], '$.String' RETURNING nvarchar(max))),
    [OptionalAssociate].modify('$.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate] IS NOT NULL
""");
        }
        else
        {
            AssertExecuteUpdateSql(
                """
@p='foo_updated' (Size = 4000)

UPDATE [r]
SET [r].[RequiredAssociate] = JSON_MODIFY([r].[RequiredAssociate], '$.String', JSON_VALUE([r].[OptionalAssociate], '$.String')),
    [r].[OptionalAssociate] = JSON_MODIFY([r].[OptionalAssociate], '$.String', @p)
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate] IS NOT NULL
""");
        }
    }

    #endregion Multiple updates

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
