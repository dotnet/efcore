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
	public class OracleUpdateSqlGenerator : UpdateSqlGenerator, IOracleUpdateSqlGenerator
	{
		private readonly IRelationalTypeMapper _typeMapper;

		public OracleUpdateSqlGenerator(
			[NotNull] UpdateSqlGeneratorDependencies dependencies,
			[NotNull] IRelationalTypeMapper typeMapper)
			: base(dependencies)
		{
			_typeMapper = typeMapper;
		}

		public virtual ResultSetMapping AppendBulkInsertOperation(
			StringBuilder commandStringBuilder, 
			StringBuilder variablesCommand,
			IReadOnlyList<ModificationCommand> modificationCommands, 
			int commandPosition,
			ref int cursorPosition)
		{
			commandStringBuilder.Clear();
			var resultSetMapping = ResultSetMapping.NoResultSet;

			var name = modificationCommands[0].TableName;
			var schema = modificationCommands[0].Schema;
			var reads = modificationCommands[0].ColumnModifications
											   .Where(o => o.IsRead).ToArray();
			if (reads.Any())
			{
				variablesCommand.AppendLine($"TYPE efRow{name} IS RECORD")
								.AppendLine("("); 
				variablesCommand.AppendJoin(
							reads,
							(sb, cm) =>
								sb.Append(cm.ColumnName)
									.Append(" ")
									.AppendLine(GetVariableType(cm))
									, ",")
							.Append(")")
							.AppendLine(SqlGenerationHelper.StatementTerminator);

				variablesCommand.Append($"TYPE ef{name} IS TABLE OF efRow{name}")
								.AppendLine(SqlGenerationHelper.StatementTerminator)
								.Append($"list{name} ef{name}")
								.AppendLine(SqlGenerationHelper.StatementTerminator);

				commandStringBuilder.Append("list")
					.Append(name)
					.Append(" := ")
					.Append($"ef{name}")
					.Append("()")
					.AppendLine(SqlGenerationHelper.StatementTerminator);

				commandStringBuilder.Append($"list{name}.extend(")
									.Append(modificationCommands.Count)
									.Append(")")
									.AppendLine(SqlGenerationHelper.StatementTerminator);
			}
			 
			for (var i = 0; i < modificationCommands.Count; i++)
			{
				var operations = modificationCommands[i].ColumnModifications;
				var readOperations = operations.Where(o => o.IsRead).ToArray();
				var writeOperations = operations.Where(o => o.IsWrite).ToArray(); 
				AppendInsertCommand(commandStringBuilder, name, schema, writeOperations, readOperations);
				AppendReturnInsert(commandStringBuilder, name, readOperations, i); 
			}

			//Hack: to return cursor to ResultNext (DataReader) 
			for (var i = 0; i < modificationCommands.Count; i++)
			{
				var operations = modificationCommands[i].ColumnModifications;
				var readOperations = operations.Where(o => o.IsRead).ToArray();
				if (readOperations.Any())
				{ 
					AppendReturnCursor(commandStringBuilder, name, readOperations, i, cursorPosition);
					resultSetMapping = ResultSetMapping.LastInResultSet;
					cursorPosition++;
				} 
			} 
			return resultSetMapping;
		}

		public override void AppendNextSequenceValueOperation(StringBuilder commandStringBuilder, string name, string schema)
		{
			commandStringBuilder.Append("SELECT ");
			SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, Check.NotNull(name, nameof(name)), schema);
			commandStringBuilder.Append(".NEXTVAL FROM DUAL");
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

		private void AppendReturnInsert(
			StringBuilder commandStringBuilder,
			string name,
			IReadOnlyList<ColumnModification> operations, 
			int commandPosition)
		{
			if (operations.Any())
			{
				commandStringBuilder
					.AppendLine()
					.Append("RETURNING ")
					.AppendJoin(
						operations,
						(sb, cm) => sb.Append(SqlGenerationHelper.DelimitIdentifier(cm.ColumnName)))
					.Append($" INTO list{name}({commandPosition + 1})");
			}
			commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator); 
		}

		private void AppendReturnCursor(
			StringBuilder commandStringBuilder,
			string name,
			IReadOnlyList<ColumnModification> operations,
			int commandPosition,
			int cursorPosition)
		{
			if (operations.Any())
			{
				commandStringBuilder
					.Append("OPEN :cur") 
					.Append(cursorPosition)
					.Append(" FOR")
					.Append(" SELECT ")
					.AppendJoin(operations,
								(sb, o) => sb.Append("list")
											 .Append(name)
											 .Append("(")
											 .Append(commandPosition+1)
											 .Append(").")
											 .Append(o.ColumnName), ",")
					.Append(" FROM DUAL")
					.AppendLine(SqlGenerationHelper.StatementTerminator);
			}
		}

		private static string GetVariableName(ColumnModification columnModification)
		{
			return $"v_{columnModification.ColumnName}";
		}

		private string GetVariableType(ColumnModification columnModification)
		{
			return _typeMapper.FindMapping(columnModification.Property).StoreType;
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
	}
}
