// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="IForeignKeyConstraint" />.
    /// </summary>
    public static class ForeignKeyConstraintExtensions
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
        /// <param name="foreignKey"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IForeignKeyConstraint foreignKey,
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
                .Append(foreignKey.Name)
                .Append(" ")
                .Append(foreignKey.Table.Name)
                .Append(" ")
                .Append(ColumnBase.Format(foreignKey.Columns))
                .Append(" -> ")
                .Append(foreignKey.PrincipalTable.Name)
                .Append(" ")
                .Append(ColumnBase.Format(foreignKey.PrincipalColumns));

            if (foreignKey.OnDeleteAction != ReferentialAction.NoAction)
            {
                builder
                    .Append(" ")
                    .Append(foreignKey.OnDeleteAction);
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(foreignKey.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
