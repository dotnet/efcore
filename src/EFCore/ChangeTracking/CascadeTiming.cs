// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Defines different strategies for when cascading actions will be performed.
///     See <see cref="ChangeTracker.CascadeDeleteTiming" /> and <see cref="ChangeTracker.DeleteOrphansTiming" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information and
///     examples.
/// </remarks>
public enum CascadeTiming
{
    /// <summary>
    ///     Cascading actions are made to dependent/child entities as soon as the principal/parent
    ///     entity changes.
    /// </summary>
    Immediate,

    /// <summary>
    ///     Cascading actions are made to dependent/child entities as part of <see cref="DbContext.SaveChanges()" />.
    /// </summary>
    OnSaveChanges,

    /// <summary>
    ///     Cascading actions are never made automatically to dependent/child entities, but must instead
    ///     be triggered by an explicit call.
    /// </summary>
    Never
}
