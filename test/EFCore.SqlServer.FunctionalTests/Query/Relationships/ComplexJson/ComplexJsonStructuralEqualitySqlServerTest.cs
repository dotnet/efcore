// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonStructuralEqualitySqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonStructuralEqualityRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    // The SQL Server json type cannot be compared ("The JSON data type cannot be compared or sorted, except when using the
    // IS NULL operator").
    // So we find comparisons that involve the json type, and apply a conversion to string (nvarchar(max)) to both sides.

    public override async Task Two_related()
    {
        await base.Two_related();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST([r].[RequiredRelated] AS nvarchar(max)) = CAST([r].[OptionalRelated] AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated] = [r].[OptionalRelated]
""");
        }
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') AS nvarchar(max)) = CAST(JSON_QUERY([r].[OptionalRelated], '$.RequiredNested') AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') = JSON_QUERY([r].[OptionalRelated], '$.RequiredNested')
""");
        }
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST([r].[RequiredRelated] AS nvarchar(max)) <> CAST([r].[OptionalRelated] AS nvarchar(max)) OR [r].[OptionalRelated] IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated] <> [r].[OptionalRelated] OR [r].[OptionalRelated] IS NULL
""");
        }
    }

    public override async Task Related_with_inline_null()
    {
        await base.Related_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NULL
""");
    }

    public override async Task Related_with_parameter_null()
    {
        await base.Related_with_parameter_null();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST([r].[OptionalRelated] AS nvarchar(max)) = CAST(NULL AS nvarchar(max)) OR [r].[OptionalRelated] IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NULL
""");
        }
    }

    public override async Task Nested_with_inline_null()
    {
        await base.Nested_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.OptionalNested') IS NULL
""");
    }

    public override async Task Nested_with_inline()
    {
        await base.Nested_with_inline();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') AS nvarchar(max)) = CAST('{"Id":1000,"Int":8,"Name":"Root1_RequiredRelated_RequiredNested","String":"foo"}' AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') = '{"Id":1000,"Int":8,"Name":"Root1_RequiredRelated_RequiredNested","String":"foo"}'
""");
        }
    }

    public override async Task Nested_with_parameter()
    {
        await base.Nested_with_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@entity_equality_nested='?' (Size = 80)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') AS nvarchar(max)) = CAST(@entity_equality_nested AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
@entity_equality_nested='?' (Size = 80)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') = @entity_equality_nested
""");
        }
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') AS nvarchar(max)) = CAST(JSON_QUERY([r].[OptionalRelated], '$.NestedCollection') AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') = JSON_QUERY([r].[OptionalRelated], '$.NestedCollection')
""");
        }
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') AS nvarchar(max)) = CAST('[{"Id":1002,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_2","String":"foo"}]' AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') = '[{"Id":1002,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_2","String":"foo"}]'
""");
        }
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@entity_equality_nestedCollection='?' (Size = 171)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') AS nvarchar(max)) = CAST(@entity_equality_nestedCollection AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
@entity_equality_nestedCollection='?' (Size = 171)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') = @entity_equality_nestedCollection
""");
        }
    }

    #region Value types

    public override async Task Nullable_value_type_with_null()
    {
        await base.Nullable_value_type_with_null();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalRelated], [v].[RelatedCollection], [v].[RequiredRelated]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalRelated] IS NULL
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
