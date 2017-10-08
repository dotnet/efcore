// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public class OracleModificationCommandBatch : AffectedCountModificationCommandBatch
    {
		internal const int MaxParameterCount = 1000;
		internal const int MaxRowCount = 300;
		internal int _CountParameter = 1;
		internal readonly List<ModificationCommand> _BatchInsertCommands;
		internal readonly StringBuilder _VariablesCommand;
		internal int _CursorPosition = 1;

		public OracleModificationCommandBatch(
			[NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
			[NotNull] ISqlGenerationHelper sqlGenerationHelper,
			[NotNull] IUpdateSqlGenerator updateSqlGenerator,
			[NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
			: base(commandBuilderFactory, sqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
		{
			_BatchInsertCommands = new List<ModificationCommand>();
			_VariablesCommand = new StringBuilder();
		}

		protected new virtual IOracleUpdateSqlGenerator UpdateSqlGenerator => (IOracleUpdateSqlGenerator)base.UpdateSqlGenerator;

		protected override bool CanAddCommand(ModificationCommand modificationCommand)
		{
			if (ModificationCommands.Count >= MaxRowCount)
				return false;

			var additionalParameterCount = CountParameters(modificationCommand);
			if (_CountParameter + additionalParameterCount >= MaxParameterCount)
				return false;

			_CountParameter += additionalParameterCount;
			return true;
		}

		protected override void ResetCommandText()
		{
			base.ResetCommandText();
			_BatchInsertCommands.Clear();
		}

		protected override string GetCommandText()
		{
			var bulkOperation = new StringBuilder();
			_VariablesCommand.Clear();
			bulkOperation.AppendLine(base.GetCommandText());
			bulkOperation.Append(GetBatchInsertCommandText(ModificationCommands.Count));

			if (_VariablesCommand.Length > 0)
			{
				var declareVariable = new StringBuilder();
				declareVariable.AppendLine("DECLARE");
				declareVariable.Append(_VariablesCommand)
							   .AppendLine("BEGIN");
				bulkOperation.Insert(0, declareVariable);
				bulkOperation.AppendLine("END;");
			}
			return bulkOperation.ToString();
		}

		private string GetBatchInsertCommandText(int lastIndex)
		{
			if (_BatchInsertCommands.Count == 0)
				return string.Empty;

			var stringBuilder = new StringBuilder();
			var resultSetMapping = UpdateSqlGenerator
				.AppendBulkInsertOperation(stringBuilder, _VariablesCommand, _BatchInsertCommands, lastIndex - _BatchInsertCommands.Count,ref _CursorPosition);

			for (var i = lastIndex - _BatchInsertCommands.Count; i < lastIndex; i++)
				CommandResultSet[i] = resultSetMapping;

			if (resultSetMapping != ResultSetMapping.NoResultSet)
				CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;

			return stringBuilder.ToString();
		}

		protected override void UpdateCachedCommandText(int commandPosition)
		{
			var newModificationCommand = ModificationCommands[commandPosition];
			if (newModificationCommand.EntityState == EntityState.Added)
			{
				if (_BatchInsertCommands.Count > 0
					&& !CanBeInserted(_BatchInsertCommands[0], newModificationCommand))
				{
					CachedCommandText.Append(GetBatchInsertCommandText(commandPosition));
					_BatchInsertCommands.Clear();
				}
				_BatchInsertCommands.Add(newModificationCommand);
				LastCachedCommandIndex = commandPosition;
			}
			else
			{
				CachedCommandText.Append(GetBatchInsertCommandText(commandPosition));
				_BatchInsertCommands.Clear();
				base.UpdateCachedCommandText(commandPosition);
			}
		}

		protected override bool IsCommandTextValid() => true;

		protected override int GetParameterCount() => _CountParameter;

		private static int CountParameters(ModificationCommand modificationCommand)
		{
			var parameterCount = 0;
			foreach (var columnModification in modificationCommand.ColumnModifications)
			{
				if (columnModification.UseCurrentValueParameter)
					parameterCount++;

				if (columnModification.UseOriginalValueParameter)
					parameterCount++;
			}
			return parameterCount;
		}

		private static bool CanBeInserted(ModificationCommand first, ModificationCommand second)
			=> string.Equals(first.TableName, second.TableName, StringComparison.Ordinal)
			   && string.Equals(first.Schema, second.Schema, StringComparison.Ordinal)
			   && first.ColumnModifications.Where(o => o.IsWrite)
				.Select(o => o.ColumnName)
				.SequenceEqual(second.ColumnModifications.Where(o => o.IsWrite)
				.Select(o => o.ColumnName))
			   && first.ColumnModifications.Where(o => o.IsRead)
				.Select(o => o.ColumnName)
				.SequenceEqual(second.ColumnModifications.Where(o => o.IsRead)
				.Select(o => o.ColumnName));
	}
}
