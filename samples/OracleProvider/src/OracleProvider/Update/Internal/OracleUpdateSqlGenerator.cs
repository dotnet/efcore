// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public class OracleUpdateSqlGenerator : UpdateSqlGenerator
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public OracleUpdateSqlGenerator(
            [NotNull] UpdateSqlGeneratorDependencies dependencies,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(dependencies)
        {
            _typeMapper = typeMapper;
        }

        public override void AppendNextSequenceValueOperation(StringBuilder commandStringBuilder, string name, string schema)
        {
            commandStringBuilder.Append("SELECT ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, Check.NotNull(name, nameof(name)), schema);
            commandStringBuilder.Append(".NEXTVAL FROM DUAL");
        }

        public override ResultSetMapping AppendInsertOperation(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            var resultSetMapping = ResultSetMapping.NoResultSet;
            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            if (readOperations.Count > 0)
            {
                commandStringBuilder
                    .AppendLine("DECLARE")
                    .AppendJoin(
                        readOperations,
                        (sb, cm) =>
                            sb.Append(GetVariableName(cm))
                                .Append(" ")
                                .Append(GetVariableType(cm))
                                .Append(";"),
                        Environment.NewLine)
                    .AppendLine()
                    .AppendLine("BEGIN");
            }

            AppendInsertCommand(commandStringBuilder, name, schema, writeOperations, readOperations);

            if (readOperations.Count > 0)
            {
                commandStringBuilder
                    .AppendLine("OPEN :cur FOR")
                    .Append("SELECT ")
                    .AppendJoin(
                        readOperations,
                        (sb, o) => sb.Append(GetVariableName(o)))
                    .AppendLine(" FROM DUAL;")
                    .Append("END;");

                resultSetMapping = ResultSetMapping.LastInResultSet;
            }

            return resultSetMapping;
        }

        private void AppendInsertCommand(
            StringBuilder commandStringBuilder,
            string name,
            string schema,
            IReadOnlyList<ColumnModification> writeOperations,
            IReadOnlyCollection<ColumnModification> readOperations)
        {
            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations.Count > 0 ? writeOperations : readOperations.ToArray());

            if (readOperations.Count > 0)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("RETURN ")
                    .AppendJoin(
                        readOperations,
                        (sb, cm) => sb.Append(SqlGenerationHelper.DelimitIdentifier(cm.ColumnName)))
                    .Append(" INTO ")
                    .AppendJoin(
                        readOperations,
                        (sb, cm) => sb.Append(GetVariableName(cm)));
            }

            commandStringBuilder
                .AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        protected override void AppendValuesHeader(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.AppendLine();
            commandStringBuilder.Append("VALUES ");
        }

        public override ResultSetMapping AppendUpdateOperation(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var conditionOperations = operations.Where(o => o.IsCondition).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            commandStringBuilder
                .AppendLine("DECLARE")
                .AppendLine("v_RowCount INTEGER;");

            if (readOperations.Count > 0)
            {
                commandStringBuilder
                    .AppendJoin(
                        readOperations,
                        (sb, cm) =>
                            sb.Append(GetVariableName(cm))
                                .Append(" ")
                                .Append(GetVariableType(cm))
                                .Append(";"),
                        Environment.NewLine)
                    .AppendLine();
            }

            commandStringBuilder
                .AppendLine("BEGIN");

            AppendUpdateCommand(commandStringBuilder, name, schema, writeOperations, conditionOperations, readOperations);

            ResultSetMapping resultSetMapping;

            if (readOperations.Count > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToList();

                resultSetMapping
                    = AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
            }
            else
            {
                resultSetMapping
                    = AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
            }

            commandStringBuilder.Append("END;");

            return resultSetMapping;
        }

        private void AppendUpdateCommand(
            StringBuilder commandStringBuilder,
            string name,
            string schema,
            IReadOnlyList<ColumnModification> writeOperations,
            IReadOnlyList<ColumnModification> conditionOperations,
            IReadOnlyCollection<ColumnModification> readOperations)
        {
            AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendWhereClause(commandStringBuilder, conditionOperations);

            if (readOperations.Count > 0)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("RETURN ")
                    .AppendJoin(
                        readOperations,
                        (sb, cm) => sb.Append(SqlGenerationHelper.DelimitIdentifier(cm.ColumnName)))
                    .Append(" INTO ")
                    .AppendJoin(
                        readOperations,
                        (sb, cm) => sb.Append(GetVariableName(cm)));
            }

            commandStringBuilder
                .AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        public override ResultSetMapping AppendDeleteOperation(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            commandStringBuilder
                .AppendLine("DECLARE")
                .AppendLine("v_RowCount INTEGER;")
                .AppendLine("BEGIN");

            var resultSetMapping = base.AppendDeleteOperation(commandStringBuilder, command, commandPosition);

            commandStringBuilder.Append("END;");

            return resultSetMapping;
        }

        private static string GetVariableName(ColumnModification columnModification)
        {
            return $"v_{columnModification.ColumnName}";
        }

        private string GetVariableType(ColumnModification columnModification)
        {
            return _typeMapper.FindMapping(columnModification.Property).StoreType;
        }

        protected override void AppendIdentityWhereCondition(
            StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);

            commandStringBuilder
                .Append(" = ")
                .Append(GetVariableName(columnModification));
        }

        protected override ResultSetMapping AppendSelectAffectedCommand(
            StringBuilder commandStringBuilder,
            string name,
            string schema,
            IReadOnlyList<ColumnModification> readOperations,
            IReadOnlyList<ColumnModification> conditionOperations,
            int commandPosition)
        {
            commandStringBuilder
                .AppendLine("v_RowCount := SQL%ROWCOUNT;")
                .AppendLine("OPEN :cur FOR")
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    (sb, o) => sb.Append(GetVariableName(o)))
                .AppendLine()
                .AppendLine("FROM DUAL")
                .Append("WHERE ");

            AppendRowsAffectedWhereCondition(commandStringBuilder, 1);

            commandStringBuilder.AppendLine(";");

            return ResultSetMapping.LastInResultSet;
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => commandStringBuilder
                .Append("v_RowCount = ")
                .Append(expectedRowsAffected.ToString(CultureInfo.InvariantCulture));

        protected override ResultSetMapping AppendSelectAffectedCountCommand(
            StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            commandStringBuilder
                .AppendLine("v_RowCount := SQL%ROWCOUNT;")
                .AppendLine("OPEN :cur FOR SELECT v_RowCount FROM DUAL;");

            return ResultSetMapping.LastInResultSet;
        }
    }
}
