// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindQueryTaggingQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindQueryTaggingQueryTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Single_query_tag()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .TagWith("Yanni")
                    .First();

            Assert.NotNull(customer);
        }

        [ConditionalFact]
        public virtual void Single_query_multiple_tags()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .TagWith("Yanni")
                    .TagWith("Enya")
                    .First();

            Assert.NotNull(customer);
        }

        [ConditionalFact]
        public virtual void Duplicate_tags()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .TagWith("Yanni")
                    .TagWith("Yanni")
                    .First();

            Assert.NotNull(customer);
        }

        [ConditionalFact]
        public virtual void Tags_on_subquery()
        {
            using var context = CreateContext();
            var customers
                = (from c in context.Set<Customer>().Where(c => c.CustomerID == "ALFKI").AsNoTracking().TagWith("Yanni")
                   from o in context.Orders.OrderBy(o => o.OrderID).Take(5).TagWith("Laurel")
                   select c).ToList();

            Assert.Equal(5, customers.Count);
        }

        [ConditionalFact]
        public virtual void Tag_on_include_query()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Customer>()
                    .Include(c => c.Orders)
                    .OrderBy(c => c.CustomerID)
                    .TagWith("Yanni")
                    .First();

            Assert.NotNull(customer);
        }

        [ConditionalFact]
        public virtual void Tag_on_scalar_query()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Order>()
                    .OrderBy(o => o.OrderID)
                    .Select(o => o.OrderDate)
                    .TagWith("Yanni")
                    .First();

            Assert.NotNull(customer);
        }

        [ConditionalFact]
        public virtual void Single_query_multiline_tag()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .TagWith(
                        @"Yanni
AND
Laurel")
                    .First();

            Assert.NotNull(customer);
        }

        [ConditionalFact]
        public virtual void Single_query_multiple_multiline_tag()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .TagWith(
                        @"Yanni
AND
Laurel")
                    .TagWith(
                        @"Yet
Another
Multiline
Tag")
                    .First();

            Assert.NotNull(customer);
        }

        [ConditionalFact]
        public virtual void Single_query_multiline_tag_with_empty_lines()
        {
            using var context = CreateContext();
            var customer
                = context.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .TagWith(
                        @"Yanni

AND

Laurel")
                    .First();

            Assert.NotNull(customer);
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();
    }
}
