// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="ISkipNavigation" />.
    /// </summary>
    public static class SkipNavigationExtensions
    {
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
            [NotNull] this ISkipNavigation navigation,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append($"SkipNavigation: {navigation.DeclaringEntityType.DisplayName()}.");
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
