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
    ///     A <see cref="MigrationOperation" /> for inserting seed data into a table.
    /// </summary>
    [DebuggerDisplay("INSERT INTO {Table}")]
    public class InsertDataOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the table into which data will be inserted.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns into which data will be inserted.
        /// </summary>
        public virtual string[] Columns { get; [param: NotNull] set; }

        /// <summary>
        ///     The data to be inserted, represented as a list of value arrays where each
        ///     value in the array corresponds to a column in the <see cref="Columns" /> property.
        /// </summary>
        public virtual object[,] Values { get; [param: NotNull] set; }

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        public virtual IEnumerable<ModificationCommand> GenerateModificationCommands([CanBeNull] IModel model)
        {
            Debug.Assert(
                Columns.Length == Values.GetLength(1),
                $"The number of values doesn't match the number of keys (${Columns.Length})");

            var properties = model != null
                ? TableMapping.GetTableMapping(model, Table, Schema)?.GetPropertyMap()
                : null;

            for (var i = 0; i < Values.GetLength(0); i++)
            {
                var modifications = new ColumnModification[Columns.Length];
                for (var j = 0; j < Columns.Length; j++)
                {
                    modifications[j] = new ColumnModification(
                        Columns[j], originalValue: null, value: Values[i, j], property: properties?.Find(Columns[j]),
                        isRead: false, isWrite: true, isKey: true, isCondition: false, sensitiveLoggingEnabled: true);
                }

                yield return new ModificationCommand(Table, Schema, modifications, sensitiveLoggingEnabled: true);
            }
        }
    }
}
