// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a column in a table-valued function.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IFunctionColumn : IColumnBase
{
    /// <summary>
    ///     Gets the containing function.
    /// </summary>
    IStoreFunction Function { get; }

    /// <summary>
    ///     Gets the property mappings.
    /// </summary>
    new IReadOnlyList<IFunctionColumnMapping> PropertyMappings { get; }

    /// <summary>
    ///     Returns the property mapping for the given entity type.
    /// </summary>
    /// <param name="entityType">An entity type.</param>
    /// <returns>The property mapping or <see langword="null" /> if not found.</returns>
    new IFunctionColumnMapping? FindColumnMapping(IReadOnlyEntityType entityType)
        => (IFunctionColumnMapping?)((IColumnBase)this).FindColumnMapping(entityType);

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
    string IColumnBase.ToDebugString(MetadataDebugStringOptions options, int indent)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder.Append(indentString);

        var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
        if (singleLine)
        {
            builder.Append($"FunctionColumn: {Table.Name}.");
        }

        builder.Append(Name).Append(" (");
        builder.Append(StoreType).Append(')');
        builder.Append(IsNullable ? " Nullable" : " NonNullable");
        builder.Append(')');

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
