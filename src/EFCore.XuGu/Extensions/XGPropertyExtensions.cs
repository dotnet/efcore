// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     MySQL specific extension methods for properties.
    /// </summary>
    public static class XGPropertyExtensions
    {
        /// <summary>
        ///     <para>
        ///         Returns the <see cref="XGValueGenerationStrategy" /> to use for the property.
        ///     </para>
        ///     <para>
        ///         If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
        ///     </para>
        /// </summary>
        /// <returns> The strategy, or <see cref="XGValueGenerationStrategy.None"/> if none was set. </returns>
        public static XGValueGenerationStrategy GetValueGenerationStrategy([NotNull] this IReadOnlyProperty property)
        {
            if (property.FindAnnotation(XGAnnotationNames.ValueGenerationStrategy) is { } annotation)
            {
                if (annotation.Value is { } annotationValue)
                {
                    // Allow users to use the underlying type value instead of the enum itself.
                    // Workaround for: https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/issues/1205
                    if (ObjectToEnumConverter.GetEnumValue<XGValueGenerationStrategy>(annotationValue) is { } enumValue)
                    {
                        return enumValue;
                    }

                    return (XGValueGenerationStrategy)annotationValue;
                }

                return XGValueGenerationStrategy.None;
            }

            if (property.ValueGenerated == ValueGenerated.OnAdd)
            {
                if (property.IsForeignKey()
                    || property.TryGetDefaultValue(out _)
                    || property.GetDefaultValueSql() != null
                    || property.GetComputedColumnSql() != null)
                {
                    return XGValueGenerationStrategy.None;
                }

                return GetDefaultValueGenerationStrategy(property);
            }

            if (property.ValueGenerated == ValueGenerated.OnAddOrUpdate)
            {
                // We explicitly check for RowVersion when generation migrations. We therefore handle RowVersion separately from other cases
                // of using CURRENT_TIMESTAMP etc. and we don't generate a XGValueGenerationStrategy.ComputedColumn annotation.
                if (IsCompatibleComputedColumn(property) &&
                    !property.IsConcurrencyToken)
                {
                    return XGValueGenerationStrategy.ComputedColumn;
                }
            }

            return XGValueGenerationStrategy.None;
        }

        public static XGValueGenerationStrategy GetValueGenerationStrategy(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
            => GetValueGenerationStrategy(property, storeObject, null);

        internal static XGValueGenerationStrategy GetValueGenerationStrategy(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            [CanBeNull] ITypeMappingSource typeMappingSource)
        {
            if (property.FindOverrides(storeObject)?.FindAnnotation(XGAnnotationNames.ValueGenerationStrategy) is { } @override)
            {
                return ObjectToEnumConverter.GetEnumValue<XGValueGenerationStrategy>(@override.Value) ?? XGValueGenerationStrategy.None;
            }

            var annotation = property.FindAnnotation(XGAnnotationNames.ValueGenerationStrategy);
            if (annotation?.Value is { } annotationValue
                && ObjectToEnumConverter.GetEnumValue<XGValueGenerationStrategy>(annotationValue) is { } enumValue
                && StoreObjectIdentifier.Create(property.DeclaringType, storeObject.StoreObjectType) == storeObject)
            {
                return enumValue;
            }

            var table = storeObject;
            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.GetValueGenerationStrategy(storeObject, typeMappingSource)
                    == XGValueGenerationStrategy.IdentityColumn
                    && table.StoreObjectType == StoreObjectType.Table
                    && !property.GetContainingForeignKeys().Any(
                        fk =>
                            !fk.IsBaseLinking()
                            || (StoreObjectIdentifier.Create(fk.PrincipalEntityType, StoreObjectType.Table)
                                    is StoreObjectIdentifier principal
                                && fk.GetConstraintName(table, principal) != null))
                        ? XGValueGenerationStrategy.IdentityColumn
                        : XGValueGenerationStrategy.None;
            }

            if (property.ValueGenerated == ValueGenerated.OnAdd)
            {
                if (table.StoreObjectType != StoreObjectType.Table
                    || property.TryGetDefaultValue(storeObject, out _)
                    || property.GetDefaultValueSql(storeObject) != null
                    || property.GetComputedColumnSql(storeObject) != null
                    || property.GetContainingForeignKeys()
                        .Any(
                            fk =>
                                !fk.IsBaseLinking()
                                || (StoreObjectIdentifier.Create(fk.PrincipalEntityType, StoreObjectType.Table)
                                        is StoreObjectIdentifier principal
                                    && fk.GetConstraintName(table, principal) != null)))
                {
                    return XGValueGenerationStrategy.None;
                }

                var defaultStrategy = GetDefaultValueGenerationStrategy(property, storeObject, typeMappingSource);
                if (defaultStrategy != XGValueGenerationStrategy.None)
                {
                    if (annotation != null)
                    {
                        return (XGValueGenerationStrategy?)annotation.Value ?? XGValueGenerationStrategy.None;
                    }
                }

                return defaultStrategy;
            }

            if (property.ValueGenerated == ValueGenerated.OnAddOrUpdate)
            {
                // We explicitly check for RowVersion when generation migrations. We therefore handle RowVersion separately from other cases
                // of using CURRENT_TIMESTAMP etc. and we don't generate a XGValueGenerationStrategy.ComputedColumn annotation.
                if (IsCompatibleComputedColumn(property, storeObject, typeMappingSource) &&
                    !property.IsConcurrencyToken)
                {
                    return XGValueGenerationStrategy.ComputedColumn;
                }
            }

            return XGValueGenerationStrategy.None;
        }

        /// <summary>
        ///     Returns the <see cref="XGValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <remarks>
        ///     If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
        /// </remarks>
        /// <param name="overrides">The property overrides.</param>
        /// <returns>The strategy, or <see cref="XGValueGenerationStrategy.None" /> if none was set.</returns>
        public static XGValueGenerationStrategy? GetValueGenerationStrategy(this IReadOnlyRelationalPropertyOverrides overrides)
            => overrides.FindAnnotation(XGAnnotationNames.ValueGenerationStrategy) is { } @override
                ? ObjectToEnumConverter.GetEnumValue<XGValueGenerationStrategy>(@override.Value) ??
                  XGValueGenerationStrategy.None
                : null;

        private static XGValueGenerationStrategy GetDefaultValueGenerationStrategy(IReadOnlyProperty property)
        {
            var modelStrategy = property.DeclaringType.Model.GetValueGenerationStrategy();

            return modelStrategy == XGValueGenerationStrategy.IdentityColumn &&
                   IsCompatibleIdentityColumn(property)
                ? XGValueGenerationStrategy.IdentityColumn
                : XGValueGenerationStrategy.None;
        }

        private static XGValueGenerationStrategy GetDefaultValueGenerationStrategy(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            [CanBeNull] ITypeMappingSource typeMappingSource)
        {
            var modelStrategy = property.DeclaringType.Model.GetValueGenerationStrategy();

            return modelStrategy == XGValueGenerationStrategy.IdentityColumn
                   && IsCompatibleIdentityColumn(property, storeObject, typeMappingSource)
                ? XGValueGenerationStrategy.IdentityColumn
                : XGValueGenerationStrategy.None;
        }

        /// <summary>
        ///     Sets the <see cref="XGValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The strategy to use. </param>
        public static void SetValueGenerationStrategy(
            [NotNull] this IMutableProperty property,
            XGValueGenerationStrategy? value)
            => property.SetOrRemoveAnnotation(
                XGAnnotationNames.ValueGenerationStrategy,
                CheckValueGenerationStrategy(property, value));

        /// <summary>
        ///     Sets the <see cref="XGValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The strategy to use. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static XGValueGenerationStrategy? SetValueGenerationStrategy(
            [NotNull] this IConventionProperty property,
            XGValueGenerationStrategy? value,
            bool fromDataAnnotation = false)
            => (XGValueGenerationStrategy?)property.SetOrRemoveAnnotation(
                    XGAnnotationNames.ValueGenerationStrategy,
                    CheckValueGenerationStrategy(property, value),
                    fromDataAnnotation)
                ?.Value;

        /// <summary>
        ///     Sets the <see cref="XGValueGenerationStrategy" /> to use for the property for a particular table.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The strategy to use.</param>
        /// <param name="storeObject">The identifier of the table containing the column.</param>
        public static void SetValueGenerationStrategy(
            this IMutableProperty property,
            XGValueGenerationStrategy? value,
            in StoreObjectIdentifier storeObject)
            => property.GetOrCreateOverrides(storeObject)
                .SetValueGenerationStrategy(value);

        /// <summary>
        ///     Sets the <see cref="XGValueGenerationStrategy" /> to use for the property for a particular table.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The strategy to use.</param>
        /// <param name="storeObject">The identifier of the table containing the column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static XGValueGenerationStrategy? SetValueGenerationStrategy(
            this IConventionProperty property,
            XGValueGenerationStrategy? value,
            in StoreObjectIdentifier storeObject,
            bool fromDataAnnotation = false)
            => property.GetOrCreateOverrides(storeObject, fromDataAnnotation)
                .SetValueGenerationStrategy(value, fromDataAnnotation);

        /// <summary>
        ///     Sets the <see cref="XGValueGenerationStrategy" /> to use for the property for a particular table.
        /// </summary>
        /// <param name="overrides">The property overrides.</param>
        /// <param name="value">The strategy to use.</param>
        public static void SetValueGenerationStrategy(
            this IMutableRelationalPropertyOverrides overrides,
            XGValueGenerationStrategy? value)
            => overrides.SetOrRemoveAnnotation(
                XGAnnotationNames.ValueGenerationStrategy,
                CheckValueGenerationStrategy(overrides.Property, value));

        /// <summary>
        ///     Sets the <see cref="XGValueGenerationStrategy" /> to use for the property for a particular table.
        /// </summary>
        /// <param name="overrides">The property overrides.</param>
        /// <param name="value">The strategy to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static XGValueGenerationStrategy? SetValueGenerationStrategy(
            this IConventionRelationalPropertyOverrides overrides,
            XGValueGenerationStrategy? value,
            bool fromDataAnnotation = false)
            => (XGValueGenerationStrategy?)overrides.SetOrRemoveAnnotation(
                XGAnnotationNames.ValueGenerationStrategy,
                CheckValueGenerationStrategy(overrides.Property, value),
                fromDataAnnotation)?.Value;

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the <see cref="XGValueGenerationStrategy" />.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="XGValueGenerationStrategy" />.</returns>
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
            this IConventionProperty property)
            => property.FindAnnotation(XGAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the <see cref="XGValueGenerationStrategy" /> for a particular table.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the table containing the column.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="XGValueGenerationStrategy" />.</returns>
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
            this IConventionProperty property,
            in StoreObjectIdentifier storeObject)
            => property.FindOverrides(storeObject)?.GetValueGenerationStrategyConfigurationSource();

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the <see cref="XGValueGenerationStrategy" /> for a particular table.
        /// </summary>
        /// <param name="overrides">The property overrides.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="XGValueGenerationStrategy" />.</returns>
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
            this IConventionRelationalPropertyOverrides overrides)
            => overrides.FindAnnotation(XGAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        private static XGValueGenerationStrategy? CheckValueGenerationStrategy(IReadOnlyProperty property, XGValueGenerationStrategy? value)
        {
            if (value == null)
            {
                return null;
            }

            var propertyType = property.ClrType;

            if (value == XGValueGenerationStrategy.IdentityColumn
                && !IsCompatibleIdentityColumn(property))
            {
                throw new ArgumentException(
                    XGStrings.IdentityBadType(
                        property.Name, property.DeclaringType.DisplayName(), propertyType.ShortDisplayName()));
            }

            if (value == XGValueGenerationStrategy.ComputedColumn
                && !IsCompatibleComputedColumn(property))
            {
                throw new ArgumentException(
                    XGStrings.ComputedBadType(
                        property.Name, property.DeclaringType.DisplayName(), propertyType.ShortDisplayName()));
            }

            return value;
        }

        /// <summary>
        ///     Returns a value indicating whether the property is compatible with <see cref="XGValueGenerationStrategy.IdentityColumn"/>.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> <see langword="true"/> if compatible. </returns>
        public static bool IsCompatibleIdentityColumn(IReadOnlyProperty property)
            => IsCompatibleAutoIncrementColumn(property) ||
               IsCompatibleCurrentTimestampColumn(property);

        private static bool IsCompatibleIdentityColumn(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            [CanBeNull] ITypeMappingSource typeMappingSource)
            => IsCompatibleAutoIncrementColumn(property, storeObject, typeMappingSource) ||
               IsCompatibleCurrentTimestampColumn(property, storeObject, typeMappingSource);

        /// <summary>
        ///     Returns a value indicating whether the property is compatible with an `AUTO_INCREMENT` column.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> <see langword="true"/> if compatible. </returns>
        public static bool IsCompatibleAutoIncrementColumn(IReadOnlyProperty property)
        {
            var valueConverter = property.GetValueConverter() ??
                                 property.FindTypeMapping()?.Converter;

            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();
            return type.IsInteger() ||
                   type.IsEnum ||
                   type == typeof(decimal);
        }

        private static bool IsCompatibleAutoIncrementColumn(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            [CanBeNull] ITypeMappingSource typeMappingSource)
        {
            if (storeObject.StoreObjectType != StoreObjectType.Table)
            {
                return false;
            }

            var valueConverter = property.GetValueConverter() ??
                                 (property.FindRelationalTypeMapping(storeObject) ??
                                  typeMappingSource?.FindMapping((IProperty)property))?.Converter;

            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

            return (type.IsInteger() ||
                    type.IsEnum ||
                    type == typeof(decimal));
        }

        /// <summary>
        ///     Returns a value indicating whether the property is compatible with a `CURRENT_TIMESTAMP` column default.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> <see langword="true"/> if compatible. </returns>
        public static bool IsCompatibleCurrentTimestampColumn(IReadOnlyProperty property)
        {
            var valueConverter = GetConverter(property);
            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();
            return type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset);
        }

        private static bool IsCompatibleCurrentTimestampColumn(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            [CanBeNull] ITypeMappingSource typeMappingSource)
        {
            if (storeObject.StoreObjectType != StoreObjectType.Table)
            {
                return false;
            }

            var valueConverter = GetConverter(property, storeObject, typeMappingSource);
            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

            return type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset);
        }

        /// <summary>
        ///     Returns a value indicating whether the property is compatible with <see cref="XGValueGenerationStrategy.ComputedColumn"/>.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> <see langword="true"/> if compatible. </returns>
        public static bool IsCompatibleComputedColumn(IReadOnlyProperty property)
        {
            var valueConverter = GetConverter(property);
            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

            // RowVersion uses byte[] and the BytesToDateTimeConverter.
            return type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(byte[]) && valueConverter is BytesToDateTimeConverter;
        }

        private static bool IsCompatibleComputedColumn(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource typeMappingSource)
        {
            if (storeObject.StoreObjectType != StoreObjectType.Table)
            {
                return false;
            }

            var valueConverter = property.GetValueConverter() ??
                                 (property.FindRelationalTypeMapping(storeObject) ??
                                  typeMappingSource?.FindMapping((IProperty)property))?.Converter;

            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

            // RowVersion uses byte[] and the BytesToDateTimeConverter.
            return type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(byte[]) && valueConverter is BytesToDateTimeConverter;
        }

        private static ValueConverter GetConverter(IReadOnlyProperty property)
            => property.GetValueConverter() ??
               property.FindTypeMapping()?.Converter;

        private static ValueConverter GetConverter(
            IReadOnlyProperty property,
            StoreObjectIdentifier storeObject,
            [CanBeNull] ITypeMappingSource typeMappingSource)
            => property.GetValueConverter()
               ?? (property.FindRelationalTypeMapping(storeObject)
                   ?? typeMappingSource?.FindMapping((IProperty)property))?.Converter;

        /// <summary>
        /// Returns the name of the charset used by the column of the property.
        /// </summary>
        /// <param name="property">The property of which to get the columns charset from.</param>
        /// <returns>The name of the charset or null, if no explicit charset was set.</returns>
        public static string GetCharSet([NotNull] this IReadOnlyProperty property)
            => (property is RuntimeProperty)
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : property[XGAnnotationNames.CharSet] as string ??
                  property.GetXGLegacyCharSet();

        /// <summary>
        /// Returns the name of the charset used by the column of the property.
        /// </summary>
        /// <param name="property">The property of which to get the columns charset from.</param>
        /// <param name="storeObject">The identifier of the table-like store object containing the column.</param>
        /// <returns>The name of the charset or null, if no explicit charset was set.</returns>
        public static string GetCharSet(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
            => property is RuntimeProperty
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : property.FindAnnotation(XGAnnotationNames.CharSet) is { } annotation
                    ? annotation.Value as string ??
                      property.GetXGLegacyCharSet()
                    : property.FindSharedStoreObjectRootProperty(storeObject)?.GetCharSet(storeObject);

        /// <summary>
        /// Returns the name of the charset used by the column of the property, defined as part of the column type.
        /// </summary>
        /// <remarks>
        /// It was common before 5.0 to specify charsets this way, because there were no character set specific annotations available yet.
        /// Users might still use migrations generated with previous versions and just add newer migrations on top of those.
        /// </remarks>
        /// <param name="property">The property of which to get the columns charset from.</param>
        /// <returns>The name of the charset or null, if no explicit charset was set.</returns>
        internal static string GetXGLegacyCharSet([NotNull] this IReadOnlyProperty property)
        {
            var columnType = property.GetColumnType();

            if (columnType is not null)
            {
                const string characterSet = "character set";
                const string charSet = "charset";

                var characterSetOccurrenceIndex = columnType.IndexOf(characterSet, StringComparison.OrdinalIgnoreCase);
                var clauseLength = characterSet.Length;

                if (characterSetOccurrenceIndex < 0)
                {
                    characterSetOccurrenceIndex = columnType.IndexOf(charSet, StringComparison.OrdinalIgnoreCase);
                    clauseLength = charSet.Length;
                }

                if (characterSetOccurrenceIndex >= 0)
                {
                    var result = string.Concat(
                        columnType.Skip(characterSetOccurrenceIndex + clauseLength)
                            .SkipWhile(c => c == ' ')
                            .TakeWhile(c => c != ' '));

                    if (result.Length > 0)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the name of the charset in use by the column of the property.
        /// </summary>
        /// <param name="property">The property to set the columns charset for.</param>
        /// <param name="charSet">The name of the charset used for the column of the property.</param>
        public static void SetCharSet([NotNull] this IMutableProperty property, string charSet)
            => property.SetOrRemoveAnnotation(XGAnnotationNames.CharSet, charSet);

        /// <summary>
        /// Sets the name of the charset in use by the column of the property.
        /// </summary>
        /// <param name="property">The property to set the columns charset for.</param>
        /// <param name="charSet">The name of the charset used for the column of the property.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static string SetCharSet([NotNull] this IConventionProperty property, string charSet, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(XGAnnotationNames.CharSet, charSet, fromDataAnnotation);

            return charSet;
        }

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for the character set.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the character set.</returns>
        public static ConfigurationSource? GetCharSetConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(XGAnnotationNames.CharSet)?.GetConfigurationSource();

        /// <summary>
        /// Returns the name of the collation used by the column of the property.
        /// </summary>
        /// <param name="property">The property of which to get the columns collation from.</param>
        /// <returns>The name of the collation or null, if no explicit collation was set.</returns>
#pragma warning disable 618
        internal static string GetXGLegacyCollation([NotNull] this IReadOnlyProperty property)
            => property[XGAnnotationNames.Collation] as string;
#pragma warning restore 618

        /// <summary>
        /// Returns the Spatial Reference System Identifier (SRID) used by the column of the property.
        /// </summary>
        /// <param name="property">The property of which to get the columns SRID from.</param>
        /// <returns>The SRID or null, if no explicit SRID has been set.</returns>
        public static int? GetSpatialReferenceSystem([NotNull] this IReadOnlyProperty property)
            => (property is RuntimeProperty)
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : (int?)property[XGAnnotationNames.SpatialReferenceSystemId];

        /// <summary>
        /// Returns the Spatial Reference System Identifier (SRID) used by the column of the property.
        /// </summary>
        /// <param name="property">The property of which to get the columns SRID from.</param>
        /// <param name="storeObject">The identifier of the table-like store object containing the column.</param>
        /// <returns>The SRID or null, if no explicit SRID has been set.</returns>
        public static int? GetSpatialReferenceSystem(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
            => property is RuntimeProperty
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : property.FindAnnotation(XGAnnotationNames.SpatialReferenceSystemId) is { } annotation
                    ? (int?)annotation.Value
                    : property.FindSharedStoreObjectRootProperty(storeObject)?.GetSpatialReferenceSystem(storeObject);

        /// <summary>
        /// Sets the Spatial Reference System Identifier (SRID) in use by the column of the property.
        /// </summary>
        /// <param name="property">The property to set the columns SRID for.</param>
        /// <param name="srid">The SRID to configure for the property's column.</param>
        public static void SetSpatialReferenceSystem([NotNull] this IMutableProperty property, int? srid)
            => property.SetOrRemoveAnnotation(XGAnnotationNames.SpatialReferenceSystemId, srid);

        /// <summary>
        /// Sets the Spatial Reference System Identifier (SRID) in use by the column of the property.
        /// </summary>
        /// <param name="property">The property to set the columns SRID for.</param>
        /// <param name="srid">The SRID to configure for the property's column.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static int? SetSpatialReferenceSystem([NotNull] this IConventionProperty property, int? srid, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(XGAnnotationNames.SpatialReferenceSystemId, srid, fromDataAnnotation);

            return srid;
        }

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for the Spatial Reference System Identifier (SRID).
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the Spatial Reference System Identifier (SRID).</returns>
        public static ConfigurationSource? GetSpatialReferenceSystemConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(XGAnnotationNames.SpatialReferenceSystemId)?.GetConfigurationSource();
    }
}
