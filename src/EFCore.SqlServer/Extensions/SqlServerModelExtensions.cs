// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IModel" /> for SQL Server-specific metadata.
    /// </summary>
    public static class SqlServerModelExtensions
    {
        /// <summary>
        ///     The default name for the hi-lo sequence.
        /// </summary>
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        /// <summary>
        ///     Returns the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The name to use for the default hi-lo sequence. </returns>
        public static string GetHiLoSequenceName([NotNull] this IModel model)
            => (string)model[SqlServerAnnotationNames.HiLoSequenceName]
                ?? DefaultHiLoSequenceName;

        /// <summary>
        ///     Sets the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The value to set. </param>
        public static void SetHiLoSequenceName([NotNull] this IMutableModel model, [CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.HiLoSequenceName, name);
        }

        /// <summary>
        ///     Sets the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetHiLoSequenceName(
            [NotNull] this IConventionModel model,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.HiLoSequenceName, name, fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default hi-lo sequence name. </returns>
        public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the schema to use for the default hi-lo sequence.
        ///     <see cref="SqlServerPropertyBuilderExtensions.UseHiLo" />
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The schema to use for the default hi-lo sequence. </returns>
        public static string GetHiLoSequenceSchema([NotNull] this IModel model)
            => (string)model[SqlServerAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        ///     Sets the schema to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetHiLoSequenceSchema([NotNull] this IMutableModel model, [CanBeNull] string value)
        {
            Check.NullButNotEmpty(value, nameof(value));

            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, value);
        }

        /// <summary>
        ///     Sets the schema to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetHiLoSequenceSchema(
            [NotNull] this IConventionModel model,
            [CanBeNull] string value,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(value, nameof(value));

            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, value, fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence schema.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default hi-lo sequence schema. </returns>
        public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the default identity seed.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default identity seed. </returns>
        public static int GetIdentitySeed([NotNull] this IModel model)
            => (int?)model[SqlServerAnnotationNames.IdentitySeed] ?? 1;

        /// <summary>
        ///     Sets the default identity seed.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="seed"> The value to set. </param>
        public static void SetIdentitySeed([NotNull] this IMutableModel model, int? seed)
            => model.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentitySeed,
                seed);

        /// <summary>
        ///     Sets the default identity seed.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="seed"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static int? SetIdentitySeed([NotNull] this IConventionModel model, int? seed, bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentitySeed,
                seed,
                fromDataAnnotation);

            return seed;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default schema.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default schema. </returns>
        public static ConfigurationSource? GetIdentitySeedConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.IdentitySeed)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the default identity increment.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default identity increment. </returns>
        public static int GetIdentityIncrement([NotNull] this IModel model)
            => (int?)model[SqlServerAnnotationNames.IdentityIncrement] ?? 1;

        /// <summary>
        ///     Sets the default identity increment.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="increment"> The value to set. </param>
        public static void SetIdentityIncrement([NotNull] this IMutableModel model, int? increment)
            => model.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentityIncrement,
                increment);

        /// <summary>
        ///     Sets the default identity increment.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="increment"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static int? SetIdentityIncrement(
            [NotNull] this IConventionModel model,
            int? increment,
            bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(
                SqlServerAnnotationNames.IdentityIncrement,
                increment,
                fromDataAnnotation);

            return increment;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default identity increment.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default identity increment. </returns>
        public static ConfigurationSource? GetIdentityIncrementConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.IdentityIncrement)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model, unless the property has a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default <see cref="SqlServerValueGenerationStrategy" />. </returns>
        public static SqlServerValueGenerationStrategy? GetValueGenerationStrategy([NotNull] this IModel model)
            => (SqlServerValueGenerationStrategy?)model[SqlServerAnnotationNames.ValueGenerationStrategy];

        /// <summary>
        ///     Sets the <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model that don't have a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetValueGenerationStrategy(
            [NotNull] this IMutableModel model,
            SqlServerValueGenerationStrategy? value)
            => model.SetOrRemoveAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value);

        /// <summary>
        ///     Sets the <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model that don't have a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static SqlServerValueGenerationStrategy? SetValueGenerationStrategy(
            [NotNull] this IConventionModel model,
            SqlServerValueGenerationStrategy? value,
            bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default <see cref="SqlServerValueGenerationStrategy" />.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default <see cref="SqlServerValueGenerationStrategy" />. </returns>
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the maximum size of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The maximum size of the database. </returns>
        public static string GetDatabaseMaxSize([NotNull] this IModel model)
            => (string)model[SqlServerAnnotationNames.MaxDatabaseSize];

        /// <summary>
        ///     Sets the maximum size of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDatabaseMaxSize([NotNull] this IMutableModel model, [CanBeNull] string value)
            => model.SetOrRemoveAnnotation(SqlServerAnnotationNames.MaxDatabaseSize, value);

        /// <summary>
        ///     Sets the maximum size of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetDatabaseMaxSize(
            [NotNull] this IConventionModel model,
            [CanBeNull] string value,
            bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.MaxDatabaseSize, value, fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the maximum size of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the maximum size of the database. </returns>
        public static ConfigurationSource? GetDatabaseMaxSizeConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.MaxDatabaseSize)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the service tier of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The service tier of the database. </returns>
        public static string GetServiceTierSql([NotNull] this IModel model)
            => (string)model[SqlServerAnnotationNames.ServiceTierSql];

        /// <summary>
        ///     Sets the service tier of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetServiceTierSql([NotNull] this IMutableModel model, [CanBeNull] string value)
            => model.SetOrRemoveAnnotation(SqlServerAnnotationNames.ServiceTierSql, value);

        /// <summary>
        ///     Sets the service tier of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetServiceTierSql(
            [NotNull] this IConventionModel model,
            [CanBeNull] string value,
            bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.ServiceTierSql, value, fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the service tier of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the service tier of the database. </returns>
        public static ConfigurationSource? GetServiceTierSqlConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.ServiceTierSql)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the performance level of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The performance level of the database. </returns>
        public static string GetPerformanceLevelSql([NotNull] this IModel model)
            => (string)model[SqlServerAnnotationNames.PerformanceLevelSql];

        /// <summary>
        ///     Sets the performance level of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetPerformanceLevelSql([NotNull] this IMutableModel model, [CanBeNull] string value)
            => model.SetOrRemoveAnnotation(SqlServerAnnotationNames.PerformanceLevelSql, value);

        /// <summary>
        ///     Sets the performance level of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetPerformanceLevelSql(
            [NotNull] this IConventionModel model,
            [CanBeNull] string value,
            bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(SqlServerAnnotationNames.PerformanceLevelSql, value, fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the performance level of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the performance level of the database. </returns>
        public static ConfigurationSource? GetPerformanceLevelSqlConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.PerformanceLevelSql)?.GetConfigurationSource();
    }
}
