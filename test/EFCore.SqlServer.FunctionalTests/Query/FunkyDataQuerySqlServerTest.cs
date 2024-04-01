// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class FunkyDataQuerySqlServerTest : FunkyDataQueryTestBase<FunkyDataQuerySqlServerTest.FunkyDataQuerySqlServerFixture>
{
    public FunkyDataQuerySqlServerTest(FunkyDataQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override QueryAsserter CreateQueryAsserter(FunkyDataQuerySqlServerFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public override async Task String_contains_on_argument_with_wildcard_constant(bool async)
    {
        await base.String_contains_on_argument_with_wildcard_constant(async);

        AssertSql(
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'%\%B%' ESCAPE N'\'
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'%a\_%' ESCAPE N'\'
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE 0 = 1
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] IS NOT NULL
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'%\_Ba\_%' ESCAPE N'\'
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE N'%\%B\%a\%r%' ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] IS NULL
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
""");
    }

    public override async Task String_contains_on_argument_with_wildcard_parameter(bool async)
    {
        await base.String_contains_on_argument_with_wildcard_parameter(async);

        AssertSql(
            """
@__prm1_0_contains='%\%B%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm1_0_contains ESCAPE N'\'
""",
            //
            """
@__prm2_0_contains='%a\_%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm2_0_contains ESCAPE N'\'
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE 0 = 1
""",
            //
            """
@__prm4_0_contains='%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm4_0_contains ESCAPE N'\'
""",
            //
            """
@__prm5_0_contains='%\_Ba\_%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm5_0_contains ESCAPE N'\'
""",
            //
            """
@__prm6_0_contains='%\%B\%a\%r%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE @__prm6_0_contains ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
            //
            """
@__prm7_0_contains='%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE @__prm7_0_contains ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
""");
    }

    public override async Task String_contains_on_argument_with_wildcard_column(bool async)
    {
        await base.String_contains_on_argument_with_wildcard_column(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE [f].[FirstName] IS NOT NULL AND [f0].[LastName] IS NOT NULL AND (CHARINDEX([f0].[LastName], [f].[FirstName]) > 0 OR [f0].[LastName] LIKE N'')
""");
    }

    public override async Task String_contains_on_argument_with_wildcard_column_negated(bool async)
    {
        await base.String_contains_on_argument_with_wildcard_column_negated(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE NOT ([f].[FirstName] IS NOT NULL AND [f0].[LastName] IS NOT NULL AND (CHARINDEX([f0].[LastName], [f].[FirstName]) > 0 OR [f0].[LastName] LIKE N''))
""");
    }

    public override async Task String_starts_with_on_argument_with_wildcard_constant(bool async)
    {
        await base.String_starts_with_on_argument_with_wildcard_constant(async);

        AssertSql(
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'\%B%' ESCAPE N'\'
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'\_B%' ESCAPE N'\'
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE 0 = 1
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] IS NOT NULL
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'\_Ba\_%' ESCAPE N'\'
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE N'\%B\%a\%r%' ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] IS NULL
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
""");
    }

    public override async Task String_starts_with_on_argument_with_wildcard_parameter(bool async)
    {
        await base.String_starts_with_on_argument_with_wildcard_parameter(async);

        AssertSql(
            """
@__prm1_0_startswith='\%B%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm1_0_startswith ESCAPE N'\'
""",
            //
            """
@__prm2_0_startswith='\_B%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm2_0_startswith ESCAPE N'\'
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE 0 = 1
""",
            //
            """
@__prm4_0_startswith='%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm4_0_startswith ESCAPE N'\'
""",
            //
            """
@__prm5_0_startswith='\_Ba\_%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm5_0_startswith ESCAPE N'\'
""",
            //
            """
@__prm6_0_startswith='\%B\%a\%r%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE @__prm6_0_startswith ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
            //
            """
@__prm7_0_startswith='%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE @__prm7_0_startswith ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
""");
    }

    public override async Task String_starts_with_on_argument_with_bracket(bool async)
    {
        await base.String_starts_with_on_argument_with_bracket(async);

        AssertSql(
            """
SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'\[%' ESCAPE N'\'
""",
            //
            """
SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'B\[%' ESCAPE N'\'
""",
            //
            """
SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'B\[\[a^%' ESCAPE N'\'
""",
            //
            """
@__prm1_0_startswith='\[%' (Size = 4000)

SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm1_0_startswith ESCAPE N'\'
""",
            //
            """
@__prm2_0_startswith='B\[%' (Size = 4000)

SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm2_0_startswith ESCAPE N'\'
""",
            //
            """
@__prm3_0_startswith='B\[\[a^%' (Size = 4000)

SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm3_0_startswith ESCAPE N'\'
""",
            //
            """
SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] IS NOT NULL AND [f].[LastName] IS NOT NULL AND LEFT([f].[FirstName], LEN([f].[LastName])) = [f].[LastName]
""");
    }

    public override async Task String_starts_with_on_argument_with_wildcard_column(bool async)
    {
        await base.String_starts_with_on_argument_with_wildcard_column(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE [f].[FirstName] IS NOT NULL AND [f0].[LastName] IS NOT NULL AND LEFT([f].[FirstName], LEN([f0].[LastName])) = [f0].[LastName]
""");
    }

    public override async Task String_starts_with_on_argument_with_wildcard_column_negated(bool async)
    {
        await base.String_starts_with_on_argument_with_wildcard_column_negated(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE [f].[FirstName] IS NULL OR [f0].[LastName] IS NULL OR LEFT([f].[FirstName], LEN([f0].[LastName])) <> [f0].[LastName]
""");
    }

    public override async Task String_ends_with_on_argument_with_wildcard_constant(bool async)
    {
        await base.String_ends_with_on_argument_with_wildcard_constant(async);

        AssertSql(
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'%\%r' ESCAPE N'\'
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'%r\_' ESCAPE N'\'
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE 0 = 1
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] IS NOT NULL
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE N'%\_r\_' ESCAPE N'\'
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE N'%a\%r\%' ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] IS NULL
""",
                //
                """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
""");
    }

    public override async Task String_ends_with_on_argument_with_wildcard_parameter(bool async)
    {
        await base.String_ends_with_on_argument_with_wildcard_parameter(async);

        AssertSql(
            """
@__prm1_0_endswith='%\%r' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm1_0_endswith ESCAPE N'\'
""",
            //
            """
@__prm2_0_endswith='%r\_' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm2_0_endswith ESCAPE N'\'
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE 0 = 1
""",
            //
            """
@__prm4_0_endswith='%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm4_0_endswith ESCAPE N'\'
""",
            //
            """
@__prm5_0_endswith='%\_r\_' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__prm5_0_endswith ESCAPE N'\'
""",
            //
            """
@__prm6_0_endswith='%a\%r\%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE @__prm6_0_endswith ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
            //
            """
@__prm7_0_endswith='%' (Size = 4000)

SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] NOT LIKE @__prm7_0_endswith ESCAPE N'\' OR [f].[FirstName] IS NULL
""",
            //
            """
SELECT [f].[FirstName]
FROM [FunkyCustomers] AS [f]
""");
    }

    public override async Task String_ends_with_on_argument_with_wildcard_column(bool async)
    {
        await base.String_ends_with_on_argument_with_wildcard_column(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE [f].[FirstName] IS NOT NULL AND [f0].[LastName] IS NOT NULL AND RIGHT([f].[FirstName], LEN([f0].[LastName])) = [f0].[LastName]
""");
    }

    public override async Task String_ends_with_on_argument_with_wildcard_column_negated(bool async)
    {
        await base.String_ends_with_on_argument_with_wildcard_column_negated(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE [f].[FirstName] IS NULL OR [f0].[LastName] IS NULL OR RIGHT([f].[FirstName], LEN([f0].[LastName])) <> [f0].[LastName]
""");
    }

    public override async Task String_ends_with_inside_conditional(bool async)
    {
        await base.String_ends_with_inside_conditional(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE CASE
    WHEN [f].[FirstName] IS NOT NULL AND [f0].[LastName] IS NOT NULL AND RIGHT([f].[FirstName], LEN([f0].[LastName])) = [f0].[LastName] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task String_ends_with_inside_conditional_negated(bool async)
    {
        await base.String_ends_with_inside_conditional_negated(async);

        AssertSql(
            """
SELECT [f].[FirstName] AS [fn], [f0].[LastName] AS [ln]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE CASE
    WHEN [f].[FirstName] IS NULL OR [f0].[LastName] IS NULL OR RIGHT([f].[FirstName], LEN([f0].[LastName])) <> [f0].[LastName] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task String_ends_with_equals_nullable_column(bool async)
    {
        await base.String_ends_with_equals_nullable_column(async);

        AssertSql(
            """
SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool], [f0].[Id], [f0].[FirstName], [f0].[LastName], [f0].[NullableBool]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE CASE
    WHEN [f].[FirstName] IS NOT NULL AND [f0].[LastName] IS NOT NULL AND RIGHT([f].[FirstName], LEN([f0].[LastName])) = [f0].[LastName] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = [f].[NullableBool]
""");
    }

    public override async Task String_ends_with_not_equals_nullable_column(bool async)
    {
        await base.String_ends_with_not_equals_nullable_column(async);

        AssertSql(
            """
SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool], [f0].[Id], [f0].[FirstName], [f0].[LastName], [f0].[NullableBool]
FROM [FunkyCustomers] AS [f]
CROSS JOIN [FunkyCustomers] AS [f0]
WHERE CASE
    WHEN [f].[FirstName] IS NOT NULL AND [f0].[LastName] IS NOT NULL AND RIGHT([f].[FirstName], LEN([f0].[LastName])) = [f0].[LastName] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> [f].[NullableBool] OR [f].[NullableBool] IS NULL
""");
    }

    public override async Task String_FirstOrDefault_and_LastOrDefault(bool async)
    {
        await base.String_FirstOrDefault_and_LastOrDefault(async);

        AssertSql(
            """
SELECT SUBSTRING([f].[FirstName], 1, 1) AS [first], SUBSTRING([f].[FirstName], LEN([f].[FirstName]), 1) AS [last]
FROM [FunkyCustomers] AS [f]
ORDER BY [f].[Id]
""");
    }

    public override async Task String_Contains_and_StartsWith_with_same_parameter(bool async)
    {
        await base.String_Contains_and_StartsWith_with_same_parameter(async);

        AssertSql(
            """
@__s_0_contains='%B%' (Size = 4000)
@__s_0_startswith='B%' (Size = 4000)

SELECT [f].[Id], [f].[FirstName], [f].[LastName], [f].[NullableBool]
FROM [FunkyCustomers] AS [f]
WHERE [f].[FirstName] LIKE @__s_0_contains ESCAPE N'\' OR [f].[LastName] LIKE @__s_0_startswith ESCAPE N'\'
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FunkyDataQuerySqlServerFixture : FunkyDataQueryFixtureBase, ITestSqlLoggerFactory
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
