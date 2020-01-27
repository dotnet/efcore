// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class QueryAsserter<TContext> : QueryAsserterBase
        where TContext : DbContext
    {
        private readonly Func<TContext> _contextCreator;
        private readonly Dictionary<Type, object> _entitySorters;
        private readonly Dictionary<Type, object> _entityAsserters;
        private readonly IncludeQueryResultAsserter _includeResultAsserter;
        private readonly ExpectedQueryRewritingVisitor _expectedQueryRewritingVisitor;

        private const bool ProceduralQueryGeneration = false;

        public QueryAsserter(
            Func<TContext> contextCreator,
            ISetSource expectedData,
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters,
            ExpectedQueryRewritingVisitor expectedQueryRewritingVisitor = null)
        {
            _contextCreator = contextCreator;
            ExpectedData = expectedData;
            _expectedQueryRewritingVisitor = expectedQueryRewritingVisitor ?? new ExpectedQueryRewritingVisitor();
            _entitySorters = entitySorters ?? new Dictionary<Type, object>();
            _entityAsserters = entityAsserters ?? new Dictionary<Type, object>();

            SetSourceCreator = ctx => new DefaultSetSource(ctx);
            _includeResultAsserter = new IncludeQueryResultAsserter(_entitySorters, _entityAsserters);
        }

        protected virtual void AssertRogueExecution(int expectedCount, IQueryable queryable)
        {
        }

        public override async Task AssertSingleResultTyped<TResult>(
            Expression<Func<ISetSource, TResult>> actualSyncQuery,
            Expression<Func<ISetSource, Task<TResult>>> actualAsyncQuery,
            Expression<Func<ISetSource, TResult>> expectedQuery,
            Action<TResult, TResult> asserter,
            int entryCount,
            bool async,
            string testMethodName)
        {
            using var context = _contextCreator();
            TResult actual;

            if (async)
            {
                actual = await actualAsyncQuery.Compile()(SetSourceCreator(context));
            }
            else
            {
                actual = actualSyncQuery.Compile()(SetSourceCreator(context));
            }

            var rewrittenExpectedQueryExpression = (Expression<Func<ISetSource, TResult>>)_expectedQueryRewritingVisitor.Visit(expectedQuery);
            var expected = rewrittenExpectedQueryExpression.Compile()(ExpectedData);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        private IQueryable<TResult> GetExpectedResults<TResult>(Func<ISetSource, IQueryable<TResult>> expectedQueryFunc)
        {
            var expectedQuery = expectedQueryFunc(ExpectedData);
            var expectedQueryExpression = expectedQuery.Expression;
            var rewrittenExpectedQueryExpression = _expectedQueryRewritingVisitor.Visit(expectedQueryExpression);

            return expectedQuery.Provider.CreateQuery<TResult>(rewrittenExpectedQueryExpression);
        }

        public override async Task AssertQuery<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Func<TResult, object> elementSorter,
            Action<TResult, TResult> elementAsserter,
            bool assertOrder,
            int entryCount,
            bool async,
            string testMethodName)
        {
            using var context = _contextCreator();
            var query = actualQuery(SetSourceCreator(context));
            if (ProceduralQueryGeneration && !async)
            {
                new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                return;
            }

            OrderingSettingsVerifier(assertOrder, query.Expression.Type, elementSorter);

            var actual = async
                ? await query.ToListAsync()
                : query.ToList();

            AssertRogueExecution(actual.Count, query);

            var expected = GetExpectedResults(expectedQuery).ToList();

            if (!assertOrder
                && elementSorter == null)
            {
                _entitySorters.TryGetValue(typeof(TResult), out var sorter);
                elementSorter = (Func<TResult, object>)sorter;
            }

            if (elementAsserter == null)
            {
                _entityAsserters.TryGetValue(typeof(TResult), out var asserter);
                elementAsserter = (Action<TResult, TResult>)asserter;
            }

            TestHelpers.AssertResults(
                expected,
                actual,
                elementSorter,
                elementAsserter,
                assertOrder);

            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        private void OrderingSettingsVerifier(bool assertOrder, Type type)
            => OrderingSettingsVerifier(assertOrder, type, elementSorter: null);

        private void OrderingSettingsVerifier(bool assertOrder, Type type, object elementSorter)
        {
            if (!assertOrder
                && type.IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>)
                    || type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>)))
            {
                throw new InvalidOperationException(
                    "Query result is OrderedQueryable - you need to set AssertQuery option: 'assertOrder' to 'true'. If the resulting order is non-deterministic by design, add identity projection to the top of the query to disable this check.");
            }

            if (assertOrder && elementSorter != null)
            {
                throw new InvalidOperationException("You shouldn't apply element sorter when 'assertOrder' is set to 'true'.");
            }
        }

        public override async Task AssertQueryScalar<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool async,
            string testMethodName)
            where TResult : struct
        {
            using var context = _contextCreator();
            var query = actualQuery(SetSourceCreator(context));
            if (ProceduralQueryGeneration && !async)
            {
                new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                return;
            }

            OrderingSettingsVerifier(assertOrder, query.Expression.Type);

            var actual = async
                ? await query.ToListAsync()
                : query.ToList();

            AssertRogueExecution(actual.Count, query);

            var expected = GetExpectedResults(expectedQuery).ToList();

            TestHelpers.AssertResults(
                expected,
                actual,
                e => e,
                Assert.Equal,
                assertOrder);
        }

        public override async Task AssertQueryScalar<TResult>(
            Func<ISetSource, IQueryable<TResult?>> actualQuery,
            Func<ISetSource, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool async,
            string testMethodName)
            where TResult : struct
        {
            using var context = _contextCreator();
            var query = actualQuery(SetSourceCreator(context));
            if (ProceduralQueryGeneration && !async)
            {
                new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                return;
            }

            OrderingSettingsVerifier(assertOrder, query.Expression.Type);

            var actual = async
                ? await query.ToListAsync()
                : query.ToList();

            AssertRogueExecution(actual.Count, query);

            var expected = GetExpectedResults(expectedQuery).ToList();

            TestHelpers.AssertResults(
                expected,
                actual,
                e => e,
                Assert.Equal,
                assertOrder);
        }

        public override async Task<List<TResult>> AssertIncludeQuery<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<TResult, object> elementSorter,
            List<Func<TResult, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool async,
            string testMethodName)
        {
            using var context = _contextCreator();
            var query = actualQuery(SetSourceCreator(context));
            if (ProceduralQueryGeneration && !async)
            {
                new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                return default;
            }

            OrderingSettingsVerifier(assertOrder, query.Expression.Type, elementSorter);

            var actual = async
                ? await query.ToListAsync()
                : query.ToList();

            AssertRogueExecution(actual.Count, query);

            var expected = GetExpectedResults(expectedQuery).ToList();

            if (!assertOrder
                && elementSorter == null)
            {
                _entitySorters.TryGetValue(typeof(TResult), out var sorter);
                elementSorter = (Func<TResult, object>)sorter;
            }

            if (elementSorter != null)
            {
                actual = actual.OrderBy(elementSorter).ToList();
                expected = expected.OrderBy(elementSorter).ToList();
            }

            if (clientProjections != null)
            {
                foreach (var clientProjection in clientProjections)
                {
                    var projectedActual = actual.Select(clientProjection).ToList();
                    var projectedExpected = expected.Select(clientProjection).ToList();

                    _includeResultAsserter.AssertResult(projectedExpected, projectedActual, expectedIncludes);
                }
            }
            else
            {
                _includeResultAsserter.AssertResult(expected, actual, expectedIncludes);
            }

            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());

            return actual;
        }

        #region Assert termination operation methods

        public override async Task AssertAny<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AnyAsync()
                : actualQuery(SetSourceCreator(context)).Any();

            var expected = GetExpectedResults(expectedQuery).Any();

            Assert.Equal(expected, actual);
        }

        public override async Task AssertAny<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using (var context = _contextCreator())
            {
                var actual = async
                    ? await actualQuery(SetSourceCreator(context)).AnyAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).Any(actualPredicate);

                var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
                var expected = GetExpectedResults(expectedQuery).Any(rewrittenExpectedPredicate);

                Assert.Equal(expected, actual);
            }

            using (var context = _contextCreator())
            {
                var actual = async
                    ? await actualQuery(SetSourceCreator(context)).AnyAsync()
                    : actualQuery(SetSourceCreator(context)).Any();

                var expected = expectedQuery(ExpectedData).Any();

                Assert.Equal(expected, actual);
            }
        }

        public override async Task AssertAll<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AllAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).All(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).All(rewrittenExpectedPredicate);

            Assert.Equal(expected, actual);
        }

        public override async Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).FirstAsync()
                : actualQuery(SetSourceCreator(context)).First();

            var expected = GetExpectedResults(expectedQuery).First();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).FirstAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).First(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).First(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).FirstOrDefaultAsync()
                : actualQuery(SetSourceCreator(context)).FirstOrDefault();

            var expected = GetExpectedResults(expectedQuery).FirstOrDefault();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).FirstOrDefaultAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).FirstOrDefault(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).FirstOrDefault(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SingleAsync()
                : actualQuery(SetSourceCreator(context)).Single();

            var expected = GetExpectedResults(expectedQuery).Single();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SingleAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).Single(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).Single(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SingleOrDefaultAsync()
                : actualQuery(SetSourceCreator(context)).SingleOrDefault();

            var expected = GetExpectedResults(expectedQuery).SingleOrDefault();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SingleOrDefaultAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).SingleOrDefault(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).SingleOrDefault(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).LastAsync()
                : actualQuery(SetSourceCreator(context)).Last();

            var expected = GetExpectedResults(expectedQuery).Last();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).LastAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).Last(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).Last(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).LastOrDefaultAsync()
                : actualQuery(SetSourceCreator(context)).LastOrDefault();

            var expected = GetExpectedResults(expectedQuery).LastOrDefault();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).LastOrDefaultAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).LastOrDefault(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).LastOrDefault(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).CountAsync()
                : actualQuery(SetSourceCreator(context)).Count();

            var expected = GetExpectedResults(expectedQuery).Count();

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).CountAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).Count(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).Count(rewrittenExpectedPredicate);

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).LongCountAsync()
                : actualQuery(SetSourceCreator(context)).LongCount();

            var expected = GetExpectedResults(expectedQuery).LongCount();

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).LongCountAsync(actualPredicate)
                : actualQuery(SetSourceCreator(context)).LongCount(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = GetExpectedResults(expectedQuery).LongCount(rewrittenExpectedPredicate);

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertMin<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).MinAsync()
                : actualQuery(SetSourceCreator(context)).Min();

            var expected = GetExpectedResults(expectedQuery).Min();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertMin<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<TSelector, TSelector> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).MinAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Min(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, TSelector>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Min(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertMax<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).MaxAsync()
                : actualQuery(SetSourceCreator(context)).Max();

            var expected = GetExpectedResults(expectedQuery).Max();

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertMax<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<TSelector, TSelector> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).MaxAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Max(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, TSelector>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Max(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<int, int> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<int?, int?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<long, long> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<long?>> actualQuery,
            Func<ISetSource, IQueryable<long?>> expectedQuery,
            Action<long?, long?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<decimal>> actualQuery,
            Func<ISetSource, IQueryable<decimal>> expectedQuery,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<decimal?>> actualQuery,
            Func<ISetSource, IQueryable<decimal?>> expectedQuery,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<float>> actualQuery,
            Func<ISetSource, IQueryable<float>> expectedQuery,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<float?>> actualQuery,
            Func<ISetSource, IQueryable<float?>> expectedQuery,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<double>> actualQuery,
            Func<ISetSource, IQueryable<double>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<double?>> actualQuery,
            Func<ISetSource, IQueryable<double?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync()
                : actualQuery(SetSourceCreator(context)).Sum();

            var expected = GetExpectedResults(expectedQuery).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<int, int> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<int?, int?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<long, long> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<long?, long?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, decimal>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal?>> actualSelector,
            Expression<Func<TResult, decimal?>> expectedSelector,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, decimal?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float?>> actualSelector,
            Expression<Func<TResult, float?>> expectedSelector,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double>> actualSelector,
            Expression<Func<TResult, double>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double?>> actualSelector,
            Expression<Func<TResult, double?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<long?>> actualQuery,
            Func<ISetSource, IQueryable<long?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<decimal>> actualQuery,
            Func<ISetSource, IQueryable<decimal>> expectedQuery,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<decimal?>> actualQuery,
            Func<ISetSource, IQueryable<decimal?>> expectedQuery,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<float>> actualQuery,
            Func<ISetSource, IQueryable<float>> expectedQuery,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<float?>> actualQuery,
            Func<ISetSource, IQueryable<float?>> expectedQuery,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<double>> actualQuery,
            Func<ISetSource, IQueryable<double>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<double?>> actualQuery,
            Func<ISetSource, IQueryable<double?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                : actualQuery(SetSourceCreator(context)).Average();

            var expected = GetExpectedResults(expectedQuery).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, decimal>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal?>> actualSelector,
            Expression<Func<TResult, decimal?>> expectedSelector,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, decimal?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float?>> actualSelector,
            Expression<Func<TResult, float?>> expectedSelector,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double>> actualSelector,
            Expression<Func<TResult, double>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double?>> actualSelector,
            Expression<Func<TResult, double?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                : actualQuery(SetSourceCreator(context)).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = GetExpectedResults(expectedQuery).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public override async Task AssertContains<TElement>(
            Func<ISetSource, IQueryable<TElement>> actualQuery,
            Func<ISetSource, IQueryable<TElement>> expectedQuery,
            TElement actualElement,
            TElement expectedElement,
            Action<bool, bool> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualQuery(SetSourceCreator(context)).ContainsAsync(actualElement)
                : actualQuery(SetSourceCreator(context)).Contains(actualElement);

            var expected = expectedQuery(ExpectedData).Contains(expectedElement);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        #endregion

        #region Helpers

        public override void AssertEqual<T>(T expected, T actual, Action<T, T> asserter = null)
        {
            if (asserter == null
                && expected != null)
            {
                _entityAsserters.TryGetValue(typeof(T), out var entityAsserter);
                asserter ??= (Action<T, T>)entityAsserter;
            }

            asserter ??= Assert.Equal;
            asserter(expected, actual);
        }

        public override void AssertEqual<T>(T? expected, T? actual, Action<T?, T?> asserter = null)
            where T : struct
        {
            asserter ??= Assert.Equal;

            asserter(expected, actual);
        }

        public override void AssertCollection<TElement>(
            IEnumerable<TElement> expected,
            IEnumerable<TElement> actual,
            bool ordered = false,
            Func<TElement, object> elementSorter = null,
            Action<TElement, TElement> elementAsserter = null)
        {
            if (expected == null
                && actual == null)
            {
                return;
            }

            if (expected == null != (actual == null))
            {
                throw new InvalidOperationException(
                    $"Nullability doesn't match. Expected: {(expected == null ? "NULL" : "NOT NULL")}. Actual: {(actual == null ? "NULL." : "NOT NULL.")}.");
            }

            _entitySorters.TryGetValue(typeof(TElement), out var sorter);
            _entityAsserters.TryGetValue(typeof(TElement), out var asserter);

            elementSorter ??= (Func<TElement, object>)sorter;
            elementAsserter ??= (Action<TElement, TElement>)asserter ?? Assert.Equal;

            if (!ordered)
            {
                if (elementSorter != null)
                {
                    var sortedActual = actual.OrderBy(elementSorter).ToList();
                    var sortedExpected = expected.OrderBy(elementSorter).ToList();

                    Assert.Equal(sortedExpected.Count, sortedActual.Count);
                    for (var i = 0; i < sortedExpected.Count; i++)
                    {
                        elementAsserter(sortedExpected[i], sortedActual[i]);
                    }
                }
                else
                {
                    var sortedActual = actual.OrderBy(e => e).ToList();
                    var sortedExpected = expected.OrderBy(e => e).ToList();

                    Assert.Equal(sortedExpected.Count, sortedActual.Count);
                    for (var i = 0; i < sortedExpected.Count; i++)
                    {
                        elementAsserter(sortedExpected[i], sortedActual[i]);
                    }
                }
            }
            else
            {
                var expectedList = expected.ToList();
                var actualList = actual.ToList();

                Assert.Equal(expectedList.Count, actualList.Count);
                for (var i = 0; i < expectedList.Count; i++)
                {
                    elementAsserter(expectedList[i], actualList[i]);
                }
            }
        }

        #endregion

        private class DefaultSetSource : ISetSource
        {
            private readonly DbContext _context;

            public DefaultSetSource(DbContext context)
            {
                _context = context;
            }

            public IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
                => _context.Set<TEntity>();
        }
    }
}
