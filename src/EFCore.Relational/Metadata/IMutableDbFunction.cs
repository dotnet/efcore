// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relational database function in an model in
///     the form that can be mutated while the model is being built.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IMutableDbFunction : IReadOnlyDbFunction, IMutableAnnotatable
{
    /// <summary>
    ///     Gets or sets the name of the function in the database.
    /// </summary>
    new string Name { get; set; }

    /// <summary>
    ///     Gets or sets the schema of the function in the database.
    /// </summary>
    new string? Schema { get; set; }

    /// <summary>
    ///     Gets or sets the value indicating whether the database function is built-in or not.
    /// </summary>
    new bool IsBuiltIn { get; set; }

    /// <summary>
    ///     Gets or sets the value indicating whether the database function can return null value or not.
    /// </summary>
    new bool IsNullable { get; set; }

    /// <summary>
    ///     Gets or sets the store type of the function in the database.
    /// </summary>
    new string? StoreType { get; set; }

    /// <summary>
    ///     Gets or sets the type mapping of the function in the database.
    /// </summary>
    new RelationalTypeMapping? TypeMapping { get; set; }

    /// <summary>
    ///     Gets the model in which this function is defined.
    /// </summary>
    new IMutableModel Model { get; }

    /// <summary>
    ///     Gets the parameters for this function
    /// </summary>
    new IReadOnlyList<IMutableDbFunctionParameter> Parameters { get; }

    /// <summary>
    ///     Gets or sets the translation callback for performing custom translation of the method call into a SQL expression fragment.
    /// </summary>
    new Func<IReadOnlyList<SqlExpression>, SqlExpression>? Translation { get; set; }
}
