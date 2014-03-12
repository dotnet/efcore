// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Relational.Update
{
    internal class BatchExecutor
    {
        private readonly IEnumerable<ModificationCommandBatch> _commandBatches;
        private readonly SqlGenerator _sqlGenerator;

        public BatchExecutor([NotNull] IEnumerable<ModificationCommandBatch> commandBatches, [NotNull] SqlGenerator sqlGenerator)
        {
            _commandBatches = commandBatches;
            _sqlGenerator = sqlGenerator;
        }

        public async Task ExecuteAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            foreach (var commandbatch in _commandBatches)
            {
                await ExecuteBatchAsync(connection, commandbatch, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ExecuteBatchAsync([NotNull] DbConnection connection, [NotNull] ModificationCommandBatch commandBatch, CancellationToken cancellationToken)
        {
            using (var cmd = CreateCommand(connection, commandBatch))
            {
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    do
                    {
                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            // TODO: propagate results
                        }
                    }
                    while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));

                    if (reader.RecordsAffected != commandBatch.BatchCommands.Count())
                    {
                        throw new DbUpdateConcurrencyException(
                            Strings.FormatUpdateConcurrencyException(commandBatch.BatchCommands.Count(), reader.RecordsAffected));
                    }
                }
            }
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
    }
}
