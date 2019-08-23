// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
// ReSharper disable StringStartsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FiltersTestBase<TFixture> : IClassFixture<TFixture>, IDisposable
        where TFixture : NorthwindQueryFixtureBase<NorthwindFiltersCustomizer>, new()
    {
        private readonly NorthwindContext _context;

        protected FiltersTestBase(TFixture fixture)
        {
            Fixture = fixture;

            _context = CreateContext();
        }

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Count_query()
        {
            Assert.Equal(7, _context.Customers.Count());
        }

        [ConditionalFact]
        public virtual void Materialized_query()
        {
            Assert.Equal(7, _context.Customers.ToList().Count);
        }

        [ConditionalFact]
        public virtual void Find()
        {
            Assert.Null(_context.Find<Customer>("ALFKI"));
        }

        [ConditionalFact]
        public virtual void Client_eval()
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Product>(    source: DbSet<Product>,     predicate: (p) => ClientMethod(p))"),
                RemoveNewLines(Assert.Throws<InvalidOperationException>(
                    () => _context.Products.ToList()).Message));
        }

        [ConditionalFact]
        public virtual async Task Materialized_query_async()
        {
            Assert.Equal(7, (await _context.Customers.ToListAsync()).Count);
        }

        [ConditionalFact]
        public virtual void Materialized_query_parameter()
        {
            _context.TenantPrefix = "F";

            Assert.Equal(8, _context.Customers.ToList().Count);
        }

        [ConditionalFact]
        public virtual void Materialized_query_parameter_new_context()
        {
            Assert.Equal(7, _context.Customers.ToList().Count);

            using (var context = CreateContext())
            {
                context.TenantPrefix = "T";

                Assert.Equal(6, context.Customers.ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual void Projection_query()
        {
            Assert.Equal(7, _context.Customers.Select(c => c.CustomerID).ToList().Count);
        }

        [ConditionalFact]
        public virtual void Projection_query_parameter()
        {
            _context.TenantPrefix = "F";

            Assert.Equal(8, _context.Customers.Select(c => c.CustomerID).ToList().Count);
        }

        [ConditionalFact]
        public virtual void Include_query()
        {
            var results = _context.Customers.Include(c => c.Orders).ToList();

            Assert.Equal(7, results.Count);
        }

        [ConditionalFact]
        public virtual void Include_query_opt_out()
        {
            var results = _context.Customers.Include(c => c.Orders).IgnoreQueryFilters().ToList();

            Assert.Equal(91, results.Count);
        }

        [ConditionalFact]
        public virtual void Included_many_to_one_query()
        {
            var results = _context.Orders.Include(o => o.Customer).ToList();

            Assert.Equal(80, results.Count);
            Assert.True(results.All(o => o.Customer == null || o.CustomerID.StartsWith("B")));
        }

        [ConditionalFact]
        public virtual void Project_reference_that_itself_has_query_filter_with_another_reference()
        {
            var results = _context.OrderDetails.Select(od => od.Order).ToList();

            Assert.Equal(5, results.Count);
            Assert.True(results.All(o => o.Customer == null || o.CustomerID.StartsWith("B")));
        }

        [ConditionalFact]
        public virtual void Included_one_to_many_query_with_client_eval()
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Product>(    source: DbSet<Product>,     predicate: (p) => ClientMethod(p))"),
                RemoveNewLines(Assert.Throws<InvalidOperationException>(
                    () => _context.Products.Include(p => p.OrderDetails).ToList()).Message));
        }

        [ConditionalFact(Skip = "issue #15081")]
        public virtual void Navs_query()
        {
            var results
                = (from c in _context.Customers
                   from o in c.Orders
                   from od in o.OrderDetails
                   where od.Discount < 10
                   select c).ToList();

            Assert.Equal(5, results.Count);
        }

        [ConditionalFact]
        public virtual void Compiled_query()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            Assert.Equal("BERGS", query(_context, "BERGS").First().CustomerID);

            using (var context = CreateContext())
            {
                Assert.Equal("BLAUS", query(context, "BLAUS").First().CustomerID);
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        public void Dispose() => _context.Dispose();

        private string RemoveNewLines(string message)
            => message.Replace("\n", "").Replace("\r", "");
    }
}
