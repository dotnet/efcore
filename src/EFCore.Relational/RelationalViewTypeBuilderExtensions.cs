// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="QueryTypeBuilder" />.
    /// </summary>
    public static class RelationalQueryTypeBuilderExtensions
    {
        /// <summary>
        ///     Configures the view or table that the view maps to when targeting a relational database.
        /// </summary>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static QueryTypeBuilder ToView(
            [NotNull] this QueryTypeBuilder queryTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(queryTypeBuilder, nameof(queryTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            queryTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name);

            return queryTypeBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the view maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TQuery"> The query type being configured. </typeparam>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static QueryTypeBuilder<TQuery> ToView<TQuery>(
            [NotNull] this QueryTypeBuilder<TQuery> queryTypeBuilder,
            [CanBeNull] string name)
            where TQuery : class
            => (QueryTypeBuilder<TQuery>)ToView((QueryTypeBuilder)queryTypeBuilder, name);

        /// <summary>
        ///     Configures the view or table that the view maps to when targeting a relational database.
        /// </summary>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static QueryTypeBuilder ToView(
            [NotNull] this QueryTypeBuilder queryTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(queryTypeBuilder, nameof(queryTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            queryTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name, schema);

            return queryTypeBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the view maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TQuery"> The query type being configured. </typeparam>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static QueryTypeBuilder<TQuery> ToView<TQuery>(
            [NotNull] this QueryTypeBuilder<TQuery> queryTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TQuery : class
            => (QueryTypeBuilder<TQuery>)ToView((QueryTypeBuilder)queryTypeBuilder, name, schema);

        /// <summary>
        ///     Configures the discriminator column used to identify which query type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder HasDiscriminator([NotNull] this QueryTypeBuilder queryTypeBuilder)
        {
            Check.NotNull(queryTypeBuilder, nameof(queryTypeBuilder));

            return queryTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit).HasDiscriminator();
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which query type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <param name="discriminatorType"> The type of values stored in the discriminator column. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder HasDiscriminator(
            [NotNull] this QueryTypeBuilder queryTypeBuilder,
            [NotNull] string name,
            [NotNull] Type discriminatorType)
        {
            Check.NotNull(queryTypeBuilder, nameof(queryTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(discriminatorType, nameof(discriminatorType));

            return queryTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit).HasDiscriminator(name, discriminatorType);
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which query type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <typeparam name="TDiscriminator"> The type of values stored in the discriminator column. </typeparam>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(
            [NotNull] this QueryTypeBuilder queryTypeBuilder,
            [NotNull] string name)
        {
            Check.NotNull(queryTypeBuilder, nameof(queryTypeBuilder));
            Check.NotEmpty(name, nameof(name));

            return new DiscriminatorBuilder<TDiscriminator>(
                queryTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                    .Relational(ConfigurationSource.Explicit).HasDiscriminator(name, typeof(TDiscriminator)));
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which query type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <typeparam name="TQuery"> The query type being configured. </typeparam>
        /// <typeparam name="TDiscriminator"> The type of values stored in the discriminator column. </typeparam>
        /// <param name="queryTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be used as the discriminator (
        ///     <c>blog => blog.Discriminator</c>).
        /// </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TQuery, TDiscriminator>(
            [NotNull] this QueryTypeBuilder<TQuery> queryTypeBuilder,
            [NotNull] Expression<Func<TQuery, TDiscriminator>> propertyExpression)
            where TQuery : class
        {
            Check.NotNull(queryTypeBuilder, nameof(queryTypeBuilder));
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            return new DiscriminatorBuilder<TDiscriminator>(
                queryTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                    .Relational(ConfigurationSource.Explicit).HasDiscriminator(propertyExpression.GetPropertyAccess()));
        }
    }
}
