// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a structural property that refers to a type.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IReadOnlyStructuralProperty" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableStructuralProperty : IReadOnlyStructuralProperty, IMutablePropertyBase
{
    /// <summary>
    ///     Gets the type that this structural property refers to.
    /// </summary>
    new IMutableTypeBase TargetType { get; }
}
