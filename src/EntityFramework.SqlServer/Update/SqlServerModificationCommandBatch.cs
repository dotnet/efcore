// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerModificationCommandBatch : ReaderModificationCommandBatch
    {
        private static readonly int _defaultNetworkPacketSizeBytes = 4096;
        private static readonly int _maxScriptLength = 65536 * _defaultNetworkPacketSizeBytes / 2;
        private static readonly int _maxParameterCount = 2100;
        private int _parameterCount;
        private readonly int? _maxBatchSize;

        public SqlServerModificationCommandBatch([CanBeNull] int? maxBatchSize)
        {
            if (maxBatchSize.HasValue
                && maxBatchSize.Value <= 0)
            {
                throw new ArgumentOutOfRangeException("maxBatchSize", Strings.FormatMaxBatchSizeMustBePositive());
            }

            _maxBatchSize = maxBatchSize;
        }

        protected override bool CanAddCommand(ModificationCommand modificationCommand, StringBuilder newSql)
        {
            if (_maxBatchSize.HasValue && _maxBatchSize.Value <= ModificationCommands.Count)
            {
                return false;
            }

            var additionalParameterCount = CountParameters(modificationCommand);

            if (ModificationCommands.Count == 0)
            {
                _parameterCount = additionalParameterCount;
                return true;
            }

            if (_parameterCount + additionalParameterCount >= _maxParameterCount)
            {
                return false;
            }

            if (newSql.Length >= _maxScriptLength)
            {
                return false;
            }

            _parameterCount += additionalParameterCount;
            return true;
        }

        // TODO: Merge insert statements

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
    }
}
