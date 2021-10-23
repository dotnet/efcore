// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IReadOnlyProperty" />.
    /// </summary>
    [Obsolete("Use IReadOnlyProperty")]
    public static class PropertyExtensions
    {
        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property">The foreign key property.</param>
        /// <returns>The list of all associated principal properties including the given property.</returns>
        [Obsolete("Use IReadOnlyProperty.GetPrincipals")]
        public static IReadOnlyList<IProperty> FindPrincipals(this IProperty property)
            => property.GetPrincipals();

        /// <summary>
        ///     Gets a value indicating whether this property is used as a foreign key (or part of a composite foreign key).
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns><see langword="true" /> if the property is used as a foreign key, otherwise <see langword="false" />.</returns>
        [Obsolete("Use IReadOnlyProperty.IsForeignKey")]
        public static bool IsForeignKey(this IProperty property)
            => property.IsForeignKey();

        /// <summary>
        ///     Gets a value indicating whether this property is used as an index (or part of a composite index).
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns><see langword="true" /> if the property is used as an index, otherwise <see langword="false" />.</returns>
        [Obsolete("Use IReadOnlyProperty.IsIndex")]
        public static bool IsIndex(this IProperty property)
            => property.IsIndex();

        /// <summary>
        ///     Gets a value indicating whether this property is used as a unique index (or part of a unique composite index).
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns><see langword="true" /> if the property is used as an unique index, otherwise <see langword="false" />.</returns>
        [Obsolete("Use IReadOnlyProperty.IsUniqueIndex")]
        public static bool IsUniqueIndex(this IProperty property)
            => property.IsUniqueIndex();

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key (or part of a composite primary key).
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns><see langword="true" /> if the property is used as the primary key, otherwise <see langword="false" />.</returns>
        [Obsolete("Use IReadOnlyProperty.IsPrimaryKey")]
        public static bool IsPrimaryKey(this IProperty property)
            => property.IsPrimaryKey();

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key or alternate key
        ///     (or part of a composite primary or alternate key).
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns><see langword="true" /> if the property is used as a key, otherwise <see langword="false" />.</returns>
        [Obsolete("Use IReadOnlyProperty.IsKey")]
        public static bool IsKey(this IProperty property)
            => property.IsKey();

        /// <summary>
        ///     Gets a value indicating whether or not this property can be modified before the entity is
        ///     saved to the database.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a value is assigned to this property when it is in
        ///         the <see cref="EntityState.Added" /> state.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any value
        ///         set will be ignored when it is in the <see cref="EntityState.Added" /> state.
        ///     </para>
        /// </remarks>
        /// <param name="property">The property.</param>
        [Obsolete("Use IReadOnlyProperty.GetBeforeSaveBehavior")]
        public static PropertySaveBehavior GetBeforeSaveBehavior(this IProperty property)
            => property.GetBeforeSaveBehavior();

        /// <summary>
        ///     Gets a value indicating whether or not this property can be modified after the entity is
        ///     saved to the database.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a new value is assigned to this property after the entity exists in the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any modification to the
        ///         property value of an entity that already exists in the database will be ignored.
        ///     </para>
        /// </remarks>
        /// <param name="property">The property.</param>
        [Obsolete("Use IReadOnlyProperty.GetAfterSaveBehavior")]
        public static PropertySaveBehavior GetAfterSaveBehavior(this IProperty property)
            => property.GetAfterSaveBehavior();

        /// <summary>
        ///     Gets the factory that has been set to generate values for this property, if any.
        /// </summary>
        /// <param name="property">The property to get the value generator factory for.</param>
        /// <returns>The factory, or <see langword="null" /> if no factory has been set.</returns>
        [Obsolete("Use IReadOnlyProperty.GetValueGeneratorFactory")]
        public static Func<IProperty, IEntityType, ValueGenerator>? GetValueGeneratorFactory(this IProperty property)
            => property.GetValueGeneratorFactory();

        /// <summary>
        ///     Gets the custom <see cref="ValueConverter" /> set for this property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The converter, or <see langword="null" /> if none has been set.</returns>
        [Obsolete("Use IReadOnlyProperty.GetValueConverter")]
        public static ValueConverter? GetValueConverter(this IProperty property)
            => property.GetValueConverter();

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use for structural copies for this property, or <see langword="null" /> if none is set.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The comparer, or <see langword="null" /> if none has been set.</returns>
        [Obsolete("Use GetKeyValueComparer. A separate structural comparer is no longer supported.")]
        public static ValueComparer? GetStructuralValueComparer(this IProperty property)
            => property.GetKeyValueComparer();
    }
}
