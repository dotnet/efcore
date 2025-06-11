// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public class PrecompiledQuerySqlServerTest(
    PrecompiledQuerySqlServerTest.PrecompiledQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledQuerySqlServerTest.PrecompiledQuerySqlServerFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    #region Expression types

    public override async Task BinaryExpression()
    {
        await base.BinaryExpression();

        AssertSql(
            """
@id='3'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] > @id
""");
    }

    public override async Task Conditional_no_evaluatable()
    {
        await base.Conditional_no_evaluatable();

        AssertSql(
            """
SELECT CASE
    WHEN [b].[Id] = 2 THEN N'yes'
    ELSE N'no'
END
FROM [Blogs] AS [b]
""");
    }

    public override async Task Conditional_contains_captured_variable()
    {
        await base.Conditional_contains_captured_variable();

        AssertSql(
            """
@yes='yes' (Size = 4000)

SELECT CASE
    WHEN [b].[Id] = 2 THEN @yes
    ELSE N'no'
END
FROM [Blogs] AS [b]
""");
    }

    public override async Task Invoke_no_evaluatability_is_not_supported()
    {
        await base.Invoke_no_evaluatability_is_not_supported();

        AssertSql();
    }

    public override async Task ListInit_no_evaluatability()
    {
        await base.ListInit_no_evaluatability();

        AssertSql(
            """
SELECT [b].[Id], [b].[Id] + 1
FROM [Blogs] AS [b]
""");
    }

    public override async Task ListInit_with_evaluatable_with_captured_variable()
    {
        await base.ListInit_with_evaluatable_with_captured_variable();

        AssertSql(
            """
SELECT [b].[Id]
FROM [Blogs] AS [b]
""");
    }

    public override async Task ListInit_with_evaluatable_without_captured_variable()
    {
        await base.ListInit_with_evaluatable_without_captured_variable();

        AssertSql(
            """
SELECT [b].[Id]
FROM [Blogs] AS [b]
""");
    }

    public override async Task ListInit_fully_evaluatable()
    {
        await base.ListInit_fully_evaluatable();

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] IN (7, 8)
""");
    }

    public override async Task MethodCallExpression_no_evaluatability()
    {
        await base.MethodCallExpression_no_evaluatability();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Name] IS NOT NULL AND LEFT([b].[Name], LEN([b].[Name])) = [b].[Name]
""");
    }

    public override async Task MethodCallExpression_with_evaluatable_with_captured_variable()
    {
        await base.MethodCallExpression_with_evaluatable_with_captured_variable();

        AssertSql(
            """
@pattern_startswith='foo%' (Size = 4000)

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Name] LIKE @pattern_startswith ESCAPE N'\'
""");
    }

    public override async Task MethodCallExpression_with_evaluatable_without_captured_variable()
    {
        await base.MethodCallExpression_with_evaluatable_without_captured_variable();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Name] LIKE N'foo%'
""");
    }

    public override async Task MethodCallExpression_fully_evaluatable()
    {
        await base.MethodCallExpression_fully_evaluatable();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task New_with_no_arguments()
    {
        await base.New_with_no_arguments();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 0
""");
    }

    public override async Task Where_New_with_captured_variable()
    {
        await base.Where_New_with_captured_variable();

        AssertSql();
    }

    public override async Task Select_New_with_captured_variable()
    {
        await base.Select_New_with_captured_variable();

        AssertSql(
            """
SELECT [b].[Name]
FROM [Blogs] AS [b]
""");
    }

    public override async Task MemberInit_no_evaluatable()
    {
        await base.MemberInit_no_evaluatable();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
""");
    }

    public override async Task MemberInit_contains_captured_variable()
    {
        await base.MemberInit_contains_captured_variable();

        AssertSql(
            """
@id='8'

SELECT @id AS [Id], [b].[Name]
FROM [Blogs] AS [b]
""");
    }

    public override async Task MemberInit_evaluatable_as_constant()
    {
        await base.MemberInit_evaluatable_as_constant();

        AssertSql(
            """
SELECT 1 AS [Id], N'foo' AS [Name]
FROM [Blogs] AS [b]
""");
    }

    public override async Task MemberInit_evaluatable_as_parameter()
    {
        await base.MemberInit_evaluatable_as_parameter();

        AssertSql(
            """
SELECT 1
FROM [Blogs] AS [b]
""");
    }

    public override async Task NewArray()
    {
        await base.NewArray();

        AssertSql(
            """
@i='8'

SELECT [b].[Id], [b].[Id] + @i
FROM [Blogs] AS [b]
""");
    }

    public override async Task Unary()
    {
        await base.Unary();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE CAST([b].[Id] AS smallint) = CAST(8 AS smallint)
""");
    }

    public virtual async Task Collate()
    {
        await Test("""_ = context.Blogs.Where(b => EF.Functions.Collate(b.Name, "German_PhoneBook_CI_AS") == "foo").ToList();""");

        AssertSql();
    }

    #endregion Expression types

    #region Regular operators

    public override async Task OrderBy()
    {
        await base.OrderBy();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Name]
""");
    }

    public override async Task Skip_with_constant()
    {
        await base.Skip_with_constant();

        AssertSql(
            """
@p='1'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Name]
OFFSET @p ROWS
""");
    }

    public override async Task Skip_with_parameter()
    {
        await base.Skip_with_parameter();

        AssertSql(
            """
@p='1'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Name]
OFFSET @p ROWS
""");
    }

    public override async Task Take_with_constant()
    {
        await base.Take_with_constant();

        AssertSql(
            """
@p='1'

SELECT TOP(@p) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Name]
""");
    }

    public override async Task Take_with_parameter()
    {
        await base.Take_with_parameter();

        AssertSql(
            """
@p='1'

SELECT TOP(@p) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Name]
""");
    }

    public override async Task Select_changes_type()
    {
        await base.Select_changes_type();

        AssertSql(
            """
SELECT [b].[Name]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Select_anonymous_object()
    {
        await base.Select_anonymous_object();

        AssertSql(
            """
SELECT COALESCE([b].[Name], N'') + N'Foo' AS [Foo]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Include_single()
    {
        await base.Include_single();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json], [p].[Id], [p].[BlogId], [p].[Title]
FROM [Blogs] AS [b]
LEFT JOIN [Posts] AS [p] ON [b].[Id] = [p].[BlogId]
WHERE [b].[Id] > 8
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_split()
    {
        await base.Include_split();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [p].[Id], [p].[BlogId], [p].[Title], [b].[Id]
FROM [Blogs] AS [b]
INNER JOIN [Posts] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Final_GroupBy()
    {
        await base.Final_GroupBy();

        AssertSql(
            """
SELECT [b].[Name], [b].[Id], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Name]
""");
    }

    #endregion Regular operators

    #region Terminating operators

    public override async Task Terminating_AsEnumerable()
    {
        await base.Terminating_AsEnumerable();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_AsAsyncEnumerable_on_DbSet()
    {
        await base.Terminating_AsAsyncEnumerable_on_DbSet();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_AsAsyncEnumerable_on_IQueryable()
    {
        await base.Terminating_AsAsyncEnumerable_on_IQueryable();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""");
    }

    public override async Task Foreach_sync_over_operator()
    {
        await base.Foreach_sync_over_operator();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""");
    }

    public override async Task Terminating_ToArray()
    {
        await base.Terminating_ToArray();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToArrayAsync()
    {
        await base.Terminating_ToArrayAsync();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToDictionary()
    {
        await base.Terminating_ToDictionary();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToDictionaryAsync()
    {
        await base.Terminating_ToDictionaryAsync();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task ToDictionary_over_anonymous_type()
    {
        await base.ToDictionary_over_anonymous_type();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
""");
    }

    public override async Task ToDictionaryAsync_over_anonymous_type()
    {
        await base.ToDictionaryAsync_over_anonymous_type();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToHashSet()
    {
        await base.Terminating_ToHashSet();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToHashSetAsync()
    {
        await base.Terminating_ToHashSetAsync();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToLookup()
    {
        await base.Terminating_ToLookup();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToList()
    {
        await base.Terminating_ToList();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ToListAsync()
    {
        await base.Terminating_ToListAsync();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Foreach_sync_over_DbSet_property_is_not_supported()
    {
        await base.Foreach_sync_over_DbSet_property_is_not_supported();

        AssertSql();
    }

    public override async Task Foreach_async_is_not_supported()
    {
        await base.Foreach_async_is_not_supported();

        AssertSql();
    }

    #endregion Terminating operators

    #region Reducing terminating operators

    public override async Task Terminating_All()
    {
        await base.Terminating_All();

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] <= 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] <= 8) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Terminating_AllAsync()
    {
        await base.Terminating_AllAsync();

        AssertSql(
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] <= 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] <= 8) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Terminating_Any()
    {
        await base.Terminating_Any();

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] > 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] < 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] > 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] < 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Terminating_AnyAsync()
    {
        await base.Terminating_AnyAsync();

        AssertSql(
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] > 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] < 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] > 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Blogs] AS [b]
        WHERE [b].[Id] < 7) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Terminating_Average()
    {
        await base.Terminating_Average();

        AssertSql(
            """
SELECT AVG(CAST([b].[Id] AS float))
FROM [Blogs] AS [b]
""",
            //
            """
SELECT AVG(CAST([b].[Id] AS float))
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_AverageAsync()
    {
        await base.Terminating_AverageAsync();

        AssertSql(
            """
SELECT AVG(CAST([b].[Id] AS float))
FROM [Blogs] AS [b]
""",
            //
            """
SELECT AVG(CAST([b].[Id] AS float))
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_Contains()
    {
        await base.Terminating_Contains();

        AssertSql(
            """
@p='8'

SELECT CASE
    WHEN @p IN (
        SELECT [b].[Id]
        FROM [Blogs] AS [b]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
@p='7'

SELECT CASE
    WHEN @p IN (
        SELECT [b].[Id]
        FROM [Blogs] AS [b]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Terminating_ContainsAsync()
    {
        await base.Terminating_ContainsAsync();

        AssertSql(
            """
@p='8'

SELECT CASE
    WHEN @p IN (
        SELECT [b].[Id]
        FROM [Blogs] AS [b]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""",
            //
            """
@p='7'

SELECT CASE
    WHEN @p IN (
        SELECT [b].[Id]
        FROM [Blogs] AS [b]
    ) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Terminating_Count()
    {
        await base.Terminating_Count();

        AssertSql(
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""");
    }

    public override async Task Terminating_CountAsync()
    {
        await base.Terminating_CountAsync();

        AssertSql(
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""");
    }

    public override async Task Terminating_ElementAt()
    {
        await base.Terminating_ElementAt();

        AssertSql(
            """
@p='1'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""",
            //
            """
@p='3'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""");
    }

    public override async Task Terminating_ElementAtAsync()
    {
        await base.Terminating_ElementAtAsync();

        AssertSql(
            """
@p='1'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""",
            //
            """
@p='3'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""");
    }

    public override async Task Terminating_ElementAtOrDefault()
    {
        await base.Terminating_ElementAtOrDefault();

        AssertSql(
            """
@p='1'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""",
            //
            """
@p='3'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""");
    }

    public override async Task Terminating_ElementAtOrDefaultAsync()
    {
        await base.Terminating_ElementAtOrDefaultAsync();

        AssertSql(
            """
@p='1'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""",
            //
            """
@p='3'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
OFFSET @p ROWS FETCH NEXT 1 ROWS ONLY
""");
    }

    public override async Task Terminating_First()
    {
        await base.Terminating_First();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_FirstAsync()
    {
        await base.Terminating_FirstAsync();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_FirstOrDefault()
    {
        await base.Terminating_FirstOrDefault();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_FirstOrDefaultAsync()
    {
        await base.Terminating_FirstOrDefaultAsync();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_GetEnumerator()
    {
        await base.Terminating_GetEnumerator();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""");
    }

    public override async Task Terminating_Last()
    {
        await base.Terminating_Last();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""");
    }

    public override async Task Terminating_LastAsync()
    {
        await base.Terminating_LastAsync();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""");
    }

    public override async Task Terminating_LastOrDefault()
    {
        await base.Terminating_LastOrDefault();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""");
    }

    public override async Task Terminating_LastOrDefaultAsync()
    {
        await base.Terminating_LastOrDefaultAsync();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
ORDER BY [b].[Id] DESC
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
ORDER BY [b].[Id] DESC
""");
    }

    public override async Task Terminating_LongCount()
    {
        await base.Terminating_LongCount();

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Blogs] AS [b]
""",
            //
            """
SELECT COUNT_BIG(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""");
    }

    public override async Task Terminating_LongCountAsync()
    {
        await base.Terminating_LongCountAsync();

        AssertSql(
            """
SELECT COUNT_BIG(*)
FROM [Blogs] AS [b]
""",
            //
            """
SELECT COUNT_BIG(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""");
    }

    public override async Task Terminating_Max()
    {
        await base.Terminating_Max();

        AssertSql(
            """
SELECT MAX([b].[Id])
FROM [Blogs] AS [b]
""",
            //
            """
SELECT MAX([b].[Id])
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_MaxAsync()
    {
        await base.Terminating_MaxAsync();

        AssertSql(
            """
SELECT MAX([b].[Id])
FROM [Blogs] AS [b]
""",
            //
            """
SELECT MAX([b].[Id])
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_Min()
    {
        await base.Terminating_Min();

        AssertSql(
            """
SELECT MIN([b].[Id])
FROM [Blogs] AS [b]
""",
            //
            """
SELECT MIN([b].[Id])
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_MinAsync()
    {
        await base.Terminating_MinAsync();

        AssertSql(
            """
SELECT MIN([b].[Id])
FROM [Blogs] AS [b]
""",
            //
            """
SELECT MIN([b].[Id])
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_Single()
    {
        await base.Terminating_Single();

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_SingleAsync()
    {
        await base.Terminating_SingleAsync();

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_SingleOrDefault()
    {
        await base.Terminating_SingleOrDefault();

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_SingleOrDefaultAsync()
    {
        await base.Terminating_SingleOrDefaultAsync();

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    public override async Task Terminating_Sum()
    {
        await base.Terminating_Sum();

        AssertSql(
            """
SELECT COALESCE(SUM([b].[Id]), 0)
FROM [Blogs] AS [b]
""",
            //
            """
SELECT COALESCE(SUM([b].[Id]), 0)
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_SumAsync()
    {
        await base.Terminating_SumAsync();

        AssertSql(
            """
SELECT COALESCE(SUM([b].[Id]), 0)
FROM [Blogs] AS [b]
""",
            //
            """
SELECT COALESCE(SUM([b].[Id]), 0)
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ExecuteDelete()
    {
        await base.Terminating_ExecuteDelete();

        AssertSql(
            """
DELETE FROM [b]
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ExecuteDeleteAsync()
    {
        await base.Terminating_ExecuteDeleteAsync();

        AssertSql(
            """
DELETE FROM [b]
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
""");
    }

    public override async Task Terminating_ExecuteUpdate_with_lambda()
    {
        await base.Terminating_ExecuteUpdate_with_lambda();

        AssertSql(
            """
@suffix='Suffix' (Size = 4000)

UPDATE [b]
SET [b].[Name] = COALESCE([b].[Name], N'') + @suffix
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] = 9 AND [b].[Name] = N'Blog2Suffix'
""");
    }

    public override async Task Terminating_ExecuteUpdate_without_lambda()
    {
        await base.Terminating_ExecuteUpdate_without_lambda();

        AssertSql(
            """
@newValue='NewValue' (Size = 4000)

UPDATE [b]
SET [b].[Name] = @newValue
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] = 9 AND [b].[Name] = N'NewValue'
""");
    }

    public override async Task Terminating_ExecuteUpdateAsync_with_lambda()
    {
        await base.Terminating_ExecuteUpdateAsync_with_lambda();

        AssertSql(
            """
@suffix='Suffix' (Size = 4000)

UPDATE [b]
SET [b].[Name] = COALESCE([b].[Name], N'') + @suffix
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] = 9 AND [b].[Name] = N'Blog2Suffix'
""");
    }

    public override async Task Terminating_ExecuteUpdateAsync_without_lambda()
    {
        await base.Terminating_ExecuteUpdateAsync_without_lambda();

        AssertSql(
            """
@newValue='NewValue' (Size = 4000)

UPDATE [b]
SET [b].[Name] = @newValue
FROM [Blogs] AS [b]
WHERE [b].[Id] > 8
""",
            //
            """
SELECT COUNT(*)
FROM [Blogs] AS [b]
WHERE [b].[Id] = 9 AND [b].[Name] = N'NewValue'
""");
    }

    public override async Task Terminating_with_cancellation_token()
    {
        await base.Terminating_with_cancellation_token();

        AssertSql(
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 8
""",
            //
            """
SELECT TOP(1) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = 7
""");
    }

    #endregion Reducing terminating operators

    #region SQL expression quotability

    public override async Task Union()
    {
        await base.Union();

        AssertSql(
            """
SELECT [u].[Id], [u].[BlogId], [u].[Title]
FROM (
    SELECT [p].[Id], [p].[BlogId], [p].[Title]
    FROM [Posts] AS [p]
    WHERE [p].[Id] > 11
    UNION
    SELECT [p0].[Id], [p0].[BlogId], [p0].[Title]
    FROM [Posts] AS [p0]
    WHERE [p0].[Id] < 21
) AS [u]
ORDER BY [u].[Id]
""");
    }

    public override async Task UnionOnEntitiesWithJson()
    {
        await base.UnionOnEntitiesWithJson();

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    UNION
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] < 10
) AS [u]
ORDER BY [u].[Id]
""");
    }

    public override async Task Concat()
    {
        await base.Concat();

        AssertSql(
            """
SELECT [u].[Id], [u].[BlogId], [u].[Title]
FROM (
    SELECT [p].[Id], [p].[BlogId], [p].[Title]
    FROM [Posts] AS [p]
    WHERE [p].[Id] > 11
    UNION ALL
    SELECT [p0].[Id], [p0].[BlogId], [p0].[Title]
    FROM [Posts] AS [p0]
    WHERE [p0].[Id] < 21
) AS [u]
ORDER BY [u].[Id]
""");
    }

    public override async Task ConcatOnEntitiesWithJson()
    {
        await base.ConcatOnEntitiesWithJson();

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    UNION ALL
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] < 10
) AS [u]
ORDER BY [u].[Id]
""");
    }

    public override async Task Intersect()
    {
        await base.Intersect();

        AssertSql(
            """
SELECT [i].[Id], [i].[BlogId], [i].[Title]
FROM (
    SELECT [p].[Id], [p].[BlogId], [p].[Title]
    FROM [Posts] AS [p]
    WHERE [p].[Id] > 11
    INTERSECT
    SELECT [p0].[Id], [p0].[BlogId], [p0].[Title]
    FROM [Posts] AS [p0]
    WHERE [p0].[Id] < 22
) AS [i]
ORDER BY [i].[Id]
""");
    }

    public override async Task IntersectOnEntitiesWithJson()
    {
        await base.IntersectOnEntitiesWithJson();

        AssertSql(
            """
SELECT [i].[Id], [i].[Name], [i].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    INTERSECT
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] > 8
) AS [i]
ORDER BY [i].[Id]
""");
    }

    public override async Task Except()
    {
        await base.Except();

        AssertSql(
            """
SELECT [e].[Id], [e].[BlogId], [e].[Title]
FROM (
    SELECT [p].[Id], [p].[BlogId], [p].[Title]
    FROM [Posts] AS [p]
    WHERE [p].[Id] > 11
    EXCEPT
    SELECT [p0].[Id], [p0].[BlogId], [p0].[Title]
    FROM [Posts] AS [p0]
    WHERE [p0].[Id] > 21
) AS [e]
ORDER BY [e].[Id]
""");
    }

    public override async Task ExceptOnEntitiesWithJson()
    {
        await base.ExceptOnEntitiesWithJson();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    EXCEPT
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] > 8
) AS [e]
ORDER BY [e].[Id]
""");
    }

    public override async Task ValuesExpression()
    {
        await base.ValuesExpression();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(7 AS int)), ([b].[Id])) AS [v]([Value])
    WHERE [v].[Value] > 8) = 2
""");
    }

    public override async Task Contains_with_parameterized_collection()
    {
        await base.Contains_with_parameterized_collection();

        AssertSql(
            """
@ids='[1,2,3]' (Size = 4000)

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task FromSqlRaw()
    {
        await base.FromSqlRaw();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [m].[Json]
FROM (
    SELECT * FROM "Blogs" WHERE "Id" > 8
) AS [m]
ORDER BY [m].[Id]
""");
    }

    public override async Task FromSql_with_FormattableString_parameters()
    {
        await base.FromSql_with_FormattableString_parameters();

        AssertSql(
            """
p0='8'
p1='9'

SELECT [m].[Id], [m].[Name], [m].[Json]
FROM (
    SELECT * FROM "Blogs" WHERE "Id" > @p0 AND "Id" < @p1
) AS [m]
ORDER BY [m].[Id]
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public virtual async Task SqlServerAggregateFunctionExpression()
    {
        await Test(
            """
_ = context.Blogs
    .GroupBy(b => b.Id)
    .Select(g => string.Join(", ", g.OrderBy(b => b.Name).Select(b => b.Name)))
    .ToList();
""");

        AssertSql(
            """
SELECT COALESCE(STRING_AGG(COALESCE([b].[Name], N''), N', ') WITHIN GROUP (ORDER BY [b].[Name]), N'')
FROM [Blogs] AS [b]
GROUP BY [b].[Id]
""");
    }

    // SqlServerOpenJsonExpression is covered by PrecompiledQueryRelationalTestBase.Contains_with_parameterized_collection

//     [ConditionalFact]
//     public virtual Task TableValuedFunctionExpression_toplevel()
//         => Test(
//             "_ = context.GetBlogsWithAtLeast(9).ToList();",
//             modelSourceCode: providerOptions => $$"""
// public class BlogContext : DbContext
// {
//     public DbSet<Blog> Blogs { get; set; }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder
//             {{providerOptions}}
//             .ReplaceService<IQueryCompiler, Microsoft.EntityFrameworkCore.Query.NonCompilingQueryCompiler>();
//
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder.HasDbFunction(typeof(BlogContext).GetMethod(nameof(GetBlogsWithAtLeast)));
//     }
//
//     public IQueryable<Blog> GetBlogsWithAtLeast(int minBlogId) => FromExpression(() => GetBlogsWithAtLeast(minBlogId));
// }
//
// public class Blog
// {
//     [DatabaseGenerated(DatabaseGeneratedOption.None)]
//     public int Id { get; set; }
//     public string StringProperty { get; set; }
// }
// """,
//             setupSql: """
// CREATE FUNCTION dbo.GetBlogsWithAtLeast(@minBlogId int)
// RETURNS TABLE AS RETURN
// (
//     SELECT [b].[Id], [b].[Name] FROM [Blogs] AS [b] WHERE [b].[Id] >= @minBlogId
// )
// """,
//             cleanupSql: "DROP FUNCTION dbo.GetBlogsWithAtLeast;");
//
//     [ConditionalFact]
//     public virtual Task TableValuedFunctionExpression_non_toplevel()
//         => Test(
//             "_ = context.Blogs.Where(b => context.GetPosts(b.Id).Count() == 2).ToList();",
//             modelSourceCode: providerOptions => $$"""
// public class BlogContext : DbContext
// {
//     public DbSet<Blog> Blogs { get; set; }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder
//             {{providerOptions}}
//             .ReplaceService<IQueryCompiler, Microsoft.EntityFrameworkCore.Query.NonCompilingQueryCompiler>();
//
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder.HasDbFunction(typeof(BlogContext).GetMethod(nameof(GetPosts)));
//     }
//
//     public IQueryable<Post> GetPosts(int blogId) => FromExpression(() => GetPosts(blogId));
// }
//
// public class Blog
// {
//     public int Id { get; set; }
//     public string StringProperty { get; set; }
//     public List<Post> Post { get; set; }
// }
//
// public class Post
// {
//     public int Id { get; set; }
//     public string Title { get; set; }
//
//     public Blog Blog { get; set; }
// }
// """,
//             setupSql: """
// CREATE FUNCTION dbo.GetPosts(@blogId int)
// RETURNS TABLE AS RETURN
// (
//     SELECT [p].[Id], [p].[Title], [p].[BlogId] FROM [Posts] AS [p] WHERE [p].[BlogId] = @blogId
// )
// """,
//             cleanupSql: "DROP FUNCTION dbo.GetPosts;");

    #endregion SQL expression quotability

    #region Different query roots

    public override async Task DbContext_as_local_variable()
    {
        await base.DbContext_as_local_variable();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task DbContext_as_field()
    {
        await base.DbContext_as_field();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task DbContext_as_property()
    {
        await base.DbContext_as_property();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task DbContext_as_captured_variable()
    {
        await base.DbContext_as_captured_variable();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    public override async Task DbContext_as_method_invocation_result()
    {
        await base.DbContext_as_method_invocation_result();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    #endregion Different query roots

    #region Captured variable handling

    public override async Task Two_captured_variables_in_same_lambda()
    {
        await base.Two_captured_variables_in_same_lambda();

        AssertSql(
            """
@yes='yes' (Size = 4000)
@no='no' (Size = 4000)

SELECT CASE
    WHEN [b].[Id] = 3 THEN @yes
    ELSE @no
END
FROM [Blogs] AS [b]
""");
    }

    public override async Task Two_captured_variables_in_different_lambdas()
    {
        await base.Two_captured_variables_in_different_lambdas();

        AssertSql(
            """
@starts_startswith='Blog%' (Size = 4000)
@ends_endswith='%2' (Size = 4000)

SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Name] LIKE @starts_startswith ESCAPE N'\' AND [b].[Name] LIKE @ends_endswith ESCAPE N'\'
""");
    }

    public override async Task Same_captured_variable_twice_in_same_lambda()
    {
        await base.Same_captured_variable_twice_in_same_lambda();

        AssertSql(
            """
@foo_startswith='X%' (Size = 4000)
@foo_endswith='%X' (Size = 4000)

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Name] LIKE @foo_startswith ESCAPE N'\' AND [b].[Name] LIKE @foo_endswith ESCAPE N'\'
""");
    }

    public override async Task Same_captured_variable_twice_in_different_lambdas()
    {
        await base.Same_captured_variable_twice_in_different_lambdas();

        AssertSql(
            """
@foo_startswith='X%' (Size = 4000)
@foo_endswith='%X' (Size = 4000)

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Name] LIKE @foo_startswith ESCAPE N'\' AND [b].[Name] LIKE @foo_endswith ESCAPE N'\'
""");
    }

    public override async Task Multiple_queries_with_captured_variables()
    {
        await base.Multiple_queries_with_captured_variables();

        AssertSql(
            """
@id1='8'
@id2='9'

SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = @id1 OR [b].[Id] = @id2
""",
            //
            """
@id1='8'

SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Id] = @id1
""");
    }

    #endregion Captured variable handling

    #region Negative cases

    public override async Task Dynamic_query_does_not_get_precompiled()
    {
        await base.Dynamic_query_does_not_get_precompiled();

        AssertSql();
    }

    public override async Task ToList_over_objects_does_not_get_precompiled()
    {
        await base.ToList_over_objects_does_not_get_precompiled();

        AssertSql();
    }

    public override async Task Query_compilation_failure()
    {
        await base.Query_compilation_failure();

        AssertSql();
    }

    public override async Task EF_Constant_is_not_supported()
    {
        await base.EF_Constant_is_not_supported();

        AssertSql();
    }

    public override async Task NotParameterizedAttribute_with_constant()
    {
        await base.NotParameterizedAttribute_with_constant();

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
WHERE [b].[Name] = N'Blog2'
""");
    }

    public override async Task NotParameterizedAttribute_is_not_supported_with_non_constant_argument()
    {
        await base.NotParameterizedAttribute_is_not_supported_with_non_constant_argument();

        AssertSql();
    }

    public override async Task Query_syntax_is_not_supported()
    {
        await base.Query_syntax_is_not_supported();

        AssertSql();
    }

    #endregion Negative cases

    public override async Task Unsafe_accessor_gets_generated_once_for_multiple_queries()
    {
        await base.Unsafe_accessor_gets_generated_once_for_multiple_queries();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""",
            //
            """
SELECT [b].[Id], [b].[Name], [b].[Json]
FROM [Blogs] AS [b]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public class PrecompiledQuerySqlServerFixture : PrecompiledQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            builder = base.AddOptions(builder);

            // TODO: Figure out if there's a nice way to continue using the retrying strategy
            var sqlServerOptionsBuilder = new SqlServerDbContextOptionsBuilder(builder);
            sqlServerOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
            return builder;
        }

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
            => SqlServerPrecompiledQueryTestHelpers.Instance;
    }
}
