// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Property extension methods for SQL Server-specific metadata.
    /// </summary>
    public static class SqlServerPropertyExtensions
    {
        /// <summary>
        ///     Returns the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The name to use for the hi-lo sequence. </returns>
        public static string? GetHiLoSequenceName(this IReadOnlyProperty property)
            => (string?)property[SqlServerAnnotationNames.HiLoSequenceName];

        /// <summary>
        ///     Returns the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The name to use for the hi-lo sequence. </returns>
        public static string? GetHiLoSequenceName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceName);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            return property.FindSharedStoreObjectRootProperty(storeObject)?.GetHiLoSequenceName(storeObject);
        }

        /// <summary>
        ///     Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The sequence name to use. </param>
        public static void SetHiLoSequenceName(this IMutableProperty property, string? name)
            => property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The sequence name to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetHiLoSequenceName(
            this IConventionProperty property,
            string? name,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence name.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the hi-lo sequence name. </returns>
        public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The schema to use for the hi-lo sequence. </returns>
        public static string? GetHiLoSequenceSchema(this IReadOnlyProperty property)
            => (string?)property[SqlServerAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        ///     Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The schema to use for the hi-lo sequence. </returns>
        public static string? GetHiLoSequenceSchema(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            return property.FindSharedStoreObjectRootProperty(storeObject)?.GetHiLoSequenceSchema(storeObject);
        }

        /// <summary>
        ///     Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="schema"> The schema to use. </param>
        public static void SetHiLoSequenceSchema(this IMutableProperty property, string? schema)
            => property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(schema, nameof(schema)));

        /// <summary>
        ///     Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="schema"> The schema to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetHiLoSequenceSchema(
            this IConventionProperty property,
            string? schema,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(schema, nameof(schema)),
                fromDataAnnotation);

            return schema;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence schema.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the hi-lo sequence schema. </returns>
        public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
        public static IReadOnlySequence? FindHiLoSequence(this IReadOnlyProperty property)
        {
            var model = property.DeclaringEntityType.Model;

            var sequenceName = property.GetHiLoSequenceName()
                ?? model.GetHiLoSequenceName();

            var sequenceSchema = property.GetHiLoSequenceSchema()
                ?? model.GetHiLoSequenceSchema();

            return model.FindSequence(sequenceName, sequenceSchema);
        }

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
        public static IReadOnlySequence? FindHiLoSequence(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var model = property.DeclaringEntityType.Model;

            var sequenceName = property.GetHiLoSequenceName(storeObject)
                ?? model.GetHiLoSequenceName();

            var sequenceSchema = property.GetHiLoSequenceSchema(storeObject)
                ?? model.GetHiLoSequenceSchema();

            return model.FindSequence(sequenceName, sequenceSchema);
        }

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
        public static ISequence? FindHiLoSequence(this IProperty property)
            => (ISequence?)((IReadOnlyProperty)property).FindHiLoSequence();

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
        public static ISequence? FindHiLoSequence(this IProperty property, in StoreObjectIdentifier storeObject)
            => (ISequence?)((IReadOnlyProperty)property).FindHiLoSequence(storeObject);

        /// <summary>
        ///     Returns the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The identity seed. </returns>
        public static int? GetIdentitySeed(this IReadOnlyProperty property)
            => property is RuntimeProperty
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (int?)property[SqlServerAnnotationNames.IdentitySeed];

        /// <summary>
        ///     Returns the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The identity seed. </returns>
        public static int? GetIdentitySeed(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(SqlServerAnnotationNames.IdentitySeed);
            if (annotation != null)
            {
                return (int?)annotation.Value;
            }

            return property.FindSharedStoreObjectRootProperty(storeObject)?.GetIdentitySeed(storeObject);
        }

        /// <summary>
        ///     Sets the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="seed"> The value to set. </param>
        public static void SetIdentitySeed(this IMutableProperty property, int? seed)
            => property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentitySeed,
                seed);

        /// <summary>
        ///     Sets the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="seed"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static int? SetIdentitySeed(
            this IConventionProperty property,
            int? seed,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentitySeed,
                seed,
                fromDataAnnotation);

            return seed;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the identity seed. </returns>
        public static ConfigurationSource? GetIdentitySeedConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.IdentitySeed)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The identity increment. </returns>
        public static int? GetIdentityIncrement(this IReadOnlyProperty property)
            => property is RuntimeProperty
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (int?)property[SqlServerAnnotationNames.IdentityIncrement];

        /// <summary>
        ///     Returns the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The identity increment. </returns>
        public static int? GetIdentityIncrement(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(SqlServerAnnotationNames.IdentityIncrement);
            if (annotation != null)
            {
                return (int?)annotation.Value;
            }

            return property.FindSharedStoreObjectRootProperty(storeObject)?.GetIdentityIncrement(storeObject);
        }

        /// <summary>
        ///     Sets the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="increment"> The value to set. </param>
        public static void SetIdentityIncrement(this IMutableProperty property, int? increment)
            => property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentityIncrement,
                increment);

        /// <summary>
        ///     Sets the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="increment"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static int? SetIdentityIncrement(
            this IConventionProperty property,
            int? increment,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentityIncrement,
                increment,
                fromDataAnnotation);

            return increment;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the identity increment. </returns>
        public static ConfigurationSource? GetIdentityIncrementConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.IdentityIncrement)?.GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Returns the <see cref="SqlServerValueGenerationStrategy" /> to use for the property.
        ///     </para>
        ///     <para>
        ///         If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The strategy, or <see cref="SqlServerValueGenerationStrategy.None" /> if none was set. </returns>
        public static SqlServerValueGenerationStrategy GetValueGenerationStrategy(this IReadOnlyProperty property)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy);
            if (annotation != null)
            {
                return (SqlServerValueGenerationStrategy?)annotation.Value ?? SqlServerValueGenerationStrategy.None;
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.IsForeignKey()
                || property.TryGetDefaultValue(out _)
                || property.GetDefaultValueSql() != null
                || property.GetComputedColumnSql() != null)
            {
                return SqlServerValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property);
        }

        /// <summary>
        ///     <para>
        ///         Returns the <see cref="SqlServerValueGenerationStrategy" /> to use for the property.
        ///     </para>
        ///     <para>
        ///         If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The strategy, or <see cref="SqlServerValueGenerationStrategy.None" /> if none was set. </returns>
        public static SqlServerValueGenerationStrategy GetValueGenerationStrategy(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
            => GetValueGenerationStrategy(property, storeObject, null);

        internal static SqlServerValueGenerationStrategy GetValueGenerationStrategy(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource? typeMappingSource)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy);
            if (annotation != null)
            {
                return (SqlServerValueGenerationStrategy?)annotation.Value ?? SqlServerValueGenerationStrategy.None;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.GetValueGenerationStrategy(storeObject)
                    == SqlServerValueGenerationStrategy.IdentityColumn
                    && !property.GetContainingForeignKeys().Any(fk => !fk.IsBaseLinking())
                        ? SqlServerValueGenerationStrategy.IdentityColumn
                        : SqlServerValueGenerationStrategy.None;
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.GetContainingForeignKeys().Any(fk => !fk.IsBaseLinking())
                || property.TryGetDefaultValue(storeObject, out _)
                || property.GetDefaultValueSql(storeObject) != null
                || property.GetComputedColumnSql(storeObject) != null)
            {
                return SqlServerValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property, storeObject, typeMappingSource);
        }

        private static SqlServerValueGenerationStrategy GetDefaultValueGenerationStrategy(IReadOnlyProperty property)
        {
            var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

            if (modelStrategy == SqlServerValueGenerationStrategy.SequenceHiLo
                && IsCompatibleWithValueGeneration(property))
            {
                return SqlServerValueGenerationStrategy.SequenceHiLo;
            }

            return modelStrategy == SqlServerValueGenerationStrategy.IdentityColumn
                && IsCompatibleWithValueGeneration(property)
                    ? SqlServerValueGenerationStrategy.IdentityColumn
                    : SqlServerValueGenerationStrategy.None;
        }

        private static SqlServerValueGenerationStrategy GetDefaultValueGenerationStrategy(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource? typeMappingSource)
        {
            var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

            if (modelStrategy == SqlServerValueGenerationStrategy.SequenceHiLo
                && IsCompatibleWithValueGeneration(property, storeObject, typeMappingSource))
            {
                return SqlServerValueGenerationStrategy.SequenceHiLo;
            }

            return modelStrategy == SqlServerValueGenerationStrategy.IdentityColumn
                && IsCompatibleWithValueGeneration(property, storeObject, typeMappingSource)
                    ? SqlServerValueGenerationStrategy.IdentityColumn
                    : SqlServerValueGenerationStrategy.None;
        }

        /// <summary>
        ///     Sets the <see cref="SqlServerValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The strategy to use. </param>
        public static void SetValueGenerationStrategy(
            this IMutableProperty property,
            SqlServerValueGenerationStrategy? value)
        {
            CheckValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value);
        }

        /// <summary>
        ///     Sets the <see cref="SqlServerValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The strategy to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static SqlServerValueGenerationStrategy? SetValueGenerationStrategy(
            this IConventionProperty property,
            SqlServerValueGenerationStrategy? value,
            bool fromDataAnnotation = false)
        {
            CheckValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

            return value;
        }

        private static void CheckValueGenerationStrategy(IReadOnlyProperty property, SqlServerValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = property.ClrType;

                if (value == SqlServerValueGenerationStrategy.IdentityColumn
                    && !IsCompatibleWithValueGeneration(property))
                {
                    throw new ArgumentException(
                        SqlServerStrings.IdentityBadType(
                            property.Name, property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                }

                if (value == SqlServerValueGenerationStrategy.SequenceHiLo
                    && !IsCompatibleWithValueGeneration(property))
                {
                    throw new ArgumentException(
                        SqlServerStrings.SequenceBadType(
                            property.Name, property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                }
            }
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the <see cref="SqlServerValueGenerationStrategy" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the <see cref="SqlServerValueGenerationStrategy" />. </returns>
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
            this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value indicating whether the property is compatible with any <see cref="SqlServerValueGenerationStrategy" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> <see langword="true" /> if compatible. </returns>
        public static bool IsCompatibleWithValueGeneration(IReadOnlyProperty property)
        {
            var type = property.ClrType;

            return (type.IsInteger()
                    || type == typeof(decimal))
                && (property.GetValueConverter()
                    ?? property.FindTypeMapping()?.Converter)
                == null;
        }

        private static bool IsCompatibleWithValueGeneration(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource? typeMappingSource)
        {
            var type = property.ClrType;

            return (type.IsInteger()
                    || type == typeof(decimal))
                && (property.GetValueConverter()
                    ?? (property.FindRelationalTypeMapping(storeObject)
                        ?? typeMappingSource?.FindMapping((IProperty)property))?.Converter)
                == null;
        }

        /// <summary>
        ///     Returns a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> <see langword="true" /> if the property's column is sparse. </returns>
        public static bool? IsSparse(this IReadOnlyProperty property)
            => property is RuntimeProperty
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (bool?)property[SqlServerAnnotationNames.Sparse];

        /// <summary>
        ///     Returns a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> <see langword="true" /> if the property's column is sparse. </returns>
        public static bool? IsSparse(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(SqlServerAnnotationNames.Sparse);
            if (annotation != null)
            {
                return (bool?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.IsSparse(storeObject)
                : null;
        }

        /// <summary>
        ///     Sets a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="sparse"> The value to set. </param>
        public static void SetIsSparse(this IMutableProperty property, bool? sparse)
            => property.SetAnnotation(SqlServerAnnotationNames.Sparse, sparse);

        /// <summary>
        ///     Sets a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="sparse"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsSparse(
            this IConventionProperty property,
            bool? sparse,
            bool fromDataAnnotation = false)
        {
            property.SetAnnotation(
                SqlServerAnnotationNames.Sparse,
                sparse,
                fromDataAnnotation);

            return sparse;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for whether the property's column is sparse.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for whether the property's column is sparse. </returns>
        public static ConfigurationSource? GetIsSparseConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.Sparse)?.GetConfigurationSource();
    }
}
