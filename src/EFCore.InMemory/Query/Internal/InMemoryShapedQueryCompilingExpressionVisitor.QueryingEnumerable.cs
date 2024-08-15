// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class InMemoryShapedQueryCompilingExpressionVisitor
{
    private sealed class QueryingEnumerable<T>(
        QueryContext queryContext,
        IEnumerable<ValueBuffer> innerEnumerable,
        Func<QueryContext, ValueBuffer, T> shaper,
        Type contextType,
        bool standAloneStateManager,
        bool threadSafetyChecksEnabled)
        : IAsyncEnumerable<T>, IEnumerable<T>, IQueryingEnumerable
    {
        private readonly QueryContext _queryContext = queryContext;
        private readonly IEnumerable<ValueBuffer> _innerEnumerable = innerEnumerable;
        private readonly Func<QueryContext, ValueBuffer, T> _shaper = shaper;
        private readonly Type _contextType = contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger = queryContext.QueryLogger;
        private readonly bool _standAloneStateManager = standAloneStateManager;
        private readonly bool _threadSafetyChecksEnabled = threadSafetyChecksEnabled;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(this, cancellationToken);

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        public string ToQueryString()
            => InMemoryStrings.NoQueryStrings;

        private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<ValueBuffer> _innerEnumerable;
            private readonly Func<QueryContext, ValueBuffer, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly CancellationToken _cancellationToken;
            private readonly IConcurrencyDetector? _concurrencyDetector;
            private readonly IExceptionDetector _exceptionDetector;

            private IEnumerator<ValueBuffer>? _enumerator;

            public Enumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken = default)
            {
                this._queryContext = queryingEnumerable._queryContext;
                this._innerEnumerable = queryingEnumerable._innerEnumerable;
                this._shaper = queryingEnumerable._shaper;
                this._contextType = queryingEnumerable._contextType;
                this._queryLogger = queryingEnumerable._queryLogger;
                this._standAloneStateManager = queryingEnumerable._standAloneStateManager;
                this._cancellationToken = cancellationToken;
                this._exceptionDetector = this._queryContext.ExceptionDetector;
                this.Current = default!;

                this._concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                    ? this._queryContext.ConcurrencyDetector
                    : null;
            }

            public T Current { get; private set; }

            object IEnumerator.Current
                => this.Current!;

            public bool MoveNext()
            {
                try
                {
                    using var _ = this._concurrencyDetector?.EnterCriticalSection();

                    return this.MoveNextHelper();
                }
                catch (Exception exception)
                {
                    if (this._exceptionDetector.IsCancellation(exception))
                    {
                        this._queryLogger.QueryCanceled(this._contextType);
                    }
                    else
                    {
                        this._queryLogger.QueryIterationFailed(this._contextType, exception);
                    }

                    throw;
                }
            }

            public ValueTask<bool> MoveNextAsync()
            {
                try
                {
                    using var _ = this._concurrencyDetector?.EnterCriticalSection();

                    this._cancellationToken.ThrowIfCancellationRequested();

                    return ValueTask.FromResult(this.MoveNextHelper());
                }
                catch (Exception exception)
                {
                    if (this._exceptionDetector.IsCancellation(exception, this._cancellationToken))
                    {
                        this._queryLogger.QueryCanceled(this._contextType);
                    }
                    else
                    {
                        this._queryLogger.QueryIterationFailed(this._contextType, exception);
                    }

                    throw;
                }
            }

            private bool MoveNextHelper()
            {
                if (this._enumerator == null)
                {
                    EntityFrameworkMetricsData.ReportQueryExecuting();

                    this._enumerator = this._innerEnumerable.GetEnumerator();
                    this._queryContext.InitializeStateManager(this._standAloneStateManager);
                }

                var hasNext = this._enumerator.MoveNext();

                this.Current = hasNext
                    ? this._shaper(this._queryContext, this._enumerator.Current)
                    : default!;

                return hasNext;
            }

            public void Dispose()
            {
                this._enumerator?.Dispose();
                this._enumerator = null;
            }

            public ValueTask DisposeAsync()
            {
                var enumerator = this._enumerator;
                this._enumerator = null;

                return enumerator.DisposeAsyncIfAvailable();
            }

            public void Reset()
                => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
        }
    }
}
