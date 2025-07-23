// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsStructuralEqualitySqlServerTest(
    NavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : NavigationsStructuralEqualityRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related(bool async)
    {
        await base.Two_related(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
WHERE [r0].[Id] = [r1].[Id]
""");
    }

    public override async Task Two_nested(bool async)
    {
        await base.Two_nested(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
WHERE [n].[Id] = [n0].[Id]
""");
    }

    public override async Task Not_equals(bool async)
    {
        await base.Not_equals(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
WHERE [r0].[Id] <> [r1].[Id] OR [r1].[Id] IS NULL
""");
    }

    public override async Task Related_with_inline_null(bool async)
    {
        await base.Related_with_inline_null(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
WHERE [r0].[Id] IS NULL
""");
    }

    public override async Task Related_with_parameter_null(bool async)
    {
        await base.Related_with_parameter_null(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
WHERE [r0].[Id] IS NULL
""");
    }

    public override async Task Nested_with_inline_null(bool async)
    {
        await base.Nested_with_inline_null(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
WHERE [n].[Id] IS NULL
""");
    }

    public override async Task Nested_with_inline(bool async)
    {
        await base.Nested_with_inline(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
WHERE [n].[Id] = 1000
""");
    }

    public override async Task Nested_with_parameter(bool async)
    {
        await base.Nested_with_parameter(async);

        AssertSql(
            """
@entity_equality_nested_Id='1000' (Nullable = true)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
WHERE [n].[Id] = @entity_equality_nested_Id
""");
    }

    public override async Task Two_nested_collections(bool async)
    {
        await base.Two_nested_collections(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
WHERE [r0].[Id] = [r1].[Id]
""");
    }

    public override async Task Nested_collection_with_inline(bool async)
    {
        await base.Nested_collection_with_inline(async);

        AssertSql();
    }

    public override async Task Nested_collection_with_parameter(bool async)
    {
        await base.Nested_collection_with_parameter(async);

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
