// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public class SqlServerUpdateSqlGenerator : UpdateSqlGenerator, ISqlServerUpdateSqlGenerator
    {
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
                commandStringBuilder.Append(BatchCommandSeparator).AppendLine();

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
            commandStringBuilder.Append(BatchCommandSeparator).AppendLine();

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
                .AppendJoin(operations.Select(c => "INSERTED." + DelimitIdentifier(c.ColumnName)));

        protected override void AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SELECT @@ROWCOUNT")
                .Append(BatchCommandSeparator).AppendLine();

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SET NOCOUNT OFF")
                .Append(BatchCommandSeparator).AppendLine();

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append(DelimitIdentifier(Check.NotNull(columnModification, nameof(columnModification)).ColumnName))
                .Append(" = ")
                .Append("scope_identity()");

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("@@ROWCOUNT = " + expectedRowsAffected);

        public override string BatchSeparator => "GO" + Environment.NewLine + Environment.NewLine;

        public override string DelimitIdentifier(string identifier)
            => "[" + EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier))) + "]";

        protected override string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("]", "]]");

        public override string GenerateLiteral(byte[] literal)
        {
            Check.NotNull(literal, nameof(literal));

            var builder = new StringBuilder();

            builder.Append("0x");

            var parts = literal.Select(b => b.ToString("X2", CultureInfo.InvariantCulture));
            foreach (var part in parts)
            {
                builder.Append(part);
            }

            return builder.ToString();
        }

        public override string GenerateLiteral(bool literal) => literal ? "1" : "0";
        public override string GenerateLiteral(DateTime literal) => "'" + literal.ToString(@"yyyy-MM-dd HH\:mm\:ss.fffffff") + "'";
        public override string GenerateLiteral(DateTimeOffset literal) => "'" + literal.ToString(@"yyyy-MM-dd HH\:mm\:ss.fffffffzzz") + "'";
        public virtual string GenerateLiteral(Guid literal) => "'" + literal + "'";

        public enum ResultsGrouping
        {
            OneResultSet,
            OneCommandPerResultSet
        }
    }
}
