// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for SQL Server-specific metadata.
    /// </summary>
    public static class SqlServerPropertyExtensions
    {
        /// <summary>
        ///     Returns the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The name to use for the hi-lo sequence. </returns>
        public static string GetHiLoSequenceName([NotNull] this IProperty property)
            => (string)property[SqlServerAnnotationNames.HiLoSequenceName];

        /// <summary>
        ///     Returns the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The name to use for the hi-lo sequence. </returns>
        public static string GetHiLoSequenceName([NotNull] this IProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceName);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.GetHiLoSequenceName(storeObject)
                : null;
        }

        /// <summary>
        ///     Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The sequence name to use. </param>
        public static void SetHiLoSequenceName([NotNull] this IMutableProperty property, [CanBeNull] string name)
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
        public static string SetHiLoSequenceName(
            [NotNull] this IConventionProperty property,
            [CanBeNull] string name,
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
        public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The schema to use for the hi-lo sequence. </returns>
        public static string GetHiLoSequenceSchema([NotNull] this IProperty property)
            => (string)property[SqlServerAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        ///     Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The schema to use for the hi-lo sequence. </returns>
        public static string GetHiLoSequenceSchema([NotNull] this IProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.GetHiLoSequenceSchema(storeObject)
                : null;
        }

        /// <summary>
        ///     Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="schema"> The schema to use. </param>
        public static void SetHiLoSequenceSchema([NotNull] this IMutableProperty property, [CanBeNull] string schema)
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
        public static string SetHiLoSequenceSchema(
            [NotNull] this IConventionProperty property,
            [CanBeNull] string schema,
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
        public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
        public static ISequence FindHiLoSequence([NotNull] this IProperty property)
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
        public static ISequence FindHiLoSequence([NotNull] this IProperty property, in StoreObjectIdentifier storeObject)
        {
            var model = property.DeclaringEntityType.Model;

            var sequenceName = property.GetHiLoSequenceName(storeObject)
                ?? model.GetHiLoSequenceName();

            var sequenceSchema = property.GetHiLoSequenceSchema(storeObject)
                ?? model.GetHiLoSequenceSchema();

            return model.FindSequence(sequenceName, sequenceSchema);
        }

        /// <summary>
        ///     Returns the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The identity seed. </returns>
        public static int? GetIdentitySeed([NotNull] this IProperty property)
            => (int?)property[SqlServerAnnotationNames.IdentitySeed];

        /// <summary>
        ///     Returns the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The identity seed. </returns>
        public static int? GetIdentitySeed([NotNull] this IProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.IdentitySeed);
            if (annotation != null)
            {
                return (int?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.GetIdentitySeed(storeObject)
                : null;
        }

        /// <summary>
        ///     Sets the identity seed.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="seed"> The value to set. </param>
        public static void SetIdentitySeed([NotNull] this IMutableProperty property, int? seed)
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
            [NotNull] this IConventionProperty property,
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
        public static ConfigurationSource? GetIdentitySeedConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.IdentitySeed)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The identity increment. </returns>
        public static int? GetIdentityIncrement([NotNull] this IProperty property)
            => (int?)property[SqlServerAnnotationNames.IdentityIncrement];

        /// <summary>
        ///     Returns the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The identity increment. </returns>
        public static int? GetIdentityIncrement([NotNull] this IProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.IdentityIncrement);
            if (annotation != null)
            {
                return (int?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.GetIdentityIncrement(storeObject)
                : null;
        }

        /// <summary>
        ///     Sets the identity increment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="increment"> The value to set. </param>
        public static void SetIdentityIncrement([NotNull] this IMutableProperty property, int? increment)
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
            [NotNull] this IConventionProperty property,
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
        public static ConfigurationSource? GetIdentityIncrementConfigurationSource([NotNull] this IConventionProperty property)
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
        public static SqlServerValueGenerationStrategy GetValueGenerationStrategy([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy);
            if (annotation != null)
            {
                return (SqlServerValueGenerationStrategy)annotation.Value;
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.IsForeignKey()
                || property.GetDefaultValue() != null
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
            [NotNull] this IProperty property,
            in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy);
            if (annotation != null)
            {
                return (SqlServerValueGenerationStrategy)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.GetValueGenerationStrategy(storeObject)
                    == SqlServerValueGenerationStrategy.IdentityColumn
                        ? SqlServerValueGenerationStrategy.IdentityColumn
                        : SqlServerValueGenerationStrategy.None;
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.GetContainingForeignKeys().Any(fk => !fk.IsBaseLinking())
                || property.GetDefaultValue(storeObject) != null
                || property.GetDefaultValueSql(storeObject) != null
                || property.GetComputedColumnSql(storeObject) != null)
            {
                return SqlServerValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property);
        }

        private static SqlServerValueGenerationStrategy GetDefaultValueGenerationStrategy(IProperty property)
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

        /// <summary>
        ///     Sets the <see cref="SqlServerValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The strategy to use. </param>
        public static void SetValueGenerationStrategy(
            [NotNull] this IMutableProperty property,
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
            [NotNull] this IConventionProperty property,
            SqlServerValueGenerationStrategy? value,
            bool fromDataAnnotation = false)
        {
            CheckValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

            return value;
        }

        private static void CheckValueGenerationStrategy(IProperty property, SqlServerValueGenerationStrategy? value)
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
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value indicating whether the property is compatible with any <see cref="SqlServerValueGenerationStrategy" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> <see langword="true" /> if compatible. </returns>
        public static bool IsCompatibleWithValueGeneration([NotNull] IProperty property)
        {
            var type = property.ClrType;

            return (type.IsInteger()
                    || type == typeof(decimal))
                && (property.GetValueConverter()
                    ?? property.FindTypeMapping()?.Converter)
                == null;
        }
    }
}
