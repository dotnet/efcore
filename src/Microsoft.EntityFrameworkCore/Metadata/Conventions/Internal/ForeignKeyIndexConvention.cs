// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class ForeignKeyIndexConvention : IForeignKeyConvention, IForeignKeyRemovedConvention, IKeyConvention, IKeyRemovedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            var newIndex = CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);

            if (newIndex != null)
            {
                foreach (var index in newIndex.DeclaringEntityType.GetDerivedIndexesInclusive()
                    .Where(i => i != newIndex && AreIndexedBy(i.Properties, i.IsUnique, newIndex.Properties, newIndex.IsUnique)).ToList())
                {
                    index.DeclaringEntityType.Builder.RemoveIndex(index, ConfigurationSource.Convention);
                }
            }

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
            foreach (var otherForeignKey in foreignKey.DeclaringEntityType.GetDerivedForeignKeysInclusive()
                .Where(fk => AreIndexedBy(fk.Properties, fk.IsUnique, foreignKey.Properties, foreignKey.IsUnique)))
            {
                CreateIndex(otherForeignKey.Properties, otherForeignKey.IsUnique, otherForeignKey.DeclaringEntityType.Builder);
            }
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

        private static bool AreIndexedBy(
            IReadOnlyList<Property> properties, bool unique, IReadOnlyList<Property> existingIndexProperties, bool existingIndexUniqueness)
            => (!unique || existingIndexUniqueness) && existingIndexProperties.Select(p => p.Name).StartsWith(properties.Select(p => p.Name));
    }
}
