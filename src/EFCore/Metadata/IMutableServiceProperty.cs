// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     A <see cref="IReadOnlyPropertyBase" /> in the Entity Framework model that represents an
///     injected service from the <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IServiceProperty" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableServiceProperty : IReadOnlyServiceProperty, IMutablePropertyBase
{
    /// <summary>
    ///     Gets the type that this property belongs to.
    /// </summary>
    new IMutableEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets or sets <see cref="ServiceParameterBinding" /> for this property.
    /// </summary>
    new ServiceParameterBinding? ParameterBinding { get; set; }
}
