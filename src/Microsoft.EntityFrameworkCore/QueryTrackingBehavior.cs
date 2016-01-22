// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Indicates how the results of a query are tracked by the <see cref="ChangeTracker" />.
    /// </summary>
    public enum QueryTrackingBehavior
    {
        /// <summary>
        ///     The change tracker will keep track of changes for all entities that are returned from a LINQ query.
        ///     Any modification to the entity instances will be detected and persisted to the database during
        ///     <see cref="DbContext.SaveChanges()" />.
        /// </summary>
        TrackAll = 0,

        /// <summary>
        ///     <para>
        ///         The change tracker will not track any of the entities that are returned from a LINQ query. If the
        ///         entity instances are modified, this will not be detected by the change tracker and
        ///         <see cref="DbContext.SaveChanges()" /> will not persist those changes to the database.
        ///     </para>
        ///     <para>
        ///         Disabling change tracking is useful for read-only scenarios because it avoids the overhead of setting
        ///         up change tracking for each entity instance. You should not disable change tracking if you want to
        ///         manipulate entity instances and persist those changes to the database using
        ///         <see cref="DbContext.SaveChanges()" />.
        ///     </para>
        ///     <para>
        ///         Identity resolution will still be performed to ensure that all occurrences of an entity with a given key
        ///         in the result set are represented by the same entity instance.
        ///     </para>
        /// </summary>
        NoTracking
    }
}
