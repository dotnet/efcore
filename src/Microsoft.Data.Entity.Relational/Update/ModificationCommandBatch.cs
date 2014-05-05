// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommandBatch
    {
        private readonly ModificationCommand[] _batchCommands;

        public ModificationCommandBatch([NotNull] ModificationCommand[] batchCommands)
        {
            Check.NotNull(batchCommands, "batchCommands");

            Contract.Assert(batchCommands.Any(), "batchCommands array is empty");

            _batchCommands = batchCommands;
        }

        public virtual IEnumerable<ModificationCommand> BatchCommands
        {
            get { return _batchCommands; }
        }

        public virtual string CompileBatch([NotNull] SqlGenerator sqlGenerator, [NotNull] out List<KeyValuePair<string, object>> parameters)
        {
            Check.NotNull(sqlGenerator, "sqlGenerator");

            var stringBuilder = new StringBuilder();
            parameters = new List<KeyValuePair<string, object>>();

            sqlGenerator.AppendBatchHeader(stringBuilder);
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append(sqlGenerator.BatchCommandSeparator).AppendLine();
            }

            foreach (var command in _batchCommands)
            {
                if (command.Operation == ModificationOperation.Insert)
                {
                    AppendInsertCommand(command, sqlGenerator, stringBuilder, parameters);
                }

                if (command.Operation == ModificationOperation.Update)
                {
                    AppendUpdateCommand(command, sqlGenerator, stringBuilder, parameters);
                }

                if (command.Operation == ModificationOperation.Delete)
                {
                    AppendDeleteCommand(command, sqlGenerator, stringBuilder, parameters);
                }

                stringBuilder.Append(sqlGenerator.BatchCommandSeparator).AppendLine();
            }

            return stringBuilder.ToString();
        }

        private void AppendInsertCommand(ModificationCommand modificationCommand, SqlGenerator sqlGenerator,
            StringBuilder stringBuilder, List<KeyValuePair<string, object>> parameters)
        {
            var commandParameters = CreateParameters(modificationCommand.ColumnValues, parameters);

            sqlGenerator.AppendInsertOperation(
                stringBuilder,
                modificationCommand.Table,
                modificationCommand.ColumnValues.Zip(
                    commandParameters,
                    (c, p) => new KeyValuePair<Column, string>(c.Key, p.Key)).ToArray());
        }

        private void AppendUpdateCommand(ModificationCommand modificationCommand, SqlGenerator sqlGenerator,
            StringBuilder stringBuilder, List<KeyValuePair<string, object>> parameters)
        {
            var updateParameters = CreateParameters(modificationCommand.ColumnValues, parameters);
            var whereClauseParameters = CreateParameters(modificationCommand.WhereClauses, parameters);

            sqlGenerator.AppendUpdateOperation(stringBuilder, modificationCommand.Table,
                modificationCommand.ColumnValues.Zip(
                    updateParameters, (c, p) => new KeyValuePair<Column, string>(c.Key, p.Key)).ToArray(),
                modificationCommand.WhereClauses.Zip(
                    whereClauseParameters, (c, p) => new KeyValuePair<Column, string>(c.Key, p.Key)).ToArray());
        }

        private void AppendDeleteCommand(ModificationCommand modificationCommand, SqlGenerator sqlGenerator,
            StringBuilder stringBuilder, List<KeyValuePair<string, object>> parameters)
        {
            var whereClauseParameters = CreateParameters(modificationCommand.WhereClauses, parameters);

            sqlGenerator.AppendDeleteCommand(
                stringBuilder,
                modificationCommand.Table,
                modificationCommand.WhereClauses.Zip(
                    whereClauseParameters, (c, p) => new KeyValuePair<Column, string>(c.Key, p.Key)));
        }

        private static List<KeyValuePair<string, object>> CreateParameters(IEnumerable<KeyValuePair<Column, object>> values,
            List<KeyValuePair<string, object>> parameters)
        {
            var newParameters = new List<KeyValuePair<string, object>>();

            foreach (var parameter in values.Select(value => new KeyValuePair<string, object>("@p" + parameters.Count, value.Value)))
            {
                parameters.Add(parameter);
                newParameters.Add(parameter);
            }

            return newParameters;
        }

        public virtual void SaveStoreGeneratedValues(
            int commandIndex, [NotNull] IReadOnlyList<string> columnNames, [NotNull] IValueReader reader)
        {
            Check.NotNull(columnNames, "columnNames");
            Check.NotNull(reader, "reader");

            var stateEntry = _batchCommands[commandIndex].StateEntry;
            for (var i = 0; i < columnNames.Count; i++)
            {
                // TODO: Consider using strongly typed ReadValue instead of just <object>
                // Note that this call sets the value into a sidecar and will only commit to the actual entity
                // if SaveChanges is successful.
                stateEntry[GetProperty(stateEntry, columnNames[i])] = reader.ReadValue<object>(i);
            }
        }

        private static IProperty GetProperty(StateEntry stateEntry, string columnName)
        {
            // TODO: poor man's model to store mapping
            return stateEntry.EntityType.Properties.Single(p => p.StorageName == columnName);
        }

        public virtual bool CommandRequiresResultPropagation(int commandIndex)
        {
            return _batchCommands[commandIndex].RequiresResultPropagation;
        }
    }
}
