// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Relational.Update
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
    }
}
