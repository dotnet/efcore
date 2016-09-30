// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite specific extension methods for metadata.
    /// </summary>
    public static class SqliteMetadataExtensions
    {
        /// <summary>
        ///     Gets the SQLite specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the entity. </returns>
        public static IRelationalEntityTypeAnnotations Sqlite([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), SqliteFullAnnotationNames.Instance);

        /// <summary>
        ///     Gets the SQLite specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the entity. </returns>
        public static RelationalEntityTypeAnnotations Sqlite([NotNull] this IMutableEntityType entityType)
            => (RelationalEntityTypeAnnotations)Sqlite((IEntityType)entityType);

        /// <summary>
        ///     Gets the SQLite specific metadata for a foreign key.
        /// </summary>
        /// <param name="foreignKey"> The entity to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the foreign key. </returns>
        public static IRelationalForeignKeyAnnotations Sqlite([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), SqliteFullAnnotationNames.Instance);

        /// <summary>
        ///     Gets the SQLite specific metadata for a foreign key.
        /// </summary>
        /// <param name="foreignKey"> The entity to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the foreign key. </returns>
        public static RelationalForeignKeyAnnotations Sqlite([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Sqlite((IForeignKey)foreignKey);

        /// <summary>
        ///     Gets the SQLite specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the index. </returns>
        public static IRelationalIndexAnnotations Sqlite([NotNull] this IIndex index)
            => new RelationalIndexAnnotations(Check.NotNull(index, nameof(index)), SqliteFullAnnotationNames.Instance);

        /// <summary>
        ///     Gets the SQLite specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the index. </returns>
        public static RelationalIndexAnnotations Sqlite([NotNull] this IMutableIndex index)
            => (RelationalIndexAnnotations)Sqlite((IIndex)index);

        /// <summary>
        ///     Gets the SQLite specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the key. </returns>
        public static IRelationalKeyAnnotations Sqlite([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)), SqliteFullAnnotationNames.Instance);

        /// <summary>
        ///     Gets the SQLite specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the key. </returns>
        public static RelationalKeyAnnotations Sqlite([NotNull] this IMutableKey key)
            => (RelationalKeyAnnotations)Sqlite((IKey)key);

        /// <summary>
        ///     Gets the SQLite specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the model. </returns>
        public static IRelationalModelAnnotations Sqlite([NotNull] this IModel model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)), SqliteFullAnnotationNames.Instance);

        /// <summary>
        ///     Gets the SQLite specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the model. </returns>
        public static RelationalModelAnnotations Sqlite([NotNull] this IMutableModel model)
            => (RelationalModelAnnotations)Sqlite((IModel)model);

        /// <summary>
        ///     Gets the SQLite specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the property. </returns>
        public static IRelationalPropertyAnnotations Sqlite([NotNull] this IProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)), SqliteFullAnnotationNames.Instance);

        /// <summary>
        ///     Gets the SQLite specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the property. </returns>
        public static RelationalPropertyAnnotations Sqlite([NotNull] this IMutableProperty property)
            => (RelationalPropertyAnnotations)Sqlite((IProperty)property);
    }
}
