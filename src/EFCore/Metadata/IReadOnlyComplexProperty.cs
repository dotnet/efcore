// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a complex property of a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyComplexProperty : IReadOnlyPropertyBase
{
    /// <summary>
    ///     Gets the associated complex type.
    /// </summary>
    IReadOnlyComplexType ComplexType { get; }

    /// <summary>
    ///     Gets a value indicating whether this property can contain <see langword="null" />.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    ///     Gets a value indicating whether this property represents a collection.
    /// </summary>
    bool IsCollection { get; }

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
                builder.Append($"ComplexProperty: {DeclaringType.DisplayName()}.");
            }

            builder.Append(Name).Append(" (");

            var field = GetFieldName();
            if (field == null)
            {
                builder.Append("no field, ");
            }
            else if (!field.EndsWith(">k__BackingField", StringComparison.Ordinal))
            {
                builder.Append(field).Append(", ");
            }

            builder.Append(ClrType.ShortDisplayName()).Append(')');

            if (IsShadowProperty())
            {
                builder.Append(" Shadow");
            }

            if (IsIndexerProperty())
            {
                builder.Append(" Indexer");
            }

            if (!IsNullable)
            {
                builder.Append(" Required");
            }

            if (Sentinel != null && !Equals(Sentinel, ClrType.GetDefaultValue()))
            {
                builder.Append(" Sentinel:").Append(Sentinel);
            }

            if (GetPropertyAccessMode() != PropertyAccessMode.PreferField)
            {
                builder.Append(" PropertyAccessMode.").Append(GetPropertyAccessMode());
            }

            if ((options & MetadataDebugStringOptions.IncludePropertyIndexes) != 0
                && ((AnnotatableBase)this).IsReadOnly)
            {
                var indexes = ((IProperty)this).GetPropertyIndexes();
                builder.Append(' ').Append(indexes.Index);
                builder.Append(' ').Append(indexes.OriginalValueIndex);
                builder.Append(' ').Append(indexes.RelationshipIndex);
                builder.Append(' ').Append(indexes.ShadowIndex);
                builder.Append(' ').Append(indexes.StoreGenerationIndex);
            }

            if (!singleLine)
            {
                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent + 2));
                }

                builder
                    .AppendLine()
                    .Append(ComplexType.ToDebugString(options, indent + 2));
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}
