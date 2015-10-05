// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Model
{
    public class ColumnInfo
    {
        public virtual string TableName { get; [param: NotNull] set; }
        public virtual string SchemaName { get; [param: NotNull] set; }
        public virtual string DataType { get; [param: NotNull] set; }
        public virtual bool IsPrimaryKey { get; [param: NotNull] set; }
        public virtual bool IsNullable { get; [param: NotNull] set; }
        public virtual string Name { get; [param: NotNull] set; }
        public virtual Type ClrType { get; [param: NotNull] set; }
        public virtual string DefaultValue { get; [param: NotNull] set; }
        public virtual int Ordinal { get; [param: NotNull] set; }
    }
}
