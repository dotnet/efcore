// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Data.Relational.Update
{
    internal class ModificationCommandBatch
    {
        private readonly ModificationCommand[] _batchCommands;

        public ModificationCommandBatch([NotNull] ModificationCommand[] batchCommands)
        {
            Debug.Assert(batchCommands.Any(), "batchCommands array is empty");

            _batchCommands = batchCommands;
        }

        public IEnumerable<ModificationCommand> BatchCommands
        {
            get
            {
                return _batchCommands;
            }
        }

        public string CompileBatch([NotNull] SqlGenerator sqlGenerator, out List<KeyValuePair<string, object>> parameters)
        {
            var stringBuilder = new StringBuilder();
            parameters = new List<KeyValuePair<string, object>>();

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

            sqlGenerator.AppendInsertCommand(
                stringBuilder, 
                modificationCommand.TableName,
                modificationCommand.ColumnValues.Select(c => c.Key),
                commandParameters.Select(p => p.Key));
        }

        private void AppendUpdateCommand(ModificationCommand modificationCommand, SqlGenerator sqlGenerator, 
            StringBuilder stringBuilder, List<KeyValuePair<string, object>> parameters)
        {
            var updateParameters = CreateParameters(modificationCommand.ColumnValues, parameters);
            var whereClauseParameters = CreateParameters(modificationCommand.WhereClauses, parameters);

            sqlGenerator.AppendUpdateCommand(
                stringBuilder, 
                modificationCommand.TableName,
                modificationCommand.ColumnValues.Zip(
                    updateParameters, (c, p) => new KeyValuePair<string, string>(c.Key, p.Key)),
                modificationCommand.WhereClauses.Zip(
                    whereClauseParameters, (c, p) => new KeyValuePair<string, string>(c.Key, p.Key)));
        }

        private void AppendDeleteCommand(ModificationCommand modificationCommand, SqlGenerator sqlGenerator,
            StringBuilder stringBuilder, List<KeyValuePair<string, object>> parameters)
        {
            var whereClauseParameters = CreateParameters(modificationCommand.WhereClauses, parameters);

            sqlGenerator.AppendDeleteCommand(
                stringBuilder, 
                modificationCommand.TableName,
                modificationCommand.WhereClauses.Zip(
                    whereClauseParameters, (c, p) => new KeyValuePair<string, string>(c.Key, p.Key)));

        }

        private IEnumerable<KeyValuePair<string, object>> CreateParameters(IEnumerable<KeyValuePair<string, object>> values, 
            List<KeyValuePair<string, object>> parameters)
        {
            foreach (var parameter in values.Select(value => new KeyValuePair<string, object>("@p" + parameters.Count, value.Value)))
            {
                parameters.Add(parameter);
                yield return parameter;
            }
        }
    }
}
