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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();
        protected virtual StringBuilder CachedCommandText { get; [param: NotNull] set; }
        protected int LastCachedCommandIndex;

        protected ReaderModificationCommandBatch(
            [NotNull] IUpdateSqlGenerator sqlGenerator,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
            : base(sqlGenerator)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _commandBuilderFactory = commandBuilderFactory;
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

        protected virtual IRelationalCommand CreateStoreCommand([NotNull] string commandText)
        {
            var commandBuilder = _commandBuilderFactory.Create();

            commandBuilder.Append(commandText);

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                PopulateParameters(commandBuilder.RelationalParameterList, columnModification);
            }

            return commandBuilder.BuildRelationalCommand();
        }

        protected virtual void PopulateParameters(
            [NotNull] RelationalParameterList parameterList,
            [NotNull] ColumnModification columnModification)
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

        public override void Execute([NotNull] IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var command = CreateStoreCommand(GetCommandText());

            try
            {
                using (var reader = command.ExecuteReader(connection))
                {
                    Consume(reader);
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

        public override async Task ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));

            var command = CreateStoreCommand(GetCommandText());

            try
            {
                using (var reader = await command.ExecuteReaderAsync(connection, cancellationToken))
                {
                    await ConsumeAsync(reader, cancellationToken);
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

        protected abstract void Consume([NotNull] DbDataReader reader);

        protected abstract Task ConsumeAsync(
            [NotNull] DbDataReader reader,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
