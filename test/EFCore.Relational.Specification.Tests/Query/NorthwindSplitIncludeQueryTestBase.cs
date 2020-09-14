// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable FormatStringProblem
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindSplitIncludeQueryTestBase<TFixture> : NorthwindIncludeQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        private static readonly MethodInfo _asSplitIncludeMethodInfo
            = typeof(RelationalQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(RelationalQueryableExtensions.AsSplitQuery));

        protected NorthwindSplitIncludeQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override async Task Include_closes_reader(bool async)
        {
            using var context = CreateContext();
            if (async)
            {
                Assert.NotNull(await context.Set<Customer>().Include(c => c.Orders).AsSplitQuery().FirstOrDefaultAsync());
                Assert.NotNull(await context.Set<Product>().AsNoTracking().ToListAsync());
            }
            else
            {
                Assert.NotNull(context.Set<Customer>().Include(c => c.Orders).AsSplitQuery().FirstOrDefault());
                Assert.NotNull(context.Set<Product>().AsNoTracking().ToList());
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
                        .Include(c => c.Orders)
                        .AsSplitQuery()
                        .SingleAsync(c => c.CustomerID == "ALFKI")
                    : context.Set<Customer>()
                        .Include(c => c.Orders)
                        .AsSplitQuery()
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
                        .Include(c => c.Orders)
                        .AsSplitQuery()
                        .SingleAsync(c => c.CustomerID == "ALFKI")
                    : context.Set<Customer>()
                        .Include(c => c.Orders)
                        .AsSplitQuery()
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
                    ? await context.Set<Order>().Include(o => o.Customer).AsSplitQuery().Where(o => o.CustomerID == "ALFKI").ToListAsync()
                    : context.Set<Order>().Include(o => o.Customer).AsSplitQuery().Where(o => o.CustomerID == "ALFKI").ToList();

            Assert.Equal(6, orders.Count);
            Assert.True(orders.All(o => ReferenceEquals(o.Customer, customer)));
            Assert.Equal(7, context.ChangeTracker.Entries().Count());
        }

        public override Task Include_collection_with_last_no_orderby(bool async)
        {
            return AssertTranslationFailedWithDetails(
                () => base.Include_collection_with_last_no_orderby(async), RelationalStrings.MissingOrderingInSelectExpression);
        }

        [ConditionalTheory(Skip = "Issue#22283 Collection Include on nested collection")]
        public override Task Multi_level_includes_are_applied_with_take(bool async)
        {
            return base.Multi_level_includes_are_applied_with_take(async);
        }

        [ConditionalTheory(Skip = "Issue#22283 Collection Include on nested collection")]
        public override Task Multi_level_includes_are_applied_with_skip(bool async)
        {
            return base.Multi_level_includes_are_applied_with_skip(async);
        }

        [ConditionalTheory(Skip = "Issue#22283 Collection Include on nested collection")]
        public override Task Multi_level_includes_are_applied_with_skip_take(bool async)
        {
            return base.Multi_level_includes_are_applied_with_skip_take(async);
        }

        [ConditionalTheory(Skip = "Issue#22283 Collection Include on nested entity")]
        public override Task Include_in_let_followed_by_FirstOrDefault(bool async)
        {
            return base.Include_in_let_followed_by_FirstOrDefault(async);
        }

        protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        {
            serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

            return Expression.Call(
                _asSplitIncludeMethodInfo.MakeGenericMethod(serverQueryExpression.Type.TryGetSequenceType()),
                serverQueryExpression);
        }
    }
}
