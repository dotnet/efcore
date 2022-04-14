// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class FakeDiagnosticsLogger<T> : IDiagnosticsLogger<T>, ILogger
    where T : LoggerCategory<T>, new()
{
    public ILoggingOptions Options { get; } = new LoggingOptions();

    public bool ShouldLogSensitiveData()
        => false;

    public ILogger Logger
        => this;

    public DiagnosticSource DiagnosticSource { get; } = new DiagnosticListener("Fake");

    public IDbContextLogger DbContextLogger { get; } = new NullDbContextLogger();

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
    }

    public bool IsEnabled(LogLevel logLevel)
        => true;

    public bool IsEnabled(EventId eventId, LogLevel logLevel)
        => true;

    public IDisposable BeginScope<TState>(TState state)
        => null;

    public virtual LoggingDefinitions Definitions { get; } = new TestRelationalLoggingDefinitions();

    public IInterceptors Interceptors { get; }
}
