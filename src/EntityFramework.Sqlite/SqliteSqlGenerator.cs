// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteSqlGenerator : SqlGenerator
    {
        protected override void AppendIdentityWhereCondition(StringBuilder builder, ColumnModification columnModification)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(columnModification, nameof(columnModification));

            builder
                .Append(DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("last_insert_rowid()");
        }

        public override void AppendSelectAffectedCountCommand(StringBuilder builder, string tableName, string schemaName)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotEmpty(tableName, nameof(tableName));

            builder
                .Append("SELECT changes()")
                .Append(BatchCommandSeparator)
                .AppendLine();
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder builder, int expectedRowsAffected)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Append("changes() = " + expectedRowsAffected);
        }
    }
}
