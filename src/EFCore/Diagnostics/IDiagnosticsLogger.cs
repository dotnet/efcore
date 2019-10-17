// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Combines <see cref="ILogger" /> and <see cref="DiagnosticSource" />
    ///         for use by all EF Core logging so that events can be sent to both <see cref="ILogger" />
    ///         for ASP.NET and <see cref="DiagnosticSource" /> for everything else.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IDiagnosticsLogger
    {
        /// <summary>
        ///     Entity Framework logging options.
        /// </summary>
        ILoggingOptions Options { get; }

        /// <summary>
        ///     Caching for logging definitions.
        /// </summary>
        LoggingDefinitions Definitions { get; }

        /// <summary>
        ///     Gets a value indicating whether sensitive information should be written
        ///     to the underlying logger. This also has the side effect of writing a warning
        ///     to the log the first time sensitive data is logged.
        /// </summary>
        bool ShouldLogSensitiveData();

        /// <summary>
        ///     The underlying <see cref="ILogger" />.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        ///     The <see cref="DiagnosticSource" />.
        /// </summary>
        DiagnosticSource DiagnosticSource { get; }
    }
}
