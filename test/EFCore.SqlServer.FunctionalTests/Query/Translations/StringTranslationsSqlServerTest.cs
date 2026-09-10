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

    public override async Task Equals()
    {
        await base.Equals();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'seattle'
""");
    }

    public override async Task Equals_with_OrdinalIgnoreCase()
    {
        await base.Equals_with_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task Equals_with_Ordinal()
    {
        await base.Equals_with_Ordinal();

        AssertSql();
    }

    public override async Task Static_Equals()
    {
        await base.Static_Equals();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'seattle'
""");
    }

    public override async Task Static_Equals_with_OrdinalIgnoreCase()
    {
        await base.Static_Equals_with_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task Static_Equals_with_Ordinal()
    {
        await base.Static_Equals_with_Ordinal();

        AssertSql();
    }

    #endregion Equals

    #region Miscellaneous

    public override async Task Length()
    {
        await base.Length();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) = 7
""");
    }

    public override async Task ToUpper()
    {
        await base.ToUpper();

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

    public override async Task ToLower()
    {
        await base.ToLower();

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

    public override async Task IndexOf()
    {
        await base.IndexOf();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CHARINDEX(N'Eattl', [b].[String]) AS int) - 1 <> -1
""");
    }

    public override async Task IndexOf_Char()
    {
        await base.IndexOf_Char();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CHARINDEX('e', [b].[String]) AS int) - 1 <> -1
""");
    }

    public override async Task IndexOf_with_empty_string()
    {
        await base.IndexOf_with_empty_string();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task IndexOf_with_one_parameter_arg()
    {
        await base.IndexOf_with_one_parameter_arg();

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

    public override async Task IndexOf_with_one_parameter_arg_char()
    {
        await base.IndexOf_with_one_parameter_arg_char();

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

    public override async Task IndexOf_with_constant_starting_position()
    {
        await base.IndexOf_with_constant_starting_position();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX(N'e', [b].[String], 3) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_with_constant_starting_position_char()
    {
        await base.IndexOf_with_constant_starting_position_char();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX('e', [b].[String], 3) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_with_parameter_starting_position()
    {
        await base.IndexOf_with_parameter_starting_position();

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX(N'E', [b].[String], @start + 1) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_with_parameter_starting_position_char()
    {
        await base.IndexOf_with_parameter_starting_position_char();

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) > 2 AND CAST(CHARINDEX('e', [b].[String], @start + 1) AS int) - 1 = 6
""");
    }

    public override async Task IndexOf_after_ToString()
    {
        await base.IndexOf_after_ToString();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CHARINDEX('55', CONVERT(varchar(11), [b].[Int])) - 1 = 1
""");
    }

    public override async Task IndexOf_over_ToString()
    {
        await base.IndexOf_over_ToString();

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

    public override async Task Replace()
    {
        await base.Replace();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE REPLACE([b].[String], N'sea', N'rea') = N'reattle'
""");
    }

    public override async Task Replace_Char()
    {
        await base.Replace_Char();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE REPLACE([b].[String], 'S', 'R') = N'Reattle'
""");
    }

    public override async Task Replace_with_empty_string()
    {
        await base.Replace_with_empty_string();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> N'' AND REPLACE([b].[String], [b].[String], N'') = N''
""");
    }

    public override async Task Replace_using_property_arguments()
    {
        await base.Replace_using_property_arguments();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] <> N'' AND REPLACE([b].[String], [b].[String], CONVERT(varchar(11), [b].[Int])) = CONVERT(varchar(11), [b].[Int])
""");
    }

    #endregion Replace

    #region Substring

    public override async Task Substring()
    {
        await base.Substring();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 3 AND SUBSTRING([b].[String], 1 + 1, 2) = N'ea'
""");
    }

    public override async Task Substring_with_one_arg_with_zero_startIndex()
    {
        await base.Substring_with_one_arg_with_zero_startIndex();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE SUBSTRING([b].[String], 0 + 1, LEN([b].[String])) = N'Seattle'
""");
    }

    public override async Task Substring_with_one_arg_with_constant()
    {
        await base.Substring_with_one_arg_with_constant();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 1 AND SUBSTRING([b].[String], 1 + 1, LEN([b].[String])) = N'eattle'
""");
    }

    public override async Task Substring_with_one_arg_with_parameter()
    {
        await base.Substring_with_one_arg_with_parameter();

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 2 AND SUBSTRING([b].[String], @start + 1, LEN([b].[String])) = N'attle'
""");
    }

    public override async Task Substring_with_two_args_with_zero_startIndex()
    {
        await base.Substring_with_two_args_with_zero_startIndex();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 3 AND SUBSTRING([b].[String], 0 + 1, 3) = N'Sea'
""");
    }

    public override async Task Substring_with_two_args_with_zero_length()
    {
        await base.Substring_with_two_args_with_zero_length();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 2 AND SUBSTRING([b].[String], 2 + 1, 0) = N''
""");
    }

    public override async Task Substring_with_two_args_with_parameter()
    {
        await base.Substring_with_two_args_with_parameter();

        AssertSql(
            """
@start='2'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(LEN([b].[String]) AS int) >= 5 AND SUBSTRING([b].[String], @start + 1, 3) = N'att'
""");
    }

    public override async Task Substring_with_two_args_with_IndexOf()
    {
        await base.Substring_with_two_args_with_IndexOf();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%a%' AND SUBSTRING([b].[String], (CAST(CHARINDEX(N'a', [b].[String]) AS int) - 1) + 1, 3) = N'att'
""");
    }

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    public override async Task IsNullOrEmpty()
    {
        await base.IsNullOrEmpty();

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

    public override async Task IsNullOrEmpty_negated()
    {
        await base.IsNullOrEmpty_negated();

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

    public override async Task IsNullOrWhiteSpace()
    {
        await base.IsNullOrWhiteSpace();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N''
""");
    }

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    public override async Task StartsWith_Literal()
    {
        await base.StartsWith_Literal();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'se%'
""");
    }

    public override async Task StartsWith_Literal_Char()
    {
        await base.StartsWith_Literal_Char();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'S%'
""");
    }

    public override async Task StartsWith_Parameter()
    {
        await base.StartsWith_Parameter();

        AssertSql(
            """
@pattern_startswith='se%' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_startswith ESCAPE N'\'
""");
    }

    public override async Task StartsWith_Parameter_Char()
    {
        await base.StartsWith_Parameter_Char();

        AssertSql(
            """
@pattern_startswith='S%' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_startswith ESCAPE N'\'
""");
    }

    public override async Task StartsWith_Column()
    {
        await base.StartsWith_Column();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LEFT([b].[String], LEN([b].[String])) = [b].[String]
""");
    }

    public override async Task StartsWith_with_StringComparison_Ordinal()
    {
        await base.StartsWith_with_StringComparison_Ordinal();

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.StartsWith_with_StringComparison_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_unsupported()
    {
        await base.StartsWith_with_StringComparison_unsupported();

        AssertSql();
    }

    #endregion StartsWith

    #region EndsWith

    public override async Task EndsWith_Literal()
    {
        await base.EndsWith_Literal();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%Le'
""");
    }

    public override async Task EndsWith_Literal_Char()
    {
        await base.EndsWith_Literal_Char();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%e'
""");
    }

    public override async Task EndsWith_Parameter()
    {
        await base.EndsWith_Parameter();

        AssertSql(
            """
@pattern_endswith='%LE' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_endswith ESCAPE N'\'
""");
    }

    public override async Task EndsWith_Parameter_Char()
    {
        await base.EndsWith_Parameter_Char();

        AssertSql(
            """
@pattern_endswith='%e' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE @pattern_endswith ESCAPE N'\'
""");
    }

    public override async Task EndsWith_Column()
    {
        // SQL Server trims trailing whitespace for length calculations, making our EndsWith() column translation not work reliably in that
        // case
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String == "Seattle" && b.String.EndsWith(b.String)));

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] = N'Seattle' AND RIGHT([b].[String], LEN([b].[String])) = [b].[String]
""");
    }

    public override async Task EndsWith_with_StringComparison_Ordinal()
    {
        await base.EndsWith_with_StringComparison_Ordinal();

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.EndsWith_with_StringComparison_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_unsupported()
    {
        await base.EndsWith_with_StringComparison_unsupported();

        AssertSql();
    }

    #endregion EndsWith

    #region Contains

    public override async Task Contains_Literal()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl")), // SQL Server is case-insensitive by default
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.OrdinalIgnoreCase)));

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%eattl%'
""");
    }

    public override async Task Contains_Literal_Char()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains('e')));

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%e%'
""");
    }

    public override async Task Contains_Column()
    {
        await base.Contains_Column();

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

    public override async Task Contains_negated()
    {
        await base.Contains_negated();

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

    public override async Task Contains_with_StringComparison_Ordinal()
    {
        await base.Contains_with_StringComparison_Ordinal();

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.Contains_with_StringComparison_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_unsupported()
    {
        await base.Contains_with_StringComparison_unsupported();

        AssertSql();
    }

    public override async Task Contains_constant_with_whitespace()
    {
        await base.Contains_constant_with_whitespace();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'%     %'
""");
    }

    public override async Task Contains_parameter_with_whitespace()
    {
        await base.Contains_parameter_with_whitespace();

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

    public override async Task TrimStart_without_arguments()
    {
        await base.TrimStart_without_arguments();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LTRIM([b].[String]) = N'Boston  '
""");
    }

    public override Task TrimStart_with_char_argument()
        => AssertTranslationFailed(() => base.TrimStart_with_char_argument());

    public override Task TrimStart_with_char_array_argument()
        => AssertTranslationFailed(() => base.TrimStart_with_char_array_argument());

    #endregion TrimStart

    #region TrimEnd

    public override async Task TrimEnd_without_arguments()
    {
        await base.TrimEnd_without_arguments();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE RTRIM([b].[String]) = N'  Boston'
""");
    }

    public override Task TrimEnd_with_char_argument()
        => AssertTranslationFailed(() => base.TrimEnd_with_char_argument());

    public override Task TrimEnd_with_char_array_argument()
        => AssertTranslationFailed(() => base.TrimEnd_with_char_array_argument());

    #endregion TrimEnd

    #region Trim

    public override async Task Trim_without_argument_in_predicate()
    {
        await base.Trim_without_argument_in_predicate();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE LTRIM(RTRIM([b].[String])) = N'Boston'
""");
    }

    public override async Task Trim_with_char_argument_in_predicate()
    {
        // String.Trim with parameters. Issue #22927.
        await AssertTranslationFailed(() => base.Trim_with_char_argument_in_predicate());

        AssertSql();
    }

    public override async Task Trim_with_char_array_argument_in_predicate()
    {
        // String.Trim with parameters. Issue #22927.
        await AssertTranslationFailed(() => base.Trim_with_char_array_argument_in_predicate());

        AssertSql();
    }

    #endregion Trim

    #region Compare

    public override async Task Compare_simple_zero()
    {
        await base.Compare_simple_zero();

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

    public override async Task Compare_simple_one()
    {
        await base.Compare_simple_one();

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

    public override async Task Compare_with_parameter()
    {
        await base.Compare_with_parameter();

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

    public override async Task Compare_simple_more_than_one()
    {
        await base.Compare_simple_more_than_one();

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

    public override async Task Compare_nested()
    {
        await base.Compare_nested();

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

    public override async Task Compare_multi_predicate()
    {
        await base.Compare_multi_predicate();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] >= N'Seattle' AND [b].[String] < N'Toronto'
""");
    }

    public override async Task CompareTo_simple_zero()
    {
        await base.CompareTo_simple_zero();

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

    public override async Task CompareTo_simple_one()
    {
        await base.CompareTo_simple_one();

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

    public override async Task CompareTo_with_parameter()
    {
        await base.CompareTo_with_parameter();

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

    public override async Task CompareTo_simple_more_than_one()
    {
        await base.CompareTo_simple_more_than_one();

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

    public override async Task CompareTo_nested()
    {
        await base.CompareTo_nested();

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

    public override async Task Compare_to_multi_predicate()
    {
        await base.Compare_to_multi_predicate();

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
    public override async Task Join_over_non_nullable_column()
    {
        await base.Join_over_non_nullable_column();

        AssertSql(
            """
SELECT [b].[Int] AS [Key], COALESCE(STRING_AGG([b].[String], N'|'), N'') AS [Strings]
FROM [BasicTypesEntities] AS [b]
GROUP BY [b].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Join_over_nullable_column()
    {
        await base.Join_over_nullable_column();

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
    public override async Task Join_with_predicate()
    {
        await base.Join_with_predicate();

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
    public override async Task Join_with_ordering()
    {
        await base.Join_with_ordering();

        AssertSql(
            """
SELECT [b].[Int] AS [Key], COALESCE(STRING_AGG([b].[String], N'|') WITHIN GROUP (ORDER BY [b].[Id] DESC), N'') AS [Strings]
FROM [BasicTypesEntities] AS [b]
GROUP BY [b].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Join_non_aggregate()
    {
        await base.Join_non_aggregate();

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

    public override async Task Concat_operator()
    {
        await base.Concat_operator();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] + N'Boston' = N'SeattleBoston'
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2017)]
    public override async Task Concat_aggregate()
    {
        await base.Concat_aggregate();

        AssertSql(
            """
SELECT [b].[Int] AS [Key], COALESCE(STRING_AGG([b].[String], N''), N'') AS [BasicTypesEntitys]
FROM [BasicTypesEntities] AS [b]
GROUP BY [b].[Int]
""");
    }

    public override async Task Concat_string_int_comparison1()
    {
        await base.Concat_string_int_comparison1();

        AssertSql(
            """
@i='10'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] + CAST(@i AS nvarchar(max)) = N'Seattle10'
""");
    }

    public override async Task Concat_string_int_comparison2()
    {
        await base.Concat_string_int_comparison2();

        AssertSql(
            """
@i='10'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(@i AS nvarchar(max)) + [b].[String] = N'10Seattle'
""");
    }

    public override async Task Concat_string_int_comparison3()
    {
        await base.Concat_string_int_comparison3();

        AssertSql(
            """
@p='30'
@j='21'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(@p AS nvarchar(max)) + [b].[String] + CAST(@j AS nvarchar(max)) + CAST(42 AS nvarchar(max)) = N'30Seattle2142'
""");
    }

    public override async Task Concat_string_int_comparison4()
    {
        await base.Concat_string_int_comparison4();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[Int] AS nvarchar(max)) + [b].[String] = N'8Seattle'
""");
    }

    public override async Task Concat_string_string_comparison()
    {
        await base.Concat_string_string_comparison();

        AssertSql(
            """
@i='A' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE @i + [b].[String] = N'ASeattle'
""");
    }

    public override async Task Concat_method_comparison()
    {
        await base.Concat_method_comparison();

        AssertSql(
            """
@i='A' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE @i + [b].[String] = N'ASeattle'
""");
    }

    public override async Task Concat_method_comparison_2()
    {
        await base.Concat_method_comparison_2();

        AssertSql(
            """
@i='A' (Size = 4000)
@j='B' (Size = 4000)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE @i + @j + [b].[String] = N'ABSeattle'
""");
    }

    public override async Task Concat_method_comparison_3()
    {
        await base.Concat_method_comparison_3();

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

    public override async Task FirstOrDefault()
    {
        await base.FirstOrDefault();
        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE SUBSTRING([b].[String], 1, 1) = N'S'
""");
    }

    public override async Task LastOrDefault()
    {
        await base.LastOrDefault();
        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE SUBSTRING([b].[String], LEN([b].[String]), 1) = N'e'
""");
    }

    #endregion LINQ Operators

    #region Like

    public override async Task Where_Like_and_comparison()
    {
        await base.Where_Like_and_comparison();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'S%' AND [b].[Int] = 8
""");
    }

    public override async Task Where_Like_or_comparison()
    {
        await base.Where_Like_or_comparison();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[String] LIKE N'S%' OR [b].[Int] = 2147483647
""");
    }

    public override async Task Like_with_non_string_column_using_ToString()
    {
        await base.Like_with_non_string_column_using_ToString();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CONVERT(varchar(11), [b].[Int]) LIKE '%5%'
""");
    }

    public override async Task Like_with_non_string_column_using_double_cast()
    {
        await base.Like_with_non_string_column_using_double_cast();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[Int] AS nvarchar(max)) LIKE N'%5%'
""");
    }

    #endregion Like

    #region Regex

    public override Task Regex_IsMatch()
        => AssertTranslationFailed(() => base.Regex_IsMatch());

    public override Task Regex_IsMatch_constant_input()
        => AssertTranslationFailed(() => base.Regex_IsMatch_constant_input());

    #endregion Regex

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
