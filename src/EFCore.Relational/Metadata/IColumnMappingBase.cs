// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents property mapping to a column-like object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IColumnMappingBase : IAnnotatable
{
    /// <summary>
    ///     Gets the mapped property.
    /// </summary>
    IProperty Property { get; }

    /// <summary>
    ///     Gets the target column-like object.
    /// </summary>
    IColumnBase Column { get; }

    /// <summary>
    ///     Gets the type mapping for the column-like object.
    /// </summary>
    RelationalTypeMapping TypeMapping { get; }

    /// <summary>
    ///     Gets the containing table mapping.
    /// </summary>
    ITableMappingBase TableMapping { get; }
}
