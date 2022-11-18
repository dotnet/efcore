// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCRelationshipsQuerySqlServerTest
    : TPCRelationshipsQueryTestBase<TPCRelationshipsQuerySqlServerTest.TPCRelationshipsQuerySqlServerFixture>
{
    public TPCRelationshipsQuerySqlServerTest(
        TPCRelationshipsQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override void Changes_in_derived_related_entities_are_detected()
    {
        base.Changes_in_derived_related_entities_are_detected();

        AssertSql(
"""
SELECT [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[BaseInheritanceRelationshipEntityId], [t0].[Id1], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t0].[Id0], [t0].[Name0], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name], [t1].[Id], [t1].[BaseParentId], [t1].[Name], [t1].[DerivedProperty], [t1].[Discriminator]
FROM (
    SELECT TOP(2) [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [o].[Id] AS [Id0], [o].[Name] AS [Name0], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM (
        SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
        FROM [BaseEntities] AS [b0]
        UNION ALL
        SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
        FROM [DerivedEntities] AS [d2]
    ) AS [t]
    LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
    LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
    WHERE [t].[Name] = N'Derived1(4)'
) AS [t0]
LEFT JOIN [OwnedCollections] AS [o0] ON [t0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t0].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t1] ON [t0].[Id] = [t1].[BaseParentId]
ORDER BY [t0].[Id], [t0].[BaseInheritanceRelationshipEntityId], [t0].[Id1], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance(bool async)
    {
        await base.Include_collection_without_inheritance(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [t].[Id] = [c].[ParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_reverse(async);

        AssertSql(
"""
SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [t].[Id] = [c].[ParentId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse(async);

        AssertSql(
"""
SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance(bool async)
    {
        await base.Include_collection_with_inheritance(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d2]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived1(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator]
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
) AS [t] ON [d].[Id] = [t].[BaseParentId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived2(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator]
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
) AS [t] ON [d].[Id] = [t].[ParentId]
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
SELECT [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[ParentId], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseCollectionOnDerived' AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId], N'DerivedCollectionOnDerived' AS [Discriminator]
    FROM [DerivedCollectionsOnDerived] AS [d1]
) AS [t]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[ParentId] = [d].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t0].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d2]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t0].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance(bool async)
    {
        await base.Include_reference_without_inheritance(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [ReferencesOnBase] AS [r] ON [t].[Id] = [r].[ParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
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
SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t] ON [r].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [ReferencesOnBase] AS [r] ON [t].[Id] = [r].[ParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [r].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter_reverse(async);

        AssertSql(
"""
SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t] ON [r].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [r].[Name] <> N'Bar' OR [r].[Name] IS NULL
ORDER BY [r].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance(bool async)
    {
        await base.Include_reference_with_inheritance(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d2]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived1(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[BaseParentId], [t].[Name], [t].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived2(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d1]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
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
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d1]
) AS [t]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[BaseParentId] = [d].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter1(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[BaseParentId], [t].[Name], [t].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [d].[Name] <> N'Bar' OR [d].[Name] IS NULL
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter2(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d1]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [d].[Name] <> N'Bar' OR [d].[Name] IS NULL
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
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
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseReferenceOnDerived' AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], N'DerivedReferenceOnDerived' AS [Discriminator]
    FROM [DerivedReferencesOnDerived] AS [d1]
) AS [t]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[BaseParentId] = [d].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t0].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d2]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t0].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance(bool async)
    {
        await base.Include_self_reference_with_inheritance(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name], [d].[Name], [d].[BaseId], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [o2].[Name], [o0].[Id], [o0].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d3].[Id], [d3].[Name], [d3].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d3]
) AS [t]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[BaseId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [t].[Id] = [d0].[Id]
LEFT JOIN [OwnedReferences] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o1] ON [t].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o2] ON [d].[Id] = [o2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [d].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_self_reference_with_inheritance_reverse(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [o1].[Name], [o].[Id], [o].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Name], [t].[BaseId], [t].[Discriminator], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [o2].[Name], [o0].[Id], [o0].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d3].[Id], [d3].[Name], [d3].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d3]
) AS [t] ON [d].[BaseId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedReferences] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d0] ON [t].[Id] = [d0].[Id]
LEFT JOIN [OwnedCollections] AS [o1] ON [d].[Id] = [o1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [d].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o2] ON [t].[Id] = [o2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [t].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [d0].[Id], [o1].[BaseInheritanceRelationshipEntityId], [o1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [o2].[BaseInheritanceRelationshipEntityId], [o2].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_collection_reference_on_non_entity_base(bool async)
    {
        await base.Nested_include_collection_reference_on_non_entity_base(async);

        AssertSql(
"""
SELECT [r].[Id], [r].[Name], [t].[Id], [t].[Name], [t].[ReferenceId], [t].[ReferencedEntityId], [t].[Id0], [t].[Name0]
FROM [ReferencedEntities] AS [r]
LEFT JOIN (
    SELECT [p].[Id], [p].[Name], [p].[ReferenceId], [p].[ReferencedEntityId], [r0].[Id] AS [Id0], [r0].[Name] AS [Name0]
    FROM [PrincipalEntities] AS [p]
    LEFT JOIN [ReferencedEntities] AS [r0] ON [p].[ReferenceId] = [r0].[Id]
) AS [t] ON [r].[Id] = [t].[ReferencedEntityId]
ORDER BY [r].[Id], [t].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t1].[Id], [t1].[BaseParentId], [t1].[Name], [t1].[DerivedProperty], [t1].[Discriminator], [t1].[Id0], [t1].[Name0], [t1].[ParentCollectionId], [t1].[ParentReferenceId], [t1].[Discriminator0]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t2].[Id] AS [Id0], [t2].[Name] AS [Name0], [t2].[ParentCollectionId], [t2].[ParentReferenceId], [t2].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
        FROM [BaseCollectionsOnBase] AS [b0]
        UNION ALL
        SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
        FROM [DerivedCollectionsOnBase] AS [d2]
    ) AS [t0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
        FROM [NestedCollections] AS [n]
        UNION ALL
        SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
        FROM [NestedCollectionsDerived] AS [n0]
    ) AS [t2] ON [t0].[Id] = [t2].[ParentCollectionId]
) AS [t1] ON [t].[Id] = [t1].[BaseParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t1].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[BaseId], [t1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t1].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t1].[Id], [t1].[BaseParentId], [t1].[Name], [t1].[DerivedProperty], [t1].[Discriminator], [t1].[Id0], [t1].[Name0], [t1].[ParentCollectionId], [t1].[ParentReferenceId], [t1].[Discriminator0]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t2].[Id] AS [Id0], [t2].[Name] AS [Name0], [t2].[ParentCollectionId], [t2].[ParentReferenceId], [t2].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
        FROM [BaseCollectionsOnBase] AS [b0]
        UNION ALL
        SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], [d2].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
        FROM [DerivedCollectionsOnBase] AS [d2]
    ) AS [t0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
        FROM [NestedReferences] AS [n]
        UNION ALL
        SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
        FROM [NestedReferencesDerived] AS [n0]
    ) AS [t2] ON [t0].[Id] = [t2].[ParentCollectionId]
) AS [t1] ON [t].[Id] = [t1].[BaseParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t1].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[BaseId], [t1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t1].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[ParentCollectionId], [t1].[ParentReferenceId], [t1].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d2]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t1] ON [t0].[Id] = [t1].[ParentReferenceId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[BaseId], [t1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t0] ON [t].[ParentReferenceId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t1].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator], [t1].[Name], [t1].[ParentCollectionId], [t1].[ParentReferenceId], [t1].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId], [d2].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d2]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [t1] ON [t0].[Id] = [t1].[ParentReferenceId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_on_base(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_reverse(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[BaseId], [t1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t0] ON [t].[ParentReferenceId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t1].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Collection_projection_on_base_type(bool async)
    {
        await base.Collection_projection_on_base_type(async);

        AssertSql(
"""
SELECT [t].[Id], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
ORDER BY [t].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [o].[Id], [o].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [d1].[Id], [d1].[Name], [d1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d2].[Id], [d2].[Name], [d2].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d2]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
LEFT JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [DerivedCollectionsOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Id] >= 4
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_split(bool async)
    {
        await base.Include_collection_with_inheritance_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_reverse_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id]
    FROM [DerivedEntities] AS [d2]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t0].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t0].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id]
    FROM [DerivedEntities] AS [d2]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t0].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t0].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t0].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_split(bool async)
    {
        await base.Include_collection_without_inheritance_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [CollectionsOnBase] AS [c] ON [t].[Id] = [c].[ParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_reverse_split(async);

        AssertSql(
"""
SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id], [b].[Name]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [CollectionsOnBase] AS [c] ON [t].[Id] = [c].[ParentId]
WHERE [t].[Name] <> N'Bar' OR [t].[Name] IS NULL
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse_split(async);

        AssertSql(
"""
SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
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
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
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
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN (
    SELECT [b].[Id], [b].[Name], [b].[ParentId], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseCollectionOnDerived' AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[ParentId], [d0].[DerivedInheritanceRelationshipEntityId], N'DerivedCollectionOnDerived' AS [Discriminator]
    FROM [DerivedCollectionsOnDerived] AS [d0]
) AS [t] ON [d].[Id] = [t].[ParentId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
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
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [d0].[Id], [d0].[Name], [d0].[ParentId], [d0].[DerivedInheritanceRelationshipEntityId], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedCollectionsOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [d].[Id], [d].[Name], [d].[BaseId], [o].[BaseInheritanceRelationshipEntityId], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[ParentId], NULL AS [DerivedInheritanceRelationshipEntityId], N'BaseCollectionOnDerived' AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[ParentId], [d0].[DerivedInheritanceRelationshipEntityId], N'DerivedCollectionOnDerived' AS [Discriminator]
    FROM [DerivedCollectionsOnDerived] AS [d0]
) AS [t]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[ParentId] = [d].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM (
    SELECT [b].[Id], [b].[ParentId]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[ParentId]
    FROM [DerivedCollectionsOnDerived] AS [d0]
) AS [t]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[ParentId] = [d].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM (
    SELECT [b].[Id], [b].[ParentId]
    FROM [BaseCollectionsOnDerived] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[ParentId]
    FROM [DerivedCollectionsOnDerived] AS [d1]
) AS [t]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[ParentId] = [d].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [d].[Id], [o].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d2].[Id], [d2].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d2]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [t1].[Id], [t1].[Name], [t1].[ParentCollectionId], [t1].[ParentReferenceId], [t1].[Discriminator], [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t1] ON [t0].[Id] = [t1].[ParentReferenceId]
ORDER BY [t].[Id], [t0].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

        AssertSql(
"""
SELECT [d].[Id], [d].[Name], [d].[BaseId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [o].[Id], [o].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[BaseParentId], [t].[Name], [t].[Discriminator]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [OwnedCollections] AS [o0] ON [d].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [d].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId]
""",
            //
"""
SELECT [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator], [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId]
FROM [DerivedEntities] AS [d]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [t] ON [d].[Id] = [t].[BaseParentId]
LEFT JOIN [OwnedReferences] AS [o] ON [d].[Id] = [o].[BaseInheritanceRelationshipEntityId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
ORDER BY [d].[Id], [t].[Id], [o].[BaseInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[BaseId], [t1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], N'BaseReferenceOnBase' AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], N'DerivedReferenceOnBase' AS [Discriminator]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [t0] ON [t].[ParentReferenceId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentReferenceId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentReferenceId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d0]
) AS [t0] ON [t].[ParentReferenceId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentReferenceId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentReferenceId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedReferencesOnBase] AS [d1]
) AS [t0] ON [t].[ParentReferenceId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id]
    FROM [DerivedEntities] AS [d2]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t1].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [t1].[Id], [t1].[BaseParentId], [t1].[Name], [t1].[DerivedProperty], [t1].[Discriminator], [t1].[Id0], [t1].[Name0], [t1].[ParentCollectionId], [t1].[ParentReferenceId], [t1].[Discriminator0] AS [Discriminator], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN (
    SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t2].[Id] AS [Id0], [t2].[Name] AS [Name0], [t2].[ParentCollectionId], [t2].[ParentReferenceId], [t2].[Discriminator] AS [Discriminator0]
    FROM (
        SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
        FROM [BaseCollectionsOnBase] AS [b0]
        UNION ALL
        SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
        FROM [DerivedCollectionsOnBase] AS [d1]
    ) AS [t0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
        FROM [NestedReferences] AS [n]
        UNION ALL
        SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
        FROM [NestedReferencesDerived] AS [n0]
    ) AS [t2] ON [t0].[Id] = [t2].[ParentCollectionId]
) AS [t1] ON [t].[Id] = [t1].[BaseParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[BaseId], [t1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedReferenceBase' AS [Discriminator]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedReferenceDerived' AS [Discriminator]
    FROM [NestedReferencesDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedReferencesDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedReferences] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedReferencesDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id]
    FROM [DerivedEntities] AS [d2]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t1].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId], [d1].[Name], [d1].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [t0].[Id]
""",
            //
"""
SELECT [t1].[Id], [t1].[Name], [t1].[ParentCollectionId], [t1].[ParentReferenceId], [t1].[Discriminator], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [t0].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t1] ON [t0].[Id] = [t1].[ParentCollectionId]
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [t0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t1].[Id], [t1].[Name], [t1].[BaseId], [t1].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], N'NestedCollectionBase' AS [Discriminator]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[Name], [n0].[ParentCollectionId], [n0].[ParentReferenceId], N'NestedCollectionDerived' AS [Discriminator]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id], [d1].[Name], [d1].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d1]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t1].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [n].[Id], [n].[ParentCollectionId]
    FROM [NestedCollections] AS [n]
    UNION ALL
    SELECT [n0].[Id], [n0].[ParentCollectionId]
    FROM [NestedCollectionsDerived] AS [n0]
) AS [t]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
    UNION ALL
    SELECT [d1].[Id], [d1].[BaseParentId]
    FROM [DerivedCollectionsOnBase] AS [d1]
) AS [t0] ON [t].[ParentCollectionId] = [t0].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    UNION ALL
    SELECT [d2].[Id]
    FROM [DerivedEntities] AS [d2]
) AS [t1] ON [t0].[BaseParentId] = [t1].[Id]
LEFT JOIN [OwnedReferences] AS [o] ON [t1].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t1].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t1].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [t].[Id], [t0].[Id], [t1].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
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
SELECT [t].[Id], [t].[Name], [t].[ReferenceId], [t].[ReferencedEntityId], [t].[Id0], [t].[Name0], [r].[Id]
FROM [ReferencedEntities] AS [r]
INNER JOIN (
    SELECT [p].[Id], [p].[Name], [p].[ReferenceId], [p].[ReferencedEntityId], [r0].[Id] AS [Id0], [r0].[Name] AS [Name0]
    FROM [PrincipalEntities] AS [p]
    LEFT JOIN [ReferencedEntities] AS [r0] ON [p].[ReferenceId] = [r0].[Id]
) AS [t] ON [r].[Id] = [t].[ReferencedEntityId]
ORDER BY [r].[Id]
""");
    }

    public override async Task Collection_projection_on_base_type_split(bool async)
    {
        await base.Collection_projection_on_base_type_split(async);

        AssertSql(
"""
SELECT [t].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [t]
ORDER BY [t].[Id]
""",
            //
"""
SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d].[Id]
    FROM [DerivedEntities] AS [d]
) AS [t]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], NULL AS [DerivedProperty], N'BaseCollectionOnBase' AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    UNION ALL
    SELECT [d0].[Id], [d0].[BaseParentId], [d0].[Name], [d0].[DerivedProperty], N'DerivedCollectionOnBase' AS [Discriminator]
    FROM [DerivedCollectionsOnBase] AS [d0]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
ORDER BY [t].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast_split(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast_split(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [o].[BaseInheritanceRelationshipEntityId], [d].[Id], [o].[Id], [o].[Name], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM (
    SELECT [b].[Id], [b].[Name], NULL AS [BaseId], N'BaseInheritanceRelationshipEntity' AS [Discriminator]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id], [d0].[Name], [d0].[BaseId], N'DerivedInheritanceRelationshipEntity' AS [Discriminator]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
WHERE [t].[Id] >= 4
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [o0].[BaseInheritanceRelationshipEntityId], [o0].[Id], [o0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d0].[Id]
    FROM [DerivedEntities] AS [d0]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [OwnedCollections] AS [o0] ON [t].[Id] = [o0].[BaseInheritanceRelationshipEntityId]
WHERE [t].[Id] >= 4
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Id] >= 4
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
""",
            //
"""
SELECT [d0].[Id], [d0].[Name], [d0].[ParentId], [d0].[DerivedInheritanceRelationshipEntityId], [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
FROM (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
    UNION ALL
    SELECT [d1].[Id]
    FROM [DerivedEntities] AS [d1]
) AS [t]
LEFT JOIN [OwnedReferences] AS [o] ON [t].[Id] = [o].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities] AS [d] ON [t].[Id] = [d].[Id]
INNER JOIN [DerivedCollectionsOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [t].[Id] >= 4
ORDER BY [t].[Id], [o].[BaseInheritanceRelationshipEntityId], [d].[Id]
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
