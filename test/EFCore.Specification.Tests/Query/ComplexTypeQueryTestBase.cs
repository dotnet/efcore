// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ComplexTypeQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : ComplexTypeQueryFixtureBase, new()
{
    protected ComplexTypeQueryTestBase(TFixture fixture)
        : base(fixture)
        => fixture.ListLoggerFactory.Clear();

    // [Theory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Filter_on_property_inside_complex_type(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<Customer>().Where(c => c.ShippingAddress.ZipCode == 07728));

    // [Theory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Filter_on_property_inside_nested_complex_type(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<Customer>().Where(c => c.ShippingAddress.Country.Code == "DE"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.ZipCode == 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.Country.Code == "DE"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_complex_type_on_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Where(cg => cg.OptionalCustomer!.ShippingAddress.ZipCode != 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_complex_type_on_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Where(cg => cg.RequiredCustomer.ShippingAddress.ZipCode != 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_complex_type_via_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Select(cg => cg.OptionalCustomer!.ShippingAddress),
            ss => ss.Set<CustomerGroup>().Select(cg => cg.OptionalCustomer != null ? cg.OptionalCustomer.ShippingAddress : default));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_complex_type_via_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Select(cg => cg.RequiredCustomer.ShippingAddress));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Load_complex_type_after_subquery_on_entity_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.Country));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_single_property_on_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.Country.FullName));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_type_Where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Where(a => a.ZipCode == 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_type_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Distinct());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_type_equals_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress == c.BillingAddress));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_type_equals_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress
                == new Address
                {
                    AddressLine1 = "804 S. Lakeshore Road",
                    ZipCode = 38654,
                    Country = new Country { FullName = "United States", Code = "US" },
                    Tags = new List<string> { "foo", "bar" }
                }),
            ss => ss.Set<Customer>().Where(c =>
                c.ShippingAddress.AddressLine1 == "804 S. Lakeshore Road"
                && c.ShippingAddress.ZipCode == 38654
                && c.ShippingAddress.Country == new Country { FullName = "United States", Code = "US" }
                && c.ShippingAddress.Tags.SequenceEqual(new List<string> { "foo", "bar" })));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_type_equals_parameter(bool async)
    {
        var address = new Address
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new Country { FullName = "United States", Code = "US" },
            Tags = ["foo", "bar"]
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress == address),
            ss => ss.Set<Customer>().Where(c =>
                c.ShippingAddress.AddressLine1 == "804 S. Lakeshore Road"
                && c.ShippingAddress.ZipCode == 38654
                && c.ShippingAddress.Country == new Country { FullName = "United States", Code = "US" }
                && c.ShippingAddress.Tags.SequenceEqual(new List<string> { "foo", "bar" })));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_over_complex_type(bool async)
    {
        var address = new Address
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new Country { FullName = "United States", Code = "US" }
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => ss.Set<Customer>().Select(c => c.ShippingAddress).OrderBy(a => a.ZipCode).First() == address));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_complex_type(bool async)
    {
        var address = new Address
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new Country { FullName = "United States", Code = "US" },
            Tags = ["foo", "bar"]
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ss.Set<Customer>().Select(c => c.ShippingAddress).Contains(address)),
            ss => ss.Set<Customer>().Where(c => ss.Set<Customer>().Select(c => c.ShippingAddress).Any(a =>
                a.AddressLine1 == "804 S. Lakeshore Road"
                && a.ZipCode == 38654
                && a.Country == new Country { FullName = "United States", Code = "US" }
                && a.Tags.SequenceEqual(new List<string> { "foo", "bar" }))));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_entity_type_containing_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Concat(ss.Set<Customer>().Where(c => c.Id == 2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_entity_type_containing_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Union(ss.Set<Customer>().Where(c => c.Id == 2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Concat(ss.Set<Customer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Union(ss.Set<Customer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_property_in_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.AddressLine1)
                .Concat(ss.Set<Customer>().Select(c => c.BillingAddress.AddressLine1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_property_in_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.AddressLine1)
                .Union(ss.Set<Customer>().Select(c => c.BillingAddress.AddressLine1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_two_different_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Concat(ss.Set<Customer>().Select(c => c.BillingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_two_different_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Union(ss.Set<Customer>().Select(c => c.BillingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress.ZipCode == 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress.Country.Code == "DE"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_struct_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.ZipCode == 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_struct_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.Country.Code == "DE"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Where(cg => cg.OptionalCustomer!.ShippingAddress.ZipCode != 07728),
            ss => ss.Set<ValuedCustomerGroup>()
                .Where(cg => cg.OptionalCustomer == null || cg.OptionalCustomer.ShippingAddress.ZipCode != 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Where(cg => cg.RequiredCustomer.ShippingAddress.ZipCode != 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_struct_complex_type_via_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Select(cg => cg.OptionalCustomer!.ShippingAddress),
            ss => ss.Set<ValuedCustomerGroup>().Select(cg => cg.OptionalCustomer != null ? cg.OptionalCustomer.ShippingAddress : default));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_nullable_struct_complex_type_via_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>()
                .Select(cg => cg.OptionalCustomer != null ? cg.OptionalCustomer.ShippingAddress : (AddressStruct?)null));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_struct_complex_type_via_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Select(cg => cg.RequiredCustomer.ShippingAddress));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Load_struct_complex_type_after_subquery_on_entity_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.Country));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_single_property_on_nested_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.Country.FullName));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_struct_complex_type_Where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Where(a => a.ZipCode == 07728));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_struct_complex_type_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Distinct());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Struct_complex_type_equals_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress == c.BillingAddress));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Struct_complex_type_equals_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress
                == new AddressStruct
                {
                    AddressLine1 = "804 S. Lakeshore Road",
                    ZipCode = 38654,
                    Country = new CountryStruct { FullName = "United States", Code = "US" }
                }));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Struct_complex_type_equals_parameter(bool async)
    {
        var address = new AddressStruct
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new CountryStruct { FullName = "United States", Code = "US" }
        };

        return AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress == address));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_over_struct_complex_type(bool async)
    {
        var address = new AddressStruct
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new CountryStruct { FullName = "United States", Code = "US" }
        };

        return AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c
                => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).OrderBy(a => a.ZipCode).First() == address));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_struct_complex_type(bool async)
    {
        var address = new AddressStruct
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new CountryStruct { FullName = "United States", Code = "US" }
        };

        return AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Contains(address)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_entity_type_containing_struct_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Concat(ss.Set<ValuedCustomer>().Where(c => c.Id == 2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_entity_type_containing_struct_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Union(ss.Set<ValuedCustomer>().Where(c => c.Id == 2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Concat(ss.Set<ValuedCustomer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Union(ss.Set<ValuedCustomer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_property_in_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.AddressLine1)
                .Concat(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress.AddressLine1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_property_in_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.AddressLine1)
                .Union(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress.AddressLine1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_two_different_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Concat(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_two_different_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Union(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_nested_complex_type_twice_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .Distinct()
                .Select(x => new { x.BA1, x.BA2 }),
            elementSorter: e => (e.BA1.ZipCode, e.BA2.ZipCode),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.BA1, a.BA1);
                AssertEqual(e.BA2, a.BA2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_entity_with_nested_complex_type_twice_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   select new { c1, c2 })
                .Distinct()
                .Select(x => new { x.c1, x.c2 }),
            elementSorter: e => (e.c1.Id, e.c2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   orderby c1.Id, c2.Id
                   select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .Take(50)
                .Distinct()
                .Select(x => new { x.BA1, x.BA2 }),
            elementSorter: e => (e.BA1.ZipCode, e.BA2.ZipCode),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.BA1, a.BA1);
                AssertEqual(e.BA2, a.BA2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   orderby c1.Id, c2.Id
                   select new { c1, c2 })
                .Take(50)
                .Distinct()
                .Select(x => new { x.c1, x.c2 }),
            elementSorter: e => (e.c1.Id, e.c2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_struct_nested_complex_type_twice_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<ValuedCustomer>()
                   from c2 in ss.Set<ValuedCustomer>()
                   select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .Distinct()
                .Select(x => new { x.BA1, x.BA2 }),
            elementSorter: e => (e.BA1.ZipCode, e.BA2.ZipCode),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.BA1, a.BA1);
                AssertEqual(e.BA2, a.BA2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<ValuedCustomer>()
                   from c2 in ss.Set<ValuedCustomer>()
                   select new { c1, c2 })
                .Distinct()
                .Select(x => new { x.c1, x.c2 }),
            elementSorter: e => (e.c1.Id, e.c2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_struct_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<ValuedCustomer>()
                   from c2 in ss.Set<ValuedCustomer>()
                   orderby c1.Id, c2.Id
                   select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .Take(50)
                .Distinct()
                .Select(x => new { x.BA1, x.BA2 }),
            elementSorter: e => (e.BA1.ZipCode, e.BA2.ZipCode),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.BA1, a.BA1);
                AssertEqual(e.BA2, a.BA2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<ValuedCustomer>()
                   from c2 in ss.Set<ValuedCustomer>()
                   orderby c1.Id, c2.Id
                   select new { c1, c2 })
                .Take(50)
                .Distinct()
                .Select(x => new { x.c1, x.c2 }),
            elementSorter: e => (e.c1.Id, e.c2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   orderby c1.Id, c2.Id
                   select new { c1, c2 })
                .Union(
                    from c1 in ss.Set<Customer>()
                    from c2 in ss.Set<Customer>()
                    orderby c1.Id, c2.Id
                    select new { c1, c2 })
                .OrderBy(x => x.c1.Id).ThenBy(x => x.c2.Id)
                .Take(50)
                .Select(x => new { x.c1, x.c2 }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   orderby c1.Id, c2.Id
                   select new { c1, c2 })
                .Union(
                    from c1 in ss.Set<Customer>()
                    from c2 in ss.Set<Customer>()
                    orderby c1.Id, c2.Id
                    select new { c1, c2 })
                .OrderBy(x => x.c1.Id).ThenBy(x => x.c2.Id)
                .Take(50)
                .Select(x => new { x.c1, x.c2 })
                .Distinct()
                .OrderBy(x => x.c1.Id).ThenBy(x => x.c2.Id)
                .Take(50)
                .Select(x => new { x.c1, x.c2 }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_of_same_nested_complex_type_projected_twice_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   orderby c1.Id, c2.Id
                   select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .Union(
                    from c1 in ss.Set<Customer>()
                    from c2 in ss.Set<Customer>()
                    orderby c1.Id, c2.Id
                    select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .OrderBy(x => x.BA1.ZipCode).ThenBy(x => x.BA2.ZipCode)
                .Take(50)
                .Select(x => new { x.BA1, x.BA2 }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.BA1, a.BA1);
                AssertEqual(e.BA2, a.BA2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(bool async)
        => AssertQuery(
            async,
            ss => (from c1 in ss.Set<Customer>()
                   from c2 in ss.Set<Customer>()
                   orderby c1.Id, c2.Id
                   select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .Union(
                    from c1 in ss.Set<Customer>()
                    from c2 in ss.Set<Customer>()
                    orderby c1.Id, c2.Id
                    select new { BA1 = c1.BillingAddress, BA2 = c2.BillingAddress })
                .OrderBy(x => x.BA1.ZipCode).ThenBy(x => x.BA2.ZipCode)
                .Take(50)
                .Select(x => new { x.BA1, x.BA2 })
                .Distinct()
                .OrderBy(x => x.BA1.ZipCode).ThenBy(x => x.BA2.ZipCode)
                .Take(50)
                .Select(x => new { x.BA1, x.BA2 }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.BA1, a.BA1);
                AssertEqual(e.BA2, a.BA2);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(x => new
            {
                x.Id,
                Complex = (from c1 in ss.Set<Customer>()
                           from c2 in ss.Set<Customer>()
                           orderby c1.Id, c2.Id descending
                           select new { One = c1, Two = c2 }).FirstOrDefault()
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Complex?.One, a.Complex?.One);
                AssertEqual(e.Complex?.Two, a.Complex?.Two);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Same_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(x => new
            {
                x.Id,
                Complex = (from c1 in ss.Set<Customer>()
                           from c2 in ss.Set<Customer>()
                           orderby c1.Id, c2.Id descending
                           select new { One = c1.BillingAddress, Two = c2.BillingAddress }).FirstOrDefault()
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Complex?.One, a.Complex?.One);
                AssertEqual(e.Complex?.Two, a.Complex?.Two);
            });

    #region GroupBy

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_over_property_in_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().GroupBy(x => x.ShippingAddress.Country.Code).Select(g => new { Code = g.Key, Count = g.Count() }),
            elementSorter: g => g.Code);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_over_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().GroupBy(x => x.ShippingAddress).Select(g => new { Address = g.Key, Count = g.Count() }),
            elementSorter: g => g.Address.ZipCode,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Address, a.Address);
                Assert.Equal(e.Count, a.Count);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_over_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().GroupBy(x => x.ShippingAddress.Country).Select(g => new { Country = g.Key, Count = g.Count() }),
            elementSorter: g => g.Country.Code);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_with_complex_type_with_group_by_and_first(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().GroupBy(x => x.Id).Select(x => x.First()));

    #endregion GroupBy

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_of_complex_type_using_left_join_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>()
                .LeftJoin(ss.Set<Customer>().Where(x => x.Id > 5), cg => cg.Id, c => c.Id, (cg, c) => c)
                .Select(c => c == null ? null : (int?)c.BillingAddress.ZipCode));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_complex_from_optional_navigation_using_conditional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Select(x => x.OptionalCustomer == null ? null : x.OptionalCustomer.ShippingAddress)
                .OrderBy(x => x!.ZipCode).Take(20).Distinct().Select(x => (int?)x!.ZipCode),
            ss => ss.Set<CustomerGroup>().Select(x => x.OptionalCustomer == null ? null : x.OptionalCustomer.ShippingAddress)
                .OrderBy(x => x.MaybeScalar(xx => xx!.ZipCode)).Take(20).Distinct().Select(x => x.MaybeScalar(xx => xx!.ZipCode)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_entity_with_complex_type_pushdown_and_then_left_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(x => x.Id).Take(20).Distinct()
                .LeftJoin(
                    ss.Set<Customer>().OrderByDescending(x => x.Id).Take(30).Distinct(),
                    c1 => c1.Id,
                    c2 => c2.Id,
                    (c1, c2) => new { Zip1 = c1.BillingAddress.ZipCode, Zip2 = c2 == null ? (int?)null : c2.ShippingAddress.ZipCode }));

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    #region Non-shared test resources

    #region 33449

    [Fact]
    public virtual async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        var contextFactory = await InitializeNonSharedTest<Context33449>(
            seed: context =>
            {
                context.AddRange(
                    new Context33449.EntityType
                    {
                        Id = 1,
                        ComplexContainer = new Context33449.ComplexContainer
                        {
                            Id = 1,
                            Containee1 = new Context33449.ComplexContainee1 { Id = 2 },
                            Containee2 = new Context33449.ComplexContainee2 { Id = 3 }
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var container = new Context33449.ComplexContainer
        {
            Id = 1,
            Containee1 = new Context33449.ComplexContainee1 { Id = 2 },
            Containee2 = new Context33449.ComplexContainee2 { Id = 3 }
        };

        _ = await context.Set<Context33449.EntityType>().Where(b => b.ComplexContainer == container).SingleAsync();
    }

    private class Context33449(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
                b.ComplexProperty(
                    b => b.ComplexContainer, x =>
                    {
                        x.ComplexProperty(c => c.Containee1);
                        x.ComplexProperty(c => c.Containee2);
                    });
            });

        public class EntityType
        {
            public int Id { get; set; }
            public ComplexContainer ComplexContainer { get; set; } = null!;
        }

        public class ComplexContainer
        {
            public int Id { get; set; }

            public ComplexContainee1 Containee1 { get; set; } = null!;
            public ComplexContainee2 Containee2 { get; set; } = null!;
        }

        public class ComplexContainee1
        {
            public int Id { get; set; }
        }

        public class ComplexContainee2
        {
            public int Id { get; set; }
        }
    }

    #endregion 33449

    #region 34749

    [Fact]
    public virtual async Task Projecting_complex_property_does_not_auto_include_owned_types()
    {
        var contextFactory = await InitializeNonSharedTest<Context34749>();

        await using var context = contextFactory.CreateDbContext();

        _ = await context.Set<Context34749.EntityType>().Select(x => x.Complex).ToListAsync();
    }

    private class Context34749(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
                b.ComplexProperty(x => x.Complex);
                b.OwnsOne(x => x.OwnedReference);
            });

        public class EntityType
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public OwnedType OwnedReference { get; set; } = null!;
            public ComplexType Complex { get; set; } = null!;
        }

        public class ComplexType
        {
            public int Number { get; set; }
            public string? Name { get; set; }
        }

        public class OwnedType
        {
            public string? Foo { get; set; }
            public int Bar { get; set; }
        }
    }

    #endregion

    #region ShadowDiscriminator

    [Fact]
    public virtual async Task Optional_complex_type_with_discriminator()
    {
        var contextFactory = await InitializeNonSharedTest<ContextShadowDiscriminator>(
            seed: context =>
            {
                context.AddRange(
                    new ContextShadowDiscriminator.EntityType
                    {
                        Id = 1,
                        AllOptionalsComplexType =
                            new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = "Non-null" }
                    },
                    new ContextShadowDiscriminator.EntityType
                    {
                        Id = 2,
                        AllOptionalsComplexType = new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = null }
                    },
                    new ContextShadowDiscriminator.EntityType { Id = 3, AllOptionalsComplexType = null }
                );
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var complexTypeNull = await context.Set<ContextShadowDiscriminator.EntityType>()
                .SingleAsync(b => b.AllOptionalsComplexType == null);
            Assert.Null(complexTypeNull.AllOptionalsComplexType);

            complexTypeNull.AllOptionalsComplexType =
                new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = "New thing" };
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entities = await context.Set<ContextShadowDiscriminator.EntityType>().ToListAsync();
            Assert.Equal(3, entities.Count);
            Assert.All(entities, e => Assert.NotNull(e.AllOptionalsComplexType));
        }
    }

    private class ContextShadowDiscriminator(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
                b.ComplexProperty(b => b.AllOptionalsComplexType, x => x.HasDiscriminator());
            });

        public class EntityType
        {
            public int Id { get; set; }
            public AllOptionalsComplexType? AllOptionalsComplexType { get; set; }
        }

        public class AllOptionalsComplexType
        {
            public string? OptionalProperty { get; set; }
        }
    }

    #endregion ShadowDiscriminator

    #region 37162

    [Fact]
    public virtual async Task Non_optional_complex_type_with_all_nullable_properties()
    {
        var contextFactory = await InitializeNonSharedTest<Context37162>(
            seed: context =>
            {
                context.Add(
                    new Context37162.EntityType
                    {
                        Id = 1,
                        NonOptionalComplexType = new Context37162.ComplexTypeWithAllNulls
                        {
                            // All properties are null
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entity = await context.Set<Context37162.EntityType>().SingleAsync();

        Assert.NotNull(entity.NonOptionalComplexType);
        Assert.Null(entity.NonOptionalComplexType.NullableString);
        Assert.Null(entity.NonOptionalComplexType.NullableDateTime);
    }

    private class Context37162(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
                b.ComplexProperty(b => b.NonOptionalComplexType);
            });

        public class EntityType
        {
            public int Id { get; set; }
            public ComplexTypeWithAllNulls NonOptionalComplexType { get; set; } = null!;
        }

        public class ComplexTypeWithAllNulls
        {
            public string? NullableString { get; set; }
            public DateTime? NullableDateTime { get; set; }
        }
    }

    #endregion 37162

    #region 37304

    [Fact]
    public virtual async Task Non_optional_complex_type_with_all_nullable_properties_via_left_join()
    {
        var contextFactory = await InitializeNonSharedTest<Context37304>(
            seed: context =>
            {
                context.Add(
                    new Context37304.Parent
                    {
                        Id = 1,
                        Children =
                        [
                            new Context37304.Child { Id = 1, ComplexType = new Context37304.ComplexTypeWithAllNulls() }
                        ]
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var parent = await context.Set<Context37304.Parent>().Include(p => p.Children).SingleAsync();

        var child = parent.Children.Single();
        Assert.NotNull(child.ComplexType);
        Assert.Null(child.ComplexType.NullableString);
        Assert.Null(child.ComplexType.NullableDateTime);
    }

    private class Context37304(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(b => b.Property(p => p.Id).ValueGeneratedNever());

            modelBuilder.Entity<Child>(b =>
            {
                b.Property(c => c.Id).ValueGeneratedNever();
                b.HasOne(c => c.Parent).WithMany(p => p.Children).HasForeignKey(c => c.ParentId);
                b.ComplexProperty(c => c.ComplexType);
            });
        }

        public class Parent
        {
            public int Id { get; set; }
            public List<Child> Children { get; set; } = [];
        }

        public class Child
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public Parent Parent { get; set; } = null!;
            public ComplexTypeWithAllNulls ComplexType { get; set; } = null!;
        }

        public class ComplexTypeWithAllNulls
        {
            public string? NullableString { get; set; }
            public DateTime? NullableDateTime { get; set; }
        }
    }

    #endregion 37304

    #region Issue37337

    private const string Issue37337CreatedByShadowPropertyName = "CreatedBy";

    [Fact]
    public virtual async Task Nullable_complex_type_with_discriminator_and_shadow_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context37337>(
            seed: context =>
            {
                var entity = new Context37337.EntityType
                {
                    Id = Guid.NewGuid(),
                    Prop = new Context37337.OptionalComplexProperty { OptionalValue = true }
                };
                context.Add(entity);
                context.Entry(entity).Property(Issue37337CreatedByShadowPropertyName).CurrentValue = "Seeder";
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entities = await context.Set<Context37337.EntityType>().ToArrayAsync();

        Assert.Single(entities);
        var entity = entities[0];
        Assert.NotNull(entity.Prop);
        Assert.True(entity.Prop.OptionalValue);

        var entry = context.Entry(entity);
        Assert.Equal("Seeder", entry.Property(Issue37337CreatedByShadowPropertyName).CurrentValue);
    }

    protected class Context37337(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<EntityType>();
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.HasKey(p => p.Id);

            var compl = entity.ComplexProperty(p => p.Prop);
            compl.Property(p => p.OptionalValue);
            compl.HasDiscriminator();

            // Shadow property added via convention (e.g., audit field)
            entity.Property<string>(Issue37337CreatedByShadowPropertyName).IsRequired(false);
        }

        public class EntityType
        {
            public Guid Id { get; set; }
            public OptionalComplexProperty? Prop { get; set; }
        }

        public class OptionalComplexProperty
        {
            public bool? OptionalValue { get; set; }
        }
    }

    #endregion Issue37337

    #region Issue38119

    [Fact]
    public virtual async Task Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        var contextFactory = await InitializeNonSharedTest<Context38119>(
            seed: context =>
            {
                context.Add(new Context38119.EntityType { Id = Guid.NewGuid() });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.Null(entity.Prop);

            entity.Prop = new Context38119.OptionalComplexProperty { OptionalValue = true };
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }
    }

    [Fact]
    public virtual async Task Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip()
    {
        var contextFactory = await InitializeNonSharedTest<Context38119>(
            seed: context =>
            {
                context.Add(
                    new Context38119.EntityType
                    {
                        Id = Guid.NewGuid(),
                        Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);

            entity.Prop = null;
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.Null(entity.Prop);
        }
    }

    [Fact]
    public virtual async Task Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip()
    {
        var contextFactory = await InitializeNonSharedTest<Context38119>(
            seed: context =>
            {
                context.Add(
                    new Context38119.EntityType
                    {
                        Id = Guid.NewGuid(),
                        Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);

            context.Update(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }
    }

    [Fact]
    public virtual async Task Nullable_complex_type_with_discriminator_set_to_different_value()
    {
        var contextFactory = await InitializeNonSharedTest<Context38119>();

        Guid entityId;
        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = new Context38119.EntityType
            {
                Id = Guid.NewGuid(),
                Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
            };
            context.Add(entity);
            entityId = entity.Id;

            // Override the discriminator value before saving
            var discriminatorEntry = context.Entry(entity).ComplexProperty(e => e.Prop).Property("Discriminator");
            Assert.Equal("OptionalComplexProperty", discriminatorEntry.CurrentValue);
            discriminatorEntry.CurrentValue = "SomeOtherValue";
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            // The discriminator is non-null so the complex property is still materialized
            var entity = await context.Set<Context38119.EntityType>().SingleAsync(e => e.Id == entityId);
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }
    }

    [Fact]
    public virtual async Task Nullable_complex_type_with_discriminator_set_to_null()
    {
        var contextFactory = await InitializeNonSharedTest<Context38119>();

        Guid entityId;
        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = new Context38119.EntityType
            {
                Id = Guid.NewGuid(),
                Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
            };
            context.Add(entity);
            entityId = entity.Id;

            // Set discriminator to null before saving, which should cause the complex property to be null on reload
            var discriminatorEntry = context.Entry(entity).ComplexProperty(e => e.Prop).Property("Discriminator");
            Assert.Equal("OptionalComplexProperty", discriminatorEntry.CurrentValue);
            discriminatorEntry.CurrentValue = null;
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            // With null discriminator, the complex property should be materialized as null
            var entity = await context.Set<Context38119.EntityType>().SingleAsync(e => e.Id == entityId);
            Assert.Null(entity.Prop);
        }
    }

    [Fact]
    public virtual async Task Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        var contextFactory = await InitializeNonSharedTest<Context38119Nested>(
            seed: context =>
            {
                context.Add(
                    new Context38119Nested.EntityType
                    {
                        Id = Guid.NewGuid(),
                        Outer = new Context38119Nested.OuterComplexProperty { Name = "outer" }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119Nested.EntityType>().SingleAsync();
            Assert.NotNull(entity.Outer);
            Assert.Null(entity.Outer.Inner);

            entity.Outer.Inner = new Context38119Nested.InnerComplexProperty { Value = 42 };
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119Nested.EntityType>().SingleAsync();
            Assert.NotNull(entity.Outer);
            Assert.NotNull(entity.Outer.Inner);
            Assert.Equal(42, entity.Outer.Inner.Value);
        }
    }

    protected class Context38119(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<EntityType>();
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();

            var compl = entity.ComplexProperty(p => p.Prop);
            compl.HasDiscriminator();
        }

        public class EntityType
        {
            public Guid Id { get; set; }
            public OptionalComplexProperty? Prop { get; set; }
        }

        public class OptionalComplexProperty
        {
            public bool? OptionalValue { get; set; }
        }
    }

    private class Context38119Nested(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<EntityType>();
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();

            entity.ComplexProperty(
                p => p.Outer, outer => outer.ComplexProperty(
                    p => p.Inner, inner => inner.HasDiscriminator()));
        }

        public class EntityType
        {
            public Guid Id { get; set; }
            public OuterComplexProperty Outer { get; set; } = null!;
        }

        public class OuterComplexProperty
        {
            public string? Name { get; set; }
            public InnerComplexProperty? Inner { get; set; }
        }

        public class InnerComplexProperty
        {
            public int? Value { get; set; }
        }
    }

    #endregion Issue38119

    #region Issue38105

    [Fact]
    public virtual async Task Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw()
    {
        var contextFactory = await InitializeNonSharedTest<Context37337>(
            seed: context =>
            {
                var entity = new Context37337.EntityType
                {
                    Id = Guid.NewGuid(),
                    Prop = new Context37337.OptionalComplexProperty { OptionalValue = true }
                };
                context.Add(entity);
                context.Entry(entity).Property(Issue37337CreatedByShadowPropertyName).CurrentValue = "Seeder";
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entity = await context.Set<Context37337.EntityType>().SingleAsync();
        var id = entity.Id;
        context.ChangeTracker.Clear();

        // Create a new disconnected instance with the same key and Update it.
        // The complex type discriminator (shadow property with AfterSaveBehavior.Throw) should not
        // be marked as modified by Update(), and SaveChanges should succeed without throwing.
        var updatedEntity = new Context37337.EntityType
        {
            Id = id,
            Prop = new Context37337.OptionalComplexProperty { OptionalValue = false }
        };

        context.Update(updatedEntity);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var reloaded = await context.Set<Context37337.EntityType>().SingleAsync();
        Assert.Equal(id, reloaded.Id);
        Assert.NotNull(reloaded.Prop);
        Assert.False(reloaded.Prop.OptionalValue);
    }

    #endregion Issue38105

    #region Issue31246

    [Fact]
    public virtual async Task Can_query_by_complex_type_property_with_index()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(
            seed: context =>
            {
                context.AddRange(
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(1),
                        Address = new Context31246.Address { City = "Seattle", PostalCode = "98101" }
                    },
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(2),
                        Address = new Context31246.Address { City = "Redmond", PostalCode = "98052" }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        // The primary key Id.Id reorders the entity's complex properties so that the chain containing
        // the PK property comes first, and reorders the properties inside the Id complex type so that
        // the PK property is listed first.
        var personType = context.Model.FindEntityType(typeof(Context31246.Person))!;
        Assert.Equal(
            [nameof(Context31246.Person.Id), nameof(Context31246.Person.Address)],
            personType.GetComplexProperties().Select(p => p.Name));
        var idComplexType = personType.FindComplexProperty(nameof(Context31246.Person.Id))!.ComplexType;
        Assert.Equal(
            [nameof(Context31246.StronglyTypedId.Id)],
            idComplexType.GetProperties().Select(p => p.Name));

        var found = await context.Set<Context31246.Person>().SingleAsync(p => p.Address.City == "Seattle");
        Assert.Equal(new Context31246.StronglyTypedId(1), found.Id);
        Assert.Equal("98101", found.Address.PostalCode);
    }

    [Fact]
    public virtual async Task Can_update_entity_with_index_on_complex_type_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(
            seed: context =>
            {
                context.Add(
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(1),
                        Address = new Context31246.Address { City = "Seattle", PostalCode = "98101" }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var person = await context.Set<Context31246.Person>().SingleAsync();
            person.Address.PostalCode = "98102";
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var person = await context.Set<Context31246.Person>().SingleAsync();
            Assert.Equal("98102", person.Address.PostalCode);
        }
    }

    [Fact]
    public virtual async Task Can_delete_entity_with_index_on_complex_type_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(
            seed: context =>
            {
                context.Add(
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(1),
                        Address = new Context31246.Address { City = "Seattle", PostalCode = "98101" }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var person = await context.Set<Context31246.Person>().SingleAsync();
            context.Remove(person);
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            Assert.Equal(0, await context.Set<Context31246.Person>().CountAsync());
        }
    }

    [Fact]
    public virtual async Task Can_query_by_alternate_key_on_complex_type_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(
            seed: context =>
            {
                context.AddRange(
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(1),
                        Address = new Context31246.Address { City = "Seattle", PostalCode = "98101" }
                    },
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(2),
                        Address = new Context31246.Address { City = "Redmond", PostalCode = "98052" }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var found = await context.Set<Context31246.Person>().SingleAsync(p => p.Address.City == "Redmond");
        Assert.Equal(new Context31246.StronglyTypedId(2), found.Id);
    }

    [Fact]
    public virtual async Task Can_save_batch_swapping_alternate_key_values_on_complex_type_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(
            seed: context =>
            {
                context.AddRange(
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(1),
                        Address = new Context31246.Address { City = "Seattle", PostalCode = "98101" }
                    },
                    new Context31246.Person
                    {
                        Id = new Context31246.StronglyTypedId(2),
                        Address = new Context31246.Address { City = "Redmond", PostalCode = "98052" }
                    });
                return context.SaveChangesAsync();
            });

        // Update non-AK columns on multiple rows in the same batch. The CommandBatchPreparer
        // still has to read the alternate-key column values (Address_City) for each command in
        // order to build edges in the topological sort (AddUniqueValueEdges).
        await using (var context = contextFactory.CreateDbContext())
        {
            var people = await context.Set<Context31246.Person>().OrderBy(p => p.Id.Id).ToListAsync();
            people[0].Address.PostalCode = "98103";
            people[1].Address.PostalCode = "98054";
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var people = await context.Set<Context31246.Person>().OrderBy(p => p.Id.Id).ToListAsync();
            Assert.Equal("98103", people[0].Address.PostalCode);
            Assert.Equal("98054", people[1].Address.PostalCode);
        }
    }

    protected class Context31246(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Person>(b =>
            {
                b.ComplexProperty(p => p.Id);
                b.HasKey(p => p.Id.Id);
                b.Property(p => p.Id.Id).ValueGeneratedNever();
                b.HasAlternateKey(p => p.Address.City);
                b.HasIndex(p => p.Address.PostalCode);
            });

        public readonly record struct StronglyTypedId(int Id);

        public class Person
        {
            public StronglyTypedId Id { get; set; }
            public Address Address { get; set; } = null!;
        }

        public class Address
        {
            public string City { get; set; } = null!;
            public string PostalCode { get; set; } = null!;
        }
    }

    #endregion Issue31246

    #endregion Non-shared test resources
}
