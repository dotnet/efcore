// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IReadOnlyPropertyBase" />.
    /// </summary>
    public static class PropertyBaseExtensions
    {
        /// <summary>
        ///     Gets a value indicating whether this is a shadow property. A shadow property is one that does not have a
        ///     corresponding property in the entity class. The current value for the property is stored in
        ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <see langword="true" /> if the property is a shadow property, otherwise <see langword="false" />.
        /// </returns>
        [Obsolete("Use IReadOnlyPropertyBase.IsShadowProperty")]
        public static bool IsShadowProperty(this IPropertyBase property)
            => property.IsShadowProperty();

        /// <summary>
        ///     Creates a formatted string representation of the given properties such as is useful
        ///     when throwing exceptions about keys, indexes, etc. that use the properties.
        /// </summary>
        /// <param name="properties"> The properties to format. </param>
        /// <param name="includeTypes"> If true, then type names are included in the string. The default is <see langword="false" />.</param>
        /// <returns> The string representation. </returns>
        public static string Format(this IEnumerable<IReadOnlyPropertyBase> properties, bool includeTypes = false)
            => "{"
                + string.Join(
                    ", ",
                    properties.Select(
                        p => "'" + p.Name + "'" + (includeTypes ? " : " + p.ClrType.DisplayName(fullName: false) : "")))
                + "}";
    }
}
