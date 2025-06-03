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
        => Fixture.TestSqlLoggerFactory.Clear();

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

    public override async Task Collate_is_null(bool async)
    {
        await base.Collate_is_null(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Customers" AS "c"
WHERE "c"."Region" IS NULL
""");
    }

    protected override string CaseInsensitiveCollation
        => "NOCASE";

    protected override string CaseSensitiveCollation
        => "BINARY";

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
