// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Parameter object for <see cref="IEntityMaterializerSource" />.
/// </summary>
public readonly record struct EntityMaterializerSourceParameters
{
    /// <summary>
    ///     Creates a new <see cref="EntityMaterializerSourceParameters" />.
    /// </summary>
    /// <param name="entityType">The entity type being materialized.</param>
    /// <param name="entityInstanceName">The name of the instance being materialized.</param>
    /// <param name="queryTrackingBehavior">The query tracking behavior, or <see langword="null" /> if this materialization is not from a query.</param>
    public EntityMaterializerSourceParameters(
        IEntityType entityType,
        string entityInstanceName,
        QueryTrackingBehavior? queryTrackingBehavior)
    {
        EntityType = entityType;
        EntityInstanceName = entityInstanceName;
        QueryTrackingBehavior = queryTrackingBehavior;
    }

    /// <summary>
    ///     The entity type being materialized.
    /// </summary>
    public IEntityType EntityType { get; }

    /// <summary>
    ///     The name of the instance being materialized.
    /// </summary>
    public string EntityInstanceName { get; }

    /// <summary>
    ///     The query tracking behavior, or <see langword="null" /> if this materialization is not from a query.
    /// </summary>
    public QueryTrackingBehavior? QueryTrackingBehavior { get; }
}
