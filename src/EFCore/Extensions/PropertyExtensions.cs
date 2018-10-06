// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" />.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     Gets the factory that has been set to generate values for this property, if any.
        /// </summary>
        /// <param name="property"> The property to get the value generator factory for. </param>
        /// <returns> The factory, or null if no factory has been set. </returns>
        public static Func<IProperty, IEntityType, ValueGenerator> GetValueGeneratorFactory([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (Func<IProperty, IEntityType, ValueGenerator>)property[CoreAnnotationNames.ValueGeneratorFactoryAnnotation];
        }

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
        ///     Gets a value indicating whether or not the property can persist Unicode characters.
        /// </summary>
        /// <param name="property"> The property to get the Unicode setting for. </param>
        /// <returns> The Unicode setting, or null if none if defined. </returns>
        public static bool? IsUnicode([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (bool?)property[CoreAnnotationNames.UnicodeAnnotation];
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

        /// <summary>
        ///     Gets the type that the property value will be converted to before being sent to the database provider.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The provider type, or <c>null</c> if none has been set. </returns>
        public static Type GetProviderClrType([NotNull] this IProperty property)
            => (Type)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ProviderClrType];

        /// <summary>
        ///     Gets the custom <see cref="ValueConverter" /> set for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The converter, or <c>null</c> if none has been set. </returns>
        public static ValueConverter GetValueConverter([NotNull] this IProperty property)
            => (ValueConverter)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ValueConverter];

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> for this property, or null if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <c>null</c> if none has been set. </returns>
        public static ValueComparer GetValueComparer([NotNull] this IProperty property)
            => (ValueComparer)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ValueComparer];

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use with keys for this property, or null if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <c>null</c> if none has been set. </returns>
        public static ValueComparer GetKeyValueComparer([NotNull] this IProperty property)
            => (ValueComparer)Check.NotNull(property, nameof(property))[CoreAnnotationNames.KeyValueComparer];

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use for structural copies for this property, or null if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <c>null</c> if none has been set. </returns>
        public static ValueComparer GetStructuralValueComparer([NotNull] this IProperty property)
            => (ValueComparer)Check.NotNull(property, nameof(property))[CoreAnnotationNames.KeyValueComparer];
    }
}
