// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Migrations.Operations
{
    public class AlterColumnOperation : MigrationOperation
    {
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Type { get; [param: NotNull] set; }
        public virtual bool IsNullable { get; set; }
        public virtual object DefaultValue { get; [param: CanBeNull] set; }
        public virtual string DefaultValueSql { get; [param: CanBeNull] set; }
    }
}
