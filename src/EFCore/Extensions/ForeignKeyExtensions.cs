// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IForeignKey" />.
    /// </summary>
    public static class ForeignKeyExtensions
    {
        /// <summary>
        ///     <para>
        ///         Creates a factory for key values based on the foreign key values taken
        ///         from various forms of entity data.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The <see cref="IForeignKey" /> for which a factory is needed. </param>
        /// <typeparam name="TKey"> The type of key instanceas. </typeparam>
        /// <returns> A new factory. </returns>
        public static IDependentKeyValueFactory<TKey> GetDependentKeyValueFactory<TKey>(
            [NotNull] this IForeignKey foreignKey)
            => (IDependentKeyValueFactory<TKey>)foreignKey.AsForeignKey().DependentKeyValueFactory;

        /// <summary>
        ///     Gets the entity type related to the given one.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="entityType"> One of the entity types related by the foreign key. </param>
        /// <returns> The entity type related to the given one. </returns>
        public static IEntityType GetRelatedEntityType([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationshipStrict(
                        entityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        /// <summary>
        ///     Returns a navigation associated with this foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <returns>
        ///     A navigation associated with this foreign key or null.
        /// </returns>
        public static INavigation GetNavigation([NotNull] this IForeignKey foreignKey, bool pointsToPrincipal)
            => pointsToPrincipal ? foreignKey.DependentToPrincipal : foreignKey.PrincipalToDependent;

        /// <summary>
        ///     Returns a value indicating whether the foreign key is defined on the primary key and pointing to the same primary key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> A value indicating whether the foreign key is defined on the primary key and pointing to the same primary key. </returns>
        public static bool IsBaseLinking([NotNull] this IForeignKey foreignKey)
        {
            var primaryKey = foreignKey.DeclaringEntityType.FindPrimaryKey();
            return primaryKey == foreignKey.PrincipalKey
                && foreignKey.Properties.SequenceEqual(primaryKey.Properties);
        }

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IForeignKey foreignKey,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("ForeignKey: ");
            }

            builder
                .Append(foreignKey.DeclaringEntityType.DisplayName())
                .Append(" ")
                .Append(foreignKey.Properties.Format())
                .Append(" -> ")
                .Append(foreignKey.PrincipalEntityType.DisplayName())
                .Append(" ")
                .Append(foreignKey.PrincipalKey.Properties.Format());

            if (foreignKey.IsUnique)
            {
                builder.Append(" Unique");
            }

            if (foreignKey.IsOwnership)
            {
                builder.Append(" Ownership");
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                builder.Append(" ToDependent: ").Append(foreignKey.PrincipalToDependent.Name);
            }

            if (foreignKey.DependentToPrincipal != null)
            {
                builder.Append(" ToPrincipal: ").Append(foreignKey.DependentToPrincipal.Name);
            }

            if (foreignKey.DeleteBehavior != DeleteBehavior.NoAction)
            {
                builder
                    .Append(" ")
                    .Append(foreignKey.DeleteBehavior);
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(foreignKey.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
