// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public static class RelationalDbFunctionsExtensions
{
    /// <summary>
    ///     Explicitly specifies a collation to be used in a LINQ query. Can be used to generate fragments such as
    ///     <c>WHERE customer.name COLLATE 'de_DE' = 'John Doe'</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The available collations and their names vary across databases, consult your database's documentation for more
    ///         information.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
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

    /// <summary>
    ///     Returns the smallest value from the given list of values. Usually corresponds to the <c>LEAST</c> SQL function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The list of values from which return the smallest value.</param>
    public static T Least<T>(
        this DbFunctions _,
        [NotParameterized] params T[] values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Least)));

    /// <summary>
    ///     Returns the greatest value from the given list of values. Usually corresponds to the <c>GREATEST</c> SQL function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The list of values from which return the greatest value.</param>
    public static T Greatest<T>(
        this DbFunctions _,
        [NotParameterized] params T[] values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Greatest)));

    /// <summary>
    ///     Checks whether a specified JSON path exists within a JSON string.
    ///     Typically corresponds to a database function or SQL expression.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is translated to a database-specific function or expression. 
    ///         The support for this function depends on the database and provider being used. 
    ///         Refer to your database provider's documentation for detailed support information.
    ///     </para>
    ///     <para>
    ///         For more details, see <see href="https://learn.microsoft.com/en-us/ef/core/providers">EF Core database providers</see>.
    ///     </para>
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="expression">The JSON string or column containing JSON text.</param>
    /// <param name="path">The JSON path to check for existence.</param>
    /// <typeparam name="T">The type of the JSON expression.</typeparam>
    /// <returns>
    ///     A nullable boolean value, <see langword="true"/> if the JSON path exists, <see langword="false"/> if not, and <see langword="null"/>
    ///     when the JSON string is null.
    /// </returns>
    public static bool? JsonExists<T>(
        this DbFunctions _,
        T expression,
        string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonExists)));
}
