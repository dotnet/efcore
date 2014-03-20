// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    internal class SqlServerSqlGenerator : SqlGenerator
    {
        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");

            commandStringBuilder.Append("SET NOCOUNT OFF");
        }

        public override void AppendInsertOperation(StringBuilder commandStringBuilder, string tableName, KeyValuePair<string, string>[] keyColumns, 
            KeyValuePair<string, string>[] columnsToParameters, KeyValuePair<string, ValueGenerationStrategy>[] storeGeneratedColumns)
        {
            var dbGeneratedNonIdentityKeys = 
                storeGeneratedColumns.Where(
                    c => keyColumns.Select(k => k.Key).Contains(c.Key) && c.Value == ValueGenerationStrategy.StoreComputed);

            if (dbGeneratedNonIdentityKeys.Any())
            {
                var tableVariableName = CreateTableVariableName(tableName);
                var keyColumnNames = keyColumns.Select(c => c.Key).ToArray();

                // TODO: do not create the variable multiple times (multiple inserts to the same table in a batch)
                commandStringBuilder
                    .Append("DECLARE ")
                    .Append(tableVariableName)
                    .Append(" table(")
                    .AppendJoin(keyColumns, (sb, k) => sb.Append(k.Key).Append(" ").Append(k.Value), ", ")
                    .Append(")")
                    .Append(BatchCommandSeparator)
                    .AppendLine();

                AppendInsertCommandHeader(commandStringBuilder, tableName, columnsToParameters.Select(c => c.Key));
                commandStringBuilder
                    .AppendLine()
                    .Append("OUTPUT ")
                    .AppendJoin(keyColumnNames, (sb, k) => sb.Append("inserted.").Append(k), ", ")
                    .Append(" INTO ")
                    .Append(tableVariableName)
                    .AppendLine();
                AppendValues(commandStringBuilder, columnsToParameters.Select(c => c.Value));
                commandStringBuilder
                    .Append(BatchCommandSeparator)
                    .AppendLine();

                commandStringBuilder
                    .Append("SELECT ")
                    .AppendJoin(storeGeneratedColumns.Select(c=>c.Key), (sb, c) => sb.Append("t.").Append(c), ", ");

                commandStringBuilder
                    .Append(" FROM ")
                    .Append(tableVariableName)
                    .Append(" AS g JOIN ")
                    .Append(tableName)
                    .Append(" AS t ON ")
                    .AppendJoin(
                        keyColumnNames,
                        (sb, c) => sb.Append("g.").Append(c).Append(" = ").Append("t.").Append(c),
                        " AND ");
            }
            else
            {
                base.AppendInsertOperation(commandStringBuilder, tableName, keyColumns, columnsToParameters, storeGeneratedColumns);
            }
        }

        public override void AppendModificationOperationSelectWhereClause(StringBuilder commandStringBuilder,
            IEnumerable<KeyValuePair<string, string>> knownKeyValues,
            IEnumerable<KeyValuePair<string, ValueGenerationStrategy>> generatedKeys)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(knownKeyValues, "knownKeyValues");
            Check.NotNull(generatedKeys, "generatedKeys");

            AppendWhereClause(
                commandStringBuilder,
                knownKeyValues.Concat(
                    generatedKeys.Where(k => k.Value == ValueGenerationStrategy.StoreIdentity)
                        .Select(k => new KeyValuePair<string, string>(k.Key, "scope_identity()"))));
        }

        private static string CreateTableVariableName(string tableName)
        {
            return string.Format("@generated_keys_{0}", tableName);
        }

    }
}
