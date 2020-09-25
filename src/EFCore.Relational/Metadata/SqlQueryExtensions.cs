// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="ISqlQuery" />.
    /// </summary>
    public static class SqlQueryExtensions
    {
        /// <summary>
        ///     Gets the name used for the <see cref="ISqlQuery" /> mapped using
        ///     <see cref="M:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
        /// </summary>
        public static readonly string DefaultQueryNameBase = "MappedSqlQuery";

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="sqlQuery"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this ISqlQuery sqlQuery,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder
                .Append(indentString)
                .Append("SqlQuery: ");

            if (sqlQuery.Schema != null)
            {
                builder
                    .Append(sqlQuery.Schema)
                    .Append(".");
            }

            builder.Append(sqlQuery.Name);

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                if (sqlQuery.Sql != null)
                {
                    builder.AppendLine().Append(indentString).Append("  Sql: ");
                    builder.AppendLine().Append(indentString).Append(new string(' ', 4)).Append(sqlQuery.Sql);
                }

                var mappings = sqlQuery.EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + 4));
                    }
                }

                var columns = sqlQuery.Columns.ToList();
                if (columns.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Columns: ");
                    foreach (var column in columns)
                    {
                        builder.AppendLine().Append(column.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(sqlQuery.AnnotationsToDebugString(indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
