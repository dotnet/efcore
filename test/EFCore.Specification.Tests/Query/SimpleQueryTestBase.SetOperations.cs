// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Concat(cs.Where(c => c.City == "London")),
                entryCount: 7);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_nested(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "México D.F.")
                    .Concat(cs.Where(s => s.City == "Berlin"))
                    .Concat(cs.Where(e => e.City == "London")),
                entryCount: 12);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_non_entity(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "México D.F.")
                    .Select(c => c.CustomerID)
                    .Concat(cs
                        .Where(s => s.ContactTitle == "Owner")
                        .Select(c => c.CustomerID)));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "London")
                    .Except(cs.Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 5);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_simple_followed_by_projecting_constant(bool isAsync)
            => AssertQueryScalar<Customer>(isAsync, cs => cs
                    .Except(cs)
                    .Select(e => 1));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_nested(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(s => s.ContactTitle == "Owner")
                    .Except(cs.Where(s => s.City == "México D.F."))
                    .Except(cs.Where(e => e.City == "Seattle")),
                entryCount: 13);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_non_entity(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(s => s.ContactTitle == "Owner")
                    .Select(c => c.CustomerID)
                    .Except(
                        cs
                            .Where(c => c.City == "México D.F.")
                            .Select(c => c.CustomerID)));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "London")
                    .Intersect(cs.Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect_nested(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "México D.F.")
                    .Intersect(cs.Where(s => s.ContactTitle == "Owner"))
                    .Intersect(cs.Where(e => e.Fax != null)),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect_non_entity(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                .Where(c => c.City == "México D.F.")
                .Select(c => c.CustomerID)
                .Intersect(cs
                        .Where(s => s.ContactTitle == "Owner")
                        .Select(c => c.CustomerID)));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London")),
                entryCount: 7);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_nested(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(s => s.City == "México D.F."))
                    .Union(cs.Where(e => e.City == "London")),
                entryCount: 25);

        [ConditionalTheory(Skip = "Issue#16365")]
        [MemberData(nameof(IsAsyncData))]
        public virtual void Union_non_entity(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(s => s.ContactTitle == "Owner")
                    .Select(c => c.CustomerID)
                    .Union(cs
                        .Where(c => c.City == "México D.F.")
                        .Select(c => c.CustomerID)));

        // OrderBy, Skip and Take are typically supported on the set operation itself (no need for query pushdown)
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_OrderBy_Skip_Take(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London"))
                    .OrderBy(c => c.ContactName)
                    .Skip(1)
                    .Take(1),
                entryCount: 1,
                assertOrder: true);

        // Should cause pushdown into a subquery
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Where(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London"))
                    .Where(c => c.ContactName.Contains("Thomas")),  // pushdown
                entryCount: 1);

        // Should cause pushdown into a subquery, keeping the ordering, offset and limit inside the subquery
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Skip_Take_OrderBy_ThenBy_Where(bool isAsync)
            => AssertQuery<Customer>(
                isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London"))
                    .OrderBy(c => c.Region)
                    .ThenBy(c => c.City)
                    .Skip(0)  // prevent pushdown from removing OrderBy
                    .Where(c => c.ContactName.Contains("Thomas")),  // pushdown
                entryCount: 1);

        // Nested set operation with same operation type - no parentheses are needed.
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Union(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London"))
                    .Union(cs.Where(c => c.City == "Mannheim")),
                entryCount: 8);

        // Nested set operation but with different operation type. On SqlServer and PostgreSQL INTERSECT binds
        // more tightly than UNION/EXCEPT, so parentheses are needed.
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Intersect(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London"))
                    .Intersect(cs.Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Take_Union_Take(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London"))
                    .OrderBy(c => c.CustomerID)
                    .Take(1)
                    .Union(cs.Where(c => c.City == "Mannheim"))
                    .Take(1)
                    .OrderBy(c => c.CustomerID),
                entryCount: 1, assertOrder: true);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Union(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Select(c => c.Address)
                    .Union(cs
                        .Where(c => c.City == "London")
                        .Select(c => c.Address)));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Select(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                .Where(c => c.City == "Berlin")
                .Union(cs.Where(c => c.City == "London"))
                .Select(c => c.Address)
                .Where(a => a.Contains("Hanover")));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_with_anonymous_type_projection(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                .Where(c => c.CompanyName.StartsWith("A"))
                .Union(cs.Where(c => c.CompanyName.StartsWith("B")))
                .Select(c => new CustomerDeets { Id = c.CustomerID }));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Union_unrelated(bool isAsync)
            => AssertQuery<Customer, Product>(isAsync, (cs, pd) => cs
                    .Select(c => c.ContactName)
                    .Union(pd.Select(p => p.ProductName))
                    .Where(x => x.StartsWith("C"))
                    .OrderBy(x => x),
                assertOrder: true);

        [ConditionalTheory(Skip = "Very similar to #16298")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Union_different_fields_in_anonymous_with_subquery(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                .Where(c => c.City == "Berlin")
                .Select(c => new { Foo = c.City, Customer = c })   // Foo is City
                .Union(cs
                    .Where(c => c.City == "London")
                    .Select(c => new { Foo = c.Region, Customer = c }))  // Foo is Region
                .OrderBy(x => x.Foo)
                .Skip(1)
                .Take(10)
                .Where(x => x.Foo == "Berlin"),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Include(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                .Where(c => c.City == "Berlin")
                .Union(cs.Where(c => c.City == "London"))
                .Include(c => c.Orders),
                entryCount: 59);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_Union(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                .Where(c => c.City == "Berlin")
                .Include(c => c.Orders)
                .Union(cs
                    .Where(c => c.City == "London")
                    .Include(c => c.Orders)),
                entryCount: 59);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Except_reference_projection(bool isAsync)
            => AssertQuery<Order>(isAsync, od => od
                .Select(o => o.Customer)
                .Except(od
                        .Where(o => o.CustomerID == "ALFKI")
                        .Select(o => o.Customer)),
                entryCount: 88);

        [ConditionalFact]
        public virtual void Include_Union_only_on_one_side_throws()
        {
            using (var ctx = CreateContext())
            {
                Assert.Throws<NotSupportedException>(() =>
                    ctx.Customers
                        .Where(c => c.City == "Berlin")
                        .Include(c => c.Orders)
                        .Union(ctx.Customers.Where(c => c.City == "London"))
                        .ToList());

                Assert.Throws<NotSupportedException>(() =>
                    ctx.Customers
                        .Where(c => c.City == "Berlin")
                        .Union(ctx.Customers
                            .Where(c => c.City == "London")
                            .Include(c => c.Orders))
                        .ToList());
            }
        }

        [ConditionalFact]
        public virtual void Include_Union_different_includes_throws()
        {
            using (var ctx = CreateContext())
            {
                Assert.Throws<NotSupportedException>(() =>
                    ctx.Customers
                        .Where(c => c.City == "Berlin")
                        .Include(c => c.Orders)
                        .Union(ctx.Customers
                            .Where(c => c.City == "London")
                            .Include(c => c.Orders)
                            .ThenInclude(o => o.OrderDetails))
                        .ToList());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SubSelect_Union(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Select(c => new { Customer = c, Orders = c.Orders.Count })
                    .Union(cs
                        .Select(c => new { Customer = c, Orders = c.Orders.Count })
                    ),
                entryCount: 91);

        [ConditionalTheory(Skip = "#16243")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_eval_Union_FirstOrDefault(bool isAsync)
            => AssertFirstOrDefault<Customer>(isAsync, cs => cs
                .Select(c => ClientSideMethod(c))
                .Union(cs));

        private static Customer ClientSideMethod(Customer c) => c;
    }
}
