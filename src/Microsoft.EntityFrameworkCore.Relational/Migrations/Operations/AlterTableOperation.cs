// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class AlterTableOperation : MigrationOperation, IAlterMigrationOperation
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual Annotatable OldTable { get; [param: NotNull] set; } = new Annotatable();
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations => OldTable;
    }
}
