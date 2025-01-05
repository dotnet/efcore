// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindDbFunctionsQuerySqlServerTest : NorthwindDbFunctionsQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindDbFunctionsQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Like_literal(bool async)
    {
        await base.Like_literal(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%M%'
""");
    }

    public override async Task Like_identity(bool async)
    {
        await base.Like_identity(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName]
""");
    }

    public override async Task Like_literal_with_escape(bool async)
    {
        await base.Like_literal_with_escape(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'!%' ESCAPE N'!'
""");
    }

    public override async Task Like_all_literals(bool async)
    {
        await base.Like_all_literals(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE N'FOO' LIKE N'%O%'
""");
    }

    public override async Task Like_all_literals_with_escape(bool async)
    {
        await base.Like_all_literals_with_escape(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE N'%' LIKE N'!%' ESCAPE N'!'
""");
    }

    public override async Task Collate_case_insensitive(bool async)
    {
        await base.Collate_case_insensitive(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] COLLATE Latin1_General_CI_AI = N'maria anders'
""");
    }

    public override async Task Collate_case_sensitive(bool async)
    {
        await base.Collate_case_sensitive(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] COLLATE Latin1_General_CS_AS = N'maria anders'
""");
    }

    public override async Task Collate_case_sensitive_constant(bool async)
    {
        await base.Collate_case_sensitive_constant(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] = N'maria anders' COLLATE Latin1_General_CS_AS
""");
    }

    public override async Task Collate_is_null(bool async)
    {
        await base.Collate_is_null(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL
""");
    }

    public override Task Least(bool async)
        => AssertTranslationFailed(() => base.Least(async));

    public override Task Greatest(bool async)
        => AssertTranslationFailed(() => base.Greatest(async));

    public override Task Least_with_nullable_value_type(bool async)
        => AssertTranslationFailed(() => base.Least_with_nullable_value_type(async));

    public override Task Greatest_with_nullable_value_type(bool async)
        => AssertTranslationFailed(() => base.Greatest_with_nullable_value_type(async));

    public override async Task Least_with_parameter_array_is_not_supported(bool async)
    {
        await base.Least_with_parameter_array_is_not_supported(async);

        AssertSql();
    }

    public override async Task Greatest_with_parameter_array_is_not_supported(bool async)
    {
        await base.Greatest_with_parameter_array_is_not_supported(async);

        AssertSql();
    }

    protected override string CaseInsensitiveCollation
        => "Latin1_General_CI_AI";

    protected override string CaseSensitiveCollation
        => "Latin1_General_CS_AS";

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task FreeText_literal()
    {
        using var context = CreateContext();
        var result = await context.Employees
            .Where(c => EF.Functions.FreeText(c.Title, "Representative"))
            .ToListAsync();

        Assert.Equal(1u, result.First().EmployeeID);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'Representative')
""");
    }

    [ConditionalFact]
    public void FreeText_client_eval_throws()
    {
        Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring"));
        Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring", 1033));
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_multiple_words()
    {
        using var context = CreateContext();
        var result = context.Employees
            .Where(c => EF.Functions.FreeText(c.Title, "Representative Sales"))
            .Count();

        Assert.Equal(9, result);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'Representative Sales')
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_with_language_term()
    {
        using var context = CreateContext();
        var result = context.Employees.SingleOrDefault(c => EF.Functions.FreeText(c.Title, "President", 1033));

        Assert.Equal(2u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'President', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_with_non_literal_language_term()
    {
        var language = 1033;
        using var context = CreateContext();
        var result = context.Employees.SingleOrDefault(c => EF.Functions.FreeText(c.Title, "President", language));

        Assert.Equal(2u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'President', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_with_multiple_words_and_language_term()
    {
        using var context = CreateContext();
        var result = context.Employees
            .Where(c => EF.Functions.FreeText(c.Title, "Representative President", 1033))
            .ToList();

        Assert.Equal(1u, result.First().EmployeeID);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'Representative President', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_multiple_predicates()
    {
        using var context = CreateContext();
        var result = context.Employees
            .Where(
                c => EF.Functions.FreeText(c.City, "London")
                    && EF.Functions.FreeText(c.Title, "Manager", 1033))
            .FirstOrDefault();

        Assert.Equal(5u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(1) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[City], N'London') AND FREETEXT([e].[Title], N'Manager', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_throws_for_no_FullText_index()
    {
        using var context = CreateContext();
        Assert.Throws<SqlException>(
            () => context.Employees.Where(c => EF.Functions.FreeText(c.FirstName, "Fred")).ToArray());
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_through_navigation()
    {
        using var context = CreateContext();
        var result = context.Employees
            .Where(
                c => EF.Functions.FreeText(c.Manager.Title, "President")
                    && EF.Functions.FreeText(c.Title, "Inside")
                    && c.FirstName.Contains("Lau"))
            .OrderBy(e => e.EmployeeID)
            .LastOrDefault();

        Assert.Equal(8u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(1) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[ReportsTo] = [e0].[EmployeeID]
WHERE FREETEXT([e0].[Title], N'President') AND FREETEXT([e].[Title], N'Inside') AND [e].[FirstName] LIKE N'%Lau%'
ORDER BY [e].[EmployeeID] DESC
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void FreeText_through_navigation_with_language_terms()
    {
        using var context = CreateContext();
        var result = context.Employees
            .Where(
                c => EF.Functions.FreeText(c.Manager.Title, "President", 1033)
                    && EF.Functions.FreeText(c.Title, "Inside", 1031)
                    && c.FirstName.Contains("Lau"))
            .FirstOrDefault();

        Assert.Equal(8u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(1) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[ReportsTo] = [e0].[EmployeeID]
WHERE FREETEXT([e0].[Title], N'President', LANGUAGE 1033) AND FREETEXT([e].[Title], N'Inside', LANGUAGE 1031) AND [e].[FirstName] LIKE N'%Lau%'
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task FreeText_throws_when_using_non_parameter_or_constant_for_freetext_string()
    {
        using var context = CreateContext();
        await Assert.ThrowsAsync<SqlException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.FreeText(e.City, e.FirstName)));

        await Assert.ThrowsAsync<SqlException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.FreeText(e.City, "")));

        await Assert.ThrowsAsync<SqlException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.FreeText(e.City, e.FirstName.ToUpper())));
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task FreeText_throws_when_using_non_column_for_property_reference()
    {
        using var context = CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.FreeText(e.City + "1", "President")));

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.FreeText(e.City.ToLower(), "President")));

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (from e1 in context.Employees
                               join m1 in context.Employees.OrderBy(e => e.EmployeeID).Skip(0)
                                   on e1.ReportsTo equals m1.EmployeeID
                               where EF.Functions.FreeText(m1.Title, "President")
                               select e1).LastOrDefaultAsync());
    }

    [ConditionalFact]
    public void Contains_should_throw_on_client_eval()
    {
        var exNoLang = Assert.Throws<InvalidOperationException>(() => EF.Functions.Contains("teststring", "teststring"));
        Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.Contains)),
            exNoLang.Message);

        var exLang = Assert.Throws<InvalidOperationException>(() => EF.Functions.Contains("teststring", "teststring", 1033));
        Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.Contains)),
            exLang.Message);
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task Contains_should_throw_when_using_non_parameter_or_constant_for_contains_string()
    {
        using var context = CreateContext();
        await Assert.ThrowsAsync<SqlException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.Contains(e.City, e.FirstName)));

        await Assert.ThrowsAsync<SqlException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.Contains(e.City, "")));

        await Assert.ThrowsAsync<SqlException>(
            async () => await context.Employees.FirstOrDefaultAsync(
                e => EF.Functions.Contains(e.City, e.FirstName.ToUpper())));
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void Contains_should_throw_for_no_FullText_index()
    {
        using var context = CreateContext();
        Assert.Throws<SqlException>(
            () => context.Employees.Where(c => EF.Functions.Contains(c.FirstName, "Fred")).ToArray());
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task Contains_literal()
    {
        using var context = CreateContext();
        var result = await context.Employees
            .Where(c => EF.Functions.Contains(c.Title, "Representative"))
            .ToListAsync();

        Assert.Equal(1u, result.First().EmployeeID);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'Representative')
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void Contains_with_language_term()
    {
        using var context = CreateContext();
        var result = context.Employees.SingleOrDefault(c => EF.Functions.Contains(c.Title, "President", 1033));

        Assert.Equal(2u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'President', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void Contains_with_non_literal_language_term()
    {
        var language = 1033;
        using var context = CreateContext();
        var result = context.Employees.SingleOrDefault(c => EF.Functions.Contains(c.Title, "President", language));

        Assert.Equal(2u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'President', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task Contains_with_logical_operator()
    {
        using var context = CreateContext();
        var result = await context.Employees
            .Where(c => EF.Functions.Contains(c.Title, "Vice OR Inside"))
            .ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(2u, result.First().EmployeeID);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'Vice OR Inside')
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task Contains_with_prefix_term_and_language_term()
    {
        using var context = CreateContext();
        var result = await context.Employees
            .SingleOrDefaultAsync(c => EF.Functions.Contains(c.Title, "\"Mana*\"", 1033));

        Assert.Equal(5u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'"Mana*"', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public async Task Contains_with_proximity_term_and_language_term()
    {
        using var context = CreateContext();
        var result = await context.Employees
            .SingleOrDefaultAsync(c => EF.Functions.Contains(c.Title, "NEAR((Sales, President), 1)", 1033));

        Assert.Equal(2u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'NEAR((Sales, President), 1)', LANGUAGE 1033)
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
    public void Contains_through_navigation()
    {
        using var context = CreateContext();
        var result = context.Employees
            .Where(
                c => EF.Functions.Contains(c.Manager.Title, "President")
                    && EF.Functions.Contains(c.Title, "\"Ins*\""))
            .FirstOrDefault();

        Assert.NotNull(result);
        Assert.Equal(8u, result.EmployeeID);

        AssertSql(
            """
SELECT TOP(1) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[ReportsTo] = [e0].[EmployeeID]
WHERE CONTAINS([e0].[Title], N'President') AND CONTAINS([e].[Title], N'"Ins*"')
""");
    }

    [ConditionalFact]
    public async Task PatIndex_literal()
    {
        using var context = CreateContext();
        var result = await context.Employees
            .Where(c => EF.Functions.PatIndex("%Nancy%", c.FirstName) == 1)
            .ToListAsync();

        Assert.Equal(1u, result.First().EmployeeID);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE PATINDEX(N'%Nancy%', [e].[FirstName]) = CAST(1 AS bigint)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Year(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffYear(c.OrderDate, DateTime.Now) == 0,
            c => c.OrderDate.Value.Year - DateTime.Now.Year == 0);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(year, [o].[OrderDate], GETDATE()) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Month(bool async)
    {
        var now = DateTime.Now;
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffMonth(c.OrderDate, DateTime.Now) == 0,
            c => c.OrderDate.Value.Year * 12 + c.OrderDate.Value.Month - (now.Year * 12 + now.Month) == 0);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(month, [o].[OrderDate], GETDATE()) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Day(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffDay(c.OrderDate, DateTime.Now) == 0,
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(day, [o].[OrderDate], GETDATE()) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Hour(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffHour(c.OrderDate, DateTime.Now) == 0,
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(hour, [o].[OrderDate], GETDATE()) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Minute(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffMinute(c.OrderDate, DateTime.Now) == 0,
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(minute, [o].[OrderDate], GETDATE()) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Second(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffSecond(c.OrderDate, DateTime.Now) == 0,
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(second, [o].[OrderDate], GETDATE()) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Millisecond(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffMillisecond(DateTime.Now, DateTime.Now.AddDays(1)) == 0,
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(millisecond, GETDATE(), DATEADD(day, CAST(1.0E0 AS int), GETDATE())) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Microsecond(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffMicrosecond(DateTime.Now, DateTime.Now.AddSeconds(1)) == 0,
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(microsecond, GETDATE(), DATEADD(second, CAST(1.0E0 AS int), GETDATE())) = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateDiff_Nanosecond(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.DateDiffNanosecond(DateTime.Now, DateTime.Now.AddSeconds(1)) == 0,
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(nanosecond, GETDATE(), DATEADD(second, CAST(1.0E0 AS int), GETDATE())) = 0
""");
    }

    [ConditionalFact]
    public virtual void DateDiff_Week_datetime()
    {
        using var context = CreateContext();
        var count = context.Orders
            .Count(
                c => EF.Functions.DateDiffWeek(
                        c.OrderDate,
                        new DateTime(1998, 5, 6, 0, 0, 0))
                    == 5);

        Assert.Equal(16, count);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(week, [o].[OrderDate], '1998-05-06T00:00:00.000') = 5
""");
    }

    [ConditionalFact]
    public virtual void DateDiff_Week_datetimeoffset()
    {
        using var context = CreateContext();
        var count = context.Orders
            .Count(
                c => EF.Functions.DateDiffWeek(
                        c.OrderDate,
                        new DateTimeOffset(1998, 5, 6, 0, 0, 0, TimeSpan.Zero))
                    == 5);

        Assert.Equal(16, count);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(week, CAST([o].[OrderDate] AS datetimeoffset), '1998-05-06T00:00:00.0000000+00:00') = 5
""");
    }

    [ConditionalFact]
    public virtual void DateDiff_Week_parameters_null()
    {
        using var context = CreateContext();
        var count = context.Orders
            .Count(
                c => EF.Functions.DateDiffWeek(
                        null,
                        c.OrderDate)
                    == 5);

        Assert.Equal(0, count);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(week, NULL, [o].[OrderDate]) = 5
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsDate_not_valid(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Where(o => !EF.Functions.IsDate(o.CustomerID)).Select(o => EF.Functions.IsDate(o.CustomerID)),
            ss => ss.Set<Order>().Select(c => false));

        AssertSql(
            """
SELECT CAST(ISDATE([o].[CustomerID]) AS bit)
FROM [Orders] AS [o]
WHERE CAST(ISDATE([o].[CustomerID]) AS bit) = CAST(0 AS bit)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsDate_valid(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => EF.Functions.IsDate(o.OrderDate.Value.ToString()))
                .Select(o => EF.Functions.IsDate(o.OrderDate.Value.ToString())),
            ss => ss.Set<Order>().Select(o => true));

        AssertSql(
            """
SELECT CAST(ISDATE(COALESCE(CONVERT(varchar(100), [o].[OrderDate]), '')) AS bit)
FROM [Orders] AS [o]
WHERE CAST(ISDATE(COALESCE(CONVERT(varchar(100), [o].[OrderDate]), '')) AS bit) = CAST(1 AS bit)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsDate_join_fields(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.IsDate(c.CustomerID + c.OrderID),
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE CAST(ISDATE(COALESCE([o].[CustomerID], N'') + CAST([o].[OrderID] AS nvarchar(max))) AS bit) = CAST(1 AS bit)
""");
    }

    [ConditionalFact]
    public void IsDate_should_throw_on_client_eval()
    {
        var exIsDate = Assert.Throws<InvalidOperationException>(() => EF.Functions.IsDate("#ISDATE#"));

        Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.IsDate)),
            exIsDate.Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsNumeric_not_valid(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => !EF.Functions.IsNumeric(o.OrderDate.Value.ToString()))
                .Select(o => EF.Functions.IsNumeric(o.OrderDate.Value.ToString())),
            ss => ss.Set<Order>().Select(c => false));

        AssertSql(
            """
SELECT ~CAST(ISNUMERIC(COALESCE(CONVERT(varchar(100), [o].[OrderDate]), '')) ^ 1 AS bit)
FROM [Orders] AS [o]
WHERE ISNUMERIC(COALESCE(CONVERT(varchar(100), [o].[OrderDate]), '')) <> 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsNummeric_valid(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(o => EF.Functions.IsNumeric(o.UnitPrice.ToString()))
                .Select(o => EF.Functions.IsNumeric(o.UnitPrice.ToString())),
            ss => ss.Set<OrderDetail>().Select(o => true));

        AssertSql(
            """
SELECT ~CAST(ISNUMERIC(CONVERT(varchar(100), [o].[UnitPrice])) ^ 1 AS bit)
FROM [Order Details] AS [o]
WHERE ISNUMERIC(CONVERT(varchar(100), [o].[UnitPrice])) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsNumeric_join_fields(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => EF.Functions.IsNumeric(c.CustomerID + c.OrderID),
            c => false);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE ISNUMERIC(COALESCE([o].[CustomerID], N'') + CAST([o].[OrderID] AS nvarchar(max))) = 1
""");
    }

    [ConditionalFact]
    public void IsNumeric_should_throw_on_client_eval()
    {
        var exIsDate = Assert.Throws<InvalidOperationException>(() => EF.Functions.IsNumeric("#ISNUMERIC#"));

        Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.IsNumeric)),
            exIsDate.Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateTimeFromParts_column_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => c.OrderDate > EF.Functions.DateTimeFromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 999),
            c => c.OrderDate > new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59, 999));

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > DATETIMEFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 999)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateTimeFromParts_constant_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => new DateTime(2018, 12, 29, 23, 20, 40) > EF.Functions.DateTimeFromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 999),
            c => new DateTime(2018, 12, 29, 23, 20, 40) > new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59, 999));

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE '2018-12-29T23:20:40.000' > DATETIMEFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 999)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateTimeFromParts_compare_with_local_variable(bool async)
    {
        var dateTime = new DateTime(1919, 12, 12, 10, 20, 15, 0);
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => dateTime
                > EF.Functions.DateTimeFromParts(
                    DateTime.Now.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second,
                    dateTime.Millisecond),
            c => dateTime
                > new DateTime(
                    DateTime.Now.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second,
                    dateTime.Millisecond));

        AssertSql(
            """
@dateTime='1919-12-12T10:20:15.0000000' (DbType = DateTime)
@dateTime_Month='12'
@dateTime_Day='12'
@dateTime_Hour='10'
@dateTime_Minute='20'
@dateTime_Second='15'
@dateTime_Millisecond='0'

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @dateTime > DATETIMEFROMPARTS(DATEPART(year, GETDATE()), @dateTime_Month, @dateTime_Day, @dateTime_Hour, @dateTime_Minute, @dateTime_Second, @dateTime_Millisecond)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateFromParts_column_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => c.OrderDate > EF.Functions.DateFromParts(DateTime.Now.Year, 12, 31),
            c => c.OrderDate > new DateTime(DateTime.Now.Year, 12, 31));

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > DATEFROMPARTS(DATEPART(year, GETDATE()), 12, 31)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateFromParts_constant_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => new DateTime(2018, 12, 29) > EF.Functions.DateFromParts(DateTime.Now.Year, 12, 31),
            c => new DateTime(2018, 12, 29) > new DateTime(DateTime.Now.Year, 12, 31));

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE '2018-12-29' > DATEFROMPARTS(DATEPART(year, GETDATE()), 12, 31)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateFromParts_compare_with_local_variable(bool async)
    {
        var date = new DateTime(1919, 12, 12);
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => date > EF.Functions.DateFromParts(DateTime.Now.Year, date.Month, date.Day),
            c => date > new DateTime(DateTime.Now.Year, date.Month, date.Day));

        AssertSql(
            """
@date='1919-12-12T00:00:00.0000000' (DbType = Date)
@date_Month='12'
@date_Day='12'

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @date > DATEFROMPARTS(DATEPART(year, GETDATE()), @date_Month, @date_Day)
""");
    }

    [ConditionalFact]
    public virtual void DateTime2FromParts_column_compare()
    {
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(c => c.OrderDate > EF.Functions.DateTime2FromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 999, 3));

            Assert.Equal(0, count);

            AssertSql(
                """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > DATETIME2FROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 999, 3)
""");
        }
    }

    [ConditionalFact]
    public virtual void DateTime2FromParts_constant_compare()
    {
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(
                    c => new DateTime(2018, 12, 29, 23, 20, 40)
                        > EF.Functions.DateTime2FromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 9999999, 7));

            Assert.Equal(0, count);

            AssertSql(
                """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE '2018-12-29T23:20:40.0000000' > DATETIME2FROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 9999999, 7)
""");
        }
    }

    [ConditionalFact]
    public virtual void DateTime2FromParts_compare_with_local_variable()
    {
        var dateTime = new DateTime(1919, 12, 12, 10, 20, 15);
        var fractions = 9999999;
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(
                    c => dateTime
                        > EF.Functions.DateTime2FromParts(
                            DateTime.Now.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, fractions,
                            7));

            Assert.Equal(0, count);

            AssertSql(
                """
@dateTime='1919-12-12T10:20:15.0000000'
@dateTime_Month='12'
@dateTime_Day='12'
@dateTime_Hour='10'
@dateTime_Minute='20'
@dateTime_Second='15'
@fractions='9999999'

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @dateTime > DATETIME2FROMPARTS(DATEPART(year, GETDATE()), @dateTime_Month, @dateTime_Day, @dateTime_Hour, @dateTime_Minute, @dateTime_Second, @fractions, 7)
""");
        }
    }

    [ConditionalFact]
    public virtual void DateTimeOffsetFromParts_column_compare()
    {
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(c => c.OrderDate > EF.Functions.DateTimeOffsetFromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 5, 12, 30, 1));

            Assert.Equal(0, count);

            AssertSql(
                """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE CAST([o].[OrderDate] AS datetimeoffset) > DATETIMEOFFSETFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 5, 12, 30, 1)
""");
        }
    }

    [ConditionalFact]
    public virtual void DateTimeOffsetFromParts_constant_compare()
    {
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(
                    c => new DateTimeOffset(2018, 12, 29, 23, 20, 40, new TimeSpan(1, 0, 0))
                        > EF.Functions.DateTimeOffsetFromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 50, 1, 0, 7));

            Assert.Equal(0, count);

            AssertSql(
                """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE '2018-12-29T23:20:40.0000000+01:00' > DATETIMEOFFSETFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 50, 1, 0, 7)
""");
        }
    }

    [ConditionalFact]
    public virtual void DateTimeOffsetFromParts_compare_with_local_variable()
    {
        var dateTimeOffset = new DateTimeOffset(1919, 12, 12, 10, 20, 15, new TimeSpan(1, 30, 0));
        var fractions = 5;
        var hourOffset = 1;
        var minuteOffset = 30;
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(
                    c => dateTimeOffset
                        > EF.Functions.DateTimeOffsetFromParts(
                            DateTime.Now.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute,
                            dateTimeOffset.Second, fractions, hourOffset, minuteOffset, 7));

            Assert.Equal(0, count);

            AssertSql(
                """
@dateTimeOffset='1919-12-12T10:20:15.0000000+01:30'
@dateTimeOffset_Month='12'
@dateTimeOffset_Day='12'
@dateTimeOffset_Hour='10'
@dateTimeOffset_Minute='20'
@dateTimeOffset_Second='15'
@fractions='5'
@hourOffset='1'
@minuteOffset='30'

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @dateTimeOffset > DATETIMEOFFSETFROMPARTS(DATEPART(year, GETDATE()), @dateTimeOffset_Month, @dateTimeOffset_Day, @dateTimeOffset_Hour, @dateTimeOffset_Minute, @dateTimeOffset_Second, @fractions, @hourOffset, @minuteOffset, 7)
""");
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SmallDateTimeFromParts_column_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => c.OrderDate > EF.Functions.SmallDateTimeFromParts(DateTime.Now.Year, 12, 31, 12, 59),
            c => c.OrderDate > new DateTime(DateTime.Now.Year, 12, 31, 12, 59, 0));

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > SMALLDATETIMEFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 12, 59)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SmallDateTimeFromParts_constant_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => new DateTime(2018, 12, 29, 23, 20, 0) > EF.Functions.SmallDateTimeFromParts(DateTime.Now.Year, 12, 31, 12, 59),
            c => new DateTime(2018, 12, 29, 23, 20, 0) > new DateTime(DateTime.Now.Year, 12, 31, 12, 59, 0));

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE '2018-12-29T23:20:00' > SMALLDATETIMEFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 12, 59)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SmallDateTimeFromParts_compare_with_local_variable(bool async)
    {
        var dateTime = new DateTime(1919, 12, 12, 23, 20, 0);
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => dateTime
                > EF.Functions.SmallDateTimeFromParts(DateTime.Now.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute),
            c => dateTime > new DateTime(DateTime.Now.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0));

        AssertSql(
            """
@dateTime='1919-12-12T23:20:00.0000000' (DbType = DateTime)
@dateTime_Month='12'
@dateTime_Day='12'
@dateTime_Hour='23'
@dateTime_Minute='20'

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @dateTime > SMALLDATETIMEFROMPARTS(DATEPART(year, GETDATE()), @dateTime_Month, @dateTime_Day, @dateTime_Hour, @dateTime_Minute)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task TimeFromParts_constant_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => new TimeSpan(23, 59, 0) > EF.Functions.TimeFromParts(23, 59, 59, c.OrderID % 60, 3),
            c => new TimeSpan(23, 59, 0) > new TimeSpan(0, 23, 59, 59, c.OrderID % 60));

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE '23:59:00' > TIMEFROMPARTS(23, 59, 59, [o].[OrderID] % 60, 3)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task TimeFromParts_select(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Select(o => EF.Functions.TimeFromParts(23, 59, 59, o.OrderID % 60, 3)),
            ss => ss.Set<Order>().Select(o => new TimeSpan(0, 23, 59, 59, o.OrderID % 60)));

        AssertSql(
            """
SELECT TIMEFROMPARTS(23, 59, 59, [o].[OrderID] % 60, 3)
FROM [Orders] AS [o]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DataLength_column_compare(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            c => c.OrderID % 10 == EF.Functions.DataLength(c.OrderDate),
            c => c.OrderID % 10 == 8);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderID] % 10 = DATALENGTH([o].[OrderDate])
""");
    }

    [ConditionalFact]
    public virtual void DataLength_constant_compare()
    {
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(c => 100 < EF.Functions.DataLength(c.OrderDate));

            Assert.Equal(0, count);

            AssertSql(
                """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE 100 < DATALENGTH([o].[OrderDate])
""");
        }
    }

    [ConditionalFact]
    public virtual void DataLength_compare_with_local_variable()
    {
        int? length = 100;
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(c => length < EF.Functions.DataLength(c.OrderDate));

            Assert.Equal(0, count);

            AssertSql(
                """
@length='100' (Nullable = true)

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @length < DATALENGTH([o].[OrderDate])
""");
        }
    }

    [ConditionalFact]
    public virtual void DataLength_all_constants()
    {
        using (var context = CreateContext())
        {
            var count = context.Orders
                .Count(c => EF.Functions.DataLength("foo") == 3);

            Assert.Equal(0, count);

            AssertSql(
                """
SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE CAST(DATALENGTH(N'foo') AS int) = 3
""");
        }
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
