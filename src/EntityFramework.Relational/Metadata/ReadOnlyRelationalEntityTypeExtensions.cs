// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class ReadOnlyRelationalEntityTypeExtensions : IRelationalEntityTypeExtensions
    {
        protected const string RelationalTableAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.TableName;
        protected const string RelationalSchemaAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Schema;

        private readonly IEntityType _entityType;

        public ReadOnlyRelationalEntityTypeExtensions([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entityType = entityType;
        }

        public virtual string Table
        {
            get { return _entityType[RelationalTableAnnotation] ?? _entityType.SimpleName; }
        }

        public virtual string Schema
        {
            get { return _entityType[RelationalSchemaAnnotation]; }
        }

        protected virtual IEntityType EntityType
        {
            get { return _entityType; }
        }
    }
}
