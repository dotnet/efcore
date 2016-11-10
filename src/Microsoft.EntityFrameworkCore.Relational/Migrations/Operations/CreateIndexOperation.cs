// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class CreateIndexOperation : MigrationOperation
    {
        public virtual bool IsUnique { get; set; }
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string[] Columns { get; [param: NotNull] set; }
        public virtual string Filter { get; [param: NotNull] set; }
    }
}
