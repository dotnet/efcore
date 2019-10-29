// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class SqlServerPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder UseHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name ??= SqlServerModelExtensions.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            if (model.FindSequence(name, schema) == null)
            {
                model.AddSequence(name, schema).IncrementBy = 10;
            }

            property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
            property.SetHiLoSequenceName(name);
            property.SetHiLoSequenceSchema(schema);
            property.SetIdentitySeed(null);
            property.SetIdentityIncrement(null);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> UseHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)UseHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        ///     Configures the database sequence used for the hi-lo pattern to generate values for the key property,
        ///     when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static IConventionSequenceBuilder HasHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetHiLoSequence(name, schema, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetHiLoSequenceName(name, fromDataAnnotation);
            propertyBuilder.Metadata.SetHiLoSequenceSchema(schema, fromDataAnnotation);

            return name == null
                ? null
                : propertyBuilder.Metadata.DeclaringEntityType.Model.Builder.HasSequence(name, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given name and schema can be set for the hi-lo sequence. </returns>
        public static bool CanSetHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
                && propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="seed"> The value that is used for the very first row loaded into the table. </param>
        /// <param name="increment"> The incremental value that is added to the identity value of the previous row that was loaded. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder UseIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder,
            int seed = 1,
            int increment = 1)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
            property.SetIdentitySeed(seed);
            property.SetIdentityIncrement(increment);
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="seed"> The value that is used for the very first row loaded into the table. </param>
        /// <param name="increment"> The incremental value that is added to the identity value of the previous row that was loaded. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            int seed = 1,
            int increment = 1)
            => (PropertyBuilder<TProperty>)UseIdentityColumn((PropertyBuilder)propertyBuilder, seed, increment);

        /// <summary>
        ///     Configures the seed for SQL Server IDENTITY.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="seed"> The value that is used for the very first row loaded into the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasIdentityColumnSeed(
            [NotNull] this IConventionPropertyBuilder propertyBuilder, int? seed, bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetIdentityColumnSeed(seed, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetIdentitySeed(seed, fromDataAnnotation);
                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as the seed for SQL Server IDENTITY.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="seed"> The value that is used for the very first row loaded into the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given value can be set as the seed for SQL Server IDENTITY. </returns>
        public static bool CanSetIdentityColumnSeed(
            [NotNull] this IConventionPropertyBuilder propertyBuilder, int? seed, bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.IdentitySeed, seed, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the increment for SQL Server IDENTITY.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="increment"> The incremental value that is added to the identity value of the previous row that was loaded. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasIdentityColumnIncrement(
            [NotNull] this IConventionPropertyBuilder propertyBuilder, int? increment, bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetIdentityColumnIncrement(increment, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetIdentityIncrement(increment, fromDataAnnotation);
                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as the increment for SQL Server IDENTITY.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="increment"> The incremental value that is added to the identity value of the previous row that was loaded. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given value can be set as the default increment for SQL Server IDENTITY. </returns>
        public static bool CanSetIdentityColumnIncrement(
            [NotNull] this IConventionPropertyBuilder propertyBuilder, int? increment, bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.IdentityIncrement, increment, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the value generation strategy for the key property, when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="valueGenerationStrategy"> The value generation strategy. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            SqlServerValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetAnnotation(
                SqlServerAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
                if (valueGenerationStrategy != SqlServerValueGenerationStrategy.IdentityColumn)
                {
                    propertyBuilder.HasIdentityColumnSeed(null, fromDataAnnotation);
                    propertyBuilder.HasIdentityColumnIncrement(null, fromDataAnnotation);
                }

                if (valueGenerationStrategy != SqlServerValueGenerationStrategy.SequenceHiLo)
                {
                    propertyBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
                }

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as the value generation strategy.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="valueGenerationStrategy"> The value generation strategy. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given value can be set as the default value generation strategy. </returns>
        public static bool CanSetValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            SqlServerValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return (valueGenerationStrategy == null
                    || SqlServerPropertyExtensions.IsCompatibleWithValueGeneration(propertyBuilder.Metadata))
                && propertyBuilder.CanSetAnnotation(
                    SqlServerAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        [Obsolete("Use UseHiLo")]
        public static PropertyBuilder ForSqlServerUseSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => propertyBuilder.UseHiLo(name, schema);

        /// <summary>
        ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        [Obsolete("Use UseHiLo")]
        public static PropertyBuilder<TProperty> ForSqlServerUseSequenceHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => propertyBuilder.UseHiLo(name, schema);

        /// <summary>
        ///     Configures the database sequence used for the hi-lo pattern to generate values for the key property,
        ///     when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        [Obsolete("Use HasHiLoSequence")]
        public static IConventionSequenceBuilder ForSqlServerHasHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
            => propertyBuilder.HasHiLoSequence(name, schema);

        /// <summary>
        ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="seed"> The value that is used for the very first row loaded into the table. </param>
        /// <param name="increment"> The incremental value that is added to the identity value of the previous row that was loaded. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        [Obsolete("Use UseIdentityColumn")]
        public static PropertyBuilder UseSqlServerIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder,
            int seed = 1,
            int increment = 1)
            => propertyBuilder.UseIdentityColumn(seed, increment);

        /// <summary>
        ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
        ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="seed"> The value that is used for the very first row loaded into the table. </param>
        /// <param name="increment"> The incremental value that is added to the identity value of the previous row that was loaded. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        [Obsolete("Use UseIdentityColumn")]
        public static PropertyBuilder<TProperty> UseSqlServerIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            int seed = 1,
            int increment = 1)
            => propertyBuilder.UseIdentityColumn(seed, increment);

        /// <summary>
        ///     Configures the seed for SQL Server IDENTITY.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="seed"> The value that is used for the very first row loaded into the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use HasIdentityColumnSeed")]
        public static IConventionPropertyBuilder ForSqlServerHasIdentitySeed(
            [NotNull] this IConventionPropertyBuilder propertyBuilder, int? seed, bool fromDataAnnotation = false)
            => propertyBuilder.HasIdentityColumnSeed(seed, fromDataAnnotation);

        /// <summary>
        ///     Configures the increment for SQL Server IDENTITY.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="increment"> The incremental value that is added to the identity value of the previous row that was loaded. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use HasIdentityColumnIncrement")]
        public static IConventionPropertyBuilder ForSqlServerHasIdentityIncrement(
            [NotNull] this IConventionPropertyBuilder propertyBuilder, int? increment, bool fromDataAnnotation = false)
            => propertyBuilder.HasIdentityColumnIncrement(increment, fromDataAnnotation);

        /// <summary>
        ///     Configures the value generation strategy for the key property, when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="valueGenerationStrategy"> The value generation strategy. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use HasValueGenerationStrategy")]
        public static IConventionPropertyBuilder ForSqlServerHasValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            SqlServerValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
            => propertyBuilder.HasValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
    }
}
