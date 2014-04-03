// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Update
{
    public class BatchExecutor
    {
        private readonly IEnumerable<ModificationCommandBatch> _commandBatches;
        private readonly SqlGenerator _sqlGenerator;

        public BatchExecutor([NotNull] IEnumerable<ModificationCommandBatch> commandBatches, [NotNull] SqlGenerator sqlGenerator)
        {
            Check.NotNull(commandBatches, "commandBatches");
            Check.NotNull(sqlGenerator, "sqlGenerator");

            _commandBatches = commandBatches;
            _sqlGenerator = sqlGenerator;
        }

        public virtual async Task ExecuteAsync([NotNull] DbConnection connection, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");

            foreach (var commandbatch in _commandBatches)
            {
                await ExecuteBatchAsync(connection, commandbatch, cancellationToken).ConfigureAwait(false);
            }
        }

        protected virtual async Task ExecuteBatchAsync(
            [NotNull] DbConnection connection, [NotNull] ModificationCommandBatch commandBatch,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(commandBatch, "commandBatch");

            using (var cmd = CreateCommand(connection, commandBatch))
            {
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var resultSetIdx = -1;

                    do
                    {
                        resultSetIdx++;

                        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            if (!commandBatch.CommandRequiresResultPropagation(resultSetIdx))
                            {
                                throw new DbUpdateConcurrencyException(string.Format(Strings.FormatUpdateConcurrencyException(0, 1)));
                            }

                            SaveStoreGeneratedValues(commandBatch, reader, resultSetIdx);

                            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                throw new DbUpdateException(Strings.TooManyRowsForModificationCommand);
                            }
                        }
                        else
                        {
                            if (commandBatch.CommandRequiresResultPropagation(resultSetIdx))
                            {
                                throw new DbUpdateConcurrencyException(string.Format(Strings.FormatUpdateConcurrencyException(1, 0)));
                            }
                        }
                    }
                    while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
                }
            }
        }

        private static void SaveStoreGeneratedValues([NotNull] ModificationCommandBatch commandBatch, [NotNull] DbDataReader reader, int commandIndex)
        {
            var results = new KeyValuePair<string, object>[reader.FieldCount];
            for (var ordinal = 0; ordinal < results.Length; ordinal++)
            {
                results[ordinal] = new KeyValuePair<string, object>(reader.GetName(ordinal), reader.GetValue(ordinal));
            }

            commandBatch.SaveStoreGeneratedValues(commandIndex, results);
        }

        private DbCommand CreateCommand([NotNull] DbConnection connection, [NotNull] ModificationCommandBatch commandBatch)
        {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;

            List<KeyValuePair<string, object>> parameters;
            command.CommandText = commandBatch.CompileBatch(_sqlGenerator, out parameters);

            foreach (var parameter in parameters)
            {
                var dbParam = command.CreateParameter();
                dbParam.Direction = ParameterDirection.Input;
                dbParam.ParameterName = parameter.Key;
                dbParam.Value = parameter.Value;
                command.Parameters.Add(dbParam);
            }

            return command;
        }

        public virtual void PropagateResults()
        {
            foreach (var commandbatch in _commandBatches)
            {
                commandbatch.PropagateResults();
            }            
        }
    }
}
