// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents property mapping to a stored procedure result column.
/// </summary>
public interface IStoredProcedureResultColumnMapping : IColumnMappingBase
{
    /// <summary>
    ///     Gets the target column.
    /// </summary>
    IStoreStoredProcedureResultColumn StoreResultColumn { get; }

    /// <summary>
    ///     Gets the associated stored procedure result column.
    /// </summary>
    IStoredProcedureResultColumn ResultColumn { get; }

    /// <summary>
    ///     Gets the containing stored procedure mapping.
    /// </summary>
    IStoredProcedureMapping StoredProcedureMapping { get; }

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
            builder.Append("StoredProcedureResultColumnMapping: ");
        }

        builder.Append(Property.Name).Append(" - ");

        builder.Append(Column.Name);

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
