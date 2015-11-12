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

namespace Microsoft.Data.Entity.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;

        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();

        protected virtual StringBuilder CachedCommandText { get; [param: NotNull] set; }

        protected virtual int LastCachedCommandIndex { get; set; }

        protected ReaderModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));

            _commandBuilderFactory = commandBuilderFactory;

            SqlGenerationHelper = sqlGenerationHelper;
            UpdateSqlGenerator = updateSqlGenerator;

            _valueBufferFactoryFactory = valueBufferFactoryFactory;
        }

        protected virtual ISqlGenerationHelper SqlGenerationHelper { get; }

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
                    UpdateSqlGenerator.AppendInsertOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
                case EntityState.Modified:
                    UpdateSqlGenerator.AppendUpdateOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
                case EntityState.Deleted:
                    UpdateSqlGenerator.AppendDeleteOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
            }

            LastCachedCommandIndex = commandPosition;
        }

        protected virtual IRelationalCommand CreateStoreCommand()
        {
            var commandBuilder = _commandBuilderFactory
                .Create()
                .Append(GetCommandText());

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                if (columnModification.ParameterName != null)
                {
                    commandBuilder.AddParameter(
                        SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName),
                        columnModification.Value,
                        columnModification.Property);
                }

                if (columnModification.OriginalParameterName != null)
                {
                    commandBuilder.AddParameter(
                        SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName),
                        columnModification.OriginalValue,
                        columnModification.Property);
                }
            }

            return commandBuilder.Build();
        }

        public override void Execute(IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var command = CreateStoreCommand();

            try
            {
                using (var dataReader = command.ExecuteReader(connection))
                {
                    Consume(dataReader.DbDataReader);
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

        public override async Task ExecuteAsync(
            IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));

            var command = CreateStoreCommand();

            try
            {
                using (var dataReader = await command.ExecuteReaderAsync(connection, cancellationToken: cancellationToken))
                {
                    await ConsumeAsync(dataReader.DbDataReader, cancellationToken);
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
