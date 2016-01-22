// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class CreateTableOperation : MigrationOperation
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual AddPrimaryKeyOperation PrimaryKey { get; [param: CanBeNull] set; }
        public virtual List<AddColumnOperation> Columns { get; } = new List<AddColumnOperation>();
        public virtual List<AddForeignKeyOperation> ForeignKeys { get; } = new List<AddForeignKeyOperation>();
        public virtual List<AddUniqueConstraintOperation> UniqueConstraints { get; } = new List<AddUniqueConstraintOperation>();
    }
}
