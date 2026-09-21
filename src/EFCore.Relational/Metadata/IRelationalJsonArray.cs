// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a JSON array element within a JSON column in the relational model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IRelationalJsonArray : IRelationalJsonElement
{
    /// <summary>
    ///     Gets the element type of the array items. This could be an object, another array, or a scalar property.
    /// </summary>
    IRelationalJsonElement ElementType { get; }
}
