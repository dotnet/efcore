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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class BatchExecutor
    {
        private readonly SqlGenerator _sqlGenerator;
        private readonly RelationalConnection _connection;

        public BatchExecutor(
            [NotNull] SqlGenerator sqlGenerator,
            [NotNull] RelationalConnection connection)
        {
            Check.NotNull(sqlGenerator, "sqlGenerator");
            Check.NotNull(connection, "connection");

            _sqlGenerator = sqlGenerator;
            _connection = connection;
        }

        public virtual RelationalConnection Connection
        {
            get { return _connection; }
        }

        public virtual async Task ExecuteAsync(
            [NotNull] IEnumerable<ModificationCommandBatch> commandBatches,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(commandBatches, "commandBatches");

            foreach (var commandbatch in commandBatches)
            {
                await ExecuteBatchAsync(commandbatch, cancellationToken).ConfigureAwait(false);
            }
        }

        protected virtual async Task ExecuteBatchAsync(
            [NotNull] ModificationCommandBatch commandBatch,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(commandBatch, "commandBatch");

            using (var cmd = CreateCommand(Connection.DbConnection, commandBatch))
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
            // TODO: Consider if this can be done with well-known result order (like materialization) rather than column names
            var names = new String[reader.FieldCount];
            for (var ordinal = 0; ordinal < names.Length; ordinal++)
            {
                names[ordinal] = reader.GetName(ordinal);
            }

            commandBatch.SaveStoreGeneratedValues(commandIndex, names, new RelationalTypedValueReader(reader));
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
                
                // TODO: This is a hack that allows nullable strings to be saved. We actually need proper handling
                // for parameter types, but this unblocks saving nullable strings for now. Other nullable types
                // may not work.
                dbParam.Value = parameter.Value ?? DBNull.Value;
                
                command.Parameters.Add(dbParam);
            }

            return command;
        }
    }
}
