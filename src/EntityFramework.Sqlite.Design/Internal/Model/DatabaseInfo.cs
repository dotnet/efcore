// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Model
{
    public class DatabaseInfo
    {
        public virtual List<ColumnInfo> Columns { get; [param: NotNull] set; } = new List<ColumnInfo>();
        public virtual List<ForeignKeyInfo> ForeignKeys { get; [param: NotNull] set; } = new List<ForeignKeyInfo>();
        public virtual List<IndexInfo> Indexes { get; [param: NotNull] set; } = new List<IndexInfo>();
        public virtual List<TableInfo> Tables { get; [param: NotNull] set; } = new List<TableInfo>();
    }
}
