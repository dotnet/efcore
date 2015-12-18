// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Sqlite.Design.FunctionalTests
{

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
        public IDisposable BeginScopeImpl(object state) => NullScope.Instance;
        public string FullLog => _sb.ToString();
        private readonly StringBuilder _sb = new StringBuilder();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            _sb.Append(logLevel)
                .Append(": ")
                .Append(formatter(state, exception));
        }
    }
}