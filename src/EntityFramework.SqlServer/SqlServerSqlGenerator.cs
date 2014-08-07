// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSqlGenerator : SqlGenerator
    {
        public override void AppendInsertOperation(
            StringBuilder commandStringBuilder,
            SchemaQualifiedName schemaQualifiedName,
            IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(operations, "operations");

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendInsertCommandHeader(commandStringBuilder, schemaQualifiedName, writeOperations);
            if (readOperations.Length > 0)
            {
                AppendOutputClause(commandStringBuilder, readOperations);
            }
            AppendValues(commandStringBuilder, writeOperations);
            commandStringBuilder.Append(BatchCommandSeparator).AppendLine();

            if (readOperations.Length == 0)
            {
                AppendSelectAffectedCountCommand(commandStringBuilder, schemaQualifiedName);
            }
        }

        public override void AppendUpdateOperation(
            StringBuilder commandStringBuilder,
            SchemaQualifiedName schemaQualifiedName,
            IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(operations, "operations");

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendUpdateCommandHeader(commandStringBuilder, schemaQualifiedName, writeOperations);
            if (readOperations.Length > 0)
            {
                AppendOutputClause(commandStringBuilder, readOperations);
            }
            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.Append(BatchCommandSeparator).AppendLine();

            if (readOperations.Length == 0)
            {
                AppendSelectAffectedCountCommand(commandStringBuilder, schemaQualifiedName);
            }
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void AppendOutputClause(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations)
        {
            commandStringBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(operations.Select(c => "INSERTED." + DelimitIdentifier(c.ColumnName)));
        }

        public override void AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, SchemaQualifiedName schemaQualifiedName)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");

            commandStringBuilder
                .Append("SELECT @@ROWCOUNT")
                .Append(BatchCommandSeparator).AppendLine();
        }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");

            commandStringBuilder
                .Append("SET NOCOUNT OFF")
                .Append(BatchCommandSeparator).AppendLine();
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            commandStringBuilder
                .Append(DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("scope_identity()");
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");

            commandStringBuilder
                .Append("@@ROWCOUNT = " + expectedRowsAffected);
        }

        public override string DelimitIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return "[" + EscapeIdentifier(identifier) + "]";
        }

        public override string EscapeIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return identifier.Replace("]", "]]");
        }
    }
}
