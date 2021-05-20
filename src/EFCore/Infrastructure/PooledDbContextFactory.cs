// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A factory returning pooled <see cref="DbContext" /> instances. Disposing the instance returned by this factory returns
    ///         them to the internal pooling mechanism.
    ///     </para>
    ///     <para>
    ///         A service of this type is registered in the dependency injection container by the
    ///         <see cref="M:EntityFrameworkServiceCollectionExtensions.AddDbContextPool" /> methods.
    ///     </para>
    /// </summary>
    public class PooledDbContextFactory<TContext> : IDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly IDbContextPool<TContext> _pool;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public PooledDbContextFactory(IDbContextPool<TContext> pool)
            => _pool = pool;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PooledDbContextFactory{TContext}" /> class.
        /// </summary>
        /// <param name="options"> The options to use for contexts produced by this factory. </param>
        /// <param name="poolSize"> Sets the maximum number of instances retained by the pool. Defaults to 1024. </param>
        public PooledDbContextFactory(DbContextOptions<TContext> options, int poolSize = DbContextPool<DbContext>.DefaultPoolSize)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>(options);

            var extension = (options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension())
                .WithMaxPoolSize(poolSize);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            _pool = new DbContextPool<TContext>(optionsBuilder.Options);
        }

        /// <inheritdoc />
        public virtual TContext CreateDbContext()
            => (TContext)new DbContextLease(_pool, standalone: true).Context;
    }
}
