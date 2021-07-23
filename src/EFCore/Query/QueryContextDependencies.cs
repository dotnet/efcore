// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="QueryContext" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed record QueryContextDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="QueryContext" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public QueryContextDependencies(
            ICurrentDbContext currentContext,
            IExecutionStrategyFactory executionStrategyFactory,
            IConcurrencyDetector concurrencyDetector,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger,
            IDiagnosticsLogger<DbLoggerCategory.Query> queryLogger)
        {
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(executionStrategyFactory, nameof(executionStrategyFactory));
            Check.NotNull(concurrencyDetector, nameof(concurrencyDetector));
            Check.NotNull(commandLogger, nameof(commandLogger));
            Check.NotNull(queryLogger, nameof(queryLogger));

            CurrentContext = currentContext;
            ExecutionStrategyFactory = executionStrategyFactory;
            ConcurrencyDetector = concurrencyDetector;
            CommandLogger = commandLogger;
            QueryLogger = queryLogger;
        }

        /// <summary>
        ///     The cache being used to store value generator instances.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; init; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public IStateManager StateManager
            => CurrentContext.GetDependencies().StateManager;

        /// <summary>
        ///     Gets the query provider.
        /// </summary>
        [Obsolete("Use the service by getting it from " + nameof(CurrentContext) + ".")]
        public IQueryProvider QueryProvider
            => CurrentContext.GetDependencies().QueryProvider;

        /// <summary>
        ///     The execution strategy.
        /// </summary>
        public IExecutionStrategyFactory ExecutionStrategyFactory { get; init; }

        /// <summary>
        ///     Gets the concurrency detector.
        /// </summary>
        public IConcurrencyDetector ConcurrencyDetector { get; init; }

        /// <summary>
        ///     The command logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger { get; init; }

        /// <summary>
        ///     A query logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Query> QueryLogger { get; init; }
    }
}
