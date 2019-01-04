// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestLogger<TCategory> : TestLoggerBase, IDiagnosticsLogger<TCategory>, ILogger
        where TCategory : LoggerCategory<TCategory>, new()
    {
        public ILoggingOptions Options => null;

        public WarningBehavior GetLogBehavior(EventId eventId, LogLevel logLevel)
        {
            LoggedEvent = eventId;
            return EnabledFor == logLevel ? WarningBehavior.Log : WarningBehavior.Ignore;
        }

        public bool IsEnabled(LogLevel logLevel) => EnabledFor == logLevel;

        public IDisposable BeginScope<TState>(TState state) => null;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LoggedAt = logLevel;
            Assert.Equal(LoggedEvent, eventId);
            Message = formatter(state, exception);
        }

        public bool ShouldLogSensitiveData() => false;

        public ILogger Logger => this;
    }
}
