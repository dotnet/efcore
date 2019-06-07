// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class AnnotatableExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string AnnotationsToDebugString([NotNull] this IAnnotatable annotatable, [NotNull] string indent = "")
        {
            var annotations = annotatable.GetAnnotations().ToList();
            if (annotations.Count == 0)
            {
                return "";
            }

            var builder = new StringBuilder();

            builder.AppendLine().Append(indent).Append("Annotations: ");
            foreach (var annotation in annotations)
            {
                builder
                    .AppendLine()
                    .Append(indent)
                    .Append("  ")
                    .Append(annotation.Name)
                    .Append(": ")
                    .Append(annotation.Value);
            }

            return builder.ToString();
        }
    }
}
