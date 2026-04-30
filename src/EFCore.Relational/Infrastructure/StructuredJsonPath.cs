// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Represents a structured JSON path consisting of property name segments and array index placeholders,
///     along with runtime index values for array positions.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class StructuredJsonPath
{
    /// <summary>
    ///     A <see cref="StructuredJsonPath" /> representing the root of a JSON document (<c>$</c>).
    /// </summary>
    public static StructuredJsonPath Root { get; } = new([], []);

    /// <summary>
    ///     Creates a new <see cref="StructuredJsonPath" /> instance.
    /// </summary>
    /// <param name="segments">The path segments.</param>
    /// <param name="indices">
    ///     The index values for array index placeholders. Must have one entry for each segment
    ///     where <see cref="StructuredJsonPathSegment.IsArray" /> is <see langword="true" />.
    /// </param>
    public StructuredJsonPath(IReadOnlyList<StructuredJsonPathSegment> segments, int[] indices)
    {
        var arraySegmentCount = segments.Count(s => s.IsArray);
        if (indices.Length != arraySegmentCount)
        {
            throw new ArgumentException(
                CoreStrings.InvalidStructuredJsonPathIndexCount(indices.Length, arraySegmentCount),
                nameof(indices));
        }

        Segments = segments;
        Indices = indices;
    }

    /// <summary>
    ///     Gets the path segments.
    /// </summary>
    public virtual IReadOnlyList<StructuredJsonPathSegment> Segments { get; }

    /// <summary>
    ///     Gets the index values for array index placeholders. The indices are applied in order
    ///     to the segments where <see cref="StructuredJsonPathSegment.IsArray" /> is <see langword="true" />.
    /// </summary>
    public virtual int[] Indices { get; }

    /// <summary>
    ///     Gets a value indicating whether this path represents the root of a JSON document (<c>$</c>).
    /// </summary>
    public virtual bool IsRoot
        => Segments.Count == 0;

    /// <summary>
    ///     Appends the JSON path string representation to the given <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <returns>The same <see cref="StringBuilder" /> for chaining.</returns>
    public virtual StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append('$');

        var indexPosition = 0;
        foreach (var segment in Segments)
        {
            if (segment.IsArray)
            {
                builder.Append('[');
                builder.Append(Indices[indexPosition++]);
                builder.Append(']');
            }
            else
            {
                builder.Append('.');
                builder.Append(segment.PropertyName);
            }
        }

        return builder;
    }

    /// <inheritdoc />
    public override string ToString()
        => AppendTo(new StringBuilder()).ToString();
}
