// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerEntityTypeExtensions : ISqlServerEntityTypeExtensions
    {
        protected const string SqlServerTableAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.TableName;
        protected const string SqlServerSchemaAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Schema;

        public ReadOnlySqlServerEntityTypeExtensions([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
        }

        public virtual string Table
            => EntityType[SqlServerTableAnnotation] as string
               ?? EntityType.Relational().Table;

        public virtual string Schema
            => EntityType[SqlServerSchemaAnnotation] as string
               ?? EntityType.Relational().Schema;

        protected virtual IEntityType EntityType { get; }
    }
}
