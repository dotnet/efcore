// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class RelationalDataStore : DataStore
    {
        private readonly string _nameOrConnectionString;

        public RelationalDataStore([NotNull] string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            _nameOrConnectionString = nameOrConnectionString;
        }

        public virtual string NameOrConnectionString
        {
            get { return _nameOrConnectionString; }
        }

        public override Task<int> SaveChangesAsync(IEnumerable<EntityEntry> entityEntries)
        {
            // Entities are dependency ordered.

            return base.SaveChangesAsync(entityEntries);
        }

        protected virtual void AppendInsertCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName, [NotNull] IEnumerable<String> columnNames,
            IEnumerable<string> valueParameterNames)
        {
            AppendInsertCommandHeader(commandStringBuilder, tableName, columnNames);
            commandStringBuilder.Append(" ");
            AppendValues(commandStringBuilder, valueParameterNames);
        }

        protected virtual void AppendDeleteCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            IEnumerable<KeyValuePair<string, string>> whereConditions)
        {
            AppendDeleteCommandHeader(commandStringBuilder, tableName);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, whereConditions);
        }
        protected virtual void AppendUpdateCommand(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] IEnumerable<KeyValuePair<string, string>> columnValues,
            [NotNull] IEnumerable<KeyValuePair<string, string>> whereConditions)
        {
            AppendUpdateCommandHeader(commandStringBuilder, tableName, columnValues);
            commandStringBuilder.Append(" ");
            AppendWhereClause(commandStringBuilder, whereConditions);
        }

        protected virtual void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName, [NotNull] IEnumerable<string> columnNames)
        {
            commandStringBuilder
                .Append("INSERT INTO ")
                .Append(tableName)
                .Append(" (")
                .AppendJoin(columnNames, ", ");

            // TODO: may be fine if all columns are database generated in which case we should not append brackets at all
            Debug.Assert(commandStringBuilder[commandStringBuilder.Length - 1] != '(', "empty columnNames");

            commandStringBuilder.Append(")");
        }

        protected virtual void AppendDeleteCommandHeader([NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName)
        {
            commandStringBuilder
                .Append("DELETE FROM ")
                .Append(tableName);
        }

        protected virtual void AppendUpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] string tableName,
            [NotNull] IEnumerable<KeyValuePair<string, string>> columnValues)
        {
            commandStringBuilder
                .Append("UPDATE ")
                .Append(tableName)
                .Append(" SET ")
                .AppendJoin(columnValues, (sb, v) => sb.Append(v.Key).Append(" = ").Append(v.Value), ", ");
        }

        protected virtual void AppendValues([NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<string> valueParameterNames)
        {
            commandStringBuilder
                .Append("VALUES (")
                .AppendJoin(valueParameterNames, ", ")
                .Append(")");

            Debug.Assert(!commandStringBuilder.ToString().EndsWith("()"), "empty valueParameterNames");
        }

        protected virtual void AppendWhereClause(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] IEnumerable<KeyValuePair<string, string>> whereConditions)
        {
            commandStringBuilder
                .Append("WHERE ")
                .AppendJoin(whereConditions, (sb, v) => sb.Append(v.Key).Append(" = ").Append(v.Value), " AND ");
        }
    }
}
