// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class FakeLogger<T> : ILogger<T>
{
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

    public IDisposable BeginScope<TState>(TState state)
        => throw new NotImplementedException();
}
