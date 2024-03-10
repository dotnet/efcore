// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TPTRelationshipsQuerySqlServerTest
    : TPTRelationshipsQueryTestBase<TPTRelationshipsQuerySqlServerTest.TPTRelationshipsQuerySqlServerFixture>
{
    public TPTRelationshipsQuerySqlServerTest(
        TPTRelationshipsQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override void Changes_in_derived_related_entities_are_detected()
    {
        base.Changes_in_derived_related_entities_are_detected();

        AssertSql(
            """
SELECT [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name], [s0].[Id], [s0].[BaseParentId], [s0].[Name], [s0].[DerivedProperty], [s0].[Discriminator]
FROM (
    SELECT TOP(2) [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
    WHERE [b].[Name] = N'Derived1(4)'
) AS [s]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [s].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [s].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [s0] ON [s].[Id] = [s0].[BaseParentId]
ORDER BY [s].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance(bool async)
    {
        await base.Include_collection_without_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_reverse(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [c].[ParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [s].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [s].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [s].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [c].[ParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [s].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [s].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [s].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance(bool async)
    {
        await base.Include_collection_with_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived1(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived2(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[Name], [s].[ParentId], [s].[DerivedInheritanceRelationshipEntityId], [s].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[Name], [b1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnDerived'
    END AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b1]
    LEFT JOIN [DerivedCollectionsOnDerived] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [s] ON [b].[Id] = [s].[ParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived3(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[Name], [s].[ParentId], [s].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[Name], [b1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b1]
    INNER JOIN [DerivedCollectionsOnDerived] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [s] ON [b].[Id] = [s].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[ParentId], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnDerived'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN [DerivedCollectionsOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[ParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance(bool async)
    {
        await base.Include_reference_without_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnDerived] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [s].[Id], [s].[Name], [s].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnDerived] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [r].[ParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [s].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [s].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [s].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [r].[ParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [s].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [s].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [s].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [r].[ParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [s].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [s].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE [r].[Name] <> N'Bar' OR [r].[Name] IS NULL
ORDER BY [r].[Id], [s].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance(bool async)
    {
        await base.Include_reference_with_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[DerivedInheritanceRelationshipEntityId], [s].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
    END AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b0]
    LEFT JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived4(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    INNER JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN [DerivedReferencesOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[DerivedInheritanceRelationshipEntityId], [s].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
    END AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b0]
    LEFT JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter4(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    INNER JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN [DerivedReferencesOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance(bool async)
    {
        await base.Include_self_reference_with_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Name], [s].[BaseId], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_self_reference_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Name], [s].[BaseId], [s].[Discriminator], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [d].[BaseId] = [s].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [d2].[DerivedInheritanceRelationshipEntityId]
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
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s0].[Id], [s0].[BaseParentId], [s0].[Name], [s0].[DerivedProperty], [s0].[Discriminator], [s0].[Id0], [s0].[Name0], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator0]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator], [s].[Id] AS [Id0], [s].[Name] AS [Name0], [s].[ParentCollectionId], [s].[ParentReferenceId], [s].[Discriminator] AS [Discriminator0]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
            WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
        END AS [Discriminator]
        FROM [NestedCollections] AS [n]
        LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
    ) AS [s] ON [b1].[Id] = [s].[ParentCollectionId]
) AS [s0] ON [b].[Id] = [s0].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [s0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[BaseId], [s0].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s0].[OwnedReferenceOnBase_Id], [s0].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s0].[Id0], [s0].[OwnedReferenceOnDerived_Id], [s0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s0].[Id], [s0].[BaseParentId], [s0].[Name], [s0].[DerivedProperty], [s0].[Discriminator], [s0].[Id0], [s0].[Name0], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator0]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator], [s].[Id] AS [Id0], [s].[Name] AS [Name0], [s].[ParentCollectionId], [s].[ParentReferenceId], [s].[Discriminator] AS [Discriminator0]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
            WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
        END AS [Discriminator]
        FROM [NestedReferences] AS [n]
        LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
    ) AS [s] ON [b1].[Id] = [s].[ParentCollectionId]
) AS [s0] ON [b].[Id] = [s0].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [s0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
END AS [Discriminator], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[BaseId], [s0].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s0].[OwnedReferenceOnBase_Id], [s0].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s0].[Id0], [s0].[OwnedReferenceOnDerived_Id], [s0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [s0] ON [s].[Id] = [s0].[ParentReferenceId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [s0] ON [s].[Id] = [s0].[ParentReferenceId]
ORDER BY [b].[Id], [s].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[BaseId], [s0].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s0].[OwnedReferenceOnBase_Id], [s0].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s0].[Id0], [s0].[OwnedReferenceOnDerived_Id], [s0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [n].[ParentReferenceId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator], [s0].[Name], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
    END AS [Discriminator]
    FROM [NestedReferences] AS [n]
    LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [s0] ON [s].[Id] = [s0].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_on_base(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator], [s0].[Name], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
    END AS [Discriminator]
    FROM [NestedReferences] AS [n]
    LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [s0] ON [s].[Id] = [s0].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
END AS [Discriminator], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[BaseId], [s0].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [s0].[OwnedReferenceOnBase_Id], [s0].[OwnedReferenceOnBase_Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [s0].[Id0], [s0].[OwnedReferenceOnDerived_Id], [s0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [n].[ParentReferenceId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Collection_projection_on_base_type(bool async)
    {
        await base.Collection_projection_on_base_type(async);

        AssertSql(
            """
SELECT [b].[Id], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b0].[Id] = [d].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[Name], [s].[ParentId], [s].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[Name], [b1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b1]
    INNER JOIN [DerivedCollectionsOnDerived] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [s] ON [b].[Id] = [s].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_split(bool async)
    {
        await base.Include_collection_with_inheritance_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Name], [d2].[DerivedProperty], CASE
        WHEN [d2].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d2] ON [b2].[Id] = [d2].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_reverse_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id], [s].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [b].[Id], [s].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Name], [d2].[DerivedProperty], CASE
        WHEN [d2].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d2] ON [b2].[Id] = [d2].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id], [s].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [b].[Id], [s].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s] ON [b].[BaseParentId] = [s].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [s].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_split(bool async)
    {
        await base.Include_collection_without_inheritance_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_reverse_split(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [c].[ParentId] = [s].[Id]
ORDER BY [c].[Id], [s].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [c].[Id], [s].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [s] ON [c].[ParentId] = [s].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [s].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [c].[Id], [s].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [s] ON [c].[ParentId] = [s].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [s].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse_split(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [s].[Id], [s].[Name], [s].[BaseId], [s].[Discriminator], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [c].[ParentId] = [s].[Id]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [s].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [c].[Id], [s].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [s] ON [c].[ParentId] = [s].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [s].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [s].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [c].[Id], [s].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [s] ON [c].[ParentId] = [s].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [s].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [s].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived1_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Name], [d2].[DerivedProperty], CASE
        WHEN [d2].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d2] ON [b2].[Id] = [d2].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived2_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [s].[ParentId], [s].[DerivedInheritanceRelationshipEntityId], [s].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b2].[Id], [b2].[Name], [b2].[ParentId], [d2].[DerivedInheritanceRelationshipEntityId], CASE
        WHEN [d2].[Id] IS NOT NULL THEN N'DerivedCollectionOnDerived'
    END AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b2]
    LEFT JOIN [DerivedCollectionsOnDerived] AS [d2] ON [b2].[Id] = [d2].[Id]
) AS [s] ON [b].[Id] = [s].[ParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived3_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [s].[ParentId], [s].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b2].[Id], [b2].[Name], [b2].[ParentId], [d2].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b2]
    INNER JOIN [DerivedCollectionsOnDerived] AS [d2] ON [b2].[Id] = [d2].[Id]
) AS [s] ON [b].[Id] = [s].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b].[ParentId], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnDerived'
END AS [Discriminator], [s].[Id], [s].[Name], [s].[BaseId], [s].[OwnedReferenceOnBase_Id], [s].[OwnedReferenceOnBase_Name], [s].[Id0], [s].[OwnedReferenceOnDerived_Id], [s].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN [DerivedCollectionsOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[ParentId] = [s].[Id]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id], [s].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[ParentId] = [s].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [b].[Id], [s].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[ParentId] = [s].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [s].[Id], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id], [s].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [b].[Id], [s].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [b].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator], [b].[Id], [s].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [s0] ON [s].[Id] = [s0].[ParentReferenceId]
ORDER BY [b].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], [s].[Id], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [s].[BaseParentId], [s].[Name], [s].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id], [s].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [b].[Id], [s].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [b].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator], [b].[Id], [s].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [s0] ON [s].[Id] = [s0].[ParentReferenceId]
ORDER BY [b].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse_split(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[BaseId], [s0].[Discriminator], [s0].[OwnedReferenceOnBase_Id], [s0].[OwnedReferenceOnBase_Name], [s0].[Id0], [s0].[OwnedReferenceOnDerived_Id], [s0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [n].[ParentReferenceId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [n].[Id], [s].[Id], [s0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
) AS [s] ON [n].[ParentReferenceId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s0].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [n].[Id], [s].[Id], [s0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
) AS [s] ON [n].[ParentReferenceId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[BaseParentId], [s0].[Name], [s0].[DerivedProperty], [s0].[Discriminator], [s0].[Id0], [s0].[Name0], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator0] AS [Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Name], [d2].[DerivedProperty], CASE
        WHEN [d2].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator], [s].[Id] AS [Id0], [s].[Name] AS [Name0], [s].[ParentCollectionId], [s].[ParentReferenceId], [s].[Discriminator] AS [Discriminator0]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d2] ON [b2].[Id] = [d2].[Id]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
            WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
        END AS [Discriminator]
        FROM [NestedReferences] AS [n]
        LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
    ) AS [s] ON [b2].[Id] = [s].[ParentCollectionId]
) AS [s0] ON [b].[Id] = [s0].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse_split(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
END AS [Discriminator], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[BaseId], [s0].[Discriminator], [s0].[OwnedReferenceOnBase_Id], [s0].[OwnedReferenceOnBase_Name], [s0].[Id0], [s0].[OwnedReferenceOnDerived_Id], [s0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [n].[Id], [s].[Id], [s0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s0].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [n].[Id], [s].[Id], [s0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Name], [d2].[DerivedProperty], CASE
        WHEN [d2].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d2] ON [b2].[Id] = [d2].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id], [s].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[Name], [s0].[ParentCollectionId], [s0].[ParentReferenceId], [s0].[Discriminator], [b].[Id], [s].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b2]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [s0] ON [s].[Id] = [s0].[ParentCollectionId]
ORDER BY [b].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse_split(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [s0].[Id], [s0].[Name], [s0].[BaseId], [s0].[Discriminator], [s0].[OwnedReferenceOnBase_Id], [s0].[OwnedReferenceOnBase_Name], [s0].[Id0], [s0].[OwnedReferenceOnDerived_Id], [s0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id0], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [n].[Id], [s].[Id], [s0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [s0].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
""",
            //
            """
SELECT [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name], [n].[Id], [s].[Id], [s0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [s] ON [n].[ParentCollectionId] = [s].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [s0] ON [s].[BaseParentId] = [s0].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [s0].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [s].[Id], [s0].[Id]
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
SELECT [b].[Id]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[BaseParentId], [s].[Name], [s].[DerivedProperty], [s].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast_split(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""",
            //
            """
SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Name], [s].[ParentId], [s].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b2].[Id], [b2].[Name], [b2].[ParentId], [d2].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b2]
    INNER JOIN [DerivedCollectionsOnDerived] AS [d2] ON [b2].[Id] = [d2].[Id]
) AS [s] ON [b].[Id] = [s].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""");
    }

    public override void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
    {
        base.Entity_can_make_separate_relationships_with_base_type_and_derived_type_both();

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class TPTRelationshipsQuerySqlServerFixture : TPTRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
