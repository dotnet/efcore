// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class ReadOnlySqliteForeignKeyAnnotations : ReadOnlyRelationalForeignKeyAnnotations, ISqliteForeignKeyAnnotations
    {
        protected const string SqliteNameAnnotation = SqliteAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public ReadOnlySqliteForeignKeyAnnotations([NotNull] IForeignKey foreignKey)
            : base(foreignKey)
        {
        }

        public override string Name => ForeignKey[SqliteNameAnnotation] as string ?? base.Name;
    }
}
