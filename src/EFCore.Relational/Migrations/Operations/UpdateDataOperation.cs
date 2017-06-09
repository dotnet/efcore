// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class UpdateDataOperation : ModificationOperation
    {
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string[] KeyColumns { get; [param: NotNull] set; }
        public virtual object[,] KeyValues { get; [param: NotNull] set; }
        public virtual string[] Columns { get; [param: NotNull] set; }
        public virtual object[,] Values { get; [param: NotNull] set; }

        protected override IEnumerable<ModificationCommandBase> GenerateModificationCommands()
        {
            Debug.Assert(KeyColumns.Length == KeyValues.GetLength(1),
                $"The number of key values doesn't match the number of keys (${KeyColumns.Length})");
            Debug.Assert(Columns.Length == Values.GetLength(1),
                $"The number of values doesn't match the number of keys (${Columns.Length})");
            Debug.Assert(KeyValues.GetLength(0) == Values.GetLength(0),
                $"The number of key values doesn't match the number of values (${KeyValues.GetLength(0)})");

            for (var i = 0; i < KeyValues.GetLength(0); i++)
            {
                var keys = new ColumnModificationBase[KeyColumns.Length];
                for (var j = 0; j < KeyColumns.Length; j++)
                {
                    keys[j] = new ColumnModificationBase(KeyColumns[j], null, null, KeyValues[i, j], false, false, true, true, false);
                }

                var modifications = new ColumnModificationBase[Columns.Length];
                for (var j = 0; j < Columns.Length; j++)
                {
                    modifications[j] = new ColumnModificationBase(Columns[j], null, null, Values[i, j], false, true, true, false, false);
                }

                yield return new ModificationCommandBase(Table, Schema, keys.Concat(modifications).ToArray());
            }
        }
    }
}