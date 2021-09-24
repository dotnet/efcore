// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information.
    /// </remarks>
    public static class RelationalDbFunctionsExtensions
    {
        /// <summary>
        ///     <para>
        ///         Explicitly specifies a collation to be used in a LINQ query. Can be used to generate fragments such as
        ///         <c>WHERE customer.name COLLATE 'de_DE' = 'John Doe'</c>.
        ///     </para>
        ///     <para>
        ///         The available collations and their names vary across databases, consult your database's documentation for more
        ///         information.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information.
        /// </remarks>
        /// <typeparam name="TProperty">The type of the operand on which the collation is being specified.</typeparam>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="operand">The operand to which to apply the collation.</param>
        /// <param name="collation">The name of the collation.</param>
        public static TProperty Collate<TProperty>(
            this DbFunctions _,
            TProperty operand,
            [NotParameterized] string collation)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Collate)));
    }
}
