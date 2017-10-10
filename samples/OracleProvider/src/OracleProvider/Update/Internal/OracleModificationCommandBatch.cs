// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public class OracleModificationCommandBatch : ReaderModificationCommandBatch
    {
        private const int MaxParameterCount = 1000;
        private const int MaxRowCount = 200;
        private int _countParameter = 1;
        private int _cursorPosition = 1;
        private readonly List<ModificationCommand> _batchInsertCommands;
        private readonly StringBuilder _variablesCommand;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory = null;

        public OracleModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            : base(commandBuilderFactory, sqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
        {
            _commandBuilderFactory = commandBuilderFactory;
            _batchInsertCommands = new List<ModificationCommand>();
            _variablesCommand = new StringBuilder();
        }

        protected new virtual IOracleUpdateSqlGenerator UpdateSqlGenerator 
            => (IOracleUpdateSqlGenerator)base.UpdateSqlGenerator;

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (ModificationCommands.Count >= MaxRowCount)
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

            for (int i = 1; i < _cursorPosition; i++)
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
            _variablesCommand.Clear();

            bulkOperation.AppendLine("BEGIN");
            bulkOperation.AppendLine(base.GetCommandText());
            bulkOperation.Append(GetBatchInsertCommandText(ModificationCommands.Count));
            if (_cursorPosition > 1)
            {
                var declare = new StringBuilder();
                declare.AppendLine("DECLARE")
                       .Append(_variablesCommand);
                bulkOperation.Insert(0, declare);
            }
            bulkOperation.AppendLine("END;");

            return bulkOperation.ToString();
        }

        private string GetBatchInsertCommandText(int lastIndex)
        {
            if (_batchInsertCommands.Count == 0)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            var resultSetMapping = UpdateSqlGenerator
                .AppendBulkInsertOperation(stringBuilder, _variablesCommand, _batchInsertCommands, lastIndex - _batchInsertCommands.Count, ref _cursorPosition);

            for (var i = lastIndex - _batchInsertCommands.Count; i < lastIndex; i++)
                CommandResultSet[i] = resultSetMapping;

            if (resultSetMapping != ResultSetMapping.NoResultSet)
                CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;

            return stringBuilder.ToString();
        }

        protected override void UpdateCachedCommandText(int commandPosition)
        {
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
            else
            {
                CachedCommandText.Append(GetBatchInsertCommandText(commandPosition));
                _batchInsertCommands.Clear();
                base.UpdateCachedCommandText(commandPosition);
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
                .SequenceEqual(second.ColumnModifications.Where(o => o.IsWrite)
                .Select(o => o.ColumnName))
               && first.ColumnModifications.Where(o => o.IsRead)
                .Select(o => o.ColumnName)
                .SequenceEqual(second.ColumnModifications.Where(o => o.IsRead)
                .Select(o => o.ColumnName));

        protected override void Consume(RelationalDataReader relationalReader)
        {
            if (relationalReader == null)
            {
                throw new ArgumentNullException(nameof(relationalReader));
            }
            var dataReader = relationalReader.DbDataReader;
            var commandIndex = 0;
            try
            {
                while (true)
                {
                    while (commandIndex < CommandResultSet.Count && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                    {
                        commandIndex++;
                    }

                    var propragation = commandIndex;
                    while (propragation < ModificationCommands.Count && !ModificationCommands[propragation].RequiresResultPropagation)
                    {
                        propragation++;
                    }

                    while (commandIndex < propragation)
                    {
                        commandIndex++;
                        if (!dataReader.Read())
                        {

                        }
                    }

                    //check if you've gone through all notifications
                    if (propragation == ModificationCommands.Count)
                    {
                        break;
                    }

                    var modifications = ModificationCommands[commandIndex];
                    if (!relationalReader.Read())
                    {
                        throw new DbUpdateConcurrencyException(
                                RelationalStrings.UpdateConcurrencyException(
                                ModificationCommands.Count(m => m.RequiresResultPropagation), 0),
                                ModificationCommands[commandIndex].Entries);
                    }

                    var bufferFactory = CreateValueBufferFactory(modifications.ColumnModifications);
                    modifications.PropagateResults(bufferFactory.Create(dataReader));
                    dataReader.NextResult();
                    commandIndex++;
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex, ModificationCommands[commandIndex].Entries);
            }
        }

        protected override async Task ConsumeAsync(
            RelationalDataReader relationalReader,
            CancellationToken cancellationToken = default)
        {
            if (relationalReader == null)
            {
                throw new ArgumentNullException(nameof(relationalReader));
            }
            var dataReader = relationalReader.DbDataReader;
            var commandIndex = 0;
            try
            {
                while (true)
                {
                    while (commandIndex < CommandResultSet.Count && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                    {
                        commandIndex++;
                    }

                    var propragation = commandIndex;
                    while (propragation < ModificationCommands.Count && !ModificationCommands[propragation].RequiresResultPropagation)
                    {
                        propragation++;
                    }

                    while (commandIndex < propragation)
                    {
                        commandIndex++;
                        if (!(await relationalReader.ReadAsync()))
                        {
                            throw new DbUpdateConcurrencyException(
                                    RelationalStrings.UpdateConcurrencyException(
                                    ModificationCommands.Count(m => m.RequiresResultPropagation), 0),
                                    ModificationCommands[commandIndex].Entries);
                        }
                    }

                    //check if you've gone through all notifications
                    if (propragation == ModificationCommands.Count)
                    {
                        break;
                    }

                    var modifications = ModificationCommands[commandIndex];
                    if (!(await relationalReader.ReadAsync()))
                    {
                        throw new DbUpdateConcurrencyException(
                                RelationalStrings.UpdateConcurrencyException(
                                ModificationCommands.Count(m => m.RequiresResultPropagation), 0),
                                ModificationCommands[commandIndex].Entries);
                    }

                    var bufferFactory = CreateValueBufferFactory(modifications.ColumnModifications);
                    modifications.PropagateResults(bufferFactory.Create(dataReader));
                    await dataReader.NextResultAsync();
                    commandIndex++;
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex, ModificationCommands[commandIndex].Entries);
            }
        }
    }
}
