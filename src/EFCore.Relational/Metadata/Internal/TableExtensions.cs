// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class TableExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString(
            [NotNull] this ITable table,
            MetadataDebugStringOptions options,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder
                .Append(indent)
                .Append("Table: ");

            if (table.Schema != null)
            {
                builder
                    .Append(table.Schema)
                    .Append(".");
            }

            builder.Append(table.Name);

            if (!table.IsMigratable)
            {
                builder.Append(" NonMigratable");
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var mappings = table.EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + "    "));
                    }
                }

                var columns = table.Columns.ToList();
                if (columns.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Columns: ");
                    foreach (var column in columns)
                    {
                        builder.AppendLine().Append(column.ToDebugString(options, indent + "    "));
                    }
                }

                var checkConstraints = table.CheckConstraints.ToList();
                if (checkConstraints.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Check constraints: ");
                    foreach (var checkConstraint in checkConstraints)
                    {
                        builder.AppendLine().Append(checkConstraint.ToDebugString(options, indent + "    "));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(table.AnnotationsToDebugString(indent + "  "));
                }
            }

            return builder.ToString();
        }
    }
}
