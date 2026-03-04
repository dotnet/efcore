// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class EntitySplittingQuerySqliteTest : EntitySplittingQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."EntityThreeId", "e"."IntValue1", "e"."IntValue2", "e"."IntValue3", "e"."IntValue4", "e"."StringValue1", "e"."StringValue2", "e"."StringValue3", "e"."StringValue4", "o"."EntityOneId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "EntityOnes" AS "e"
LEFT JOIN "OwnedReferences" AS "o" ON "e"."Id" = "o"."EntityOneId"
LEFT JOIN "OwnedReferenceExtras2" AS "o0" ON "o"."EntityOneId" = "o0"."EntityOneId"
LEFT JOIN "OwnedReferenceExtras1" AS "o1" ON "o"."EntityOneId" = "o1"."EntityOneId"
""");
    }

    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing_custom_projection(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing_custom_projection(async);

        AssertSql(
            """
SELECT "e"."Id", "o0"."OwnedIntValue4", "o0"."OwnedStringValue4"
FROM "EntityOnes" AS "e"
LEFT JOIN "OwnedReferences" AS "o" ON "e"."Id" = "o"."EntityOneId"
LEFT JOIN "OwnedReferenceExtras2" AS "o0" ON "o"."EntityOneId" = "o0"."EntityOneId"
""");
    }

    public override async Task Normal_entity_owning_a_split_collection(bool async)
    {
        await base.Normal_entity_owning_a_split_collection(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."EntityThreeId", "e"."IntValue1", "e"."IntValue2", "e"."IntValue3", "e"."IntValue4", "e"."StringValue1", "e"."StringValue2", "e"."StringValue3", "e"."StringValue4", "s"."EntityOneId", "s"."Id", "s"."OwnedIntValue1", "s"."OwnedIntValue2", "s"."OwnedIntValue3", "s"."OwnedIntValue4", "s"."OwnedStringValue1", "s"."OwnedStringValue2", "s"."OwnedStringValue3", "s"."OwnedStringValue4"
FROM "EntityOnes" AS "e"
LEFT JOIN (
    SELECT "o"."EntityOneId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedCollection" AS "o"
    INNER JOIN "OwnedCollectionExtras2" AS "o0" ON "o"."EntityOneId" = "o0"."EntityOneId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedCollectionExtras1" AS "o1" ON "o"."EntityOneId" = "o1"."EntityOneId" AND "o"."Id" = "o1"."Id"
) AS "s" ON "e"."Id" = "s"."EntityOneId"
ORDER BY "e"."Id", "s"."EntityOneId"
""");
    }

    public override async Task Split_entity_owning_a_split_reference_without_table_sharing(bool async)
    {
        await base.Split_entity_owning_a_split_reference_without_table_sharing(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."EntityThreeId", "e"."IntValue1", "e"."IntValue2", "s0"."IntValue3", "s"."IntValue4", "e"."StringValue1", "e"."StringValue2", "s0"."StringValue3", "s"."StringValue4", "o"."EntityOneId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "EntityOne" AS "e"
INNER JOIN "SplitEntityOnePart3" AS "s" ON "e"."Id" = "s"."Id"
INNER JOIN "SplitEntityOnePart2" AS "s0" ON "e"."Id" = "s0"."Id"
LEFT JOIN "OwnedReferences" AS "o" ON "e"."Id" = "o"."EntityOneId"
LEFT JOIN "OwnedReferenceExtras2" AS "o0" ON "o"."EntityOneId" = "o0"."EntityOneId"
LEFT JOIN "OwnedReferenceExtras1" AS "o1" ON "o"."EntityOneId" = "o1"."EntityOneId"
""");
    }

    public override async Task Split_entity_owning_a_split_collection(bool async)
    {
        await base.Split_entity_owning_a_split_collection(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."EntityThreeId", "e"."IntValue1", "e"."IntValue2", "s0"."IntValue3", "s"."IntValue4", "e"."StringValue1", "e"."StringValue2", "s0"."StringValue3", "s"."StringValue4", "s1"."EntityOneId", "s1"."Id", "s1"."OwnedIntValue1", "s1"."OwnedIntValue2", "s1"."OwnedIntValue3", "s1"."OwnedIntValue4", "s1"."OwnedStringValue1", "s1"."OwnedStringValue2", "s1"."OwnedStringValue3", "s1"."OwnedStringValue4"
FROM "EntityOne" AS "e"
INNER JOIN "SplitEntityOnePart3" AS "s" ON "e"."Id" = "s"."Id"
INNER JOIN "SplitEntityOnePart2" AS "s0" ON "e"."Id" = "s0"."Id"
LEFT JOIN (
    SELECT "o"."EntityOneId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedCollection" AS "o"
    INNER JOIN "OwnedCollectionExtras2" AS "o0" ON "o"."EntityOneId" = "o0"."EntityOneId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedCollectionExtras1" AS "o1" ON "o"."EntityOneId" = "o1"."EntityOneId" AND "o"."Id" = "o1"."Id"
) AS "s1" ON "e"."Id" = "s1"."EntityOneId"
ORDER BY "e"."Id", "s1"."EntityOneId"
""");
    }

    public override async Task Split_entity_owning_a_split_reference_with_table_sharing_6(bool async)
    {
        await base.Split_entity_owning_a_split_reference_with_table_sharing_6(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."EntityThreeId", "s"."IntValue1", "s"."IntValue2", "s1"."IntValue3", "s0"."IntValue4", "s"."StringValue1", "s"."StringValue2", "s1"."StringValue3", "s0"."StringValue4", "s1"."Id", "s1"."OwnedReference_Id", "s1"."OwnedReference_OwnedIntValue1", "s1"."OwnedReference_OwnedIntValue2", "o0"."OwnedIntValue3", "o"."OwnedIntValue4", "s1"."OwnedReference_OwnedStringValue1", "s1"."OwnedReference_OwnedStringValue2", "o0"."OwnedStringValue3", "o"."OwnedStringValue4"
FROM "SplitEntityOnePart1" AS "s"
INNER JOIN "SplitEntityOnePart3" AS "s0" ON "s"."Id" = "s0"."Id"
INNER JOIN "SplitEntityOnePart2" AS "s1" ON "s"."Id" = "s1"."Id"
LEFT JOIN "OwnedReferencePart3" AS "o" ON "s1"."Id" = "o"."EntityOneId"
LEFT JOIN "OwnedReferencePart2" AS "o0" ON "s1"."Id" = "o0"."EntityOneId"
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_base_without_table_sharing(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "b"."Discriminator", "b"."MiddleValue", "b"."SiblingValue", "b"."LeafValue", "o"."BaseEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "OwnedReferencePart1" AS "o" ON "b"."Id" = "o"."BaseEntityId"
LEFT JOIN "OwnedReferencePart4" AS "o0" ON "o"."BaseEntityId" = "o0"."BaseEntityId"
LEFT JOIN "OwnedReferencePart3" AS "o1" ON "o"."BaseEntityId" = "o1"."BaseEntityId"
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_base_without_table_sharing(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "m"."MiddleValue", "s"."SiblingValue", "l"."LeafValue", CASE
    WHEN "l"."Id" IS NOT NULL THEN 'LeafEntity'
    WHEN "s"."Id" IS NOT NULL THEN 'SiblingEntity'
    WHEN "m"."Id" IS NOT NULL THEN 'MiddleEntity'
END AS "Discriminator", "o"."BaseEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "MiddleEntity" AS "m" ON "b"."Id" = "m"."Id"
LEFT JOIN "SiblingEntity" AS "s" ON "b"."Id" = "s"."Id"
LEFT JOIN "LeafEntity" AS "l" ON "b"."Id" = "l"."Id"
LEFT JOIN "OwnedReferencePart1" AS "o" ON "b"."Id" = "o"."BaseEntityId"
LEFT JOIN "OwnedReferencePart4" AS "o0" ON "o"."BaseEntityId" = "o0"."BaseEntityId"
LEFT JOIN "OwnedReferencePart3" AS "o1" ON "o"."BaseEntityId" = "o1"."BaseEntityId"
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_middle_without_table_sharing(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "b"."Discriminator", "b"."MiddleValue", "b"."SiblingValue", "b"."LeafValue", "o"."MiddleEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "OwnedReferencePart1" AS "o" ON "b"."Id" = "o"."MiddleEntityId"
LEFT JOIN "OwnedReferencePart4" AS "o0" ON "o"."MiddleEntityId" = "o0"."MiddleEntityId"
LEFT JOIN "OwnedReferencePart3" AS "o1" ON "o"."MiddleEntityId" = "o1"."MiddleEntityId"
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_middle_without_table_sharing(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "m"."MiddleValue", "s"."SiblingValue", "l"."LeafValue", CASE
    WHEN "l"."Id" IS NOT NULL THEN 'LeafEntity'
    WHEN "s"."Id" IS NOT NULL THEN 'SiblingEntity'
    WHEN "m"."Id" IS NOT NULL THEN 'MiddleEntity'
END AS "Discriminator", "o"."MiddleEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "MiddleEntity" AS "m" ON "b"."Id" = "m"."Id"
LEFT JOIN "SiblingEntity" AS "s" ON "b"."Id" = "s"."Id"
LEFT JOIN "LeafEntity" AS "l" ON "b"."Id" = "l"."Id"
LEFT JOIN "OwnedReferencePart1" AS "o" ON "b"."Id" = "o"."MiddleEntityId"
LEFT JOIN "OwnedReferencePart4" AS "o0" ON "o"."MiddleEntityId" = "o0"."MiddleEntityId"
LEFT JOIN "OwnedReferencePart3" AS "o1" ON "o"."MiddleEntityId" = "o1"."MiddleEntityId"
""");
    }

    public override async Task Tph_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await base.Tph_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "b"."Discriminator", "b"."MiddleValue", "b"."SiblingValue", "b"."LeafValue", "o"."LeafEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "OwnedReferencePart1" AS "o" ON "b"."Id" = "o"."LeafEntityId"
LEFT JOIN "OwnedReferencePart4" AS "o0" ON "o"."LeafEntityId" = "o0"."LeafEntityId"
LEFT JOIN "OwnedReferencePart3" AS "o1" ON "o"."LeafEntityId" = "o1"."LeafEntityId"
""");
    }

    public override async Task Tpt_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await base.Tpt_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "m"."MiddleValue", "s"."SiblingValue", "l"."LeafValue", CASE
    WHEN "l"."Id" IS NOT NULL THEN 'LeafEntity'
    WHEN "s"."Id" IS NOT NULL THEN 'SiblingEntity'
    WHEN "m"."Id" IS NOT NULL THEN 'MiddleEntity'
END AS "Discriminator", "o"."LeafEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "MiddleEntity" AS "m" ON "b"."Id" = "m"."Id"
LEFT JOIN "SiblingEntity" AS "s" ON "b"."Id" = "s"."Id"
LEFT JOIN "LeafEntity" AS "l" ON "b"."Id" = "l"."Id"
LEFT JOIN "OwnedReferencePart1" AS "o" ON "b"."Id" = "o"."LeafEntityId"
LEFT JOIN "OwnedReferencePart4" AS "o0" ON "o"."LeafEntityId" = "o0"."LeafEntityId"
LEFT JOIN "OwnedReferencePart3" AS "o1" ON "o"."LeafEntityId" = "o1"."LeafEntityId"
""");
    }

    public override async Task Tpc_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await base.Tpc_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);

        AssertSql(
            """
SELECT "u"."Id", "u"."BaseValue", "u"."MiddleValue", "u"."SiblingValue", "u"."LeafValue", "u"."Discriminator", "o"."LeafEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
FROM (
    SELECT "b"."Id", "b"."BaseValue", NULL AS "MiddleValue", NULL AS "SiblingValue", NULL AS "LeafValue", 'BaseEntity' AS "Discriminator"
    FROM "BaseEntity" AS "b"
    UNION ALL
    SELECT "m"."Id", "m"."BaseValue", "m"."MiddleValue", NULL AS "SiblingValue", NULL AS "LeafValue", 'MiddleEntity' AS "Discriminator"
    FROM "MiddleEntity" AS "m"
    UNION ALL
    SELECT "s"."Id", "s"."BaseValue", NULL AS "MiddleValue", "s"."SiblingValue", NULL AS "LeafValue", 'SiblingEntity' AS "Discriminator"
    FROM "SiblingEntity" AS "s"
    UNION ALL
    SELECT "l"."Id", "l"."BaseValue", "l"."MiddleValue", NULL AS "SiblingValue", "l"."LeafValue", 'LeafEntity' AS "Discriminator"
    FROM "LeafEntity" AS "l"
) AS "u"
LEFT JOIN "OwnedReferencePart1" AS "o" ON "u"."Id" = "o"."LeafEntityId"
LEFT JOIN "OwnedReferencePart4" AS "o0" ON "o"."LeafEntityId" = "o0"."LeafEntityId"
LEFT JOIN "OwnedReferencePart3" AS "o1" ON "o"."LeafEntityId" = "o1"."LeafEntityId"
""");
    }

    public override async Task Tph_entity_owning_a_split_collection_on_base(bool async)
    {
        await base.Tph_entity_owning_a_split_collection_on_base(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "b"."Discriminator", "b"."MiddleValue", "b"."SiblingValue", "b"."LeafValue", "s"."BaseEntityId", "s"."Id", "s"."OwnedIntValue1", "s"."OwnedIntValue2", "s"."OwnedIntValue3", "s"."OwnedIntValue4", "s"."OwnedStringValue1", "s"."OwnedStringValue2", "s"."OwnedStringValue3", "s"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN (
    SELECT "o"."BaseEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedReferencePart1" AS "o"
    INNER JOIN "OwnedReferencePart4" AS "o0" ON "o"."BaseEntityId" = "o0"."BaseEntityId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedReferencePart3" AS "o1" ON "o"."BaseEntityId" = "o1"."BaseEntityId" AND "o"."Id" = "o1"."Id"
) AS "s" ON "b"."Id" = "s"."BaseEntityId"
ORDER BY "b"."Id", "s"."BaseEntityId"
""");
    }

    public override async Task Tpt_entity_owning_a_split_collection_on_base(bool async)
    {
        await base.Tpt_entity_owning_a_split_collection_on_base(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "m"."MiddleValue", "s"."SiblingValue", "l"."LeafValue", CASE
    WHEN "l"."Id" IS NOT NULL THEN 'LeafEntity'
    WHEN "s"."Id" IS NOT NULL THEN 'SiblingEntity'
    WHEN "m"."Id" IS NOT NULL THEN 'MiddleEntity'
END AS "Discriminator", "s0"."BaseEntityId", "s0"."Id", "s0"."OwnedIntValue1", "s0"."OwnedIntValue2", "s0"."OwnedIntValue3", "s0"."OwnedIntValue4", "s0"."OwnedStringValue1", "s0"."OwnedStringValue2", "s0"."OwnedStringValue3", "s0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "MiddleEntity" AS "m" ON "b"."Id" = "m"."Id"
LEFT JOIN "SiblingEntity" AS "s" ON "b"."Id" = "s"."Id"
LEFT JOIN "LeafEntity" AS "l" ON "b"."Id" = "l"."Id"
LEFT JOIN (
    SELECT "o"."BaseEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedReferencePart1" AS "o"
    INNER JOIN "OwnedReferencePart4" AS "o0" ON "o"."BaseEntityId" = "o0"."BaseEntityId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedReferencePart3" AS "o1" ON "o"."BaseEntityId" = "o1"."BaseEntityId" AND "o"."Id" = "o1"."Id"
) AS "s0" ON "b"."Id" = "s0"."BaseEntityId"
ORDER BY "b"."Id", "s0"."BaseEntityId"
""");
    }

    public override async Task Tph_entity_owning_a_split_collection_on_middle(bool async)
    {
        await base.Tph_entity_owning_a_split_collection_on_middle(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "b"."Discriminator", "b"."MiddleValue", "b"."SiblingValue", "b"."LeafValue", "s"."MiddleEntityId", "s"."Id", "s"."OwnedIntValue1", "s"."OwnedIntValue2", "s"."OwnedIntValue3", "s"."OwnedIntValue4", "s"."OwnedStringValue1", "s"."OwnedStringValue2", "s"."OwnedStringValue3", "s"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN (
    SELECT "o"."MiddleEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedReferencePart1" AS "o"
    INNER JOIN "OwnedReferencePart4" AS "o0" ON "o"."MiddleEntityId" = "o0"."MiddleEntityId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedReferencePart3" AS "o1" ON "o"."MiddleEntityId" = "o1"."MiddleEntityId" AND "o"."Id" = "o1"."Id"
) AS "s" ON "b"."Id" = "s"."MiddleEntityId"
ORDER BY "b"."Id", "s"."MiddleEntityId"
""");
    }

    public override async Task Tpt_entity_owning_a_split_collection_on_middle(bool async)
    {
        await base.Tpt_entity_owning_a_split_collection_on_middle(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "m"."MiddleValue", "s"."SiblingValue", "l"."LeafValue", CASE
    WHEN "l"."Id" IS NOT NULL THEN 'LeafEntity'
    WHEN "s"."Id" IS NOT NULL THEN 'SiblingEntity'
    WHEN "m"."Id" IS NOT NULL THEN 'MiddleEntity'
END AS "Discriminator", "s0"."MiddleEntityId", "s0"."Id", "s0"."OwnedIntValue1", "s0"."OwnedIntValue2", "s0"."OwnedIntValue3", "s0"."OwnedIntValue4", "s0"."OwnedStringValue1", "s0"."OwnedStringValue2", "s0"."OwnedStringValue3", "s0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "MiddleEntity" AS "m" ON "b"."Id" = "m"."Id"
LEFT JOIN "SiblingEntity" AS "s" ON "b"."Id" = "s"."Id"
LEFT JOIN "LeafEntity" AS "l" ON "b"."Id" = "l"."Id"
LEFT JOIN (
    SELECT "o"."MiddleEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedReferencePart1" AS "o"
    INNER JOIN "OwnedReferencePart4" AS "o0" ON "o"."MiddleEntityId" = "o0"."MiddleEntityId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedReferencePart3" AS "o1" ON "o"."MiddleEntityId" = "o1"."MiddleEntityId" AND "o"."Id" = "o1"."Id"
) AS "s0" ON "b"."Id" = "s0"."MiddleEntityId"
ORDER BY "b"."Id", "s0"."MiddleEntityId"
""");
    }

    public override async Task Tph_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await base.Tph_entity_owning_a_split_collection_on_leaf(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "b"."Discriminator", "b"."MiddleValue", "b"."SiblingValue", "b"."LeafValue", "s"."LeafEntityId", "s"."Id", "s"."OwnedIntValue1", "s"."OwnedIntValue2", "s"."OwnedIntValue3", "s"."OwnedIntValue4", "s"."OwnedStringValue1", "s"."OwnedStringValue2", "s"."OwnedStringValue3", "s"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN (
    SELECT "o"."LeafEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedReferencePart1" AS "o"
    INNER JOIN "OwnedReferencePart4" AS "o0" ON "o"."LeafEntityId" = "o0"."LeafEntityId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedReferencePart3" AS "o1" ON "o"."LeafEntityId" = "o1"."LeafEntityId" AND "o"."Id" = "o1"."Id"
) AS "s" ON "b"."Id" = "s"."LeafEntityId"
ORDER BY "b"."Id", "s"."LeafEntityId"
""");
    }

    public override async Task Tpt_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await base.Tpt_entity_owning_a_split_collection_on_leaf(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."BaseValue", "m"."MiddleValue", "s"."SiblingValue", "l"."LeafValue", CASE
    WHEN "l"."Id" IS NOT NULL THEN 'LeafEntity'
    WHEN "s"."Id" IS NOT NULL THEN 'SiblingEntity'
    WHEN "m"."Id" IS NOT NULL THEN 'MiddleEntity'
END AS "Discriminator", "s0"."LeafEntityId", "s0"."Id", "s0"."OwnedIntValue1", "s0"."OwnedIntValue2", "s0"."OwnedIntValue3", "s0"."OwnedIntValue4", "s0"."OwnedStringValue1", "s0"."OwnedStringValue2", "s0"."OwnedStringValue3", "s0"."OwnedStringValue4"
FROM "BaseEntity" AS "b"
LEFT JOIN "MiddleEntity" AS "m" ON "b"."Id" = "m"."Id"
LEFT JOIN "SiblingEntity" AS "s" ON "b"."Id" = "s"."Id"
LEFT JOIN "LeafEntity" AS "l" ON "b"."Id" = "l"."Id"
LEFT JOIN (
    SELECT "o"."LeafEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedReferencePart1" AS "o"
    INNER JOIN "OwnedReferencePart4" AS "o0" ON "o"."LeafEntityId" = "o0"."LeafEntityId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedReferencePart3" AS "o1" ON "o"."LeafEntityId" = "o1"."LeafEntityId" AND "o"."Id" = "o1"."Id"
) AS "s0" ON "b"."Id" = "s0"."LeafEntityId"
ORDER BY "b"."Id", "s0"."LeafEntityId"
""");
    }

    public override async Task Tpc_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await base.Tpc_entity_owning_a_split_collection_on_leaf(async);

        AssertSql(
            """
SELECT "u"."Id", "u"."BaseValue", "u"."MiddleValue", "u"."SiblingValue", "u"."LeafValue", "u"."Discriminator", "s0"."LeafEntityId", "s0"."Id", "s0"."OwnedIntValue1", "s0"."OwnedIntValue2", "s0"."OwnedIntValue3", "s0"."OwnedIntValue4", "s0"."OwnedStringValue1", "s0"."OwnedStringValue2", "s0"."OwnedStringValue3", "s0"."OwnedStringValue4"
FROM (
    SELECT "b"."Id", "b"."BaseValue", NULL AS "MiddleValue", NULL AS "SiblingValue", NULL AS "LeafValue", 'BaseEntity' AS "Discriminator"
    FROM "BaseEntity" AS "b"
    UNION ALL
    SELECT "m"."Id", "m"."BaseValue", "m"."MiddleValue", NULL AS "SiblingValue", NULL AS "LeafValue", 'MiddleEntity' AS "Discriminator"
    FROM "MiddleEntity" AS "m"
    UNION ALL
    SELECT "s"."Id", "s"."BaseValue", NULL AS "MiddleValue", "s"."SiblingValue", NULL AS "LeafValue", 'SiblingEntity' AS "Discriminator"
    FROM "SiblingEntity" AS "s"
    UNION ALL
    SELECT "l"."Id", "l"."BaseValue", "l"."MiddleValue", NULL AS "SiblingValue", "l"."LeafValue", 'LeafEntity' AS "Discriminator"
    FROM "LeafEntity" AS "l"
) AS "u"
LEFT JOIN (
    SELECT "o"."LeafEntityId", "o"."Id", "o"."OwnedIntValue1", "o"."OwnedIntValue2", "o1"."OwnedIntValue3", "o0"."OwnedIntValue4", "o"."OwnedStringValue1", "o"."OwnedStringValue2", "o1"."OwnedStringValue3", "o0"."OwnedStringValue4"
    FROM "OwnedReferencePart1" AS "o"
    INNER JOIN "OwnedReferencePart4" AS "o0" ON "o"."LeafEntityId" = "o0"."LeafEntityId" AND "o"."Id" = "o0"."Id"
    INNER JOIN "OwnedReferencePart3" AS "o1" ON "o"."LeafEntityId" = "o1"."LeafEntityId" AND "o"."Id" = "o1"."Id"
) AS "s0" ON "u"."Id" = "s0"."LeafEntityId"
ORDER BY "u"."Id", "s0"."LeafEntityId"
""");
    }
}
