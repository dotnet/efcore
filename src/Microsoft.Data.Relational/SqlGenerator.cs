// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class SqlGenerator
    {
        public virtual void AppendInsertCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName, [NotNull] IEnumerable<String> columnNames,
            [NotNull] IEnumerable<string> valueParameterNames)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(tableName, "tableName");
            Check.NotNull(columnNames, "columnNames");
            Check.NotNull(valueParameterNames, "valueParameterNames");

            AppendInsertCommandHeader(commandStringBuilder, tableName, columnNames);
            commandStringBuilder.Append(" ");
            AppendValues(commandStringBuilder, valueParameterNames);
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
            Check.NotNull(tableName, "tableName");
            Check.NotNull(columnValues, "columnValues");
            Check.NotNull(whereConditions, "whereConditions");

            AppendUpdateCommandHeader(commandStringBuilder, tableName, columnValues);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, whereConditions);
        }

        public virtual void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName, [NotNull] IEnumerable<string> columnNames)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(tableName, "tableName");
            Check.NotNull(columnNames, "columnNames");

            commandStringBuilder
                .Append("INSERT INTO ")
                .Append(tableName)
                .Append(" (")
                .AppendJoin(columnNames, ", ");

            // TODO: may be fine if all columns are database generated in which case we should not append brackets at all
            Debug.Assert(commandStringBuilder[commandStringBuilder.Length - 1] != '(', "empty columnNames");

            commandStringBuilder.Append(")");
        }

        public virtual void AppendDeleteCommandHeader([NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(tableName, "tableName");

            commandStringBuilder
                .Append("DELETE FROM ")
                .Append(tableName);
        }

        public virtual void AppendUpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] IEnumerable<KeyValuePair<string, string>> columnValues)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(tableName, "tableName");
            Check.NotNull(columnValues, "columnValues");

            commandStringBuilder
                .Append("UPDATE ")
                .Append(tableName)
                .Append(" SET ")
                .AppendJoin(columnValues, (sb, v) => sb.Append(v.Key).Append(" = ").Append(v.Value), ", ");
        }

        public virtual void AppendValues([NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<string> valueParameterNames)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(valueParameterNames, "valueParameterNames");

            commandStringBuilder
                .Append("VALUES (")
                .AppendJoin(valueParameterNames, ", ")
                .Append(")");

            Debug.Assert(!commandStringBuilder.ToString().EndsWith("()"), "empty valueParameterNames");
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

        public virtual string BatchCommandSeparator
        {
            get { return ";"; }
        }
    }
}
