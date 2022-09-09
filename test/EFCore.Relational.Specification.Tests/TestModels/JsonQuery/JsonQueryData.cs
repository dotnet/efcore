// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonQueryData : ISetSource
{
    public JsonQueryData()
    {
        EntitiesBasic = new List<EntityBasic>();
        JsonEntitiesBasic = CreateJsonEntitiesBasic();
        JsonEntitiesBasicForReference = CreateJsonEntitiesBasicForReference();
        JsonEntitiesBasicForCollection = CreateJsonEntitiesBasicForCollection();
        WireUp(JsonEntitiesBasic, JsonEntitiesBasicForReference, JsonEntitiesBasicForCollection);

        JsonEntitiesCustomNaming = CreateJsonEntitiesCustomNaming();
        JsonEntitiesSingleOwned = CreateJsonEntitiesSingleOwned();
        JsonEntitiesInheritance = CreateJsonEntitiesInheritance();
        JsonEntitiesAllTypes = CreateJsonEntitiesAllTypes();
    }

    public IReadOnlyList<EntityBasic> EntitiesBasic { get; }
    public IReadOnlyList<JsonEntityBasic> JsonEntitiesBasic { get; }
    public IReadOnlyList<JsonEntityBasicForReference> JsonEntitiesBasicForReference { get; }
    public IReadOnlyList<JsonEntityBasicForCollection> JsonEntitiesBasicForCollection { get; }
    public IReadOnlyList<JsonEntityCustomNaming> JsonEntitiesCustomNaming { get; set; }
    public IReadOnlyList<JsonEntitySingleOwned> JsonEntitiesSingleOwned { get; set; }
    public IReadOnlyList<JsonEntityInheritanceBase> JsonEntitiesInheritance { get; set; }
    public IReadOnlyList<JsonEntityAllTypes> JsonEntitiesAllTypes { get; set; }

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
            OwnedReferenceLeaf = e1_r_r_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_r_r_c1, e1_r_r_c2 }
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
            OwnedReferenceLeaf = e1_r_c1_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_r_c1_c1, e1_r_c1_c2 }
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
            OwnedReferenceLeaf = e1_r_c2_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_r_c2_c1, e1_r_c2_c2 }
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
            OwnedReferenceBranch = e1_r_r,
            OwnedCollectionBranch = new List<JsonOwnedBranch> { e1_r_c1, e1_r_c2 }
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
            OwnedReferenceLeaf = e1_c1_r_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_c1_r_c1, e1_c1_r_c2 }
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
            OwnedReferenceLeaf = e1_c1_c1_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_c1_c1_c1, e1_c1_c1_c2 }
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
            OwnedReferenceLeaf = e1_c1_c2_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_c1_c2_c1, e1_c1_c2_c2 }
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
            OwnedReferenceBranch = e1_c1_r,
            OwnedCollectionBranch = new List<JsonOwnedBranch> { e1_c1_c1, e1_c1_c2 }
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
            OwnedReferenceLeaf = e1_c2_r_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_c2_r_c1, e1_c2_r_c2 }
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
            OwnedReferenceLeaf = e1_c2_c1_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_c2_c1_c1, e1_c2_c1_c2 }
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
            OwnedReferenceLeaf = e1_c2_c2_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { e1_c2_c2_c1, e1_c2_c2_c2 }
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
            OwnedReferenceBranch = e1_c2_r,
            OwnedCollectionBranch = new List<JsonOwnedBranch> { e1_c2_c1, e1_c2_c2 }
        };

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        var entity1 = new JsonEntityBasic
        {
            Id = 1,
            Name = "JsonEntityBasic1",
            OwnedReferenceRoot = e1_r,
            OwnedCollectionRoot = new List<JsonOwnedRoot> { e1_c1, e1_c2 }
        };

        e1_r.Owner = entity1;
        e1_c1.Owner = entity1;
        e1_c2.Owner = entity1;

        return new List<JsonEntityBasic> { entity1 };
    }

    public static IReadOnlyList<JsonEntityBasicForReference> CreateJsonEntitiesBasicForReference()
    {
        var entity1 = new JsonEntityBasicForReference { Id = 1, Name = "EntityReference1" };

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
        IReadOnlyList<JsonEntityBasic> entitiesBasic,
        IReadOnlyList<JsonEntityBasicForReference> entitiesBasicForReference,
        IReadOnlyList<JsonEntityBasicForCollection> entitiesBasicForCollection)
    {
        entitiesBasic[0].EntityReference = entitiesBasicForReference[0];
        entitiesBasicForReference[0].Parent = entitiesBasic[0];
        entitiesBasicForReference[0].ParentId = entitiesBasic[0].Id;

        entitiesBasic[0].EntityCollection = new List<JsonEntityBasicForCollection>
        {
            entitiesBasicForCollection[0],
            entitiesBasicForCollection[1],
            entitiesBasicForCollection[2]
        };

        entitiesBasicForCollection[0].Parent = entitiesBasic[0];
        entitiesBasicForCollection[0].ParentId = entitiesBasic[0].Id;
        entitiesBasicForCollection[1].Parent = entitiesBasic[0];
        entitiesBasicForCollection[1].ParentId = entitiesBasic[0].Id;
        entitiesBasicForCollection[2].Parent = entitiesBasic[0];
        entitiesBasicForCollection[2].ParentId = entitiesBasic[0].Id;
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
            OwnedCollectionBranch = new List<JsonOwnedCustomNameBranch> { e1_r_c1, e1_r_c2 }
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
            OwnedCollectionBranch = new List<JsonOwnedCustomNameBranch> { e1_c1_c1, e1_c1_c2 }
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
            OwnedCollectionBranch = new List<JsonOwnedCustomNameBranch> { e1_c2_c1, e1_c2_c2 }
        };

        var entity1 = new JsonEntityCustomNaming
        {
            Id = 1,
            Title = "JsonEntityCustomNaming1",
            OwnedReferenceRoot = e1_r,
            OwnedCollectionRoot = new List<JsonOwnedCustomNameRoot> { e1_c1, e1_c2 }
        };

        return new List<JsonEntityCustomNaming> { entity1 };
    }

    public static IReadOnlyList<JsonEntitySingleOwned> CreateJsonEntitiesSingleOwned()
    {
        var e1 = new JsonEntitySingleOwned
        {
            Id = 1,
            Name = "JsonEntitySingleOwned1",
            OwnedCollection = new List<JsonOwnedLeaf>
            {
                new() { SomethingSomething = "owned_1_1" },
                new() { SomethingSomething = "owned_1_2" },
                new() { SomethingSomething = "owned_1_3" },
            }
        };

        var e2 = new JsonEntitySingleOwned
        {
            Id = 2,
            Name = "JsonEntitySingleOwned2",
            OwnedCollection = new List<JsonOwnedLeaf>()
        };

        var e3 = new JsonEntitySingleOwned
        {
            Id = 3,
            Name = "JsonEntitySingleOwned3",
            OwnedCollection = new List<JsonOwnedLeaf>
            {
                new() { SomethingSomething = "owned_3_1" }, new() { SomethingSomething = "owned_3_2" },
            }
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
            OwnedReferenceLeaf = b1_r_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { b1_r_c1, b1_r_c2 }
        };

        var b1_c1_r = new JsonOwnedLeaf { SomethingSomething = "b1_r_r", };

        var b1_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c1", };

        var b1_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c2", };

        var b1_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2011, 1, 1),
            Fraction = 11.1M,
            Enum = JsonEnum.Three,
            OwnedReferenceLeaf = b1_c1_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { b1_c1_c1, b1_c1_c2 }
        };

        var b1_c2_r = new JsonOwnedLeaf { SomethingSomething = "b1_r_r", };

        var b1_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c1", };

        var b1_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "b1_r_c2", };

        var b1_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2012, 1, 1),
            Fraction = 12.1M,
            Enum = JsonEnum.Two,
            OwnedReferenceLeaf = b1_c2_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { b1_c2_c1, b1_c2_c2 }
        };

        var b2_r_r = new JsonOwnedLeaf { SomethingSomething = "b2_r_r", };

        var b2_r_c1 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c1", };

        var b2_r_c2 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c2", };

        var b2_r = new JsonOwnedBranch
        {
            Date = new DateTime(2020, 1, 1),
            Fraction = 2.0M,
            Enum = JsonEnum.Two,
            OwnedReferenceLeaf = b2_r_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { b2_r_c1, b2_r_c2 }
        };

        var b2_c1_r = new JsonOwnedLeaf { SomethingSomething = "b2_r_r", };

        var b2_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c1", };

        var b2_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c2", };

        var b2_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2021, 1, 1),
            Fraction = 21.1M,
            Enum = JsonEnum.Three,
            OwnedReferenceLeaf = b2_c1_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { b2_c1_c1, b2_c1_c2 }
        };

        var b2_c2_r = new JsonOwnedLeaf { SomethingSomething = "b2_r_r", };

        var b2_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c1", };

        var b2_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "b2_r_c2", };

        var b2_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2022, 1, 1),
            Fraction = 22.1M,
            Enum = JsonEnum.One,
            OwnedReferenceLeaf = b2_c2_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { b2_c2_c1, b2_c2_c2 }
        };

        var d2_r_r = new JsonOwnedLeaf { SomethingSomething = "d2_r_r", };

        var d2_r_c1 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c1", };

        var d2_r_c2 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c2", };

        var d2_r = new JsonOwnedBranch
        {
            Date = new DateTime(2220, 1, 1),
            Fraction = 22.0M,
            Enum = JsonEnum.One,
            OwnedReferenceLeaf = d2_r_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { d2_r_c1, d2_r_c2 }
        };

        var d2_c1_r = new JsonOwnedLeaf { SomethingSomething = "d2_r_r", };

        var d2_c1_c1 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c1", };

        var d2_c1_c2 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c2", };

        var d2_c1 = new JsonOwnedBranch
        {
            Date = new DateTime(2221, 1, 1),
            Fraction = 221.1M,
            Enum = JsonEnum.Two,
            OwnedReferenceLeaf = d2_c1_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { d2_c1_c1, d2_c1_c2 }
        };

        var d2_c2_r = new JsonOwnedLeaf { SomethingSomething = "d2_r_r", };

        var d2_c2_c1 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c1", };

        var d2_c2_c2 = new JsonOwnedLeaf { SomethingSomething = "d2_r_c2", };

        var d2_c2 = new JsonOwnedBranch
        {
            Date = new DateTime(2222, 1, 1),
            Fraction = 222.1M,
            Enum = JsonEnum.Three,
            OwnedReferenceLeaf = d2_c2_r,
            OwnedCollectionLeaf = new List<JsonOwnedLeaf> { d2_c2_c1, d2_c2_c2 }
        };

        var baseEntity = new JsonEntityInheritanceBase
        {
            Id = 1,
            Name = "JsonEntityInheritanceBase1",
            ReferenceOnBase = b1_r,
            CollectionOnBase = new List<JsonOwnedBranch> { b1_c1, b1_c2 }
        };

        var derivedEntity = new JsonEntityInheritanceDerived
        {
            Id = 2,
            Name = "JsonEntityInheritanceDerived2",
            ReferenceOnBase = b2_r,
            CollectionOnBase = new List<JsonOwnedBranch> { b2_c1, b2_c2 },
            ReferenceOnDerived = d2_r,
            CollectionOnDerived = new List<JsonOwnedBranch> { d2_c1, d2_c2 },
        };

        return new List<JsonEntityInheritanceBase> { baseEntity, derivedEntity };
    }

    public static IReadOnlyList<JsonEntityAllTypes> CreateJsonEntitiesAllTypes()
    {
        var r = new JsonOwnedAllTypes
        {
            TestInt16 = -1234,
            TestInt32 = -123456789,
            TestInt64 = -1234567890123456789L,
            TestDouble = -1.23456789,
            TestDecimal = -1234567890.01M,
            TestDateTime = DateTime.Parse("01/01/2000 12:34:56"),
            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
            TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
            TestSingle = -1.234F,
            TestBoolean = true,
            TestByte = 255,
            TestGuid = new Guid("12345678-1234-4321-7777-987654321000"),
            TestUnsignedInt16 = 1234,
            TestUnsignedInt32 = 1234565789U,
            TestUnsignedInt64 = 1234567890123456789UL,
            TestCharacter = 'a',
            TestSignedByte = -128,
        };

        var c = new JsonOwnedAllTypes
        {
            TestInt16 = -12,
            TestInt32 = -12345,
            TestInt64 = -1234567890L,
            TestDouble = -1.2345,
            TestDecimal = -123450.01M,
            TestDateTime = DateTime.Parse("11/11/2100 12:34:56"),
            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("11/11/2200 12:34:56"), TimeSpan.FromHours(-5.0)),
            TestTimeSpan = new TimeSpan(0, 6, 5, 4, 3),
            TestSingle = -1.4F,
            TestBoolean = false,
            TestByte = 25,
            TestGuid = new Guid("00000000-0000-0000-0000-000000000000"),
            TestUnsignedInt16 = 12,
            TestUnsignedInt32 = 12345U,
            TestUnsignedInt64 = 1234567867UL,
            TestCharacter = 'h',
            TestSignedByte = -18,
        };

        return new List<JsonEntityAllTypes>
        {
            new()
            {
                Id = 1,
                Reference = r,
                Collection = new List<JsonOwnedAllTypes> { c }
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

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }
}
