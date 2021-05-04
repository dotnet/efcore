// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        protected QueryNoClientEvalTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Throws_when_where()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers.Where(c => c.IsLondon).ToList(),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_orderby()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers.OrderBy(c => c.IsLondon).ToList(),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_orderby_multiple()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers
                    .OrderBy(c => c.IsLondon)
                    .ThenBy(c => ClientMethod(c))
                    .ToList(),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        private static object ClientMethod(object o)
            => o.GetHashCode();

        [ConditionalFact]
        public virtual void Throws_when_where_subquery_correlated()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers
                    .Where(c1 => context.Customers.Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon))
                    .ToList(),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_all()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers.All(c => c.IsLondon),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_from_sql_composed()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers
                    .FromSqlRaw(NormalizeDelimitersInRawString("select * from [Customers]"))
                    .Where(c => c.IsLondon)
                    .ToList(),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Doesnt_throw_when_from_sql_not_composed()
        {
            using var context = CreateContext();
            var customers
                = context.Customers
                    .FromSqlRaw(NormalizeDelimitersInRawString("select * from [Customers]"))
                    .ToList();

            Assert.Equal(91, customers.Count);
        }

        [ConditionalFact]
        public virtual void Throws_when_subquery_main_from_clause()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => (from c1 in context.Customers
                           .Where(c => c.IsLondon)
                           .OrderBy(c => c.CustomerID)
                           .Take(5)
                       select c1)
                    .ToList(),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_select_many()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c1 in context.Customers
                       from i in new[] { 1, 2, 3 }
                       select c1)
                    .ToList());
        }

        [ConditionalFact]
        public virtual void Throws_when_join()
        {
            using var context = CreateContext();
            AssertTranslationFailed(
                () => (from e1 in context.Employees
                       join i in new uint[] { 1, 2, 3 } on e1.EmployeeID equals i
                       select e1)
                    .ToList());
        }

        [ConditionalFact]
        public virtual void Throws_when_group_join()
        {
            using var context = CreateContext();
            AssertTranslationFailed(
                () => (from e1 in context.Employees
                       join i in new uint[] { 1, 2, 3 } on e1.EmployeeID equals i into g
                       select e1)
                    .ToList());
        }

        [ConditionalFact(Skip = "Issue#18923")]
        public virtual void Throws_when_group_by()
        {
            using var context = CreateContext();
            context.Customers
                .GroupBy(c => c.CustomerID)
                .ToList();

            AssertTranslationFailed(
                () => context.Customers
                    .GroupBy(c => c.CustomerID)
                    .ToList());
        }

        [ConditionalFact]
        public virtual void Throws_when_first()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers.First(c => c.IsLondon),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_single()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers.Single(c => c.IsLondon),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_first_or_default()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers.FirstOrDefault(c => c.IsLondon),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        [ConditionalFact]
        public virtual void Throws_when_single_or_default()
        {
            using var context = CreateContext();
            AssertTranslationFailedWithDetails(
                () => context.Customers.SingleOrDefault(c => c.IsLondon),
                CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));
        }

        private string NormalizeDelimitersInRawString(string sql)
            => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

        private void AssertTranslationFailed(Action testCode)
            => Assert.Contains(
                CoreStrings.TranslationFailed("").Substring(21),
                Assert.Throws<InvalidOperationException>(testCode).Message);

        private void AssertTranslationFailedWithDetails(Action testCode, string details)
            => Assert.Contains(
                CoreStrings.TranslationFailedWithDetails("", details).Substring(21),
                Assert.Throws<InvalidOperationException>(testCode).Message);

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();
    }
}
