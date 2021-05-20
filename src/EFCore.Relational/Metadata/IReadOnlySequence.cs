// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the model.
    /// </summary>
    public interface IReadOnlySequence : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the name of the sequence in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the database schema that contains the sequence.
        /// </summary>
        string? Schema { get; }

        /// <summary>
        ///     Gets the model in which this sequence is defined.
        /// </summary>
        IReadOnlyModel Model { get; }

        /// <summary>
        ///     Gets the value at which the sequence will start.
        /// </summary>
        long StartValue { get; }

        /// <summary>
        ///     Gets the amount incremented to obtain each new value in the sequence.
        /// </summary>
        int IncrementBy { get; }

        /// <summary>
        ///     Gets the minimum value supported by the sequence, or <see langword="null" /> if none has been set.
        /// </summary>
        long? MinValue { get; }

        /// <summary>
        ///     Gets the maximum value supported by the sequence, or <see langword="null" /> if none has been set.
        /// </summary>
        long? MaxValue { get; }

        /// <summary>
        ///     Gets the type of values returned by the sequence.
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     Gets the <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        [Obsolete("Use Type")]
        Type ClrType { get; }

        /// <summary>
        ///     Gets the value indicating whether the sequence will start again from the beginning when the max value
        ///     is reached.
        /// </summary>
        bool IsCyclic { get; }

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder
                .Append(indentString)
                .Append("Sequence: ");

            if (Schema != null)
            {
                builder
                    .Append(Schema)
                    .Append('.');
            }

            builder.Append(Name);

            if (!IsCyclic)
            {
                builder.Append(" Cyclic");
            }

            if (StartValue != 1)
            {
                builder.Append(" Start: ")
                    .Append(StartValue);
            }

            if (IncrementBy != 1)
            {
                builder.Append(" IncrementBy: ")
                    .Append(IncrementBy);
            }

            if (MinValue != null)
            {
                builder.Append(" Min: ")
                    .Append(MinValue);
            }

            if (MaxValue != null)
            {
                builder.Append(" Max: ")
                    .Append(MaxValue);
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent: indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
