// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class ColumnModel : Annotatable
    {
        public virtual TableModel Table { get; [param: NotNull] set; }
        public virtual string Name { get; [param: NotNull] set; }
        public virtual int? PrimaryKeyOrdinal { get; [param: CanBeNull] set; }
        public virtual int Ordinal { get; [param: NotNull] set; }
        public virtual bool IsNullable { get; [param: NotNull] set; }

        public virtual string DataType { get; [param: CanBeNull] set; }

        public virtual string DefaultValue { get; [param: CanBeNull] set; }

        public virtual string ComputedValue { get; [param: CanBeNull] set; }
        public virtual int? MaxLength { get; [param: CanBeNull] set; }
        public virtual int? Precision { get; [param: CanBeNull] set; }
        public virtual int? Scale { get; [param: CanBeNull] set; }
        public virtual ValueGenerated? ValueGenerated { get; set; }

        public virtual string DisplayName
        {
            get
            {
                var tablePrefix = Table?.DisplayName;
                return (!string.IsNullOrEmpty(tablePrefix) ? tablePrefix + "." : "") + Name;
            }
        }
    }
}
