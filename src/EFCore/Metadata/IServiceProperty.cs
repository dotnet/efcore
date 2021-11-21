// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a property on an entity type that represents an
///     injected service from the <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IServiceProperty : IReadOnlyServiceProperty, IPropertyBase
{
    /// <summary>
    ///     Gets the entity type that this property belongs to.
    /// </summary>
    new IEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     The <see cref="ServiceParameterBinding" /> for this property.
    /// </summary>
    new ServiceParameterBinding ParameterBinding { get; }
}
