// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryExecutor : IQueryExecutor
    {
        private readonly ContextService<DbContext> _context;
        private readonly ContextService<DataStore> _dataStore;
        private readonly LazyRef<ILogger> _logger;

        public EntityQueryExecutor(
            [NotNull] ContextService<DbContext> context,
            [NotNull] ContextService<DataStore> dataStore,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(context, "context");
            Check.NotNull(dataStore, "dataStore");
            Check.NotNull(loggerFactory, "loggerFactory");

            _context = context;
            _dataStore = dataStore;
            _logger = new LazyRef<ILogger>(loggerFactory.Create<EntityQueryExecutor>);
        }

        public virtual T ExecuteScalar<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            return ExecuteCollection<T>(queryModel).First();
        }

        public virtual Task<T> ExecuteScalarAsync<T>(
            [NotNull] QueryModel queryModel, CancellationToken cancellationToken)
        {
            Check.NotNull(queryModel, "queryModel");

            return AsyncExecuteCollection<T>(queryModel, cancellationToken).First(cancellationToken);
        }

        public virtual T ExecuteSingle<T>([NotNull] QueryModel queryModel, bool _)
        {
            Check.NotNull(queryModel, "queryModel");

            return ExecuteCollection<T>(queryModel).First();
        }

        public virtual Task<T> ExecuteSingleAsync<T>(
            [NotNull] QueryModel queryModel, bool _, CancellationToken cancellationToken)
        {
            Check.NotNull(queryModel, "queryModel");

            return AsyncExecuteCollection<T>(queryModel, cancellationToken).First(cancellationToken);
        }

        [DebuggerStepThrough]
        public virtual IEnumerable<T> ExecuteCollection<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            _logger.Value.WriteInformation(queryModel, Strings.LogCompilingQueryModel);

            try
            {
                var enumerable = _dataStore.Service.Query<T>(queryModel);

                return new EnumerableExceptionInterceptor<T>(enumerable, _context.Service, _logger);
            }
            catch (Exception ex)
            {
                _logger.Value.WriteError(
                    new DataStoreErrorLogState(_context.Service.GetType()),
                    ex,
                    (state, exception) =>
                        Strings.LogExceptionDuringQueryIteration(Environment.NewLine, exception));

                throw;
            }
        }

        [DebuggerStepThrough]
        public virtual IAsyncEnumerable<T> AsyncExecuteCollection<T>(
            [NotNull] QueryModel queryModel, CancellationToken cancellationToken)
        {
            Check.NotNull(queryModel, "queryModel");

            _logger.Value.WriteInformation(queryModel, Strings.LogCompilingQueryModel);

            try
            {
                var asyncEnumerable
                    = _dataStore.Service
                        .AsyncQuery<T>(queryModel, cancellationToken);

                return new AsyncEnumerableExceptionInterceptor<T>(asyncEnumerable, _context.Service, _logger);
            }
            catch (Exception ex)
            {
                _logger.Value.WriteError(
                    new DataStoreErrorLogState(_context.Service.GetType()),
                    ex,
                    (state, exception) =>
                        Strings.LogExceptionDuringQueryIteration(Environment.NewLine, exception));

                throw;
            }
        }

        private sealed class EnumerableExceptionInterceptor<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _inner;
            private readonly DbContext _context;
            private readonly LazyRef<ILogger> _logger;

            public EnumerableExceptionInterceptor(IEnumerable<T> inner, DbContext context, LazyRef<ILogger> logger)
            {
                _inner = inner;
                _context = context;
                _logger = logger;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new EnumeratorExceptionInterceptor(_inner.GetEnumerator(), _context, _logger);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            [DebuggerStepThrough]
            private sealed class EnumeratorExceptionInterceptor : IEnumerator<T>
            {
                private readonly IEnumerator<T> _inner;
                private readonly DbContext _context;
                private readonly LazyRef<ILogger> _logger;

                public EnumeratorExceptionInterceptor(IEnumerator<T> inner, DbContext context, LazyRef<ILogger> logger)
                {
                    _inner = inner;
                    _context = context;
                    _logger = logger;
                }

                public T Current
                {
                    get { return _inner.Current; }
                }

                object IEnumerator.Current
                {
                    get { return _inner.Current; }
                }

                public bool MoveNext()
                {
                    try
                    {
                        return _inner.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        _logger.Value.WriteError(
                            new DataStoreErrorLogState(_context.GetType()),
                            ex,
                            (state, exception) =>
                                Strings.LogExceptionDuringQueryIteration(Environment.NewLine, exception));

                        throw;
                    }
                }

                public void Reset()
                {
                    _inner.Reset();
                }

                public void Dispose()
                {
                    _inner.Dispose();
                }
            }
        }

        [DebuggerStepThrough]
        private sealed class AsyncEnumerableExceptionInterceptor<T> : IAsyncEnumerable<T>
        {
            private readonly IAsyncEnumerable<T> _inner;
            private readonly DbContext _context;
            private readonly LazyRef<ILogger> _logger;

            public AsyncEnumerableExceptionInterceptor(IAsyncEnumerable<T> inner, DbContext context, LazyRef<ILogger> logger)
            {
                _inner = inner;
                _context = context;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new AsyncEnumeratorExceptionInterceptor(_inner.GetEnumerator(), _context, _logger);
            }

            [DebuggerStepThrough]
            private sealed class AsyncEnumeratorExceptionInterceptor : IAsyncEnumerator<T>
            {
                private readonly IAsyncEnumerator<T> _inner;
                private readonly DbContext _context;
                private readonly LazyRef<ILogger> _logger;

                public AsyncEnumeratorExceptionInterceptor(IAsyncEnumerator<T> inner, DbContext context, LazyRef<ILogger> logger)
                {
                    _inner = inner;
                    _context = context;
                    _logger = logger;
                }

                public T Current
                {
                    get { return _inner.Current; }
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    try
                    {
                        return await _inner.MoveNext(cancellationToken).WithCurrentCulture();
                    }
                    catch (Exception ex)
                    {
                        _logger.Value.WriteError(
                            new DataStoreErrorLogState(_context.GetType()),
                            ex,
                            (state, exception) =>
                                Strings.LogExceptionDuringQueryIteration(Environment.NewLine, exception));

                        throw;
                    }
                }

                public void Dispose()
                {
                    _inner.Dispose();
                }
            }
        }
    }
}
