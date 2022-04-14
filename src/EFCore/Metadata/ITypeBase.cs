// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a type in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface ITypeBase : IReadOnlyTypeBase, IAnnotatable
{
    /// <summary>
    ///     Gets the model that this type belongs to.
    /// </summary>
    new IModel Model { get; }
}
