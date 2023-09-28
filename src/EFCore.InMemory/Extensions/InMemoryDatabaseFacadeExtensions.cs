// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     In-memory specific extension methods for <see cref="DbContext.Database" />.
/// </summary>
public static class InMemoryDatabaseFacadeExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> if the database provider currently in use is the in-memory provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method can only be used after the <see cref="DbContext" /> has been configured because
    ///         it is only then that the provider is known. This means that this method cannot be used
    ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
    ///         provider to use as part of configuring the context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="database">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns><see langword="true" /> if the in-memory database is being used.</returns>
    public static bool IsInMemory(this DatabaseFacade database)
        => database.ProviderName == typeof(InMemoryOptionsExtension).Assembly.GetName().Name;
}
