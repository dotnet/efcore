// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class DiagnosticsLogger<TLoggerCategory> : IDiagnosticsLogger<TLoggerCategory>
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        private readonly WarningsConfiguration _warningsConfiguration;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DiagnosticsLogger(
            [NotNull] ILoggerFactory loggerFactory,
            [CanBeNull] ILoggingOptions loggingOptions,
            [NotNull] DiagnosticSource diagnosticSource)
        {
            DiagnosticSource = diagnosticSource;
            Logger = loggerFactory.CreateLogger(new TLoggerCategory());

            Options = loggingOptions;
            _warningsConfiguration = loggingOptions?.WarningsConfiguration;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ILoggingOptions Options { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ILogger Logger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DiagnosticSource DiagnosticSource { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ShouldLogSensitiveData()
        {
            var options = Options;
            if (options == null)
            {
                return false;
            }

            if (options.IsSensitiveDataLoggingEnabled
                && !options.IsSensitiveDataLoggingWarned)
            {
                this.SensitiveDataLoggingEnabledWarning();

                options.IsSensitiveDataLoggingWarned = true;
            }

            return options.IsSensitiveDataLoggingEnabled;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual WarningBehavior GetLogBehavior(EventId eventId, LogLevel logLevel)
        {
            var warningBehavior = _warningsConfiguration?.GetBehavior(eventId) ??
                                  (logLevel == LogLevel.Warning
                                   && _warningsConfiguration?.DefaultBehavior == WarningBehavior.Throw
                                      ? WarningBehavior.Throw
                                      : WarningBehavior.Log);

            return warningBehavior == WarningBehavior.Log
                ? Logger.IsEnabled(logLevel) ? WarningBehavior.Log : WarningBehavior.Ignore
                : warningBehavior;
        }
    }
}
