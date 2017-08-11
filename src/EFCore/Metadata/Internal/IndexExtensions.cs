// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class IndexExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static INullableValueFactory<TKey> GetNullableValueFactory<TKey>([NotNull] this IIndex index)
            => index.AsIndex().GetNullableValueFactory<TKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Index AsIndex([NotNull] this IIndex index, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IIndex, Index>(index, methodName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ToDebugString([NotNull] this IIndex index, bool singleLine = true, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            if (singleLine)
            {
                builder.Append("Index: ");
            }

            builder
                .Append(
                    string.Join(
                        ", ",
                        index.Properties.Select(
                            p => singleLine
                                ? p.DeclaringEntityType.DisplayName() + "." + p.Name
                                : p.Name)));

            if (index.IsUnique)
            {
                builder.Append(" Unique");
            }

            if (!singleLine)
            {
                builder.Append(index.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }
    }
}
