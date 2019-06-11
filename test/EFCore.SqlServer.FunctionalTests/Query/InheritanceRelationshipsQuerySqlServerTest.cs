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

        public override void Include_self_refence_with_inheritence()
        {
            base.Include_self_refence_with_inheritence();

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

        public override void Include_self_refence_with_inheritence_reverse()
        {
            base.Include_self_refence_with_inheritence_reverse();

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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
ORDER BY [e].[Id]",
                //
                @"SELECT [e.BaseCollectionOnBase].[Id], [e.BaseCollectionOnBase].[BaseParentId], [e.BaseCollectionOnBase].[Discriminator], [e.BaseCollectionOnBase].[Name], [e.BaseCollectionOnBase].[DerivedProperty]
FROM [BaseCollectionsOnBase] AS [e.BaseCollectionOnBase]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e.BaseCollectionOnBase].[BaseParentId] = [t].[Id]
WHERE [e.BaseCollectionOnBase].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
ORDER BY [t].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)
ORDER BY [e].[Id]",
                //
                @"SELECT [e.BaseCollectionOnBase].[Id], [e.BaseCollectionOnBase].[BaseParentId], [e.BaseCollectionOnBase].[Discriminator], [e.BaseCollectionOnBase].[Name], [e.BaseCollectionOnBase].[DerivedProperty]
FROM [BaseCollectionsOnBase] AS [e.BaseCollectionOnBase]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity') AND (([e0].[Name] <> N'Bar') OR [e0].[Name] IS NULL)
) AS [t] ON [e.BaseCollectionOnBase].[BaseParentId] = [t].[Id]
WHERE [e.BaseCollectionOnBase].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
ORDER BY [t].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
ORDER BY [e].[Id]",
                //
                @"SELECT [e.CollectionOnBase].[Id], [e.CollectionOnBase].[Name], [e.CollectionOnBase].[ParentId]
FROM [CollectionsOnBase] AS [e.CollectionOnBase]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e.CollectionOnBase].[ParentId] = [t].[Id]
ORDER BY [t].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)
ORDER BY [e].[Id]",
                //
                @"SELECT [e.CollectionOnBase].[Id], [e.CollectionOnBase].[Name], [e.CollectionOnBase].[ParentId]
FROM [CollectionsOnBase] AS [e.CollectionOnBase]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity') AND (([e0].[Name] <> N'Bar') OR [e0].[Name] IS NULL)
) AS [t] ON [e.CollectionOnBase].[ParentId] = [t].[Id]
ORDER BY [t].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [e].[Id]",
                //
                @"SELECT [e.BaseCollectionOnBase].[Id], [e.BaseCollectionOnBase].[BaseParentId], [e.BaseCollectionOnBase].[Discriminator], [e.BaseCollectionOnBase].[Name], [e.BaseCollectionOnBase].[DerivedProperty]
FROM [BaseCollectionsOnBase] AS [e.BaseCollectionOnBase]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e.BaseCollectionOnBase].[BaseParentId] = [t].[Id]
WHERE [e.BaseCollectionOnBase].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
ORDER BY [t].[Id]");
        }

        public override void Include_collection_with_inheritance_on_derived2()
        {
            base.Include_collection_with_inheritance_on_derived2();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [e].[Id]",
                //
                @"SELECT [e.BaseCollectionOnDerived].[Id], [e.BaseCollectionOnDerived].[Discriminator], [e.BaseCollectionOnDerived].[Name], [e.BaseCollectionOnDerived].[ParentId], [e.BaseCollectionOnDerived].[DerivedInheritanceRelationshipEntityId]
FROM [BaseCollectionsOnDerived] AS [e.BaseCollectionOnDerived]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e.BaseCollectionOnDerived].[ParentId] = [t].[Id]
WHERE [e.BaseCollectionOnDerived].[Discriminator] IN (N'DerivedCollectionOnDerived', N'BaseCollectionOnDerived')
ORDER BY [t].[Id]");
        }

        public override void Include_collection_with_inheritance_on_derived3()
        {
            base.Include_collection_with_inheritance_on_derived3();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [e].[Id]",
                //
                @"SELECT [e.DerivedCollectionOnDerived].[Id], [e.DerivedCollectionOnDerived].[Discriminator], [e.DerivedCollectionOnDerived].[Name], [e.DerivedCollectionOnDerived].[ParentId], [e.DerivedCollectionOnDerived].[DerivedInheritanceRelationshipEntityId]
FROM [BaseCollectionsOnDerived] AS [e.DerivedCollectionOnDerived]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e.DerivedCollectionOnDerived].[DerivedInheritanceRelationshipEntityId] = [t].[Id]
WHERE [e.DerivedCollectionOnDerived].[Discriminator] = N'DerivedCollectionOnDerived'
ORDER BY [t].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
ORDER BY [t].[Id]",
                //
                @"SELECT [e.BaseReferenceOnBase.NestedCollection].[Id], [e.BaseReferenceOnBase.NestedCollection].[Discriminator], [e.BaseReferenceOnBase.NestedCollection].[Name], [e.BaseReferenceOnBase.NestedCollection].[ParentCollectionId], [e.BaseReferenceOnBase.NestedCollection].[ParentReferenceId]
FROM [NestedCollections] AS [e.BaseReferenceOnBase.NestedCollection]
INNER JOIN (
    SELECT DISTINCT [t0].[Id]
    FROM [BaseEntities] AS [e0]
    LEFT JOIN (
        SELECT [e.BaseReferenceOnBase0].*
        FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase0]
        WHERE [e.BaseReferenceOnBase0].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
    ) AS [t0] ON [e0].[Id] = [t0].[BaseParentId]
    WHERE [e0].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t1] ON [e.BaseReferenceOnBase.NestedCollection].[ParentReferenceId] = [t1].[Id]
WHERE [e.BaseReferenceOnBase.NestedCollection].[Discriminator] IN (N'NestedCollectionDerived', N'NestedCollectionBase')
ORDER BY [t1].[Id]");
        }

        public override void Nested_include_with_inheritance_reference_collection3()
        {
            base.Nested_include_with_inheritance_reference_collection3();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [t].[Id]",
                //
                @"SELECT [e.BaseReferenceOnBase.NestedCollection].[Id], [e.BaseReferenceOnBase.NestedCollection].[Discriminator], [e.BaseReferenceOnBase.NestedCollection].[Name], [e.BaseReferenceOnBase.NestedCollection].[ParentCollectionId], [e.BaseReferenceOnBase.NestedCollection].[ParentReferenceId]
FROM [NestedCollections] AS [e.BaseReferenceOnBase.NestedCollection]
INNER JOIN (
    SELECT DISTINCT [t0].[Id]
    FROM [BaseEntities] AS [e0]
    LEFT JOIN (
        SELECT [e.BaseReferenceOnBase0].*
        FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase0]
        WHERE [e.BaseReferenceOnBase0].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
    ) AS [t0] ON [e0].[Id] = [t0].[BaseParentId]
    WHERE [e0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t1] ON [e.BaseReferenceOnBase.NestedCollection].[ParentReferenceId] = [t1].[Id]
WHERE [e.BaseReferenceOnBase.NestedCollection].[Discriminator] IN (N'NestedCollectionDerived', N'NestedCollectionBase')
ORDER BY [t1].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
ORDER BY [e].[Id]",
                //
                @"SELECT [e.BaseCollectionOnBase].[Id], [e.BaseCollectionOnBase].[BaseParentId], [e.BaseCollectionOnBase].[Discriminator], [e.BaseCollectionOnBase].[Name], [e.BaseCollectionOnBase].[DerivedProperty], [t].[Id], [t].[Discriminator], [t].[Name], [t].[ParentCollectionId], [t].[ParentReferenceId]
FROM [BaseCollectionsOnBase] AS [e.BaseCollectionOnBase]
LEFT JOIN (
    SELECT [b.NestedReference].*
    FROM [NestedReferences] AS [b.NestedReference]
    WHERE [b.NestedReference].[Discriminator] IN (N'NestedReferenceDerived', N'NestedReferenceBase')
) AS [t] ON [e.BaseCollectionOnBase].[Id] = [t].[ParentCollectionId]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t0] ON [e.BaseCollectionOnBase].[BaseParentId] = [t0].[Id]
WHERE [e.BaseCollectionOnBase].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
ORDER BY [t0].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId]
FROM [BaseEntities] AS [e]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
ORDER BY [e].[Id]",
                //
                @"SELECT [e.BaseCollectionOnBase].[Id], [e.BaseCollectionOnBase].[BaseParentId], [e.BaseCollectionOnBase].[Discriminator], [e.BaseCollectionOnBase].[Name], [e.BaseCollectionOnBase].[DerivedProperty]
FROM [BaseCollectionsOnBase] AS [e.BaseCollectionOnBase]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [BaseEntities] AS [e0]
    WHERE [e0].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e.BaseCollectionOnBase].[BaseParentId] = [t].[Id]
WHERE [e.BaseCollectionOnBase].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
ORDER BY [t].[Id], [e.BaseCollectionOnBase].[Id]",
                //
                @"SELECT [e.BaseCollectionOnBase.NestedCollection].[Id], [e.BaseCollectionOnBase.NestedCollection].[Discriminator], [e.BaseCollectionOnBase.NestedCollection].[Name], [e.BaseCollectionOnBase.NestedCollection].[ParentCollectionId], [e.BaseCollectionOnBase.NestedCollection].[ParentReferenceId]
FROM [NestedCollections] AS [e.BaseCollectionOnBase.NestedCollection]
INNER JOIN (
    SELECT DISTINCT [e.BaseCollectionOnBase0].[Id], [t0].[Id] AS [Id0]
    FROM [BaseCollectionsOnBase] AS [e.BaseCollectionOnBase0]
    INNER JOIN (
        SELECT [e1].[Id]
        FROM [BaseEntities] AS [e1]
        WHERE [e1].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
    ) AS [t0] ON [e.BaseCollectionOnBase0].[BaseParentId] = [t0].[Id]
    WHERE [e.BaseCollectionOnBase0].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
) AS [t1] ON [e.BaseCollectionOnBase.NestedCollection].[ParentCollectionId] = [t1].[Id]
WHERE [e.BaseCollectionOnBase.NestedCollection].[Discriminator] IN (N'NestedCollectionDerived', N'NestedCollectionBase')
ORDER BY [t1].[Id0], [t1].[Id]");
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
                @"SELECT [e].[Id], [e].[Name]
FROM [ReferencedEntities] AS [e]
ORDER BY [e].[Id]",
                //
                @"SELECT [e.Principals].[Id], [e.Principals].[Name], [e.Principals].[ReferenceId], [e.Principals].[ReferencedEntityId], [p.Reference].[Id], [p.Reference].[Name]
FROM [PrincipalEntities] AS [e.Principals]
LEFT JOIN [ReferencedEntities] AS [p.Reference] ON [e.Principals].[ReferenceId] = [p.Reference].[Id]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [ReferencedEntities] AS [e0]
) AS [t] ON [e.Principals].[ReferencedEntityId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
