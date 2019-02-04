// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="OwnedNavigationBuilder" />.
    /// </summary>
    public static class RelationalOwnedNavigationBuilderExtensions
    {
        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            [NotNull] this OwnedNavigationBuilder referenceOwnershipBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceOwnershipBuilder.GetInfrastructure()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name);

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable((OwnedNavigationBuilder)referenceOwnershipBuilder, name);

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            [NotNull] this OwnedNavigationBuilder referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            referenceOwnershipBuilder.GetInfrastructure()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name, schema);

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable((OwnedNavigationBuilder)referenceOwnershipBuilder, name, schema);
    }
}
