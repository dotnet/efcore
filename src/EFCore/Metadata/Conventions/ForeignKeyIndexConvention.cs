// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates indexes on foreign key properties unless they are already covered by existing indexes or keys.
    /// </summary>
    public class ForeignKeyIndexConvention :
        IForeignKeyAddedConvention,
        IForeignKeyRemovedConvention,
        IForeignKeyPropertiesChangedConvention,
        IForeignKeyUniquenessChangedConvention,
        IKeyAddedConvention,
        IKeyRemovedConvention,
        IEntityTypeBaseTypeChangedConvention,
        IIndexAddedConvention,
        IIndexRemovedConvention,
        IIndexUniquenessChangedConvention,
        IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ForeignKeyIndexConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ForeignKeyIndexConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyAdded(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context)
        {
            var foreignKey = relationshipBuilder.Metadata;
            CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionForeignKey foreignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            OnForeignKeyRemoved(foreignKey.DeclaringEntityType, foreignKey.Properties);
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyPropertiesChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IReadOnlyList<IConventionProperty> oldDependentProperties,
            IConventionKey oldPrincipalKey,
            IConventionContext<IReadOnlyList<IConventionProperty>> context)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (!foreignKey.Properties.SequenceEqual(oldDependentProperties))
            {
                OnForeignKeyRemoved(foreignKey.DeclaringEntityType, oldDependentProperties);
                if (relationshipBuilder.Metadata.Builder != null)
                {
                    CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
                }
            }
        }

        private static void OnForeignKeyRemoved(
            IConventionEntityType declaringType,
            IReadOnlyList<IConventionProperty> foreignKeyProperties)
        {
            var index = declaringType.FindIndex(foreignKeyProperties);
            if (index == null)
            {
                return;
            }

            var otherForeignKeys = declaringType.FindForeignKeys(foreignKeyProperties).ToList();
            if (otherForeignKeys.Count != 0)
            {
                if (index.IsUnique
                    && otherForeignKeys.All(fk => !fk.IsUnique))
                {
                    index.Builder.IsUnique(false);
                }

                return;
            }

            index.DeclaringEntityType.Builder.HasNoIndex(index);
        }

        /// <inheritdoc />
        public virtual void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
        {
            var key = keyBuilder.Metadata;
            foreach (var index in key.DeclaringEntityType.GetDerivedTypesInclusive()
                .SelectMany(t => t.GetDeclaredIndexes())
                .Where(i => AreIndexedBy(i.Properties, i.IsUnique, key.Properties, true)).ToList())
            {
                RemoveIndex(index);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey key,
            IConventionContext<IConventionKey> context)
        {
            foreach (var otherForeignKey in key.DeclaringEntityType.GetDerivedTypesInclusive()
                .SelectMany(t => t.GetDeclaredForeignKeys())
                .Where(fk => AreIndexedBy(fk.Properties, fk.IsUnique, key.Properties, coveringIndexUnique: true)))
            {
                CreateIndex(otherForeignKey.Properties, otherForeignKey.IsUnique, otherForeignKey.DeclaringEntityType.Builder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (entityTypeBuilder.Metadata.BaseType != newBaseType)
            {
                return;
            }

            var baseKeys = newBaseType?.GetKeys().ToList();
            var baseIndexes = newBaseType?.GetIndexes().ToList();
            foreach (var foreignKey in entityTypeBuilder.Metadata.GetDeclaredForeignKeys()
                .Concat(entityTypeBuilder.Metadata.GetDerivedForeignKeys()))
            {
                var index = foreignKey.DeclaringEntityType.FindIndex(foreignKey.Properties);
                if (index == null)
                {
                    CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
                }
                else if (newBaseType != null)
                {
                    var coveringKey = baseKeys.FirstOrDefault(
                        k => AreIndexedBy(foreignKey.Properties, foreignKey.IsUnique, k.Properties, coveringIndexUnique: true));
                    if (coveringKey != null)
                    {
                        RemoveIndex(index);
                    }
                    else
                    {
                        var coveringIndex = baseIndexes.FirstOrDefault(
                            i => AreIndexedBy(foreignKey.Properties, foreignKey.IsUnique, i.Properties, i.IsUnique));
                        if (coveringIndex != null)
                        {
                            RemoveIndex(index);
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessIndexAdded(IConventionIndexBuilder indexBuilder, IConventionContext<IConventionIndexBuilder> context)
        {
            var index = indexBuilder.Metadata;
            foreach (var otherIndex in index.DeclaringEntityType.GetDerivedTypesInclusive()
                .SelectMany(t => t.GetDeclaredIndexes())
                .Where(i => i != index && AreIndexedBy(i.Properties, i.IsUnique, index.Properties, index.IsUnique)).ToList())
            {
                RemoveIndex(otherIndex);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessIndexRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionIndex index,
            IConventionContext<IConventionIndex> context)
        {
            foreach (var foreignKey in index.DeclaringEntityType.GetDerivedTypesInclusive()
                .SelectMany(t => t.GetDeclaredForeignKeys())
                .Where(fk => AreIndexedBy(fk.Properties, fk.IsUnique, index.Properties, index.IsUnique)))
            {
                CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyUniquenessChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
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
                        .FirstOrDefault(k => AreIndexedBy(foreignKey.Properties, false, k.Properties, coveringIndexUnique: true));
                    if (coveringKey != null)
                    {
                        RemoveIndex(index);
                        return;
                    }

                    var coveringIndex = foreignKey.DeclaringEntityType.GetIndexes()
                        .FirstOrDefault(i => AreIndexedBy(foreignKey.Properties, false, i.Properties, i.IsUnique));
                    if (coveringIndex != null)
                    {
                        RemoveIndex(index);
                        return;
                    }
                }

                index.Builder.IsUnique(foreignKey.IsUnique);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessIndexUniquenessChanged(
            IConventionIndexBuilder indexBuilder,
            IConventionContext<bool?> context)
        {
            var index = indexBuilder.Metadata;
            if (index.IsUnique)
            {
                foreach (var otherIndex in index.DeclaringEntityType.GetDerivedTypesInclusive()
                    .SelectMany(t => t.GetDeclaredIndexes())
                    .Where(i => i != index && AreIndexedBy(i.Properties, i.IsUnique, index.Properties, coveringIndexUnique: true))
                    .ToList())
                {
                    RemoveIndex(otherIndex);
                }
            }
            else
            {
                foreach (var foreignKey in index.DeclaringEntityType.GetDerivedTypesInclusive()
                    .SelectMany(t => t.GetDeclaredForeignKeys())
                    .Where(fk => fk.IsUnique && AreIndexedBy(fk.Properties, fk.IsUnique, index.Properties, coveringIndexUnique: true)))
                {
                    CreateIndex(foreignKey.Properties, foreignKey.IsUnique, foreignKey.DeclaringEntityType.Builder);
                }
            }
        }

        /// <summary>
        ///     Creates an <see cref="IConventionIndex" />.
        /// </summary>
        /// <param name="properties"> The properties that constitute the index. </param>
        /// <param name="unique"> Whether the index to create should be unique. </param>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <returns> The created index. </returns>
        protected virtual IConventionIndex CreateIndex(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            bool unique,
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder)
        {
            foreach (var key in entityTypeBuilder.Metadata.GetKeys())
            {
                if (AreIndexedBy(properties, unique, key.Properties, coveringIndexUnique: true))
                {
                    return null;
                }
            }

            foreach (var existingIndex in entityTypeBuilder.Metadata.GetIndexes())
            {
                if (AreIndexedBy(properties, unique, existingIndex.Properties, existingIndex.IsUnique))
                {
                    return null;
                }
            }

            var indexBuilder = entityTypeBuilder.HasIndex(properties);
            if (unique)
            {
                indexBuilder?.IsUnique(true);
            }

            return indexBuilder?.Metadata;
        }

        /// <summary>
        ///     Returns a value indicating whether the given properties are already covered by an existing index.
        /// </summary>
        /// <param name="properties"> The properties to check. </param>
        /// <param name="unique"> Whether the index to create should be unique. </param>
        /// <param name="coveringIndexProperties"> The properties of an existing index. </param>
        /// <param name="coveringIndexUnique"> Whether the existing index is unique. </param>
        /// <returns> <see langword="true" /> if the existing index covers the given properties. </returns>
        protected virtual bool AreIndexedBy(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            bool unique,
            [NotNull] IReadOnlyList<IConventionProperty> coveringIndexProperties,
            bool coveringIndexUnique)
            => (!unique && coveringIndexProperties.Select(p => p.Name).StartsWith(properties.Select(p => p.Name)))
                || (unique && coveringIndexUnique && coveringIndexProperties.SequenceEqual(properties));

        private static void RemoveIndex(IConventionIndex index)
            => index.DeclaringEntityType.Builder.HasNoIndex(index);

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var definition = CoreResources.LogRedundantIndexRemoved(Dependencies.Logger);
            if (!Dependencies.Logger.ShouldLog(definition)
                && !Dependencies.Logger.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                return;
            }

            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var declaredForeignKey in entityType.GetDeclaredForeignKeys())
                {
                    foreach (var key in entityType.GetKeys())
                    {
                        if (AreIndexedBy(
                            declaredForeignKey.Properties, declaredForeignKey.IsUnique, key.Properties, coveringIndexUnique: true))
                        {
                            if (declaredForeignKey.Properties.Count != key.Properties.Count)
                            {
                                Dependencies.Logger.RedundantIndexRemoved(declaredForeignKey.Properties, key.Properties);
                            }
                        }
                    }

                    foreach (var existingIndex in entityType.GetIndexes())
                    {
                        if (AreIndexedBy(
                            declaredForeignKey.Properties, declaredForeignKey.IsUnique, existingIndex.Properties, existingIndex.IsUnique))
                        {
                            if (declaredForeignKey.Properties.Count != existingIndex.Properties.Count)
                            {
                                Dependencies.Logger.RedundantIndexRemoved(declaredForeignKey.Properties, existingIndex.Properties);
                            }
                        }
                    }
                }
            }
        }
    }
}
