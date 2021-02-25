// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Update;
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
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns that will be used to identify
        ///     the rows that should be deleted.
        /// </summary>
        public virtual string[] KeyColumns { get; [param: NotNull] set; }

        /// <summary>
        ///     A list of store types for the columns that will be used to identify
        ///     the rows that should be deleted.
        /// </summary>
        public virtual string[] KeyColumnTypes { get; [param: NotNull] set; }

        /// <summary>
        ///     The rows to be deleted, represented as a list of key value arrays where each
        ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
        /// </summary>
        public virtual object[,] KeyValues { get; [param: NotNull] set; }

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        [Obsolete]
        public virtual IEnumerable<ModificationCommand> GenerateModificationCommands([CanBeNull] IModel model)
        {
            Check.DebugAssert(
                KeyColumns.Length == KeyValues.GetLength(1),
                $"The number of key values doesn't match the number of keys (${KeyColumns.Length})");

            var table = model?.GetRelationalModel().FindTable(Table, Schema);
            var properties = table != null
                ? MigrationsModelDiffer.GetMappedProperties(table, KeyColumns)
                : null;

            for (var i = 0; i < KeyValues.GetLength(0); i++)
            {
                var modifications = new ColumnModification[KeyColumns.Length];
                for (var j = 0; j < KeyColumns.Length; j++)
                {
                    modifications[j] = new ColumnModification(
                        KeyColumns[j], originalValue: null, value: KeyValues[i, j], property: properties?[j],
                        columnType: KeyColumnTypes?[j], isRead: false, isWrite: true, isKey: true, isCondition: true,
                        sensitiveLoggingEnabled: false);
                }

                yield return new ModificationCommand(Table, Schema, modifications, sensitiveLoggingEnabled: false);
            }
        }
    }
}
