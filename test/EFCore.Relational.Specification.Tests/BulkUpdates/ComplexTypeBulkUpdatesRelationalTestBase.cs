// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class ComplexTypeBulkUpdatesRelationalTestBase<TFixture> : ComplexTypeBulkUpdatesTestBase<TFixture>
    where TFixture : ComplexTypeBulkUpdatesRelationalFixtureBase, new()
{
    protected ComplexTypeBulkUpdatesRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Delete_complex_type(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => base.Delete_complex_type(async));

    public override Task Update_projected_complex_type_via_OrderBy_Skip(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteUpdateSubqueryNotSupportedOverComplexTypes("Customer.ShippingAddress#Address"),
            () => base.Update_projected_complex_type_via_OrderBy_Skip(async));

    protected static async Task AssertTranslationFailed(string details, Func<Task> query)
        => Assert.Contains(
            CoreStrings.NonQueryTranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query)).Message);

    private void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
