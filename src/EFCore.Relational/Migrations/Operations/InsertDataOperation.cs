// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class InsertDataOperation : ModificationOperation
    {
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string[] Columns { get; [param: NotNull] set; }
        public virtual object[,] Values { get; [param: NotNull] set; }

        protected override IEnumerable<ModificationCommandBase> GenerateModificationCommands()
        {
            Debug.Assert(Columns.Length == Values.GetLength(1),
                $"The number of values doesn't match the number of keys (${Columns.Length})");

            for (var i = 0; i < Values.GetLength(0); i++)
            {
                var modifications = new ColumnModificationBase[Columns.Length];
                for (var j = 0; j < Columns.Length; j++)
                {
                    modifications[j] = new ColumnModificationBase(Columns[j], null, null, Values[i, j], false, true, true, false, false);
                }

                yield return new ModificationCommandBase(Table, Schema, modifications);
            }
        }
    }
}
