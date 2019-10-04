// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerModificationCommandBatch : AffectedCountModificationCommandBatch
    {
        private const int DefaultNetworkPacketSizeBytes = 4096;
        private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;
        private const int MaxParameterCount = 2100;
        private const int MaxRowCount = 1000;
        private int _parameterCount = 1; // Implicit parameter for the command text
        private readonly int _maxBatchSize;
        private readonly List<ModificationCommand> _bulkInsertCommands = new List<ModificationCommand>();
        private int _commandsLeftToLengthCheck = 50;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] ISqlServerUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            int? maxBatchSize)
            : base(
                commandBuilderFactory,
                sqlGenerationHelper,
                updateSqlGenerator,
                valueBufferFactoryFactory)
        {
            if (maxBatchSize.HasValue
                && maxBatchSize.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize), RelationalStrings.InvalidMaxBatchSize);
            }

            _maxBatchSize = Math.Min(maxBatchSize ?? int.MaxValue, MaxRowCount);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new virtual ISqlServerUpdateSqlGenerator UpdateSqlGenerator => (ISqlServerUpdateSqlGenerator)base.UpdateSqlGenerator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (ModificationCommands.Count >= _maxBatchSize)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool IsCommandTextValid()
        {
            if (--_commandsLeftToLengthCheck < 0)
            {
                var commandTextLength = GetCommandText().Length;
                if (commandTextLength >= MaxScriptLength)
                {
                    return false;
                }

                var averageCommandLength = commandTextLength / ModificationCommands.Count;
                var expectedAdditionalCommandCapacity = (MaxScriptLength - commandTextLength) / averageCommandLength;
                _commandsLeftToLengthCheck = Math.Max(1, expectedAdditionalCommandCapacity / 4);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override int GetParameterCount()
            => _parameterCount;

        private static int CountParameters(ModificationCommand modificationCommand)
        {
            var parameterCount = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var columnIndex = 0; columnIndex < modificationCommand.ColumnModifications.Count; columnIndex++)
            {
                var columnModification = modificationCommand.ColumnModifications[columnIndex];
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ResetCommandText()
        {
            base.ResetCommandText();
            _bulkInsertCommands.Clear();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GetCommandText()
            => base.GetCommandText() + GetBulkInsertCommandText(ModificationCommands.Count);

        private string GetBulkInsertCommandText(int lastIndex)
        {
            if (_bulkInsertCommands.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var resultSetMapping = UpdateSqlGenerator.AppendBulkInsertOperation(stringBuilder, _bulkInsertCommands, lastIndex - _bulkInsertCommands.Count);
            for (var i = lastIndex - _bulkInsertCommands.Count; i < lastIndex; i++)
            {
                CommandResultSet[i] = resultSetMapping;
            }

            if (resultSetMapping != ResultSetMapping.NoResultSet)
            {
                CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        private static bool CanBeInsertedInSameStatement(ModificationCommand firstCommand, ModificationCommand secondCommand)
            => string.Equals(firstCommand.TableName, secondCommand.TableName, StringComparison.Ordinal)
               && string.Equals(firstCommand.Schema, secondCommand.Schema, StringComparison.Ordinal)
               && firstCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName).SequenceEqual(
                   secondCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName))
               && firstCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName).SequenceEqual(
                   secondCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName));
    }
}
