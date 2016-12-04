// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class AlterColumnOperation : ColumnOperation, IAlterMigrationOperation
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string Table { get; [param: NotNull] set; }
        public virtual ColumnOperation OldColumn { get; [param: NotNull] set; } = new ColumnOperation();
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations => OldColumn;
    }
}
