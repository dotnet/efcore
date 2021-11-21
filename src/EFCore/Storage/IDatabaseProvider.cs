// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The primary point where a database provider can tell EF that it has been selected for the current context
///         and provide the services required for it to function.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IDatabaseProvider
{
    /// <summary>
    ///     The unique name used to identify the database provider. This should be the same as the NuGet package name
    ///     for the providers runtime.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The value of the <see cref="AssemblyInformationalVersionAttribute.InformationalVersion" />
    ///     for the database provider assembly.
    /// </summary>
    string? Version
        => null;

    /// <summary>
    ///     Gets a value indicating whether this database provider has been configured for a given context.
    /// </summary>
    /// <param name="options">The options for the context.</param>
    /// <returns><see langword="true" /> if the database provider has been configured, otherwise <see langword="false" />.</returns>
    bool IsConfigured(IDbContextOptions options);
}
