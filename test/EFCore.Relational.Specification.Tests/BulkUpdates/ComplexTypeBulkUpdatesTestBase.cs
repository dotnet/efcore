// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class ComplexTypeBulkUpdatesTestBase<TFixture> : BulkUpdatesTestBase<TFixture>
    where TFixture : ComplexTypeBulkUpdatesFixtureBase, new()
{
    protected ComplexTypeBulkUpdatesTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_entity_type_with_complex_type(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Customer>().Where(e => e.Name == "Monty Elias"),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_complex_type_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => AssertDelete(
                async,
                ss => ss.Set<Customer>().Select(c => c.ShippingAddress),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_property_inside_complex_type(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress.ZipCode == 07728),
            e => e,
            s => s.SetProperty(c => c.ShippingAddress.ZipCode, 12345),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_property_inside_nested_complex_type(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress.Country.Code == "US"),
            e => e,
            s => s.SetProperty(c => c.ShippingAddress.Country.FullName, "United States Modified"),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_multiple_properties_inside_multiple_complex_types_and_on_entity_type(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress.ZipCode == 07728),
            e => e,
            s => s
                .SetProperty(c => c.Name, c => c.Name + "Modified")
                .SetProperty(c => c.ShippingAddress.ZipCode, c => c.BillingAddress.ZipCode)
                .SetProperty(c => c.BillingAddress.ZipCode, 54321),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_projected_complex_type(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress),
            a => a,
            s => s.SetProperty(c => c.ZipCode, 12345),
            rowsAffectedCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_multiple_projected_complex_types_via_anonymous_type(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Select(
                c => new
                {
                    c.ShippingAddress,
                    c.BillingAddress,
                    Customer = c
                }),
            x => x.Customer,
            s => s
                .SetProperty(x => x.ShippingAddress.ZipCode, x => x.BillingAddress.ZipCode)
                .SetProperty(x => x.BillingAddress.ZipCode, 54321),
            rowsAffectedCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_projected_complex_type_via_OrderBy_Skip_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteUpdateSubqueryNotSupportedOverComplexTypes("Customer.ShippingAddress#Address"),
            () => AssertUpdate(
                async,
                ss => ss.Set<Customer>().Select(c => c.ShippingAddress).OrderBy(a => a.ZipCode).Skip(1),
                a => a,
                s => s.SetProperty(c => c.ZipCode, 12345),
                rowsAffectedCount: 3));

    private void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
