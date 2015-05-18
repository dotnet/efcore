// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerIndexExtensions : ISqlServerIndexExtensions
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Name;
        protected const string SqlServerClusteredAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered;

        public ReadOnlySqlServerIndexExtensions([NotNull] IIndex index)
        {
            Check.NotNull(index, nameof(index));

            Index = index;
        }

        public virtual string Name
            => Index[SqlServerNameAnnotation] as string
               ?? Index.Relational().Name;

        public virtual bool? IsClustered
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                var value = Index[SqlServerClusteredAnnotation] as string;
                return value == null ? null : (bool?)bool.Parse(value);
            }
        }

        protected virtual IIndex Index { get; }
    }
}
