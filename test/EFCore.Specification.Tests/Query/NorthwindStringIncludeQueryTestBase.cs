// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable StringStartsWithIsCultureSpecific

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindStringIncludeQueryTestBase<TFixture> : NorthwindIncludeQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        private static readonly IncludeRewritingExpressionVisitor _includeRewritingExpressionVisitor =
            new IncludeRewritingExpressionVisitor();

        protected NorthwindStringIncludeQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_non_existing_navigation(bool async)
        {
            Assert.Contains(
                CoreResources.LogInvalidIncludePath(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage("ArcticMonkeys", "ArcticMonkeys"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Order>().Include("ArcticMonkeys")))).Message);
        }

        public override async Task Include_property(bool async)
        {
            Assert.Contains(
                CoreResources.LogInvalidIncludePath(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage("OrderDate", "OrderDate"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Order>().Include(o => o.OrderDate)))).Message);
        }

        public override async Task Include_property_after_navigation(bool async)
        {
            Assert.Contains(
                CoreResources.LogInvalidIncludePath(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage("Customer.CustomerID", "CustomerID"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Order>().Include(o => o.Customer.CustomerID)))).Message);
        }

        // Property expression cannot be converted to string include
        public override Task Include_property_expression_invalid(bool async)
            => Task.CompletedTask;

        // Property expression cannot be converted to string include
        public override Task Then_include_property_expression_invalid(bool async)
            => Task.CompletedTask;

        public override async Task Include_closes_reader(bool async)
        {
            using var context = CreateContext();
            if (async)
            {
                Assert.NotNull(await context.Set<Customer>().Include("Orders").FirstOrDefaultAsync());
                Assert.NotNull(await context.Set<Product>().ToListAsync());
            }
            else
            {
                Assert.NotNull(context.Set<Customer>().Include("Orders").FirstOrDefault());
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
                        .Include("Orders")
                        .SingleAsync(c => c.CustomerID == "ALFKI")
                    : context.Set<Customer>()
                        .Include("Orders")
                        .Single(c => c.CustomerID == "ALFKI");

            Assert.Equal(orders, customer.Orders, LegacyReferenceEqualityComparer.Instance);
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
                        .Include("Orders")
                        .SingleAsync(c => c.CustomerID == "ALFKI")
                    : context.Set<Customer>()
                        .Include("Orders")
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
                    ? await context.Set<Order>().Include("Customer").Where(o => o.CustomerID == "ALFKI").ToListAsync()
                    : context.Set<Order>().Include("Customer").Where(o => o.CustomerID == "ALFKI").ToList();

            Assert.Equal(6, orders.Count);
            Assert.True(orders.All(o => ReferenceEquals(o.Customer, customer)));
            Assert.Equal(7, context.ChangeTracker.Entries().Count());
        }

        // Filtered include does not work for string based API.
        public override Task Filtered_include_with_multiple_ordering(bool async)
            => Task.CompletedTask;

        public override async Task Include_specified_on_non_entity_not_supported(bool async)
        {
            Assert.Equal(
                CoreStrings.IncludeOnNonEntity("\"Item1.Orders\""),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Customer>().Select(c => new Tuple<Customer, int>(c, 5)).Include(t => t.Item1.Orders)))).Message);
        }

        protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        {
            serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

            return _includeRewritingExpressionVisitor.Visit(serverQueryExpression);
        }

        private class IncludeRewritingExpressionVisitor : ExpressionVisitor
        {
            private static readonly MethodInfo _includeMethodInfo
                = typeof(EntityFrameworkQueryableExtensions)
                    .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include))
                    .Single(
                        mi =>
                            mi.GetGenericArguments().Count() == 2
                            && mi.GetParameters().Any(
                                pi => pi.Name == "navigationPropertyPath" && pi.ParameterType != typeof(string)));

            private static readonly MethodInfo _stringIncludeMethodInfo
                = typeof(EntityFrameworkQueryableExtensions)
                    .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include))
                    .Single(
                        mi => mi.GetParameters().Any(
                            pi => pi.Name == "navigationPropertyPath" && pi.ParameterType == typeof(string)));

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

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                    && methodCallExpression.Method.IsGenericMethod)
                {
                    var genericMethodDefinition = methodCallExpression.Method.GetGenericMethodDefinition();
                    if (genericMethodDefinition == _includeMethodInfo)
                    {
                        var source = Visit(methodCallExpression.Arguments[0]);

                        return Expression.Call(
                            _stringIncludeMethodInfo.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]),
                            source,
                            Expression.Constant(GetPath(methodCallExpression.Arguments[1].UnwrapLambdaFromQuote().Body)));
                    }

                    if (genericMethodDefinition == _thenIncludeAfterEnumerableMethodInfo
                        || genericMethodDefinition == _thenIncludeAfterReferenceMethodInfo)
                    {
                        var innerIncludeMethodCall = (MethodCallExpression)Visit(methodCallExpression.Arguments[0]);
                        var innerNavigationPath = (string)((ConstantExpression)innerIncludeMethodCall.Arguments[1]).Value;
                        var currentNavigationpath = GetPath(methodCallExpression.Arguments[1].UnwrapLambdaFromQuote().Body);

                        return innerIncludeMethodCall.Update(
                            innerIncludeMethodCall.Object,
                            new[]
                            {
                                innerIncludeMethodCall.Arguments[0],
                                Expression.Constant($"{innerNavigationPath}.{currentNavigationpath}")
                            });
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private static string GetPath(Expression expression)
            {
                switch (expression)
                {
                    case MemberExpression memberExpression:
                        if (memberExpression.Expression is ParameterExpression)
                        {
                            return memberExpression.Member.Name;
                        }

                        return $"{GetPath(memberExpression.Expression)}.{memberExpression.Member.Name}";

                    case UnaryExpression unaryExpression
                        when unaryExpression.NodeType == ExpressionType.Convert
                        || unaryExpression.NodeType == ExpressionType.Convert
                        || unaryExpression.NodeType == ExpressionType.TypeAs:
                        return GetPath(unaryExpression.Operand);

                    default:
                        throw new NotImplementedException("Unhandled expression tree in Include lambda");
                }
            }
        }
    }
}
