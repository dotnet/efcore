// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonCollectionSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonCollectionRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count(bool async)
    {
        await base.Count(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RelatedCollection], '$') AS [r0]) = 2
""");
    }

    public override async Task Where(bool async)
    {
        await base.Where(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RelatedCollection], '$') WITH ([Int] int '$.Int') AS [r0]
    WHERE [r0].[Int] <> 8) = 2
""");
    }

    public override async Task OrderBy_ElementAt(bool async)
    {
        await base.OrderBy_ElementAt(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT [r0].[Int]
    FROM OPENJSON([r].[RelatedCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int'
    ) AS [r0]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
""");
    }

    public override async Task Index_constant(bool async)
    {
        // Complex collection indexing currently fails because SubqueryMemberPushdownExpressionVisitor moves the Int member access to before the
        // ElementAt (making a Select()), this interferes with our translation. See #36335.
        await Assert.ThrowsAsync<EqualException>(() => base.Index_constant(async));

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[0]') AS int) = 8
""");
    }

    public override async Task Index_parameter(bool async)
    {
        // Complex collection indexing currently fails because SubqueryMemberPushdownExpressionVisitor moves the Int member access to before the
        // ElementAt (making a Select()), this interferes with our translation. See #36335.
        await Assert.ThrowsAsync<EqualException>(() => base.Index_parameter(async));

        AssertSql(
            """
@i='?' (DbType = Int32)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[' + CAST(@i AS nvarchar(max)) + ']') AS int) = 8
""");
    }

    public override async Task Index_column(bool async)
    {
        // Complex collection indexing currently fails because SubqueryMemberPushdownExpressionVisitor moves the Int member access to before the
        // ElementAt (making a Select()), this interferes with our translation. See #36335.
        await Assert.ThrowsAsync<EqualException>(() => base.Index_column(async));

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[' + CAST([r].[Id] - 1 AS nvarchar(max)) + ']') AS int) = 8
""");
    }

    public override async Task Index_out_of_bounds(bool async)
    {
        await base.Index_out_of_bounds(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[9999]') AS int) = 8
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
