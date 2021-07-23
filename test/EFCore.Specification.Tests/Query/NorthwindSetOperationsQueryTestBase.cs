// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindSetOperationsQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindSetOperationsQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Concat(ss.Set<Customer>().Where(c => c.City == "London")),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Concat(ss.Set<Customer>().Where(s => s.City == "Berlin"))
                    .Concat(ss.Set<Customer>().Where(e => e.City == "London")),
                entryCount: 12);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_non_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Select(c => c.CustomerID)
                    .Concat(
                        ss.Set<Customer>()
                            .Where(s => s.ContactTitle == "Owner")
                            .Select(c => c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "London")
                    .Except(ss.Set<Customer>().Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_simple_followed_by_projecting_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>()
                    .Except(ss.Set<Customer>())
                    .Select(e => 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(s => s.ContactTitle == "Owner")
                    .Except(ss.Set<Customer>().Where(s => s.City == "México D.F."))
                    .Except(ss.Set<Customer>().Where(e => e.City == "Seattle")),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_non_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(s => s.ContactTitle == "Owner")
                    .Select(c => c.CustomerID)
                    .Except(
                        ss.Set<Customer>()
                            .Where(c => c.City == "México D.F.")
                            .Select(c => c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "London")
                    .Intersect(ss.Set<Customer>().Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Intersect(ss.Set<Customer>().Where(s => s.ContactTitle == "Owner"))
                    .Intersect(ss.Set<Customer>().Where(e => e.Fax != null)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect_non_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Select(c => c.CustomerID)
                    .Intersect(
                        ss.Set<Customer>()
                            .Where(s => s.ContactTitle == "Owner")
                            .Select(c => c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London")),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(s => s.ContactTitle == "Owner")
                    .Union(ss.Set<Customer>().Where(s => s.City == "México D.F."))
                    .Union(ss.Set<Customer>().Where(e => e.City == "London")),
                entryCount: 25);
        }

        [ConditionalTheory(Skip = "Issue#16365")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_non_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(s => s.ContactTitle == "Owner")
                    .Select(c => c.CustomerID)
                    .Union(
                        ss.Set<Customer>()
                            .Where(c => c.City == "México D.F.")
                            .Select(c => c.CustomerID)));
        }

        // OrderBy, Skip and Take are typically supported on the set operation itself (no need for query pushdown)
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_OrderBy_Skip_Take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .OrderBy(c => c.ContactName)
                    .Skip(1)
                    .Take(1),
                entryCount: 1,
                assertOrder: true);
        }

        // Should cause pushdown into a subquery
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Where(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .Where(c => c.ContactName.Contains("Thomas")), // pushdown
                entryCount: 1);
        }

        // Should cause pushdown into a subquery, keeping the ordering, offset and limit inside the subquery
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Skip_Take_OrderBy_ThenBy_Where(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .OrderBy(c => c.Region)
                    .ThenBy(c => c.City)
                    .Skip(0) // prevent pushdown from removing OrderBy
                    .Where(c => c.ContactName.Contains("Thomas")), // pushdown
                entryCount: 1);
        }

        // Nested set operation with same operation type - no parentheses are needed.
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Union(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .Union(ss.Set<Customer>().Where(c => c.City == "Mannheim")),
                entryCount: 8);
        }

        // Nested set operation but with different operation type. On SqlServer and PostgreSQL INTERSECT binds
        // more tightly than UNION/EXCEPT, so parentheses are needed.
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Intersect(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .Intersect(ss.Set<Customer>().Where(c => c.ContactName.Contains("Thomas"))),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Take_Union_Take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .OrderBy(c => c.CustomerID)
                    .Take(1)
                    .Union(ss.Set<Customer>().Where(c => c.City == "Mannheim"))
                    .Take(1)
                    .OrderBy(c => c.CustomerID),
                entryCount: 1, assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Union(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Select(c => c.Address)
                    .Union(
                        ss.Set<Customer>()
                            .Where(c => c.City == "London")
                            .Select(c => c.Address)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .Select(c => c.Address)
                    .Where(a => a.Contains("Hanover")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Select_scalar(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Except(ss.Set<Customer>())
                    .Select(c => (object)1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_with_anonymous_type_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.CompanyName.StartsWith("A"))
                    .Union(ss.Set<Customer>().Where(c => c.CompanyName.StartsWith("B")))
                    .Select(c => new CustomerDeets { Id = c.CustomerID }));
        }

        public class CustomerDeets
        {
            public string Id { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return ReferenceEquals(this, obj)
                    ? true
                    : obj.GetType() == GetType()
                    && string.Equals(Id, ((CustomerDeets)obj).Id);
            }

            public override int GetHashCode()
                => Id != null ? Id.GetHashCode() : 0;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Union_unrelated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => c.ContactName)
                    .Union(ss.Set<Product>().Select(p => p.ProductName))
                    .Where(x => x.StartsWith("C"))
                    .OrderBy(x => x),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Union_different_fields_in_anonymous_with_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Select(c => new { Foo = c.City, Customer = c }) // Foo is City
                    .Union(
                        ss.Set<Customer>()
                            .Where(c => c.City == "London")
                            .Select(c => new { Foo = c.Region, Customer = c })) // Foo is Region
                    .OrderBy(x => x.Foo)
                    .Skip(1)
                    .Take(10)
                    .Where(x => x.Foo == "Berlin"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_Include(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Union(ss.Set<Customer>().Where(c => c.City == "London"))
                    .Include(c => c.Orders),
                entryCount: 59);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_Union(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .Include(c => c.Orders)
                    .Union(
                        ss.Set<Customer>()
                            .Where(c => c.City == "London")
                            .Include(c => c.Orders)),
                entryCount: 59);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Except_reference_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Select(o => o.Customer)
                    .Except(
                        ss.Set<Order>()
                            .Where(o => o.CustomerID == "ALFKI")
                            .Select(o => o.Customer)),
                entryCount: 88);
        }

        [ConditionalFact]
        public virtual void Include_Union_only_on_one_side_throws()
        {
            using var ctx = CreateContext();
            Assert.Throws<InvalidOperationException>(
                () =>
                    ctx.Customers
                        .Where(c => c.City == "Berlin")
                        .Include(c => c.Orders)
                        .Union(ctx.Customers.Where(c => c.City == "London"))
                        .ToList());

            Assert.Throws<InvalidOperationException>(
                () =>
                    ctx.Customers
                        .Where(c => c.City == "Berlin")
                        .Union(
                            ctx.Customers
                                .Where(c => c.City == "London")
                                .Include(c => c.Orders))
                        .ToList());
        }

        [ConditionalFact]
        public virtual void Include_Union_different_includes_throws()
        {
            using var ctx = CreateContext();
            Assert.Throws<InvalidOperationException>(
                () =>
                    ctx.Customers
                        .Where(c => c.City == "Berlin")
                        .Include(c => c.Orders)
                        .Union(
                            ctx.Customers
                                .Where(c => c.City == "London")
                                .Include(c => c.Orders)
                                .ThenInclude(o => o.OrderDetails))
                        .ToList());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SubSelect_Union(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => new { Customer = c, Orders = c.Orders.Count })
                    .Union(
                        ss.Set<Customer>()
                            .Select(c => new { Customer = c, Orders = c.Orders.Count })),
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "#16243")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_eval_Union_FirstOrDefault(bool async)
            => AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => ClientSideMethod(c))
                    .Union(ss.Set<Customer>()));

        private static Customer ClientSideMethod(Customer c)
            => c;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Select_Union(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Berlin")
                    .GroupBy(c => c.CustomerID)
                    .Select(g => new { CustomerID = g.Key, Count = g.Count() })
                    .Union(
                        ss.Set<Customer>()
                            .Where(c => c.City == "London")
                            .GroupBy(c => c.CustomerID)
                            .Select(g => new { CustomerID = g.Key, Count = g.Count() })));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_over_columns_with_different_nullability(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => "NonNullableConstant")
                    .Concat(
                        ss.Set<Customer>()
                            .Select(c => (string)null)));
        }

        [ConditionalTheory]
#pragma warning disable xUnit1016 // MemberData must reference a public member
        [MemberData(nameof(GetSetOperandTestCases))]
#pragma warning restore xUnit1016 // MemberData must reference a public member
        public virtual Task Union_over_different_projection_types(bool async, string leftType, string rightType)
        {
            var (left, right) = (ExpressionGenerator(leftType), ExpressionGenerator(rightType));
            return AssertQuery(async, ss => left(ss.Set<Order>()).Union(right(ss.Set<Order>())));

            static Func<IQueryable<Order>, IQueryable<object>> ExpressionGenerator(string expressionType)
            {
                switch (expressionType)
                {
                    case "Column":
                        return os => os.Select(o => (object)o.OrderID);
                    case "Function":
                        return os => os
                            .GroupBy(o => o.OrderID)
                            .Select(g => (object)g.Count());
                    case "Constant":
                        return os => os.Select(o => (object)8);
                    case "Unary":
                        return os => os.Select(o => (object)-o.OrderID);
                    case "Binary":
                        return os => os.Select(o => (object)(o.OrderID + 1));
                    case "ScalarSubquery":
                        return os => os.Select(o => (object)o.OrderDetails.Count());
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private static IEnumerable<object[]> GetSetOperandTestCases()
            => from async in new[] { true, false }
               from leftType in _supportedOperandExpressionType
               from rightType in _supportedOperandExpressionType
               select new object[] { async, leftType, rightType };

        // ReSharper disable once StaticMemberInGenericType
        private static readonly string[] _supportedOperandExpressionType =
        {
            "Column", "Function", "Constant", "Unary", "Binary", "ScalarSubquery"
        };

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_Union(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .OrderBy(c => c.ContactName)
                    .Take(1)
                    .Union(
                        ss.Set<Customer>()
                            .OrderBy(c => c.ContactName)
                            .Take(1)),
                entryCount: 1,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_projection_after_set_operation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == "Seatte")
                    .Union(ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")))
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.Orders
                    }),
                elementSorter: c => c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.CustomerID, a.CustomerID);
                    AssertCollection(e.Orders, a.Orders);
                },
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_projection_after_set_operation_fails_if_distinct(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == "Seatte")
                    .Concat(ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")))
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.Orders
                    }),
                elementSorter: c => c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.CustomerID, a.CustomerID);
                    AssertCollection(e.Orders, a.Orders);
                },
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_projection_before_set_operation_fails(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.City == "Seatte")
                    .Select(c => new
                    {
                        c.Orders
                    })
                    .Union(ss.Set<Customer>()
                        .Where(c => c.CustomerID.StartsWith("F"))
                        .Select(c => new
                        {
                            c.Orders
                        })),
                elementSorter: a => a.Orders.FirstOrDefault().Maybe(e => e.CustomerID),
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.Orders, a.Orders);
                },
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_with_one_side_being_GroupBy_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(c => c.Customer.City == "Seatte")
                    .Select(c => new
                    {
                        c.OrderDate
                    })
                    .Union(ss.Set<Order>()
                        .GroupBy(e => e.CustomerID)
                        .Select(g => new
                        {
                            OrderDate = g.Max(e => e.OrderDate)
                        })),
                elementSorter: a => a.OrderDate,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.OrderDate, a.OrderDate);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_on_entity_with_correlated_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.Customer.City == "Seatte").Select(c => c.Customer)
                    .Union(ss.Set<Order>().Where(o => o.OrderID < 10250).Select(c => c.Customer))
                    .OrderBy(c => c.CustomerID)
                    .Select(c => c.Orders),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e, a);
                },
                entryCount: 11);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_on_entity_plus_other_column_with_correlated_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.Customer.City == "Seatte").Select(c => new { c.Customer, c.OrderDate })
                    .Union(ss.Set<Order>().Where(o => o.OrderID < 10250).Select(c => new { c.Customer, c.OrderDate }))
                    .OrderBy(c => c.Customer.CustomerID)
                    .Select(c => new { c.OrderDate, Orders = ss.Set<Order>().Where(o => o.CustomerID == c.Customer.CustomerID).ToList() }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.OrderDate, a.OrderDate);
                    AssertCollection(e.Orders, a.Orders);
                },
                entryCount: 11);
        }
    }
}
