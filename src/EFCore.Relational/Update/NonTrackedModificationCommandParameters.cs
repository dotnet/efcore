// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Parameters for creating a <see cref="INonTrackedModificationCommand" /> instance.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public readonly record struct NonTrackedModificationCommandParameters
{
    /// <summary>
    ///     Creates a new <see cref="NonTrackedModificationCommandParameters" /> instance.
    /// </summary>
    /// <param name="tableName">The name of the table containing the data to be modified.</param>
    /// <param name="schemaName">The schema containing the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    public NonTrackedModificationCommandParameters(
        string tableName,
        string? schemaName,
        bool sensitiveLoggingEnabled)
    {
        Table = null;
        TableName = tableName;
        Schema = schemaName;
        SensitiveLoggingEnabled = sensitiveLoggingEnabled;
    }

    /// <summary>
    ///     Creates a new <see cref="ModificationCommandParameters" /> instance.
    /// </summary>
    /// <param name="table">The table containing the data to be modified.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    public NonTrackedModificationCommandParameters(
        ITable table,
        bool sensitiveLoggingEnabled)
    {
        Table = table;
        TableName = table.Name;
        Schema = table.Schema;
        SensitiveLoggingEnabled = sensitiveLoggingEnabled;
    }

    /// <summary>
    ///     The name of the table containing the data to be modified.
    /// </summary>
    public string TableName { get; init; }

    /// <summary>
    ///     The schema containing the table, or <see langword="null" /> to use the default schema.
    /// </summary>
    public string? Schema { get; init; }

    /// <summary>
    ///     The table containing the data to be modified.
    /// </summary>
    public ITable? Table { get; init; }

    /// <summary>
    ///     Indicates whether potentially sensitive data (e.g. database values) can be logged.
    /// </summary>
    public bool SensitiveLoggingEnabled { get; init; }
}
