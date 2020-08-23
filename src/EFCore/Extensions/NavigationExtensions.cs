// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="INavigation" />.
    /// </summary>
    public static class NavigationExtensions
    {
        /// <summary>
        ///     Gets a value indicating whether the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity.
        /// </summary>
        /// <param name="navigation"> The navigation property to check. </param>
        /// <returns>
        ///     <see langword="true" /> if the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity, otherwise <see langword="false" />.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use INavigation.IsOnDependent")]
        public static bool IsDependentToPrincipal([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).IsOnDependent;

        /// <summary>
        ///     Gets a value indicating whether the given navigation property is a collection property.
        /// </summary>
        /// <param name="navigation"> The navigation property to check. </param>
        /// <returns>
        ///     <see langword="true" /> if this is a collection property, false if it is a reference property.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use INavigation.IsCollection")]
        public static bool IsCollection([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).IsCollection;

        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns null if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation"> The navigation property to find the inverse of. </param>
        /// <returns>
        ///     The inverse navigation, or null if none is defined.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use INavigation.Inverse")]
        public static INavigation FindInverse([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).Inverse;

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation"> The navigation property to find the target entity type of. </param>
        /// <returns> The target entity type. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use INavigation.TargetEntityType")]
        public static IEntityType GetTargetType([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).TargetEntityType;

        /// <summary>
        ///     Gets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        /// <param name="navigation"> The navigation property to find whether it should be eager loaded. </param>
        /// <returns> A value indicating whether this navigation should be eager loaded by default. </returns>
        [Obsolete("Use INavigation.IsEagerLoaded")]
        public static bool IsEagerLoaded([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).IsEagerLoaded;

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="navigation"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this INavigation navigation,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append($"Navigation: {navigation.DeclaringEntityType.DisplayName()}.");
            }

            builder.Append(navigation.Name);

            var field = navigation.GetFieldName();
            if (field == null)
            {
                builder.Append(" (no field, ");
            }
            else if (!field.EndsWith(">k__BackingField", StringComparison.Ordinal))
            {
                builder.Append($" ({field}, ");
            }
            else
            {
                builder.Append(" (");
            }

            builder.Append(navigation.ClrType?.ShortDisplayName()).Append(")");

            if (navigation.IsCollection)
            {
                builder.Append(" Collection");
            }

            builder.Append(navigation.IsOnDependent ? " ToPrincipal " : " ToDependent ");

            builder.Append(navigation.TargetEntityType.DisplayName());

            if (navigation.Inverse != null)
            {
                builder.Append(" Inverse: ").Append(navigation.Inverse.Name);
            }

            if (navigation.GetPropertyAccessMode() != PropertyAccessMode.PreferField)
            {
                builder.Append(" PropertyAccessMode.").Append(navigation.GetPropertyAccessMode());
            }

            if ((options & MetadataDebugStringOptions.IncludePropertyIndexes) != 0)
            {
                var indexes = navigation.GetPropertyIndexes();
                builder.Append(" ").Append(indexes.Index);
                builder.Append(" ").Append(indexes.OriginalValueIndex);
                builder.Append(" ").Append(indexes.RelationshipIndex);
                builder.Append(" ").Append(indexes.ShadowIndex);
                builder.Append(" ").Append(indexes.StoreGenerationIndex);
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(navigation.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
