// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ExecutionStrategyFactory : IExecutionStrategyFactory
{
    private readonly NonRetryingExecutionStrategy _instance;

    /// <summary>
    ///     Creates a new instance of this class with the given service dependencies.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public ExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
    {
        Dependencies = dependencies;
        _instance = new NonRetryingExecutionStrategy(Dependencies);
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ExecutionStrategyDependencies Dependencies { get; }

    /// <summary>
    ///     Creates a new <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <returns>An instance of <see cref="IExecutionStrategy" />.</returns>
    public virtual IExecutionStrategy Create()
        => _instance;
}
