// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         A base class for <see cref="ModificationCommandBatch" /> implementations that make use
///         of a data reader.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
{
    private readonly List<IReadOnlyModificationCommand> _modificationCommands = new();
    private readonly int _batchHeaderLength;
    private bool _requiresTransaction = true;
    private int _sqlBuilderPosition, _commandResultSetCount, _resultsPositionalMappingEnabledLength;
    private int _pendingParameters;

    /// <summary>
    ///     Creates a new <see cref="ReaderModificationCommandBatch" /> instance.
    /// </summary>
    /// <param name="dependencies">Service dependencies.</param>
    protected ReaderModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies)
    {
        Dependencies = dependencies;

        RelationalCommandBuilder = dependencies.CommandBuilderFactory.Create();

        UpdateSqlGenerator = dependencies.UpdateSqlGenerator;
        UpdateSqlGenerator.AppendBatchHeader(SqlBuilder);
        _batchHeaderLength = SqlBuilder.Length;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual ModificationCommandBatchFactoryDependencies Dependencies { get; }

    /// <summary>
    ///     The update SQL generator.
    /// </summary>
    protected virtual IUpdateSqlGenerator UpdateSqlGenerator { get; }

    /// <summary>
    ///     Gets the relational command builder for the commands in the batch.
    /// </summary>
    protected virtual IRelationalCommandBuilder RelationalCommandBuilder { get; }

    /// <summary>
    ///     The maximum number of <see cref="ModificationCommand"/> instances that can be added to a single batch.
    /// </summary>
    protected virtual int MaxBatchSize
        => 1000;

    /// <summary>
    ///     Gets the command text builder for the commands in the batch.
    /// </summary>
    protected virtual StringBuilder SqlBuilder { get; } = new();

    /// <summary>
    ///     Gets the parameter values for the commands in the batch.
    /// </summary>
    protected virtual Dictionary<string, object?> ParameterValues { get; } = new();

    /// <summary>
    ///     The list of conceptual insert/update/delete <see cref="ModificationCommands" />s in the batch.
    /// </summary>
    public override IReadOnlyList<IReadOnlyModificationCommand> ModificationCommands
        => _modificationCommands;

    /// <summary>
    ///     The <see cref="ResultSetMapping" />s for each command in <see cref="ModificationCommands" />.
    /// </summary>
    protected virtual IList<ResultSetMapping> CommandResultSet { get; } = new List<ResultSetMapping>();

    /// <summary>
    ///     When rows with database-generated values are returned in non-deterministic ordering, it is necessary to project out a synthetic
    ///     position value, in order to look up the correct <see cref="ModificationCommand" /> and propagate the values. When this array
    ///     isn't <see langword="null" />, it determines whether the current result row contains such a position value.
    /// </summary>
    protected virtual BitArray? ResultsPositionalMappingEnabled { get; set; }

    /// <summary>
    ///     The store command generated from this batch when <see cref="Complete" /> is called.
    /// </summary>
    protected virtual RawSqlCommand? StoreCommand { get; set; }

    /// <inheritdoc />
    public override bool TryAddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (StoreCommand is not null)
        {
            throw new InvalidOperationException(RelationalStrings.ModificationCommandBatchAlreadyComplete);
        }

        if (_modificationCommands.Count >= MaxBatchSize)
        {
            return false;
        }

        _sqlBuilderPosition = SqlBuilder.Length;
        _commandResultSetCount = CommandResultSet.Count;
        _pendingParameters = 0;
        _resultsPositionalMappingEnabledLength = ResultsPositionalMappingEnabled?.Length ?? 0;

        AddCommand(modificationCommand);
        _modificationCommands.Add(modificationCommand);

        // Check if the batch is still valid after having added the command (e.g. have we bypassed a maximum CommandText size?)
        // A batch with only one command is always considered valid (otherwise we'd get an endless loop); allow the batch to fail
        // server-side.
        if (IsValid() || _modificationCommands.Count == 1)
        {
            return true;
        }

        RollbackLastCommand();

        // The command's column modifications had their parameter names generated, that needs to be rolled back as well.
        foreach (var columnModification in modificationCommand.ColumnModifications)
        {
            columnModification.ResetParameterNames();
        }

        return false;
    }

    /// <summary>
    ///     Rolls back the last command added. Used when adding a command caused the batch to become invalid (e.g. CommandText too long).
    /// </summary>
    protected virtual void RollbackLastCommand()
    {
        _modificationCommands.RemoveAt(_modificationCommands.Count - 1);

        SqlBuilder.Length = _sqlBuilderPosition;

        while (CommandResultSet.Count > _commandResultSetCount)
        {
            CommandResultSet.RemoveAt(CommandResultSet.Count - 1);
        }

        if (ResultsPositionalMappingEnabled is not null)
        {
            ResultsPositionalMappingEnabled.Length = _resultsPositionalMappingEnabledLength;
        }

        for (var i = 0; i < _pendingParameters; i++)
        {
            var parameterIndex = RelationalCommandBuilder.Parameters.Count - 1;
            var parameter = RelationalCommandBuilder.Parameters[parameterIndex];

            RelationalCommandBuilder.RemoveParameterAt(parameterIndex);
            ParameterValues.Remove(parameter.InvariantName);
        }
    }

    /// <summary>
    ///     Whether any SQL has already been added to the batch command text.
    /// </summary>
    protected virtual bool IsCommandTextEmpty
        => SqlBuilder.Length == _batchHeaderLength;

    /// <inheritdoc />
    public override bool RequiresTransaction
        => _requiresTransaction;

    /// <summary>
    ///     Sets whether the batch requires a transaction in order to execute correctly.
    /// </summary>
    /// <param name="requiresTransaction">Whether the batch requires a transaction in order to execute correctly.</param>
    protected virtual void SetRequiresTransaction(bool requiresTransaction)
        => _requiresTransaction = requiresTransaction;

    /// <summary>
    ///     Checks whether the command text is valid.
    /// </summary>
    /// <returns><see langword="true" /> if the command text is valid; <see langword="false" /> otherwise.</returns>
    protected virtual bool IsValid()
        => true;

    /// <summary>
    ///     Adds Updates the command text for the command at the given position in the <see cref="ModificationCommands" /> list.
    /// </summary>
    /// <param name="modificationCommand">The command to add.</param>
    protected virtual void AddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        bool requiresTransaction;

        var commandPosition = CommandResultSet.Count;

        switch (modificationCommand.EntityState)
        {
            case EntityState.Added:
                CommandResultSet.Add(
                    UpdateSqlGenerator.AppendInsertOperation(
                        SqlBuilder, modificationCommand, commandPosition, out requiresTransaction));
                break;
            case EntityState.Modified:
                CommandResultSet.Add(
                    UpdateSqlGenerator.AppendUpdateOperation(
                        SqlBuilder, modificationCommand, commandPosition, out requiresTransaction));
                break;
            case EntityState.Deleted:
                CommandResultSet.Add(
                    UpdateSqlGenerator.AppendDeleteOperation(
                        SqlBuilder, modificationCommand, commandPosition, out requiresTransaction));
                break;

            default:
                throw new InvalidOperationException(
                    RelationalStrings.ModificationCommandInvalidEntityState(
                        modificationCommand.Entries[0].EntityType,
                        modificationCommand.EntityState));
        }

        AddParameters(modificationCommand);

        _requiresTransaction = commandPosition > 0 || requiresTransaction;
    }

    /// <inheritdoc />
    public override void Complete()
    {
        if (StoreCommand is not null)
        {
            throw new InvalidOperationException(RelationalStrings.ModificationCommandBatchAlreadyComplete);
        }

        // Some database have a mode where autocommit is off, and so executing a command outside of an explicit transaction implicitly
        // creates a new transaction (which needs to be explicitly committed).
        // The below is a hook for allowing providers to turn autocommit on, in case it's off.
        if (!RequiresTransaction)
        {
            UpdateSqlGenerator.PrependEnsureAutocommit(SqlBuilder);
        }

        RelationalCommandBuilder.Append(SqlBuilder.ToString());

        StoreCommand = new RawSqlCommand(RelationalCommandBuilder.Build(), ParameterValues);
    }

    /// <summary>
    ///     Adds parameters for all column modifications in the given <paramref name="modificationCommand" /> to the relational command
    ///     being built for this batch.
    /// </summary>
    /// <param name="modificationCommand">The modification command for which to add parameters.</param>
    protected virtual void AddParameters(IReadOnlyModificationCommand modificationCommand)
    {
        foreach (var columnModification in modificationCommand.ColumnModifications)
        {
            AddParameter(columnModification);
        }
    }

    /// <summary>
    ///     Adds a parameter for the given <paramref name="columnModification" /> to the relational command being built for this batch.
    /// </summary>
    /// <param name="columnModification">The column modification for which to add parameters.</param>
    protected virtual void AddParameter(IColumnModification columnModification)
    {
        if (columnModification.UseCurrentValueParameter)
        {
            RelationalCommandBuilder.AddParameter(
                columnModification.ParameterName,
                Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName),
                columnModification.TypeMapping!,
                columnModification.IsNullable);

            ParameterValues.Add(columnModification.ParameterName, columnModification.Value);

            _pendingParameters++;
        }

        if (columnModification.UseOriginalValueParameter)
        {
            RelationalCommandBuilder.AddParameter(
                columnModification.OriginalParameterName,
                Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName),
                columnModification.TypeMapping!,
                columnModification.IsNullable);

            ParameterValues.Add(columnModification.OriginalParameterName, columnModification.OriginalValue);

            _pendingParameters++;
        }
    }

    /// <summary>
    ///     Executes the command generated by this batch against a database using the given connection.
    /// </summary>
    /// <param name="connection">The connection to the database to update.</param>
    public override void Execute(IRelationalConnection connection)
    {
        if (StoreCommand is null)
        {
            throw new InvalidOperationException(RelationalStrings.ModificationCommandBatchNotComplete);
        }

        try
        {
            using var dataReader = StoreCommand.RelationalCommand.ExecuteReader(
                new RelationalCommandParameterObject(
                    connection,
                    StoreCommand.ParameterValues,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.Logger, CommandSource.SaveChanges));
            Consume(dataReader);
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands.SelectMany(c => c.Entries).ToList());
        }
    }

    /// <summary>
    ///     Executes the command generated by this batch against a database using the given connection.
    /// </summary>
    /// <param name="connection">The connection to the database to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public override async Task ExecuteAsync(
        IRelationalConnection connection,
        CancellationToken cancellationToken = default)
    {
        if (StoreCommand is null)
        {
            throw new InvalidOperationException(RelationalStrings.ModificationCommandBatchNotComplete);
        }

        try
        {
            var dataReader = await StoreCommand.RelationalCommand.ExecuteReaderAsync(
                new RelationalCommandParameterObject(
                    connection,
                    StoreCommand.ParameterValues,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.Logger, CommandSource.SaveChanges),
                cancellationToken).ConfigureAwait(false);

            await using var _ = dataReader.ConfigureAwait(false);
            await ConsumeAsync(dataReader, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands.SelectMany(c => c.Entries).ToList());
        }
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="Execute" />.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    protected abstract void Consume(RelationalDataReader reader);

    /// <summary>
    ///     Consumes the data reader created by <see cref="ExecuteAsync" />.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected abstract Task ConsumeAsync(
        RelationalDataReader reader,
        CancellationToken cancellationToken = default);
}
