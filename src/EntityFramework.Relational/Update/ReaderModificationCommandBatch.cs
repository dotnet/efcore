// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly ISensitiveDataLogger _logger;
#pragma warning disable 0618
        private readonly TelemetrySource _telemetrySource;

        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();

        protected virtual StringBuilder CachedCommandText { get; [param: NotNull] set; }

        protected virtual int LastCachedCommandIndex { get; set; }

        protected ReaderModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] TelemetrySource telemetrySource)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(telemetrySource, nameof(telemetrySource));

            _commandBuilderFactory = commandBuilderFactory;

            SqlGenerator = sqlGenerator;
            UpdateSqlGenerator = updateSqlGenerator;

            _valueBufferFactoryFactory = valueBufferFactoryFactory;
            _logger = logger;
            _telemetrySource = telemetrySource;
        }
#pragma warning restore 0618

        protected virtual ISqlGenerator SqlGenerator { get; }

        protected virtual IUpdateSqlGenerator UpdateSqlGenerator { get; }

        public override IReadOnlyList<ModificationCommand> ModificationCommands => _modificationCommands;

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

            if (!IsCommandTextValid())
            {
                ResetCommandText();
                _modificationCommands.RemoveAt(_modificationCommands.Count - 1);
                return false;
            }

            return true;
        }

        protected virtual void ResetCommandText()
        {
            CachedCommandText = new StringBuilder();
            UpdateSqlGenerator.AppendBatchHeader(CachedCommandText);
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
                    UpdateSqlGenerator.AppendInsertOperation(CachedCommandText, newModificationCommand);
                    break;
                case EntityState.Modified:
                    UpdateSqlGenerator.AppendUpdateOperation(CachedCommandText, newModificationCommand);
                    break;
                case EntityState.Deleted:
                    UpdateSqlGenerator.AppendDeleteOperation(CachedCommandText, newModificationCommand);
                    break;
            }

            LastCachedCommandIndex = commandPosition;
        }

        protected virtual DbCommand CreateStoreCommand(
            [NotNull] string commandText,
            [NotNull] IRelationalConnection connection)
        {
            var commandBuilder = _commandBuilderFactory
                .Create()
                .Append(commandText);

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                PopulateParameters(commandBuilder, columnModification);
            }

            return commandBuilder.BuildRelationalCommand().CreateCommand(connection);
        }

        protected virtual void PopulateParameters(
            [NotNull] IRelationalCommandBuilder commandBuilder,
            [NotNull] ColumnModification columnModification)
        {
            if (columnModification.ParameterName != null)
            {
                commandBuilder.AddParameter(
                    SqlGenerator.GenerateParameterName(columnModification.ParameterName),
                    columnModification.Value,
                    columnModification.Property);
            }

            if (columnModification.OriginalParameterName != null)
            {
                commandBuilder.AddParameter(
                    SqlGenerator.GenerateParameterName(columnModification.OriginalParameterName),
                    columnModification.OriginalValue,
                    columnModification.Property);
            }
        }

        public override void Execute(IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var commandText = GetCommandText();

            using (var command = CreateStoreCommand(commandText, connection))
            {
                _logger.LogCommand(command);

                WriteTelemetry(RelationalTelemetry.BeforeExecuteCommand, command);

                try
                {
                    DbDataReader dataReader;

                    try
                    {
                        dataReader = command.ExecuteReader();
                    }
                    catch (Exception exception)
                    {
                        _telemetrySource
                            .WriteCommandError(
                                command,
                                RelationalTelemetry.ExecuteMethod.ExecuteReader,
                                async: false,
                                exception: exception);

                        throw;
                    }

                    WriteTelemetry(RelationalTelemetry.AfterExecuteCommand, command);

                    using (dataReader)
                    {
                        Consume(dataReader);
                    }
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DbUpdateException(RelationalStrings.UpdateStoreException, ex);
                }
            }
        }

        public override async Task ExecuteAsync(
            IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));

            var commandText = GetCommandText();

            using (var command = CreateStoreCommand(commandText, connection))
            {
                _logger.LogCommand(command);

                WriteTelemetry(RelationalTelemetry.BeforeExecuteCommand, command, async: true);

                try
                {
                    DbDataReader dataReader;

                    try
                    {
                        dataReader = await command.ExecuteReaderAsync(cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        _telemetrySource
                            .WriteCommandError(
                                command,
                                RelationalTelemetry.ExecuteMethod.ExecuteReader,
                                async: true,
                                exception: exception);

                        throw;
                    }

                    WriteTelemetry(RelationalTelemetry.AfterExecuteCommand, command, async: true);

                    using (dataReader)
                    {
                        await ConsumeAsync(dataReader, cancellationToken);
                    }
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DbUpdateException(RelationalStrings.UpdateStoreException, ex);
                }
            }
        }

        private void WriteTelemetry(string name, DbCommand command, bool async = false)
            => _telemetrySource
                .WriteCommand(
                    name,
                    command,
                    RelationalTelemetry.ExecuteMethod.ExecuteReader,
                    async: async);

        protected abstract void Consume([NotNull] DbDataReader reader);

        protected abstract Task ConsumeAsync(
            [NotNull] DbDataReader reader,
            CancellationToken cancellationToken = default(CancellationToken));

        protected virtual IRelationalValueBufferFactory CreateValueBufferFactory([NotNull] IReadOnlyList<ColumnModification> columnModifications)
            => _valueBufferFactoryFactory
                .Create(
                    Check.NotNull(columnModifications, nameof(columnModifications))
                        .Where(c => c.IsRead)
                        .Select(c => c.Property.ClrType)
                        .ToArray(),
                    indexMap: null);
    }
}
