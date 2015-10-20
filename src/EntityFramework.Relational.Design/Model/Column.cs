// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Model
{
    public class Column
    {
        public virtual Table Table { get; [param: NotNull] set; }
        public virtual string Name { get; [param: NotNull] set; }
        public virtual int? PrimaryKeyOrdinal { get; [param: CanBeNull] set; }
        public virtual int Ordinal { get; [param: NotNull] set; }
        public virtual bool IsNullable { get; [param: NotNull] set; }

        [CanBeNull]
        public virtual string DataType { get; [param: CanBeNull] set; }

        [CanBeNull]
        public virtual string DefaultValue { get; [param: CanBeNull] set; }

        public virtual int? MaxLength { get; [param: CanBeNull] set; }
        public virtual int? Precision { get; [param: CanBeNull] set; }
        public virtual int? Scale { get; [param: CanBeNull] set; }
        public virtual bool? IsStoreGenerated { get; [param: CanBeNull] set; }
        public virtual bool? IsComputed { get; [param: CanBeNull] set; }
        // SQL Server
        public virtual bool? IsIdentity { get; [param: CanBeNull] set; }
    }
}
