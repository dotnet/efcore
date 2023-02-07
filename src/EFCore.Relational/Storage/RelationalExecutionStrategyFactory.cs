// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Factory for creating <see cref="IExecutionStrategy" /> instances for use with relational
///     database providers.
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
public class RelationalExecutionStrategyFactory : IExecutionStrategyFactory
{
    private readonly Func<ExecutionStrategyDependencies, IExecutionStrategy> _createExecutionStrategy;

    /// <summary>
    ///     Creates a new instance of this class with the given service dependencies.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public RelationalExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
    {
        Dependencies = dependencies;

        var configuredFactory = RelationalOptionsExtension.Extract(dependencies.Options).ExecutionStrategyFactory;

        _createExecutionStrategy = configuredFactory ?? CreateDefaultStrategy;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ExecutionStrategyDependencies Dependencies { get; }

    /// <summary>
    ///     Creates or returns a cached instance of the default <see cref="IExecutionStrategy" /> for the
    ///     current database provider.
    /// </summary>
    protected virtual IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
        => new NonRetryingExecutionStrategy(dependencies);

    /// <summary>
    ///     Creates an <see cref="IExecutionStrategy" /> for the current database provider.
    /// </summary>
    public virtual IExecutionStrategy Create()
        => _createExecutionStrategy(Dependencies);
}
