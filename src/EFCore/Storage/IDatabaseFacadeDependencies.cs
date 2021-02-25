// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Exposes dependencies needed by <see cref="DatabaseFacade" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
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
        public ICoreSingletonOptions CoreOptions { get; }
    }
}
