// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteIndexExtensions : ReadOnlySqliteIndexExtensions
    {
        public SqliteIndexExtensions([NotNull] Index index)
            : base(index)
        {
        }

        public virtual new string Name
        {
            get { return base.Name; }
            [param: CanBeNull]
            set { Index[SqliteNameAnnotation] = value; }
        }

        protected virtual new Index Index => (Index)base.Index;
    }
}
