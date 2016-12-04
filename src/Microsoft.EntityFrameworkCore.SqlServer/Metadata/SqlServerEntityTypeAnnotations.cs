// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerEntityTypeAnnotations : RelationalEntityTypeAnnotations, ISqlServerEntityTypeAnnotations
    {
        public SqlServerEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType, SqlServerFullAnnotationNames.Instance)
        {
        }

        public SqlServerEntityTypeAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, SqlServerFullAnnotationNames.Instance)
        {
        }

        public virtual bool IsMemoryOptimized
        {
            get { return Annotations.GetAnnotation(SqlServerFullAnnotationNames.Instance.MemoryOptimized, null) as bool? ?? false; }
            set { SetIsMemoryOptimized(value); }
        }

        protected virtual bool SetIsMemoryOptimized(bool value)
            => Annotations.SetAnnotation(SqlServerFullAnnotationNames.Instance.MemoryOptimized, null, value);
    }
}
