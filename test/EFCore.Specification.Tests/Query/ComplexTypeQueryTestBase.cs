// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable enable

public abstract class ComplexTypeQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : ComplexTypeQueryFixtureBase, new()
{
    protected ComplexTypeQueryTestBase(TFixture fixture)
        : base(fixture)
    {
        fixture.ListLoggerFactory.Clear();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress.Country.Code == "DE"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.Country.Code == "DE"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_complex_type_on_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Where(cg => cg.OptionalCustomer!.ShippingAddress.ZipCode != 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_complex_type_on_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Where(cg => cg.RequiredCustomer.ShippingAddress.ZipCode != 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_complex_type_via_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Select(cg => cg.OptionalCustomer!.ShippingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_complex_type_via_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerGroup>().Select(cg => cg.RequiredCustomer.ShippingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Load_complex_type_after_subquery_on_entity_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.Country));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_single_property_on_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.Country.FullName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_type_Where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Where(a => a.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_type_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_type_equals_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress == c.BillingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_type_equals_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.ShippingAddress
                    == new Address
                    {
                        AddressLine1 = "804 S. Lakeshore Road",
                        ZipCode = 38654,
                        Country = new Country { FullName = "United States", Code = "US" }
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_type_equals_parameter(bool async)
    {
        var address = new Address
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new Country { FullName = "United States", Code = "US" }
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress == address));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_type_equals_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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
            ss => ss.Set<Customer>().Where(
                c => ss.Set<Customer>().Select(c => c.ShippingAddress).OrderBy(a => a.ZipCode).First() == address));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_complex_type(bool async)
    {
        var address = new Address
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new Country { FullName = "United States", Code = "US" }
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => ss.Set<Customer>().Select(c => c.ShippingAddress).Contains(address)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_entity_type_containing_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Concat(ss.Set<Customer>().Where(c => c.Id == 2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_entity_type_containing_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Union(ss.Set<Customer>().Where(c => c.Id == 2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Concat(ss.Set<Customer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Union(ss.Set<Customer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_property_in_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.AddressLine1)
                .Concat(ss.Set<Customer>().Select(c => c.BillingAddress.AddressLine1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_property_in_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.AddressLine1)
                .Union(ss.Set<Customer>().Select(c => c.BillingAddress.AddressLine1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_two_different_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Concat(ss.Set<Customer>().Select(c => c.BillingAddress)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_two_different_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress).Union(ss.Set<Customer>().Select(c => c.BillingAddress)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress.Country.Code == "DE"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_struct_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_struct_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.Country.Code == "DE"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(bool async)
    {
        return AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Where(cg => cg.OptionalCustomer!.ShippingAddress.ZipCode != 07728),
            ss => ss.Set<ValuedCustomerGroup>().Where(cg => cg.OptionalCustomer == null || cg.OptionalCustomer.ShippingAddress.ZipCode != 07728));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Where(cg => cg.RequiredCustomer.ShippingAddress.ZipCode != 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_struct_complex_type_via_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Select(cg => cg.OptionalCustomer!.ShippingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_struct_complex_type_via_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomerGroup>().Select(cg => cg.RequiredCustomer.ShippingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Load_struct_complex_type_after_subquery_on_entity_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.Country));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_single_property_on_nested_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.Country.FullName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_struct_complex_type_Where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Where(a => a.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_struct_complex_type_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Struct_complex_type_equals_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.ShippingAddress == c.BillingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Struct_complex_type_equals_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(
                c => c.ShippingAddress
                    == new AddressStruct
                    {
                        AddressLine1 = "804 S. Lakeshore Road",
                        ZipCode = 38654,
                        Country = new CountryStruct { FullName = "United States", Code = "US" }
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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
            ss => ss.Set<ValuedCustomer>().Where(
                c => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).OrderBy(a => a.ZipCode).First() == address));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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
            ss => ss.Set<ValuedCustomer>().Where(
                c => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Contains(address)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_entity_type_containing_struct_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Concat(ss.Set<ValuedCustomer>().Where(c => c.Id == 2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_entity_type_containing_struct_complex_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Union(ss.Set<ValuedCustomer>().Where(c => c.Id == 2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Concat(ss.Set<ValuedCustomer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Where(c => c.Id == 1).Select(c => c.ShippingAddress)
                .Union(ss.Set<ValuedCustomer>().Where(c => c.Id == 2).Select(c => c.ShippingAddress)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_property_in_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.AddressLine1)
                .Concat(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress.AddressLine1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_property_in_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress.AddressLine1)
                .Union(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress.AddressLine1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_two_different_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Concat(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_two_different_struct_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ValuedCustomer>().Select(c => c.ShippingAddress).Union(ss.Set<ValuedCustomer>().Select(c => c.BillingAddress)));

    protected DbContext CreateContext()
        => Fixture.CreateContext();
}
