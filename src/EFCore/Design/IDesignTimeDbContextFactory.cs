// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     A factory for creating derived <see cref="DbContext" /> instances. Implement this interface to enable
///     design-time services for context types that do not have a public default constructor. At design-time,
///     derived <see cref="DbContext" /> instances can be created in order to enable specific design-time
///     experiences such as Migrations. Design-time services will automatically discover implementations of
///     this interface that are in the startup assembly or the same assembly as the derived context.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
/// <typeparam name="TContext">The type of the context.</typeparam>
public interface IDesignTimeDbContextFactory<out TContext>
    where TContext : DbContext
{
    /// <summary>
    ///     Creates a new instance of a derived context.
    /// </summary>
    /// <param name="args">Arguments provided by the design-time service.</param>
    /// <returns>An instance of <typeparamref name="TContext" />.</returns>
    TContext CreateDbContext(string[] args);
}
