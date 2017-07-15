// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Microsoft.EntityFrameworkCore.Query
{
    // ReSharper disable once UnusedTypeParameter
    public abstract partial class SimpleQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
        public virtual void String_StartsWith_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith("M")),
                entryCount: 12);
        }

        [ConditionalFact]
        public virtual void String_StartsWith_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_StartsWith_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_StartsWith_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(LocalMethod1())),
                entryCount: 12);
        }

        [ConditionalFact]
        public virtual void String_EndsWith_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith("b")),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void String_EndsWith_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_EndsWith_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_EndsWith_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(LocalMethod2())),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")),
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual void String_Contains_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Contains_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())),
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual void String_Compare_simple_zero()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") == 0),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 != string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") > 0),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 >= string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 < string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") <= 0),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void String_Compare_simple_one()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") == 1),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 == string.Compare(c.CustomerID, "ALFKI")));

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") < 1),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 1 > string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") > -1),
                entryCount: 91);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 < string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_compare_with_parameter()
        {
            Customer customer = null;
            using (var context = CreateContext())
            {
                customer = context.Customers.OrderBy(c => c.CustomerID).First();
            }

            ClearLog();

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, customer.CustomerID) == 1),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 == string.Compare(c.CustomerID, customer.CustomerID)));

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, customer.CustomerID) < 1),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 1 > string.Compare(c.CustomerID, customer.CustomerID)),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, customer.CustomerID) > -1),
                entryCount: 91);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 < string.Compare(c.CustomerID, customer.CustomerID)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Compare_simple_client()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") == 42));

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") > 42));

            AssertQuery<Customer>(
                cs => cs.Where(c => 42 > string.Compare(c.CustomerID, "ALFKI")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Compare_nested()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "M" + c.CustomerID) == 0));

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 != string.Compare(c.CustomerID, c.CustomerID.ToUpper())));

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) > 0));

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 >= string.Compare(c.CustomerID, "M" + c.CustomerID)),
                entryCount: 51);

            AssertQuery<Customer>(
                cs => cs.Where(c => 1 == string.Compare(c.CustomerID, c.CustomerID.ToUpper())));

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) == -1),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Compare_multi_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.CustomerID, "ALFKI") > -1).Where(c => string.Compare(c.CustomerID, "CACTU") == -1),
                entryCount: 11);

            AssertQuery<Customer>(
                cs => cs.Where(c => string.Compare(c.ContactTitle, "Owner") == 0).Where(c => string.Compare(c.Country, "USA") != 0),
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual void String_Compare_to_simple_zero()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") == 0),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 != c.CustomerID.CompareTo("ALFKI")),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") > 0),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 >= c.CustomerID.CompareTo("ALFKI")),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 < c.CustomerID.CompareTo("ALFKI")),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") <= 0),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void String_Compare_to_simple_one()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") == 1),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 == c.CustomerID.CompareTo("ALFKI")));

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") < 1),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 1 > c.CustomerID.CompareTo("ALFKI")),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") > -1),
                entryCount: 91);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 < c.CustomerID.CompareTo("ALFKI")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_compare_to_with_parameter()
        {
            Customer customer = null;
            using (var context = CreateContext())
            {
                customer = context.Customers.OrderBy(c => c.CustomerID).First();
            }

            ClearLog();

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo(customer.CustomerID) == 1),
                entryCount: 90);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 == c.CustomerID.CompareTo(customer.CustomerID)));

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo(customer.CustomerID) < 1),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => 1 > c.CustomerID.CompareTo(customer.CustomerID)),
                entryCount: 1);

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo(customer.CustomerID) > -1),
                entryCount: 91);

            AssertQuery<Customer>(
                cs => cs.Where(c => -1 < c.CustomerID.CompareTo(customer.CustomerID)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Compare_to_simple_client()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") == 42));

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") > 42));

            AssertQuery<Customer>(
                cs => cs.Where(c => 42 > c.CustomerID.CompareTo("ALFKI")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Compare_to_nested()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("M" + c.CustomerID) == 0));

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 != c.CustomerID.CompareTo(c.CustomerID.ToUpper())));

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) > 0));

            AssertQuery<Customer>(
                cs => cs.Where(c => 0 >= c.CustomerID.CompareTo("M" + c.CustomerID)),
                entryCount: 51);

            AssertQuery<Customer>(
                cs => cs.Where(c => 1 == c.CustomerID.CompareTo(c.CustomerID.ToUpper())));

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) == -1),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void String_Compare_to_multi_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.CompareTo("ALFKI") > -1).Where(c => c.CustomerID.CompareTo("CACTU") == -1),
                entryCount: 11);

            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.CompareTo("Owner") == 0).Where(c => c.Country.CompareTo("USA") != 0),
                entryCount: 15);
        }

        protected static string LocalMethod1()
        {
            return "M";
        }

        protected static string LocalMethod2()
        {
            return "m";
        }

        [ConditionalFact]
        public virtual void Where_math_abs1()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(od.ProductID) > 10),
                entryCount: 1939);
        }

        [ConditionalFact]
        public virtual void Where_math_abs2()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(od.Quantity) > 10),
                entryCount: 1547);
        }

        [ConditionalFact]
        public virtual void Where_math_abs3()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(od.UnitPrice) > 10),
                entryCount: 1677);
        }

        [ConditionalFact]
        public virtual void Where_math_abs_uncorrelated()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(-10) < od.ProductID),
                entryCount: 1939);
        }

        [ConditionalFact]
        public virtual void Where_math_ceiling1()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Ceiling(od.Discount) > 0),
                entryCount: 838);
        }

        [ConditionalFact]
        public virtual void Where_math_ceiling2()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Ceiling(od.UnitPrice) > 10),
                entryCount: 1677);
        }

        [ConditionalFact]
        public virtual void Where_math_floor()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Floor(od.UnitPrice) > 10),
                entryCount: 1658);
        }

        [ConditionalFact]
        public virtual void Where_math_power()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Pow(od.Discount, 2) > 0.05f),
                entryCount: 154);
        }

        [ConditionalFact]
        public virtual void Where_math_round()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Round(od.UnitPrice) > 10),
                entryCount: 1662);
        }

        [ConditionalFact]
        public virtual void Select_math_round_int()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10250).Select(o => new { A = Math.Round((double)o.OrderID) }),
                e => e.A);
        }

        [ConditionalFact]
        public virtual void Select_math_truncate_int()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10250).Select(o => new { A = Math.Truncate((double)o.OrderID) }),
                e => e.A);
        }

        [ConditionalFact]
        public virtual void Where_math_round2()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Round(od.UnitPrice, 2) > 100),
                entryCount: 46);
        }

        [ConditionalFact]
        public virtual void Where_math_truncate()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Truncate(od.UnitPrice) > 10),
                entryCount: 1658);
        }

        [ConditionalFact]
        public virtual void Where_math_exp()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Exp(od.Discount) > 1),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_log10()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log10(od.Discount) < 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_log()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log(od.Discount) < 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_log_new_base()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log(od.Discount, 7) < 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_sqrt()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Sqrt(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_acos()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Acos(od.Discount) > 1),
                entryCount: 25);
        }

        [ConditionalFact]
        public virtual void Where_math_asin()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Asin(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_atan()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Atan(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_atan2()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Atan2(od.Discount, 1) > 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_cos()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Cos(od.Discount) > 0),
                entryCount: 25);
        }

        [ConditionalFact]
        public virtual void Where_math_sin()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Sin(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_tan()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Tan(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_math_sign()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => od.OrderID == 11077).Where(od => Math.Sign(od.Discount) > 0),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Where_guid_newguid()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Guid.NewGuid() != default(Guid)),
                entryCount: 2155);
        }

        [ConditionalFact]
        public virtual void Where_string_to_upper()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.ToUpper() == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_string_to_lower()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.ToLower() == "alfki"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_functions_nested()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Math.Pow(c.CustomerID.Length, 2) == 25),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Convert_ToByte()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
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
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalFact]
        public virtual void Convert_ToDecimal()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
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
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalFact]
        public virtual void Convert_ToDouble()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
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
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalFact]
        public virtual void Convert_ToInt16()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
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
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalFact]
        public virtual void Convert_ToInt32()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
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
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalFact]
        public virtual void Convert_ToInt64()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
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
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalFact]
        public virtual void Convert_ToString()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToString(Convert.ToByte(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToDecimal(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToDouble(o.OrderID % 1)) != "10",
                o => Convert.ToString((float)Convert.ToDouble(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt16(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt32(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt64(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToString(o.OrderID % 1)) != "10"
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [ConditionalFact]
        public virtual void Substring_with_constant()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    "ari",
                    context.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => c.ContactName.Substring(1, 3)).First());
            }
        }

        [ConditionalFact]
        public virtual void Substring_with_closure()
        {
            var start = 2;

            using (var context = CreateContext())
            {
                Assert.Equal(
                    "ria",
                    context.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => c.ContactName.Substring(start, 3)).First());
            }
        }

        [ConditionalFact]
        public virtual void Substring_with_client_eval()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    "ari",
                    context.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => c.ContactName.Substring(c.ContactName.IndexOf('a'), 3)).First());
            }
        }

        [ConditionalFact]
        public virtual void IsNullOrEmpty_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => string.IsNullOrEmpty(c.Region)),
                entryCount: 60);
        }

        [ConditionalFact]
        public virtual void IsNullOrEmpty_in_projection()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Select(c => new { Id = c.CustomerID, Value = string.IsNullOrEmpty(c.Region) })
                    .ToList();

                Assert.Equal(91, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void IsNullOrEmpty_negated_in_projection()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Select(c => new { Id = c.CustomerID, Value = !string.IsNullOrEmpty(c.Region) })
                    .ToList();

                Assert.Equal(91, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void IsNullOrWhiteSpace_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => string.IsNullOrWhiteSpace(c.Region)),
                entryCount: 60);
        }

        [ConditionalFact]
        public virtual void TrimStart_without_arguments_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.TrimStart() == "Owner"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void TrimStart_with_char_argument_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.TrimStart('O') == "wner"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void TrimStart_with_char_array_argument_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.TrimStart('O', 'w') == "ner"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void TrimEnd_without_arguments_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.TrimEnd() == "Owner"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void TrimEnd_with_char_argument_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.TrimEnd('r') == "Owne"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void TrimEnd_with_char_array_argument_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.TrimEnd('e', 'r') == "Own"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void Trim_without_argument_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.Trim() == "Owner"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void Trim_with_char_argument_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.Trim('O') == "wner"),
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void Trim_with_char_array_argument_in_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactTitle.Trim('O', 'r') == "wne"),
                entryCount: 17);
        }
    }
}
