// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceRelationshipsQuerySqlServerTest
        : InheritanceRelationshipsQueryTestBase<InheritanceRelationshipsQuerySqlServerFixture>
    {
        public InheritanceRelationshipsQuerySqlServerTest(InheritanceRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Include_reference_with_inheritance1()
        {
            base.Include_reference_with_inheritance1();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')");
        }

        public override void Include_reference_with_inheritance_reverse()
        {
            base.Include_reference_with_inheritance_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[BaseParentId], [e].[Discriminator], [e].[Name], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnBase] AS [e]
LEFT JOIN (
    SELECT [e.BaseParent].*
    FROM [BaseEntities] AS [e.BaseParent]
    WHERE [e.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[BaseParentId] = [t].[Id]
WHERE [e].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')");
        }

        public override void Include_self_refence_with_inheritence()
        {
            base.Include_self_refence_with_inheritence();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.DerivedSefReferenceOnBase].*
    FROM [BaseEntities] AS [e.DerivedSefReferenceOnBase]
    WHERE [e.DerivedSefReferenceOnBase].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e].[Id] = [t].[BaseId]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')");
        }

        public override void Include_self_refence_with_inheritence_reverse()
        {
            base.Include_self_refence_with_inheritence_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseSelfRerefenceOnDerived].*
    FROM [BaseEntities] AS [e.BaseSelfRerefenceOnDerived]
    WHERE [e.BaseSelfRerefenceOnDerived].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[BaseId] = [t].[Id]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_with_filter1()
        {
            base.Include_reference_with_inheritance_with_filter1();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[BaseParentId], [e].[Discriminator], [e].[Name], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnBase] AS [e]
LEFT JOIN (
    SELECT [e.BaseParent].*
    FROM [BaseEntities] AS [e.BaseParent]
    WHERE [e.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[BaseParentId] = [t].[Id]
WHERE [e].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
        }

        public override void Include_reference_without_inheritance()
        {
            base.Include_reference_without_inheritance();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [e.ReferenceOnBase].[Id], [e.ReferenceOnBase].[Name], [e.ReferenceOnBase].[ParentId]
FROM [BaseEntities] AS [e]
LEFT JOIN [ReferencesOnBase] AS [e.ReferenceOnBase] ON [e].[Id] = [e.ReferenceOnBase].[ParentId]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')");
        }

        public override void Include_reference_without_inheritance_reverse()
        {
            base.Include_reference_without_inheritance_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [ReferencesOnBase] AS [e]
LEFT JOIN (
    SELECT [e.Parent].*
    FROM [BaseEntities] AS [e.Parent]
    WHERE [e.Parent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[ParentId] = [t].[Id]");
        }

        public override void Include_reference_without_inheritance_with_filter()
        {
            base.Include_reference_without_inheritance_with_filter();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [e.ReferenceOnBase].[Id], [e.ReferenceOnBase].[Name], [e.ReferenceOnBase].[ParentId]
FROM [BaseEntities] AS [e]
LEFT JOIN [ReferencesOnBase] AS [e.ReferenceOnBase] ON [e].[Id] = [e.ReferenceOnBase].[ParentId]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
        }

        public override void Include_reference_without_inheritance_with_filter_reverse()
        {
            base.Include_reference_without_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [ReferencesOnBase] AS [e]
LEFT JOIN (
    SELECT [e.Parent].*
    FROM [BaseEntities] AS [e.Parent]
    WHERE [e.Parent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[ParentId] = [t].[Id]
WHERE ([e].[Name] <> N'Bar') OR [e].[Name] IS NULL");
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
                @"SELECT [e].[Id], [e].[BaseParentId], [e].[Discriminator], [e].[Name], [e].[DerivedProperty], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseCollectionsOnBase] AS [e]
LEFT JOIN (
    SELECT [e.BaseParent].*
    FROM [BaseEntities] AS [e.BaseParent]
    WHERE [e.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[BaseParentId] = [t].[Id]
WHERE [e].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')");
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
                @"SELECT [e].[Id], [e].[BaseParentId], [e].[Discriminator], [e].[Name], [e].[DerivedProperty], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseCollectionsOnBase] AS [e]
LEFT JOIN (
    SELECT [e.BaseParent].*
    FROM [BaseEntities] AS [e.BaseParent]
    WHERE [e.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[BaseParentId] = [t].[Id]
WHERE [e].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
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
                @"SELECT [e].[Id], [e].[Name], [e].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [CollectionsOnBase] AS [e]
LEFT JOIN (
    SELECT [e.Parent].*
    FROM [BaseEntities] AS [e.Parent]
    WHERE [e.Parent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[ParentId] = [t].[Id]");
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
                @"SELECT [e].[Id], [e].[Name], [e].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [CollectionsOnBase] AS [e]
LEFT JOIN (
    SELECT [e.Parent].*
    FROM [BaseEntities] AS [e.Parent]
    WHERE [e.Parent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t] ON [e].[ParentId] = [t].[Id]
WHERE ([e].[Name] <> N'Bar') OR [e].[Name] IS NULL");
        }

        public override void Include_reference_with_inheritance_on_derived1()
        {
            base.Include_reference_with_inheritance_on_derived1();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_on_derived2()
        {
            base.Include_reference_with_inheritance_on_derived2();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnDerived].*
    FROM [BaseReferencesOnDerived] AS [e.BaseReferenceOnDerived]
    WHERE [e.BaseReferenceOnDerived].[Discriminator] IN (N'DerivedReferenceOnDerived', N'BaseReferenceOnDerived')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_on_derived4()
        {
            base.Include_reference_with_inheritance_on_derived4();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.DerivedReferenceOnDerived].*
    FROM [BaseReferencesOnDerived] AS [e.DerivedReferenceOnDerived]
    WHERE [e.DerivedReferenceOnDerived].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [t] ON [e].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_on_derived_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[BaseParentId], [e].[Discriminator], [e].[Name], [e].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnDerived] AS [e]
LEFT JOIN (
    SELECT [e.BaseParent].*
    FROM [BaseEntities] AS [e.BaseParent]
    WHERE [e.BaseParent].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e].[BaseParentId] = [t].[Id]
WHERE [e].[Discriminator] IN (N'DerivedReferenceOnDerived', N'BaseReferenceOnDerived')");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter1()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter1();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE ([e].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter2()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter2();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnDerived].*
    FROM [BaseReferencesOnDerived] AS [e.BaseReferenceOnDerived]
    WHERE [e.BaseReferenceOnDerived].[Discriminator] IN (N'DerivedReferenceOnDerived', N'BaseReferenceOnDerived')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
WHERE ([e].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter4()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter4();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.DerivedReferenceOnDerived].*
    FROM [BaseReferencesOnDerived] AS [e.DerivedReferenceOnDerived]
    WHERE [e.DerivedReferenceOnDerived].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [t] ON [e].[Id] = [t].[DerivedInheritanceRelationshipEntityId]
WHERE ([e].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[BaseParentId], [e].[Discriminator], [e].[Name], [e].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseReferencesOnDerived] AS [e]
LEFT JOIN (
    SELECT [e.BaseParent].*
    FROM [BaseEntities] AS [e.BaseParent]
    WHERE [e.BaseParent].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e].[BaseParentId] = [t].[Id]
WHERE [e].[Discriminator] IN (N'DerivedReferenceOnDerived', N'BaseReferenceOnDerived') AND (([e].[Name] <> N'Bar') OR [e].[Name] IS NULL)");
        }

        public override void Include_reference_without_inheritance_on_derived1()
        {
            base.Include_reference_without_inheritance_on_derived1();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [e.ReferenceOnBase].[Id], [e.ReferenceOnBase].[Name], [e.ReferenceOnBase].[ParentId]
FROM [BaseEntities] AS [e]
LEFT JOIN [ReferencesOnBase] AS [e.ReferenceOnBase] ON [e].[Id] = [e.ReferenceOnBase].[ParentId]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_without_inheritance_on_derived2()
        {
            base.Include_reference_without_inheritance_on_derived2();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [e.ReferenceOnDerived].[Id], [e.ReferenceOnDerived].[Name], [e.ReferenceOnDerived].[ParentId]
FROM [BaseEntities] AS [e]
LEFT JOIN [ReferencesOnDerived] AS [e.ReferenceOnDerived] ON [e].[Id] = [e.ReferenceOnDerived].[ParentId]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_without_inheritance_on_derived_reverse()
        {
            base.Include_reference_without_inheritance_on_derived_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], [e].[ParentId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [ReferencesOnDerived] AS [e]
LEFT JOIN (
    SELECT [e.Parent].*
    FROM [BaseEntities] AS [e.Parent]
    WHERE [e.Parent].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e].[ParentId] = [t].[Id]");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[ParentId], [e].[DerivedInheritanceRelationshipEntityId], [t].[Id], [t].[Discriminator], [t].[Name], [t].[BaseId]
FROM [BaseCollectionsOnDerived] AS [e]
LEFT JOIN (
    SELECT [e.BaseParent].*
    FROM [BaseEntities] AS [e.BaseParent]
    WHERE [e.BaseParent].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [t] ON [e].[ParentId] = [t].[Id]
WHERE [e].[Discriminator] IN (N'DerivedCollectionOnDerived', N'BaseCollectionOnDerived')");
        }

        public override void Nested_include_with_inheritance_reference_reference1()
        {
            base.Nested_include_with_inheritance_reference_reference1();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase.NestedReference].*
    FROM [NestedReferences] AS [e.BaseReferenceOnBase.NestedReference]
    WHERE [e.BaseReferenceOnBase.NestedReference].[Discriminator] IN (N'NestedReferenceDerived', N'NestedReferenceBase')
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
WHERE [e].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')");
        }

        public override void Nested_include_with_inheritance_reference_reference3()
        {
            base.Nested_include_with_inheritance_reference_reference3();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[ParentCollectionId], [t0].[ParentReferenceId]
FROM [BaseEntities] AS [e]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase].*
    FROM [BaseReferencesOnBase] AS [e.BaseReferenceOnBase]
    WHERE [e.BaseReferenceOnBase].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[Id] = [t].[BaseParentId]
LEFT JOIN (
    SELECT [e.BaseReferenceOnBase.NestedReference].*
    FROM [NestedReferences] AS [e.BaseReferenceOnBase.NestedReference]
    WHERE [e.BaseReferenceOnBase.NestedReference].[Discriminator] IN (N'NestedReferenceDerived', N'NestedReferenceBase')
) AS [t0] ON [t].[Id] = [t0].[ParentReferenceId]
WHERE [e].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Nested_include_with_inheritance_reference_reference_reverse()
        {
            base.Nested_include_with_inheritance_reference_reference_reverse();

            AssertSql(
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[ParentCollectionId], [e].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedReferences] AS [e]
LEFT JOIN (
    SELECT [e.ParentReference].*
    FROM [BaseReferencesOnBase] AS [e.ParentReference]
    WHERE [e.ParentReference].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [e.ParentReference.BaseParent].*
    FROM [BaseEntities] AS [e.ParentReference.BaseParent]
    WHERE [e.ParentReference.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [e].[Discriminator] IN (N'NestedReferenceDerived', N'NestedReferenceBase')");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[ParentCollectionId], [e].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedCollections] AS [e]
LEFT JOIN (
    SELECT [e.ParentReference].*
    FROM [BaseReferencesOnBase] AS [e.ParentReference]
    WHERE [e.ParentReference].[Discriminator] IN (N'DerivedReferenceOnBase', N'BaseReferenceOnBase')
) AS [t] ON [e].[ParentReferenceId] = [t].[Id]
LEFT JOIN (
    SELECT [e.ParentReference.BaseParent].*
    FROM [BaseEntities] AS [e.ParentReference.BaseParent]
    WHERE [e.ParentReference.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [e].[Discriminator] IN (N'NestedCollectionDerived', N'NestedCollectionBase')");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[ParentCollectionId], [e].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedReferences] AS [e]
LEFT JOIN (
    SELECT [e.ParentCollection].*
    FROM [BaseCollectionsOnBase] AS [e.ParentCollection]
    WHERE [e.ParentCollection].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
) AS [t] ON [e].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [e.ParentCollection.BaseParent].*
    FROM [BaseEntities] AS [e.ParentCollection.BaseParent]
    WHERE [e.ParentCollection.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [e].[Discriminator] IN (N'NestedReferenceDerived', N'NestedReferenceBase')");
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
                @"SELECT [e].[Id], [e].[Discriminator], [e].[Name], [e].[ParentCollectionId], [e].[ParentReferenceId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[BaseId]
FROM [NestedCollections] AS [e]
LEFT JOIN (
    SELECT [e.ParentCollection].*
    FROM [BaseCollectionsOnBase] AS [e.ParentCollection]
    WHERE [e.ParentCollection].[Discriminator] IN (N'DerivedCollectionOnBase', N'BaseCollectionOnBase')
) AS [t] ON [e].[ParentCollectionId] = [t].[Id]
LEFT JOIN (
    SELECT [e.ParentCollection.BaseParent].*
    FROM [BaseEntities] AS [e.ParentCollection.BaseParent]
    WHERE [e.ParentCollection.BaseParent].[Discriminator] IN (N'DerivedInheritanceRelationshipEntity', N'BaseInheritanceRelationshipEntity')
) AS [t0] ON [t].[BaseParentId] = [t0].[Id]
WHERE [e].[Discriminator] IN (N'NestedCollectionDerived', N'NestedCollectionBase')");
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
