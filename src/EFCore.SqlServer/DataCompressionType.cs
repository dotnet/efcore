// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates type of data compression used on a index.
/// </summary>
/// <remarks>
///     See <see href="https://docs.microsoft.com/sql/relational-databases/data-compression">Data Compression</see> for more information on
///     data compression.
/// </remarks>
public enum DataCompressionType
{
    /// <summary>
    ///     Index is not compressed.
    /// </summary>
    None,

    /// <summary>
    ///     Index is compressed by using row compression.
    /// </summary>
    Row,

    /// <summary>
    ///     Index is compressed by using page compression.
    /// </summary>
    Page
}
