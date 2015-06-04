// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class ReadOnlySqliteIndexExtensions : ReadOnlyRelationalIndexExtensions, ISqliteIndexExtensions
    {
        protected const string SqliteNameAnnotation = SqliteAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public ReadOnlySqliteIndexExtensions([NotNull] IIndex index)
            : base(index)
        {
        }

        public override string Name => Index[SqliteNameAnnotation] as string ?? base.Name;
    }
}
