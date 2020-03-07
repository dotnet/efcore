// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class DbFunctionsExtensions
    {
        /// <summary>
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that if this function is translated into SQL, then the semantics of the comparison will
        ///         depend on the database configuration. In particular, it may be either case-sensitive or
        ///         case-insensitive. If this function is evaluated on the client, then it will always use
        ///         a case-insensitive comparison.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
        /// <returns>true if there is a match.</returns>
        public static bool Like(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern)
            => LikeCore(matchExpression, pattern, escapeCharacter: null);

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that if this function is translated into SQL, then the semantics of the comparison will
        ///         depend on the database configuration. In particular, it may be either case-sensitive or
        ///         case-insensitive. If this function is evaluated on the client, then it will always use
        ///         a case-insensitive comparison.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
        /// <param name="escapeCharacter">
        ///     The escape character (as a single character string) to use in front of %,_,[,],^
        ///     if they are not used as wildcards.
        /// </param>
        /// <returns>true if there is a match.</returns>
        public static bool Like(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern,
            [CanBeNull] string escapeCharacter)
            => LikeCore(matchExpression, pattern, escapeCharacter);

        // Regex special chars defined here:
        // https://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx

        private static readonly char[] _regexSpecialChars
            = { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\' };

        private static readonly string _defaultEscapeRegexCharsPattern
            = BuildEscapeRegexCharsPattern(_regexSpecialChars);

        private static readonly TimeSpan _regexTimeout = TimeSpan.FromMilliseconds(value: 1000.0);

        private static string BuildEscapeRegexCharsPattern(IEnumerable<char> regexSpecialChars)
        {
            return string.Join("|", regexSpecialChars.Select(c => @"\" + c));
        }

        private static bool LikeCore(string matchExpression, string pattern, string escapeCharacter)
        {
            //TODO: this fixes https://github.com/aspnet/EntityFramework/issues/8656 by insisting that
            // the "escape character" is a string but just using the first character of that string,
            // but we may later want to allow the complete string as the "escape character"
            // in which case we need to change the way we construct the regex below.
            var singleEscapeCharacter =
                (escapeCharacter == null || escapeCharacter.Length == 0)
                    ? (char?)null
                    : escapeCharacter.First();

            if (matchExpression == null
                || pattern == null)
            {
                return false;
            }

            if (matchExpression.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (matchExpression.Length == 0
                || pattern.Length == 0)
            {
                return false;
            }

            var escapeRegexCharsPattern
                = singleEscapeCharacter == null
                    ? _defaultEscapeRegexCharsPattern
                    : BuildEscapeRegexCharsPattern(_regexSpecialChars.Where(c => c != singleEscapeCharacter));

            var regexPattern
                = Regex.Replace(
                    pattern,
                    escapeRegexCharsPattern,
                    c => @"\" + c,
                    default,
                    _regexTimeout);

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < regexPattern.Length; i++)
            {
                var c = regexPattern[i];
                var escaped = i > 0 && regexPattern[i - 1] == singleEscapeCharacter;

                switch (c)
                {
                    case '_':
                    {
                        stringBuilder.Append(escaped ? '_' : '.');
                        break;
                    }
                    case '%':
                    {
                        stringBuilder.Append(escaped ? "%" : ".*");
                        break;
                    }
                    default:
                    {
                        if (c != singleEscapeCharacter)
                        {
                            stringBuilder.Append(c);
                        }

                        break;
                    }
                }
            }

            regexPattern = stringBuilder.ToString();

            return Regex.IsMatch(
                matchExpression,
                @"\A" + regexPattern + @"\s*\z",
                RegexOptions.IgnoreCase | RegexOptions.Singleline,
                _regexTimeout);
        }
    }
}
