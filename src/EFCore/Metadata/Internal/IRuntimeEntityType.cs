// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public interface IRuntimeEntityType : IEntityType, IRuntimeTypeBase
{
    /// <summary>
    ///     Gets the base type of this entity type. Returns <see langword="null" /> if this is not a derived type in an inheritance
    ///     hierarchy.
    /// </summary>
    new IRuntimeEntityType? BaseType
        => (IRuntimeEntityType?)((IEntityType)this).BaseType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    Func<IInternalEntry, ISnapshot> RelationshipSnapshotFactory { get; }
}
