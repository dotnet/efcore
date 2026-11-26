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
    ///     Returns a value indicating whether the given value is valid JSON.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A <see langword="null" /> value is not considered valid JSON, and so causes this function to return <see langword="false" />.
    ///     </para>
    ///     <para>
    ///         Note that not all databases support this function; consult your database's documentation for more information.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="value">The value to check for valid JSON.</param>
    public static bool IsJson(this DbFunctions _, object? value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsJson)));
}
