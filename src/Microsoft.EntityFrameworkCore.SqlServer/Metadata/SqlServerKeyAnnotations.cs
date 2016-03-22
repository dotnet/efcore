// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerKeyAnnotations : RelationalKeyAnnotations, ISqlServerKeyAnnotations
    {
        public SqlServerKeyAnnotations([NotNull] IKey key)
            : base(key, SqlServerFullAnnotationNames.Instance)
        {
        }

        protected SqlServerKeyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, SqlServerFullAnnotationNames.Instance)
        {
        }

        public virtual bool? IsClustered
        {
            get { return (bool?)Annotations.GetAnnotation(SqlServerFullAnnotationNames.Instance.Clustered, null); }
            [param: CanBeNull] set { SetIsClustered(value); }
        }

        protected virtual bool SetIsClustered(bool? value)
            => Annotations.SetAnnotation(SqlServerFullAnnotationNames.Instance.Clustered, null, value);
    }
}
