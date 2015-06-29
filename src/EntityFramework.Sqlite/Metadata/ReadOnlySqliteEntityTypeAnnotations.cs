// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class ReadOnlySqliteEntityTypeAnnotations : ReadOnlyRelationalEntityTypeAnnotations, ISqliteEntityTypeAnnotations
    {
        protected const string SqliteTableAnnotation = SqliteAnnotationNames.Prefix + RelationalAnnotationNames.TableName;
        protected const string SqliteSchemaAnnotation = SqliteAnnotationNames.Prefix + RelationalAnnotationNames.Schema;

        public ReadOnlySqliteEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        public override string Table => EntityType[SqliteTableAnnotation] as string ?? base.Table;
        public override string Schema => null;
    }
}
