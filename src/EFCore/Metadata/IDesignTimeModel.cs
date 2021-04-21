// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         The metadata about the shape of entities, the relationships between them, and how they map to the database.
    ///         Also includes all the information necessary to initialize the database.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IDesignTimeModel
    {
        /// <summary>
        ///     Gets the metadata about the shape of entities, the relationships between them, and how they map to the database.
        ///     Also includes all the information necessary to initialize the database.
        /// </summary>
        public IModel Model { get; }
    }
}
