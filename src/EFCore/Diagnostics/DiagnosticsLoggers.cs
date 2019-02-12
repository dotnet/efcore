// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A collection of <see cref="IDiagnosticsLogger{TLoggerCategory}" /> for
    ///     different logger categories. The producer is responsible for adding
    ///     the loggers needed by the consumer.
    /// </summary>
    public readonly struct DiagnosticsLoggers
    {
        private readonly IDiagnosticsLogger[] _loggers;

        /// <summary>
        ///     Creates a new collection of loggers.
        /// </summary>
        /// <param name="loggers"> The loggers </param>
        public DiagnosticsLoggers([NotNull] params IDiagnosticsLogger[] loggers)
        {
            Check.NotNull(loggers, nameof(loggers));

            _loggers = loggers;
        }

        /// <summary>
        ///     Gets the logger for the given category, or null if it is not available.
        /// </summary>
        /// <typeparam name="T"> The logging category. </typeparam>
        /// <returns> The logger. </returns>
        public IDiagnosticsLogger<T> GetLogger<T>() where T : LoggerCategory<T>, new()
            => _loggers.OfType<IDiagnosticsLogger<T>>().FirstOrDefault();
    }
}
