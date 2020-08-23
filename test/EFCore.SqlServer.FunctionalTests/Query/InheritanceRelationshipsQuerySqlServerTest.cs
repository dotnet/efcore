// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceRelationshipsQuerySqlServerTest
        : InheritanceRelationshipsQueryRelationalTestBase<
            InheritanceRelationshipsQuerySqlServerTest.InheritanceRelationshipsQuerySqlServerFixture>
    {
        public InheritanceRelationshipsQuerySqlServerTest(
            InheritanceRelationshipsQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override async Task Include_reference_with_inheritance(bool async)
        {
            await base.Include_reference_with_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_self_reference_with_inheritance(bool async)
        {
            await base.Include_self_reference_with_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[Id] = [t].[BaseId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [t].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [t].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id]");
        }

        public override async Task Include_self_reference_with_inheritance_reverse(bool async)
        {
            await base.Include_self_reference_with_inheritance_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b0].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b0].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id]");
        }

        public override async Task Include_reference_with_inheritance_with_filter(bool async)
        {
            await base.Include_reference_with_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_without_inheritance(bool async)
        {
            await base.Include_reference_without_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_reference_without_inheritance_reverse(bool async)
        {
            await base.Include_reference_without_inheritance_reverse(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN [BaseEntities] AS [b] ON [r].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_reference_without_inheritance_with_filter(bool async)
        {
            await base.Include_reference_without_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_reference_without_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_reference_without_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN [BaseEntities] AS [b] ON [r].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE ([r].[Name] <> N'Bar') OR [r].[Name] IS NULL
ORDER BY [r].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_collection_with_inheritance(bool async)
        {
            await base.Include_collection_with_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnBase] AS [b2] ON [b].[Id] = [b2].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b2].[Id]");
        }

        public override async Task Include_collection_with_inheritance_reverse(bool async)
        {
            await base.Include_collection_with_inheritance_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnBase] AS [b2] ON [b].[Id] = [b2].[BaseParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b2].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_collection_without_inheritance(bool async)
        {
            await base.Include_collection_without_inheritance(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [c].[Id]");
        }

        public override async Task Include_collection_without_inheritance_reverse(bool async)
        {
            await base.Include_collection_without_inheritance_reverse(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_collection_without_inheritance_with_filter(bool async)
        {
            await base.Include_collection_without_inheritance_with_filter(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [c].[Id]");
        }

        public override async Task Include_collection_without_inheritance_with_filter_reverse(bool async)
        {
            await base.Include_collection_without_inheritance_with_filter_reverse(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived1(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived2(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnDerived] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived4(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived4(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [t].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnDerived] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter4(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
        {
            await base.Include_reference_with_inheritance_on_derived_with_filter_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[BaseParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [t].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Include_reference_without_inheritance_on_derived1(bool async)
        {
            await base.Include_reference_without_inheritance_on_derived1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_reference_without_inheritance_on_derived2(bool async)
        {
            await base.Include_reference_without_inheritance_on_derived2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Id], [r].[Name], [r].[ParentId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnDerived] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_reference_without_inheritance_on_derived_reverse(bool async)
        {
            await base.Include_reference_without_inheritance_on_derived_reverse(async);

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name]
FROM [ReferencesOnDerived] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[Id] AS [Id0], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [r].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [t].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [t].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [t].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived1(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived1(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnBase] AS [b2] ON [b].[Id] = [b2].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b2].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived2(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived2(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[Id], [b2].[Discriminator], [b2].[Name], [b2].[ParentId], [b2].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnDerived] AS [b2] ON [b].[Id] = [b2].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b2].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived3(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived3(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [t].[Id], [t].[Discriminator], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b2].[Id], [b2].[Discriminator], [b2].[Name], [b2].[ParentId], [b2].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b2]
    WHERE [b2].[Discriminator] = N'DerivedCollectionOnDerived'
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [t].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived_reverse(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived_reverse(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[ParentId], [b].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[ParentId] = [t].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [t].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_reference(bool async)
        {
            await base.Nested_include_with_inheritance_reference_reference(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [n].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
        {
            await base.Nested_include_with_inheritance_reference_reference_on_base(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [n].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_reference_reference_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [n].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_on_base(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [n].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t].[Id0], [t].[Discriminator0], [t].[Name0], [t].[ParentCollectionId], [t].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [NestedReferences] AS [n] ON [b2].[Id] = [n].[ParentCollectionId]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t].[Id0], [t].[Discriminator0], [t].[Name0], [t].[ParentCollectionId], [t].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [NestedCollections] AS [n] ON [b2].[Id] = [n].[ParentCollectionId]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection_reverse(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]");
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

        public override async Task Collection_projection_on_base_type(bool async)
        {
            await base.Collection_projection_on_base_type(async);

            AssertSql(
                @"SELECT [b].[Id], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override async Task Include_collection_with_inheritance_split(bool async)
        {
            await base.Include_collection_with_inheritance_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_reverse_split(bool async)
        {
            await base.Include_collection_with_inheritance_reverse_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b0].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter_split(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
        {
            await base.Include_collection_with_inheritance_with_filter_reverse_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b0].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override async Task Include_collection_without_inheritance_split(bool async)
        {
            await base.Include_collection_without_inheritance_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
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
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
ORDER BY [c].[Id], [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [b].[Id]");
        }

        public override async Task Include_collection_without_inheritance_with_filter_split(bool async)
        {
            await base.Include_collection_without_inheritance_with_filter_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
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
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived1_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived1_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived2_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived2_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[ParentId], [b0].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnDerived] AS [b0] ON [b].[Id] = [b0].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived3_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived3_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[ParentId], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedCollectionOnDerived'
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]");
        }

        public override async Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
        {
            await base.Include_collection_with_inheritance_on_derived_reverse_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[ParentId], [b].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId], [t].[Id0], [t].[OwnedReferenceOnBase_Id], [t].[OwnedReferenceOnBase_Name], [t].[OwnedReferenceOnDerived_Id], [t].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[Id] AS [Id0], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[ParentId] = [t].[Id]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[ParentId] = [t].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [t].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [t].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[ParentId] = [t].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [t].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_split(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
        {
            await base.Nested_include_with_inheritance_reference_collection_reverse_split(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b0].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t].[Id0], [t].[Discriminator0], [t].[Name0], [t].[ParentCollectionId], [t].[ParentReferenceId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentCollectionId]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_reference_reverse_split(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b0].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection_split(async);

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[DerivedInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b0] ON [b].[Id] = [b0].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]",
                //
                @"SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id], [b0].[Id]",
                //
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentCollectionId]
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override async Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
        {
            await base.Nested_include_with_inheritance_collection_collection_reverse_split(async);

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]",
                //
                @"SELECT [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b0].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]");
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
                @"SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class InheritanceRelationshipsQuerySqlServerFixture : InheritanceRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }
}
