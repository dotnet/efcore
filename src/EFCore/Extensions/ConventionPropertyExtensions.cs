// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionProperty" />.
    /// </summary>
    [Obsolete("Use IConventionProperty")]
    public static class ConventionPropertyExtensions
    {
        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property">The foreign key property.</param>
        /// <returns>The list of all associated principal properties including the given property.</returns>
        [Obsolete("Use IConventionProperty.GetPrincipals")]
        public static IReadOnlyList<IConventionProperty> FindPrincipals(this IConventionProperty property)
            => property.GetPrincipals();

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for this property when performing key comparisons.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetKeyValueComparer(
            this IConventionProperty property,
            ValueComparer? comparer,
            bool fromDataAnnotation = false)
            => property.SetValueComparer(comparer);

        /// <summary>
        ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetKeyValueComparer" />.
        /// </summary>
        /// <param name="property">The property to find configuration source for.</param>
        /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetKeyValueComparer" />.</returns>
        [Obsolete("Use GetValueComparerConfigurationSource. Only a single value comparer is allowed for a given property.")]
        public static ConfigurationSource? GetKeyValueComparerConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.KeyValueComparer)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for structural copies for this property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetStructuralValueComparer(
            this IConventionProperty property,
            ValueComparer? comparer,
            bool fromDataAnnotation = false)
            => property.SetKeyValueComparer(comparer, fromDataAnnotation);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetStructuralValueComparer" />.
        /// </summary>
        /// <param name="property">The property to find configuration source for.</param>
        /// <returns>The configuration source for <see cref="PropertyExtensions.GetStructuralValueComparer" />.</returns>
        [Obsolete("Use GetValueComparerConfigurationSource. Only a single value comparer is allowed for a given property.")]
        public static ConfigurationSource? GetStructuralValueComparerConfigurationSource(this IConventionProperty property)
            => property.GetKeyValueComparerConfigurationSource();
    }
}
