// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerIndexAnnotations : RelationalIndexAnnotations, ISqlServerIndexAnnotations
    {
        public SqlServerIndexAnnotations([NotNull] IIndex index)
            : base(index, SqlServerFullAnnotationNames.Instance)
        {
        }

        protected SqlServerIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, SqlServerFullAnnotationNames.Instance)
        {
        }

        public virtual bool? IsClustered
        {
            get { return (bool?)Annotations.GetAnnotation(SqlServerFullAnnotationNames.Instance.Clustered, null); }
            [param: CanBeNull] set { SetIsClustered(value); }
        }

        protected virtual bool SetIsClustered(bool? value) => Annotations.SetAnnotation(
            SqlServerFullAnnotationNames.Instance.Clustered,
            null,
            value);
    }
}
