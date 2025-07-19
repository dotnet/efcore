// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public class ComplexTableSplittingStructuralEqualitySqliteTest(
    ComplexTableSplittingSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingStructuralEqualityRelationalTestBase<ComplexTableSplittingSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related(bool async)
    {
        await base.Two_related(async);

        AssertSql();
    }

    public override async Task Two_nested(bool async)
    {
        await base.Two_nested(async);

        AssertSql();
    }

    public override async Task Not_equals(bool async)
    {
        await base.Not_equals(async);

        AssertSql();
    }

    public override async Task Related_with_inline_null(bool async)
    {
        await base.Related_with_inline_null(async);

        AssertSql();
    }

    public override async Task Related_with_parameter_null(bool async)
    {
        await base.Related_with_parameter_null(async);

        AssertSql();
    }

    public override async Task Nested_with_inline_null(bool async)
    {
        await base.Nested_with_inline_null(async);

        AssertSql();
    }

    public override async Task Nested_with_inline(bool async)
    {
        await base.Nested_with_inline(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated_RequiredNested_Id" = 1000 AND "r"."RequiredRelated_RequiredNested_Int" = 8 AND "r"."RequiredRelated_RequiredNested_Name" = 'Root1_RequiredRelated_RequiredNested' AND "r"."RequiredRelated_RequiredNested_String" = 'foo'
""");
    }

    public override async Task Nested_with_parameter(bool async)
    {
        await base.Nested_with_parameter(async);

        AssertSql(
            """
@entity_equality_nested_Id='?' (DbType = Int32)
@entity_equality_nested_Int='?' (DbType = Int32)
@entity_equality_nested_Name='?' (Size = 36)
@entity_equality_nested_String='?' (Size = 3)

SELECT "r"."Id", "r"."Name", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated_RequiredNested_Id" = @entity_equality_nested_Id AND "r"."RequiredRelated_RequiredNested_Int" = @entity_equality_nested_Int AND "r"."RequiredRelated_RequiredNested_Name" = @entity_equality_nested_Name AND "r"."RequiredRelated_RequiredNested_String" = @entity_equality_nested_String
""");
    }

    public override async Task Two_nested_collections(bool async)
    {
        await base.Two_nested_collections(async);

        AssertSql();
    }

    public override async Task Nested_collection_with_inline(bool async)
    {
        await base.Nested_collection_with_inline(async);

        AssertSql();
    }

    public override async Task Nested_collection_with_parameter(bool async)
    {
        await base.Nested_collection_with_parameter(async);

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
