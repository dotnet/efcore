// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SensitiveDataLogger<T> : ISensitiveDataLogger<T>
    {
        private readonly ILogger<T> _logger;
        private readonly RelationalOptionsExtension _relationalOptionsExtension;

        public SensitiveDataLogger(
            [NotNull] ILogger<T> logger, [CanBeNull] IDbContextOptions contextOptions)
        {
            _logger = logger;

            _relationalOptionsExtension
                = contextOptions?.Extensions
                    .OfType<RelationalOptionsExtension>()
                    .FirstOrDefault();
        }

        public virtual bool LogSensitiveData
        {
            get
            {
                if (_relationalOptionsExtension == null)
                {
                    return false;
                }

                if (_relationalOptionsExtension.LogSqlParameterValues
                    && !_relationalOptionsExtension.LogSqlParameterValuesWarned)
                {
                    _logger.LogWarning(RelationalStrings.ParameterLoggingEnabled);

                    _relationalOptionsExtension.LogSqlParameterValuesWarned = true;
                }

                return _relationalOptionsExtension.LogSqlParameterValues;
            }
        }

        void ILogger.Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);

        bool ILogger.IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        IDisposable ILogger.BeginScopeImpl(object state) => _logger.BeginScopeImpl(state);
    }
}
