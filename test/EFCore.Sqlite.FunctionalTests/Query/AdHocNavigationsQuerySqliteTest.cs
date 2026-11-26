// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocNavigationsQuerySqliteTest(NonSharedFixture fixture) : AdHocNavigationsQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Include_collection_with_complex_struct_key_39722(bool async)
    {
        await base.Include_collection_with_complex_struct_key_39722(async);
        AssertSql(
            """
SELECT "p"."Id_Value", "c"."Id", "c"."ParentId"
FROM "Parent" AS "p"
LEFT JOIN "Child" AS "c" ON "p"."Id_Value" = "c"."ParentId"
ORDER BY "p"."Id_Value"
""");
    }

    public override async Task Include_collection_with_complex_struct_key_and_filter_39722(bool async)
    {
        await base.Include_collection_with_complex_struct_key_and_filter_39722(async);
        AssertSql(
            """
@entity_equality_id_Value='00000000-0000-0000-0000-000000000001' (Nullable = true)

SELECT "p0"."Id_Value", "c"."Id", "c"."ParentId"
FROM (
    SELECT "p"."Id_Value"
    FROM "Parent" AS "p"
    WHERE "p"."Id_Value" = @entity_equality_id_Value
    LIMIT 2
) AS "p0"
LEFT JOIN "Child" AS "c" ON "p0"."Id_Value" = "c"."ParentId"
ORDER BY "p0"."Id_Value"
""");
    }

    public override async Task Include_reference_with_complex_struct_key_39722(bool async)
    {
        await base.Include_reference_with_complex_struct_key_39722(async);
        AssertSql(
            """
SELECT "c"."Id", "c"."ParentId", "p"."Id_Value"
FROM "Child" AS "c"
LEFT JOIN "Parent" AS "p" ON "c"."ParentId" = "p"."Id_Value"
ORDER BY "c"."Id"
""");
    }

    public override async Task Find_with_complex_struct_key_39722(bool async)
    {
        await base.Find_with_complex_struct_key_39722(async);
        AssertSql(
            """
@p='00000000-0000-0000-0000-000000000001' (Nullable = true)

SELECT "p"."Id_Value"
FROM "Parent" AS "p"
WHERE "p"."Id_Value" = @p
LIMIT 1
""");
    }

    public override async Task Load_reference_with_complex_struct_key_39722(bool async)
    {
        await base.Load_reference_with_complex_struct_key_39722(async);
        AssertSql(
            """
SELECT "c"."Id", "c"."ParentId"
FROM "Child" AS "c"
WHERE "c"."Id" = 11
LIMIT 2
""",
            //
            """
@p='00000000-0000-0000-0000-000000000001' (Nullable = true)

SELECT "p"."Id_Value"
FROM "Parent" AS "p"
WHERE "p"."Id_Value" = @p
LIMIT 1
""");
    }

    public override async Task Include_collection_with_complex_composite_key_39722(bool async)
    {
        await base.Include_collection_with_complex_composite_key_39722(async);
        AssertSql(
            """
SELECT "p"."Id_Id", "p"."Id_TenantId", "c"."Id", "c"."ParentId", "c"."ParentTenantId"
FROM "Parent" AS "p"
LEFT JOIN "Child" AS "c" ON "p"."Id_Id" = "c"."ParentId" AND "p"."Id_TenantId" = "c"."ParentTenantId"
ORDER BY "p"."Id_Id", "p"."Id_TenantId"
""");
    }

    public override async Task Find_with_complex_composite_key_and_auto_include_39722(bool async)
    {
        await base.Find_with_complex_composite_key_and_auto_include_39722(async);
        AssertSql(
            """
@p='1'
@p1='B' (Size = 1)

SELECT "p0"."Id_Id", "p0"."Id_TenantId", "c"."Id", "c"."ParentId", "c"."ParentTenantId"
FROM (
    SELECT "p"."Id_Id", "p"."Id_TenantId"
    FROM "Parent" AS "p"
    WHERE "p"."Id_Id" = @p AND "p"."Id_TenantId" = @p1
    LIMIT 1
) AS "p0"
LEFT JOIN "Child" AS "c" ON "p0"."Id_Id" = "c"."ParentId" AND "p0"."Id_TenantId" = "c"."ParentTenantId"
ORDER BY "p0"."Id_Id", "p0"."Id_TenantId"
""");
    }

    public override async Task Include_and_find_with_nested_complex_key_39722(bool async)
    {
        await base.Include_and_find_with_nested_complex_key_39722(async);
        AssertSql(
            """
SELECT "p0"."Id_Inner_Value", "c"."Id", "c"."ParentId"
FROM (
    SELECT "p"."Id_Inner_Value"
    FROM "Parent" AS "p"
    LIMIT 2
) AS "p0"
LEFT JOIN "Child" AS "c" ON "p0"."Id_Inner_Value" = "c"."ParentId"
ORDER BY "p0"."Id_Inner_Value"
""",
            //
            """
@p='3'

SELECT "p"."Id_Inner_Value"
FROM "Parent" AS "p"
WHERE "p"."Id_Inner_Value" = @p
LIMIT 1
""");
    }

    public override async Task Include_and_find_with_inherited_complex_key_type_39722(bool async)
    {
        await base.Include_and_find_with_inherited_complex_key_type_39722(async);
        AssertSql(
            """
SELECT "p"."Value", "p"."Id_Value", "c"."Id", "c"."ParentId"
FROM "Parent" AS "p"
LEFT JOIN "Child" AS "c" ON "p"."Id_Value" = "c"."ParentId"
ORDER BY "p"."Id_Value"
""",
            //
            """
@p='1'

SELECT "p"."Value", "p"."Id_Value"
FROM "Parent" AS "p"
WHERE "p"."Id_Value" = @p
LIMIT 1
""");
    }

    public override async Task Projection_with_multiple_includes_and_subquery_with_set_operation()
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Projection_with_multiple_includes_and_subquery_with_set_operation())).Message);

        AssertSql();
    }

    public override async Task Let_multiple_references_with_reference_to_outer()
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Let_multiple_references_with_reference_to_outer())).Message);

        AssertSql();
    }

    public override async Task SelectMany_and_collection_in_projection_in_FirstOrDefault()
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_and_collection_in_projection_in_FirstOrDefault()))
            .Message);

        AssertSql();
    }

    public override async Task Consecutive_selects_with_conditional_projection_should_not_include_unnecessary_joins(bool async)
    {
        await base.Consecutive_selects_with_conditional_projection_should_not_include_unnecessary_joins(async);

        AssertSql(
            """
SELECT "u"."Id", "j"."Id" IS NULL, "j"."Id"
FROM "Users" AS "u"
LEFT JOIN "Job" AS "j" ON "u"."JobId" = "j"."Id"
WHERE "u"."Id" = 1
LIMIT 1
""");
    }

    public override async Task Consecutive_selects_with_conditional_projection_null_navigation_returns_null(bool async)
    {
        await base.Consecutive_selects_with_conditional_projection_null_navigation_returns_null(async);

        AssertSql(
            """
SELECT "u"."Id", "j"."Id" IS NULL, "j"."Id"
FROM "Users" AS "u"
LEFT JOIN "Job" AS "j" ON "u"."JobId" = "j"."Id"
WHERE "u"."JobId" IS NULL
LIMIT 1
""");
    }

    public override async Task Consecutive_selects_with_conditional_projection_nested_navigation_accessed_includes_join(bool async)
    {
        await base.Consecutive_selects_with_conditional_projection_nested_navigation_accessed_includes_join(async);

        AssertSql(
            """
SELECT "u"."Id", "j"."Id" IS NULL, "j"."Id", "a"."Id"
FROM "Users" AS "u"
LEFT JOIN "Job" AS "j" ON "u"."JobId" = "j"."Id"
LEFT JOIN "Address" AS "a" ON "j"."AddressId" = "a"."Id"
WHERE "u"."Id" = 1
LIMIT 1
""");
    }

    public override async Task Filtered_collection_through_optional_navigation_does_not_match_on_null_keys(bool async)
    {
        await base.Filtered_collection_through_optional_navigation_does_not_match_on_null_keys(async);

        AssertSql(
            """
SELECT "p"."Name", "p"."PersonId", "p0"."Name", "p0"."PersonId"
FROM "People" AS "p"
LEFT JOIN "Employers" AS "e" ON "p"."EmployerId" = "e"."EmployerId"
LEFT JOIN "People" AS "p0" ON "e"."EmployerId" IS NOT NULL AND "e"."EmployerId" = "p0"."EmployerId" AND "p"."PersonId" <> "p0"."PersonId"
ORDER BY "p"."PersonId"
""");
    }
}
