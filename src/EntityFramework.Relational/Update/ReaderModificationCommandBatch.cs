// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();
        private readonly List<bool> _resultSetEnd = new List<bool>();
        protected StringBuilder CachedCommandText { get; set; }
        protected int LastCachedCommandIndex = 0;
        
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ReaderModificationCommandBatch()
        {
        }

        protected ReaderModificationCommandBatch([NotNull] SqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override IReadOnlyList<ModificationCommand> ModificationCommands
        {
            get { return _modificationCommands; }
        }

        // contains true if the command at the corresponding index is the last command in its result set
        // the last value will not be read
        protected IList<bool> ResultSetEnds
        {
            get { return _resultSetEnd; }
        }

        public override bool AddCommand(ModificationCommand modificationCommand)
        {
            Check.NotNull(modificationCommand, "modificationCommand");

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
            [NotNull] RelationalTypeMapper typeMapper)
        {
            var command = transaction.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            command.Transaction = transaction;

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                PopulateParameters(command, columnModification, typeMapper);
            }

            return command;
        }

        public override async Task<int> ExecuteAsync(
            RelationalTransaction transaction,
            RelationalTypeMapper typeMapper,
            DbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(typeMapper, "typeMapper");
            Check.NotNull(context, "context");
            Check.NotNull(logger, "logger");

            var commandText = GetCommandText();
            if (logger.IsEnabled(TraceType.Verbose))
            {
                // TODO: Write parameter values
                logger.WriteSql(commandText);
            }

            Contract.Assert(ResultSetEnds.Count == ModificationCommands.Count);

            var commandIndex = 0;
            using (var storeCommand = CreateStoreCommand(commandText, transaction.DbTransaction, typeMapper))
            {
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

                        Contract.Assert(commandIndex == ModificationCommands.Count, "Expected " + ModificationCommands.Count + " results, got " + commandIndex);
#if DEBUG
                        var expectedResultSetCount = 1 + ResultSetEnds.Count(e => e);
                        expectedResultSetCount += ResultSetEnds[ResultSetEnds.Count - 1] ? -1 : 0;

                        Contract.Assert(actualResultSetCount == expectedResultSetCount, "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
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
                        Strings.FormatUpdateStoreException(),
                        context,
                        ex,
                        commandIndex < ModificationCommands.Count ? ModificationCommands[commandIndex].StateEntries : new StateEntry[0]);
                }
            }

            return commandIndex;
        }

        private async Task<int> ConsumeResultSetWithPropagationAsync(int commandIndex, DbDataReader reader, DbContext context, CancellationToken cancellationToken)
        {
            var rowsAffected = 0;
            var valueReader = new RelationalTypedValueReader(reader);
            do
            {
                var tableModification = ModificationCommands[commandIndex];
                Contract.Assert(tableModification.RequiresResultPropagation);

                if (!await reader.ReadAsync(cancellationToken).WithCurrentCulture())
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while (++commandIndex < ResultSetEnds.Count
                           && !ResultSetEnds[commandIndex - 1])
                    {
                        expectedRowsAffected++;
                    }

                    throw new DbUpdateConcurrencyException(
                        Strings.FormatUpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                        context,
                        AggregateStateEntries(commandIndex, expectedRowsAffected));
                }

                tableModification.PropagateResults(valueReader);
                rowsAffected++;
            }
            while (++commandIndex < ResultSetEnds.Count
                   && !ResultSetEnds[commandIndex - 1]);

            return commandIndex;
        }

        private async Task<int> ConsumeResultSetWithoutPropagationAsync(int commandIndex, DbDataReader reader, DbContext context, CancellationToken cancellationToken)
        {
            var expectedRowsAffected = 1;
            while (++commandIndex < ResultSetEnds.Count
                   && !ResultSetEnds[commandIndex - 1])
            {
                Contract.Assert(!ModificationCommands[commandIndex].RequiresResultPropagation);

                expectedRowsAffected++;
            }

            if (await reader.ReadAsync(cancellationToken).WithCurrentCulture())
            {
                var rowsAffected = reader.GetFieldValue<int>(0);
                if (rowsAffected != expectedRowsAffected)
                {
                    throw new DbUpdateConcurrencyException(
                        Strings.FormatUpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                        context,
                        AggregateStateEntries(commandIndex, expectedRowsAffected));
                }
            }
            else
            {
                throw new DbUpdateConcurrencyException(
                    Strings.FormatUpdateConcurrencyException(1, 0),
                    context,
                    AggregateStateEntries(commandIndex, expectedRowsAffected));
            }

            return commandIndex;
        }

        private IReadOnlyList<StateEntry> AggregateStateEntries(int endIndex, int commandCount)
        {
            var stateEntries = new List<StateEntry>();
            for (var i = endIndex - commandCount; i < endIndex; i++)
            {
                stateEntries.AddRange(ModificationCommands[i].StateEntries);
            }
            return stateEntries;
        }

        protected virtual void PopulateParameters(DbCommand command, ColumnModification columnModification, RelationalTypeMapper typeMapper)
        {
            if (columnModification.ParameterName != null
                || columnModification.OriginalParameterName != null)
            {
                var property = columnModification.Property;

                var isKey = columnModification.IsKey
                            || property.IsForeignKey();

                // TODO: It would be nice to just pass IProperty to the type mapper, but Migrations uses its own
                // store model for which there is no easy way to get an IProperty.
                var typeMapping = typeMapper
                    .GetTypeMapping(
                        property.ColumnType(), property.ColumnName(), property.PropertyType, isKey, property.IsConcurrencyToken);

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
