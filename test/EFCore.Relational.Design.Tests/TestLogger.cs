// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Relational.Design
{
    public class TestLogger<T> : IInterceptingLogger<T>
        where T : LoggerCategory<T>, new()
    {
        public IDisposable BeginScope<TState>(TState state) => new NullScope();
        public string FullLog => _sb.ToString();
        private readonly StringBuilder _sb = new StringBuilder();

        public bool IsEnabled(EventId eventId, LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => _sb.Append(logLevel)
                .Append(": ")
                .Append(formatter(state, exception));

        public ILoggingOptions Options { get; }

        public bool ShouldLogSensitiveData(IDiagnosticsLogger<T> diagnostics) => false;

        public class NullScope : IDisposable
        {
            public void Dispose()
            {
                // intentionally does nothing
            }
        }
    }
}
