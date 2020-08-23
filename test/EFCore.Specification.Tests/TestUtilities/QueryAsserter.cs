// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class QueryAsserter
    {
        private static readonly MethodInfo _assertIncludeEntity =
            typeof(QueryAsserter).GetTypeInfo().GetDeclaredMethod(nameof(AssertIncludeEntity));

        private static readonly MethodInfo _assertIncludeCollectionMethodInfo =
            typeof(QueryAsserter).GetTypeInfo().GetDeclaredMethod(nameof(AssertIncludeCollection));

        private static readonly MethodInfo _filteredIncludeMethodInfo =
            typeof(QueryAsserter).GetTypeInfo().GetDeclaredMethod(nameof(FilteredInclude));

        private readonly Func<DbContext> _contextCreator;
        private readonly IReadOnlyDictionary<Type, object> _entitySorters;
        private readonly IReadOnlyDictionary<Type, object> _entityAsserters;
        private readonly Func<Expression, Expression> _rewriteExpectedQueryExpression;
        private readonly Func<Expression, Expression> _rewriteServerQueryExpression;
        private readonly bool _ignoreEntryCount;
        private const bool ProceduralQueryGeneration = false;
        private readonly List<string> _includePath = new List<string>();

        public QueryAsserter(
            IQueryFixtureBase queryFixture,
            Func<Expression, Expression> rewriteExpectedQueryExpression,
            Func<Expression, Expression> rewriteServerQueryExpression,
            bool ignoreEntryCount = false)
        {
            _contextCreator = queryFixture.GetContextCreator();
            ExpectedData = queryFixture.GetExpectedData();
            _entitySorters = queryFixture.GetEntitySorters() ?? new Dictionary<Type, object>();
            _entityAsserters = queryFixture.GetEntityAsserters() ?? new Dictionary<Type, object>();
            SetSourceCreator = queryFixture.GetSetSourceCreator();

            _rewriteExpectedQueryExpression = rewriteExpectedQueryExpression;
            _rewriteServerQueryExpression = rewriteServerQueryExpression;
            _ignoreEntryCount = ignoreEntryCount;
        }

        public virtual Func<DbContext, ISetSource> SetSourceCreator { get; }

        public virtual ISetSource ExpectedData { get; }

        protected virtual void AssertRogueExecution(int expectedCount, IQueryable queryable)
        {
        }

        public async Task AssertSingleResult<TResult>(
            Expression<Func<ISetSource, TResult>> actualSyncQuery,
            Expression<Func<ISetSource, Task<TResult>>> actualAsyncQuery,
            Expression<Func<ISetSource, TResult>> expectedQuery,
            Action<TResult, TResult> asserter,
            int entryCount,
            bool async)
        {
            using var context = _contextCreator();
            var actual = async
                ? await actualAsyncQuery.Compile()(SetSourceCreator(context))
                : actualSyncQuery.Compile()(SetSourceCreator(context));

            var rewrittenExpectedQueryExpression = (Expression<Func<ISetSource, TResult>>)_rewriteExpectedQueryExpression(expectedQuery);
            var expected = rewrittenExpectedQueryExpression.Compile()(ExpectedData);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertQuery<TResult>(
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
            var query = RewriteServerQuery(actualQuery(SetSourceCreator(context)));
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

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).ToList();

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

            AssertEntryCount(context, entryCount);
        }

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

        public async Task AssertQueryScalar<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool async,
            string testMethodName)
            where TResult : struct
        {
            using var context = _contextCreator();
            var query = RewriteServerQuery(actualQuery(SetSourceCreator(context)));
            if (ProceduralQueryGeneration && !async)
            {
                new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                return;
            }

            OrderingSettingsVerifier(assertOrder, query.Expression.Type, elementSorter: null);

            var actual = async
                ? await query.ToListAsync()
                : query.ToList();

            AssertRogueExecution(actual.Count, query);

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).ToList();

            TestHelpers.AssertResults(
                expected,
                actual,
                e => e,
                Assert.Equal,
                assertOrder);
        }

        public async Task AssertQueryScalar<TResult>(
            Func<ISetSource, IQueryable<TResult?>> actualQuery,
            Func<ISetSource, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool async,
            string testMethodName)
            where TResult : struct
        {
            using var context = _contextCreator();
            var query = RewriteServerQuery(actualQuery(SetSourceCreator(context)));
            if (ProceduralQueryGeneration && !async)
            {
                new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                return;
            }

            OrderingSettingsVerifier(assertOrder, query.Expression.Type, elementSorter: null);

            var actual = async
                ? await query.ToListAsync()
                : query.ToList();

            AssertRogueExecution(actual.Count, query);

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).ToList();

            TestHelpers.AssertResults(
                expected,
                actual,
                e => e,
                Assert.Equal,
                assertOrder);
        }

        #region Assert termination operation methods

        public async Task AssertAny<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AnyAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Any();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Any();

            Assert.Equal(expected, actual);
        }

        public async Task AssertAny<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AnyAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Any(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Any(rewrittenExpectedPredicate);

            Assert.Equal(expected, actual);
        }

        public async Task AssertAll<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AllAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).All(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).All(rewrittenExpectedPredicate);

            Assert.Equal(expected, actual);
        }

        public async Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).FirstAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).First();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).First();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertFirst<TResult>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).FirstAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).First(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).First(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).FirstOrDefaultAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).FirstOrDefault();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).FirstOrDefault();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertFirstOrDefault<TResult>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).FirstOrDefaultAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).FirstOrDefault(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).FirstOrDefault(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SingleAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Single();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Single();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertSingle<TResult>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SingleAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Single(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Single(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SingleOrDefaultAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).SingleOrDefault();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).SingleOrDefault();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertSingleOrDefault<TResult>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SingleOrDefaultAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).SingleOrDefault(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).SingleOrDefault(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).LastAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Last();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Last();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertLast<TResult>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).LastAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Last(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Last(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).LastOrDefaultAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).LastOrDefault();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).LastOrDefault();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertLastOrDefault<TResult>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).LastOrDefaultAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).LastOrDefault(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).LastOrDefault(rewrittenExpectedPredicate);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).CountAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Count();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Count();

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).CountAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Count(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Count(rewrittenExpectedPredicate);

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).LongCountAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).LongCount();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).LongCount();

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).LongCountAsync(actualPredicate)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).LongCount(actualPredicate);

            var rewrittenExpectedPredicate = (Expression<Func<TResult, bool>>)new ExpectedQueryRewritingVisitor().Visit(expectedPredicate);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).LongCount(rewrittenExpectedPredicate);

            Assert.Equal(expected, actual);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertMin<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).MinAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Min();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Min();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertMin<TResult, TSelector>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).MinAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Min(actualSelector);

            var rewrittenExpectedSelector =
                (Expression<Func<TResult, TSelector>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Min(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertMax<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).MaxAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Max();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Max();

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertMax<TResult, TSelector>(
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
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).MaxAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Max(actualSelector);

            var rewrittenExpectedSelector =
                (Expression<Func<TResult, TSelector>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Max(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            AssertEntryCount(context, entryCount);
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<int, int> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<int?, int?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<long, long> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<long?>> actualQuery,
            Func<ISetSource, IQueryable<long?>> expectedQuery,
            Action<long?, long?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<decimal>> actualQuery,
            Func<ISetSource, IQueryable<decimal>> expectedQuery,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<decimal?>> actualQuery,
            Func<ISetSource, IQueryable<decimal?>> expectedQuery,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<float>> actualQuery,
            Func<ISetSource, IQueryable<float>> expectedQuery,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<float?>> actualQuery,
            Func<ISetSource, IQueryable<float?>> expectedQuery,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<double>> actualQuery,
            Func<ISetSource, IQueryable<double>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum(
            Func<ISetSource, IQueryable<double?>> actualQuery,
            Func<ISetSource, IQueryable<double?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<int, int> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<int?, int?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<long, long> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<long?, long?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, decimal>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal?>> actualSelector,
            Expression<Func<TResult, decimal?>> expectedSelector,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector =
                (Expression<Func<TResult, decimal?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float?>> actualSelector,
            Expression<Func<TResult, float?>> expectedSelector,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double>> actualSelector,
            Expression<Func<TResult, double>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double?>> actualSelector,
            Expression<Func<TResult, double?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).SumAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Sum(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Sum(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<long?>> actualQuery,
            Func<ISetSource, IQueryable<long?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<decimal>> actualQuery,
            Func<ISetSource, IQueryable<decimal>> expectedQuery,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<decimal?>> actualQuery,
            Func<ISetSource, IQueryable<decimal?>> expectedQuery,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<float>> actualQuery,
            Func<ISetSource, IQueryable<float>> expectedQuery,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<float?>> actualQuery,
            Func<ISetSource, IQueryable<float?>> expectedQuery,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<double>> actualQuery,
            Func<ISetSource, IQueryable<double>> expectedQuery,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage(
            Func<ISetSource, IQueryable<double?>> actualQuery,
            Func<ISetSource, IQueryable<double?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync()
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average();

            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average();

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, int?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, long?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<decimal, decimal> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, decimal>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal?>> actualSelector,
            Expression<Func<TResult, decimal?>> expectedSelector,
            Action<decimal?, decimal?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector =
                (Expression<Func<TResult, decimal?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<float, float> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float?>> actualSelector,
            Expression<Func<TResult, float?>> expectedSelector,
            Action<float?, float?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, float?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double>> actualSelector,
            Expression<Func<TResult, double>> expectedSelector,
            Action<double, double> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        public async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double?>> actualSelector,
            Expression<Func<TResult, double?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool async = false)
        {
            using var context = _contextCreator();
            var actual = async
                ? await RewriteServerQuery(actualQuery(SetSourceCreator(context))).AverageAsync(actualSelector)
                : RewriteServerQuery(actualQuery(SetSourceCreator(context))).Average(actualSelector);

            var rewrittenExpectedSelector = (Expression<Func<TResult, double?>>)new ExpectedQueryRewritingVisitor().Visit(expectedSelector);
            var expected = RewriteExpectedQuery(expectedQuery(ExpectedData)).Average(rewrittenExpectedSelector);

            AssertEqual(expected, actual, asserter);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        #endregion

        #region Helpers

        public void AssertEqual<T>(T expected, T actual, Action<T, T> asserter = null)
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

        public void AssertEqual<T>(T? expected, T? actual, Action<T?, T?> asserter = null)
            where T : struct
        {
            asserter ??= Assert.Equal;

            asserter(expected, actual);
        }

        public void AssertCollection<TElement>(
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

        public void AssertInclude<TEntity>(TEntity expected, TEntity actual, IExpectedInclude[] expectedIncludes)
        {
            _includePath.Clear();

            AssertIncludeObject(expected, actual, expectedIncludes, assertOrder: false);
        }

        private void AssertIncludeObject(object expected, object actual, IEnumerable<IExpectedInclude> expectedIncludes, bool assertOrder)
        {
            if (expected == null
                && actual == null)
            {
                return;
            }

            Assert.Equal(expected == null, actual == null);

            var expectedType = expected.GetType();
            if (expectedType.IsGenericType
                && expectedType.GetTypeInfo().ImplementedInterfaces.Any(
                    i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                _assertIncludeCollectionMethodInfo.MakeGenericMethod(expectedType.GenericTypeArguments[0])
                    .Invoke(this, new[] { expected, actual, expectedIncludes, assertOrder });
            }
            else
            {
                _assertIncludeEntity.MakeGenericMethod(expectedType).Invoke(this, new[] { expected, actual, expectedIncludes });
            }
        }

        private void AssertIncludeEntity<TElement>(TElement expected, TElement actual, IEnumerable<IExpectedInclude> expectedIncludes)
        {
            Assert.Equal(expected.GetType(), actual.GetType());

            if (_entityAsserters.TryGetValue(typeof(TElement), out var asserter))
            {
                ((Action<TElement, TElement>)asserter)(expected, actual);
                ProcessIncludes(expected, actual, expectedIncludes);
            }
        }

        private void AssertIncludeCollection<TElement>(
            IEnumerable<TElement> expected,
            IEnumerable<TElement> actual,
            IEnumerable<IExpectedInclude> expectedIncludes,
            bool assertOrder)
        {
            var expectedList = expected.ToList();
            var actualList = actual.ToList();

            if (!assertOrder && _entitySorters.TryGetValue(typeof(TElement), out var sorter))
            {
                var actualSorter = (Func<TElement, object>)sorter;
                expectedList = expectedList.OrderBy(actualSorter).ToList();
                actualList = actualList.OrderBy(actualSorter).ToList();
            }

            Assert.Equal(expectedList.Count, actualList.Count);

            for (var i = 0; i < expectedList.Count; i++)
            {
                var elementType = expectedList[i].GetType();
                _assertIncludeEntity.MakeGenericMethod(elementType)
                    .Invoke(this, new object[] { expectedList[i], actualList[i], expectedIncludes });
            }
        }

        private void ProcessIncludes<TEntity>(TEntity expected, TEntity actual, IEnumerable<IExpectedInclude> expectedIncludes)
        {
            var currentPath = string.Join(".", _includePath);

            foreach (var expectedInclude in expectedIncludes.OfType<ExpectedInclude<TEntity>>().Where(i => i.NavigationPath == currentPath))
            {
                var expectedIncludedNavigation = GetIncluded(expected, expectedInclude.IncludeMember);
                var assertOrder = false;
                if (expectedInclude.GetType().BaseType != typeof(object))
                {
                    var includedType = expectedInclude.GetType().GetGenericArguments()[1];
                    var filterTypedMethod = _filteredIncludeMethodInfo.MakeGenericMethod(typeof(TEntity), includedType);
                    expectedIncludedNavigation = filterTypedMethod.Invoke(
                        this,
                        BindingFlags.NonPublic,
                        null,
                        new[] { expectedIncludedNavigation, expectedInclude },
                        CultureInfo.CurrentCulture);

                    assertOrder = (bool)expectedInclude.GetType()
                        .GetProperty(nameof(ExpectedFilteredInclude<object, object>.AssertOrder))
                        .GetValue(expectedInclude);
                }

                var actualIncludedNavigation = GetIncluded(actual, expectedInclude.IncludeMember);

                _includePath.Add(expectedInclude.IncludeMember.Name);

                AssertIncludeObject(expectedIncludedNavigation, actualIncludedNavigation, expectedIncludes, assertOrder);

                _includePath.RemoveAt(_includePath.Count - 1);
            }
        }

        private IEnumerable<TIncluded> FilteredInclude<TEntity, TIncluded>(
            IEnumerable<TIncluded> expected,
            ExpectedFilteredInclude<TEntity, TIncluded> expectedFilteredInclude)
            => expectedFilteredInclude.IncludeFilter(expected);

        private object GetIncluded<TEntity>(TEntity entity, MemberInfo includeMember)
            => includeMember switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(entity),
                PropertyInfo propertyInfo => propertyInfo.GetValue(entity),
                _ => throw new InvalidOperationException(),
            };

        private void AssertEntryCount(DbContext context, int entryCount)
        {
            if (!_ignoreEntryCount)
            {
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private IQueryable<T> RewriteServerQuery<T>(IQueryable<T> query)
            => query.Provider.CreateQuery<T>(_rewriteServerQueryExpression(query.Expression));

        private IQueryable<T> RewriteExpectedQuery<T>(IQueryable<T> query)
            => query.Provider.CreateQuery<T>(_rewriteExpectedQueryExpression(query.Expression));

        #endregion
    }
}
