// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class StringTranslationsSqlServerTest : StringTranslationsRelationalTestBase<BasicTypesQuerySqlServerFixture>
{
    public StringTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool IsCaseSensitive
        => false;

    #region Equals

    public override async Task Equals(bool async)
    {
        await base.Equals(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'seattle'
""");
    }

    public override async Task Equals_with_OrdinalIgnoreCase(bool async)
    {
        await base.Equals_with_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task Equals_with_Ordinal(bool async)
    {
        await base.Equals_with_Ordinal(async);

        AssertSql();
    }

    public override async Task Static_Equals(bool async)
    {
        await base.Static_Equals(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'seattle'
""");
    }

    public override async Task Static_Equals_with_OrdinalIgnoreCase(bool async)
    {
        await base.Static_Equals_with_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task Static_Equals_with_Ordinal(bool async)
    {
        await base.Static_Equals_with_Ordinal(async);

        AssertSql();
    }

    #endregion Equals

    #region Miscellaneous

    public override async Task Length(bool async)
    {
        await base.Length(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) = 7
""");
    }

    public override async Task ToUpper(bool async)
    {
        await base.ToUpper(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE UPPER([b].[String]) = N'SEATTLE'
""",
            //
            """
SELECT UPPER([b].[String])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task ToLower(bool async)
    {
        await base.ToLower(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LOWER([b].[String]) = N'seattle'
""",
            //
            """
SELECT LOWER([b].[String])
FROM [BasicTypesEntities] AS [b]
""");
    }

    #endregion Miscellaneous

    #region IndexOf

    public override async Task IndexOf(bool async)
    {
        await base.IndexOf(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CHARINDEX(N'Eattl', [b].[String]) AS int) - 1 <> -1
""");
    }

    public override async Task IndexOf_Char(bool async)
    {
        await base.IndexOf_Char(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CHARINDEX('e', [b].[String]) AS int) - 1 <> -1
""");
    }

    public override async Task IndexOf_with_empty_string(bool async)
    {
        await base.IndexOf_with_empty_string(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task IndexOf_with_one_parameter_arg(bool async)
    {
        await base.IndexOf_with_one_parameter_arg(async);

        AssertSql(
            """
@pattern='Eattl' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CHARINDEX(@pattern, [b].[String]) AS int) - CASE
    WHEN @pattern = N'' THEN 0
    ELSE 1
END = 1
""");

    }

    public override async Task IndexOf_with_one_parameter_arg_char(bool async)
    {
        await base.IndexOf_with_one_parameter_arg_char(async);

        AssertSql(
            """
@pattern='e' (Size = 1) (DbType = String)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CHARINDEX(@pattern, [b].[String]) AS int) - CASE
    WHEN @pattern = N'' THEN 0
    ELSE 1
END = 1
""");

    }

    public override async Task IndexOf_with_constant_starting_position(bool async)
    {
        await base.IndexOf_with_constant_starting_position(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX(N'e', [b].[String], 3) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_with_constant_starting_position_char(bool async)
    {
        await base.IndexOf_with_constant_starting_position_char(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX('e', [b].[String], 3) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_with_parameter_starting_position(bool async)
    {
        await base.IndexOf_with_parameter_starting_position(async);

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX(N'E', [b].[String], @start + 1) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_with_parameter_starting_position_char(bool async)
    {
        await base.IndexOf_with_parameter_starting_position_char(async);

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX('e', [b].[String], @start + 1) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_after_ToString(bool async)
    {
        await base.IndexOf_after_ToString(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CHARINDEX('55', CONVERT(varchar(11), [b].[Int])) - 1 = 1
""");
    }

    public override async Task IndexOf_over_ToString(bool async)
    {
        await base.IndexOf_over_ToString(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CHARINDEX(CONVERT(varchar(11), [b].[Int]), '12559') - CASE
    WHEN CONVERT(varchar(11), [b].[Int]) = '' THEN 0
    ELSE 1
END = 1
""");
    }

    #endregion IndexOf

    #region Replace

    public override async Task Replace(bool async)
    {
        await base.Replace(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE REPLACE([b].[String], N'sea', N'rea') = N'reattle'
""");
    }

    public override async Task Replace_Char(bool async)
    {
        await base.Replace_Char(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE REPLACE([b].[String], 'S', 'R') = N'Reattle'
""");
    }

    public override async Task Replace_with_empty_string(bool async)
    {
        await base.Replace_with_empty_string(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> N'' AND REPLACE([b].[String], [b].[String], N'') = N''
""");
    }

    public override async Task Replace_using_property_arguments(bool async)
    {
        await base.Replace_using_property_arguments(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> N'' AND REPLACE([b].[String], [b].[String], CONVERT(varchar(11), [b].[Int])) = CONVERT(varchar(11), [b].[Int])
""");
    }

    #endregion Replace

    #region Substring

    public override async Task Substring(bool async)
    {
        await base.Substring(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 3 AND SUBSTRING([b].[String], 1 + 1, 2) = N'ea'
""");
    }

    public override async Task Substring_with_one_arg_with_zero_startIndex(bool async)
    {
        await base.Substring_with_one_arg_with_zero_startIndex(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE SUBSTRING([b].[String], 0 + 1, LEN([b].[String])) = N'Seattle'
""");
    }

    public override async Task Substring_with_one_arg_with_constant(bool async)
    {
        await base.Substring_with_one_arg_with_constant(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 1 AND SUBSTRING([b].[String], 1 + 1, LEN([b].[String])) = N'eattle'
""");
    }

    public override async Task Substring_with_one_arg_with_parameter(bool async)
    {
        await base.Substring_with_one_arg_with_parameter(async);

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 2 AND SUBSTRING([b].[String], @start + 1, LEN([b].[String])) = N'attle'
""");
    }

    public override async Task Substring_with_two_args_with_zero_startIndex(bool async)
    {
        await base.Substring_with_two_args_with_zero_startIndex(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 3 AND SUBSTRING([b].[String], 0 + 1, 3) = N'Sea'
""");
    }

    public override async Task Substring_with_two_args_with_zero_length(bool async)
    {
        await base.Substring_with_two_args_with_zero_length(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 2 AND SUBSTRING([b].[String], 2 + 1, 0) = N''
""");
    }

    public override async Task Substring_with_two_args_with_parameter(bool async)
    {
        await base.Substring_with_two_args_with_parameter(async);

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 5 AND SUBSTRING([b].[String], @start + 1, 3) = N'att'
""");
    }

    public override async Task Substring_with_two_args_with_IndexOf(bool async)
    {
        await base.Substring_with_two_args_with_IndexOf(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%a%' AND SUBSTRING([b].[String], (CAST(CHARINDEX(N'a', [b].[String]) AS int) - 1) + 1, 3) = N'att'
""");
    }

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    public override async Task IsNullOrEmpty(bool async)
    {
        await base.IsNullOrEmpty(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Bool], [n].[Byte], [n].[ByteArray], [n].[DateOnly], [n].[DateTime], [n].[DateTimeOffset], [n].[Decimal], [n].[Double], [n].[Enum], [n].[FlagsEnum], [n].[Float], [n].[Guid], [n].[Int], [n].[Long], [n].[Short], [n].[String], [n].[TimeOnly], [n].[TimeSpan]
FROM [NullableBasicTypesEntities] AS [n]
WHERE [n].[String] IS NULL OR [n].[String] LIKE N''
""",
            //
            """
SELECT CASE
    WHEN [n].[String] IS NULL OR [n].[String] LIKE N'' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [NullableBasicTypesEntities] AS [n]
""");
    }

    public override async Task IsNullOrEmpty_negated(bool async)
    {
        await base.IsNullOrEmpty_negated(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Bool], [n].[Byte], [n].[ByteArray], [n].[DateOnly], [n].[DateTime], [n].[DateTimeOffset], [n].[Decimal], [n].[Double], [n].[Enum], [n].[FlagsEnum], [n].[Float], [n].[Guid], [n].[Int], [n].[Long], [n].[Short], [n].[String], [n].[TimeOnly], [n].[TimeSpan]
FROM [NullableBasicTypesEntities] AS [n]
WHERE [n].[String] IS NOT NULL AND [n].[String] NOT LIKE N''
""",
            //
            """
SELECT CASE
    WHEN [n].[String] IS NOT NULL AND [n].[String] NOT LIKE N'' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [NullableBasicTypesEntities] AS [n]
""");
    }

    public override async Task IsNullOrWhiteSpace(bool async)
    {
        await base.IsNullOrWhiteSpace(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N''
""");
    }

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    public override async Task StartsWith_Literal(bool async)
    {
        await base.StartsWith_Literal(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'se%'
""");
    }

    public override async Task StartsWith_Literal_Char(bool async)
    {
        await base.StartsWith_Literal_Char(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'S%'
""");
    }

    public override async Task StartsWith_Parameter(bool async)
    {
        await base.StartsWith_Parameter(async);

        AssertSql(
            """
@pattern_startswith='se%' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_startswith ESCAPE N'\'
""");
    }

    public override async Task StartsWith_Parameter_Char(bool async)
    {
        await base.StartsWith_Parameter_Char(async);

        AssertSql(
            """
@pattern_startswith='S%' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_startswith ESCAPE N'\'
""");
    }

    public override async Task StartsWith_Column(bool async)
    {
        await base.StartsWith_Column(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEFT([b].[String], LEN([b].[String])) = [b].[String]
""");
    }

    public override async Task StartsWith_with_StringComparison_Ordinal(bool async)
    {
        await base.StartsWith_with_StringComparison_Ordinal(async);

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
    {
        await base.StartsWith_with_StringComparison_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_unsupported(bool async)
    {
        await base.StartsWith_with_StringComparison_unsupported(async);

        AssertSql();
    }

    #endregion StartsWith

    #region EndsWith

    public override async Task EndsWith_Literal(bool async)
    {
        await base.EndsWith_Literal(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%Le'
""");
    }

    public override async Task EndsWith_Literal_Char(bool async)
    {
        await base.EndsWith_Literal_Char(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%e'
""");
    }

    public override async Task EndsWith_Parameter(bool async)
    {
        await base.EndsWith_Parameter(async);

        AssertSql(
            """
@pattern_endswith='%LE' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_endswith ESCAPE N'\'
""");
    }

    public override async Task EndsWith_Parameter_Char(bool async)
    {
        await base.EndsWith_Parameter_Char(async);

        AssertSql(
            """
@pattern_endswith='%e' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_endswith ESCAPE N'\'
""");
    }

    public override async Task EndsWith_Column(bool async)
    {
        // SQL Server trims trailing whitespace for length calculations, making our EndsWith() column translation not work reliably in that
        // case
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String == "Seattle" && b.String.EndsWith(b.String)));

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'Seattle' AND RIGHT([b].[String], LEN([b].[String])) = [b].[String]
""");
    }

    public override async Task EndsWith_with_StringComparison_Ordinal(bool async)
    {
        await base.EndsWith_with_StringComparison_Ordinal(async);

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
    {
        await base.EndsWith_with_StringComparison_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_unsupported(bool async)
    {
        await base.EndsWith_with_StringComparison_unsupported(async);

        AssertSql();
    }

    #endregion EndsWith

    #region Contains

    public override async Task Contains_Literal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl")), // SQL Server is case-insensitive by default
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.OrdinalIgnoreCase)));

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%eattl%'
""");
    }

    public override async Task Contains_Literal_Char(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains('e')));

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%e%'
""");
    }

    public override async Task Contains_Column(bool async)
    {
        await base.Contains_Column(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CHARINDEX([b].[String], [b].[String]) > 0 OR [b].[String] LIKE N''
""",
            //
            """
SELECT CASE
    WHEN CHARINDEX([b].[String], [b].[String]) > 0 OR [b].[String] LIKE N'' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task Contains_negated(bool async)
    {
        await base.Contains_negated(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] NOT LIKE N'%Eattle%'
""",
            //
            """
SELECT CASE
    WHEN [b].[String] NOT LIKE N'%Eattle%' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task Contains_with_StringComparison_Ordinal(bool async)
    {
        await base.Contains_with_StringComparison_Ordinal(async);

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_OrdinalIgnoreCase(bool async)
    {
        await base.Contains_with_StringComparison_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_unsupported(bool async)
    {
        await base.Contains_with_StringComparison_unsupported(async);

        AssertSql();
    }

    public override async Task Contains_constant_with_whitespace(bool async)
    {
        await base.Contains_constant_with_whitespace(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%     %'
""");
    }

    public override async Task Contains_parameter_with_whitespace(bool async)
    {
        await base.Contains_parameter_with_whitespace(async);

        AssertSql(
            """
@pattern_contains='%     %' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_contains ESCAPE N'\'
""");
    }

    #endregion Contains

    #region TrimStart

    public override async Task TrimStart_without_arguments(bool async)
    {
        await base.TrimStart_without_arguments(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LTRIM([b].[String]) = N'Boston  '
""");
    }

    public override Task TrimStart_with_char_argument(bool async)
        => AssertTranslationFailed(() => base.TrimStart_with_char_argument(async));

    public override Task TrimStart_with_char_array_argument(bool async)
        => AssertTranslationFailed(() => base.TrimStart_with_char_array_argument(async));

    #endregion TrimStart

    #region TrimEnd

    public override async Task TrimEnd_without_arguments(bool async)
    {
        await base.TrimEnd_without_arguments(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE RTRIM([b].[String]) = N'  Boston'
""");
    }

    public override Task TrimEnd_with_char_argument(bool async)
        => AssertTranslationFailed(() => base.TrimEnd_with_char_argument(async));

    public override Task TrimEnd_with_char_array_argument(bool async)
        => AssertTranslationFailed(() => base.TrimEnd_with_char_array_argument(async));

    #endregion TrimEnd

    #region Trim

    public override async Task Trim_without_argument_in_predicate(bool async)
    {
        await base.Trim_without_argument_in_predicate(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LTRIM(RTRIM([b].[String])) = N'Boston'
""");
    }

    public override async Task Trim_with_char_argument_in_predicate(bool async)
    {
        // String.Trim with parameters. Issue #22927.
        await AssertTranslationFailed(() => base.Trim_with_char_argument_in_predicate(async));

        AssertSql();
    }

    public override async Task Trim_with_char_array_argument_in_predicate(bool async)
    {
        // String.Trim with parameters. Issue #22927.
        await AssertTranslationFailed(() => base.Trim_with_char_array_argument_in_predicate(async));

        AssertSql();
    }

    #endregion Trim

    #region Compare

    public override async Task Compare_simple_zero(bool async)
    {
        await base.Compare_simple_zero(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> N'seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > N'seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > N'seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'seattle'
""");
    }

    public override async Task Compare_simple_one(bool async)
    {
        await base.Compare_simple_one(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] < N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= N'Seattle'
""");
    }

    public override async Task Compare_with_parameter(bool async)
    {
        await base.Compare_with_parameter(async);

        AssertSql(
            """
@basicTypeEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] < @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= @basicTypeEntity_String
""");
    }

    public override async Task Compare_simple_more_than_one(bool async)
    {
        await base.Compare_simple_more_than_one(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CASE
    WHEN [b].[String] = N'Seattle' THEN 0
    WHEN [b].[String] > N'Seattle' THEN 1
    WHEN [b].[String] < N'Seattle' THEN -1
END = 42
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CASE
    WHEN [b].[String] = N'Seattle' THEN 0
    WHEN [b].[String] > N'Seattle' THEN 1
    WHEN [b].[String] < N'Seattle' THEN -1
END > 42
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE 42 > CASE
    WHEN [b].[String] = N'Seattle' THEN 0
    WHEN [b].[String] > N'Seattle' THEN 1
    WHEN [b].[String] < N'Seattle' THEN -1
END
""");
    }

    public override async Task Compare_nested(bool async)
    {
        await base.Compare_nested(async);

        AssertSql(
"""
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'M' + [b].[String]
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> SUBSTRING([b].[String], 0 + 1, 0)
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > REPLACE(N'Seattle', N'Sea', [b].[String])
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'M' + [b].[String]
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > SUBSTRING([b].[String], 0 + 1, 0)
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] < REPLACE(N'Seattle', N'Sea', [b].[String])
""");
    }

    public override async Task Compare_multi_predicate(bool async)
    {
        await base.Compare_multi_predicate(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= N'Seattle' AND [b].[String] < N'Toronto'
""");
    }

    public override async Task CompareTo_simple_zero(bool async)
    {
        await base.CompareTo_simple_zero(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'Seattle'
""");
    }

    public override async Task CompareTo_simple_one(bool async)
    {
        await base.CompareTo_simple_one(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] < N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= N'Seattle'
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= N'Seattle'
""");
    }

    public override async Task CompareTo_with_parameter(bool async)
    {
        await base.CompareTo_with_parameter(async);

        AssertSql(
            """
@basicTypesEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] < @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= @basicTypesEntity_String
""");
    }

    public override async Task CompareTo_simple_more_than_one(bool async)
    {
        await base.CompareTo_simple_more_than_one(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CASE
    WHEN [b].[String] = N'Seattle' THEN 0
    WHEN [b].[String] > N'Seattle' THEN 1
    WHEN [b].[String] < N'Seattle' THEN -1
END = 42
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CASE
    WHEN [b].[String] = N'Seattle' THEN 0
    WHEN [b].[String] > N'Seattle' THEN 1
    WHEN [b].[String] < N'Seattle' THEN -1
END > 42
""",
            //
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE 42 > CASE
    WHEN [b].[String] = N'Seattle' THEN 0
    WHEN [b].[String] > N'Seattle' THEN 1
    WHEN [b].[String] < N'Seattle' THEN -1
END
""");
    }

    public override async Task CompareTo_nested(bool async)
    {
        await base.CompareTo_nested(async);

        AssertSql(
"""
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'M' + [b].[String]
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> SUBSTRING([b].[String], 0 + 1, 0)
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > REPLACE(N'Seattle', N'Sea', [b].[String])
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <= N'M' + [b].[String]
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] > SUBSTRING([b].[String], 0 + 1, 0)
""",
                //
                """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] < REPLACE(N'Seattle', N'Sea', [b].[String])
""");
    }

    public override async Task Compare_to_multi_predicate(bool async)
    {
        await base.Compare_to_multi_predicate(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= N'Seattle' AND [b].[String] < N'Toronto'
""");
    }

    #endregion Compare

    #region Join

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Join_over_non_nullable_column(bool async)
    {
        await base.Join_over_non_nullable_column(async);

        AssertSql(
            """
SELECT [b].[Int] AS [Key], COALESCE(STRING_AGG([b].[String], N'|'), N'') AS [Strings]
FROM [BasicTypesEntities] AS [b]
GROUP BY [b].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Join_over_nullable_column(bool async)
    {
        await base.Join_over_nullable_column(async);

        AssertSql(
            """
SELECT [n0].[Key], COALESCE(STRING_AGG(COALESCE([n0].[String], N''), N'|'), N'') AS [Regions]
FROM (
    SELECT [n].[String], COALESCE([n].[Int], 0) AS [Key]
    FROM [NullableBasicTypesEntities] AS [n]
) AS [n0]
GROUP BY [n0].[Key]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Join_with_predicate(bool async)
    {
        await base.Join_with_predicate(async);

        AssertSql(
            """
SELECT [b].[Int] AS [Key], COALESCE(STRING_AGG(CASE
    WHEN CAST(LEN([b].[String]) AS int) > 6 THEN [b].[String]
END, N'|'), N'') AS [Strings]
FROM [BasicTypesEntities] AS [b]
GROUP BY [b].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Join_with_ordering(bool async)
    {
        await base.Join_with_ordering(async);

        AssertSql(
            """
SELECT [b].[Int] AS [Key], COALESCE(STRING_AGG([b].[String], N'|') WITHIN GROUP (ORDER BY [b].[Id] DESC), N'') AS [Strings]
FROM [BasicTypesEntities] AS [b]
GROUP BY [b].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Join_non_aggregate(bool async)
    {
        await base.Join_non_aggregate(async);

        AssertSql(
            """
@foo='foo' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CONCAT_WS(N'|', [b].[String], @foo, N'', N'bar') = N'Seattle|foo||bar'
""");
    }

    #endregion Join

    #region Concatenation

    public override async Task Concat_operator(bool async)
    {
        await base.Concat_operator(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] + N'Boston' = N'SeattleBoston'
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Concat_aggregate(bool async)
    {
        await base.Concat_aggregate(async);

        AssertSql(
            """
SELECT [b].[Int] AS [Key], COALESCE(STRING_AGG([b].[String], N''), N'') AS [BasicTypesEntitys]
FROM [BasicTypesEntities] AS [b]
GROUP BY [b].[Int]
""");
    }

    public override async Task Concat_string_int_comparison1(bool async)
    {
        await base.Concat_string_int_comparison1(async);

        AssertSql(
            """
@i='10'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] + CAST(@i AS nvarchar(max)) = N'Seattle10'
""");
    }

    public override async Task Concat_string_int_comparison2(bool async)
    {
        await base.Concat_string_int_comparison2(async);

        AssertSql(
            """
@i='10'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(@i AS nvarchar(max)) + [b].[String] = N'10Seattle'
""");
    }

    public override async Task Concat_string_int_comparison3(bool async)
    {
        await base.Concat_string_int_comparison3(async);

        AssertSql(
            """
@p='30'
@j='21'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(@p AS nvarchar(max)) + [b].[String] + CAST(@j AS nvarchar(max)) + CAST(42 AS nvarchar(max)) = N'30Seattle2142'
""");
    }

    public override async Task Concat_string_int_comparison4(bool async)
    {
        await base.Concat_string_int_comparison4(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[Int] AS nvarchar(max)) + [b].[String] = N'8Seattle'
""");
    }

    public override async Task Concat_string_string_comparison(bool async)
    {
        await base.Concat_string_string_comparison(async);

        AssertSql(
            """
@i='A' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE @i + [b].[String] = N'ASeattle'
""");
    }

    public override async Task Concat_method_comparison(bool async)
    {
        await base.Concat_method_comparison(async);

        AssertSql(
            """
@i='A' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE @i + [b].[String] = N'ASeattle'
""");
    }

    public override async Task Concat_method_comparison_2(bool async)
    {
        await base.Concat_method_comparison_2(async);

        AssertSql(
            """
@i='A' (Size = 4000)
@j='B' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE @i + @j + [b].[String] = N'ABSeattle'
""");
    }

    public override async Task Concat_method_comparison_3(bool async)
    {
        await base.Concat_method_comparison_3(async);

        AssertSql(
            """
@i='A' (Size = 4000)
@j='B' (Size = 4000)
@k='C' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE @i + @j + @k + [b].[String] = N'ABCSeattle'
""");
    }

    #endregion Concatenation

    #region LINQ Operators

    public override async Task FirstOrDefault(bool async)
    {
        await base.FirstOrDefault(async);
        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE SUBSTRING([b].[String], 1, 1) = N'S'
""");
    }

    public override async Task LastOrDefault(bool async)
    {
        await base.LastOrDefault(async);
        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE SUBSTRING([b].[String], LEN([b].[String]), 1) = N'e'
""");
    }

    #endregion LINQ Operators

    #region Like

    public override async Task Where_Like_and_comparison(bool async)
    {
        await base.Where_Like_and_comparison(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'S%' AND [b].[Int] = 8
""");
    }

    public override async Task Where_Like_or_comparison(bool async)
    {
        await base.Where_Like_or_comparison(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'S%' OR [b].[Int] = 2147483647
""");
    }

    public override async Task Like_with_non_string_column_using_ToString(bool async)
    {
        await base.Like_with_non_string_column_using_ToString(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CONVERT(varchar(11), [b].[Int]) LIKE '%5%'
""");
    }

    public override async Task Like_with_non_string_column_using_double_cast(bool async)
    {
        await base.Like_with_non_string_column_using_double_cast(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[Int] AS nvarchar(max)) LIKE N'%5%'
""");
    }

    #endregion Like

    #region Regex

    public override Task Regex_IsMatch(bool async)
        => AssertTranslationFailed(() => base.Regex_IsMatch(async));

    public override Task Regex_IsMatch_constant_input(bool async)
        => AssertTranslationFailed(() => base.Regex_IsMatch_constant_input(async));

    #endregion Regex

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
