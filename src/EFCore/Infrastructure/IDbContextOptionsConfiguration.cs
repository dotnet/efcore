// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Configures the options to be used by a <see cref="DbContext" />. You normally call
///     <see cref="EntityFrameworkServiceCollectionExtensions.ConfigureDbContext{TContext}(IServiceCollection, Action{DbContextOptionsBuilder}, ServiceLifetime)" />
///     to register this class, it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TContext">The type of the context these options apply to.</typeparam>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
/// </remarks>
public interface IDbContextOptionsConfiguration<TContext>
    where TContext : DbContext
{
    /// <summary>
    ///     Applies the specified configuration.
    /// </summary>
    /// <param name="serviceProvider">The application's <see cref="IServiceProvider" /> that can be used to resolve services.</param>
    /// <param name="optionsBuilder">The options being configured.</param>
    void Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder);
}
