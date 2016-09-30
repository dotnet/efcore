// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InterceptingLogger<T> : ILogger<T>
    {
        private static readonly string _name = typeof(T).DisplayName();

        private readonly ILogger _logger;
        private readonly WarningsConfiguration _warningsConfiguration;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InterceptingLogger(
            [NotNull] IDbContextServices contextServices,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] IDbContextOptions contextOptions)
        {
            _logger = (contextServices.LoggerFactory
                       ?? serviceProvider.GetRequiredService<ILoggerFactory>())
                .CreateLogger(_name);

            _warningsConfiguration
                = contextOptions
                    ?.FindExtension<CoreOptionsExtension>()
                    ?.WarningsConfiguration;
        }

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
            if (logLevel == LogLevel.Warning
                && state != null
                && _warningsConfiguration != null)
            {
                var warningBehavior = _warningsConfiguration.GetBehavior(state);

                if (warningBehavior == WarningBehavior.Throw)
                {
                    throw new InvalidOperationException(
                        CoreStrings.WarningAsErrorTemplate(
                            $"{state.GetType().Name}.{state}", formatter(state, exception)));
                }

                if (warningBehavior == WarningBehavior.Log
                    && IsEnabled(logLevel))
                {
                    _logger.Log(logLevel, eventId, state, exception,
                        (s, _) => CoreStrings.WarningLogTemplate(
                            formatter(s, _), $"{state.GetType().Name}.{state}"));
                }
            }
            else if (IsEnabled(logLevel))
            {
                _logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsEnabled(LogLevel logLevel)
            => _logger.IsEnabled(logLevel);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDisposable BeginScope<TState>(TState state)
            => _logger.BeginScope(state);
    }
}
