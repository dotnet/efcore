// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     A factory returning pooled <see cref="DbContext" /> instances. Disposing the instance returned by this factory returns
///     them to the internal pooling mechanism.
/// </summary>
/// <remarks>
///     <para>
///         A service of this type is registered in the dependency injection container by the
///         <see cref="O:EntityFrameworkServiceCollectionExtensions.AddDbContextPool" /> methods.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see>,
///         <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContext factories</see>, and
///         <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
///     </para>
/// </remarks>
public class PooledDbContextFactory<[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>
    : IDbContextFactory<TContext>
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
    {
        _pool = pool;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PooledDbContextFactory{TContext}" /> class.
    /// </summary>
    /// <param name="options">The options to use for contexts produced by this factory.</param>
    /// <param name="poolSize">Sets the maximum number of instances retained by the pool. Defaults to 1024.</param>
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
    {
        var lease = new DbContextLease(_pool, standalone: true);
        lease.Context.SetLease(lease);

        return (TContext)lease.Context;
    }

    /// <inheritdoc />
    public virtual async Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var lease = new DbContextLease(_pool, standalone: true);
        await lease.Context.SetLeaseAsync(lease, cancellationToken).ConfigureAwait(false);

        return (TContext)lease.Context;
    }
}
