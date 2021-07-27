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
    ///     A <see cref="MigrationOperation" /> for inserting seed data into a table.
    /// </summary>
    [DebuggerDisplay("INSERT INTO {Table}")]
    public class InsertDataOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The name of the table into which data will be inserted.
        /// </summary>
        public virtual string Table { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     A list of column names that represent the columns into which data will be inserted.
        /// </summary>
        public virtual string[] Columns { get; set; } = null!;

        /// <summary>
        ///     A list of store types for the columns into which data will be inserted.
        /// </summary>
        public virtual string[]? ColumnTypes { get; set; }

        /// <summary>
        ///     The data to be inserted, represented as a list of value arrays where each
        ///     value in the array corresponds to a column in the <see cref="Columns" /> property.
        /// </summary>
        public virtual object?[,] Values { get; set; } = null!;

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        [Obsolete]
        public virtual IEnumerable<ModificationCommand> GenerateModificationCommands(IModel? model)
        {
            Check.DebugAssert(
                Columns.Length == Values.GetLength(1),
                $"The number of values doesn't match the number of keys (${Columns.Length})");

            var table = model?.GetRelationalModel().FindTable(Table, Schema);
            var properties = table != null
                ? MigrationsModelDiffer.GetMappedProperties(table, Columns)
                : null;

            var modificationCommandFactory = new MutableModificationCommandFactory();

            for (var i = 0; i < Values.GetLength(0); i++)
            {
                var modificationCommand = modificationCommandFactory.CreateModificationCommand(new ModificationCommandParameters(
                    Table, Schema, sensitiveLoggingEnabled: false));

                for (var j = 0; j < Columns.Length; j++)
                {
                    var columnModificationParameters = new ColumnModificationParameters(
                        Columns[j], originalValue: null, value: Values[i, j], property: properties?[j],
                        columnType: ColumnTypes?[j], typeMapping: null, read: false, write: true, key: true, condition: false,
                        sensitiveLoggingEnabled: false);

                    modificationCommand.AddColumnModification(columnModificationParameters);
                }

                yield return (ModificationCommand)modificationCommand;
            }
        }
    }
}
