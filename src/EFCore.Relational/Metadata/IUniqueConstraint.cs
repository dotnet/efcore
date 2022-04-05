// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a unique constraint.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IUniqueConstraint : IAnnotatable
{
    /// <summary>
    ///     Gets the name of the unique constraint.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the mapped keys.
    /// </summary>
    IEnumerable<IKey> MappedKeys { get; }

    /// <summary>
    ///     Gets the table on with the unique constraint is declared.
    /// </summary>
    ITable Table { get; }

    /// <summary>
    ///     Gets the columns that are participating in the unique constraint.
    /// </summary>
    IReadOnlyList<IColumn> Columns { get; }

    /// <summary>
    ///     Gets a value indicating whether this constraint is the primary key.
    /// </summary>
    /// <returns><see langword="true" /> if the constraint is the primary key</returns>
    bool GetIsPrimaryKey()
        => Table.PrimaryKey == this;

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
            builder.Append("Key: ");
        }

        builder
            .Append(Name)
            .Append(' ')
            .Append(ColumnBase<IColumnMappingBase>.Format(Columns));

        if (GetIsPrimaryKey())
        {
            builder.Append(" PrimaryKey");
        }

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
