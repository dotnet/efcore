// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class ReadOnlySqliteKeyAnnotations : ReadOnlyRelationalKeyAnnotations, ISqliteKeyAnnotations
    {
        protected const string SqliteNameAnnotation = SqliteAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public ReadOnlySqliteKeyAnnotations([NotNull] IKey key)
            : base(key)
        {
        }

        public override string Name => Key[SqliteNameAnnotation] as string ?? base.Name;
    }
}
