// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;

namespace Microsoft.Data.Entity.Update
{
    /// <summary>
    ///     The type of value to get or set when accessing values of properties tracked by the context.
    /// </summary>
    public enum ValueSource
    {
        /// <summary>
        ///     The value currently assigned to the property.
        /// </summary>
        Current,

        /// <summary>
        ///     The value assigned to the given property when it was retrieved from the database.
        /// </summary>
        Original,

        /// <summary>
        ///     The value assigned to the property when a snapshot was last taken by <see cref="ChangeTracker.DetectChanges" />.
        /// </summary>
        RelationshipSnapshot
    }
}
