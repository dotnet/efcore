// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public interface IEntityGraphAttacher
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void AttachGraph(
        InternalEntityEntry rootEntry,
        EntityState targetState,
        EntityState storeGeneratedWithKeySetTargetState,
        bool forceStateWhenUnknownKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    Task AttachGraphAsync(
        InternalEntityEntry rootEntry,
        EntityState targetState,
        EntityState storeGeneratedWithKeySetTargetState,
        bool forceStateWhenUnknownKey,
        CancellationToken cancellationToken = default);
}
