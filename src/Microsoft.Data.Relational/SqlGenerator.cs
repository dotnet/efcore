// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public abstract class SqlGenerator
    {
        public virtual void AppendInsertOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] KeyValuePair<string, string>[] keyColumns, [NotNull] KeyValuePair<string, string>[] columnsToParameters,
            [NotNull] KeyValuePair<string, ValueGenerationStrategy>[] storeGeneratedColumns)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(keyColumns, "keyColumns");
            Check.NotNull(columnsToParameters, "columnsToParameters");
            Check.NotNull(storeGeneratedColumns, "storeGeneratedColumns");

            Contract.Assert(keyColumns.Any(), "keyColumnNames is empty");

            AppendInsertCommand(commandStringBuilder, tableName, columnsToParameters);
            if (storeGeneratedColumns.Any())
            {
                commandStringBuilder.Append(BatchCommandSeparator).AppendLine();
                AppendModificationOperationSelectCommand(commandStringBuilder, tableName, keyColumns, 
                    storeGeneratedColumns, columnsToParameters);
            }
        }

        public virtual void AppendInsertCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName, 
            [NotNull] IEnumerable<KeyValuePair<string, string>> columnsToParameters)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(columnsToParameters, "columnsToParameters");

            var columnsToParametersArray = columnsToParameters.ToArray();

            AppendInsertCommandHeader(commandStringBuilder, tableName, columnsToParametersArray.Select(c => c.Key));
            commandStringBuilder.Append(" ");
            AppendValues(commandStringBuilder, columnsToParametersArray.Select(c => c.Value));
        }

        public virtual void AppendDeleteCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] IEnumerable<KeyValuePair<string, string>> whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(tableName, "tableName");
            Check.NotNull(whereConditions, "whereConditions");

            AppendDeleteCommandHeader(commandStringBuilder, tableName);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, whereConditions);
        }

        public virtual void AppendUpdateCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] IEnumerable<KeyValuePair<string, string>> columnValues,
            [NotNull] IEnumerable<KeyValuePair<string, string>> whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(columnValues, "columnValues");
            Check.NotNull(whereConditions, "whereConditions");

            AppendUpdateCommandHeader(commandStringBuilder, tableName, columnValues);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, whereConditions);
        }

        public virtual void AppendModificationOperationSelectCommand([NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] KeyValuePair<string, string>[] keyColumns, [NotNull] KeyValuePair<string, ValueGenerationStrategy>[] storeGeneratedColumns,
            [NotNull] KeyValuePair<string, string>[] columnsToParameters)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(keyColumns, "keyColumns");
            Check.NotNull(storeGeneratedColumns, "storeGeneratedColumns");
            Check.NotNull(columnsToParameters, "columnsToParameters");

            AppendSelectCommandHeader(commandStringBuilder, storeGeneratedColumns.Select(c => c.Key));
            commandStringBuilder.Append(" ");
            AppendFromClause(commandStringBuilder, tableName);
            commandStringBuilder.Append(" ");

            var knownKeyValues = columnsToParameters.Where(c => keyColumns.Select(k => k.Key).Contains(c.Key));
            var generatedKeys = storeGeneratedColumns.Where(c => keyColumns.Select(k => k.Key).Contains(c.Key));
            AppendModificationOperationSelectWhereClause(commandStringBuilder, knownKeyValues, generatedKeys);
        }

        public virtual void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName, [NotNull] IEnumerable<string> columnNames)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(columnNames, "columnNames");

            commandStringBuilder
                .Append("INSERT INTO ")
                .Append(tableName)
                .Append(" (")
                .AppendJoin(columnNames);

            // TODO: may be fine if all columns are database generated in which case we should not append brackets at all
            Contract.Assert(commandStringBuilder[commandStringBuilder.Length - 1] != '(', "empty columnNames");

            commandStringBuilder.Append(")");
        }

        public virtual void AppendDeleteCommandHeader([NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");

            commandStringBuilder
                .Append("DELETE FROM ")
                .Append(tableName);
        }

        public virtual void AppendUpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] IEnumerable<KeyValuePair<string, string>> columnValues)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(columnValues, "columnValues");

            commandStringBuilder
                .Append("UPDATE ")
                .Append(tableName)
                .Append(" SET ")
                .AppendJoin(columnValues, (sb, v) => sb.Append(v.Key).Append(" = ").Append(v.Value), ", ");
        }

        public virtual void AppendSelectCommandHeader([NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<string> columnNames)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(columnNames, "columnNames");

            commandStringBuilder
                .Append("SELECT ")
                .AppendJoin(columnNames);
        }

        public virtual void AppendFromClause([NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(tableName, "tableName");

            commandStringBuilder
                .Append("FROM ")
                .Append(tableName);
        }

        public abstract void AppendModificationOperationSelectWhereClause([NotNull] StringBuilder commandStringBuilder,
            [NotNull] IEnumerable<KeyValuePair<string, string>> knownKeyValues,
            [NotNull] IEnumerable<KeyValuePair<string, ValueGenerationStrategy>> generatedKeys);

        public virtual void AppendValues([NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<string> valueParameterNames)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(valueParameterNames, "valueParameterNames");

            commandStringBuilder
                .Append("VALUES (")
                .AppendJoin(valueParameterNames)
                .Append(")");

            Contract.Assert(!commandStringBuilder.ToString().EndsWith("()"), "empty valueParameterNames");
        }

        public virtual void AppendWhereClause(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<KeyValuePair<string, string>> whereConditions)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(whereConditions, "whereConditions");

            commandStringBuilder
                .Append("WHERE ")
                .AppendJoin(whereConditions, (sb, v) => sb.Append(v.Key).Append(" = ").Append(v.Value), " AND ");
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
