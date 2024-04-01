// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindEFPropertyIncludeQueryTestBase<TFixture> : NorthwindIncludeQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    private static readonly IncludeRewritingExpressionVisitor _includeRewritingExpressionVisitor = new();

    protected NorthwindEFPropertyIncludeQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_non_existing_navigation(bool async)
        => Assert.Contains(
            CoreStrings.InvalidIncludeExpression("Property(o, \"ArcticMonkeys\")"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Order>().Include(o => EF.Property<Order>(o, "ArcticMonkeys"))))).Message);

    public override async Task Include_property(bool async)
        => Assert.Contains(
            CoreStrings.InvalidIncludeExpression("Property(o, \"OrderDate\")"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Order>().Include(o => o.OrderDate)))).Message);

    public override async Task Include_closes_reader(bool async)
    {
        using var context = CreateContext();
        if (async)
        {
            Assert.NotNull(await context.Set<Customer>().Include(c => EF.Property<Customer>(c, "Orders")).FirstOrDefaultAsync());
            Assert.NotNull(await context.Set<Product>().ToListAsync());
        }
        else
        {
            Assert.NotNull(context.Set<Customer>().Include(c => EF.Property<Customer>(c, "Orders")).FirstOrDefault());
            Assert.NotNull(context.Set<Product>().ToList());
        }
    }

    public override async Task Include_collection_dependent_already_tracked(bool async)
    {
        using var context = CreateContext();
        var orders = context.Set<Order>().Where(o => o.CustomerID == "ALFKI").ToList();
        Assert.Equal(6, context.ChangeTracker.Entries().Count());

        var customer
            = async
                ? await context.Set<Customer>()
                    .Include(c => EF.Property<Customer>(c, "Orders"))
                    .SingleAsync(c => c.CustomerID == "ALFKI")
                : context.Set<Customer>()
                    .Include(c => EF.Property<Customer>(c, "Orders"))
                    .Single(c => c.CustomerID == "ALFKI");

        Assert.Equal(orders, customer.Orders, ReferenceEqualityComparer.Instance);
        Assert.Equal(6, customer.Orders.Count);
        Assert.True(orders.All(o => ReferenceEquals(o.Customer, customer)));
        Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
    }

    public override async Task Include_collection_principal_already_tracked(bool async)
    {
        using var context = CreateContext();
        var customer1 = context.Set<Customer>().Single(c => c.CustomerID == "ALFKI");
        Assert.Single(context.ChangeTracker.Entries());

        var customer2
            = async
                ? await context.Set<Customer>()
                    .Include(c => EF.Property<Customer>(c, "Orders"))
                    .SingleAsync(c => c.CustomerID == "ALFKI")
                : context.Set<Customer>()
                    .Include(c => EF.Property<Customer>(c, "Orders"))
                    .Single(c => c.CustomerID == "ALFKI");

        Assert.Same(customer1, customer2);
        Assert.Equal(6, customer2.Orders.Count);
        Assert.True(customer2.Orders.All(o => o.Customer != null));
        Assert.Equal(7, context.ChangeTracker.Entries().Count());
    }

    public override async Task Include_reference_dependent_already_tracked(bool async)
    {
        using var context = CreateContext();
        var customer = context.Set<Customer>().Single(o => o.CustomerID == "ALFKI");
        Assert.Single(context.ChangeTracker.Entries());

        var orders
            = async
                ? await context.Set<Order>().Include(o => EF.Property<Order>(o, "Customer")).Where(o => o.CustomerID == "ALFKI")
                    .ToListAsync()
                : context.Set<Order>().Include(o => EF.Property<Order>(o, "Customer")).Where(o => o.CustomerID == "ALFKI").ToList();

        Assert.Equal(6, orders.Count);
        Assert.True(orders.All(o => ReferenceEquals(o.Customer, customer)));
        Assert.Equal(7, context.ChangeTracker.Entries().Count());
    }

    public override async Task Include_specified_on_non_entity_not_supported(bool async)
        => Assert.Equal(
            CoreStrings.IncludeOnNonEntity("t => t.Item1.Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Select(c => new Tuple<Customer, int>(c, 5)).Include(t => t.Item1.Orders)))).Message);

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        return _includeRewritingExpressionVisitor.Visit(serverQueryExpression);
    }

    private class IncludeRewritingExpressionVisitor : ExpressionVisitor
    {
        // ReSharper disable StaticMemberInGenericType
        private static readonly MethodInfo _includeMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include))
                .Single(
                    mi =>
                        mi.GetGenericArguments().Count() == 2
                        && mi.GetParameters().Any(
                            pi => pi.Name == "navigationPropertyPath" && pi.ParameterType != typeof(string)));

        private static readonly MethodInfo _thenIncludeAfterReferenceMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                .Single(
                    mi => mi.GetGenericArguments().Count() == 3
                        && mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter);

        private static readonly MethodInfo _thenIncludeAfterEnumerableMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                .Where(mi => mi.GetGenericArguments().Count() == 3)
                .Single(
                    mi =>
                    {
                        var typeInfo = mi.GetParameters()[0].ParameterType.GenericTypeArguments[1];
                        return typeInfo.IsGenericType
                            && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                    });

        private static readonly MethodInfo _propertyMethod
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(EF.Property))!;

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && methodCallExpression.Method.IsGenericMethod)
            {
                var genericMethodDefinition = methodCallExpression.Method.GetGenericMethodDefinition();
                if (genericMethodDefinition == _includeMethodInfo)
                {
                    return BuildEFPropertyCallExpression(0, 1);
                }

                if (genericMethodDefinition == _thenIncludeAfterEnumerableMethodInfo
                    || genericMethodDefinition == _thenIncludeAfterReferenceMethodInfo)
                {
                    return BuildEFPropertyCallExpression(1, 2);
                }
            }

            return base.VisitMethodCall(methodCallExpression);

            Expression BuildEFPropertyCallExpression(int entityTypeIndex, int propertyTypeIndex)
            {
                var arguments = methodCallExpression.Arguments;
                var genericArguments = methodCallExpression.Method.GetGenericArguments();
                var fromQuote = arguments[1].UnwrapLambdaFromQuote();
                var parameterExpression = fromQuote.Parameters[0];

                var path = GetPath(fromQuote.Body);
                if (path != null && !path.Contains("."))
                {
                    return Expression.Call(
                        methodCallExpression.Method,
                        Visit(arguments[0]),
                        Expression.Quote(
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(genericArguments[entityTypeIndex], genericArguments[propertyTypeIndex]),
                                Expression.Call(
                                    _propertyMethod.MakeGenericMethod(genericArguments[propertyTypeIndex]),
                                    parameterExpression,
                                    Expression.Constant(path)),
                                parameterExpression)));
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        private static string GetPath(Expression expression)
            => expression switch
            {
                MemberExpression { Expression: ParameterExpression } memberExpression
                    => memberExpression.Member.Name,
                MemberExpression memberExpression
                    => $"{GetPath(memberExpression.Expression)}.{memberExpression.Member.Name}",
                UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.Convert or ExpressionType.TypeAs } unaryExpression
                    => GetPath(unaryExpression.Operand),
                _ => null
            };
    }
}
