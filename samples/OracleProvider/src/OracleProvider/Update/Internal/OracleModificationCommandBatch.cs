// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Oracle.Update.Internal
{
    public class OracleModificationCommandBatch : ReaderModificationCommandBatch
    {
        private const int MaxParameterCount = 1000;
        private const int MaxRowCount = 200;
        private int _countParameter = 1;
        private int _cursorPosition = 1;
        private readonly int _maxBatchSize;
        private readonly List<ModificationCommand> _batchInsertCommands;
        private readonly Dictionary<string, string> _variablesInsert;
        private readonly StringBuilder _variablesCommand;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        public OracleModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            int? maxBatchSize)
            : base(commandBuilderFactory, sqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
        {
            _commandBuilderFactory = commandBuilderFactory;
            _batchInsertCommands = new List<ModificationCommand>();
            _variablesInsert = new Dictionary<string, string>();
            _variablesCommand = new StringBuilder();
            _maxBatchSize = Math.Min(maxBatchSize ?? int.MaxValue, MaxRowCount);
        }

        protected new virtual IOracleUpdateSqlGenerator UpdateSqlGenerator
            => (IOracleUpdateSqlGenerator)base.UpdateSqlGenerator;

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (ModificationCommands.Count >= _maxBatchSize)
            {
                return false;
            }

            var additionalParameterCount = CountParameters(modificationCommand);

            if (_countParameter + additionalParameterCount >= MaxParameterCount)
            {
                return false;
            }

            _countParameter += additionalParameterCount;
            return true;
        }

        protected override void ResetCommandText()
        {
            base.ResetCommandText();
            _batchInsertCommands.Clear();
            _cursorPosition = 1;
        }

        protected override RawSqlCommand CreateStoreCommand()
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

            for (var i = 1; i < _cursorPosition; i++)
            {
                var nameParameter = $"cur{i}";

                commandBuilder.AddRawParameter(
                    nameParameter,
                    new OracleParameter(
                        nameParameter,
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output));
            }

            return new RawSqlCommand(
                commandBuilder.Build(),
                parameterValues);
        }

        protected override string GetCommandText()
        {
            var bulkOperation = new StringBuilder();
            _variablesInsert.Clear();
            _variablesCommand.Clear();

            bulkOperation.AppendLine("BEGIN");
            bulkOperation.AppendLine(base.GetCommandText());
            bulkOperation.Append(GetBatchInsertCommandText(ModificationCommands.Count));
            if (_cursorPosition > 1)
            {
                var declare = new StringBuilder();
                declare
                    .AppendLine("DECLARE")
                    .AppendJoin(
                        _variablesInsert.Select(v => v.Value),
                        (sb, cm) => sb.Append(cm), Environment.NewLine)
                    .Append(_variablesCommand)
                    .AppendLine("v_RowCount INTEGER;");
                bulkOperation.Insert(0, declare);
            }
            bulkOperation.AppendLine("END;");

            return bulkOperation.ToString();
        }

        private string GetBatchInsertCommandText(int lastIndex)
        {
            if (_batchInsertCommands.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var resultSetMapping = UpdateSqlGenerator
                .AppendBatchInsertOperation(stringBuilder, _variablesInsert, _batchInsertCommands, lastIndex - _batchInsertCommands.Count, ref _cursorPosition);

            for (var i = lastIndex - _batchInsertCommands.Count; i < lastIndex; i++)
            {
                CommandResultSet[i] = resultSetMapping;
            }

            if (resultSetMapping != ResultSetMapping.NoResultSet)
            {
                CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;
            }

            return stringBuilder.ToString();
        }

        private string GetBatchUpdateCommandText(int lastIndex)
        {
            var stringBuilder = new StringBuilder();
            var resultSetMapping = UpdateSqlGenerator
                .AppendBatchUpdateOperation(stringBuilder, _variablesCommand, ModificationCommands, lastIndex, ref _cursorPosition);

            CommandResultSet[lastIndex] = resultSetMapping;
            return stringBuilder.ToString();
        }

        private string GetBatchDeleteCommandText(int lastIndex)
        {
            var stringBuilder = new StringBuilder();
            var resultSetMapping = UpdateSqlGenerator
                .AppendBatchDeleteOperation(stringBuilder, _variablesCommand, ModificationCommands, lastIndex, ref _cursorPosition);

            CommandResultSet[lastIndex] = resultSetMapping;

            return stringBuilder.ToString();
        }

        protected override void UpdateCachedCommandText(int commandPosition)
        {
            var stringBuilder = new StringBuilder();
            var newModificationCommand = ModificationCommands[commandPosition];
            if (newModificationCommand.EntityState == EntityState.Added)
            {
                if (_batchInsertCommands.Count > 0
                    && !CanBeInserted(_batchInsertCommands[0], newModificationCommand))
                {
                    CachedCommandText.Append(GetBatchInsertCommandText(commandPosition));
                    _batchInsertCommands.Clear();
                }
                _batchInsertCommands.Add(newModificationCommand);
                LastCachedCommandIndex = commandPosition;
            }
            else if (newModificationCommand.EntityState == EntityState.Deleted)
            {
                CachedCommandText.Append(GetBatchDeleteCommandText(commandPosition));
                LastCachedCommandIndex = commandPosition;
            }
            else
            {
                CachedCommandText.Append(GetBatchUpdateCommandText(commandPosition));
                LastCachedCommandIndex = commandPosition;
            }
        }

        protected override bool IsCommandTextValid() => true;

        protected override int GetParameterCount() => _countParameter;

        private static int CountParameters(ModificationCommand modificationCommand)
        {
            var parameterCount = 0;
            foreach (var columnModification in modificationCommand.ColumnModifications)
            {
                if (columnModification.UseCurrentValueParameter)
                {
                    parameterCount++;
                }

                if (columnModification.UseOriginalValueParameter)
                {
                    parameterCount++;
                }
            }
            return parameterCount;
        }

        private static bool CanBeInserted(ModificationCommand first, ModificationCommand second)
            => string.Equals(first.TableName, second.TableName, StringComparison.Ordinal)
               && string.Equals(first.Schema, second.Schema, StringComparison.Ordinal)
               && first.ColumnModifications.Where(o => o.IsWrite)
                   .Select(o => o.ColumnName)
                   .SequenceEqual(
                       second.ColumnModifications.Where(o => o.IsWrite)
                           .Select(o => o.ColumnName))
               && first.ColumnModifications.Where(o => o.IsRead)
                   .Select(o => o.ColumnName)
                   .SequenceEqual(
                       second.ColumnModifications.Where(o => o.IsRead)
                           .Select(o => o.ColumnName));

        protected override void Consume(RelationalDataReader relationalReader)
        {
            var commandPosition = 0;
            int rowsAffected;

            try
            {
                do
                {
                    while (commandPosition < CommandResultSet.Count
                           && CommandResultSet[commandPosition] == ResultSetMapping.NoResultSet)
                    {
                        commandPosition++;
                    }

                    if (commandPosition < CommandResultSet.Count)
                    {
                        if (ModificationCommands[commandPosition].RequiresResultPropagation)
                        {
                            rowsAffected = 0;
                            do
                            {
                                var tableModification = ModificationCommands[commandPosition];
                                if (!relationalReader.Read())
                                {
                                    throw new DbUpdateConcurrencyException(
                                        RelationalStrings.UpdateConcurrencyException(
                                            ModificationCommands.Count(m => m.RequiresResultPropagation), rowsAffected),
                                        ModificationCommands[commandPosition].Entries);
                                }

                                var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);
                                tableModification.PropagateResults(valueBufferFactory.Create(relationalReader.DbDataReader));
                                rowsAffected++;
                            }
                            while (++commandPosition < CommandResultSet.Count
                                   && CommandResultSet[commandPosition - 1] == ResultSetMapping.NotLastInResultSet);
                        }
                        else
                        {
                            var expectedRowsAffected = 1;
                            while (++commandPosition < CommandResultSet.Count
                                   && CommandResultSet[commandPosition - 1] == ResultSetMapping.NotLastInResultSet)
                            {
                                expectedRowsAffected++;
                            }

                            if (relationalReader.Read())
                            {
                                rowsAffected = relationalReader.DbDataReader.GetInt32(0);
                                if (rowsAffected != expectedRowsAffected)
                                {
                                    throw new DbUpdateConcurrencyException(
                                        RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                                        ModificationCommands[commandPosition - 1].Entries);
                                }
                            }
                            else
                            {
                                throw new DbUpdateConcurrencyException(
                                    RelationalStrings.UpdateConcurrencyException(1, 0),
                                    ModificationCommands[commandPosition - 1].Entries);
                            }
                        }
                    }
                }
                while (commandPosition < CommandResultSet.Count
                       && relationalReader.DbDataReader.NextResult());
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex, ModificationCommands[commandPosition - 1].Entries);
            }
        }

        protected override async Task ConsumeAsync(
            RelationalDataReader relationalReader, CancellationToken cancellationToken = default)
        {
            var commandPosition = 0;
            int rowsAffected;

            try
            {
                do
                {
                    while (commandPosition < CommandResultSet.Count
                           && CommandResultSet[commandPosition] == ResultSetMapping.NoResultSet)
                    {
                        commandPosition++;
                    }

                    if (commandPosition < CommandResultSet.Count)
                    {
                        if (ModificationCommands[commandPosition].RequiresResultPropagation)
                        {
                            rowsAffected = 0;
                            do
                            {
                                var tableModification = ModificationCommands[commandPosition];
                                if (!await relationalReader.ReadAsync(cancellationToken))
                                {
                                    throw new DbUpdateConcurrencyException(
                                        RelationalStrings.UpdateConcurrencyException(
                                            ModificationCommands.Count(m => m.RequiresResultPropagation), rowsAffected),
                                        ModificationCommands[commandPosition].Entries);
                                }

                                var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);
                                tableModification.PropagateResults(valueBufferFactory.Create(relationalReader.DbDataReader));
                                rowsAffected++;
                            }
                            while (++commandPosition < CommandResultSet.Count
                                   && CommandResultSet[commandPosition - 1] == ResultSetMapping.NotLastInResultSet);
                        }
                        else
                        {
                            var expectedRowsAffected = 1;
                            while (++commandPosition < CommandResultSet.Count
                                   && CommandResultSet[commandPosition - 1] == ResultSetMapping.NotLastInResultSet)
                            {
                                expectedRowsAffected++;
                            }

                            if (relationalReader.Read())
                            {
                                rowsAffected = relationalReader.DbDataReader.GetInt32(0);
                                if (rowsAffected != expectedRowsAffected)
                                {
                                    throw new DbUpdateConcurrencyException(
                                        RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                                        ModificationCommands[commandPosition - 1].Entries);
                                }
                            }
                            else
                            {
                                throw new DbUpdateConcurrencyException(
                                    RelationalStrings.UpdateConcurrencyException(1, 0),
                                    ModificationCommands[commandPosition - 1].Entries);
                            }
                        }
                    }
                }
                while (commandPosition < CommandResultSet.Count
                       && await relationalReader.DbDataReader.NextResultAsync(cancellationToken));
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex, ModificationCommands[commandPosition].Entries);
            }
        }
    }
}
