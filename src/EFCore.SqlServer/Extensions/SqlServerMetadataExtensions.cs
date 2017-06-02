// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for metadata.
    /// </summary>
    public static class SqlServerMetadataExtensions
    {
        /// <summary>
        ///     Gets the SQL Server specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the property. </returns>
        public static SqlServerPropertyAnnotations SqlServer([NotNull] this IMutableProperty property)
            => (SqlServerPropertyAnnotations)SqlServer((IProperty)property);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the property. </returns>
        public static ISqlServerPropertyAnnotations SqlServer([NotNull] this IProperty property)
            => new SqlServerPropertyAnnotations(Check.NotNull(property, nameof(property)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the entity. </returns>
        public static SqlServerEntityTypeAnnotations SqlServer([NotNull] this IMutableEntityType entityType)
            => (SqlServerEntityTypeAnnotations)SqlServer((IEntityType)entityType);

        /// <summary>
        ///     Gets the SQL Server specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the entity. </returns>
        public static ISqlServerEntityTypeAnnotations SqlServer([NotNull] this IEntityType entityType)
            => new SqlServerEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the key. </returns>
        public static SqlServerKeyAnnotations SqlServer([NotNull] this IMutableKey key)
            => (SqlServerKeyAnnotations)SqlServer((IKey)key);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the key. </returns>
        public static ISqlServerKeyAnnotations SqlServer([NotNull] this IKey key)
            => new SqlServerKeyAnnotations(Check.NotNull(key, nameof(key)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the index. </returns>
        public static SqlServerIndexAnnotations SqlServer([NotNull] this IMutableIndex index)
            => (SqlServerIndexAnnotations)SqlServer((IIndex)index);

        /// <summary>
        ///     Gets the SQL Server specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the index. </returns>
        public static ISqlServerIndexAnnotations SqlServer([NotNull] this IIndex index)
            => new SqlServerIndexAnnotations(Check.NotNull(index, nameof(index)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the model. </returns>
        public static SqlServerModelAnnotations SqlServer([NotNull] this IMutableModel model)
            => (SqlServerModelAnnotations)SqlServer((IModel)model);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the model. </returns>
        public static ISqlServerModelAnnotations SqlServer([NotNull] this IModel model)
            => new SqlServerModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
