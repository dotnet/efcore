// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerModificationCommandBatch : ReaderModificationCommandBatch
    {
        private const int DefaultNetworkPacketSizeBytes = 4096;
        private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;
        private const int MaxParameterCount = 2100;
        private int _parameterCount;

        protected override bool CanAddCommand(ModificationCommand modificationCommand, StringBuilder newSql)
        {
            var additionalParameterCount = CountParameters(modificationCommand);

            if (ModificationCommands.Count == 0)
            {
                _parameterCount = additionalParameterCount;
                return true;
            }

            if (_parameterCount + additionalParameterCount >= MaxParameterCount)
            {
                return false;
            }

            if (newSql.Length >= MaxScriptLength)
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
