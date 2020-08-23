// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="IFunctionMapping" />.
    /// </summary>
    public static class FunctionMappingExtensions
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
        /// <param name="functionMapping"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IFunctionMapping functionMapping,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("FunctionMapping: ");
            }

            builder.Append(functionMapping.EntityType.Name).Append(" - ");

            builder.Append(functionMapping.Table.Name);

            if (functionMapping.IsDefaultFunctionMapping)
            {
                builder.Append(" DefaultMapping");
            }

            if (functionMapping.IncludesDerivedTypes)
            {
                builder.Append(" IncludesDerivedTypes");
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(functionMapping.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
