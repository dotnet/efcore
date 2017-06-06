// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="IExecutionStrategy" />
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
    /// </summary>
    public sealed class ExecutionStrategyDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="IExecutionStrategy" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="currentDbContext"> Indirection to the current <see cref="DbContext" /> instance. </param>
        /// <param name="options"> The options for the current <see cref="DbContext" /> instance. </param>
        /// <param name="logger"> A logger.</param>
        public ExecutionStrategyDependencies(
            [NotNull] ICurrentDbContext currentDbContext,
            [CanBeNull] IDbContextOptions options,
            [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
        {
            Check.NotNull(currentDbContext, nameof(currentDbContext));

            Options = options;
            CurrentDbContext = currentDbContext;
            Logger = logger;
        }

        /// <summary>
        ///     The options for the current <see cref="DbContext" /> instance.
        /// </summary>
        public IDbContextOptions Options { get; }

        /// <summary>
        ///     Indirection to the current <see cref="DbContext" /> instance.
        /// </summary>
        public ICurrentDbContext CurrentDbContext { get; }

        /// <summary>
        ///     The logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Infrastructure> Logger { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentDbContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ExecutionStrategyDependencies With([NotNull] ICurrentDbContext currentDbContext)
            => new ExecutionStrategyDependencies(currentDbContext, Options, Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="options"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ExecutionStrategyDependencies With([NotNull] IDbContextOptions options)
            => new ExecutionStrategyDependencies(CurrentDbContext, options, Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ExecutionStrategyDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
            => new ExecutionStrategyDependencies(CurrentDbContext, Options, logger);
    }
}
