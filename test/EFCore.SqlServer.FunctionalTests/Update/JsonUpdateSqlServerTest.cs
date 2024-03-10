// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

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
            """
@p0='[{"Date":"2101-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":10.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":10.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2010-10-10T00:00:00","Enum":-3,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (Size = 789)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Add_element_to_json_collection_leaf()
    {
        await base.Add_element_to_json_collection_leaf();

        AssertSql(
            """
@p0='[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"},{"SomethingSomething":"ss1"}]' (Nullable = false) (Size = 100)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch.OwnedCollectionLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Add_element_to_json_collection_on_derived()
    {
        await base.Add_element_to_json_collection_on_derived();

        AssertSql(
            """
@p0='[{"Date":"2221-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":221.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2222-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":222.1,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2010-10-10T00:00:00","Enum":-3,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (Size = 773)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesInheritance] SET [CollectionOnDerived] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[Discriminator], [j].[Name], [j].[Fraction], [j].[CollectionOnBase], [j].[ReferenceOnBase], [j].[CollectionOnDerived], [j].[ReferenceOnDerived]
FROM [JsonEntitiesInheritance] AS [j]
WHERE [j].[Discriminator] = N'JsonEntityInheritanceDerived'
""");
    }

    public override async Task Add_element_to_json_collection_root()
    {
        await base.Add_element_to_json_collection_root();

        AssertSql(
            """
@p0='[{"Name":"e1_c1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":11.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":11.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":11.0,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"e1_c2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":12.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":12.2,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":12.0,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}},{"Name":"new Name","Names":null,"Number":142,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":-3,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}]' (Nullable = false) (Size = 2268)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Add_element_to_json_collection_root_null_navigations()
    {
        await base.Add_element_to_json_collection_root_null_navigations();

        AssertSql(
            """
@p0='[{"Name":"e1_c1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":11.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":11.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":11.0,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"e1_c2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":12.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":12.2,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":12.0,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}},{"Name":"new Name","Names":null,"Number":142,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":-3,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":null}}]' (Nullable = false) (Size = 2191)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Add_entity_with_json()
    {
        await base.Add_entity_with_json();

        AssertSql(
            """
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":-3,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (Size = 353)
@p1='[]' (Nullable = false) (Size = 2)
@p2='2'
@p3=NULL (DbType = Int32)
@p4='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedReferenceRoot], [OwnedCollectionRoot], [Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
            //
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Add_entity_with_json_null_navigations()
    {
        await base.Add_entity_with_json_null_navigations();

        AssertSql(
            """
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":-3,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":null}}' (Nullable = false) (Size = 331)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedReferenceRoot], [Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2, @p3);
""",
            //
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Add_json_reference_leaf()
    {
        await base.Add_json_reference_leaf();

        AssertSql(
            """
@p0='{"SomethingSomething":"ss3"}' (Nullable = false) (Size = 28)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch[0].OwnedReferenceLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Add_json_reference_root()
    {
        await base.Add_json_reference_root();

        AssertSql(
            """
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":-3,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (Size = 353)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Delete_entity_with_json()
    {
        await base.Delete_entity_with_json();

        AssertSql(
            """
@p0='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [JsonEntitiesBasic]
OUTPUT 1
WHERE [Id] = @p0;
""",
            //
            """
SELECT COUNT(*)
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Delete_json_collection_branch()
    {
        await base.Delete_json_collection_branch();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Delete_json_collection_root()
    {
        await base.Delete_json_collection_root();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Delete_json_reference_leaf()
    {
        await base.Delete_json_reference_leaf();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch.OwnedReferenceLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Delete_json_reference_root()
    {
        await base.Delete_json_reference_root();

        AssertSql(
            """
@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_element_in_json_collection_branch()
    {
        await base.Edit_element_in_json_collection_branch();

        AssertSql(
            """
@p0='2111-11-11T00:00:00' (Nullable = false) (Size = 19)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[0].OwnedCollectionBranch[0].Date', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_element_in_json_collection_root1()
    {
        await base.Edit_element_in_json_collection_root1();

        AssertSql(
            """
@p0='Modified' (Nullable = false) (Size = 4000)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[0].Name', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_element_in_json_collection_root2()
    {
        await base.Edit_element_in_json_collection_root2();

        AssertSql(
            """
@p0='Modified' (Nullable = false) (Size = 4000)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[1].Name', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_element_in_json_multiple_levels_partial_update()
    {
        await base.Edit_element_in_json_multiple_levels_partial_update();

        AssertSql(
            """
@p0='[{"Date":"2111-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":11.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"...and another"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":11.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"yet another change"},{"SomethingSomething":"and another"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}]' (Nullable = false) (Size = 561)
@p1='{"Name":"edit","Names":["e1_r1","e1_r2"],"Number":10,"Numbers":[-2147483648,-1,0,1,2147483647],"OwnedCollectionBranch":[{"Date":"2101-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":10.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":10.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}}],"OwnedReferenceBranch":{"Date":"2111-11-11T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":10.0,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}}' (Nullable = false) (Size = 960)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[0].OwnedCollectionBranch', JSON_QUERY(@p0)), [OwnedReferenceRoot] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection()
    {
        await base.Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection();

        AssertSql(
            """
@p0='[{"Date":"2101-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":4321.3,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":10.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2222-11-11T00:00:00","Enum":-3,"Enums":null,"Fraction":45.32,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":{"SomethingSomething":"cc"}}]' (Nullable = false) (Size = 735)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection()
    {
        await base.Edit_two_elements_in_the_same_json_collection();

        AssertSql(
            """
@p0='[{"SomethingSomething":"edit1"},{"SomethingSomething":"edit2"}]' (Nullable = false) (Size = 63)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch[0].OwnedCollectionLeaf', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection_at_the_root()
    {
        await base.Edit_two_elements_in_the_same_json_collection_at_the_root();

        AssertSql(
            """
@p0='[{"Name":"edit1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":11.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":11.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":11.0,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"edit2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":2,"Enums":[-1,-1,2],"Fraction":12.1,"NullableEnum":-1,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":12.2,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":12.0,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}}]' (Nullable = false) (Size = 1913)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_collection_element_and_reference_at_once()
    {
        await base.Edit_collection_element_and_reference_at_once();

        AssertSql(
            """
@p0='{"Date":"2102-01-01T00:00:00","Enum":-3,"Enums":[-1,-1,2],"Fraction":10.2,"NullableEnum":2,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"edit1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit2"}}' (Nullable = false) (Size = 262)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch[1]', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_single_enum_property()
    {
        await base.Edit_single_enum_property();

        AssertSql(
            """
@p0='2'
@p1='2'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[1].OwnedCollectionBranch[1].Enum', @p0), [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch.Enum', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_single_numeric_property()
    {
        await base.Edit_single_numeric_property();

        AssertSql(
            """
@p0='1024'
@p1='999'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[1].Number', @p0), [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.Number', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_single_property_bool()
    {
        await base.Edit_single_property_bool();

        AssertSql(
            """
@p0='True'
@p1='False'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestBoolean', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestBoolean', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_byte()
    {
        await base.Edit_single_property_byte();

        AssertSql(
            """
@p0='14' (Size = 1)
@p1='25' (Size = 1)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestByte', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestByte', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_char()
    {
        await base.Edit_single_property_char();

        AssertSql(
            """
@p0='t' (Nullable = false) (Size = 1)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Reference] = JSON_MODIFY([Reference], 'strict $.TestCharacter', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_datetime()
    {
        await base.Edit_single_property_datetime();

        AssertSql(
            """
@p0='3000-01-01T12:34:56' (Nullable = false) (Size = 19)
@p1='3000-01-01T12:34:56' (Nullable = false) (Size = 19)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDateTime', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDateTime', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_datetimeoffset()
    {
        await base.Edit_single_property_datetimeoffset();

        AssertSql(
            """
@p0='3000-01-01T12:34:56-04:00' (Nullable = false) (Size = 25)
@p1='3000-01-01T12:34:56-04:00' (Nullable = false) (Size = 25)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDateTimeOffset', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDateTimeOffset', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_decimal()
    {
        await base.Edit_single_property_decimal();

        AssertSql(
            """
@p0='-13579.01' (Precision = 18) (Scale = 3)
@p1='-13579.01' (Precision = 18) (Scale = 3)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDecimal', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDecimal', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_double()
    {
        await base.Edit_single_property_double();

        AssertSql(
            """
@p0='-1.23579'
@p1='-1.23579'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDouble', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDouble', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_guid()
    {
        await base.Edit_single_property_guid();

        AssertSql(
            """
@p0='12345678-1234-4321-5555-987654321000' (Nullable = false) (Size = 36)
@p1='12345678-1234-4321-5555-987654321000' (Nullable = false) (Size = 36)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestGuid', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestGuid', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_int16()
    {
        await base.Edit_single_property_int16();

        AssertSql(
            """
@p0='-3234'
@p1='-3234'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestInt16', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestInt16', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_int32()
    {
        await base.Edit_single_property_int32();

        AssertSql(
            """
@p0='-3234'
@p1='-3234'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestInt32', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestInt32', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_int64()
    {
        await base.Edit_single_property_int64();

        AssertSql(
            """
@p0='-3234'
@p1='-3234'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestInt64', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestInt64', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_signed_byte()
    {
        await base.Edit_single_property_signed_byte();

        AssertSql(
            """
@p0='-108'
@p1='-108'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestSignedByte', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestSignedByte', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_single()
    {
        await base.Edit_single_property_single();

        AssertSql(
            """
@p0='-7.234'
@p1='-7.234'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestSingle', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestSingle', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_timespan()
    {
        await base.Edit_single_property_timespan();

        AssertSql(
            """
@p0='10:01:01.007' (Nullable = false) (Size = 12)
@p1='10:01:01.007' (Nullable = false) (Size = 12)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestTimeSpan', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestTimeSpan', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_uint16()
    {
        await base.Edit_single_property_uint16();

        AssertSql(
            """
@p0='1534'
@p1='1534'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestUnsignedInt16', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestUnsignedInt16', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_uint32()
    {
        await base.Edit_single_property_uint32();

        AssertSql(
            """
@p0='1237775789'
@p1='1237775789'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestUnsignedInt32', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestUnsignedInt32', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_uint64()
    {
        await base.Edit_single_property_uint64();

        AssertSql(
            """
@p0='1234555555123456789' (Precision = 20)
@p1='1234555555123456789' (Precision = 20)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestUnsignedInt64', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestUnsignedInt64', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_int32()
    {
        await base.Edit_single_property_nullable_int32();

        AssertSql(
            """
@p0='122354'
@p1='64528'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableInt32', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableInt32', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_int32_set_to_null()
    {
        await base.Edit_single_property_nullable_int32_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false) (DbType = Int32)
@p1=NULL (Nullable = false) (DbType = Int32)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableInt32', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableInt32', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_enum()
    {
        await base.Edit_single_property_enum();

        AssertSql(
            """
@p0='-3'
@p1='-3'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestEnum', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestEnum', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_enum_with_int_converter()
    {
        await base.Edit_single_property_enum_with_int_converter();

        AssertSql(
            """
@p0='-3'
@p1='-3'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestEnumWithIntConverter', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestEnumWithIntConverter', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_enum()
    {
        await base.Edit_single_property_nullable_enum();

        AssertSql(
            """
@p0='-3'
@p1='-3'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestEnum', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestEnum', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_enum_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false) (DbType = Int32)
@p1=NULL (Nullable = false) (DbType = Int32)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnum', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnum', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_int_converter()
    {
        await base.Edit_single_property_nullable_enum_with_int_converter();

        AssertSql(
            """
@p0='-1'
@p1='-3'
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithIntConverter', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithIntConverter', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_int_converter_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_with_int_converter_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false) (DbType = Int32)
@p1=NULL (Nullable = false) (DbType = Int32)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithIntConverter', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithIntConverter', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls()
    {
        await base.Edit_single_property_nullable_enum_with_converter_that_handles_nulls();

        AssertSql(
            """
@p0='Three' (Nullable = false) (Size = 4000)
@p1='One' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithConverterThatHandlesNulls', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithConverterThatHandlesNulls', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_with_converter_that_handles_nulls_set_to_null();

        AssertSql(
            """
@p0='Null' (Nullable = false) (Size = 4000)
@p1='Null' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithConverterThatHandlesNulls', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithConverterThatHandlesNulls', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_two_properties_on_same_entity_updates_the_entire_entity()
    {
        await base.Edit_two_properties_on_same_entity_updates_the_entire_entity();

        AssertSql(
            """
@p0='{"TestBoolean":false,"TestBooleanCollection":[true,false],"TestByte":25,"TestByteCollection":null,"TestCharacter":"h","TestCharacterCollection":["A","B","\u0022"],"TestDateOnly":"2323-04-03","TestDateOnlyCollection":["3234-01-23","4331-01-21"],"TestDateTime":"2100-11-11T12:34:56","TestDateTimeCollection":["2000-01-01T12:34:56","3000-01-01T12:34:56"],"TestDateTimeOffset":"2200-11-11T12:34:56-05:00","TestDateTimeOffsetCollection":["2000-01-01T12:34:56-08:00"],"TestDecimal":-123450.01,"TestDecimalCollection":[-1234567890.01],"TestDefaultString":"MyDefaultStringInCollection1","TestDefaultStringCollection":["S1","\u0022S2\u0022","S3"],"TestDouble":-1.2345,"TestDoubleCollection":[-1.23456789,1.23456789,0],"TestEnum":-1,"TestEnumCollection":[-1,-3,-7],"TestEnumWithIntConverter":2,"TestEnumWithIntConverterCollection":[-1,-3,-7],"TestGuid":"00000000-0000-0000-0000-000000000000","TestGuidCollection":["12345678-1234-4321-7777-987654321000"],"TestInt16":-12,"TestInt16Collection":[-32768,0,32767],"TestInt32":32,"TestInt32Collection":[-2147483648,0,2147483647],"TestInt64":64,"TestInt64Collection":[-9223372036854775808,0,9223372036854775807],"TestMaxLengthString":"Baz","TestMaxLengthStringCollection":["S1","S2","S3"],"TestNullableEnum":-1,"TestNullableEnumCollection":[-1,null,-3,-7],"TestNullableEnumWithConverterThatHandlesNulls":"Two","TestNullableEnumWithConverterThatHandlesNullsCollection":[-1,null,-7],"TestNullableEnumWithIntConverter":-3,"TestNullableEnumWithIntConverterCollection":[-1,null,-3,-7],"TestNullableInt32":90,"TestNullableInt32Collection":[null,-2147483648,0,null,2147483647,null],"TestSignedByte":-18,"TestSignedByteCollection":[-128,0,127],"TestSingle":-1.4,"TestSingleCollection":[-1.234,0,-1.234],"TestTimeOnly":"05:07:08.0000000","TestTimeOnlyCollection":["13:42:23.0000000","07:17:25.0000000"],"TestTimeSpan":"6:05:04.003","TestTimeSpanCollection":["10:09:08.007","-9:50:51.993"],"TestUnsignedInt16":12,"TestUnsignedInt16Collection":[0,0,65535],"TestUnsignedInt32":12345,"TestUnsignedInt32Collection":[0,0,4294967295],"TestUnsignedInt64":1234567867,"TestUnsignedInt64Collection":[0,0,18446744073709551615]}' (Nullable = false) (Size = 2139)
@p1='{"TestBoolean":true,"TestBooleanCollection":[true,false],"TestByte":255,"TestByteCollection":null,"TestCharacter":"a","TestCharacterCollection":["A","B","\u0022"],"TestDateOnly":"2023-10-10","TestDateOnlyCollection":["1234-01-23","4321-01-21"],"TestDateTime":"2000-01-01T12:34:56","TestDateTimeCollection":["2000-01-01T12:34:56","3000-01-01T12:34:56"],"TestDateTimeOffset":"2000-01-01T12:34:56-08:00","TestDateTimeOffsetCollection":["2000-01-01T12:34:56-08:00"],"TestDecimal":-1234567890.01,"TestDecimalCollection":[-1234567890.01],"TestDefaultString":"MyDefaultStringInReference1","TestDefaultStringCollection":["S1","\u0022S2\u0022","S3"],"TestDouble":-1.23456789,"TestDoubleCollection":[-1.23456789,1.23456789,0],"TestEnum":-1,"TestEnumCollection":[-1,-3,-7],"TestEnumWithIntConverter":2,"TestEnumWithIntConverterCollection":[-1,-3,-7],"TestGuid":"12345678-1234-4321-7777-987654321000","TestGuidCollection":["12345678-1234-4321-7777-987654321000"],"TestInt16":-1234,"TestInt16Collection":[-32768,0,32767],"TestInt32":32,"TestInt32Collection":[-2147483648,0,2147483647],"TestInt64":64,"TestInt64Collection":[-9223372036854775808,0,9223372036854775807],"TestMaxLengthString":"Foo","TestMaxLengthStringCollection":["S1","S2","S3"],"TestNullableEnum":-1,"TestNullableEnumCollection":[-1,null,-3,-7],"TestNullableEnumWithConverterThatHandlesNulls":"Three","TestNullableEnumWithConverterThatHandlesNullsCollection":[-1,null,-7],"TestNullableEnumWithIntConverter":2,"TestNullableEnumWithIntConverterCollection":[-1,null,-3,-7],"TestNullableInt32":78,"TestNullableInt32Collection":[null,-2147483648,0,null,2147483647,null],"TestSignedByte":-128,"TestSignedByteCollection":[-128,0,127],"TestSingle":-1.234,"TestSingleCollection":[-1.234,0,-1.234],"TestTimeOnly":"11:12:13.0000000","TestTimeOnlyCollection":["11:42:23.0000000","07:17:27.0000000"],"TestTimeSpan":"10:09:08.007","TestTimeSpanCollection":["10:09:08.007","-9:50:51.993"],"TestUnsignedInt16":1234,"TestUnsignedInt16Collection":[0,0,65535],"TestUnsignedInt32":1234565789,"TestUnsignedInt32Collection":[0,0,4294967295],"TestUnsignedInt64":1234567890123456789,"TestUnsignedInt64Collection":[0,0,18446744073709551615]}' (Nullable = false) (Size = 2169)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0]', JSON_QUERY(@p0)), [Reference] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_a_scalar_property_and_reference_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_reference_navigation_on_the_same_entity();

        AssertSql(
            """
@p0='{"Date":"2100-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":523.532,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (Size = 270)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_a_scalar_property_and_collection_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_collection_navigation_on_the_same_entity();

        AssertSql(
            """
@p0='{"Date":"2100-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":523.532,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"edit"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}' (Nullable = false) (Size = 234)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_a_scalar_property_and_another_property_behind_reference_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_another_property_behind_reference_navigation_on_the_same_entity();

        AssertSql(
            """
@p0='{"Date":"2100-01-01T00:00:00","Enum":-1,"Enums":[-1,-1,2],"Fraction":523.532,"NullableEnum":null,"NullableEnums":[null,-1,2],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (Size = 270)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedReferenceBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_int_zero_one()
    {
        await base.Edit_single_property_with_converter_bool_to_int_zero_one();

        AssertSql(
            """
@p0='0'
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesConverters] SET [Reference] = JSON_MODIFY([Reference], 'strict $.BoolConvertedToIntZeroOne', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_string_True_False()
    {
        await base.Edit_single_property_with_converter_bool_to_string_True_False();

        AssertSql(
            """
@p0='True' (Nullable = false) (Size = 5)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesConverters] SET [Reference] = JSON_MODIFY([Reference], 'strict $.BoolConvertedToStringTrueFalse', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_string_Y_N()
    {
        await base.Edit_single_property_with_converter_bool_to_string_Y_N();

        AssertSql(
            """
@p0='N' (Nullable = false) (Size = 1)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesConverters] SET [Reference] = JSON_MODIFY([Reference], 'strict $.BoolConvertedToStringYN', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_with_converter_int_zero_one_to_bool()
    {
        await base.Edit_single_property_with_converter_int_zero_one_to_bool();

        AssertSql(
            """
@p0='True'
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesConverters] SET [Reference] = JSON_MODIFY([Reference], 'strict $.IntZeroOneConvertedToBool', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE [j].[Id] = 1
""");
    }

    [ConditionalFact]
    public override async Task Edit_single_property_with_converter_string_True_False_to_bool()
    {
        await base.Edit_single_property_with_converter_string_True_False_to_bool();

        AssertSql(
            """
@p0='False'
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesConverters] SET [Reference] = JSON_MODIFY([Reference], 'strict $.StringTrueFalseConvertedToBool', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE [j].[Id] = 1
""");
    }

    [ConditionalFact]
    public override async Task Edit_single_property_with_converter_string_Y_N_to_bool()
    {
        await base.Edit_single_property_with_converter_string_Y_N_to_bool();

        AssertSql(
            """
@p0='True'
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesConverters] SET [Reference] = JSON_MODIFY([Reference], 'strict $.StringYNConvertedToBool', @p0)
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_numeric()
    {
        await base.Edit_single_property_collection_of_numeric();

        AssertSql(
            """
@p0='[1024,2048]' (Nullable = false) (Size = 4000)
@p1='[999,997]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = JSON_MODIFY([OwnedCollectionRoot], 'strict $[1].Numbers', JSON_QUERY(@p0)), [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.Numbers', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Edit_single_property_collection_of_bool()
    {
        await base.Edit_single_property_collection_of_bool();

        AssertSql(
            """
@p0='[true,true,true,false]' (Nullable = false) (Size = 4000)
@p1='[true,true,false]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestBooleanCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestBooleanCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_byte()
    {
        await base.Edit_single_property_collection_of_byte();

        AssertSql(
            """
@p0='Dg==' (Nullable = false) (Size = 4)
@p1='GRo=' (Nullable = false) (Size = 4)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestByteCollection', @p0), [Reference] = JSON_MODIFY([Reference], 'strict $.TestByteCollection', @p1)
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_char()
    {
        await base.Edit_single_property_collection_of_char();

        AssertSql(
            """
@p0='["A","B","\u0022","\u0000"]' (Nullable = false) (Size = 4000)
@p1='["E","F","C","\u00F6","r","E","\u0022","\\"]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestCharacterCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestCharacterCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_datetime()
    {
        await base.Edit_single_property_collection_of_datetime();

        AssertSql(
            """
@p0='["2000-01-01T12:34:56","3000-01-01T12:34:56","3000-01-01T12:34:56"]' (Nullable = false) (Size = 4000)
@p1='["2000-01-01T12:34:56","3000-01-01T12:34:56","3000-01-01T12:34:56"]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDateTimeCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDateTimeCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_datetimeoffset()
    {
        await base.Edit_single_property_collection_of_datetimeoffset();

        AssertSql(
            """
@p0='["3000-01-01T12:34:56-04:00"]' (Nullable = false) (Size = 4000)
@p1='["3000-01-01T12:34:56-04:00"]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDateTimeOffsetCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDateTimeOffsetCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_decimal()
    {
        await base.Edit_single_property_collection_of_decimal();

        AssertSql(
            """
@p0='[-13579.01]' (Nullable = false) (Size = 4000)
@p1='[-13579.01]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDecimalCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDecimalCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_double()
    {
        await base.Edit_single_property_collection_of_double();

        AssertSql(
            """
@p0='[-1.23456789,1.23456789,0,-1.23579]' (Nullable = false) (Size = 4000)
@p1='[-1.23456789,1.23456789,0,-1.23579]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDoubleCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDoubleCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_guid()
    {
        await base.Edit_single_property_collection_of_guid();

        AssertSql(
            """
@p0='["12345678-1234-4321-5555-987654321000"]' (Nullable = false) (Size = 4000)
@p1='["12345678-1234-4321-5555-987654321000"]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestGuidCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestGuidCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_int16()
    {
        await base.Edit_single_property_collection_of_int16();

        AssertSql(
            """
@p0='[-3234]' (Nullable = false) (Size = 4000)
@p1='[-3234]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestInt16Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestInt16Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_int32()
    {
        await base.Edit_single_property_collection_of_int32();

        AssertSql(
            """
@p0='[-3234]' (Nullable = false) (Size = 4000)
@p1='[-3234]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestInt32Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestInt32Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_int64()
    {
        await base.Edit_single_property_collection_of_int64();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 4000)
@p1='[]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestInt64Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestInt64Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_signed_byte()
    {
        await base.Edit_single_property_collection_of_signed_byte();

        AssertSql(
            """
@p0='[-108]' (Nullable = false) (Size = 4000)
@p1='[-108]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestSignedByteCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestSignedByteCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_single()
    {
        await base.Edit_single_property_collection_of_single();

        AssertSql(
            """
@p0='[-1.234,-1.234]' (Nullable = false) (Size = 4000)
@p1='[0,-1.234]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestSingleCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestSingleCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_timespan()
    {
        await base.Edit_single_property_collection_of_timespan();

        AssertSql(
            """
@p0='["10:09:08.007","10:01:01.007"]' (Nullable = false) (Size = 4000)
@p1='["10:01:01.007","-9:50:51.993"]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestTimeSpanCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestTimeSpanCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_dateonly()
    {
        await base.Edit_single_property_collection_of_dateonly();

        AssertSql(
            """
@p0='["3234-01-23","0001-01-07"]' (Nullable = false) (Size = 4000)
@p1='["0001-01-07","4321-01-21"]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestDateOnlyCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestDateOnlyCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_timeonly()
    {
        await base.Edit_single_property_collection_of_timeonly();

        AssertSql(
            """
@p0='["13:42:23.0000000","01:01:07.0000000"]' (Nullable = false) (Size = 4000)
@p1='["01:01:07.0000000","07:17:27.0000000"]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestTimeOnlyCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestTimeOnlyCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_uint16()
    {
        await base.Edit_single_property_collection_of_uint16();

        AssertSql(
            """
@p0='[1534]' (Nullable = false) (Size = 4000)
@p1='[1534]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestUnsignedInt16Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestUnsignedInt16Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_uint32()
    {
        await base.Edit_single_property_collection_of_uint32();

        AssertSql(
            """
@p0='[1237775789]' (Nullable = false) (Size = 4000)
@p1='[1237775789]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestUnsignedInt32Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestUnsignedInt32Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_uint64()
    {
        await base.Edit_single_property_collection_of_uint64();

        AssertSql(
            """
@p0='[1234555555123456789]' (Nullable = false) (Size = 4000)
@p1='[1234555555123456789]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestUnsignedInt64Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestUnsignedInt64Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_int32()
    {
        await base.Edit_single_property_collection_of_nullable_int32();

        AssertSql(
            """
@p0='[null,77]' (Nullable = false) (Size = 4000)
@p1='[null,-2147483648,0,null,2147483647,null,77,null]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableInt32Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableInt32Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_int32_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_int32_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false) (Size = 4000)
@p1=NULL (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableInt32Collection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableInt32Collection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_enum()
    {
        await base.Edit_single_property_collection_of_enum();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4000)
@p1='[-3]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestEnumCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestEnumCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_enum_with_int_converter()
    {
        await base.Edit_single_property_collection_of_enum_with_int_converter();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4000)
@p1='[-3]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestEnumWithIntConverterCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestEnumWithIntConverterCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum()
    {
        await base.Edit_single_property_collection_of_nullable_enum();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4000)
@p1='[-3]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestEnumCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestEnumCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false) (Size = 4000)
@p1=NULL (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_int_converter()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_int_converter();

        AssertSql(
            """
@p0='[-1,null,-7,2]' (Nullable = false) (Size = 4000)
@p1='[-1,-3,-7,2]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithIntConverterCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithIntConverterCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_int_converter_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_int_converter_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false) (Size = 4000)
@p1=NULL (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithIntConverterCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithIntConverterCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls();

        AssertSql(
            """
@p0='[-3]' (Nullable = false) (Size = 4000)
@p1='[-1]' (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithConverterThatHandlesNullsCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithConverterThatHandlesNullsCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null();

        AssertSql(
            """
@p0=NULL (Nullable = false) (Size = 4000)
@p1=NULL (Nullable = false) (Size = 4000)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestNullableEnumWithConverterThatHandlesNullsCollection', JSON_QUERY(@p0)), [Reference] = JSON_MODIFY([Reference], 'strict $.TestNullableEnumWithConverterThatHandlesNullsCollection', JSON_QUERY(@p1))
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 1
""");
    }

    public override async Task Add_and_update_top_level_optional_owned_collection_to_JSON(bool? value)
    {
        await base.Add_and_update_top_level_optional_owned_collection_to_JSON(value);

        switch (value)
        {
            case true:
                AssertSql(
                """
@p0='[{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":null}]' (Nullable = false) (Size = 111)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedCollectionRoot], [Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2, @p3);
""",
                                //
                                """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""",
                                //
                                """
@p0=NULL (Nullable = false)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
                                //
                                """
select OwnedCollectionRoot from JsonEntitiesBasic where Id = 2
""",
                                //
                                """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""");
                break;
            case false:
                AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedCollectionRoot], [Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2, @p3);
""",
                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""",
                            //
                            """
@p0='[{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":null}]' (Nullable = false) (Size = 111)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",


                            //
                            """
select OwnedCollectionRoot from JsonEntitiesBasic where Id = 2
""",
                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""");
                break;
            default:
                AssertSql(
            """
@p0='2'
@p1=NULL (DbType = Int32)
@p2='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2);
""",

                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""",
                            //
                            """
@p0='[]' (Nullable = false) (Size = 2)
@p3='2'
@p1=NULL (DbType = Int32)
@p2='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedCollectionRoot] = @p0, [EntityBasicId] = @p1, [Name] = @p2
OUTPUT 1
WHERE [Id] = @p3;
""",
                            //
                            """
select OwnedCollectionRoot from JsonEntitiesBasic where Id = 2
""",
                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""");
                break;
        }
    }

    public override async Task Add_and_update_nested_optional_owned_collection_to_JSON(bool? value)
    {
        await base.Add_and_update_nested_optional_owned_collection_to_JSON(value);

        switch (value)
        {
            case true:
                AssertSql(
                """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":[{"Date":"0001-01-01T00:00:00","Enum":0,"Enums":null,"Fraction":0,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":null}],"OwnedReferenceBranch":null}' (Nullable = false) (Size = 266)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedReferenceRoot], [Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2, @p3);
""",
                                //
                                """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""",
                                //
                                """
@p0=NULL (Nullable = false)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
                                //
                                """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""");
                break;
            case false:
                AssertSql(
            """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":null}' (Nullable = false) (Size = 107)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedReferenceRoot], [Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2, @p3);
""",
                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""",
                            //
                            """
@p0='[{"Date":"0001-01-01T00:00:00","Enum":0,"Enums":null,"Fraction":0,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":null}]' (Nullable = false) (Size = 161)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = JSON_MODIFY([OwnedReferenceRoot], 'strict $.OwnedCollectionBranch', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""");
                break;
            default:
                AssertSql(
            """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":null}' (Nullable = false) (Size = 109)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesBasic] ([OwnedReferenceRoot], [Id], [EntityBasicId], [Name])
VALUES (@p0, @p1, @p2, @p3);
""",
                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""",
                            //
                            """
@p0='{"Name":null,"Names":null,"Number":0,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":null}' (Nullable = false) (Size = 107)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesBasic] SET [OwnedReferenceRoot] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
                            //
                            """
SELECT TOP(2) [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE [j].[Id] = 2
""");
                break;
        }
    }

    public override async Task Add_and_update_nested_optional_primitive_collection(bool? value)
    {
        await base.Add_and_update_nested_optional_primitive_collection(value);

        string characterCollection = value switch
        {
            true =>  "[\"A\"]",
            false => "[]",
            _ =>     "null"
        };

        string parameterSize = value switch
        {
            true => "1537",
            false => "1534",
            _ => "1536"
        };

        string updateParameter = value switch
        {
            true => "NULL",
            false => "'[\"Z\"]'",
            _ => "'[]'"
        };

        AssertSql(
            @"@p0='[{""TestBoolean"":false,""TestBooleanCollection"":[],""TestByte"":0,""TestByteCollection"":null,""TestCharacter"":""\u0000"",""TestCharacterCollection"":" + characterCollection + @",""TestDateOnly"":""0001-01-01"",""TestDateOnlyCollection"":[],""TestDateTime"":""0001-01-01T00:00:00"",""TestDateTimeCollection"":[],""TestDateTimeOffset"":""0001-01-01T00:00:00+00:00"",""TestDateTimeOffsetCollection"":[],""TestDecimal"":0,""TestDecimalCollection"":[],""TestDefaultString"":null,""TestDefaultStringCollection"":[],""TestDouble"":0,""TestDoubleCollection"":[],""TestEnum"":0,""TestEnumCollection"":[],""TestEnumWithIntConverter"":0,""TestEnumWithIntConverterCollection"":[],""TestGuid"":""00000000-0000-0000-0000-000000000000"",""TestGuidCollection"":[],""TestInt16"":0,""TestInt16Collection"":[],""TestInt32"":0,""TestInt32Collection"":[],""TestInt64"":0,""TestInt64Collection"":[],""TestMaxLengthString"":null,""TestMaxLengthStringCollection"":[],""TestNullableEnum"":null,""TestNullableEnumCollection"":[],""TestNullableEnumWithConverterThatHandlesNulls"":null,""TestNullableEnumWithConverterThatHandlesNullsCollection"":[],""TestNullableEnumWithIntConverter"":null,""TestNullableEnumWithIntConverterCollection"":[],""TestNullableInt32"":null,""TestNullableInt32Collection"":[],""TestSignedByte"":0,""TestSignedByteCollection"":[],""TestSingle"":0,""TestSingleCollection"":[],""TestTimeOnly"":""00:00:00.0000000"",""TestTimeOnlyCollection"":[],""TestTimeSpan"":""0:00:00"",""TestTimeSpanCollection"":[],""TestUnsignedInt16"":0,""TestUnsignedInt16Collection"":[],""TestUnsignedInt32"":0,""TestUnsignedInt32Collection"":[],""TestUnsignedInt64"":0,""TestUnsignedInt64Collection"":[]}]' (Nullable = false) (Size = " + parameterSize + @")
@p1='7624'
@p2='[]' (Size = 4000)
@p3=NULL (Size = 8000) (DbType = Binary)
@p4='[]' (Size = 4000)
@p5='[]' (Size = 4000)
@p6='[]' (Size = 4000)
@p7='[]' (Size = 4000)
@p8='[]' (Size = 4000)
@p9='[]' (Size = 4000)
@p10='[]' (Size = 4000)
@p11='[]' (Size = 4000)
@p12='[]' (Nullable = false) (Size = 4000)
@p13='[]' (Size = 4000)
@p14='[]' (Size = 4000)
@p15='[]' (Size = 4000)
@p16='[]' (Size = 4000)
@p17='[]' (Size = 4000)
@p18='[]' (Size = 4000)
@p19='[]' (Size = 4000)
@p20='[]' (Size = 4000)
@p21='[]' (Size = 4000)
@p22='[]' (Size = 4000)
@p23='[]' (Size = 4000)
@p24='[]' (Size = 4000)
@p25='[]' (Size = 4000)
@p26='[]' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [JsonEntitiesAllTypes] ([Collection], [Id], [TestBooleanCollection], [TestByteCollection], [TestCharacterCollection], [TestDateTimeCollection], [TestDateTimeOffsetCollection], [TestDecimalCollection], [TestDefaultStringCollection], [TestDoubleCollection], [TestEnumCollection], [TestEnumWithIntConverterCollection], [TestGuidCollection], [TestInt16Collection], [TestInt32Collection], [TestInt64Collection], [TestMaxLengthStringCollection], [TestNullableEnumCollection], [TestNullableEnumWithConverterThatHandlesNullsCollection], [TestNullableEnumWithIntConverterCollection], [TestNullableInt32Collection], [TestSignedByteCollection], [TestSingleCollection], [TestTimeSpanCollection], [TestUnsignedInt16Collection], [TestUnsignedInt32Collection], [TestUnsignedInt64Collection])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20, @p21, @p22, @p23, @p24, @p25, @p26);",
                //
                """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 7624
""",
                //

"@p0=" + updateParameter + @" (Nullable = false) (Size = 4000)
@p1='7624'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonEntitiesAllTypes] SET [Collection] = JSON_MODIFY([Collection], 'strict $[0].TestCharacterCollection', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;",
                //
                """
SELECT TOP(2) [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE [j].[Id] = 7624
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
