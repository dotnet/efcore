// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class JsonUpdateSqlServerTest : JsonUpdateTestBase<JsonUpdateSqlServerFixture>
{
    public JsonUpdateSqlServerTest(JsonUpdateSqlServerFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    public override async Task Add_element_to_json_collection_branch()
    {
        await base.Add_element_to_json_collection_branch();

        AssertSql(
            @"@p0='[{""Date"":""2101-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":10.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_r_c1_c1""},{""SomethingSomething"":""e1_r_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_r_c1_r""}},{""Date"":""2102-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":10.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_r_c2_c1""},{""SomethingSomething"":""e1_r_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_r_c2_r""}},{""Date"":""2010-10-10T00:00:00"",""Enum"":""Three"",""Fraction"":42.42,""OwnedCollectionLeaf"":[{""SomethingSomething"":""ss1""},{""SomethingSomething"":""ss2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""ss3""}}]' (Nullable = false) (Size = 622)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Add_element_to_json_collection_leaf()
    {
        await base.Add_element_to_json_collection_leaf();

        AssertSql(
            @"@p0='[{""SomethingSomething"":""e1_r_r_c1""},{""SomethingSomething"":""e1_r_r_c2""},{""SomethingSomething"":""ss1""}]' (Nullable = false) (Size = 100)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch.OwnedCollectionLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Add_element_to_json_collection_on_derived()
    {
        await base.Add_element_to_json_collection_on_derived();

        AssertSql(
            @"@p0='[{""Date"":""2221-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":221.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""d2_r_c1""},{""SomethingSomething"":""d2_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""d2_r_r""}},{""Date"":""2222-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":222.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""d2_r_c1""},{""SomethingSomething"":""d2_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""d2_r_r""}},{""Date"":""2010-10-10T00:00:00"",""Enum"":""Three"",""Fraction"":42.42,""OwnedCollectionLeaf"":[{""SomethingSomething"":""ss1""},{""SomethingSomething"":""ss2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""ss3""}}]' (Nullable = false) (Size = 606)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesInheritance] SET [CollectionOnDerived] = @p0
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Discriminator], [j].[Name], [j].[Fraction], JSON_QUERY([j].[CollectionOnBase],'$'), JSON_QUERY([j].[ReferenceOnBase],'$'), JSON_QUERY([j].[CollectionOnDerived],'$'), JSON_QUERY([j].[ReferenceOnDerived],'$')
FROM [JsonEntitiesInheritance] AS [j]
WHERE [j].[Discriminator] = N'JsonEntityInheritanceDerived'");
    }

    public override async Task Add_element_to_json_collection_root()
    {
        await base.Add_element_to_json_collection_root();

        AssertSql(
            @"@p0='[{""Name"":""e1_c1"",""Number"":11,""OwnedCollectionBranch"":[{""Date"":""2111-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":11.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_c1_c1""},{""SomethingSomething"":""e1_c1_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c1_r""}},{""Date"":""2112-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":11.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_c2_c1""},{""SomethingSomething"":""e1_c1_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c2_r""}}],""OwnedReferenceBranch"":{""Date"":""2110-01-01T00:00:00"",""Enum"":""One"",""Fraction"":11.0,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_r_c1""},{""SomethingSomething"":""e1_c1_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_r_r""}}},{""Name"":""e1_c2"",""Number"":12,""OwnedCollectionBranch"":[{""Date"":""2121-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":12.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_c1_c1""},{""SomethingSomething"":""e1_c2_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_c1_r""}},{""Date"":""2122-01-01T00:00:00"",""Enum"":""One"",""Fraction"":12.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_c2_c1""},{""SomethingSomething"":""e1_c2_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_c2_r""}}],""OwnedReferenceBranch"":{""Date"":""2120-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":12.0,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_r_c1""},{""SomethingSomething"":""e1_c2_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_r_r""}}},{""Name"":""new Name"",""Number"":142,""OwnedCollectionBranch"":[],""OwnedReferenceBranch"":{""Date"":""2010-10-10T00:00:00"",""Enum"":""Three"",""Fraction"":42.42,""OwnedCollectionLeaf"":[{""SomethingSomething"":""ss1""},{""SomethingSomething"":""ss2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""ss3""}}}]' (Nullable = false) (Size = 1723)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Add_entity_with_json()
    {
        await base.Add_entity_with_json();

        AssertSql(
            @"@p0='{""Name"":""RootName"",""Number"":42,""OwnedCollectionBranch"":[],""OwnedReferenceBranch"":{""Date"":""2010-10-10T00:00:00"",""Enum"":""Three"",""Fraction"":42.42,""OwnedCollectionLeaf"":[{""SomethingSomething"":""ss1""},{""SomethingSomething"":""ss2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""ss3""}}}' (Nullable = false) (Size = 276)
@p1='2'
@p2='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedReferenceRoot], [Id], [Name])
VALUES (@p0, @p1, @p2);",
                //
                @"SELECT [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Add_json_reference_leaf()
    {
        await base.Add_json_reference_leaf();

        AssertSql(
            @"@p0='{""SomethingSomething"":""ss3""}' (Nullable = false) (Size = 28)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch[0].OwnedReferenceLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Add_json_reference_root()
    {
        await base.Add_json_reference_root();

        AssertSql(
            @"@p0='{""Name"":""RootName"",""Number"":42,""OwnedCollectionBranch"":[],""OwnedReferenceBranch"":{""Date"":""2010-10-10T00:00:00"",""Enum"":""Three"",""Fraction"":42.42,""OwnedCollectionLeaf"":[{""SomethingSomething"":""ss1""},{""SomethingSomething"":""ss2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""ss3""}}}' (Nullable = false) (Size = 276)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Delete_entity_with_json()
    {
        await base.Delete_entity_with_json();

        AssertSql(
            @"@p0='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [JsonEntitiesBasic]
OUTPUT 1
WHERE [Id] = @p0;",
                //
                @"SELECT COUNT(*)
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Delete_json_collection_branch()
    {
        await base.Delete_json_collection_branch();

        AssertSql(
            @"@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Delete_json_collection_root()
    {
        await base.Delete_json_collection_root();

        AssertSql(
            @"@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Delete_json_reference_leaf()
    {
        await base.Delete_json_reference_leaf();

        AssertSql(
            @"@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch.OwnedReferenceLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Delete_json_reference_root()
    {
        await base.Delete_json_reference_root();

        AssertSql(
            @"@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_element_in_json_collection_branch()
    {
        await base.Edit_element_in_json_collection_branch();

        AssertSql(
            @"@p0='{""Date"":""2111-11-11T00:00:00"",""Enum"":""Two"",""Fraction"":11.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_c1_c1""},{""SomethingSomething"":""e1_c1_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c1_r""}}' (Nullable = false) (Size = 214)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[0].OwnedCollectionBranch[0]', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_element_in_json_collection_root1()
    {
        await base.Edit_element_in_json_collection_root1();

        AssertSql(
            @"@p0='{""Name"":""Modified"",""Number"":11,""OwnedCollectionBranch"":[{""Date"":""2111-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":11.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_c1_c1""},{""SomethingSomething"":""e1_c1_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c1_r""}},{""Date"":""2112-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":11.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_c2_c1""},{""SomethingSomething"":""e1_c1_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c2_r""}}],""OwnedReferenceBranch"":{""Date"":""2110-01-01T00:00:00"",""Enum"":""One"",""Fraction"":11.0,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_r_c1""},{""SomethingSomething"":""e1_c1_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_r_r""}}}' (Nullable = false) (Size = 724)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[0]', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_element_in_json_collection_root2()
    {
        await base.Edit_element_in_json_collection_root2();

        AssertSql(
            @"@p0='{""Name"":""Modified"",""Number"":12,""OwnedCollectionBranch"":[{""Date"":""2121-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":12.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_c1_c1""},{""SomethingSomething"":""e1_c2_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_c1_r""}},{""Date"":""2122-01-01T00:00:00"",""Enum"":""One"",""Fraction"":12.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_c2_c1""},{""SomethingSomething"":""e1_c2_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_c2_r""}}],""OwnedReferenceBranch"":{""Date"":""2120-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":12.0,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_r_c1""},{""SomethingSomething"":""e1_c2_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_r_r""}}}' (Nullable = false) (Size = 724)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[1]', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_element_in_json_multiple_levels_partial_update()
    {
        await base.Edit_element_in_json_multiple_levels_partial_update();

        AssertSql(
            @"@p0='[{""Date"":""2111-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":11.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""...and another""},{""SomethingSomething"":""e1_c1_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c1_r""}},{""Date"":""2112-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":11.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""yet another change""},{""SomethingSomething"":""and another""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c2_r""}}]' (Nullable = false) (Size = 443)
@p1='{""Name"":""edit"",""Number"":10,""OwnedCollectionBranch"":[{""Date"":""2101-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":10.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_r_c1_c1""},{""SomethingSomething"":""e1_r_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_r_c1_r""}},{""Date"":""2102-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":10.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_r_c2_c1""},{""SomethingSomething"":""e1_r_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_r_c2_r""}}],""OwnedReferenceBranch"":{""Date"":""2111-11-11T00:00:00"",""Enum"":""One"",""Fraction"":10.0,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_r_r_c1""},{""SomethingSomething"":""e1_r_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_r_r_r""}}}' (Nullable = false) (Size = 711)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[0].OwnedCollectionBranch', JSON_QUERY(@p0)), [OwnedReferenceRoot] = @p1
OUTPUT 1
WHERE [Id] = @p2;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection()
    {
        await base.Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection();

        AssertSql(
            @"@p0='[{""Date"":""2101-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":4321.3,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_r_c1_c1""},{""SomethingSomething"":""e1_r_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_r_c1_r""}},{""Date"":""2102-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":10.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_r_c2_c1""},{""SomethingSomething"":""e1_r_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_r_c2_r""}},{""Date"":""2222-11-11T00:00:00"",""Enum"":""Three"",""Fraction"":45.32,""OwnedCollectionLeaf"":[],""OwnedReferenceLeaf"":{""SomethingSomething"":""cc""}}]' (Nullable = false) (Size = 566)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection()
    {
        await base.Edit_two_elements_in_the_same_json_collection();

        AssertSql(
            @"@p0='[{""SomethingSomething"":""edit1""},{""SomethingSomething"":""edit2""}]' (Nullable = false) (Size = 63)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch[0].OwnedCollectionLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection_at_the_root()
    {
        await base.Edit_two_elements_in_the_same_json_collection_at_the_root();

        AssertSql(
            @"@p0='[{""Name"":""edit1"",""Number"":11,""OwnedCollectionBranch"":[{""Date"":""2111-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":11.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_c1_c1""},{""SomethingSomething"":""e1_c1_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c1_r""}},{""Date"":""2112-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":11.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_c2_c1""},{""SomethingSomething"":""e1_c1_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_c2_r""}}],""OwnedReferenceBranch"":{""Date"":""2110-01-01T00:00:00"",""Enum"":""One"",""Fraction"":11.0,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c1_r_c1""},{""SomethingSomething"":""e1_c1_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c1_r_r""}}},{""Name"":""edit2"",""Number"":12,""OwnedCollectionBranch"":[{""Date"":""2121-01-01T00:00:00"",""Enum"":""Two"",""Fraction"":12.1,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_c1_c1""},{""SomethingSomething"":""e1_c2_c1_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_c1_r""}},{""Date"":""2122-01-01T00:00:00"",""Enum"":""One"",""Fraction"":12.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_c2_c1""},{""SomethingSomething"":""e1_c2_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_c2_r""}}],""OwnedReferenceBranch"":{""Date"":""2120-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":12.0,""OwnedCollectionLeaf"":[{""SomethingSomething"":""e1_c2_r_c1""},{""SomethingSomething"":""e1_c2_r_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""e1_c2_r_r""}}}]' (Nullable = false) (Size = 1445)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    public override async Task Edit_collection_element_and_reference_at_once()
    {
        await base.Edit_collection_element_and_reference_at_once();

        AssertSql(
            @"@p0='{""Date"":""2102-01-01T00:00:00"",""Enum"":""Three"",""Fraction"":10.2,""OwnedCollectionLeaf"":[{""SomethingSomething"":""edit1""},{""SomethingSomething"":""e1_r_c2_c2""}],""OwnedReferenceLeaf"":{""SomethingSomething"":""edit2""}}' (Nullable = false) (Size = 204)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch[1]', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                @"SELECT TOP(2) [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]");
    }

    protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
