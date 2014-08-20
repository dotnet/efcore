// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class ReaderModificationCommandBatch : ModificationCommandBatch
    {
        private ImmutableList<ModificationCommand> _modificationCommands = ImmutableList<ModificationCommand>.Empty;

        protected string SqlScript { get; set; }

        public override IReadOnlyList<ModificationCommand> ModificationCommands
        {
            get { return _modificationCommands; }
        }

        public override bool AddCommand(ModificationCommand modificationCommand, SqlGenerator sqlGenerator)
        {
            Check.NotNull(modificationCommand, "modificationCommand");
            Check.NotNull(sqlGenerator, "sqlGenerator");

            var newSqlScript = UpdateCommandText(modificationCommand, sqlGenerator);

            if (!CanAddCommand(modificationCommand, newSqlScript))
            {
                return false;
            }

            _modificationCommands = _modificationCommands.Add(modificationCommand);

            SqlScript = newSqlScript.ToString();

            return true;
        }

        protected abstract bool CanAddCommand(ModificationCommand modificationCommand, StringBuilder newSql);

        protected virtual StringBuilder UpdateCommandText([NotNull] ModificationCommand newModificationCommand, [NotNull] SqlGenerator sqlGenerator)
        {
            var stringBuilder = new StringBuilder();

            if (SqlScript == null)
            {
                sqlGenerator.AppendBatchHeader(stringBuilder);
            }
            else
            {
                stringBuilder.Append(SqlScript);
            }

            var entityState = newModificationCommand.EntityState;
            var operations = newModificationCommand.ColumnModifications;
            var schemaQualifiedName = newModificationCommand.SchemaQualifiedName;

            switch (entityState)
            {
                case EntityState.Added:
                    sqlGenerator.AppendInsertOperation(stringBuilder, schemaQualifiedName, operations);
                    break;
                case EntityState.Modified:
                    sqlGenerator.AppendUpdateOperation(stringBuilder, schemaQualifiedName, operations);
                    break;
                case EntityState.Deleted:
                    sqlGenerator.AppendDeleteOperation(stringBuilder, schemaQualifiedName, operations);
                    break;
            }

            return stringBuilder;
        }

        protected virtual DbCommand CreateStoreCommand(
            [NotNull] DbTransaction transaction,
            [NotNull] RelationalTypeMapper typeMapper)
        {
            var command = transaction.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = SqlScript;
            command.Transaction = transaction;

            foreach (var columnModification in ModificationCommands.SelectMany(t => t.ColumnModifications))
            {
                PopulateParameters(command, columnModification, typeMapper);
            }

            return command;
        }

        public override async Task<int> ExecuteAsync(
            RelationalTransaction transaction,
            RelationalTypeMapper typeMapper,
            DbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(typeMapper, "typeMapper");
            Check.NotNull(context, "context");
            Check.NotNull(logger, "logger");

            if (logger.IsEnabled(TraceType.Verbose))
            {
                // TODO: Write parameter values
                logger.WriteSql(SqlScript);
            }

            var totalRowsAffected = 0;
            using (var storeCommand = CreateStoreCommand(transaction.DbTransaction, typeMapper))
            {
                var commandIndex = 0;
                try
                {
                    using (var reader = await storeCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        do
                        {
                            var tableModification = ModificationCommands[commandIndex];

                            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                if (tableModification.RequiresResultPropagation)
                                {
                                    tableModification.PropagateResults(new RelationalTypedValueReader(reader));
                                    totalRowsAffected++;
                                }
                                else
                                {
                                    var rowsAffected = reader.GetFieldValue<int>(0);
                                    if (rowsAffected != 1)
                                    {
                                        throw new DbUpdateConcurrencyException(
                                            Strings.FormatUpdateConcurrencyException(1, rowsAffected),
                                            context,
                                            tableModification.StateEntries);
                                    }
                                    totalRowsAffected += rowsAffected;
                                }
                            }
                            else
                            {
                                throw new DbUpdateConcurrencyException(
                                    Strings.FormatUpdateConcurrencyException(1, 0),
                                    context,
                                    tableModification.StateEntries);
                            }

                            commandIndex++;
                        }
                        while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
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
                        context,
                        ex,
                        commandIndex < ModificationCommands.Count ? ModificationCommands[commandIndex].StateEntries : new StateEntry[0]);
                }
            }

            return totalRowsAffected;
        }

        protected virtual void PopulateParameters(DbCommand command, ColumnModification columnModification, RelationalTypeMapper typeMapper)
        {
            if (columnModification.ParameterName != null
                || columnModification.OriginalParameterName != null)
            {
                var property = columnModification.Property;

                var isKey = columnModification.IsKey
                            || property.IsForeignKey();

                // TODO: It would be nice to just pass IProperty to the type mapper, but Migrations uses its own
                // store model for which there is no easy way to get an IProperty.
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
    }
}
