// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Model extension methods for SQL Server-specific metadata.
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
        public static string GetHiLoSequenceName(this IReadOnlyModel model)
            => (string?)model[SqlServerAnnotationNames.HiLoSequenceName]
                ?? DefaultHiLoSequenceName;

        /// <summary>
        ///     Sets the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The value to set. </param>
        public static void SetHiLoSequenceName(this IMutableModel model, string? name)
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
        public static string? SetHiLoSequenceName(
            this IConventionModel model,
            string? name,
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
        public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the schema to use for the default hi-lo sequence.
        ///     <see cref="SqlServerPropertyBuilderExtensions.UseHiLo" />
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The schema to use for the default hi-lo sequence. </returns>
        public static string? GetHiLoSequenceSchema(this IReadOnlyModel model)
            => (string?)model[SqlServerAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        ///     Sets the schema to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetHiLoSequenceSchema(this IMutableModel model, string? value)
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
        public static string? SetHiLoSequenceSchema(
            this IConventionModel model,
            string? value,
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
        public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the default identity seed.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default identity seed. </returns>
        public static long GetIdentitySeed(this IReadOnlyModel model)
            => model is RuntimeModel
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (long?)model[SqlServerAnnotationNames.IdentitySeed] ?? 1;

        /// <summary>
        ///     Sets the default identity seed.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="seed"> The value to set. </param>
        public static void SetIdentitySeed(this IMutableModel model, long? seed)
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
        public static long? SetIdentitySeed(this IConventionModel model, long? seed, bool fromDataAnnotation = false)
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
        public static ConfigurationSource? GetIdentitySeedConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.IdentitySeed)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the default identity increment.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default identity increment. </returns>
        public static int GetIdentityIncrement(this IReadOnlyModel model)
            => model is RuntimeModel
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (int?)model[SqlServerAnnotationNames.IdentityIncrement] ?? 1;

        /// <summary>
        ///     Sets the default identity increment.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="increment"> The value to set. </param>
        public static void SetIdentityIncrement(this IMutableModel model, int? increment)
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
            this IConventionModel model,
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
        public static ConfigurationSource? GetIdentityIncrementConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.IdentityIncrement)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model, unless the property has a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default <see cref="SqlServerValueGenerationStrategy" />. </returns>
        public static SqlServerValueGenerationStrategy? GetValueGenerationStrategy(this IReadOnlyModel model)
            => (SqlServerValueGenerationStrategy?)model[SqlServerAnnotationNames.ValueGenerationStrategy];

        /// <summary>
        ///     Sets the <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model that don't have a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetValueGenerationStrategy(
            this IMutableModel model,
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
            this IConventionModel model,
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
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the maximum size of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The maximum size of the database. </returns>
        public static string? GetDatabaseMaxSize(this IReadOnlyModel model)
            => model is RuntimeModel
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)model[SqlServerAnnotationNames.MaxDatabaseSize];

        /// <summary>
        ///     Sets the maximum size of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDatabaseMaxSize(this IMutableModel model, string? value)
            => model.SetOrRemoveAnnotation(SqlServerAnnotationNames.MaxDatabaseSize, value);

        /// <summary>
        ///     Sets the maximum size of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetDatabaseMaxSize(
            this IConventionModel model,
            string? value,
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
        public static ConfigurationSource? GetDatabaseMaxSizeConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.MaxDatabaseSize)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the service tier of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The service tier of the database. </returns>
        public static string? GetServiceTierSql(this IReadOnlyModel model)
            => model is RuntimeModel
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)model[SqlServerAnnotationNames.ServiceTierSql];

        /// <summary>
        ///     Sets the service tier of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetServiceTierSql(this IMutableModel model, string? value)
            => model.SetOrRemoveAnnotation(SqlServerAnnotationNames.ServiceTierSql, value);

        /// <summary>
        ///     Sets the service tier of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetServiceTierSql(
            this IConventionModel model,
            string? value,
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
        public static ConfigurationSource? GetServiceTierSqlConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.ServiceTierSql)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the performance level of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The performance level of the database. </returns>
        public static string? GetPerformanceLevelSql(this IReadOnlyModel model)
            => model is RuntimeModel
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)model[SqlServerAnnotationNames.PerformanceLevelSql];

        /// <summary>
        ///     Sets the performance level of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetPerformanceLevelSql(this IMutableModel model, string? value)
            => model.SetOrRemoveAnnotation(SqlServerAnnotationNames.PerformanceLevelSql, value);

        /// <summary>
        ///     Sets the performance level of the database.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetPerformanceLevelSql(
            this IConventionModel model,
            string? value,
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
        public static ConfigurationSource? GetPerformanceLevelSqlConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(SqlServerAnnotationNames.PerformanceLevelSql)?.GetConfigurationSource();
    }
}
