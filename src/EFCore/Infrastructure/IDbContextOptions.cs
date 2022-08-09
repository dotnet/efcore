// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     The options to be used by a <see cref="DbContext" />. You normally override
///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder" />
///     to create instances of classes that implement this interface, they are not designed to be directly created
///     in your application code.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IDbContextOptions
{
    /// <summary>
    ///     Gets the extensions that store the configured options.
    /// </summary>
    IEnumerable<IDbContextOptionsExtension> Extensions { get; }

    /// <summary>
    ///     Gets the extension of the specified type. Returns null if no extension of the specified type is configured.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension to get.</typeparam>
    /// <returns>The extension, or <see langword="null" /> if none was found.</returns>
    TExtension? FindExtension<TExtension>()
        where TExtension : class, IDbContextOptionsExtension;
}
