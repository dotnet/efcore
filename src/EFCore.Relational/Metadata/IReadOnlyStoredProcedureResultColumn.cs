// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure result column.
/// </summary>
public interface IReadOnlyStoredProcedureResultColumn : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the stored procedure to which this result column belongs.
    /// </summary>
    IReadOnlyStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets the result column name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the name of property mapped to this result column.
    /// </summary>
    string? PropertyName { get; }

    /// <summary>
    ///     Gets a value indicating whether the result column will hold the rows affected by the stored procedure.
    /// </summary>
    bool ForRowsAffected { get; }

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

        builder
            .Append(indentString)
            .Append("StoredProcedureResultColumn: ");

        builder.Append(Name);

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
