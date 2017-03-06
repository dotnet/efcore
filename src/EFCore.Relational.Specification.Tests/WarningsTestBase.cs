// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;

// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class WarningsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture, new()
    {
        [Fact]
        public virtual void Throws_when_warning_as_error()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.WarningAsErrorTemplate(
                        $"{nameof(RelationalEventId)}.{nameof(RelationalEventId.QueryClientEvaluationWarning)}",
                        RelationalStrings.ClientEvalWarning("[c].IsLondon")),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.Where(c => c.IsLondon).ToList()).Message);
            }
        }

        [Fact]
        public virtual void Does_not_throw_for_top_level_single()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders.Single(x => x.OrderID == 10248);

                Assert.NotNull(query);
            }
        }

        [Fact]
        public virtual void Paging_operation_without_orderby_issues_warning()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.Skip(2).Take(3).ToList();
                Assert.Equal(3, query.Count);
            }
        }

        [Fact]
        public virtual void FirstOrDefault_without_orderby_and_filter_issues_warning_subquery()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.Where(c => c.CustomerID == "ALFKI" && c.Orders.FirstOrDefault().OrderID > 1000).ToList();
                Assert.Equal(1, query.Count);
            }
        }

        [Fact]
        public virtual void FirstOrDefault_without_orderby_but_with_filter_doesnt_issue_warning()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.Where(c => c.CustomerID == "ALFKI").FirstOrDefault();
                Assert.NotNull(query);
            }
        }

        [Fact]
        public virtual void Single_SingleOrDefault_without_orderby_doesnt_issue_warning()
        {
            using (var context = CreateContext())
            {
                var query1 = context.Customers.Where(c => c.CustomerID == "ALFKI").Single();
                Assert.NotNull(query1);

                var query2 = context.Customers.Where(c => c.CustomerID == "AROUT").SingleOrDefault();
                Assert.NotNull(query2);
            }
        }

        [Fact]
        public virtual void LastOrDefault_with_order_by_does_not_issue_client_eval_warning()
        {
            using (var context = CreateContext())
            {
                var query1 = context.Customers.Where(c => c.CustomerID == "ALFKI" && c.Orders.OrderBy(o => o.OrderID).LastOrDefault().OrderID > 1000).ToList();
                Assert.NotNull(query1);

                var query2 = context.Customers.OrderBy(c => c.CustomerID).LastOrDefault();
                Assert.NotNull(query2);
            }
        }

        [Fact]
        public virtual void Last_with_order_by_does_not_issue_client_eval_warning_if_at_top_level()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID).Last();
                Assert.NotNull(query);
            }
        }

        [Fact]
        public virtual void Last_without_order_by_issues_client_eval_warning()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.WarningAsErrorTemplate(
                        $"{nameof(RelationalEventId)}.{nameof(RelationalEventId.QueryClientEvaluationWarning)}",
                        RelationalStrings.ClientEvalWarning("Last()")),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.Last()).Message);
            }
        }

        [Fact]
        public virtual void Last_with_order_by_issues_client_eval_warning_in_subquery()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.WarningAsErrorTemplate(
                        $"{nameof(RelationalEventId)}.{nameof(RelationalEventId.QueryClientEvaluationWarning)}",
                        RelationalStrings.ClientEvalWarning("Last()")),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.Where(c => c.CustomerID == "ALFKI" && c.Orders.OrderBy(o => o.OrderID).Last().OrderID > 1000).ToList()).Message);
            }
        }

        [Fact]
        public virtual void LastOrDefault_without_order_by_issues_client_eval_warning()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.WarningAsErrorTemplate(
                        $"{nameof(RelationalEventId)}.{nameof(RelationalEventId.QueryClientEvaluationWarning)}",
                        RelationalStrings.ClientEvalWarning("LastOrDefault()")),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.LastOrDefault()).Message);
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected WarningsTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}
