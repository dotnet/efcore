// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for relationship builders.
    /// </summary>
    public static class RelationalForeignKeyBuilderExtensions
    {
        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="referenceCollectionBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceCollectionBuilder HasConstraintName(
            [NotNull] this ReferenceCollectionBuilder referenceCollectionBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceCollectionBuilder.Metadata.SetConstraintName(name);

            return referenceCollectionBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="referenceCollectionBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The principal entity type in this relationship. </typeparam>
        /// <typeparam name="TRelatedEntity"> The dependent entity type in this relationship. </typeparam>
        public static ReferenceCollectionBuilder<TEntity, TRelatedEntity> HasConstraintName<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceCollectionBuilder<TEntity, TRelatedEntity>)HasConstraintName(
                (ReferenceCollectionBuilder)referenceCollectionBuilder, name);

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="referenceReferenceBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceReferenceBuilder HasConstraintName(
            [NotNull] this ReferenceReferenceBuilder referenceReferenceBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceReferenceBuilder.Metadata.SetConstraintName(name);

            return referenceReferenceBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="referenceReferenceBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The entity type on one end of the relationship. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type on the other end of the relationship. </typeparam>
        public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasConstraintName<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)HasConstraintName(
                (ReferenceReferenceBuilder)referenceReferenceBuilder, name);

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="ownershipBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnershipBuilder HasConstraintName(
            [NotNull] this OwnershipBuilder ownershipBuilder,
            [CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            ownershipBuilder.Metadata.SetConstraintName(name);

            return ownershipBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="ownershipBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The entity type on one end of the relationship. </typeparam>
        /// <typeparam name="TDependentEntity"> The entity type on the other end of the relationship. </typeparam>
        public static OwnershipBuilder<TEntity, TDependentEntity> HasConstraintName<TEntity, TDependentEntity>(
            [NotNull] this OwnershipBuilder<TEntity, TDependentEntity> ownershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TDependentEntity : class
            => (OwnershipBuilder<TEntity, TDependentEntity>)HasConstraintName(
                (OwnershipBuilder)ownershipBuilder, name);

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="relationship"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionForeignKeyBuilder HasConstraintName(
            [NotNull] this IConventionForeignKeyBuilder relationship,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!relationship.CanSetConstraintName(name, fromDataAnnotation))
            {
                return null;
            }

            relationship.Metadata.SetConstraintName(name, fromDataAnnotation);
            return relationship;
        }

        /// <summary>
        ///     Returns a value indicating whether the foreign key constraint name can be set for this relationship
        ///     from the current configuration source
        /// </summary>
        /// <param name="relationship"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetConstraintName(
            [NotNull] this IConventionForeignKeyBuilder relationship,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
            => Check.NotNull(relationship, nameof(relationship))
                .CanSetAnnotation(RelationalAnnotationNames.Name, name, fromDataAnnotation);
    }
}
