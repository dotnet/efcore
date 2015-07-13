// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteKeyAnnotations : ReadOnlySqliteKeyAnnotations
    {
        public SqliteKeyAnnotations([NotNull] Key key)
            : base(key)
        {
        }

        public new virtual string Name
        {
            get { return base.Name; }
            [param: CanBeNull] set { Key[SqliteNameAnnotation] = value; }
        }

        protected new virtual Key Key => (Key)base.Key;
    }
}
