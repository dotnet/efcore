// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class StateManagerExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<InternalEntityEntry> ToListForState(
        this IStateManager stateManager,
        bool added = false,
        bool modified = false,
        bool deleted = false,
        bool unchanged = false,
        bool returnDeletedSharedIdentity = false)
    {
        var list = new List<InternalEntityEntry>(
            stateManager.GetCountForState(added, modified, deleted, unchanged, returnDeletedSharedIdentity));

        foreach (var entry in stateManager.GetEntriesForState(added, modified, deleted, unchanged, returnDeletedSharedIdentity))
        {
            list.Add(entry);
        }

        return list;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<InternalEntityEntry> ToList(
        this IStateManager stateManager)
        => stateManager.ToListForState(added: true, modified: true, deleted: true, unchanged: true, returnDeletedSharedIdentity: true);
}
