// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="ITable" />.
    /// </summary>
    public static class TableExtensions
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
        /// <param name="table"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this ITable table,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder
                .Append(indentString)
                .Append("Table: ");

            if (table.Schema != null)
            {
                builder
                    .Append(table.Schema)
                    .Append(".");
            }

            builder.Append(table.Name);

            if (table.IsExcludedFromMigrations)
            {
                builder.Append(" ExcludedFromMigrations");
            }

            if (table.PrimaryKey == null)
            {
                builder.Append(" Keyless");
            }
            else
            {
                if ((options & MetadataDebugStringOptions.SingleLine) == 0)
                {
                    builder.AppendLine();
                }

                builder.Append(table.PrimaryKey.ToDebugString(options, indent + 2));
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0 && table.Comment != null)
            {
                builder
                    .AppendLine()
                    .Append(indentString)
                    .AppendLine(" Comment:")
                    .Append(indentString)
                    .Append(table.Comment);
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var mappings = table.EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + 4));
                    }
                }

                var columns = table.Columns.ToList();
                if (columns.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Columns: ");
                    foreach (var column in columns)
                    {
                        builder.AppendLine().Append(column.ToDebugString(options, indent + 4));
                    }
                }

                var foreignKeyConstraints = table.ForeignKeyConstraints.ToList();
                if (foreignKeyConstraints.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  ForeignKeyConstraints: ");
                    foreach (var foreignKeyConstraint in foreignKeyConstraints)
                    {
                        builder.AppendLine().Append(foreignKeyConstraint.ToDebugString(options, indent + 4));
                    }
                }

                var indexes = table.Indexes.ToList();
                if (indexes.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Indexes: ");
                    foreach (var index in indexes)
                    {
                        builder.AppendLine().Append(index.ToDebugString(options, indent + 4));
                    }
                }

                var uniqueConstraints = table.UniqueConstraints.Where(uc => !uc.GetIsPrimaryKey()).ToList();
                if (uniqueConstraints.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  UniqueConstraints: ");
                    foreach (var uniqueConstraint in uniqueConstraints)
                    {
                        builder.AppendLine().Append(uniqueConstraint.ToDebugString(options, indent + 4));
                    }
                }

                var checkConstraints = table.CheckConstraints.ToList();
                if (checkConstraints.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Check constraints: ");
                    foreach (var checkConstraint in checkConstraints)
                    {
                        builder.AppendLine().Append(checkConstraint.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(table.AnnotationsToDebugString(indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
