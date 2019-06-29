// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceRelationshipsQuerySqlServerTest
        : InheritanceRelationshipsQueryTestBase<InheritanceRelationshipsQuerySqlServerFixture>
    {
        public InheritanceRelationshipsQuerySqlServerTest(
            InheritanceRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Include_reference_with_inheritance1()
        {
            base.Include_reference_with_inheritance1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')");
        }

        public override void Include_reference_with_inheritance_reverse()
        {
            base.Include_reference_with_inheritance_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [b].[BaseParentId] = [t].[Id]
WHERE [b].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')");
        }

        public override void Include_self_reference_with_inheritance()
        {
            base.Include_self_reference_with_inheritance();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[Id] = [t].[BaseId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')");
        }

        public override void Include_self_reference_with_inheritance_reverse()
        {
            base.Include_self_reference_with_inheritance_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [b].[BaseId] = [t].[Id]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_with_filter1()
        {
            base.Include_reference_with_inheritance_with_filter1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [b].[BaseParentId] = [t].[Id]
WHERE [b].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_without_inheritance()
        {
            base.Include_reference_without_inheritance();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')");
        }

        public override void Include_reference_without_inheritance_reverse()
        {
            base.Include_reference_without_inheritance_reverse();

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [r].[ParentId] = [t].[Id]");
        }

        public override void Include_reference_without_inheritance_with_filter()
        {
            base.Include_reference_without_inheritance_with_filter();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_without_inheritance_with_filter_reverse()
        {
            base.Include_reference_without_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [r].[ParentId] = [t].[Id]
WHERE ([r].[Name] <> N'Bar') OR [r].[Name] IS NULL");
        }

        public override void Include_collection_with_inheritance1()
        {
            base.Include_collection_with_inheritance1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
    FROM [BaseCollectionsOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
ORDER BY [b].[Id], [t].[Id]");
        }

        public override void Include_collection_with_inheritance_reverse()
        {
            base.Include_collection_with_inheritance_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [b].[BaseParentId] = [t].[Id]
WHERE [b].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')");
        }

        public override void Include_collection_with_inheritance_with_filter1()
        {
            base.Include_collection_with_inheritance_with_filter1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
    FROM [BaseCollectionsOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [t].[Id]");
        }

        public override void Include_collection_with_inheritance_with_filter_reverse()
        {
            base.Include_collection_with_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [b].[BaseParentId] = [t].[Id]
WHERE [b].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_collection_without_inheritance()
        {
            base.Include_collection_without_inheritance();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
ORDER BY [b].[Id], [c].[Id]");
        }

        public override void Include_collection_without_inheritance_reverse()
        {
            base.Include_collection_without_inheritance_reverse();

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [c].[ParentId] = [t].[Id]");
        }

        public override void Include_collection_without_inheritance_with_filter()
        {
            base.Include_collection_without_inheritance_with_filter();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [c].[Id]");
        }

        public override void Include_collection_without_inheritance_with_filter_reverse()
        {
            base.Include_collection_without_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN (
    SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t] ON [c].[ParentId] = [t].[Id]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL");
        }

        public override void Include_reference_with_inheritance_on_derived1()
        {
            base.Include_reference_with_inheritance_on_derived1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_on_derived2()
        {
            base.Include_reference_with_inheritance_on_derived2();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnDerived', N'DerivedReferenceOnDerived')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_on_derived4()
        {
            base.Include_reference_with_inheritance_on_derived4();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_on_derived_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[BaseParentId] = [t].[Id]
WHERE [b].[Discriminator] IN (N'BaseReferenceOnDerived', N'DerivedReferenceOnDerived')");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter1()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE ([b].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter2()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter2();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnDerived', N'DerivedReferenceOnDerived')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE ([b].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter4()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter4();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
WHERE ([b].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[BaseParentId] = [t].[Id]
WHERE [b].[Discriminator] IN (N'BaseReferenceOnDerived', N'DerivedReferenceOnDerived') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_without_inheritance_on_derived1()
        {
            base.Include_reference_without_inheritance_on_derived1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_without_inheritance_on_derived2()
        {
            base.Include_reference_without_inheritance_on_derived2();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnDerived] AS [r] ON [b].[Id] = [r].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_without_inheritance_on_derived_reverse()
        {
            base.Include_reference_without_inheritance_on_derived_reverse();

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [ReferencesOnDerived] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [r].[ParentId] = [t].[Id]");
        }

        public override void Include_collection_with_inheritance_on_derived1()
        {
            base.Include_collection_with_inheritance_on_derived1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
    FROM [BaseCollectionsOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [t].[Id]");
        }

        public override void Include_collection_with_inheritance_on_derived2()
        {
            base.Include_collection_with_inheritance_on_derived2();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[ParentId], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseCollectionOnDerived', N'DerivedCollectionOnDerived')
) AS [t] ON [b].[Id] = [t].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [t].[Id]");
        }

        public override void Include_collection_with_inheritance_on_derived3()
        {
            base.Include_collection_with_inheritance_on_derived3();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[ParentId], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[ParentId], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedCollectionOnDerived'
) AS [t] ON [b].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [t].[Id]");
        }

        public override void Include_collection_with_inheritance_on_derived_reverse()
        {
            base.Include_collection_with_inheritance_on_derived_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[ParentId], [b].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [b].[ParentId] = [t].[Id]
WHERE [b].[Discriminator] IN (N'BaseCollectionOnDerived', N'DerivedCollectionOnDerived')");
        }

        public override void Nested_include_with_inheritance_reference_reference1()
        {
            base.Nested_include_with_inheritance_reference_reference1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [NestedReferences] AS [n]
    WHERE [n].[Discriminator] IN (N'NestedReferenceBase', N'NestedReferenceDerived')
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')");
        }

        public override void Nested_include_with_inheritance_reference_reference3()
        {
            base.Nested_include_with_inheritance_reference_reference3();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [NestedReferences] AS [n]
    WHERE [n].[Discriminator] IN (N'NestedReferenceBase', N'NestedReferenceDerived')
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Nested_include_with_inheritance_reference_reference_reverse()
        {
            base.Nested_include_with_inheritance_reference_reference_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedReferences] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name]
    FROM [BaseReferencesOnBase] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [n].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [n].[Discriminator] IN (N'NestedReferenceBase', N'NestedReferenceDerived')");
        }

        public override void Nested_include_with_inheritance_reference_collection1()
        {
            base.Nested_include_with_inheritance_reference_collection1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [NestedCollections] AS [n]
    WHERE [n].[Discriminator] IN (N'NestedCollectionBase', N'NestedCollectionDerived')
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
ORDER BY [b].[Id], [t0].[Id]");
        }

        public override void Nested_include_with_inheritance_reference_collection3()
        {
            base.Nested_include_with_inheritance_reference_collection3();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
    FROM [BaseReferencesOnBase] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [b].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [NestedCollections] AS [n]
    WHERE [n].[Discriminator] IN (N'NestedCollectionBase', N'NestedCollectionDerived')
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [t0].[Id]");
        }

        public override void Nested_include_with_inheritance_reference_collection_reverse()
        {
            base.Nested_include_with_inheritance_reference_collection_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name]
    FROM [BaseReferencesOnBase] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseReferenceOnBase', N'DerivedReferenceOnBase')
) AS [t] ON [n].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [n].[Discriminator] IN (N'NestedCollectionBase', N'NestedCollectionDerived')");
        }

        public override void Nested_include_with_inheritance_collection_reference1()
        {
            base.Nested_include_with_inheritance_collection_reference1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t0].[Id], [t0].[BaseParentId], [t0].[Discriminator], [t0].[Name], [t0].[DerivedProperty], [t0].[Id0], [t0].[Discriminator0], [t0].[Name0], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [t].[Id] AS [Id0], [t].[Discriminator] AS [Discriminator0], [t].[Name] AS [Name0], [t].[ParentCollectionId], [t].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
        FROM [NestedReferences] AS [n]
        WHERE [n].[Discriminator] IN (N'NestedReferenceBase', N'NestedReferenceDerived')
    ) AS [t] ON [b0].[Id] = [t].[ParentCollectionId]
    WHERE [b0].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')
) AS [t0] ON [b].[Id] = [t0].[BaseParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
ORDER BY [b].[Id], [t0].[Id]");
        }

        public override void Nested_include_with_inheritance_collection_reference_reverse()
        {
            base.Nested_include_with_inheritance_collection_reference_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedReferences] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty]
    FROM [BaseCollectionsOnBase] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [n].[Discriminator] IN (N'NestedReferenceBase', N'NestedReferenceDerived')");
        }

        public override void Nested_include_with_inheritance_collection_collection1()
        {
            base.Nested_include_with_inheritance_collection_collection1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t0].[Id], [t0].[BaseParentId], [t0].[Discriminator], [t0].[Name], [t0].[DerivedProperty], [t0].[Id0], [t0].[Discriminator0], [t0].[Name0], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [t].[Id] AS [Id0], [t].[Discriminator] AS [Discriminator0], [t].[Name] AS [Name0], [t].[ParentCollectionId], [t].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN (
        SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
        FROM [NestedCollections] AS [n]
        WHERE [n].[Discriminator] IN (N'NestedCollectionBase', N'NestedCollectionDerived')
    ) AS [t] ON [b0].[Id] = [t].[ParentCollectionId]
    WHERE [b0].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')
) AS [t0] ON [b].[Id] = [t0].[BaseParentId]
WHERE [b].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
ORDER BY [b].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override void Nested_include_with_inheritance_collection_collection_reverse()
        {
            base.Nested_include_with_inheritance_collection_collection_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedCollections] AS [n]
LEFT JOIN (
    SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty]
    FROM [BaseCollectionsOnBase] AS [b]
    WHERE [b].[Discriminator] IN (N'BaseCollectionOnBase', N'DerivedCollectionOnBase')
) AS [t] ON [n].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] IN (N'BaseInheritanceRelationshipEntity', N'DerivedInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [n].[Discriminator] IN (N'NestedCollectionBase', N'NestedCollectionDerived')");
        }

        public override void Nested_include_collection_reference_on_non_entity_base()
        {
            base.Nested_include_collection_reference_on_non_entity_base();

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [t].[Id], [t].[Name], [t].[ReferenceId], [t].[ReferencedEntityId], [t].[Id0], [t].[Name0]
FROM [ReferencedEntities] AS [r]
LEFT JOIN (
    SELECT [p].[Id], [p].[Name], [p].[ReferenceId], [p].[ReferencedEntityId], [r0].[Id] AS [Id0], [r0].[Name] AS [Name0]
    FROM [PrincipalEntities] AS [p]
    LEFT JOIN [ReferencedEntities] AS [r0] ON [p].[ReferenceId] = [r0].[Id]
) AS [t] ON [r].[Id] = [t].[ReferencedEntityId]
ORDER BY [r].[Id], [t].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
