// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class ColumnMappingExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString(
            [NotNull] this IColumnMapping columnMapping,
            MetadataDebugStringOptions options,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

#pragma warning disable EF1001 // Internal EF Core API usage.
            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
#pragma warning restore EF1001 // Internal EF Core API usage.
            if (singleLine)
            {
                builder.Append($"ColumnMapping: ");
            }

            builder.Append(columnMapping.Property.Name).Append(" - ");

            builder.Append(columnMapping.Column.Name);

            if (!singleLine &&
#pragma warning disable EF1001 // Internal EF Core API usage.
                (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
#pragma warning restore EF1001 // Internal EF Core API usage.
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                builder.Append(columnMapping.AnnotationsToDebugString(indent + "  "));
#pragma warning restore EF1001 // Internal EF Core API usage.
            }

            return builder.ToString();
        }
    }
}
