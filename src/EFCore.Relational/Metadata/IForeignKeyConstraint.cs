// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a foreign key constraint.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IForeignKeyConstraint : IAnnotatable
{
    /// <summary>
    ///     Gets the name of the foreign key constraint.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the mapped foreign keys.
    /// </summary>
    IEnumerable<IForeignKey> MappedForeignKeys { get; }

    /// <summary>
    ///     Gets the table on with the foreign key constraint is declared.
    /// </summary>
    ITable Table { get; }

    /// <summary>
    ///     Gets the table that is referenced by the foreign key constraint.
    /// </summary>
    ITable PrincipalTable { get; }

    /// <summary>
    ///     Gets the columns that are participating in the foreign key constraint.
    /// </summary>
    IReadOnlyList<IColumn> Columns { get; }

    /// <summary>
    ///     Gets the columns that are referenced by the foreign key constraint.
    /// </summary>
    IReadOnlyList<IColumn> PrincipalColumns
        => PrincipalUniqueConstraint.Columns;

    /// <summary>
    ///     Gets the unique constraint on the columns referenced by the foreign key constraint.
    /// </summary>
    IUniqueConstraint PrincipalUniqueConstraint { get; }

    /// <summary>
    ///     Gets the action to be performed when the referenced row is deleted.
    /// </summary>
    ReferentialAction OnDeleteAction { get; }

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
            builder.Append("ForeignKey: ");
        }

        builder
            .Append(Name)
            .Append(' ')
            .Append(Table.Name)
            .Append(' ')
            .Append(ColumnBase<IColumnMappingBase>.Format(Columns))
            .Append(" -> ")
            .Append(PrincipalTable.Name)
            .Append(' ')
            .Append(ColumnBase<IColumnMappingBase>.Format(PrincipalColumns));

        if (OnDeleteAction != ReferentialAction.NoAction)
        {
            builder
                .Append(' ')
                .Append(OnDeleteAction);
        }

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
