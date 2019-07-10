// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A base class for the <see cref="IUpdateSqlGenerator" /> service that is typically inherited from
    ///         by database providers.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public abstract class UpdateSqlGenerator : IUpdateSqlGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected UpdateSqlGenerator([NotNull] UpdateSqlGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual UpdateSqlGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Helpers for generating update SQL.
        /// </summary>
        protected virtual ISqlGenerationHelper SqlGenerationHelper => Dependencies.SqlGenerationHelper;

        /// <summary>
        ///     Appends a SQL command for inserting a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="command"> The command that represents the delete operation. </param>
        /// <param name="commandPosition"> The ordinal of this command in the batch. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for the command. </returns>
        public virtual ResultSetMapping AppendInsertOperation(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            AppendInsertCommand(commandStringBuilder, name, schema, writeOperations);

            if (readOperations.Count > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToList();

                return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
            }

            return ResultSetMapping.NoResultSet;
        }

        /// <summary>
        ///     Appends a SQL command for updating a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="command"> The command that represents the delete operation. </param>
        /// <param name="commandPosition"> The ordinal of this command in the batch. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for the command. </returns>
        public virtual ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var conditionOperations = operations.Where(o => o.IsCondition).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            AppendUpdateCommand(commandStringBuilder, name, schema, writeOperations, conditionOperations);

            if (readOperations.Count > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToList();

                return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
            }

            return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
        }

        /// <summary>
        ///     Appends a SQL command for deleting a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="command"> The command that represents the delete operation. </param>
        /// <param name="commandPosition"> The ordinal of this command in the batch. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for the command. </returns>
        public virtual ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var conditionOperations = command.ColumnModifications.Where(o => o.IsCondition).ToList();

            AppendDeleteCommand(commandStringBuilder, name, schema, conditionOperations);

            return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
        }

        /// <summary>
        ///     Appends a SQL command for inserting a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        /// <param name="writeOperations"> The operations for each column. </param>
        protected virtual void AppendInsertCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> writeOperations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(writeOperations, nameof(writeOperations));

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        /// <summary>
        ///     Appends a SQL command for updating a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        /// <param name="writeOperations"> The operations for each column. </param>
        /// <param name="conditionOperations"> The operations used to generate the <c>WHERE</c> clause for the update. </param>
        protected virtual void AppendUpdateCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> writeOperations,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(writeOperations, nameof(writeOperations));
            Check.NotNull(conditionOperations, nameof(conditionOperations));

            AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        /// <summary>
        ///     Appends a SQL command for deleting a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        /// <param name="conditionOperations"> The operations used to generate the <c>WHERE</c> clause for the delete. </param>
        protected virtual void AppendDeleteCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(conditionOperations, nameof(conditionOperations));

            AppendDeleteCommandHeader(commandStringBuilder, name, schema);
            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        /// <summary>
        ///     Appends a SQL command for selecting the number of rows affected.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        /// <param name="commandPosition"> The ordinal of the command for which rows affected it being returned. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for this command.</returns>
        protected virtual ResultSetMapping AppendSelectAffectedCountCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            int commandPosition)
            => ResultSetMapping.NoResultSet;

        /// <summary>
        ///     Appends a SQL command for selecting affected data.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        /// <param name="readOperations"> The operations representing the data to be read. </param>
        /// <param name="conditionOperations"> The operations used to generate the <c>WHERE</c> clause for the select. </param>
        /// <param name="commandPosition"> The ordinal of the command for which rows affected it being returned. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for this command.</returns>
        protected virtual ResultSetMapping AppendSelectAffectedCommand(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> readOperations,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations,
            int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(readOperations, nameof(readOperations));
            Check.NotNull(conditionOperations, nameof(conditionOperations));

            AppendSelectCommandHeader(commandStringBuilder, readOperations);
            AppendFromClause(commandStringBuilder, name, schema);
            // TODO: there is no notion of operator - currently all the where conditions check equality
            AppendWhereAffectedClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        /// <summary>
        ///     Appends a SQL fragment for starting an <c>INSERT</c>.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        /// <param name="operations"> The operations representing the data to be inserted. </param>
        protected virtual void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.Append("INSERT INTO ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append(" (")
                    .AppendJoin(
                        operations,
                        SqlGenerationHelper,
                        (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName))
                    .Append(")");
            }
        }

        /// <summary>
        ///     Appends a SQL fragment for starting an <c>DELETE</c>.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        protected virtual void AppendDeleteCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));

            commandStringBuilder.Append("DELETE FROM ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
        }

        /// <summary>
        ///     Appends a SQL fragment for starting an <c>UPDATE</c>.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        /// <param name="operations"> The operations representing the data to be updated. </param>
        protected virtual void AppendUpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.Append("UPDATE ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
            commandStringBuilder.Append(" SET ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                    {
                        helper.DelimitIdentifier(sb, o.ColumnName);
                        sb.Append(" = ");
                        if (!o.UseCurrentValueParameter)
                        {
                            AppendSqlLiteral(sb, o.Value, o.Property);
                        }
                        else
                        {
                            helper.GenerateParameterNamePlaceholder(sb, o.ParameterName);
                        }
                    });
        }

        /// <summary>
        ///     Appends a SQL fragment for starting an <c>SELECT</c>.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="operations"> The operations representing the data to be read. </param>
        protected virtual void AppendSelectCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .Append("SELECT ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName));
        }

        /// <summary>
        ///     Appends a SQL fragment for starting an <c>FROM</c> clause.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The table schema, or <c>null</c> to use the default schema. </param>
        protected virtual void AppendFromClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));

            commandStringBuilder
                .AppendLine()
                .Append("FROM ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
        }

        /// <summary>
        ///     Appends a SQL fragment for a <c>VALUES</c>.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="operations"> The operations for which there are values. </param>
        protected virtual void AppendValuesHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.AppendLine();
            commandStringBuilder.Append(operations.Count > 0 ? "VALUES " : "DEFAULT VALUES");
        }

        /// <summary>
        ///     Appends values after a <see cref="AppendValuesHeader" /> call.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="operations"> The operations for which there are values. </param>
        protected virtual void AppendValues(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
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
                                if (!o.UseCurrentValueParameter)
                                {
                                    AppendSqlLiteral(sb, o.Value, o.Property);
                                }
                                else
                                {
                                    helper.GenerateParameterNamePlaceholder(sb, o.ParameterName);
                                }
                            }
                            else
                            {
                                sb.Append("DEFAULT");
                            }
                        })
                    .Append(")");
            }
        }

        /// <summary>
        ///     Appends a <c>WHERE</c> clause.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="operations"> The operations from which to build the conditions. </param>
        protected virtual void AppendWhereClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("WHERE ")
                    .AppendJoin(operations, (sb, v) => AppendWhereCondition(sb, v, v.UseOriginalValueParameter), " AND ");
            }
        }

        /// <summary>
        ///     Appends a <c>WHERE</c> clause involving rows affected.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="operations"> The operations from which to build the conditions. </param>
        protected virtual void AppendWhereAffectedClause(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .AppendLine()
                .Append("WHERE ");

            AppendRowsAffectedWhereCondition(commandStringBuilder, 1);

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append(" AND ")
                    .AppendJoin(
                        operations, (sb, v) =>
                        {
                            if (v.IsKey)
                            {
                                if (v.IsRead)
                                {
                                    AppendIdentityWhereCondition(sb, v);
                                }
                                else
                                {
                                    AppendWhereCondition(sb, v, v.UseOriginalValueParameter);
                                }
                            }
                        }, " AND ");
            }
        }

        /// <summary>
        ///     Appends a <c>WHERE</c> condition checking rows affected.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="expectedRowsAffected"> The expected number of rows affected. </param>
        protected abstract void AppendRowsAffectedWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            int expectedRowsAffected);

        /// <summary>
        ///     Appends a <c>WHERE</c> condition for the given column.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="columnModification"> The column for which the condition is being generated. </param>
        /// <param name="useOriginalValue">
        ///     If <c>true</c>, then the original value will be used in the condition, otherwise the current value will be used.
        /// </param>
        protected virtual void AppendWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ColumnModification columnModification,
            bool useOriginalValue)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(columnModification, nameof(columnModification));

            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);

            var parameterValue = useOriginalValue
                ? columnModification.OriginalValue
                : columnModification.Value;

            if (parameterValue == null)
            {
                commandStringBuilder.Append(" IS NULL");
            }
            else
            {
                commandStringBuilder.Append(" = ");
                if (!columnModification.UseCurrentValueParameter
                    && !columnModification.UseOriginalValueParameter)
                {
                    AppendSqlLiteral(commandStringBuilder, columnModification.Value, columnModification.Property);
                }
                else
                {
                    SqlGenerationHelper.GenerateParameterNamePlaceholder(
                        commandStringBuilder, useOriginalValue
                            ? columnModification.OriginalParameterName
                            : columnModification.ParameterName);
                }
            }
        }

        /// <summary>
        ///     Appends a <c>WHERE</c> condition for the identity (i.e. key value) of the given column.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="columnModification"> The column for which the condition is being generated. </param>
        protected abstract void AppendIdentityWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ColumnModification columnModification);

        /// <summary>
        ///     Appends SQL text that defines the start of a batch.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        public virtual void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
        }

        /// <summary>
        ///     Generates SQL that will obtain the next value in the given sequence.
        /// </summary>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema that contains the sequence, or <c>null</c> to use the default schema. </param>
        /// <returns> The SQL. </returns>
        public virtual string GenerateNextSequenceValueOperation(string name, string schema)
        {
            var commandStringBuilder = new StringBuilder();
            AppendNextSequenceValueOperation(commandStringBuilder, name, schema);
            return commandStringBuilder.ToString();
        }

        /// <summary>
        ///     Generates a SQL fragment that will get the next value from the given sequence and appends it to
        ///     the full command being built by the given <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL fragment should be appended. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema that contains the sequence, or <c>null</c> to use the default schema. </param>
        public virtual void AppendNextSequenceValueOperation(StringBuilder commandStringBuilder, string name, string schema)
        {
            commandStringBuilder.Append("SELECT NEXT VALUE FOR ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, Check.NotNull(name, nameof(name)), schema);
        }

        private void AppendSqlLiteral(StringBuilder commandStringBuilder, object value, IProperty property)
        {
            var mapping = property != null
                ? Dependencies.TypeMappingSource.FindMapping(property)
                : null;
            mapping ??= Dependencies.TypeMappingSource.GetMappingForValue(value);
            commandStringBuilder.Append(mapping.GenerateProviderValueSqlLiteral(value));
        }
    }
}
