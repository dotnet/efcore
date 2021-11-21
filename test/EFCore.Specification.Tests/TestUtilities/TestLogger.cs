// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestLogger : TestLoggerBase, IDiagnosticsLogger, ILogger
{
    public TestLogger(LoggingDefinitions definitions)
    {
        Definitions = definitions;
    }

    public ILoggingOptions Options
        => new LoggingOptions();

    public bool ShouldLogSensitiveData()
        => false;

    public ILogger Logger
        => this;

    public virtual LoggingDefinitions Definitions { get; }

    public IInterceptors Interceptors { get; }

    public bool IsEnabled(LogLevel logLevel)
        => EnabledFor == logLevel;

    public IDisposable BeginScope<TState>(TState state)
        => null;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        LoggedEvent = eventId;
        LoggedAt = logLevel;
        Assert.Equal(LoggedEvent, eventId);
        Message = formatter(state, exception);
    }
}
