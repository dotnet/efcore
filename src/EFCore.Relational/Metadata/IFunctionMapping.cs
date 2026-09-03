// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents entity type mapping to a function.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IFunctionMapping : ITableMappingBase
{
    /// <summary>
    ///     Gets the value indicating whether this is the function mapping
    ///     that should be used when the entity type is queried.
    /// </summary>
    bool IsDefaultFunctionMapping { get; }

    /// <summary>
    ///     Gets the target function.
    /// </summary>
    IStoreFunction StoreFunction { get; }

    /// <summary>
    ///     Gets the target function.
    /// </summary>
    IDbFunction DbFunction { get; }

    /// <summary>
    ///     Gets the properties mapped to columns on the target function.
    /// </summary>
    new IEnumerable<IFunctionColumnMapping> ColumnMappings { get; }

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ITableMappingBase.ToDebugString(MetadataDebugStringOptions options, int indent)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder.Append(indentString);

        var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
        if (singleLine)
        {
            builder.Append("FunctionMapping: ");
        }

        builder.Append(TypeBase.DisplayName()).Append(" - ");

        builder.Append(StoreFunction.Name);

        if (IsDefaultFunctionMapping)
        {
            builder.Append(" DefaultMapping");
        }

        if (IncludesDerivedTypes != null)
        {
            builder.Append(' ');
            if (!IncludesDerivedTypes.Value)
            {
                builder.Append('!');
            }

            builder.Append("IncludesDerivedTypes");
        }

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
