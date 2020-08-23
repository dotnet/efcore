// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="IUniqueConstraint" />.
    /// </summary>
    public static class UniqueConstraintExtensions
    {
        /// <summary>
        ///     Gets a value indicating whether this constraint is the primary key.
        /// </summary>
        /// <param name="uniqueConstraint"> The metadata item. </param>
        /// <returns> <see langword="true" /> if the constraint is the primary key </returns>
        public static bool GetIsPrimaryKey([NotNull] this IUniqueConstraint uniqueConstraint)
            => uniqueConstraint.Table.PrimaryKey == uniqueConstraint;

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="uniqueConstraint"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IUniqueConstraint uniqueConstraint,
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

            builder
                .Append(uniqueConstraint.Name)
                .Append(" ")
                .Append(ColumnBase.Format(uniqueConstraint.Columns));

            if (uniqueConstraint.GetIsPrimaryKey())
            {
                builder.Append(" PrimaryKey");
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(uniqueConstraint.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
