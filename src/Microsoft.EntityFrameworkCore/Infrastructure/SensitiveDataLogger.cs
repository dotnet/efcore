// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class SensitiveDataLogger<T> : ISensitiveDataLogger<T>
    {
        private readonly ILogger<T> _logger;
        private readonly CoreOptionsExtension _coreOptionsExtension;

        public SensitiveDataLogger(
            [NotNull] ILogger<T> logger, [CanBeNull] IDbContextOptions contextOptions)
        {
            _logger = logger;

            _coreOptionsExtension
                = contextOptions?.Extensions
                    .OfType<CoreOptionsExtension>()
                    .FirstOrDefault();
        }

        public virtual bool LogSensitiveData
        {
            get
            {
                if (_coreOptionsExtension == null)
                {
                    return false;
                }

                if (_coreOptionsExtension.IsSensitiveDataLoggingEnabled
                    && !_coreOptionsExtension.SensitiveDataLoggingWarned)
                {
                    _logger.LogWarning(
                        CoreLoggingEventId.SensitiveDataLoggingEnabledWarning,
                        () => CoreStrings.SensitiveDataLoggingEnabled);

                    _coreOptionsExtension.SensitiveDataLoggingWarned = true;
                }

                return _coreOptionsExtension.IsSensitiveDataLoggingEnabled;
            }
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);

        bool ILogger.IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        IDisposable ILogger.BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }
}
