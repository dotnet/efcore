// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.Sqlite.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class SqliteUpdateSqlGenerator : UpdateSqlGenerator
    {
        // TODO throw a logger warning that this call was improperly made. The SQLite provider should never specify a schema
        public override string DelimitIdentifier(string name, string schema) => base.DelimitIdentifier(name);

        protected override void AppendIdentityWhereCondition(StringBuilder builder, ColumnModification columnModification)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(columnModification, nameof(columnModification));

            builder
                .Append(DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("last_insert_rowid()");
        }

        protected override void AppendSelectAffectedCountCommand(StringBuilder builder, string name, string schema)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotEmpty(name, nameof(name));

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

        public override string GenerateLiteral(DateTime literal) => "'" + literal.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF") + "'";
        public override string GenerateLiteral(DateTimeOffset literal) => "'" + literal.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz") + "'";

        public override string GenerateNextSequenceValueOperation(string name, string schema)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }
    }
}
