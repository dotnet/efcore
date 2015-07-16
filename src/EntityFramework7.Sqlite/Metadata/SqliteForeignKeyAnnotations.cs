// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteForeignKeyAnnotations : ReadOnlySqliteForeignKeyAnnotations
    {
        public SqliteForeignKeyAnnotations([NotNull] ForeignKey foreignKey)
            : base(foreignKey)
        {
        }

        public new virtual string Name
        {
            get { return base.Name; }
            [param: CanBeNull] set { ForeignKey[SqliteNameAnnotation] = value; }
        }

        protected new virtual ForeignKey ForeignKey => (ForeignKey)base.ForeignKey;
    }
}
