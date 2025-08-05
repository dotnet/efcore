// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class NorthwindBulkUpdatesRelationalTestBase<TFixture> : NorthwindBulkUpdatesTestBase<TFixture>
    where TFixture : NorthwindBulkUpdatesRelationalFixture<NoopModelCustomizer>, new()
{
    protected NorthwindBulkUpdatesRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Delete_non_entity_projection(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => base.Delete_non_entity_projection(async));

    public override Task Delete_non_entity_projection_2(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => base.Delete_non_entity_projection_2(async));

    public override Task Delete_non_entity_projection_3(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => base.Delete_non_entity_projection_3(async));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_FromSql_converted_to_subquery(bool async)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            () => Fixture.CreateContext(),
            (facade, transaction) => Fixture.UseTransaction(facade, transaction),
            async context =>
            {
                var queryable = context.Set<OrderDetail>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [OrderID], [ProductID], [UnitPrice], [Quantity], [Discount]
FROM [Order Details]
WHERE [OrderID] < 10300"));

                if (async)
                {
                    await queryable.ExecuteDeleteAsync();
                }
                else
                {
                    queryable.ExecuteDelete();
                }
            });

    public override Task Update_without_property_to_set_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.NoSetPropertyInvocation,
            () => base.Update_without_property_to_set_throws(async));

    public override Task Update_with_invalid_lambda_in_set_property_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.InvalidPropertyInSetProperty(
                new ExpressionPrinter().PrintExpression((OrderDetail e) => e.MaybeScalar(e => e.OrderID))),
            () => base.Update_with_invalid_lambda_in_set_property_throws(async));

    public override Task Update_multiple_tables_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.MultipleTablesInExecuteUpdate("c => c.e.OrderDate", "c => c.Customer.ContactName"),
            () => base.Update_multiple_tables_throws(async));

    public override Task Update_unmapped_property_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.InvalidPropertyInSetProperty("c => c.IsLondon"),
            () => base.Update_unmapped_property_throws(async));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_FromSql_set_constant(bool async)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            () => Fixture.CreateContext(),
            (facade, transaction) => Fixture.UseTransaction(facade, transaction),
            async context =>
            {
                var queryable = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address]
FROM [Customers]
WHERE [CustomerID] LIKE 'A%'"));

                if (async)
                {
                    await queryable.ExecuteUpdateAsync(s => s.SetProperty(c => c.ContactName, "Updated"));
                }
                else
                {
                    queryable.ExecuteUpdate(s => s.SetProperty(c => c.ContactName, "Updated"));
                }
            });

    protected static async Task AssertTranslationFailed(string details, Func<Task> query)
        => Assert.Contains(
            CoreStrings.NonQueryTranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query)).Message);

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected virtual void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
