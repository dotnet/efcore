// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table index.
    /// </summary>
    public interface ITableIndex : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the index.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the mapped indexes.
        /// </summary>
        IEnumerable<IIndex> MappedIndexes { get; }

        /// <summary>
        ///     Gets the table on with the index is declared.
        /// </summary>
        ITable Table { get; }

        /// <summary>
        ///     Gets the columns that are participating in the index.
        /// </summary>
        IReadOnlyList<IColumn> Columns { get; }

        /// <summary>
        ///     Gets a value indicating whether the index enforces uniqueness.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        ///     Gets the expression used as the index filter.
        /// </summary>
        string? Filter { get; }

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

            builder.Append(indentString);
            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("Index: ");
            }

            builder
                .Append(Name)
                .Append(' ')
                .Append(ColumnBase.Format(Columns));

            if (IsUnique)
            {
                builder
                    .Append(" Unique");
            }

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                builder
                    .Append(" Filtered");
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
