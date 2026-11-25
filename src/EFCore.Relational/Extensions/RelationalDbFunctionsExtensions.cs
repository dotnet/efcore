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
    public static TProperty Collate<TProperty>(this DbFunctions _, TProperty operand, [NotParameterized] string collation)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Collate)));

    /// <summary>
    ///     Returns the smallest value from the given list of values. Usually corresponds to the <c>LEAST</c> SQL function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The list of values from which return the smallest value.</param>
    public static T Least<T>(this DbFunctions _, [NotParameterized] params T[] values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Least)));

    /// <summary>
    ///     Returns the greatest value from the given list of values. Usually corresponds to the <c>GREATEST</c> SQL function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The list of values from which return the greatest value.</param>
    public static T Greatest<T>(this DbFunctions _, [NotParameterized] params T[] values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Greatest)));

    /// <summary>
    ///     Returns a value indicating whether a given JSON path exists within the specified JSON.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="json">The JSON value to check.</param>
    /// <param name="path">The JSON path to look for.</param>
    public static bool JsonPathExists(this DbFunctions _, object json, string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonPathExists)));

    /// <summary>
    ///     Extracts a scalar value as text from JSON using the specified path.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="json">The JSON value to search.</param>
    /// <param name="path">The JSON path to the scalar value.</param>
    /// <returns>The scalar value, or <see langword="null" /> when no scalar value is found in lax mode.</returns>
    /// <seealso href="https://learn.microsoft.com/sql/t-sql/functions/json-value-transact-sql">SQL Server documentation for <c>JSON_VALUE</c>.</seealso>
    public static string? JsonValue(this DbFunctions _, object json, string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonValue)));

    /// <summary>
    ///     Extracts an object or array from JSON using the specified path.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="json">The JSON value to search.</param>
    /// <param name="path">The JSON path to the object or array.</param>
    /// <returns>The JSON fragment, or <see langword="null" /> when no object or array is found in lax mode.</returns>
    /// <seealso href="https://learn.microsoft.com/sql/t-sql/functions/json-query-transact-sql">SQL Server documentation for <c>JSON_QUERY</c>.</seealso>
    public static string? JsonQuery(this DbFunctions _, object json, string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonQuery)));
}
