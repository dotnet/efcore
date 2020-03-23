// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
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
    public class UpdateDataOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the table in which data will be updated.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns that will be used to identify
        ///     the rows that should be updated.
        /// </summary>
        public virtual string[] KeyColumns { get; [param: NotNull] set; }

        /// <summary>
        ///     The rows to be updated, represented as a list of key value arrays where each
        ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
        /// </summary>
        public virtual object[,] KeyValues { get; [param: NotNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns that contain data to be updated.
        /// </summary>
        public virtual string[] Columns { get; [param: NotNull] set; }

        /// <summary>
        ///     The data to be updated, represented as a list of value arrays where each
        ///     value in the array corresponds to a column in the <see cref="Columns" /> property.
        /// </summary>
        public virtual object[,] Values { get; [param: NotNull] set; }

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        public virtual IEnumerable<ModificationCommand> GenerateModificationCommands([CanBeNull] IModel model)
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

            for (var i = 0; i < KeyValues.GetLength(0); i++)
            {
                var keys = new ColumnModification[KeyColumns.Length];
                for (var j = 0; j < KeyColumns.Length; j++)
                {
                    keys[j] = new ColumnModification(
                        KeyColumns[j], originalValue: null, value: KeyValues[i, j], property: keyProperties?[j],
                        isRead: false, isWrite: false, isKey: true, isCondition: true, sensitiveLoggingEnabled: true);
                }

                var modifications = new ColumnModification[Columns.Length];
                for (var j = 0; j < Columns.Length; j++)
                {
                    modifications[j] = new ColumnModification(
                        Columns[j], originalValue: null, value: Values[i, j], property: properties?[j],
                        isRead: false, isWrite: true, isKey: true, isCondition: false, sensitiveLoggingEnabled: true);
                }

                yield return new ModificationCommand(Table, Schema, keys.Concat(modifications).ToArray(), sensitiveLoggingEnabled: true);
            }
        }
    }
}
