// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexTypeQueryInMemoryTest(ComplexTypeQueryInMemoryTest.ComplexTypeQueryInMemoryFixture fixture)
    : ComplexTypeQueryTestBase<ComplexTypeQueryInMemoryTest.ComplexTypeQueryInMemoryFixture>(fixture)
{
    public override async Task Subquery_over_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Subquery_over_complex_type(async));

        Assert.Equal(InMemoryStrings.SubqueryOverComplexTypesNotSupported("Customer.ShippingAddress#Address"), exception.Message);
    }

    public override async Task Concat_two_different_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Concat_two_different_complex_type(async));

        Assert.Equal(
            InMemoryStrings.SetOperationOverDifferentStructuralTypes(
                "Customer.ShippingAddress#Address", "Customer.BillingAddress#Address"), exception.Message);
    }

    public override async Task Union_two_different_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Union_two_different_complex_type(async));

        Assert.Equal(
            InMemoryStrings.SetOperationOverDifferentStructuralTypes(
                "Customer.ShippingAddress#Address", "Customer.BillingAddress#Address"), exception.Message);
    }

    public override async Task Subquery_over_struct_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Subquery_over_struct_complex_type(async));

        Assert.Equal(
            InMemoryStrings.SubqueryOverComplexTypesNotSupported("ValuedCustomer.ShippingAddress#AddressStruct"), exception.Message);
    }

    public override async Task Concat_two_different_struct_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Concat_two_different_struct_complex_type(async));

        Assert.Equal(
            InMemoryStrings.SetOperationOverDifferentStructuralTypes(
                "ValuedCustomer.ShippingAddress#AddressStruct", "ValuedCustomer.BillingAddress#AddressStruct"), exception.Message);
    }

    public override async Task Union_two_different_struct_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Union_two_different_struct_complex_type(async));

        Assert.Equal(
            InMemoryStrings.SetOperationOverDifferentStructuralTypes(
                "ValuedCustomer.ShippingAddress#AddressStruct", "ValuedCustomer.BillingAddress#AddressStruct"), exception.Message);
    }

    public class ComplexTypeQueryInMemoryFixture : ComplexTypeQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
