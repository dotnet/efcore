// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Defines a factory for creating <see cref="DbContext" /> instances.
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
        ///         The caller is responsible for disposing the context; it will not be disposed by any dependency injection container.
        ///     </para>
        /// </summary>
        /// <returns> A new context instance. </returns>
        TContext CreateDbContext();
    }
}
