// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.FunctionalTests
{
    // Watch the log in PS with: "tail -f $env:userprofile\.klog\data-test.log"
    public class TestFileLogger : ILogger
    {
        public static readonly ILoggerFactory Factory = new TestFileLoggerFactory();

        private class TestFileLoggerFactory : ILoggerFactory
        {
            public LogLevel MinimumLevel { get; set; }

            public ILogger CreateLogger(string name)
            {
                return Instance;
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }
        }

        public static readonly ILogger Instance = new TestFileLogger();

        private readonly string _logFilePath;

        protected TestFileLogger(string fileName = "data-test.log")
        {
            var logDirectory
                = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), ".klog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(logDirectory, fileName);
        }

        public void Log(
            LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    lock (_logFilePath)
                    {
                        File.AppendAllText(_logFilePath, message + Environment.NewLine);
                    }
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope(object state)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return NullScope.Instance;
        }
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
