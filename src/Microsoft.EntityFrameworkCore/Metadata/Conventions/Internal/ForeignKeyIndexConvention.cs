// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ForeignKeyIndexConvention :
        IForeignKeyConvention,
        IForeignKeyRemovedConvention,
        IForeignKeyUniquenessConvention,
        IKeyConvention,
        IKeyRemovedConvention,
        IBaseTypeConvention,
        IIndexConvention,
        IIndexRemovedConvention,
        IIndexUniquenessConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            var index = foreignKey.DeclaringEntityType.FindIndex(foreignKey.Properties);
            if (index == null)
            {
                return;
            }

            var otherForeignKeys = foreignKey.DeclaringEntityType.FindForeignKeys(foreignKey.Properties).ToList();
            if (otherForeignKeys.Count != 0)
            {
                if (index.IsUnique
                    && otherForeignKeys.All(fk => !fk.IsUnique))
                {
                    index.Builder.IsUnique(false, ConfigurationSource.Convention);
                }
                return;
            }

            index.DeclaringEntityType.Builder.RemoveIndex(index, ConfigurationSource.Convention);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            var key = keyBuilder.Metadata;
            foreach (var index in key.DeclaringEntityType.GetDerivedIndexesInclusive()
                .Where(i => AreIndexedBy(i.Properties, i.IsUnique, key.Properties, true)).ToList())
            {
                index.DeclaringEntityType.Builder.RemoveIndex(index, ConfigurationSource.Convention);
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, Key key)
        {
            foreach (var otherForeignKey in key.DeclaringEntityType.GetDerivedForeignKeysInclusive()
                .Where(fk => AreIndexedBy(fk.Properties, fk.IsUnique, key.Properties, true)))
            {
                CreateIndex(otherForeignKey.Properties, otherForeignKey.IsUnique, otherForeignKey.DeclaringEntityType.Builder);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            var baseType = entityTypeBuilder.Metadata.BaseType;
            var baseKeys = baseType?.GetKeys().ToList();
            var baseIndexes = baseType?.GetIndexes().ToList();
            foreach (var foreignKey in entityTypeBuilder.Metadata.GetDerivedForeignKeysInclusive())
            {
                var index = foreignKey.DeclaringEntityType.FindIndex(foreignKey.Properties);
                if (baseType != null
                    && index != null
                    && (baseKeys.Any(k => AreIndexedBy(foreignKey.Properties, foreignKey.IsUnique, k.Properties, true))
                        || baseIndexes.Any(i => AreIndexedBy(foreignKey.Properties, foreignKey.IsUnique, i.Properties, i.IsUnique))))
                {
                    index.DeclaringEntityType.Builder.RemoveIndex(index, ConfigurationSource.Convention);
                }
                else if (index == null)
                {
                    CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder Apply(InternalIndexBuilder indexBuilder)
        {
            var index = indexBuilder.Metadata;
            foreach (var otherIndex in index.DeclaringEntityType.GetDerivedIndexesInclusive()
                .Where(i => i != index && AreIndexedBy(i.Properties, i.IsUnique, index.Properties, index.IsUnique)).ToList())
            {
                otherIndex.DeclaringEntityType.Builder.RemoveIndex(otherIndex, ConfigurationSource.Convention);
            }

            return indexBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, Index index)
        {
            foreach (var foreignKey in index.DeclaringEntityType.GetDerivedForeignKeysInclusive()
                .Where(fk => AreIndexedBy(fk.Properties, fk.IsUnique, index.Properties, index.IsUnique)))
            {
                CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        bool IForeignKeyUniquenessConvention.Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            var index = foreignKey.DeclaringEntityType.FindIndex(foreignKey.Properties);
            if (index == null)
            {
                if (foreignKey.IsUnique)
                {
                    CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
                }
            }
            else
            {
                if (!foreignKey.IsUnique
                    && (foreignKey.DeclaringEntityType.GetKeys()
                        .Any(k => AreIndexedBy(foreignKey.Properties, false, k.Properties, true))
                        || foreignKey.DeclaringEntityType.GetIndexes()
                            .Any(i => AreIndexedBy(foreignKey.Properties, false, i.Properties, i.IsUnique))))
                {
                    index.DeclaringEntityType.Builder.RemoveIndex(index, ConfigurationSource.Convention);
                }
                else
                {
                    index.IsUnique = foreignKey.IsUnique;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        bool IIndexUniquenessConvention.Apply(InternalIndexBuilder indexBuilder)
        {
            var index = indexBuilder.Metadata;
            if (index.IsUnique)
            {
                foreach (var otherIndex in index.DeclaringEntityType.GetDerivedIndexesInclusive()
                    .Where(i => i != index && AreIndexedBy(i.Properties, i.IsUnique, index.Properties, true)).ToList())
                {
                    otherIndex.DeclaringEntityType.Builder.RemoveIndex(otherIndex, ConfigurationSource.Convention);
                }
            }
            else
            {
                foreach (var foreignKey in index.DeclaringEntityType.GetDerivedForeignKeysInclusive()
                    .Where(fk => fk.IsUnique && AreIndexedBy(fk.Properties, fk.IsUnique, index.Properties, true)))
                {
                    CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Index CreateIndex(
            [NotNull] IReadOnlyList<Property> properties, bool unique, [NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.GetKeys()
                .Any(key => key.Properties.StartsWith(properties)))
            {
                return null;
            }

            if (entityTypeBuilder.Metadata.GetIndexes()
                .Any(existingIndex => AreIndexedBy(properties, unique, existingIndex.Properties, existingIndex.IsUnique)))
            {
                return null;
            }

            var indexBuilder = entityTypeBuilder.HasIndex(properties, ConfigurationSource.Convention);
            if (unique)
            {
                indexBuilder?.IsUnique(true, ConfigurationSource.Convention);
            }
            return indexBuilder?.Metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool AreIndexedBy(
            [NotNull] IReadOnlyList<Property> properties,
            bool unique,
            [NotNull] IReadOnlyList<Property> existingIndexProperties,
            bool existingIndexUniqueness)
            => (!unique || existingIndexUniqueness) && existingIndexProperties.Select(p => p.Name).StartsWith(properties.Select(p => p.Name));
    }
}
