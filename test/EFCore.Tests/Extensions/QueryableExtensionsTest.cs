// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public class QueryableExtensionsTest
{
    [ConditionalFact]
    public void Include_on_non_ef_queryable_is_no_op()
    {
        var q = new List<Customer>().AsQueryable();
        var q2 = q.Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ToList();

        Assert.Empty(q2);
    }

    [ConditionalFact]
    public void AsTracking_on_non_ef_queryable_is_no_op()
    {
        var q = new List<Customer>().AsQueryable();
        var q2 = q.AsTracking().ToList();

        Assert.Empty(q2);
    }

    [ConditionalFact]
    public void AsNoTracking_on_non_ef_queryable_is_no_op()
    {
        var q = new List<Customer>().AsQueryable();
        var q2 = q.AsNoTracking().ToList();

        Assert.Empty(q2);
    }

    // ReSharper disable MethodSupportsCancellation

    [ConditionalFact]
    public void Extension_methods_call_provider_ExecuteAsync()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, bool>(value => value.AnyAsync(default));
        VerifyProducedExpression<int, bool>(value => value.AnyAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, default));
        VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, double>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<int?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<long, double>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<long?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<float, float>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<float?, float?>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<double, double>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<double?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.CountAsync(default));
        VerifyProducedExpression<int, int>(value => value.CountAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.CountAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.FirstAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, long>(value => value.LongCountAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.MaxAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.MinAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.MinAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.SingleAsync(default));
        VerifyProducedExpression<int, int>(value => value.SingleAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, default));
        VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(default));
        VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, default));
        VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.SumAsync(default));
        VerifyProducedExpression<int, int>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int, int>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<int?, int?>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<long, long>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<long, long>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<long?, long?>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<float, float>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<float, float>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<float?, float?>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<double, double>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<double, double>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<double?, double?>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(default));
        VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(cancellationTokenSource.Token));
        VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
    }

    private static void VerifyProducedExpression<TElement, TResult>(
        Expression<Func<IQueryable<TElement>, Task<TResult>>> testExpression)
    {
        var provider = new FakeAsyncQueryProvider((MethodCallExpression)testExpression.Body);
        var queryable = new FakeQueryable<TElement>(provider);
        queryable.Expression = Expression.Constant(queryable, typeof(IQueryable<TElement>));

        testExpression.Compile()(queryable);
    }

    private class FakeAsyncQueryProvider(MethodCallExpression expectedMethodCall) : IAsyncQueryProvider
    {
        private readonly MethodCallExpression _expectedMethodCall = expectedMethodCall;

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var actualMethodCall = (MethodCallExpression)expression;

            Assert.Equal(
                _expectedMethodCall.Method.Name,
                actualMethodCall.Method.Name + "Async");

            var cancellationTokenPresent
                = (_expectedMethodCall.Arguments[_expectedMethodCall.Arguments.Count - 1] is MemberExpression lastArgument)
                && (lastArgument.Type == typeof(CancellationToken));

            if (cancellationTokenPresent)
            {
                Assert.NotEqual(cancellationToken, CancellationToken.None);
            }
            else
            {
                Assert.Equal(cancellationToken, CancellationToken.None);
            }

            for (var i = 1; i < _expectedMethodCall.Arguments.Count - 1; i++)
            {
                var expectedArgument = _expectedMethodCall.Arguments[i];
                var actualArgument = actualMethodCall.Arguments[i];

                Assert.Equal(expectedArgument.ToString(), actualArgument.ToString());
            }

            return default;
        }

        public IQueryable CreateQuery(Expression expression)
            => throw new NotImplementedException();

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => throw new NotImplementedException();

        public object Execute(Expression expression)
            => throw new NotImplementedException();

        public TResult Execute<TResult>(Expression expression)
            => throw new NotImplementedException();
    }

    private class FakeQueryable<TElement>(IQueryProvider provider = null) : IQueryable<TElement>
    {
        public Type ElementType
            => typeof(TElement);

        public Expression Expression { get; set; }

        public IQueryProvider Provider { get; } = provider;

        public IEnumerator<TElement> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => throw new NotImplementedException();
    }

    [ConditionalFact]
    public async Task Extension_methods_throw_on_non_async_source()
    {
        await SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().AnyAsync());
        await SourceNonAsyncQueryableTest(() => Source().AnyAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync());
        await SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0));
        await SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().CountAsync());
        await SourceNonAsyncQueryableTest(() => Source().CountAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source().FirstAsync());
        await SourceNonAsyncQueryableTest(() => Source().FirstAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync());
        await SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true, new CancellationToken()));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => { }));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => { }, new CancellationToken()));
        await SourceNonAsyncEnumerableTest<int>(() => Source().LoadAsync());
        await SourceNonAsyncEnumerableTest<int>(() => Source().LoadAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().LongCountAsync());
        await SourceNonAsyncQueryableTest(() => Source().LongCountAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().MaxAsync());
        await SourceNonAsyncQueryableTest(() => Source().MaxAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().MinAsync());
        await SourceNonAsyncQueryableTest(() => Source().MinAsync(new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e, new CancellationToken()));
        await SourceNonAsyncQueryableTest(() => Source().SingleAsync());
        await SourceNonAsyncQueryableTest(() => Source().SingleAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync());
        await SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(e => true));
        await SourceNonAsyncQueryableTest(() => Source<int>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<long>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<float>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<double>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(e => e));
        await SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync());
        await SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(e => e));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, ReferenceEqualityComparer.Instance));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, ReferenceEqualityComparer.Instance));
        await SourceNonAsyncEnumerableTest<int>(
            () => Source().ToDictionaryAsync(e => e, e => e, ReferenceEqualityComparer.Instance));
        await SourceNonAsyncEnumerableTest<int>(
            () => Source().ToDictionaryAsync(e => e, e => e, ReferenceEqualityComparer.Instance, new CancellationToken()));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ToHashSetAsync());
        await SourceNonAsyncEnumerableTest<int>(() => Source().ToHashSetAsync(EqualityComparer<int>.Default));
        await SourceNonAsyncEnumerableTest<int>(
            () => Source().ToHashSetAsync(EqualityComparer<int>.Default, new CancellationToken()));
        await SourceNonAsyncEnumerableTest<int>(() => Source().ToListAsync());

        Assert.Equal(
            CoreStrings.IQueryableNotAsync(typeof(int)),
            Assert.Throws<InvalidOperationException>(() => Source().AsAsyncEnumerable()).Message);
    }

    private static IQueryable<T> Source<T>()
        => new FakeQueryable<T>();

    private static IQueryable<int> Source()
        => Source<int>();

    private static async Task SourceNonAsyncQueryableTest(Func<Task> test)
        => Assert.Equal(
            CoreStrings.IQueryableProviderNotAsync,
            (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message);

    private static async Task SourceNonAsyncEnumerableTest<T>(Func<Task> test)
        => Assert.Equal(
            CoreStrings.IQueryableNotAsync(typeof(T)),
            (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message);

    [ConditionalFact]
    public async Task Extension_methods_validate_arguments()
    {
        // ReSharper disable AssignNullToNotNullAttribute

        await ArgumentNullTest("predicate", () => Source().FirstAsync(null));
        await ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null));
        await ArgumentNullTest("predicate", () => Source().SingleAsync(null));
        await ArgumentNullTest("predicate", () => Source().SingleOrDefaultAsync(null));
        await ArgumentNullTest("predicate", () => Source().AnyAsync(null));
        await ArgumentNullTest("predicate", () => Source().AllAsync(null));
        await ArgumentNullTest("predicate", () => Source().AllAsync(null, new CancellationToken()));
        await ArgumentNullTest("predicate", () => Source().CountAsync(null));
        await ArgumentNullTest("predicate", () => Source().LongCountAsync(null));
        await ArgumentNullTest("predicate", () => Source().LongCountAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null));
        await ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null));
        await ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<int>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<int?>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<long>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<long?>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<float>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<float?>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<double>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<double?>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<decimal>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<decimal?>().SumAsync(null));
        await ArgumentNullTest("selector", () => Source<int>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<int>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<int?>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<int?>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<long>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<long>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<long?>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<long?>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<float>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<float>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<float?>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<float?>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<double>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<double>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<double?>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<double?>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<decimal>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<decimal>().AverageAsync(null, new CancellationToken()));
        await ArgumentNullTest("selector", () => Source<decimal?>().AverageAsync(null));
        await ArgumentNullTest("selector", () => Source<decimal?>().AverageAsync(null, new CancellationToken()));

        // ReSharper restore AssignNullToNotNullAttribute
    }

    private static async Task ArgumentNullTest(string paramName, Func<Task> test)
        => Assert.Equal(paramName, (await Assert.ThrowsAsync<ArgumentNullException>(test)).ParamName);

    // ReSharper restore MethodSupportsCancellation
}
