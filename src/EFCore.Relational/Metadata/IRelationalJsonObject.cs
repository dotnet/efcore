// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a JSON object element within a JSON column in the relational model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IRelationalJsonObject : IRelationalJsonElement
{
    /// <summary>
    ///     Gets the child elements (objects, arrays, and scalar properties) of this JSON object,
    ///     in their declaration order.
    /// </summary>
    IReadOnlyList<IRelationalJsonElement> Properties { get; }

    /// <summary>
    ///     Finds a child element by its JSON property name.
    /// </summary>
    /// <param name="name">The JSON property name.</param>
    /// <returns>The child element, or <see langword="null" /> if not found.</returns>
    IRelationalJsonElement? FindProperty(string name);
}
