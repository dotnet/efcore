// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class DatabaseModelExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString(
            [NotNull] this IRelationalModel model,
            MetadataDebugStringOptions options,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent).Append("DatabaseModel: ");

            foreach (var table in model.Tables)
            {
                builder.AppendLine().Append(table.ToDebugString(options, indent + "  "));
            }

            foreach (var view in model.Views)
            {
                builder.AppendLine().Append(view.ToDebugString(options, indent + "  "));
            }

            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(model.AnnotationsToDebugString(indent));
            }

            return builder.ToString();
        }
    }
}
