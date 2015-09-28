// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();
        protected virtual StringBuilder CachedCommandText { get;[param: NotNull] set; }
        protected int LastCachedCommandIndex;


        protected ReaderModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _commandBuilderFactory = commandBuilderFactory;
            UpdateSqlGenerator = sqlGenerator;
        }

        protected virtual IUpdateSqlGenerator UpdateSqlGenerator { get; private set; }

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
            [NotNull] IRelationalConnection connection,
            [NotNull] IRelationalTypeMapper typeMapper,
            int? commandTimeout)
        {
            var commandBuilder = _commandBuilderFactory
                .Create()
                .Append(commandText);

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                PopulateParameters(commandBuilder, columnModification);
            }

            var command = commandBuilder.BuildRelationalCommand().CreateCommand(connection);

            if (commandTimeout != null)
            {
                command.CommandTimeout = (int)commandTimeout;
            }

            return command;
        }

        protected virtual void PopulateParameters(
            [NotNull] RelationalCommandBuilder commandBuilder,
            [NotNull] ColumnModification columnModification)
        {
            if (columnModification.ParameterName != null)
            {
                commandBuilder.AddParameter(
                    columnModification.ParameterName,
                    columnModification.Value,
                    columnModification.Property);
            }

            if (columnModification.OriginalParameterName != null)
            {
                commandBuilder.AddParameter(
                    columnModification.OriginalParameterName,
                    columnModification.OriginalValue,
                    columnModification.Property);
            }
        }

        public override void Execute(
            IRelationalTransaction transaction,
            IRelationalTypeMapper typeMapper,
            DbContext context,
            ILogger logger)
        {
            Check.NotNull(transaction, nameof(transaction));
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(context, nameof(context));
            Check.NotNull(logger, nameof(logger));

            var commandText = GetCommandText();

            using (var storeCommand = CreateStoreCommand(commandText, transaction.Connection, typeMapper, transaction.Connection?.CommandTimeout))
            {
                if (logger.IsEnabled(LogLevel.Verbose))
                {
                    logger.LogCommand(storeCommand);
                }

                try
                {
                    using (var reader = storeCommand.ExecuteReader())
                    {
                        Consume(reader, context);
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
            IRelationalTransaction transaction,
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

            using (var storeCommand = CreateStoreCommand(commandText, transaction.Connection, typeMapper, transaction.Connection?.CommandTimeout))
            {
                if (logger.IsEnabled(LogLevel.Verbose))
                {
                    logger.LogCommand(storeCommand);
                }

                try
                {
                    using (var reader = await storeCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        await ConsumeAsync(reader, context, cancellationToken);
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

        protected abstract void Consume([NotNull] DbDataReader reader, [NotNull] DbContext context);

        protected abstract Task ConsumeAsync(
            [NotNull] DbDataReader reader,
            [NotNull] DbContext context,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
