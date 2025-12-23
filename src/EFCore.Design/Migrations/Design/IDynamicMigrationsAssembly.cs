// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Extends <see cref="IMigrationsAssembly" /> to support dynamically compiled migrations
///     that are not part of the original assembly.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface IDynamicMigrationsAssembly : IMigrationsAssembly
{
    /// <summary>
    ///     Registers a dynamically compiled migration so it can be discovered and applied.
    /// </summary>
    /// <param name="compiledMigration">The compiled migration to register.</param>
    void RegisterDynamicMigration(CompiledMigration compiledMigration);

    /// <summary>
    ///     Clears all dynamically registered migrations.
    /// </summary>
    void ClearDynamicMigrations();

    /// <summary>
    ///     Gets a value indicating whether any dynamic migrations have been registered.
    /// </summary>
    bool HasDynamicMigrations { get; }
}
