// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

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
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

// ReSharper disable once UnusedTypeParameter
public abstract class NorthwindFunctionsQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_evaluation_of_uncorrelated_method_call(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.UnitPrice < 7)
                .Where(od => Math.Abs(-10) < od.ProductID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_length_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID.Length).ThenBy(c => c.CustomerID)
                .Select(c => c.Orders),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_round_works_correctly_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new { o.OrderID, Sum = o.OrderDetails.Sum(i => Math.Round(i.UnitPrice, 2)) }),
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                Assert.Equal(e.Sum, a.Sum);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_round_works_correctly_in_projection_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new { o.OrderID, Sum = o.OrderDetails.Select(i => i.UnitPrice * i.UnitPrice).Sum(i => Math.Round(i, 2)) }),
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                Assert.Equal(e.Sum, a.Sum);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_truncate_works_correctly_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new { o.OrderID, Sum = o.OrderDetails.Sum(i => Math.Truncate(i.UnitPrice)) }),
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                Assert.Equal(e.Sum, a.Sum);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new { o.OrderID, Sum = o.OrderDetails.Select(i => i.UnitPrice * i.UnitPrice).Sum(i => Math.Truncate(i)) }),
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                Assert.Equal(e.Sum, a.Sum);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_functions_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => Math.Pow(c.CustomerID.Length, 2) == 25));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_Parse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                byte.Parse(c.Phone.Substring(0, 3)) == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "ALFKI" &&
                 byte.Parse(c.CustomerID) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_Parse_Greater_Than_Max_Value_Overflows(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID == "ALFKI" &&
                byte.Parse(c.PostalCode) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_Parse_Negative_Overflows(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID == "ALFKI" &&
                byte.Parse(c.Phone.Substring(3, 4)) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_Parse_Decimal_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "BLONP" &&
                byte.Parse(c.Phone.Substring(0, 4)) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Decimal_Parse(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                decimal.Parse(c.PostalCode) == 12209m));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "BLONP" &&
                decimal.Parse(c.Phone.Substring(0, 4)) == 88.6m));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                decimal.Parse(c.Phone.Substring(3, 4)) == -7m));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Decimal_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertQuery(
               async,
               ss => ss.Set<Customer>()
                       .Where(c => c.CustomerID == "ALFKI" &&
                decimal.Parse(c.CustomerID) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Double_Parse(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                double.Parse(c.PostalCode) == 12209d));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "BLONP" &&
                double.Parse(c.Phone.Substring(0, 4)) == 88.6d));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                double.Parse(c.Phone.Substring(3, 4)) == -7d));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Double_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertQuery(
               async,
               ss => ss.Set<Customer>()
                       .Where(c => c.CustomerID == "ALFKI" &&
                double.Parse(c.CustomerID) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Short_Parse(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                short.Parse(c.PostalCode) == 12209));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                short.Parse(c.Phone.Substring(3, 4)) == -7));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Short_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "ALFKI" &&
                short.Parse(c.CustomerID) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Short_Parse_Greater_Than_Max_Value_Overflows(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "BLAUS" &&
                short.Parse(c.PostalCode) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Short_Parse_Decimal_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "BLONP" &&
                short.Parse(c.Phone.Substring(0, 4)) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Int_Parse(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                int.Parse(c.PostalCode) == 12209));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                int.Parse(c.Phone.Substring(3, 4)) == -7));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Int_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "ALFKI" &&
                int.Parse(c.CustomerID) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Int_Parse_Decimal_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "BLONP" &&
                int.Parse(c.Phone.Substring(0, 4)) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Long_Parse(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                long.Parse(c.PostalCode) == 12209L));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" &&
                long.Parse(c.Phone.Substring(3, 4)) == -7L));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Long_Parse_Non_Numeric_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "ALFKI" &&
                long.Parse(c.CustomerID) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Long_Parse_Decimal_Bad_Format(bool async)
        => AssertQuery(
           async,
           ss => ss.Set<Customer>()
                   .Where(c => c.CustomerID == "BLONP" &&
                long.Parse(c.Phone.Substring(0, 4)) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Static_equals_nullable_datetime_compared_to_non_nullable(bool async)
    {
        var arg = new DateTime(1996, 7, 4);

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => Equals(o.OrderDate, arg)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Static_equals_int_compared_to_long(bool async)
    {
        long arg = 10248;

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => Equals(o.OrderID, arg)),
            assertEmpty: true);
    }
}
