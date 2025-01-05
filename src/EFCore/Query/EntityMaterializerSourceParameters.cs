// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Parameter object for <see cref="IEntityMaterializerSource" />.
/// </summary>
/// <param name="StructuralType">The entity or complex type being materialized.</param>
/// <param name="InstanceName">The name of the instance being materialized.</param>
/// <param name="QueryTrackingBehavior">
///     The query tracking behavior, or <see langword="null" /> if this materialization is not from a query.
/// </param>
public readonly record struct EntityMaterializerSourceParameters(
    ITypeBase StructuralType,
    string InstanceName,
    QueryTrackingBehavior? QueryTrackingBehavior);
