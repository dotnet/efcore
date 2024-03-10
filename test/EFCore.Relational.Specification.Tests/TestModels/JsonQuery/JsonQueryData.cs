// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonQueryData : ISetSource
{
    public JsonQueryData()
    {
        JsonEntitiesBasic = CreateJsonEntitiesBasic();
        EntitiesBasic = CreateEntitiesBasic();
        JsonEntitiesBasicForReference = CreateJsonEntitiesBasicForReference();
        JsonEntitiesBasicForCollection = CreateJsonEntitiesBasicForCollection();
        WireUp(JsonEntitiesBasic, EntitiesBasic, JsonEntitiesBasicForReference, JsonEntitiesBasicForCollection);

        JsonEntitiesCustomNaming = CreateJsonEntitiesCustomNaming();
        JsonEntitiesSingleOwned = CreateJsonEntitiesSingleOwned();
        JsonEntitiesInheritance = CreateJsonEntitiesInheritance();
        JsonEntitiesAllTypes = CreateJsonEntitiesAllTypes();
        JsonEntitiesConverters = CreateJsonEntitiesConverters();
    }

    public IReadOnlyList<EntityBasic> EntitiesBasic { get; }
    public IReadOnlyList<JsonEntityBasic> JsonEntitiesBasic { get; }
    public IReadOnlyList<JsonEntityBasicForReference> JsonEntitiesBasicForReference { get; }
    public IReadOnlyList<JsonEntityBasicForCollection> JsonEntitiesBasicForCollection { get; }
    public IReadOnlyList<JsonEntityCustomNaming> JsonEntitiesCustomNaming { get; set; }
    public IReadOnlyList<JsonEntitySingleOwned> JsonEntitiesSingleOwned { get; set; }
    public IReadOnlyList<JsonEntityInheritanceBase> JsonEntitiesInheritance { get; set; }
    public IReadOnlyList<JsonEntityAllTypes> JsonEntitiesAllTypes { get; set; }
    public IReadOnlyList<JsonEntityConverters> JsonEntitiesConverters { get; set; }

    public static IReadOnlyList<JsonEntityBasic> CreateJsonEntitiesBasic()
    {
        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_r_r_r = new JsonOwnedLeaf { SomethingSomething = "e1_r_r_r" };
        var e1_r_r_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_r_r_c1" };
        var e1_r_r_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_r_r_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_r_r = new JsonOwnedBranch
        {
            Date = new DateTime(2100, 1, 1),
            Fraction = 10.0M,
            Enum = JsonEnum.One,
            NullableEnum = null,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_r_r_r,
            OwnedCollectionLeaf = [e1_r_r_c1, e1_r_r_c2]
        };

        e1_r_r_r.Parent = e1_r_r;
        e1_r_r_c1.Parent = e1_r_r;
        e1_r_r_c2.Parent = e1_r_r;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_r_c1_r = new JsonOwnedLeaf { SomethingSomething = "e1_r_c1_r" };
        var e1_r_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_r_c1_c1" };
        var e1_r_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_r_c1_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_r_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2101, 1, 1),
            Fraction = 10.1M,
            Enum = JsonEnum.Two,
            NullableEnum = JsonEnum.One,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_r_c1_r,
            OwnedCollectionLeaf = [e1_r_c1_c1, e1_r_c1_c2]
        };

        e1_r_c1_r.Parent = e1_r_c1;
        e1_r_c1_c1.Parent = e1_r_c1;
        e1_r_c1_c2.Parent = e1_r_c1;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_r_c2_r = new JsonOwnedLeaf { SomethingSomething = "e1_r_c2_r" };
        var e1_r_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_r_c2_c1" };
        var e1_r_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_r_c2_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_r_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2102, 1, 1),
            Fraction = 10.2M,
            Enum = JsonEnum.Three,
            NullableEnum = JsonEnum.Two,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_r_c2_r,
            OwnedCollectionLeaf = [e1_r_c2_c1, e1_r_c2_c2]
        };

        e1_r_c2_r.Parent = e1_r_c2;
        e1_r_c2_c1.Parent = e1_r_c2;
        e1_r_c2_c2.Parent = e1_r_c2;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_r = new JsonOwnedRoot
        {
            Name = "e1_r",
            Number = 10,
            Names = ["e1_r1", "e1_r2"],
            Numbers = [int.MinValue, -1, 0, 1, int.MaxValue],
            OwnedReferenceBranch = e1_r_r,
            OwnedCollectionBranch = [e1_r_c1, e1_r_c2]
        };

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c1_r_r = new JsonOwnedLeaf { SomethingSomething = "e1_c1_r_r" };
        var e1_c1_r_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_c1_r_c1" };
        var e1_c1_r_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_c1_r_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_c1_r = new JsonOwnedBranch
        {
            Date = new DateTime(2110, 1, 1),
            Fraction = 11.0M,
            Enum = JsonEnum.One,
            NullableEnum = null,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_c1_r_r,
            OwnedCollectionLeaf = [e1_c1_r_c1, e1_c1_r_c2]
        };

        e1_c1_r_r.Parent = e1_c1_r;
        e1_c1_r_c1.Parent = e1_c1_r;
        e1_c1_r_c2.Parent = e1_c1_r;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c1_c1_r = new JsonOwnedLeaf { SomethingSomething = "e1_c1_c1_r" };
        var e1_c1_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_c1_c1_c1" };
        var e1_c1_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_c1_c1_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_c1_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2111, 1, 1),
            Fraction = 11.1M,
            Enum = JsonEnum.Two,
            NullableEnum = JsonEnum.One,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_c1_c1_r,
            OwnedCollectionLeaf = [e1_c1_c1_c1, e1_c1_c1_c2]
        };

        e1_c1_c1_r.Parent = e1_c1_c1;
        e1_c1_c1_c1.Parent = e1_c1_c1;
        e1_c1_c1_c2.Parent = e1_c1_c1;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c1_c2_r = new JsonOwnedLeaf { SomethingSomething = "e1_c1_c2_r" };
        var e1_c1_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_c1_c2_c1" };
        var e1_c1_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_c1_c2_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_c1_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2112, 1, 1),
            Fraction = 11.2M,
            Enum = JsonEnum.Three,
            NullableEnum = JsonEnum.Two,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_c1_c2_r,
            OwnedCollectionLeaf = [e1_c1_c2_c1, e1_c1_c2_c2]
        };

        e1_c1_c2_r.Parent = e1_c1_c2;
        e1_c1_c2_c1.Parent = e1_c1_c2;
        e1_c1_c2_c2.Parent = e1_c1_c2;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c1 = new JsonOwnedRoot
        {
            Name = "e1_c1",
            Number = 11,
            Names = ["e1_c11", "e1_c12"],
            Numbers = [-1000, 0, 1000],
            OwnedReferenceBranch = e1_c1_r,
            OwnedCollectionBranch = [e1_c1_c1, e1_c1_c2]
        };

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c2_r_r = new JsonOwnedLeaf { SomethingSomething = "e1_c2_r_r" };
        var e1_c2_r_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_c2_r_c1" };
        var e1_c2_r_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_c2_r_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_c2_r = new JsonOwnedBranch
        {
            Date = new DateTime(2120, 1, 1),
            Fraction = 12.0M,
            Enum = JsonEnum.Three,
            NullableEnum = JsonEnum.Two,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_c2_r_r,
            OwnedCollectionLeaf = [e1_c2_r_c1, e1_c2_r_c2]
        };

        e1_c2_r_r.Parent = e1_c2_r;
        e1_c2_r_c1.Parent = e1_c2_r;
        e1_c2_r_c2.Parent = e1_c2_r;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c2_c1_r = new JsonOwnedLeaf { SomethingSomething = "e1_c2_c1_r" };
        var e1_c2_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_c2_c1_c1" };
        var e1_c2_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_c2_c1_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_c2_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2121, 1, 1),
            Fraction = 12.1M,
            Enum = JsonEnum.Two,
            NullableEnum = JsonEnum.One,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_c2_c1_r,
            OwnedCollectionLeaf = [e1_c2_c1_c1, e1_c2_c1_c2]
        };

        e1_c2_c1_r.Parent = e1_c2_c1;
        e1_c2_c1_c1.Parent = e1_c2_c1;
        e1_c2_c1_c2.Parent = e1_c2_c1;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c2_c2_r = new JsonOwnedLeaf { SomethingSomething = "e1_c2_c2_r" };
        var e1_c2_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "e1_c2_c2_c1" };
        var e1_c2_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "e1_c2_c2_c2" };

        //-------------------------------------------------------------------------------------------

        var e1_c2_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2122, 1, 1),
            Fraction = 12.2M,
            Enum = JsonEnum.One,
            NullableEnum = null,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = e1_c2_c2_r,
            OwnedCollectionLeaf = [e1_c2_c2_c1, e1_c2_c2_c2]
        };

        e1_c2_c2_r.Parent = e1_c2_c2;
        e1_c2_c2_c1.Parent = e1_c2_c2;
        e1_c2_c2_c2.Parent = e1_c2_c2;

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var e1_c2 = new JsonOwnedRoot
        {
            Name = "e1_c2",
            Number = 12,
            Names = ["e1_c21", "e1_c22"],
            Numbers = [-1001, 0, 1001],
            OwnedReferenceBranch = e1_c2_r,
            OwnedCollectionBranch = [e1_c2_c1, e1_c2_c2]
        };

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var entity1 = new JsonEntityBasic
        {
            Id = 1,
            Name = "JsonEntityBasic1",
            OwnedReferenceRoot = e1_r,
            OwnedCollectionRoot = [e1_c1, e1_c2]
        };

        e1_r.Owner = entity1;
        e1_c1.Owner = entity1;
        e1_c2.Owner = entity1;

        return new List<JsonEntityBasic> { entity1 };
    }

    public static IReadOnlyList<EntityBasic> CreateEntitiesBasic()
    {
        var entity1 = new EntityBasic { Id = 1, Name = "eb 1" };

        return new List<EntityBasic> { entity1 };
    }

    public static IReadOnlyList<JsonEntityBasicForReference> CreateJsonEntitiesBasicForReference()
    {
        var entity1 = new JsonEntityBasicForReference();
        entity1.SetIdAndName(1, "EntityReference1");

        return new List<JsonEntityBasicForReference> { entity1 };
    }

    public static IReadOnlyList<JsonEntityBasicForCollection> CreateJsonEntitiesBasicForCollection()
    {
        var entity1 = new JsonEntityBasicForCollection { Id = 1, Name = "EntityCollection1" };
        var entity2 = new JsonEntityBasicForCollection { Id = 2, Name = "EntityCollection2" };
        var entity3 = new JsonEntityBasicForCollection { Id = 3, Name = "EntityCollection3" };

        return new List<JsonEntityBasicForCollection>
        {
            entity1,
            entity2,
            entity3
        };
    }

    public static void WireUp(
        IReadOnlyList<JsonEntityBasic> jsonEntitiesBasic,
        IReadOnlyList<EntityBasic> entitiesBasic,
        IReadOnlyList<JsonEntityBasicForReference> entitiesBasicForReference,
        IReadOnlyList<JsonEntityBasicForCollection> entitiesBasicForCollection)
    {
        entitiesBasic[0].JsonEntityBasics = [jsonEntitiesBasic[0]];

        jsonEntitiesBasic[0].EntityReference = entitiesBasicForReference[0];
        entitiesBasicForReference[0].Parent = jsonEntitiesBasic[0];
        entitiesBasicForReference[0].ParentId = jsonEntitiesBasic[0].Id;

        jsonEntitiesBasic[0].EntityCollection =
        [
            entitiesBasicForCollection[0],
            entitiesBasicForCollection[1],
            entitiesBasicForCollection[2]
        ];

        entitiesBasicForCollection[0].Parent = jsonEntitiesBasic[0];
        entitiesBasicForCollection[0].ParentId = jsonEntitiesBasic[0].Id;
        entitiesBasicForCollection[1].Parent = jsonEntitiesBasic[0];
        entitiesBasicForCollection[1].ParentId = jsonEntitiesBasic[0].Id;
        entitiesBasicForCollection[2].Parent = jsonEntitiesBasic[0];
        entitiesBasicForCollection[2].ParentId = jsonEntitiesBasic[0].Id;
    }

    public static IReadOnlyList<JsonEntityCustomNaming> CreateJsonEntitiesCustomNaming()
    {
        var e1_r_r = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2100, 1, 1), Fraction = 10.0,
        };

        var e1_r_c1 = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2101, 1, 1), Fraction = 10.1,
        };

        var e1_r_c2 = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2102, 1, 1), Fraction = 10.2,
        };

        var e1_r = new JsonOwnedCustomNameRoot
        {
            Name = "e1_r",
            Number = 10,
            Enum = JsonEnum.One,
            OwnedReferenceBranch = e1_r_r,
            OwnedCollectionBranch = [e1_r_c1, e1_r_c2]
        };

        var e1_c1_r = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2110, 1, 1), Fraction = 11.0,
        };

        var e1_c1_c1 = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2111, 1, 1), Fraction = 11.1,
        };

        var e1_c1_c2 = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2112, 1, 1), Fraction = 11.2,
        };

        var e1_c1 = new JsonOwnedCustomNameRoot
        {
            Name = "e1_c1",
            Number = 11,
            Enum = JsonEnum.Two,
            OwnedReferenceBranch = e1_c1_r,
            OwnedCollectionBranch = [e1_c1_c1, e1_c1_c2]
        };

        var e1_c2_r = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2120, 1, 1), Fraction = 12.0,
        };

        var e1_c2_c1 = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2121, 1, 1), Fraction = 12.1,
        };

        var e1_c2_c2 = new JsonOwnedCustomNameBranch
        {
            Date = new DateTime(2122, 1, 1), Fraction = 12.2,
        };

        var e1_c2 = new JsonOwnedCustomNameRoot
        {
            Name = "e1_c2",
            Number = 12,
            Enum = JsonEnum.Three,
            OwnedReferenceBranch = e1_c2_r,
            OwnedCollectionBranch = [e1_c2_c1, e1_c2_c2]
        };

        var entity1 = new JsonEntityCustomNaming
        {
            Id = 1,
            Title = "JsonEntityCustomNaming1",
            OwnedReferenceRoot = e1_r,
            OwnedCollectionRoot = [e1_c1, e1_c2]
        };

        return new List<JsonEntityCustomNaming> { entity1 };
    }

    public static IReadOnlyList<JsonEntitySingleOwned> CreateJsonEntitiesSingleOwned()
    {
        var e1 = new JsonEntitySingleOwned
        {
            Id = 1,
            Name = "JsonEntitySingleOwned1",
            OwnedCollection =
            [
                new() { SomethingSomething = "owned_1_1" },
                new() { SomethingSomething = "owned_1_2" },
                new() { SomethingSomething = "owned_1_3" }
            ]
        };

        var e2 = new JsonEntitySingleOwned
        {
            Id = 2,
            Name = "JsonEntitySingleOwned2",
            OwnedCollection = []
        };

        var e3 = new JsonEntitySingleOwned
        {
            Id = 3,
            Name = "JsonEntitySingleOwned3",
            OwnedCollection =
            [
                new() { SomethingSomething = "owned_3_1" }, new() { SomethingSomething = "owned_3_2" }
            ]
        };

        return new List<JsonEntitySingleOwned>
        {
            e1,
            e2,
            e3
        };
    }

    public static IReadOnlyList<JsonEntityInheritanceBase> CreateJsonEntitiesInheritance()
    {
        var b1_r_r = new JsonOwnedLeaf { SomethingSomething = "b1_r_r", };

        var b1_r_c1 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c1", };

        var b1_r_c2 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c2", };

        var b1_r = new JsonOwnedBranch
        {
            Date = new DateTime(2010, 1, 1),
            Fraction = 1.0M,
            Enum = JsonEnum.One,
            NullableEnum = null,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = b1_r_r,
            OwnedCollectionLeaf = [b1_r_c1, b1_r_c2]
        };

        var b1_c1_r = new JsonOwnedLeaf { SomethingSomething = "b1_r_r", };

        var b1_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c1", };

        var b1_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c2", };

        var b1_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2011, 1, 1),
            Fraction = 11.1M,
            Enum = JsonEnum.Three,
            NullableEnum = JsonEnum.Two,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = b1_c1_r,
            OwnedCollectionLeaf = [b1_c1_c1, b1_c1_c2]
        };

        var b1_c2_r = new JsonOwnedLeaf { SomethingSomething = "b1_r_r", };

        var b1_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c1", };

        var b1_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c2", };

        var b1_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2012, 1, 1),
            Fraction = 12.1M,
            Enum = JsonEnum.Two,
            NullableEnum = JsonEnum.One,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = b1_c2_r,
            OwnedCollectionLeaf = [b1_c2_c1, b1_c2_c2]
        };

        var b2_r_r = new JsonOwnedLeaf { SomethingSomething = "b2_r_r", };

        var b2_r_c1 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c1", };

        var b2_r_c2 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c2", };

        var b2_r = new JsonOwnedBranch
        {
            Date = new DateTime(2020, 1, 1),
            Fraction = 2.0M,
            Enum = JsonEnum.Two,
            NullableEnum = JsonEnum.One,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = b2_r_r,
            OwnedCollectionLeaf = [b2_r_c1, b2_r_c2]
        };

        var b2_c1_r = new JsonOwnedLeaf { SomethingSomething = "b2_r_r", };

        var b2_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c1", };

        var b2_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c2", };

        var b2_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2021, 1, 1),
            Fraction = 21.1M,
            Enum = JsonEnum.Three,
            NullableEnum = JsonEnum.Two,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = b2_c1_r,
            OwnedCollectionLeaf = [b2_c1_c1, b2_c1_c2]
        };

        var b2_c2_r = new JsonOwnedLeaf { SomethingSomething = "b2_r_r", };

        var b2_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c1", };

        var b2_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c2", };

        var b2_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2022, 1, 1),
            Fraction = 22.1M,
            Enum = JsonEnum.One,
            NullableEnum = null,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = b2_c2_r,
            OwnedCollectionLeaf = [b2_c2_c1, b2_c2_c2]
        };

        var d2_r_r = new JsonOwnedLeaf { SomethingSomething = "d2_r_r", };

        var d2_r_c1 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c1", };

        var d2_r_c2 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c2", };

        var d2_r = new JsonOwnedBranch
        {
            Date = new DateTime(2220, 1, 1),
            Fraction = 22.0M,
            Enum = JsonEnum.One,
            NullableEnum = null,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = d2_r_r,
            OwnedCollectionLeaf = [d2_r_c1, d2_r_c2]
        };

        var d2_c1_r = new JsonOwnedLeaf { SomethingSomething = "d2_r_r", };

        var d2_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c1", };

        var d2_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c2", };

        var d2_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2221, 1, 1),
            Fraction = 221.1M,
            Enum = JsonEnum.Two,
            NullableEnum = JsonEnum.One,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = d2_c1_r,
            OwnedCollectionLeaf = [d2_c1_c1, d2_c1_c2]
        };

        var d2_c2_r = new JsonOwnedLeaf { SomethingSomething = "d2_r_r", };

        var d2_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c1", };

        var d2_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c2", };

        var d2_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2222, 1, 1),
            Fraction = 222.1M,
            Enum = JsonEnum.Three,
            NullableEnum = JsonEnum.Two,
            Enums = [JsonEnum.One, (JsonEnum)(-1), JsonEnum.Two],
            NullableEnums = [null, (JsonEnum)(-1), JsonEnum.Two],
            OwnedReferenceLeaf = d2_c2_r,
            OwnedCollectionLeaf = [d2_c2_c1, d2_c2_c2]
        };

        var baseEntity = new JsonEntityInheritanceBase
        {
            Id = 1,
            Name = "JsonEntityInheritanceBase1",
            ReferenceOnBase = b1_r,
            CollectionOnBase = [b1_c1, b1_c2]
        };

        var derivedEntity = new JsonEntityInheritanceDerived
        {
            Id = 2,
            Name = "JsonEntityInheritanceDerived2",
            ReferenceOnBase = b2_r,
            CollectionOnBase = [b2_c1, b2_c2],
            ReferenceOnDerived = d2_r,
            CollectionOnDerived = [d2_c1, d2_c2],
        };

        return new List<JsonEntityInheritanceBase> { baseEntity, derivedEntity };
    }

    public static IReadOnlyList<JsonEntityAllTypes> CreateJsonEntitiesAllTypes()
    {
        var r1 = new JsonOwnedAllTypes
        {
            TestDefaultString = "MyDefaultStringInReference1",
            TestMaxLengthString = "Foo",
            TestInt16 = -1234,
            TestInt32 = -123456789,
            TestInt64 = -1234567890123456789L,
            TestDouble = -1.23456789,
            TestDecimal = -1234567890.01M,
            TestDateTime = DateTime.Parse("01/01/2000 12:34:56"),
            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
            TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
            TestDateOnly = new DateOnly(2023, 10, 10),
            TestTimeOnly = new TimeOnly(11, 12, 13),
            TestSingle = -1.234F,
            TestBoolean = true,
            TestByte = 255,
            TestGuid = new Guid("12345678-1234-4321-7777-987654321000"),
            TestUnsignedInt16 = 1234,
            TestUnsignedInt32 = 1234565789U,
            TestUnsignedInt64 = 1234567890123456789UL,
            TestCharacter = 'a',
            TestSignedByte = -128,
            TestNullableInt32 = 78,
            TestEnum = JsonEnum.One,
            TestEnumWithIntConverter = JsonEnum.Two,
            TestNullableEnum = JsonEnum.One,
            TestNullableEnumWithIntConverter = JsonEnum.Two,
            TestNullableEnumWithConverterThatHandlesNulls = JsonEnum.Three,
            TestDefaultStringCollection = ["S1", "\"S2\"", "S3"],
            TestMaxLengthStringCollection =
            [
                "S1",
                "S2",
                "S3"
            ],
            TestBooleanCollection = new[] { true, false },
            TestCharacterCollection =
            [
                'A',
                'B',
                '\"'
            ],
            TestDateTimeCollection = [DateTime.Parse("01/01/2000 12:34:56"), DateTime.Parse("01/01/3000 12:34:56")],
            TestDateTimeOffsetCollection = new[] { new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)) },
            TestDoubleCollection = new[] { -1.23456789, 1.23456789, 0.0 },
            TestDecimalCollection = [-1234567890.01M],
            TestGuidCollection = [new("12345678-1234-4321-7777-987654321000")],
            TestInt16Collection = new[] { short.MinValue, (short)0, short.MaxValue },
            TestInt32Collection = [int.MinValue, 0, int.MaxValue],
            TestInt64Collection =
            [
                long.MinValue,
                0,
                long.MaxValue
            ],
            TestSignedByteCollection = [sbyte.MinValue, (sbyte)0, sbyte.MaxValue],
            TestSingleCollection =
            [
                -1.234F,
                0.0F,
                -1.234F
            ],
            TestTimeSpanCollection = [new TimeSpan(0, 10, 9, 8, 7), new TimeSpan(0, -10, 9, 8, 7)],
            TestDateOnlyCollection = [new DateOnly(1234, 1, 23), new DateOnly(4321, 1, 21)],
            TestTimeOnlyCollection = [new TimeOnly(11, 42, 23), new TimeOnly(7, 17, 27)],
            TestUnsignedInt16Collection = new List<ushort>
            {
                ushort.MinValue,
                0,
                ushort.MaxValue
            },
            TestUnsignedInt32Collection = [uint.MinValue, (uint)0, uint.MaxValue],
            TestUnsignedInt64Collection =
            [
                ulong.MinValue,
                0,
                ulong.MaxValue
            ],
            TestNullableInt32Collection =
            [
                null,
                int.MinValue,
                0,
                null,
                int.MaxValue,
                null
            ],
            TestEnumCollection = new[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7) },
            TestEnumWithIntConverterCollection = [JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7)],
            TestNullableEnumCollection =
            [
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            ],
            TestNullableEnumWithIntConverterCollection =
            [
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            ],
            TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One, null, (JsonEnum)(-7)]
        };

        var r2 = new JsonOwnedAllTypes
        {
            TestDefaultString = "MyDefaultStringInReference2",
            TestMaxLengthString = "Bar",
            TestInt16 = -123,
            TestInt32 = -12356789,
            TestInt64 = -123567890123456789L,
            TestDouble = -1.2346789,
            TestDecimal = -123567890.01M,
            TestDateTime = DateTime.Parse("01/01/3000 12:34:56"),
            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-8.0)),
            TestTimeSpan = new TimeSpan(0, 5, 9, 8, 7),
            TestDateOnly = new DateOnly(2123, 7, 8),
            TestTimeOnly = new TimeOnly(9, 10, 11),
            TestSingle = -1.24F,
            TestBoolean = true,
            TestByte = 25,
            TestGuid = new Guid("12345678-1243-4321-7777-987654321000"),
            TestUnsignedInt16 = 134,
            TestUnsignedInt32 = 123565789U,
            TestUnsignedInt64 = 123567890123456789UL,
            TestCharacter = 'b',
            TestSignedByte = -18,
            TestNullableInt32 = null,
            TestEnum = JsonEnum.Two,
            TestEnumWithIntConverter = JsonEnum.Three,
            TestNullableEnum = null,
            TestNullableEnumWithIntConverter = null,
            TestNullableEnumWithConverterThatHandlesNulls = null,
            TestDefaultStringCollection = ["S1", "\"S2\"", "S3"],
            TestMaxLengthStringCollection =
            [
                "S1",
                "S2",
                "S3"
            ],
            TestBooleanCollection = new[] { true, false },
            TestCharacterCollection =
            [
                'A',
                'B',
                '\"'
            ],
            TestDateTimeCollection = [DateTime.Parse("01/01/2000 12:34:56"), DateTime.Parse("01/01/3000 12:34:56")],
            TestDateTimeOffsetCollection = new[] { new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)) },
            TestDoubleCollection = new[] { -1.23456789, 1.23456789, 0.0 },
            TestDecimalCollection = [-1234567890.01M],
            TestGuidCollection = [new("12345678-1234-4321-7777-987654321000")],
            TestInt16Collection = new[] { short.MinValue, (short)0, short.MaxValue },
            TestInt32Collection = [int.MinValue, 0, int.MaxValue],
            TestInt64Collection =
            [
                long.MinValue,
                0,
                long.MaxValue
            ],
            TestSignedByteCollection = [sbyte.MinValue, (sbyte)0, sbyte.MaxValue],
            TestSingleCollection =
            [
                -1.234F,
                0.0F,
                -1.234F
            ],
            TestTimeSpanCollection = [new TimeSpan(0, 10, 9, 8, 7), new TimeSpan(0, -10, 9, 8, 7)],
            TestDateOnlyCollection = [new DateOnly(2234, 1, 23), new DateOnly(5321, 1, 21)],
            TestTimeOnlyCollection = [new TimeOnly(21, 42, 23), new TimeOnly(17, 17, 27)],
            TestUnsignedInt16Collection = new[] { ushort.MinValue, (ushort)0, ushort.MaxValue },
            TestUnsignedInt32Collection = [uint.MinValue, (uint)0, uint.MaxValue],
            TestUnsignedInt64Collection =
            [
                ulong.MinValue,
                0,
                ulong.MaxValue
            ],
            TestNullableInt32Collection =
            [
                null,
                int.MinValue,
                0,
                null,
                int.MaxValue,
                null
            ],
            TestEnumCollection = new[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7) },
            TestEnumWithIntConverterCollection = [JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7)],
            TestNullableEnumCollection =
            [
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            ],
            TestNullableEnumWithIntConverterCollection =
            [
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            ],
            TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One, null, (JsonEnum)(-7)]
        };

        var c1 = new JsonOwnedAllTypes
        {
            TestDefaultString = "MyDefaultStringInCollection1",
            TestMaxLengthString = "Baz",
            TestInt16 = -12,
            TestInt32 = -12345,
            TestInt64 = -1234567890L,
            TestDouble = -1.2345,
            TestDecimal = -123450.01M,
            TestDateTime = DateTime.Parse("11/11/2100 12:34:56"),
            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("11/11/2200 12:34:56"), TimeSpan.FromHours(-5.0)),
            TestTimeSpan = new TimeSpan(0, 6, 5, 4, 3),
            TestDateOnly = new DateOnly(2323, 4, 3),
            TestTimeOnly = new TimeOnly(5, 7, 8),
            TestSingle = -1.4F,
            TestBoolean = false,
            TestByte = 25,
            TestGuid = new Guid("00000000-0000-0000-0000-000000000000"),
            TestUnsignedInt16 = 12,
            TestUnsignedInt32 = 12345U,
            TestUnsignedInt64 = 1234567867UL,
            TestCharacter = 'h',
            TestSignedByte = -18,
            TestNullableInt32 = 90,
            TestEnum = JsonEnum.One,
            TestEnumWithIntConverter = JsonEnum.Two,
            TestNullableEnum = JsonEnum.One,
            TestNullableEnumWithIntConverter = JsonEnum.Three,
            TestNullableEnumWithConverterThatHandlesNulls = JsonEnum.Two,
            TestDefaultStringCollection = ["S1", "\"S2\"", "S3"],
            TestMaxLengthStringCollection =
            [
                "S1",
                "S2",
                "S3"
            ],
            TestBooleanCollection = new[] { true, false },
            TestCharacterCollection =
            [
                'A',
                'B',
                '\"'
            ],
            TestDateTimeCollection = [DateTime.Parse("01/01/2000 12:34:56"), DateTime.Parse("01/01/3000 12:34:56")],
            TestDateTimeOffsetCollection = new[] { new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)) },
            TestDoubleCollection = new[] { -1.23456789, 1.23456789, 0.0 },
            TestDecimalCollection = [-1234567890.01M],
            TestGuidCollection = [new("12345678-1234-4321-7777-987654321000")],
            TestInt16Collection = new[] { short.MinValue, (short)0, short.MaxValue },
            TestInt32Collection = [int.MinValue, 0, int.MaxValue],
            TestInt64Collection =
            [
                long.MinValue,
                0,
                long.MaxValue
            ],
            TestSignedByteCollection = [sbyte.MinValue, (sbyte)0, sbyte.MaxValue],
            TestSingleCollection =
            [
                -1.234F,
                0.0F,
                -1.234F
            ],
            TestTimeSpanCollection = [new TimeSpan(0, 10, 9, 8, 7), new TimeSpan(0, -10, 9, 8, 7)],
            TestDateOnlyCollection = [new DateOnly(3234, 1, 23), new DateOnly(4331, 1, 21)],
            TestTimeOnlyCollection = [new TimeOnly(13, 42, 23), new TimeOnly(7, 17, 25)],
            TestUnsignedInt16Collection = new[] { ushort.MinValue, (ushort)0, ushort.MaxValue },
            TestUnsignedInt32Collection = [uint.MinValue, (uint)0, uint.MaxValue],
            TestUnsignedInt64Collection =
            [
                ulong.MinValue,
                0,
                ulong.MaxValue
            ],
            TestNullableInt32Collection =
            [
                null,
                int.MinValue,
                0,
                null,
                int.MaxValue,
                null
            ],
            TestEnumCollection = new[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7) },
            TestEnumWithIntConverterCollection = [JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7)],
            TestNullableEnumCollection = new ObservableCollection<JsonEnum?>
            {
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            },
            TestNullableEnumWithIntConverterCollection = new ObservableCollection<JsonEnum?>
            {
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            },
            TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One, null, (JsonEnum)(-7)]
        };

        var c2 = new JsonOwnedAllTypes
        {
            TestDefaultString = "MyDefaultStringInCollection2",
            TestMaxLengthString = "Qux",
            TestInt16 = -1,
            TestInt32 = -1245,
            TestInt64 = -123567890L,
            TestDouble = -1.235,
            TestDecimal = -12350.01M,
            TestDateTime = DateTime.Parse("11/11/3100 12:34:56"),
            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("11/11/3200 12:34:56"), TimeSpan.FromHours(-5.0)),
            TestTimeSpan = new TimeSpan(0, 6, 5, 2, 3),
            TestDateOnly = new DateOnly(4019, 2, 25),
            TestTimeOnly = new TimeOnly(5, 30, 42),
            TestSingle = -1.4F,
            TestBoolean = false,
            TestByte = 25,
            TestGuid = new Guid("00000000-0000-0000-0000-000000100000"),
            TestUnsignedInt16 = 1,
            TestUnsignedInt32 = 1245U,
            TestUnsignedInt64 = 124567867UL,
            TestCharacter = 'g',
            TestSignedByte = -8,
            TestNullableInt32 = null,
            TestEnum = JsonEnum.Two,
            TestEnumWithIntConverter = JsonEnum.Three,
            TestNullableEnum = null,
            TestNullableEnumWithIntConverter = null,
            TestNullableEnumWithConverterThatHandlesNulls = null,
            TestDefaultStringCollection = ["S1", "\"S2\"", "S3"],
            TestMaxLengthStringCollection =
            [
                "S1",
                "S2",
                "S3"
            ],
            TestBooleanCollection = new[] { true, false },
            TestCharacterCollection =
            [
                'A',
                'B',
                '\"'
            ],
            TestDateTimeCollection = [DateTime.Parse("01/01/2000 12:34:56"), DateTime.Parse("01/01/3000 12:34:56")],
            TestDateTimeOffsetCollection = new[] { new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)) },
            TestDoubleCollection = new[] { -1.23456789, 1.23456789, 0.0 },
            TestDecimalCollection = [-1234567890.01M],
            TestGuidCollection = [new("12345678-1234-4321-7777-987654321000")],
            TestInt16Collection = new[] { short.MinValue, (short)0, short.MaxValue },
            TestInt32Collection = [int.MinValue, 0, int.MaxValue],
            TestInt64Collection =
            [
                long.MinValue,
                0,
                long.MaxValue
            ],
            TestSignedByteCollection = [sbyte.MinValue, (sbyte)0, sbyte.MaxValue],
            TestSingleCollection =
            [
                -1.234F,
                0.0F,
                -1.234F
            ],
            TestTimeSpanCollection = [new TimeSpan(0, 10, 9, 8, 7), new TimeSpan(0, -10, 9, 8, 7)],
            TestDateOnlyCollection = [new DateOnly(1638, 1, 23), new DateOnly(4321, 1, 21)],
            TestTimeOnlyCollection = [new TimeOnly(8, 22, 23), new TimeOnly(7, 27, 37)],
            TestUnsignedInt16Collection = new[] { ushort.MinValue, (ushort)0, ushort.MaxValue },
            TestUnsignedInt32Collection = [uint.MinValue, (uint)0, uint.MaxValue],
            TestUnsignedInt64Collection =
            [
                ulong.MinValue,
                0,
                ulong.MaxValue
            ],
            TestNullableInt32Collection =
            [
                null,
                int.MinValue,
                0,
                null,
                int.MaxValue,
                null
            ],
            TestEnumCollection = new[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7) },
            TestEnumWithIntConverterCollection = [JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7)],
            TestNullableEnumCollection = new ObservableCollection<JsonEnum?>
            {
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            },
            TestNullableEnumWithIntConverterCollection = new ObservableCollection<JsonEnum?>
            {
                JsonEnum.One,
                null,
                JsonEnum.Three,
                (JsonEnum)(-7)
            },
            TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One, null, (JsonEnum)(-7)]
        };

        return new List<JsonEntityAllTypes>
        {
            new()
            {
                Id = 1,
                Reference = r1,
                Collection = [c1],
                TestDefaultStringCollection = ["S1", "\"S2\"", "S3"],
                TestMaxLengthStringCollection =
                [
                    "S1",
                    "S2",
                    "S3"
                ],
                TestBooleanCollection = new[] { true, false },
                TestCharacterCollection =
                [
                    'A',
                    'B',
                    '\"'
                ],
                TestDateTimeCollection =
                    [DateTime.Parse("01/01/2000 12:34:56"), DateTime.Parse("01/01/3000 12:34:56")],
                TestDateTimeOffsetCollection =
                    new[] { new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)) },
                TestDoubleCollection = new[] { -1.23456789, 1.23456789, 0.0 },
                TestDecimalCollection = [-1234567890.01M],
                TestGuidCollection = [new("12345678-1234-4321-7777-987654321000")],
                TestInt16Collection = new[] { short.MinValue, (short)0, short.MaxValue },
                TestInt32Collection = [int.MinValue, 0, int.MaxValue],
                TestInt64Collection =
                [
                    long.MinValue,
                    0,
                    long.MaxValue
                ],
                TestSignedByteCollection = [sbyte.MinValue, (sbyte)0, sbyte.MaxValue],
                TestSingleCollection =
                [
                    -1.234F,
                    0.0F,
                    -1.234F
                ],
                TestTimeSpanCollection = [new TimeSpan(0, 10, 9, 8, 7), new TimeSpan(0, 7, 9, 8, 7)],
                TestUnsignedInt16Collection = new List<ushort>
                {
                    ushort.MinValue,
                    0,
                    ushort.MaxValue
                },
                TestUnsignedInt32Collection = [uint.MinValue, (uint)0, uint.MaxValue],
                TestUnsignedInt64Collection =
                [
                    ulong.MinValue,
                    0,
                    ulong.MaxValue
                ],
                TestNullableInt32Collection =
                [
                    null,
                    int.MinValue,
                    0,
                    null,
                    int.MaxValue,
                    null
                ],
                TestEnumCollection = new[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7) },
                TestEnumWithIntConverterCollection = [JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7)],
                TestNullableEnumCollection =
                [
                    JsonEnum.One,
                    null,
                    JsonEnum.Three,
                    (JsonEnum)(-7)
                ],
                TestNullableEnumWithIntConverterCollection =
                [
                    JsonEnum.One,
                    null,
                    JsonEnum.Three,
                    (JsonEnum)(-7)
                ],
                TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One, null, (JsonEnum)(-7)]
            },
            new()
            {
                Id = 2,
                Reference = r2,
                Collection = [c2],
                TestDefaultStringCollection = ["S1", "\"S2\"", "S3"],
                TestMaxLengthStringCollection =
                [
                    "S1",
                    "S2",
                    "S3"
                ],
                TestBooleanCollection = new[] { true, false },
                TestCharacterCollection =
                [
                    'A',
                    'B',
                    '\"'
                ],
                TestDateTimeCollection =
                    [DateTime.Parse("01/01/2000 12:34:56"), DateTime.Parse("01/01/3000 12:34:56")],
                TestDateTimeOffsetCollection =
                    new[] { new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)) },
                TestDoubleCollection = new[] { -1.23456789, 1.23456789, 0.0 },
                TestDecimalCollection = [-1234567890.01M],
                TestGuidCollection = [new("12345678-1234-4321-7777-987654321000")],
                TestInt16Collection = new[] { short.MinValue, (short)0, short.MaxValue },
                TestInt32Collection = [int.MinValue, 0, int.MaxValue],
                TestInt64Collection =
                [
                    long.MinValue,
                    0,
                    long.MaxValue
                ],
                TestSignedByteCollection = [sbyte.MinValue, (sbyte)0, sbyte.MaxValue],
                TestSingleCollection =
                [
                    -1.234F,
                    0.0F,
                    -1.234F
                ],
                TestTimeSpanCollection = [new TimeSpan(0, 10, 9, 8, 7), new TimeSpan(0, 7, 9, 8, 7)],
                TestUnsignedInt16Collection = new List<ushort>
                {
                    ushort.MinValue,
                    0,
                    ushort.MaxValue
                },
                TestUnsignedInt32Collection = [uint.MinValue, (uint)0, uint.MaxValue],
                TestUnsignedInt64Collection =
                [
                    ulong.MinValue,
                    0,
                    ulong.MaxValue
                ],
                TestNullableInt32Collection =
                [
                    null,
                    int.MinValue,
                    0,
                    null,
                    int.MaxValue,
                    null
                ],
                TestEnumCollection = new[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7) },
                TestEnumWithIntConverterCollection = [JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7)],
                TestNullableEnumCollection =
                [
                    JsonEnum.One,
                    null,
                    JsonEnum.Three,
                    (JsonEnum)(-7)
                ],
                TestNullableEnumWithIntConverterCollection =
                [
                    JsonEnum.One,
                    null,
                    JsonEnum.Three,
                    (JsonEnum)(-7)
                ],
                TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One, null, (JsonEnum)(-7)]
            }
        };
    }

    public static IReadOnlyList<JsonEntityConverters> CreateJsonEntitiesConverters()
    {
        var r1 = new JsonOwnedConverters
        {
            BoolConvertedToIntZeroOne = true,
            BoolConvertedToStringTrueFalse = false,
            BoolConvertedToStringYN = true,
            IntZeroOneConvertedToBool = 0,
            StringTrueFalseConvertedToBool = "True",
            StringYNConvertedToBool = "N",
        };

        var r2 = new JsonOwnedConverters
        {
            BoolConvertedToIntZeroOne = false,
            BoolConvertedToStringTrueFalse = true,
            BoolConvertedToStringYN = false,
            IntZeroOneConvertedToBool = 1,
            StringTrueFalseConvertedToBool = "False",
            StringYNConvertedToBool = "Y",
        };

        return new List<JsonEntityConverters>
        {
            new()
            {
                Id = 1, Reference = r1,
            },
            new()
            {
                Id = 2, Reference = r2,
            }
        };
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(EntityBasic))
        {
            return (IQueryable<TEntity>)EntitiesBasic.AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityBasic))
        {
            return (IQueryable<TEntity>)JsonEntitiesBasic.AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityCustomNaming))
        {
            return (IQueryable<TEntity>)JsonEntitiesCustomNaming.AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntitySingleOwned))
        {
            return (IQueryable<TEntity>)JsonEntitiesSingleOwned.AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityInheritanceBase))
        {
            return (IQueryable<TEntity>)JsonEntitiesInheritance.AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityInheritanceDerived))
        {
            return (IQueryable<TEntity>)JsonEntitiesInheritance.OfType<JsonEntityInheritanceDerived>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityAllTypes))
        {
            return (IQueryable<TEntity>)JsonEntitiesAllTypes.OfType<JsonEntityAllTypes>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityConverters))
        {
            return (IQueryable<TEntity>)JsonEntitiesConverters.OfType<JsonEntityConverters>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityBasicForReference))
        {
            return (IQueryable<TEntity>)JsonEntitiesBasicForReference.AsQueryable();
        }

        if (typeof(TEntity) == typeof(JsonEntityBasicForCollection))
        {
            return (IQueryable<TEntity>)JsonEntitiesBasicForCollection.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }
}
