// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         A parameter object for the execution methods on <see cref="RelationalCommand" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public readonly record struct RelationalCommandParameterObject
{
    /// <summary>
    ///     <para>
    ///         Creates a new parameter object for the given parameters.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="connection">The connection on which the command will execute.</param>
    /// <param name="parameterValues">The SQL parameter values to use, or <see langword="null" /> if none.</param>
    /// <param name="readerColumns">The expected columns if the reader needs to be buffered, or <see langword="null" /> otherwise.</param>
    /// <param name="context">The current <see cref="DbContext" /> instance, or <see langword="null" /> if it is not known.</param>
    /// <param name="logger">A logger, or <see langword="null" /> if no logger is available.</param>
    public RelationalCommandParameterObject(
        IRelationalConnection connection,
        IReadOnlyDictionary<string, object?>? parameterValues,
        IReadOnlyList<ReaderColumn>? readerColumns,
        DbContext? context,
        IRelationalCommandDiagnosticsLogger? logger)
        : this(connection, parameterValues, readerColumns, context, logger, detailedErrorsEnabled: false)
    {
    }

    /// <summary>
    ///     <para>
    ///         Creates a new parameter object for the given parameters.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="connection">The connection on which the command will execute.</param>
    /// <param name="parameterValues">The SQL parameter values to use, or null if none.</param>
    /// <param name="readerColumns">The expected columns if the reader needs to be buffered, or null otherwise.</param>
    /// <param name="context">The current <see cref="DbContext" /> instance, or null if it is not known.</param>
    /// <param name="logger">A logger, or null if no logger is available.</param>
    /// <param name="commandSource">Source of the command.</param>
    public RelationalCommandParameterObject(
        IRelationalConnection connection,
        IReadOnlyDictionary<string, object?>? parameterValues,
        IReadOnlyList<ReaderColumn>? readerColumns,
        DbContext? context,
        IRelationalCommandDiagnosticsLogger? logger,
        CommandSource commandSource)
        : this(connection, parameterValues, readerColumns, context, logger, detailedErrorsEnabled: false, commandSource)
    {
    }

    /// <summary>
    ///     <para>
    ///         Creates a new parameter object for the given parameters.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="connection">The connection on which the command will execute.</param>
    /// <param name="parameterValues">The SQL parameter values to use, or null if none.</param>
    /// <param name="readerColumns">The expected columns if the reader needs to be buffered, or null otherwise.</param>
    /// <param name="context">The current <see cref="DbContext" /> instance, or null if it is not known.</param>
    /// <param name="logger">A logger, or null if no logger is available.</param>
    /// <param name="detailedErrorsEnabled">A value indicating if detailed errors are enabled.</param>
    public RelationalCommandParameterObject(
        IRelationalConnection connection,
        IReadOnlyDictionary<string, object?>? parameterValues,
        IReadOnlyList<ReaderColumn>? readerColumns,
        DbContext? context,
        IRelationalCommandDiagnosticsLogger? logger,
        bool detailedErrorsEnabled)
        : this(connection, parameterValues, readerColumns, context, logger, detailedErrorsEnabled, CommandSource.Unknown)
    {
    }

    /// <summary>
    ///     <para>
    ///         Creates a new parameter object for the given parameters.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="connection">The connection on which the command will execute.</param>
    /// <param name="parameterValues">The SQL parameter values to use, or null if none.</param>
    /// <param name="readerColumns">The expected columns if the reader needs to be buffered, or null otherwise.</param>
    /// <param name="context">The current <see cref="DbContext" /> instance, or null if it is not known.</param>
    /// <param name="logger">A logger, or null if no logger is available.</param>
    /// <param name="detailedErrorsEnabled">A value indicating if detailed errors are enabled.</param>
    /// <param name="commandSource">Source of the command.</param>
    public RelationalCommandParameterObject(
        IRelationalConnection connection,
        IReadOnlyDictionary<string, object?>? parameterValues,
        IReadOnlyList<ReaderColumn?>? readerColumns,
        DbContext? context,
        IRelationalCommandDiagnosticsLogger? logger,
        bool detailedErrorsEnabled,
        CommandSource commandSource)
    {
        Connection = connection;
        ParameterValues = parameterValues;
        ReaderColumns = readerColumns;
        Context = context;
        Logger = logger;
        DetailedErrorsEnabled = detailedErrorsEnabled;
        CommandSource = commandSource;
    }

    /// <summary>
    ///     The connection on which the command will execute.
    /// </summary>
    public IRelationalConnection Connection { get; }

    /// <summary>
    ///     The SQL parameter values to use, or <see langword="null" /> if none.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? ParameterValues { get; }

    /// <summary>
    ///     The expected columns if the reader needs to be buffered, or <see langword="null" /> otherwise.
    /// </summary>
    public IReadOnlyList<ReaderColumn?>? ReaderColumns { get; }

    /// <summary>
    ///     The current <see cref="DbContext" /> instance, or <see langword="null" /> if it is not known.
    /// </summary>
    public DbContext? Context { get; }

    /// <summary>
    ///     A logger, or <see langword="null" /> if no logger is available.
    /// </summary>
    public IRelationalCommandDiagnosticsLogger? Logger { get; }

    /// <summary>
    ///     A value indicating if detailed errors are enabled.
    /// </summary>
    public bool DetailedErrorsEnabled { get; }

    /// <summary>
    ///     Source of the command.
    /// </summary>
    public CommandSource CommandSource { get; }
}
