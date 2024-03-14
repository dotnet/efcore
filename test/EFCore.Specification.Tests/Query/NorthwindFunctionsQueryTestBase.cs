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
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith("M")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_StartsWith_Parameter(bool async)
    {
        var pattern = "M";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith(pattern)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_StartsWith_Identity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith(c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_StartsWith_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith(c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_StartsWith_MethodCall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.StartsWith(LocalMethod1())));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_EndsWith_Literal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith("b")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_EndsWith_Parameter(bool async)
    {
        var pattern = "b";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith(pattern)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_EndsWith_Identity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith(c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_EndsWith_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith(c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_EndsWith_MethodCall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.EndsWith(LocalMethod2())));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Contains_Literal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("M")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Contains_Identity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Contains_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_FirstOrDefault_MethodCall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.FirstOrDefault() == 'A'));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Contains_constant_with_whitespace(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("     ")),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Contains_parameter_with_whitespace(bool async)
    {
        var pattern = "     ";
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(pattern)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_LastOrDefault_MethodCall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.LastOrDefault() == 's'));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Contains_MethodCall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(LocalMethod1())));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Join_over_non_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => c.City)
                .Select(g => new { City = g.Key, Customers = string.Join("|", g.Select(e => e.CustomerID)) }),
            elementSorter: x => x.City,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.City, a.City);

                // Ordering inside the string isn't specified server-side, split and reorder
                Assert.Equal(
                    e.Customers.Split("|").OrderBy(id => id).ToArray(),
                    a.Customers.Split("|").OrderBy(id => id).ToArray());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Join_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => c.City)
                .Select(
                    g => new
                    {
                        City = g.Key, Customers = string.Join("|", g.Where(e => e.ContactName.Length > 10).Select(e => e.CustomerID))
                    }),
            elementSorter: x => x.City,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.City, a.City);

                // Ordering inside the string isn't specified server-side, split and reorder
                Assert.Equal(
                    e.Customers.Split("|").OrderBy(id => id).ToArray(),
                    a.Customers.Split("|").OrderBy(id => id).ToArray());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Join_with_ordering(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => c.City)
                .Select(
                    g => new
                    {
                        City = g.Key, Customers = string.Join("|", g.OrderByDescending(e => e.CustomerID).Select(e => e.CustomerID))
                    }),
            elementSorter: x => x.City);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Join_over_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => c.City)
                .Select(g => new { City = g.Key, Regions = string.Join("|", g.Select(e => e.Region)) }),
            elementSorter: x => x.City,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.City, a.City);

                // Ordering inside the string isn't specified server-side, split and reorder
                Assert.Equal(
                    e.Regions.Split("|").OrderBy(id => id).ToArray(),
                    a.Regions.Split("|").OrderBy(id => id).ToArray());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Concat(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => c.City)
                .Select(g => new { City = g.Key, Customers = string.Concat(g.Select(e => e.CustomerID)) }),
            elementSorter: x => x.City,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.City, a.City);

                // The best we can do for Concat without server-side ordering is sort the characters (concatenating without ordering
                // and without a delimiter is somewhat dubious anyway).
                Assert.Equal(e.Customers.OrderBy(c => c).ToArray(), a.Customers.OrderBy(c => c).ToArray());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_simple_zero(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "AROUT") == 0));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 != string.Compare(c.CustomerID, "AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "AROUT") > 0));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 >= string.Compare(c.CustomerID, "AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 < string.Compare(c.CustomerID, "AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "AROUT") <= 0));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_simple_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "AROUT") == 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 == string.Compare(c.CustomerID, "AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "AROUT") < 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 1 > string.Compare(c.CustomerID, "AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "AROUT") > -1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 < string.Compare(c.CustomerID, "AROUT")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_compare_with_parameter(bool async)
    {
        Customer customer = null;
        using (var context = CreateContext())
        {
            customer = await context.Customers.SingleAsync(c => c.CustomerID == "AROUT");
        }

        ClearLog();

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, customer.CustomerID) == 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 == string.Compare(c.CustomerID, customer.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, customer.CustomerID) < 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 1 > string.Compare(c.CustomerID, customer.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, customer.CustomerID) > -1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 < string.Compare(c.CustomerID, customer.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_simple_more_than_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") == 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") > 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 42 > string.Compare(c.CustomerID, "ALFKI")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_nested(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "M" + c.CustomerID) == 0),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 != string.Compare(c.CustomerID, c.CustomerID.ToUpper())),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) > 0),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 >= string.Compare(c.CustomerID, "M" + c.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 1 == string.Compare(c.CustomerID, c.CustomerID.ToUpper())),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI".Replace("ALF".ToUpper(), c.CustomerID)) == -1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_multi_predicate(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.CustomerID, "ALFKI") > -1)
                .Where(c => string.Compare(c.CustomerID, "CACTU") == -1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Compare(c.ContactTitle, "Owner") == 0)
                .Where(c => string.Compare(c.Country, "USA") != 0));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_to_simple_zero(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT") == 0));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 != c.CustomerID.CompareTo("AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT") > 0));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 >= c.CustomerID.CompareTo("AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 < c.CustomerID.CompareTo("AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT") <= 0));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_to_simple_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT") == 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 == c.CustomerID.CompareTo("AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT") < 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 1 > c.CustomerID.CompareTo("AROUT")));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT") > -1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 < c.CustomerID.CompareTo("AROUT")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_compare_to_with_parameter(bool async)
    {
        Customer customer = null;
        using (var context = CreateContext())
        {
            customer = await context.Customers.SingleAsync(x => x.CustomerID == "AROUT");
        }

        ClearLog();

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo(customer.CustomerID) == 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 == c.CustomerID.CompareTo(customer.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo(customer.CustomerID) < 1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 1 > c.CustomerID.CompareTo(customer.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo(customer.CustomerID) > -1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => -1 < c.CustomerID.CompareTo(customer.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_to_simple_more_than_one(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") == 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") > 42),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 42 > c.CustomerID.CompareTo("ALFKI")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_to_nested(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("M" + c.CustomerID) != 0));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 == c.CustomerID.CompareTo(c.CustomerID.ToUpper())));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT".Replace("OUT".ToUpper(), c.CustomerID)) > 0));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 0 >= c.CustomerID.CompareTo("M" + c.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => 1 == c.CustomerID.CompareTo(c.CustomerID.ToUpper())),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("AROUT".Replace("OUT".ToUpper(), c.CustomerID)) == -1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Compare_to_multi_predicate(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.CompareTo("ALFKI") > -1).Where(c => c.CustomerID.CompareTo("CACTU") == -1));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.CompareTo("Owner") == 0).Where(c => c.Country.CompareTo("USA") != 0));
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
                ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 != c.OrderDate.Value.CompareTo(myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 >= c.OrderDate.Value.CompareTo(myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 < c.OrderDate.Value.CompareTo(myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) <= 0));
        }
        else
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 != DateTime.Compare(c.OrderDate.Value, myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 >= DateTime.Compare(c.OrderDate.Value, myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 < DateTime.Compare(c.OrderDate.Value, myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) <= 0));
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
                ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 != c.OrderDate.Value.CompareTo(myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 >= c.OrderDate.Value.CompareTo(myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 < c.OrderDate.Value.CompareTo(myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => c.OrderDate.Value.CompareTo(myDatetime) <= 0));
        }
        else
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 != DateTime.Compare(c.OrderDate.Value, myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 >= DateTime.Compare(c.OrderDate.Value, myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => 0 < DateTime.Compare(c.OrderDate.Value, myDatetime)));

            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(c => DateTime.Compare(c.OrderDate.Value, myDatetime) <= 0));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Int_Compare_to_simple_zero(bool async)
    {
        var orderId = 10250;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => c.OrderID.CompareTo(orderId) == 0));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => 0 != c.OrderID.CompareTo(orderId)));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => c.OrderID.CompareTo(orderId) > 0));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => 0 >= c.OrderID.CompareTo(orderId)));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => 0 < c.OrderID.CompareTo(orderId)));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => c.OrderID.CompareTo(orderId) <= 0));
    }

    protected static string LocalMethod1()
        => "M";

    protected static string LocalMethod2()
        => "m";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_abs1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>()
                .Where(od => Math.Abs(od.ProductID) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_abs2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.UnitPrice < 7)
                .Where(od => Math.Abs(od.Quantity) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_abs3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Where(od => Math.Abs(od.UnitPrice) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_abs_uncorrelated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.UnitPrice < 7)
                .Where(od => Math.Abs(-10) < od.ProductID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_ceiling1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.UnitPrice < 7)
                .Where(od => Math.Ceiling(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_ceiling2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Where(od => Math.Ceiling(od.UnitPrice) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_floor(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Where(od => Math.Floor(od.UnitPrice) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_power(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => Math.Pow(od.Discount, 3) > 0.005f));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_square(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => Math.Pow(od.Discount, 2) > 0.05f));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_round(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Where(od => Math.Round(od.UnitPrice) > 10));

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
    public virtual Task Select_math_round_int(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10250)
                .Select(o => new { A = Math.Round((double)o.OrderID) }),
            e => e.A);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_math_truncate_int(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10250)
                .Select(o => new { A = Math.Truncate((double)o.OrderID) }),
            e => e.A);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_round2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => Math.Round(od.UnitPrice, 2) > 100));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_truncate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Where(od => Math.Truncate(od.UnitPrice) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_exp(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Exp(od.Discount) > 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_log10(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log10(od.Discount) < 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_log(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log(od.Discount) < 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_log_new_base(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => Math.Log(od.Discount, 7) < -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_sqrt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Sqrt(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_acos(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Acos(od.Discount) > 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_asin(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Asin(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_atan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Atan(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_atan2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Atan2(od.Discount, 1) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_cos(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Cos(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_sin(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Sin(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_tan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Tan(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_sign(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Sign(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_max(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => Math.Max(od.OrderID, od.ProductID) == od.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_max_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077)
                .Where(od => Math.Max(od.OrderID, Math.Max(od.ProductID, 1)) == od.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_max_nested_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077)
                .Where(od => Math.Max(Math.Max(1, Math.Max(od.OrderID, 2)), od.ProductID) == od.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_min(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077)
                .Where(od => Math.Min(od.OrderID, od.ProductID) == od.ProductID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_min_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077)
                .Where(od => Math.Min(od.OrderID, Math.Min(od.ProductID, 99999)) == od.ProductID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_min_nested_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077)
                .Where(od => Math.Min(Math.Min(99999, Math.Min(od.OrderID, 99998)), od.ProductID) == od.ProductID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_degrees(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => double.RadiansToDegrees(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_math_radians(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => double.DegreesToRadians(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_abs1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>()
                .Where(od => MathF.Abs(od.ProductID) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_ceiling1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.UnitPrice < 7)
                .Where(od => MathF.Ceiling(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_floor(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Where(od => MathF.Floor((float)od.UnitPrice) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_power(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => MathF.Pow(od.Discount, 3) > 0.005f));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_square(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => MathF.Pow(od.Discount, 2) > 0.05f));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_round2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => MathF.Round((float)od.UnitPrice, 2) > 100));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_mathf_round(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10250)
                .Select(o => MathF.Round(o.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_mathf_round2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Select(od => MathF.Round((float)od.UnitPrice, 2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_truncate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Where(od => MathF.Truncate((float)od.UnitPrice) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_mathf_truncate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.Quantity < 5)
                .Select(od => MathF.Truncate((float)od.UnitPrice)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_exp(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Exp(od.Discount) > 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_log10(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => MathF.Log10(od.Discount) < 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_log(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => MathF.Log(od.Discount) < 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_log_new_base(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077 && od.Discount > 0).Where(od => MathF.Log(od.Discount, 7) < -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_sqrt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Sqrt(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_acos(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Acos(od.Discount) > 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_asin(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Asin(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_atan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Atan(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_atan2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Atan2(od.Discount, 1) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_cos(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Cos(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_sin(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Sin(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_tan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Tan(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_sign(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => MathF.Sign(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_degrees(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => float.RadiansToDegrees(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_mathf_radians(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID == 11077).Where(od => float.DegreesToRadians(od.Discount) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_guid_newguid(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(od => Guid.NewGuid() != default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_to_upper(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.ToUpper() == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_to_lower(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.ToLower() == "alfki"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_functions_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => Math.Pow(c.CustomerID.Length, 2) == 25));

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
                    .Where(convertMethod));
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
                    .Where(convertMethod));
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
                    .Where(convertMethod));
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
                    .Where(convertMethod));
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
                    .Where(convertMethod));
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
                    .Where(convertMethod));
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
                    .Where(convertMethod));
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
                    .Where(convertMethod));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Indexof_with_emptystring(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.IndexOf(string.Empty) == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Indexof_with_one_constant_arg(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.IndexOf("a") == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Indexof_with_one_parameter_arg(bool async)
    {
        var pattern = "a";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.IndexOf(pattern) == 1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Indexof_with_constant_starting_position(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.IndexOf("a", 2) == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Indexof_with_parameter_starting_position(bool async)
    {
        var start = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.IndexOf("a", start) == 4));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Replace_with_emptystring(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Replace("ia", string.Empty) == "Mar Anders"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Replace_using_property_arguments(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactName.Replace(c.ContactName, c.CustomerID) == c.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_one_arg_with_zero_startindex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.Substring(0) == "ALFKI")
                .Select(c => c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_one_arg_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.Substring(1) == "LFKI")
                .Select(c => c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_one_arg_with_closure(bool async)
    {
        var start = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.Substring(start) == "FKI")
                .Select(c => c.ContactName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_zero_startindex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(0, 3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_zero_length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(2, 0)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(1, 3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_closure(bool async)
    {
        var start = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.Substring(start, 3)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Substring_with_two_args_with_Index_of(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                .Select(c => c.ContactName.Substring(c.ContactName.IndexOf("a"), 3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsNullOrEmpty_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.IsNullOrEmpty(c.Region)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsNullOrEmpty_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { Id = c.CustomerID, Value = string.IsNullOrEmpty(c.Region) }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsNullOrEmpty_negated_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => !string.IsNullOrEmpty(c.Region)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsNullOrEmpty_negated_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { Id = c.CustomerID, Value = !string.IsNullOrEmpty(c.Region) }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsNullOrWhiteSpace_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.IsNullOrWhiteSpace(c.Region)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.IsNullOrWhiteSpace(c.CustomerID)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimStart_without_arguments_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimStart() == "Owner"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimStart_with_char_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimStart('O') == "wner"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimStart_with_char_array_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimStart('O', 'w') == "ner"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimEnd_without_arguments_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimEnd() == "Owner"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimEnd_with_char_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimEnd('r') == "Owne"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TrimEnd_with_char_array_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.TrimEnd('e', 'r') == "Own"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trim_without_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.Trim() == "Owner"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trim_with_char_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.Trim('O') == "wner"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trim_with_char_array_argument_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ContactTitle.Trim('O', 'r') == "wne"));

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
    public virtual Task Static_string_equals_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Equals(c.CustomerID, "ANATR")));

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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(o => new { A = Math.Truncate((double)o.OrderID) })
                .OrderBy(r => r.A)
                .OrderBy(r => r.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(o => new { A = Math.Truncate((double)o.OrderID) })
                .OrderBy(r => r.A)
                .OrderByDescending(r => r.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(o => new { A = Math.Truncate((double)o.OrderID) })
                .OrderByDescending(r => r.A)
                .ThenBy(r => r.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Regex_IsMatch_MethodCall_constant_input(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(o => Regex.IsMatch("ALFKI", o.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Datetime_subtraction_TotalDays(bool async)
    {
        var date = new DateTime(1997, 1, 1);
        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.HasValue && (o.OrderDate.Value - date).TotalDays > 365));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_FromDateTime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderDate.HasValue && DateOnly.FromDateTime(o.OrderDate.Value) == new DateOnly(1996, 9, 16))
                .AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_ToString_IndexOf(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(x => x.OrderID.ToString().IndexOf("123") == -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_IndexOf_ToString(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(x => "123".IndexOf(x.OrderID.ToString()) == -1));
}
