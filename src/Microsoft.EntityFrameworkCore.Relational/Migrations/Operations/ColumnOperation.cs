// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class ColumnOperation : MigrationOperation
    {
        public virtual Type ClrType { get; [param: NotNull] set; }
        public virtual string ColumnType { get; [param: CanBeNull] set; }
        public virtual bool? IsUnicode { get; [param: CanBeNull] set; }
        public virtual int? MaxLength { get; [param: CanBeNull] set; }
        public virtual bool IsRowVersion { get; set; }
        public virtual bool IsNullable { get; set; }
        public virtual object DefaultValue { get; [param: CanBeNull] set; }
        public virtual string DefaultValueSql { get; [param: CanBeNull] set; }
        public virtual string ComputedColumnSql { get; [param: CanBeNull] set; }
    }
}
