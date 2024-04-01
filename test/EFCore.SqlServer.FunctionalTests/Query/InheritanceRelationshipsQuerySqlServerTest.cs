// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Include_reference_with_inheritance(bool async)
    {
        await base.Include_reference_with_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance(bool async)
    {
        await base.Include_self_reference_with_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b1].[Discriminator], [b1].[Name], [b1].[BaseId], [b4].[BaseInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [b1].[OwnedReferenceOnBase_Id], [b1].[OwnedReferenceOnBase_Name], [b5].[DerivedInheritanceRelationshipEntityId], [b5].[Id], [b5].[Name], [b1].[OwnedReferenceOnDerived_Id], [b1].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b1] ON [b].[Id] = [b1].[BaseId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b4] ON [b1].[Id] = [b4].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b5] ON [b1].[Id] = [b5].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b4].[BaseInheritanceRelationshipEntityId], [b4].[Id], [b5].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_self_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_self_reference_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b0].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b0].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b4].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance(bool async)
    {
        await base.Include_reference_without_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN [BaseEntities] AS [b] ON [r].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnBase] AS [r]
LEFT JOIN [BaseEntities] AS [b] ON [r].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [r].[Name] <> N'Bar' OR [r].[Name] IS NULL
ORDER BY [r].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance(bool async)
    {
        await base.Include_collection_with_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnBase] AS [b2] ON [b].[Id] = [b2].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnBase] AS [b2] ON [b].[Id] = [b2].[BaseParentId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_without_inheritance(bool async)
    {
        await base.Include_collection_without_inheritance(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_reverse(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [c].[Id], [c].[Name], [c].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [CollectionsOnBase] AS [c] ON [b].[Id] = [c].[ParentId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnDerived] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived4(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b1].[BaseParentId], [b1].[Discriminator], [b1].[Name], [b1].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b3].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Discriminator], [b1].[Name], [b1].[BaseId], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b1].[OwnedReferenceOnBase_Id], [b1].[OwnedReferenceOnBase_Name], [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b1].[OwnedReferenceOnDerived_Id], [b1].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b1] ON [b].[BaseParentId] = [b1].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b1].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b1].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b3].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity' AND ([b].[Name] <> N'Bar' OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnDerived] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity' AND ([b].[Name] <> N'Bar' OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter4(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b1].[BaseParentId], [b1].[Discriminator], [b1].[Name], [b1].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseReferencesOnDerived] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedReferenceOnDerived'
) AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity' AND ([b].[Name] <> N'Bar' OR [b].[Name] IS NULL)
ORDER BY [b].[Id], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b3].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Discriminator], [b1].[Name], [b1].[BaseId], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b1].[OwnedReferenceOnBase_Id], [b1].[OwnedReferenceOnBase_Name], [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b1].[OwnedReferenceOnDerived_Id], [b1].[OwnedReferenceOnDerived_Name]
FROM [BaseReferencesOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b1] ON [b].[BaseParentId] = [b1].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b1].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b1].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b3].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnBase] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [r].[Name], [r].[ParentId]
FROM [BaseEntities] AS [b]
LEFT JOIN [ReferencesOnDerived] AS [r] ON [b].[Id] = [r].[ParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [r].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_reference_without_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[ParentId], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [ReferencesOnDerived] AS [r]
LEFT JOIN (
    SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b0] ON [r].[ParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [r].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived1(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnBase] AS [b2] ON [b].[Id] = [b2].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived2(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b2].[Id], [b2].[Discriminator], [b2].[Name], [b2].[ParentId], [b2].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnDerived] AS [b2] ON [b].[Id] = [b2].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived3(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b3].[Id], [b3].[Discriminator], [b3].[Name], [b3].[ParentId], [b3].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b2].[Id], [b2].[Discriminator], [b2].[Name], [b2].[ParentId], [b2].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b2]
    WHERE [b2].[Discriminator] = N'DerivedCollectionOnDerived'
) AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[ParentId], [b].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Discriminator], [b1].[Name], [b1].[BaseId], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b1].[OwnedReferenceOnBase_Id], [b1].[OwnedReferenceOnBase_Name], [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b1].[OwnedReferenceOnDerived_Id], [b1].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b1] ON [b].[ParentId] = [b1].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b1].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b1].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b1].[Id], [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b3].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [n].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id], [n].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_on_base(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [n].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [NestedReferences] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [n].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[BaseParentId], [s].[Discriminator], [s].[Name], [s].[DerivedProperty], [s].[Id0], [s].[Discriminator0], [s].[Name0], [s].[ParentCollectionId], [s].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [NestedReferences] AS [n] ON [b2].[Id] = [n].[ParentCollectionId]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [s].[Id], [s].[BaseParentId], [s].[Discriminator], [s].[Name], [s].[DerivedProperty], [s].[Id0], [s].[Discriminator0], [s].[Name0], [s].[ParentCollectionId], [s].[ParentReferenceId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b2]
    LEFT JOIN [NestedCollections] AS [n] ON [b2].[Id] = [n].[ParentCollectionId]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [s].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b2].[DerivedInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b1] ON [b0].[Id] = [b1].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b2] ON [b0].[Id] = [b2].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id], [b1].[BaseInheritanceRelationshipEntityId], [b1].[Id], [b2].[DerivedInheritanceRelationshipEntityId]
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

    public override async Task Collection_projection_on_base_type(bool async)
    {
        await base.Collection_projection_on_base_type(async);

        AssertSql(
            """
SELECT [b].[Id], [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b3].[Id], [b3].[Discriminator], [b3].[Name], [b3].[ParentId], [b3].[DerivedInheritanceRelationshipEntityId]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN (
    SELECT [b2].[Id], [b2].[Discriminator], [b2].[Name], [b2].[ParentId], [b2].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b2]
    WHERE [b2].[Discriminator] = N'DerivedCollectionOnDerived'
) AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_split(bool async)
    {
        await base.Include_collection_with_inheritance_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b4].[Id], [b4].[BaseParentId], [b4].[Discriminator], [b4].[Name], [b4].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b4] ON [b].[Id] = [b4].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_reverse_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b0].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b0].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b4].[Id], [b4].[BaseParentId], [b4].[Discriminator], [b4].[Name], [b4].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b4] ON [b].[Id] = [b4].[BaseParentId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b0].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [b].[Id], [b0].[Id]
FROM [BaseCollectionsOnBase] AS [b]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b0].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id], [b0].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_split(bool async)
    {
        await base.Include_collection_without_inheritance_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
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
SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
ORDER BY [c].[Id], [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [c].[Id], [b].[Id]
""");
    }

    public override async Task Include_collection_without_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Name] <> N'Bar' OR [b].[Name] IS NULL
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
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
SELECT [c].[Id], [c].[Name], [c].[ParentId], [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [c].[Id], [b].[Id]
FROM [CollectionsOnBase] AS [c]
LEFT JOIN [BaseEntities] AS [b] ON [c].[ParentId] = [b].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [c].[Name] <> N'Bar' OR [c].[Name] IS NULL
ORDER BY [c].[Id], [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived1_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b4].[Id], [b4].[BaseParentId], [b4].[Discriminator], [b4].[Name], [b4].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b4] ON [b].[Id] = [b4].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived2_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b4].[Id], [b4].[Discriminator], [b4].[Name], [b4].[ParentId], [b4].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnDerived] AS [b4] ON [b].[Id] = [b4].[ParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived3_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b5].[Id], [b5].[Discriminator], [b5].[Name], [b5].[ParentId], [b5].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b4].[Id], [b4].[Discriminator], [b4].[Name], [b4].[ParentId], [b4].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b4]
    WHERE [b4].[Discriminator] = N'DerivedCollectionOnDerived'
) AS [b5] ON [b].[Id] = [b5].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[ParentId], [b].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Discriminator], [b1].[Name], [b1].[BaseId], [b1].[OwnedReferenceOnBase_Id], [b1].[OwnedReferenceOnBase_Name], [b1].[OwnedReferenceOnDerived_Id], [b1].[OwnedReferenceOnDerived_Name]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b1] ON [b].[ParentId] = [b1].[Id]
ORDER BY [b].[Id], [b1].[Id]
""",
            //
            """
SELECT [b4].[BaseInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [b].[Id], [b1].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b1] ON [b].[ParentId] = [b1].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b4] ON [b1].[Id] = [b4].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b1].[Id]
""",
            //
            """
SELECT [b5].[DerivedInheritanceRelationshipEntityId], [b5].[Id], [b5].[Name], [b].[Id], [b1].[Id]
FROM [BaseCollectionsOnDerived] AS [b]
LEFT JOIN (
    SELECT [b0].[Id]
    FROM [BaseEntities] AS [b0]
    WHERE [b0].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
) AS [b1] ON [b].[ParentId] = [b1].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b5] ON [b1].[Id] = [b5].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b1].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
ORDER BY [b].[Id], [b0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b0].[Id], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b0].[Id]
FROM [BaseEntities] AS [b]
LEFT JOIN [BaseReferencesOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
INNER JOIN [NestedCollections] AS [n] ON [b0].[Id] = [n].[ParentReferenceId]
WHERE [b].[Discriminator] = N'DerivedInheritanceRelationshipEntity'
ORDER BY [b].[Id], [b0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse_split(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b0].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseReferencesOnBase] AS [b] ON [n].[ParentReferenceId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b0].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[BaseParentId], [s].[Discriminator], [s].[Name], [s].[DerivedProperty], [s].[Id0], [s].[Discriminator0], [s].[Name0], [s].[ParentCollectionId], [s].[ParentReferenceId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b4].[Id], [b4].[BaseParentId], [b4].[Discriminator], [b4].[Name], [b4].[DerivedProperty], [n].[Id] AS [Id0], [n].[Discriminator] AS [Discriminator0], [n].[Name] AS [Name0], [n].[ParentCollectionId], [n].[ParentReferenceId]
    FROM [BaseCollectionsOnBase] AS [b4]
    LEFT JOIN [NestedReferences] AS [n] ON [b4].[Id] = [n].[ParentCollectionId]
) AS [s] ON [b].[Id] = [s].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse_split(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b0].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedReferences] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b0].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b4].[Id], [b4].[BaseParentId], [b4].[Discriminator], [b4].[Name], [b4].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b4] ON [b].[Id] = [b4].[BaseParentId]
ORDER BY [b].[Id], [b4].[Id]
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b4].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b4] ON [b].[Id] = [b4].[BaseParentId]
INNER JOIN [NestedCollections] AS [n] ON [b4].[Id] = [n].[ParentCollectionId]
ORDER BY [b].[Id], [b4].[Id]
""");
    }

    public override async Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse_split(async);

        AssertSql(
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[ParentCollectionId], [n].[ParentReferenceId], [b].[Id], [b].[BaseParentId], [b].[Discriminator], [b].[Name], [b].[DerivedProperty], [b0].[Id], [b0].[Discriminator], [b0].[Name], [b0].[BaseId], [b0].[OwnedReferenceOnBase_Id], [b0].[OwnedReferenceOnBase_Name], [b0].[OwnedReferenceOnDerived_Id], [b0].[OwnedReferenceOnDerived_Name]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b3].[BaseInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b3] ON [b0].[Id] = [b3].[BaseInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
""",
            //
            """
SELECT [b4].[DerivedInheritanceRelationshipEntityId], [b4].[Id], [b4].[Name], [n].[Id], [b].[Id], [b0].[Id]
FROM [NestedCollections] AS [n]
LEFT JOIN [BaseCollectionsOnBase] AS [b] ON [n].[ParentCollectionId] = [b].[Id]
LEFT JOIN [BaseEntities] AS [b0] ON [b].[BaseParentId] = [b0].[Id]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b4] ON [b0].[Id] = [b4].[DerivedInheritanceRelationshipEntityId]
ORDER BY [n].[Id], [b].[Id], [b0].[Id]
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
SELECT [b0].[Id], [b0].[BaseParentId], [b0].[Discriminator], [b0].[Name], [b0].[DerivedProperty], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseCollectionsOnBase] AS [b0] ON [b].[Id] = [b0].[BaseParentId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_on_derived_type_with_queryable_Cast_split(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast_split(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
FROM [BaseEntities] AS [b]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b2].[BaseInheritanceRelationshipEntityId], [b2].[Id], [b2].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnBase] AS [b2] ON [b].[Id] = [b2].[BaseInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b3].[DerivedInheritanceRelationshipEntityId], [b3].[Id], [b3].[Name], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b3] ON [b].[Id] = [b3].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b5].[Id], [b5].[Discriminator], [b5].[Name], [b5].[ParentId], [b5].[DerivedInheritanceRelationshipEntityId], [b].[Id]
FROM [BaseEntities] AS [b]
INNER JOIN (
    SELECT [b4].[Id], [b4].[Discriminator], [b4].[Name], [b4].[ParentId], [b4].[DerivedInheritanceRelationshipEntityId]
    FROM [BaseCollectionsOnDerived] AS [b4]
    WHERE [b4].[Discriminator] = N'DerivedCollectionOnDerived'
) AS [b5] ON [b].[Id] = [b5].[DerivedInheritanceRelationshipEntityId]
WHERE [b].[Id] >= 4
ORDER BY [b].[Id]
""");
    }

    public override void Changes_in_derived_related_entities_are_detected()
    {
        base.Changes_in_derived_related_entities_are_detected();

        AssertSql(
            """
SELECT [b3].[Id], [b3].[Discriminator], [b3].[Name], [b3].[BaseId], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b0].[Name], [b3].[OwnedReferenceOnBase_Id], [b3].[OwnedReferenceOnBase_Name], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id], [b1].[Name], [b3].[OwnedReferenceOnDerived_Id], [b3].[OwnedReferenceOnDerived_Name], [b2].[Id], [b2].[BaseParentId], [b2].[Discriminator], [b2].[Name], [b2].[DerivedProperty]
FROM (
    SELECT TOP(2) [b].[Id], [b].[Discriminator], [b].[Name], [b].[BaseId], [b].[OwnedReferenceOnBase_Id], [b].[OwnedReferenceOnBase_Name], [b].[OwnedReferenceOnDerived_Id], [b].[OwnedReferenceOnDerived_Name]
    FROM [BaseEntities] AS [b]
    WHERE [b].[Name] = N'Derived1(4)'
) AS [b3]
LEFT JOIN [BaseEntities_OwnedCollectionOnBase] AS [b0] ON [b3].[Id] = [b0].[BaseInheritanceRelationshipEntityId]
LEFT JOIN [BaseEntities_OwnedCollectionOnDerived] AS [b1] ON [b3].[Id] = [b1].[DerivedInheritanceRelationshipEntityId]
LEFT JOIN [BaseCollectionsOnBase] AS [b2] ON [b3].[Id] = [b2].[BaseParentId]
ORDER BY [b3].[Id], [b0].[BaseInheritanceRelationshipEntityId], [b0].[Id], [b1].[DerivedInheritanceRelationshipEntityId], [b1].[Id]
""");
    }

    public override void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
    {
        base.Entity_can_make_separate_relationships_with_base_type_and_derived_type_both();

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class InheritanceRelationshipsQuerySqlServerFixture : InheritanceRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
