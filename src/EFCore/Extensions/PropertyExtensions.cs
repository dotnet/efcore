// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable enable

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IReadOnlyProperty" />.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The list of all associated principal properties including the given property. </returns>
        [Obsolete("Use IReadOnlyProperty.GetPrincipals")]
        public static IReadOnlyList<IReadOnlyProperty> FindPrincipals([NotNull] this IReadOnlyProperty property)
            => property.GetPrincipals();

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use for structural copies for this property, or <see langword="null" /> if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <see langword="null" /> if none has been set. </returns>
        [Obsolete("Use GetKeyValueComparer. A separate structural comparer is no longer supported.")]
        public static ValueComparer? GetStructuralValueComparer([NotNull] this IReadOnlyProperty property)
            => property.GetKeyValueComparer();

        /// <summary>
        ///     Creates a formatted string representation of the given properties such as is useful
        ///     when throwing exceptions about keys, indexes, etc. that use the properties.
        /// </summary>
        /// <param name="properties"> The properties to format. </param>
        /// <param name="includeTypes"> If true, then type names are included in the string. The default is <see langword="false" />.</param>
        /// <returns> The string representation. </returns>
        public static string Format([NotNull] this IEnumerable<IReadOnlyPropertyBase> properties, bool includeTypes = false)
            => "{"
                + string.Join(
                    ", ",
                    properties.Select(
                        p => "'" + p.Name + "'" + (includeTypes ? " : " + p.ClrType.DisplayName(fullName: false) : "")))
                + "}";
    }
}
