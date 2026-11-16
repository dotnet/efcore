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
    ///     The timeout in seconds for the operation to complete before timing out. If <see langword="null" />, the context or connection default is used.
    /// </summary>
    public int? Timeout { get; init; }

    /// <summary>
    ///     Whether identity/auto-increment values should be preserved from the source entities.
    ///     If <see langword="false" />, the database will generate identity values.
    /// </summary>
    public bool KeepIdentity { get; init; }

    /// <summary>
    ///     Whether to obtain a table lock for the duration of the bulk copy operation.
    /// </summary>
    public bool TableLock { get; init; }

    /// <summary>
    ///     Whether insert triggers should be executed for the imported entities.
    /// </summary>
    public bool FireTriggers { get; init; }

    /// <summary>
    ///     An optional destination table name override (e.g., when importing into a staging or temporary table).
    /// </summary>
    public string? DestinationTableName { get; init; }
}
