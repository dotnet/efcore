// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class DatabaseForeignKey : Annotatable
    {
        public virtual DatabaseTable Table { get; [param: CanBeNull] set; }

        public virtual DatabaseTable PrincipalTable { get; [param: CanBeNull]  set; }

        public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

        public virtual IList<DatabaseColumn> PrincipalColumns { get; } = new List<DatabaseColumn>();

        public virtual string Name { get; [param: CanBeNull] set; }

        public virtual ReferentialAction? OnDelete { get; [param: NotNull] set; }
    }
}
