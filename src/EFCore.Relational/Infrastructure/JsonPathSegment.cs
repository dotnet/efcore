// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Represents a single segment in a JSON path. A segment is either a property name or an array index placeholder.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public sealed class JsonPathSegment
{
    /// <summary>
    ///     Creates a new <see cref="JsonPathSegment" /> for a named property.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    public JsonPathSegment(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        PropertyName = propertyName;
        IsArray = false;
    }

    private JsonPathSegment(bool isArray)
    {
        PropertyName = null;
        IsArray = isArray;
    }

    /// <summary>
    ///     Creates a new <see cref="JsonPathSegment" /> for an array index placeholder.
    /// </summary>
    /// <returns>A new array index placeholder segment.</returns>
    public static JsonPathSegment Array { get; } = new(isArray: true);

    /// <summary>
    ///     Gets the JSON property name. <see langword="null" /> for array index placeholder segments.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    ///     Gets a value indicating whether this segment represents an array index placeholder.
    /// </summary>
    public bool IsArray { get; }

    /// <inheritdoc />
    public override string ToString()
        => IsArray ? "[]" : PropertyName!;
}
