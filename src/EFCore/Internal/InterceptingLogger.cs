// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InterceptingLogger<TLoggerCategory> : IInterceptingLogger<TLoggerCategory>
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        private readonly ILogger _logger;
        private readonly WarningsConfiguration _warningsConfiguration;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InterceptingLogger(
            [NotNull] ILoggerFactory loggerFactory,
            [CanBeNull] ILoggingOptions loggingOptions)
        {
            _logger = loggerFactory.CreateLogger(new TLoggerCategory());

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
        public virtual void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var warningBehavior = _warningsConfiguration?.GetBehavior(eventId);

            if (warningBehavior != WarningBehavior.Ignore)
            {
                if (ShouldThrow(logLevel, warningBehavior))
                {
                    throw new InvalidOperationException(
                        CoreStrings.WarningAsErrorTemplate(
                            eventId.ToString(), formatter(state, exception)));
                }

                if (_logger.IsEnabled(logLevel))
                {
                    _logger.Log(logLevel, eventId, state, exception, formatter);
                }
            }
        }

        private bool ShouldThrow(LogLevel logLevel, WarningBehavior? warningBehavior)
            => warningBehavior == WarningBehavior.Throw
               || (logLevel == LogLevel.Warning
                   && _warningsConfiguration?.DefaultBehavior == WarningBehavior.Throw);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ShouldLogSensitiveData(IDiagnosticsLogger<TLoggerCategory> diagnostics)
        {
            var options = Options;
            if (options == null)
            {
                return false;
            }

            if (options.SensitiveDataLoggingEnabled
                && !options.SensitiveDataLoggingWarned)
            {
                diagnostics.SensitiveDataLoggingEnabledWarning();

                options.SensitiveDataLoggingWarned = true;
            }

            return options.SensitiveDataLoggingEnabled;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsEnabled(EventId eventId, LogLevel logLevel)
        {
            var warningBehavior = _warningsConfiguration?.GetBehavior(eventId);

            return warningBehavior != WarningBehavior.Ignore
                   && (ShouldThrow(logLevel, warningBehavior)
                       || _logger.IsEnabled(logLevel));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDisposable BeginScope<TState>(TState state)
            => _logger.BeginScope(state);
    }
}
