// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSqlGenerator : SqlGenerator
    {
        public override void AppendInsertOperation(
            StringBuilder commandStringBuilder,
            ModificationCommand command)
        {
            Check.NotNull(command, "command");

            AppendBulkInsertOperation(commandStringBuilder, new[] { command });
        }

        public virtual ResultsGrouping AppendBulkInsertOperation(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ModificationCommand> modificationCommands)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotEmpty(modificationCommands, "modificationCommands");

            var schemaQualifiedName = modificationCommands[0].SchemaQualifiedName;

            // TODO: Support TPH
            var defaultValuesOnly = !modificationCommands.First().ColumnModifications.Any(o => o.IsWrite);
            var statementCount = defaultValuesOnly
                ? modificationCommands.Count
                : 1;
            var valueSetCount = defaultValuesOnly
                ? 1
                : modificationCommands.Count;

            for (int i = 0; i < statementCount; i++)
            {
                var operations = modificationCommands[i].ColumnModifications;
                var writeOperations = operations.Where(o => o.IsWrite).ToArray();
                var readOperations = operations.Where(o => o.IsRead).ToArray();

                AppendInsertCommandHeader(commandStringBuilder, schemaQualifiedName, writeOperations);
                if (readOperations.Length > 0)
                {
                    AppendOutputClause(commandStringBuilder, readOperations);
                }
                AppendValuesHeader(commandStringBuilder, writeOperations);
                AppendValues(commandStringBuilder, writeOperations);
                for (int j = 1; j < valueSetCount; j++)
                {
                    commandStringBuilder.Append(",").AppendLine();
                    AppendValues(commandStringBuilder, modificationCommands[j].ColumnModifications.Where(o => o.IsWrite).ToArray());
                }
                commandStringBuilder.Append(BatchCommandSeparator).AppendLine();

                if (readOperations.Length == 0)
                {
                    AppendSelectAffectedCountCommand(commandStringBuilder, schemaQualifiedName);
                }
            }

            return defaultValuesOnly
                ? ResultsGrouping.OneCommandPerResultSet
                : ResultsGrouping.OneResultSet;
        }

        public override void AppendUpdateOperation(
            StringBuilder commandStringBuilder,
            ModificationCommand command)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(command, "command");

            var schemaQualifiedName = command.SchemaQualifiedName;
            var operations = command.ColumnModifications;

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

        public enum ResultsGrouping
        {
            OneResultSet,
            OneCommandPerResultSet
        }
    }
}
