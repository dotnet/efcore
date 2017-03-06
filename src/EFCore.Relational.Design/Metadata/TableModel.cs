// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class TableModel : Annotatable
    {
        public virtual DatabaseModel Database { get; [param: NotNull] set; }

        public virtual string Name { get; [param: NotNull] set; }

        [CanBeNull]
        public virtual string SchemaName { get; [param: CanBeNull] set; }

        public virtual ICollection<ColumnModel> Columns { get; [param: NotNull] set; } = new List<ColumnModel>();
        public virtual ICollection<IndexModel> Indexes { get; } = new List<IndexModel>();
        public virtual ICollection<ForeignKeyModel> ForeignKeys { get; } = new List<ForeignKeyModel>();

        public virtual string DisplayName
            => (!string.IsNullOrEmpty(SchemaName) ? SchemaName + "." : "") + Name;
    }
}
