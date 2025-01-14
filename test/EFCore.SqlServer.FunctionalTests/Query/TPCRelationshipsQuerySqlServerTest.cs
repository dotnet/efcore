// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TPCRelationshipsQuerySqlServerTest
    : TPCRelationshipsQueryTestBase<TPCRelationshipsQuerySqlServerTest.TPCRelationshipsQuerySqlServerFixture>
{
    public TPCRelationshipsQuerySqlServerTest(
        TPCRelationshipsQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override void Changes_in_derived_related_entities_are_detected()
    {
        base.Changes_in_derived_related_entities_are_detected();

        AssertSql(
            """
SELECT [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [s].[BaseInheritanceRelationshipEntityId], [s].[Id1], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [s].[Id0], [s].[Name0], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator]
FROM (
    SELECT TOP(2) [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [o].[Id] AS [Id0], [o].[Name] AS [Name0], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM (
        SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
        FROM [BaseEntities] AS [b]
        UNION ALL
        SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
        FROM [DerivedEntities] AS [d]
    ) AS [u]
    LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
    LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
    WHERE [u].[Name] = N'Derived1(4)'
) AS [s]
LEFT JOIN [OwnedCollections] AS [o0] ON [s].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d2]
) AS [u0] ON [s].[Id] = [u0].[BaseParentId]
ORDER BY [s].[Id], [s].[BaseInheritanceRelationshipEntityId], [s].[Id1], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance(bool async)
    {
        await base.Include_collection_without_inheritance(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [u].[Id] = [c].[ParentId]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_reverse(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [u].[Id] = [c].[ParentId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance(bool async)
    {
        await base.Include_collection_with_inheritance(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d2]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived1(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedProperty], [u].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived2(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[Id], [u].[Name], [u].[ParentId], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [b].[ParentId], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseCollectionOnDerived' AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId], N'DerivedCollectionOnDerived' AS [Discriminator]
    FROM [DerivedCollectionsOnDerived] AS [d1]
) AS [u] ON [d].[Id] = [u].[ParentId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived3(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [d1].[Id], [d1].[Name], [d1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [DerivedCollectionsOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentId], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator], [d0].[Id], [d0].[Name], [d0].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[ParentId], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseCollectionOnDerived' AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[ParentId], [d].[DerivedInheritanceRelationshipEntityId], N'DerivedCollectionOnDerived' AS [Discriminator]
    FROM [DerivedCollectionsOnDerived] AS [d]
) AS [u]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[ParentId] = [d0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedProperty], [u].[Discriminator], [u0].[Id], [u0].[Name], [u0].[BaseId], [u0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u0].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d2]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedProperty], [u].[Discriminator], [u0].[Id], [u0].[Name], [u0].[BaseId], [u0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u0].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance(bool async)
    {
        await base.Include_reference_without_inheritance(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [ReferencesOnBase] AS [r] ON [u].[Id] = [r].[ParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [ReferencesOnBase] AS [r] ON [d].[Id] = [r].[ParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [ReferencesOnDerived] AS [r] ON [d].[Id] = [r].[ParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnDerived] AS [r]
LEFT JOIN [DerivedEntities] AS [d] ON [r].[ParentId] = [d].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [r].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [ReferencesOnBase] AS [r] ON [u].[Id] = [r].[ParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [r].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [r].[Name] <> N'Bar' OR [r].[Name] IS NULL
ORDER BY [r].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance(bool async)
    {
        await base.Include_reference_with_inheritance(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[BaseParentId], [u].[Name], [u].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[BaseParentId], [u].[Name], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived4(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [DerivedReferencesOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator], [d0].[Id], [d0].[Name], [d0].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d]
) AS [u]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[BaseParentId] = [d0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter1(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[BaseParentId], [u].[Name], [u].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [d].[Name] <> N'Bar' OR [d].[Name] IS NULL
ORDER BY [d].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter2(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[BaseParentId], [u].[Name], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [d].[Name] <> N'Bar' OR [d].[Name] IS NULL
ORDER BY [d].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter4(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [DerivedReferencesOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [d].[Name] <> N'Bar' OR [d].[Name] IS NULL
ORDER BY [d].[Id], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator], [d0].[Id], [d0].[Name], [d0].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d]
) AS [u]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[BaseParentId] = [d0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[Discriminator], [u0].[Id], [u0].[Name], [u0].[BaseId], [u0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u0].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[Discriminator], [u0].[Id], [u0].[Name], [u0].[BaseId], [u0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u0].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance(bool async)
    {
        await base.Include_self_reference_with_inheritance(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name], [d0].[Name], [d0].[BaseId], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [o2].[Name], [o0].[Id], [o0].[Name], [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[BaseId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [d0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o2] ON [d0].[Id] = [o2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [d0].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [d3].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_self_reference_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[Name], [u].[BaseId], [u].[Discriminator], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [o2].[Name], [o0].[Id], [o0].[Name], [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u] ON [d].[BaseId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o1] ON [d].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [d].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o2] ON [u].[Id] = [o2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [u].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [d3].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_collection_reference_on_non_entity_base(bool async)
    {
        await base.Nested_include_collection_reference_on_non_entity_base(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [s].[Id], [s].[Name], [s].[ReferenceId], [s].[ReferencedEntityId], [s].[Id0], [s].[Name0]
FROM [ReferencedEntities] AS [r]
LEFT JOIN (
    SELECT [p].[Id], [p].[Name], [p].[ReferenceId], [p].[ReferencedEntityId], [r0].[Id] AS [Id0], [r0].[Name] AS [Name0]
    FROM [PrincipalEntities] AS [p]
    LEFT JOIN [ReferencedEntities] AS [r0] ON [p].[ReferenceId] = [r0].[Id]
) AS [s] ON [r].[Id] = [s].[ReferencedEntityId]
ORDER BY [r].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [s].[Id0], [s].[Name0], [s].[ParentCollectionId], [s].[ParentReferenceId], [s].[Discriminator0]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u1].[Id] AS [Id0], [u1].[Name] AS [Name0], [u1].[ParentCollectionId], [u1].[ParentReferenceId], [u1].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
        FROM [BaseCollectionsOnBase] AS [b0]
        UNION ALL
        SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
        FROM [DerivedCollectionsOnBase] AS [d2]
    ) AS [u0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
        FROM [NestedCollections] AS [n]
        UNION ALL
        SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
        FROM [NestedCollectionsDerived] AS [n0]
    ) AS [u1] ON [u0].[Id] = [u1].[ParentCollectionId]
) AS [s] ON [u].[Id] = [s].[BaseParentId]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentCollectionId], [u].[ParentReferenceId], [u].[Discriminator], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[BaseId], [u1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u1].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u1].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [s].[Id0], [s].[Name0], [s].[ParentCollectionId], [s].[ParentReferenceId], [s].[Discriminator0]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u1].[Id] AS [Id0], [u1].[Name] AS [Name0], [u1].[ParentCollectionId], [u1].[ParentReferenceId], [u1].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
        FROM [BaseCollectionsOnBase] AS [b0]
        UNION ALL
        SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
        FROM [DerivedCollectionsOnBase] AS [d2]
    ) AS [u0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
        FROM [NestedReferences] AS [n]
        UNION ALL
        SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
        FROM [NestedReferencesDerived] AS [n0]
    ) AS [u1] ON [u0].[Id] = [u1].[ParentCollectionId]
) AS [s] ON [u].[Id] = [s].[BaseParentId]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentCollectionId], [u].[ParentReferenceId], [u].[Discriminator], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[BaseId], [u1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u1].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u1].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[ParentCollectionId], [u1].[ParentReferenceId], [u1].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u1] ON [u0].[Id] = [u1].[ParentReferenceId]
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[BaseParentId], [u].[Name], [u].[Discriminator], [u0].[Id], [u0].[Name], [u0].[ParentCollectionId], [u0].[ParentReferenceId], [u0].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u0] ON [u].[Id] = [u0].[ParentReferenceId]
ORDER BY [d].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentCollectionId], [u].[ParentReferenceId], [u].[Discriminator], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[BaseId], [u1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d]
) AS [u0] ON [u].[ParentReferenceId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u1].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u1].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator], [u1].[Name], [u1].[ParentCollectionId], [u1].[ParentReferenceId], [u1].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [u1] ON [u0].[Id] = [u1].[ParentReferenceId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_on_base(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[BaseParentId], [u].[Name], [u].[Discriminator], [u0].[Name], [u0].[ParentCollectionId], [u0].[ParentReferenceId], [u0].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [u0] ON [u].[Id] = [u0].[ParentReferenceId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_reverse(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentCollectionId], [u].[ParentReferenceId], [u].[Discriminator], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[BaseId], [u1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d]
) AS [u0] ON [u].[ParentReferenceId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u1].[Id] = [d1].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u1].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Collection_projection_on_base_type(bool async)
    {
        await base.Collection_projection_on_base_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
ORDER BY [u].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [d2].[Id], [d2].[Name], [d2].[ParentId], [d2].[DerivedInheritanceRelationshipEntityId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [u].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [DerivedCollectionsOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Id] >= 4
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_split(bool async)
    {
        await base.Include_collection_with_inheritance_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d3].[Id], [d3].[BaseParentId], [d3].[Name], [d3].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d3]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_reverse_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedProperty], [u].[Discriminator], [u0].[Id], [u0].[Name], [u0].[BaseId], [u0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o].[Id], [o].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u0].[Id] = [d1].[Id]
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u0].[Id] = [d2].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u0].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""",
            //
            """
SELECT [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u0].[Id] = [d2].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [u0].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d3].[Id], [d3].[BaseParentId], [d3].[Name], [d3].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d3]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedProperty], [u].[Discriminator], [u0].[Id], [u0].[Name], [u0].[BaseId], [u0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o].[Id], [o].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u0].[Id] = [d1].[Id]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u0].[Id] = [d2].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u0].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""",
            //
            """
SELECT [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u0] ON [u].[BaseParentId] = [u0].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u0].[Id] = [d2].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [u0].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_split(bool async)
    {
        await base.Include_collection_without_inheritance_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [CollectionsOnBase] AS [c] ON [u].[Id] = [c].[ParentId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_reverse_split(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
ORDER BY [c].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [CollectionsOnBase] AS [c] ON [u].[Id] = [c].[ParentId]
WHERE [u].[Name] <> N'Bar' OR [u].[Name] IS NULL
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse_split(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u] ON [c].[ParentId] = [u].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived1_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1_split(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o].[Id], [o].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o1] ON [d].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [u].[Id], [u].[BaseParentId], [u].[Name], [u].[DerivedProperty], [u].[Discriminator], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d2]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived2_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2_split(async);
        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o].[Id], [o].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o1] ON [d].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [u].[Id], [u].[Name], [u].[ParentId], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN (
    SELECT [b].[Id], [b].[Name], [b].[ParentId], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseCollectionOnDerived' AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[ParentId], [d2].[DerivedInheritanceRelationshipEntityId], N'DerivedCollectionOnDerived' AS [Discriminator]
    FROM [DerivedCollectionsOnDerived] AS [d2]
) AS [u] ON [d].[Id] = [u].[ParentId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived3_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3_split(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o].[Id], [o].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o1] ON [d].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [d2].[Id], [d2].[Name], [d2].[ParentId], [d2].[DerivedInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedCollectionsOnDerived] AS [d2] ON [d].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentId], [u].[DerivedInheritanceRelationshipEntityId], [u].[Discriminator], [d0].[Id], [d0].[Name], [d0].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[ParentId], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseCollectionOnDerived' AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[ParentId], [d].[DerivedInheritanceRelationshipEntityId], N'DerivedCollectionOnDerived' AS [Discriminator]
    FROM [DerivedCollectionsOnDerived] AS [d]
) AS [u]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[ParentId] = [d0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [d0].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM (
    SELECT [b].[Id], [b].[ParentId]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[ParentId]
    FROM [DerivedCollectionsOnDerived] AS [d]
) AS [u]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[ParentId] = [d0].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [d0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o1] ON [d0].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM (
    SELECT [b].[Id], [b].[ParentId]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[ParentId]
    FROM [DerivedCollectionsOnDerived] AS [d]
) AS [u]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[ParentId] = [d0].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [d0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [d0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o].[Id], [o].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
ORDER BY [u].[Id], [u0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u].[Id] = [d2].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""",
            //
            """
SELECT [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u].[Id] = [d2].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [u].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""",
            //
            """
SELECT [u1].[Id], [u1].[Name], [u1].[ParentCollectionId], [u1].[ParentReferenceId], [u1].[Discriminator], [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u].[Id] = [d2].[Id]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u1] ON [u0].[Id] = [u1].[ParentReferenceId]
ORDER BY [u].[Id], [u0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Name], [d].[BaseId], [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [o].[Id], [o].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [u].[BaseParentId], [u].[Name], [u].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [u].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [d].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o1] ON [d].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [d].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""",
            //
            """
SELECT [u0].[Id], [u0].[Name], [u0].[ParentCollectionId], [u0].[ParentReferenceId], [u0].[Discriminator], [d].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [u] ON [d].[Id] = [u].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u0] ON [u].[Id] = [u0].[ParentReferenceId]
ORDER BY [d].[Id], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentCollectionId], [u].[ParentReferenceId], [u].[Discriminator], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[BaseId], [u1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o].[Id], [o].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d]
) AS [u0] ON [u].[ParentReferenceId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u1].[Id] = [d1].[Id]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentReferenceId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentReferenceId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d]
) AS [u0] ON [u].[ParentReferenceId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u1].[Id] = [d2].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u1].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""",
            //
            """
SELECT [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentReferenceId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentReferenceId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d]
) AS [u0] ON [u].[ParentReferenceId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u1].[Id] = [d2].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [u1].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [s].[Id0], [s].[Name0], [s].[ParentCollectionId], [s].[ParentReferenceId], [s].[Discriminator0] AS [Discriminator], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN (
    SELECT [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u1].[Id] AS [Id0], [u1].[Name] AS [Name0], [u1].[ParentCollectionId], [u1].[ParentReferenceId], [u1].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
        FROM [BaseCollectionsOnBase] AS [b0]
        UNION ALL
        SELECT [d3].[Id], [d3].[BaseParentId], [d3].[Name], [d3].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
        FROM [DerivedCollectionsOnBase] AS [d3]
    ) AS [u0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
        FROM [NestedReferences] AS [n]
        UNION ALL
        SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
        FROM [NestedReferencesDerived] AS [n0]
    ) AS [u1] ON [u0].[Id] = [u1].[ParentCollectionId]
) AS [s] ON [u].[Id] = [s].[BaseParentId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentCollectionId], [u].[ParentReferenceId], [u].[Discriminator], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[BaseId], [u1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o].[Id], [o].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u1].[Id] = [d1].[Id]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedReferencesDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u1].[Id] = [d2].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u1].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""",
            //
            """
SELECT [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedReferencesDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u1].[Id] = [d2].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [u1].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d3].[Id], [d3].[BaseParentId], [d3].[Name], [d3].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d3]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id], [u0].[Id]
""",
            //
            """
SELECT [u1].[Id], [u1].[Name], [u1].[ParentCollectionId], [u1].[ParentReferenceId], [u1].[Discriminator], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id], [u0].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d3].[Id], [d3].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d3]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u1] ON [u0].[Id] = [u1].[ParentCollectionId]
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id], [u0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[ParentCollectionId], [u].[ParentReferenceId], [u].[Discriminator], [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u1].[Id], [u1].[Name], [u1].[BaseId], [u1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id], [o].[Id], [o].[Name], [d1].[OwnedReferenceOnDerived_Id], [d1].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId], [d].[Name], [d].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [u1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u1].[Id] = [d1].[Id]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u1].[Id] = [d2].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u1].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""",
            //
            """
SELECT [d3].[DerivedInheritanceRelationshipEntityId], [d3].[Id], [d3].[Name], [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [u]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d]
) AS [u0] ON [u].[ParentCollectionId] = [u0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [u1] ON [u0].[BaseParentId] = [u1].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [u1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d2] ON [u1].[Id] = [d2].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d3] ON [u1].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [u].[Id], [u0].[Id], [u1].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d2].[Id]
""");
    }

    public override async Task Nested_include_collection_reference_on_non_entity_base_split(bool async)
    {
        await base.Nested_include_collection_reference_on_non_entity_base_split(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name]
FROM [ReferencedEntities] AS [r]
ORDER BY [r].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [s].[ReferenceId], [s].[ReferencedEntityId], [s].[Id0], [s].[Name0], [r].[Id]
FROM [ReferencedEntities] AS [r]
INNER JOIN (
    SELECT [p].[Id], [p].[Name], [p].[ReferenceId], [p].[ReferencedEntityId], [r0].[Id] AS [Id0], [r0].[Name] AS [Name0]
    FROM [PrincipalEntities] AS [p]
    LEFT JOIN [ReferencedEntities] AS [r0] ON [p].[ReferenceId] = [r0].[Id]
) AS [s] ON [r].[Id] = [s].[ReferencedEntityId]
ORDER BY [r].[Id]
""");
    }

    public override async Task Collection_projection_on_base_type_split(bool async)
    {
        await base.Collection_projection_on_base_type_split(async);

        AssertSql(
            """
SELECT [u].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
ORDER BY [u].[Id]
""",
            //
            """
SELECT [u0].[Id], [u0].[BaseParentId], [u0].[Name], [u0].[DerivedProperty], [u0].[Discriminator], [u].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [u0] ON [u].[Id] = [u0].[BaseParentId]
ORDER BY [u].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast_split(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast_split(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[BaseId], [u].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o].[Id], [o].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id], [d].[Name], [d].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o] ON [u].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [u].[Id] = [d0].[Id]
WHERE [u].[Id] >= 4
ORDER BY [u].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id]
""",
            //
            """
SELECT [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [OwnedCollections] AS [o1] ON [u].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
WHERE [u].[Id] >= 4
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [u].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Id] >= 4
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""",
            //
            """
SELECT [d3].[Id], [d3].[Name], [d3].[ParentId], [d3].[DerivedInheritanceRelationshipEntityId], [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [u]
LEFT JOIN [OwnedReferences] AS [o0] ON [u].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d1] ON [u].[Id] = [d1].[Id]
INNER JOIN [DerivedCollectionsOnDerived] AS [d3] ON [u].[Id] = [d3].[DerivedInheritanceRelationshipEntityId]
WHERE [u].[Id] >= 4
ORDER BY [u].[Id], [o0].[BaseInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
    {
        base.Entity_can_make_separate_relationships_with_base_type_and_derived_type_both();

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class TPCRelationshipsQuerySqlServerFixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
