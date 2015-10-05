// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.Model
{
    public class Table
    {
        public virtual string Name { get; [param: NotNull] set; }

        [CanBeNull]
        public virtual string SchemaName { get; [param: CanBeNull] set; }

        public virtual IList<Column> Columns { get; [param: NotNull] set; } = new List<Column>();
        public virtual IList<Index> Indexes { get; [param: NotNull] set; } = new List<Index>();
        public virtual IList<ForeignKey> ForeignKeys { get; [param: NotNull] set; } = new List<ForeignKey>();
    }
}
