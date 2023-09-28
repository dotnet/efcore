// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a property on an entity type that represents an
///     injected service from the <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyServiceProperty : IReadOnlyPropertyBase
{
    /// <summary>
    ///     Gets the entity type that this property belongs to.
    /// </summary>
    IReadOnlyEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     The <see cref="ServiceParameterBinding" /> for this property.
    /// </summary>
    ServiceParameterBinding? ParameterBinding { get; }

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
            builder.Append("Service property: ").Append(DeclaringType.DisplayName()).Append('.');
        }

        builder.Append(Name);

        if (GetFieldName() == null)
        {
            builder.Append(" (no field, ");
        }
        else
        {
            builder.Append(" (").Append(GetFieldName()).Append(", ");
        }

        builder.Append(ClrType.ShortDisplayName()).Append(')');

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
