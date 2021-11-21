// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a function parameter.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IDbFunctionParameter : IReadOnlyDbFunctionParameter, IAnnotatable
{
    /// <summary>
    ///     Gets the store type of this parameter.
    /// </summary>
    new string StoreType { get; }

    /// <summary>
    ///     Gets the function to which this parameter belongs.
    /// </summary>
    new IDbFunction Function { get; }

    /// <summary>
    ///     Gets the associated <see cref="IStoreFunctionParameter" />.
    /// </summary>
    IStoreFunctionParameter StoreFunctionParameter { get; }
}
