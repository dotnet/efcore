// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite-specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class SqlitePropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="srid"> The SRID. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasSrid([NotNull] this PropertyBuilder propertyBuilder, int srid)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.SetSrid(srid);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="srid"> The SRID. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasSrid<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            int srid)
            => (PropertyBuilder<TProperty>)HasSrid((PropertyBuilder)propertyBuilder, srid);

        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="srid"> The SRID. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasSrid(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            int? srid,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetSrid(srid, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetSrid(srid, fromDataAnnotation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as the SRID for the column.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="srid"> The SRID. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given value can be set as the SRID for the column. </returns>
        public static bool CanSetSrid(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            int? srid,
            bool fromDataAnnotation = false)
            => Check.NotNull(propertyBuilder, nameof(propertyBuilder)).CanSetAnnotation(
                SqliteAnnotationNames.Srid,
                srid,
                fromDataAnnotation);

        /// <summary>
        ///     Configures the dimension of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="dimension"> The dimension. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasGeometricDimension(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string dimension,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetGeometricDimension(dimension, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetGeometricDimension(dimension, fromDataAnnotation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as the dimension for the column.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="dimension"> The dimension. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given value can be set as the dimension for the column. </returns>
        public static bool CanSetGeometricDimension(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string dimension,
            bool fromDataAnnotation = false)
            => Check.NotNull(propertyBuilder, nameof(propertyBuilder)).CanSetAnnotation(
                SqliteAnnotationNames.Srid,
                dimension,
                fromDataAnnotation);

        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="srid"> The SRID. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        [Obsolete("Use HasSrid")]
        public static PropertyBuilder ForSqliteHasSrid([NotNull] this PropertyBuilder propertyBuilder, int srid)
            => propertyBuilder.HasSrid(srid);

        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="srid"> The SRID. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        [Obsolete("Use HasSrid")]
        public static PropertyBuilder<TProperty> ForSqliteHasSrid<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            int srid)
            => propertyBuilder.HasSrid(srid);

        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="srid"> The SRID. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use HasSrid")]
        public static IConventionPropertyBuilder ForSqliteHasSrid(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            int? srid,
            bool fromDataAnnotation = false)
            => propertyBuilder.HasSrid(srid, fromDataAnnotation);

        /// <summary>
        ///     Configures the dimension of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="dimension"> The dimension. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use HasGeometricDimension")]
        public static IConventionPropertyBuilder ForSqliteHasDimension(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string dimension,
            bool fromDataAnnotation = false)
            => propertyBuilder.HasGeometricDimension(dimension, fromDataAnnotation);
    }
}
