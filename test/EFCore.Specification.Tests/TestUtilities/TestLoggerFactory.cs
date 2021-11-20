// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestLoggerFactory : ILoggerFactory
    {
        public TestLogger Logger { get; }

        public LogLevel? LoggedAt
            => Logger.LoggedAt;

        public EventId LoggedEvent
            => Logger.LoggedEvent;

        public string Message
            => Logger.Message;

        public TestLoggerFactory(LoggingDefinitions definitions)
        {
            Logger = new TestLogger(definitions);
        }

        public ILogger CreateLogger(string categoryName)
            => Logger;

        public void AddProvider(ILoggerProvider provider)
            => throw new NotSupportedException();

        public void Dispose() { }
    }
}
