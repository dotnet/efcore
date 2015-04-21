// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();
        private readonly List<bool> _resultSetEnd = new List<bool>();
        protected StringBuilder CachedCommandText { get; set; }
        protected int LastCachedCommandIndex;

        protected ReaderModificationCommandBatch(
            [NotNull] ISqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override IReadOnlyList<ModificationCommand> ModificationCommands => _modificationCommands;

        // contains true if the command at the corresponding index is the last command in its result set
        // the last value will not be read
        protected IList<bool> ResultSetEnds => _resultSetEnd;

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
            _resultSetEnd.Add(true);

            if (!IsCommandTextValid())
            {
                ResetCommandText();
                _modificationCommands.RemoveAt(_modificationCommands.Count - 1);
                _resultSetEnd.RemoveAt(_resultSetEnd.Count - 1);
                return false;
            }

            return true;
        }

        protected virtual void ResetCommandText()
        {
            CachedCommandText = new StringBuilder();
            SqlGenerator.AppendBatchHeader(CachedCommandText);
            LastCachedCommandIndex = -1;
        }

        protected abstract bool CanAddCommand([NotNull] ModificationCommand modificationCommand);

        protected abstract bool IsCommandTextValid();

        protected virtual string GetCommandText()
        {
            for (var i = LastCachedCommandIndex + 1; i < ModificationCommands.Count; i++)
            {
                UpdateCachedCommandText(i);
            }

            return CachedCommandText.ToString();
        }

        protected virtual void UpdateCachedCommandText(int commandPosition)
        {
            var newModificationCommand = ModificationCommands[commandPosition];

            switch (newModificationCommand.EntityState)
            {
                case EntityState.Added:
                    SqlGenerator.AppendInsertOperation(CachedCommandText, newModificationCommand);
                    break;
                case EntityState.Modified:
                    SqlGenerator.AppendUpdateOperation(CachedCommandText, newModificationCommand);
                    break;
                case EntityState.Deleted:
                    SqlGenerator.AppendDeleteOperation(CachedCommandText, newModificationCommand);
                    break;
            }

            LastCachedCommandIndex = commandPosition;
        }

        protected virtual DbCommand CreateStoreCommand(
            [NotNull] string commandText,
            [NotNull] DbTransaction transaction,
            [NotNull] IRelationalTypeMapper typeMapper,
            int? commandTimeout)
        {
            var command = transaction.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            command.Transaction = transaction;

            if (commandTimeout != null)
            {
                command.CommandTimeout = (int)commandTimeout;
            }

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                PopulateParameters(command, columnModification, typeMapper);
            }

            return command;
        }

        public override int Execute(
            RelationalTransaction transaction,
            IRelationalTypeMapper typeMapper,
            DbContext context,
            ILogger logger)
        {
            Check.NotNull(transaction, nameof(transaction));
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(context, nameof(context));
            Check.NotNull(logger, nameof(logger));

            var commandText = GetCommandText();

            Debug.Assert(ResultSetEnds.Count == ModificationCommands.Count);

            var commandIndex = 0;
            using (var storeCommand = CreateStoreCommand(commandText, transaction.DbTransaction, typeMapper, transaction.Connection?.CommandTimeout))
            {
                if (logger.IsEnabled(LogLevel.Verbose))
                {
                    logger.LogCommand(storeCommand);
                }

                try
                {
                    using (var reader = storeCommand.ExecuteReader())
                    {
                        var actualResultSetCount = 0;
                        do
                        {
                            commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                                ? ConsumeResultSetWithPropagation(commandIndex, reader, context)
                                : ConsumeResultSetWithoutPropagation(commandIndex, reader, context);
                            actualResultSetCount++;
                        }
                        while (commandIndex < ResultSetEnds.Count
                               && reader.NextResult());

                        Debug.Assert(commandIndex == ModificationCommands.Count, "Expected " + ModificationCommands.Count + " results, got " + commandIndex);
#if DEBUG
                        var expectedResultSetCount = 1 + ResultSetEnds.Count(e => e);
                        expectedResultSetCount += ResultSetEnds[ResultSetEnds.Count - 1] ? -1 : 0;

                        Debug.Assert(actualResultSetCount == expectedResultSetCount, "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
#endif
                    }
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DbUpdateException(
                        Strings.UpdateStoreException,
                        context,
                        ex,
                        commandIndex < ModificationCommands.Count ? ModificationCommands[commandIndex].Entries : new InternalEntityEntry[0]);
                }
            }

            return commandIndex;
        }

        public override async Task<int> ExecuteAsync(
            RelationalTransaction transaction,
            IRelationalTypeMapper typeMapper,
            DbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(transaction, nameof(transaction));
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(context, nameof(context));
            Check.NotNull(logger, nameof(logger));

            var commandText = GetCommandText();

            Debug.Assert(ResultSetEnds.Count == ModificationCommands.Count);

            var commandIndex = 0;
            using (var storeCommand = CreateStoreCommand(commandText, transaction.DbTransaction, typeMapper, transaction.Connection?.CommandTimeout))
            {
                if (logger.IsEnabled(LogLevel.Verbose))
                {
                    logger.LogCommand(storeCommand);
                }

                try
                {
                    using (var reader = await storeCommand.ExecuteReaderAsync(cancellationToken).WithCurrentCulture())
                    {
                        var actualResultSetCount = 0;
                        do
                        {
                            commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                                ? await ConsumeResultSetWithPropagationAsync(commandIndex, reader, context, cancellationToken)
                                    .WithCurrentCulture()
                                : await ConsumeResultSetWithoutPropagationAsync(commandIndex, reader, context, cancellationToken)
                                    .WithCurrentCulture();
                            actualResultSetCount++;
                        }
                        while (commandIndex < ResultSetEnds.Count
                               && await reader.NextResultAsync(cancellationToken).WithCurrentCulture());

                        Debug.Assert(commandIndex == ModificationCommands.Count, "Expected " + ModificationCommands.Count + " results, got " + commandIndex);
#if DEBUG
                        var expectedResultSetCount = 1 + ResultSetEnds.Count(e => e);
                        expectedResultSetCount += ResultSetEnds[ResultSetEnds.Count - 1] ? -1 : 0;

                        Debug.Assert(actualResultSetCount == expectedResultSetCount, "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
#endif
                    }
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DbUpdateException(
                        Strings.UpdateStoreException,
                        context,
                        ex,
                        commandIndex < ModificationCommands.Count ? ModificationCommands[commandIndex].Entries : new InternalEntityEntry[0]);
                }
            }

            return commandIndex;
        }

        private int ConsumeResultSetWithPropagation(int commandIndex, DbDataReader reader, DbContext context)
        {
            var rowsAffected = 0;
            do
            {
                var tableModification = ModificationCommands[commandIndex];
                Debug.Assert(tableModification.RequiresResultPropagation);

                if (!reader.Read())
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while (++commandIndex < ResultSetEnds.Count
                           && !ResultSetEnds[commandIndex - 1])
                    {
                        expectedRowsAffected++;
                    }

                    throw new DbUpdateConcurrencyException(
                        Strings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                        context,
                        AggregateEntries(commandIndex, expectedRowsAffected));
                }

                tableModification.PropagateResults(tableModification.ValueReaderFactory.CreateValueReader(reader));
                rowsAffected++;
            }
            while (++commandIndex < ResultSetEnds.Count
                   && !ResultSetEnds[commandIndex - 1]);

            return commandIndex;
        }

        private async Task<int> ConsumeResultSetWithPropagationAsync(int commandIndex, DbDataReader reader, DbContext context, CancellationToken cancellationToken)
        {
            var rowsAffected = 0;
            do
            {
                var tableModification = ModificationCommands[commandIndex];
                Debug.Assert(tableModification.RequiresResultPropagation);

                if (!await reader.ReadAsync(cancellationToken).WithCurrentCulture())
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while (++commandIndex < ResultSetEnds.Count
                           && !ResultSetEnds[commandIndex - 1])
                    {
                        expectedRowsAffected++;
                    }

                    throw new DbUpdateConcurrencyException(
                        Strings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                        context,
                        AggregateEntries(commandIndex, expectedRowsAffected));
                }

                tableModification.PropagateResults(tableModification.ValueReaderFactory.CreateValueReader(reader));
                rowsAffected++;
            }
            while (++commandIndex < ResultSetEnds.Count
                   && !ResultSetEnds[commandIndex - 1]);

            return commandIndex;
        }

        private int ConsumeResultSetWithoutPropagation(int commandIndex, DbDataReader reader, DbContext context)
        {
            var expectedRowsAffected = 1;
            while (++commandIndex < ResultSetEnds.Count
                   && !ResultSetEnds[commandIndex - 1])
            {
                Debug.Assert(!ModificationCommands[commandIndex].RequiresResultPropagation);

                expectedRowsAffected++;
            }

            if (reader.Read())
            {
                var rowsAffected = reader.GetInt32(0);
                if (rowsAffected != expectedRowsAffected)
                {
                    throw new DbUpdateConcurrencyException(
                        Strings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                        context,
                        AggregateEntries(commandIndex, expectedRowsAffected));
                }
            }
            else
            {
                throw new DbUpdateConcurrencyException(
                    Strings.UpdateConcurrencyException(1, 0),
                    context,
                    AggregateEntries(commandIndex, expectedRowsAffected));
            }

            return commandIndex;
        }

        private async Task<int> ConsumeResultSetWithoutPropagationAsync(int commandIndex, DbDataReader reader, DbContext context, CancellationToken cancellationToken)
        {
            var expectedRowsAffected = 1;
            while (++commandIndex < ResultSetEnds.Count
                   && !ResultSetEnds[commandIndex - 1])
            {
                Debug.Assert(!ModificationCommands[commandIndex].RequiresResultPropagation);

                expectedRowsAffected++;
            }

            if (await reader.ReadAsync(cancellationToken).WithCurrentCulture())
            {
                var rowsAffected = reader.GetInt32(0);
                if (rowsAffected != expectedRowsAffected)
                {
                    throw new DbUpdateConcurrencyException(
                        Strings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                        context,
                        AggregateEntries(commandIndex, expectedRowsAffected));
                }
            }
            else
            {
                throw new DbUpdateConcurrencyException(
                    Strings.UpdateConcurrencyException(1, 0),
                    context,
                    AggregateEntries(commandIndex, expectedRowsAffected));
            }

            return commandIndex;
        }

        private IReadOnlyList<InternalEntityEntry> AggregateEntries(int endIndex, int commandCount)
        {
            var entries = new List<InternalEntityEntry>();
            for (var i = endIndex - commandCount; i < endIndex; i++)
            {
                entries.AddRange(ModificationCommands[i].Entries);
            }
            return entries;
        }

        public abstract IRelationalPropertyExtensions GetPropertyExtensions([NotNull] IProperty property);

        protected virtual void PopulateParameters(DbCommand command, ColumnModification columnModification, IRelationalTypeMapper typeMapper)
        {
            if (columnModification.ParameterName != null
                || columnModification.OriginalParameterName != null)
            {
                var property = columnModification.Property;

                var isKey = columnModification.IsKey
                            || property.IsKey()
                            || property.IsForeignKey();

                // TODO: It would be nice to just pass IProperty to the type mapper, but Migrations uses its own
                // store model for which there is no easy way to get an IProperty.
                // Issue #769
                var extensions = GetPropertyExtensions(property);
                var typeMapping = typeMapper
                    .GetTypeMapping(extensions.ColumnType, extensions.Column, property.ClrType, isKey, property.IsConcurrencyToken);

                if (columnModification.ParameterName != null)
                {
                    command.Parameters.Add(typeMapping.CreateParameter(command, columnModification, false));
                }

                if (columnModification.OriginalParameterName != null)
                {
                    command.Parameters.Add(typeMapping.CreateParameter(command, columnModification, true));
                }
            }
        }
    }
}
