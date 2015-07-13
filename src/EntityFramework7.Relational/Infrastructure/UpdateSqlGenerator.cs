// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class UpdateSqlGenerator : IUpdateSqlGenerator
    {
        public virtual void AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schemaName = command.SchemaName;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendInsertCommand(commandStringBuilder, name, schemaName, writeOperations);

            if (readOperations.Length > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToArray();

                AppendSelectAffectedCommand(commandStringBuilder, name, schemaName, readOperations, keyOperations);
            }
            else
            {
                AppendSelectAffectedCountCommand(commandStringBuilder, name, schemaName);
            }
        }

        public virtual void AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schemaName = command.SchemaName;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendUpdateCommand(commandStringBuilder, name, schemaName, writeOperations, conditionOperations);

            if (readOperations.Length > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToArray();

                AppendSelectAffectedCommand(commandStringBuilder, name, schemaName, readOperations, keyOperations);
            }
            else
            {
                AppendSelectAffectedCountCommand(commandStringBuilder, name, schemaName);
            }
        }

        public virtual void AppendDeleteOperation(StringBuilder commandStringBuilder, ModificationCommand command)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schemaName = command.SchemaName;
            var conditionOperations = command.ColumnModifications.Where(o => o.IsCondition).ToArray();

            AppendDeleteCommand(commandStringBuilder, name, schemaName, conditionOperations);

            AppendSelectAffectedCountCommand(commandStringBuilder, name, schemaName);
        }

        public virtual void AppendInsertCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName,
            [NotNull] IReadOnlyList<ColumnModification> writeOperations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(writeOperations, nameof(writeOperations));

            AppendInsertCommandHeader(commandStringBuilder, name, schemaName, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations);
            commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
        }

        public virtual void AppendUpdateCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName,
            [NotNull] IReadOnlyList<ColumnModification> writeOperations,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(writeOperations, nameof(writeOperations));
            Check.NotNull(conditionOperations, nameof(conditionOperations));

            AppendUpdateCommandHeader(commandStringBuilder, name, schemaName, writeOperations);
            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
        }

        public virtual void AppendDeleteCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(conditionOperations, nameof(conditionOperations));

            AppendDeleteCommandHeader(commandStringBuilder, name, schemaName);
            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
        }

        public virtual void AppendSelectAffectedCountCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName)
        {
        }

        public virtual void AppendSelectAffectedCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName,
            [NotNull] IReadOnlyList<ColumnModification> readOperations,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(readOperations, nameof(readOperations));
            Check.NotNull(conditionOperations, nameof(conditionOperations));

            AppendSelectCommandHeader(commandStringBuilder, readOperations);
            AppendFromClause(commandStringBuilder, name, schemaName);
            // TODO: there is no notion of operator - currently all the where conditions check equality
            AppendWhereAffectedClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
        }

        protected virtual void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .Append("INSERT INTO ")
                .Append(DelimitIdentifier(name, schemaName));

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append(" (")
                    .AppendJoin(operations.Select(o => DelimitIdentifier(o.ColumnName)))
                    .Append(")");
            }
        }

        protected virtual void AppendDeleteCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));

            commandStringBuilder
                .Append("DELETE FROM ")
                .Append(DelimitIdentifier(name, schemaName));
        }

        protected virtual void AppendUpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .Append("UPDATE ")
                .Append(DelimitIdentifier(name, schemaName))
                .Append(" SET ")
                .AppendJoin(
                    operations,
                    (sb, v) => sb.Append(DelimitIdentifier(v.ColumnName)).Append(" = ").Append(v.ParameterName), ", ");
        }

        protected virtual void AppendSelectCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .Append("SELECT ")
                .AppendJoin(operations.Select(c => DelimitIdentifier(c.ColumnName)));
        }

        protected virtual void AppendFromClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schemaName)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));

            commandStringBuilder
                .AppendLine()
                .Append("FROM ")
                .Append(DelimitIdentifier(name, schemaName));
        }

        protected virtual void AppendValuesHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.AppendLine();
            commandStringBuilder.Append(operations.Count > 0 ? "VALUES " : "DEFAULT VALUES");
        }

        protected virtual void AppendValues(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append("(")
                    .AppendJoin(operations.Select(o => o.ParameterName))
                    .Append(")");
            }
        }

        protected virtual void AppendWhereClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("WHERE ")
                    .AppendJoin(operations, (sb, v) => AppendWhereCondition(sb, v, useOriginalValue: true), " AND ");
            }
        }

        protected virtual void AppendWhereAffectedClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .AppendLine()
                .Append("WHERE ");

            AppendRowsAffectedWhereCondition(commandStringBuilder, 1);

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append(" AND ")
                    .AppendJoin(operations, (sb, v) =>
                        {
                            if (v.IsKey)
                            {
                                if (v.IsRead)
                                {
                                    AppendIdentityWhereCondition(sb, v);
                                }
                                else
                                {
                                    AppendWhereCondition(sb, v, useOriginalValue: !v.IsWrite);
                                }
                            }
                        }, " AND ");
            }
        }

        protected abstract void AppendRowsAffectedWhereCondition([NotNull] StringBuilder commandStringBuilder, int expectedRowsAffected);

        protected virtual void AppendWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ColumnModification columnModification,
            bool useOriginalValue)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(columnModification, nameof(columnModification));

            commandStringBuilder
                .Append(DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append(useOriginalValue
                    ? columnModification.OriginalParameterName
                    : columnModification.ParameterName);
        }

        protected abstract void AppendIdentityWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ColumnModification columnModification);

        public virtual void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
        }

        public virtual string BatchCommandSeparator => ";";

        public virtual string BatchSeparator => string.Empty;

        // TODO: Consider adding a base class for all SQL generators (DDL, DML),
        // to avoid duplicating the five methods below.

        public virtual string DelimitIdentifier(string name, string schemaName)
            => (!string.IsNullOrEmpty(schemaName)
                ? DelimitIdentifier(schemaName) + "."
                : string.Empty)
               + DelimitIdentifier(Check.NotEmpty(name, nameof(name)));

        public virtual string DelimitIdentifier(string identifier)
            => "\"" + EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier))) + "\"";

        public virtual string EscapeIdentifier([NotNull] string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("\"", "\"\"");

        public virtual string GenerateLiteral(string literal)
            => "'" + EscapeLiteral(Check.NotNull(literal, nameof(literal))) + "'";

        public virtual string EscapeLiteral(string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");

        public virtual string GenerateLiteral(byte[] literal)
        {
            Check.NotNull(literal, nameof(literal));

            var builder = new StringBuilder();

            builder.Append("X'");

            var parts = literal.Select(b => b.ToString("X2", CultureInfo.InvariantCulture));
            foreach (var part in parts)
            {
                builder.Append(part);
            }

            builder.Append("'");

            return builder.ToString();
        }

        public virtual string GenerateLiteral(bool literal) => literal ? "TRUE" : "FALSE";
        public virtual string GenerateLiteral(char literal) => "'" + literal + "'";
        public virtual string GenerateLiteral(DateTime literal) => "TIMESTAMP '" + literal.ToString(@"yyyy-MM-dd HH\:mm\:ss.fffffff") + "'";
        public virtual string GenerateLiteral(DateTimeOffset literal) => "TIMESTAMP '" + literal.ToString(@"yyyy-MM-dd HH\:mm\:ss.fffffffzzz") + "'";

        public virtual string GenerateLiteral<T>(T? literal) where T : struct
            => literal.HasValue
                ? GenerateLiteral((dynamic)literal.Value)
                : "NULL";

        public virtual string GenerateLiteral(object literal)
            => literal != null
                ? string.Format(CultureInfo.InvariantCulture, "{0}", literal)
                : "NULL";

        public virtual string GenerateNextSequenceValueOperation(string sequenceName)
            => "SELECT NEXT VALUE FOR " + DelimitIdentifier(Check.NotNull(sequenceName, nameof(sequenceName)));
    }
}
