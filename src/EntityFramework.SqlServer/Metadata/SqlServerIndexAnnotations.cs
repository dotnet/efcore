// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerIndexAnnotations : RelationalIndexAnnotations, ISqlServerIndexAnnotations
    {
        public SqlServerIndexAnnotations([NotNull] IIndex index)
            : base(index, SqlServerAnnotationNames.Prefix)
        {
        }

        public SqlServerIndexAnnotations(
            [NotNull] InternalIndexBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(internalBuilder, configurationSource, SqlServerAnnotationNames.Prefix)
        {
        }

        public virtual bool? IsClustered
        {
            get { return (bool?)GetAnnotation(SqlServerAnnotationNames.Clustered); }
            [param: CanBeNull] set { SetAnnotation(SqlServerAnnotationNames.Clustered, value); }
        }
    }
}
