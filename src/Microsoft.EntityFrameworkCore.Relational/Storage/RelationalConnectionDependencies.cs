// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalConnection" />
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
    public sealed class RelationalConnectionDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalConnection" />.
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
        /// <param name="contextOptions"> The options for the current context instance. </param>
        /// <param name="logger"> The logger to write to. </param>
        /// <param name="diagnosticSource"> The diagnostic source to write to. </param>
        public RelationalConnectionDependencies(
            [NotNull] IDbContextOptions contextOptions,
            [NotNull] ILogger<IRelationalConnection> logger,
            [NotNull] DiagnosticSource diagnosticSource)
        {
            Check.NotNull(contextOptions, nameof(contextOptions));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(diagnosticSource, nameof(diagnosticSource));

            ContextOptions = contextOptions;
            Logger = logger;
            DiagnosticSource = diagnosticSource;
        }

        /// <summary>
        ///     The options for the current context instance.
        /// </summary>
        public IDbContextOptions ContextOptions { get; }

        /// <summary>
        ///     The logger to write to.
        /// </summary>
        public ILogger<IRelationalConnection> Logger { get; }

        /// <summary>
        ///     The diagnostic source to write to.
        /// </summary>
        public DiagnosticSource DiagnosticSource { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="contextOptions">
        ///     A replacement for the current dependency of this type.
        /// </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalConnectionDependencies With([NotNull] IDbContextOptions contextOptions)
            => new RelationalConnectionDependencies(contextOptions, Logger, DiagnosticSource);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger">
        ///     A replacement for the current dependency of this type.
        /// </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalConnectionDependencies With([NotNull] ILogger<IRelationalConnection> logger)
            => new RelationalConnectionDependencies(ContextOptions, logger, DiagnosticSource);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="diagnosticSource">
        ///     A replacement for the current dependency of this type.
        /// </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalConnectionDependencies With([NotNull] DiagnosticSource diagnosticSource)
            => new RelationalConnectionDependencies(ContextOptions, Logger, diagnosticSource);
    }
}
