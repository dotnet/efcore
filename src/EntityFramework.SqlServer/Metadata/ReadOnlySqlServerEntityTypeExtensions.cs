// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerEntityTypeExtensions : ReadOnlyRelationalEntityTypeExtensions, ISqlServerEntityTypeExtensions
    {
        protected const string SqlServerTableAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.TableName;
        protected const string SqlServerSchemaAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Schema;

        public ReadOnlySqlServerEntityTypeExtensions([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        public override string Table
        {
            get { return EntityType[SqlServerTableAnnotation] ?? base.Table; }
        }

        public override string Schema
        {
            get { return EntityType[SqlServerSchemaAnnotation] ?? base.Schema; }
        }
    }
}
