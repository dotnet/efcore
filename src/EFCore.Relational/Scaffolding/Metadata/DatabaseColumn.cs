// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class DatabaseColumn : Annotatable
    {
        public virtual DatabaseTable Table { get; [param: NotNull] set; }
        public virtual string Name { get; [param: NotNull] set; }
        public virtual bool IsNullable { get; [param: NotNull] set; }
        public virtual string StoreType { get; [param: CanBeNull] set; }
        public virtual string DefaultValueSql { get; [param: CanBeNull] set; }
        public virtual string ComputedColumnSql { get; [param: CanBeNull] set; }
        public virtual ValueGenerated? ValueGenerated { get; set; }
    }
}
