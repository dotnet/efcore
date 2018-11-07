// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryNoClientEvalTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
    {
        protected QueryNoClientEvalTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void Throws_when_where()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                            () => context.Customers.Where(c => c.IsLondon).ToList())
                        .Message);
            }
        }

        [Fact]
        public virtual void Throws_when_orderby()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("orderby [c].IsLondon asc"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.OrderBy(c => c.IsLondon).ToList()).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_orderby_multiple()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("orderby [c].IsLondon asc, ClientMethod([c]) asc"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers
                            .OrderBy(c => c.IsLondon)
                            .ThenBy(c => ClientMethod(c))
                            .ToList()).Message);
            }
        }

        private static object ClientMethod(object o) => o.GetHashCode();

        [Fact]
        public virtual void Throws_when_where_subquery_correlated()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage(
                            "where {from Customer c2 in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.Northwind.Customer]) where (([c1].CustomerID == [c2].CustomerID) AndAlso [c2].IsLondon) select [c2] => Any()}"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers
                            .Where(
                                c1 => context.Customers
                                    .Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon))
                            .ToList()).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_all()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("All([c].IsLondon)"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.All(c => c.IsLondon)).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_from_sql_composed()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers
                            .FromSql(NormalizeDelimeters("select * from [Customers]"))
                            .Where(c => c.IsLondon)
                            .ToList()).Message);
            }
        }

        [Fact]
        public virtual void Doesnt_throw_when_from_sql_not_composed()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Customers
                        .FromSql(NormalizeDelimeters("select * from [Customers]"))
                        .ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Throws_when_subquery_main_from_clause()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            (from c1 in context.Customers
                                 .Where(c => c.IsLondon)
                                 .OrderBy(c => c.CustomerID)
                                 .Take(5)
                             select c1)
                            .ToList()).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_select_many()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("from Int32 i in value(System.Int32[])"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            (from c1 in context.Customers
                             from i in new[] { 1, 2, 3 }
                             select c1)
                            .ToList()).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_join()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage(
                            "join UInt32 i in __p_0 on [e1].EmployeeID equals [i]"
                        ),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            (from e1 in context.Employees
                             join i in new uint[] { 1, 2, 3 } on e1.EmployeeID equals i
                             select e1)
                            .ToList()).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_group_join()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage(
                            "join UInt32 i in __p_0 on [e1].EmployeeID equals [i]"
                        ),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            (from e1 in context.Employees
                             join i in new uint[] { 1, 2, 3 } on e1.EmployeeID equals i into g
                             select e1)
                            .ToList()).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_group_by()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("GroupBy([c].CustomerID, [c])"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers
                            .GroupBy(c => c.CustomerID)
                            .ToList()).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_first()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.First(c => c.IsLondon)).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_single()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.Single(c => c.IsLondon)).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_first_or_default()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.FirstOrDefault(c => c.IsLondon)).Message);
            }
        }

        [Fact]
        public virtual void Throws_when_single_or_default()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.SingleOrDefault(c => c.IsLondon)).Message);
            }
        }

        private RawSqlString NormalizeDelimeters(RawSqlString sql)
            => Fixture.TestStore.NormalizeDelimeters(sql);

        private FormattableString NormalizeDelimeters(FormattableString sql)
            => Fixture.TestStore.NormalizeDelimeters(sql);

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
