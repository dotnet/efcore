// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerEntityTypeAnnotations : RelationalEntityTypeAnnotations, ISqlServerEntityTypeAnnotations
    {
        public SqlServerEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        public SqlServerEntityTypeAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool IsMemoryOptimized
        {
            get => Annotations.Metadata[SqlServerAnnotationNames.MemoryOptimized] as bool? ?? false;
            set => SetIsMemoryOptimized(value);
        }

        protected virtual bool SetIsMemoryOptimized(bool value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.MemoryOptimized, value);
    }
}
