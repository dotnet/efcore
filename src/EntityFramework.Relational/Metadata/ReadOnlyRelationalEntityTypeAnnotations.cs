// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ReadOnlyRelationalEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        protected const string RelationalTableAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.TableName;
        protected const string RelationalSchemaAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Schema;
        protected internal const string DiscriminatorPropertyAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.DiscriminatorProperty;
        protected const string DiscriminatorValueAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.DiscriminatorValue;

        private readonly IEntityType _entityType;

        public ReadOnlyRelationalEntityTypeAnnotations([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            _entityType = entityType;
        }

        public virtual string Table
            => _entityType.RootType()[RelationalTableAnnotation] as string
               ?? _entityType.RootType().DisplayName();

        public virtual string Schema
            => _entityType.RootType()[RelationalSchemaAnnotation] as string;

        public virtual IProperty DiscriminatorProperty
            => _entityType.RootType()
                .GetProperty(
                    (string)_entityType.RootType()
                        .GetAnnotation(DiscriminatorPropertyAnnotation).Value);

        public virtual string DiscriminatorValue
            => _entityType[DiscriminatorValueAnnotation] as string
               ?? _entityType.DisplayName();

        protected virtual IEntityType EntityType => _entityType;
    }
}
