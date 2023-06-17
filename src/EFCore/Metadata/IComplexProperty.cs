// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a complex property of a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IComplexProperty : IReadOnlyComplexProperty, IPropertyBase
{
    /// <summary>
    ///     Gets the associated complex type.
    /// </summary>
    new IComplexType ComplexType { get; }
}
