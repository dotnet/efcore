// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a parameter in a stored procedure.
/// </summary>
public interface IStoreStoredProcedureParameter : IColumnBase
{
    /// <summary>
    ///     Gets the containing stored procedure.
    /// </summary>
    IStoreStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets the property mappings.
    /// </summary>
    new IReadOnlyList<IStoredProcedureParameterMapping> PropertyMappings { get; }
    
    /// <summary>
    ///     Gets the direction of the parameter.
    /// </summary>
    ParameterDirection Direction { get; }

    /// <summary>
    ///     Gets the 0-based position of the parameter in the declaring stored procedure.
    /// </summary>
    int Position { get; }

    /// <summary>
    ///     Returns the property mapping for the given entity type.
    /// </summary>
    /// <param name="entityType">An entity type.</param>
    /// <returns>The property mapping or <see langword="null" /> if not found.</returns>
    IStoredProcedureParameterMapping? FindParameterMapping(IReadOnlyEntityType entityType)
        => (IStoredProcedureParameterMapping?)FindColumnMapping(entityType);

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
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder.Append(indentString);

        var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
        if (singleLine)
        {
            builder.Append($"StoredProcedureParameter: {Table.Name}.");
        }

        builder.Append(Name).Append(" (");
        builder.Append(StoreType).Append(')');
        builder.Append(IsNullable ? " Nullable" : " NonNullable");
        builder.Append(')');

        if (Direction != ParameterDirection.Input)
        {
            builder.Append(" ").Append(Direction);
        }

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
