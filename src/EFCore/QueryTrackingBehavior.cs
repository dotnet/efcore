// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates how the results of a query are tracked by the <see cref="ChangeTracker" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-query-tracking">Tracking vs. no-tracking queries in EF Core</see> for more information and
///     examples.
/// </remarks>
public enum QueryTrackingBehavior
{
    /// <summary>
    ///     The change tracker will keep track of changes for all entities that are returned from a LINQ query.
    ///     Any modification to the entity instances will be detected and persisted to the database during
    ///     <see cref="DbContext.SaveChanges()" />.
    /// </summary>
    TrackAll = 0,

    /// <summary>
    ///     The change tracker will not track any of the entities that are returned from a LINQ query. If the
    ///     entity instances are modified, this will not be detected by the change tracker and
    ///     <see cref="DbContext.SaveChanges()" /> will not persist those changes to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Disabling change tracking is useful for read-only scenarios because it avoids the overhead of setting
    ///         up change tracking for each entity instance. You should not disable change tracking if you want to
    ///         manipulate entity instances and persist those changes to the database using
    ///         <see cref="DbContext.SaveChanges()" />.
    ///     </para>
    ///     <para>
    ///         Identity resolution will not be performed. If an entity with a given key is in different result in the result set
    ///         then they will be different instances.
    ///     </para>
    /// </remarks>
    NoTracking,

    /// <summary>
    ///     The change tracker will not track any of the entities that are returned from a LINQ query. If the
    ///     entity instances are modified, this will not be detected by the change tracker and
    ///     <see cref="DbContext.SaveChanges()" /> will not persist those changes to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Disabling change tracking is useful for read-only scenarios because it avoids the overhead of setting
    ///         up change tracking for each entity instance. You should not disable change tracking if you want to
    ///         manipulate entity instances and persist those changes to the database using
    ///         <see cref="DbContext.SaveChanges()" />.
    ///     </para>
    ///     <para>
    ///         Identity resolution will be performed to ensure that all occurrences of an entity with a given key
    ///         in the result set are represented by the same entity instance.
    ///     </para>
    /// </remarks>
    NoTrackingWithIdentityResolution
}
