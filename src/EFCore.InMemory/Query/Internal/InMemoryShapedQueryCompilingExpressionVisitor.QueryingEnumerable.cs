// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public partial class InMemoryShapedQueryCompilingExpressionVisitor
    {
        private sealed class QueryingEnumerable<T> : IAsyncEnumerable<T>, IEnumerable<T>, IQueryingEnumerable
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<ValueBuffer> _innerEnumerable;
            private readonly Func<QueryContext, ValueBuffer, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly bool _concurrencyDetectionEnabled;

            public QueryingEnumerable(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerEnumerable,
                Func<QueryContext, ValueBuffer, T> shaper,
                Type contextType,
                bool standAloneStateManager,
                bool concurrencyDetectionEnabled)
            {
                _queryContext = queryContext;
                _innerEnumerable = innerEnumerable;
                _shaper = shaper;
                _contextType = contextType;
                _queryLogger = queryContext.QueryLogger;
                _standAloneStateManager = standAloneStateManager;
                _concurrencyDetectionEnabled = concurrencyDetectionEnabled;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new Enumerator(this, cancellationToken);

            public IEnumerator<T> GetEnumerator()
                => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

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

                private IEnumerator<ValueBuffer>? _enumerator;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken = default)
                {
                    _queryContext = queryingEnumerable._queryContext;
                    _innerEnumerable = queryingEnumerable._innerEnumerable;
                    _shaper = queryingEnumerable._shaper;
                    _contextType = queryingEnumerable._contextType;
                    _queryLogger = queryingEnumerable._queryLogger;
                    _standAloneStateManager = queryingEnumerable._standAloneStateManager;
                    _cancellationToken = cancellationToken;
                    Current = default!;

                    _concurrencyDetector = queryingEnumerable._concurrencyDetectionEnabled
                        ? _queryContext.ConcurrencyDetector
                        : null;
                }

                public T Current { get; private set; }

                object IEnumerator.Current
                    => Current!;

                public bool MoveNext()
                {
                    _concurrencyDetector?.EnterCriticalSection();

                    try
                    {
                        return MoveNextHelper();
                    }
                    catch (Exception exception)
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                    finally
                    {
                        _concurrencyDetector?.ExitCriticalSection();
                    }
                }

                public ValueTask<bool> MoveNextAsync()
                {
                    _concurrencyDetector?.EnterCriticalSection();

                    try
                    {
                        _cancellationToken.ThrowIfCancellationRequested();

                        return new ValueTask<bool>(MoveNextHelper());
                    }
                    catch (Exception exception)
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                    finally
                    {
                        _concurrencyDetector?.ExitCriticalSection();
                    }
                }

                private bool MoveNextHelper()
                {
                    if (_enumerator == null)
                    {
                        EntityFrameworkEventSource.Log.QueryExecuting();

                        _enumerator = _innerEnumerable.GetEnumerator();
                        _queryContext.InitializeStateManager(_standAloneStateManager);
                    }

                    var hasNext = _enumerator.MoveNext();

                    Current = hasNext
                        ? _shaper(_queryContext, _enumerator.Current)
                        : default!;

                    return hasNext;
                }

                public void Dispose()
                {
                    _enumerator?.Dispose();
                    _enumerator = null;
                }

                public ValueTask DisposeAsync()
                {
                    var enumerator = _enumerator;
                    _enumerator = null;

                    return enumerator.DisposeAsyncIfAvailable();
                }

                public void Reset()
                    => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
            }
        }
    }
}
