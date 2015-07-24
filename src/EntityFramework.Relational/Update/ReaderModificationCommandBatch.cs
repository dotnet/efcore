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
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Commands;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();
        protected StringBuilder CachedCommandText { get; set; }
        protected int LastCachedCommandIndex;

        protected ReaderModificationCommandBatch(
            [NotNull] IUpdateSqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

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
            var commandBuilder = new RelationalCommandBuilder();

            commandBuilder.Append(commandText);

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                PopulateParameters(commandBuilder.RelationalParameterList, columnModification);
            }

            var command = commandBuilder.RelationalCommand.CreateDbCommand(connection, typeMapper);

            if (commandTimeout != null)
            {
                command.CommandTimeout = (int)commandTimeout;
            }

            return command;
        }

        protected virtual void PopulateParameters(RelationalParameterList parameterList, ColumnModification columnModification)
        {
            if (columnModification.ParameterName != null)
            {
                parameterList.GetOrAdd(
                    columnModification.ParameterName,
                    columnModification.Value,
                    columnModification.Property);
            }

            if (columnModification.OriginalParameterName != null)
            {
                parameterList.GetOrAdd(
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
                    throw new DbUpdateException(Strings.UpdateStoreException, ex);
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
                    throw new DbUpdateException(Strings.UpdateStoreException, ex);
                }
            }
        }

        protected abstract void Consume(DbDataReader reader, DbContext context);

        protected abstract Task ConsumeAsync(
            DbDataReader reader,
            DbContext context,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
