// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindGroupByQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindGroupByQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        #region GroupByProperty

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Average(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory(Skip = "issue #18923")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Average_with_group_enumerable_projected(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Where(o => o.Customer.City != "London")
                    .GroupBy(o => o.CustomerID, (k, es) => new { k, es })
                    .Select(g => g.es.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_LongCount(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Max(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Sum(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => EF.Property<string>(o, "CustomerID")).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => (e.Min, e.Max));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Average(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new { g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Count(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => EF.Property<string>(o, "CustomerID")).Select(
                    g =>
                        new { g.Key, Count = g.Count() }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_LongCount(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new { g.Key, LongCount = g.LongCount() }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Max(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new { g.Key, Max = g.Max(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Min(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new { g.Key, Min = g.Min(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new { g.Key, Sum = g.Sum(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => EF.Property<string>(o, "CustomerID")).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_key_multiple_times_and_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            Key1 = g.Key,
                            Key2 = g.Key,
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Key1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_with_constant(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => new { Name = "CustomerID", Value = o.CustomerID }).Select(
                    g =>
                        new { g.Key, Count = g.Count() }),
                e => e.Key.Value);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_projecting_conditional_expression(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.OrderDate).Select(
                    g =>
                        new { g.Key, SomeValue = g.Count() == 0 ? 1 : g.Sum(o => o.OrderID % 2 == 0 ? 1 : 0) / g.Count() }),
                e => (e.Key, e.SomeValue));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.OrderDate).Select(
                    g =>
                        new { Key = g.Key == null ? "is null" : "is not null", Sum = g.Sum(o => o.OrderID) }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_group_key_access_thru_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .GroupBy(od => od.Order.CustomerID)
                    .Select(g => new { g.Key, Aggregate = g.Sum(od => od.OrderID) }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_group_key_access_thru_nested_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .GroupBy(od => od.Order.Customer.Country)
                    .Select(g => new { g.Key, Aggregate = g.Sum(od => od.OrderID) }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task GroupBy_with_grouping_key_using_Like(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<Order>()
                .GroupBy(o => EF.Functions.Like(o.CustomerID, "A%"))
                .Select(g => new { g.Key, Count = g.Count() });

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(800, result.Single(t => !t.Key).Count);
            Assert.Equal(30, result.Single(t => t.Key).Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_grouping_key_DateTime_Day(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.OrderDate.Value.Day)
                    .Select(g => new { g.Key, Count = g.Count() }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_cast_inside_grouping_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Count = g.Count(),
                            Sum = g.Sum(o => (long)o.OrderID)
                        }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_with_arithmetic_operation_inside_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID + o.CustomerID.Length) }),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Key, a.Key);
                    Assert.Equal(e.Sum, a.Sum);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_with_projection_into_DTO(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.OrderID).Select(x => new LongIntDto { Id = x.Key, Count = x.Count() }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Count, a.Count);
                });
        }

        private class LongIntDto
        {
            public long Id { get; set; }
            public int Count { get; set; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_select_function_groupby_followed_by_another_select_with_aggregates(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .Select(
                        o => new
                        {
                            o.CustomerID,
                            Age = 2020 - o.OrderDate.Value.Year,
                            o.OrderID
                        })
                    .GroupBy(x => x.CustomerID)
                    .Select(
                        x => new
                        {
                            x.Key,
                            Sum1 = x.Sum(y => y.Age <= 30 ? y.OrderID : 0),
                            Sum2 = x.Sum(y => y.Age > 30 && y.Age <= 60 ? y.OrderID : 0)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_column_project_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Select(e => 42));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_plus_key_in_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => (from o in ss.Set<Order>()
                       join c in ss.Set<Customer>() on o.CustomerID equals c.CustomerID into grouping
                       from c in grouping.DefaultIfEmpty()
                       select o)
                    .GroupBy(o => o.OrderID)
                    .Select(
                        g => new { Value = g.Key + g.Key, Average = g.Average(o => o.OrderID) }));
        }

        #endregion

        #region GroupByAnonymousAggregate

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Average(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID }).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_LongCount(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID }).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Max(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Sum(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_with_alias_Select_Key_Sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { Id = o.CustomerID }).Select(
                    g =>
                        new { Key = g.Key.Id, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Average(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_LongCount(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Max(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Sum(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Average(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new { g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Count(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new { g.Key, Count = g.Count() }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_LongCount(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new { g.Key, LongCount = g.LongCount() }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Max(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new { g.Key, Max = g.Max(o => o.OrderID) }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Min(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new { g.Key, Min = g.Min(o => o.OrderID) }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new { g.Key, Sum = g.Sum(o => o.OrderID) }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key.CustomerID,
                            g.Key.EmployeeID,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Dto_as_key_Select_Sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new NominalType { CustomerID = o.CustomerID, EmployeeID = o.EmployeeID }).Select(
                    g =>
                        new { Sum = g.Sum(o => o.OrderID), g.Key }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Dto_as_element_selector_Select_Sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                        o => o.CustomerID,
                        o => new NominalType { CustomerID = o.CustomerID, EmployeeID = o.EmployeeID })
                    .Select(
                        g =>
                            new { Sum = g.Sum(o => o.EmployeeID), g.Key }));
        }

        protected class NominalType
        {
            public string CustomerID { get; set; }
            public uint? EmployeeID { get; set; }

            public override bool Equals(object obj)
                => obj is null
                    ? false
                    : ReferenceEquals(this, obj)
                        ? true
                        : obj.GetType() == GetType() && Equals((NominalType)obj);

            public override int GetHashCode()
                => 0;

            private bool Equals(NominalType other)
                => string.Equals(CustomerID, other.CustomerID)
                    && EmployeeID == other.EmployeeID;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new CompositeDto
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            CustomerId = g.Key.CustomerID,
                            EmployeeId = g.Key.EmployeeID,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.CustomerId + " " + e.EmployeeId);
        }

        protected class CompositeDto
        {
            public int Sum { get; set; }
            public int Min { get; set; }
            public int Max { get; set; }
            public double Avg { get; set; }
            public string CustomerId { get; set; }
            public uint? EmployeeId { get; set; }

            public override bool Equals(object obj)
                => obj != null && (ReferenceEquals(this, obj) || (obj is CompositeDto dto && Equals(dto)));

            public override int GetHashCode()
                => 0;

            private bool Equals(CompositeDto other)
                => Sum == other.Sum
                    && Min == other.Min
                    && Max == other.Max
                    && Avg == other.Avg
                    && EmployeeId == other.EmployeeId
                    && string.Equals(CustomerId, other.CustomerId);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key.CustomerID,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => 2).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => 2, o => new { o.OrderID, o.OrderDate }).Select(
                    g =>
                        new { Sum = g.Sum(o => o.OrderID) }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => 2, o => new { o.OrderID }).Select(
                    g =>
                        new { Sum = g.Sum(o => o.OrderID) }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => 2, o => new
                    {
                        o.OrderID,
                        o.OrderDate,
                        o.CustomerID
                    }).Select(
                    g =>
                        new { Sum = g.Sum(o => o.OrderID) }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID > 10500).GroupBy(o => 2).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Random = g.Key,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => 2, o => o.OrderID).Select(
                    g =>
                        new { Sum = g.Sum(), g.Key }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            var a = 2;

            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => a).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key,
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_with_element_selector_Select_Sum(bool async)
        {
            var a = 2;

            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => a, o => new { o.OrderID, o.OrderDate }).Select(
                    g =>
                        new { Sum = g.Sum(o => o.OrderID) }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_with_element_selector_Select_Sum2(bool async)
        {
            var a = 2;

            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => a, o => new { o.OrderID }).Select(
                    g =>
                        new { Sum = g.Sum(o => o.OrderID) }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_with_element_selector_Select_Sum3(bool async)
        {
            var a = 2;

            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => a, o => new
                    {
                        o.OrderID,
                        o.OrderDate,
                        o.CustomerID
                    }).Select(
                    g =>
                        new { Sum = g.Sum(o => o.OrderID) }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            var a = 2;

            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => a, o => o.OrderID).Select(
                    g =>
                        new { Sum = g.Sum(), g.Key }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_key_type_mismatch_with_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => new { I0 = (int?)o.OrderDate.Value.Year })
                    .OrderBy(g => g.Key.I0)
                    .Select(g => new { I0 = g.Count(), I1 = g.Key.I0 }),
                elementSorter: a => a.I1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_based_on_renamed_property_simple(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .GroupBy(g => new { Renamed = g.City })
                    .Select(x => new { x.Key, Count = x.Count() }),
                elementSorter: e => e.Key.Renamed);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_based_on_renamed_property_complex(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(x => new { Renamed = x.City, x.CustomerID })
                    .Distinct()
                    .GroupBy(g => g.Renamed)
                    .Select(x => new { x.Key, Count = x.Count() }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_groupby_anonymous_orderby_anonymous_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                      group new { c, o } by new { c.CustomerID, o.OrderDate }
                      into grouping
                      orderby grouping.Key.OrderDate
                      select new { grouping.Key.CustomerID, grouping.Key.OrderDate });
        }

        #endregion

        #region GroupByWithElementSelectorAggregate

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Average(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Average()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_LongCount(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Max(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Max()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Min()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Sum(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(),
                            Min = g.Min(),
                            Max = g.Max(),
                            Avg = g.Average()
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Average(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_LongCount(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Max(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Sum(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.EmployeeID),
                            Max = g.Max(o => o.EmployeeID),
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Sum + " " + e.Avg);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_element_selector_complex_aggregate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => new { o.OrderID })
                    .Select(g => g.Sum(e => e.OrderID + 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_element_selector_complex_aggregate2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => new { o.OrderID, o.OrderDate })
                    .Select(g => g.Sum(e => e.OrderID + 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_element_selector_complex_aggregate3(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID)
                    .Select(g => g.Sum(e => e + 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_element_selector_complex_aggregate4(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID, o => o.OrderID + 1)
                    .Select(g => g.Sum(e => e)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Element_selector_with_case_block_repeated_inside_another_case_block_in_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => from order in ss.Set<Order>()
                      group new
                          {
                              IsAlfki = order.CustomerID == "ALFKI", OrderId = order.OrderID > 1000 ? order.OrderID : -order.OrderID
                          } by
                          new { order.OrderID }
                      into g
                      select new { g.Key.OrderID, Aggregate = g.Sum(s => s.IsAlfki ? s.OrderId : -s.OrderId) });
        }

        #endregion

        #region GroupByAfterComposition

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_empty_key_Aggregate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => new { })
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_empty_key_Aggregate_Key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => new { })
                    .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_GroupBy_Aggregate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_GroupBy_Aggregate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID)
                    .Skip(80)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_GroupBy_Aggregate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID)
                    .Take(500)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_Take_GroupBy_Aggregate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID)
                    .Skip(80)
                    .Take(500)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Distinct()
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_projection_Distinct_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Select(o => new { o.OrderID, o.EmployeeID })
                    .Distinct()
                    .GroupBy(o => o.EmployeeID)
                    .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().SelectMany(c => c.Orders)
                    .GroupBy(o => o.EmployeeID)
                    .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from o in ss.Set<Order>()
                     join c in ss.Set<Customer>() on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_required_navigation_member_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().GroupBy(od => od.Order.CustomerID)
                    .Select(g => new { CustomerId = g.Key, Count = g.Count() }),
                e => e.CustomerId);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_complex_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from o in ss.Set<Order>().Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                     join c in ss.Set<Customer>().Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO")
                             .OrderBy(c => c.City).Skip(10).Take(50)
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(
                        g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from c in ss.Set<Customer>()
                     join o in ss.Set<Order>()
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     where o != null
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new { g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from c in ss.Set<Customer>()
                     join o in ss.Set<Order>()
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     select c)
                    .GroupBy(c => c.CustomerID)
                    .Select(
                        g => new { g.Key, Max = g.Max(c => c.City) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_3(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from o in ss.Set<Order>()
                     join c in ss.Set<Customer>()
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping.DefaultIfEmpty()
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new { g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_4(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from c in ss.Set<Customer>()
                     join o in ss.Set<Order>()
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     select c)
                    .GroupBy(c => c.CustomerID)
                    .Select(
                        g => new { Value = g.Key, Max = g.Max(c => c.City) }),
                e => e.Value);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_5(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from o in ss.Set<Order>()
                     join c in ss.Set<Customer>()
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping.DefaultIfEmpty()
                     select o)
                    .GroupBy(o => o.OrderID)
                    .Select(
                        g => new { Value = g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Value);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_optional_navigation_member_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.Customer.Country)
                    .Select(g => new { Country = g.Key, Count = g.Count() }),
                e => e.Country);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_complex_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from c in ss.Set<Customer>().Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO")
                         .OrderBy(c => c.City).Skip(10).Take(50)
                     join o in ss.Set<Order>().Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping
                     where o.OrderID > 10300
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Self_join_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => (from o1 in ss.Set<Order>().Where(o => o.OrderID < 10400)
                       join o2 in ss.Set<Order>() on o1.OrderID equals o2.OrderID
                       group o2 by o1.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_multi_navigation_members_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().GroupBy(od => new { od.Order.CustomerID, od.Product.ProductName })
                    .Select(g => new { CompositeKey = g.Key, Count = g.Count() }),
                e => e.CompositeKey.CustomerID + " " + e.CompositeKey.ProductName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_simple_groupby(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(s => s.ContactTitle == "Owner")
                    .Union(ss.Set<Customer>().Where(c => c.City == "México D.F."))
                    .GroupBy(c => c.City)
                    .Select(g => new { g.Key, Total = g.Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_GroupBy_Aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(
                        o => new
                        {
                            A = o.CustomerID,
                            B = o.OrderDate,
                            C = o.OrderID
                        })
                    .GroupBy(e => e.A)
                    .Select(
                        g => new
                        {
                            Min = g.Min(o => o.B),
                            Max = g.Max(o => o.B),
                            Sum = g.Sum(o => o.C),
                            Avg = g.Average(o => o.C)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_principal_key_property_optimization(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.Customer.CustomerID)
                    .Select(
                        g => new { g.Key, Count = g.Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_after_anonymous_projection_and_distinct_followed_by_another_anonymous_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Select(o => new { o.CustomerID, o.OrderID })
                    .Distinct()
                    .GroupBy(x => new { x.CustomerID })
                    .Select(g => new { Key = g.Key.CustomerID, Count = g.Count() }),
                elementSorter: e => (e.Key, e.Count),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Key, a.Key);
                    Assert.Equal(e.Count, a.Count);
                });
        }

        #endregion

        #region GroupByAggregateComposition

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_OrderBy_key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID)
                    .OrderBy(o => o.Key)
                    .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_OrderBy_count(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID)
                    .OrderBy(o => o.Count())
                    .ThenBy(o => o.Key)
                    .Select(g => new { g.Key, Count = g.Count() }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_OrderBy_count_Select_sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID)
                    .OrderBy(o => o.Count())
                    .ThenBy(o => o.Key)
                    .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Contains(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(
                    o => ss.Set<Order>().GroupBy(e => e.CustomerID)
                        .Where(g => g.Count() > 30)
                        .Select(g => g.Key)
                        .Contains(o.CustomerID)),
                entryCount: 31);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Pushdown(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(e => e.CustomerID)
                    .Where(g => g.Count() > 10)
                    .Select(g => g.Key)
                    .OrderBy(t => t)
                    .Take(20)
                    .Skip(4));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Pushdown_followed_by_projecting_Length(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(e => e.CustomerID)
                    .Where(g => g.Count() > 10)
                    .Select(g => g.Key)
                    .OrderBy(t => t)
                    .Take(20)
                    .Skip(4)
                    .Select(e => e.Length));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Pushdown_followed_by_projecting_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(e => e.CustomerID)
                    .Where(g => g.Count() > 10)
                    .Select(g => g.Key)
                    .OrderBy(t => t)
                    .Take(20)
                    .Skip(4)
                    .Select(e => 5));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_filter_key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID)
                    .Where(o => o.Key == "ALFKI")
                    .Select(g => new { g.Key, c = g.Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_filter_count(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID)
                    .Where(o => o.Count() > 4)
                    .Select(g => new { g.Key, Count = g.Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_filter_count_OrderBy_count_Select_sum(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID)
                    .Where(o => o.Count() > 4)
                    .OrderBy(o => o.Count())
                    .ThenBy(o => o.Key)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Count = g.Count(),
                            Sum = g.Sum(o => o.OrderID)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Aggregate_Join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                        .Where(g => g.Count() > 5)
                        .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                    join c in ss.Set<Customer>() on a.CustomerID equals c.CustomerID
                    join o in ss.Set<Order>() on a.LastOrderID equals o.OrderID
                    select new { c, o },
                entryCount: 126);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_multijoins(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID
                    join o in ss.Set<Order>() on a.LastOrderID equals o.OrderID
                    select new { c, o },
                entryCount: 126);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_single_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID
                    select new { c, a.LastOrderID },
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_with_another_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(
                                g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                    from g in grouping
                    select new
                    {
                        c,
                        a.LastOrderID,
                        g.OrderID
                    },
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_with_left_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                    join a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(
                                g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID into grouping
                    from g in grouping.DefaultIfEmpty()
                    select new { c, LastOrderID = (int?)g.LastOrderID },
                ss =>
                    from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                    join a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(
                                g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID into grouping
                    from g in grouping.DefaultIfEmpty()
                    select new { c, LastOrderID = g != null ? g.LastOrderID : (int?)null },
                elementSorter: r => r.c.CustomerID,
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_in_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from o in ss.Set<Order>().Where(o => o.OrderID < 10400)
                    join i in (from c in ss.Set<Customer>()
                               join a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                                       .Where(g => g.Count() > 5)
                                       .Select(
                                           g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                                   on c.CustomerID equals a.CustomerID
                               select new { c, a.LastOrderID })
                        on o.CustomerID equals i.c.CustomerID
                    select new
                    {
                        o,
                        i.c,
                        i.c.CustomerID
                    },
                entryCount: 187);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_on_key(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from c in ss.Set<Customer>()
                     join a in ss.Set<Order>().GroupBy(o => o.CustomerID)
                             .Where(g => g.Count() > 5)
                             .Select(
                                 g => new { g.Key, LastOrderID = g.Max(o => o.OrderID) })
                         on c.CustomerID equals a.Key
                     select new { c, a.LastOrderID }),
                e => e.c.CustomerID,
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_result_selector(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, (k, g) =>
                        new
                        {
                            // ReSharper disable once PossibleMultipleEnumeration
                            Sum = g.Sum(o => o.OrderID),
                            // ReSharper disable once PossibleMultipleEnumeration
                            Min = g.Min(o => o.OrderID),
                            // ReSharper disable once PossibleMultipleEnumeration
                            Max = g.Max(o => o.OrderID),
                            // ReSharper disable once PossibleMultipleEnumeration
                            Avg = g.Average(o => o.OrderID)
                        }),
                e => e.Min + " " + e.Max);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Sum_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(e => 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Sum_constant_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(e => 1L)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_GroupBy_OrderBy_key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Distinct()
                    .GroupBy(o => o.CustomerID)
                    .OrderBy(o => o.Key)
                    .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue #21965")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_collection_with_groupby(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : Array.Empty<int>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_uncorrelated_collection_with_groupby_works(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => ss.Set<Order>().GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => c.Customer.City)
                    .Select(c => new
                    {
                        c1 = ss.Set<Product>().GroupBy(p => p.ProductID).Select(g => g.Key).ToArray(),
                        c2 = ss.Set<Product>().GroupBy(p => p.ProductID).Select(g => g.Count()).ToArray()
                    }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.c1, a.c1);
                    AssertCollection(e.c2, a.c2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_GroupBy_All(bool async)
        {
            return AssertAll(
                async,
                ss => ss.Set<Order>().Select(o => new ProjectedType { Order = o.OrderID, Customer = o.CustomerID })
                    .GroupBy(a => a.Customer),
                a => a.Key == "ALFKI");
        }

        private class ProjectedType
        {
            public int Order { get; set; }
            public string Customer { get; set; }

            private bool Equals(ProjectedType other)
                => Equals(Order, other.Order);

            public override bool Equals(object obj)
                => obj is null
                    ? false
                    : ReferenceEquals(this, obj)
                        ? true
                        : obj.GetType() == GetType()
                        && Equals((ProjectedType)obj);

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            public override int GetHashCode()
                => Order.GetHashCode();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_multiple_Count_with_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select new
                      {
                          g.Key,
                          All = g.Count(),
                          TenK = g.Count(e => e.OrderID < 11000),
                          EleventK = g.Count(e => e.OrderID < 12000)
                      },
                elementSorter: e => e.Key.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_multiple_Sum_with_conditional_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select new
                      {
                          g.Key,
                          TenK = g.Sum(e => e.OrderID < 11000 ? e.OrderID : 0),
                          EleventK = g.Sum(e => e.OrderID >= 11000 ? e.OrderID : 0)
                      },
                elementSorter: e => e.Key.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Key_as_part_of_element_selector(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                        o => o.OrderID, o => new { o.OrderID, o.OrderDate })
                    .Select(
                        g => new
                        {
                            g.Key,
                            Avg = g.Average(e => e.OrderID),
                            Max = g.Max(o => o.OrderDate)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_composite_Key_as_part_of_element_selector(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                        o => new { o.OrderID, o.CustomerID }, o => new { o.OrderID, o.OrderDate })
                    .Select(
                        g => new
                        {
                            g.Key,
                            Avg = g.Average(e => e.OrderID),
                            Max = g.Max(o => o.OrderDate)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_order_by_skip_and_another_order_by(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .OrderBy(o => o.CustomerID)
                    .ThenBy(o => o.OrderID)
                    .Skip(80)
                    .OrderBy(o => o.CustomerID)
                    .ThenBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Sum(o => o.OrderID))
            );
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Count_with_predicate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Count(o => o.OrderID < 10300)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_LongCount_with_predicate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.LongCount(o => o.OrderID < 10300)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_orderby_projection_with_coalesce_operation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .GroupBy(c => c.City)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key)
                    .Select(x => new { Locality = x.Key ?? "Unknown", Count = x.Count() }));
        }

        [ConditionalTheory(Skip = "issue #18923")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_let_orderby_projection_with_coalesce_operation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .GroupBy(c => c.City)
                    .Select(g => new { citiesCount = g.Count(), g })
                    .OrderByDescending(x => x.citiesCount)
                    .ThenBy(x => x.g.Key)
                    .Select(x => new { Locality = x.g.Key ?? "Unknown", Count = x.citiesCount }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Min_Where_optional_relationship(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.Customer.CustomerID)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .Where(x => x.Count != 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Min_Where_optional_relationship_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.Customer.CustomerID)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .Where(x => x.Count < 2 || x.Count > 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_over_a_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Count = (from c in ss.Set<Customer>() where c.CustomerID == g.Key select c).Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_join_with_grouping_key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .Join(ss.Set<Customer>(), o => o.Key, c => c.CustomerID, (o, c) => new { c, o.Count }),
                elementSorter: a => a.c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    AssertEqual(e.Count, a.Count);
                },
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_join_with_group_result(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID, e => e.OrderDate)
                    .Select(g => new { g.Key, LastOrderDate = g.Max() })
                    .Join(ss.Set<Order>(), o => o, i => new { Key = i.CustomerID, LastOrderDate = i.OrderDate }, (_, x) => x),
                entryCount: 90);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_from_right_side_of_join(bool async)
        {
            return AssertQuery(
                async,
                ss => (from c in ss.Set<Customer>()
                       join o in ss.Set<Order>().GroupBy(i => i.CustomerID).Select(e => new { e.Key, Max = e.Max(i => i.OrderDate) })
                           on c.CustomerID equals o.Key
                       select new { c, o.Max })
                    .OrderBy(e => e.Max)
                    .ThenBy(c => c.c.CustomerID)
                    .Skip(10)
                    .Take(10),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    AssertEqual(e.Max, a.Max);
                },
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_join_another_GroupBy_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Total = g.Count() })
                    .Join(
                        ss.Set<Order>().Where(o => o.OrderDate.Value.Year == 1997)
                            .GroupBy(o => o.CustomerID)
                            .Select(g => new { g.Key, ThatYear = g.Count() }),
                        o => o.Key,
                        i => i.Key,
                        (o, i) => new
                        {
                            o.Key,
                            o.Total,
                            i.ThatYear
                        }),
                elementSorter: o => o.Key);
        }

        #endregion

        #region GroupByAggregateChainComposition

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Average(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Select(e => (int?)e.OrderID).Average());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_LongCount(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).LongCount());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Max(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Select(e => (int?)e.OrderID).Max());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Select(e => (int?)e.OrderID).Min());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Sum(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Select(e => e.OrderID).Sum());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Count_with_predicate(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Count(e => e.OrderDate.HasValue && e.OrderDate.Value.Year == 1997));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Where_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Where(e => e.OrderDate.HasValue && e.OrderDate.Value.Year == 1997).Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Select_Where_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300).Select(e => e.OrderDate).Where(e => e.HasValue && e.Value.Year == 1997)
                          .Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_Select_Where_Select_Min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select g.Where(e => e.OrderID < 10300)
                          .Select(e => new { e.OrderID, e.OrderDate })
                          .Where(e => e.OrderDate.HasValue && e.OrderDate.Value.Year == 1997)
                          .Select(e => (int?)e.OrderID).Min());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_multiple_Sum_with_Select_conditional_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<Order>()
                      group o by new { o.CustomerID }
                      into g
                      select new
                      {
                          g.Key,
                          TenK = g.Select(e => e.OrderID < 11000 ? e.OrderID : 0).Sum(),
                          EleventK = g.Select(e => e.OrderID >= 11000 ? e.OrderID : 0).Sum()
                      },
                elementSorter: e => e.Key.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LongCount_after_GroupBy_aggregate(bool async)
        {
            return AssertSingleResult(
                async,
                ss => (from o in ss.Set<Order>()
                       group o by new { o.CustomerID }
                       into g
                       select g.Where(e => e.OrderID < 10300).Count()).LongCount(),
                ss => (from o in ss.Set<Order>()
                       group o by new { o.CustomerID }
                       into g
                       select g.Where(e => e.OrderID < 10300).Count()).LongCountAsync(default));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Select_Distinct_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g =>
                            new
                            {
                                g.Key,
                                Average = g.Select(e => e.OrderID).Distinct().Average(),
                                Count = g.Select(e => e.EmployeeID).Distinct().Count(),
                                LongCount = g.Select(e => e.EmployeeID).Distinct().LongCount(),
                                Max = g.Select(e => e.OrderDate).Distinct().Max(),
                                Min = g.Select(e => e.OrderDate).Distinct().Min(),
                                Sum = g.Select(e => e.OrderID).Distinct().Sum(),
                            }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_group_Distinct_Select_Distinct_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g =>
                            new
                            {
                                g.Key, Max = g.Distinct().Select(e => e.OrderDate).Distinct().Max(),
                            }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_group_Where_Select_Distinct_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g =>
                            new
                            {
                                g.Key, Max = g.Where(e => e.OrderDate.HasValue).Select(e => e.OrderDate).Distinct().Max(),
                            }),
                elementSorter: e => e.Key);
        }

        #endregion

        #region GroupByWithoutAggregate

        [ConditionalTheory(Skip = "Issue #18923")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_SelectMany(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().GroupBy(c => c.City).SelectMany(g => g),
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "Issue #18923")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_GroupBy_SelectMany(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .SelectMany(g => g),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #18923")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_GroupBy_SelectMany_shadow(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                    .GroupBy(e => e.EmployeeID)
                    .SelectMany(g => g)
                    .Select(g => EF.Property<string>(g, "Title")));
        }

        [ConditionalTheory(Skip = "Issue#17761")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct().Select(g => g.Key),
                assertOrder: true,
                entryCount: 31);
        }

        [ConditionalTheory(Skip = "Issue #17761")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Distinct(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [ConditionalTheory(Skip = "Issue #18923")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_aggregate_through_navigation_property(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(c => c.EmployeeID).Select(
                    g => new { max = g.Max(i => i.Customer.Region) }),
                elementSorter: e => e.max);
        }

        #endregion

        #region GroupBySelectFirst

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Shadow(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative" && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Shadow2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative" && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => g.First()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Shadow3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID == 1)
                    .GroupBy(e => e.EmployeeID)
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        #endregion

        #region GroupByEntityType

        [ConditionalTheory(Skip = "Issue #18923")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_GroupBy_SelectMany(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Select(
                        o => new ProjectedType { Order = o.OrderID, Customer = o.CustomerID })
                    .GroupBy(p => p.Customer)
                    .SelectMany(g => g),
                elementSorter: g => g.Order);
        }

        [ConditionalTheory(Skip = "issue #15938")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_group_key_being_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .GroupBy(od => od.Order)
                    .Select(g => new { g.Key, Aggregate = g.Sum(od => od.OrderID) }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory(Skip = "issue #15938")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_group_key_being_nested_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .GroupBy(od => od.Order.Customer)
                    .Select(g => new { g.Key, Aggregate = g.Sum(od => od.OrderID) }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory(Skip = "issue #15938")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_group_key_being_navigation_with_entity_key_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .GroupBy(od => od.Order)
                    .Select(g => g.Key));
        }

        [ConditionalTheory(Skip = "issue #15938")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_group_key_being_navigation_with_complex_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .GroupBy(od => od.Order)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Id1 = g.Key.CustomerID,
                            Id2 = g.Key.Customer.CustomerID,
                            Id3 = g.Key.OrderID,
                            Aggregate = g.Sum(od => od.OrderID)
                        }),
                elementSorter: e => e.Id3,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Key, a.Key);
                    Assert.Equal(e.Id1, a.Id1);
                    Assert.Equal(e.Id2, a.Id2);
                    Assert.Equal(e.Id3, a.Id3);
                    Assert.Equal(e.Aggregate, a.Aggregate);
                });
        }

        #endregion

        #region ResultOperatorsAfterGroupBy

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_after_GroupBy_aggregate(bool async)
        {
            return AssertSingleResult(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).Count(),
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).CountAsync(default));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task MinMax_after_GroupBy_aggregate(bool async)
        {
            await AssertMin(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)));

            await AssertMax(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_after_GroupBy_aggregate(bool async)
        {
            return AssertAll(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)),
                predicate: ee => true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_after_GroupBy_aggregate2(bool async)
        {
            return AssertAll(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)),
                predicate: ee => ee >= 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_after_GroupBy_aggregate(bool async)
        {
            return AssertAny(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_after_GroupBy_without_aggregate(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID),
                g => g.Count() > 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LongCount_after_GroupBy_without_aggregate(bool async)
        {
            return AssertLongCount(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LongCount_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            return AssertLongCount(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID),
                g => g.Count() > 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_after_GroupBy_without_aggregate(bool async)
        {
            return AssertAny(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            return AssertAny(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID),
                g => g.Count() > 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_with_predicate_after_GroupBy_without_aggregate(bool async)
        {
            return AssertAll(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID),
                g => g.Count() > 1);
        }

        #endregion

        # region GroupByInSubquery

        [ConditionalTheory(Skip = "issue #15279")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_query_with_groupBy_in_subquery1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(
                        c => new
                        {
                            Key = c.CustomerID,
                            Subquery = c.Orders
                                .Select(o => new { First = o.CustomerID, Second = o.OrderID })
                                .GroupBy(x => x.First)
                                .Select(g => new { Sum = g.Sum(x => x.Second) }).ToList()
                        }),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Key, a.Key);
                    AssertCollection(e.Subquery, a.Subquery);
                });
        }

        [ConditionalTheory(Skip = "issue #15279")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_query_with_groupBy_in_subquery2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(
                        c => new
                        {
                            Key = c.CustomerID,
                            Subquery = c.Orders
                                .Select(o => new { First = o.CustomerID, Second = o.OrderID })
                                .GroupBy(x => x.First)
                                .Select(g => new { Max = g.Max(x => x.First.Length), Sum = g.Sum(x => x.Second) }).ToList()
                        }),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Key, a.Key);
                    AssertCollection(e.Subquery, a.Subquery);
                });
        }

        [ConditionalTheory(Skip = "issue #15279")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_query_with_groupBy_in_subquery3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(
                        c => new
                        {
                            Key = c.CustomerID,
                            Subquery = ss.Set<Order>()
                                .Select(o => new { First = o.CustomerID, Second = o.OrderID })
                                .GroupBy(x => x.First)
                                .Select(g => new { Max = g.Max(x => x.First.Length), Sum = g.Sum(x => x.Second) }).ToList()
                        }),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Key, a.Key);
                    AssertCollection(e.Subquery, a.Subquery);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_scalar_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .GroupBy(
                        o => ss.Set<Customer>()
                            .Where(c => c.CustomerID == o.CustomerID)
                            .Select(c => c.ContactName)
                            .FirstOrDefault())
                    .Select(
                        g => new { g.Key, Count = g.Count() }),
                elementSorter: e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_scalar_aggregate_in_set_operation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("F"))
                    .Select(c => new { c.CustomerID, Sequence = 0 })
                    .Union(
                        ss.Set<Order>()
                            .GroupBy(o => o.CustomerID)
                            .Select(g => new { CustomerID = g.Key, Sequence = 1 })),
                elementSorter: e => (e.CustomerID, e.Sequence));
        }

        #endregion
    }
}
