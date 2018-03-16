// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Oracle specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class OraclePropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting Oracle. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForOracleUseSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name = name ?? OracleModelAnnotations.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            if (model.Oracle().FindSequence(name, schema) == null)
            {
                model.Oracle().GetOrAddSequence(name, schema).IncrementBy = 10;
            }

            GetOracleInternalBuilder(propertyBuilder).ValueGenerationStrategy(OracleValueGenerationStrategy.SequenceHiLo);

            property.Oracle().HiLoSequenceName = name;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting Oracle. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForOracleUseSequenceHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)ForOracleUseSequenceHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        ///     Configures the key property to use the Oracle IDENTITY feature to generate values for new entities,
        ///     when targeting Oracle. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder UseOracleIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            GetOracleInternalBuilder(propertyBuilder).ValueGenerationStrategy(OracleValueGenerationStrategy.IdentityColumn);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the key property to use the Oracle IDENTITY feature to generate values for new entities,
        ///     when targeting Oracle. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> UseOracleIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseOracleIdentityColumn((PropertyBuilder)propertyBuilder);

        private static OraclePropertyBuilderAnnotations GetOracleInternalBuilder(PropertyBuilder propertyBuilder)
            => propertyBuilder.GetInfrastructure<InternalPropertyBuilder>().Oracle(ConfigurationSource.Explicit);
    }
}
