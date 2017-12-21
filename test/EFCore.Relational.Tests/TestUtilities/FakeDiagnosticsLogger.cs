// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class FakeDiagnosticsLogger<T> : IDiagnosticsLogger<T>, ILogger
        where T : LoggerCategory<T>, new()
    {
        public ILoggingOptions Options { get; }

        public bool ShouldLogSensitiveData() => false;

        public ILogger Logger => this;

        public DiagnosticSource DiagnosticSource { get; } = new DiagnosticListener("Fake");

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public bool IsEnabled(EventId eventId, LogLevel logLevel) => true;

        public WarningBehavior GetLogBehavior(EventId eventId, LogLevel logLevel) => WarningBehavior.Log;

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
