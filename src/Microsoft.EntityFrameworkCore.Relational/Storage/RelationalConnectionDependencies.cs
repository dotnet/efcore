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
    /// </summary>
    public sealed class RelationalConnectionDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalConnection" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from provider or application code as it may change
        ///         as new dependencies are added. Use the 'With...' methods instead.
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
        /// <returns></returns>
        public RelationalConnectionDependencies With([NotNull] IDbContextOptions contextOptions)
            => new RelationalConnectionDependencies(Check.NotNull(contextOptions, nameof(contextOptions)), Logger, DiagnosticSource);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger">
        ///     A replacement for the current dependency of this type.
        /// </param>
        /// <returns></returns>
        public RelationalConnectionDependencies With([NotNull] ILogger<IRelationalConnection> logger)
            => new RelationalConnectionDependencies(ContextOptions, Check.NotNull(logger, nameof(logger)), DiagnosticSource);
    }
}
