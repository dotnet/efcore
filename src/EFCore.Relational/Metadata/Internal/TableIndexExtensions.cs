// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public static class TableIndexExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString(
            [NotNull] this ITableIndex index,
            MetadataDebugStringOptions options,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);
            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("Index: ");
            }

            builder
                .Append(index.Name)
                .Append(" ")
                .Append(Column.Format(index.Columns));

            if (index.IsUnique)
            {
                builder
                    .Append(" Unique");
            }

            if (!string.IsNullOrWhiteSpace(index.Filter))
            {
                builder
                    .Append(" Filtered");
            }

            if (!singleLine &&
                (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(index.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }
    }
}
