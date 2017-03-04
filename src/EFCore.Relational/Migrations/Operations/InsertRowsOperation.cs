// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class InsertRowsOperation : MigrationOperation
    {
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string[] Columns { get; [param: NotNull] set; }
        public virtual object[,] Values { get; [param: NotNull] set; }
    }
}
