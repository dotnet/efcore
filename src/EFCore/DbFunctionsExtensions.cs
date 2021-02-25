// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
        ///         An implementation of the SQL <c>LIKE</c> operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that the semantics of the comparison will depend on the database configuration.
        ///         In particular, it may be either case-sensitive or case-insensitive.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards <c>%,_,[,],^</c>.</param>
        /// <returns><see langword="true" /> if there is a match.</returns>
        public static bool Like(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Like)));

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that the semantics of the comparison will depend on the database configuration.
        ///         In particular, it may be either case-sensitive or case-insensitive.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
        /// <param name="escapeCharacter">
        ///     The escape character (as a single character string) to use in front of %,_,[,],^
        ///     if they are not used as wildcards.
        /// </param>
        /// <returns><see langword="true" /> if there is a match.</returns>
        public static bool Like(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern,
            [CanBeNull] string escapeCharacter)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Like)));

        /// <summary>
        ///     A random double number generator which generates a number between 0 and 1, exclusive.
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_"> The <see cref="DbFunctions" /> instance. </param>
        /// <returns> A random double number between 0 and 1, exclusive. </returns>
        public static double Random([CanBeNull] this DbFunctions _)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Random)));
    }
}
