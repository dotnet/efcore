// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IIndex" />.
    /// </summary>
    public static class IndexExtensions
    {
        /// <summary>
        ///     <para>
        ///         Gets a factory for key values based on the index key values taken from various forms of entity data.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="index"> The index metadata. </param>
        /// <typeparam name="TKey"> The type of the index instance. </typeparam>
        /// <returns> The factory. </returns>
        public static IDependentKeyValueFactory<TKey> GetNullableValueFactory<TKey>([NotNull] this IIndex index)
            => index.AsIndex().GetNullableValueFactory<TKey>();

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="index"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IIndex index,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("Index: ");
            }

            builder
                .AppendJoin(
                    ", ",
                    index.Properties.Select(
                        p => singleLine
                            ? p.DeclaringEntityType.DisplayName() + "." + p.Name
                            : p.Name));

            builder.Append(" " + index.Name ?? "<unnamed>");

            if (index.IsUnique)
            {
                builder.Append(" Unique");
            }

            if (!singleLine
                && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(index.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
