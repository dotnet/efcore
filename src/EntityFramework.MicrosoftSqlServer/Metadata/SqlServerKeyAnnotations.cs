// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata
{
    public class SqlServerKeyAnnotations : RelationalKeyAnnotations, ISqlServerKeyAnnotations
    {
        public SqlServerKeyAnnotations([NotNull] IKey key)
            : base(key, SqlServerAnnotationNames.Prefix)
        {
        }

        protected SqlServerKeyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool? IsClustered
        {
            get { return Key.Index.SqlServer().IsClustered; }
            [param: CanBeNull] set { ((SqlServerIndexAnnotations)(Key.Index.SqlServer())).IsClustered = value; }
        }
    }
}
