// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsData : ISetSource
{
    public RelationshipsData()
    {
        RootEntities = CreateRootEntities();
        TrunkEntities = CreateTrunkEntities();
        BranchEntities = CreateBranchEntities();
        LeafEntities = CreateLeafEntities();

        WireUp(RootEntities, TrunkEntities, BranchEntities, LeafEntities, wireUpRootToTrunkOnly: false);
    }

    public IReadOnlyList<RelationshipsRootEntity> RootEntities { get; }
    public IReadOnlyList<RelationshipsTrunkEntity> TrunkEntities { get; }
    public IReadOnlyList<RelationshipsBranchEntity> BranchEntities { get; }
    public IReadOnlyList<RelationshipsLeafEntity> LeafEntities { get; }

    public static IReadOnlyList<RelationshipsRootEntity> CreateRootEntitiesWithOwnerships()
    {
        var roots = new List<RelationshipsRootEntity>();
        for (var i = 0; i < 16; i++)
        {
            roots.Add(BuildRootEntity(i));
        }

        return roots;
    }

    public static IReadOnlyList<RelationshipsTrunkEntity> CreateTrunkEntitiesWithOwnerships()
    {
        var trunks = new List<RelationshipsTrunkEntity>();
        for (var i = 0; i < 16; i++)
        {
            trunks.Add(BuildTrunkEntity(i));
        }

        return trunks;
    }

    private static RelationshipsRootEntity BuildRootEntity(int i)
    {
        var root = new RelationshipsRootEntity { Id = i + 1, Name = "Root " + (i + 1) };
        root.RequiredReferenceTrunk = BuildTrunkEntity(15 - i);
        if (i < 8)
        {
            root.OptionalReferenceTrunk = BuildTrunkEntity(i);
        }

        root.CollectionTrunk = new List<RelationshipsTrunkEntity>();
        if (i % 4 == 0)
        {
            root.CollectionTrunk.Add(BuildTrunkEntity(i + 1));
            root.CollectionTrunk.Add(BuildTrunkEntity(i + 2));
            if (i == 0)
            {
                root.CollectionTrunk.Add(BuildTrunkEntity(4));
            }
        }

        return root;
    }

    private static RelationshipsTrunkEntity BuildTrunkEntity(int i)
    {
        var trunk = new RelationshipsTrunkEntity { Id = i + 1, Name = "Trunk " + (i + 1) };
        trunk.RequiredReferenceBranch = BuildBranchEntity(15 - i);

        if (i % 8 < 4)
        {
            trunk.OptionalReferenceBranch = BuildBranchEntity(i);
        }

        trunk.CollectionBranch = new List<RelationshipsBranchEntity>();
        if (i % 4 == 0)
        {
            trunk.CollectionBranch.Add(BuildBranchEntity(i + 1));
            trunk.CollectionBranch.Add(BuildBranchEntity(i + 2));
            if (i == 4)
            {
                trunk.CollectionBranch.Add(BuildBranchEntity(8));
            }
        }

        return trunk;
    }

    private static RelationshipsBranchEntity BuildBranchEntity(int i)
    {
        var branch = new RelationshipsBranchEntity { Id = i + 1, Name = "Branch " + (i + 1) };
        branch.RequiredReferenceLeaf = BuildLeafEntity(15 - i);

        if (i % 4 < 2)
        {
            branch.OptionalReferenceLeaf = BuildLeafEntity(i);
        }

        branch.CollectionLeaf = new List<RelationshipsLeafEntity>();
        if (i % 4 == 0)
        {
            branch.CollectionLeaf.Add(BuildLeafEntity(i + 1));
            branch.CollectionLeaf.Add(BuildLeafEntity(i + 2));
            if (i == 8)
            {
                branch.CollectionLeaf.Add(BuildLeafEntity(12));
            }
        }

        return branch;
    }

    private static RelationshipsLeafEntity BuildLeafEntity(int i)
        => new RelationshipsLeafEntity { Id = i + 1, Name = "Leaf " + (i + 1) };

    public static IReadOnlyList<RelationshipsRootEntity> CreateRootEntities()
    {
        var roots = new List<RelationshipsRootEntity>();

        var id = 0;
        for (var i = 0; i < 16; i++)
        {
            roots.Add(new RelationshipsRootEntity { Id = ++id, Name = "Root " + id });
        }

        return roots;
    }

    public static IReadOnlyList<RelationshipsTrunkEntity> CreateTrunkEntities()
    {
        var trunks = new List<RelationshipsTrunkEntity>();

        var id = 0;
        for (var i = 0; i < 16; i++)
        {
            trunks.Add(new RelationshipsTrunkEntity { Id = ++id, Name = "Trunk " + id });
        }

        return trunks;
    }

    public static IReadOnlyList<RelationshipsBranchEntity> CreateBranchEntities()
    {
        var branches = new List<RelationshipsBranchEntity>();

        var id = 0;
        for (var i = 0; i < 16; i++)
        {
            branches.Add(new RelationshipsBranchEntity { Id = ++id, Name = "Branch " + id });
        }

        return branches;
    }

    public static IReadOnlyList<RelationshipsLeafEntity> CreateLeafEntities()
    {
        var leaves = new List<RelationshipsLeafEntity>();

        var id = 0;
        for (var i = 0; i < 16; i++)
        {
            leaves.Add(new RelationshipsLeafEntity { Id = ++id, Name = "Leaf " + id });
        }

        return leaves;
    }


    public static void WireUp(
        IReadOnlyList<RelationshipsRootEntity> rootEntities,
        IReadOnlyList<RelationshipsTrunkEntity> trunkEntities,
        IReadOnlyList<RelationshipsBranchEntity> branchEntities,
        IReadOnlyList<RelationshipsLeafEntity> leafEntities,
        bool wireUpRootToTrunkOnly)
    {
        for (int i = 0; i < 16; i++)
        {
            rootEntities[i].RequiredReferenceTrunk = trunkEntities[15 - i];
            rootEntities[i].RequiredReferenceTrunkId = trunkEntities[15 - i].Id;
            trunkEntities[15 - i].RequiredReferenceInverseRoot = rootEntities[i];

            if (!wireUpRootToTrunkOnly)
            {
                trunkEntities[i].RequiredReferenceBranch = branchEntities[15 - i];
                trunkEntities[i].RequiredReferenceBranchId = branchEntities[15 - i].Id;
                branchEntities[15 - i].RequiredReferenceInverseTrunk = trunkEntities[i];

                branchEntities[i].RequiredReferenceLeaf = leafEntities[15 - i];
                branchEntities[i].RequiredReferenceLeafId = leafEntities[15 - i].Id;
                leafEntities[15 - i].RequiredReferenceInverseBranch = branchEntities[i];
            }

            rootEntities[i].CollectionTrunk = new List<RelationshipsTrunkEntity>();

            if (i < 8)
            {
                rootEntities[i].OptionalReferenceTrunk = trunkEntities[i];
                rootEntities[i].OptionalReferenceTrunkId = trunkEntities[i].Id;
                trunkEntities[i].OptionalReferenceInverseRoot = rootEntities[i];
            }

            if (!wireUpRootToTrunkOnly)
            {
                if (i % 8 < 4)
                {
                    trunkEntities[i].OptionalReferenceBranch = branchEntities[i];
                    trunkEntities[i].OptionalReferenceBranchId = branchEntities[i].Id;
                    branchEntities[i].OptionalReferenceInverseTrunk = trunkEntities[i];
                }

                if (i % 4 < 2)
                {
                    branchEntities[i].OptionalReferenceLeaf = leafEntities[i];
                    branchEntities[i].OptionalReferenceLeafId = leafEntities[i].Id;
                    leafEntities[i].OptionalReferenceInverseBranch = branchEntities[i];
                }

                trunkEntities[i].CollectionBranch = new List<RelationshipsBranchEntity>();
                branchEntities[i].CollectionLeaf = new List<RelationshipsLeafEntity>();
            }

            if (i % 4 == 0)
            {
                rootEntities[i].CollectionTrunk.AddRange(trunkEntities[i + 1], trunkEntities[i + 2]);
                if (i == 0)
                {
                    rootEntities[i].CollectionTrunk.Add(trunkEntities[4]);
                }

                trunkEntities[i + 1].CollectionInverseRoot = rootEntities[i];
                trunkEntities[i + 1].CollectionRootId = rootEntities[i].Id;

                trunkEntities[i + 2].CollectionInverseRoot = rootEntities[i];
                trunkEntities[i + 2].CollectionRootId = rootEntities[i].Id;

                if (i == 0)
                {
                    trunkEntities[4].CollectionInverseRoot = rootEntities[i];
                    trunkEntities[4].CollectionRootId = rootEntities[i].Id;
                }

                if (!wireUpRootToTrunkOnly)
                {
                    trunkEntities[i].CollectionBranch.AddRange(branchEntities[i + 1], branchEntities[i + 2]);
                    if (i == 4)
                    {
                        trunkEntities[i].CollectionBranch.Add(branchEntities[8]);
                    }

                    branchEntities[i + 1].CollectionInverseTrunk = trunkEntities[i];
                    branchEntities[i + 1].CollectionTrunkId = trunkEntities[i].Id;

                    branchEntities[i + 2].CollectionInverseTrunk = trunkEntities[i];
                    branchEntities[i + 2].CollectionTrunkId = trunkEntities[i].Id;

                    if (i == 4)
                    {
                        branchEntities[8].CollectionInverseTrunk = trunkEntities[i];
                        branchEntities[8].CollectionTrunkId = trunkEntities[i].Id;
                    }

                    branchEntities[i].CollectionLeaf.AddRange(leafEntities[i + 1], leafEntities[i + 2]);
                    if (i == 8)
                    {
                        branchEntities[i].CollectionLeaf.Add(leafEntities[12]);
                    }

                    leafEntities[i + 1].CollectionInverseBranch = branchEntities[i];
                    leafEntities[i + 1].CollectionBranchId = branchEntities[i].Id;

                    leafEntities[i + 2].CollectionInverseBranch = branchEntities[i];
                    leafEntities[i + 2].CollectionBranchId = branchEntities[i].Id;

                    if (i == 8)
                    {
                        leafEntities[12].CollectionInverseBranch = branchEntities[i];
                        leafEntities[12].CollectionBranchId = branchEntities[i].Id;
                    }
                }
            }
        }
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(RelationshipsRootEntity))
        {
            return (IQueryable<TEntity>)RootEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(RelationshipsTrunkEntity))
        {
            return (IQueryable<TEntity>)TrunkEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(RelationshipsBranchEntity))
        {
            return (IQueryable<TEntity>)BranchEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(RelationshipsLeafEntity))
        {
            return (IQueryable<TEntity>)LeafEntities.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }
}
