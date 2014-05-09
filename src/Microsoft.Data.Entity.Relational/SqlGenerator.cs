// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class SqlGenerator
    {
        public virtual void AppendInsertOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] Table table,
            [NotNull] KeyValuePair<Column, string>[] columnsToParameters)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columnsToParameters, "columnsToParameters");

            AppendInsertCommand(commandStringBuilder, table, columnsToParameters);

            var storeGeneratedColumns = table.GetStoreGeneratedColumns().ToArray();

            if (storeGeneratedColumns.Any())
            {
                commandStringBuilder.Append(BatchCommandSeparator).AppendLine();

                var primaryKeyColumns = table.PrimaryKey.Columns;

                var whereConditions =
                    columnsToParameters.Where(p => primaryKeyColumns.Contains(p.Key));

                var storeGeneratedKeyColumns = storeGeneratedColumns.Where(primaryKeyColumns.Contains).ToArray();
                if (storeGeneratedKeyColumns.Any())
                {
                    whereConditions = whereConditions.Concat(
                        CreateWhereConditionsForStoreGeneratedKeys(storeGeneratedKeyColumns));
                }

                AppendSelectCommand(commandStringBuilder, table, storeGeneratedColumns, whereConditions);
            }
        }

        public abstract IEnumerable<KeyValuePair<Column, string>> CreateWhereConditionsForStoreGeneratedKeys(
            [NotNull] Column[] storeGeneratedKeyColumns);

        public virtual void AppendUpdateOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] Table table,
            [NotNull] KeyValuePair<Column, string>[] columnValues, [NotNull] KeyValuePair<Column, string>[] whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columnValues, "columnValues");
            Check.NotNull(whereConditions, "whereConditions");

            AppendUpdateCommand(commandStringBuilder, table, columnValues, whereConditions);

            var storeGeneratedNonKeyColumns =
                table.GetStoreGeneratedColumns().Except(table.PrimaryKey.Columns).ToArray();

            if (storeGeneratedNonKeyColumns.Any())
            {
                commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
                AppendSelectCommand(commandStringBuilder, table, storeGeneratedNonKeyColumns, whereConditions);
            }
        }

        public virtual void AppendInsertCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] Table table,
            [NotNull] IEnumerable<KeyValuePair<Column, string>> columnsToParameters)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columnsToParameters, "columnsToParameters");

            var columnsToParametersArray = columnsToParameters.ToArray();

            AppendInsertCommandHeader(commandStringBuilder, table, columnsToParametersArray.Select(c => c.Key));
            commandStringBuilder.Append(" ");
            AppendValues(commandStringBuilder, columnsToParametersArray.Select(c => c.Value));
        }

        public virtual void AppendDeleteCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] Table table,
            [NotNull] IEnumerable<KeyValuePair<Column, string>> whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(whereConditions, "whereConditions");

            AppendDeleteCommandHeader(commandStringBuilder, table);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, whereConditions);
        }

        public virtual void AppendUpdateCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] Table table,
            [NotNull] IEnumerable<KeyValuePair<Column, string>> columnValues,
            [NotNull] IEnumerable<KeyValuePair<Column, string>> whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columnValues, "columnValues");
            Check.NotNull(whereConditions, "whereConditions");

            AppendUpdateCommandHeader(commandStringBuilder, table, columnValues);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, whereConditions);
        }

        public virtual void AppendSelectCommand([NotNull] StringBuilder commandStringBuilder, [NotNull] Table table,
            [NotNull] IEnumerable<Column> columns, [NotNull] IEnumerable<KeyValuePair<Column, string>> whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columns, "columns");
            Check.NotNull(whereConditions, "whereConditions");

            AppendSelectCommandHeader(commandStringBuilder, columns);
            commandStringBuilder.Append(" ");
            AppendFromClause(commandStringBuilder, table);
            commandStringBuilder.Append(" ");
            // TODO: there is no notion of operator - currently all the where conditions check equality
            AppendWhereClause(commandStringBuilder, whereConditions);
        }

        public virtual void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] Table table, [NotNull] IEnumerable<Column> columns)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columns, "columns");

            columns = columns.ToArray();

            commandStringBuilder
                .Append("INSERT INTO ")
                .Append(table.Name);

            if (columns.Any())
            {
                commandStringBuilder
                    .Append(" (")
                    .AppendJoin(columns.Select(c => c.Name))
                    .Append(")");
            }
        }

        public virtual void AppendDeleteCommandHeader([NotNull] StringBuilder commandStringBuilder, [NotNull] Table table)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");

            commandStringBuilder
                .Append("DELETE FROM ")
                .Append(table.Name);
        }

        public virtual void AppendUpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] Table table,
            [NotNull] IEnumerable<KeyValuePair<Column, string>> columnValues)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columnValues, "columnValues");

            commandStringBuilder
                .Append("UPDATE ")
                .Append(table.Name)
                .Append(" SET ")
                .AppendJoin(columnValues, (sb, v) => sb.Append(v.Key.Name).Append(" = ").Append(v.Value), ", ");
        }

        public virtual void AppendSelectCommandHeader([NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<Column> columns)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(columns, "columns");

            commandStringBuilder
                .Append("SELECT ")
                .AppendJoin(columns.Select(c => c.Name));
        }

        public virtual void AppendFromClause([NotNull] StringBuilder commandStringBuilder, [NotNull] Table table)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");

            commandStringBuilder
                .Append("FROM ")
                .Append(table.Name);
        }

        public virtual void AppendValues([NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<string> valueParameterNames)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(valueParameterNames, "valueParameterNames");

            valueParameterNames = valueParameterNames.ToArray();

            if (valueParameterNames.Any())
            {
                commandStringBuilder
                    .Append("VALUES (")
                    .AppendJoin(valueParameterNames)
                    .Append(")");
            }
            else
            {
                commandStringBuilder
                    .Append("DEFAULT VALUES");
            }
        }

        public virtual void AppendWhereClause(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<KeyValuePair<Column, string>> whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(whereConditions, "whereConditions");

            commandStringBuilder
                .Append("WHERE ")
                .AppendJoin(whereConditions, (sb, v) => sb.Append(v.Key.Name).Append(" = ").Append(v.Value), " AND ");
        }

        public virtual void AppendBatchHeader([NotNull] StringBuilder commandStringBuilder)
        {
        }

        public virtual string BatchCommandSeparator
        {
            get { return ";"; }
        }
    }
}
