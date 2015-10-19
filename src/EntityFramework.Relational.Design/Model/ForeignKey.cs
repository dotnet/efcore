// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;

namespace Microsoft.Data.Entity.Relational.Design.Model
{
    public class ForeignKey
    {
        [CanBeNull]
        public virtual Table Table { get; [param: CanBeNull] set; }

        [CanBeNull]
        public virtual Table PrincipalTable { get; [param: CanBeNull] set; }

        public virtual IList<Column> From { get; [param: NotNull] set; } = new List<Column>();
        public virtual IList<Column> To { get; [param: NotNull] set; } = new List<Column>();

        [NotNull]
        public virtual string Name { get; [param: CanBeNull] set; }

        public virtual ReferentialAction? OnDelete { get; [param: NotNull] set; }

        // TODO foreign key triggers
        //public virtual ReferentialAction OnUpdate { get; [param: NotNull] set; }
    }
}
