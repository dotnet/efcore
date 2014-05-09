// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Query;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class QueryableExtensionsTest
    {
        // TODO: Uncomment asserts below when more extension methods come online

        [Fact]
        public void Async_extension_methods_throw_OperatationCanceledException_if_task_is_cancelled()
        {
            var source = CreateThrowingMockQueryable<int>();

//            Assert.Throws<OperationCanceledException>(
//                () => source.FirstAsync(new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.FirstAsync(n => true, new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.FirstOrDefaultAsync(new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.FirstOrDefaultAsync(n => true, new CancellationToken(canceled: true)).Wait());

            Assert.Throws<OperationCanceledException>(
                () => source.SingleAsync(new CancellationToken(canceled: true)).Wait());

//            Assert.Throws<OperationCanceledException>(
//                () => source.SingleAsync(n => true, new CancellationToken(canceled: true)).Wait());

//            Assert.Throws<OperationCanceledException>(
//                () => source.SingleOrDefaultAsync(new CancellationToken(canceled: true)).Wait());

//            Assert.Throws<OperationCanceledException>(
//                () => source.SingleOrDefaultAsync(n => true, new CancellationToken(canceled: true)).Wait());

//            Assert.Throws<OperationCanceledException>(
//                () => source.ContainsAsync(42, new CancellationToken(canceled: true)).Wait());

            Assert.Throws<OperationCanceledException>(
                () => source.AnyAsync(new CancellationToken(canceled: true)).Wait());

//            Assert.Throws<OperationCanceledException>(
//                () => source.AnyAsync(n => true, new CancellationToken(canceled: true)).Wait());

//            Assert.Throws<OperationCanceledException>(
//                () => source.AllAsync(n => true, new CancellationToken(canceled: true)).Wait());

            Assert.Throws<OperationCanceledException>(
                () => source.CountAsync(new CancellationToken(canceled: true)).Wait());

//            Assert.Throws<OperationCanceledException>(
//                () => source.CountAsync(n => true, new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.LongCountAsync(new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.LongCountAsync(n => true, new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.MinAsync(new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.MinAsync(n => true, new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.MaxAsync(new CancellationToken(canceled: true)).Wait());
//
//            Assert.Throws<OperationCanceledException>(
//                () => source.MaxAsync(n => true, new CancellationToken(canceled: true)).Wait());
        }

        [Fact]
        public void Extension_methods_call_provider_ExecuteAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
//            VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true));
//            VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, bool>(value => value.AnyAsync(default(CancellationToken)));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, default(CancellationToken)));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, cancellationTokenSource.Token));

//            VerifyProducedExpression<int, double>(value => value.AverageAsync());
//            VerifyProducedExpression<int, double>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<int?, double?>(value => value.AverageAsync());
//            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<long, double>(value => value.AverageAsync());
//            VerifyProducedExpression<long, double>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<long?, double?>(value => value.AverageAsync());
//            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<float, float>(value => value.AverageAsync());
//            VerifyProducedExpression<float, float>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<float?, float?>(value => value.AverageAsync());
//            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<double, double>(value => value.AverageAsync());
//            VerifyProducedExpression<double, double>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<double?, double?>(value => value.AverageAsync());
//            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync());
//            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync());
//            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e));
//            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));

//            VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0));
//            VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.CountAsync(default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.CountAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, int>(value => value.CountAsync(e => true));
//            VerifyProducedExpression<int, int>(value => value.CountAsync(e => true, cancellationTokenSource.Token));

//            VerifyProducedExpression<int, int>(value => value.FirstAsync());
//            VerifyProducedExpression<int, int>(value => value.FirstAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true));
//            VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true, cancellationTokenSource.Token));
//
//            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync());
//            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true));
//            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true, cancellationTokenSource.Token));
//
//            VerifyProducedExpression<int, long>(value => value.LongCountAsync());
//            VerifyProducedExpression<int, long>(value => value.LongCountAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true));
//            VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true, cancellationTokenSource.Token));
//
//            VerifyProducedExpression<int, int>(value => value.MaxAsync());
//            VerifyProducedExpression<int, int>(value => value.MaxAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e));
//            VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e, cancellationTokenSource.Token));
//
//            VerifyProducedExpression<int, int>(value => value.MinAsync());
//            VerifyProducedExpression<int, int>(value => value.MinAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, int>(value => value.MinAsync(e => e));
//            VerifyProducedExpression<int, int>(value => value.MinAsync(e => e, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SingleAsync(default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SumAsync(default(CancellationToken)));
//            VerifyProducedExpression<int, int>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int, int>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<int, int>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<int?, int?>(value => value.SumAsync());
//            VerifyProducedExpression<int?, int?>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<long, long>(value => value.SumAsync());
//            VerifyProducedExpression<long, long>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<long, long>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<long, long>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<long?, long?>(value => value.SumAsync());
//            VerifyProducedExpression<long?, long?>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<float, float>(value => value.SumAsync());
//            VerifyProducedExpression<float, float>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<float, float>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<float, float>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<float?, float?>(value => value.SumAsync());
//            VerifyProducedExpression<float?, float?>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<double, double>(value => value.SumAsync());
//            VerifyProducedExpression<double, double>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<double, double>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<double, double>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<double?, double?>(value => value.SumAsync());
//            VerifyProducedExpression<double?, double?>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(default(CancellationToken)));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
//            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync());
//            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(cancellationTokenSource.Token));
//            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e));
//            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        }

        private static IQueryable<T> CreateThrowingMockQueryable<T>()
        {
            var mockSource = new Mock<IQueryable<T>>();
            mockSource
                .Setup(s => s.Provider)
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            return mockSource.Object;
        }

        private static void VerifyProducedExpression<TElement, TResult>(
            Expression<Func<IQueryable<TElement>, Task<TResult>>> testExpression)
        {
            var queryableMock = new Mock<IQueryable<TElement>>();
            var providerMock = new Mock<IAsyncQueryProvider>();

            providerMock
                .Setup(m => m.ExecuteAsync<TResult>(It.IsAny<Expression>(), It.IsAny<CancellationToken>()))
                .Returns<Expression, CancellationToken>(
                    (e, ct) =>
                        {
                            var expectedMethodCall = (MethodCallExpression)testExpression.Body;
                            var actualMethodCall = (MethodCallExpression)e;

                            Assert.Equal(
                                expectedMethodCall.Method.Name,
                                actualMethodCall.Method.Name + "Async");

                            var lastArgument =
                                expectedMethodCall.Arguments[expectedMethodCall.Arguments.Count - 1] as MemberExpression;

                            var cancellationTokenPresent
                                = lastArgument != null && lastArgument.Type == typeof(CancellationToken);

                            if (cancellationTokenPresent)
                            {
                                Assert.NotEqual(ct, CancellationToken.None);
                            }
                            else
                            {
                                Assert.Equal(ct, CancellationToken.None);
                            }

                            for (var i = 1; i < expectedMethodCall.Arguments.Count - 1; i++)
                            {
                                var expectedArgument = expectedMethodCall.Arguments[i];
                                var actualArgument = actualMethodCall.Arguments[i];

                                Assert.Equal(expectedArgument.ToString(), actualArgument.ToString());
                            }

                            return Task.FromResult(default(TResult));
                        });

            queryableMock
                .Setup(m => m.Provider)
                .Returns(providerMock.Object);

            queryableMock
                .Setup(m => m.Expression)
                .Returns(Expression.Constant(queryableMock.Object, typeof(IQueryable<TElement>)));

            testExpression.Compile()(queryableMock.Object);
        }

        [Fact]
        public void Extension_methods_throw_on_non_async_source()
        {
//            SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true));
//            SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().AnyAsync());
//            SourceNonAsyncQueryableTest(() => Source().AnyAsync(e => true));
//
//            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e, new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync());
//            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e, new CancellationToken()));
//
//            SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0));
//            SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().CountAsync());
//            SourceNonAsyncQueryableTest(() => Source().CountAsync(e => true));

//            SourceNonAsyncQueryableTest(() => Source().FirstAsync());
//            SourceNonAsyncQueryableTest(() => Source().FirstAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true));
//            SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true, new CancellationToken()));
//
//            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync());
//            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true));
//            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true, new CancellationToken()));
//
//            SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => e.GetType()));
//            SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => e.GetType(), new CancellationToken()));
//
//            SourceNonAsyncEnumerableTest(() => Source().LoadAsync());
//            SourceNonAsyncEnumerableTest(() => Source().LoadAsync(new CancellationToken()));
//
//            SourceNonAsyncQueryableTest(() => Source().LongCountAsync());
//            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true));
//            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true, new CancellationToken()));
//
//            SourceNonAsyncQueryableTest(() => Source().MaxAsync());
//            SourceNonAsyncQueryableTest(() => Source().MaxAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e, new CancellationToken()));
//
//            SourceNonAsyncQueryableTest(() => Source().MinAsync());
//            SourceNonAsyncQueryableTest(() => Source().MinAsync(new CancellationToken()));
//            SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().SingleAsync());
//            SourceNonAsyncQueryableTest(() => Source().SingleAsync(e => true));

//            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync());
//            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(e => true));

//            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(e => e));
//            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync());
//            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(e => e));
//
//            SourceNonAsyncEnumerableTest<int>(() => Source().ToArrayAsync());
//
//            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e));
//            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e));
//            SourceNonAsyncEnumerableTest<int>(
//                () => Source().ToDictionaryAsync(
//                    e => e,
//                    new Mock<IEqualityComparer<int>>().Object));
//            SourceNonAsyncEnumerableTest<int>(
//                () => Source().ToDictionaryAsync(
//                    e => e,
//                    new Mock<IEqualityComparer<int>>().Object, new CancellationToken()));
//            SourceNonAsyncEnumerableTest<int>(
//                () => Source().ToDictionaryAsync(
//                    e => e, e => e,
//                    new Mock<IEqualityComparer<int>>().Object));
//            SourceNonAsyncEnumerableTest<int>(
//                () => Source().ToDictionaryAsync(
//                    e => e, e => e,
//                    new Mock<IEqualityComparer<int>>().Object, new CancellationToken()));

//            SourceNonAsyncEnumerableTest<int>(() => Source().ToListAsync());

//            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ForEachAsync(e => e.GetType()));
//            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ForEachAsync(e => e.GetType(), new CancellationToken()));

//            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ToListAsync());
        }

        private static IQueryable<T> Source<T>()
        {
            return new Mock<IQueryable<T>>().Object;
        }

        private static IQueryable<int> Source()
        {
            return Source<int>();
        }

        private static void SourceNonAsyncQueryableTest(Action test)
        {
            Assert.Equal(Strings.FormatIQueryableProviderNotAsync(), Assert.Throws<InvalidOperationException>(test).Message);
        }

        private static void SourceNonAsyncEnumerableTest(Action test)
        {
            Assert.Equal(Strings.FormatIQueryableNotAsync(string.Empty), Assert.Throws<InvalidOperationException>(test).Message);
        }

        private static void SourceNonAsyncEnumerableTest<T>(Action test)
        {
            Assert.Equal(
                Strings.FormatIQueryableNotAsync("<" + typeof(T) + ">"), Assert.Throws<InvalidOperationException>(test).Message);
        }

        [Fact]
        public void Extension_methods_validate_arguments()
        {
            // ReSharper disable AssignNullToNotNullAttribute

//            ArgumentNullTest("source", () => QueryableExtensions.FirstAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.FirstAsync<int>(null, s => true));
//            ArgumentNullTest("predicate", () => Source().FirstAsync(null));
//
//            ArgumentNullTest("source", () => QueryableExtensions.FirstOrDefaultAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.FirstOrDefaultAsync<int>(null, s => true));
//            ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null));

            ArgumentNullTest("source", () => QueryableExtensions.SingleAsync<int>(null));

//            ArgumentNullTest("source", () => QueryableExtensions.SingleAsync<int>(null, s => true));
//            ArgumentNullTest("predicate", () => Source().SingleAsync(null));

//            ArgumentNullTest("source", () => QueryableExtensions.SingleOrDefaultAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.SingleOrDefaultAsync<int>(null, s => true));
//            ArgumentNullTest("predicate", () => Source().SingleOrDefaultAsync(null));

//            ArgumentNullTest("source", () => QueryableExtensions.ContainsAsync(null, 1));
//            ArgumentNullTest("source", () => QueryableExtensions.ContainsAsync(null, 1, new CancellationToken()));

            ArgumentNullTest("source", () => QueryableExtensions.AnyAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.AnyAsync<int>(null, s => true));
//            ArgumentNullTest("predicate", () => Source().AnyAsync(null));

//            ArgumentNullTest("source", () => QueryableExtensions.AllAsync<int>(null, s => true));
//            ArgumentNullTest("source", () => QueryableExtensions.AllAsync<int>(null, s => true, new CancellationToken()));
//            ArgumentNullTest("predicate", () => Source().AllAsync(null));
//            ArgumentNullTest("predicate", () => Source().AllAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => QueryableExtensions.CountAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.CountAsync<int>(null, s => true));
//            ArgumentNullTest("predicate", () => Source().CountAsync(null));

//            ArgumentNullTest("source", () => QueryableExtensions.LongCountAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.LongCountAsync<int>(null, new CancellationToken()));
//            ArgumentNullTest("source", () => QueryableExtensions.LongCountAsync<int>(null, s => true));
//            ArgumentNullTest("source", () => QueryableExtensions.LongCountAsync<int>(null, s => true, new CancellationToken()));
//            ArgumentNullTest("predicate", () => Source().LongCountAsync(null));
//            ArgumentNullTest("predicate", () => Source().LongCountAsync(null, new CancellationToken()));
//
//            ArgumentNullTest("source", () => QueryableExtensions.MinAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.MinAsync<int>(null, new CancellationToken()));
//            ArgumentNullTest("source", () => QueryableExtensions.MinAsync<int, bool>(null, s => true));
//            ArgumentNullTest("source", () => QueryableExtensions.MinAsync<int, bool>(null, s => true, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null));
//            ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null, new CancellationToken()));
//
//            ArgumentNullTest("source", () => QueryableExtensions.MaxAsync<int>(null));
//            ArgumentNullTest("source", () => QueryableExtensions.MaxAsync<int>(null, new CancellationToken()));
//            ArgumentNullTest("source", () => QueryableExtensions.MaxAsync<int, bool>(null, s => true));
//            ArgumentNullTest("source", () => QueryableExtensions.MaxAsync<int, bool>(null, s => true, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null));
//            ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null, new CancellationToken()));

//            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync());
//            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<int>().SumAsync((Expression<Func<int, int>>)null));
//            ArgumentNullTest("selector", () => Source<int?>().SumAsync((Expression<Func<int?, int>>)null));
//            ArgumentNullTest("selector", () => Source<long>().SumAsync((Expression<Func<long, int>>)null));
//            ArgumentNullTest("selector", () => Source<long?>().SumAsync((Expression<Func<long?, int>>)null));
//            ArgumentNullTest("selector", () => Source<float>().SumAsync((Expression<Func<float, int>>)null));
//            ArgumentNullTest("selector", () => Source<float?>().SumAsync((Expression<Func<float?, int>>)null));
//            ArgumentNullTest("selector", () => Source<double>().SumAsync((Expression<Func<double, int>>)null));
//            ArgumentNullTest("selector", () => Source<double?>().SumAsync((Expression<Func<double?, int>>)null));
//            ArgumentNullTest("selector", () => Source<decimal>().SumAsync((Expression<Func<decimal, int>>)null));
//            ArgumentNullTest("selector", () => Source<decimal?>().SumAsync((Expression<Func<decimal?, int>>)null));

//            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync());
//            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0));
//            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null));
//            ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null));
//            ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null));
//            ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null));
//            ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null));
//            ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null));
//            ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null));
//            ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null));
//            ArgumentNullTest(
//                "selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null));
//            ArgumentNullTest(
//                "selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null, new CancellationToken()));
//            ArgumentNullTest("selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null));
//            ArgumentNullTest(
//                "selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null, new CancellationToken()));

            // ReSharper restore AssignNullToNotNullAttribute
        }

        private static void ArgumentNullTest(string paramName, Action test)
        {
            Assert.Equal(paramName, Assert.Throws<ArgumentNullException>(test).ParamName);
        }
    }
}
