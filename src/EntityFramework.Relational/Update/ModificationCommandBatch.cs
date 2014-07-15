// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using Microsoft.Data.Entity.Update;

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
            _sql = GenerateCommandText(sqlGenerator);

            return false;
        }

        public virtual IReadOnlyList<ModificationCommand> ModificationCommands
        {
            get { return _modificationCommands; }
        }

        protected virtual string GenerateCommandText([NotNull] SqlGenerator sqlGenerator)
        {
            var stringBuilder = new StringBuilder();

            sqlGenerator.AppendBatchHeader(stringBuilder);

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
            }

            return stringBuilder.ToString();
        }

        public virtual async Task<int> ExecuteAsync(
            [NotNull] RelationalTransaction transaction,
            [NotNull] RelationalTypeMapper typeMapper,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(typeMapper, "typeMapper");

            using (var storeCommand = CreateStoreCommand(transaction.DbTransaction, typeMapper))
            {
                var commandIndex = 0;
                try
                {
                    using (var reader = await storeCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    {
                        do
                        {
                            var tableModification = ModificationCommands[commandIndex];

                            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                if (tableModification.RequiresResultPropagation)
                                {
                                    tableModification.PropagateResults(new RelationalTypedValueReader(reader));
                                }
                                else
                                {
                                    var rowsAffected = reader.GetFieldValue<int>(0);
                                    if (rowsAffected != 1)
                                    {
                                        throw new DbUpdateConcurrencyException(string.Format(Strings.FormatUpdateConcurrencyException(1, rowsAffected)), tableModification.StateEntries);
                                    }
                                }
                            }
                            else
                            {
                                throw new DbUpdateConcurrencyException(string.Format(Strings.FormatUpdateConcurrencyException(1, 0)), tableModification.StateEntries);
                            }

                            commandIndex++;
                        }
                        while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
                    }
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DbUpdateException(
                        Strings.FormatUpdateStoreException(),
                        ex,
                        commandIndex < ModificationCommands.Count ? ModificationCommands[commandIndex].StateEntries : null);
                }
            }

            // TODO Return the actual results once we can get them
            return 1;
        }

        protected virtual DbCommand CreateStoreCommand(
            [NotNull] DbTransaction transaction,
            [NotNull] RelationalTypeMapper typeMapper)
        {
            var command = transaction.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = _sql;
            command.Transaction = transaction;

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                if (columnModification.ParameterName != null
                    || columnModification.OriginalParameterName != null)
                {
                    // TODO: It would be nice to just pass IProperty to the type mapper, but Migrations uses its own
                    // store model for which there is no easy way to get an IProperty.

                    var property = columnModification.Property;

                    // TODO: Perf: Avoid doing Contains check everywhere we need to know if a property is part of a foreign key
                    var isKey = columnModification.IsKey
                                || property.IsForeignKey();

                    var typeMapping = typeMapper
                        .GetTypeMapping(
                            property.ColumnType(), property.ColumnName(), property.PropertyType, isKey, property.IsConcurrencyToken);

                    if (columnModification.ParameterName != null)
                    {
                        command.Parameters.Add(typeMapping.CreateParameter(command, columnModification, useOriginalValue: false));
                    }

                    if (columnModification.OriginalParameterName != null)
                    {
                        command.Parameters.Add(typeMapping.CreateParameter(command, columnModification, useOriginalValue: true));
                    }
                }
            }

            return command;
        }
    }
}
