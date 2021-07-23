// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a SQL query string.
    /// </summary>
    public interface ISqlQuery : ITableBase
    {
        /// <summary>
        ///     Gets the entity type mappings.
        /// </summary>
        new IEnumerable<ISqlQueryMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     Gets the columns defined for this query.
        /// </summary>
        new IEnumerable<ISqlQueryColumn> Columns { get; }

        /// <summary>
        ///     Gets the column with the given name. Returns <see langword="null" /> if no column with the given name is defined.
        /// </summary>
        new ISqlQueryColumn? FindColumn(string name);

        /// <summary>
        ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
        /// </summary>
        new ISqlQueryColumn? FindColumn(IProperty property);

        /// <summary>
        ///     Gets the SQL query string.
        /// </summary>
        public string Sql { get; }

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
                .Append("SqlQuery: ");

            if (Schema != null)
            {
                builder
                    .Append(Schema)
                    .Append('.');
            }

            builder.Append(Name);

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                if (Sql != null)
                {
                    builder.AppendLine().Append(indentString).Append("  Sql: ");
                    builder.AppendLine().Append(indentString).Append(new string(' ', 4)).Append(Sql);
                }

                var mappings = EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + 4));
                    }
                }

                var columns = Columns.ToList();
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
                    builder.Append(AnnotationsToDebugString(indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
