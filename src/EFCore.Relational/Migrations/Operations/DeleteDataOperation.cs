// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class DeleteDataOperation : ModificationOperation
    {
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string[] KeyColumns { get; [param: NotNull] set; }
        public virtual object[,] KeyValues { get; [param: NotNull] set; }

        protected override IEnumerable<ModificationCommandBase> GenerateModificationCommands()
        {
            Debug.Assert(KeyColumns.Length == KeyValues.GetLength(1),
                $"The number of key values doesn't match the number of keys (${KeyColumns.Length})");

            for (var i = 0; i < KeyValues.GetLength(0); i++)
            {
                var modifications = new ColumnModificationBase[KeyColumns.Length];
                for (var j = 0; j < KeyColumns.Length; j++)
                {
                    modifications[j] = new ColumnModificationBase(KeyColumns[j], null, null, KeyValues[i, j], false, true, true, true, false);
                }

                yield return new ModificationCommandBase(Table, Schema, modifications);
            }
        }
    }
}