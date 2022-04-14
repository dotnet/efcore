// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Factory for <see cref="IExecutionStrategy" /> instances.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IExecutionStrategyFactory
{
    /// <summary>
    ///     Creates a new <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <returns>An instance of <see cref="IExecutionStrategy" />.</returns>
    IExecutionStrategy Create();
}
