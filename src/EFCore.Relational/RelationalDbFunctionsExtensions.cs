// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class RelationalDbFunctionsExtensions
    {
        /// <summary>
        /// Indicates whether the specified input string matches the given pattern by sending an SQL LIKE
        /// expression to the database.
        /// </summary>
        /// <param name="functions">Should always be <see cref="EF.Functions"/>.</param>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <returns><string>true</string> if the LIKE expression finds a match; otherwise, <string>false</string>.</returns>
        public static bool Like(
            [NotNull] this DbFunctions functions,
            [NotNull] string input,
            [NotNull] string pattern)
            => throw new NotSupportedException(RelationalStrings.DbFunctionsDirectCall);

        /// <summary>
        /// Indicates whether the specified input string matches the given pattern by sending an SQL LIKE
        /// expression to the database.
        /// </summary>
        /// <param name="functions">Should always be <see cref="EF.Functions"/>.</param>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="escapeChar">Character to use as an escape character in <paramref name="pattern"/>.</param>
        /// <returns><string>true</string> if the LIKE expression finds a match; otherwise, <string>false</string>.</returns>
        public static bool Like(
            [NotNull] this DbFunctions functions,
            [NotNull] string input,
            [NotNull] string pattern,
            char escapeChar)
            => throw new NotSupportedException(RelationalStrings.DbFunctionsDirectCall);
    }
}
