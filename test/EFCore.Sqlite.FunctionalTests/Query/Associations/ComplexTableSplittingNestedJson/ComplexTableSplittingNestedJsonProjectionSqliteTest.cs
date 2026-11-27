// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplittingNestedJson;

public class ComplexTableSplittingNestedJsonProjectionSqliteTest(ComplexTableSplittingNestedJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingNestedJsonProjectionRelationalTestBase<ComplexTableSplittingNestedJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Select_complex_properties_with_identity_resolution(bool async)
    {
        await base.Select_complex_properties_with_identity_resolution(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."RequiredAssociate_Id", "r"."RequiredAssociate_Int", "r"."RequiredAssociate_Ints", "r"."RequiredAssociate_Name", "r"."RequiredAssociate_String", "r"."RequiredAssociate_NestedCollection", "r"."RequiredAssociate_OptionalNestedAssociate", "r"."RequiredAssociate_RequiredNestedAssociate", "r"."OptionalAssociate_Id", "r"."OptionalAssociate_Int", "r"."OptionalAssociate_Ints", "r"."OptionalAssociate_Name", "r"."OptionalAssociate_String", "r"."OptionalAssociate_NestedCollection", "r"."OptionalAssociate_OptionalNestedAssociate", "r"."OptionalAssociate_RequiredNestedAssociate"
FROM "RootEntity" AS "r"
""");
    }

    public override async Task Select_complex_properties_with_identity_resolution_member_init(bool async)
    {
        await base.Select_complex_properties_with_identity_resolution_member_init(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."RequiredAssociate_Id", "r"."RequiredAssociate_Int", "r"."RequiredAssociate_Ints", "r"."RequiredAssociate_Name", "r"."RequiredAssociate_String", "r"."RequiredAssociate_NestedCollection", "r"."RequiredAssociate_OptionalNestedAssociate", "r"."RequiredAssociate_RequiredNestedAssociate", "r"."OptionalAssociate_Id", "r"."OptionalAssociate_Int", "r"."OptionalAssociate_Ints", "r"."OptionalAssociate_Name", "r"."OptionalAssociate_String", "r"."OptionalAssociate_NestedCollection", "r"."OptionalAssociate_OptionalNestedAssociate", "r"."OptionalAssociate_RequiredNestedAssociate"
FROM "RootEntity" AS "r"
""");
    }

    public override Task Select_subquery_FirstOrDefault_complex_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Select_subquery_FirstOrDefault_complex_collection(queryTrackingBehavior));

    public override Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior));

    public override Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior));

    private static async Task AssertApplyNotSupported(Func<Task> query)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
