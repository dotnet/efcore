// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for deleting seed data from an existing table.
    /// </summary>
    [DebuggerDisplay("DELETE FROM {Table}")]
    public class DeleteDataOperation : MigrationOperation
    {
        /// <summary>
        ///     The table from which data will be deleted.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns that will be used to identify
        ///     the rows that should be deleted.
        /// </summary>
        public virtual string[] KeyColumns { get; [param: NotNull] set; }

        /// <summary>
        ///     The rows to be deleted, represented as a list of key value arrays where each
        ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
        /// </summary>
        public virtual object[,] KeyValues { get; [param: NotNull] set; }

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        public virtual IEnumerable<ModificationCommand> GenerateModificationCommands([CanBeNull] IModel model)
        {
            Debug.Assert(
                KeyColumns.Length == KeyValues.GetLength(1),
                $"The number of key values doesn't match the number of keys (${KeyColumns.Length})");

            var properties = model != null
                ? TableMapping.GetTableMapping(model, Table, Schema)?.GetPropertyMap()
                : null;

            for (var i = 0; i < KeyValues.GetLength(0); i++)
            {
                var modifications = new ColumnModification[KeyColumns.Length];
                for (var j = 0; j < KeyColumns.Length; j++)
                {
                    modifications[j] = new ColumnModification(
                        KeyColumns[j], originalValue: null, value: KeyValues[i, j], property: properties?.Find(KeyColumns[j]),
                        isRead: false, isWrite: true, isKey: true, isCondition: true, sensitiveLoggingEnabled: true);
                }

                yield return new ModificationCommand(Table, Schema, modifications, sensitiveLoggingEnabled: true);
            }
        }
    }
}
