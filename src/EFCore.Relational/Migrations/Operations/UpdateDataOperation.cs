// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for updating seed data in an existing table.
    /// </summary>
    [DebuggerDisplay("UPDATE {Table}")]
    public class UpdateDataOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The name of the table in which data will be updated.
        /// </summary>
        public virtual string Table { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     A list of column names that represent the columns that will be used to identify
        ///     the rows that should be updated.
        /// </summary>
        public virtual string[] KeyColumns { get; set; } = null!;

        /// <summary>
        ///     A list of store types for the columns that will be used to identify
        ///     the rows that should be updated.
        /// </summary>
        public virtual string[]? KeyColumnTypes { get; set; }

        /// <summary>
        ///     The rows to be updated, represented as a list of key value arrays where each
        ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
        /// </summary>
        public virtual object?[,] KeyValues { get; set; } = null!;

        /// <summary>
        ///     A list of column names that represent the columns that contain data to be updated.
        /// </summary>
        public virtual string[] Columns { get; set; } = null!;

        /// <summary>
        ///     A list of store types for the columns in which data will be updated.
        /// </summary>
        public virtual string[]? ColumnTypes { get; set; }

        /// <summary>
        ///     The data to be updated, represented as a list of value arrays where each
        ///     value in the array corresponds to a column in the <see cref="Columns" /> property.
        /// </summary>
        public virtual object?[,] Values { get; set; } = null!;

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        /// <remarks>
        ///     This obsolete method creates ColumnModification directly and attaches ModificationCommand
        ///     the own implementation of IColumnModificationFactory.
        /// </remarks>
        [Obsolete]
        public virtual IEnumerable<ModificationCommand> GenerateModificationCommands(IModel? model)
        {
            Check.DebugAssert(
                KeyColumns.Length == KeyValues.GetLength(1),
                $"The number of key values doesn't match the number of keys (${KeyColumns.Length})");
            Check.DebugAssert(
                Columns.Length == Values.GetLength(1),
                $"The number of values doesn't match the number of keys (${Columns.Length})");
            Check.DebugAssert(
                KeyValues.GetLength(0) == Values.GetLength(0),
                $"The number of key values doesn't match the number of values (${KeyValues.GetLength(0)})");

            var table = model?.GetRelationalModel().FindTable(Table, Schema);
            var keyProperties = table != null
                ? MigrationsModelDiffer.GetMappedProperties(table, KeyColumns)
                : null;
            var properties = table != null
                ? MigrationsModelDiffer.GetMappedProperties(table, Columns)
                : null;

            var columnModificationFactory = new Update.Internal.ColumnModificationFactory();

            for (var i = 0; i < KeyValues.GetLength(0); i++)
            {
                var keys = new IColumnModification[KeyColumns.Length];
                for (var j = 0; j < KeyColumns.Length; j++)
                {
                    var columnModificationParameters = new ColumnModificationParameters(
                        KeyColumns[j], originalValue: null, value: KeyValues[i, j], property: keyProperties?[j],
                        columnType: KeyColumnTypes?[j], typeMapping: null, valueIsRead: false, valueIsWrite: false, columnIsKey: true, columnIsCondition: true,
                        sensitiveLoggingEnabled: false);

                    keys[j] = columnModificationFactory.CreateColumnModification(columnModificationParameters);
                }

                var modifications = new IColumnModification[Columns.Length];
                for (var j = 0; j < Columns.Length; j++)
                {
                    var columnModificationParameters = new ColumnModificationParameters(
                        Columns[j], originalValue: null, value: Values[i, j], property: properties?[j],
                        columnType: ColumnTypes?[j], typeMapping: null, valueIsRead: false, valueIsWrite: true, columnIsKey: true, columnIsCondition: false,
                        sensitiveLoggingEnabled: false);

                    modifications[j] = columnModificationFactory.CreateColumnModification(columnModificationParameters);
                }

                var modificationCommandParameters = new ModificationCommandParameters(
                    Table, Schema, keys.Concat(modifications).ToArray(), sensitiveLoggingEnabled: false);

                yield return new ModificationCommand(modificationCommandParameters);
            }
        }
    }
}
