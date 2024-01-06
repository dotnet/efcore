// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

public class InheritanceRelationshipsData : ISetSource
{
    public static readonly InheritanceRelationshipsData Instance = new();

    public IReadOnlyList<BaseInheritanceRelationshipEntity> BaseEntities { get; set; }
    public IReadOnlyList<BaseReferenceOnBase> BaseReferencesOnBase { get; set; }
    public IReadOnlyList<BaseReferenceOnDerived> BaseReferencesOnDerived { get; set; }
    public IReadOnlyList<ReferenceOnBase> ReferencesOnBase { get; set; }
    public IReadOnlyList<ReferenceOnDerived> ReferencesOnDerived { get; set; }
    public IReadOnlyList<NestedReferenceBase> NestedReferences { get; set; }
    public IReadOnlyList<BaseCollectionOnBase> BaseCollectionsOnBase { get; set; }
    public IReadOnlyList<BaseCollectionOnDerived> BaseCollectionsOnDerived { get; set; }
    public IReadOnlyList<CollectionOnBase> CollectionsOnBase { get; set; }
    public IReadOnlyList<CollectionOnDerived> CollectionsOnDerived { get; set; }
    public IReadOnlyList<NestedCollectionBase> NestedCollections { get; set; }
    public IReadOnlyList<PrincipalEntity> PrincipalEntities { get; set; }
    public IReadOnlyList<ReferencedEntity> ReferencedEntities { get; set; }

    private InheritanceRelationshipsData()
    {
        BaseEntities = CreateBaseEntities();
        BaseReferencesOnBase = CreateBaseReferencesOnBase();
        BaseReferencesOnDerived = CreateBaseReferencesOnDerived();
        ReferencesOnBase = CreateReferencesOnBase();
        ReferencesOnDerived = CreateReferencesOnDerived();
        NestedReferences = CreateNestedReferences();
        BaseCollectionsOnBase = CreateBaseCollectionsOnBase();
        BaseCollectionsOnDerived = CreateBaseCollectionsOnDerived();
        CollectionsOnBase = CreateCollectionsOnBase();
        CollectionsOnDerived = CreateCollectionsOnDerived();
        NestedCollections = CreateNestedCollections();
        PrincipalEntities = CreatePrincipalEntities();
        ReferencedEntities = CreateReferencedEntities();

        WireUp(
            BaseEntities,
            BaseReferencesOnBase,
            BaseReferencesOnDerived,
            ReferencesOnBase,
            ReferencesOnDerived,
            NestedReferences,
            BaseCollectionsOnBase,
            BaseCollectionsOnDerived,
            CollectionsOnBase,
            CollectionsOnDerived,
            NestedCollections);
    }

    public virtual IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(BaseInheritanceRelationshipEntity))
        {
            return (IQueryable<TEntity>)BaseEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(DerivedInheritanceRelationshipEntity))
        {
            return (IQueryable<TEntity>)BaseEntities.OfType<DerivedInheritanceRelationshipEntity>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(BaseReferenceOnBase))
        {
            return (IQueryable<TEntity>)BaseReferencesOnBase.AsQueryable();
        }

        if (typeof(TEntity) == typeof(BaseReferenceOnDerived))
        {
            return (IQueryable<TEntity>)BaseReferencesOnDerived.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ReferenceOnBase))
        {
            return (IQueryable<TEntity>)ReferencesOnBase.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ReferenceOnDerived))
        {
            return (IQueryable<TEntity>)ReferencesOnDerived.AsQueryable();
        }

        if (typeof(TEntity) == typeof(NestedReferenceBase))
        {
            return (IQueryable<TEntity>)NestedReferences.AsQueryable();
        }

        if (typeof(TEntity) == typeof(BaseCollectionOnBase))
        {
            return (IQueryable<TEntity>)BaseCollectionsOnBase.AsQueryable();
        }

        if (typeof(TEntity) == typeof(BaseCollectionOnDerived))
        {
            return (IQueryable<TEntity>)BaseCollectionsOnDerived.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CollectionOnBase))
        {
            return (IQueryable<TEntity>)CollectionsOnBase.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CollectionOnDerived))
        {
            return (IQueryable<TEntity>)CollectionsOnDerived.AsQueryable();
        }

        if (typeof(TEntity) == typeof(NestedCollectionBase))
        {
            return (IQueryable<TEntity>)NestedCollections.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PrincipalEntity))
        {
            return (IQueryable<TEntity>)PrincipalEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ReferencedEntity))
        {
            return (IQueryable<TEntity>)ReferencedEntities.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<BaseInheritanceRelationshipEntity> CreateBaseEntities()
        => new List<BaseInheritanceRelationshipEntity>
        {
            new()
            {
                Id = 1,
                Name = "Base1",
                OwnedReferenceOnBase = new OwnedEntity { Name = "OROB1" },
                OwnedCollectionOnBase =
                    [new() { Id = 1, Name = "OCOB11" }, new() { Id = 2, Name = "OCOB12" }],
                BaseCollectionOnBase = [],
                CollectionOnBase = [],
            },
            new()
            {
                Id = 2,
                Name = "Base2",
                OwnedReferenceOnBase = new OwnedEntity { Name = "OROB2" },
                OwnedCollectionOnBase = [new() { Id = 3, Name = "OCOB21" }],
                BaseCollectionOnBase = [],
                CollectionOnBase = [],
            },
            new()
            {
                Id = 3,
                Name = "Base3",
                BaseCollectionOnBase = [],
                CollectionOnBase = [],
                OwnedCollectionOnBase = [],
            },
            new DerivedInheritanceRelationshipEntity
            {
                Id = 4,
                Name = "Derived1(4)",
                OwnedReferenceOnBase = new OwnedEntity { Name = "OROB4" },
                OwnedCollectionOnBase =
                    [new() { Id = 4, Name = "OCOB41" }, new() { Id = 5, Name = "OCOB42" }],
                OwnedReferenceOnDerived = new OwnedEntity { Name = "OROD4" },
                OwnedCollectionOnDerived =
                    [new() { Id = 1, Name = "OCOD41" }, new() { Id = 2, Name = "OCOD42" }],
                BaseCollectionOnBase = [],
                BaseCollectionOnDerived = [],
                CollectionOnBase = [],
                CollectionOnDerived = [],
                DerivedCollectionOnDerived = [],
            },
            new DerivedInheritanceRelationshipEntity
            {
                Id = 5,
                Name = "Derived2(5)",
                OwnedReferenceOnBase = new OwnedEntity { Name = "OROB5" },
                OwnedCollectionOnBase = [new() { Id = 6, Name = "OCOB51" }],
                OwnedReferenceOnDerived = new OwnedEntity { Name = "OROD5" },
                OwnedCollectionOnDerived = [new() { Id = 3, Name = "OCOD51" }],
                BaseCollectionOnBase = [],
                BaseCollectionOnDerived = [],
                CollectionOnBase = [],
                CollectionOnDerived = [],
                DerivedCollectionOnDerived = [],
            },
            new DerivedInheritanceRelationshipEntity
            {
                Id = 6,
                Name = "Derived3(6)",
                BaseCollectionOnBase = [],
                BaseCollectionOnDerived = [],
                CollectionOnBase = [],
                CollectionOnDerived = [],
                DerivedCollectionOnDerived = [],
                OwnedCollectionOnBase = [],
                OwnedCollectionOnDerived = [],
            },
        };

    public static IReadOnlyList<BaseReferenceOnBase> CreateBaseReferencesOnBase()
        => new List<BaseReferenceOnBase>
        {
            new()
            {
                Id = 1,
                Name = "BROB1",
                NestedCollection = []
            },
            new()
            {
                Id = 2,
                Name = "BROB2",
                NestedCollection = []
            },
            new()
            {
                Id = 3,
                Name = "BROB3 (dangling)",
                NestedCollection = []
            },
            new DerivedReferenceOnBase
            {
                Id = 4,
                Name = "DROB1",
                NestedCollection = []
            },
            new DerivedReferenceOnBase
            {
                Id = 5,
                Name = "DROB2",
                NestedCollection = []
            },
            new DerivedReferenceOnBase
            {
                Id = 6,
                Name = "DROB3",
                NestedCollection = []
            },
            new DerivedReferenceOnBase
            {
                Id = 7,
                Name = "DROB4 (half-dangling)",
                NestedCollection = []
            },
            new DerivedReferenceOnBase
            {
                Id = 8,
                Name = "DROB5 (dangling)",
                NestedCollection = []
            },
        };

    public static IReadOnlyList<BaseReferenceOnDerived> CreateBaseReferencesOnDerived()
        => new List<BaseReferenceOnDerived>
        {
            new() { Id = 1, Name = "BROD1" },
            new() { Id = 2, Name = "BROD2 (dangling)" },
            new() { Id = 3, Name = "BROD3 (dangling)" },
            new DerivedReferenceOnDerived { Id = 4, Name = "DROD1" },
            new DerivedReferenceOnDerived { Id = 5, Name = "DROD2" },
            new DerivedReferenceOnDerived { Id = 6, Name = "DROD3 (dangling)" },
        };

    public static IReadOnlyList<ReferenceOnBase> CreateReferencesOnBase()
        => new List<ReferenceOnBase>
        {
            new() { Id = 1, Name = "ROB1" },
            new() { Id = 2, Name = "ROB2" },
            new() { Id = 3, Name = "ROB3" },
            new() { Id = 4, Name = "ROB4" },
        };

    public static IReadOnlyList<ReferenceOnDerived> CreateReferencesOnDerived()
        => new List<ReferenceOnDerived>
        {
            new() { Id = 1, Name = "ROD1" },
            new() { Id = 2, Name = "ROD2" },
            new() { Id = 3, Name = "ROD3 (dangling)" },
        };

    public static IReadOnlyList<NestedReferenceBase> CreateNestedReferences()
        => new List<NestedReferenceBase>
        {
            new() { Id = 1, Name = "NRB1" },
            new() { Id = 2, Name = "NRB2" },
            new() { Id = 3, Name = "NRB3" },
            new() { Id = 4, Name = "NRB4 (dangling)" },
            new NestedReferenceDerived { Id = 5, Name = "NRD1" },
            new NestedReferenceDerived { Id = 6, Name = "NRD2" },
            new NestedReferenceDerived { Id = 7, Name = "NRD3" },
            new NestedReferenceDerived { Id = 8, Name = "NRD4" },
            new NestedReferenceDerived { Id = 9, Name = "NRD4 (dangling)" },
        };

    public static IReadOnlyList<BaseCollectionOnBase> CreateBaseCollectionsOnBase()
        => new List<BaseCollectionOnBase>
        {
            new()
            {
                Id = 1,
                Name = "BCOB11",
                NestedCollection = []
            },
            new()
            {
                Id = 2,
                Name = "BCOB12",
                NestedCollection = []
            },
            new()
            {
                Id = 3,
                Name = "BCOB21",
                NestedCollection = []
            },
            new()
            {
                Id = 4,
                Name = "BCOB31 (dangling)",
                NestedCollection = []
            },
            new()
            {
                Id = 5,
                Name = "BCOB32 (dangling)",
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 6,
                Name = "DCOB11",
                DerivedProperty = 1,
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 7,
                Name = "DCOB12",
                DerivedProperty = 2,
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 8,
                Name = "DCOB21",
                DerivedProperty = 3,
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 9,
                Name = "DCOB31",
                DerivedProperty = 4,
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 10,
                Name = "DCOB32",
                DerivedProperty = 5,
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 11,
                Name = "DCOB41",
                DerivedProperty = 6,
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 12,
                Name = "DCOB51 (dangling)",
                DerivedProperty = 7,
                NestedCollection = []
            },
            new DerivedCollectionOnBase
            {
                Id = 13,
                Name = "DCOB52 (dangling)",
                DerivedProperty = 8,
                NestedCollection = []
            },
        };

    public static IReadOnlyList<BaseCollectionOnDerived> CreateBaseCollectionsOnDerived()
        => new List<BaseCollectionOnDerived>
        {
            new() { Id = 1, Name = "BCOD11" },
            new() { Id = 2, Name = "BCOD21 (dangling)" },
            new() { Id = 3, Name = "BCOD22 (dangling)" },
            new DerivedCollectionOnDerived { Id = 4, Name = "DCOD11" },
            new DerivedCollectionOnDerived { Id = 5, Name = "DCOD12" },
            new DerivedCollectionOnDerived { Id = 6, Name = "DCOD21" },
            new DerivedCollectionOnDerived { Id = 7, Name = "DCOD31 (dangling)" },
        };

    public static IReadOnlyList<CollectionOnBase> CreateCollectionsOnBase()
        => new List<CollectionOnBase>
        {
            new() { Id = 1, Name = "COB11" },
            new() { Id = 2, Name = "COB12" },
            new() { Id = 3, Name = "COB21" },
            new() { Id = 4, Name = "COB31" },
            new() { Id = 5, Name = "COB32" },
            new() { Id = 6, Name = "COB33" },
            new() { Id = 7, Name = "COB41" },
            new() { Id = 8, Name = "COB51 (dangling)" },
            new() { Id = 9, Name = "COB52 (dangling)" },
        };

    public static IReadOnlyList<CollectionOnDerived> CreateCollectionsOnDerived()
        => new List<CollectionOnDerived>
        {
            new() { Id = 1, Name = "COD11" },
            new() { Id = 2, Name = "COD21" },
            new() { Id = 3, Name = "COD22" },
            new() { Id = 4, Name = "COD31 (dangling)" },
        };

    public static IReadOnlyList<NestedCollectionBase> CreateNestedCollections()
        => new List<NestedCollectionBase>
        {
            new() { Id = 1, Name = "NCB11" },
            new() { Id = 2, Name = "NCB21" },
            new() { Id = 3, Name = "NCB22" },
            new() { Id = 4, Name = "NCB31" },
            new() { Id = 5, Name = "NCB41 (dangling)" },
            new NestedCollectionDerived { Id = 6, Name = "NCD11" },
            new NestedCollectionDerived { Id = 7, Name = "NCD21" },
            new NestedCollectionDerived { Id = 8, Name = "NCD21" },
            new NestedCollectionDerived { Id = 9, Name = "NCD32" },
            new NestedCollectionDerived { Id = 10, Name = "NCD41" },
            new NestedCollectionDerived { Id = 11, Name = "NCD42" },
            new NestedCollectionDerived { Id = 12, Name = "NCD52 (dangling)" },
            new NestedCollectionDerived { Id = 13, Name = "NCD52 (dangling)" },
        };

    public static IReadOnlyList<PrincipalEntity> CreatePrincipalEntities()
        => new List<PrincipalEntity>
        {
            new() { Id = 1, Name = "PE1" }, new() { Id = 2, Name = "PE2" },
        };

    public static IReadOnlyList<ReferencedEntity> CreateReferencedEntities()
        => new List<ReferencedEntity>
        {
            new()
            {
                Id = 1,
                Name = "RE1",
                Principals = new List<PrincipalEntity>()
            },
            new()
            {
                Id = 2,
                Name = "RE2",
                Principals = new List<PrincipalEntity>()
            },
        };

    public static void WireUp(
        IReadOnlyList<BaseInheritanceRelationshipEntity> baseEntities,
        IReadOnlyList<BaseReferenceOnBase> baseReferencesOnBase,
        IReadOnlyList<BaseReferenceOnDerived> baseReferencesOnDerived,
        IReadOnlyList<ReferenceOnBase> referencesOnBase,
        IReadOnlyList<ReferenceOnDerived> referencesOnDerived,
        IReadOnlyList<NestedReferenceBase> nestedReferences,
        IReadOnlyList<BaseCollectionOnBase> baseCollectionsOnBase,
        IReadOnlyList<BaseCollectionOnDerived> baseCollectionsOnDerived,
        IReadOnlyList<CollectionOnBase> collectionsOnBase,
        IReadOnlyList<CollectionOnDerived> collectionsOnDerived,
        IReadOnlyList<NestedCollectionBase> nestedCollections)
    {
        // BaseReferenceOnBase.NestedReference (inverse: ParentReference)
        // BaseReferenceOnBase.NestedCollection (inverse: ParentReference)
        baseReferencesOnBase[0].NestedReference = nestedReferences[0];
        nestedReferences[0].ParentReference = baseReferencesOnBase[0];
        nestedReferences[0].ParentReferenceId = baseReferencesOnBase[0].Id;

        baseReferencesOnBase[0].NestedCollection = [nestedCollections[0]];
        nestedCollections[0].ParentReference = baseReferencesOnBase[0];
        nestedCollections[0].ParentReferenceId = baseReferencesOnBase[0].Id;

        baseReferencesOnBase[1].NestedReference = nestedReferences[4];
        nestedReferences[4].ParentReference = baseReferencesOnBase[1];
        nestedReferences[4].ParentReferenceId = baseReferencesOnBase[1].Id;

        baseReferencesOnBase[1].NestedCollection = [nestedCollections[5]];
        nestedCollections[5].ParentReference = baseReferencesOnBase[1];
        nestedCollections[5].ParentReferenceId = baseReferencesOnBase[1].Id;

        baseReferencesOnBase[3].NestedReference = nestedReferences[1];
        nestedReferences[1].ParentReference = baseReferencesOnBase[3];
        nestedReferences[1].ParentReferenceId = baseReferencesOnBase[3].Id;

        baseReferencesOnBase[3].NestedCollection = [nestedCollections[1], nestedCollections[2]];
        nestedCollections[1].ParentReference = baseReferencesOnBase[3];
        nestedCollections[1].ParentReferenceId = baseReferencesOnBase[3].Id;
        nestedCollections[2].ParentReference = baseReferencesOnBase[3];
        nestedCollections[2].ParentReferenceId = baseReferencesOnBase[3].Id;

        baseReferencesOnBase[4].NestedReference = nestedReferences[5];
        nestedReferences[5].ParentReference = baseReferencesOnBase[4];
        nestedReferences[5].ParentReferenceId = baseReferencesOnBase[4].Id;

        baseReferencesOnBase[4].NestedCollection = [nestedCollections[6]];
        nestedCollections[6].ParentReference = baseReferencesOnBase[4];
        nestedCollections[6].ParentReferenceId = baseReferencesOnBase[4].Id;

        baseReferencesOnBase[6].NestedReference = nestedReferences[6];
        nestedReferences[6].ParentReference = baseReferencesOnBase[6];
        nestedReferences[6].ParentReferenceId = baseReferencesOnBase[6].Id;

        baseReferencesOnBase[6].NestedCollection = [nestedCollections[7], nestedCollections[8]];
        nestedCollections[7].ParentReference = baseReferencesOnBase[6];
        nestedCollections[7].ParentReferenceId = baseReferencesOnBase[6].Id;
        nestedCollections[8].ParentReference = baseReferencesOnBase[6];
        nestedCollections[8].ParentReferenceId = baseReferencesOnBase[6].Id;

        // BaseCollectionOnBase.NestedReference (inverse: ParentCollection)
        // BaseCollectionOnBase.NestedCollection (inverse: ParentCollection)
        baseCollectionsOnBase[0].NestedReference = nestedReferences[0];
        nestedReferences[0].ParentCollection = baseCollectionsOnBase[0];
        nestedReferences[0].ParentCollectionId = baseCollectionsOnBase[0].Id;

        baseCollectionsOnBase[0].NestedCollection = [nestedCollections[0]];
        nestedCollections[0].ParentCollection = baseCollectionsOnBase[0];
        nestedCollections[0].ParentCollectionId = baseCollectionsOnBase[0].Id;

        baseCollectionsOnBase[1].NestedReference = nestedReferences[4];
        nestedReferences[4].ParentCollection = baseCollectionsOnBase[1];
        nestedReferences[4].ParentCollectionId = baseCollectionsOnBase[1].Id;

        baseCollectionsOnBase[1].NestedCollection = [nestedCollections[5]];
        nestedCollections[5].ParentCollection = baseCollectionsOnBase[1];
        nestedCollections[5].ParentCollectionId = baseCollectionsOnBase[1].Id;

        baseCollectionsOnBase[3].NestedReference = nestedReferences[1];
        nestedReferences[1].ParentCollection = baseCollectionsOnBase[3];
        nestedReferences[1].ParentCollectionId = baseCollectionsOnBase[3].Id;

        baseCollectionsOnBase[3].NestedCollection = [nestedCollections[1], nestedCollections[2]];
        nestedCollections[1].ParentCollection = baseCollectionsOnBase[3];
        nestedCollections[1].ParentCollectionId = baseCollectionsOnBase[3].Id;
        nestedCollections[2].ParentCollection = baseCollectionsOnBase[3];
        nestedCollections[2].ParentCollectionId = baseCollectionsOnBase[3].Id;

        baseCollectionsOnBase[5].NestedReference = nestedReferences[5];
        nestedReferences[5].ParentCollection = baseCollectionsOnBase[5];
        nestedReferences[5].ParentCollectionId = baseCollectionsOnBase[5].Id;

        baseCollectionsOnBase[5].NestedCollection = [nestedCollections[6]];
        nestedCollections[6].ParentCollection = baseCollectionsOnBase[5];
        nestedCollections[6].ParentCollectionId = baseCollectionsOnBase[5].Id;

        baseCollectionsOnBase[6].NestedReference = nestedReferences[2];
        nestedReferences[2].ParentCollection = baseCollectionsOnBase[6];
        nestedReferences[2].ParentCollectionId = baseCollectionsOnBase[6].Id;

        baseCollectionsOnBase[6].NestedCollection = [nestedCollections[3]];
        nestedCollections[3].ParentCollection = baseCollectionsOnBase[6];
        nestedCollections[3].ParentCollectionId = baseCollectionsOnBase[6].Id;

        baseCollectionsOnBase[8].NestedReference = nestedReferences[6];
        nestedReferences[6].ParentCollection = baseCollectionsOnBase[8];
        nestedReferences[6].ParentCollectionId = baseCollectionsOnBase[8].Id;

        baseCollectionsOnBase[8].NestedCollection = [nestedCollections[7], nestedCollections[8]];
        nestedCollections[7].ParentCollection = baseCollectionsOnBase[8];
        nestedCollections[7].ParentCollectionId = baseCollectionsOnBase[8].Id;
        nestedCollections[8].ParentCollection = baseCollectionsOnBase[8];
        nestedCollections[8].ParentCollectionId = baseCollectionsOnBase[8].Id;

        baseCollectionsOnBase[11].NestedReference = nestedReferences[7];
        nestedReferences[7].ParentCollection = baseCollectionsOnBase[11];
        nestedReferences[7].ParentCollectionId = baseCollectionsOnBase[11].Id;

        baseCollectionsOnBase[11].NestedCollection = [nestedCollections[9], nestedCollections[10]];
        nestedCollections[9].ParentCollection = baseCollectionsOnBase[11];
        nestedCollections[9].ParentCollectionId = baseCollectionsOnBase[11].Id;
        nestedCollections[10].ParentCollection = baseCollectionsOnBase[11];
        nestedCollections[10].ParentCollectionId = baseCollectionsOnBase[11].Id;

        // BaseInheritanceRelationshipEntity.BaseReferenceOnBase (inverse: BaseParent)
        // BaseInheritanceRelationshipEntity.ReferenceOnBase (inverse: Parent)
        baseEntities[0].BaseReferenceOnBase = baseReferencesOnBase[0];
        baseReferencesOnBase[0].BaseParent = baseEntities[0];
        baseReferencesOnBase[0].BaseParentId = baseEntities[0].Id;

        baseEntities[0].ReferenceOnBase = referencesOnBase[0];
        referencesOnBase[0].Parent = baseEntities[0];
        referencesOnBase[0].ParentId = baseEntities[0].Id;

        baseEntities[1].BaseReferenceOnBase = baseReferencesOnBase[4];
        baseReferencesOnBase[4].BaseParent = baseEntities[1];
        baseReferencesOnBase[4].BaseParentId = baseEntities[1].Id;

        baseEntities[1].ReferenceOnBase = referencesOnBase[1];
        referencesOnBase[1].Parent = baseEntities[1];
        referencesOnBase[1].ParentId = baseEntities[1].Id;

        baseEntities[3].BaseReferenceOnBase = baseReferencesOnBase[3];
        baseReferencesOnBase[3].BaseParent = baseEntities[3];
        baseReferencesOnBase[3].BaseParentId = baseEntities[3].Id;

        baseEntities[3].ReferenceOnBase = referencesOnBase[2];
        referencesOnBase[2].Parent = baseEntities[3];
        referencesOnBase[2].ParentId = baseEntities[3].Id;

        // BaseInheritanceRelationshipEntity.BaseCollectionOnBase (inverse: BaseParent)
        // BaseInheritanceRelationshipEntity.CollectionOnBase (inverse: Parent)
        baseEntities[0].BaseCollectionOnBase = [baseCollectionsOnBase[0]];
        baseCollectionsOnBase[0].BaseParent = baseEntities[0];
        baseCollectionsOnBase[0].BaseParentId = baseEntities[0].Id;

        baseEntities[0].CollectionOnBase = [collectionsOnBase[0], collectionsOnBase[1]];
        collectionsOnBase[0].Parent = baseEntities[0];
        collectionsOnBase[0].ParentId = baseEntities[0].Id;
        collectionsOnBase[1].Parent = baseEntities[0];
        collectionsOnBase[1].ParentId = baseEntities[0].Id;

        baseEntities[1].CollectionOnBase = [collectionsOnBase[2]];
        collectionsOnBase[2].Parent = baseEntities[1];
        collectionsOnBase[2].ParentId = baseEntities[1].Id;

        baseEntities[2].BaseCollectionOnBase = [baseCollectionsOnBase[7]];
        baseCollectionsOnBase[7].BaseParent = baseEntities[2];
        baseCollectionsOnBase[7].BaseParentId = baseEntities[2].Id;

        baseEntities[3].BaseCollectionOnBase = [baseCollectionsOnBase[5], baseCollectionsOnBase[6]];
        baseCollectionsOnBase[5].BaseParent = baseEntities[3];
        baseCollectionsOnBase[5].BaseParentId = baseEntities[3].Id;
        baseCollectionsOnBase[6].BaseParent = baseEntities[3];
        baseCollectionsOnBase[6].BaseParentId = baseEntities[3].Id;

        baseEntities[3].CollectionOnBase = [collectionsOnBase[3], collectionsOnBase[4]];
        collectionsOnBase[3].Parent = baseEntities[3];
        collectionsOnBase[3].ParentId = baseEntities[3].Id;
        collectionsOnBase[4].Parent = baseEntities[3];
        collectionsOnBase[4].ParentId = baseEntities[3].Id;

        // DerivedInheritanceRelationshipEntity navigations
        baseEntities[0].DerivedSefReferenceOnBase = (DerivedInheritanceRelationshipEntity)baseEntities[3];
        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).BaseSelfReferenceOnDerived = baseEntities[0];
        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).BaseId = baseEntities[0].Id;

        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).BaseReferenceOnDerived = baseReferencesOnDerived[0];
        baseReferencesOnDerived[0].BaseParent = (DerivedInheritanceRelationshipEntity)baseEntities[3];
        baseReferencesOnDerived[0].BaseParentId = baseEntities[3].Id;

        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).DerivedReferenceOnDerived =
            (DerivedReferenceOnDerived)baseReferencesOnDerived[3];

        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).ReferenceOnDerived = referencesOnDerived[0];
        referencesOnDerived[0].Parent = (DerivedInheritanceRelationshipEntity)baseEntities[3];
        referencesOnDerived[0].ParentId = baseEntities[3].Id;

        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).BaseCollectionOnDerived =
            [baseCollectionsOnDerived[0]];
        baseCollectionsOnDerived[0].BaseParent = (DerivedInheritanceRelationshipEntity)baseEntities[3];
        baseCollectionsOnDerived[0].ParentId = baseEntities[3].Id;

        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).CollectionOnDerived =
            [collectionsOnDerived[0]];
        collectionsOnDerived[0].Parent = (DerivedInheritanceRelationshipEntity)baseEntities[3];
        collectionsOnDerived[0].ParentId = baseEntities[3].Id;

        ((DerivedInheritanceRelationshipEntity)baseEntities[3]).DerivedCollectionOnDerived =
        [
            (DerivedCollectionOnDerived)baseCollectionsOnDerived[3], (DerivedCollectionOnDerived)baseCollectionsOnDerived[4]
        ];
    }
}
