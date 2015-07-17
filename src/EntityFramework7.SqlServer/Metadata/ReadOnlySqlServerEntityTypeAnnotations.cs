// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerEntityTypeAnnotations : ReadOnlyRelationalEntityTypeAnnotations, ISqlServerEntityTypeAnnotations
    {
        protected const string SqlServerTableAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.TableName;
        protected const string SqlServerSchemaAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Schema;

        public ReadOnlySqlServerEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        public override string TableName
            => EntityType[SqlServerTableAnnotation] as string
               ?? base.TableName;

        public override string Schema
            => EntityType[SqlServerSchemaAnnotation] as string
               ?? base.Schema;
    }
}
