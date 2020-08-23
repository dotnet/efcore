// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IKey" />.
    /// </summary>
    public static class KeyExtensions
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
        /// <param name="key"> The key metadata. </param>
        /// <typeparam name="TKey"> The type of the key instance. </typeparam>
        /// <returns> The factory. </returns>
        public static IPrincipalKeyValueFactory<TKey> GetPrincipalKeyValueFactory<TKey>([NotNull] this IKey key)
            => key.AsKey().GetPrincipalKeyValueFactory<TKey>();

        /// <summary>
        ///     Returns the type of the key property for simple keys, or an object array for composite keys.
        /// </summary>
        /// <param name="key"> Key metadata. </param>
        /// <returns> The key type. </returns>
        public static Type GetKeyType([NotNull] this IKey key)
            => key.Properties.Count > 1 ? typeof(object[]) : key.Properties.First().ClrType;

        /// <summary>
        ///     Gets all foreign keys that target a given primary or alternate key.
        /// </summary>
        /// <param name="key"> The key to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given key. </returns>
        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IKey key)
            => Check.NotNull(key, nameof(key)).AsKey().ReferencingForeignKeys ?? Enumerable.Empty<IForeignKey>();

        /// <summary>
        ///     Returns a value indicating whether the key is the primary key.
        /// </summary>
        /// <param name="key"> The key to find whether it is primary. </param>
        /// <returns> <see langword="true" /> if the key is the primary key. </returns>
        public static bool IsPrimaryKey([NotNull] this IKey key)
            => key == key.DeclaringEntityType.FindPrimaryKey();

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="key"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IKey key,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("Key: ");
            }

            builder.AppendJoin(
                ", ", key.Properties.Select(
                    p => singleLine
                        ? p.DeclaringEntityType.DisplayName() + "." + p.Name
                        : p.Name));

            if (key.IsPrimaryKey())
            {
                builder.Append(" PK");
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(key.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
