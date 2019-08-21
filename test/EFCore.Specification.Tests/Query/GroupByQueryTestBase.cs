// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class GroupByQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected GroupByQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        #region GroupByProperty

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Average(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory(Skip = "issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Average_with_navigation_expansion(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Where(o => o.Customer.City != "London")
                    .GroupBy(o => o.CustomerID, (k, es) => new { k, es })
                    .Select(g => g.es.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Count(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_LongCount(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Max(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Min(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Sum(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => EF.Property<string>(o, "CustomerID")).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Average(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Count(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => EF.Property<string>(o, "CustomerID")).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Count = g.Count()
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_LongCount(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            LongCount = g.LongCount()
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Max(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Max = g.Max(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Min(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Min = g.Min(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => EF.Property<string>(o, "CustomerID")).Select(
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
        public virtual Task GroupBy_Property_Select_key_multiple_times_and_aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(
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
        public virtual Task GroupBy_Property_Select_Key_with_constant(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => new { Name = "CustomerID", Value = o.CustomerID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Count = g.Count()
                        }),
                e => e.Key.Value);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_projecting_conditional_expression_based_on_group_key(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.OrderDate).Select(
                    g =>
                        new
                        {
                            Key = g.Key == null ? "is null" : "is not null",
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Sum);
        }

        #endregion

        #region GroupByAnonymousAggregate

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Average(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID
                    }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Count(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID
                    }).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_LongCount(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID
                    }).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Max(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID
                    }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Min(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID
                    }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Sum(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID
                    }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID
                    }).Select(
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
        public virtual Task GroupBy_anonymous_with_alias_Select_Key_Sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        Id = o.CustomerID
                    }).Select(
                    g =>
                        new
                        {
                            Key = g.Key.Id,
                            Sum = g.Sum(o => o.OrderID)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Average(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Count(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_LongCount(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Max(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Min(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Sum(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
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
        public virtual Task GroupBy_Composite_Select_Key_Average(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Average = g.Average(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Count(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Count = g.Count()
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_LongCount(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            LongCount = g.LongCount()
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Max(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Max = g.Max(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Min(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Min = g.Min(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
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
        public virtual Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
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
        public virtual Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
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
        public virtual Task GroupBy_Dto_as_key_Select_Sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new NominalType
                    {
                        CustomerID = o.CustomerID,
                        EmployeeID = o.EmployeeID
                    }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            g.Key
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Dto_as_element_selector_Select_Sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                        o => o.CustomerID,
                        o => new NominalType
                        {
                            CustomerID = o.CustomerID,
                            EmployeeID = o.EmployeeID
                        })
                    .Select(
                        g =>
                            new
                            {
                                Sum = g.Sum(o => o.EmployeeID),
                                g.Key
                            }));
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

            public override int GetHashCode() => 0;

            private bool Equals(NominalType other)
                => string.Equals(CustomerID, other.CustomerID)
                   && EmployeeID == other.EmployeeID;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
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

            public override int GetHashCode() => 0;

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
        public virtual Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).Select(
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
        public virtual Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => 2).Select(
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
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => 2, o => new { o.OrderID, o.OrderDate }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum2(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => 2, o => new { o.OrderID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum3(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => 2, o => new { o.OrderID, o.OrderDate, o.CustomerID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID > 10500).GroupBy(o => 2).Select(
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
        public virtual Task GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => 2, o => o.OrderID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(),
                            g.Key
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            var a = 2;

            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => a).Select(
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
        public virtual Task GroupBy_param_with_element_selector_Select_Sum(bool isAsync)
        {
            var a = 2;

            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => a, o => new { o.OrderID, o.OrderDate }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_with_element_selector_Select_Sum2(bool isAsync)
        {
            var a = 2;

            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => a, o => new { o.OrderID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_with_element_selector_Select_Sum3(bool isAsync)
        {
            var a = 2;

            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => a, o => new { o.OrderID, o.OrderDate, o.CustomerID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            var a = 2;

            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => a, o => o.OrderID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(),
                            g.Key
                        }),
                e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_key_type_mismatch_with_aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => new { I0 = (int?)o.OrderDate.Value.Year })
                    .OrderBy(g => g.Key.I0)
                    .Select(g => new
                    {
                        I0 = g.Count(),
                        I1 = g.Key.I0
                    }),
                elementSorter: a => a.I1);
        }

        #endregion

        #region GroupByWithElementSelectorAggregate

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Average(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Average()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Count(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_LongCount(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Max(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Max()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Min(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Min()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Sum(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Average(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => o.CustomerID, o => new
                    {
                        o.OrderID,
                        o.EmployeeID
                    }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Count(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => o.CustomerID, o => new
                    {
                        o.OrderID,
                        o.EmployeeID
                    }).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_LongCount(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => o.CustomerID, o => new
                    {
                        o.OrderID,
                        o.EmployeeID
                    }).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Max(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => o.CustomerID, o => new
                    {
                        o.OrderID,
                        o.EmployeeID
                    }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Min(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => o.CustomerID, o => new
                    {
                        o.OrderID,
                        o.EmployeeID
                    }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Sum(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(
                    o => o.CustomerID, o => new
                    {
                        o.OrderID,
                        o.EmployeeID
                    }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                    o => o.CustomerID, o => new
                    {
                        o.OrderID,
                        o.EmployeeID
                    }).Select(
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
        public virtual Task GroupBy_element_selector_complex_aggregate(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID })
                    .Select(g => g.Sum(e => e.OrderID + 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_element_selector_complex_aggregate2(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.OrderDate })
                    .Select(g => g.Sum(e => e.OrderID + 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_element_selector_complex_aggregate3(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .Select(g => g.Sum(e => e + 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_element_selector_complex_aggregate4(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID + 1)
                    .Select(g => g.Sum(e => e)));
        }

        #endregion

        #region GroupByAfterComposition

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_empty_key_Aggregate(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os =>
                    os.GroupBy(
                            o => new
                            {
                            })
                        .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_empty_key_Aggregate_Key(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(
                            o => new
                            {
                            })
                        .Select(
                            g => new
                            {
                                g.Key,
                                Sum = g.Sum(o => o.OrderID)
                            }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Skip(80)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Take(500)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_Take_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Skip(80)
                        .Take(500)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count()
                            }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_projection_Distinct_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.Select(
                            o => new
                            {
                                o.OrderID,
                                o.EmployeeID
                            })
                        .Distinct()
                        .GroupBy(o => o.EmployeeID)
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count()
                            }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.SelectMany(c => c.Orders)
                        .GroupBy(o => o.EmployeeID)
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count()
                            }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Count = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_required_navigation_member_Aggregate(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods =>
                    ods.GroupBy(od => od.Order.CustomerID)
                        .Select(
                            g =>
                                new
                                {
                                    CustomerId = g.Key,
                                    Count = g.Count()
                                }),
                e => e.CustomerId);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_complex_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from o in os.Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                     join c in cs.Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO")
                             .OrderBy(c => c.City).Skip(10).Take(50)
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Count = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     where o != null
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Average = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_2(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     select c)
                    .GroupBy(c => c.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Max = g.Max(c => c.City)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_3(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping.DefaultIfEmpty()
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Average = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_4(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     select c)
                    .GroupBy(c => c.CustomerID)
                    .Select(
                        g => new
                        {
                            Value = g.Key,
                            Max = g.Max(c => c.City)
                        }),
                e => e.Value);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_GroupBy_Aggregate_5(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping.DefaultIfEmpty()
                     select o)
                    .GroupBy(o => o.OrderID)
                    .Select(
                        g => new
                        {
                            Value = g.Key,
                            Average = g.Average(o => o.OrderID)
                        }),
                e => e.Value);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_optional_navigation_member_Aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(o => o.Customer.Country)
                        .Select(
                            g =>
                                new
                                {
                                    Country = g.Key,
                                    Count = g.Count()
                                }),
                e => e.Country);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_complex_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from c in cs.Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO")
                         .OrderBy(c => c.City).Skip(10).Take(50)
                     join o in os.Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping
                     where o.OrderID > 10300
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Count = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Self_join_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order, Order>(
                isAsync,
                (os1, os2) =>
                    (from o1 in os1.Where(o => o.OrderID < 10400)
                     join o2 in os2
                         on o1.OrderID equals o2.OrderID
                     group o2 by o1.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Count = g.Average(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_multi_navigation_members_Aggregate(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods =>
                    ods.GroupBy(
                            od => new
                            {
                                od.Order.CustomerID,
                                od.Product.ProductName
                            })
                        .Select(
                            g =>
                                new
                                {
                                    CompositeKey = g.Key,
                                    Count = g.Count()
                                }),
                e => e.CompositeKey.CustomerID + " " + e.CompositeKey.ProductName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_simple_groupby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F."))
                    .GroupBy(c => c.City)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Total = g.Count()
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_GroupBy_Aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10300)
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
        public virtual Task GroupBy_principal_key_property_optimization(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.Customer.CustomerID)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Count = g.Count()
                        }));
        }

        #endregion

        #region GroupByAggregateComposition

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_OrderBy_key(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Key)
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count()
                            }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_OrderBy_count(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Count())
                        .ThenBy(o => o.Key)
                        .Select(
                            g => new
                            {
                                g.Key,
                                Count = g.Count()
                            }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_OrderBy_count_Select_sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Count())
                        .ThenBy(o => o.Key)
                        .Select(
                            g => new
                            {
                                g.Key,
                                Sum = g.Sum(o => o.OrderID)
                            }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Contains(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(
                    o => os.GroupBy(e => e.CustomerID)
                        .Where(g => g.Count() > 30)
                        .Select(g => g.Key)
                        .Contains(o.CustomerID)),
                entryCount: 31);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Pushdown(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(e => e.CustomerID)
                    .Where(g => g.Count() > 10)
                    .Select(g => g.Key)
                    .OrderBy(t => t)
                    .Take(20)
                    .Skip(4));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Pushdown_followed_by_projecting_Length(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(e => e.CustomerID)
                    .Where(g => g.Count() > 10)
                    .Select(g => g.Key)
                    .OrderBy(t => t)
                    .Take(20)
                    .Skip(4)
                    .Select(e => e.Length));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_aggregate_Pushdown_followed_by_projecting_constant(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(e => e.CustomerID)
                    .Where(g => g.Count() > 10)
                    .Select(g => g.Key)
                    .OrderBy(t => t)
                    .Take(20)
                    .Skip(4)
                    .Select(e => 5));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Select_sum_over_unmapped_property(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID)
                    .Select(g => new
                    {
                        g.Key,
                        Sum = g.Sum(o => o.Freight)
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_filter_key(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .Where(o => o.Key == "ALFKI")
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count()
                            }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_filter_count(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .Where(o => o.Count() > 4)
                        .Select(
                            g => new
                            {
                                g.Key,
                                Count = g.Count()
                            }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_filter_count_OrderBy_count_Select_sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.GroupBy(o => o.CustomerID)
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
        public virtual Task GroupBy_Aggregate_Join(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    from a in os.GroupBy(o => o.CustomerID)
                        .Where(g => g.Count() > 5)
                        .Select(
                            g => new
                            {
                                CustomerID = g.Key,
                                LastOrderID = g.Max(o => o.OrderID)
                            })
                    join c in cs on a.CustomerID equals c.CustomerID
                    join o in os on a.LastOrderID equals o.OrderID
                    select new
                    {
                        c,
                        o
                    },
                entryCount: 126);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_multijoins(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    from c in cs
                    join a in os.GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(
                                g => new
                                {
                                    CustomerID = g.Key,
                                    LastOrderID = g.Max(o => o.OrderID)
                                })
                        on c.CustomerID equals a.CustomerID
                    join o in os on a.LastOrderID equals o.OrderID
                    select new
                    {
                        c,
                        o
                    },
                entryCount: 126);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_single_join(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    from c in cs
                    join a in os.GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(
                                g => new
                                {
                                    CustomerID = g.Key,
                                    LastOrderID = g.Max(o => o.OrderID)
                                })
                        on c.CustomerID equals a.CustomerID
                    select new
                    {
                        c,
                        a.LastOrderID
                    },
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_Aggregate_with_another_join(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    from c in cs
                    join a in os.GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 5)
                            .Select(
                                g => new
                                {
                                    CustomerID = g.Key,
                                    LastOrderID = g.Max(o => o.OrderID)
                                })
                        on c.CustomerID equals a.CustomerID
                    join o in os on c.CustomerID equals o.CustomerID into grouping
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
        public virtual Task Join_GroupBy_Aggregate_in_subquery(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    from o in os.Where(o => o.OrderID < 10400)
                    join i in (from c in cs
                               join a in os.GroupBy(o => o.CustomerID)
                                       .Where(g => g.Count() > 5)
                                       .Select(
                                           g => new
                                           {
                                               CustomerID = g.Key,
                                               LastOrderID = g.Max(o => o.OrderID)
                                           })
                                   on c.CustomerID equals a.CustomerID
                               select new
                               {
                                   c,
                                   a.LastOrderID
                               })
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
        public virtual Task Join_GroupBy_Aggregate_on_key(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from c in cs
                     join a in os.GroupBy(o => o.CustomerID)
                             .Where(g => g.Count() > 5)
                             .Select(
                                 g => new
                                 {
                                     g.Key,
                                     LastOrderID = g.Max(o => o.OrderID)
                                 })
                         on c.CustomerID equals a.Key
                     select new
                     {
                         c,
                         a.LastOrderID
                     }),
                e => e.c.CustomerID,
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_result_selector(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Sum_constant(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(e => 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Sum_constant_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(e => 1L)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_GroupBy_OrderBy_key(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Key)
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count()
                            }),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_collection_with_groupby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : Array.Empty<int>()));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_GroupBy_All(bool isAsync)
        {
            return AssertAll<Order, IGrouping<string, ProjectedType>>(
                isAsync,
                os => os.Select(o => new ProjectedType
                {
                    Order = o.OrderID,
                    Customer = o.CustomerID
                })
                .GroupBy(a => a.Customer),
                a => a.Key == "ALFKI");
        }

        private class ProjectedType
        {
            public int Order { get; set; }
            public string Customer { get; set; }

            private bool Equals(ProjectedType other) => Equals(Order, other.Order);

            public override bool Equals(object obj)
                => obj is null
                    ? false
                    : ReferenceEquals(this, obj)
                        ? true
                        : obj.GetType() == GetType()
                          && Equals((ProjectedType)obj);

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => Order.GetHashCode();
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Where_in_aggregate(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => from o in os
                      group o by new
                      {
                          o.CustomerID
                      }
                      into g
                      select g.Where(e => e.OrderID < 10300).Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Key_as_part_of_element_selector(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                        o => o.OrderID, o => new
                        {
                            o.OrderID,
                            o.OrderDate
                        })
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
        public virtual Task GroupBy_composite_Key_as_part_of_element_selector(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                        o => new
                        {
                            o.OrderID,
                            o.CustomerID
                        }, o => new
                        {
                            o.OrderID,
                            o.OrderDate
                        })
                    .Select(
                        g => new
                        {
                            g.Key,
                            Avg = g.Average(e => e.OrderID),
                            Max = g.Max(o => o.OrderDate)
                        }));
        }

        #endregion

        #region GroupByWithoutAggregate

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.City,
                        c.CustomerID
                    }).GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_with_where(bool isAsync)
        {
            var countries = new[] { "Argentina", "Austria", "Brazil", "France", "Germany", "USA" };
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => countries.Contains(c.Country))
                    .Select(
                        c => new
                        {
                            c.City,
                            c.CustomerID
                        })
                    .GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalTheory(Skip = "Test does not pass. See issue#7160")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_subquery(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Select(
                            c => new
                            {
                                c.City,
                                c.CustomerID
                            })
                        .GroupBy(a => from c2 in cs select c2),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_nested_order_by_enumerable(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Select(
                            c => new
                            {
                                c.Country,
                                c.CustomerID
                            })
                        .OrderBy(a => a.Country)
                        .GroupBy(a => a.Country)
                        .Select(g => g.OrderBy(a => a.CustomerID)),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_join_default_if_empty_anonymous(bool isAsync)
        {
            return AssertQuery<Order, OrderDetail>(
                isAsync,
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

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_SelectMany(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.GroupBy(c => c.City).SelectMany(g => g),
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_simple(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_simple2(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g),
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_first(bool isAsync)
        {
            return AssertFirst<Order>(
                isAsync,
                os => os.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Cast<object>(),
                asserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 6);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_element_selector(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<int>());
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_element_selector2(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o.OrderID)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>());
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_element_selector3(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => es.GroupBy(e => e.EmployeeID)
                    .OrderBy(g => g.Key)
                    .Select(
                        g => g.Select(
                            e => new
                            {
                                Title = EF.Property<string>(e, "Title"),
                                e
                            }).ToList()),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_DateTimeOffset_Property(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate.HasValue).GroupBy(o => o.OrderDate.Value.Month),
                e => ((IGrouping<int, Order>)e).Key,
                elementAsserter: GroupingAsserter<int, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_GroupBy_SelectMany(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .SelectMany(g => g),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_GroupBy_SelectMany_shadow(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    es.OrderBy(e => e.EmployeeID)
                        .GroupBy(e => e.EmployeeID)
                        .SelectMany(g => g)
                        .Select(g => EF.Property<string>(g, "Title")));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_orderby(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).OrderBy(g => g.Key),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_orderby_and_anonymous_projection(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Select(
                    g => new
                    {
                        Foo = "Foo",
                        Group = g
                    }),
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

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_orderby_take_skip_distinct(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct(),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 31);
        }

        // issue #12576
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_orderby_take_skip_distinct_followed_by_group_key_projection(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct().Select(g => g.Key),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 31);
        }

        // issue #12641
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_orderby_take_skip_distinct_followed_by_order_by_group_key(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct().OrderBy(g => g.Key),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 31);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_join_anonymous(bool isAsync)
        {
            return AssertQuery<Order, OrderDetail>(
                isAsync,
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

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Distinct(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    // TODO: See issue#11215
                    os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_GroupBy(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderID).Skip(800).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 30);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_GroupBy(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderDate).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_Take_GroupBy(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.CustomerID != "SAVEA").OrderBy(o => o.OrderDate).Skip(450).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Distinct_GroupBy(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(
                    o => new
                    {
                        o.CustomerID,
                        o.EmployeeID
                    }).OrderBy(a => a.EmployeeID).Distinct().GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.EmployeeID));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_aggregate_through_navigation_property(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(c => c.EmployeeID).Select(
                    g => new
                    {
                        max = g.Max(i => i.Customer.Region)
                    }),
                elementSorter: e => e.max);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_anonymous_key_without_aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                        o => new
                        {
                            o.CustomerID,
                            o.OrderDate
                        })
                    .Select(
                        g => new
                        {
                            g.Key,
                            g
                        }),
                elementSorter: g => g.Key + " " + g.g.Count());
        }

        #endregion

        #region GroupBySelectFirst

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Shadow(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Shadow2(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => g.First()));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Shadow3(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    es.Where(e => e.EmployeeID == 1)
                        .GroupBy(e => e.EmployeeID)
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Select_First_GroupBy(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.GroupBy(c => c.City)
                        .Select(g => g.OrderBy(c => c.CustomerID).First())
                        .GroupBy(c => c.ContactName),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        // issue #12573
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Select_First_GroupBy_followed_by_identity_projection(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.GroupBy(c => c.City)
                        .Select(g => g.OrderBy(c => c.CustomerID).First())
                        .GroupBy(c => c.ContactName)
                        .Select(g => g),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        #endregion

        #region GroupByEntityType

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_GroupBy(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    })
                .GroupBy(p => p.Customer),
                elementSorter: g => g.Key + " " + g.Count());
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_GroupBy_SelectMany(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    })
                .GroupBy(p => p.Customer)
                .SelectMany(g => g),
                elementSorter: g => g.Order);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupBy_entity_ToList(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => from c in cs.OrderBy(c => c.CustomerID).Take(5)
                            join o in os.OrderBy(o => o.OrderID).Take(50)
                                on c.CustomerID equals o.CustomerID
                            group o by c into grp
                            select new
                            {
                                C = grp.Key,
                                Os = grp.ToList()
                            });
        }

        #endregion

        #region DoubleGroupBy

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_GroupBy_with_aggregate(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.GroupBy(
                        o => new
                        {
                            o.OrderID,
                            o.OrderDate
                        })
                    .GroupBy(g => g.Key.OrderDate)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Lastest = g.OrderBy(e => e.Key.OrderID).FirstOrDefault()
                        }));
        }

        #endregion

        #region ResultOperatorsAfterGroupBy

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_after_GroupBy_aggregate(bool isAsync)
        {
            return AssertSingleResult<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).Count(),
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).CountAsync());
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LongCount_after_client_GroupBy(bool isAsync)
        {
            return AssertSingleResult<Order>(
                isAsync,
                os => (from o in os
                       group o by new { o.CustomerID }
                       into g
                       select g.Where(e => e.OrderID < 10300).Count()).LongCount(),
                os => (from o in os
                       group o by new { o.CustomerID }
                       into g
                       select g.Where(e => e.OrderID < 10300).Count()).LongCountAsync());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task MinMax_after_GroupBy_aggregate(bool isAsync)
        {
            await AssertMin<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)));

            await AssertMax<Order>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task All_after_GroupBy_aggregate(bool isAsync)
        {
            await AssertAll<Order, int>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)),
                predicate: ee => true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task All_after_GroupBy_aggregate2(bool isAsync)
        {
            await AssertAll<Order, int>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)),
                predicate: ee => ee >= 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Any_after_GroupBy_aggregate(bool isAsync)
        {
            await AssertAny<Order, int>(
                isAsync,
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)));
        }

        #endregion
    }
}
