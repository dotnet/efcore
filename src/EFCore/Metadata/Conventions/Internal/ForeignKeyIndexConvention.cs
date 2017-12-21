// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ForeignKeyIndexConvention :
        IForeignKeyAddedConvention,
        IForeignKeyRemovedConvention,
        IForeignKeyUniquenessChangedConvention,
        IKeyAddedConvention,
        IKeyRemovedConvention,
        IBaseTypeChangedConvention,
        IIndexAddedConvention,
        IIndexRemovedConvention,
        IIndexUniquenessChangedConvention
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ForeignKeyIndexConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            _logger = logger;
        }

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
                RemoveIndex(index, key.Properties);
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
                .Where(fk => AreIndexedBy(fk.Properties, fk.IsUnique, key.Properties, existingIndexUniqueness: true)))
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
                if (index == null)
                {
                    CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
                }
                else if (baseType != null)
                {
                    var coveringKey = baseKeys.FirstOrDefault(
                        k => AreIndexedBy(foreignKey.Properties, foreignKey.IsUnique, k.Properties, existingIndexUniqueness: true));
                    if (coveringKey != null)
                    {
                        RemoveIndex(index, coveringKey.Properties);
                    }
                    else
                    {
                        var coveringIndex = baseIndexes.FirstOrDefault(
                            i => AreIndexedBy(foreignKey.Properties, foreignKey.IsUnique, i.Properties, i.IsUnique));
                        if (coveringIndex != null)
                        {
                            RemoveIndex(index, coveringIndex.Properties);
                        }
                    }
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
                RemoveIndex(otherIndex, index.Properties);
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
        InternalRelationshipBuilder IForeignKeyUniquenessChangedConvention.Apply(InternalRelationshipBuilder relationshipBuilder)
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
                if (!foreignKey.IsUnique)
                {
                    var coveringKey = foreignKey.DeclaringEntityType.GetKeys()
                        .FirstOrDefault(k => AreIndexedBy(foreignKey.Properties, false, k.Properties, existingIndexUniqueness: true));
                    if (coveringKey != null)
                    {
                        RemoveIndex(index, coveringKey.Properties);
                        return relationshipBuilder;
                    }

                    var coveringIndex = foreignKey.DeclaringEntityType.GetIndexes()
                        .FirstOrDefault(i => AreIndexedBy(foreignKey.Properties, false, i.Properties, i.IsUnique));
                    if (coveringIndex != null)
                    {
                        RemoveIndex(index, coveringIndex.Properties);
                        return relationshipBuilder;
                    }
                }

                index.Builder.IsUnique(foreignKey.IsUnique, ConfigurationSource.Convention);
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        bool IIndexUniquenessChangedConvention.Apply(InternalIndexBuilder indexBuilder)
        {
            var index = indexBuilder.Metadata;
            if (index.IsUnique)
            {
                foreach (var otherIndex in index.DeclaringEntityType.GetDerivedIndexesInclusive()
                    .Where(i => i != index && AreIndexedBy(i.Properties, i.IsUnique, index.Properties, existingIndexUniqueness: true)).ToList())
                {
                    RemoveIndex(otherIndex, index.Properties);
                }
            }
            else
            {
                foreach (var foreignKey in index.DeclaringEntityType.GetDerivedForeignKeysInclusive()
                    .Where(fk => fk.IsUnique && AreIndexedBy(fk.Properties, fk.IsUnique, index.Properties, existingIndexUniqueness: true)))
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
                .Any(key => AreIndexedBy(properties, unique, key.Properties, existingIndexUniqueness: true)))
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
            => (!unique && existingIndexProperties.Select(p => p.Name).StartsWith(properties.Select(p => p.Name)))
               || (unique && existingIndexUniqueness && existingIndexProperties.SequenceEqual(properties));

        private void RemoveIndex(Index index, IReadOnlyList<IProperty> coveringProperties)
        {
            if (index.Properties.Count != coveringProperties.Count)
            {
                _logger.RedundantIndexRemoved(index.Properties, coveringProperties);
            }
            index.DeclaringEntityType.Builder.RemoveIndex(index, ConfigurationSource.Convention);
        }
    }
}
