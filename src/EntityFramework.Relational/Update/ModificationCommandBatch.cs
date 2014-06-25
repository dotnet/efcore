// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommandBatch
    {
        private readonly List<ModificationCommand> _modificationCommands = new List<ModificationCommand>();
        private string _sql;

        public virtual bool AddCommand(
            [NotNull] ModificationCommand modificationCommand,
            [NotNull] SqlGenerator sqlGenerator)
        {
            Check.NotNull(modificationCommand, "modificationCommand");
            Check.NotNull(sqlGenerator, "sqlGenerator");

            _modificationCommands.Add(modificationCommand);
            _sql = CompileBatch(sqlGenerator);

            return false;
        }

        public virtual IReadOnlyList<ModificationCommand> ModificationCommands
        {
            get { return _modificationCommands; }
        }
        
        protected virtual string CompileBatch([NotNull] SqlGenerator sqlGenerator)
        {
            var stringBuilder = new StringBuilder();

            sqlGenerator.AppendBatchHeader(stringBuilder);
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append(sqlGenerator.BatchCommandSeparator).AppendLine();
            }

            foreach (var modificationCommand in _modificationCommands)
            {
                var entityState = modificationCommand.EntityState;
                var operations = modificationCommand.ColumnModifications;
                var tableName = modificationCommand.TableName;

                if (entityState == EntityState.Added)
                {
                    sqlGenerator.AppendInsertOperation(stringBuilder, tableName, operations);
                }

                if (entityState == EntityState.Modified)
                {
                    sqlGenerator.AppendUpdateOperation(stringBuilder, tableName, operations);
                }

                if (entityState == EntityState.Deleted)
                {
                    sqlGenerator.AppendDeleteOperation(stringBuilder, tableName, operations);
                }

                stringBuilder.Append(sqlGenerator.BatchCommandSeparator).AppendLine();
            }

            return stringBuilder.ToString();
        }
        
        public virtual async Task<int> ExecuteAsync(
            [NotNull] RelationalConnection connection,
            [NotNull] RelationalTypeMapper typeMapper,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(typeMapper, "typeMapper");

            // TODO: It is likely the code below won't really work correctly in the wild because
            // we currently don't actually create batches of commands and because there is no mechanism
            // for getting back affected rows for update/delete commands. Leaving the code here right now
            // because it might be a decent starting point for a real implementation.
            using (var storeCommand = CreateStoreCommand(connection.DbConnection, typeMapper))
            {
                using (var reader = await storeCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var commandIndex = -1;
                    do
                    {
                        commandIndex++;
                        var tableModification = ModificationCommands[commandIndex];

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

            // TODO Return the actual results once we can get them
            return 1;
        }

        protected virtual DbCommand CreateStoreCommand(
            [NotNull] DbConnection connection,
            [NotNull] RelationalTypeMapper typeMapper)
        {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = _sql;

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                if (columnModification.ParameterName != null)
                {
                    // TODO: It would be nice to just pass IProperty to the type mapper, but Migrations uses its own
                    // store model for which there is no easy way to get an IProperty.

                    var property = columnModification.Property;

                    // TODO: Avoid doing Contains check everywhere we need to know if a property is part of a key
                    var isKey = property.EntityType.GetKey().Properties.Contains(property)
                                || property.EntityType.ForeignKeys.SelectMany(k => k.Properties).Contains(property);

                    command.Parameters.Add(
                        typeMapper
                            .GetTypeMapping(
                                property.ColumnType(), property.StorageName, property.PropertyType, isKey, property.IsConcurrencyToken)
                            .CreateParameter(command, columnModification));
                }
            }

            return command;
        }
    }
}
