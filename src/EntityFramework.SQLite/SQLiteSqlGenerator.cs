// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteSqlGenerator : SqlGenerator
    {
        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            commandStringBuilder
                .Append(DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("last_insert_rowid()");
        }

        public override void AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, SchemaQualifiedName schemaQualifiedName)
        {
            commandStringBuilder
                .Append("SELECT changes()")
                .Append(BatchCommandSeparator).AppendLine();
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");

            commandStringBuilder
                .Append("changes() = " + expectedRowsAffected);
        }
    }
}
