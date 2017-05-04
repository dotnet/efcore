// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Relational.Design.Specification.TestUtilities
{
    public class TestDesignLoggerFactory : ILoggerFactory
    {
        public readonly DesignLogger Logger = new DesignLogger();

        ILogger ILoggerFactory.CreateLogger(string categoryName) => Logger;

        void ILoggerFactory.AddProvider(ILoggerProvider provider) => throw new NotImplementedException();

        void IDisposable.Dispose()
        {
        }

        public class DesignLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => Statements.Add(logLevel + ": " + formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public readonly ICollection<string> Statements = new List<string>();

            private class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }

    public class DesignLogger<T> : TestDesignLoggerFactory.DesignLogger, IInterceptingLogger<T>
        where T : LoggerCategory<T>, new()
    {
        public bool IsEnabled(EventId eventId, LogLevel logLevel) => true;

        public ILoggingOptions Options { get; }

        public bool ShouldLogSensitiveData(IDiagnosticsLogger<T> diagnostics) => false;
    }
}
