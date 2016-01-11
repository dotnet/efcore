// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class TestLoggerFactory : LoggerFactory
    {
        public TestLoggerFactory()
        {
            Logger = new TestLogger();
            AddProvider(new TestLoggerProvider(Logger));
        }

        public TestLogger Logger { get; }
    }

    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public TestLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string name) => _logger;

        public void Dispose()
        {
        }
    }

    public class TestLogger : ILogger
    {
        public IDisposable BeginScopeImpl(object state) => new NullScope();
        public string FullLog => _sb.ToString();
        private readonly StringBuilder _sb = new StringBuilder();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            _sb.Append(logLevel)
                .Append(": ")
                .Append(formatter(state, exception));
        }

        public class NullScope : IDisposable
        {
            public void Dispose()
            {
                // intentionally does nothing
            }
        }
    }
}
