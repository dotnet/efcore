// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     An interface to identify query roots in LINQ.
    /// </summary>
    public interface IEntityQueryable
    {
        /// <summary>
        ///     Detach context if associated with this query root.
        /// </summary>
        IEntityQueryable DetachContext();

        /// <summary>
        ///     Return entity type this query root references.
        /// </summary>
        IEntityType EntityType { get; }
    }
}
