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
    public static class DbFunctionExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString(
            [NotNull] this IDbFunction function,
            MetadataDebugStringOptions options,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder
                .Append(indent)
                .Append("DbFunction: ");

            builder.Append(function.ReturnType.ShortDisplayName())
                .Append(" ");

            if (function.Schema != null)
            {
                builder
                    .Append(function.Schema)
                    .Append(".");
            }

            builder.Append(function.Name);

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var parameters = function.Parameters.ToList();
                if (parameters.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Parameters: ");
                    foreach (var parameter in parameters)
                    {
                        builder.AppendLine().Append(parameter.ToDebugString(options, indent + "    "));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(function.AnnotationsToDebugString(indent: indent + "  "));
                }
            }

            return builder.ToString();
        }
    }
}
