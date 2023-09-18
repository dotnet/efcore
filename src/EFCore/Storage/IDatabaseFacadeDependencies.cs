// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Exposes dependencies needed by <see cref="DatabaseFacade" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
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
public interface IDatabaseFacadeDependencies
{
    /// <summary>
    ///     The transaction manager.
    /// </summary>
    IDbContextTransactionManager TransactionManager { get; }

    /// <summary>
    ///     The database creator.
    /// </summary>
    IDatabaseCreator DatabaseCreator { get; }

    /// <summary>
    ///     The execution strategy.
    /// </summary>
    IExecutionStrategy ExecutionStrategy { get; }

    /// <summary>
    ///     The execution strategy factory.
    /// </summary>
    IExecutionStrategyFactory ExecutionStrategyFactory { get; }

    /// <summary>
    ///     The registered database providers.
    /// </summary>
    IEnumerable<IDatabaseProvider> DatabaseProviders { get; }

    /// <summary>
    ///     A command logger.
    /// </summary>
    IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger { get; }

    /// <summary>
    ///     The concurrency detector.
    /// </summary>
    IConcurrencyDetector ConcurrencyDetector { get; }

    /// <summary>
    ///     The core options.
    /// </summary>
    ICoreSingletonOptions CoreOptions { get; }

    /// <summary>
    ///     The async query provider.
    /// </summary>
    IAsyncQueryProvider QueryProvider { get; }

    /// <summary>
    ///     The ad-hoc type mapper.
    /// </summary>
    IAdHocMapper AdHocMapper { get; }

    /// <summary>
    ///     The <see cref="TypeMappingSource" />.
    /// </summary>
    ITypeMappingSource TypeMappingSource { get; }
}
