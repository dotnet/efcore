// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class BatchExecutor
    {
        private readonly SqlGenerator _sqlGenerator;
        private readonly RelationalConnection _connection;
        private readonly RelationalTypeMapper _typeMapper;

        public BatchExecutor(
            [NotNull] SqlGenerator sqlGenerator,
            [NotNull] RelationalConnection connection,
            [NotNull] RelationalTypeMapper typeMapper)
        {
            Check.NotNull(sqlGenerator, "sqlGenerator");
            Check.NotNull(connection, "connection");
            Check.NotNull(typeMapper, "typeMapper");

            _sqlGenerator = sqlGenerator;
            _connection = connection;
            _typeMapper = typeMapper;
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

            // TODO: It is likely the code below won't really work correctly in the wild because
            // we currently don't actually create batches of commands and because there is no mechanism
            // for getting back affected rows for update/delete commands. Leaving the code here right now
            // because it might be a decent starting point for a real implementation.

            using (var storeCommand = CreateStoreCommand(Connection.DbConnection, commandBatch))
            {
                var modificationCommands = commandBatch.ModificationCommands;
                using (var reader = await storeCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var commandIndex = -1;
                    do
                    {
                        commandIndex++;
                        var tableModification = modificationCommands[commandIndex];

                        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            if (!tableModification.RequiresResultPropagation)
                            {
                                throw new DbUpdateConcurrencyException(string.Format(Strings.FormatUpdateConcurrencyException(0, 1)));
                            }

                            tableModification.PropagateResults(new RelationalTypedValueReader(reader));

                            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                throw new DbUpdateException(Strings.TooManyRowsForModificationCommand);
                            }
                        }
                        else
                        {
                            if (tableModification.RequiresResultPropagation)
                            {
                                throw new DbUpdateConcurrencyException(string.Format(Strings.FormatUpdateConcurrencyException(1, 0)));
                            }
                        }
                    }
                    while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
                }
            }
        }

        private DbCommand CreateStoreCommand([NotNull] DbConnection connection, [NotNull] ModificationCommandBatch commandBatch)
        {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandBatch.CompileBatch(_sqlGenerator);

            foreach (var columnModification in commandBatch.ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                if (columnModification.ParameterName != null)
                {
                    // TODO: It would be nice to just pass IProperty to the type mapper, but Migrations uses its own
                    // store model for which there is no easy way to get an IProperty.

                    var property = columnModification.Property;

                    // TODO: Avoid doing Contains check everywhere we need to know if a property is part of a key
                    var isKey = property.EntityType.GetKey().Properties.Contains(property);

                    command.Parameters.Add(
                        _typeMapper
                            .GetTypeMapping(
                                property.ColumnType(), property.StorageName, property.PropertyType, isKey, property.IsConcurrencyToken)
                            .CreateParameter(command, columnModification));
                }
            }

            return command;
        }
    }
}
