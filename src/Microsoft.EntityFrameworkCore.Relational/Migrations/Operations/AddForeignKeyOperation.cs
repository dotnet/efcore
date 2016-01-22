// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class AddForeignKeyOperation : MigrationOperation
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string[] Columns { get; [param: NotNull] set; }
        public virtual string PrincipalSchema { get; [param: CanBeNull] set; }
        public virtual string PrincipalTable { get; [param: NotNull] set; }
        public virtual string[] PrincipalColumns { get; [param: NotNull] set; }
        public virtual ReferentialAction OnUpdate { get; set; }
        public virtual ReferentialAction OnDelete { get; set; }
    }
}
