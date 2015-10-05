// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class SqlServerUpdateSqlGenerator : UpdateSqlGenerator, ISqlServerUpdateSqlGenerator
    {
        public SqlServerUpdateSqlGenerator([NotNull] ISqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override void AppendInsertOperation(
            StringBuilder commandStringBuilder,
            ModificationCommand command)
        {
            Check.NotNull(command, nameof(command));

            AppendBulkInsertOperation(commandStringBuilder, new[] { command });
        }

        public virtual ResultsGrouping AppendBulkInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(modificationCommands, nameof(modificationCommands));

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            // TODO: Support TPH
            var defaultValuesOnly = !modificationCommands.First().ColumnModifications.Any(o => o.IsWrite);
            var statementCount = defaultValuesOnly
                ? modificationCommands.Count
                : 1;
            var valueSetCount = defaultValuesOnly
                ? 1
                : modificationCommands.Count;

            for (var i = 0; i < statementCount; i++)
            {
                var operations = modificationCommands[i].ColumnModifications;
                var writeOperations = operations.Where(o => o.IsWrite).ToArray();
                var readOperations = operations.Where(o => o.IsRead).ToArray();

                AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
                if (readOperations.Length > 0)
                {
                    AppendOutputClause(commandStringBuilder, readOperations);
                }
                AppendValuesHeader(commandStringBuilder, writeOperations);
                AppendValues(commandStringBuilder, writeOperations);
                for (var j = 1; j < valueSetCount; j++)
                {
                    commandStringBuilder.Append(",").AppendLine();
                    AppendValues(commandStringBuilder, modificationCommands[j].ColumnModifications.Where(o => o.IsWrite).ToArray());
                }
                commandStringBuilder.Append(SqlGenerator.BatchCommandSeparator).AppendLine();

                if (readOperations.Length == 0)
                {
                    AppendSelectAffectedCountCommand(commandStringBuilder, name, schema);
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
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);
            if (readOperations.Length > 0)
            {
                AppendOutputClause(commandStringBuilder, readOperations);
            }
            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.Append(SqlGenerator.BatchCommandSeparator).AppendLine();

            if (readOperations.Length == 0)
            {
                AppendSelectAffectedCountCommand(commandStringBuilder, name, schema);
            }
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void AppendOutputClause(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations)
            => commandStringBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(operations.Select(c => "INSERTED." + SqlGenerator.DelimitIdentifier(c.ColumnName)));

        protected override void AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SELECT @@ROWCOUNT")
                .Append(SqlGenerator.BatchCommandSeparator).AppendLine();

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SET NOCOUNT OFF")
                .Append(SqlGenerator.BatchCommandSeparator).AppendLine();

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append(SqlGenerator.DelimitIdentifier(Check.NotNull(columnModification, nameof(columnModification)).ColumnName))
                .Append(" = ")
                .Append("scope_identity()");

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("@@ROWCOUNT = " + expectedRowsAffected);

        public enum ResultsGrouping
        {
            OneResultSet,
            OneCommandPerResultSet
        }
    }
}
