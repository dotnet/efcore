// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A wrapping logger for which logging of sensitive data can be enabled or disabled.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="T"> The type who's name is used for the logger category name. </typeparam>
    public class SensitiveDataLogger<T> : ISensitiveDataLogger<T>
    {
        private readonly ILogger<T> _logger;
        private readonly CoreOptionsExtension _coreOptionsExtension;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SensitiveDataLogger{T}" /> class.
        /// </summary>
        /// <param name="logger">
        ///     The underlying logger to which logging information should be written.
        /// </param>
        /// <param name="contextOptions">
        ///     The options for the context that this logger is being used with.
        /// </param>
        public SensitiveDataLogger(
            [NotNull] ILogger<T> logger, [CanBeNull] IDbContextOptions contextOptions)
        {
            _logger = logger;

            _coreOptionsExtension
                = contextOptions?.Extensions
                    .OfType<CoreOptionsExtension>()
                    .FirstOrDefault();
        }

        /// <summary>
        ///     Gets a value indicating whether sensitive information should be written to the underlying logger.
        /// </summary>
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
                        CoreEventId.SensitiveDataLoggingEnabledWarning,
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
