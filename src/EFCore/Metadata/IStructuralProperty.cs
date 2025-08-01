// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a structural property that refers to a type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IStructuralProperty : IReadOnlyStructuralProperty, IPropertyBase
{
    /// <summary>
    ///     Gets the type that this structural property refers to.
    /// </summary>
    new ITypeBase TargetType { get; }
}
