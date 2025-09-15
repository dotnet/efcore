// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Parameter object for <see cref="IStructuralTypeMaterializerSource" />.
/// </summary>
/// <param name="StructuralType">The entity or complex type being materialized.</param>
/// <param name="InstanceName">The name of the instance being materialized.</param>
/// <param name="ClrType">CLR type of the result.</param>
/// <param name="QueryTrackingBehavior">
///     The query tracking behavior, or <see langword="null" /> if this materialization is not from a query.
/// </param>
public readonly record struct StructuralTypeMaterializerSourceParameters(
    ITypeBase StructuralType,
    string InstanceName,
    Type ClrType,
    QueryTrackingBehavior? QueryTrackingBehavior);

/// <summary>
///     This type has been obsoleted, use <see cref="StructuralTypeMaterializerSourceParameters" /> instead.
/// </summary>
[Obsolete("This type has been obsoleted, use StructuralTypeMaterializerSourceParameters instead.", error: true)]
public readonly record struct EntityMaterializerSourceParameters(
    ITypeBase StructuralType,
    string InstanceName,
    QueryTrackingBehavior? QueryTrackingBehavior);
