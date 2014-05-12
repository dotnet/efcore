// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class SqlGenerator
    {
        public virtual void AppendInsertOperation(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();

            AppendInsertCommand(commandStringBuilder, tableName, writeOperations);

            var readOperations = operations.Where(o => o.IsRead).ToArray();
            if (readOperations.Length > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToArray();

                commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
                AppendSelectCommand(commandStringBuilder, tableName, readOperations, keyOperations);
            }
        }

        public virtual void AppendUpdateOperation(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();

            AppendUpdateCommand(commandStringBuilder, tableName, writeOperations, conditionOperations);

            var readOperations = operations.Where(o => o.IsRead).ToArray();

            if (readOperations.Length > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToArray();

                commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
                AppendSelectCommand(commandStringBuilder, tableName, readOperations, keyOperations);
            }
        }

        public virtual void AppendDeleteOperation(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");

            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();

            AppendDeleteCommandHeader(commandStringBuilder, tableName);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, conditionOperations);
        }

        protected virtual void AppendInsertCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");

            AppendInsertCommandHeader(commandStringBuilder, tableName, operations);
            commandStringBuilder.Append(" ");
            AppendValues(commandStringBuilder, operations);
        }

        protected virtual void AppendUpdateCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");
            Check.NotNull(conditionOperations, "conditionOperations");

            AppendUpdateCommandHeader(commandStringBuilder, tableName, operations);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, conditionOperations);
        }

        protected virtual void AppendSelectCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");
            Check.NotNull(conditionOperations, "conditionOperations");

            AppendSelectCommandHeader(commandStringBuilder, operations);
            commandStringBuilder.Append(" ");
            AppendFromClause(commandStringBuilder, tableName);
            commandStringBuilder.Append(" ");
            // TODO: there is no notion of operator - currently all the where conditions check equality
            AppendWhereClause(commandStringBuilder, conditionOperations);
        }

        protected virtual void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");

            commandStringBuilder
                .Append("INSERT INTO ")
                .Append(QuoteIdentifier(tableName));

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append(" (")
                    .AppendJoin(operations.Select(o => QuoteIdentifier(o.ColumnName)))
                    .Append(")");
            }
        }

        protected virtual void AppendDeleteCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");

            commandStringBuilder
                .Append("DELETE FROM ")
                .Append(QuoteIdentifier(tableName));
        }

        protected virtual void AppendUpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(operations, "operations");

            commandStringBuilder
                .Append("UPDATE ")
                .Append(QuoteIdentifier(tableName))
                .Append(" SET ")
                .AppendJoin(
                operations, 
                (sb, v) => sb.Append(QuoteIdentifier(v.ColumnName)).Append(" = ").Append(v.ParameterName), ", ");
        }

        protected virtual void AppendSelectCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(operations, "operations");

            commandStringBuilder
                .Append("SELECT ")
                .AppendJoin(operations.Select(c => QuoteIdentifier(c.ColumnName)));
        }

        protected virtual void AppendFromClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string tableName)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");

            commandStringBuilder
                .Append("FROM ")
                .Append(QuoteIdentifier(tableName));
        }

        protected virtual void AppendValues(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(operations, "operations");

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append("VALUES (")
                    .AppendJoin(operations.Select(o => o.ParameterName))
                    .Append(")");
            }
            else
            {
                commandStringBuilder
                    .Append("DEFAULT VALUES");
            }
        }

        protected virtual void AppendWhereClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(operations, "operations");

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append("WHERE ")
                    .AppendJoin(operations, (sb, v) =>
                        {
                            if (v.Property.ValueGenerationStrategy == ValueGenerationStrategy.StoreIdentity
                                && v.IsRead)
                            {
                                AppendIdentityWhereCondition(sb, v);
                            }
                            else
                            {
                                AppendWhereCondition(sb, v);
                            }
                        }, " AND ");
            }
        }

        protected virtual void AppendWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ColumnModification columnModification)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(columnModification, "columnModification");

            commandStringBuilder
                .Append(QuoteIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append(columnModification.ParameterName);
        }

        protected abstract void AppendIdentityWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ColumnModification columnModification);

        public virtual void AppendBatchHeader([NotNull] StringBuilder commandStringBuilder)
        {
        }

        public virtual string BatchCommandSeparator
        {
            get { return ";"; }
        }

        public virtual string QuoteIdentifier([NotNull] string identifier)
        {
            Check.NotNull(identifier, "identifier");

            return "[" + identifier.Replace("]", "]]") + "]";
        }
    }
}
