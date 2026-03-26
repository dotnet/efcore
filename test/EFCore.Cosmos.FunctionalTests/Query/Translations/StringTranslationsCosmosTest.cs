// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class StringTranslationsCosmosTest : StringTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public StringTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Equals

    public override async Task Equals()
    {
        await base.Equals();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["String"] = "Seattle")
""");
    }

    public override async Task Equals_with_OrdinalIgnoreCase()
    {
        await base.Equals_with_OrdinalIgnoreCase();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "seattle", true)
""");
    }

    public override async Task Equals_with_Ordinal()
    {
        await base.Equals_with_Ordinal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "Seattle")
""");
    }

    public override async Task Static_Equals()
    {
        await base.Static_Equals();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["String"] = "Seattle")
""");
    }

    public override async Task Static_Equals_with_OrdinalIgnoreCase()
    {
        await base.Static_Equals_with_OrdinalIgnoreCase();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "seattle", true)
""");
    }

    public override async Task Static_Equals_with_Ordinal()
    {
        await base.Static_Equals_with_Ordinal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "Seattle")
""");
    }

    #endregion Equals

    #region Miscellaneous

    public override async Task Length()
    {
        await base.Length();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (LENGTH(c["String"]) = 7)
""");
    }

    public override async Task ToUpper()
    {
        await base.ToUpper();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (UPPER(c["String"]) = "SEATTLE")
""",
            //
            """
SELECT VALUE UPPER(c["String"])
FROM root c
""");
    }

    public override async Task ToLower()
    {
        await base.ToLower();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (LOWER(c["String"]) = "seattle")
""",
            //
            """
SELECT VALUE LOWER(c["String"])
FROM root c
""");
    }

    #endregion Miscellaneous

    #region IndexOf

    public override async Task IndexOf()
    {
        await base.IndexOf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], "eattl") != -1)
""");
    }

    public override async Task IndexOf_Char()
    {
        await base.IndexOf_Char();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], "e") != -1)
""");
    }

    public override async Task IndexOf_with_empty_string()
    {
        await base.IndexOf_with_empty_string();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], "") = 0)
""");
    }

    public override async Task IndexOf_with_one_parameter_arg()
    {
        await base.IndexOf_with_one_parameter_arg();

        AssertSql(
            """
@pattern=?

SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], @pattern) = 1)
""");
    }

    public override async Task IndexOf_with_one_parameter_arg_char()
    {
        await base.IndexOf_with_one_parameter_arg_char();

        AssertSql(
            """
@pattern=?

SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], @pattern) = 1)
""");
    }

    public override async Task IndexOf_with_constant_starting_position()
    {
        await base.IndexOf_with_constant_starting_position();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", 2) = 6))
""");
    }

    public override async Task IndexOf_with_constant_starting_position_char()
    {
        await base.IndexOf_with_constant_starting_position_char();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", 2) = 6))
""");
    }

    public override async Task IndexOf_with_parameter_starting_position()
    {
        await base.IndexOf_with_parameter_starting_position();

        AssertSql(
            """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", @start) = 6))
""");
    }

    public override async Task IndexOf_with_parameter_starting_position_char()
    {
        await base.IndexOf_with_parameter_starting_position_char();

        AssertSql(
            """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", @start) = 6))
""");
    }

    public override Task IndexOf_after_ToString()
        => AssertTranslationFailed(() => base.IndexOf_after_ToString());

    public override Task IndexOf_over_ToString()
        => AssertTranslationFailed(() => base.IndexOf_over_ToString());

    #endregion IndexOf

    #region Replace

    public override async Task Replace()
    {
        await base.Replace();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (REPLACE(c["String"], "Sea", "Rea") = "Reattle")
""");
    }

    public override async Task Replace_Char()
    {
        await base.Replace_Char();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (REPLACE(c["String"], "S", "R") = "Reattle")
""");
    }

    public override async Task Replace_with_empty_string()
    {
        await base.Replace_with_empty_string();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["String"] != "") AND (REPLACE(c["String"], c["String"], "") = ""))
""");
    }

    public override async Task Replace_using_property_arguments()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Replace_using_property_arguments());

        AssertSql();
    }

    #endregion Replace

    #region Substring

    public override async Task Substring()
    {
        await base.Substring();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 3) AND (SUBSTRING(c["String"], 1, 2) = "ea"))
""");
    }

    public override async Task Substring_with_one_arg_with_zero_startIndex()
    {
        await base.Substring_with_one_arg_with_zero_startIndex();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (SUBSTRING(c["String"], 0, LENGTH(c["String"])) = "Seattle")
""");
    }

    public override async Task Substring_with_one_arg_with_constant()
    {
        await base.Substring_with_one_arg_with_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 1) AND (SUBSTRING(c["String"], 1, LENGTH(c["String"])) = "eattle"))
""");
    }

    public override async Task Substring_with_one_arg_with_parameter()
    {
        await base.Substring_with_one_arg_with_parameter();

        AssertSql(
            """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 2) AND (SUBSTRING(c["String"], @start, LENGTH(c["String"])) = "attle"))
""");
    }

    public override async Task Substring_with_two_args_with_zero_startIndex()
    {
        await base.Substring_with_two_args_with_zero_startIndex();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 3) AND (LEFT(c["String"], 3) = "Sea"))
""");
    }

    public override async Task Substring_with_two_args_with_zero_length()
    {
        await base.Substring_with_two_args_with_zero_length();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 2) AND (SUBSTRING(c["String"], 2, 0) = ""))
""");
    }

    public override async Task Substring_with_two_args_with_parameter()
    {
        await base.Substring_with_two_args_with_parameter();

        AssertSql(
            """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 5) AND (SUBSTRING(c["String"], @start, 3) = "att"))
""");
    }

    public override async Task Substring_with_two_args_with_IndexOf()
    {
        await base.Substring_with_two_args_with_IndexOf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (CONTAINS(c["String"], "a") AND (SUBSTRING(c["String"], INDEX_OF(c["String"], "a"), 3) = "att"))
""");
    }

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    public override async Task IsNullOrEmpty()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrEmpty());

        AssertSql();
    }

    public override async Task IsNullOrEmpty_negated()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrEmpty_negated());

        AssertSql();
    }

    public override async Task IsNullOrWhiteSpace()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrWhiteSpace());

        AssertSql();
    }

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    public override async Task StartsWith_Literal()
    {
        await base.StartsWith_Literal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "Se")
""");
    }

    public override async Task StartsWith_Literal_Char()
    {
        await base.StartsWith_Literal_Char();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "S")
""");
    }

    public override async Task StartsWith_Parameter()
    {
        await base.StartsWith_Parameter();

        AssertSql(
            """
@pattern=?

SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], @pattern)
""");
    }

    public override async Task StartsWith_Parameter_Char()
    {
        await base.StartsWith_Parameter_Char();

        AssertSql(
            """
@pattern=?

SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], @pattern)
""");
    }

    public override async Task StartsWith_Column()
    {
        await base.StartsWith_Column();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], c["String"])
""");
    }

    public override async Task StartsWith_with_StringComparison_Ordinal()
    {
        await base.StartsWith_with_StringComparison_Ordinal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "Se", false)
""");
    }

    public override async Task StartsWith_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.StartsWith_with_StringComparison_OrdinalIgnoreCase();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "Se", true)
""");
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
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "le")
""");
    }

    public override async Task EndsWith_Literal_Char()
    {
        await base.EndsWith_Literal_Char();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "e")
""");
    }

    public override async Task EndsWith_Parameter()
    {
        await base.EndsWith_Parameter();

        AssertSql(
            """
@pattern=?

SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], @pattern)
""");
    }

    public override async Task EndsWith_Parameter_Char()
    {
        await base.EndsWith_Parameter_Char();

        AssertSql(
            """
@pattern=?

SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], @pattern)
""");
    }

    public override async Task EndsWith_Column()
    {
        await base.EndsWith_Column();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], c["String"])
""");
    }

    public override async Task EndsWith_with_StringComparison_Ordinal()
    {
        await base.EndsWith_with_StringComparison_Ordinal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "le", false)
""");
    }

    public override async Task EndsWith_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.EndsWith_with_StringComparison_OrdinalIgnoreCase();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "LE", true)
""");
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
        await base.Contains_Literal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "eattl")
""");
    }

    public override async Task Contains_Literal_Char()
    {
        await base.Contains_Literal_Char();

        AssertSql(
            """
    SELECT VALUE c
    FROM root c
    WHERE CONTAINS(c["String"], "e")
    """);
    }

    public override async Task Contains_Column()
    {
        await base.Contains_Column();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], c["String"])
""",
            //
            """
SELECT VALUE CONTAINS(c["String"], c["String"])
FROM root c
""");
    }

    public override async Task Contains_negated()
    {
        await base.Contains_negated();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE NOT(CONTAINS(c["String"], "eattle"))
""",
            //
            """
SELECT VALUE NOT(CONTAINS(c["String"], "eattle"))
FROM root c
""");
    }

    public override async Task Contains_with_StringComparison_Ordinal()
    {
        await base.Contains_with_StringComparison_Ordinal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "eattl", false)
""");
    }

    public override async Task Contains_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.Contains_with_StringComparison_OrdinalIgnoreCase();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "EATTL", true)
""");
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
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "     ")
""");
    }

    public override async Task Contains_parameter_with_whitespace()
    {
        await base.Contains_parameter_with_whitespace();

        AssertSql(
            """
@pattern=?

SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], @pattern)
""");
    }

    #endregion Contains

    #region TrimStart

    public override async Task TrimStart_without_arguments()
    {
        await base.TrimStart_without_arguments();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (LTRIM(c["String"]) = "Boston  ")
""");
    }

    public override async Task TrimStart_with_char_argument()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimStart_with_char_argument());

        AssertSql();
    }

    public override async Task TrimStart_with_char_array_argument()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimStart_with_char_array_argument());

        AssertSql();
    }

    #endregion TrimStart

    #region TrimEnd

    public override async Task TrimEnd_without_arguments()
    {
        await base.TrimEnd_without_arguments();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (RTRIM(c["String"]) = "  Boston")
""");
    }

    public override async Task TrimEnd_with_char_argument()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimEnd_with_char_argument());

        AssertSql();
    }

    public override async Task TrimEnd_with_char_array_argument()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimEnd_with_char_array_argument());

        AssertSql();
    }

    #endregion TrimEnd

    #region Trim

    public override async Task Trim_without_argument_in_predicate()
    {
        await base.Trim_without_argument_in_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (TRIM(c["String"]) = "Boston")
""");
    }

    public override async Task Trim_with_char_argument_in_predicate()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Trim_with_char_argument_in_predicate());

        AssertSql();
    }

    public override async Task Trim_with_char_array_argument_in_predicate()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Trim_with_char_array_argument_in_predicate());

        AssertSql();
    }

    #endregion Trim

    #region Compare

    public override async Task Compare_simple_zero()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_simple_zero());

        AssertSql();
    }

    public override async Task Compare_simple_one()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_simple_one());

        AssertSql();
    }

    public override async Task Compare_with_parameter()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_with_parameter());

        AssertSql(
            """
ReadItem(?, ?)
""");
    }

    public override async Task Compare_simple_more_than_one()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_simple_more_than_one());

        AssertSql();
    }

    public override async Task Compare_nested()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_nested());

        AssertSql();
    }

    public override async Task Compare_multi_predicate()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_multi_predicate());

        AssertSql();
    }

    public override async Task CompareTo_simple_zero()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_simple_zero());

        AssertSql();
    }

    public override async Task CompareTo_simple_one()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_simple_one());

        AssertSql();
    }

    public override async Task CompareTo_with_parameter()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_with_parameter());

        AssertSql(
            """
ReadItem(?, ?)
""");
    }

    public override async Task CompareTo_simple_more_than_one()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_simple_more_than_one());

        AssertSql();
    }

    public override async Task CompareTo_nested()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_nested());

        AssertSql();
    }

    public override async Task Compare_to_multi_predicate()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_to_multi_predicate());

        AssertSql();
    }

    #endregion Compare

    #region Join

    public override Task Join_over_non_nullable_column()
        => AssertTranslationFailed(() => base.Join_over_non_nullable_column());

    public override Task Join_with_predicate()
        => AssertTranslationFailed(() => base.Join_with_predicate());

    public override Task Join_with_ordering()
        => AssertTranslationFailed(() => base.Join_with_ordering());

    public override Task Join_over_nullable_column()
        => AssertTranslationFailed(() => base.Join_over_nullable_column());

    public override Task Join_non_aggregate()
        => AssertTranslationFailed(() => base.Join_non_aggregate());

    #endregion Join

    #region Concatenation

    public override async Task Concat_operator()
    {
        await base.Concat_operator();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["String"] || "Boston") = "SeattleBoston")
""");
    }

    public override Task Concat_aggregate()
        => AssertTranslationFailed(() => base.Concat_aggregate());

    public override async Task Concat_string_string_comparison()
    {
        await base.Concat_string_string_comparison();

        AssertSql(
            """
@i=?

SELECT VALUE c
FROM root c
WHERE ((@i || c["String"]) = "ASeattle")
""");
    }

    public override async Task Concat_string_int_comparison1()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison1());

        AssertSql();
    }

    public override async Task Concat_string_int_comparison2()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison2());

        AssertSql();
    }

    public override async Task Concat_string_int_comparison3()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison3());

        AssertSql();
    }

    public override async Task Concat_string_int_comparison4()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison4());

        AssertSql(
        );
    }

    public override async Task Concat_method_comparison()
    {
        await base.Concat_method_comparison();

        AssertSql(
            """
@i=?

SELECT VALUE c
FROM root c
WHERE ((@i || c["String"]) = "ASeattle")
""");
    }

    public override async Task Concat_method_comparison_2()
    {
        await base.Concat_method_comparison_2();

        AssertSql(
            """
@i=?
@j=?

SELECT VALUE c
FROM root c
WHERE ((@i || (@j || c["String"])) = "ABSeattle")
""");
    }

    public override async Task Concat_method_comparison_3()
    {
        await base.Concat_method_comparison_3();

        AssertSql(
            """
@i=?
@j=?
@k=?

SELECT VALUE c
FROM root c
WHERE ((@i || (@j || (@k || c["String"]))) = "ABCSeattle")
""");
    }

    #endregion Concatenation

    #region LINQ Operators

    public override async Task FirstOrDefault()
    {
        await base.FirstOrDefault();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (LEFT(c["String"], 1) = "S")
""");
    }

    public override async Task LastOrDefault()
    {
        await base.LastOrDefault();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (RIGHT(c["String"], 1) = "e")
""");
    }

    #endregion LINQ Operators

    #region Regex

    public override async Task Regex_IsMatch()
    {
        await base.Regex_IsMatch();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE RegexMatch(c["String"], "^S")
""");
    }

    public override async Task Regex_IsMatch_constant_input()
    {
        await base.Regex_IsMatch_constant_input();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE RegexMatch("Seattle", c["String"])
""");
    }

    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_None()
    //         => Fixture.NoSyncTest(
    //             async, async a =>
    //             {
    //                 await AssertQuery(
    //                     async,
    //                     ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.None)));
    //
    //                 AssertSql(
    //                     """
    // SELECT VALUE c
    // FROM root c
    // WHERE RegexMatch(c["id"], "^T")
    // """);
    //             }
    //
    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_IgnoreCase()
    //         => Fixture.NoSyncTest(
    //             async, async a =>
    //             {
    //                 await AssertQuery(
    //                     async,
    //                     ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnoreCase)));
    //
    //                 AssertSql(
    //                     """
    // SELECT VALUE c
    // FROM root c
    // WHERE RegexMatch(c["id"], "^T", "i")
    // """);
    //             }
    //
    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_Multiline()
    //         => Fixture.NoSyncTest(
    //             async, async a =>
    //             {
    //                 await AssertQuery(
    //                     async,
    //                     ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.Multiline)));
    //
    //                 AssertSql(
    //                     """
    // SELECT VALUE c
    // FROM root c
    // WHERE RegexMatch(c["id"], "^T", "m")
    // """);
    //             }
    //
    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_Singleline()
    //         => Fixture.NoSyncTest(
    //             async, async a =>
    //             {
    //                 await AssertQuery(
    //                     async,
    //                     ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.Singleline)));
    //
    //                 AssertSql(
    //                     """
    // SELECT VALUE c
    // FROM root c
    // WHERE RegexMatch(c["id"], "^T", "s")
    // """);
    //             }
    //
    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_IgnorePatternWhitespace()
    //         => Fixture.NoSyncTest(
    //             async, async a =>
    //             {
    //                 await AssertQuery(
    //                     async,
    //                     ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnorePatternWhitespace)));
    //
    //                 AssertSql(
    //                     """
    // SELECT VALUE c
    // FROM root c
    // WHERE RegexMatch(c["id"], "^T", "x")
    // """);
    //             }
    //
    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_IgnoreCase_and_IgnorePatternWhitespace()
    //         => Fixture.NoSyncTest(
    //             async, async a =>
    //             {
    //                 await AssertQuery(
    //                     async,
    //                     ss => ss.Set<Customer>().Where(
    //                         o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace)));
    //
    //                 AssertSql(
    //                     """
    // SELECT VALUE c
    // FROM root c
    // WHERE RegexMatch(c["id"], "^T", "ix")
    // """);
    //             }
    //
    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_RightToLeft()
    //         => AssertTranslationFailed(
    //             () => AssertQuery(
    //                 async,
    //                 ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.RightToLeft))));
    //
    //     [ConditionalTheory]
    //     [MemberData(nameof(IsAsyncData))]
    //     public virtual Task Regex_IsMatch_with_RegexOptions_IgnoreCase_and_RightToLeft()
    //         => AssertTranslationFailed(
    //             () => AssertQuery(
    //                 async,
    //                 ss => ss.Set<Customer>()
    //                     .Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.IgnoreCase | RegexOptions.RightToLeft))));

    #endregion Regex

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
