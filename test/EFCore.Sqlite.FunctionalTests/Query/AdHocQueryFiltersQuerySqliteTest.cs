// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQueryFiltersQuerySqliteTest(NonSharedFixture fixture) : AdHocQueryFiltersQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task GroupBy_aggregate_over_required_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("p"."Value") AS "MaxValue"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""",
            //
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("p"."Value") AS "MaxValue"
FROM "Dependents" AS "d"
INNER JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id"
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_aggregates_of_every_kind_over_required_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_aggregates_of_every_kind_over_required_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", COALESCE(SUM("p"."Value"), 0) AS "Sum", AVG(CAST("p"."Value" AS REAL)) AS "Average", MIN("p"."Value") AS "Min", COUNT(CASE
    WHEN "p"."Id" IS NOT NULL AND "p"."Value" > 15 THEN 1
END) AS "Large", COUNT(CASE
    WHEN "p"."Id" IS NOT NULL AND "p"."Value" > 15 THEN 1
END) > 0 AS "AnyLarge"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_All_over_required_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_All_over_required_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COUNT(CASE
    WHEN "p"."Id" IS NOT NULL THEN CASE
        WHEN "p"."Value" > 15 THEN NULL
        ELSE 1
    END
END) = 0 AS "AllLarge"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_required_navigation_keeps_the_principals_own_filter_exact(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_keeps_the_principals_own_filter_exact(async);

        AssertSql(
            """
SELECT "c"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("s"."Value") AS "MaxValue"
FROM "CategorizedDependent38965" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Value"
    FROM "CategorizedPrincipal38965" AS "c0"
    INNER JOIN "Category38965" AS "c1" ON "c0"."CategoryId" = "c1"."Id" AND "c1"."DeletedOn" IS NULL
    WHERE "c1"."DeletedOn" IS NULL
) AS "s" ON "c"."PrincipalId" = "s"."Id"
GROUP BY "c"."GroupId"
""");
    }

    public override async Task GroupBy_aggregates_over_filtered_and_unfiltered_required_navigations(bool async)
    {
        await base.GroupBy_aggregates_over_filtered_and_unfiltered_required_navigations(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("p"."Value") AS "MaxValue", MAX("u"."Value") AS "MaxUnfiltered"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
INNER JOIN "Unfiltered38965" AS "u" ON "d"."UnfilteredId" = "u"."Id"
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_optional_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_optional_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("p"."Value") AS "MaxOptional"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."OptionalPrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_key_over_filtered_required_navigation_removes_the_rows(bool async)
    {
        await base.GroupBy_key_over_filtered_required_navigation_removes_the_rows(async);

        AssertSql(
            """
SELECT "p"."Value" AS "Key", COUNT(*) AS "Count", MAX("p"."Value") AS "MaxValue"
FROM "Dependents" AS "d"
INNER JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "p"."Value"
""");
    }

    public override async Task GroupBy_aggregate_over_chain_with_query_filter_on_the_far_principal(bool async)
    {
        await base.GroupBy_aggregate_over_chain_with_query_filter_on_the_far_principal(async);

        AssertSql(
            """
SELECT "c"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("c2"."Value") AS "MaxValue"
FROM "ChainDependent38965" AS "c"
INNER JOIN "ChainMiddle38965" AS "c0" ON "c"."MiddleId" = "c0"."Id"
LEFT JOIN (
    SELECT "c1"."Id", "c1"."Value"
    FROM "ChainLeaf38965" AS "c1"
    WHERE NOT ("c1"."Filtered")
) AS "c2" ON "c0"."LeafId" = "c2"."Id"
GROUP BY "c"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_self_referencing_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_self_referencing_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT "e"."DepartmentId" AS "Key", COUNT(*) AS "Count", MAX("e0"."Salary") AS "MaxManagerSalary"
FROM "Employee38965" AS "e"
LEFT JOIN "Employee38965" AS "e0" ON "e"."ManagerId" = "e0"."Id" AND NOT ("e0"."Deleted")
WHERE NOT ("e"."Deleted")
GROUP BY "e"."DepartmentId"
""");
    }

    public override async Task GroupBy_aggregate_over_required_navigation_to_TPH_principal_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_to_TPH_principal_with_query_filter(async);

        AssertSql(
            """
SELECT "i"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("i0"."Value") AS "MaxValue"
FROM "InheritanceDependent38965" AS "i"
LEFT JOIN "InheritancePrincipal38965" AS "i0" ON "i"."PrincipalId" = "i0"."Id" AND NOT ("i0"."Filtered")
GROUP BY "i"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_required_navigation_to_TPT_principal_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_to_TPT_principal_with_query_filter(async);

        AssertSql(
            """
SELECT "i"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("i0"."Value") AS "MaxValue"
FROM "InheritanceDependent38965" AS "i"
LEFT JOIN "InheritancePrincipal38965" AS "i0" ON "i"."PrincipalId" = "i0"."Id" AND NOT ("i0"."Filtered")
GROUP BY "i"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_required_navigation_to_TPC_principal_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_to_TPC_principal_with_query_filter(async);

        AssertSql(
            """
SELECT "i"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("u0"."Value") AS "MaxValue"
FROM "InheritanceDependent38965" AS "i"
LEFT JOIN (
    SELECT "u"."Id", "u"."Value"
    FROM (
        SELECT "i0"."Id", "i0"."Filtered", "i0"."Value"
        FROM "InheritancePrincipal38965" AS "i0"
        UNION ALL
        SELECT "i1"."Id", "i1"."Filtered", "i1"."Value"
        FROM "InheritanceDerivedPrincipal38965" AS "i1"
    ) AS "u"
    WHERE NOT ("u"."Filtered")
) AS "u0" ON "i"."PrincipalId" = "u0"."Id"
GROUP BY "i"."GroupId"
""");
    }

    public override async Task GroupBy_aggregates_sharing_one_filtered_navigation(bool async)
    {
        await base.GroupBy_aggregates_sharing_one_filtered_navigation(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", MAX("p"."Value") AS "MaxValue", MIN("p"."Value") AS "MinValue"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_filtered_navigation_after_Take(bool async)
    {
        await base.GroupBy_aggregate_over_filtered_navigation_after_Take(async);

        AssertSql(
            """
@p='2'

SELECT "d0"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("p"."Value") AS "MaxValue"
FROM (
    SELECT "d"."GroupId", "d"."PrincipalId"
    FROM "Dependents" AS "d"
    ORDER BY "d"."GroupId"
    LIMIT @p
) AS "d0"
LEFT JOIN "Principals" AS "p" ON "d0"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d0"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_required_navigation_with_null_observing_selector(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_with_null_observing_selector(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COALESCE(SUM(CASE
    WHEN "p"."Id" IS NOT NULL THEN COALESCE("p"."Value", 5)
END), 0) AS "SumOrFive", COUNT(CASE
    WHEN "p"."Id" IS NOT NULL AND COALESCE("p"."Value", 0) = 0 THEN 1
END) AS "CountOfZero", COUNT(CASE
    WHEN "p"."Id" IS NOT NULL AND COALESCE("p"."Value", 0) = 0 THEN 1
END) > 0 AS "AnyZero"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_reaching_the_principal_through_EF_Property(bool async)
    {
        await base.GroupBy_aggregate_reaching_the_principal_through_EF_Property(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", MAX("p"."Value") AS "MaxValue", COALESCE(SUM(CASE
    WHEN "p"."Id" IS NOT NULL THEN COALESCE("p"."Value", 5)
END), 0) AS "SumOrFive"
FROM "Dependents" AS "d"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_chain_with_two_filtered_principals(bool async)
    {
        await base.GroupBy_aggregate_over_chain_with_two_filtered_principals(async);

        AssertSql(
            """
SELECT "c"."GroupId" AS "Key", COALESCE(SUM(CASE
    WHEN "c1"."Id" IS NOT NULL AND "c3"."Id" IS NOT NULL THEN COALESCE("c3"."Value", 5)
END), 0) AS "SumOrFive", COUNT(CASE
    WHEN "c1"."Id" IS NOT NULL AND "c3"."Id" IS NOT NULL THEN CASE
        WHEN "c3"."Value" > 100 THEN NULL
        ELSE 1
    END
END) = 0 AS "AllBig"
FROM "ChainDependent38965" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."LeafId"
    FROM "ChainMiddle38965" AS "c0"
    WHERE NOT ("c0"."Filtered")
) AS "c1" ON "c"."MiddleId" = "c1"."Id"
LEFT JOIN (
    SELECT "c2"."Id", "c2"."Value"
    FROM "ChainLeaf38965" AS "c2"
    WHERE NOT ("c2"."Filtered")
) AS "c3" ON "c1"."LeafId" = "c3"."Id"
GROUP BY "c"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_filtered_principal_lifted_by_an_unfiltered_sibling(bool async)
    {
        await base.GroupBy_aggregate_over_filtered_principal_lifted_by_an_unfiltered_sibling(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("u"."Value") AS "MaxUnfiltered", COALESCE(SUM(CASE
    WHEN "p"."Id" IS NOT NULL THEN COALESCE("p"."Value", 5)
END), 0) AS "SumOrFive"
FROM "Dependents" AS "d"
INNER JOIN "Unfiltered38965" AS "u" ON "d"."UnfilteredId" = "u"."Id"
LEFT JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
GROUP BY "d"."GroupId"
""");
    }

    public override async Task GroupBy_aggregate_over_filtered_principal_behind_an_optional_navigation(bool async)
    {
        await base.GroupBy_aggregate_over_filtered_principal_behind_an_optional_navigation(async);

        AssertSql(
            """
SELECT "o"."GroupId" AS "Key", COALESCE(SUM(COALESCE("c1"."Value", 5)), 0) AS "SumOrFive"
FROM "OptionalChainDependent38965" AS "o"
LEFT JOIN "ChainMiddle38965" AS "c" ON "o"."MiddleId" = "c"."Id"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Value"
    FROM "ChainLeaf38965" AS "c0"
    WHERE NOT ("c0"."Filtered")
) AS "c1" ON "c"."LeafId" = "c1"."Id"
GROUP BY "o"."GroupId"
""");
    }
}
