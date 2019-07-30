// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for SQLite metadata.
    /// </summary>
    public static class SqlitePropertyExtensions
    {
        /// <summary>
        ///     Returns the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The SRID to use when creating a column for this property. </returns>
        public static int? GetSrid([NotNull] this IProperty property)
            => (int?)property[SqliteAnnotationNames.Srid];

        /// <summary>
        ///     Sets the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The SRID. </param>
        public static void SetSrid([NotNull] this IMutableProperty property, int? value)
            => property.SetOrRemoveAnnotation(SqliteAnnotationNames.Srid, value);

        /// <summary>
        ///     Sets the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The SRID. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetSrid([NotNull] this IConventionProperty property, int? value, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(SqliteAnnotationNames.Srid, value, fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column SRID.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column SRID. </returns>
        public static ConfigurationSource? GetSridConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(SqliteAnnotationNames.Srid)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the dimension to use when creating a column for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The dimension to use when creating a column for this property. </returns>
        public static string GetGeometricDimension([NotNull] this IProperty property)
            => (string)property[SqliteAnnotationNames.Dimension];

        /// <summary>
        ///     Sets the dimension to use when creating a column for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The dimension. </param>
        public static void SetGeometricDimension([NotNull] this IMutableProperty property, [CanBeNull] string value)
            => property.SetOrRemoveAnnotation(SqliteAnnotationNames.Dimension, value);

        /// <summary>
        ///     Sets the dimension to use when creating a column for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The dimension. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetGeometricDimension(
            [NotNull] this IConventionProperty property, [CanBeNull] string value, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(SqliteAnnotationNames.Dimension, value, fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column dimension.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column dimension. </returns>
        public static ConfigurationSource? GetGeometricDimensionConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(SqliteAnnotationNames.Dimension)?.GetConfigurationSource();
    }
}
