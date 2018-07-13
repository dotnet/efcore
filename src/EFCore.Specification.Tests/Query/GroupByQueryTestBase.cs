// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
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

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Average()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Count()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_LongCount()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Max()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Min()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Sum()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Property_Select_Key_Average()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Property_Select_Key_Count()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Property_Select_Key_LongCount()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            LongCount = g.LongCount()
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Key_Max()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Max = g.Max(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Key_Min()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Min = g.Min(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Key_Sum()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_Select_Key_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Property_Select_Sum_Min_Key_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_anonymous_Select_Average()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_Select_Count()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_Select_LongCount()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_Select_Max()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_Select_Min()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_Select_Sum()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_Select_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_anonymous_with_alias_Select_Key_Sum()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => new { Id = o.CustomerID }).Select(
                    g =>
                    new
                    {
                        Key = g.Key.Id,
                        Sum = g.Sum(o => o.OrderID)
                    }));
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Average()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Count()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_LongCount()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Max()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Min()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Sum()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Composite_Select_Key_Average()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Composite_Select_Key_Count()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Composite_Select_Key_LongCount()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            LongCount = g.LongCount()
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Key_Max()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Max = g.Max(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Key_Min()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Min = g.Min(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Key_Sum()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID)
                        }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Key_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Composite_Select_Sum_Min_Key_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Dto_as_key_Select_Sum()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => new NominalType { CustomerID = o.CustomerID, EmployeeID = o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            g.Key,
                        }));
        }

        [ConditionalFact]
        public virtual void GroupBy_Dto_as_element_selector_Select_Sum()
        {
            AssertQuery<Order>(
                os => os.GroupBy(
                    o => o.CustomerID,
                    o => new NominalType { CustomerID = o.CustomerID, EmployeeID = o.EmployeeID })
                .Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.EmployeeID),
                            g.Key,
                        }));
        }

        protected class NominalType
        {
            public string CustomerID { get; set; }
#if Test20
            public int? EmployeeID { get; set; }
#else
            public uint? EmployeeID { get; set; }
#endif

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((NominalType)obj);
            }

            public override int GetHashCode() => 0;

            private bool Equals(NominalType other)
                => string.Equals(CustomerID, other.CustomerID)
                    && EmployeeID == other.EmployeeID;
        }

        [ConditionalFact(Skip = "https://github.com/dotnet/corefx/issues/30955")]
        public virtual void GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => new { o.CustomerID, o.EmployeeID }).Select(
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
#if Test20
            public int? EmployeeId { get; set; }
#else
            public uint? EmployeeId { get; set; }
#endif

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((CompositeDto)obj);
            }

            public override int GetHashCode() => 0;

            private bool Equals(CompositeDto other)
                => Sum == other.Sum
                    && Min == other.Min
                    && Max == other.Max
                    && Avg == other.Avg
                    && EmployeeId == other.EmployeeId
                    && string.Equals(CustomerId, other.CustomerId);
        }

        [ConditionalFact]
        public virtual void GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg()
        {
            AssertQuery<Order>(
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

        [ConditionalFact]
        public virtual void GroupBy_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            AssertQuery<Order>(
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

        [ConditionalFact]
        public virtual void GroupBy_Constant_with_element_selector_Select_Sum()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => 2, o => new { o.OrderID, o.OrderDate }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                        }),
                e => e.Sum);
        }

        [ConditionalFact]
        public virtual void GroupBy_Constant_with_element_selector_Select_Sum2()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => 2, o => new { o.OrderID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                        }),
                e => e.Sum);
        }

        [ConditionalFact]
        public virtual void GroupBy_Constant_with_element_selector_Select_Sum3()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => 2, o => new { o.OrderID, o.OrderDate, o.CustomerID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                        }),
                e => e.Sum);
        }

        [ConditionalFact]
        public virtual void GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            AssertQuery<Order>(
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

        [ConditionalFact]
        public virtual void GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => 2, o => o.OrderID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(),
                            g.Key,
                        }),
                e => e.Sum);
        }

        [ConditionalFact]
        public virtual void GroupBy_param_Select_Sum_Min_Key_Max_Avg()
        {
            var a = 2;
            AssertQuery<Order>(
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

        [ConditionalFact]
        public virtual void GroupBy_param_with_element_selector_Select_Sum()
        {
            var a = 2;
            AssertQuery<Order>(
                os => os.GroupBy(o => a, o => new { o.OrderID, o.OrderDate }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                        }),
                e => e.Sum);
        }

        [ConditionalFact]
        public virtual void GroupBy_param_with_element_selector_Select_Sum2()
        {
            var a = 2;
            AssertQuery<Order>(
                os => os.GroupBy(o => a, o => new { o.OrderID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                        }),
                e => e.Sum);
        }

        [ConditionalFact]
        public virtual void GroupBy_param_with_element_selector_Select_Sum3()
        {
            var a = 2;
            AssertQuery<Order>(
                os => os.GroupBy(o => a, o => new { o.OrderID, o.OrderDate, o.CustomerID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                        }),
                e => e.Sum);
        }

        [ConditionalFact]
        public virtual void GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            var a = 2;
            AssertQuery<Order>(
                os => os.GroupBy(o => a, o => o.OrderID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(),
                            g.Key,
                        }),
                e => e.Sum);
        }

        #endregion

        #region GroupByWithElementSelectorAggregate

        [ConditionalFact]
        public virtual void GroupBy_Property_scalar_element_selector_Average()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Average()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_scalar_element_selector_Count()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_scalar_element_selector_LongCount()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_scalar_element_selector_Max()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Max()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_scalar_element_selector_Min()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Min()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_scalar_element_selector_Sum()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Property_anonymous_element_selector_Average()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_anonymous_element_selector_Count()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Count()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_anonymous_element_selector_LongCount()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.LongCount()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_anonymous_element_selector_Max()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_anonymous_element_selector_Min()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_anonymous_element_selector_Sum()
        {
            AssertQueryScalar<Order>(os => os.GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_empty_key_Aggregate()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.GroupBy(o => new { })
                        .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void GroupBy_empty_key_Aggregate_Key()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => new { })
                        .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy_Aggregate()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void OrderBy_Skip_GroupBy_Aggregate()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Skip(80)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Average(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void OrderBy_Take_GroupBy_Aggregate()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Take(500)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Min(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void OrderBy_Skip_Take_GroupBy_Aggregate()
        {
            AssertQueryScalar<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .Skip(80)
                        .Take(500)
                        .GroupBy(o => o.CustomerID)
                        .Select(g => g.Max(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void Distinct_GroupBy_Aggregate()
        {
            AssertQuery<Order>(
                os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void Anonymous_projection_Distinct_GroupBy_Aggregate()
        {
            AssertQuery<Order>(
                os =>
                    os.Select(o => new { o.OrderID, o.EmployeeID })
                        .Distinct()
                        .GroupBy(o => o.EmployeeID)
                        .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void SelectMany_GroupBy_Aggregate()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.SelectMany(c => c.Orders)
                        .GroupBy(o => o.EmployeeID)
                        .Select(g => new { g.Key, c = g.Count() }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void Join_GroupBy_Aggregate()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupBy_required_navigation_member_Aggregate()
        {
            AssertQuery<OrderDetail>(
                ods =>
                    ods.GroupBy(od => od.Order.CustomerID)
                    .Select(g =>
                        new
                        {
                            CustomerId = g.Key,
                            Count = g.Count()
                        }),
                e => e.CustomerId);
        }

        [ConditionalFact]
        public virtual void Join_complex_GroupBy_Aggregate()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from o in os.Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                     join c in cs.Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO").OrderBy(c => c.City).Skip(10).Take(50)
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupJoin_GroupBy_Aggregate()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     where o != null
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupJoin_GroupBy_Aggregate_2()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     select c)
                    .GroupBy(c => c.CustomerID)
                    .Select(g => new { g.Key, Max = g.Max(c => c.City) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupJoin_GroupBy_Aggregate_3()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping.DefaultIfEmpty()
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => new { g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupJoin_GroupBy_Aggregate_4()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from c in cs
                     join o in os
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     select c)
                    .GroupBy(c => c.CustomerID)
                    .Select(g => new { Value = g.Key, Max = g.Max(c => c.City) }),
                e => e.Value);
        }

        [ConditionalFact]
        public virtual void GroupJoin_GroupBy_Aggregate_5()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from o in os
                     join c in cs
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping.DefaultIfEmpty()
                     select o)
                    .GroupBy(o => o.OrderID)
                    .Select(g => new { Value = g.Key, Average = g.Average(o => o.OrderID) }),
                e => e.Value);
        }

        [ConditionalFact]
        public virtual void GroupBy_optional_navigation_member_Aggregate()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.Customer.Country)
                    .Select(g =>
                        new
                        {
                            Country = g.Key,
                            Count = g.Count()
                        }),
                e => e.Country);
        }

        [ConditionalFact]
        public virtual void GroupJoin_complex_GroupBy_Aggregate()
        {
            AssertQuery<Order, Customer>(
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
        public virtual void Self_join_GroupBy_Aggregate()
        {
            AssertQuery<Order, Order>(
                (os1, os2) =>
                    (from o1 in os1.Where(o => o.OrderID < 10400)
                     join o2 in os2
                         on o1.OrderID equals o2.OrderID
                     group o2 by o1.CustomerID)
                    .Select(g => new { g.Key, Count = g.Average(o => o.OrderID) }),
                e => e.Key);
        }

        [ConditionalFact]
        public virtual void GroupBy_multi_navigation_members_Aggregate()
        {
            AssertQuery<OrderDetail>(
                ods =>
                    ods.GroupBy(od => new { od.Order.CustomerID, od.Product.ProductName })
                    .Select(g =>
                        new
                        {
                            CompositeKey = g.Key,
                            Count = g.Count()
                        }),
                e => e.CompositeKey.CustomerID + " " + e.CompositeKey.ProductName);
        }

        [ConditionalFact(Skip = "Unable to bind group by. See Issue#6658")]
        public virtual void Union_simple_groupby()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void Select_anonymous_GroupBy_Aggregate()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10300)
                        .Select(o => new { A = o.CustomerID, B = o.OrderDate, C = o.OrderID })
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

        #endregion

        #region GroupByAggregateComposition

        [ConditionalFact]
        public virtual void GroupBy_OrderBy_key()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Key)
                        .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_OrderBy_count()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Count())
                        .ThenBy(o => o.Key)
                        .Select(g => new { g.Key, Count = g.Count() }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_OrderBy_count_Select_sum()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Count())
                        .ThenBy(o => o.Key)
                        .Select(g => new { g.Key, Sum = g.Sum(o => o.OrderID) }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_aggregate_Contains()
        {
            AssertQuery<Order>(
                os => os.Where(
                    o => os.GroupBy(e => e.CustomerID)
                        .Where(g => g.Count() > 30)
                        .Select(g => g.Key)
                        .Contains(o.CustomerID)),
                entryCount: 31);
        }

        [ConditionalFact]
        public virtual void GroupBy_aggregate_Pushdown()
        {
            AssertQuery<Order>(
                os => os.GroupBy(e => e.CustomerID)
                        .Where(g => g.Count() > 10)
                        .Select(g => g.Key)
                        .OrderBy(t => t)
                        .Take(20)
                        .Skip(4));
        }

        [ConditionalFact]
        public virtual void GroupBy_Select_sum_over_unmapped_property()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders
                        .GroupBy(o => o.CustomerID)
                        .Select(g => new { g.Key, Sum = g.Sum(o => o.Freight) })
                        .ToList();

                // Do not do deep assertion of result. We don't have data for unmapped property in EF model
                Assert.Equal(89, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void GroupBy_filter_key()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .Where(o => o.Key == "ALFKI")
                        .Select(g => new { g.Key, c = g.Count() }));
        }

        [ConditionalFact]
        public virtual void GroupBy_filter_count()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .Where(o => o.Count() > 4)
                        .Select(g => new { g.Key, Count = g.Count() }));
        }

        [ConditionalFact]
        public virtual void GroupBy_filter_count_OrderBy_count_Select_sum()
        {
            AssertQuery<Order>(
                os =>
                    os.GroupBy(o => o.CustomerID)
                        .Where(o => o.Count() > 4)
                        .OrderBy(o => o.Count())
                        .ThenBy(o => o.Key)
                        .Select(g => new { g.Key, Count = g.Count(), Sum = g.Sum(o => o.OrderID) }));
        }

        [ConditionalFact]
        public virtual void GroupBy_Aggregate_Join()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    from a in os.GroupBy(o => o.CustomerID)
                                .Where(g => g.Count() > 5)
                                .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                    join c in cs on a.CustomerID equals c.CustomerID
                    join o in os on a.LastOrderID equals o.OrderID
                    select new { c, o },
                entryCount: 126);
        }

        [ConditionalFact]
        public virtual void Join_GroupBy_Aggregate_multijoins()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    from c in cs
                    join a in os.GroupBy(o => o.CustomerID)
                                .Where(g => g.Count() > 5)
                                .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID
                    join o in os on a.LastOrderID equals o.OrderID
                    select new { c, o },
                entryCount: 126);
        }

        [ConditionalFact]
        public virtual void Join_GroupBy_Aggregate_single_join()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    from c in cs
                    join a in os.GroupBy(o => o.CustomerID)
                                .Where(g => g.Count() > 5)
                                .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID
                    select new { c, a.LastOrderID },
                entryCount: 63);
        }

        [ConditionalFact]
        public virtual void Join_GroupBy_Aggregate_with_another_join()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    from c in cs
                    join a in os.GroupBy(o => o.CustomerID)
                                .Where(g => g.Count() > 5)
                                .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                        on c.CustomerID equals a.CustomerID
                    join o in os on c.CustomerID equals o.CustomerID into grouping
                    from g in grouping
                    select new { c, a.LastOrderID, g.OrderID },
                entryCount: 63);
        }


        [ConditionalFact]
        public virtual void Join_GroupBy_Aggregate_in_subquery()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    from o in os.Where(o => o.OrderID < 10400)
                    join i in (from c in cs
                               join a in os.GroupBy(o => o.CustomerID)
                                           .Where(g => g.Count() > 5)
                                           .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                                   on c.CustomerID equals a.CustomerID
                               select new { c, a.LastOrderID })
                        on o.CustomerID equals i.c.CustomerID
                    select new { o, i.c, i.c.CustomerID },
                entryCount: 133);
        }

        [ConditionalFact]
        public virtual void Join_GroupBy_Aggregate_on_key()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    (from c in cs
                     join a in os.GroupBy(o => o.CustomerID)
                                 .Where(g => g.Count() > 5)
                                 .Select(g => new { g.Key, LastOrderID = g.Max(o => o.OrderID) })
                         on c.CustomerID equals a.Key
                     select new { c, a.LastOrderID }),
                e => e.c.CustomerID,
                entryCount: 63);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_result_selector()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_Sum_constant()
        {
            AssertQueryScalar<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(e => 1)));
        }

        [ConditionalFact]
        public virtual void GroupBy_Sum_constant_cast()
        {
            AssertQueryScalar<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(e => 1L)));
        }

        [ConditionalFact]
        public virtual void Distinct_GroupBy_OrderBy_key()
        {
            AssertQuery<Order>(
                os =>
                    os.Distinct()
                        .GroupBy(o => o.CustomerID)
                        .OrderBy(o => o.Key)
                        .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_with_groupby()
        {
            using (var context = CreateContext())
            {
                var expected = context.Customers
                    .Include(c => c.Orders)
                    // ReSharper disable once StringStartsWithIsCultureSpecific
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .ToList()
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : Array.Empty<int>()).ToList();

                ClearLog();

                var query = context.Customers
                    // ReSharper disable once StringStartsWithIsCultureSpecific
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                            : Array.Empty<int>());

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_GroupBy_All()
        {
            using (var context = CreateContext())
            {
                Assert.False(
                    context
                        .Set<Order>()
                        .Select(
                            o => new ProjectedType
                            {
                                Order = o.OrderID,
                                Customer = o.CustomerID
                            })
                        .GroupBy(a => a.Customer)
                        .All(a => a.Key == "ALFKI")
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
                if (obj is null)
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

        [ConditionalFact]
        public virtual void GroupBy_Where_in_aggregate()
        {
            AssertQueryScalar<Order>(
                os => from o in os
                      group o by new { o.CustomerID } into g
                      select g.Where(e => e.OrderID < 10300).Count());
        }

        #endregion

        #region GroupByWithoutAggregate

        [ConditionalFact]
        public virtual void GroupBy_anonymous()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.CustomerID }).GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_with_where()
        {
            var countries = new[] { "Argentina", "Austria", "Brazil", "France", "Germany", "USA" };
            AssertQuery<Customer>(
                cs => cs.Where(c => countries.Contains(c.Country))
                    .Select(c => new { c.City, c.CustomerID })
                    .GroupBy(a => a.City),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        [ConditionalFact(Skip = "Test does not pass. See issue#7160")]
        public virtual void GroupBy_anonymous_subquery()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Select(c => new { c.City, c.CustomerID })
                        .GroupBy(a => from c2 in cs select c2),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_nested_order_by_enumerable()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Select(c => new { c.Country, c.CustomerID })
                        .OrderBy(a => a.Country)
                        .GroupBy(a => a.Country)
                        .Select(g => g.OrderBy(a => a.CustomerID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_join_default_if_empty_anonymous()
        {
            AssertQuery<Order, OrderDetail>(
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
        public virtual void GroupBy_SelectMany()
        {
            AssertQuery<Customer>(
                cs => cs.GroupBy(c => c.City).SelectMany(g => g),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void GroupBy_simple()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_simple2()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g),
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_first()
        {
            AssertSingleResult<Order>(
                os => os.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Cast<object>().First(),
                asserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<int>());
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector2()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o.OrderID)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>());
        }

        [ConditionalFact]
        public virtual void GroupBy_with_element_selector3()
        {
            AssertQuery<Employee>(
                es => es.GroupBy(e => e.EmployeeID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Select(e => new { Title = EF.Property<string>(e, "Title"), e }).ToList()),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupBy_DateTimeOffset_Property()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate.HasValue).GroupBy(o => o.OrderDate.Value.Month),
                e => ((IGrouping<int, Order>)e).Key,
                elementAsserter: GroupingAsserter<int, Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy_SelectMany()
        {
            AssertQuery<Order>(
                os =>
                    os.OrderBy(o => o.OrderID)
                        .GroupBy(o => o.CustomerID)
                        .SelectMany(g => g),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void OrderBy_GroupBy_SelectMany_shadow()
        {
            AssertQuery<Employee>(
                es =>
                    es.OrderBy(e => e.EmployeeID)
                        .GroupBy(e => e.EmployeeID)
                        .SelectMany(g => g)
                        .Select(g => EF.Property<string>(g, "Title")));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).OrderBy(g => g.Key),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void GroupBy_with_orderby_and_anonymous_projection()
        {
            AssertQuery<Order>(
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
        public virtual void GroupBy_with_orderby_take_skip_distinct()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).OrderBy(g => g.Key).Take(5).Skip(3).Distinct(),
                assertOrder: true,
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID),
                entryCount: 31);
        }

        [ConditionalFact]
        public virtual void GroupBy_join_anonymous()
        {
            AssertQuery<Order, OrderDetail>(
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
        public virtual void GroupBy_Distinct()
        {
            AssertQuery<Order>(
                os =>
                    // TODO: See issue#11215
                    os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [ConditionalFact]
        public virtual void OrderBy_Skip_GroupBy()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderID).Skip(800).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 30);
        }

        [ConditionalFact]
        public virtual void OrderBy_Take_GroupBy()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }

        [ConditionalFact]
        public virtual void OrderBy_Skip_Take_GroupBy()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderDate).Skip(450).Take(50).GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.OrderDate),
                entryCount: 50);
        }

        [ConditionalFact]
        public virtual void Select_Distinct_GroupBy()
        {
            AssertQuery<Order>(
                os => os.Select(o => new { o.CustomerID, o.EmployeeID }).OrderBy(a => a.EmployeeID).Distinct().GroupBy(o => o.CustomerID),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.EmployeeID));
        }

        [ConditionalFact]
        public virtual void GroupBy_with_aggregate_through_navigation_property()
        {
            AssertQuery<Order>(
                os => os.GroupBy(c => c.EmployeeID).Select(g => new { max = g.Max(i => i.Customer.Region) }),
                elementSorter: e => e.max);
        }

        [ConditionalFact]
        public virtual void GroupBy_anonymous_key_without_aggregate()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => new { g.Key, g })
                    .ToList()
                    .OrderBy(g => g.Key + " " + g.g.Count()).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => new { g.Key, g })
                    .ToList()
                    .OrderBy(g => g.Key + " " + g.g.Count()).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].g.Count(), actual[i].g.Count());
                }
            }
        }

        #endregion

        #region GroupBySelectFirst

        [ConditionalFact]
        public virtual void GroupBy_Shadow()
        {
            AssertQuery<Employee>(
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow2()
        {
            AssertQuery<Employee>(
                es =>
                    es.Where(
                            e => EF.Property<string>(e, "Title") == "Sales Representative"
                                 && e.EmployeeID == 1)
                        .GroupBy(e => EF.Property<string>(e, "Title"))
                        .Select(g => g.First()));
        }

        [ConditionalFact]
        public virtual void GroupBy_Shadow3()
        {
            AssertQuery<Employee>(
                es =>
                    es.Where(e => e.EmployeeID == 1)
                        .GroupBy(e => e.EmployeeID)
                        .Select(g => EF.Property<string>(g.First(), "Title")));
        }



        [ConditionalFact]
        public virtual void GroupBy_Select_First_GroupBy()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.GroupBy(c => c.City)
                      .Select(g => g.OrderBy(c => c.CustomerID).First())
                      .GroupBy(c => c.ContactName),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID));
        }

        #endregion

        #region GroupByEntityType

        [ConditionalFact]
        public virtual void Select_GroupBy()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>().Select(
                    o => new ProjectedType
                    {
                        Order = o.OrderID,
                        Customer = o.CustomerID
                    }).GroupBy(p => p.Customer).ToList().OrderBy(g => g.Key + " " + g.Count()).ToList();

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
        public virtual void Select_GroupBy_SelectMany()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>().Select(
                        o => new ProjectedType
                        {
                            Order = o.OrderID,
                            Customer = o.CustomerID
                        })
                    .GroupBy(o => o.Order)
                    .SelectMany(g => g).ToList().OrderBy(e => e.Order).ToList();

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

        [ConditionalFact]
        public virtual void Join_GroupBy_entity_ToList()
        {
            using (var context = CreateContext())
            {
                var actual = (from c in context.Customers.OrderBy(c => c.CustomerID).Take(5)
                              join o in context.Orders.OrderBy(o => o.OrderID).Take(50)
                                on c.CustomerID equals o.CustomerID
                              group o by c into grp
                              select new
                              {
                                  C = grp.Key,
                                  Os = grp.ToList()
                              }).ToList();

                var expected = (from c in Fixture.QueryAsserter.ExpectedData.Set<Customer>()
                                            .OrderBy(c => c.CustomerID).Take(5)
                                join o in Fixture.QueryAsserter.ExpectedData.Set<Order>()
                                            .OrderBy(o => o.OrderID).Take(50)
                                    on c.CustomerID equals o.CustomerID
                                group o by c into grp
                                select new
                                {
                                    C = grp.Key,
                                    Os = grp.ToList()
                                }).ToList();

                Assert.Equal(expected.Count, actual.Count);

                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].C, actual[i].C);
                    Assert.Equal(expected[i].Os, actual[i].Os);
                }
            }
        }

        #endregion

        #region DoubleGroupBy

        [ConditionalFact]
        public virtual void Double_GroupBy_with_aggregate()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .GroupBy(o => new { o.OrderID, o.OrderDate })
                    .GroupBy(g => g.Key.OrderDate)
                    .Select(g => new { g.Key, Lastest = g.OrderBy(e => e.Key.OrderID).FirstOrDefault() })
                    .ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .GroupBy(o => new { o.OrderID, o.OrderDate })
                    .GroupBy(g => g.Key.OrderDate)
                    .Select(g => new { g.Key, Lastest = g.OrderBy(e => e.Key.OrderID).FirstOrDefault() })
                    .ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Lastest.Key, actual[i].Lastest.Key);
                    Assert.Equal(expected[i].Lastest.Count(), actual[i].Lastest.Count());
                }
            }
        }

        #endregion

        #region ResultOperatorsAfterGroupBy

        [ConditionalFact]
        public virtual void Count_after_GroupBy_aggregate()
        {
            AssertSingleResult<Order>(
                os =>os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).Count());
        }

        [ConditionalFact]
        public virtual void LongCount_after_client_GroupBy()
        {
            AssertSingleResult<Order>(
                os => (from o in os
                      group o by new { o.CustomerID } into g
                      select g.Where(e => e.OrderID < 10300).Count()).LongCount());
        }

        [ConditionalFact]
        public virtual void MinMax_after_GroupBy_aggregate()
        {
            AssertSingleResult<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).Min());

            AssertSingleResult<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).Max());
        }

        [ConditionalFact]
        public virtual void AllAny_after_GroupBy_aggregate()
        {
            AssertSingleResult<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).All(ee => true));

            AssertSingleResult<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g.Sum(gg => gg.OrderID)).Any());
        }

        #endregion
    }
}
