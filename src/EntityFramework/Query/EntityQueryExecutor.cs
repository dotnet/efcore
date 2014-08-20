// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Transformations;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryExecutor : IQueryExecutor
    {
        private readonly DbContext _context;
        private readonly LazyRef<ILogger> _logger;

        public EntityQueryExecutor([NotNull] DbContext context)
        {
            Check.NotNull(context, "context");

            _context = context;
            _logger = new LazyRef<ILogger>(() => (_context.Configuration.LoggerFactory.Create("EntityQueryExecutor")));
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

        public virtual IEnumerable<T> ExecuteCollection<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            new SubQueryFlattener().VisitQueryModel(queryModel);

            LogQueryModel(queryModel);

            return new EnumerableExceptionInterceptor<T>(
                _context.Configuration.DataStore.Query<T>(queryModel, _context.Configuration.StateManager),
                _context,
                _logger);
        }

        public virtual IAsyncEnumerable<T> AsyncExecuteCollection<T>(
            [NotNull] QueryModel queryModel, CancellationToken cancellationToken)
        {
            Check.NotNull(queryModel, "queryModel");

            new SubQueryFlattener().VisitQueryModel(queryModel);

            LogQueryModel(queryModel);

            return new AsyncEnumerableExceptionInterceptor<T>(
                _context.Configuration.DataStore.AsyncQuery<T>(
                    queryModel, _context.Configuration.StateManager, cancellationToken),
                _context,
                _logger);
        }

        private void LogQueryModel(QueryModel queryModel)
        {
            if (_logger.Value.IsEnabled(TraceType.Information))
            {
                _logger.Value.WriteInformation(queryModel + Environment.NewLine);
            }
        }

        private class SubQueryFlattener : SubQueryFromClauseFlattener
        {
            protected override void FlattenSubQuery(
                SubQueryExpression subQueryExpression,
                FromClauseBase fromClause,
                QueryModel queryModel,
                int destinationIndex)
            {
                var subQueryModel = subQueryExpression.QueryModel;

                if (!(subQueryModel.ResultOperators.Count <= 0
                      && !subQueryModel.BodyClauses.Any(bc => bc is OrderByClause)))
                {
                    return;
                }

                var innerMainFromClause
                    = subQueryExpression.QueryModel.MainFromClause;

                CopyFromClauseData(innerMainFromClause, fromClause);

                var innerSelectorMapping = new QuerySourceMapping();
                innerSelectorMapping.AddMapping(fromClause, subQueryExpression.QueryModel.SelectClause.Selector);

                queryModel.TransformExpressions(
                    ex => ReferenceReplacingExpressionTreeVisitor
                        .ReplaceClauseReferences(ex, innerSelectorMapping, false));

                InsertBodyClauses(subQueryExpression.QueryModel.BodyClauses, queryModel, destinationIndex);

                var innerBodyClauseMapping = new QuerySourceMapping();
                innerBodyClauseMapping.AddMapping(innerMainFromClause, new QuerySourceReferenceExpression(fromClause));

                queryModel.TransformExpressions(
                    ex => ReferenceReplacingExpressionTreeVisitor
                        .ReplaceClauseReferences(ex, innerBodyClauseMapping, false));
            }
        }

        private class EnumerableExceptionInterceptor<T> : IEnumerable<T>
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

            private class EnumeratorExceptionInterceptor : IEnumerator<T>
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
                        if (DataStoreException.ContainsDataStoreException(ex))
                        {
                            if (_logger.Value.IsEnabled(TraceType.Error))
                            {
                                _logger.Value.WriteError(Strings.FormatLogDataStoreExceptionRethrow(Strings.LogExceptionDuringQueryIteration), ex);
                            }

                            throw;
                        }

                        if (_logger.Value.IsEnabled(TraceType.Error))
                        {
                            _logger.Value.WriteError(Strings.FormatLogDataStoreExceptionWrap(Strings.LogExceptionDuringQueryIteration), ex);
                        }

                        throw new DataStoreException(Strings.DataStoreException, _context, ex);
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

        private class AsyncEnumerableExceptionInterceptor<T> : IAsyncEnumerable<T>
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

            private class AsyncEnumeratorExceptionInterceptor : IAsyncEnumerator<T>
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
                        return await _inner.MoveNext().ConfigureAwait(continueOnCapturedContext: false);
                    }
                    catch (Exception ex)
                    {
                        if (DataStoreException.ContainsDataStoreException(ex))
                        {
                            if (_logger.Value.IsEnabled(TraceType.Error))
                            {
                                _logger.Value.WriteError(Strings.FormatLogDataStoreExceptionRethrow(Strings.LogExceptionDuringQueryIteration), ex);
                            }

                            throw;
                        }

                        if (_logger.Value.IsEnabled(TraceType.Error))
                        {
                            _logger.Value.WriteError(Strings.FormatLogDataStoreExceptionWrap(Strings.LogExceptionDuringQueryIteration), ex);
                        }

                        throw new DataStoreException(Strings.DataStoreException, _context, ex);
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
