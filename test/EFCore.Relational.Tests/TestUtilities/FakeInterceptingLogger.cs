// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities
{
    public class FakeInterceptingLogger<T> : IInterceptingLogger<T>
        where T : LoggerCategory<T>, new()
    {
        public ILoggingOptions Options { get; }

        public bool ShouldLogSensitiveData(IDiagnosticsLogger<T> diagnostics) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(EventId eventId, LogLevel logLevel) => true;

        public IDisposable BeginScope(object state)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
