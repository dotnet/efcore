// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        public virtual Task Union(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Union(cs.Where(c => c.City == "London")),
                entryCount: 7);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat(bool isAsync)
            => AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "Berlin")
                    .Concat(cs.Where(c => c.City == "London")),
                entryCount: 7);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect(bool isAsync)
        {
            return AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "London")
                    .Intersect(cs.Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except(bool isAsync)
        {
            return AssertQuery<Customer>(isAsync, cs => cs
                    .Where(c => c.City == "London")
                    .Except(cs.Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 5);
        }

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
                    .Take(1)
                    .Union(cs.Where(c => c.City == "Mannheim"))
                    .Take(1),
                entryCount: 666);

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
        public virtual Task Select_Union_unrelated(bool isAsync)
            => AssertQuery<Customer, Product>(isAsync, (cs, pd) => cs
                    .Select(c => c.ContactName)
                    .Union(pd.Select(p => p.ProductName))
                    .Where(x => x.StartsWith("C"))
                    .OrderBy(x => x),
                assertOrder: true);

        [ConditionalTheory]
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
    }
}
