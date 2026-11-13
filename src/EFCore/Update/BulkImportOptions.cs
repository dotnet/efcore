// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     Options for configuring bulk import operations.
/// </summary>
public record BulkImportOptions
{
    /// <summary>
    ///     Default options instance.
    /// </summary>
    public static BulkImportOptions Default { get; } = new();

    /// <summary>
    ///     The number of rows in each batch. If <see langword="null" />, provider default is used.
    /// </summary>
    public int? BatchSize { get; init; }

    /// <summary>
    ///     The timeout in seconds for the operation to complete before timing out. If <see langword="null" />, provider default is used.
    /// </summary>
    public int? Timeout { get; init; }

    /// <summary>
    ///     Whether identity/auto-increment values should be preserved from the source entities.
    ///     If <see langword="false" />, the database will generate identity values.
    /// </summary>
    public bool KeepIdentity { get; init; }
}
