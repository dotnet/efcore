// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Used by EF internal code and database providers to detect various types of exceptions.
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
public interface IExceptionDetector
{
    /// <summary>
    ///     Returns whether the provided exception represents a cancellation event for the current provider.
    /// </summary>
    /// <param name="exception">The exception to be checked for cancellation.</param>
    /// <param name="cancellationToken">
    ///     If <paramref name="exception" /> is insufficient for identifying a cancellation, this is the cancellation token passed to the
    ///     asynchronous operation; it can be checked instead as a fallback mechanism.
    /// </param>
    public bool IsCancellation(Exception exception, CancellationToken cancellationToken = default);
}
