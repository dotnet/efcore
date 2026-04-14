// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an element within a JSON column in the relational model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IRelationalJsonElement
{
    /// <summary>
    ///     Gets the JSON property name of this element, if contained within a JSON object.
    /// </summary>
    string? PropertyName { get; }

    /// <summary>
    ///     Gets the column that contains this JSON element.
    /// </summary>
    IColumnBase ContainingColumn { get; }

    /// <summary>
    ///     Gets the type mapping for this JSON element.
    /// </summary>
    RelationalTypeMapping? StoreTypeMapping { get; }

    /// <summary>
    ///     Gets the path segments from the root of the JSON document to this element.
    /// </summary>
    IReadOnlyList<JsonPathSegment> Path { get; }

    /// <summary>
    ///     Gets the parent element, or <see langword="null" /> if this is a root element.
    /// </summary>
    IRelationalJsonElement? ParentElement { get; }

    /// <summary>
    ///     Gets a value indicating whether this element can be <see langword="null" /> in JSON.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    ///     Gets the property mappings for this element.
    /// </summary>
    IReadOnlyList<IJsonElementMapping> PropertyMappings { get; }
}
