// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    // Watch the log in PS with: "tail -f $env:userprofile\.klog\data-test.log"
    public class TestFileLogger : ILogger
    {
        public static readonly ILoggerFactory Factory = new TestFileLoggerFactory();

        private class TestFileLoggerFactory : ILoggerFactory
        {
            public ILogger CreateLogger(string name) => Instance;

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public void Dispose()
            {
            }
        }

        public static readonly ILogger Instance = new TestFileLogger();

        private readonly string _logFilePath;

        protected TestFileLogger(string fileName = "data-test.log")
        {
            var homeDir = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
            var logDirectory = Path.Combine(homeDir, ".klog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(logDirectory, fileName);
        }

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter?.Invoke(state, exception);

            if (!string.IsNullOrWhiteSpace(message))
            {
                lock (_logFilePath)
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    }

    public class NullScope : IDisposable
    {
        public static NullScope Instance = new NullScope();

        public void Dispose()
        {
            // intentionally does nothing
        }
    }
}
