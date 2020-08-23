// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
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

        public override void Changes_in_derived_related_entities_are_detected()
        {
            base.Changes_in_derived_related_entities_are_detected();

            AssertSql(
                @"SELECT [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id0], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator]
FROM (
    SELECT TOP(2) [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id0], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
    WHERE [b].[Name] = N'Derived1(4)'
) AS [t]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [t0] ON [t].[Id] = [t0].[BaseParentId]
ORDER BY [t].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t0].[Id]");
        }

        public override async Task Include_collection_without_inheritance(bool async)
        {
            await base.Include_collection_without_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [c].[Id]");
        }

        public override async Task Include_collection_without_inheritance_reverse(bool async)
        {
            await base.Include_collection_without_inheritance_reverse(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [t].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_collection_without_inheritance_with_filter(bool async)
        {
            await base.Include_collection_without_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [c].[Id]");
        }

        public override async Task Include_collection_without_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_collection_without_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [c].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_collection_with_inheritance(bool async)
        {
            await base.Include_collection_with_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator]
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
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived1(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator]
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
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived2(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator]
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
) AS [t] ON [b].[Id] = [t].[ParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived3(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived3(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[Name], [b1].[ParentId], [d1].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b1]
    INNER JOIN [DerivedCollectionsOnDerived] AS [d1] ON [b1].[Id] = [d1].[Id]
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived_reverse(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [b].[ParentId], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnDerived'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN [DerivedCollectionsOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_collection_with_inheritance_reverse(bool async)
        {
            await base.Include_collection_with_inheritance_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator]
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
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_without_inheritance(bool async)
        {
            await base.Include_reference_without_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_reference_without_inheritance_on_derived1(bool async)
        {
            await base.Include_reference_without_inheritance_on_derived1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_reference_without_inheritance_on_derived2(bool async)
        {
            await base.Include_reference_without_inheritance_on_derived2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnDerived] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_reference_without_inheritance_on_derived_reverse(bool async)
        {
            await base.Include_reference_without_inheritance_on_derived_reverse(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [ReferencesOnDerived] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [r].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [t].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_reference_without_inheritance_reverse(bool async)
        {
            await base.Include_reference_without_inheritance_reverse(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [r].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [t].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_reference_without_inheritance_with_filter(bool async)
        {
            await base.Include_reference_without_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_reference_without_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_reference_without_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [r].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE ([r].[Name] <> N'Bar') OR [r].[Name] IS NULL
ORDER BY [r].[Id], [t].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id]");
        }

        public override async Task Include_reference_with_inheritance(bool async)
        {
            await base.Include_reference_with_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived1(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived2(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
    END AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b0]
    LEFT JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived4(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived4(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    INNER JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN [DerivedReferencesOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
    END AS [Discriminator]
    FROM [BaseReferencesOnDerived] AS [b0]
    LEFT JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter4(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    INNER JOIN [DerivedReferencesOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnDerived'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN [DerivedReferencesOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_with_filter(bool async)
        {
            await base.Include_reference_with_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Include_self_reference_with_inheritance(bool async)
        {
            await base.Include_self_reference_with_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [t].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [t].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id]");
        }

        public override async Task Include_self_reference_with_inheritance_reverse(bool async)
        {
            await base.Include_self_reference_with_inheritance_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id], [d2].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [d].[BaseId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [t].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d2] ON [t].[Id] = [d2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [d2].[DerivedInheritanceRelationshipEntityId], [d2].[Id]");
        }

        public override async Task Nested_include_collection_reference_on_non_entity_base(bool async)
        {
            await base.Nested_include_collection_reference_on_non_entity_base(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [t].[Id], [t].[Name], [t].[ReferenceId], [t].[ReferencedEntityId], [t].[Id0], [t].[Name0]
FROM [ReferencedEntities] AS [r]
LEFT JOIN (
    SELECT [p].[Id], [p].[Name], [p].[ReferenceId], [p].[ReferencedEntityId], [r0].[Id] AS [Id0], [r0].[Name] AS [Name0]
    FROM [PrincipalEntities] AS [p]
    LEFT JOIN [ReferencedEntities] AS [r0] ON [p].[ReferenceId] = [r0].[Id]
) AS [t] ON [r].[Id] = [t].[ReferencedEntityId]
ORDER BY [r].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t0].[Id0], [t0].[Name0], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator0]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator], [t].[Id] AS [Id0], [t].[Name] AS [Name0], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator] AS [Discriminator0]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
            WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
        END AS [Discriminator]
        FROM [NestedCollections] AS [n]
        LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
    ) AS [t] ON [b1].[Id] = [t].[ParentCollectionId]
) AS [t0] ON [b].[Id] = [t0].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[Id0], [t0].[OwnedReferenceOnBase_Id], [t0].[OwnedReferenceOnBase_Name], [t0].[Id1], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t0].[Id0], [t0].[Name0], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator0]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b1].[Id], [b1].[BaseParentId], [b1].[Name], [d1].[DerivedProperty], CASE
        WHEN [d1].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator], [t].[Id] AS [Id0], [t].[Name] AS [Name0], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator] AS [Discriminator0]
    FROM [BaseCollectionsOnBase] AS [b1]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d1] ON [b1].[Id] = [d1].[Id]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
            WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
        END AS [Discriminator]
        FROM [NestedReferences] AS [n]
        LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
    ) AS [t] ON [b1].[Id] = [t].[ParentCollectionId]
) AS [t0] ON [b].[Id] = [t0].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
END AS [Discriminator], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[Id0], [t0].[OwnedReferenceOnBase_Id], [t0].[OwnedReferenceOnBase_Name], [t0].[Id1], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [t0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_on_base(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [t0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[Id0], [t0].[OwnedReferenceOnBase_Id], [t0].[OwnedReferenceOnBase_Name], [t0].[Id1], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [n].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_reference(bool async)
        {
            await base.Nested_include_with_inheritance_reference_reference(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
    END AS [Discriminator]
    FROM [NestedReferences] AS [n]
    LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [t0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
        {
            await base.Nested_include_with_inheritance_reference_reference_on_base(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
    END AS [Discriminator]
    FROM [NestedReferences] AS [n]
    LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [t0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_reference_reference_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
END AS [Discriminator], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[Id0], [t0].[OwnedReferenceOnBase_Id], [t0].[OwnedReferenceOnBase_Name], [t0].[Id1], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [n].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id]");
        }

        public override async Task Collection_projection_on_base_type(bool async)
        {
            await base.Collection_projection_on_base_type(async);

            AssertSql(
                @"SELECT [b].[Id], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_split(bool async)
        {
            await base.Include_collection_with_inheritance_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_reverse_split(bool async)
        {
            await base.Include_collection_with_inheritance_reverse_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter_split(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter_reverse_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t] ON [b].[BaseParentId] = [t].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Include_collection_without_inheritance_split(bool async)
        {
            await base.Include_collection_without_inheritance_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_without_inheritance_reverse_split(bool async)
        {
            await base.Include_collection_without_inheritance_reverse_split(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [c].[ParentId] = [t].[Id]
ORDER BY [c].[Id], [t].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [c].[Id], [t].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [t] ON [c].[ParentId] = [t].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [t].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [c].[Id], [t].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [t] ON [c].[ParentId] = [t].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [t].[Id]");
        }

        public override async Task Include_collection_without_inheritance_with_filter_split(bool async)
        {
            await base.Include_collection_without_inheritance_with_filter_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
        {
            await base.Include_collection_without_inheritance_with_filter_reverse_split(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Name], [t].[BaseId], [t].[Discriminator], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id] AS [Id1], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [c].[ParentId] = [t].[Id]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [c].[Id], [t].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [t] ON [c].[ParentId] = [t].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [c].[Id], [t].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id]
    FROM [BaseEntities] AS [b]
) AS [t] ON [c].[ParentId] = [t].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [t].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived1_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived1_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived2_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived2_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [t].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[Name], [b0].[ParentId], [d0].[DerivedInheritanceRelationshipEntityId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnDerived'
    END AS [Discriminator]
    FROM [BaseCollectionsOnDerived] AS [b0]
    LEFT JOIN [DerivedCollectionsOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[ParentId]
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived3_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived3_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
INNER JOIN (
    SELECT [b0].[Id], [b0].[Name], [b0].[ParentId], [d0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b0]
    INNER JOIN [DerivedCollectionsOnDerived] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived_reverse_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [b].[ParentId], [d].[DerivedInheritanceRelationshipEntityId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnDerived'
END AS [Discriminator], [t].[Id], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[Id1], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN [DerivedCollectionsOnDerived] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[ParentId] = [t].[Id]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[ParentId] = [t].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    INNER JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[ParentId] = [t].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_split(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [t].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id], [t].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator], [b].[Id], [t].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b0]
    LEFT JOIN [DerivedReferencesOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [t].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [b].[Id], [t].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [b].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator], [b].[Id], [t].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b0]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_reverse_split(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[Id0], [t0].[OwnedReferenceOnBase_Id], [t0].[OwnedReferenceOnBase_Name], [t0].[Id1], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedReferenceOnBase'
    END AS [Discriminator]
    FROM [BaseReferencesOnBase] AS [b]
    LEFT JOIN [DerivedReferencesOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [n].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [t].[Id], [t0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
) AS [t] ON [n].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [n].[Id], [t].[Id], [t0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseReferencesOnBase] AS [b]
) AS [t] ON [n].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[BaseParentId], [t0].[Name], [t0].[DerivedProperty], [t0].[Discriminator], [t0].[Id0], [t0].[Name0], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator0] AS [Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator], [t].[Id] AS [Id0], [t].[Name] AS [Name0], [t].[ParentCollectionId], [t].[ParentReferenceId], [t].[Discriminator] AS [Discriminator0]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
            WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
        END AS [Discriminator]
        FROM [NestedReferences] AS [n]
        LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
    ) AS [t] ON [b0].[Id] = [t].[ParentCollectionId]
) AS [t0] ON [b].[Id] = [t0].[BaseParentId]
ORDER BY [b].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference_reverse_split(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedReferenceDerived'
END AS [Discriminator], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[Id0], [t0].[OwnedReferenceOnBase_Id], [t0].[OwnedReferenceOnBase_Name], [t0].[Id1], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [NestedReferencesDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [t].[Id], [t0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [n].[Id], [t].[Id], [t0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Name], [d].[BaseId], CASE
    WHEN [d].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
END AS [Discriminator], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [d].[Id], [d].[OwnedReferenceOnDerived_Id], [d].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [DerivedEntities] AS [d] ON [b].[Id] = [d].[Id]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [d0].[DerivedInheritanceRelationshipEntityId], [d0].[Id], [d0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d0] ON [b].[Id] = [d0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId], [t0].[Discriminator], [b].[Id], [t].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b0]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
INNER JOIN (
    SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
        WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
    END AS [Discriminator]
    FROM [NestedCollections] AS [n]
    LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
) AS [t0] ON [t].[Id] = [t0].[ParentCollectionId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection_reverse_split(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], CASE
    WHEN [n0].[Id] IS NOT NULL THEN N'NestedCollectionDerived'
END AS [Discriminator], [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [t0].[Id], [t0].[Name], [t0].[BaseId], [t0].[Discriminator], [t0].[Id0], [t0].[OwnedReferenceOnBase_Id], [t0].[OwnedReferenceOnBase_Name], [t0].[Id1], [t0].[OwnedReferenceOnDerived_Id], [t0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [NestedCollectionsDerived] AS [n0] ON [n].[Id] = [n0].[Id]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Name], [d].[DerivedProperty], CASE
        WHEN [d].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d] ON [b].[Id] = [d].[Id]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Name], [d0].[BaseId], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedInheritanceRelationshipEntity'
    END AS [Discriminator], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [d0].[Id] AS [Id1], [d0].[OwnedReferenceOnDerived_Id], [d0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    LEFT JOIN [DerivedEntities] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [t].[Id], [t0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]",
                //
                @"SELECT [d1].[DerivedInheritanceRelationshipEntityId], [d1].[Id], [d1].[Name], [n].[Id], [t].[Id], [t0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId]
    FROM [BaseCollectionsOnBase] AS [b]
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
INNER JOIN [DerivedEntities_OwnedCollectionOnDerived] AS [d1] ON [t0].[Id] = [d1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [t].[Id], [t0].[Id]");
        }

        public override async Task Nested_include_collection_reference_on_non_entity_base_split(bool async)
        {
            await base.Nested_include_collection_reference_on_non_entity_base_split(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name]
FROM [ReferencedEntities] AS [r]
ORDER BY [r].[Id]",
                //
                @"SELECT [t].[Id], [t].[Name], [t].[ReferenceId], [t].[ReferencedEntityId], [t].[Id0], [t].[Name0], [r].[Id]
FROM [ReferencedEntities] AS [r]
INNER JOIN (
    SELECT [p].[Id], [p].[Name], [p].[ReferenceId], [p].[ReferencedEntityId], [r0].[Id] AS [Id0], [r0].[Name] AS [Name0]
    FROM [PrincipalEntities] AS [p]
    LEFT JOIN [ReferencedEntities] AS [r0] ON [p].[ReferenceId] = [r0].[Id]
) AS [t] ON [r].[Id] = [t].[ReferencedEntityId]
ORDER BY [r].[Id]");
        }

        public override async Task Collection_projection_on_base_type_split(bool async)
        {
            await base.Collection_projection_on_base_type_split(async);

            AssertSql(
                @"SELECT [b].[Id]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[BaseParentId], [t].[Name], [t].[DerivedProperty], [t].[Discriminator], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Name], [d0].[DerivedProperty], CASE
        WHEN [d0].[Id] IS NOT NULL THEN N'DerivedCollectionOnBase'
    END AS [Discriminator]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [DerivedCollectionsOnBase] AS [d0] ON [b0].[Id] = [d0].[Id]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class TPTRelationshipsQuerySqlServerFixture : TPTRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }
}
