// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class ColumnModel : Annotatable
    {
        public virtual TableModel Table { get; [param: NotNull] set; }
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
