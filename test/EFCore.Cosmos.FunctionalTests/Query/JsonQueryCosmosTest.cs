// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable EF8001 // Owned JSON entities are obsolete

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
            await Assert.ThrowsAsync<NullReferenceException>(()
                => base.Basic_json_projection_owned_collection_branch_NoTrackingWithIdentityResolution(async));
        }
    }

    public override async Task Basic_json_projection_owned_collection_leaf(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(() => base.Basic_json_projection_owned_collection_leaf(async));
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
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Entity_including_collection_with_json(async)))
            .Message;

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
        => AssertTranslationFailed(() => base.Json_branch_collection_distinct_and_other_collection(async));

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
        => AssertTranslationFailed(() => base.Json_collection_Distinct_Count_with_predicate(async));

    public override Task Json_collection_distinct_in_projection(bool async)
        => AssertTranslationFailed(() => base.Json_collection_distinct_in_projection(async));

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
        => AssertTranslationFailed(() => base.Json_collection_filter_in_projection(async));

    public override async Task Json_collection_index_in_predicate_nested_mix(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(() => base.Json_collection_index_in_predicate_nested_mix(async)))
            .Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_predicate_using_column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = (await Assert.ThrowsAsync<CosmosException>(() => base.Json_collection_index_in_predicate_using_column(async)));
        }
    }

    public override async Task Json_collection_index_in_predicate_using_complex_expression1(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception =
                (await Assert.ThrowsAsync<CosmosException>(() => base.Json_collection_index_in_predicate_using_complex_expression1(async)));
        }
    }

    public override Task Json_collection_index_in_predicate_using_complex_expression2(bool async)
        => AssertTranslationFailed(() => base.Json_collection_index_in_predicate_using_complex_expression2(async));

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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(() => base.Json_collection_index_in_projection_nested(async)))
            .Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_collection(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_nested_project_collection(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_collection_anonymous_projection(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_nested_project_collection_anonymous_projection(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_reference(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_nested_project_reference(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_nested_project_scalar(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_nested_project_scalar(async))).Message;

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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_when_owner_is_not_present_misc1(async))).Message;

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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_when_owner_is_not_present_multiple(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_present_misc1(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_when_owner_is_present_misc1(async))).Message;

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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_in_projection_when_owner_is_present_multiple(async))).Message;

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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_with_expression_Select_ElementAt(async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task Json_collection_index_with_parameter_Select_ElementAt(bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_collection_index_with_parameter_Select_ElementAt(async))).Message;

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
        => AssertTranslationFailed(() => base.Json_leaf_collection_distinct_and_other_collection(async));

    public override Task Json_multiple_collection_projections(bool async)
        => AssertTranslationFailed(() => base.Json_multiple_collection_projections(async));

    public override Task Json_nested_collection_anonymous_projection_in_projection(bool async)
        => AssertTranslationFailed(() => base.Json_nested_collection_anonymous_projection_in_projection(async));

    public override Task Json_nested_collection_anonymous_projection_of_primitives_in_projection_NoTrackingWithIdentityResolution(
        bool async)
        => AssertTranslationFailed(()
            => base.Json_nested_collection_anonymous_projection_of_primitives_in_projection_NoTrackingWithIdentityResolution(async));

    public override Task Json_nested_collection_filter_in_projection(bool async)
        => AssertTranslationFailed(() => base.Json_nested_collection_filter_in_projection(async));

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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(()
            => base.Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(async))).Message;

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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(() => base
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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(() => base
            .Json_projection_only_second_element_through_collection_element_constant_projected_nested_AsNoTrackingWithIdentityResolution(
                async))).Message;

        // issue #34348
        Assert.Equal(NotImplementedBindPropertyMessage, message);
    }

    public override async Task
        Json_projection_only_second_element_through_collection_element_parameter_projected_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var message = (await Assert.ThrowsAsync<NotImplementedException>(() => base
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
        var message = (await Assert.ThrowsAsync<NotImplementedException>(() => base
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
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_with_include_on_entity_collection(async)))
            .Message;

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
        => AssertTranslationFailed(() => base.Json_with_projection_of_json_collection_and_entity_collection(async));

    public override Task Json_with_projection_of_json_collection_element_and_entity_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Json_with_projection_of_json_collection_element_and_entity_collection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(JsonEntityBasicForReference), nameof(JsonEntityBasic)));

    public override Task Json_with_projection_of_json_collection_leaf_and_entity_collection(bool async)
        => AssertTranslationFailed(() => base.Json_with_projection_of_json_collection_leaf_and_entity_collection(async));

    public override Task Json_with_projection_of_json_reference_and_entity_collection(bool async)
        => AssertTranslationFailed(() => base.Json_with_projection_of_json_reference_and_entity_collection(async));

    public override Task Json_with_projection_of_json_reference_leaf_and_entity_collection(bool async)
        => AssertTranslationFailed(() => base.Json_with_projection_of_json_reference_leaf_and_entity_collection(async));

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
        => AssertTranslationFailed(() => base.Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(async));

    public override Task Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(bool async)
        => AssertTranslationFailed(() => base.Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(async));

    public override async Task Project_json_reference_in_tracking_query_fails(bool async)
    {
        var message =
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_json_reference_in_tracking_query_fails(async))).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    public override async Task Project_json_collection_in_tracking_query_fails(bool async)
    {
        var message =
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_json_collection_in_tracking_query_fails(async)))
            .Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    [ConditionalTheory(Skip = "issue #34350")]
    public override async Task Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(async))).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }


    #region 21006

    public override async Task Project_root_with_missing_scalars(bool async)
    {
        if (async)
        {
            await base.Project_root_with_missing_scalars(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["Id"] < 4)
""");
        }
    }

    [ConditionalTheory(Skip = "issue #35702")]
    public override async Task Project_top_level_json_entity_with_missing_scalars(bool async)
    {
        if (async)
        {
            await base.Project_top_level_json_entity_with_missing_scalars(async);

            AssertSql();
        }
    }

    public override async Task Project_nested_json_entity_with_missing_scalars(bool async)
    {
        if (async)
        {
            await AssertTranslationFailed(() => base.Project_nested_json_entity_with_missing_scalars(async));

            AssertSql();
        }
    }

    [ConditionalTheory(Skip = "issue #34067")]
    public override async Task Project_top_level_entity_with_null_value_required_scalars(bool async)
    {
        if (async)
        {
            await base.Project_top_level_entity_with_null_value_required_scalars(async);

            AssertSql(
                """
SELECT c["Id"], c
FROM root c
WHERE (c["Id"] = 4)
""");
        }
    }

    public override async Task Project_root_entity_with_missing_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_root_entity_with_missing_required_navigation(async);

            AssertSql(
                """
ReadItem(?, ?)
""");
        }
    }

    public override async Task Project_missing_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_missing_required_navigation(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["Id"] = 5)
""");
        }
    }

    public override async Task Project_root_entity_with_null_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_root_entity_with_null_required_navigation(async);

            AssertSql(
                """
ReadItem(?, ?)
""");
        }
    }

    public override async Task Project_null_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_null_required_navigation(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["Id"] = 6)
""");
        }
    }

    public override async Task Project_missing_required_scalar(bool async)
    {
        if (async)
        {
            await base.Project_missing_required_scalar(async);

            AssertSql(
                """
SELECT c["Id"], c["RequiredReference"]["Number"]
FROM root c
WHERE (c["Id"] = 2)
""");
        }
    }

    public override async Task Project_null_required_scalar(bool async)
    {
        if (async)
        {
            await base.Project_null_required_scalar(async);

            AssertSql(
                """
SELECT c["Id"], c["RequiredReference"]["Number"]
FROM root c
WHERE (c["Id"] = 4)
""");
        }
    }

    protected override void OnModelCreating21006(ModelBuilder modelBuilder)
    {
        base.OnModelCreating21006(modelBuilder);

        modelBuilder.Entity<Context21006.Entity>().ToContainer("Entities");
    }

    protected override async Task Seed21006(Context21006 context)
    {
        await base.Seed21006(context);

        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var missingTopLevel =
            """
{
    "Id": 2,
    "$type": "Entity",
    "Name": "e2",
    "id": "2",
    "Collection":
    [
        {
            "Text": "e2 c1",
            "NestedCollection": [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c1 c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c1 c2"
            }
            ],
            "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c1 nor"
            },
            "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c1 nrr"
            }
        },
        {
            "Text": "e2 c2",
            "NestedCollection": [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c2 c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c2 c2"
            }
            ],
            "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c2 nor"
            },
            "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c2 nrr"
            }
        }
    ],
    "OptionalReference": {
        "Text": "e2 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 or nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 or nrr"
        }
    },
    "RequiredReference": {
        "Text": "e2 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 rr nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 rr nrr"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingTopLevel,
            CancellationToken.None);

        var missingNested =
            """
{
    "Id": 3,
    "$type": "Entity",
    "Name": "e3",
    "id": "3",
    "Collection":
    [
        {
            "Number": 7.0,
            "Text": "e3 c1",
            "NestedCollection":
            [
                {
                  "Text": "e3 c1 c1"
                },
                {
                  "Text": "e3 c1 c2"
                }
            ],
            "NestedOptionalReference": {
            "Text": "e3 c1 nor"
            },
            "NestedRequiredReference": {
            "Text": "e3 c1 nrr"
            }
        },
        {
            "Number": 7.0,
            "Text": "e3 c2",
            "NestedCollection":
            [
                {
                  "Text": "e3 c2 c1"
                },
                {
                  "Text": "e3 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "Text": "e3 c2 nor"
            },
            "NestedRequiredReference": {
                "Text": "e3 c2 nrr"
            }
        }
    ],
    "OptionalReference": {
        "Number": 7.0,
        "Text": "e3 or",
        "NestedCollection":
        [
            {
                "Text": "e3 or c1"
            },
            {
                "Text": "e3 or c2"
            }
        ],
        "NestedOptionalReference": {
            "Text": "e3 or nor"
        },
        "NestedRequiredReference": {
            "Text": "e3 or nrr"
        }
    },
    "RequiredReference": {
        "Number": 7.0,
        "Text": "e3 rr",
        "NestedCollection":
        [
            {
                "Text": "e3 rr c1"
            },
            {
                "Text": "e3 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "Text": "e3 rr nor"
        },
        "NestedRequiredReference": {
            "Text": "e3 rr nrr"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingNested,
            CancellationToken.None);

        var nullTopLevel =
            """
{
    "Id": 4,
    "$type": "Entity",
    "Name": "e4",
    "id": "4",
    "Collection":
    [
        {
            "Number": null,
            "Text": "e4 c1",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c1 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c1 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c1 nor"
            },
            "NestedRequiredReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c1 nrr"
            }
        },
        {
            "Number": null,
            "Text": "e4 c2",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c2 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c2 nor"
            },
            "NestedRequiredReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c2 nrr"
            }
        }
    ],
    "OptionalReference": {
        "Number": null,
        "Text": "e4 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 or nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 or nrr"
        }
    },
    "RequiredReference": {
        "Number": null,
        "Text": "e4 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 rr nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 rr nrr"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullTopLevel,
            CancellationToken.None);

        var missingRequiredNav =
            """
{
    "Id": 5,
    "$type": "Entity",
    "Name": "e5",
    "id": "5",
    "Collection":
    [
        {
            "Number": 7.0,
            "Text": "e5 c1",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c1 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c1 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 c1 nor"
            },
        },
        {
            "Number": 7.0,
            "Text": "e5 c2",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c2 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 c2 nor"
            },
        }
    ],
    "OptionalReference": {
        "Number": 7.0,
        "Text": "e5 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e5 or nor"
        },
    },
    "RequiredReference": {
        "Number": 7.0,
        "Text": "e5 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e5 rr nor"
        },
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingRequiredNav,
            CancellationToken.None);

        var nullRequiredNav =
            """
{
    "Id": 6,
    "$type": "Entity",
    "Name": "e6",
    "id": "6",
    "Collection":
    [
        {
            "Number": 7.0,
            "Text": "e6 c1",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c1 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c1 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 c1 nor"
            },
            "NestedRequiredReference": null
        },
        {
            "Number": 7.0,
            "Text": "e6 c2",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c2 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 c2 nor"
            },
            "NestedRequiredReference": null
        }
    ],
    "OptionalReference": {
        "Number": 7.0,
        "Text": "e6 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e6 or nor"
        },
        "NestedRequiredReference": null
    },
    "RequiredReference": {
        "Number": 7.0,
        "Text": "e6 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e6 rr nor"
        },
        "NestedRequiredReference": null
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullRequiredNav,
            CancellationToken.None);
    }

    #endregion

    #region 34960

    public override async Task Try_project_collection_but_JSON_is_entity()
    {
        var message = (await Assert.ThrowsAsync<JsonSerializationException>(() => base.Try_project_collection_but_JSON_is_entity()))
            .Message;

        Assert.Equal(
            $"Deserialized JSON type '{typeof(JObject).FullName}' is not compatible with expected type '{typeof(JArray).FullName}'. Path 'Collection'.",
            message);
    }

    public override async Task Try_project_reference_but_JSON_is_collection()
    {
        var message = (await Assert.ThrowsAsync<JsonSerializationException>(() => base.Try_project_reference_but_JSON_is_collection()))
            .Message;

        Assert.Equal(
            $"Deserialized JSON type '{typeof(JArray).FullName}' is not compatible with expected type '{typeof(JObject).FullName}'. Path 'Reference'.",
            message);
    }

    protected override void OnModelCreating34960(ModelBuilder modelBuilder)
    {
        base.OnModelCreating34960(modelBuilder);
        modelBuilder.Entity<Context34960.Entity>().ToContainer("Entities");
        modelBuilder.Entity<Context34960.JunkEntity>().ToContainer("Junk");
    }

    protected override async Task Seed34960(Context34960 context)
    {
        await base.Seed34960(context);

        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
            """
{
    "Id": 4,
    "$type": "Entity",
    "id": "4",
    "Collection": null,
    "Reference": null
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);

        var junkContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Junk");

        var objectWhereCollectionShouldBe =
            """
{
    "Id": 1,
    "$type": "JunkEntity",
    "id": "1",
    "Collection": { "DoB":"2000-01-01T00:00:00","Text":"junk" },
    "Reference": null
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            junkContainer,
            objectWhereCollectionShouldBe,
            CancellationToken.None);

        var collectionWhereEntityShouldBe =
            """
{
    "Id": 2,
    "$type": "JunkEntity",
    "id": "2",
    "Collection": null,
    "Reference": [{ "DoB":"2000-01-01T00:00:00","Text":"junk" }]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            junkContainer,
            collectionWhereEntityShouldBe,
            CancellationToken.None);
    }

    #endregion

    #region 33046

    protected override void OnModelCreating33046(ModelBuilder modelBuilder)
    {
        base.OnModelCreating33046(modelBuilder);

        modelBuilder.Entity<Context33046.Review>().ToContainer("Reviews");
    }

    protected override async Task Seed33046(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Reviews");

        var json =
            """
{
    "Id": 1,
    "$type": "Review",
    "id": "1",
    "Rounds":
    [
        {
            "RoundNumber":11,
            "SubRounds":
            [
                {
                    "SubRoundNumber":111
                },
                {
                    "SubRoundNumber":112
                }
            ]
        }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region ArrayOfPrimitives

    protected override void OnModelCreatingArrayOfPrimitives(ModelBuilder modelBuilder)
        => base.OnModelCreatingArrayOfPrimitives(modelBuilder);

    #endregion

    #region NotICollection

    protected override void OnModelCreatingNotICollection(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingNotICollection(modelBuilder);

        modelBuilder.Entity<ContextNotICollection.MyEntity>().ToContainer("Entities");
    }

    protected override async Task SeedNotICollection(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json1 =
            """
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Json":
    {
        "Collection":
        [
            {
                "Bar":11,"Foo":"c11"
            },
            {
                "Bar":12,"Foo":"c12"
            },
            {
                "Bar":13,"Foo":"c13"
            }
        ]
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json1,
            CancellationToken.None);

        var json2 =
            """
{
    "Id": 2,
    "$type": "MyEntity",
    "id": "2",
    "Json": {
        "Collection":
        [
            {
                "Bar":21,"Foo":"c21"
            },
            {
                "Bar":22,"Foo":"c22"
            }
        ]
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json2,
            CancellationToken.None);
    }

    #endregion

    #region 30028

    [ConditionalTheory(Skip = "issue #35702")]
    public override Task Missing_navigation_works_with_deduplication(bool async)
        => base.Missing_navigation_works_with_deduplication(async);

    // missing array comes out as empty on Cosmos
    public override Task Accessing_missing_navigation_works()
        => Task.CompletedTask;

    protected override void OnModelCreating30028(ModelBuilder modelBuilder)
    {
        base.OnModelCreating30028(modelBuilder);

        modelBuilder.Entity<Context30028.MyEntity>().ToContainer("Entities");
    }

    protected override async Task Seed30028(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var complete =
            """
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Json": {
        "RootName":"e1",
        "Collection":
        [
            {
                "BranchName":"e1 c1",
                "Nested":{
                    "LeafName":"e1 c1 l"
                }
            },
            {
                "BranchName":"e1 c2",
                "Nested":{
                    "LeafName":"e1 c2 l"
                }
            }
        ],
        "OptionalReference":{
            "BranchName":"e1 or",
            "Nested":{
                "LeafName":"e1 or l"
            }
        },
        "RequiredReference":{
            "BranchName":"e1 rr",
            "Nested":{
                "LeafName":"e1 rr l"
            }
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            complete,
            CancellationToken.None);

        var missingCollection =
            """
{
    "Id": 2,
    "$type": "MyEntity",
    "id": "2",
    "Json": {
        "RootName":"e2",
        "OptionalReference":{
            "BranchName":"e2 or",
            "Nested":{
                "LeafName":"e2 or l"
            }
        },
        "RequiredReference":{
            "BranchName":"e2 rr",
            "Nested":{
                "LeafName":"e2 rr l"
            }
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingCollection,
            CancellationToken.None);

        var missingOptionalReference =
            """
{
    "Id": 3,
    "$type": "MyEntity",
    "id": "3",
    "Json": {
        "RootName":"e3",
        "Collection":
        [
            {
                "BranchName":"e3 c1",
                "Nested":{
                    "LeafName":"e3 c1 l"
                }
            },
            {
                "BranchName":"e3 c2",
                "Nested":{
                    "LeafName":"e3 c2 l"
                }
            }
        ],
        "RequiredReference":{
            "BranchName":"e3 rr",
            "Nested":{
                "LeafName":"e3 rr l"
            }
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingOptionalReference,
            CancellationToken.None);

        var missingRequiredReference =
            """
{
    "Id": 4,
    "$type": "MyEntity",
    "id": "4",
    "Json": {
        "RootName":"e4",
        "Collection":
        [
            {
                "BranchName":"e4 c1",
                "Nested":{
                    "LeafName":"e4 c1 l"
                }
            },
            {
                "BranchName":"e4 c2",
                "Nested":{
                    "LeafName":"e4 c2 l"
                }
            }
        ],
        "OptionalReference":{
            "BranchName":"e4 or",
            "Nested":{
                "LeafName":"e4 or l"
            }
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingRequiredReference,
            CancellationToken.None);
    }

    #endregion

    #region 29219

    // Cosmos returns unexpected number of results (i.e. not returning row with non-existent NullableScalar
    // this is by design behavior in Cosmos, so we just skip the test to avoid validation error
    public override Task Can_project_nullable_json_property_when_the_element_in_json_is_not_present()
        => Task.CompletedTask;

    protected override void OnModelCreating29219(ModelBuilder modelBuilder)
    {
        base.OnModelCreating29219(modelBuilder);

        modelBuilder.Entity<Context29219.MyEntity>().ToContainer("Entities");
    }

    protected override async Task Seed29219(DbContext context)
    {
        await base.Seed29219(context);

        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var missingNullableScalars =
            """
{
    "Id": 3,
    "$type": "MyEntity",
    "id": "3",
    "Collection":
    [
        {
            "NonNullableScalar" : 10001
        }
    ],
    "Reference": {
        "NonNullableScalar" : 30
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingNullableScalars,
            CancellationToken.None);
    }

    #endregion

    #region LazyLoadingProxies

    protected override void OnModelCreatingLazyLoadingProxies(ModelBuilder modelBuilder)
        => base.OnModelCreatingLazyLoadingProxies(modelBuilder);

    #endregion

    #region ShadowProperties

    protected override void OnModelCreatingShadowProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingShadowProperties(modelBuilder);

        modelBuilder.Entity<ContextShadowProperties.MyEntity>(b =>
        {
            b.ToContainer("Entities");

            //b.OwnsOne(x => x.Reference, b =>
            //{
            //    //      b.ToJson().HasColumnType(JsonColumnType);
            //    b.Property<string>("ShadowString");
            //});

            b.OwnsOne(
                x => x.ReferenceWithCtor, b =>
                {
                    //    b.ToJson().HasColumnType(JsonColumnType);
                    b.Property<int>("Shadow_Int").ToJsonProperty("ShadowInt");
                });

            //b.OwnsMany(
            //    x => x.Collection, b =>
            //    {
            //        //  b.ToJson().HasColumnType(JsonColumnType);
            //        b.Property<double>("ShadowDouble");
            //    });

            //b.OwnsMany(
            //    x => x.CollectionWithCtor, b =>
            //    {
            //        //b.ToJson().HasColumnType(JsonColumnType);
            //        b.Property<byte?>("ShadowNullableByte");
            //    });
        });
    }

    protected override async Task SeedShadowProperties(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
            """
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Name": "e1",
    "Collection":
    [
        {
            "Name":"e1_c1","ShadowDouble":5.5
        },
        {
            "ShadowDouble":20.5,"Name":"e1_c2"
        }
    ],
    "CollectionWithCtor":
    [
        {
            "Name":"e1_c1 ctor","ShadowNullableByte":6
        },
        {
            "ShadowNullableByte":null,"Name":"e1_c2 ctor"
        }
    ],
    "Reference": { "Name":"e1_r", "ShadowString":"Foo" },
    "ReferenceWithCtor": { "ShadowInt":143,"Name":"e1_r ctor" }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region JunkInJson

    protected override void OnModelCreatingJunkInJson(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingJunkInJson(modelBuilder);

        modelBuilder.Entity<ContextJunkInJson.MyEntity>().ToContainer("Entities");
    }

    protected override async Task SeedJunkInJson(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
            """
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Collection":
    [
        {
            "JunkReference": {
                "Something":"SomeValue"
            },
            "Name":"c11",
            "JunkProperty1":50,
            "Number":11.5,
            "JunkCollection1":[],
            "JunkCollection2":
            [
                {
                    "Foo":"junk value"
                }
            ],
            "NestedCollection":
            [
                {
                    "DoB":"2002-04-01T00:00:00",
                    "DummyProp":"Dummy value"
                },
                {
                    "DoB":"2002-04-02T00:00:00",
                    "DummyReference":{
                        "Foo":5
                    }
                }
            ],
            "NestedReference":{
                "DoB":"2002-03-01T00:00:00"
            }
        },
        {
            "Name":"c12",
            "Number":12.5,
            "NestedCollection":
            [
                {
                    "DoB":"2002-06-01T00:00:00"
                },
                {
                    "DoB":"2002-06-02T00:00:00"
                }
            ],
            "NestedDummy":59,
            "NestedReference":{
                "DoB":"2002-05-01T00:00:00"
            }
        }
    ],
    "CollectionWithCtor":
    [
        {
            "MyBool":true,
            "Name":"c11 ctor",
            "JunkReference":{
                "Something":"SomeValue",
                "JunkCollection":
                [
                    {
                        "Foo":"junk value"
                    }
                ]
            },
            "NestedCollection":
            [
                {
                    "DoB":"2002-08-01T00:00:00"
                },
                {
                    "DoB":"2002-08-02T00:00:00"
                }
            ],
            "NestedReference":{
                "DoB":"2002-07-01T00:00:00"
            }
        },
        {
            "MyBool":false,
            "Name":"c12 ctor",
            "NestedCollection":
            [
                {
                    "DoB":"2002-10-01T00:00:00"
                },
                {
                    "DoB":"2002-10-02T00:00:00"
                }
            ],
            "JunkCollection":
            [
                {
                    "Foo":"junk value"
                }
            ],
            "NestedReference":{
                "DoB":"2002-09-01T00:00:00"
            }
        }
    ],
    "Reference": {
        "Name":"r1",
        "JunkCollection":
        [
            {
                "Foo":"junk value"
            }
        ],
        "JunkReference":{
            "Something":"SomeValue"
        },
        "Number":1.5,
        "NestedCollection":
        [
            {
                "DoB":"2000-02-01T00:00:00",
                "JunkReference":{
                    "Something":"SomeValue"
                }
            },
            {
                "DoB":"2000-02-02T00:00:00"
            }
        ],
        "NestedReference":{
            "DoB":"2000-01-01T00:00:00"
        }
    },
    "ReferenceWithCtor":{
        "MyBool":true,
        "JunkCollection":
        [
            {
                "Foo":"junk value"
            }
        ],
        "Name":"r1 ctor",
        "JunkReference":{
            "Something":"SomeValue"
        },
        "NestedCollection":
        [
            {
                "DoB":"2001-02-01T00:00:00"
            },
            {
                "DoB":"2001-02-02T00:00:00"
            }
        ],
        "NestedReference":{
            "JunkCollection":
            [
                {
                    "Foo":"junk value"
                }
            ],
            "DoB":"2001-01-01T00:00:00"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region TrickyBuffering

    protected override void OnModelCreatingTrickyBuffering(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingTrickyBuffering(modelBuilder);

        modelBuilder.Entity<ContextTrickyBuffering.MyEntity>().ToContainer("Entities");
    }

    protected override async Task SeedTrickyBuffering(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
            """
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Reference": {
        "Name": "r1",
        "Number": 7,
        "JunkReference": {
            "Something": "SomeValue"
        },
        "JunkCollection":
        [
            {
                "Foo": "junk value"
            }
        ],
        "NestedReference": {
            "DoB": "2000-01-01T00:00:00"
        },
        "NestedCollection":
        [
            {
                "DoB": "2000-02-01T00:00:00",
                "JunkReference": {
                    "Something": "SomeValue"
                }
            },
            {
                "DoB": "2000-02-02T00:00:00"
            }
        ]
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region BadJsonProperties

    // missing collection comes back as empty on Cosmos
    public override Task Bad_json_properties_empty_navigations(bool noTracking)
        => Task.CompletedTask;

    protected override void OnModelCreatingBadJsonProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingBadJsonProperties(modelBuilder);

        modelBuilder.Entity<ContextBadJsonProperties.Entity>().ToContainer("Entities");
    }

    protected override async Task SeedBadJsonProperties(ContextBadJsonProperties context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var baseline =
            """
{
    "Id": 1,
    "$type": "Entity",
    "id": "1",
    "Scenario": "baseline",
    "OptionalReference": {"NestedOptional": { "Text":"or no" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] },
    "RequiredReference": {"NestedOptional": { "Text":"rr no" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] },
    "Collection":
    [
        {"NestedOptional": { "Text":"c 1 no" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
        {"NestedOptional": { "Text":"c 2 no" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            baseline,
            CancellationToken.None);

        var duplicatedNavigations =
            """
{
    "Id": 2,
    "$type": "Entity",
    "id": "2",
    "Scenario": "duplicated navigations",
    "OptionalReference": {"NestedOptional": { "Text":"or no" }, "NestedOptional": { "Text":"or no dupnav" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ], "NestedCollection": [ { "Text":"or nc 1 dupnav" }, { "Text":"or nc 2 dupnav" } ], "NestedRequired": { "Text":"or nr dupnav" } },
    "RequiredReference": {"NestedOptional": { "Text":"rr no" }, "NestedOptional": { "Text":"rr no dupnav" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ], "NestedCollection": [ { "Text":"rr nc 1 dupnav" }, { "Text":"rr nc 2 dupnav" } ], "NestedRequired": { "Text":"rr nr dupnav" } },
    "Collection":
    [
        {"NestedOptional": { "Text":"c 1 no" }, "NestedOptional": { "Text":"c 1 no dupnav" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ], "NestedCollection": [ { "Text":"c 1 nc 1 dupnav" }, { "Text":"c 1 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 1 nr dupnav" } },
        {"NestedOptional": { "Text":"c 2 no" }, "NestedOptional": { "Text":"c 2 no dupnav" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ], "NestedCollection": [ { "Text":"c 2 nc 1 dupnav" }, { "Text":"c 2 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 2 nr dupnav" } }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            duplicatedNavigations,
            CancellationToken.None);

        var duplicatedScalars =
            """
{
    "Id": 3,
    "$type": "Entity",
    "id": "3",
    "Scenario": "duplicated scalars",
    "OptionalReference": {"NestedOptional": { "Text":"or no", "Text":"or no dupprop" }, "NestedRequired": { "Text":"or nr", "Text":"or nr dupprop" }, "NestedCollection": [ { "Text":"or nc 1", "Text":"or nc 1 dupprop" }, { "Text":"or nc 2", "Text":"or nc 2 dupprop" } ] },
    "RequiredReference": {"NestedOptional": { "Text":"rr no", "Text":"rr no dupprop" }, "NestedRequired": { "Text":"rr nr", "Text":"rr nr dupprop" }, "NestedCollection": [ { "Text":"rr nc 1", "Text":"rr nc 1 dupprop" }, { "Text":"rr nc 2", "Text":"rr nc 2 dupprop" } ] },
    "Collection":
    [
        {"NestedOptional": { "Text":"c 1 no", "Text":"c 1 no dupprop" }, "NestedRequired": { "Text":"c 1 nr", "Text":"c 1 nr dupprop" }, "NestedCollection": [ { "Text":"c 1 nc 1", "Text":"c 1 nc 1 dupprop" }, { "Text":"c 1 nc 2", "Text":"c 1 nc 2 dupprop" } ] },
        {"NestedOptional": { "Text":"c 2 no", "Text":"c 2 no dupprop" }, "NestedRequired": { "Text":"c 2 nr", "Text":"c 2 nr dupprop" }, "NestedCollection": [ { "Text":"c 2 nc 1", "Text":"c 2 nc 1 dupprop" }, { "Text":"c 2 nc 2", "Text":"c 2 nc 2 dupprop" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            duplicatedScalars,
            CancellationToken.None);

        var emptyNavs =
            """
{
    "Id": 4,
    "$type": "Entity",
    "id": "4",
    "Scenario": "empty navigation property names",
    "OptionalReference": {"": { "Text":"or no" }, "": { "Text":"or nr" }, "": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] },
    "RequiredReference": {"": { "Text":"rr no" }, "": { "Text":"rr nr" }, "": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] },
    "Collection":
    [
        {"": { "Text":"c 1 no" }, "": { "Text":"c 1 nr" }, "": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
        {"": { "Text":"c 2 no" }, "": { "Text":"c 2 nr" }, "": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            emptyNavs,
            CancellationToken.None);

        var emptyScalars =
            """
{
    "Id": 5,
    "$type": "Entity",
    "id": "5",
    "Scenario": "empty scalar property names",
    "OptionalReference": {"NestedOptional": { "":"or no" }, "NestedRequired": { "":"or nr" }, "NestedCollection": [ { "":"or nc 1" }, { "":"or nc 2" } ] },
    "RequiredReference": {"NestedOptional": { "":"rr no" }, "NestedRequired": { "":"rr nr" }, "NestedCollection": [ { "":"rr nc 1" }, { "":"rr nc 2" } ] },
    "Collection":
    [
        {"NestedOptional": { "":"c 1 no" }, "NestedRequired": { "":"c 1 nr" }, "NestedCollection": [ { "":"c 1 nc 1" }, { "":"c 1 nc 2" } ] },
        {"NestedOptional": { "":"c 2 no" }, "NestedRequired": { "":"c 2 nr" }, "NestedCollection": [ { "":"c 2 nc 1" }, { "":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            emptyScalars,
            CancellationToken.None);

        var nullNavs =
            """
{
    "Id": 10,
    "$type": "Entity",
    "id": "10",
    "Scenario": "null navigation property names",
    "OptionalReference": {null: { "Text":"or no" }, null: { "Text":"or nr" }, null: [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] },
    "RequiredReference": {null: { "Text":"rr no" }, null: { "Text":"rr nr" }, null: [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] },
    "Collection":
    [
        {null: { "Text":"c 1 no" }, null: { "Text":"c 1 nr" }, null: [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
        {null: { "Text":"c 2 no" }, null: { "Text":"c 2 nr" }, null: [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullNavs,
            CancellationToken.None);

        var nullScalars =
            """
{
    "Id": 11,
    "$type": "Entity",
    "id": "11",
    "Scenario": "null scalar property names",
    "OptionalReference": {"NestedOptional": { null:"or no", "Text":"or no nonnull" }, "NestedRequired": { null:"or nr", "Text":"or nr nonnull" }, "NestedCollection": [ { null:"or nc 1", "Text":"or nc 1 nonnull" }, { null:"or nc 2", "Text":"or nc 2 nonnull" } ] },
    "RequiredReference": {"NestedOptional": { null:"rr no", "Text":"rr no nonnull" }, "NestedRequired": { null:"rr nr", "Text":"rr nr nonnull" }, "NestedCollection": [ { null:"rr nc 1", "Text":"rr nc 1 nonnull" }, { null:"rr nc 2", "Text":"rr nc 2 nonnull" } ] },
    "Collection":
    [
        {"NestedOptional": { null:"c 1 no", "Text":"c 1 no nonnull" }, "NestedRequired": { null:"c 1 nr", "Text":"c 1 nr nonnull" }, "NestedCollection": [ { null:"c 1 nc 1", "Text":"c 1 nc 1 nonnull" }, { null:"c 1 nc 2", "Text":"c 1 nc 2 nonnull" } ] },
        {"NestedOptional": { null:"c 2 no", "Text":"c 2 no nonnull" }, "NestedRequired": { null:"c 2 nr", "Text":"c 2 nr nonnull" }, "NestedCollection": [ { null:"c 2 nc 1", "Text":"c 2 nc 1 nonnull" }, { null:"c 2 nc 2", "Text":"c 2 nc 2 nonnull" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullScalars,
            CancellationToken.None);
    }

    #endregion


    private TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    new static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[..^1],
            (await Assert.ThrowsAsync<InvalidOperationException>(query)).Message);

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => CosmosTestStoreFactory.Instance;

}
