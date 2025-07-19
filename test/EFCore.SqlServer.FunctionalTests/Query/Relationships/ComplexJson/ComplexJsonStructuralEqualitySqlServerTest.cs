// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonStructuralEqualitySqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonStructuralEqualityRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related(bool async)
    {
        await base.Two_related(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated] = [r].[OptionalRelated]
""");
    }

    public override async Task Two_nested(bool async)
    {
        await base.Two_nested(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') = JSON_QUERY([r].[OptionalRelated], '$.RequiredNested')
""");
    }

    public override async Task Not_equals(bool async)
    {
        await base.Not_equals(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated] <> [r].[OptionalRelated] OR [r].[OptionalRelated] IS NULL
""");
    }

    public override async Task Related_with_inline_null(bool async)
    {
        await base.Related_with_inline_null(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NULL
""");
    }

    public override async Task Related_with_parameter_null(bool async)
    {
        await base.Related_with_parameter_null(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated] IS NULL
""");
    }

    public override async Task Nested_with_inline_null(bool async)
    {
        await base.Nested_with_inline_null(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.OptionalNested') IS NULL
""");
    }

    public override async Task Nested_with_inline(bool async)
    {
        await base.Nested_with_inline(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') = '{"Id":1000,"Int":8,"Name":"Root1_RequiredRelated_RequiredNested","String":"foo"}'
""");
    }

    public override async Task Nested_with_parameter(bool async)
    {
        await base.Nested_with_parameter(async);

        AssertSql(
            """
@entity_equality_nested='?' (Size = 80)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.RequiredNested') = @entity_equality_nested
""");
    }

    public override async Task Two_nested_collections(bool async)
    {
        await base.Two_nested_collections(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') = JSON_QUERY([r].[OptionalRelated], '$.NestedCollection')
""");
    }

    public override async Task Nested_collection_with_inline(bool async)
    {
        await base.Nested_collection_with_inline(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') = '[{"Id":1002,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_2","String":"foo"}]'
""");
    }

    public override async Task Nested_collection_with_parameter(bool async)
    {
        await base.Nested_collection_with_parameter(async);

        AssertSql(
            """
@entity_equality_nestedCollection='?' (Size = 171)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_QUERY([r].[RequiredRelated], '$.NestedCollection') = @entity_equality_nestedCollection
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
