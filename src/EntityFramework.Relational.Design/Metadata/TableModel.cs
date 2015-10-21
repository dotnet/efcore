// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class TableModel
    {
        public virtual string Name { get; [param: NotNull] set; }

        [CanBeNull]
        public virtual string SchemaName { get; [param: CanBeNull] set; }

        public virtual IList<ColumnModel> Columns { get; [param: NotNull] set; } = new List<ColumnModel>();
        public virtual IList<IndexModel> Indexes { get; [param: NotNull] set; } = new List<IndexModel>();
        public virtual IList<ForeignKeyModel> ForeignKeys { get; [param: NotNull] set; } = new List<ForeignKeyModel>();

        public virtual string DisplayName
            => (!string.IsNullOrEmpty(SchemaName) ? SchemaName + "." : "") + Name;
    }
}
