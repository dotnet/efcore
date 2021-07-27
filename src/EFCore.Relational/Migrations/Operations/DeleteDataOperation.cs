// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for deleting seed data from an existing table.
    /// </summary>
    [DebuggerDisplay("DELETE FROM {Table}")]
    public class DeleteDataOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The table from which data will be deleted.
        /// </summary>
        public virtual string Table { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     A list of column names that represent the columns that will be used to identify
        ///     the rows that should be deleted.
        /// </summary>
        public virtual string[] KeyColumns { get; set; } = null!;

        /// <summary>
        ///     A list of store types for the columns that will be used to identify
        ///     the rows that should be deleted.
        /// </summary>
        public virtual string[]? KeyColumnTypes { get; set; }

        /// <summary>
        ///     The rows to be deleted, represented as a list of key value arrays where each
        ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
        /// </summary>
        public virtual object?[,] KeyValues { get; set; } = null!;

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        [Obsolete]
        public virtual IEnumerable<ModificationCommand> GenerateModificationCommands(IModel? model)
        {
            Check.DebugAssert(
                KeyColumns.Length == KeyValues.GetLength(1),
                $"The number of key values doesn't match the number of keys (${KeyColumns.Length})");

            var table = model?.GetRelationalModel().FindTable(Table, Schema);
            var properties = table != null
                ? MigrationsModelDiffer.GetMappedProperties(table, KeyColumns)
                : null;

            var modificationCommandFactory = new MutableModificationCommandFactory();

            for (var i = 0; i < KeyValues.GetLength(0); i++)
            {
                var modificationCommand = modificationCommandFactory.CreateModificationCommand(new ModificationCommandParameters(
                    Table, Schema, sensitiveLoggingEnabled: false));
                for (var j = 0; j < KeyColumns.Length; j++)
                {
                    var columnModificationParameters = new ColumnModificationParameters(
                        KeyColumns[j], originalValue: null, value: KeyValues[i, j], property: properties?[j],
                        columnType: KeyColumnTypes?[j], typeMapping: null, read: false, write: true, key: true, condition: true,
                        sensitiveLoggingEnabled: false);

                    modificationCommand.AddColumnModification(columnModificationParameters);
                }

                yield return (ModificationCommand)modificationCommand;
            }
        }
    }
}
