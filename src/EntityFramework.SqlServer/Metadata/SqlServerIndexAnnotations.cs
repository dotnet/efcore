// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerIndexAnnotations : RelationalIndexAnnotations, ISqlServerIndexAnnotations
    {
        public SqlServerIndexAnnotations([NotNull] IIndex index)
            : base(index, SqlServerAnnotationNames.Prefix)
        {
        }

        protected SqlServerIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool? IsClustered
        {
            get { return (bool?)Annotations.GetAnnotation(SqlServerAnnotationNames.Clustered); }
            [param: CanBeNull] set { SetIsClustered(value); }
        }

        protected bool SetIsClustered(bool? value) => Annotations.SetAnnotation(SqlServerAnnotationNames.Clustered, value);
    }
}
