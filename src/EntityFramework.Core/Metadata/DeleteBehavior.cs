// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata
{
    /// <summary>
    ///     Indicates how a delete operation is applied to dependent entities in a relationship when the principal is deleted.
    /// </summary>
    public enum DeleteBehavior
    {
        /// <summary>
        ///     The delete operation is not applied to dependent entities. The dependent entities remain unchanged.
        /// </summary>
        Restrict,

        /// <summary>
        ///     The foreign key properties in dependent entities are set to null.
        /// </summary>
        SetNull,

        /// <summary>
        ///     Dependent entities are also deleted.
        /// </summary>
        Cascade
    }
}
