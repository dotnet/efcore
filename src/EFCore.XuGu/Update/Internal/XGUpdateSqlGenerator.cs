// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Update.Internal
{
    public class XGUpdateSqlGenerator : UpdateAndSelectSqlGenerator, IXGUpdateSqlGenerator
    {
        [NotNull] private readonly IXGOptions _options;

        public XGUpdateSqlGenerator(
            [NotNull] UpdateSqlGeneratorDependencies dependencies,
            [NotNull] IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        public override ResultSetMapping AppendInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition,
            out bool requiresTransaction)
            => _options.ServerVersion.Supports.Returning ||
               command.ColumnModifications.All(o => !o.IsRead)
                ? AppendInsertReturningOperation(commandStringBuilder, command, commandPosition, out requiresTransaction)
                : base.AppendInsertOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

        public virtual ResultSetMapping AppendBulkInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
            int commandPosition,
            out bool requiresTransaction)
        {
            if (modificationCommands.Count == 1)
            {
                return AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition, out requiresTransaction);
            }

            var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
            var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();

            if (readOperations.Count == 0)
            {
                return AppendInsertMultipleRowsInSingleStatementOperation(commandStringBuilder, modificationCommands, writeOperations, out requiresTransaction);
            }

            requiresTransaction = modificationCommands.Count > 1;
            foreach (var modification in modificationCommands)
            {
                AppendInsertOperation(commandStringBuilder, modification, commandPosition, out var localRequiresTransaction);
                requiresTransaction = requiresTransaction || localRequiresTransaction;
            }

            return ResultSetMapping.LastInResultSet;
        }

        private ResultSetMapping AppendInsertMultipleRowsInSingleStatementOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
            List<IColumnModification> writeOperations,
            out bool requiresTransaction)
        {
            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, name, schema, writeOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(commandStringBuilder, name, schema, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            // A single INSERT command should run atomically, regardless of how many value lists it contains.
            requiresTransaction = false;

            return ResultSetMapping.NoResults;
        }

        protected override void AppendInsertCommandHeader(
            StringBuilder commandStringBuilder,
            string name,
            string schema,
            IReadOnlyList<IColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            base.AppendInsertCommandHeader(commandStringBuilder, name, schema, operations);

            if (operations.Count <= 0)
            {
                // An empty column and value list signales MySQL that only default values should be used.
                // If not all columns have default values defined, an error occurs if STRICT_ALL_TABLES has been set.
                commandStringBuilder.Append(" ()");
            }
        }

        protected override void AppendValuesHeader(
            StringBuilder commandStringBuilder,
            IReadOnlyList<IColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.AppendLine();
            commandStringBuilder.Append("VALUES ");
        }

        protected override void AppendValues(
            StringBuilder commandStringBuilder,
            string name,
            string schema,
            IReadOnlyList<IColumnModification> operations)
        {
            base.AppendValues(commandStringBuilder, name, schema, operations);

            if (operations.Count <= 0)
            {
                commandStringBuilder.Append("()");
            }
        }

        public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition,
            out bool requiresTransaction)
            => _options.ServerVersion.Supports.Returning
                ? AppendDeleteReturningOperation(commandStringBuilder, command, commandPosition, out requiresTransaction)
                : base.AppendDeleteOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

        protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            commandStringBuilder
                .Append("SELECT ROW_COUNT()")
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine()
                .AppendLine();

            return ResultSetMapping.LastInResultSet | ResultSetMapping.ResultSetWithRowsAffectedOnly;
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, IColumnModification columnModification)
        {
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
            commandStringBuilder.Append(" = ")
                .Append("LAST_INSERT_ID()");
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => commandStringBuilder
                .Append("ROW_COUNT() = ")
                .Append(expectedRowsAffected.ToString(CultureInfo.InvariantCulture));

        public override ResultSetMapping AppendStoredProcedureCall(
            StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition,
            out bool requiresTransaction)
        {
            Check.DebugAssert(command.StoreStoredProcedure is not null, "command.StoreStoredProcedure is not null");

            var storedProcedure = command.StoreStoredProcedure;

            Check.DebugAssert(storedProcedure.Parameters.Any(), "Stored procedure call without parameters");

            var resultSetMapping = ResultSetMapping.NoResults;

            // IN parameters will get injected directly into the argument list of the CALL statement.
            // FOR INOUT or OUT parameters, we will declare variables.
            // For OUT parameters, we initialize those variables to NUll.
            // For IN parameters, we initialize those variables with their corresponding parameter of the command that executes the CALL
            // statement.
            for (var i = 0; i < command.ColumnModifications.Count; i++)
            {
                var columnModification = command.ColumnModifications[i];
                var parameter = (IStoreStoredProcedureParameter)columnModification.Column!;

                // MySQL stored procedures cannot return a regular result set, and output parameter values are simply sent back to us as the
                // result set, if we append a SELECT query for them. This is very different from SQL Server, where output parameter values
                // can be sent back in addition to result sets.
                if (!parameter.Direction.HasFlag(ParameterDirection.Output))
                {
                    continue;
                }

                // The distinction between having only a rows affected output parameter and having other non-rows affected parameters
                // is important later on (i.e. whether we need to propagate or not).
                resultSetMapping = parameter == command.RowsAffectedColumn &&
                                   resultSetMapping == ResultSetMapping.NoResults
                    ? ResultSetMapping.ResultSetWithRowsAffectedOnly | ResultSetMapping.LastInResultSet
                    : ResultSetMapping.LastInResultSet;

                commandStringBuilder.Append("SET ");

                var commandParameterName = columnModification.UseOriginalValueParameter
                    ? columnModification.OriginalParameterName!
                    : columnModification.ParameterName!;

                var procedureCallParameterName = GetProcedureCallOutParameterVariableName(commandParameterName);

                SqlGenerationHelper.GenerateParameterNamePlaceholder(commandStringBuilder, procedureCallParameterName);

                commandStringBuilder.Append(" = ");

                if (parameter.Direction.HasFlag(ParameterDirection.Input))
                {
                    SqlGenerationHelper.GenerateParameterNamePlaceholder(commandStringBuilder, commandParameterName);
                }
                else
                {
                    commandStringBuilder.Append("NULL");
                }

                commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
            }

            commandStringBuilder.Append("CALL ");

            // MySQL supports neither a return value nor a result set that gets returned from inside of a stored procedures. It only
            // supports output parameters to propagate values back to the caller.
            Check.DebugAssert(storedProcedure.ReturnValue is null, "storedProcedure.Return is null");
            Check.DebugAssert(!storedProcedure.ResultColumns.Any(), "!storedProcedure.ResultColumns.Any()");

            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, storedProcedure.Name, storedProcedure.Schema);

            commandStringBuilder.Append('(');

            // Only positional parameter style supported for now, see https://github.com/dotnet/efcore/issues/28439

            // Note: the column modifications are already ordered according to the sproc parameter ordering
            // (see ModificationCommand.GenerateColumnModifications)
            for (var i = 0; i < command.ColumnModifications.Count; i++)
            {
                var columnModification = command.ColumnModifications[i];
                var parameter = (IStoreStoredProcedureParameter)columnModification.Column!;

                if (i > 0)
                {
                    commandStringBuilder.Append(", ");
                }

                Check.DebugAssert(columnModification.UseParameter, "Column modification matched a parameter, but UseParameter is false");

                var commandParameterName = columnModification.UseOriginalValueParameter
                    ? columnModification.OriginalParameterName!
                    : columnModification.ParameterName!;

                var procedureCallParameterName = GetProcedureCallOutParameterVariableName(commandParameterName);

                SqlGenerationHelper.GenerateParameterNamePlaceholder(
                    commandStringBuilder,
                    parameter.Direction.HasFlag(ParameterDirection.Output)
                        ? procedureCallParameterName
                        : commandParameterName);
            }

            commandStringBuilder.Append(')');
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

            // The CALL has propagated any INOUT and OUT values back into our previously declared variables.
            // To get those values back to the caller, we need to run a SELECT statement against those variables.
            // We start by checking, whether there exist any INOUT or OUT parameters.
            if (resultSetMapping != ResultSetMapping.NoResults)
            {
                commandStringBuilder.Append("SELECT ");

                var first = true;

                for (var i = 0; i < command.ColumnModifications.Count; i++)
                {
                    var columnModification = command.ColumnModifications[i];
                    var parameter = (IStoreStoredProcedureParameter)columnModification.Column!;

                    if (!parameter.Direction.HasFlag(ParameterDirection.Output))
                    {
                        continue;
                    }

                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandStringBuilder.Append(", ");
                    }

                    var commandParameterName = columnModification.UseOriginalValueParameter
                        ? columnModification.OriginalParameterName!
                        : columnModification.ParameterName!;

                    var procedureCallParameterName = GetProcedureCallOutParameterVariableName(commandParameterName);

                    SqlGenerationHelper.GenerateParameterNamePlaceholder(
                        commandStringBuilder,
                        procedureCallParameterName);
                }

                commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
            }

            requiresTransaction = true;

            return resultSetMapping;
        }

        /// <summary>
        /// Returns the name (without the @ prefix) used for any temporary user variables, that need to be declared to get values out of a
        /// stored procedure.
        /// </summary>
        /// <param name="commandParameterName">The name of the parameter of the command that executes the CALL statement.</param>
        /// <returns>The variable name (without the @ prefix).</returns>
        protected virtual string GetProcedureCallOutParameterVariableName(string commandParameterName)
            => "_out_" + commandParameterName;

        protected override bool IsIdentityOperation(IColumnModification modification)
        {
            var isIdentityOperation = base.IsIdentityOperation(modification);

            if (isIdentityOperation &&
                modification.Property is { } property)
            {
                var (tableName, schema) = GetTableNameAndSchema(modification, property);
                var storeObject = StoreObjectIdentifier.Table(tableName, schema);

                return property.GetValueGenerationStrategy(storeObject) is XGValueGenerationStrategy.IdentityColumn;
            }

            return isIdentityOperation;
        }

        public override void PrependEnsureAutocommit(StringBuilder commandStringBuilder)
            => commandStringBuilder.Insert(0, $"SET AUTOCOMMIT = 1{SqlGenerationHelper.StatementTerminator}{Environment.NewLine}");

        private static (string tableName, string schema) GetTableNameAndSchema(IColumnModification modification, IProperty property)
        {
            if (modification.Column?.Table is { } table)
            {
                return (table.Name, table.Schema);
            }
            else
            {
                // CHECK: Is this branch ever hit and then returns something different than null, or can we just rely on
                // `modification.Column?.Table`?
                return (property.DeclaringType.GetTableName(), property.DeclaringType.GetSchema());
            }
        }
    }
}
