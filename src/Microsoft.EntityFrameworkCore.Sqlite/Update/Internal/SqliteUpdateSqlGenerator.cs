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
        public SqliteUpdateSqlGenerator([NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(sqlGenerationHelper)
        {
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(columnModification, nameof(columnModification));

            commandStringBuilder
                .Append(SqlGenerationHelper.DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("last_insert_rowid()");
        }

        protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));

            commandStringBuilder
                .Append("SELECT changes()")
                .Append(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));

            commandStringBuilder.Append("changes() = " + expectedRowsAffected);
        }

        public override string GenerateNextSequenceValueOperation(string name, string schema)
        {
            throw new NotSupportedException(SqliteStrings.SequencesNotSupported);
        }
    }
}
