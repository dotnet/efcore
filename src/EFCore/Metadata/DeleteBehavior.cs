// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Indicates how a delete operation is applied to dependent entities in a relationship when the principal is deleted
    ///     or the relationship is severed.
    /// </summary>
    public enum DeleteBehavior
    {
        /// <summary>
        ///     The delete operation is not applied to dependent entities. The dependent entities remain unchanged.
        /// </summary>
        Restrict,

        /// <summary>
        ///     The foreign key properties in dependent entities are set to null. This cascading behavior is only applied
        ///     to entities that are being tracked by the context. A corresponding cascade behavior should be setup in the
        ///     database to ensure data that is not being tracked by the context has the same action applied. If you use
        ///     EF to create the database, this cascade behavior will be setup for you.
        /// </summary>
        SetNull,

        /// <summary>
        ///     Dependent entities are also deleted. This cascading behavior is only applied
        ///     to entities that are being tracked by the context. A corresponding cascade behavior should be setup in the
        ///     database to ensure data that is not being tracked by the context has the same action applied. If you use
        ///     EF to create the database, this cascade behavior will be setup for you.
        /// </summary>
        Cascade
    }
}
