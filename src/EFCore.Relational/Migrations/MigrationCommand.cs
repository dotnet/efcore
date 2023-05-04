// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Represents a command ready to be sent to the database to migrate it.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public class MigrationCommand
{
    private readonly IRelationalCommand _relationalCommand;
    private readonly DbContext? _context;

    /// <summary>
    ///     Creates a new instance of the command.
    /// </summary>
    /// <param name="relationalCommand">The underlying <see cref="IRelationalCommand" /> that will be used to execute the command.</param>
    /// <param name="context">The current <see cref="DbContext" /> or <see langword="null" /> if not known.</param>
    /// <param name="logger">The command logger.</param>
    /// <param name="transactionSuppressed">Indicates whether or not transactions should be suppressed while executing the command.</param>
    public MigrationCommand(
        IRelationalCommand relationalCommand,
        DbContext? context,
        IRelationalCommandDiagnosticsLogger logger,
        bool transactionSuppressed = false)
    {
        _relationalCommand = relationalCommand;
        _context = context;
        CommandLogger = logger;
        TransactionSuppressed = transactionSuppressed;
    }

    /// <summary>
    ///     Indicates whether or not transactions should be suppressed while executing the command.
    /// </summary>
    public virtual bool TransactionSuppressed { get; }

    /// <summary>
    ///     The SQL command text that will be executed against the database.
    /// </summary>
    public virtual string CommandText
        => _relationalCommand.CommandText;

    /// <summary>
    ///     The associated command logger.
    /// </summary>
    public virtual IRelationalCommandDiagnosticsLogger CommandLogger { get; }

    /// <summary>
    ///     Executes the command and returns the number of rows affected.
    /// </summary>
    /// <param name="connection">The connection to execute against.</param>
    /// <param name="parameterValues">The values for the parameters, or <see langword="null" /> if the command has no parameters.</param>
    /// <returns>The number of rows affected.</returns>
    public virtual int ExecuteNonQuery(
        IRelationalConnection connection,
        IReadOnlyDictionary<string, object?>? parameterValues = null)
        => _relationalCommand.ExecuteNonQuery(
            new RelationalCommandParameterObject(
                connection,
                parameterValues,
                null,
                _context,
                CommandLogger, CommandSource.Migrations));

    /// <summary>
    ///     Executes the command and returns the number of rows affected.
    /// </summary>
    /// <param name="connection">The connection to execute against.</param>
    /// <param name="parameterValues">The values for the parameters, or <see langword="null" /> if the command has no parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows affected. </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task<int> ExecuteNonQueryAsync(
        IRelationalConnection connection,
        IReadOnlyDictionary<string, object?>? parameterValues = null,
        CancellationToken cancellationToken = default)
        => _relationalCommand.ExecuteNonQueryAsync(
            new RelationalCommandParameterObject(
                connection,
                parameterValues,
                null,
                _context,
                CommandLogger, CommandSource.Migrations),
            cancellationToken);
}
