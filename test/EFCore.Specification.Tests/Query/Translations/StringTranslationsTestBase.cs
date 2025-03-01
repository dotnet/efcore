// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

// See additional string tests for special values in FunkyDataQueryTestBase
public abstract class StringTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    /// <summary>
    ///     Controls whether the tests assert case-sensitive or insensitive string comparisons. Defaults to <see langword="true" />.
    /// </summary>
    protected virtual bool IsCaseSensitive => true;

    #region Equals

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Equals(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("Seattle")))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Equals_with_OrdinalIgnoreCase(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Equals_with_Ordinal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Equals("Seattle", StringComparison.Ordinal)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Static_Equals(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "Seattle")))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Static_Equals_with_OrdinalIgnoreCase(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "seattle", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Static_Equals_with_Ordinal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => string.Equals(b.String, "Seattle", StringComparison.Ordinal)));

    #endregion Equals

    #region Miscellaneous

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task ToUpper(bool async)
    {
        // Note that if the database is case-insensitive, the Where() assertion checks nothing.
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.ToUpper() == "SEATTLE"));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.String.ToUpper()));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task ToLower(bool async)
    {
        // Note that if the database is case-insensitive, the Where() assertion checks nothing.
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.ToLower() == "seattle"));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.String.ToLower()));
    }

    #endregion Miscellaneous

    #region IndexOf

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf("eattl") != -1))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf("Eattl") != -1),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf("Eattl", StringComparison.OrdinalIgnoreCase) != -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_Char(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf('e') != -1))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf('e') != -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_with_empty_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(string.Empty) == 0),
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String != null && b.String.IndexOf(string.Empty) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_with_one_parameter_arg(bool async)
    {
        if (IsCaseSensitive)
        {
            var pattern = "eattl";

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1));
        }
        else
        {
            var pattern = "Eattl";

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) == 1));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_with_one_parameter_arg_char(bool async)
    {
        if (IsCaseSensitive)
        {
            var pattern = 'e';

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1));
        }
        else
        {
            var pattern = 'e';

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern) == 1),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) == 1));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_with_constant_starting_position(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf("e", 2) == 6));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_with_constant_starting_position_char(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf('e', 2) == 6));

#pragma warning disable CA1866 // Use 'string.Method(char)' instead of 'string.Method(string)' for string with single char
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_with_parameter_starting_position(bool async)
    {
        var start = 2;

        return IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf("e", start) == 6))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(
                    b => b.String.Length > 2 && b.String.IndexOf("E", start) == 6),
                ss => ss.Set<BasicTypesEntity>().Where(
                    b => b.String.Length > 2 && b.String.IndexOf("E", start, StringComparison.OrdinalIgnoreCase) == 6));
    }
#pragma warning restore CA1866

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_with_parameter_starting_position_char(bool async)
    {
        var start = 2;

        return IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length > 2 && b.String.IndexOf('e', start) == 6))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(
                    b => b.String.Length > 2 && b.String.IndexOf('e', start) == 6));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_after_ToString(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(x => x.Int.ToString().IndexOf("55") == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IndexOf_over_ToString(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(x => "12559".IndexOf(x.Int.ToString()) == 1));

    #endregion IndexOf

    #region Replace

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Replace(bool async)
        => IsCaseSensitive
        ? AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Replace("Sea", "Rea") == "Reattle"))
        : AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Replace("sea", "rea") == "reattle"),
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Replace("sea", "rea", StringComparison.OrdinalIgnoreCase) == "reattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Replace_Char(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Replace('S', 'R') == "Reattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Replace_with_empty_string(bool async)
        => AssertQuery(
            async,
            ss => ss
                .Set<BasicTypesEntity>()
                .Where(c => c.String != "" && c.String.Replace(c.String, string.Empty) == ""));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Replace_using_property_arguments(bool async)
        => AssertQuery(
            async,
            ss => ss
                .Set<BasicTypesEntity>()
                .Where(c => c.String != "" && c.String.Replace(c.String, c.Int.ToString()) == c.Int.ToString()));

    #endregion Replace

    #region Substring

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 3 && b.String.Substring(1, 2) == "ea"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_one_arg_with_zero_startIndex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Substring(0) == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_one_arg_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 1 && b.String.Substring(1) == "eattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_one_arg_with_parameter(bool async)
    {
        var start = 2;

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 2 && b.String.Substring(start) == "attle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_zero_startIndex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 3 && b.String.Substring(0, 3) == "Sea"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_zero_length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 2 && b.String.Substring(2, 0) == ""));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_parameter(bool async)
    {
        var start = 2;

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Length >= 5 && b.String.Substring(start, 3) == "att"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_IndexOf(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c =>
                c.String.Contains("a") && c.String.Substring(c.String.IndexOf("a"), 3) == "att"));

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsNullOrEmpty(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NullableBasicTypesEntity>().Where(n => string.IsNullOrEmpty(n.String)));

        await AssertQueryScalar(
            async,
            ss => ss.Set<NullableBasicTypesEntity>().Select(n => string.IsNullOrEmpty(n.String)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsNullOrEmpty_negated(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<NullableBasicTypesEntity>().Where(n => !string.IsNullOrEmpty(n.String)));

        await AssertQuery(
            async,
            ss => ss.Set<NullableBasicTypesEntity>().Select(n => !string.IsNullOrEmpty(n.String)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsNullOrWhiteSpace(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.IsNullOrWhiteSpace(c.String)));

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartsWith_Literal(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se")))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("se")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("se", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartsWith_Literal_Char(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith('S')))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith('S')),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith('S')));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartsWith_Parameter(bool async)
    {
        if (IsCaseSensitive)
        {
            var pattern = "Se";

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)));
        }
        else
        {
            var pattern = "se";

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartsWith_Parameter_Char(bool async)
    {
        if (IsCaseSensitive)
        {
            var pattern = 'S';

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)));
        }
        else
        {
            var pattern = 'S';

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(pattern)));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartsWith_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith(b.String)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartsWith_with_StringComparison_Ordinal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.Ordinal)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task StartsWith_with_StringComparison_unsupported(bool async)
    {
        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.CurrentCulture))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.CurrentCultureIgnoreCase))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.InvariantCulture))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.StartsWith("Se", StringComparison.InvariantCultureIgnoreCase))));
    }

    #endregion StartsWith

    #region EndsWith

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndsWith_Literal(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le")))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("Le")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("Le", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndsWith_Literal_Char(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith('e')))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith('e')));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndsWith_Parameter(bool async)
    {
        if (IsCaseSensitive)
        {
            var pattern = "le";

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)));
        }
        else
        {
            var pattern = "LE";

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndsWith_Parameter_Char(bool async)
    {
        if (IsCaseSensitive)
        {
            var pattern = 'e';

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)));
        }
        else
        {
            var pattern = 'e';

            return AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(pattern)));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndsWith_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith(b.String)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndsWith_with_StringComparison_Ordinal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.Ordinal)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("LE", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task EndsWith_with_StringComparison_unsupported(bool async)
    {
        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.CurrentCulture))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.CurrentCultureIgnoreCase))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.InvariantCulture))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(b => b.String.EndsWith("le", StringComparison.InvariantCultureIgnoreCase))));
    }

    #endregion EndsWith

    #region Contains

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_Literal(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains("eattl")))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains("Eattl")),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains("Eattl", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_Literal_Char(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains('e')))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains('e')),
                ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains('e')));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_Column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String.Contains(b.String)));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.String.Contains(b.String)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_negated(bool async)
    {
        if (IsCaseSensitive)
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => !c.String.Contains("eattle")));

            await AssertQueryScalar(
                async,
                ss => ss.Set<BasicTypesEntity>().Select(c => !c.String.Contains("eattle")));
        }
        else
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => !c.String.Contains("Eattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => !c.String.Contains("Eattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQueryScalar(
                async,
                ss => ss.Set<BasicTypesEntity>().Select(c => !c.String.Contains("Eattle")),
                ss => ss.Set<BasicTypesEntity>().Select(c => !c.String.Contains("Eattle", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_StringComparison_Ordinal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.Ordinal)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("EATTL", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_StringComparison_unsupported(bool async)
    {
        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.CurrentCulture))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.CurrentCultureIgnoreCase))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.InvariantCulture))));

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.InvariantCultureIgnoreCase))));
    }

    [ConditionalTheory] // Probably belongs in FunkyDataQueryTestBase
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_constant_with_whitespace(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("     ")),
            assertEmpty: true);

    [ConditionalTheory] // Probably belongs in FunkyDataQueryTestBase
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_parameter_with_whitespace(bool async)
    {
        var pattern = "     ";
        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains(pattern)),
            assertEmpty: true);
    }

    #endregion Contains

    #region TrimStart

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimStart_without_arguments(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimStart() == "Boston  "));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimStart_with_char_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimStart('S') == "eattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimStart_with_char_array_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimStart(new [] { 'S', 'e' }) == "attle"));

    #endregion TrimStart

    #region TrimEnd

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimEnd_without_arguments(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimEnd() == "  Boston"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimEnd_with_char_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimEnd('e') == "Seattl"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimEnd_with_char_array_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.TrimEnd(new [] { 'l', 'e' }) == "Seatt"));

    #endregion TrimEnd

    #region Trim

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trim_without_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Trim() == "Boston"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trim_with_char_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Trim('S') == "eattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trim_with_char_array_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Trim(new [] { 'S', 'e' }) == "attl"));

    #endregion Trim

    #region Compare

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_simple_zero(bool async)
    {
        if (IsCaseSensitive)
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, "Seattle")));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "Seattle")));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < string.Compare(c.String, "Seattle")));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") <= 0));
        }
        else
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle") == 0),
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle") > 0),
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < string.Compare(c.String, "seattle")),
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle") <= 0),
                ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "seattle", StringComparison.OrdinalIgnoreCase) <= 0));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_simple_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") == 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 == string.Compare(c.String, "Seattle")));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") < 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 1 > string.Compare(c.String, "Seattle")));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") > -1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 < string.Compare(c.String, "Seattle")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_with_parameter(bool async)
    {
        BasicTypesEntity? basicTypeEntity;
        await using (var context = CreateContext())
        {
            basicTypeEntity = await context.BasicTypesEntities.SingleAsync(c => c.Id == 1);
        }

        ClearLog();

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, basicTypeEntity.String) == 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 == string.Compare(c.String, basicTypeEntity.String)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, basicTypeEntity.String) < 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 1 > string.Compare(c.String, basicTypeEntity.String)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, basicTypeEntity.String) > -1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 < string.Compare(c.String, basicTypeEntity.String)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_simple_more_than_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") == 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle") > 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 42 > string.Compare(c.String, "Seattle")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_nested(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "M" + c.String) == 0),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 != string.Compare(c.String, c.String.Substring(0, 0))));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle".Replace("Sea", c.String)) > 0),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= string.Compare(c.String, "M" + c.String)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 1 == string.Compare(c.String, c.String.Substring(0, 0))));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Compare(c.String, "Seattle".Replace("Sea", c.String)) == -1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_multi_predicate(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(c => string.Compare(c.String, "Seattle") > -1)
                .Where(c => string.Compare(c.String, "Toronto") == -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task CompareTo_simple_zero(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") == 0));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.String.CompareTo("Seattle")));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") > 0));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.String.CompareTo("Seattle")));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 < c.String.CompareTo("Seattle")));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") <= 0));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task CompareTo_simple_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") == 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 == c.String.CompareTo("Seattle")));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") < 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 1 > c.String.CompareTo("Seattle")));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") > -1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 < c.String.CompareTo("Seattle")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task CompareTo_with_parameter(bool async)
    {
        BasicTypesEntity? basicTypesEntity;
        using (var context = CreateContext())
        {
            basicTypesEntity = await context.BasicTypesEntities.SingleAsync(x => x.Id == 1);
        }

        ClearLog();

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo(basicTypesEntity.String) == 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 == c.String.CompareTo(basicTypesEntity.String)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo(basicTypesEntity.String) < 1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 1 > c.String.CompareTo(basicTypesEntity.String)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo(basicTypesEntity.String) > -1));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => -1 < c.String.CompareTo(basicTypesEntity.String)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task CompareTo_simple_more_than_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") == 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle") > 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 42 > c.String.CompareTo("Seattle")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task CompareTo_nested(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("M" + c.String) == 0),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.String.CompareTo(c.String.Substring(0, 0))));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle".Replace("Sea", c.String)) > 0),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.String.CompareTo("M" + c.String)));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => 1 == c.String.CompareTo(c.String.Substring(0, 0))));

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.CompareTo("Seattle".Replace("Sea", c.String)) == -1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_to_multi_predicate(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(c => c.String.CompareTo("Seattle") > -1)
                .Where(c => c.String.CompareTo("Toronto") == -1));

    #endregion Compare

    #region Join

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_over_non_nullable_column(bool async)
        => AssertQuery(
            async,
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_over_nullable_column(bool async)
        => AssertQuery(
            async,
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .GroupBy(c => c.Int)
                .Select(
                    g => new
                    {
                        g.Key,
                        Strings = string.Join("|", g.Where(e => e.String.Length > 6).Select(e => e.String))
                    }),
            elementSorter: x => x.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);

                // Ordering inside the string isn't specified server-side, split and reorder
                Assert.Equal(
                    e.Strings.Split("|").OrderBy(id => id).ToArray(),
                    a.Strings.Split("|").OrderBy(id => id).ToArray());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_ordering(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .GroupBy(c => c.Int)
                .Select(
                    g => new
                    {
                        g.Key, Strings = string.Join("|", g.OrderByDescending(e => e.Id).Select(e => e.String))
                    }),
            elementSorter: x => x.Key);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_non_aggregate(bool async)
    {
        var foo = "foo";

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Join("|", new[] { c.String, foo, null, "bar" }) == "Seattle|foo||bar"));
    }

    #endregion Join

    #region Concatenation

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String + "Boston" == "SeattleBoston"));

    // TODO: Possibly move to aggregate-specific test suite, not sure. Also Join above.

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_aggregate(bool async)
        => AssertQuery(
            async,
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

    [ConditionalTheory] // #31917
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_string_int_comparison1(bool async)
    {
        var i = 10;

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String + i == "Seattle10"));
    }

    [ConditionalTheory] // #31917
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_string_int_comparison2(bool async)
    {
        var i = 10;

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => i + c.String == "10Seattle"));
    }

    [ConditionalTheory] // #31917
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_string_int_comparison3(bool async)
    {
        var i = 10;
        var j = 21;

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => i + 20 + c.String + j + 42 == "30Seattle2142"));
    }

    [ConditionalTheory] // #31917
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_string_int_comparison4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int + o.String == "8Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_string_string_comparison(bool async)
    {
        var i = "A";

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => i + c.String == "ASeattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_method_comparison(bool async)
    {
        var i = "A";

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Concat(i, c.String) == "ASeattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_method_comparison_2(bool async)
    {
        var i = "A";
        var j = "B";

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Concat(i, j, c.String) == "ABSeattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_method_comparison_3(bool async)
    {
        var i = "A";
        var j = "B";
        var k = "C";

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => string.Concat(i, j, k, c.String) == "ABCSeattle"));
    }

    #endregion Concatenation

    #region LINQ Operators

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.FirstOrDefault() == 'S'));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LastOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.LastOrDefault() == 'e'));

    #endregion LINQ Operators

    #region Regex

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch(o.String, "^S")))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch(o.String, "^s")),
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch(o.String, "^s", RegexOptions.IgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_constant_input(bool async)
        => IsCaseSensitive
            ? AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch("Seattle", o.String)))
            : AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch("seattle", o.String)),
                ss => ss.Set<BasicTypesEntity>().Where(o => Regex.IsMatch("seattle", o.String, RegexOptions.IgnoreCase)));

    #endregion Regex

    protected BasicTypesContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
