// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Defines a factory for creating <see cref="DbContext" /> instances.
    ///     A service of this type is registered in the dependency injection container by the
    ///     <see cref="M:EntityFrameworkServiceCollectionExtensions.AddDbContextPool" /> methods.
    /// </summary>
    /// <typeparam name="TContext"> The <see cref="DbContext" /> type to create. </typeparam>
    public interface IDbContextFactory<out TContext>
        where TContext : DbContext
    {
        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="DbContext" /> instance.
        ///     </para>
        ///     <para>
        ///         The caller is responsible for disposing the context; it will not be disposed by the dependency injection container.
        ///     </para>
        /// </summary>
        /// <returns> A new context instance. </returns>
        TContext CreateDbContext();
    }
}
