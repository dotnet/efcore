// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Indicates which values should be appended to the store type name.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public enum StoreTypePostfix
{
    /// <summary>
    ///     Append nothing.
    /// </summary>
    None,

    /// <summary>
    ///     Append only the size.
    /// </summary>
    Size,

    /// <summary>
    ///     Append only the precision.
    /// </summary>
    Precision,

    /// <summary>
    ///     Append the precision and scale.
    /// </summary>
    PrecisionAndScale
}
