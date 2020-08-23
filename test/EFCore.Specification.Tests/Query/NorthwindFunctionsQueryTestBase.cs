// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable SpecifyACultureInStringConversionExplicitly
#pragma warning disable RCS1215 // Expression is always equal to true/false.
#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
namespace Microsoft.EntityFrameworkCore.Query
{
    // ReSharper disable once UnusedTypeParameter
    public abstract class NorthwindFunctionsQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindFunctionsQueryTestBase(TFixture fixture)
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
        public virtual Task String_StartsWith_Literal(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith("M")),
                entryCount: 12);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_StartsWith_Identity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_StartsWith_Column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_StartsWith_MethodCall(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith(LocalMethod1())),
                entryCount: 12);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_EndsWith_Literal(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith("b")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_EndsWith_Identity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_EndsWith_Column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_EndsWith_MethodCall(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith(LocalMethod2())),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_Contains_Literal(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("M")),
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_Contains_Identity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_Contains_Column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_FirstOrDefault_MethodCall(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.FirstOrDefault() == 'A'),
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_Contains_constant_with_whitespace(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("     ")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_Contains_parameter_with_whitespace(bool async)
        {
            var pattern = "     ";
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(pattern)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_LastOrDefault_MethodCall(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.LastOrDefault() == 's'),
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_Contains_MethodCall(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(LocalMethod1())),
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_simple_zero(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") == 0),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 != string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") > 0),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 >= string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 < string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") <= 0),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_simple_one(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") == 1),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 == string.Compare(c.CustomerID, "ALFKI")));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") < 1),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 1 > string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") > -1),
                entryCount: 91);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 < string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_compare_with_parameter(bool async)
        {
            Customer customer = null;
            using (var context = CreateContext())
            {
                customer = context.Customers.OrderBy(c => c.CustomerID).First();
            }

            ClearLog();

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, customer.CustomerID) == 1),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 == string.Compare(c.CustomerID, customer.CustomerID)));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, customer.CustomerID) < 1),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 1 > string.Compare(c.CustomerID, customer.CustomerID)),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, customer.CustomerID) > -1),
                entryCount: 91);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 < string.Compare(c.CustomerID, customer.CustomerID)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_simple_more_than_one(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") == 42));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") > 42));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 42 > string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_nested(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "M" + c.CustomerID) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 != string.Compare(c.CustomerID, c.CustomerID.ToUpper())));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 >= string.Compare(c.CustomerID, "M" + c.CustomerID)),
                entryCount: 51);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 1 == string.Compare(c.CustomerID, c.CustomerID.ToUpper())));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) == -1),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_multi_predicate(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") > -1)
                    .Where(c => string.Compare(c.CustomerID, "CACTU") == -1),
                entryCount: 11);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Compare(c.ContactTitle, "Owner") == 0)
                    .Where(c => string.Compare(c.Country, "USA") != 0),
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_to_simple_zero(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") == 0),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 != c.CustomerID.CompareTo("ALFKI")),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") > 0),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 >= c.CustomerID.CompareTo("ALFKI")),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 < c.CustomerID.CompareTo("ALFKI")),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") <= 0),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_to_simple_one(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") == 1),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 == c.CustomerID.CompareTo("ALFKI")));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") < 1),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 1 > c.CustomerID.CompareTo("ALFKI")),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") > -1),
                entryCount: 91);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 < c.CustomerID.CompareTo("ALFKI")),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_compare_to_with_parameter(bool async)
        {
            Customer customer = null;
            using (var context = CreateContext())
            {
                customer = context.Customers.OrderBy(c => c.CustomerID).First();
            }

            ClearLog();

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo(customer.CustomerID) == 1),
                entryCount: 90);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 == c.CustomerID.CompareTo(customer.CustomerID)));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo(customer.CustomerID) < 1),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 1 > c.CustomerID.CompareTo(customer.CustomerID)),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo(customer.CustomerID) > -1),
                entryCount: 91);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => -1 < c.CustomerID.CompareTo(customer.CustomerID)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_to_simple_more_than_one(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") == 42));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") > 42));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 42 > c.CustomerID.CompareTo("ALFKI")),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_to_nested(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("M" + c.CustomerID) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 != c.CustomerID.CompareTo(c.CustomerID.ToUpper())));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 0 >= c.CustomerID.CompareTo("M" + c.CustomerID)),
                entryCount: 51);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => 1 == c.CustomerID.CompareTo(c.CustomerID.ToUpper())));

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) == -1),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_Compare_to_multi_predicate(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") > -1).Where(c => c.CustomerID.CompareTo("CACTU") == -1),
                entryCount: 11);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.CompareTo("Owner") == 0).Where(c => c.Country.CompareTo("USA") != 0),
                entryCount: 15);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
        {
            var myDatetime = new DateTime(1998, 5, 4);

            if (compareTo)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) == 0),
                    entryCount: 3);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 != c.OrderDate.Value.CompareTo(myDatetime)),
                    entryCount: 827);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) > 0),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 >= c.OrderDate.Value.CompareTo(myDatetime)),
                    entryCount: 822);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 < c.OrderDate.Value.CompareTo(myDatetime)),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) <= 0),
                    entryCount: 822);
            }
            else
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) == 0),
                    entryCount: 3);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 != DateTime.Compare(c.OrderDate.Value, myDatetime)),
                    entryCount: 827);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) > 0),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 >= DateTime.Compare(c.OrderDate.Value, myDatetime)),
                    entryCount: 822);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 < DateTime.Compare(c.OrderDate.Value, myDatetime)),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) <= 0),
                    entryCount: 822);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task TimeSpan_Compare_to_simple_zero(bool async, bool compareTo)
        {
            var myDatetime = new DateTime(1998, 5, 4);

            if (compareTo)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) == 0),
                    entryCount: 3);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 != c.OrderDate.Value.CompareTo(myDatetime)),
                    entryCount: 827);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) > 0),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 >= c.OrderDate.Value.CompareTo(myDatetime)),
                    entryCount: 822);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 < c.OrderDate.Value.CompareTo(myDatetime)),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) <= 0),
                    entryCount: 822);
            }
            else
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) == 0),
                    entryCount: 3);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 != DateTime.Compare(c.OrderDate.Value, myDatetime)),
                    entryCount: 827);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) > 0),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 >= DateTime.Compare(c.OrderDate.Value, myDatetime)),
                    entryCount: 822);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => 0 < DateTime.Compare(c.OrderDate.Value, myDatetime)),
                    entryCount: 8);

                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) <= 0),
                    entryCount: 822);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Int_Compare_to_simple_zero(bool async)
        {
            var orderId = 10250;

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.OrderID.CompareTo(orderId) == 0),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 != c.OrderID.CompareTo(orderId)),
                entryCount: 829);

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.OrderID.CompareTo(orderId) > 0),
                entryCount: 827);

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 >= c.OrderID.CompareTo(orderId)),
                entryCount: 3);

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 < c.OrderID.CompareTo(orderId)),
                entryCount: 827);

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.OrderID.CompareTo(orderId) <= 0),
                entryCount: 3);
        }

        protected static string LocalMethod1()
            => "M";

        protected static string LocalMethod2()
            => "m";

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_abs1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Abs(od.ProductID) > 10),
                entryCount: 1939);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_abs2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Abs(od.Quantity) > 10),
                entryCount: 1547);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_abs3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Abs(od.UnitPrice) > 10),
                entryCount: 1677);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_abs_uncorrelated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Abs(-10) < od.ProductID),
                entryCount: 1939);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_ceiling1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Ceiling(od.Discount) > 0),
                entryCount: 838);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_ceiling2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Ceiling(od.UnitPrice) > 10),
                entryCount: 1677);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_floor(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Floor(od.UnitPrice) > 10),
                entryCount: 1658);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_power(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Pow(od.Discount, 2) > 0.05f),
                entryCount: 154);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_round(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Round(od.UnitPrice) > 10),
                entryCount: 1662);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_math_round_int(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.OrderID < 10250)
                    .Select(o => new { A = Math.Round((double)o.OrderID) }),
                e => e.A);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_math_truncate_int(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.OrderID < 10250)
                    .Select(o => new { A = Math.Truncate((double)o.OrderID) }),
                e => e.A);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_round2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Round(od.UnitPrice, 2) > 100),
                entryCount: 46);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_truncate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Math.Truncate(od.UnitPrice) > 10),
                entryCount: 1658);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_exp(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Exp(od.Discount) > 1),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_log10(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log10(od.Discount) < 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_log(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log(od.Discount) < 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_log_new_base(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log(od.Discount, 7) < 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_sqrt(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Sqrt(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_acos(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Acos(od.Discount) > 1),
                entryCount: 25);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_asin(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Asin(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_atan(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Atan(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_atan2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Atan2(od.Discount, 1) > 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_cos(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Cos(od.Discount) > 0),
                entryCount: 25);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_sin(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Sin(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_tan(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Tan(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_sign(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Sign(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_max(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Max(od.OrderID, od.ProductID) == od.OrderID),
                entryCount: 25);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_math_min(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077)
                    .Where(od => Math.Min(od.OrderID, od.ProductID) == od.ProductID),
                entryCount: 25);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_guid_newguid(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => Guid.NewGuid() != default),
                entryCount: 2155);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_string_to_upper(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.ToUpper() == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_string_to_lower(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.ToLower() == "alfki"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_functions_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => Math.Pow(c.CustomerID.Length, 2) == 25),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToBoolean(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToBoolean(Convert.ToBoolean(o.OrderID % 3)),
                o => Convert.ToBoolean(Convert.ToByte(o.OrderID % 3)),
                o => Convert.ToBoolean(Convert.ToDecimal(o.OrderID % 3)),
                o => Convert.ToBoolean(Convert.ToDouble(o.OrderID % 3)),
                o => Convert.ToBoolean((float)Convert.ToDouble(o.OrderID % 3)),
                o => Convert.ToBoolean(Convert.ToInt16(o.OrderID % 3)),
                o => Convert.ToBoolean(Convert.ToInt32(o.OrderID % 3)),
                o => Convert.ToBoolean(Convert.ToInt64(o.OrderID % 3)),
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 5);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToByte(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToByte(Convert.ToBoolean(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToByte((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToString(o.OrderID % 1)) >= 0
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToDecimal(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToDecimal(Convert.ToBoolean(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToString(o.OrderID % 1)) >= 0
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToDouble(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToDouble(Convert.ToBoolean(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToString(o.OrderID % 1)) >= 0
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToInt16(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToInt16(Convert.ToBoolean(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToString(o.OrderID % 1)) >= 0
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToInt32(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToInt32(Convert.ToBoolean(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToString(o.OrderID % 1)) >= 0
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToInt64(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToInt64(Convert.ToBoolean(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToString(o.OrderID % 1)) >= 0
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Convert_ToString(bool async)
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToString(Convert.ToBoolean(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToByte(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToDecimal(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToDouble(o.OrderID % 1)) != "10",
                o => Convert.ToString((float)Convert.ToDouble(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt16(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt32(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt64(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToString(o.OrderID % 1)) != "10",
                o => Convert.ToString(o.OrderDate.Value).Contains("1997") || Convert.ToString(o.OrderDate.Value).Contains("1998")
            };

            foreach (var convertMethod in convertMethods)
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Indexof_with_emptystring(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.IndexOf(string.Empty)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Replace_with_emptystring(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Replace("ari", string.Empty)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Substring_with_zero_startindex(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(0, 3)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Substring_with_zero_length(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(2, 0)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Substring_with_constant(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(1, 3)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Substring_with_closure(bool async)
        {
            var start = 2;

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(start, 3)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Substring_with_Index_of(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                    .Select(c => c.ContactName.Substring(c.ContactName.IndexOf("a"), 3)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsNullOrEmpty_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.IsNullOrEmpty(c.Region)),
                entryCount: 60);
        }

        [ConditionalFact]
        public virtual void IsNullOrEmpty_in_projection()
        {
            using var context = CreateContext();
            var query = context.Set<Customer>()
                .Select(
                    c => new { Id = c.CustomerID, Value = string.IsNullOrEmpty(c.Region) })
                .ToList();

            Assert.Equal(91, query.Count);
        }

        [ConditionalFact]
        public virtual void IsNullOrEmpty_negated_in_projection()
        {
            using var context = CreateContext();
            var query = context.Set<Customer>()
                .Select(
                    c => new { Id = c.CustomerID, Value = !string.IsNullOrEmpty(c.Region) })
                .ToList();

            Assert.Equal(91, query.Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsNullOrWhiteSpace_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.IsNullOrWhiteSpace(c.Region)),
                entryCount: 60);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.IsNullOrWhiteSpace(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task TrimStart_without_arguments_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimStart() == "Owner"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task TrimStart_with_char_argument_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimStart('O') == "wner"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task TrimStart_with_char_array_argument_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimStart('O', 'w') == "ner"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task TrimEnd_without_arguments_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimEnd() == "Owner"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task TrimEnd_with_char_argument_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimEnd('r') == "Owne"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task TrimEnd_with_char_array_argument_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimEnd('e', 'r') == "Own"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Trim_without_argument_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.Trim() == "Owner"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Trim_with_char_argument_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.Trim('O') == "wner"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Trim_with_char_array_argument_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactTitle.Trim('O', 'r') == "wne"),
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_length_twice(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID)
                    .Select(c => c.Orders),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a),
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Static_string_equals_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => string.Equals(c.CustomerID, "ANATR")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Static_equals_nullable_datetime_compared_to_non_nullable(bool async)
        {
            var arg = new DateTime(1996, 7, 4);

            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => Equals(o.OrderDate, arg)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Static_equals_int_compared_to_long(bool async)
        {
            long arg = 10248;

            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => Equals(o.OrderID, arg)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(o => new { A = Math.Truncate((double)o.OrderID) })
                    .OrderBy(r => r.A)
                    .OrderBy(r => r.A),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(o => new { A = Math.Truncate((double)o.OrderID) })
                    .OrderBy(r => r.A)
                    .OrderByDescending(r => r.A),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(o => new { A = Math.Truncate((double)o.OrderID) })
                    .OrderByDescending(r => r.A)
                    .ThenBy(r => r.A),
                assertOrder: true);
        }
    }
}
