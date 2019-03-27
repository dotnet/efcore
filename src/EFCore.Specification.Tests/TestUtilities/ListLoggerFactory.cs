// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ListLoggerFactory : ILoggerFactory
    {
        private readonly Func<string, bool> _shouldLogCategory;

        public ListLoggerFactory()
            : this(_ => true)
        {
        }

        public ListLoggerFactory(Func<string, bool> shouldLogCategory)
        {
            _shouldLogCategory = shouldLogCategory;
            Logger = new ListLogger();
        }

        public List<(LogLevel Level, EventId Id, string Message, object State, Exception Exception)> Log => Logger.LoggedEvents;
        protected ListLogger Logger { get; set; }

        public virtual void Clear() => Logger.Clear();
        public CancellationToken CancelQuery() => Logger.CancelOnNextLogEntry();

        public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        {
            Logger.TestOutputHelper = testOutputHelper;
        }

        public virtual ILogger CreateLogger(string name)
            => !_shouldLogCategory(name)
                ? (ILogger)NullLogger.Instance
                : Logger;

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }

        protected class ListLogger : ILogger
        {
            private readonly object _sync = new object();
            private CancellationTokenSource _cancellationTokenSource;

            public ITestOutputHelper TestOutputHelper { get; set; }

            public List<(LogLevel, EventId, string, object, Exception)> LoggedEvents { get; }
                = new List<(LogLevel, EventId, string, object, Exception)>();

            public CancellationToken CancelOnNextLogEntry()
            {
                lock (_sync) // Guard against tests with explicit concurrency
                {
                    _cancellationTokenSource = new CancellationTokenSource();

                    return _cancellationTokenSource.Token;
                }
            }

            public void Clear()
            {
                lock (_sync) // Guard against tests with explicit concurrency
                {
                    UnsafeClear();
                }
            }

            protected virtual void UnsafeClear()
            {
                LoggedEvents.Clear();
                _cancellationTokenSource = null;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                lock (_sync) // Guard against tests with explicit concurrency
                {
                    var message = formatter(state, exception)?.Trim();
                    UnsafeLog(logLevel, eventId, message, state, exception);
                }
            }

            protected virtual void UnsafeLog<TState>(
                LogLevel logLevel, EventId eventId, string message, TState state, Exception exception)
            {
                if (message != null)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource = null;
                    }
                    TestOutputHelper?.WriteLine(message + Environment.NewLine);
                }

                LoggedEvents.Add((logLevel, eventId, message, state, exception));
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope(object state) => null;

            public IDisposable BeginScope<TState>(TState state) => null;
        }
    }
}
