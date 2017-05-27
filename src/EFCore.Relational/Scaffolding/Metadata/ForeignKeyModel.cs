// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class ForeignKeyModel : Annotatable
    {
        public virtual TableModel Table { get; [param: CanBeNull] set; }

        public virtual TableModel PrincipalTable { get; [param: CanBeNull] set; }

        public virtual ICollection<ForeignKeyColumnModel> Columns { get; } = new List<ForeignKeyColumnModel>();

        public virtual string Name { get; [param: CanBeNull] set; }

        public virtual ReferentialAction? OnDelete { get; [param: NotNull] set; }

        // TODO foreign key triggers
        //public virtual ReferentialAction OnUpdate { get; [param: NotNull] set; }

        public virtual string DisplayName
            => Table?.DisplayName + "(" + string.Join(",", Columns.OrderBy(f => f.Ordinal).Select(f => f.Column.Name)) + ")";
    }
}
