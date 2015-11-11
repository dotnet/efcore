// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class ForeignKeyModel
    {
        [CanBeNull]
        public virtual TableModel Table { get; [param: CanBeNull] set; }

        [CanBeNull]
        public virtual TableModel PrincipalTable { get; [param: CanBeNull] set; }

        public virtual IList<ColumnModel> Columns { get; } = new List<ColumnModel>();
        public virtual IList<ColumnModel> PrincipalColumns { get; } = new List<ColumnModel>();

        [NotNull]
        public virtual string Name { get; [param: CanBeNull] set; }

        public virtual ReferentialAction? OnDelete { get; [param: NotNull] set; }

        // TODO foreign key triggers
        //public virtual ReferentialAction OnUpdate { get; [param: NotNull] set; }

        public virtual string DisplayName
            => Table?.DisplayName + "(" + string.Join(",", Columns.Select(f => f.Name)) + ")";
    }
}
