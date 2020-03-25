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
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]");
        }

        public override void Include_reference_with_inheritance_reverse()
        {
            base.Include_reference_with_inheritance_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]");
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
) AS [t] ON [b].[Id] = [t].[BaseId]");
        }

        public override void Include_self_reference_with_inheritance_reverse()
        {
            base.Include_self_reference_with_inheritance_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseId] = [b0].[Id]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_with_filter1()
        {
            base.Include_reference_with_inheritance_with_filter1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL");
        }

        public override void Include_reference_with_inheritance_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL");
        }

        public override void Include_reference_without_inheritance()
        {
            base.Include_reference_without_inheritance();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]");
        }

        public override void Include_reference_without_inheritance_reverse()
        {
            base.Include_reference_without_inheritance_reverse();

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN [BaseEntities] AS [b] ON [r].[ParentId] = [b].[Id]");
        }

        public override void Include_reference_without_inheritance_with_filter()
        {
            base.Include_reference_without_inheritance_with_filter();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL");
        }

        public override void Include_reference_without_inheritance_with_filter_reverse()
        {
            base.Include_reference_without_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [r].[Id], [r].[Name], [r].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN [BaseEntities] AS [b] ON [r].[ParentId] = [b].[Id]
WHERE ([r].[Name] <> N'Bar') OR [r].[Name] IS NULL");
        }

        public override void Include_collection_with_inheritance1()
        {
            base.Include_collection_with_inheritance1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override void Include_collection_with_inheritance_reverse()
        {
            base.Include_collection_with_inheritance_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]");
        }

        public override void Include_collection_with_inheritance_with_filter1()
        {
            base.Include_collection_with_inheritance_with_filter1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override void Include_collection_with_inheritance_with_filter_reverse()
        {
            base.Include_collection_with_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL");
        }

        public override void Include_collection_without_inheritance()
        {
            base.Include_collection_without_inheritance();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
ORDER BY [b].[Id], [c].[Id]");
        }

        public override void Include_collection_without_inheritance_reverse()
        {
            base.Include_collection_without_inheritance_reverse();

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]");
        }

        public override void Include_collection_without_inheritance_with_filter()
        {
            base.Include_collection_without_inheritance_with_filter();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL
ORDER BY [b].[Id], [c].[Id]");
        }

        public override void Include_collection_without_inheritance_with_filter_reverse()
        {
            base.Include_collection_without_inheritance_with_filter_reverse();

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
WHERE ([c].[Name] <> N'Bar') OR [c].[Name] IS NULL");
        }

        public override void Include_reference_with_inheritance_on_derived1()
        {
            base.Include_reference_with_inheritance_on_derived1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Include_reference_with_inheritance_on_derived2()
        {
            base.Include_reference_with_inheritance_on_derived2();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnDerived] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
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
) AS [t] ON [b].[BaseParentId] = [t].[Id]");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter1()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE ([b].[Discriminator] = N'DerivedInheritanceRelationshipEntity') AND (([b].[Name] <> N'Bar') OR [b].[Name] IS NULL)");
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter2()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter2();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnDerived] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
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
WHERE ([b].[Name] <> N'Bar') OR [b].[Name] IS NULL");
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
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]");
        }

        public override void Include_collection_with_inheritance_on_derived2()
        {
            base.Include_collection_with_inheritance_on_derived2();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[ParentId], [b0].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseCollectionsOnDerived] AS [b0] ON [b].[Id] = [b0].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]");
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
) AS [t] ON [b].[ParentId] = [t].[Id]");
        }

        public override void Nested_include_with_inheritance_reference_reference1()
        {
            base.Nested_include_with_inheritance_reference_reference1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]");
        }

        public override void Nested_include_with_inheritance_reference_reference3()
        {
            base.Nested_include_with_inheritance_reference_reference3();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'");
        }

        public override void Nested_include_with_inheritance_reference_reference_reverse()
        {
            base.Nested_include_with_inheritance_reference_reference_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]");
        }

        public override void Nested_include_with_inheritance_reference_collection1()
        {
            base.Nested_include_with_inheritance_reference_collection1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
ORDER BY [b].[Id], [n].[Id]");
        }

        public override void Nested_include_with_inheritance_reference_collection3()
        {
            base.Nested_include_with_inheritance_reference_collection3();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [n].[Id]");
        }

        public override void Nested_include_with_inheritance_reference_collection_reverse()
        {
            base.Nested_include_with_inheritance_reference_collection_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]");
        }

        public override void Nested_include_with_inheritance_collection_reference1()
        {
            base.Nested_include_with_inheritance_collection_reference1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t].[Id0], [t].[Discriminator0], [t].[Name0], [t].[ParentCollectionId], [t].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentCollectionId]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [t].[Id]");
        }

        public override void Nested_include_with_inheritance_collection_reference_reverse()
        {
            base.Nested_include_with_inheritance_collection_reference_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]");
        }

        public override void Nested_include_with_inheritance_collection_collection1()
        {
            base.Nested_include_with_inheritance_collection_collection1();

            AssertSql(
                @"SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [t].[Id], [t].[BaseParentId], [t].[Discriminator], [t].[Name], [t].[DerivedProperty], [t].[Id0], [t].[Discriminator0], [t].[Name0], [t].[ParentCollectionId], [t].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b0]
    LEFT JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentCollectionId]
) AS [t] ON [b].[Id] = [t].[BaseParentId]
ORDER BY [b].[Id], [t].[Id], [t].[Id0]");
        }

        public override void Nested_include_with_inheritance_collection_collection_reverse()
        {
            base.Nested_include_with_inheritance_collection_collection_reverse();

            AssertSql(
                @"SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]");
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
