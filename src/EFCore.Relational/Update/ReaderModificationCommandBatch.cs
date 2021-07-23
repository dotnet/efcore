// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A base class for <see cref="ModificationCommandBatch" /> implementations that make use
    ///         of a data reader.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly List<ModificationCommand> _modificationCommands = new();

        /// <summary>
        ///     Creates a new <see cref="ReaderModificationCommandBatch" /> instance.
        /// </summary>
        /// <param name="dependencies"> Service dependencies. </param>
        protected ReaderModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
            CachedCommandText = new StringBuilder();
        }

        /// <summary>
        ///     Service dependencies.
        /// </summary>
        public virtual ModificationCommandBatchFactoryDependencies Dependencies { get; }

        /// <summary>
        ///     The update SQL generator.
        /// </summary>
        protected virtual IUpdateSqlGenerator UpdateSqlGenerator
            => Dependencies.UpdateSqlGenerator;

        /// <summary>
        ///     Gets or sets the cached command text for the commands in the batch.
        /// </summary>
        protected virtual StringBuilder CachedCommandText { get; set; }

        /// <summary>
        ///     The ordinal of the last command for which command text was built.
        /// </summary>
        protected virtual int LastCachedCommandIndex { get; set; }

        /// <summary>
        ///     The list of conceptual insert/update/delete <see cref="ModificationCommands" />s in the batch.
        /// </summary>
        public override IReadOnlyList<ModificationCommand> ModificationCommands
            => _modificationCommands;

        /// <summary>
        ///     The <see cref="ResultSetMapping" />s for each command in <see cref="ModificationCommands" />.
        /// </summary>
        protected virtual IList<ResultSetMapping> CommandResultSet { get; } = new List<ResultSetMapping>();

        /// <summary>
        ///     Adds the given insert/update/delete <see cref="ModificationCommands" /> to the batch.
        /// </summary>
        /// <param name="modificationCommand"> The command to add. </param>
        /// <returns>
        ///     <see langword="true" /> if the command was successfully added; <see langword="false" /> if there was no
        ///     room in the current batch to add the command and it must instead be added to a new batch.
        /// </returns>
        public override bool AddCommand(ModificationCommand modificationCommand)
        {
            Check.NotNull(modificationCommand, nameof(modificationCommand));

            if (ModificationCommands.Count == 0)
            {
                ResetCommandText();
            }

            if (!CanAddCommand(modificationCommand))
            {
                return false;
            }

            _modificationCommands.Add(modificationCommand);
            CommandResultSet.Add(ResultSetMapping.LastInResultSet);

            if (!IsCommandTextValid())
            {
                ResetCommandText();
                _modificationCommands.RemoveAt(_modificationCommands.Count - 1);
                CommandResultSet.RemoveAt(CommandResultSet.Count - 1);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Resets the builder to start building a new batch.
        /// </summary>
        protected virtual void ResetCommandText()
        {
            if (CachedCommandText.Length > 0)
            {
                CachedCommandText = new StringBuilder();
            }

            UpdateSqlGenerator.AppendBatchHeader(CachedCommandText);
            LastCachedCommandIndex = -1;
        }

        /// <summary>
        ///     Checks whether or not a new command can be added to the batch.
        /// </summary>
        /// <param name="modificationCommand"> The command to potentially add. </param>
        /// <returns> <see langword="true" /> if the command can be added; <see langword="false" /> otherwise. </returns>
        protected abstract bool CanAddCommand(ModificationCommand modificationCommand);

        /// <summary>
        ///     Checks whether or not the command text is valid.
        /// </summary>
        /// <returns> <see langword="true" /> if the command text is valid; <see langword="false" /> otherwise. </returns>
        protected abstract bool IsCommandTextValid();

        /// <summary>
        ///     Gets the command text for all the commands in the current batch and also caches it
        ///     on <see cref="CachedCommandText" />.
        /// </summary>
        /// <returns> The command text. </returns>
        protected virtual string GetCommandText()
        {
            for (var i = LastCachedCommandIndex + 1; i < ModificationCommands.Count; i++)
            {
                UpdateCachedCommandText(i);
            }

            return CachedCommandText.ToString();
        }

        /// <summary>
        ///     Updates the command text for the command at the given position in the
        ///     <see cref="ModificationCommands" /> list.
        /// </summary>
        /// <param name="commandPosition"> The position of the command to generate command text for. </param>
        protected virtual void UpdateCachedCommandText(int commandPosition)
        {
            var newModificationCommand = ModificationCommands[commandPosition];

            switch (newModificationCommand.EntityState)
            {
                case EntityState.Added:
                    CommandResultSet[commandPosition] =
                        UpdateSqlGenerator.AppendInsertOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
                case EntityState.Modified:
                    CommandResultSet[commandPosition] =
                        UpdateSqlGenerator.AppendUpdateOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
                case EntityState.Deleted:
                    CommandResultSet[commandPosition] =
                        UpdateSqlGenerator.AppendDeleteOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
            }

            LastCachedCommandIndex = commandPosition;
        }

        /// <summary>
        ///     Gets the total number of parameters needed for the batch.
        /// </summary>
        /// <returns> The total parameter count. </returns>
        protected virtual int GetParameterCount()
            => ModificationCommands.Sum(c => c.ColumnModifications.Count);

        /// <summary>
        ///     Generates a <see cref="RawSqlCommand" /> for the batch.
        /// </summary>
        /// <returns> The command. </returns>
        protected virtual RawSqlCommand CreateStoreCommand()
        {
            var commandBuilder = Dependencies.CommandBuilderFactory
                .Create()
                .Append(GetCommandText());

            var parameterValues = new Dictionary<string, object?>(GetParameterCount());

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var commandIndex = 0; commandIndex < ModificationCommands.Count; commandIndex++)
            {
                var command = ModificationCommands[commandIndex];
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var columnIndex = 0; columnIndex < command.ColumnModifications.Count; columnIndex++)
                {
                    var columnModification = command.ColumnModifications[columnIndex];
                    if (columnModification.UseCurrentValueParameter)
                    {
                        commandBuilder.AddParameter(
                            columnModification.ParameterName,
                            Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName),
                            columnModification.TypeMapping!,
                            columnModification.IsNullable);

                        parameterValues.Add(columnModification.ParameterName, columnModification.Value);
                    }

                    if (columnModification.UseOriginalValueParameter)
                    {
                        commandBuilder.AddParameter(
                            columnModification.OriginalParameterName,
                            Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName),
                            columnModification.TypeMapping!,
                            columnModification.IsNullable);

                        parameterValues.Add(columnModification.OriginalParameterName, columnModification.OriginalValue);
                    }
                }
            }

            return new RawSqlCommand(commandBuilder.Build(), parameterValues);
        }

        /// <summary>
        ///     Executes the command generated by <see cref="CreateStoreCommand" /> against a
        ///     database using the given connection.
        /// </summary>
        /// <param name="connection"> The connection to the database to update. </param>
        public override void Execute(IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var storeCommand = CreateStoreCommand();

            try
            {
                using var dataReader = storeCommand.RelationalCommand.ExecuteReader(
                    new RelationalCommandParameterObject(
                        connection,
                        storeCommand.ParameterValues,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.Logger));
                Consume(dataReader);
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex,
                    ModificationCommands.SelectMany(c => c.Entries).ToList());
            }
        }

        /// <summary>
        ///     Executes the command generated by <see cref="CreateStoreCommand" /> against a
        ///     database using the given connection.
        /// </summary>
        /// <param name="connection"> The connection to the database to update. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public override async Task ExecuteAsync(
            IRelationalConnection connection,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(connection, nameof(connection));

            var storeCommand = CreateStoreCommand();

            try
            {
                await using var dataReader = await storeCommand.RelationalCommand.ExecuteReaderAsync(
                    new RelationalCommandParameterObject(
                        connection,
                        storeCommand.ParameterValues,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.Logger),
                    cancellationToken).ConfigureAwait(false);
                await ConsumeAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
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
        /// <param name="reader"> The data reader. </param>
        protected abstract void Consume(RelationalDataReader reader);

        /// <summary>
        ///     Consumes the data reader created by <see cref="ExecuteAsync" />.
        /// </summary>
        /// <param name="reader"> The data reader. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        protected abstract Task ConsumeAsync(
            RelationalDataReader reader,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Creates the <see cref="IRelationalValueBufferFactory" /> that will be used for creating a
        ///     <see cref="ValueBuffer" /> to consume the data reader.
        /// </summary>
        /// <param name="columnModifications">
        ///     The list of <see cref="ColumnModification" />s for all the columns
        ///     being modified such that a ValueBuffer with appropriate slots can be created.
        /// </param>
        /// <returns> The factory. </returns>
        protected virtual IRelationalValueBufferFactory CreateValueBufferFactory(
            IReadOnlyList<ColumnModification> columnModifications)
            => Dependencies.ValueBufferFactoryFactory
                .Create(
                    Check.NotNull(columnModifications, nameof(columnModifications))
                        .Where(c => c.IsRead)
                        .Select(c => new TypeMaterializationInfo(c.Property!.ClrType, c.Property, c.TypeMapping!))
                        .ToArray());
    }
}
