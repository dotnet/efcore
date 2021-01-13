// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class SqlServerUpdateSqlGenerator : UpdateSqlGenerator, ISqlServerUpdateSqlGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerUpdateSqlGenerator(
            [NotNull] UpdateSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ResultSetMapping AppendBulkInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition)
        {
            if (modificationCommands.Count == 1
                && modificationCommands[0].ColumnModifications.All(
                    o =>
                        !o.IsKey
                        || !o.IsRead
                        || o.Property?.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn))
            {
                return AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition);
            }

            var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
            var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();
            var keyOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsKey).ToList();

            var defaultValuesOnly = writeOperations.Count == 0;
            var nonIdentityOperations = modificationCommands[0].ColumnModifications
                .Where(o => o.Property?.GetValueGenerationStrategy() != SqlServerValueGenerationStrategy.IdentityColumn)
                .ToList();

            if (defaultValuesOnly)
            {
                if (nonIdentityOperations.Count == 0
                    || readOperations.Count == 0)
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
                    nonIdentityOperations.RemoveRange(1, nonIdentityOperations.Count - 1);
                }
            }

            if (readOperations.Count == 0)
            {
                return AppendBulkInsertWithoutServerValues(commandStringBuilder, modificationCommands, writeOperations);
            }

            if (defaultValuesOnly)
            {
                return AppendBulkInsertWithServerValuesOnly(
                    commandStringBuilder, modificationCommands, commandPosition, nonIdentityOperations, keyOperations, readOperations);
            }

            if (modificationCommands[0].Entries.SelectMany(e => e.EntityType.GetAllBaseTypesInclusive())
                .Any(e => e.IsMemoryOptimized()))
            {
                if (!nonIdentityOperations.Any(o => o.IsRead && o.IsKey))
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperation(commandStringBuilder, modification, commandPosition++);
                    }
                }
                else
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperationWithServerKeys(
                            commandStringBuilder, modification, keyOperations, readOperations, commandPosition++);
                    }
                }

                return ResultSetMapping.LastInResultSet;
            }

            return AppendBulkInsertWithServerValues(
                commandStringBuilder, modificationCommands, commandPosition, writeOperations, keyOperations, readOperations);
        }

        private ResultSetMapping AppendBulkInsertWithoutServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            List<ColumnModification> writeOperations)
        {
            Check.DebugAssert(writeOperations.Count > 0, $"writeOperations.Count is {writeOperations.Count}");

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, name, schema, writeOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.AppendLine(",");
                AppendValues(
                    commandStringBuilder, name, schema, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
            }

            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

            return ResultSetMapping.NoResultSet;
        }

        private const string InsertedTableBaseName = "@inserted";
        private const string ToInsertTableAlias = "i";
        private const string PositionColumnName = "_Position";
        private const string PositionColumnDeclaration = "[" + PositionColumnName + "] [int]";
        private const string FullPositionColumnName = ToInsertTableAlias + "." + PositionColumnName;

        private ResultSetMapping AppendBulkInsertWithServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> writeOperations,
            List<ColumnModification> keyOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(
                commandStringBuilder,
                InsertedTableBaseName,
                commandPosition,
                keyOperations,
                PositionColumnDeclaration);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            AppendMergeCommandHeader(
                commandStringBuilder,
                name,
                schema,
                ToInsertTableAlias,
                modificationCommands,
                writeOperations,
                PositionColumnName);
            AppendOutputClause(
                commandStringBuilder,
                keyOperations,
                InsertedTableBaseName,
                commandPosition,
                FullPositionColumnName);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(
                commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema,
                orderColumn: PositionColumnName);

            return ResultSetMapping.NotLastInResultSet;
        }

        private ResultSetMapping AppendBulkInsertWithServerValuesOnly(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> nonIdentityOperations,
            List<ColumnModification> keyOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, keyOperations);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;
            AppendInsertCommandHeader(commandStringBuilder, name, schema, nonIdentityOperations);
            AppendOutputClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
            AppendValuesHeader(commandStringBuilder, nonIdentityOperations);
            AppendValues(commandStringBuilder, name, schema, nonIdentityOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.AppendLine(",");
                AppendValues(commandStringBuilder, name, schema, nonIdentityOperations);
            }

            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema);

            return ResultSetMapping.NotLastInResultSet;
        }

        private void AppendMergeCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string toInsertTableAlias,
            [NotNull] IReadOnlyList<ModificationCommand> modificationCommands,
            [NotNull] IReadOnlyList<ColumnModification> writeOperations,
            string additionalColumns = null)
        {
            commandStringBuilder.Append("MERGE ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);

            commandStringBuilder
                .Append(" USING (");

            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations, "0");
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.AppendLine(",");
                AppendValues(
                    commandStringBuilder,
                    modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList(),
                    i.ToString(CultureInfo.InvariantCulture));
            }

            commandStringBuilder
                .Append(") AS ").Append(toInsertTableAlias)
                .Append(" (")
                .AppendJoin(
                    writeOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName));
            if (additionalColumns != null)
            {
                commandStringBuilder
                    .Append(", ")
                    .Append(additionalColumns);
            }

            commandStringBuilder
                .Append(")")
                .AppendLine(" ON 1=0")
                .AppendLine("WHEN NOT MATCHED THEN");

            commandStringBuilder
                .Append("INSERT ")
                .Append("(")
                .AppendJoin(
                    writeOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName))
                .Append(")");

            AppendValuesHeader(commandStringBuilder, writeOperations);
            commandStringBuilder
                .Append("(")
                .AppendJoin(
                    writeOperations,
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
            IReadOnlyList<ColumnModification> operations,
            string additionalColumns = null)
        {
            commandStringBuilder
                .Append("DECLARE ")
                .Append(name)
                .Append(index)
                .Append(" TABLE (")
                .AppendJoin(
                    operations,
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
                .AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        private string GetTypeNameForCopy(IProperty property)
        {
            var typeName = property.GetColumnType();
            if (typeName == null)
            {
                var principalProperty = property.FindFirstPrincipal();

                typeName = principalProperty?.GetColumnType()
                    ?? Dependencies.TypeMappingSource.FindMapping(property.ClrType)?.StoreType;
            }

            return property.ClrType == typeof(byte[])
                && typeName != null
                && (typeName.Equals("rowversion", StringComparison.OrdinalIgnoreCase)
                    || typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
                    ? property.IsNullable ? "varbinary(8)" : "binary(8)"
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

        private ResultSetMapping AppendInsertOperationWithServerKeys(
            StringBuilder commandStringBuilder,
            ModificationCommand command,
            IReadOnlyList<ColumnModification> keyOperations,
            IReadOnlyList<ColumnModification> readOperations,
            int commandPosition)
        {
            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();

            AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, keyOperations);

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendOutputClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, name, schema, writeOperations);
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            return AppendSelectCommand(
                commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema);
        }

        private ResultSetMapping AppendSelectCommand(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> readOperations,
            IReadOnlyList<ColumnModification> keyOperations,
            string insertedTableName,
            int insertedTableIndex,
            string tableName,
            string schema,
            string orderColumn = null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName, "t"))
                .Append(" FROM ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, tableName, schema);
            commandStringBuilder
                .AppendLine(" t")
                .Append("INNER JOIN ")
                .Append(insertedTableName).Append(insertedTableIndex)
                .Append(" i")
                .Append(" ON ")
                .AppendJoin(
                    keyOperations, (sb, c) =>
                    {
                        sb.Append("(");
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "t");
                        sb.Append(" = ");
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "i");
                        sb.Append(")");
                    }, " AND ");

            if (orderColumn != null)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("ORDER BY ");
                SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, orderColumn, "i");
            }

            commandStringBuilder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ResultSetMapping AppendSelectAffectedCountCommand(
            StringBuilder commandStringBuilder,
            string name,
            string schema,
            int commandPosition)
        {
            commandStringBuilder
                .Append("SELECT @@ROWCOUNT")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            => commandStringBuilder
                .Append("SET NOCOUNT ON")
                .AppendLine(SqlGenerationHelper.StatementTerminator);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
            commandStringBuilder.Append(" = ");

            commandStringBuilder.Append("scope_identity()");
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => commandStringBuilder
                .Append("@@ROWCOUNT = ")
                .Append(expectedRowsAffected.ToString(CultureInfo.InvariantCulture));
    }
}
