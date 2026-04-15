// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Represents a structured JSON path consisting of property name segments and array index placeholders,
///     along with runtime ordinal values for array positions.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class JsonPath
{
    /// <summary>
    ///     A <see cref="JsonPath" /> representing the root of a JSON document (<c>$</c>).
    /// </summary>
    public static JsonPath Root { get; } = new([], []);

    /// <summary>
    ///     Creates a new <see cref="JsonPath" /> instance.
    /// </summary>
    /// <param name="segments">The path segments.</param>
    /// <param name="ordinals">
    ///     The ordinal values for array index placeholders. Must have one entry for each segment
    ///     where <see cref="JsonPathSegment.IsArray" /> is <see langword="true" />.
    /// </param>
    public JsonPath(IReadOnlyList<JsonPathSegment> segments, int[] ordinals)
    {
        var arraySegmentCount = segments.Count(s => s.IsArray);
        if (ordinals.Length != arraySegmentCount)
        {
            throw new ArgumentException(
                CoreStrings.InvalidJsonPathOrdinalCount(ordinals.Length, arraySegmentCount),
                nameof(ordinals));
        }

        Segments = segments;
        Ordinals = ordinals;
    }

    /// <summary>
    ///     Gets the path segments.
    /// </summary>
    public virtual IReadOnlyList<JsonPathSegment> Segments { get; }

    /// <summary>
    ///     Gets the ordinal values for array index placeholders. The ordinals are applied in order
    ///     to the segments where <see cref="JsonPathSegment.IsArray" /> is <see langword="true" />.
    /// </summary>
    public virtual int[] Ordinals { get; }

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

        var ordinalIndex = 0;
        foreach (var segment in Segments)
        {
            if (segment.IsArray)
            {
                builder.Append('[');
                builder.Append(Ordinals[ordinalIndex++]);
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
