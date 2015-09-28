// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class SqliteUpdateSqlGenerator : UpdateSqlGenerator
    {
        public SqliteUpdateSqlGenerator([NotNull] ISqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        protected override void AppendIdentityWhereCondition(StringBuilder builder, ColumnModification columnModification)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(columnModification, nameof(columnModification));

            builder
                .Append(SqlGenerator.DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("last_insert_rowid()");
        }

        protected override void AppendSelectAffectedCountCommand(StringBuilder builder, string name, string schema)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotEmpty(name, nameof(name));

            builder
                .Append("SELECT changes()")
                .Append(SqlGenerator.BatchCommandSeparator)
                .AppendLine();
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder builder, int expectedRowsAffected)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Append("changes() = " + expectedRowsAffected);
        }

        public override string GenerateNextSequenceValueOperation(string name, string schema)
        {
            throw new NotSupportedException(SqliteStrings.SequencesNotSupported);
        }
    }
}
