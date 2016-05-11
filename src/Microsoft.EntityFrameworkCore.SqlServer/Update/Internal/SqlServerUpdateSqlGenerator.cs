// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public class SqlServerUpdateSqlGenerator : UpdateSqlGenerator, ISqlServerUpdateSqlGenerator
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public SqlServerUpdateSqlGenerator([NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(sqlGenerationHelper)
        {
            _typeMapper = typeMapper;
        }

        public virtual ResultSetMapping AppendBulkInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(modificationCommands, nameof(modificationCommands));

            if ((modificationCommands.Count == 1)
                && modificationCommands[0].ColumnModifications.All(o =>
                    !o.IsKey
                    || !o.IsRead
                    || (o.Property.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn)))
            {
                return AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition);
            }

            var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
            var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();
            var defaultValuesOnly = writeOperations.Count == 0;
            var nonIdentityOperations = defaultValuesOnly
                ? modificationCommands.First().ColumnModifications
                    .Where(o => o.Property.SqlServer().ValueGenerationStrategy != SqlServerValueGenerationStrategy.IdentityColumn)
                    .ToList()
                : new List<ColumnModification>();

            if (defaultValuesOnly)
            {
                if ((nonIdentityOperations.Count == 0)
                    || (readOperations.Count == 0))
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperation(commandStringBuilder, modification, commandPosition);
                    }

                    return readOperations.Count == 0
                        ? ResultSetMapping.NoResultSet
                        : ResultSetMapping.LastInResultSet;
                }

                if (nonIdentityOperations.Count > 1)
                {
                    nonIdentityOperations = new List<ColumnModification> { nonIdentityOperations.First() };
                }
            }

            if (readOperations.Count == 0)
            {
                return AppendBulkInsertWithoutServerValues(commandStringBuilder, modificationCommands, writeOperations);
            }

            if (defaultValuesOnly)
            {
                return AppendBulkInsertWithServerValuesOnly(commandStringBuilder, modificationCommands, commandPosition, nonIdentityOperations, readOperations);
            }

            return AppendBulkInsertWithServerValues(commandStringBuilder, modificationCommands, commandPosition, writeOperations, readOperations);
        }

        private ResultSetMapping AppendBulkInsertWithoutServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            List<ColumnModification> writeOperations)
        {
            Debug.Assert(writeOperations.Count > 0);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(commandStringBuilder, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            return ResultSetMapping.NoResultSet;
        }

        private const string InsertedTableBaseName = "@inserted";
        private const string ToInsertTableBaseName = "@toInsert";
        private const string ToInsertTableAlias = "i";
        private const string PositionColumnName = "_Position";
        private const string PositionColumnDeclaration = "[" + PositionColumnName + "] [int]";
        private const string FullPositionColumnName = ToInsertTableAlias + "." + PositionColumnName;

        private ResultSetMapping AppendBulkInsertWithServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> writeOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(
                commandStringBuilder,
                ToInsertTableBaseName,
                commandPosition,
                writeOperations,
                PositionColumnDeclaration);

            commandStringBuilder.Append("INSERT INTO ").Append(ToInsertTableBaseName).Append(commandPosition);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations, "0");
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(
                    commandStringBuilder,
                    modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList(),
                    i.ToString());
            }
            commandStringBuilder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            AppendDeclareTable(
                commandStringBuilder,
                InsertedTableBaseName,
                commandPosition,
                modificationCommands[0].ColumnModifications,
                PositionColumnDeclaration);

            AppendMergeCommandHeader(
                commandStringBuilder,
                modificationCommands[0].TableName,
                modificationCommands[0].Schema,
                ToInsertTableBaseName,
                commandPosition,
                ToInsertTableAlias,
                writeOperations);
            AppendOutputClause(
                commandStringBuilder,
                modificationCommands[0].ColumnModifications,
                InsertedTableBaseName,
                commandPosition,
                FullPositionColumnName);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(commandStringBuilder, readOperations, InsertedTableBaseName, commandPosition, PositionColumnName);

            return ResultSetMapping.NotLastInResultSet;
        }

        private ResultSetMapping AppendBulkInsertWithServerValuesOnly(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> nonIdentityOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, readOperations);

            AppendInsertCommandHeader(commandStringBuilder, modificationCommands[0].TableName, modificationCommands[0].Schema, nonIdentityOperations);
            AppendOutputClause(commandStringBuilder, readOperations, InsertedTableBaseName, commandPosition);
            AppendValuesHeader(commandStringBuilder, nonIdentityOperations);
            AppendValues(commandStringBuilder, nonIdentityOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(commandStringBuilder, nonIdentityOperations);
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(commandStringBuilder, readOperations, InsertedTableBaseName, commandPosition);

            return ResultSetMapping.NotLastInResultSet;
        }

        public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            if (readOperations.Length > 0)
            {
                AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, readOperations);
            }
            AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);
            if (readOperations.Length > 0)
            {
                AppendOutputClause(commandStringBuilder, readOperations, InsertedTableBaseName, commandPosition);
            }
            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            if (readOperations.Length > 0)
            {
                return AppendSelectCommand(commandStringBuilder, readOperations, InsertedTableBaseName, commandPosition);
            }
            commandStringBuilder.AppendLine();

            return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
        }

        private void AppendMergeCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string toInsertTableName,
            int toInsertTableIndex,
            [NotNull] string toInsertTableAlias,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.Append("MERGE ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);

            commandStringBuilder
                .Append(" USING ")
                .Append(toInsertTableName)
                .Append(toInsertTableIndex)
                .Append(" AS ").Append(toInsertTableAlias).AppendLine(" ON 1=0")
                .AppendLine("WHEN NOT MATCHED THEN");

            commandStringBuilder
                .Append("INSERT ")
                .Append("(")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) => { helper.DelimitIdentifier(sb, o.ColumnName); })
                .Append(")");

            AppendValuesHeader(commandStringBuilder, operations);
            commandStringBuilder
                .Append("(")
                .AppendJoin(
                    operations,
                    toInsertTableAlias,
                    SqlGenerationHelper,
                    (sb, o, alias, helper) =>
                        {
                            sb.Append(alias).Append(".");
                            helper.DelimitIdentifier(sb, o.ColumnName);
                        })
                .Append(")");
        }

        private void AppendValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations,
            string additionalLiteral)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append("(")
                    .AppendJoin(
                        operations,
                        SqlGenerationHelper,
                        (sb, o, helper) =>
                            {
                                if (o.IsWrite)
                                {
                                    helper.GenerateParameterName(sb, o.ParameterName);
                                }
                                else
                                {
                                    sb.Append("DEFAULT");
                                }
                            })
                    .Append(", ")
                    .Append(additionalLiteral)
                    .Append(")");
            }
        }

        private void AppendDeclareTable(
            StringBuilder commandStringBuilder,
            string name,
            int index,
            IReadOnlyList<ColumnModification> readOperations,
            string additionalColumns = null)
        {
            commandStringBuilder
                .Append("DECLARE ")
                .Append(name)
                .Append(index)
                .Append(" TABLE (")
                .AppendJoin(
                    readOperations,
                    this,
                    (sb, o, generator) =>
                        {
                            generator.SqlGenerationHelper.DelimitIdentifier(sb, o.ColumnName);
                            sb.Append(" ").Append(generator.GetTypeNameForCopy(o.Property));
                        });

            if (additionalColumns != null)
            {
                commandStringBuilder
                    .Append(", ")
                    .Append(additionalColumns);
            }
            commandStringBuilder
                .Append(")")
                .Append(SqlGenerationHelper.StatementTerminator)
                .AppendLine();
        }

        private string GetTypeNameForCopy(IProperty property)
        {
            var typeName = property.SqlServer().ColumnType
                           ?? _typeMapper.GetMapping(property).StoreType;

            return typeName.Equals("rowversion", StringComparison.OrdinalIgnoreCase)
                   || typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase)
                ? (property.IsNullable ? "varbinary(8)" : "binary(8)")
                : typeName;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void AppendOutputClause(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations,
            string tableName,
            int tableIndex,
            string additionalColumns = null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                        {
                            sb.Append("INSERTED.");
                            helper.DelimitIdentifier(sb, o.ColumnName);
                        });

            if (additionalColumns != null)
            {
                commandStringBuilder
                    .Append(", ").Append(additionalColumns);
            }

            commandStringBuilder.AppendLine()
                .Append("INTO ").Append(tableName).Append(tableIndex);
        }

        private ResultSetMapping AppendSelectCommand(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> readOperations,
            string tableName,
            int tableIndex,
            string orderColumn = null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => { helper.DelimitIdentifier(sb, o.ColumnName); })
                .Append(" FROM ").Append(tableName).Append(tableIndex);

            if (orderColumn != null)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("ORDER BY ")
                    .Append(orderColumn);
            }

            commandStringBuilder
                .Append(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SELECT @@ROWCOUNT")
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SET NOCOUNT ON")
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine();

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(columnModification, nameof(columnModification));

            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
            commandStringBuilder.Append(" = ").Append("scope_identity()");
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("@@ROWCOUNT = ")
                .Append(expectedRowsAffected);
    }
}
