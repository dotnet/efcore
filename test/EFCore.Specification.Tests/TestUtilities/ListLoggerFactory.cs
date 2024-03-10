// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ListLoggerFactory(Func<string, bool> shouldLogCategory) : ILoggerFactory
{
    private readonly Func<string, bool> _shouldLogCategory = shouldLogCategory;
    private bool _disposed;

    public ListLoggerFactory()
        : this(_ => true)
    {
    }

    public List<(LogLevel Level, EventId Id, string? Message, object? State, Exception? Exception)> Log
        => Logger.LoggedEvents;

    protected ListLogger Logger { get; set; } = new ListLogger();

    public virtual void Clear()
        => Logger.Clear();

    public CancellationToken CancelQuery()
        => Logger.CancelOnNextLogEntry();

    public virtual IDisposable SuspendRecordingEvents()
        => Logger.SuspendRecordingEvents();

    public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        => Logger.TestOutputHelper = testOutputHelper;

    public virtual ILogger CreateLogger(string name)
    {
        CheckDisposed();

        return !_shouldLogCategory(name)
            ? NullLogger.Instance
            : Logger;
    }

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ListLoggerFactory));
        }
    }

    public void AddProvider(ILoggerProvider provider)
        => CheckDisposed();

    public void Dispose()
        => _disposed = true;

    public static string NormalizeLineEndings(string expectedString)
        => expectedString.Replace("\r", string.Empty).Replace("\n", Environment.NewLine);

    protected class ListLogger : ILogger
    {
        private readonly object _sync = new();
        private CancellationTokenSource? _cancellationTokenSource;
        protected bool IsRecordingSuspended { get; private set; }

        public ITestOutputHelper? TestOutputHelper { get; set; }

        public List<(LogLevel, EventId, string?, object?, Exception?)> LoggedEvents { get; }
            = [];

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

        public IDisposable SuspendRecordingEvents()
        {
            IsRecordingSuspended = true;
            return new RecordingSuspensionHandle(this);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            lock (_sync) // Guard against tests with explicit concurrency
            {
                var message = formatter(state, exception).Trim();
                UnsafeLog(logLevel, eventId, message, state, exception);
            }
        }

        protected virtual void UnsafeLog<TState>(
            LogLevel logLevel,
            EventId eventId,
            string? message,
            TState state,
            Exception? exception)
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

            if (!IsRecordingSuspended)
            {
                LoggedEvents.Add((logLevel, eventId, message, state, exception));
            }
        }

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public IDisposable? BeginScope(object state)
            => null;

        public IDisposable? BeginScope<TState>(TState state)  where TState : notnull
            => null;

        private class RecordingSuspensionHandle(ListLogger logger) : IDisposable
        {
            private readonly ListLogger _logger = logger;

            public void Dispose()
                => _logger.IsRecordingSuspended = false;
        }
    }
}
