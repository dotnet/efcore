// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     The state in which an entity is being tracked by a context.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
/// </remarks>
public enum EntityState
{
    /// <summary>
    ///     The entity is not being tracked by the context.
    /// </summary>
    Detached = 0,

    /// <summary>
    ///     The entity is being tracked by the context and exists in the database. Its property
    ///     values have not changed from the values in the database.
    /// </summary>
    Unchanged = 1,

    /// <summary>
    ///     The entity is being tracked by the context and exists in the database. It has been marked
    ///     for deletion from the database.
    /// </summary>
    Deleted = 2,

    /// <summary>
    ///     The entity is being tracked by the context and exists in the database. Some or all of its
    ///     property values have been modified.
    /// </summary>
    Modified = 3,

    /// <summary>
    ///     The entity is being tracked by the context but does not yet exist in the database.
    /// </summary>
    Added = 4
}
