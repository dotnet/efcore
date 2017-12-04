// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    public static class SqlServerDbFunctionsExtensions
    {
        /// <summary>
        /// <para>
        ///     A DbFunction method stub that can be used in LINQ queries to target the SQL Server FREETEXT store function.
        /// </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="propertyName">The property on which the search will be performed.</param>
        /// <param name="freeText">The text that will be searched for in the property.</param>
        /// <param name="languageTerm">A Language ID from the sys.syslanguages table.</param>
        public static bool FreeText(
            [CanBeNull] this DbFunctions _,
            [NotNull] string propertyName,
            [NotNull] string freeText,
            int languageTerm)
            => FreeTextCore(propertyName, freeText, languageTerm);

        /// <summary>
        /// <para>
        ///     A DbFunction method stub that can be used in LINQ queries to target the SQL Server FREETEXT store function.
        /// </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="propertyName">The property on which the search will be performed.</param>
        /// <param name="freeText">The text that will be searched for in the property.</param>
        public static bool FreeText(
            [CanBeNull] this DbFunctions _,
            [NotNull] string propertyName,
            [NotNull] string freeText)
            => FreeTextCore(propertyName, freeText, null);

        private static bool FreeTextCore(string propertyName, string freeText, int? languageTerm)
        {
            throw new InvalidOperationException(SqlServerStrings.FreeTextFunctionOnClient);
        }
    }
}
