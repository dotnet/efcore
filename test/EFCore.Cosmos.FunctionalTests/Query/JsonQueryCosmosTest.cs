// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQueryCosmosTest : JsonQueryTestBase<JsonQueryCosmosFixture>
{
    private const string NotImplementedBindPropertyMessage
        = "Bind property on structural type coming out of scalar subquery";

    public JsonQueryCosmosTest(JsonQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Basic_json_projection_enum_inside_json_entity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_enum_inside_json_entity(a);

                AssertSql(
                    """
SELECT c["Id"], c["OwnedReferenceRoot"]["OwnedReferenceBranch"]["Enum"]
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override async Task Basic_json_projection_owned_collection_branch_NoTrackingWithIdentityResolution(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Basic_json_projection_owned_collection_branch_NoTrackingWithIdentityResolution(async));
        }
    }

    public override async Task Basic_json_projection_owned_collection_leaf(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Basic_json_projection_owned_collection_leaf(async));
        }
    }

    public override Task Basic_json_projection_owned_collection_root_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owned_collection_root_NoTrackingWithIdentityResolution(a);

                // TODO: issue #34067 (?)
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Basic_json_projection_owned_reference_branch_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owned_reference_branch_NoTrackingWithIdentityResolution(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Basic_json_projection_owned_reference_duplicated2_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owned_reference_duplicated2_NoTrackingWithIdentityResolution(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
ORDER BY c["Id"]
""");
            });

    public override Task Basic_json_projection_owned_reference_duplicated_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owned_reference_duplicated_NoTrackingWithIdentityResolution(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
ORDER BY c["Id"]
""");
            });

    public override Task Basic_json_projection_owned_reference_leaf(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owned_reference_leaf(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Basic_json_projection_owned_reference_root_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owned_reference_root_NoTrackingWithIdentityResolution(a);

                // TODO: issue #34067 (?)
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Basic_json_projection_owner_entity_duplicated_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owner_entity_duplicated_NoTrackingWithIdentityResolution(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "SingleOwned")
""");
            });

    public override Task Basic_json_projection_owner_entity_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owner_entity_NoTrackingWithIdentityResolution(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Basic_json_projection_owner_entity_twice_NoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_owner_entity_twice_NoTrackingWithIdentityResolution(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Basic_json_projection_scalar(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Basic_json_projection_scalar(a);

                AssertSql(
                    """
SELECT VALUE c["OwnedReferenceRoot"]["Name"]
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    [ConditionalTheory(Skip = "issue #34350")]
    public override Task Custom_naming_projection_everything(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Custom_naming_projection_everything(a);

                AssertSql("");
            });

    public override Task Custom_naming_projection_owned_collection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Custom_naming_projection_owned_collection(a);

                // TODO: issue #34067 (?)
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "CustomNaming")
ORDER BY c["Id"]
""");
            });

    public override Task Custom_naming_projection_owned_reference(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Custom_naming_projection_owned_reference(async);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "CustomNaming")
""");
            });

    public override Task Custom_naming_projection_owned_scalar(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Custom_naming_projection_owned_scalar(a);

                AssertSql(
                    """
SELECT VALUE c["OwnedReferenceRoot"]["OwnedReferenceBranch"]["Fraction"]
FROM root c
WHERE (c["Discriminator"] = "CustomNaming")
""");
            });

    public override Task Custom_naming_projection_owner_entity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Custom_naming_projection_owner_entity(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "CustomNaming")
""");
            });

    public override async Task Entity_including_collection_with_json(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Entity_including_collection_with_json(async))).Message;

        Assert.Equal(
            CosmosStrings.NonEmbeddedIncludeNotSupported(
                "Navigation: EntityBasic.JsonEntityBasics (List<JsonEntityBasic>) Collection ToDependent JsonEntityBasic"),
            message);
    }

    [ConditionalTheory(Skip = "issue #17313")]
    public override Task Group_by_FirstOrDefault_on_json_scalar(bool async)
        => base.Group_by_FirstOrDefault_on_json_scalar(async);

    [ConditionalTheory(Skip = "issue #17313")]
    public override Task Group_by_First_on_json_scalar(bool async)
        => base.Group_by_First_on_json_scalar(async);

    [ConditionalTheory(Skip = "issue #17313")]
    public override Task Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(bool async)
        => base.Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(async);

    [ConditionalTheory(Skip = "issue #17313")]
    public override Task Group_by_json_scalar_Skip_First_project_json_scalar(bool async)
        => base.Group_by_json_scalar_Skip_First_project_json_scalar(async);

    [ConditionalTheory(Skip = "issue #17313")]
    public override Task Group_by_on_json_scalar(bool async)
        => base.Group_by_on_json_scalar(async);

    [ConditionalTheory(Skip = "issue #17313")]
    public override Task Group_by_on_json_scalar_using_collection_indexer(bool async)
        => base.Group_by_on_json_scalar_using_collection_indexer(async);

    [ConditionalTheory(Skip = "issue #17313")]
    public override Task Group_by_Skip_Take_on_json_scalar(bool async)
        => base.Group_by_Skip_Take_on_json_scalar(async);

    [SkipOnCiCondition]
    public override Task Json_all_types_entity_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_all_types_entity_projection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "AllTypes")
""");
            });

    [SkipOnCiCondition]
    public override Task Json_all_types_projection_from_owned_entity_reference(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_all_types_projection_from_owned_entity_reference(a);

                // TODO: issue #34067 (?)
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "AllTypes")
""");
            });

    public override Task Json_all_types_projection_individual_properties(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_all_types_projection_individual_properties(a);

                AssertSql(
                    """
SELECT c["Reference"]["TestDefaultString"], c["Reference"]["TestMaxLengthString"], c["Reference"]["TestBoolean"], c["Reference"]["TestByte"], c["Reference"]["TestCharacter"], c["Reference"]["TestDateTime"], c["Reference"]["TestDateTimeOffset"], c["Reference"]["TestDecimal"], c["Reference"]["TestDouble"], c["Reference"]["TestGuid"], c["Reference"]["TestInt16"], c["Reference"]["TestInt32"], c["Reference"]["TestInt64"], c["Reference"]["TestSignedByte"], c["Reference"]["TestSingle"], c["Reference"]["TestTimeSpan"], c["Reference"]["TestDateOnly"], c["Reference"]["TestTimeOnly"], c["Reference"]["TestUnsignedInt16"], c["Reference"]["TestUnsignedInt32"], c["Reference"]["TestUnsignedInt64"], c["Reference"]["TestEnum"], c["Reference"]["TestEnumWithIntConverter"], c["Reference"]["TestNullableEnum"], c["Reference"]["TestNullableEnumWithIntConverter"], c["Reference"]["TestNullableEnumWithConverterThatHandlesNulls"]
FROM root c
WHERE (c["Discriminator"] = "AllTypes")
""");
            });

    [SkipOnCiCondition]
    public override Task Json_boolean_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_boolean_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND c["Reference"]["TestBoolean"])
""");
            });

    public override Task Json_boolean_predicate_negated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_boolean_predicate_negated(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND NOT(c["Reference"]["TestBoolean"]))
""");
            });

    public override Task Json_boolean_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_boolean_projection(a);

                AssertSql(
                    """
SELECT VALUE c["Reference"]["TestBoolean"]
FROM root c
WHERE (c["Discriminator"] = "AllTypes")
""");
            });

    public override Task Json_boolean_projection_negated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_boolean_projection_negated(a);

                AssertSql(
                    """
SELECT VALUE NOT(c["Reference"]["TestBoolean"])
FROM root c
WHERE (c["Discriminator"] = "AllTypes")
""");
            });

    public override Task Json_branch_collection_distinct_and_other_collection(bool async)
        => AssertTranslationFailed(
            () => base.Json_branch_collection_distinct_and_other_collection(async));

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_not_present(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_present(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_not_present(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_present(a);

                AssertSql("");
            });

    public override Task Json_collection_anonymous_projection_distinct_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_anonymous_projection_distinct_in_projection(a);

                AssertSql("");
            });

    public override Task Json_collection_Any_with_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_Any_with_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND EXISTS (
    SELECT 1
    FROM o IN c["OwnedReferenceRoot"]["OwnedCollectionBranch"]
    WHERE (o["OwnedReferenceLeaf"]["SomethingSomething"] = "e1_r_c1_r")))
""");
            });

    public override Task Json_collection_Distinct_Count_with_predicate(bool async)
        => AssertTranslationFailed(
            () => base.Json_collection_Distinct_Count_with_predicate(async));

    public override Task Json_collection_distinct_in_projection(bool async)
        => AssertTranslationFailed(
            () => base.Json_collection_distinct_in_projection(async));

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_ElementAtOrDefault_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_ElementAtOrDefault_in_projection(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_ElementAtOrDefault_project_collection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_ElementAtOrDefault_project_collection(a);

                AssertSql("");
            });

    public override Task Json_collection_ElementAt_and_pushdown(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_ElementAt_and_pushdown(a);

                AssertSql(
                    """
SELECT VALUE
{
    "Id" : c["Id"],
    "CollectionElement" : c["OwnedCollectionRoot"][0]["Number"]
}
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Json_collection_ElementAt_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_ElementAt_in_predicate(a);

                AssertSql(
                    """
SELECT VALUE c["Id"]
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (c["OwnedCollectionRoot"][1]["Name"] != "Foo"))
""");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_ElementAt_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_ElementAt_in_projection(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_ElementAt_project_collection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_ElementAt_project_collection(a);

                AssertSql("");
            });

    public override Task Json_collection_filter_in_projection(bool async)
        => AssertTranslationFailed(
            () => base.Json_collection_filter_in_projection(async));

    public override async Task Json_collection_index_in_predicate_nested_mix(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_predicate_nested_mix(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_predicate_using_column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = (await Assert.ThrowsAsync<CosmosException>(
                () => base.Json_collection_index_in_predicate_using_column(async)));
        }
    }

    public override async Task Json_collection_index_in_predicate_using_complex_expression1(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = (await Assert.ThrowsAsync<CosmosException>(
                () => base.Json_collection_index_in_predicate_using_complex_expression1(async)));
        }
    }

    public override Task Json_collection_index_in_predicate_using_complex_expression2(bool async)
        => AssertTranslationFailed(
            () => base.Json_collection_index_in_predicate_using_complex_expression2(async));

    public override Task Json_collection_index_in_predicate_using_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_predicate_using_constant(a);

                AssertSql(
                    """
SELECT VALUE c["Id"]
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (c["OwnedCollectionRoot"][0]["Name"] != "Foo"))
""");
            });

    public override Task Json_collection_index_in_predicate_using_variable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_predicate_using_variable(a);

                AssertSql(
                    """
@prm='1'

SELECT VALUE c["Id"]
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (c["OwnedCollectionRoot"][@prm]["Name"] != "Foo"))
""");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_basic(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_basic(a);

                AssertSql("");
            });

    public override async Task Json_collection_index_in_projection_nested(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_nested(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_collection(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_nested_project_collection(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_collection_anonymous_projection(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_nested_project_collection_anonymous_projection(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_reference(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_nested_project_reference(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_scalar(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_nested_project_scalar(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_project_collection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_project_collection(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_using_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_using_column(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_using_constant_when_owner_is_not_present(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_using_constant_when_owner_is_present(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_using_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_using_parameter(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_using_parameter_when_owner_is_not_present(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_using_parameter_when_owner_is_present(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_in_projection_using_untranslatable_client_method(bool async)
        => base.Json_collection_index_in_projection_using_untranslatable_client_method(async);

    public override Task Json_collection_index_in_projection_using_untranslatable_client_method2(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_index_in_projection_using_untranslatable_client_method2(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_misc1(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_when_owner_is_not_present_misc1(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    [ConditionalTheory(Skip = "issue #34350")]
    public override Task Json_collection_index_in_projection_when_owner_is_not_present_misc2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_when_owner_is_not_present_misc2(a);

                AssertSql("");
            });

    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_multiple(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_when_owner_is_not_present_multiple(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_present_misc1(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_when_owner_is_present_misc1(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    [ConditionalTheory(Skip = "issue #34350")]
    public override Task Json_collection_index_in_projection_when_owner_is_present_misc2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_in_projection_when_owner_is_present_misc2(a);

                AssertSql("");
            });

    public override async Task Json_collection_index_in_projection_when_owner_is_present_multiple(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_in_projection_when_owner_is_present_multiple(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_index_outside_bounds(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_outside_bounds(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34350")]
    public override Task Json_collection_index_outside_bounds2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_index_outside_bounds2(a);

                AssertSql("");
            });

    // returns "wrong" results by design - see #34351 for more context
    public override Task Json_collection_index_outside_bounds_with_property_access(bool async)
        => Task.CompletedTask;

    public override async Task Json_collection_index_with_expression_Select_ElementAt(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_with_expression_Select_ElementAt(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_with_parameter_Select_ElementAt(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_collection_index_with_parameter_Select_ElementAt(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    [ConditionalTheory(Skip = "issue #34004")] // anonymous projection
    public override Task Json_collection_in_projection_with_anonymous_projection_of_scalars(bool async)
        => base.Json_collection_in_projection_with_anonymous_projection_of_scalars(async);

    public override Task Json_collection_in_projection_with_composition_count(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_in_projection_with_composition_count(a);

                AssertSql(
                    """
SELECT VALUE ARRAY_LENGTH(c["OwnedCollectionRoot"])
FROM root c
WHERE (c["Discriminator"] = "Basic")
ORDER BY c["Id"]
""");
            });

    [ConditionalTheory(Skip = "issue #34004")] // anonymous projection
    public override Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(bool async)
        => base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(async);

    [ConditionalTheory(Skip = "issue #34004")] // anonymous projection
    public override Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(bool async)
        => base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(async);

    public override Task Json_collection_leaf_filter_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_leaf_filter_in_projection(a);

                AssertSql(
                    """
SELECT VALUE ARRAY(
    SELECT VALUE o
    FROM o IN c["OwnedReferenceRoot"]["OwnedReferenceBranch"]["OwnedCollectionLeaf"]
    WHERE (o["SomethingSomething"] != "Baz"))
FROM root c
WHERE (c["Discriminator"] = "Basic")
ORDER BY c["Id"]
""");
            });

    public override Task Json_collection_of_primitives_contains_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_of_primitives_contains_in_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND ARRAY_CONTAINS(c["OwnedReferenceRoot"]["Names"], "e1_r1"))
""");
            });

    public override Task Json_collection_of_primitives_index_used_in_orderby(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_of_primitives_index_used_in_orderby(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
ORDER BY c["OwnedReferenceRoot"]["Numbers"][0]
""");
            });

    public override Task Json_collection_of_primitives_index_used_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_of_primitives_index_used_in_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (c["OwnedReferenceRoot"]["Names"][0] = "e1_r1"))
""");
            });

    [ConditionalTheory(Skip = "issue #34026")] //enums property is ignored
    public override Task Json_collection_of_primitives_index_used_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_of_primitives_index_used_in_projection(a);

                AssertSql(
                    """
SELECT VALUE c["OwnedReferenceRoot"]["OwnedReferenceBranch"]["Enums"][0]
FROM root c
WHERE (c["Discriminator"] = "JsonEntityBasic")
ORDER BY c["Id"]
""");
            });

    public override Task Json_collection_of_primitives_SelectMany(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_of_primitives_SelectMany(a);

                AssertSql(
                    """
SELECT VALUE n
FROM root c
JOIN n IN c["OwnedReferenceRoot"]["Names"]
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Json_collection_OrderByDescending_Skip_ElementAt(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_OrderByDescending_Skip_ElementAt(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries + Environment.NewLine + CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    [ConditionalTheory(Skip = "issue #34335")]
    public override Task Json_collection_Select_entity_collection_ElementAt(bool async)
        => base.Json_collection_Select_entity_collection_ElementAt(async);

    public override Task Json_collection_Select_entity_ElementAt(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_Select_entity_ElementAt(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_collection_Select_entity_in_anonymous_object_ElementAt(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_Select_entity_in_anonymous_object_ElementAt(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_collection_Select_entity_with_initializer_ElementAt(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_Select_entity_with_initializer_ElementAt(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_collection_Skip(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_Skip(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (ARRAY(
    SELECT VALUE o["OwnedReferenceLeaf"]["SomethingSomething"]
    FROM o IN (SELECT VALUE ARRAY_SLICE(c["OwnedReferenceRoot"]["OwnedCollectionBranch"], 1)))[0] = "e1_r_c2_r"))
""");
            });

    public override Task Json_collection_skip_take_in_projection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_skip_take_in_projection(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_collection_skip_take_in_projection_project_into_anonymous_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_skip_take_in_projection_project_into_anonymous_type(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_collection_Where_ElementAt(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                // TODO: note the enum value -3
                await base.Json_collection_Where_ElementAt(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (ARRAY(
    SELECT VALUE o["OwnedReferenceLeaf"]["SomethingSomething"]
    FROM o IN c["OwnedReferenceRoot"]["OwnedCollectionBranch"]
    WHERE (o["Enum"] = -3))[0] = "e1_r_c2_r"))
""");
            });

    public override Task Json_collection_within_collection_Count(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_collection_within_collection_Count(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND EXISTS (
    SELECT 1
    FROM o IN c["OwnedCollectionRoot"]
    WHERE (ARRAY_LENGTH(o["OwnedCollectionBranch"]) = 2)))
""");
            });

    public override Task Json_entity_backtracking(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_entity_backtracking(a);

                AssertSql("");
            });

    public override Task Json_entity_with_inheritance_basic_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_entity_with_inheritance_basic_projection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("JsonEntityInheritanceBase", "JsonEntityInheritanceDerived")
""");
            });

    public override Task Json_entity_with_inheritance_project_derived(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_entity_with_inheritance_project_derived(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("JsonEntityInheritanceBase", "JsonEntityInheritanceDerived") AND (c["$type"] = "JsonEntityInheritanceDerived"))
""");
            });

    [ConditionalTheory(Skip = "issue #34350")]
    public override Task Json_entity_with_inheritance_project_navigations(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_entity_with_inheritance_project_navigations(a);

                AssertSql("");
            });

    [ConditionalTheory(Skip = "issue #34350")]
    public override Task Json_entity_with_inheritance_project_navigations_on_derived(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_entity_with_inheritance_project_navigations_on_derived(a);

                AssertSql("");
            });

    public override Task Json_leaf_collection_distinct_and_other_collection(bool async)
        => AssertTranslationFailed(
            () => base.Json_leaf_collection_distinct_and_other_collection(async));

    public override Task Json_multiple_collection_projections(bool async)
        => AssertTranslationFailed(
            () => base.Json_multiple_collection_projections(async));

    public override Task Json_nested_collection_anonymous_projection_in_projection(bool async)
        => AssertTranslationFailed(
            () => base.Json_nested_collection_anonymous_projection_in_projection(async));

    public override Task Json_nested_collection_anonymous_projection_of_primitives_in_projection_NoTrackingWithIdentityResolution(
        bool async)
        => AssertTranslationFailed(
            () => base.Json_nested_collection_anonymous_projection_of_primitives_in_projection_NoTrackingWithIdentityResolution(async));

    public override Task Json_nested_collection_filter_in_projection(bool async)
        => AssertTranslationFailed(
            () => base.Json_nested_collection_filter_in_projection(async));

    public override Task Json_predicate_on_bool_converted_to_int_zero_one(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_bool_converted_to_int_zero_one(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["BoolConvertedToIntZeroOne"] = 1))
""");
            });

    public override Task Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["BoolConvertedToIntZeroOne"] = 0))
""");
            });

    public override Task Json_predicate_on_bool_converted_to_string_True_False(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_bool_converted_to_string_True_False(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["BoolConvertedToStringTrueFalse"] = "True"))
""");
            });

    public override Task Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["BoolConvertedToStringTrueFalse"] = "True"))
""");
            });

    public override Task Json_predicate_on_bool_converted_to_string_Y_N(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_bool_converted_to_string_Y_N(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["BoolConvertedToStringYN"] = "Y"))
""");
            });

    public override Task Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["BoolConvertedToStringYN"] = "N"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_byte(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_byte(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestByte"] != 3))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_byte_array(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_byte_array(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestByteArray"] != "AQID"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_character(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_character(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestCharacter"] != "z"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_dateonly(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_dateonly(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestDateOnly"] != "0003-02-01"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_datetime(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_datetime(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestDateTime"] != "2000-01-03T00:00:00"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_datetimeoffset(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_datetimeoffset(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestDateTimeOffset"] != "2000-01-04T00:00:00+03:02"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_decimal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_decimal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestDecimal"] != 1.35))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_default_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_default_string(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestDefaultString"] != "MyDefaultStringInReference1"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_double(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_double(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestDouble"] != 33.25))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_enum(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_enum(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestEnum"] != 2))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_enumwithintconverter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_enumwithintconverter(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestEnumWithIntConverter"] != -3))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_guid(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_guid(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestGuid"] != "00000000-0000-0000-0000-000000000000"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_int16(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_int16(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestInt16"] != 3))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_int32(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_int32(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestInt32"] != 33))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_int64(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_int64(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestInt64"] != 333))
""");
            });

    public override Task Json_predicate_on_int_zero_one_converted_to_bool(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_int_zero_one_converted_to_bool(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["IntZeroOneConvertedToBool"] = true))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_max_length_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_max_length_string(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestMaxLengthString"] != "Foo"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_nullableenum1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableenum1(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestNullableEnum"] != -1))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_nullableenum2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableenum2(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestNullableEnum"] != null))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_nullableenumwithconverter1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableenumwithconverter1(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestNullableEnumWithIntConverter"] != 2))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_nullableenumwithconverter2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableenumwithconverter2(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestNullableEnumWithIntConverter"] != null))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_nullableenumwithconverterthathandlesnulls1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableenumwithconverterthathandlesnulls1(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestNullableEnumWithConverterThatHandlesNulls"] != "One"))
""");
            });

    public override Task Json_predicate_on_nullableenumwithconverterthathandlesnulls2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableenumwithconverterthathandlesnulls2(a);

                AssertSql("");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_nullableint321(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableint321(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestNullableInt32"] != 100))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_nullableint322(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_nullableint322(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestNullableInt32"] != null))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_signedbyte(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_signedbyte(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestSignedByte"] != 100))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_single(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_single(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestSingle"] != 10.4))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_string_condition(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_string_condition(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND ((NOT(c["Reference"]["TestBoolean"]) ? c["Reference"]["TestMaxLengthString"] : c["Reference"]["TestDefaultString"]) = "MyDefaultStringInReference1"))
""");
            });

    public override Task Json_predicate_on_string_True_False_converted_to_bool(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_string_True_False_converted_to_bool(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["StringTrueFalseConvertedToBool"] = false))
""");
            });

    public override Task Json_predicate_on_string_Y_N_converted_to_bool(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_string_Y_N_converted_to_bool(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Converters") AND (c["Reference"]["StringYNConvertedToBool"] = false))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_timeonly(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_timeonly(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestTimeOnly"] != "03:02:00"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_timespan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_timespan(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestTimeSpan"] != "03:02:00"))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_unisgnedint16(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_unisgnedint16(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestUnsignedInt16"] != 100))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_unsignedint32(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_unsignedint32(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestUnsignedInt32"] != 1000))
""");
            });

    [SkipOnCiCondition]
    public override Task Json_predicate_on_unsignedint64(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_predicate_on_unsignedint64(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "AllTypes") AND (c["Reference"]["TestUnsignedInt64"] != 10000))
""");
            });

    public override Task Json_projection_collection_element_and_reference_AsNoTrackingWithIdentityResolution(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_projection_collection_element_and_reference_AsNoTrackingWithIdentityResolution(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_projection_deduplication_with_collection_indexer_in_original(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_projection_deduplication_with_collection_indexer_in_original(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_projection_deduplication_with_collection_indexer_in_target(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_projection_deduplication_with_collection_indexer_in_target(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base.Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override Task Json_projection_enum_with_custom_conversion(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_projection_enum_with_custom_conversion(a);

                AssertSql(
                    """
SELECT c["Id"], c["OwnedReferenceRoot"]["Enum"]
FROM root c
WHERE (c["Discriminator"] = "CustomNaming")
""");
            });

    public override Task Json_projection_nested_collection_and_element_correct_order_AsNoTrackingWithIdentityResolution(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_projection_nested_collection_and_element_correct_order_AsNoTrackingWithIdentityResolution(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task
        Json_projection_nested_collection_element_using_parameter_and_the_owner_in_correct_order_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base
                .Json_projection_nested_collection_element_using_parameter_and_the_owner_in_correct_order_AsNoTrackingWithIdentityResolution(
                    async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override Task Json_projection_nothing_interesting_AsNoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_projection_nothing_interesting_AsNoTrackingWithIdentityResolution(a);

                AssertSql(
                    """
SELECT c["Id"], c["Name"]
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override async Task
        Json_projection_only_second_element_through_collection_element_constant_projected_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base
                .Json_projection_only_second_element_through_collection_element_constant_projected_nested_AsNoTrackingWithIdentityResolution(
                    async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task
        Json_projection_only_second_element_through_collection_element_parameter_projected_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base
                .Json_projection_only_second_element_through_collection_element_parameter_projected_nested_AsNoTrackingWithIdentityResolution(
                    async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override Task Json_projection_owner_entity_AsNoTrackingWithIdentityResolution(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_projection_owner_entity_AsNoTrackingWithIdentityResolution(a);

                AssertSql(
                    """
SELECT c["Id"], c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Json_projection_reference_collection_and_collection_element_nested_AsNoTrackingWithIdentityResolution(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_projection_reference_collection_and_collection_element_nested_AsNoTrackingWithIdentityResolution(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_projection_second_element_projected_before_owner_as_well_as_root_AsNoTrackingWithIdentityResolution(
        bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_projection_second_element_projected_before_owner_as_well_as_root_AsNoTrackingWithIdentityResolution(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    [ConditionalTheory(Skip = "issue #34350")]
    public override Task Json_projection_second_element_projected_before_owner_nested_as_well_as_root_AsNoTrackingWithIdentityResolution(
        bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_projection_second_element_projected_before_owner_nested_as_well_as_root_AsNoTrackingWithIdentityResolution(
                    a);

                AssertSql("");
            });

    public override async Task
        Json_projection_second_element_through_collection_element_constant_different_values_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(
            () => base
                .Json_projection_second_element_through_collection_element_constant_different_values_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
                    async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override Task
        Json_projection_second_element_through_collection_element_constant_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
        => AssertTranslationFailedWithDetails(
            () => base
                .Json_projection_second_element_through_collection_element_constant_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
                    async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task
        Json_projection_second_element_through_collection_element_parameter_correctly_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
        => AssertTranslationFailedWithDetails(
            () => base
                .Json_projection_second_element_through_collection_element_parameter_correctly_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
                    async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_property_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_property_in_predicate(a);

                AssertSql(
                    """
SELECT VALUE c["Id"]
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (c["OwnedReferenceRoot"]["OwnedReferenceBranch"]["Fraction"] < 20.5))
""");
            });

    public override Task Json_scalar_length(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_scalar_length(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (LENGTH(c["OwnedReferenceRoot"]["Name"]) > 2))
""");
            });

    public override Task Json_scalar_optional_null_semantics(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_scalar_optional_null_semantics(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (c["OwnedReferenceRoot"]["Name"] != c["OwnedReferenceRoot"]["OwnedReferenceBranch"]["OwnedReferenceLeaf"]["SomethingSomething"]))
""");
            });

    public override Task Json_scalar_required_null_semantics(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_scalar_required_null_semantics(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE ((c["Discriminator"] = "Basic") AND (c["OwnedReferenceRoot"]["Number"] != LENGTH(c["OwnedReferenceRoot"]["Name"])))
""");
            });

    public override Task Json_subquery_property_pushdown_length(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_subquery_property_pushdown_length(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_subquery_reference_pushdown_property(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_subquery_reference_pushdown_property(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_subquery_reference_pushdown_reference(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_subquery_reference_pushdown_reference(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_subquery_reference_pushdown_reference_anonymous_projection(bool async)
        => base.Json_subquery_reference_pushdown_reference_anonymous_projection(async);

    public override Task Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(bool async)
        => base.Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(async);

    public override Task Json_subquery_reference_pushdown_reference_pushdown_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_subquery_reference_pushdown_reference_pushdown_collection(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Json_subquery_reference_pushdown_reference_pushdown_reference(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_subquery_reference_pushdown_reference_pushdown_reference(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Json_with_include_on_entity_collection(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Json_with_include_on_entity_collection(async))).Message;

        Assert.Equal(
            CosmosStrings.NonEmbeddedIncludeNotSupported(
                "Navigation: JsonEntityBasic.EntityCollection (List<JsonEntityBasicForCollection>) Collection ToDependent JsonEntityBasicForCollection Inverse: Parent"),
            message);
    }

    public override Task Json_with_include_on_entity_collection_and_reference(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_with_include_on_entity_collection_and_reference(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntityBasicForReference), nameof(JsonEntityBasic)));

    public override Task Json_with_include_on_entity_reference(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_with_include_on_entity_reference(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntityBasicForReference), nameof(JsonEntityBasic)));

    public override Task Json_with_include_on_json_entity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Json_with_include_on_json_entity(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Basic")
""");
            });

    public override Task Json_with_projection_of_json_collection_and_entity_collection(bool async)
        => AssertTranslationFailed(
            () => base.Json_with_projection_of_json_collection_and_entity_collection(async));

    public override Task Json_with_projection_of_json_collection_element_and_entity_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_with_projection_of_json_collection_element_and_entity_collection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntityBasicForReference), nameof(JsonEntityBasic)));

    public override Task Json_with_projection_of_json_collection_leaf_and_entity_collection(bool async)
        => AssertTranslationFailed(
            () => base.Json_with_projection_of_json_collection_leaf_and_entity_collection(async));

    public override Task Json_with_projection_of_json_reference_and_entity_collection(bool async)
        => AssertTranslationFailed(
            () => base.Json_with_projection_of_json_reference_and_entity_collection(async));

    public override Task Json_with_projection_of_json_reference_leaf_and_entity_collection(bool async)
        => AssertTranslationFailed(
            () => base.Json_with_projection_of_json_reference_leaf_and_entity_collection(async));

    public override Task Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntityBasicForReference), nameof(JsonEntityBasic)));

    public override Task Json_with_projection_of_multiple_json_references_and_entity_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_with_projection_of_multiple_json_references_and_entity_collection(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task LeftJoin_json_entities(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.LeftJoin_json_entities(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntityBasic), nameof(JsonEntitySingleOwned)));

    public override Task RightJoin_json_entities(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.RightJoin_json_entities(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntitySingleOwned), nameof(JsonEntityBasic)));

    public override Task Left_join_json_entities_complex_projection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Left_join_json_entities_complex_projection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntityBasic), nameof(JsonEntitySingleOwned)));

    public override Task Left_join_json_entities_complex_projection_json_being_inner(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Left_join_json_entities_complex_projection_json_being_inner(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntitySingleOwned), nameof(JsonEntityBasic)));

    public override Task Left_join_json_entities_json_being_inner(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Left_join_json_entities_json_being_inner(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntitySingleOwned), nameof(JsonEntityBasic)));

    public override Task Project_entity_with_single_owned(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_entity_with_single_owned(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "SingleOwned")
""");
            });

    public override Task Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(bool async)
        => AssertTranslationFailed(
            () => base.Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(async));

    public override Task Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(bool async)
        => AssertTranslationFailed(
            () => base.Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(async));

    public override async Task Project_json_reference_in_tracking_query_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_json_reference_in_tracking_query_fails(async))).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    public override async Task Project_json_collection_in_tracking_query_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_json_collection_in_tracking_query_fails(async))).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    [ConditionalTheory(Skip = "issue #34350")]
    public override async Task Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(async))).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
