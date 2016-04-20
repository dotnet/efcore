// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" />.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     Gets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" /> '
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <param name="property"> The property to get the maximum length of. </param>
        /// <returns> The maximum length, or null if none if defined. </returns>
        public static int? GetMaxLength([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.MaxLengthAnnotation];
        }

        /// <summary>
        ///     Gets a value indicating whether this property is used as a foreign key (or part of a composite foreign key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     True if the property is used as a foreign key, otherwise false.
        /// </returns>
        public static bool IsForeignKey([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().ForeignKeys != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as an index (or part of a composite index).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     True if the property is used as an index, otherwise false.
        /// </returns>
        public static bool IsIndex([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().Indexes != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key (or part of a composite primary key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     True if the property is used as the primary key, otherwise false.
        /// </returns>
        public static bool IsPrimaryKey([NotNull] this IProperty property)
            => GetContainingPrimaryKey(property) != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as part of a primary or alternate key
        ///     (or part of a composite primary or alternate key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     True if the property is part of a key, otherwise false.
        /// </returns>
        public static bool IsKey([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().Keys != null;

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get foreign keys for. </param>
        /// <returns>
        ///     The foreign keys that use this property.
        /// </returns>
        public static IEnumerable<IForeignKey> GetContainingForeignKeys([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().ForeignKeys
               ?? Enumerable.Empty<IForeignKey>();

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get indexes for. </param>
        /// <returns>
        ///     The indexes that use this property.
        /// </returns>
        public static IEnumerable<IIndex> GetContainingIndexes([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().Indexes
               ?? Enumerable.Empty<IIndex>();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary key for. </param>
        /// <returns>
        ///     The primary that use this property, or null if it is not part of the primary key.
        /// </returns>
        public static IKey GetContainingPrimaryKey([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().PrimaryKey;

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary and alternate keys for. </param>
        /// <returns>
        ///     The primary and alternate keys that use this property.
        /// </returns>
        public static IEnumerable<IKey> GetContainingKeys([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().Keys
               ?? Enumerable.Empty<IKey>();
    }
}
