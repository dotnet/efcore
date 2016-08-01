// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableProperty" />.
    /// </summary>
    public static class MutablePropertyExtensions
    {
        /// <summary>
        ///     <para>
        ///         Sets the factory to use for generating values for this property, or null to clear any previously set factory.
        ///     </para>
        ///     <para>
        ///         Setting null does not disable value generation for this property, it just clears any generator explicitly
        ///         configured for this property. The database provider may still have a value generator for the property type.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property to set the value generator for. </param>
        /// <param name="valueGeneratorFactory">
        ///     A factory that will be used to create the value generator, or null to
        ///     clear any previously set factory.
        /// </param>
        public static void SetValueGeneratorFactory(
            [NotNull] this IMutableProperty property,
            [NotNull] Func<IProperty, IEntityType, ValueGenerator> valueGeneratorFactory)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(valueGeneratorFactory, nameof(valueGeneratorFactory));

            property[CoreAnnotationNames.ValueGeneratorFactoryAnnotation] = valueGeneratorFactory;
        }

        /// <summary>
        ///     Sets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" /> '
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <param name="property"> The property to set the maximum length of. </param>
        /// <param name="maxLength"> The maximum length of data that is allowed in this property. </param>
        public static void SetMaxLength([NotNull] this IMutableProperty property, int? maxLength)
        {
            Check.NotNull(property, nameof(property));

            if (maxLength != null
                && maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            property[CoreAnnotationNames.MaxLengthAnnotation] = maxLength;
        }

        /// <summary>
        ///     Sets a value indicating whether or not this property can persist unicode characters.
        /// </summary>
        /// <param name="property"> The property to set the value for. </param>
        /// <param name="unicode"> True if the property accepts unicode characters, false if it does not, null to clear the setting. </param>
        public static void IsUnicode([NotNull] this IMutableProperty property, bool? unicode)
        {
            Check.NotNull(property, nameof(property));

            property[CoreAnnotationNames.UnicodeAnnotation] = unicode;
        }

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get foreign keys for. </param>
        /// <returns>
        ///     The foreign keys that use this property.
        /// </returns>
        public static IEnumerable<IMutableForeignKey> GetContainingForeignKeys([NotNull] this IMutableProperty property)
            => ((IProperty)property).GetContainingForeignKeys().Cast<IMutableForeignKey>();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary key for. </param>
        /// <returns>
        ///     The primary that use this property, or null if it is not part of the primary key.
        /// </returns>
        public static IMutableKey GetContainingPrimaryKey([NotNull] this IMutableProperty property)
            => (IMutableKey)((IProperty)property).GetContainingPrimaryKey();

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary and alternate keys for. </param>
        /// <returns>
        ///     The primary and alternate keys that use this property.
        /// </returns>
        public static IEnumerable<IMutableKey> GetContainingKeys([NotNull] this IMutableProperty property)
            => ((IProperty)property).GetContainingKeys().Cast<IMutableKey>();
    }
}
