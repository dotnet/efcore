// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         A service for creating <see cref="IModificationCommand" /> instances.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
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
public interface IModificationCommandFactory
{
    /// <summary>
    ///     Creates a new database CUD command.
    /// </summary>
    /// <param name="modificationCommandParameters">The creation parameters.</param>
    /// <returns>A new <see cref="IModificationCommand" /> instance.</returns>
    IModificationCommand CreateModificationCommand(
        in ModificationCommandParameters modificationCommandParameters);

    /// <summary>
    ///     Creates a new database CUD command.
    /// </summary>
    /// <param name="modificationCommandParameters">The creation parameters.</param>
    /// <returns>A new <see cref="INonTrackedModificationCommand" /> instance.</returns>
    INonTrackedModificationCommand CreateNonTrackedModificationCommand(
        in NonTrackedModificationCommandParameters modificationCommandParameters);
}
