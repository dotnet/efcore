// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class Table : Annotatable
    {
        public virtual DatabaseModel Database { get; [param: NotNull] set; }

        public virtual string Name { get; [param: NotNull] set; }

        public virtual string Schema { get; [param: CanBeNull] set; }

        public virtual PrimaryKey PrimaryKey { get; [param: CanBeNull] set; }

        public virtual IList<Column> Columns { get; } = new List<Column>();
        public virtual IList<UniqueConstraint> UniqueConstraints { get; } = new List<UniqueConstraint>();
        public virtual IList<DatabaseIndex> Indexes { get; } = new List<DatabaseIndex>();
        public virtual IList<DatabaseForeignKey> ForeignKeys { get; } = new List<DatabaseForeignKey>();
    }
}
