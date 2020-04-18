// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="ISequence" />.
    /// </summary>
    public static class SequenceExtensions
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
        /// <param name="sequence"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this ISequence sequence,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder
                .Append(indentString)
                .Append("Sequence: ");

            if (sequence.Schema != null)
            {
                builder
                    .Append(sequence.Schema)
                    .Append(".");
            }

            builder.Append(sequence.Name);

            if (!sequence.IsCyclic)
            {
                builder.Append(" Cyclic");
            }

            if (sequence.StartValue != 1)
            {
                builder.Append(" Start: ")
                    .Append(sequence.StartValue);
            }

            if (sequence.IncrementBy != 1)
            {
                builder.Append(" IncrementBy: ")
                    .Append(sequence.IncrementBy);
            }

            if (sequence.MinValue != null)
            {
                builder.Append(" Min: ")
                    .Append(sequence.MinValue);
            }

            if (sequence.MaxValue != null)
            {
                builder.Append(" Max: ")
                    .Append(sequence.MaxValue);
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(sequence.AnnotationsToDebugString(indent: indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
