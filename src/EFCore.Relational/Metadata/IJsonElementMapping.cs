// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a mapping between a model property and a JSON element in the relational model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IJsonElementMapping
{
    /// <summary>
    ///     Gets the model property that is mapped to the JSON element.
    /// </summary>
    IPropertyBase Property { get; }

    /// <summary>
    ///     Gets the JSON element in the relational model.
    /// </summary>
    IRelationalJsonElement Element { get; }

    /// <summary>
    ///     Gets the table mapping that contains this JSON element mapping.
    /// </summary>
    ITableMappingBase TableMapping { get; }
}
