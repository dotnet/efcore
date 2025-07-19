// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsCollectionSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsCollectionRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count(bool async)
    {
        await base.Count(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId]) = 2
""");
    }

    public override async Task Where(bool async)
    {
        await base.Where(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId] AND [r0].[Int] <> 8) = 2
""");
    }

    public override async Task OrderBy_ElementAt(bool async)
    {
        await base.OrderBy_ElementAt(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
""");
    }

    public override async Task Index_constant(bool async)
    {
        await base.Index_constant(async);

        AssertSql();
    }

    public override async Task Index_parameter(bool async)
    {
        await base.Index_parameter(async);

        AssertSql();
    }

    public override async Task Index_column(bool async)
    {
        await base.Index_column(async);

        AssertSql();
    }

    public override async Task Index_out_of_bounds(bool async)
    {
        await base.Index_out_of_bounds(async);

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
