// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a function parameter.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IMutableDbFunctionParameter : IReadOnlyDbFunctionParameter, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the function to which this parameter belongs.
    /// </summary>
    new IMutableDbFunction Function { get; }

    /// <summary>
    ///     Gets or sets the store type of this parameter.
    /// </summary>
    new string? StoreType { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="RelationalTypeMapping" /> for this parameter.
    /// </summary>
    new RelationalTypeMapping? TypeMapping { get; set; }
}
