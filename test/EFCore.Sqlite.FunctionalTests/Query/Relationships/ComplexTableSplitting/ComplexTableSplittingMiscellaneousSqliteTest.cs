// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public class ComplexTableSplittingMiscellaneousSqliteTest(
    ComplexTableSplittingSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingMiscellaneousRelationalTestBase<ComplexTableSplittingSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Where_related_property()
    {
        await base.Where_related_property();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated_Int" = 8
""");
    }

    public override async Task Where_optional_related_property()
    {
        await base.Where_optional_related_property();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
WHERE "r"."OptionalRelated_Int" = 8
""");
    }

    public override async Task Where_nested_related_property()
    {
        await base.Where_nested_related_property();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated_RequiredNested_Int" = 8
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
