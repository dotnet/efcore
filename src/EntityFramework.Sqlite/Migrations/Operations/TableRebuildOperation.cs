// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Sqlite.Migrations.Operations
{
    public class TableRebuildOperation : MigrationOperation
    {
        public virtual string Table { get; [param: NotNull] set; }
        public virtual List<MigrationOperation> Operations { get; [param: NotNull] set; } = new List<MigrationOperation>();
    }
}
