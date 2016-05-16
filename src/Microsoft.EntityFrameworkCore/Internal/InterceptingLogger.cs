// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class InterceptingLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;
        private readonly List<IWarningsAsErrorsOptionsExtension> _warningsAsErrorsOptionsExtensions;

        public InterceptingLogger(
            [NotNull] IDbContextServices contextServices,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] IDbContextOptions contextOptions)
        {
            _logger = (contextServices.LoggerFactory
                       ?? serviceProvider.GetRequiredService<ILoggerFactory>())
                .CreateLogger(typeof(T).DisplayName());

            _warningsAsErrorsOptionsExtensions
                = contextOptions?.Extensions
                    .OfType<IWarningsAsErrorsOptionsExtension>()
                    .ToList();
        }

        public virtual void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Warning
                && _warningsAsErrorsOptionsExtensions.Count > 0)
            {
                var stateAsEnum = state as Enum;

                if (stateAsEnum != null
                    && _warningsAsErrorsOptionsExtensions.Any(oe => oe.WarningIsError(stateAsEnum)))
                {
                    var enumType = state.GetType();

                    throw new InvalidOperationException(
                        CoreStrings.WarningAsError(
                            $"{enumType.Name}.{Enum.GetName(enumType, stateAsEnum)}",
                            formatter(state, exception)));
                }
            }

            if (IsEnabled(logLevel))
            {
                _logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        public virtual bool IsEnabled(LogLevel logLevel)
            => _logger.IsEnabled(logLevel);

        public virtual IDisposable BeginScope<TState>(TState state)
            => _logger.BeginScope(state);
    }
}
