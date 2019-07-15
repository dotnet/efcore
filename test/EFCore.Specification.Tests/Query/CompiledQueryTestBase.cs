// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class CompiledQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected CompiledQueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void DbSet_query()
        {
            var query = EF.CompileQuery((NorthwindContext context) => context.Customers);

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).Count());
            }

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual void DbSet_query_first()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Customers.OrderBy(c => c.CustomerID).First());

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context).CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void DbQuery_query()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var query = EF.CompileQuery((NorthwindContext context) => context.CustomerQueries);
#pragma warning restore CS0618 // Type or member is obsolete

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).Count());
            }

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual void DbQuery_query_first()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.CustomerQueries.OrderBy(c => c.CompanyName).First());

            using (var context = CreateContext())
            {
                Assert.Equal("Alfreds Futterkiste", query(context).CompanyName);
            }
        }

        [ConditionalFact]
        public virtual void Query_ending_with_include()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context)
                    => context.Customers.Include(c => c.Orders));

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).ToList().Count);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual void Untyped_context()
        {
            var query = EF.CompileQuery((DbContext context) => context.Set<Customer>());

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).Count());
            }

            using (var context = CreateContext())
            {
                Assert.Equal(91, query(context).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_single_parameter()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context, "ALFKI").First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, "ANATR").First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_single_parameter_with_include()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID).Include(c => c.Orders));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context, "ALFKI").First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, "ANATR").First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void First_query_with_single_parameter()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string customerID)
                    => context.Customers.First(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context, "ALFKI").CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, "ANATR").CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_two_parameters()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, object _, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context, null, "ALFKI").First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, null, "ANATR").First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_three_parameters()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, object _, int __, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context, null, 1, "ALFKI").First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, null, 1, "ANATR").First().CustomerID);
            }
        }

        [ConditionalFact(Skip = "Issue #14935. Cannot eval 'where ([c].CustomerID == __args[0])'")]
        public virtual void Query_with_array_parameter()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string[] args)
                    => context.Customers.Where(c => c.CustomerID == args[0]));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context, new[] { "ALFKI" }).First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, new[] { "ANATR" }).First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_contains()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string[] args)
                    => context.Customers.Where(c => args.Contains(c.CustomerID)));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context, new[] { "ALFKI" }).First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, new[] { "ANATR" }).First().CustomerID);
            }
        }

        [ConditionalFact(Skip = "Test does not pass. See issue#7016")]
        public virtual void Multiple_queries()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context)
                    => context.Customers.OrderBy(c => c.CustomerID).Select(c => c.CustomerID).FirstOrDefault()
                       + context.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).FirstOrDefault());

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context));
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context));
            }
        }

        [ConditionalFact]
        public virtual void Query_with_closure()
        {
            var customerID = "ALFKI";

            var query = EF.CompileQuery(
                (NorthwindContext context)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context).First().CustomerID);
            }

            customerID = "ANATR";

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", query(context).First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_closure_null()
        {
            string customerID = null;

            var query = EF.CompileQuery(
                (NorthwindContext context)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Null(query(context).FirstOrDefault());
            }
        }

        [ConditionalFact]
        public virtual async Task DbSet_query_async()
        {
            var query = EF.CompileAsyncQuery((NorthwindContext context) => context.Customers);

            using (var context = CreateContext())
            {
                Assert.Equal(91, (await query(context).ToListAsync()).Count);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(91, (await query(context).ToListAsync()).Count);
            }
        }

        [ConditionalFact]
        public virtual async Task DbSet_query_first_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context)
                    => context.Customers.OrderBy(c => c.CustomerID).First());

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context)).CustomerID);
            }
        }

        [ConditionalFact]
        public virtual async Task DbQuery_query_async()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var query = EF.CompileAsyncQuery((NorthwindContext context) => context.CustomerQueries);
#pragma warning restore CS0618 // Type or member is obsolete

            using (var context = CreateContext())
            {
                Assert.Equal(91, (await query(context).ToListAsync()).Count);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(91, (await query(context).ToListAsync()).Count);
            }
        }

        [ConditionalFact]
        public virtual async Task DbQuery_query_first_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context)
                    => context.CustomerQueries.OrderBy(c => c.CompanyName).First());

            using (var context = CreateContext())
            {
                Assert.Equal("Alfreds Futterkiste", (await query(context)).CompanyName);
            }
        }

        [ConditionalFact]
        public virtual async Task Untyped_context_async()
        {
            var query = EF.CompileAsyncQuery((DbContext context) => context.Set<Customer>());

            using (var context = CreateContext())
            {
                Assert.Equal(91, (await query(context).ToListAsync()).Count);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(91, (await query(context).ToListAsync()).Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Query_with_single_parameter_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context, "ALFKI").ToListAsync()).First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", (await query(context, "ANATR").ToListAsync()).First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual async Task First_query_with_single_parameter_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context, string customerID)
                    => context.Customers.First(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context, "ALFKI")).CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", (await query(context, "ANATR")).CustomerID);
            }
        }

        [ConditionalFact]
        public virtual async Task First_query_with_cancellation_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context, string customerID, CancellationToken ct)
                    => context.Customers.First(c => c.CustomerID == customerID));

            var cancellationToken = default(CancellationToken);

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context, "ALFKI", cancellationToken)).CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", (await query(context, "ANATR", cancellationToken)).CustomerID);
            }
        }

        [ConditionalFact]
        public virtual async Task Query_with_two_parameters_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context, object _, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context, null, "ALFKI").ToListAsync()).First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", (await query(context, null, "ANATR").ToListAsync()).First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual async Task Query_with_three_parameters_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context, object _, int __, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context, null, 1, "ALFKI").ToListAsync()).First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", (await query(context, null, 1, "ANATR").ToListAsync()).First().CustomerID);
            }
        }

        [ConditionalFact(Skip = "Issue #14935. Cannot eval 'where ([c].CustomerID == __args[0])'")]
        public virtual async Task Query_with_array_parameter_async()
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context, string[] args)
                    => context.Customers.Where(c => c.CustomerID == args[0]));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context, new[] { "ALFKI" }).ToListAsync()).First().CustomerID);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", (await query(context, new[] { "ANATR" }).ToListAsync()).First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual async Task Query_with_closure_async()
        {
            var customerID = "ALFKI";

            var query = EF.CompileAsyncQuery(
                (NorthwindContext context)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context).ToListAsync()).First().CustomerID);
            }

            customerID = "ANATR";

            using (var context = CreateContext())
            {
                Assert.Equal("ALFKI", (await query(context).ToListAsync()).First().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual async Task Query_with_closure_async_null()
        {
            string customerID = null;

            var query = EF.CompileAsyncQuery(
                (NorthwindContext context)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using (var context = CreateContext())
            {
                Assert.Empty(await query(context).ToListAsync());
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
