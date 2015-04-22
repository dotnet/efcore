// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;
using RelationalStrings = Microsoft.Data.Entity.Relational.Strings;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerModificationCommandBatch : ReaderModificationCommandBatch
    {
        private const int DefaultNetworkPacketSizeBytes = 4096;
        private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;
        private const int MaxParameterCount = 2100;
        private const int MaxRowCount = 1000;
        private int _parameterCount = 1; // Implicit parameter for the command text
        private readonly int _maxBatchSize;
        private readonly List<ModificationCommand> _bulkInsertCommands = new List<ModificationCommand>();
        private int _commandsLeftToLengthCheck = 50;
        
        public SqlServerModificationCommandBatch(
            [NotNull] ISqlServerSqlGenerator sqlGenerator,
            [CanBeNull] int? maxBatchSize)
            : base(sqlGenerator)
        {
            if (maxBatchSize.HasValue
                && maxBatchSize.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize), RelationalStrings.InvalidMaxBatchSize);
            }

            _maxBatchSize = Math.Min(maxBatchSize ?? Int32.MaxValue, MaxRowCount);
        }

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (_maxBatchSize <= ModificationCommands.Count)
            {
                return false;
            }

            var additionalParameterCount = CountParameters(modificationCommand);

            if (_parameterCount + additionalParameterCount >= MaxParameterCount)
            {
                return false;
            }

            _parameterCount += additionalParameterCount;
            return true;
        }

        protected override bool IsCommandTextValid()
        {
            if (--_commandsLeftToLengthCheck < 0)
            {
                var commandTextLength = GetCommandText().Length;
                if (commandTextLength >= MaxScriptLength)
                {
                    return false;
                }

                var avarageCommandLength = commandTextLength / ModificationCommands.Count;
                var expectedAdditionalCommandCapacity = (MaxScriptLength - commandTextLength) / avarageCommandLength;
                _commandsLeftToLengthCheck = Math.Max(1, expectedAdditionalCommandCapacity / 4);
            }

            return true;
        }

        private int CountParameters(ModificationCommand modificationCommand)
        {
            var parameterCount = 0;
            foreach (var columnModification in modificationCommand.ColumnModifications)
            {
                if (columnModification.ParameterName != null)
                {
                    parameterCount++;
                }

                if (columnModification.OriginalParameterName != null)
                {
                    parameterCount++;
                }
            }

            return parameterCount;
        }

        protected override void ResetCommandText()
        {
            base.ResetCommandText();
            _bulkInsertCommands.Clear();
        }

        protected override string GetCommandText()
        {
            return base.GetCommandText() + GetBulkInsertCommandText(ModificationCommands.Count);
        }

        private string GetBulkInsertCommandText(int lastIndex)
        {
            if (_bulkInsertCommands.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var grouping = ((ISqlServerSqlGenerator)SqlGenerator).AppendBulkInsertOperation(stringBuilder, _bulkInsertCommands);
            for (var i = lastIndex - _bulkInsertCommands.Count; i < lastIndex; i++)
            {
                ResultSetEnds[i] = grouping == SqlServerSqlGenerator.ResultsGrouping.OneCommandPerResultSet;
            }

            ResultSetEnds[lastIndex - 1] = true;

            return stringBuilder.ToString();
        }

        protected override void UpdateCachedCommandText(int commandPosition)
        {
            var newModificationCommand = ModificationCommands[commandPosition];

            if (newModificationCommand.EntityState == EntityState.Added)
            {
                if (_bulkInsertCommands.Count > 0
                    && !CanBeInsertedInSameStatement(_bulkInsertCommands[0], newModificationCommand))
                {
                    CachedCommandText.Append(GetBulkInsertCommandText(commandPosition));
                    _bulkInsertCommands.Clear();
                }
                _bulkInsertCommands.Add(newModificationCommand);

                LastCachedCommandIndex = commandPosition;
            }
            else
            {
                CachedCommandText.Append(GetBulkInsertCommandText(commandPosition));
                _bulkInsertCommands.Clear();

                base.UpdateCachedCommandText(commandPosition);
            }
        }

        private bool CanBeInsertedInSameStatement(ModificationCommand firstCommand, ModificationCommand secondCommand)
        {
            return string.Equals(firstCommand.TableName, secondCommand.TableName, StringComparison.Ordinal)
                   && string.Equals(firstCommand.SchemaName, secondCommand.SchemaName, StringComparison.Ordinal)
                   && firstCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName).SequenceEqual(
                       secondCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName))
                   && firstCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName).SequenceEqual(
                       secondCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName));
        }

        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.SqlServer();
        }
    }
}
