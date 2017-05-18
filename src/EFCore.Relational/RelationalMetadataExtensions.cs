// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for metadata.
    /// </summary>
    public static class RelationalMetadataExtensions
    {
        /// <summary>
        ///     Gets the relational database specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The relational database specific metadata for the property. </returns>
        public static RelationalPropertyAnnotations Relational([NotNull] this IMutableProperty property)
            => (RelationalPropertyAnnotations)Relational((IProperty)property);

        /// <summary>
        ///     Gets the relational database specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The relational database specific metadata for the property. </returns>
        public static IRelationalPropertyAnnotations Relational([NotNull] this IProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)));

        /// <summary>
        ///     Gets the relational database specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The relational database specific metadata for the entity. </returns>
        public static RelationalEntityTypeAnnotations Relational([NotNull] this IMutableEntityType entityType)
            => (RelationalEntityTypeAnnotations)Relational((IEntityType)entityType);

        /// <summary>
        ///     Gets the relational database specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The relational database specific metadata for the entity. </returns>
        public static IRelationalEntityTypeAnnotations Relational([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        /// <summary>
        ///     Gets the relational database specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The relational database specific metadata for the key. </returns>
        public static RelationalKeyAnnotations Relational([NotNull] this IMutableKey key)
            => (RelationalKeyAnnotations)Relational((IKey)key);

        /// <summary>
        ///     Gets the relational database specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The relational database specific metadata for the key. </returns>
        public static IRelationalKeyAnnotations Relational([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)));

        /// <summary>
        ///     Gets the relational database specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The relational database specific metadata for the index. </returns>
        public static RelationalIndexAnnotations Relational([NotNull] this IMutableIndex index)
            => (RelationalIndexAnnotations)Relational((IIndex)index);

        /// <summary>
        ///     Gets the relational database specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The relational database specific metadata for the index. </returns>
        public static IRelationalIndexAnnotations Relational([NotNull] this IIndex index)
            => new RelationalIndexAnnotations(Check.NotNull(index, nameof(index)));

        /// <summary>
        ///     Gets the relational database specific metadata for a foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to get metadata for. </param>
        /// <returns> The relational database specific metadata for the foreign key. </returns>
        public static RelationalForeignKeyAnnotations Relational([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Relational((IForeignKey)foreignKey);

        /// <summary>
        ///     Gets the relational database specific metadata for a foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to get metadata for. </param>
        /// <returns> The relational database specific metadata for the foreign key. </returns>
        public static IRelationalForeignKeyAnnotations Relational([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)));

        /// <summary>
        ///     Gets the relational database specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The relational database specific metadata for the model. </returns>
        public static RelationalModelAnnotations Relational([NotNull] this IMutableModel model)
            => (RelationalModelAnnotations)Relational((IModel)model);

        /// <summary>
        ///     Gets the relational database specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The relational database specific metadata for the model. </returns>
        public static IRelationalModelAnnotations Relational([NotNull] this IModel model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
