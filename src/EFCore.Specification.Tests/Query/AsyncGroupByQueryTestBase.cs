// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AsyncGroupByQueryTestBase<TFixture> : AsyncQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected AsyncGroupByQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        #region GroupByProperty

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Average()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Count()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_LongCount()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Max()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Min()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Sum()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Key_Average()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Key_Count()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Count = g.Count()
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Key_LongCount()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.LongCount()
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Key_Max()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Max(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Key_Min()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Min(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Key_Sum()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Sum(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
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

        #endregion

        #region GroupByAnonymousAggregate

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_Select_Average()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_Select_Count()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_Select_LongCount()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_Select_Max()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_Select_Min()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_Select_Sum()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_Select_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID }).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Average()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Count()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_LongCount()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Max()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Min()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Sum()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Key_Average()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Average(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Key_Count()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Count = g.Count()
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Key_LongCount()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.LongCount()
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Key_Max()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Max(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Key_Min()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Min(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Key_Sum()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Sum(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
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

        #endregion

        #region GroupByWithElementSelectorAggregate

        [ConditionalFact]
        public virtual async Task GroupBy_Property_scalar_element_selector_Average()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Average()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_scalar_element_selector_Count()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_scalar_element_selector_LongCount()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_scalar_element_selector_Max()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Max()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_scalar_element_selector_Min()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Min()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_scalar_element_selector_Sum()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Property_anonymous_element_selector_Average()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_anonymous_element_selector_Count()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_anonymous_element_selector_LongCount()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_anonymous_element_selector_Max()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_anonymous_element_selector_Min()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_anonymous_element_selector_Sum()
        {
            await AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(
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

        #endregion

        #region GroupByAfterComposition

        [ConditionalFact]
        public virtual async Task OrderBy_GroupBy_Aggregate()
        {
            await AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Skip_GroupBy_Aggregate()
        {
            await AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Skip(80)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Take_GroupBy_Aggregate()
        {
            await AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Take(500)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Skip_Take_GroupBy_Aggregate()
        {
            await AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Skip(80)
                        .Take(500)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task Distinct_GroupBy_Aggregate()
        {
            await AssertQuery<Order>(
                os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task Anonymous_projection_Distinct_GroupBy_Aggregate()
        {
            await AssertQuery<Order>(
                os =>
                    os.Select(o => new { o.OrderID, o.EmployeeID })
                        .Distinct()
                        .GroupBy(o => o.EmployeeID)
                        .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_GroupBy_Aggregate()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.SelectMany(c => c.Orders)
                        .GroupBy(o => o.EmployeeID)
                        .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task Join_GroupBy_Aggregate()
        {
            await AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task Join_complex_GroupBy_Aggregate()
        {
            await AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from o in os.Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                     join c in cs.Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO").OrderBy(c => c.City).Skip(10).Take(50)
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_GroupBy_Aggregate()
        {
            await AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_GroupBy_Aggregate_2()
        {
            await AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping
                     select c)
                    .GroupBy(c => c.CustomerID)
                    .Select(g => new { g.Key, Count = g.Max(c => c.City) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_GroupBy_Aggregate_3()
        {
            await AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_complex_GroupBy_Aggregate()
        {
            await AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from c in cs.Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO").OrderBy(c => c.City).Skip(10).Take(50)
                     join o in os.Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping
                     where o.OrderID > 10300
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual async Task Self_join_GroupBy_Aggregate()
        {
            await AssertQuery<Order, Order>(
                (os1, os2) =>
                    (from o1 in os1.Where(o => o.OrderID < 10400)
                     join o2 in os2
                         on o1.OrderID equals o2.OrderID
                     group o2 by o1.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact(Skip = "Unable to bind group by. See Issue#6658")]
        public virtual async Task Union_simple_groupby()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F."))
                    .GroupBy(c => c.City)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Total = g.Count()
                        }),
                entryCount: 19);
        }

        #endregion

        #region GroupByAggregateVariations

        [ConditionalFact]
        public virtual async Task GroupBy_with_result_selector()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(
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

        [ConditionalFact]
        public virtual async Task GroupBy_Sum_constant()
        {
            await AssertQueryScalar<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(e => 1)));
        }

        #endregion

        #region GroupByKeyBinding

        [ConditionalFact]
        public virtual async Task Distinct_GroupBy_OrderBy_key()
        {
            await AssertQuery<Order>(
                os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Key)
                        .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task Select_nested_collection_with_groupby()
        {
            using (var context = CreateContext())
            {
                var expected = (await context.Customers
                        .Include(c => c.Orders)
                        // ReSharper disable once StringStartsWithIsCultureSpecific
                        .Where(c => c.CustomerID.StartsWith("A"))
                        .ToListAsync())
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : new int[0]).ToList();

                var query = context.Customers
                    // ReSharper disable once StringStartsWithIsCultureSpecific
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : new int[0]);

                var result = await query.ToListAsync();

                Assert.Equal(expected.Count, result.Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Select_GroupBy_All()
        {
            using (var context = CreateContext())
            {
                Assert.False(
                    await context
                        .Set<Order>()
                        .Select(
                            o => new ProjectedType
                            {
                                Order = o.OrderID,
                                Customer = o.CustomerID
                            })
                        .GroupBy(a => a.Customer)
                        .AllAsync(a => a.Key == "ALFKI")
                );
            }
        }

        private class ProjectedType
        {
            public int Order { get; set; }
            public string Customer { get; set; }

            private bool Equals(ProjectedType other) => Equals(Order, other.Order);

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType()
                       && Equals((ProjectedType)obj);
            }

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => Order.GetHashCode();
        }

        #endregion

        #region GroupByWithoutAggregate

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.CustomerID }).GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_anonymous_with_where()
        {
            var countries = new[] { "Argentina", "Austria", "Brazil", "France", "Germany", "USA" };
            await AssertQuery<Customer>(
                cs => cs.Where(c => countries.Contains(c.Country))
                    .Select(c => new { c.City, c.CustomerID })
                    .GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalFact(Skip = "Test does not pass. See issue#7160")]
        public virtual async Task GroupBy_anonymous_subquery()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Select(c => new { c.City, c.CustomerID })
                        .GroupBy(a => from c2 in cs select c2),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_nested_order_by_enumerable()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Select(c => new { c.Country, c.CustomerID })
                        .OrderBy(a => a.Country)
                        .GroupBy(a => a.Country)
                        .Select(g => g.OrderBy(a => a.CustomerID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_join_default_if_empty_anonymous()
        {
            await AssertQuery<Order, OrderDetail>(
                (os, ods) =>
                    (from order in os
                     join orderDetail in ods on order.OrderID equals orderDetail.OrderID into orderJoin
                     from orderDetail in orderJoin.DefaultIfEmpty()
                     group new
                     {
                         orderDetail.ProductID,
                         orderDetail.Quantity,
                         orderDetail.UnitPrice
                     } by new
                     {
                         order.OrderID,
                         order.OrderDate
                     }).Where(x => x.Key.OrderID == 10248),
                elementAsserter: GroupingAsserter<dynamic, dynamic>(d => d.ProductID));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_SelectMany()
        {
            await AssertQuery<Customer>(
                cs => cs.GroupBy(c => c.City).SelectMany(g => g),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_simple()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_simple2()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g),
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_first()
        {
            await AssertSingleResult<Order>(
                os => os.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Cast<object>().FirstAsync(),
                asserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<int>());
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector2()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o.OrderID)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>());
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_element_selector3()
        {
            await AssertQuery<Employee>(
                es => es.GroupBy(e => e.EmployeeID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Select(e => new { Title = EF.Property<string>(e, "Title"), e }).ToList()),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_DateTimeOffset_Property()
        {
            await AssertQuery<Order>(
                os => os.Where(o => o.OrderDate.HasValue).GroupBy(o => o.OrderDate.Value.Month),
                e => ((IGrouping<int, Order>)e).Key,
                elementAsserter: GroupingAsserter<int, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_GroupBy_SelectMany()
        {
            await AssertQuery<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .SelectMany(g => g),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_GroupBy_SelectMany_shadow()
        {
            await AssertQuery<Employee>(
                es =>
                    es.OrderBy(e => e.EmployeeID)
                        .GroupBy(e => e.EmployeeID)
                        .SelectMany(g => g)
                        .Select(g => EF.Property<string>(g, "Title")));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_orderby()
        {
            await AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).OrderBy(g => g.Key),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_orderby_and_anonymous_projection()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Select(g => new { Foo = "Foo", Group = g }),
                e => GroupingSorter<string, object>()(e.Group),
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.Foo, a.Foo);
                        IGrouping<string, Order> eGrouping = e.Group;
                        IGrouping<string, Order> aGrouping = a.Group;
                        Assert.Equal(eGrouping.OrderBy(p => p.OrderID), aGrouping.OrderBy(p => p.OrderID));
                    },
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_orderby_take_skip_distinct()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct(),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 31);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_join_anonymous()
        {
            await AssertQuery<Order, OrderDetail>(
                (os, ods) =>
                    (from order in os
                     join orderDetail in ods on order.OrderID equals orderDetail.OrderID into orderJoin
                     from orderDetail in orderJoin
                     group new
                     {
                         orderDetail.ProductID,
                         orderDetail.Quantity,
                         orderDetail.UnitPrice
                     } by new
                     {
                         order.OrderID,
                         order.OrderDate
                     }).Where(x => x.Key.OrderID == 10248),
                elementAsserter: GroupingAsserter<dynamic, dynamic>(d => d.ProductID));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Distinct()
        {
            await AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Skip_GroupBy()
        {
            await AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Skip(800).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 30);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Take_GroupBy()
        {
            await AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Skip_Take_GroupBy()
        {
            await AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Skip(450).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }

        [ConditionalFact]
        public virtual async Task Select_Distinct_GroupBy()
        {
            await AssertQuery<Order>(
                os => os.Select(o => new { o.CustomerID, o.EmployeeID }).OrderBy(a => a.EmployeeID).Distinct().GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.EmployeeID));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_with_aggregate_through_navigation_property()
        {
            await AssertQuery<Order>(
                os => os.GroupBy(c => c.EmployeeID).Select(g => new { max = g.Max(i => i.Customer.Region) }),
                elementSorter: e => e.max);
        }

        #endregion

        #region GroupBySelectFirst

        [ConditionalFact]
        public virtual async Task GroupBy_Shadow()
        {
            await AssertQuery<Employee>(
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Shadow2()
        {
            await AssertQuery<Employee>(
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => g.First()));
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Shadow3()
        {
            await AssertQuery<Employee>(
                es =>
                    es.Where(e => e.EmployeeID == 1)
                        .GroupBy(e => e.EmployeeID)
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        #endregion

        #region GroupByEntityType

        [ConditionalFact]
        public virtual async Task Select_GroupBy()
        {
            using (var context = CreateContext())
            {
                var actual = (await context.Set<Order>().Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    }).GroupBy(p => p.Customer).ToListAsync()).OrderBy(g => g.Key + " " + g.Count()).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>().Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    }).GroupBy(p => p.Customer).ToList().OrderBy(g => g.Key + " " + g.Count()).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Count(), actual[i].Count());
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Select_GroupBy_SelectMany()
        {
            using (var context = CreateContext())
            {
                var actual = (await context.Set<Order>().Select(
                        o => new ProjectedType
                        {
                            Order = o.OrderID,
                            Customer = o.CustomerID
                        })
                    .GroupBy(o => o.Order)
                    .SelectMany(g => g).ToListAsync()).OrderBy(e => e.Order).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>().Select(
                        o => new ProjectedType
                        {
                            Order = o.OrderID,
                            Customer = o.CustomerID
                        })
                    .GroupBy(o => o.Order)
                    .SelectMany(g => g).ToList().OrderBy(e => e.Order).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i], actual[i]);
                }
            }
        }

        #endregion
    }
}
