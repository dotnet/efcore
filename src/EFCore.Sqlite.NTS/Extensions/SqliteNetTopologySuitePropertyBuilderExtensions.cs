// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite and NetTopologySuite specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class SqliteNetTopologySuitePropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the dimension of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="ordinates"> The dimension ordinates. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForSqliteHasDimension(
            [NotNull] this PropertyBuilder propertyBuilder,
            Ordinates ordinates)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            string dimension = null;
            if (ordinates.HasFlag(Ordinates.Z))
            {
                dimension += "Z";
            }
            if (ordinates.HasFlag(Ordinates.M))
            {
                dimension += "M";
            }

            propertyBuilder.Metadata.Sqlite().Dimension = dimension;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the dimension of the column that the property maps to when targeting SQLite.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="ordinates"> The dimension ordinates. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForSqliteHasDimension<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            Ordinates ordinates)
            => (PropertyBuilder<TProperty>)ForSqliteHasDimension((PropertyBuilder)propertyBuilder, ordinates);
    }
}
