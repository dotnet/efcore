// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents type base mapping to a table-like object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface ITableMappingBase : IAnnotatable
{
    /// <summary>
    ///     Gets the mapped type base.
    /// </summary>
    ITypeBase TypeBase { get; }

    /// <summary>
    ///     Gets the target table-like object.
    /// </summary>
    ITableBase Table { get; }

    /// <summary>
    ///     Gets the properties mapped to columns on the target table.
    /// </summary>
    IEnumerable<IColumnMappingBase> ColumnMappings { get; }

    /// <summary>
    ///     Gets the value indicating whether this is the mapping for the principal entity type
    ///     if the table-like object is shared. <see langword="null" /> if the table-like object is not shared.
    /// </summary>
    bool? IsSharedTablePrincipal { get; }

    /// <summary>
    ///     Gets the value indicating whether this is the mapping for the principal table-like object
    ///     if the entity type is split. <see langword="null" /> if the entity type is not split.
    /// </summary>
    bool? IsSplitEntityTypePrincipal { get; }

    /// <summary>
    ///     Gets the value indicating whether the mapped table-like object includes rows for the derived entity types.
    ///     Set to <see langword="false" /> for inherited mappings. <see langword="null" /> if the entity type has no derived types.
    /// </summary>
    bool? IncludesDerivedTypes { get; }

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

        try
        {
            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("TableMapping: ");
            }

            builder
                .Append(TypeBase.Name)
                .Append(" - ")
                .Append(Table.Name);

            if (IncludesDerivedTypes != null)
            {
                builder.Append(' ');
                if (!IncludesDerivedTypes.Value)
                {
                    builder.Append('!');
                }

                builder.Append("IncludesDerivedTypes");
            }

            if (IsSharedTablePrincipal != null)
            {
                builder.Append(' ');
                if (!IsSharedTablePrincipal.Value)
                {
                    builder.Append('!');
                }

                builder.Append("IsSharedTablePrincipal");
            }

            if (IsSplitEntityTypePrincipal != null)
            {
                builder.Append(' ');
                if (!IsSplitEntityTypePrincipal.Value)
                {
                    builder.Append('!');
                }

                builder.Append("IsSplitEntityTypePrincipal");
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent + 2));
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}
