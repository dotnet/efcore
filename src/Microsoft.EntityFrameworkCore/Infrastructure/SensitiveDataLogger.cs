// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         The underlying logger to which logging information should be written.
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
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public SensitiveDataLogger(
            [NotNull] SensitiveDataLoggerDependencies<T> dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
            _logger = dependencies.Logger;
            _coreOptionsExtension
                = dependencies.ContextOptions?.Extensions
                    .OfType<CoreOptionsExtension>()
                    .FirstOrDefault();
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="SensitiveDataLogger{T}" />
        /// </summary>
        protected virtual SensitiveDataLoggerDependencies<T> Dependencies { get; }

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
