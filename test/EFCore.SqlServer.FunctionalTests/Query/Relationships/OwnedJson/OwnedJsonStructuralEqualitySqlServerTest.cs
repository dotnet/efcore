// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public class OwnedJsonStructuralEqualitySqlServerTest(
    OwnedJsonSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedJsonStructuralEqualityRelationalTestBase<OwnedJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related(bool async)
    {
        await base.Two_related(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE 0 = 1
""");
    }

    public override async Task Two_nested(bool async)
    {
        await base.Two_nested(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE 0 = 1
""");
    }

    public override async Task Not_equals(bool async)
    {
        await base.Not_equals(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE 0 = 1
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
WHERE 0 = 1
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
);
    }

    public override async Task Nested_with_parameter(bool async)
    {
        await base.Nested_with_parameter(async);

        AssertSql(
);
    }

    public override async Task Two_nested_collections(bool async)
    {
        await base.Two_nested_collections(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE 0 = 1
""");
    }

    public override async Task Nested_collection_with_inline(bool async)
    {
        await base.Nested_collection_with_inline(async);

        AssertSql(
);
    }

    public override async Task Nested_collection_with_parameter(bool async)
    {
        await base.Nested_collection_with_parameter(async);

        AssertSql(
);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
