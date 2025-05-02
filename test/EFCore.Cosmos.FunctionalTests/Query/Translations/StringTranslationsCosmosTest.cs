// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class StringTranslationsCosmosTest : StringTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public StringTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Equals

    public override Task Equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Equals(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["String"] = "Seattle")
""");
            });

    public override Task Equals_with_OrdinalIgnoreCase(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Equals_with_OrdinalIgnoreCase(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "seattle", true)
""");
            });

    public override Task Equals_with_Ordinal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Equals_with_Ordinal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "Seattle")
""");
            });

    public override Task Static_Equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_Equals(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["String"] = "Seattle")
""");
            });

    public override Task Static_Equals_with_OrdinalIgnoreCase(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_Equals_with_OrdinalIgnoreCase(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "seattle", true)
""");
            });

    public override Task Static_Equals_with_Ordinal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_Equals_with_Ordinal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["String"], "Seattle")
""");
            });

    #endregion Equals

    #region Miscellaneous

    public override Task Length(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Length(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (LENGTH(c["String"]) = 7)
""");
            });

    public override Task ToUpper(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToUpper(a);

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
            });

    public override Task ToLower(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToLower(a);

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
            });

    #endregion Miscellaneous

    #region IndexOf

    public override Task IndexOf(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], "eattl") != -1)
""");
            });

    public override Task IndexOf_Char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_Char(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], "e") != -1)
""");
            });

    public override Task IndexOf_with_empty_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_with_empty_string(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], "") = 0)
""");
            });

    public override Task IndexOf_with_one_parameter_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_with_one_parameter_arg(a);

                AssertSql(
                    """
@pattern=?

SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], @pattern) = 1)
""");
            });

    public override Task IndexOf_with_one_parameter_arg_char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_with_one_parameter_arg_char(a);

                AssertSql(
                    """
@pattern=?

SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["String"], @pattern) = 1)
""");
            });

    public override Task IndexOf_with_constant_starting_position(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_with_constant_starting_position(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", 2) = 6))
""");
            });

    public override Task IndexOf_with_constant_starting_position_char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_with_constant_starting_position_char(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", 2) = 6))
""");
            });

    public override Task IndexOf_with_parameter_starting_position(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_with_parameter_starting_position(a);

                AssertSql(
                    """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", @start) = 6))
""");
            });

    public override Task IndexOf_with_parameter_starting_position_char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IndexOf_with_parameter_starting_position_char(a);

                AssertSql(
                    """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) > 2) AND (INDEX_OF(c["String"], "e", @start) = 6))
""");
            });

    public override Task IndexOf_after_ToString(bool async)
        => AssertTranslationFailed(() => base.IndexOf_after_ToString(async));

    public override Task IndexOf_over_ToString(bool async)
        => AssertTranslationFailed(() => base.IndexOf_over_ToString(async));

    #endregion IndexOf

    #region Replace

    public override Task Replace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Replace(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (REPLACE(c["String"], "Sea", "Rea") = "Reattle")
""");
            });

    public override Task Replace_Char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Replace_Char(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (REPLACE(c["String"], "S", "R") = "Reattle")
""");
            });

    public override Task Replace_with_empty_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Replace_with_empty_string(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["String"] != "") AND (REPLACE(c["String"], c["String"], "") = ""))
""");
            });

    public override async Task Replace_using_property_arguments(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Replace_using_property_arguments(async));

        AssertSql();
    }

    #endregion Replace

    #region Substring

    public override Task Substring(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 3) AND (SUBSTRING(c["String"], 1, 2) = "ea"))
""");
            });

    public override Task Substring_with_one_arg_with_zero_startIndex(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_one_arg_with_zero_startIndex(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (SUBSTRING(c["String"], 0, LENGTH(c["String"])) = "Seattle")
""");
            });

    public override Task Substring_with_one_arg_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_one_arg_with_constant(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 1) AND (SUBSTRING(c["String"], 1, LENGTH(c["String"])) = "eattle"))
""");
            });

    public override Task Substring_with_one_arg_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_one_arg_with_parameter(a);

                AssertSql(
                    """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 2) AND (SUBSTRING(c["String"], @start, LENGTH(c["String"])) = "attle"))
""");
            });

    public override Task Substring_with_two_args_with_zero_startIndex(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_zero_startIndex(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 3) AND (LEFT(c["String"], 3) = "Sea"))
""");
            });

    public override Task Substring_with_two_args_with_zero_length(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_zero_length(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 2) AND (SUBSTRING(c["String"], 2, 0) = ""))
""");
            });

    public override Task Substring_with_two_args_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_parameter(a);

                AssertSql(
                    """
@start=?

SELECT VALUE c
FROM root c
WHERE ((LENGTH(c["String"]) >= 5) AND (SUBSTRING(c["String"], @start, 3) = "att"))
""");
            });

    public override Task Substring_with_two_args_with_IndexOf(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Substring_with_two_args_with_IndexOf(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (CONTAINS(c["String"], "a") AND (SUBSTRING(c["String"], INDEX_OF(c["String"], "a"), 3) = "att"))
""");
            });

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    public override async Task IsNullOrEmpty(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrEmpty(async));

        AssertSql();
    }

    public override async Task IsNullOrEmpty_negated(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrEmpty_negated(async));

        AssertSql();
    }

    public override async Task IsNullOrWhiteSpace(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.IsNullOrWhiteSpace(async));

        AssertSql();
    }

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    public override Task StartsWith_Literal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.StartsWith_Literal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "Se")
""");
            });

    public override Task StartsWith_Literal_Char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.StartsWith_Literal_Char(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "S")
""");
            });

    public override Task StartsWith_Parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.StartsWith_Parameter(a);

                AssertSql(
                    """
@pattern=?

SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], @pattern)
""");
            });

    public override Task StartsWith_Parameter_Char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.StartsWith_Parameter_Char(a);

                AssertSql(
                    """
@pattern=?

SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], @pattern)
""");
            });

    public override Task StartsWith_Column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.StartsWith_Column(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], c["String"])
""");
            });

    public override Task StartsWith_with_StringComparison_Ordinal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.StartsWith_with_StringComparison_Ordinal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "Se", false)
""");
            });

    public override Task StartsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.StartsWith_with_StringComparison_OrdinalIgnoreCase(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STARTSWITH(c["String"], "Se", true)
""");
            });

    public override async Task StartsWith_with_StringComparison_unsupported(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.StartsWith_with_StringComparison_unsupported(async);
        }
    }

    #endregion StartsWith

    #region EndsWith

    public override Task EndsWith_Literal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EndsWith_Literal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "le")
""");
            });

    public override Task EndsWith_Literal_Char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EndsWith_Literal_Char(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "e")
""");
            });

    public override Task EndsWith_Parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EndsWith_Parameter(a);

                AssertSql(
                    """
@pattern=?

SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], @pattern)
""");
            });

    public override Task EndsWith_Parameter_Char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EndsWith_Parameter_Char(a);

                AssertSql(
                    """
@pattern=?

SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], @pattern)
""");
            });

    public override Task EndsWith_Column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EndsWith_Column(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], c["String"])
""");
            });

    public override Task EndsWith_with_StringComparison_Ordinal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EndsWith_with_StringComparison_Ordinal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "le", false)
""");
            });

    public override Task EndsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EndsWith_with_StringComparison_OrdinalIgnoreCase(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ENDSWITH(c["String"], "LE", true)
""");
            });

    public override async Task EndsWith_with_StringComparison_unsupported(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.EndsWith_with_StringComparison_unsupported(async);
        }
    }

    #endregion EndsWith

    #region Contains

    public override Task Contains_Literal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_Literal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "eattl")
""");
            });

    public override Task Contains_Literal_Char(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_Literal_Char(a);

                AssertSql(
                    """
    SELECT VALUE c
    FROM root c
    WHERE CONTAINS(c["String"], "e")
    """);
            });

    public override Task Contains_Column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_Column(a);

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
            });

    public override Task Contains_negated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_negated(a);

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
            });

    public override Task Contains_with_StringComparison_Ordinal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_StringComparison_Ordinal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "eattl", false)
""");
            });

    public override Task Contains_with_StringComparison_OrdinalIgnoreCase(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_StringComparison_OrdinalIgnoreCase(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "EATTL", true)
""");
            });

    public override async Task Contains_with_StringComparison_unsupported(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Contains_with_StringComparison_unsupported(async);
        }
    }

    public override Task Contains_constant_with_whitespace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_constant_with_whitespace(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], "     ")
""");
            });

    public override Task Contains_parameter_with_whitespace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_parameter_with_whitespace(a);

                AssertSql(
                    """
@pattern=?

SELECT VALUE c
FROM root c
WHERE CONTAINS(c["String"], @pattern)
""");
            });

    #endregion Contains

    #region TrimStart

    public override Task TrimStart_without_arguments(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.TrimStart_without_arguments(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (LTRIM(c["String"]) = "Boston  ")
""");
            });

    public override async Task TrimStart_with_char_argument(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimStart_with_char_argument(async));

        AssertSql();
    }

    public override async Task TrimStart_with_char_array_argument(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimStart_with_char_array_argument(async));

        AssertSql();
    }

    #endregion TrimStart

    #region TrimEnd

    public override Task TrimEnd_without_arguments(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.TrimEnd_without_arguments(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (RTRIM(c["String"]) = "  Boston")
""");
            });

    public override async Task TrimEnd_with_char_argument(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimEnd_with_char_argument(async));

        AssertSql();
    }

    public override async Task TrimEnd_with_char_array_argument(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TrimEnd_with_char_array_argument(async));

        AssertSql();
    }

    #endregion TrimEnd

    #region Trim

    public override Task Trim_without_argument_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Trim_without_argument_in_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (TRIM(c["String"]) = "Boston")
""");
            });

    public override async Task Trim_with_char_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Trim_with_char_argument_in_predicate(async));

        AssertSql();
    }

    public override async Task Trim_with_char_array_argument_in_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Trim_with_char_array_argument_in_predicate(async));

        AssertSql();
    }

    #endregion Trim

    #region Compare

    public override async Task Compare_simple_zero(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_simple_zero(async));

        AssertSql();
    }

    public override async Task Compare_simple_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_simple_one(async));

        AssertSql();
    }

    public override async Task Compare_with_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_with_parameter(async));

        AssertSql(
            """
ReadItem(?, ?)
""");
    }

    public override async Task Compare_simple_more_than_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_simple_more_than_one(async));

        AssertSql();
    }

    public override async Task Compare_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_nested(async));

        AssertSql();
    }

    public override async Task Compare_multi_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_multi_predicate(async));

        AssertSql();
    }

    public override async Task CompareTo_simple_zero(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_simple_zero(async));

        AssertSql();
    }

    public override async Task CompareTo_simple_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_simple_one(async));

        AssertSql();
    }

    public override async Task CompareTo_with_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_with_parameter(async));

        AssertSql(
            """
ReadItem(?, ?)
""");
    }

    public override async Task CompareTo_simple_more_than_one(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_simple_more_than_one(async));

        AssertSql();
    }

    public override async Task CompareTo_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.CompareTo_nested(async));

        AssertSql();
    }

    public override async Task Compare_to_multi_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_to_multi_predicate(async));

        AssertSql();
    }

    #endregion Compare

    #region Join

    public override Task Join_over_non_nullable_column(bool async)
        => AssertTranslationFailed(() => base.Join_over_non_nullable_column(async));

    public override Task Join_with_predicate(bool async)
        => AssertTranslationFailed(() => base.Join_with_predicate(async));

    public override Task Join_with_ordering(bool async)
        => AssertTranslationFailed(() => base.Join_with_ordering(async));

    public override Task Join_over_nullable_column(bool async)
        => AssertTranslationFailed(() => base.Join_over_nullable_column(async));

    public override Task Join_non_aggregate(bool async)
        => AssertTranslationFailed(() => base.Join_non_aggregate(async));

    #endregion Join

    #region Concatenation

    public override Task Concat_operator(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_operator(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["String"] || "Boston") = "SeattleBoston")
""");
            });

    public override Task Concat_aggregate(bool async)
        => AssertTranslationFailed(() => base.Concat_aggregate(async));

    public override Task Concat_string_string_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_string_string_comparison(a);

                AssertSql(
                    """
@i=?

SELECT VALUE c
FROM root c
WHERE ((@i || c["String"]) = "ASeattle")
""");
            });

    public override async Task Concat_string_int_comparison1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison1(async));

        AssertSql();
    }

    public override async Task Concat_string_int_comparison2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison2(async));

        AssertSql();
    }

    public override async Task Concat_string_int_comparison3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison3(async));

        AssertSql();
    }

    public override async Task Concat_string_int_comparison4(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Concat_string_int_comparison4(async));

        AssertSql(
        );
    }

    public override Task Concat_method_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_method_comparison(a);

                AssertSql(
                    """
@i=?

SELECT VALUE c
FROM root c
WHERE ((@i || c["String"]) = "ASeattle")
""");
            });

    public override Task Concat_method_comparison_2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_method_comparison_2(a);

                AssertSql(
                    """
@i=?
@j=?

SELECT VALUE c
FROM root c
WHERE ((@i || (@j || c["String"])) = "ABSeattle")
""");
            });

    public override Task Concat_method_comparison_3(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_method_comparison_3(a);

                AssertSql(
                    """
@i=?
@j=?
@k=?

SELECT VALUE c
FROM root c
WHERE ((@i || (@j || (@k || c["String"]))) = "ABCSeattle")
""");
            });

    #endregion Concatenation

    #region LINQ Operators

    public override Task FirstOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.FirstOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (LEFT(c["String"], 1) = "S")
""");
            });

    public override Task LastOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.LastOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (RIGHT(c["String"], 1) = "e")
""");
            });

    #endregion LINQ Operators

    #region Regex

    public override Task Regex_IsMatch(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Regex_IsMatch(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE RegexMatch(c["String"], "^S")
""");
            });

    public override Task Regex_IsMatch_constant_input(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Regex_IsMatch_constant_input(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE RegexMatch("Seattle", c["String"])
""");
            });

//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_None(bool async)
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
//             });
//
//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_IgnoreCase(bool async)
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
//             });
//
//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_Multiline(bool async)
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
//             });
//
//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_Singleline(bool async)
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
//             });
//
//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_IgnorePatternWhitespace(bool async)
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
//             });
//
//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_IgnoreCase_and_IgnorePatternWhitespace(bool async)
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
//             });
//
//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_RightToLeft(bool async)
//         => AssertTranslationFailed(
//             () => AssertQuery(
//                 async,
//                 ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T", RegexOptions.RightToLeft))));
//
//     [ConditionalTheory]
//     [MemberData(nameof(IsAsyncData))]
//     public virtual Task Regex_IsMatch_with_RegexOptions_IgnoreCase_and_RightToLeft(bool async)
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
