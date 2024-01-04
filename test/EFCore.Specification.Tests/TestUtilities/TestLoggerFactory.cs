// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestLoggerFactory(LoggingDefinitions definitions) : ILoggerFactory
{
    public TestLogger Logger { get; } = new TestLogger(definitions);

    public LogLevel? LoggedAt
        => Logger.LoggedAt;

    public EventId LoggedEvent
        => Logger.LoggedEvent;

    public string Message
        => Logger.Message;

    public ILogger CreateLogger(string categoryName)
        => Logger;

    public void AddProvider(ILoggerProvider provider)
        => throw new NotSupportedException();

    public void Dispose() { }
}
