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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();

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

        protected virtual StringBuilder CachedCommandText { get; [param: NotNull] set; }
        protected virtual int LastCachedCommandIndex { get; set; }
        protected virtual ISqlGenerationHelper SqlGenerationHelper { get; }
        protected virtual IUpdateSqlGenerator UpdateSqlGenerator { get; }
        public override IReadOnlyList<ModificationCommand> ModificationCommands => _modificationCommands;
        protected virtual IList<ResultSetMapping> CommandResultSet { get; } = new List<ResultSetMapping>();

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

        protected virtual int GetParameterCount()
            => ModificationCommands.Sum(c => c.ColumnModifications.Count);

        protected virtual RawSqlCommand CreateStoreCommand()
        {
            var commandBuilder = _commandBuilderFactory
                .Create()
                .Append(GetCommandText());

            var parameterValues = new Dictionary<string, object>(GetParameterCount());

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                if (columnModification.UseCurrentValueParameter)
                {
                    commandBuilder.AddParameter(
                        columnModification.ParameterName,
                        SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName),
                        columnModification.Property);

                    parameterValues.Add(
                        columnModification.ParameterName,
                        columnModification.Value);
                }

                if (columnModification.UseOriginalValueParameter)
                {
                    commandBuilder.AddParameter(
                        columnModification.OriginalParameterName,
                        SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName),
                        columnModification.Property);

                    parameterValues.Add(
                        columnModification.OriginalParameterName,
                        columnModification.OriginalValue);
                }
            }

            return new RawSqlCommand(
                commandBuilder.Build(),
                parameterValues);
        }

        public override void Execute(IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var storeCommand = CreateStoreCommand();

            try
            {
                using (var dataReader = storeCommand.RelationalCommand.ExecuteReader(
                    connection,
                    parameterValues: storeCommand.ParameterValues))
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

            var storeCommand = CreateStoreCommand();

            try
            {
                using (var dataReader = await storeCommand.RelationalCommand.ExecuteReaderAsync(
                    connection,
                    parameterValues: storeCommand.ParameterValues,
                    cancellationToken: cancellationToken))
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
