// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SharedTableConvention :
        IEntityTypeConvention,
        IEntityTypeAnnotationSetConvention,
        IForeignKeyOwnershipConvention,
        IForeignKeyUniquenessConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SharedTableConvention([NotNull] IRelationalAnnotationProvider annotationProvider)
        {
            AnnotationProvider = annotationProvider;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IRelationalAnnotationProvider AnnotationProvider { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var ownership = entityTypeBuilder.Metadata.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership && fk.IsUnique);
            if (ownership != null)
            {
                SetOwnedTable(ownership);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation Apply(
            InternalEntityTypeBuilder entityTypeBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            var entityType = entityTypeBuilder.Metadata;
            var providerAnnotations = (AnnotationProvider.For(entityType) as RelationalEntityTypeAnnotations)?.ProviderFullAnnotationNames;
            if (name == RelationalFullAnnotationNames.Instance.TableName
                || name == RelationalFullAnnotationNames.Instance.Schema
                || (providerAnnotations != null
                    && (name == providerAnnotations.TableName
                        || name == providerAnnotations.Schema)))
            {
                foreach (var foreignKey in entityType.GetReferencingForeignKeys())
                {
                    if (foreignKey.IsOwnership
                        && foreignKey.IsUnique)
                    {
                        SetOwnedTable(foreignKey);
                    }
                }
            }

            return annotation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (foreignKey.IsOwnership
                && foreignKey.IsUnique)
            {
                SetOwnedTable(foreignKey);
            }

            return relationshipBuilder;
        }

        private void SetOwnedTable(ForeignKey foreignKey)
        {
            var ownerType = foreignKey.PrincipalEntityType;
            foreignKey.DeclaringEntityType.Builder.Relational(ConfigurationSource.Convention)
                .ToTable(AnnotationProvider.For(ownerType).TableName, AnnotationProvider.For(ownerType).Schema);
        }
    }
}
