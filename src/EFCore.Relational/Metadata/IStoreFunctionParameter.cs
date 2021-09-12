// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a <see cref="IStoreFunction" /> parameter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information.
    /// </remarks>
    public interface IStoreFunctionParameter : IAnnotatable
    {
        /// <summary>
        ///     Gets the <see cref="IStoreFunction" /> to which this parameter belongs.
        /// </summary>
        IStoreFunction Function { get; }

        /// <summary>
        ///     Gets the associated <see cref="IDbFunctionParameter" />s.
        /// </summary>
        IEnumerable<IDbFunctionParameter> DbFunctionParameters { get; }

        /// <summary>
        ///     Gets the parameter name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the store type of this parameter.
        /// </summary>
        string Type { get; }

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
                .Append("StoreFunctionParameter: ");

            builder.Append(Name)
                .Append(' ')
                .Append(Type);

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
