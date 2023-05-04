// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Parameters for creating a <see cref="IModificationCommand" /> instance.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public readonly record struct ModificationCommandParameters
{
    /// <summary>
    ///     Creates a new <see cref="ModificationCommandParameters" /> instance.
    /// </summary>
    /// <param name="table">The table containing the data to be modified.</param>
    /// <param name="storeStoredProcedure">The stored procedure to use for updating the data.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    /// <param name="detailedErrorsEnabled">Indicates whether detailed errors should be logged.</param>
    /// <param name="comparer">An <see cref="IComparer{T}" /> for <see cref="IUpdateEntry" />.</param>
    /// <param name="generateParameterName">A delegate to generate parameter names.</param>
    /// <param name="logger">An <see cref="IDiagnosticsLogger{TLoggerCategory}" /> for <see cref="DbLoggerCategory.Update" />.</param>
    public ModificationCommandParameters(
        ITable table,
        IStoreStoredProcedure? storeStoredProcedure,
        bool sensitiveLoggingEnabled,
        bool detailedErrorsEnabled = false,
        IComparer<IUpdateEntry>? comparer = null,
        Func<string>? generateParameterName = null,
        IDiagnosticsLogger<DbLoggerCategory.Update>? logger = null)
    {
        Table = table;
        TableName = table.Name;
        Schema = table.Schema;
        StoreStoredProcedure = storeStoredProcedure;
        GenerateParameterName = generateParameterName;
        SensitiveLoggingEnabled = sensitiveLoggingEnabled;
        DetailedErrorsEnabled = detailedErrorsEnabled;
        Comparer = comparer;
        Logger = logger;
    }

    /// <summary>
    ///     Creates a new <see cref="ModificationCommandParameters" /> instance.
    /// </summary>
    /// <param name="table">The table containing the data to be modified.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    /// <param name="detailedErrorsEnabled">Indicates whether detailed errors should be logged.</param>
    /// <param name="comparer">An <see cref="IComparer{T}" /> for <see cref="IUpdateEntry" />.</param>
    /// <param name="generateParameterName">A delegate to generate parameter names.</param>
    /// <param name="logger">An <see cref="IDiagnosticsLogger{TLoggerCategory}" /> for <see cref="DbLoggerCategory.Update" />.</param>
    public ModificationCommandParameters(
        ITable table,
        bool sensitiveLoggingEnabled,
        bool detailedErrorsEnabled = false,
        IComparer<IUpdateEntry>? comparer = null,
        Func<string>? generateParameterName = null,
        IDiagnosticsLogger<DbLoggerCategory.Update>? logger = null)
        : this(table, storeStoredProcedure: null, sensitiveLoggingEnabled, detailedErrorsEnabled, comparer, generateParameterName, logger)
    {
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
    ///     The stored procedure to use for updating the data.
    /// </summary>
    public IStoreStoredProcedure? StoreStoredProcedure { get; }

    /// <summary>
    ///     A delegate to generate parameter names.
    /// </summary>
    public Func<string>? GenerateParameterName { get; init; }

    /// <summary>
    ///     Indicates whether potentially sensitive data (e.g. database values) can be logged.
    /// </summary>
    public bool SensitiveLoggingEnabled { get; init; }

    /// <summary>
    ///     Indicates whether detailed errors should be logged.
    /// </summary>
    public bool DetailedErrorsEnabled { get; init; }

    /// <summary>
    ///     An <see cref="IComparer{T}" /> for <see cref="IUpdateEntry" />.
    /// </summary>
    public IComparer<IUpdateEntry>? Comparer { get; init; }

    /// <summary>
    ///     A <see cref="IDiagnosticsLogger{T}" /> for <see cref="DbLoggerCategory.Update" />.
    /// </summary>
    public IDiagnosticsLogger<DbLoggerCategory.Update>? Logger { get; init; }
}
