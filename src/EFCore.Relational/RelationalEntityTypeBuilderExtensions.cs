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
    ///     Relational database specific extension methods for <see cref="EntityTypeBuilder" />.
    /// </summary>
    public static class RelationalEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name);

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            entityTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name, schema);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name, schema);

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder HasDiscriminator([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit).HasDiscriminator();
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <param name="discriminatorType"> The type of values stored in the discriminator column. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder HasDiscriminator(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [NotNull] Type discriminatorType)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(discriminatorType, nameof(discriminatorType));

            return entityTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit).HasDiscriminator(name, discriminatorType);
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <typeparam name="TDiscriminator"> The type of values stored in the discriminator column. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));

            return new DiscriminatorBuilder<TDiscriminator>(
                entityTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                    .Relational(ConfigurationSource.Explicit).HasDiscriminator(name, typeof(TDiscriminator)));
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TDiscriminator"> The type of values stored in the discriminator column. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be used as the discriminator (
        ///     <c>blog => blog.Discriminator</c>).
        /// </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TEntity, TDiscriminator>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Expression<Func<TEntity, TDiscriminator>> propertyExpression)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            return new DiscriminatorBuilder<TDiscriminator>(
                entityTypeBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                    .Relational(ConfigurationSource.Explicit).HasDiscriminator(propertyExpression.GetPropertyAccess()));
        }
    }
}
