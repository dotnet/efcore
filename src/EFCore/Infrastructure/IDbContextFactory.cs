// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A factory for creating derived <see cref="DbContext" /> instances. Implement this interface to enable
    ///     design-time services for context types that do not have a public default constructor. At design-time,
    ///     derived <see cref="DbContext" /> instances can be created in order to enable specific design-time
    ///     experiences such as Migrations. Design-time services will automatically discover implementations of
    ///     this interface that are in the same assembly as the derived context.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface IDbContextFactory<out TContext>
        where TContext : DbContext
    {
        /// <summary>
        ///     Creates a new instance of a derived context.
        /// </summary>
        /// <param name="options"> Information about the environment an application is running in. </param>
        /// <returns> An instance of <typeparamref name="TContext" />. </returns>
        TContext Create([NotNull] DbContextFactoryOptions options);
    }
}
