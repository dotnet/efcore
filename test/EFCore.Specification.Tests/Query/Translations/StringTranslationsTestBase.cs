// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

// See additional string tests for special values in FunkyDataQueryTestBase
public abstract class StringTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    /// <summary>
    ///     Controls whether the tests assert case-sensitive or insensitive string comparisons. Defaults to <see langword="true" />.
    /// </summary>
    protected virtual bool IsCaseSensitive
        => true;

    #region Equals

    [ConditionalFact]
    public virtual Task Equals()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("Seattle")))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual Task Equals_with_OrdinalIgnoreCase()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual Task Equals_with_Ordinal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("Seattle", StringComparison.Ordinal)));

    [ConditionalFact]
    public virtual Task Static_Equals()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "Seattle")))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual Task Static_Equals_with_OrdinalIgnoreCase()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual Task Static_Equals_with_Ordinal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "Seattle", StringComparison.Ordinal)));

    #endregion Equals

    #region Miscellaneous

    [ConditionalFact]
    public virtual Task Length()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length == 7));

    [ConditionalFact]
    public virtual async Task ToUpper()
    {
        // Note that if the database is case-insensitive, the Where() assertion checks nothing.
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.ToUpper() == "SEATTLE"));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Select(b => b.String.ToUpper()));
    }

    [ConditionalFact]
    public virtual async Task ToLower()
    {
        // Note that if the database is case-insensitive, the Where() assertion checks nothing.
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.ToLower() == "seattle"));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Select(b => b.String.ToLower()));
    }

    #endregion Miscellaneous

    #region IndexOf

    [ConditionalFact]
    public virtual Task IndexOf()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf("eattl") != -1))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf("Eattl") != -1),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf("Eattl", StringComparison.OrdinalIgnoreCase) != -1));

    [ConditionalFact]
    public virtual Task IndexOf_Char()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf('e') != -1))
            : AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf('e') != -1));

    [ConditionalFact]
    public virtual Task IndexOf_with_empty_string()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(string.Empty) == 0),
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String != null && b.String.IndexOf(string.Empty) == 0));

    [ConditionalFact]
    public virtual Task IndexOf_with_one_parameter_arg()
    {
        if (IsCaseSensitive)
        {
            var pattern = "eattl";

            return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1));
        }
        else
        {
            var pattern = "Eattl";

            return AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) == 1));
        }
    }

    [ConditionalFact]
    public virtual Task IndexOf_with_one_parameter_arg_char()
    {
        if (IsCaseSensitive)
        {
            var pattern = 'e';

            return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1));
        }
        else
        {
            var pattern = 'e';

            return AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) == 1));
        }
    }

    [ConditionalFact]
    public virtual Task IndexOf_with_constant_starting_position()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf("e", 2) == 6));

    [ConditionalFact]
    public virtual Task IndexOf_with_constant_starting_position_char()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf('e', 2) == 6));

#pragma warning disable CA1866 // Use 'string.Method(char)' instead of 'string.Method(string)' for string with single char
    [ConditionalFact]
    public virtual Task IndexOf_with_parameter_starting_position()
    {
        var start = 2;

        return IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf("e", start) == 6))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf("E", start) == 6),
                ss => ss.Set<BasicTypesEntity>().Where(b
                    => b.String.Length > 2 && b.String.IndexOf("E", start, StringComparison.OrdinalIgnoreCase) == 6));
    }
#pragma warning restore CA1866

    [ConditionalFact]
    public virtual Task IndexOf_with_parameter_starting_position_char()
    {
        var start = 2;

        return IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf('e', start) == 6))
            : AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf('e', start) == 6));
    }

    [ConditionalFact]
    public virtual Task IndexOf_after_ToString()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => x.Int.ToString().IndexOf("55") == 1));

    [ConditionalFact]
    public virtual Task IndexOf_over_ToString()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => "12559".IndexOf(x.Int.ToString()) == 1));

    #endregion IndexOf

    #region Replace

    [ConditionalFact]
    public virtual Task Replace()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Replace("Sea", "Rea") == "Reattle"))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Replace("sea", "rea") == "reattle"),
                ss => ss.Set<BasicTypesEntity>()
                    .Where(b => b.String.Replace("sea", "rea", StringComparison.OrdinalIgnoreCase) == "reattle"));

    [ConditionalFact]
    public virtual Task Replace_Char()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Replace('S', 'R') == "Reattle"));

    [ConditionalFact]
    public virtual Task Replace_with_empty_string()
        => AssertQuery(ss => ss
            .Set<BasicTypesEntity>()
            .Where(c => c.String != "" && c.String.Replace(c.String, string.Empty) == ""));

    [ConditionalFact]
    public virtual Task Replace_using_property_arguments()
        => AssertQuery(ss => ss
            .Set<BasicTypesEntity>()
            .Where(c => c.String != "" && c.String.Replace(c.String, c.Int.ToString()) == c.Int.ToString()));

    #endregion Replace

    #region Substring

    [ConditionalFact]
    public virtual Task Substring()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 3 && b.String.Substring(1, 2) == "ea"));

    [ConditionalFact]
    public virtual Task Substring_with_one_arg_with_zero_startIndex()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Substring(0) == "Seattle"));

    [ConditionalFact]
    public virtual Task Substring_with_one_arg_with_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 1 && b.String.Substring(1) == "eattle"));

    [ConditionalFact]
    public virtual Task Substring_with_one_arg_with_parameter()
    {
        var start = 2;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 2 && b.String.Substring(start) == "attle"));
    }

    [ConditionalFact]
    public virtual Task Substring_with_two_args_with_zero_startIndex()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 3 && b.String.Substring(0, 3) == "Sea"));

    [ConditionalFact]
    public virtual Task Substring_with_two_args_with_zero_length()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 2 && b.String.Substring(2, 0) == ""));

    [ConditionalFact]
    public virtual Task Substring_with_two_args_with_parameter()
    {
        var start = 2;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 5 && b.String.Substring(start, 3) == "att"));
    }

    [ConditionalFact]
    public virtual Task Substring_with_two_args_with_IndexOf()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c =>
            c.String.Contains("a") && c.String.Substring(c.String.IndexOf("a"), 3) == "att"));

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    [ConditionalFact]
    public virtual async Task IsNullOrEmpty()
    {
        await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(n => string.IsNullOrEmpty(n.String)));

        await AssertQueryScalar(ss => ss.Set<NullableBasicTypesEntity>().Select(n => string.IsNullOrEmpty(n.String)));
    }

    [ConditionalFact]
    public virtual async Task IsNullOrEmpty_negated()
    {
        await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(n => !string.IsNullOrEmpty(n.String)));

        await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Select(n => !string.IsNullOrEmpty(n.String)));
    }

    [ConditionalFact]
    public virtual Task IsNullOrWhiteSpace()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.IsNullOrWhiteSpace(c.String)));

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    [ConditionalFact]
    public virtual Task StartsWith_Literal()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se")))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("se")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("se", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual Task StartsWith_Literal_Char()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith('S')))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith('S')),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith('S')));

    [ConditionalFact]
    public virtual Task StartsWith_Parameter()
    {
        if (IsCaseSensitive)
        {
            var pattern = "Se";

            return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)));
        }
        else
        {
            var pattern = "se";

            return AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [ConditionalFact]
    public virtual Task StartsWith_Parameter_Char()
    {
        if (IsCaseSensitive)
        {
            var pattern = 'S';

            return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)));
        }
        else
        {
            var pattern = 'S';

            return AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)));
        }
    }

    [ConditionalFact]
    public virtual Task StartsWith_Column()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(b.String)));

    [ConditionalFact]
    public virtual Task StartsWith_with_StringComparison_Ordinal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.Ordinal)));

    [ConditionalFact]
    public virtual Task StartsWith_with_StringComparison_OrdinalIgnoreCase()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual async Task StartsWith_with_StringComparison_unsupported()
    {
        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.CurrentCulture))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.CurrentCultureIgnoreCase))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.InvariantCulture))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss
                => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.InvariantCultureIgnoreCase))));
    }

    #endregion StartsWith

    #region EndsWith

    [ConditionalFact]
    public virtual Task EndsWith_Literal()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le")))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("Le")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("Le", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual Task EndsWith_Literal_Char()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith('e')))
            : AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith('e')));

    [ConditionalFact]
    public virtual Task EndsWith_Parameter()
    {
        if (IsCaseSensitive)
        {
            var pattern = "le";

            return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)));
        }
        else
        {
            var pattern = "LE";

            return AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [ConditionalFact]
    public virtual Task EndsWith_Parameter_Char()
    {
        if (IsCaseSensitive)
        {
            var pattern = 'e';

            return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)));
        }
        else
        {
            var pattern = 'e';

            return AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)));
        }
    }

    [ConditionalFact]
    public virtual Task EndsWith_Column()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(b.String)));

    [ConditionalFact]
    public virtual Task EndsWith_with_StringComparison_Ordinal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.Ordinal)));

    [ConditionalFact]
    public virtual Task EndsWith_with_StringComparison_OrdinalIgnoreCase()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("LE", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual async Task EndsWith_with_StringComparison_unsupported()
    {
        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.CurrentCulture))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.CurrentCultureIgnoreCase))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.InvariantCulture))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.InvariantCultureIgnoreCase))));
    }

    #endregion EndsWith

    #region Contains

    [ConditionalFact]
    public virtual Task Contains_Literal()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains("eattl")))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains("Eattl")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains("Eattl", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual Task Contains_Literal_Char()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains('e')))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains('e')),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains('e')));

    [ConditionalFact]
    public virtual async Task Contains_Column()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains(b.String)));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.String.Contains(b.String)));
    }

    [ConditionalFact]
    public virtual async Task Contains_negated()
    {
        if (IsCaseSensitive)
        {
            await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => !c.String.Contains("eattle")));

            await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(c => !c.String.Contains("eattle")));
        }
        else
        {
            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => !c.String.Contains("Eattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => !c.String.Contains("Eattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQueryScalar(
                ss => ss.Set<BasicTypesEntity>().Select(c => !c.String.Contains("Eattle")),
                ss => ss.Set<BasicTypesEntity>().Select(c => !c.String.Contains("Eattle", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [ConditionalFact]
    public virtual Task Contains_with_StringComparison_Ordinal()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.Ordinal)));

    [ConditionalFact]
    public virtual Task Contains_with_StringComparison_OrdinalIgnoreCase()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("EATTL", StringComparison.OrdinalIgnoreCase)));

    [ConditionalFact]
    public virtual async Task Contains_with_StringComparison_unsupported()
    {
        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.CurrentCulture))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss
                => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.CurrentCultureIgnoreCase))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.InvariantCulture))));

        await AssertTranslationFailed(() =>
            AssertQuery(ss
                => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.InvariantCultureIgnoreCase))));
    }

    [ConditionalFact] // Probably belongs in FunkyDataQueryTestBase
    public virtual Task Contains_constant_with_whitespace()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("     ")),
            assertEmpty: true);

    [ConditionalFact] // Probably belongs in FunkyDataQueryTestBase
    public virtual Task Contains_parameter_with_whitespace()
    {
        var pattern = "     ";
        return AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains(pattern)),
            assertEmpty: true);
    }

    #endregion Contains

    #region TrimStart

    [ConditionalFact]
    public virtual Task TrimStart_without_arguments()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimStart() == "Boston  "));

    [ConditionalFact]
    public virtual Task TrimStart_with_char_argument()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimStart('S') == "eattle"));

    [ConditionalFact]
    public virtual Task TrimStart_with_char_array_argument()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimStart(new[] { 'S', 'e' }) == "attle"));

    #endregion TrimStart

    #region TrimEnd

    [ConditionalFact]
    public virtual Task TrimEnd_without_arguments()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimEnd() == "  Boston"));

    [ConditionalFact]
    public virtual Task TrimEnd_with_char_argument()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimEnd('e') == "Seattl"));

    [ConditionalFact]
    public virtual Task TrimEnd_with_char_array_argument()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimEnd(new[] { 'l', 'e' }) == "Seatt"));

    #endregion TrimEnd

    #region Trim

    [ConditionalFact]
    public virtual Task Trim_without_argument_in_predicate()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Trim() == "Boston"));

    [ConditionalFact]
    public virtual Task Trim_with_char_argument_in_predicate()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Trim('S') == "eattle"));

    [ConditionalFact]
    public virtual Task Trim_with_char_array_argument_in_predicate()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Trim(new[] { 'S', 'e' }) == "attl"));

    #endregion Trim

    #region Compare

    [ConditionalFact]
    public virtual async Task Compare_simple_zero()
    {
        if (IsCaseSensitive)
        {
            await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") == 0));

            await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, "Seattle")));

            await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") > 0));

            await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "Seattle")));

            await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 < string.Compare(c.String, "Seattle")));

            await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") <= 0));
        }
        else
        {
            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle") == 0),
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase) == 0));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle") > 0),
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase) > 0));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < string.Compare(c.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle") <= 0),
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase) <= 0));
        }
    }

    [ConditionalFact]
    public virtual async Task Compare_simple_one()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") == 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 == string.Compare(c.String, "Seattle")));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") < 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 1 > string.Compare(c.String, "Seattle")));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") > -1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 < string.Compare(c.String, "Seattle")));
    }

    [ConditionalFact]
    public virtual async Task Compare_with_parameter()
    {
        BasicTypesEntity? basicTypeEntity;
        await using (var context = CreateContext())
        {
            basicTypeEntity = await context.BasicTypesEntities.SingleAsync(c => c.Id == 1);
        }

        ClearLog();

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, basicTypeEntity.String) == 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 == string.Compare(c.String, basicTypeEntity.String)));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, basicTypeEntity.String) < 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 1 > string.Compare(c.String, basicTypeEntity.String)));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, basicTypeEntity.String) > -1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 < string.Compare(c.String, basicTypeEntity.String)));
    }

    [ConditionalFact]
    public virtual async Task Compare_simple_more_than_one()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") == 42),
            assertEmpty: true);

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") > 42),
            assertEmpty: true);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 42 > string.Compare(c.String, "Seattle")));
    }

    [ConditionalFact]
    public virtual async Task Compare_nested()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "M" + c.String) == 0),
            assertEmpty: true);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, c.String.Substring(0, 0))));

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle".Replace("Sea", c.String)) > 0),
            assertEmpty: true);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "M" + c.String)));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 1 == string.Compare(c.String, c.String.Substring(0, 0))));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle".Replace("Sea", c.String)) == -1));
    }

    [ConditionalFact]
    public virtual async Task Compare_multi_predicate()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(c => string.Compare(c.String, "Seattle") > -1)
            .Where(c => string.Compare(c.String, "Toronto") == -1));

    [ConditionalFact]
    public virtual async Task CompareTo_simple_zero()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") == 0));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.String.CompareTo("Seattle")));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") > 0));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.String.CompareTo("Seattle")));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 < c.String.CompareTo("Seattle")));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") <= 0));
    }

    [ConditionalFact]
    public virtual async Task CompareTo_simple_one()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") == 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 == c.String.CompareTo("Seattle")));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") < 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 1 > c.String.CompareTo("Seattle")));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") > -1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 < c.String.CompareTo("Seattle")));
    }

    [ConditionalFact]
    public virtual async Task CompareTo_with_parameter()
    {
        BasicTypesEntity? basicTypesEntity;
        using (var context = CreateContext())
        {
            basicTypesEntity = await context.BasicTypesEntities.SingleAsync(x => x.Id == 1);
        }

        ClearLog();

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo(basicTypesEntity.String) == 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 == c.String.CompareTo(basicTypesEntity.String)));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo(basicTypesEntity.String) < 1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 1 > c.String.CompareTo(basicTypesEntity.String)));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo(basicTypesEntity.String) > -1));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => -1 < c.String.CompareTo(basicTypesEntity.String)));
    }

    [ConditionalFact]
    public virtual async Task CompareTo_simple_more_than_one()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") == 42),
            assertEmpty: true);

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") > 42),
            assertEmpty: true);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 42 > c.String.CompareTo("Seattle")));
    }

    [ConditionalFact]
    public virtual async Task CompareTo_nested()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("M" + c.String) == 0),
            assertEmpty: true);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.String.CompareTo(c.String.Substring(0, 0))));

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle".Replace("Sea", c.String)) > 0),
            assertEmpty: true);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.String.CompareTo("M" + c.String)));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => 1 == c.String.CompareTo(c.String.Substring(0, 0))));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle".Replace("Sea", c.String)) == -1));
    }

    [ConditionalFact]
    public virtual async Task Compare_to_multi_predicate()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(c => c.String.CompareTo("Seattle") > -1)
            .Where(c => c.String.CompareTo("Toronto") == -1));

    #endregion Compare

    #region Join

    [ConditionalFact]
    public virtual Task Join_over_non_nullable_column()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .GroupBy(c => c.Int)
                .Select(g => new { g.Key, Strings = string.Join("|", g.Select(e => e.String)) }),
            elementSorter: x => x.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);

                // Ordering inside the string isn't specified server-side, split and reorder
                Assert.Equal(
                    e.Strings.Split("|").OrderBy(id => id).ToArray(),
                    a.Strings.Split("|").OrderBy(id => id).ToArray());
            });

    [ConditionalFact]
    public virtual Task Join_over_nullable_column()
        => AssertQuery(
            ss => ss.Set<NullableBasicTypesEntity>()
                .GroupBy(c => c.Int ?? 0)
                .Select(g => new { g.Key, Regions = string.Join("|", g.Select(e => e.String)) }),
            elementSorter: x => x.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);

                // Ordering inside the string isn't specified server-side, split and reorder
                Assert.Equal(
                    e.Regions.Split("|").OrderBy(id => id).ToArray(),
                    a.Regions.Split("|").OrderBy(id => id).ToArray());
            });

    [ConditionalFact]
    public virtual Task Join_with_predicate()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .GroupBy(c => c.Int)
                .Select(g => new { g.Key, Strings = string.Join("|", g.Where(e => e.String.Length > 6).Select(e => e.String)) }),
            elementSorter: x => x.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);

                // Ordering inside the string isn't specified server-side, split and reorder
                Assert.Equal(
                    e.Strings.Split("|").OrderBy(id => id).ToArray(),
                    a.Strings.Split("|").OrderBy(id => id).ToArray());
            });

    [ConditionalFact]
    public virtual Task Join_with_ordering()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .GroupBy(c => c.Int)
                .Select(g => new { g.Key, Strings = string.Join("|", g.OrderByDescending(e => e.Id).Select(e => e.String)) }),
            elementSorter: x => x.Key);

    [ConditionalFact]
    public virtual Task Join_non_aggregate()
    {
        var foo = "foo";

        return AssertQuery(ss
            => ss.Set<BasicTypesEntity>().Where(c => string.Join("|", new[] { c.String, foo, null, "bar" }) == "Seattle|foo||bar"));
    }

    #endregion Join

    #region Concatenation

    [ConditionalFact]
    public virtual Task Concat_operator()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.String + "Boston" == "SeattleBoston"));

    // TODO: Possibly move to aggregate-specific test suite, not sure. Also Join above.

    [ConditionalFact]
    public virtual Task Concat_aggregate()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .GroupBy(c => c.Int)
                .Select(g => new { g.Key, BasicTypesEntitys = string.Concat(g.Select(e => e.String)) }),
            elementSorter: x => x.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);

                // The best we can do for Concat without server-side ordering is sort the characters (concatenating without ordering
                // and without a delimiter is somewhat dubious anyway).
                Assert.Equal(e.BasicTypesEntitys.OrderBy(c => c).ToArray(), a.BasicTypesEntitys.OrderBy(c => c).ToArray());
            });

    [ConditionalFact] // #31917
    public virtual Task Concat_string_int_comparison1()
    {
        var i = 10;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String + i == "Seattle10"));
    }

    [ConditionalFact] // #31917
    public virtual Task Concat_string_int_comparison2()
    {
        var i = 10;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => i + c.String == "10Seattle"));
    }

    [ConditionalFact] // #31917
    public virtual Task Concat_string_int_comparison3()
    {
        var i = 10;
        var j = 21;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => i + 20 + c.String + j + 42 == "30Seattle2142"));
    }

    [ConditionalFact] // #31917
    public virtual Task Concat_string_int_comparison4()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.Int + o.String == "8Seattle"));

    [ConditionalFact]
    public virtual Task Concat_string_string_comparison()
    {
        var i = "A";

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => i + c.String == "ASeattle"));
    }

    [ConditionalFact]
    public virtual Task Concat_method_comparison()
    {
        var i = "A";

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Concat(i, c.String) == "ASeattle"));
    }

    [ConditionalFact]
    public virtual Task Concat_method_comparison_2()
    {
        var i = "A";
        var j = "B";

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Concat(i, j, c.String) == "ABSeattle"));
    }

    [ConditionalFact]
    public virtual Task Concat_method_comparison_3()
    {
        var i = "A";
        var j = "B";
        var k = "C";

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => string.Concat(i, j, k, c.String) == "ABCSeattle"));
    }

    #endregion Concatenation

    #region LINQ Operators

    [ConditionalFact]
    public virtual Task FirstOrDefault()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.FirstOrDefault() == 'S'));

    [ConditionalFact]
    public virtual Task LastOrDefault()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => c.String.LastOrDefault() == 'e'));

    #endregion LINQ Operators

    #region Regex

    [ConditionalFact]
    public virtual Task Regex_IsMatch()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch(o.String, "^S")))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch(o.String, "^s")),
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch(o.String, "^s", RegexOptions.IgnoreCase)));

    [ConditionalFact]
    public virtual Task Regex_IsMatch_constant_input()
        => IsCaseSensitive
            ? AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch("Seattle", o.String)))
            : AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch("seattle", o.String)),
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch("seattle", o.String, RegexOptions.IgnoreCase)));

    #endregion Regex

    protected BasicTypesContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
