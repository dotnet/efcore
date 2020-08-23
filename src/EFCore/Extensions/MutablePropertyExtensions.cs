// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        ///     Finds the first principal property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The first associated principal property, or <see langword="null" /> if none exists. </returns>
        public static IMutableProperty FindFirstPrincipal([NotNull] this IMutableProperty property)
            => (IMutableProperty)((IProperty)property).FindFirstPrincipal();

        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The list of all associated principal properties including the given property. </returns>
        public static IReadOnlyList<IMutableProperty> FindPrincipals([NotNull] this IMutableProperty property)
            => ((IProperty)property).FindPrincipals().Cast<IMutableProperty>().ToList();

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get foreign keys for. </param>
        /// <returns>
        ///     The foreign keys that use this property.
        /// </returns>
        public static IEnumerable<IMutableForeignKey> GetContainingForeignKeys([NotNull] this IMutableProperty property)
            => ((Property)property).GetContainingForeignKeys();

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get indexes for. </param>
        /// <returns>
        ///     The indexes that use this property.
        /// </returns>
        public static IEnumerable<IMutableIndex> GetContainingIndexes([NotNull] this IMutableProperty property)
            => ((Property)property).GetContainingIndexes();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary key for. </param>
        /// <returns>
        ///     The primary that use this property, or <see langword="null" /> if it is not part of the primary key.
        /// </returns>
        public static IMutableKey FindContainingPrimaryKey([NotNull] this IMutableProperty property)
            => (IMutableKey)((IProperty)property).FindContainingPrimaryKey();

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary and alternate keys for. </param>
        /// <returns>
        ///     The primary and alternate keys that use this property.
        /// </returns>
        public static IEnumerable<IMutableKey> GetContainingKeys([NotNull] this IMutableProperty property)
            => ((Property)property).GetContainingKeys();

        /// <summary>
        ///     Sets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" /> '
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <param name="property"> The property to set the maximum length of. </param>
        /// <param name="maxLength"> The maximum length of data that is allowed in this property. </param>
        public static void SetMaxLength([NotNull] this IMutableProperty property, int? maxLength)
            => property.AsProperty().SetMaxLength(maxLength, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the precision of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" />
        ///     then this is the maximum number of digits.
        /// </summary>
        /// <param name="property"> The property to set the precision of. </param>
        /// <param name="precision"> The maximum number of digits that is allowed in this property. </param>
        public static void SetPrecision([NotNull] this IMutableProperty property, int? precision)
            => property.AsProperty().SetPrecision(precision, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the scale of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" />
        ///     then this is the maximum number of decimal places.
        /// </summary>
        /// <param name="property"> The property to set the scale of. </param>
        /// <param name="scale"> The maximum number of decimal places that is allowed in this property. </param>
        public static void SetScale([NotNull] this IMutableProperty property, int? scale)
            => property.AsProperty().SetScale(scale, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets a value indicating whether this property can persist Unicode characters.
        /// </summary>
        /// <param name="property"> The property to set the value for. </param>
        /// <param name="unicode">
        ///     <see langword="true" /> if the property accepts Unicode characters, <see langword="false" /> if it does not, <see langword="null" /> to
        ///     clear the setting.
        /// </param>
        public static void SetIsUnicode([NotNull] this IMutableProperty property, bool? unicode)
            => property.AsProperty().SetIsUnicode(unicode, ConfigurationSource.Explicit);

        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating whether this property can be modified before the entity is
        ///         saved to the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a value is assigned to this property when it is in
        ///         the <see cref="EntityState.Added" /> state.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any value
        ///         set will be ignored when it is in the <see cref="EntityState.Added" /> state.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="beforeSaveBehavior">
        ///     A value indicating whether this property can be modified before the entity is saved to the database.
        /// </param>
        public static void SetBeforeSaveBehavior([NotNull] this IMutableProperty property, PropertySaveBehavior? beforeSaveBehavior)
            => property.AsProperty().SetBeforeSaveBehavior(beforeSaveBehavior, ConfigurationSource.Explicit);

        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating whether this property can be modified after the entity is
        ///         saved to the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a new value is assigned to this property after the entity exists in the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any modification to the
        ///         property value of an entity that already exists in the database will be ignored.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="afterSaveBehavior">
        ///     A value indicating whether this property can be modified after the entity is saved to the database.
        /// </param>
        public static void SetAfterSaveBehavior([NotNull] this IMutableProperty property, PropertySaveBehavior? afterSaveBehavior)
            => property.AsProperty().SetAfterSaveBehavior(afterSaveBehavior, ConfigurationSource.Explicit);

        /// <summary>
        ///     <para>
        ///         Sets the factory to use for generating values for this property, or <see langword="null" /> to clear any previously set factory.
        ///     </para>
        ///     <para>
        ///         Setting <see langword="null" /> does not disable value generation for this property, it just clears any generator explicitly
        ///         configured for this property. The database provider may still have a value generator for the property type.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property to set the value generator for. </param>
        /// <param name="valueGeneratorFactory">
        ///     A factory that will be used to create the value generator, or <see langword="null" /> to
        ///     clear any previously set factory.
        /// </param>
        public static void SetValueGeneratorFactory(
            [NotNull] this IMutableProperty property,
            [NotNull] Func<IProperty, IEntityType, ValueGenerator> valueGeneratorFactory)
            => property.AsProperty().SetValueGeneratorFactory(valueGeneratorFactory, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the custom <see cref="ValueConverter" /> for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="converter"> The converter, or <see langword="null" /> to remove any previously set converter. </param>
        public static void SetValueConverter([NotNull] this IMutableProperty property, [CanBeNull] ValueConverter converter)
            => property.AsProperty().SetValueConverter(converter, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the type that the property value will be converted to before being sent to the database provider.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="providerClrType"> The type to use, or <see langword="null" /> to remove any previously set type. </param>
        public static void SetProviderClrType([NotNull] this IMutableProperty property, [CanBeNull] Type providerClrType)
            => property.AsProperty().SetProviderClrType(providerClrType, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the <see cref="CoreTypeMapping" /> for the given property
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="typeMapping"> The <see cref="CoreTypeMapping" /> for this property. </param>
        public static CoreTypeMapping SetTypeMapping(
            [NotNull] this IMutableProperty property,
            [NotNull] CoreTypeMapping typeMapping)
            => ((Property)property).SetTypeMapping(typeMapping, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comparer"> The comparer, or <see langword="null" /> to remove any previously set comparer. </param>
        public static void SetValueComparer([NotNull] this IMutableProperty property, [CanBeNull] ValueComparer comparer)
            => property.AsProperty().SetValueComparer(comparer, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for this property when performing key comparisons.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comparer"> The comparer, or <see langword="null" /> to remove any previously set comparer. </param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetKeyValueComparer([NotNull] this IMutableProperty property, [CanBeNull] ValueComparer comparer)
            => property.AsProperty().SetValueComparer(comparer, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for structural copies for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comparer"> The comparer, or <see langword="null" /> to remove any previously set comparer. </param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetStructuralValueComparer([NotNull] this IMutableProperty property, [CanBeNull] ValueComparer comparer)
            => property.SetValueComparer(comparer);
    }
}
