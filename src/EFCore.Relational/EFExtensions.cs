// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Methods that are useful in application code. For example, referencing a shadow state property in a LINQ query.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> and
///     <see href="https://aka.ms/efcore-docs-efproperty">Using EF.Property in EF Core queries</see> for more information and examples.
/// </remarks>
public static class EFExtensions
{
    /// <summary>
    ///     Methods that are useful in application code. For example, referencing a shadow state property in a LINQ query.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> and
    ///     <see href="https://aka.ms/efcore-docs-efproperty">Using EF.Property in EF Core queries</see> for more information and examples.
    /// </remarks>
    extension(EF)
    {
        /// <summary>
        ///     Within the context of an EF LINQ query, forces its argument to be inserted into the query as a multiple parameter expressions.
        /// </summary>
        /// <remarks>Note that this is a static method accessed through the top-level <see cref="EF" /> static type.</remarks>
        /// <typeparam name="T">The type of collection element.</typeparam>
        /// <param name="argument">The collection to be integrated as parameters into the query.</param>
        /// <returns>The same value for further use in the query.</returns>
        public static IEnumerable<T> MultipleParameters<T>(IEnumerable<T> argument)
            => throw new InvalidOperationException(RelationalStrings.EFMultipleParametersInvoked);
    }
}
