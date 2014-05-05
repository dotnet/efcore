// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerSqlGenerator : SqlGenerator
    {
        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");

            commandStringBuilder.Append("SET NOCOUNT OFF");
        }

        public override void AppendInsertOperation(StringBuilder commandStringBuilder, Table table,
            KeyValuePair<Column, string>[] columnsToParameters)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(table, "table");
            Check.NotNull(columnsToParameters, "columnsToParameters");

            var dbGeneratedNonIdentityKeys =
                table.PrimaryKey.Columns.Where(c => c.ValueGenerationStrategy == StoreValueGenerationStrategy.Computed);

            if (dbGeneratedNonIdentityKeys.Any())
            {
                var tableVariableName = CreateTableVariableName(table.Name);

                // TODO: do not create the variable multiple times (multiple inserts to the same table in a batch)
                commandStringBuilder
                    .Append("DECLARE ")
                    .Append(tableVariableName)
                    .Append(" TABLE(")
                    .AppendJoin(table.PrimaryKey.Columns, (sb, k) => sb.Append(k.Name).Append(" ").Append(k.DataType), ", ")
                    .Append(")")
                    .Append(BatchCommandSeparator)
                    .AppendLine();

                AppendInsertCommandHeader(commandStringBuilder, table, columnsToParameters.Select(c => c.Key));
                commandStringBuilder
                    .AppendLine()
                    .Append("OUTPUT ")
                    .AppendJoin(table.PrimaryKey.Columns, (sb, k) => sb.Append("inserted.").Append(k.Name), ", ")
                    .Append(" INTO ")
                    .Append(tableVariableName)
                    .AppendLine();
                AppendValues(commandStringBuilder, columnsToParameters.Select(c => c.Value));
                commandStringBuilder
                    .Append(BatchCommandSeparator)
                    .AppendLine();

                commandStringBuilder
                    .Append("SELECT ")
                    .AppendJoin(table.GetStoreGeneratedColumns(), (sb, c) => sb.Append("t.").Append(c.Name), ", ");

                commandStringBuilder
                    .Append(" FROM ")
                    .Append(tableVariableName)
                    .Append(" AS g JOIN ")
                    .Append(table.Name)
                    .Append(" AS t ON ")
                    .AppendJoin(
                        table.PrimaryKey.Columns,
                        (sb, c) => sb.Append("g.").Append(c.Name).Append(" = ").Append("t.").Append(c.Name), " AND ");
            }
            else
            {
                base.AppendInsertOperation(commandStringBuilder, table, columnsToParameters);
            }
        }

        public override IEnumerable<KeyValuePair<Column, string>> CreateWhereConditionsForStoreGeneratedKeys(Column[] storeGeneratedKeyColumns)
        {
            Check.NotNull(storeGeneratedKeyColumns, "storeGeneratedKeyColumns");

            Contract.Assert(
                storeGeneratedKeyColumns.All(k => k.ValueGenerationStrategy == StoreValueGenerationStrategy.Identity),
                "Non-identity store generated keys should be handled elsewhere");

            return storeGeneratedKeyColumns.Where(k => k.ValueGenerationStrategy == StoreValueGenerationStrategy.Identity)
                .Select(k => new KeyValuePair<Column, string>(k, "scope_identity()"));
        }

        private static string CreateTableVariableName(string tableName)
        {
            return string.Format("@generated_keys_{0}", tableName);
        }
    }
}
