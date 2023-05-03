// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a view in the database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IView : ITableBase
{
    /// <summary>
    ///     Gets the entity type mappings.
    /// </summary>
    new IEnumerable<IViewMapping> EntityTypeMappings { get; }

    /// <summary>
    ///     Gets the columns defined for this view.
    /// </summary>
    new IEnumerable<IViewColumn> Columns { get; }

    /// <summary>
    ///     Gets the column with the given name. Returns <see langword="null" /> if no column with the given name is defined.
    /// </summary>
    new IViewColumn? FindColumn(string name);

    /// <summary>
    ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
    /// </summary>
    new IViewColumn? FindColumn(IProperty property);

    /// <summary>
    ///     Gets the view definition or <see langword="null" /> if this view is not managed by migrations.
    /// </summary>
    public string? ViewDefinitionSql { get; }

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
    string ITableBase.ToDebugString(MetadataDebugStringOptions options, int indent)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            builder
                .Append(indentString)
                .Append("View: ");

            if (Schema != null)
            {
                builder
                    .Append(Schema)
                    .Append('.');
            }

            builder.Append(Name);

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                if (ViewDefinitionSql != null)
                {
                    builder.AppendLine().Append(indentString).Append("  DefinitionSql: ");
                    builder.AppendLine().Append(indentString).Append(new string(' ', 4)).Append(ViewDefinitionSql);
                }

                var mappings = EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + 4));
                    }
                }

                var columns = Columns.ToList();
                if (columns.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Columns: ");
                    foreach (var column in columns)
                    {
                        builder.AppendLine().Append(column.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent + 2));
                }
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}
