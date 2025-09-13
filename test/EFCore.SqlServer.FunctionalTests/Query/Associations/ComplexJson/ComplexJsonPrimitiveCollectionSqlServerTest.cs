// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonPrimitiveCollectionSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonPrimitiveCollectionRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(JSON_QUERY([r].[RequiredRelated], '$.Ints')) AS [i]) = 3
""");
    }

    public override async Task Index()
    {
        await base.Index();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredRelated], '$.Ints[0]' RETURNING int) = 1
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredRelated], '$.Ints[0]') AS int) = 1
""");
        }
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE 3 IN (
    SELECT [i].[value]
    FROM OPENJSON(JSON_QUERY([r].[RequiredRelated], '$.Ints')) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE 2 IN (
    SELECT [i].[value]
    FROM OPENJSON(JSON_QUERY([r].[RequiredRelated], '$.Ints')) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(JSON_QUERY([r].[RequiredRelated], '$.RequiredNested.Ints')) AS [i]) = 3
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([i0].[value]), 0)
    FROM OPENJSON(JSON_QUERY([r].[RequiredRelated], '$.Ints')) WITH ([value] int '$') AS [i0])
FROM [RootEntity] AS [r]
WHERE (
    SELECT COALESCE(SUM([i].[value]), 0)
    FROM OPENJSON(JSON_QUERY([r].[RequiredRelated], '$.Ints')) WITH ([value] int '$') AS [i]) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
