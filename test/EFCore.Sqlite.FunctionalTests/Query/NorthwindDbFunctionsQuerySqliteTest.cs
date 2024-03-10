// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindDbFunctionsQuerySqliteTest : NorthwindDbFunctionsQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindDbFunctionsQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Glob(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Glob(c.ContactName, "*M*"),
            c => c.ContactName.Contains("M"));

        AssertSql(
            """
SELECT COUNT(*)
FROM "Customers" AS "c"
WHERE "c"."ContactName" GLOB '*M*'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Glob_negated(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => !EF.Functions.Glob(c.CustomerID, "T*"),
            c => !c.CustomerID.StartsWith("T"));

        AssertSql(
            """
SELECT COUNT(*)
FROM "Customers" AS "c"
WHERE "c"."CustomerID" NOT GLOB 'T*'
""");
    }

    protected override string CaseInsensitiveCollation
        => "NOCASE";

    protected override string CaseSensitiveCollation
        => "BINARY";

    public override async Task Random_return_less_than_1(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            ss => EF.Functions.Random() <= 1,
            c => true);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Orders" AS "o"
WHERE abs(random() / 9.2233720368547799E+18) <= 1.0
""");
    }

    public override async Task Random_return_greater_than_0(bool async)
    {
        await base.Random_return_greater_than_0(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Orders" AS "o"
WHERE abs(random() / 9.2233720368547799E+18) >= 0.0
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
