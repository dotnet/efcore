// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableProperty" />.
    /// </summary>
    [Obsolete("Use IMutableProperty")]
    public static class MutablePropertyExtensions
    {
        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property">The foreign key property.</param>
        /// <returns>The list of all associated principal properties including the given property.</returns>
        [Obsolete("Use IMutableProperty.GetPrincipals")]
        public static IReadOnlyList<IMutableProperty> FindPrincipals(this IMutableProperty property)
            => property.GetPrincipals();

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for this property when performing key comparisons.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetKeyValueComparer(this IMutableProperty property, ValueComparer? comparer)
            => property.SetValueComparer(comparer);

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for structural copies for this property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetStructuralValueComparer(this IMutableProperty property, ValueComparer? comparer)
            => property.SetValueComparer(comparer);
    }
}
