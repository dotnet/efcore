// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OperationLogger : ILogger
    {
        // TODO: Consider removing. Providers will need to react.
        private const string FormerlyWellKnownLoggerName = "Microsoft.EntityFrameworkCore.Tools";

        private readonly bool _enabledByName;
        private readonly IOperationReporter _reporter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OperationLogger([NotNull] string name, [NotNull] IOperationReporter reporter)
        {
            _enabledByName = name == FormerlyWellKnownLoggerName;
            _reporter = reporter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsEnabled(LogLevel logLevel)
            => logLevel == LogLevel.Warning
                || logLevel == LogLevel.Information
                || logLevel == LogLevel.Debug;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDisposable BeginScope<TState>(TState state)
            => null;

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
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var reportedState = state as IReportedLogData;
            if (reportedState == null && _enabledByName)
            {
                reportedState = new ReportedLogData<TState>(state, exception, formatter);
            }
            else if (reportedState == null)
            {
                return;
            }

            switch (logLevel)
            {
                case LogLevel.Warning:
                    _reporter.WriteWarning(reportedState.Message);
                    break;

                case LogLevel.Debug:
                    _reporter.WriteVerbose(reportedState.Message);
                    break;

                default:
                    Debug.Assert(logLevel == LogLevel.Information, "Unexpected logLevel: " + logLevel);
                    _reporter.WriteInformation(reportedState.Message);
                    break;
            }
        }

        private string GetMessage<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var builder = new StringBuilder();
            if (formatter != null)
            {
                builder.Append(formatter(state, exception));
            }
            else if (state != null)
            {
                builder.Append(state);

                if (exception != null)
                {
                    builder
                        .AppendLine()
                        .Append(exception);
                }
            }

            return builder.ToString();
        }
    }
}
