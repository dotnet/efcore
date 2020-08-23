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
    ///     Extension methods for <see cref="IConventionProperty" />.
    /// </summary>
    public static class ConventionPropertyExtensions
    {
        /// <summary>
        ///     Finds the first principal property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The first associated principal property, or <see langword="null" /> if none exists. </returns>
        public static IConventionProperty FindFirstPrincipal([NotNull] this IConventionProperty property)
            => (IConventionProperty)((IProperty)property).FindFirstPrincipal();

        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The list of all associated principal properties including the given property. </returns>
        public static IReadOnlyList<IConventionProperty> FindPrincipals([NotNull] this IConventionProperty property)
            => ((IProperty)property).FindPrincipals().Cast<IConventionProperty>().ToList();

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get foreign keys for. </param>
        /// <returns>
        ///     The foreign keys that use this property.
        /// </returns>
        public static IEnumerable<IConventionForeignKey> GetContainingForeignKeys([NotNull] this IConventionProperty property)
            => ((IProperty)property).GetContainingForeignKeys().Cast<IConventionForeignKey>();

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get indexes for. </param>
        /// <returns>
        ///     The indexes that use this property.
        /// </returns>
        public static IEnumerable<IConventionIndex> GetContainingIndexes([NotNull] this IConventionProperty property)
            => ((Property)property).GetContainingIndexes();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary key for. </param>
        /// <returns>
        ///     The primary that use this property, or <see langword="null" /> if it is not part of the primary key.
        /// </returns>
        public static IConventionKey FindContainingPrimaryKey([NotNull] this IConventionProperty property)
            => (IConventionKey)((IProperty)property).FindContainingPrimaryKey();

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary and alternate keys for. </param>
        /// <returns>
        ///     The primary and alternate keys that use this property.
        /// </returns>
        public static IEnumerable<IConventionKey> GetContainingKeys([NotNull] this IConventionProperty property)
            => ((Property)property).GetContainingKeys();

        /// <summary>
        ///     Sets the <see cref="CoreTypeMapping" /> for the given property
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="typeMapping"> The <see cref="CoreTypeMapping" /> for this property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static CoreTypeMapping SetTypeMapping(
            [NotNull] this IConventionProperty property,
            [NotNull] CoreTypeMapping typeMapping,
            bool fromDataAnnotation = false)
            => ((Property)property).SetTypeMapping(
                typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for <see cref="PropertyExtensions.FindTypeMapping(IProperty)" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for <see cref="PropertyExtensions.FindTypeMapping(IProperty)" />. </returns>
        public static ConfigurationSource? GetTypeMappingConfigurationSource([NotNull] this IConventionProperty property)
            => ((Property)property).GetTypeMappingConfigurationSource();

        /// <summary>
        ///     Sets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" /> '
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <param name="property"> The property to set the maximum length of. </param>
        /// <param name="maxLength"> The maximum length of data that is allowed in this property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured property. </returns>
        public static int? SetMaxLength(
            [NotNull] this IConventionProperty property,
            int? maxLength,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetMaxLength(
                maxLength, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetMaxLength" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetMaxLength" />. </returns>
        public static ConfigurationSource? GetMaxLengthConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.MaxLength)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the precision of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" />
        ///     then this is the maximum number of digits.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="precision"> The maximum number of digits that is allowed in this property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static int? SetPrecision([NotNull] this IConventionProperty property, int? precision, bool fromDataAnnotation = false)
            => property.AsProperty().SetPrecision(
                precision, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetPrecision" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetPrecision" />. </returns>
        public static ConfigurationSource? GetPrecisionConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.Precision)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the scale of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" />
        ///     then this is the maximum number of decimal places.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="scale"> The maximum number of decimal places that is allowed in this property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static int? SetScale([NotNull] this IConventionProperty property, int? scale, bool fromDataAnnotation = false)
            => property.AsProperty().SetScale(
                scale, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetScale" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetScale" />. </returns>
        public static ConfigurationSource? GetScaleConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.Scale)?.GetConfigurationSource();

        /// <summary>
        ///     Sets a value indicating whether this property can persist Unicode characters.
        /// </summary>
        /// <param name="property"> The property to set the value for. </param>
        /// <param name="unicode">
        ///     <see langword="true" /> if the property accepts Unicode characters, <see langword="false" /> if it does not, <see langword="null" /> to
        ///     clear the setting.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsUnicode(
            [NotNull] this IConventionProperty property,
            bool? unicode,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetIsUnicode(
                unicode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.IsUnicode" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.IsUnicode" />. </returns>
        public static ConfigurationSource? GetIsUnicodeConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.Unicode)?.GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Sets a value indicating whether this property can be modified before the entity is
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
        ///     A value indicating whether this property can be modified before the entity is
        ///     saved to the database. <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static PropertySaveBehavior? SetBeforeSaveBehavior(
            [NotNull] this IConventionProperty property,
            PropertySaveBehavior? beforeSaveBehavior,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetBeforeSaveBehavior(
                beforeSaveBehavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetBeforeSaveBehavior" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetBeforeSaveBehavior" />. </returns>
        public static ConfigurationSource? GetBeforeSaveBehaviorConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.BeforeSaveBehavior)?.GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Sets a value indicating whether this property can be modified after the entity is
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
        ///     Sets a value indicating whether this property can be modified after the entity is
        ///     saved to the database. <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static PropertySaveBehavior? SetAfterSaveBehavior(
            [NotNull] this IConventionProperty property,
            PropertySaveBehavior? afterSaveBehavior,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetAfterSaveBehavior(
                afterSaveBehavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetAfterSaveBehavior" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetAfterSaveBehavior" />. </returns>
        public static ConfigurationSource? GetAfterSaveBehaviorConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.AfterSaveBehavior)?.GetConfigurationSource();

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
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static Func<IProperty, IEntityType, ValueGenerator> SetValueGeneratorFactory(
            [NotNull] this IConventionProperty property,
            [NotNull] Func<IProperty, IEntityType, ValueGenerator> valueGeneratorFactory,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetValueGeneratorFactory(
                valueGeneratorFactory, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetValueGeneratorFactory" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetValueGeneratorFactory" />. </returns>
        public static ConfigurationSource? GetValueGeneratorFactoryConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.ValueGeneratorFactory)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the custom <see cref="ValueConverter" /> for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="converter"> The converter, or <see langword="null" /> to remove any previously set converter. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static ValueConverter SetValueConverter(
            [NotNull] this IConventionProperty property,
            [CanBeNull] ValueConverter converter,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetValueConverter(
                converter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetValueConverter" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetValueConverter" />. </returns>
        public static ConfigurationSource? GetValueConverterConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.ValueConverter)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the type that the property value will be converted to before being sent to the database provider.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="providerClrType"> The type to use, or <see langword="null" /> to remove any previously set type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static Type SetProviderClrType(
            [NotNull] this IConventionProperty property,
            [CanBeNull] Type providerClrType,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetProviderClrType(
                providerClrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetProviderClrType" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetProviderClrType" />. </returns>
        public static ConfigurationSource? GetProviderClrTypeConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.ProviderClrType)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comparer"> The comparer, or <see langword="null" /> to remove any previously set comparer. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static ValueComparer SetValueComparer(
            [NotNull] this IConventionProperty property,
            [CanBeNull] ValueComparer comparer,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetValueComparer(
                comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetValueComparer" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetValueComparer" />. </returns>
        public static ConfigurationSource? GetValueComparerConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.ValueComparer)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for this property when performing key comparisons.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comparer"> The comparer, or <see langword="null" /> to remove any previously set comparer. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetKeyValueComparer(
            [NotNull] this IConventionProperty property,
            [CanBeNull] ValueComparer comparer,
            bool fromDataAnnotation = false)
            => property.AsProperty().SetValueComparer(
                comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetKeyValueComparer" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetKeyValueComparer" />. </returns>
        [Obsolete("Use GetValueComparerConfigurationSource. Only a single value comparer is allowed for a given property.")]
        public static ConfigurationSource? GetKeyValueComparerConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CoreAnnotationNames.KeyValueComparer)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the custom <see cref="ValueComparer" /> for structural copies for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comparer"> The comparer, or <see langword="null" /> to remove any previously set comparer. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        [Obsolete("Use SetValueComparer. Only a single value comparer is allowed for a given property.")]
        public static void SetStructuralValueComparer(
            [NotNull] this IConventionProperty property,
            [CanBeNull] ValueComparer comparer,
            bool fromDataAnnotation = false)
            => property.SetKeyValueComparer(comparer, fromDataAnnotation);

        /// <summary>
        ///     Returns the configuration source for <see cref="PropertyExtensions.GetStructuralValueComparer" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="PropertyExtensions.GetStructuralValueComparer" />. </returns>
        [Obsolete("Use GetValueComparerConfigurationSource. Only a single value comparer is allowed for a given property.")]
        public static ConfigurationSource? GetStructuralValueComparerConfigurationSource([NotNull] this IConventionProperty property)
            => property.GetKeyValueComparerConfigurationSource();
    }
}
