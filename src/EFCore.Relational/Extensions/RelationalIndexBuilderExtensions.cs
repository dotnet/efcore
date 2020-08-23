// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class RelationalIndexBuilderExtensions
    {
        /// <summary>
        ///     Configures the name of the index in the database when targeting a relational database.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder HasDatabaseName([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string name)
        {
            indexBuilder.Metadata.SetDatabaseName(name);

            return indexBuilder;
        }

        /// <summary>
        ///     Configures the name of the index in the database when targeting a relational database.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use HasDatabaseName() instead.")]
        public static IndexBuilder HasName([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string name)
            => HasDatabaseName(indexBuilder, name);

        /// <summary>
        ///     Configures the name of the index in the database when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> HasDatabaseName<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder,
            [CanBeNull] string name)
        {
            indexBuilder.Metadata.SetDatabaseName(name);

            return indexBuilder;
        }

        /// <summary>
        ///     Configures the name of the index in the database when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use HasDatabaseName() instead.")]
        public static IndexBuilder<TEntity> HasName<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, [CanBeNull] string name)
            => indexBuilder.HasDatabaseName(name);

        /// <summary>
        ///     Configures the name of the index in the database when targeting a relational database.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionIndexBuilder HasDatabaseName(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetDatabaseName(name, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetDatabaseName(name, fromDataAnnotation);
                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Configures the name of the index in the database when targeting a relational database.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        [Obsolete("Use HasDatabaseName() instead.")]
        public static IConventionIndexBuilder HasName(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
            => indexBuilder.HasDatabaseName(name, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the given name can be set for the index.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given name can be set for the index. </returns>
        public static bool CanSetDatabaseName(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
            => indexBuilder.CanSetAnnotation(RelationalAnnotationNames.Name, name, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the given name can be set for the index.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given name can be set for the index. </returns>
        [Obsolete("Use CanSetDatabaseName() instead.")]
        public static bool CanSetName(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
            => CanSetDatabaseName(indexBuilder, name, fromDataAnnotation);

        /// <summary>
        ///     Configures the filter expression for the index.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="sql"> The filter expression for the index. </param>
        /// <returns>A builder to further configure the index. </returns>
        public static IndexBuilder HasFilter([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string sql)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            indexBuilder.Metadata.SetFilter(sql);

            return indexBuilder;
        }

        /// <summary>
        ///     Configures the filter expression for the index.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="sql"> The filter expression for the index. </param>
        /// <returns>A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> HasFilter<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, [CanBeNull] string sql)
            => (IndexBuilder<TEntity>)HasFilter((IndexBuilder)indexBuilder, sql);

        /// <summary>
        ///     Configures the filter expression for the index.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="sql"> The filter expression for the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionIndexBuilder HasFilter(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetFilter(sql, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetFilter(sql, fromDataAnnotation);
                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given expression can be set as the filter for the index.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="sql"> The filter expression for the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given name can be set for the index. </returns>
        public static bool CanSetFilter(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
            => indexBuilder.CanSetAnnotation(RelationalAnnotationNames.Filter, sql, fromDataAnnotation);
    }
}
