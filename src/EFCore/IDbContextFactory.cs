// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Defines a factory for creating <see cref="DbContext" /> instances.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContextFactory</see> for more information and examples.
/// </remarks>
/// <typeparam name="TContext">The <see cref="DbContext" /> type to create.</typeparam>
public interface IDbContextFactory<[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>
    where TContext : DbContext
{
    /// <summary>
    ///     Creates a new <see cref="DbContext" /> instance.
    /// </summary>
    /// <remarks>
    ///     The caller is responsible for disposing the context; it will not be disposed by any dependency injection container.
    /// </remarks>
    /// <returns>A new context instance.</returns>
    TContext CreateDbContext();

    /// <summary>
    ///     Creates a new <see cref="DbContext" /> instance in an async context.
    /// </summary>
    /// <remarks>
    ///     The caller is responsible for disposing the context; it will not be disposed by any dependency injection container.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task containing the created context that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}
