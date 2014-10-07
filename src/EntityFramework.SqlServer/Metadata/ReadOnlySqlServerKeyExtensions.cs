// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerKeyExtensions : ReadOnlyRelationalKeyExtensions, ISqlServerKeyExtensions
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Name;
        protected const string SqlServerClusteredAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered;

        public ReadOnlySqlServerKeyExtensions([NotNull] IKey key)
            : base(key)
        {
        }

        public override string Name
        {
            get { return Key[SqlServerNameAnnotation] ?? base.Name; }
        }

        public virtual bool? IsClustered
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                // TODO: Issue #700: Annotate associated index object instead
                var value = Key[SqlServerClusteredAnnotation];
                return value == null ? null : (bool?)bool.Parse(value);
            }
        }
    }
}
