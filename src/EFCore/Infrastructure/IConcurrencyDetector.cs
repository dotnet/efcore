// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Used by EF internal code and database providers to detect concurrent access to non-thread-safe
///     resources.
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
public interface IConcurrencyDetector
{
    /// <summary>
    ///     Enters a critical section.
    /// </summary>
    /// <returns>A disposer that will exit the critical section when disposed.</returns>
    ConcurrencyDetectorCriticalSectionDisposer EnterCriticalSection();

    /// <summary>
    ///     Exits the critical section.
    /// </summary>
    void ExitCriticalSection();
}
