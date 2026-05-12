// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class ManyToManyData : ISetSource
{
    private readonly bool _useGeneratedKeys;
    private readonly EntityOne[] _ones;
    private readonly EntityTwo[] _twos;
    private readonly EntityThree[] _threes;
    private readonly EntityCompositeKey[] _compositeKeys;
    private readonly EntityRoot[] _roots;
    private readonly UnidirectionalEntityOne[] _unidirectionalOnes;
    private readonly UnidirectionalEntityTwo[] _unidirectionalTwos;
    private readonly UnidirectionalEntityThree[] _unidirectionalThrees;
    private readonly UnidirectionalEntityCompositeKey[] _unidirectionalCompositeKeys;
    private readonly UnidirectionalEntityRoot[] _unidirectionalRoots;

    public ManyToManyData(ManyToManyContext context, bool useGeneratedKeys)
    {
        _useGeneratedKeys = useGeneratedKeys;
        _ones = CreateOnes(context);
        context.Set<EntityOne>().AddRange(_ones);
        _twos = CreateTwos(context);
        context.Set<EntityTwo>().AddRange(_twos);
        _threes = CreateThrees(context);
        context.Set<EntityThree>().AddRange(_threes);
        _compositeKeys = CreateCompositeKeys(context);
        context.Set<EntityCompositeKey>().AddRange(_compositeKeys);
        _roots = CreateRoots(context);
        context.Set<EntityRoot>().AddRange(_roots);

        context.Set<JoinCompositeKeyToLeaf>().AddRange(CreateJoinCompositeKeyToLeaves(context));
        context.Set<JoinOneSelfPayload>().AddRange(CreateJoinOneSelfPayloads(context));
        context.Set<JoinOneToBranch>().AddRange(CreateJoinOneToBranches(context));
        context.Set<JoinOneToThreePayloadFull>().AddRange(CreateJoinOneToThreePayloadFulls(context));
        context.Set<JoinOneToTwo>().AddRange(CreateJoinOneToTwos(context));
        context.Set<JoinThreeToCompositeKeyFull>().AddRange(CreateJoinThreeToCompositeKeyFulls(context));
        context.Set<JoinTwoToThree>().AddRange(CreateJoinTwoToThrees(context));

        context.Set<Dictionary<string, object>>("EntityOneEntityTwo").AddRange(CreateEntityOneEntityTwos(context));
        context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
            .AddRange(CreateJoinOneToThreePayloadFullShareds(context));
        context.Set<Dictionary<string, object>>("EntityTwoEntityTwo").AddRange(CreateJoinTwoSelfShareds(context));
        context.Set<Dictionary<string, object>>("EntityCompositeKeyEntityTwo").AddRange(CreateJoinTwoToCompositeKeyShareds(context));
        context.Set<Dictionary<string, object>>("EntityRootEntityThree").AddRange(CreateEntityRootEntityThrees(context));
        context.Set<Dictionary<string, object>>("EntityCompositeKeyEntityRoot").AddRange(CreateJoinCompositeKeyToRootShareds(context));
        context.Set<Dictionary<string, object>>("EntityBranchEntityRoot").AddRange(CreateEntityRootEntityBranches(context));

        _unidirectionalOnes = CreateUnidirectionalOnes(context);
        context.Set<UnidirectionalEntityOne>().AddRange(_unidirectionalOnes);
        _unidirectionalTwos = CreateUnidirectionalTwos(context);
        context.Set<UnidirectionalEntityTwo>().AddRange(_unidirectionalTwos);
        _unidirectionalThrees = CreateUnidirectionalThrees(context);
        context.Set<UnidirectionalEntityThree>().AddRange(_unidirectionalThrees);
        _unidirectionalCompositeKeys = CreateUnidirectionalCompositeKeys(context);
        context.Set<UnidirectionalEntityCompositeKey>().AddRange(_unidirectionalCompositeKeys);
        _unidirectionalRoots = CreateUnidirectionalRoots(context);
        context.Set<UnidirectionalEntityRoot>().AddRange(_unidirectionalRoots);

        context.Set<UnidirectionalJoinCompositeKeyToLeaf>().AddRange(CreateUnidirectionalJoinCompositeKeyToLeaves(context));
        context.Set<UnidirectionalJoinOneSelfPayload>().AddRange(CreateUnidirectionalJoinOneSelfPayloads(context));
        context.Set<UnidirectionalJoinOneToBranch>().AddRange(CreateUnidirectionalJoinOneToBranches(context));
        context.Set<UnidirectionalJoinOneToThreePayloadFull>().AddRange(CreateUnidirectionalJoinOneToThreePayloadFulls(context));
        context.Set<UnidirectionalJoinOneToTwo>().AddRange(CreateUnidirectionalJoinOneToTwos(context));
        context.Set<UnidirectionalJoinThreeToCompositeKeyFull>().AddRange(CreateUnidirectionalJoinThreeToCompositeKeyFulls(context));
        context.Set<UnidirectionalJoinTwoToThree>().AddRange(CreateUnidirectionalJoinTwoToThrees(context));

        context.Set<Dictionary<string, object>>("UnidirectionalEntityOneUnidirectionalEntityTwo")
            .AddRange(CreateUnidirectionalEntityOneEntityTwos(context));
        context.Set<Dictionary<string, object>>("UnidirectionalJoinOneToThreePayloadFullShared")
            .AddRange(CreateUnidirectionalJoinOneToThreePayloadFullShareds(context));
        context.Set<Dictionary<string, object>>("UnidirectionalEntityTwoUnidirectionalEntityTwo")
            .AddRange(CreateUnidirectionalJoinTwoSelfShareds(context));
        context.Set<Dictionary<string, object>>("UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo")
            .AddRange(CreateUnidirectionalJoinTwoToCompositeKeyShareds(context));
        context.Set<Dictionary<string, object>>("UnidirectionalEntityRootUnidirectionalEntityThree")
            .AddRange(CreateUnidirectionalEntityRootEntityThrees(context));
        context.Set<Dictionary<string, object>>("UnidirectionalEntityCompositeKeyUnidirectionalEntityRoot")
            .AddRange(CreateUnidirectionalJoinCompositeKeyToRootShareds(context));
        context.Set<Dictionary<string, object>>("UnidirectionalEntityBranchUnidirectionalEntityRoot")
            .AddRange(CreateUnidirectionalEntityRootUnidirectionalEntityBranches(context));
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
        => typeof(TEntity).Name switch
        {
            nameof(EntityOne) => (IQueryable<TEntity>)_ones.AsQueryable(),
            nameof(EntityTwo) => (IQueryable<TEntity>)_twos.AsQueryable(),
            nameof(EntityThree) => (IQueryable<TEntity>)_threes.AsQueryable(),
            nameof(EntityCompositeKey) => (IQueryable<TEntity>)_compositeKeys.AsQueryable(),
            nameof(EntityRoot) => (IQueryable<TEntity>)_roots.AsQueryable(),
            nameof(EntityBranch) => (IQueryable<TEntity>)_roots.OfType<EntityBranch>().AsQueryable(),
            nameof(EntityLeaf) => (IQueryable<TEntity>)_roots.OfType<EntityLeaf>().AsQueryable(),
            nameof(UnidirectionalEntityOne) => (IQueryable<TEntity>)_unidirectionalOnes.AsQueryable(),
            nameof(UnidirectionalEntityTwo) => (IQueryable<TEntity>)_unidirectionalTwos.AsQueryable(),
            nameof(UnidirectionalEntityThree) => (IQueryable<TEntity>)_unidirectionalThrees.AsQueryable(),
            nameof(UnidirectionalEntityCompositeKey) => (IQueryable<TEntity>)_unidirectionalCompositeKeys.AsQueryable(),
            nameof(UnidirectionalEntityRoot) => (IQueryable<TEntity>)_unidirectionalRoots.AsQueryable(),
            nameof(UnidirectionalEntityBranch) => (IQueryable<TEntity>)_unidirectionalRoots.OfType<UnidirectionalEntityBranch>()
                .AsQueryable(),
            nameof(UnidirectionalEntityLeaf) => (IQueryable<TEntity>)_unidirectionalRoots.OfType<UnidirectionalEntityLeaf>()
                .AsQueryable(),
            _ => throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity))
        };

    private EntityOne[] CreateOnes(ManyToManyContext context)
        =>
        [
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 1, "EntityOne 1"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 2, "EntityOne 2"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 3, "EntityOne 3"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 4, "EntityOne 4"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 5, "EntityOne 5"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 6, "EntityOne 6"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 7, "EntityOne 7"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 8, "EntityOne 8"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 9, "EntityOne 9"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 10, "EntityOne 10"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 11, "EntityOne 11"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 12, "EntityOne 12"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 13, "EntityOne 13"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 14, "EntityOne 14"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 15, "EntityOne 15"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 16, "EntityOne 16"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 17, "EntityOne 17"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 18, "EntityOne 18"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 19, "EntityOne 19"),
            CreateEntityOne(context, _useGeneratedKeys ? 0 : 20, "EntityOne 20")
        ];

    private static EntityOne CreateEntityOne(ManyToManyContext context, int id, string name)
        => CreateInstance(
            context?.EntityOnes, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.Collection = CreateCollection<EntityTwo>(p);
                e.TwoSkip = CreateCollection<EntityTwo>(p);
                e.ThreeSkipPayloadFull = CreateCollection<EntityThree>(p);
                e.JoinThreePayloadFull = CreateCollection<JoinOneToThreePayloadFull>(p);
                e.TwoSkipShared = CreateCollection<EntityTwo>(p);
                e.ThreeSkipPayloadFullShared = CreateCollection<EntityThree>(p);
                e.JoinThreePayloadFullShared = CreateCollection<Dictionary<string, object>>(p);
                e.SelfSkipPayloadLeft = CreateCollection<EntityOne>(p);
                e.JoinSelfPayloadLeft = CreateCollection<JoinOneSelfPayload>(p);
                e.SelfSkipPayloadRight = CreateCollection<EntityOne>(p);
                e.JoinSelfPayloadRight = CreateCollection<JoinOneSelfPayload>(p);
                e.BranchSkip = CreateCollection<EntityBranch>(p);
            });

    private EntityTwo[] CreateTwos(ManyToManyContext context)
        =>
        [
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 1, "EntityTwo 1", null, _ones[0]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 2, "EntityTwo 2", null, _ones[0]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 3, "EntityTwo 3", null, null),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 4, "EntityTwo 4", null, _ones[2]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 5, "EntityTwo 5", null, _ones[2]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 6, "EntityTwo 6", null, _ones[4]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 7, "EntityTwo 7", null, _ones[4]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 8, "EntityTwo 8", null, _ones[6]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 9, "EntityTwo 9", null, _ones[6]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 10, "EntityTwo 10", _ones[19], _ones[8]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 11, "EntityTwo 11", _ones[17], _ones[8]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 12, "EntityTwo 12", _ones[15], _ones[10]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 13, "EntityTwo 13", _ones[13], _ones[10]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 14, "EntityTwo 14", _ones[11], _ones[12]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 15, "EntityTwo 15", _ones[10], _ones[12]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 16, "EntityTwo 16", _ones[8], _ones[14]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 17, "EntityTwo 17", _ones[6], _ones[14]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 18, "EntityTwo 18", _ones[4], _ones[15]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 19, "EntityTwo 19", _ones[2], _ones[15]),
            CreateEntityTwo(context, _useGeneratedKeys ? 0 : 20, "EntityTwo 20", _ones[0], _ones[16])
        ];

    private static EntityTwo CreateEntityTwo(
        ManyToManyContext context,
        int id,
        string name,
        EntityOne referenceInverse,
        EntityOne collectionInverse)
        => CreateInstance(
            context?.EntityTwos, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.ReferenceInverse = referenceInverse;
                e.CollectionInverse = collectionInverse;
                e.Collection = CreateCollection<EntityThree>(p);
                e.OneSkip = CreateCollection<EntityOne>(p);
                e.ThreeSkipFull = CreateCollection<EntityThree>(p);
                e.JoinThreeFull = CreateCollection<JoinTwoToThree>(p);
                e.SelfSkipSharedLeft = CreateCollection<EntityTwo>(p);
                e.SelfSkipSharedRight = CreateCollection<EntityTwo>(p);
                e.OneSkipShared = CreateCollection<EntityOne>(p);
                e.CompositeKeySkipShared = CreateCollection<EntityCompositeKey>(p);
            });

    private EntityThree[] CreateThrees(ManyToManyContext context)
        =>
        [
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 1, "EntityThree 1", null, null),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 2, "EntityThree 2", _twos[18], _twos[16]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 3, "EntityThree 3", _twos[1], _twos[15]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 4, "EntityThree 4", _twos[19], _twos[15]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 5, "EntityThree 5", _twos[3], _twos[14]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 6, "EntityThree 6", null, _twos[14]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 7, "EntityThree 7", _twos[5], _twos[12]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 8, "EntityThree 8", null, _twos[12]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 9, "EntityThree 9", _twos[7], _twos[10]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 10, "EntityThree 10", null, _twos[10]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 11, "EntityThree 11", _twos[18], _twos[8]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 12, "EntityThree 12", null, _twos[8]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 13, "EntityThree 13", _twos[11], _twos[6]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 14, "EntityThree 14", null, _twos[6]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 15, "EntityThree 15", _twos[13], _twos[4]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 16, "EntityThree 16", null, _twos[4]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 17, "EntityThree 17", _twos[15], _twos[2]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 18, "EntityThree 18", null, _twos[2]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 19, "EntityThree 19", _twos[17], _twos[0]),
            CreateEntityThree(context, _useGeneratedKeys ? 0 : 20, "EntityThree 20", null, _twos[0])
        ];

    private static EntityThree CreateEntityThree(
        ManyToManyContext context,
        int id,
        string name,
        EntityTwo referenceInverse,
        EntityTwo collectionInverse)
        => CreateInstance(
            context?.EntityThrees, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.ReferenceInverse = referenceInverse;
                e.CollectionInverse = collectionInverse;
                e.OneSkipPayloadFull = CreateCollection<EntityOne>(p);
                e.JoinOnePayloadFull = CreateCollection<JoinOneToThreePayloadFull>(p);
                e.TwoSkipFull = CreateCollection<EntityTwo>(p);
                e.JoinTwoFull = CreateCollection<JoinTwoToThree>(p);
                e.OneSkipPayloadFullShared = CreateCollection<EntityOne>(p);
                e.JoinOnePayloadFullShared = CreateCollection<Dictionary<string, object>>(p);
                e.CompositeKeySkipFull = CreateCollection<EntityCompositeKey>(p);
                e.JoinCompositeKeyFull = CreateCollection<JoinThreeToCompositeKeyFull>(p);
                e.RootSkipShared = CreateCollection<EntityRoot>(p);
            });

    private EntityCompositeKey[] CreateCompositeKeys(ManyToManyContext context)
        =>
        [
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 1, "1_1", new DateTime(2001, 1, 1), "Composite 1"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 1, "1_2", new DateTime(2001, 2, 1), "Composite 2"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 3, "3_1", new DateTime(2003, 1, 1), "Composite 3"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 3, "3_2", new DateTime(2003, 2, 1), "Composite 4"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 3, "3_3", new DateTime(2003, 3, 1), "Composite 5"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 6, "6_1", new DateTime(2006, 1, 1), "Composite 6"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 7, "7_1", new DateTime(2007, 1, 1), "Composite 7"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 7, "7_2", new DateTime(2007, 2, 1), "Composite 8"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_1", new DateTime(2008, 1, 1), "Composite 9"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_2", new DateTime(2008, 2, 1), "Composite 10"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_3", new DateTime(2008, 3, 1), "Composite 11"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_4", new DateTime(2008, 4, 1), "Composite 12"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_5", new DateTime(2008, 5, 1), "Composite 13"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_1", new DateTime(2009, 1, 1), "Composite 14"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_2", new DateTime(2009, 2, 1), "Composite 15"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_3", new DateTime(2009, 3, 1), "Composite 16"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_4", new DateTime(2009, 4, 1), "Composite 17"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_5", new DateTime(2009, 5, 1), "Composite 18"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_6", new DateTime(2009, 6, 1), "Composite 19"),
            CreateEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_7", new DateTime(2009, 7, 1), "Composite 20")
        ];

    private static EntityCompositeKey CreateEntityCompositeKey(
        ManyToManyContext context,
        int key1,
        string key2,
        DateTime key3,
        string name)
        => CreateInstance(
            context?.EntityCompositeKeys, (e, p) =>
            {
                e.Key1 = key1;
                e.Key2 = key2;
                e.Key3 = key3;
                e.Name = name;
                e.TwoSkipShared = CreateCollection<EntityTwo>(p);
                e.ThreeSkipFull = CreateCollection<EntityThree>(p);
                e.JoinThreeFull = CreateCollection<JoinThreeToCompositeKeyFull>(p);
                e.RootSkipShared = CreateCollection<EntityRoot>(p);
                e.LeafSkipFull = CreateCollection<EntityLeaf>(p);
                e.JoinLeafFull = CreateCollection<JoinCompositeKeyToLeaf>(p);
            });

    private EntityRoot[] CreateRoots(ManyToManyContext context)
        =>
        [
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 1, "Root 1"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 2, "Root 2"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 3, "Root 3"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 4, "Root 4"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 5, "Root 5"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 6, "Root 6"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 7, "Root 7"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 8, "Root 8"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 9, "Root 9"),
            CreateEntityRoot(context, _useGeneratedKeys ? 0 : 10, "Root 10"),
            CreateEntityBranch(context, _useGeneratedKeys ? 0 : 11, "Branch 1", 7),
            CreateEntityBranch(context, _useGeneratedKeys ? 0 : 12, "Branch 2", 77),
            CreateEntityBranch(context, _useGeneratedKeys ? 0 : 13, "Branch 3", 777),
            CreateEntityBranch(context, _useGeneratedKeys ? 0 : 14, "Branch 4", 7777),
            CreateEntityBranch(context, _useGeneratedKeys ? 0 : 15, "Branch 5", 77777),
            CreateEntityBranch(context, _useGeneratedKeys ? 0 : 16, "Branch 6", 777777),
            CreateEntityLeaf(context, _useGeneratedKeys ? 0 : 21, "Leaf 1", 42, true),
            CreateEntityLeaf(context, _useGeneratedKeys ? 0 : 22, "Leaf 2", 421, true),
            CreateEntityLeaf(context, _useGeneratedKeys ? 0 : 23, "Leaf 3", 1337, false),
            CreateEntityLeaf(context, _useGeneratedKeys ? 0 : 24, "Leaf 4", 1729, false)
        ];

    private static EntityRoot CreateEntityRoot(
        ManyToManyContext context,
        int id,
        string name)
        => CreateInstance(
            context?.EntityRoots, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.ThreeSkipShared = CreateCollection<EntityThree>(p);
                e.CompositeKeySkipShared = CreateCollection<EntityCompositeKey>(p);
            });

    private static EntityBranch CreateEntityBranch(
        ManyToManyContext context,
        int id,
        string name,
        long number)
        => CreateInstance(
            context?.Set<EntityBranch>(), (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.Number = number;
                e.ThreeSkipShared = CreateCollection<EntityThree>(p);
                e.CompositeKeySkipShared = CreateCollection<EntityCompositeKey>(p);
                e.OneSkip = CreateCollection<EntityOne>(p);
            });

    private static EntityLeaf CreateEntityLeaf(
        ManyToManyContext context,
        int id,
        string name,
        long number,
        bool? isGreen)
        => CreateInstance(
            context?.Set<EntityLeaf>(), (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.Number = number;
                e.IsGreen = isGreen;
                e.ThreeSkipShared = CreateCollection<EntityThree>(p);
                e.CompositeKeySkipShared = CreateCollection<EntityCompositeKey>(p);
                e.OneSkip = CreateCollection<EntityOne>(p);
                e.CompositeKeySkipFull = CreateCollection<EntityCompositeKey>(p);
                e.JoinCompositeKeyFull = CreateCollection<JoinCompositeKeyToLeaf>(p);
            });

    private JoinCompositeKeyToLeaf[] CreateJoinCompositeKeyToLeaves(ManyToManyContext context)
        =>
        [
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[0]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[1]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[1]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[17], _compositeKeys[2]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[2]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[19], _compositeKeys[3]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[17], _compositeKeys[4]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[5]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[17], _compositeKeys[7]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[19], _compositeKeys[7]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[8]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[9]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[17], _compositeKeys[10]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[10]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[12]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[12]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[19], _compositeKeys[12]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[13]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[17], _compositeKeys[13]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[13]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[14]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[17], _compositeKeys[14]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[15]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[15]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[19], _compositeKeys[15]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[16]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[19], _compositeKeys[16]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[18], _compositeKeys[17]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[19], _compositeKeys[17]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[16], _compositeKeys[18]),
            CreateJoinCompositeKeyToLeaf(context, (EntityLeaf)_roots[17], _compositeKeys[18])
        ];

    private static JoinCompositeKeyToLeaf CreateJoinCompositeKeyToLeaf(
        ManyToManyContext context,
        EntityLeaf leaf,
        EntityCompositeKey composite)
        => CreateInstance(
            context?.Set<JoinCompositeKeyToLeaf>(), (e, p) =>
            {
                e.Leaf = leaf;
                e.Composite = composite;
            });

    private JoinOneSelfPayload[] CreateJoinOneSelfPayloads(ManyToManyContext context)
        =>
        [
            CreateJoinOneSelfPayload(context, _ones[2], _ones[3], DateTime.Parse("2020-01-11 19:26:36")),
            CreateJoinOneSelfPayload(context, _ones[2], _ones[5], DateTime.Parse("2005-10-03 12:57:54")),
            CreateJoinOneSelfPayload(context, _ones[2], _ones[7], DateTime.Parse("2015-12-20 01:09:24")),
            CreateJoinOneSelfPayload(context, _ones[2], _ones[17], DateTime.Parse("1999-12-26 02:51:57")),
            CreateJoinOneSelfPayload(context, _ones[2], _ones[19], DateTime.Parse("2011-06-15 19:08:00")),
            CreateJoinOneSelfPayload(context, _ones[4], _ones[2], DateTime.Parse("2019-12-08 05:40:16")),
            CreateJoinOneSelfPayload(context, _ones[4], _ones[3], DateTime.Parse("2014-03-09 12:58:26")),
            CreateJoinOneSelfPayload(context, _ones[5], _ones[4], DateTime.Parse("2014-05-15 16:34:38")),
            CreateJoinOneSelfPayload(context, _ones[5], _ones[6], DateTime.Parse("2014-03-08 18:59:49")),
            CreateJoinOneSelfPayload(context, _ones[5], _ones[12], DateTime.Parse("2013-12-10 07:01:53")),
            CreateJoinOneSelfPayload(context, _ones[6], _ones[12], DateTime.Parse("2005-05-31 02:21:16")),
            CreateJoinOneSelfPayload(context, _ones[7], _ones[8], DateTime.Parse("2011-12-31 19:37:25")),
            CreateJoinOneSelfPayload(context, _ones[7], _ones[10], DateTime.Parse("2012-08-02 16:33:07")),
            CreateJoinOneSelfPayload(context, _ones[7], _ones[11], DateTime.Parse("2018-07-19 09:10:12")),
            CreateJoinOneSelfPayload(context, _ones[9], _ones[6], DateTime.Parse("2018-12-28 01:21:23")),
            CreateJoinOneSelfPayload(context, _ones[12], _ones[1], DateTime.Parse("2014-03-22 02:20:06")),
            CreateJoinOneSelfPayload(context, _ones[12], _ones[17], DateTime.Parse("2005-03-21 14:45:37")),
            CreateJoinOneSelfPayload(context, _ones[13], _ones[8], DateTime.Parse("2016-06-26 08:03:32")),
            CreateJoinOneSelfPayload(context, _ones[14], _ones[12], DateTime.Parse("2018-09-18 12:51:22")),
            CreateJoinOneSelfPayload(context, _ones[15], _ones[4], DateTime.Parse("2016-12-17 14:20:25")),
            CreateJoinOneSelfPayload(context, _ones[15], _ones[5], DateTime.Parse("2008-07-30 03:43:17")),
            CreateJoinOneSelfPayload(context, _ones[16], _ones[13], DateTime.Parse("2019-08-01 16:26:31")),
            CreateJoinOneSelfPayload(context, _ones[18], _ones[0], DateTime.Parse("2010-02-19 13:24:07")),
            CreateJoinOneSelfPayload(context, _ones[18], _ones[7], DateTime.Parse("2004-07-28 09:06:02")),
            CreateJoinOneSelfPayload(context, _ones[18], _ones[11], DateTime.Parse("2004-08-21 11:07:20")),
            CreateJoinOneSelfPayload(context, _ones[19], _ones[0], DateTime.Parse("2014-11-21 18:13:02")),
            CreateJoinOneSelfPayload(context, _ones[19], _ones[6], DateTime.Parse("2009-08-24 21:44:46")),
            CreateJoinOneSelfPayload(context, _ones[19], _ones[13], DateTime.Parse("2013-02-18 02:19:19")),
            CreateJoinOneSelfPayload(context, _ones[19], _ones[15], DateTime.Parse("2016-02-05 14:18:12"))
        ];

    private static JoinOneSelfPayload CreateJoinOneSelfPayload(
        ManyToManyContext context,
        EntityOne left,
        EntityOne right,
        DateTime payload)
        => CreateInstance(
            context?.Set<JoinOneSelfPayload>(), (e, p) =>
            {
                e.Left = left;
                e.Right = right;
                e.Payload = payload;
            });

    private JoinOneToBranch[] CreateJoinOneToBranches(ManyToManyContext context)
        =>
        [
            CreateJoinOneToBranch(context, _ones[1], _roots[15]),
            CreateJoinOneToBranch(context, _ones[1], _roots[19]),
            CreateJoinOneToBranch(context, _ones[2], _roots[13]),
            CreateJoinOneToBranch(context, _ones[2], _roots[15]),
            CreateJoinOneToBranch(context, _ones[2], _roots[17]),
            CreateJoinOneToBranch(context, _ones[2], _roots[19]),
            CreateJoinOneToBranch(context, _ones[4], _roots[12]),
            CreateJoinOneToBranch(context, _ones[5], _roots[15]),
            CreateJoinOneToBranch(context, _ones[5], _roots[17]),
            CreateJoinOneToBranch(context, _ones[5], _roots[18]),
            CreateJoinOneToBranch(context, _ones[7], _roots[10]),
            CreateJoinOneToBranch(context, _ones[7], _roots[11]),
            CreateJoinOneToBranch(context, _ones[7], _roots[12]),
            CreateJoinOneToBranch(context, _ones[8], _roots[10]),
            CreateJoinOneToBranch(context, _ones[8], _roots[11]),
            CreateJoinOneToBranch(context, _ones[8], _roots[13]),
            CreateJoinOneToBranch(context, _ones[8], _roots[15]),
            CreateJoinOneToBranch(context, _ones[8], _roots[16]),
            CreateJoinOneToBranch(context, _ones[8], _roots[19]),
            CreateJoinOneToBranch(context, _ones[9], _roots[11]),
            CreateJoinOneToBranch(context, _ones[9], _roots[12]),
            CreateJoinOneToBranch(context, _ones[9], _roots[13]),
            CreateJoinOneToBranch(context, _ones[9], _roots[16]),
            CreateJoinOneToBranch(context, _ones[11], _roots[10]),
            CreateJoinOneToBranch(context, _ones[11], _roots[11]),
            CreateJoinOneToBranch(context, _ones[11], _roots[13]),
            CreateJoinOneToBranch(context, _ones[11], _roots[18]),
            CreateJoinOneToBranch(context, _ones[12], _roots[14]),
            CreateJoinOneToBranch(context, _ones[13], _roots[11]),
            CreateJoinOneToBranch(context, _ones[13], _roots[13]),
            CreateJoinOneToBranch(context, _ones[13], _roots[15]),
            CreateJoinOneToBranch(context, _ones[13], _roots[18]),
            CreateJoinOneToBranch(context, _ones[14], _roots[14]),
            CreateJoinOneToBranch(context, _ones[14], _roots[15]),
            CreateJoinOneToBranch(context, _ones[14], _roots[19]),
            CreateJoinOneToBranch(context, _ones[15], _roots[10]),
            CreateJoinOneToBranch(context, _ones[16], _roots[10]),
            CreateJoinOneToBranch(context, _ones[16], _roots[16]),
            CreateJoinOneToBranch(context, _ones[17], _roots[11]),
            CreateJoinOneToBranch(context, _ones[17], _roots[14]),
            CreateJoinOneToBranch(context, _ones[17], _roots[19]),
            CreateJoinOneToBranch(context, _ones[18], _roots[10]),
            CreateJoinOneToBranch(context, _ones[18], _roots[11]),
            CreateJoinOneToBranch(context, _ones[18], _roots[15]),
            CreateJoinOneToBranch(context, _ones[18], _roots[18]),
            CreateJoinOneToBranch(context, _ones[19], _roots[16]),
            CreateJoinOneToBranch(context, _ones[19], _roots[18])
        ];

    private static JoinOneToBranch CreateJoinOneToBranch(
        ManyToManyContext context,
        EntityOne one,
        EntityRoot branch)
        => CreateInstance(
            context?.Set<JoinOneToBranch>(), (e, p) =>
            {
                e.EntityOneId = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e.EntityBranchId = context?.Entry(branch).Property(e => e.Id).CurrentValue ?? branch.Id;
            });

    private JoinOneToThreePayloadFull[] CreateJoinOneToThreePayloadFulls(ManyToManyContext context)
        =>
        [
            CreateJoinOneToThreePayloadFull(context, _ones[0], _threes[1], "Ira Watts"),
            CreateJoinOneToThreePayloadFull(context, _ones[0], _threes[5], "Harold May"),
            CreateJoinOneToThreePayloadFull(context, _ones[0], _threes[8], "Freda Vaughn"),
            CreateJoinOneToThreePayloadFull(context, _ones[0], _threes[12], "Pedro Mccarthy"),
            CreateJoinOneToThreePayloadFull(context, _ones[0], _threes[16], "Elaine Simon"),
            CreateJoinOneToThreePayloadFull(context, _ones[1], _threes[8], "Melvin Maldonado"),
            CreateJoinOneToThreePayloadFull(context, _ones[1], _threes[10], "Lora George"),
            CreateJoinOneToThreePayloadFull(context, _ones[1], _threes[12], "Joey Cohen"),
            CreateJoinOneToThreePayloadFull(context, _ones[1], _threes[13], "Erik Carroll"),
            CreateJoinOneToThreePayloadFull(context, _ones[1], _threes[15], "April Rodriguez"),
            CreateJoinOneToThreePayloadFull(context, _ones[2], _threes[4], "Gerardo Colon"),
            CreateJoinOneToThreePayloadFull(context, _ones[2], _threes[11], "Alexander Willis"),
            CreateJoinOneToThreePayloadFull(context, _ones[2], _threes[15], "Laura Wheeler"),
            CreateJoinOneToThreePayloadFull(context, _ones[2], _threes[18], "Lester Summers"),
            CreateJoinOneToThreePayloadFull(context, _ones[3], _threes[1], "Raquel Curry"),
            CreateJoinOneToThreePayloadFull(context, _ones[3], _threes[3], "Steven Fisher"),
            CreateJoinOneToThreePayloadFull(context, _ones[3], _threes[10], "Casey Williams"),
            CreateJoinOneToThreePayloadFull(context, _ones[3], _threes[12], "Lauren Clayton"),
            CreateJoinOneToThreePayloadFull(context, _ones[3], _threes[18], "Maureen Weber"),
            CreateJoinOneToThreePayloadFull(context, _ones[4], _threes[3], "Joyce Ford"),
            CreateJoinOneToThreePayloadFull(context, _ones[4], _threes[5], "Willie Mccormick"),
            CreateJoinOneToThreePayloadFull(context, _ones[4], _threes[8], "Geraldine Jackson"),
            CreateJoinOneToThreePayloadFull(context, _ones[6], _threes[0], "Victor Aguilar"),
            CreateJoinOneToThreePayloadFull(context, _ones[6], _threes[3], "Cathy Allen"),
            CreateJoinOneToThreePayloadFull(context, _ones[6], _threes[8], "Edwin Burke"),
            CreateJoinOneToThreePayloadFull(context, _ones[6], _threes[9], "Eugene Flores"),
            CreateJoinOneToThreePayloadFull(context, _ones[6], _threes[10], "Ginger Patton"),
            CreateJoinOneToThreePayloadFull(context, _ones[6], _threes[11], "Israel Mitchell"),
            CreateJoinOneToThreePayloadFull(context, _ones[6], _threes[17], "Joy Francis"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[0], "Orville Parker"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[2], "Alyssa Mann"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[3], "Hugh Daniel"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[12], "Kim Craig"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[13], "Lucille Moreno"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[16], "Virgil Drake"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[17], "Josephine Dawson"),
            CreateJoinOneToThreePayloadFull(context, _ones[7], _threes[19], "Milton Huff"),
            CreateJoinOneToThreePayloadFull(context, _ones[8], _threes[1], "Jody Clarke"),
            CreateJoinOneToThreePayloadFull(context, _ones[8], _threes[8], "Elisa Cooper"),
            CreateJoinOneToThreePayloadFull(context, _ones[8], _threes[10], "Grace Owen"),
            CreateJoinOneToThreePayloadFull(context, _ones[8], _threes[11], "Donald Welch"),
            CreateJoinOneToThreePayloadFull(context, _ones[8], _threes[14], "Marian Day"),
            CreateJoinOneToThreePayloadFull(context, _ones[8], _threes[16], "Cory Cortez"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[1], "Chad Rowe"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[2], "Simon Reyes"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[3], "Shari Jensen"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[7], "Ricky Bradley"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[9], "Debra Gibbs"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[10], "Everett Mckenzie"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[13], "Kirk Graham"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[15], "Paulette Adkins"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[17], "Raul Holloway"),
            CreateJoinOneToThreePayloadFull(context, _ones[9], _threes[18], "Danielle Ross"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[0], "Frank Garner"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[5], "Stella Thompson"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[7], "Peggy Wagner"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[8], "Geneva Holmes"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[9], "Ignacio Black"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[12], "Phillip Wells"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[13], "Hubert Lambert"),
            CreateJoinOneToThreePayloadFull(context, _ones[10], _threes[18], "Courtney Gregory"),
            CreateJoinOneToThreePayloadFull(context, _ones[11], _threes[1], "Esther Carter"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[5], "Thomas Benson"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[8], "Kara Baldwin"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[9], "Yvonne Sparks"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[10], "Darin Mathis"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[11], "Glenda Castillo"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[12], "Larry Walters"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[14], "Meredith Yates"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[15], "Rosemarie Henry"),
            CreateJoinOneToThreePayloadFull(context, _ones[12], _threes[17], "Nora Leonard"),
            CreateJoinOneToThreePayloadFull(context, _ones[13], _threes[16], "Corey Delgado"),
            CreateJoinOneToThreePayloadFull(context, _ones[13], _threes[17], "Kari Strickland"),
            CreateJoinOneToThreePayloadFull(context, _ones[14], _threes[7], "Joann Stanley"),
            CreateJoinOneToThreePayloadFull(context, _ones[14], _threes[10], "Camille Gordon"),
            CreateJoinOneToThreePayloadFull(context, _ones[14], _threes[13], "Flora Anderson"),
            CreateJoinOneToThreePayloadFull(context, _ones[14], _threes[14], "Wilbur Soto"),
            CreateJoinOneToThreePayloadFull(context, _ones[14], _threes[17], "Shirley Andrews"),
            CreateJoinOneToThreePayloadFull(context, _ones[14], _threes[19], "Marcus Mcguire"),
            CreateJoinOneToThreePayloadFull(context, _ones[15], _threes[0], "Saul Dixon"),
            CreateJoinOneToThreePayloadFull(context, _ones[15], _threes[5], "Cynthia Hart"),
            CreateJoinOneToThreePayloadFull(context, _ones[15], _threes[9], "Elbert Spencer"),
            CreateJoinOneToThreePayloadFull(context, _ones[15], _threes[12], "Darrell Norris"),
            CreateJoinOneToThreePayloadFull(context, _ones[15], _threes[13], "Jamie Kelley"),
            CreateJoinOneToThreePayloadFull(context, _ones[15], _threes[14], "Francis Briggs"),
            CreateJoinOneToThreePayloadFull(context, _ones[15], _threes[15], "Lindsey Morris"),
            CreateJoinOneToThreePayloadFull(context, _ones[16], _threes[1], "James Castro"),
            CreateJoinOneToThreePayloadFull(context, _ones[16], _threes[4], "Carlos Chavez"),
            CreateJoinOneToThreePayloadFull(context, _ones[16], _threes[6], "Janis Valdez"),
            CreateJoinOneToThreePayloadFull(context, _ones[16], _threes[12], "Alfredo Bowen"),
            CreateJoinOneToThreePayloadFull(context, _ones[16], _threes[13], "Viola Torres"),
            CreateJoinOneToThreePayloadFull(context, _ones[16], _threes[14], "Dianna Lowe"),
            CreateJoinOneToThreePayloadFull(context, _ones[17], _threes[2], "Craig Howell"),
            CreateJoinOneToThreePayloadFull(context, _ones[17], _threes[6], "Sandy Curtis"),
            CreateJoinOneToThreePayloadFull(context, _ones[17], _threes[11], "Alonzo Pierce"),
            CreateJoinOneToThreePayloadFull(context, _ones[17], _threes[17], "Albert Harper"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[1], "Frankie Baker"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[4], "Candace Tucker"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[5], "Willis Christensen"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[6], "Juan Joseph"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[9], "Thelma Sanders"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[10], "Kerry West"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[14], "Sheri Castro"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[15], "Mark Schultz"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[16], "Priscilla Summers"),
            CreateJoinOneToThreePayloadFull(context, _ones[18], _threes[19], "Allan Valdez"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[2], "Bill Peters"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[4], "Cora Stone"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[5], "Frankie Pope"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[9], "Christian Young"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[10], "Shari Brewer"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[11], "Antonia Wolfe"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[13], "Lawrence Matthews"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[17], "Van Hubbard"),
            CreateJoinOneToThreePayloadFull(context, _ones[19], _threes[19], "Lindsay Pena")
        ];

    private static JoinOneToThreePayloadFull CreateJoinOneToThreePayloadFull(
        ManyToManyContext context,
        EntityOne one,
        EntityThree three,
        string payload)
        => CreateInstance(
            context?.Set<JoinOneToThreePayloadFull>(), (e, p) =>
            {
                e.One = one;
                e.Three = three;
                e.Payload = payload;
            });

    private JoinOneToTwo[] CreateJoinOneToTwos(ManyToManyContext context)
        =>
        [
            CreateJoinOneToTwo(context, _ones[0], _twos[0]),
            CreateJoinOneToTwo(context, _ones[0], _twos[1]),
            CreateJoinOneToTwo(context, _ones[0], _twos[2]),
            CreateJoinOneToTwo(context, _ones[0], _twos[3]),
            CreateJoinOneToTwo(context, _ones[0], _twos[4]),
            CreateJoinOneToTwo(context, _ones[0], _twos[5]),
            CreateJoinOneToTwo(context, _ones[0], _twos[6]),
            CreateJoinOneToTwo(context, _ones[0], _twos[7]),
            CreateJoinOneToTwo(context, _ones[0], _twos[8]),
            CreateJoinOneToTwo(context, _ones[0], _twos[9]),
            CreateJoinOneToTwo(context, _ones[0], _twos[10]),
            CreateJoinOneToTwo(context, _ones[0], _twos[11]),
            CreateJoinOneToTwo(context, _ones[0], _twos[12]),
            CreateJoinOneToTwo(context, _ones[0], _twos[13]),
            CreateJoinOneToTwo(context, _ones[0], _twos[14]),
            CreateJoinOneToTwo(context, _ones[0], _twos[15]),
            CreateJoinOneToTwo(context, _ones[0], _twos[16]),
            CreateJoinOneToTwo(context, _ones[0], _twos[17]),
            CreateJoinOneToTwo(context, _ones[0], _twos[18]),
            CreateJoinOneToTwo(context, _ones[0], _twos[19]),
            CreateJoinOneToTwo(context, _ones[1], _twos[0]),
            CreateJoinOneToTwo(context, _ones[1], _twos[2]),
            CreateJoinOneToTwo(context, _ones[1], _twos[4]),
            CreateJoinOneToTwo(context, _ones[1], _twos[6]),
            CreateJoinOneToTwo(context, _ones[1], _twos[8]),
            CreateJoinOneToTwo(context, _ones[1], _twos[10]),
            CreateJoinOneToTwo(context, _ones[1], _twos[12]),
            CreateJoinOneToTwo(context, _ones[1], _twos[14]),
            CreateJoinOneToTwo(context, _ones[1], _twos[16]),
            CreateJoinOneToTwo(context, _ones[1], _twos[18]),
            CreateJoinOneToTwo(context, _ones[2], _twos[0]),
            CreateJoinOneToTwo(context, _ones[2], _twos[3]),
            CreateJoinOneToTwo(context, _ones[2], _twos[6]),
            CreateJoinOneToTwo(context, _ones[2], _twos[9]),
            CreateJoinOneToTwo(context, _ones[2], _twos[12]),
            CreateJoinOneToTwo(context, _ones[2], _twos[15]),
            CreateJoinOneToTwo(context, _ones[2], _twos[18]),
            CreateJoinOneToTwo(context, _ones[3], _twos[0]),
            CreateJoinOneToTwo(context, _ones[3], _twos[4]),
            CreateJoinOneToTwo(context, _ones[3], _twos[8]),
            CreateJoinOneToTwo(context, _ones[3], _twos[12]),
            CreateJoinOneToTwo(context, _ones[3], _twos[16]),
            CreateJoinOneToTwo(context, _ones[4], _twos[0]),
            CreateJoinOneToTwo(context, _ones[4], _twos[5]),
            CreateJoinOneToTwo(context, _ones[4], _twos[10]),
            CreateJoinOneToTwo(context, _ones[4], _twos[15]),
            CreateJoinOneToTwo(context, _ones[5], _twos[0]),
            CreateJoinOneToTwo(context, _ones[5], _twos[6]),
            CreateJoinOneToTwo(context, _ones[5], _twos[12]),
            CreateJoinOneToTwo(context, _ones[5], _twos[18]),
            CreateJoinOneToTwo(context, _ones[6], _twos[0]),
            CreateJoinOneToTwo(context, _ones[6], _twos[7]),
            CreateJoinOneToTwo(context, _ones[6], _twos[14]),
            CreateJoinOneToTwo(context, _ones[7], _twos[0]),
            CreateJoinOneToTwo(context, _ones[7], _twos[8]),
            CreateJoinOneToTwo(context, _ones[7], _twos[16]),
            CreateJoinOneToTwo(context, _ones[8], _twos[0]),
            CreateJoinOneToTwo(context, _ones[8], _twos[9]),
            CreateJoinOneToTwo(context, _ones[8], _twos[18]),
            CreateJoinOneToTwo(context, _ones[9], _twos[0]),
            CreateJoinOneToTwo(context, _ones[9], _twos[10]),
            CreateJoinOneToTwo(context, _ones[10], _twos[19]),
            CreateJoinOneToTwo(context, _ones[10], _twos[18]),
            CreateJoinOneToTwo(context, _ones[10], _twos[17]),
            CreateJoinOneToTwo(context, _ones[10], _twos[16]),
            CreateJoinOneToTwo(context, _ones[10], _twos[15]),
            CreateJoinOneToTwo(context, _ones[10], _twos[14]),
            CreateJoinOneToTwo(context, _ones[10], _twos[13]),
            CreateJoinOneToTwo(context, _ones[10], _twos[12]),
            CreateJoinOneToTwo(context, _ones[10], _twos[11]),
            CreateJoinOneToTwo(context, _ones[10], _twos[10]),
            CreateJoinOneToTwo(context, _ones[10], _twos[9]),
            CreateJoinOneToTwo(context, _ones[10], _twos[8]),
            CreateJoinOneToTwo(context, _ones[10], _twos[7]),
            CreateJoinOneToTwo(context, _ones[10], _twos[6]),
            CreateJoinOneToTwo(context, _ones[10], _twos[5]),
            CreateJoinOneToTwo(context, _ones[10], _twos[4]),
            CreateJoinOneToTwo(context, _ones[10], _twos[3]),
            CreateJoinOneToTwo(context, _ones[10], _twos[2]),
            CreateJoinOneToTwo(context, _ones[10], _twos[1]),
            CreateJoinOneToTwo(context, _ones[10], _twos[0]),
            CreateJoinOneToTwo(context, _ones[11], _twos[19]),
            CreateJoinOneToTwo(context, _ones[11], _twos[16]),
            CreateJoinOneToTwo(context, _ones[11], _twos[13]),
            CreateJoinOneToTwo(context, _ones[11], _twos[10]),
            CreateJoinOneToTwo(context, _ones[11], _twos[7]),
            CreateJoinOneToTwo(context, _ones[11], _twos[4]),
            CreateJoinOneToTwo(context, _ones[11], _twos[1]),
            CreateJoinOneToTwo(context, _ones[12], _twos[19]),
            CreateJoinOneToTwo(context, _ones[12], _twos[15]),
            CreateJoinOneToTwo(context, _ones[12], _twos[11]),
            CreateJoinOneToTwo(context, _ones[12], _twos[7]),
            CreateJoinOneToTwo(context, _ones[12], _twos[3]),
            CreateJoinOneToTwo(context, _ones[13], _twos[19]),
            CreateJoinOneToTwo(context, _ones[13], _twos[14]),
            CreateJoinOneToTwo(context, _ones[13], _twos[9]),
            CreateJoinOneToTwo(context, _ones[13], _twos[4]),
            CreateJoinOneToTwo(context, _ones[14], _twos[19]),
            CreateJoinOneToTwo(context, _ones[14], _twos[13]),
            CreateJoinOneToTwo(context, _ones[14], _twos[7]),
            CreateJoinOneToTwo(context, _ones[14], _twos[1]),
            CreateJoinOneToTwo(context, _ones[15], _twos[19]),
            CreateJoinOneToTwo(context, _ones[15], _twos[12]),
            CreateJoinOneToTwo(context, _ones[15], _twos[5]),
            CreateJoinOneToTwo(context, _ones[16], _twos[19]),
            CreateJoinOneToTwo(context, _ones[16], _twos[11]),
            CreateJoinOneToTwo(context, _ones[16], _twos[3]),
            CreateJoinOneToTwo(context, _ones[17], _twos[19]),
            CreateJoinOneToTwo(context, _ones[17], _twos[10]),
            CreateJoinOneToTwo(context, _ones[17], _twos[1]),
            CreateJoinOneToTwo(context, _ones[18], _twos[19]),
            CreateJoinOneToTwo(context, _ones[18], _twos[9])
        ];

    private static JoinOneToTwo CreateJoinOneToTwo(
        ManyToManyContext context,
        EntityOne one,
        EntityTwo two)
        => CreateInstance(
            context?.Set<JoinOneToTwo>(), (e, p) =>
            {
                e.OneId = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e.TwoId = context?.Entry(two).Property(e => e.Id).CurrentValue ?? two.Id;
            });

    private JoinThreeToCompositeKeyFull[] CreateJoinThreeToCompositeKeyFulls(ManyToManyContext context)
        =>
        [
            CreateJoinThreeToCompositeKeyFull(context, _threes[0], _compositeKeys[5]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[1], _compositeKeys[0]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[1], _compositeKeys[14]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[1], _compositeKeys[19]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[2], _compositeKeys[5]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[2], _compositeKeys[14]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[2], _compositeKeys[19]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[4], _compositeKeys[11]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[4], _compositeKeys[12]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[4], _compositeKeys[17]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[5], _compositeKeys[5]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[6], _compositeKeys[3]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[6], _compositeKeys[8]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[7], _compositeKeys[10]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[7], _compositeKeys[18]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[8], _compositeKeys[8]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[8], _compositeKeys[15]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[9], _compositeKeys[15]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[10], _compositeKeys[6]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[10], _compositeKeys[14]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[11], _compositeKeys[7]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[11], _compositeKeys[10]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[11], _compositeKeys[12]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[12], _compositeKeys[5]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[12], _compositeKeys[7]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[12], _compositeKeys[13]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[12], _compositeKeys[14]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[13], _compositeKeys[9]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[13], _compositeKeys[12]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[13], _compositeKeys[15]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[14], _compositeKeys[9]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[14], _compositeKeys[13]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[14], _compositeKeys[18]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[15], _compositeKeys[4]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[15], _compositeKeys[6]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[15], _compositeKeys[18]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[16], _compositeKeys[1]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[16], _compositeKeys[9]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[17], _compositeKeys[3]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[18], _compositeKeys[1]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[18], _compositeKeys[12]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[18], _compositeKeys[14]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[18], _compositeKeys[19]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[19], _compositeKeys[3]),
            CreateJoinThreeToCompositeKeyFull(context, _threes[19], _compositeKeys[6])
        ];

    private static JoinThreeToCompositeKeyFull CreateJoinThreeToCompositeKeyFull(
        ManyToManyContext context,
        EntityThree three,
        EntityCompositeKey composite)
        => CreateInstance(
            context?.Set<JoinThreeToCompositeKeyFull>(), (e, p) =>
            {
                e.Three = three;
                e.Composite = composite;
            });

    private JoinTwoToThree[] CreateJoinTwoToThrees(ManyToManyContext context)
        =>
        [
            CreateJoinTwoToThree(context, _twos[0], _threes[1]),
            CreateJoinTwoToThree(context, _twos[0], _threes[2]),
            CreateJoinTwoToThree(context, _twos[0], _threes[12]),
            CreateJoinTwoToThree(context, _twos[0], _threes[17]),
            CreateJoinTwoToThree(context, _twos[1], _threes[0]),
            CreateJoinTwoToThree(context, _twos[1], _threes[8]),
            CreateJoinTwoToThree(context, _twos[1], _threes[14]),
            CreateJoinTwoToThree(context, _twos[2], _threes[10]),
            CreateJoinTwoToThree(context, _twos[2], _threes[16]),
            CreateJoinTwoToThree(context, _twos[3], _threes[1]),
            CreateJoinTwoToThree(context, _twos[3], _threes[4]),
            CreateJoinTwoToThree(context, _twos[3], _threes[10]),
            CreateJoinTwoToThree(context, _twos[4], _threes[3]),
            CreateJoinTwoToThree(context, _twos[4], _threes[4]),
            CreateJoinTwoToThree(context, _twos[5], _threes[2]),
            CreateJoinTwoToThree(context, _twos[5], _threes[9]),
            CreateJoinTwoToThree(context, _twos[5], _threes[15]),
            CreateJoinTwoToThree(context, _twos[5], _threes[17]),
            CreateJoinTwoToThree(context, _twos[6], _threes[11]),
            CreateJoinTwoToThree(context, _twos[6], _threes[14]),
            CreateJoinTwoToThree(context, _twos[6], _threes[19]),
            CreateJoinTwoToThree(context, _twos[7], _threes[0]),
            CreateJoinTwoToThree(context, _twos[7], _threes[2]),
            CreateJoinTwoToThree(context, _twos[7], _threes[19]),
            CreateJoinTwoToThree(context, _twos[8], _threes[2]),
            CreateJoinTwoToThree(context, _twos[8], _threes[12]),
            CreateJoinTwoToThree(context, _twos[8], _threes[18]),
            CreateJoinTwoToThree(context, _twos[9], _threes[16]),
            CreateJoinTwoToThree(context, _twos[10], _threes[5]),
            CreateJoinTwoToThree(context, _twos[10], _threes[6]),
            CreateJoinTwoToThree(context, _twos[10], _threes[7]),
            CreateJoinTwoToThree(context, _twos[10], _threes[12]),
            CreateJoinTwoToThree(context, _twos[11], _threes[8]),
            CreateJoinTwoToThree(context, _twos[12], _threes[0]),
            CreateJoinTwoToThree(context, _twos[12], _threes[10]),
            CreateJoinTwoToThree(context, _twos[12], _threes[18]),
            CreateJoinTwoToThree(context, _twos[13], _threes[1]),
            CreateJoinTwoToThree(context, _twos[14], _threes[16]),
            CreateJoinTwoToThree(context, _twos[15], _threes[2]),
            CreateJoinTwoToThree(context, _twos[15], _threes[15]),
            CreateJoinTwoToThree(context, _twos[17], _threes[0]),
            CreateJoinTwoToThree(context, _twos[17], _threes[4]),
            CreateJoinTwoToThree(context, _twos[17], _threes[9]),
            CreateJoinTwoToThree(context, _twos[18], _threes[4]),
            CreateJoinTwoToThree(context, _twos[18], _threes[15]),
            CreateJoinTwoToThree(context, _twos[18], _threes[17]),
            CreateJoinTwoToThree(context, _twos[19], _threes[5]),
            CreateJoinTwoToThree(context, _twos[19], _threes[9]),
            CreateJoinTwoToThree(context, _twos[19], _threes[11]),
            CreateJoinTwoToThree(context, _twos[19], _threes[15]),
            CreateJoinTwoToThree(context, _twos[19], _threes[16]),
            CreateJoinTwoToThree(context, _twos[19], _threes[17])
        ];

    private static JoinTwoToThree CreateJoinTwoToThree(
        ManyToManyContext context,
        EntityTwo two,
        EntityThree three)
        => CreateInstance(
            context?.Set<JoinTwoToThree>(), (e, p) =>
            {
                e.Two = two;
                e.Three = three;
            });

    private Dictionary<string, object>[] CreateEntityOneEntityTwos(ManyToManyContext context)
        =>
        [
            CreateEntityOneEntityTwo(context, _ones[0], _twos[2]),
            CreateEntityOneEntityTwo(context, _ones[0], _twos[15]),
            CreateEntityOneEntityTwo(context, _ones[1], _twos[2]),
            CreateEntityOneEntityTwo(context, _ones[1], _twos[9]),
            CreateEntityOneEntityTwo(context, _ones[1], _twos[17]),
            CreateEntityOneEntityTwo(context, _ones[2], _twos[9]),
            CreateEntityOneEntityTwo(context, _ones[2], _twos[10]),
            CreateEntityOneEntityTwo(context, _ones[2], _twos[15]),
            CreateEntityOneEntityTwo(context, _ones[4], _twos[1]),
            CreateEntityOneEntityTwo(context, _ones[4], _twos[4]),
            CreateEntityOneEntityTwo(context, _ones[4], _twos[6]),
            CreateEntityOneEntityTwo(context, _ones[4], _twos[8]),
            CreateEntityOneEntityTwo(context, _ones[4], _twos[13]),
            CreateEntityOneEntityTwo(context, _ones[5], _twos[11]),
            CreateEntityOneEntityTwo(context, _ones[6], _twos[2]),
            CreateEntityOneEntityTwo(context, _ones[6], _twos[15]),
            CreateEntityOneEntityTwo(context, _ones[6], _twos[16]),
            CreateEntityOneEntityTwo(context, _ones[7], _twos[18]),
            CreateEntityOneEntityTwo(context, _ones[8], _twos[8]),
            CreateEntityOneEntityTwo(context, _ones[8], _twos[10]),
            CreateEntityOneEntityTwo(context, _ones[9], _twos[5]),
            CreateEntityOneEntityTwo(context, _ones[9], _twos[16]),
            CreateEntityOneEntityTwo(context, _ones[9], _twos[19]),
            CreateEntityOneEntityTwo(context, _ones[10], _twos[16]),
            CreateEntityOneEntityTwo(context, _ones[10], _twos[17]),
            CreateEntityOneEntityTwo(context, _ones[11], _twos[5]),
            CreateEntityOneEntityTwo(context, _ones[11], _twos[18]),
            CreateEntityOneEntityTwo(context, _ones[12], _twos[6]),
            CreateEntityOneEntityTwo(context, _ones[12], _twos[7]),
            CreateEntityOneEntityTwo(context, _ones[12], _twos[8]),
            CreateEntityOneEntityTwo(context, _ones[12], _twos[12]),
            CreateEntityOneEntityTwo(context, _ones[13], _twos[3]),
            CreateEntityOneEntityTwo(context, _ones[13], _twos[8]),
            CreateEntityOneEntityTwo(context, _ones[13], _twos[18]),
            CreateEntityOneEntityTwo(context, _ones[14], _twos[9]),
            CreateEntityOneEntityTwo(context, _ones[15], _twos[0]),
            CreateEntityOneEntityTwo(context, _ones[15], _twos[6]),
            CreateEntityOneEntityTwo(context, _ones[15], _twos[18]),
            CreateEntityOneEntityTwo(context, _ones[16], _twos[7]),
            CreateEntityOneEntityTwo(context, _ones[16], _twos[14]),
            CreateEntityOneEntityTwo(context, _ones[17], _twos[3]),
            CreateEntityOneEntityTwo(context, _ones[17], _twos[12]),
            CreateEntityOneEntityTwo(context, _ones[17], _twos[13]),
            CreateEntityOneEntityTwo(context, _ones[18], _twos[3]),
            CreateEntityOneEntityTwo(context, _ones[18], _twos[13])
        ];

    private static Dictionary<string, object> CreateEntityOneEntityTwo(
        ManyToManyContext context,
        EntityOne one,
        EntityTwo two)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("EntityOneEntityTwo"), (e, p) =>
            {
                e["OneSkipSharedId"] = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e["TwoSkipSharedId"] = context?.Entry(two).Property(e => e.Id).CurrentValue ?? two.Id;
            });

    private Dictionary<string, object>[] CreateJoinOneToThreePayloadFullShareds(ManyToManyContext context)
        =>
        [
            CreateJoinOneToThreePayloadFullShared(context, _ones[2], _threes[0], "Capbrough"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[2], _threes[1], "East Eastdol"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[2], _threes[3], "Southingville"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[2], _threes[8], "Goldbrough"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[3], _threes[4], "Readingworth"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[3], _threes[17], "Skillpool"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[4], _threes[0], "Lawgrad"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[4], _threes[3], "Kettleham Park"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[4], _threes[8], "Sayford Park"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[4], _threes[15], "Hamstead"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[5], _threes[10], "North Starside"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[5], _threes[12], "Goldfolk"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[6], _threes[3], "Winstead"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[7], _threes[10], "Transworth"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[7], _threes[17], "Parkpool"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[7], _threes[18], "Fishham"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[9], _threes[0], "Passmouth"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[9], _threes[4], "Valenfield"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[9], _threes[19], "Passford Park"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[10], _threes[9], "Chatfield"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[11], _threes[10], "Hosview"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[11], _threes[16], "Dodgewich"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[12], _threes[2], "Skillhampton"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[12], _threes[13], "Hardcaster"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[12], _threes[15], "Hollowmouth"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[13], _threes[5], "Cruxcaster"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[13], _threes[10], "Elcaster"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[13], _threes[16], "Clambrough"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[14], _threes[9], "Millwich"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[14], _threes[12], "Hapcester"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[15], _threes[6], "Sanddol Beach"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[15], _threes[12], "Hamcaster"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[16], _threes[8], "New Foxbrough"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[16], _threes[12], "Chatpool"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[17], _threes[7], "Duckworth"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[17], _threes[11], "Snowham"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[17], _threes[12], "Bannview Island"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[19], _threes[3], "Rockbrough"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[19], _threes[4], "Sweetfield"),
            CreateJoinOneToThreePayloadFullShared(context, _ones[19], _threes[15], "Bayburgh Hills")
        ];

    private static Dictionary<string, object> CreateJoinOneToThreePayloadFullShared(
        ManyToManyContext context,
        EntityOne one,
        EntityThree three,
        string payload)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared"), (e, p) =>
            {
                e["OneId"] = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e["ThreeId"] = context?.Entry(three).Property(e => e.Id).CurrentValue ?? three.Id;
                e["Payload"] = payload;
            });

    private Dictionary<string, object>[] CreateJoinTwoSelfShareds(ManyToManyContext context)
        =>
        [
            CreateJoinTwoSelfShared(context, _twos[0], _twos[8]),
            CreateJoinTwoSelfShared(context, _twos[0], _twos[9]),
            CreateJoinTwoSelfShared(context, _twos[0], _twos[10]),
            CreateJoinTwoSelfShared(context, _twos[0], _twos[17]),
            CreateJoinTwoSelfShared(context, _twos[2], _twos[1]),
            CreateJoinTwoSelfShared(context, _twos[2], _twos[4]),
            CreateJoinTwoSelfShared(context, _twos[2], _twos[7]),
            CreateJoinTwoSelfShared(context, _twos[2], _twos[17]),
            CreateJoinTwoSelfShared(context, _twos[2], _twos[18]),
            CreateJoinTwoSelfShared(context, _twos[3], _twos[10]),
            CreateJoinTwoSelfShared(context, _twos[4], _twos[7]),
            CreateJoinTwoSelfShared(context, _twos[5], _twos[17]),
            CreateJoinTwoSelfShared(context, _twos[7], _twos[1]),
            CreateJoinTwoSelfShared(context, _twos[7], _twos[13]),
            CreateJoinTwoSelfShared(context, _twos[7], _twos[14]),
            CreateJoinTwoSelfShared(context, _twos[7], _twos[19]),
            CreateJoinTwoSelfShared(context, _twos[8], _twos[3]),
            CreateJoinTwoSelfShared(context, _twos[8], _twos[13]),
            CreateJoinTwoSelfShared(context, _twos[9], _twos[4]),
            CreateJoinTwoSelfShared(context, _twos[11], _twos[12]),
            CreateJoinTwoSelfShared(context, _twos[11], _twos[13]),
            CreateJoinTwoSelfShared(context, _twos[12], _twos[13]),
            CreateJoinTwoSelfShared(context, _twos[12], _twos[17]),
            CreateJoinTwoSelfShared(context, _twos[12], _twos[18]),
            CreateJoinTwoSelfShared(context, _twos[15], _twos[5]),
            CreateJoinTwoSelfShared(context, _twos[16], _twos[8]),
            CreateJoinTwoSelfShared(context, _twos[16], _twos[18]),
            CreateJoinTwoSelfShared(context, _twos[16], _twos[19]),
            CreateJoinTwoSelfShared(context, _twos[17], _twos[1]),
            CreateJoinTwoSelfShared(context, _twos[17], _twos[4]),
            CreateJoinTwoSelfShared(context, _twos[17], _twos[15]),
            CreateJoinTwoSelfShared(context, _twos[17], _twos[16]),
            CreateJoinTwoSelfShared(context, _twos[18], _twos[1]),
            CreateJoinTwoSelfShared(context, _twos[19], _twos[3])
        ];

    private static Dictionary<string, object> CreateJoinTwoSelfShared(
        ManyToManyContext context,
        EntityTwo left,
        EntityTwo right)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("EntityTwoEntityTwo"), (e, p) =>
            {
                e["SelfSkipSharedLeftId"] = context?.Entry(left).Property(e => e.Id).CurrentValue ?? left.Id;
                e["SelfSkipSharedRightId"] = context?.Entry(right).Property(e => e.Id).CurrentValue ?? right.Id;
            });

    private Dictionary<string, object>[] CreateJoinTwoToCompositeKeyShareds(ManyToManyContext context)
        =>
        [
            CreateJoinTwoToCompositeKeyShared(context, _twos[0], _compositeKeys[0]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[0], _compositeKeys[3]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[0], _compositeKeys[4]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[1], _compositeKeys[3]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[2], _compositeKeys[5]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[3], _compositeKeys[1]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[3], _compositeKeys[18]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[5], _compositeKeys[2]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[5], _compositeKeys[12]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[6], _compositeKeys[7]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[8], _compositeKeys[2]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[8], _compositeKeys[8]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[9], _compositeKeys[0]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[9], _compositeKeys[14]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[9], _compositeKeys[17]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[10], _compositeKeys[0]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[10], _compositeKeys[14]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[11], _compositeKeys[7]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[11], _compositeKeys[12]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[11], _compositeKeys[14]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[12], _compositeKeys[0]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[12], _compositeKeys[6]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[12], _compositeKeys[16]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[14], _compositeKeys[15]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[15], _compositeKeys[0]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[15], _compositeKeys[2]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[15], _compositeKeys[18]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[16], _compositeKeys[1]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[16], _compositeKeys[7]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[16], _compositeKeys[13]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[16], _compositeKeys[14]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[18], _compositeKeys[4]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[19], _compositeKeys[2]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[19], _compositeKeys[4]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[19], _compositeKeys[5]),
            CreateJoinTwoToCompositeKeyShared(context, _twos[19], _compositeKeys[13])
        ];

    private static Dictionary<string, object> CreateJoinTwoToCompositeKeyShared(
        ManyToManyContext context,
        EntityTwo two,
        EntityCompositeKey composite)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("EntityCompositeKeyEntityTwo"), (e, p) =>
            {
                e["TwoSkipSharedId"] = context?.Entry(two).Property(e => e.Id).CurrentValue ?? two.Id;
                e["CompositeKeySkipSharedKey1"] = context?.Entry(composite).Property(e => e.Key1).CurrentValue ?? composite.Key1;
                e["CompositeKeySkipSharedKey2"] = composite.Key2;
                e["CompositeKeySkipSharedKey3"] = composite.Key3;
            });

    private Dictionary<string, object>[] CreateEntityRootEntityThrees(ManyToManyContext context)
        =>
        [
            CreateEntityRootEntityThree(context, _threes[0], _roots[6]),
            CreateEntityRootEntityThree(context, _threes[0], _roots[7]),
            CreateEntityRootEntityThree(context, _threes[0], _roots[14]),
            CreateEntityRootEntityThree(context, _threes[1], _roots[3]),
            CreateEntityRootEntityThree(context, _threes[1], _roots[15]),
            CreateEntityRootEntityThree(context, _threes[2], _roots[11]),
            CreateEntityRootEntityThree(context, _threes[2], _roots[13]),
            CreateEntityRootEntityThree(context, _threes[2], _roots[19]),
            CreateEntityRootEntityThree(context, _threes[4], _roots[13]),
            CreateEntityRootEntityThree(context, _threes[4], _roots[14]),
            CreateEntityRootEntityThree(context, _threes[4], _roots[15]),
            CreateEntityRootEntityThree(context, _threes[5], _roots[16]),
            CreateEntityRootEntityThree(context, _threes[6], _roots[0]),
            CreateEntityRootEntityThree(context, _threes[6], _roots[5]),
            CreateEntityRootEntityThree(context, _threes[6], _roots[12]),
            CreateEntityRootEntityThree(context, _threes[6], _roots[19]),
            CreateEntityRootEntityThree(context, _threes[7], _roots[9]),
            CreateEntityRootEntityThree(context, _threes[9], _roots[2]),
            CreateEntityRootEntityThree(context, _threes[9], _roots[7]),
            CreateEntityRootEntityThree(context, _threes[12], _roots[4]),
            CreateEntityRootEntityThree(context, _threes[13], _roots[0]),
            CreateEntityRootEntityThree(context, _threes[13], _roots[13]),
            CreateEntityRootEntityThree(context, _threes[15], _roots[4]),
            CreateEntityRootEntityThree(context, _threes[15], _roots[6]),
            CreateEntityRootEntityThree(context, _threes[16], _roots[13]),
            CreateEntityRootEntityThree(context, _threes[17], _roots[5]),
            CreateEntityRootEntityThree(context, _threes[17], _roots[18]),
            CreateEntityRootEntityThree(context, _threes[18], _roots[10]),
            CreateEntityRootEntityThree(context, _threes[19], _roots[13])
        ];

    private static Dictionary<string, object> CreateEntityRootEntityThree(
        ManyToManyContext context,
        EntityThree three,
        EntityRoot root)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("EntityRootEntityThree"), (e, p) =>
            {
                e["ThreeSkipSharedId"] = context?.Entry(three).Property(e => e.Id).CurrentValue ?? three.Id;
                e["RootSkipSharedId"] = context?.Entry(root).Property(e => e.Id).CurrentValue ?? root.Id;
            });

    private Dictionary<string, object>[] CreateEntityRootEntityBranches(ManyToManyContext context)
    {
        var branches = _roots.OfType<EntityBranch>().ToList();
        return
        [
            CreateEntityRootEntityBranch(context, branches[0], _roots[6]),
            CreateEntityRootEntityBranch(context, branches[0], _roots[7]),
            CreateEntityRootEntityBranch(context, branches[0], _roots[14]),
            CreateEntityRootEntityBranch(context, branches[1], _roots[3]),
            CreateEntityRootEntityBranch(context, branches[1], _roots[15]),
            CreateEntityRootEntityBranch(context, branches[2], _roots[11]),
            CreateEntityRootEntityBranch(context, branches[2], _roots[13]),
            CreateEntityRootEntityBranch(context, branches[2], _roots[19]),
            CreateEntityRootEntityBranch(context, branches[4], _roots[13]),
            CreateEntityRootEntityBranch(context, branches[4], _roots[14]),
            CreateEntityRootEntityBranch(context, branches[4], _roots[15]),
            CreateEntityRootEntityBranch(context, branches[5], _roots[16]),
            CreateEntityRootEntityBranch(context, branches[6], _roots[0]),
            CreateEntityRootEntityBranch(context, branches[6], _roots[5])
        ];
    }

    private static Dictionary<string, object> CreateEntityRootEntityBranch(
        ManyToManyContext context,
        EntityBranch branch,
        EntityRoot root)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("EntityBranchEntityRoot"), (e, p) =>
            {
                e["BranchSkipSharedId"] = context?.Entry(branch).Property(e => e.Id).CurrentValue ?? branch.Id;
                e["RootSkipSharedId"] = context?.Entry(root).Property(e => e.Id).CurrentValue ?? root.Id;
            });

    private Dictionary<string, object>[] CreateJoinCompositeKeyToRootShareds(ManyToManyContext context)
        =>
        [
            CreateJoinCompositeKeyToRootShared(context, _roots[5], _compositeKeys[0]),
            CreateJoinCompositeKeyToRootShared(context, _roots[8], _compositeKeys[0]),
            CreateJoinCompositeKeyToRootShared(context, _roots[19], _compositeKeys[0]),
            CreateJoinCompositeKeyToRootShared(context, _roots[0], _compositeKeys[1]),
            CreateJoinCompositeKeyToRootShared(context, _roots[1], _compositeKeys[1]),
            CreateJoinCompositeKeyToRootShared(context, _roots[3], _compositeKeys[1]),
            CreateJoinCompositeKeyToRootShared(context, _roots[5], _compositeKeys[1]),
            CreateJoinCompositeKeyToRootShared(context, _roots[10], _compositeKeys[1]),
            CreateJoinCompositeKeyToRootShared(context, _roots[17], _compositeKeys[1]),
            CreateJoinCompositeKeyToRootShared(context, _roots[3], _compositeKeys[2]),
            CreateJoinCompositeKeyToRootShared(context, _roots[13], _compositeKeys[2]),
            CreateJoinCompositeKeyToRootShared(context, _roots[15], _compositeKeys[2]),
            CreateJoinCompositeKeyToRootShared(context, _roots[1], _compositeKeys[3]),
            CreateJoinCompositeKeyToRootShared(context, _roots[2], _compositeKeys[3]),
            CreateJoinCompositeKeyToRootShared(context, _roots[3], _compositeKeys[3]),
            CreateJoinCompositeKeyToRootShared(context, _roots[1], _compositeKeys[7]),
            CreateJoinCompositeKeyToRootShared(context, _roots[7], _compositeKeys[7]),
            CreateJoinCompositeKeyToRootShared(context, _roots[15], _compositeKeys[7]),
            CreateJoinCompositeKeyToRootShared(context, _roots[17], _compositeKeys[7]),
            CreateJoinCompositeKeyToRootShared(context, _roots[6], _compositeKeys[8]),
            CreateJoinCompositeKeyToRootShared(context, _roots[7], _compositeKeys[8]),
            CreateJoinCompositeKeyToRootShared(context, _roots[18], _compositeKeys[8]),
            CreateJoinCompositeKeyToRootShared(context, _roots[2], _compositeKeys[9]),
            CreateJoinCompositeKeyToRootShared(context, _roots[11], _compositeKeys[9]),
            CreateJoinCompositeKeyToRootShared(context, _roots[17], _compositeKeys[9]),
            CreateJoinCompositeKeyToRootShared(context, _roots[1], _compositeKeys[10]),
            CreateJoinCompositeKeyToRootShared(context, _roots[3], _compositeKeys[10]),
            CreateJoinCompositeKeyToRootShared(context, _roots[4], _compositeKeys[10]),
            CreateJoinCompositeKeyToRootShared(context, _roots[6], _compositeKeys[11]),
            CreateJoinCompositeKeyToRootShared(context, _roots[2], _compositeKeys[12]),
            CreateJoinCompositeKeyToRootShared(context, _roots[7], _compositeKeys[12]),
            CreateJoinCompositeKeyToRootShared(context, _roots[13], _compositeKeys[12]),
            CreateJoinCompositeKeyToRootShared(context, _roots[3], _compositeKeys[14]),
            CreateJoinCompositeKeyToRootShared(context, _roots[10], _compositeKeys[14]),
            CreateJoinCompositeKeyToRootShared(context, _roots[0], _compositeKeys[15]),
            CreateJoinCompositeKeyToRootShared(context, _roots[6], _compositeKeys[15]),
            CreateJoinCompositeKeyToRootShared(context, _roots[14], _compositeKeys[15]),
            CreateJoinCompositeKeyToRootShared(context, _roots[0], _compositeKeys[18]),
            CreateJoinCompositeKeyToRootShared(context, _roots[5], _compositeKeys[19])
        ];

    private static Dictionary<string, object> CreateJoinCompositeKeyToRootShared(
        ManyToManyContext context,
        EntityRoot root,
        EntityCompositeKey composite)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("EntityCompositeKeyEntityRoot"), (e, p) =>
            {
                e["RootSkipSharedId"] = context?.Entry(root).Property(e => e.Id).CurrentValue ?? root.Id;
                e["CompositeKeySkipSharedKey1"] = context?.Entry(composite).Property(e => e.Key1).CurrentValue ?? composite.Key1;
                e["CompositeKeySkipSharedKey2"] = composite.Key2;
                e["CompositeKeySkipSharedKey3"] = composite.Key3;
            });

    private UnidirectionalEntityOne[] CreateUnidirectionalOnes(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 1, "EntityOne 1"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 2, "EntityOne 2"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 3, "EntityOne 3"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 4, "EntityOne 4"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 5, "EntityOne 5"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 6, "EntityOne 6"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 7, "EntityOne 7"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 8, "EntityOne 8"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 9, "EntityOne 9"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 10, "EntityOne 10"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 11, "EntityOne 11"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 12, "EntityOne 12"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 13, "EntityOne 13"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 14, "EntityOne 14"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 15, "EntityOne 15"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 16, "EntityOne 16"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 17, "EntityOne 17"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 18, "EntityOne 18"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 19, "EntityOne 19"),
            CreateUnidirectionalEntityOne(context, _useGeneratedKeys ? 0 : 20, "EntityOne 20")
        ];

    private static UnidirectionalEntityOne CreateUnidirectionalEntityOne(ManyToManyContext context, int id, string name)
        => CreateInstance(
            context?.UnidirectionalEntityOnes, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.Collection = CreateCollection<UnidirectionalEntityTwo>(p);
                e.TwoSkip = CreateCollection<UnidirectionalEntityTwo>(p);
                e.JoinThreePayloadFull = CreateCollection<UnidirectionalJoinOneToThreePayloadFull>(p);
                e.TwoSkipShared = CreateCollection<UnidirectionalEntityTwo>(p);
                e.ThreeSkipPayloadFullShared = CreateCollection<UnidirectionalEntityThree>(p);
                e.JoinThreePayloadFullShared = CreateCollection<Dictionary<string, object>>(p);
                e.SelfSkipPayloadLeft = CreateCollection<UnidirectionalEntityOne>(p);
                e.JoinSelfPayloadLeft = CreateCollection<UnidirectionalJoinOneSelfPayload>(p);
                e.JoinSelfPayloadRight = CreateCollection<UnidirectionalJoinOneSelfPayload>(p);
                e.BranchSkip = CreateCollection<UnidirectionalEntityBranch>(p);
            });

    private UnidirectionalEntityTwo[] CreateUnidirectionalTwos(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 1, "EntityTwo 1", null, _unidirectionalOnes[0]),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 2, "EntityTwo 2", null, _unidirectionalOnes[0]),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 3, "EntityTwo 3", null, null),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 4, "EntityTwo 4", null, _unidirectionalOnes[2]),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 5, "EntityTwo 5", null, _unidirectionalOnes[2]),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 6, "EntityTwo 6", null, _unidirectionalOnes[4]),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 7, "EntityTwo 7", null, _unidirectionalOnes[4]),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 8, "EntityTwo 8", null, _unidirectionalOnes[6]),
            CreateUnidirectionalEntityTwo(context, _useGeneratedKeys ? 0 : 9, "EntityTwo 9", null, _unidirectionalOnes[6]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 10, "EntityTwo 10", _unidirectionalOnes[19], _unidirectionalOnes[8]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 11, "EntityTwo 11", _unidirectionalOnes[17], _unidirectionalOnes[8]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 12, "EntityTwo 12", _unidirectionalOnes[15], _unidirectionalOnes[10]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 13, "EntityTwo 13", _unidirectionalOnes[13], _unidirectionalOnes[10]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 14, "EntityTwo 14", _unidirectionalOnes[11], _unidirectionalOnes[12]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 15, "EntityTwo 15", _unidirectionalOnes[10], _unidirectionalOnes[12]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 16, "EntityTwo 16", _unidirectionalOnes[8], _unidirectionalOnes[14]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 17, "EntityTwo 17", _unidirectionalOnes[6], _unidirectionalOnes[14]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 18, "EntityTwo 18", _unidirectionalOnes[4], _unidirectionalOnes[15]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 19, "EntityTwo 19", _unidirectionalOnes[2], _unidirectionalOnes[15]),
            CreateUnidirectionalEntityTwo(
                context, _useGeneratedKeys ? 0 : 20, "EntityTwo 20", _unidirectionalOnes[0], _unidirectionalOnes[16])
        ];

    private static UnidirectionalEntityTwo CreateUnidirectionalEntityTwo(
        ManyToManyContext context,
        int id,
        string name,
        UnidirectionalEntityOne referenceInverse,
        UnidirectionalEntityOne collectionInverse)
        => CreateInstance(
            context?.UnidirectionalEntityTwos, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.ReferenceInverse = referenceInverse;
                e.CollectionInverse = collectionInverse;
                e.Collection = CreateCollection<UnidirectionalEntityThree>(p);
                e.JoinThreeFull = CreateCollection<UnidirectionalJoinTwoToThree>(p);
                e.SelfSkipSharedRight = CreateCollection<UnidirectionalEntityTwo>(p);
            });

    private UnidirectionalEntityThree[] CreateUnidirectionalThrees(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 1, "EntityThree 1", null, null),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 2, "EntityThree 2", _unidirectionalTwos[18], _unidirectionalTwos[16]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 3, "EntityThree 3", _unidirectionalTwos[1], _unidirectionalTwos[15]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 4, "EntityThree 4", _unidirectionalTwos[19], _unidirectionalTwos[15]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 5, "EntityThree 5", _unidirectionalTwos[3], _unidirectionalTwos[14]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 6, "EntityThree 6", null, _unidirectionalTwos[14]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 7, "EntityThree 7", _unidirectionalTwos[5], _unidirectionalTwos[12]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 8, "EntityThree 8", null, _unidirectionalTwos[12]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 9, "EntityThree 9", _unidirectionalTwos[7], _unidirectionalTwos[10]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 10, "EntityThree 10", null, _unidirectionalTwos[10]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 11, "EntityThree 11", _unidirectionalTwos[18], _unidirectionalTwos[8]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 12, "EntityThree 12", null, _unidirectionalTwos[8]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 13, "EntityThree 13", _unidirectionalTwos[11], _unidirectionalTwos[6]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 14, "EntityThree 14", null, _unidirectionalTwos[6]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 15, "EntityThree 15", _unidirectionalTwos[13], _unidirectionalTwos[4]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 16, "EntityThree 16", null, _unidirectionalTwos[4]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 17, "EntityThree 17", _unidirectionalTwos[15], _unidirectionalTwos[2]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 18, "EntityThree 18", null, _unidirectionalTwos[2]),
            CreateUnidirectionalEntityThree(
                context, _useGeneratedKeys ? 0 : 19, "EntityThree 19", _unidirectionalTwos[17], _unidirectionalTwos[0]),
            CreateUnidirectionalEntityThree(context, _useGeneratedKeys ? 0 : 20, "EntityThree 20", null, _unidirectionalTwos[0])
        ];

    private static UnidirectionalEntityThree CreateUnidirectionalEntityThree(
        ManyToManyContext context,
        int id,
        string name,
        UnidirectionalEntityTwo referenceInverse,
        UnidirectionalEntityTwo collectionInverse)
        => CreateInstance(
            context?.UnidirectionalEntityThrees, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.ReferenceInverse = referenceInverse;
                e.CollectionInverse = collectionInverse;
                e.JoinOnePayloadFull = CreateCollection<UnidirectionalJoinOneToThreePayloadFull>(p);
                e.TwoSkipFull = CreateCollection<UnidirectionalEntityTwo>(p);
                e.JoinTwoFull = CreateCollection<UnidirectionalJoinTwoToThree>(p);
                e.JoinOnePayloadFullShared = CreateCollection<Dictionary<string, object>>(p);
                e.JoinCompositeKeyFull = CreateCollection<UnidirectionalJoinThreeToCompositeKeyFull>(p);
            });

    private UnidirectionalEntityCompositeKey[] CreateUnidirectionalCompositeKeys(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 1, "1_1", new DateTime(2001, 1, 1), "Composite 1"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 1, "1_2", new DateTime(2001, 2, 1), "Composite 2"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 3, "3_1", new DateTime(2003, 1, 1), "Composite 3"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 3, "3_2", new DateTime(2003, 2, 1), "Composite 4"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 3, "3_3", new DateTime(2003, 3, 1), "Composite 5"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 6, "6_1", new DateTime(2006, 1, 1), "Composite 6"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 7, "7_1", new DateTime(2007, 1, 1), "Composite 7"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 7, "7_2", new DateTime(2007, 2, 1), "Composite 8"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_1", new DateTime(2008, 1, 1), "Composite 9"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_2", new DateTime(2008, 2, 1), "Composite 10"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_3", new DateTime(2008, 3, 1), "Composite 11"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_4", new DateTime(2008, 4, 1), "Composite 12"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 8, "8_5", new DateTime(2008, 5, 1), "Composite 13"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_1", new DateTime(2009, 1, 1), "Composite 14"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_2", new DateTime(2009, 2, 1), "Composite 15"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_3", new DateTime(2009, 3, 1), "Composite 16"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_4", new DateTime(2009, 4, 1), "Composite 17"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_5", new DateTime(2009, 5, 1), "Composite 18"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_6", new DateTime(2009, 6, 1), "Composite 19"),
            CreateUnidirectionalEntityCompositeKey(context, _useGeneratedKeys ? 0 : 9, "9_7", new DateTime(2009, 7, 1), "Composite 20")
        ];

    private static UnidirectionalEntityCompositeKey CreateUnidirectionalEntityCompositeKey(
        ManyToManyContext context,
        int key1,
        string key2,
        DateTime key3,
        string name)
        => CreateInstance(
            context?.UnidirectionalEntityCompositeKeys, (e, p) =>
            {
                e.Key1 = key1;
                e.Key2 = key2;
                e.Key3 = key3;
                e.Name = name;
                e.TwoSkipShared = CreateCollection<UnidirectionalEntityTwo>(p);
                e.ThreeSkipFull = CreateCollection<UnidirectionalEntityThree>(p);
                e.RootSkipShared = CreateCollection<UnidirectionalEntityRoot>(p);
                e.JoinLeafFull = CreateCollection<UnidirectionalJoinCompositeKeyToLeaf>(p);
                e.JoinThreeFull = CreateCollection<UnidirectionalJoinThreeToCompositeKeyFull>(p);
            });

    private UnidirectionalEntityRoot[] CreateUnidirectionalRoots(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 1, "Root 1"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 2, "Root 2"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 3, "Root 3"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 4, "Root 4"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 5, "Root 5"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 6, "Root 6"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 7, "Root 7"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 8, "Root 8"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 9, "Root 9"),
            CreateUnidirectionalEntityRoot(context, _useGeneratedKeys ? 0 : 10, "Root 10"),
            CreateUnidirectionalEntityBranch(context, _useGeneratedKeys ? 0 : 11, "Branch 1", 7),
            CreateUnidirectionalEntityBranch(context, _useGeneratedKeys ? 0 : 12, "Branch 2", 77),
            CreateUnidirectionalEntityBranch(context, _useGeneratedKeys ? 0 : 13, "Branch 3", 777),
            CreateUnidirectionalEntityBranch(context, _useGeneratedKeys ? 0 : 14, "Branch 4", 7777),
            CreateUnidirectionalEntityBranch(context, _useGeneratedKeys ? 0 : 15, "Branch 5", 77777),
            CreateUnidirectionalEntityBranch(context, _useGeneratedKeys ? 0 : 16, "Branch 6", 777777),
            CreateUnidirectionalEntityLeaf(context, _useGeneratedKeys ? 0 : 21, "Leaf 1", 42, true),
            CreateUnidirectionalEntityLeaf(context, _useGeneratedKeys ? 0 : 22, "Leaf 2", 421, true),
            CreateUnidirectionalEntityLeaf(context, _useGeneratedKeys ? 0 : 23, "Leaf 3", 1337, false),
            CreateUnidirectionalEntityLeaf(context, _useGeneratedKeys ? 0 : 24, "Leaf 4", 1729, false)
        ];

    private static UnidirectionalEntityRoot CreateUnidirectionalEntityRoot(
        ManyToManyContext context,
        int id,
        string name)
        => CreateInstance(
            context?.UnidirectionalEntityRoots, (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.ThreeSkipShared = CreateCollection<UnidirectionalEntityThree>(p);
            });

    private static UnidirectionalEntityBranch CreateUnidirectionalEntityBranch(
        ManyToManyContext context,
        int id,
        string name,
        long number)
        => CreateInstance(
            context?.Set<UnidirectionalEntityBranch>(), (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.Number = number;
                e.ThreeSkipShared = CreateCollection<UnidirectionalEntityThree>(p);
            });

    private static UnidirectionalEntityLeaf CreateUnidirectionalEntityLeaf(
        ManyToManyContext context,
        int id,
        string name,
        long number,
        bool? isGreen)
        => CreateInstance(
            context?.Set<UnidirectionalEntityLeaf>(), (e, p) =>
            {
                e.Id = id;
                e.Name = name;
                e.Number = number;
                e.IsGreen = isGreen;
                e.ThreeSkipShared = CreateCollection<UnidirectionalEntityThree>(p);
                e.CompositeKeySkipFull = CreateCollection<UnidirectionalEntityCompositeKey>(p);
                e.JoinCompositeKeyFull = CreateCollection<UnidirectionalJoinCompositeKeyToLeaf>(p);
            });

    private UnidirectionalJoinCompositeKeyToLeaf[] CreateUnidirectionalJoinCompositeKeyToLeaves(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[17], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[19], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[17], _unidirectionalCompositeKeys[4]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[5]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[17], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[19], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[8]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[9]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[17], _unidirectionalCompositeKeys[10]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[10]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[19], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[13]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[17], _unidirectionalCompositeKeys[13]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[13]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[17], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[19], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[16]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[19], _unidirectionalCompositeKeys[16]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[18], _unidirectionalCompositeKeys[17]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[19], _unidirectionalCompositeKeys[17]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[16], _unidirectionalCompositeKeys[18]),
            CreateUnidirectionalJoinCompositeKeyToLeaf(
                context, (UnidirectionalEntityLeaf)_unidirectionalRoots[17], _unidirectionalCompositeKeys[18])
        ];

    private static UnidirectionalJoinCompositeKeyToLeaf CreateUnidirectionalJoinCompositeKeyToLeaf(
        ManyToManyContext context,
        UnidirectionalEntityLeaf leaf,
        UnidirectionalEntityCompositeKey composite)
        => CreateInstance(
            context?.Set<UnidirectionalJoinCompositeKeyToLeaf>(), (e, p) =>
            {
                e.Leaf = leaf;
                e.Composite = composite;
            });

    private UnidirectionalJoinOneSelfPayload[] CreateUnidirectionalJoinOneSelfPayloads(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[2], _unidirectionalOnes[3], DateTime.Parse("2020-01-11 19:26:36")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[2], _unidirectionalOnes[5], DateTime.Parse("2005-10-03 12:57:54")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[2], _unidirectionalOnes[7], DateTime.Parse("2015-12-20 01:09:24")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[2], _unidirectionalOnes[17], DateTime.Parse("1999-12-26 02:51:57")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[2], _unidirectionalOnes[19], DateTime.Parse("2011-06-15 19:08:00")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[4], _unidirectionalOnes[2], DateTime.Parse("2019-12-08 05:40:16")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[4], _unidirectionalOnes[3], DateTime.Parse("2014-03-09 12:58:26")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[5], _unidirectionalOnes[4], DateTime.Parse("2014-05-15 16:34:38")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[5], _unidirectionalOnes[6], DateTime.Parse("2014-03-08 18:59:49")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[5], _unidirectionalOnes[12], DateTime.Parse("2013-12-10 07:01:53")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[6], _unidirectionalOnes[12], DateTime.Parse("2005-05-31 02:21:16")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[7], _unidirectionalOnes[8], DateTime.Parse("2011-12-31 19:37:25")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[7], _unidirectionalOnes[10], DateTime.Parse("2012-08-02 16:33:07")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[7], _unidirectionalOnes[11], DateTime.Parse("2018-07-19 09:10:12")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[9], _unidirectionalOnes[6], DateTime.Parse("2018-12-28 01:21:23")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[12], _unidirectionalOnes[1], DateTime.Parse("2014-03-22 02:20:06")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[12], _unidirectionalOnes[17], DateTime.Parse("2005-03-21 14:45:37")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[13], _unidirectionalOnes[8], DateTime.Parse("2016-06-26 08:03:32")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[14], _unidirectionalOnes[12], DateTime.Parse("2018-09-18 12:51:22")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[15], _unidirectionalOnes[4], DateTime.Parse("2016-12-17 14:20:25")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[15], _unidirectionalOnes[5], DateTime.Parse("2008-07-30 03:43:17")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[16], _unidirectionalOnes[13], DateTime.Parse("2019-08-01 16:26:31")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[18], _unidirectionalOnes[0], DateTime.Parse("2010-02-19 13:24:07")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[18], _unidirectionalOnes[7], DateTime.Parse("2004-07-28 09:06:02")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[18], _unidirectionalOnes[11], DateTime.Parse("2004-08-21 11:07:20")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[19], _unidirectionalOnes[0], DateTime.Parse("2014-11-21 18:13:02")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[19], _unidirectionalOnes[6], DateTime.Parse("2009-08-24 21:44:46")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[19], _unidirectionalOnes[13], DateTime.Parse("2013-02-18 02:19:19")),
            CreateUnidirectionalJoinOneSelfPayload(
                context, _unidirectionalOnes[19], _unidirectionalOnes[15], DateTime.Parse("2016-02-05 14:18:12"))
        ];

    private static UnidirectionalJoinOneSelfPayload CreateUnidirectionalJoinOneSelfPayload(
        ManyToManyContext context,
        UnidirectionalEntityOne left,
        UnidirectionalEntityOne right,
        DateTime payload)
        => CreateInstance(
            context?.Set<UnidirectionalJoinOneSelfPayload>(), (e, p) =>
            {
                e.Left = left;
                e.Right = right;
                e.Payload = payload;
            });

    private UnidirectionalJoinOneToBranch[] CreateUnidirectionalJoinOneToBranches(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[1], _unidirectionalRoots[15]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[1], _unidirectionalRoots[19]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[2], _unidirectionalRoots[13]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[2], _unidirectionalRoots[15]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[2], _unidirectionalRoots[17]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[2], _unidirectionalRoots[19]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[4], _unidirectionalRoots[12]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[5], _unidirectionalRoots[15]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[5], _unidirectionalRoots[17]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[5], _unidirectionalRoots[18]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[7], _unidirectionalRoots[10]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[7], _unidirectionalRoots[11]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[7], _unidirectionalRoots[12]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[8], _unidirectionalRoots[10]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[8], _unidirectionalRoots[11]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[8], _unidirectionalRoots[13]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[8], _unidirectionalRoots[15]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[8], _unidirectionalRoots[16]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[8], _unidirectionalRoots[19]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[9], _unidirectionalRoots[11]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[9], _unidirectionalRoots[12]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[9], _unidirectionalRoots[13]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[9], _unidirectionalRoots[16]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[11], _unidirectionalRoots[10]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[11], _unidirectionalRoots[11]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[11], _unidirectionalRoots[13]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[11], _unidirectionalRoots[18]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[12], _unidirectionalRoots[14]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[13], _unidirectionalRoots[11]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[13], _unidirectionalRoots[13]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[13], _unidirectionalRoots[15]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[13], _unidirectionalRoots[18]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[14], _unidirectionalRoots[14]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[14], _unidirectionalRoots[15]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[14], _unidirectionalRoots[19]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[15], _unidirectionalRoots[10]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[16], _unidirectionalRoots[10]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[16], _unidirectionalRoots[16]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[17], _unidirectionalRoots[11]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[17], _unidirectionalRoots[14]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[17], _unidirectionalRoots[19]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[18], _unidirectionalRoots[10]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[18], _unidirectionalRoots[11]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[18], _unidirectionalRoots[15]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[18], _unidirectionalRoots[18]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[19], _unidirectionalRoots[16]),
            CreateUnidirectionalJoinOneToBranch(context, _unidirectionalOnes[19], _unidirectionalRoots[18])
        ];

    private static UnidirectionalJoinOneToBranch CreateUnidirectionalJoinOneToBranch(
        ManyToManyContext context,
        UnidirectionalEntityOne one,
        UnidirectionalEntityRoot branch)
        => CreateInstance(
            context?.Set<UnidirectionalJoinOneToBranch>(), (e, p) =>
            {
                e.UnidirectionalEntityOneId = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e.UnidirectionalEntityBranchId = context?.Entry(branch).Property(e => e.Id).CurrentValue ?? branch.Id;
            });

    private UnidirectionalJoinOneToThreePayloadFull[] CreateUnidirectionalJoinOneToThreePayloadFulls(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[0], _unidirectionalThrees[1], "Ira Watts"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[0], _unidirectionalThrees[5], "Harold May"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[0], _unidirectionalThrees[8], "Freda Vaughn"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[0], _unidirectionalThrees[12], "Pedro Mccarthy"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[0], _unidirectionalThrees[16], "Elaine Simon"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[1], _unidirectionalThrees[8], "Melvin Maldonado"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[1], _unidirectionalThrees[10], "Lora George"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[1], _unidirectionalThrees[12], "Joey Cohen"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[1], _unidirectionalThrees[13], "Erik Carroll"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[1], _unidirectionalThrees[15], "April Rodriguez"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[2], _unidirectionalThrees[4], "Gerardo Colon"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[2], _unidirectionalThrees[11], "Alexander Willis"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[2], _unidirectionalThrees[15], "Laura Wheeler"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[2], _unidirectionalThrees[18], "Lester Summers"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[3], _unidirectionalThrees[1], "Raquel Curry"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[3], _unidirectionalThrees[3], "Steven Fisher"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[3], _unidirectionalThrees[10], "Casey Williams"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[3], _unidirectionalThrees[12], "Lauren Clayton"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[3], _unidirectionalThrees[18], "Maureen Weber"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[4], _unidirectionalThrees[3], "Joyce Ford"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[4], _unidirectionalThrees[5], "Willie Mccormick"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[4], _unidirectionalThrees[8], "Geraldine Jackson"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[6], _unidirectionalThrees[0], "Victor Aguilar"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[6], _unidirectionalThrees[3], "Cathy Allen"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[6], _unidirectionalThrees[8], "Edwin Burke"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[6], _unidirectionalThrees[9], "Eugene Flores"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[6], _unidirectionalThrees[10], "Ginger Patton"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[6], _unidirectionalThrees[11], "Israel Mitchell"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[6], _unidirectionalThrees[17], "Joy Francis"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[7], _unidirectionalThrees[0], "Orville Parker"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[7], _unidirectionalThrees[2], "Alyssa Mann"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[7], _unidirectionalThrees[3], "Hugh Daniel"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[7], _unidirectionalThrees[12], "Kim Craig"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[7], _unidirectionalThrees[13], "Lucille Moreno"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[7], _unidirectionalThrees[16], "Virgil Drake"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[7], _unidirectionalThrees[17], "Josephine Dawson"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[7], _unidirectionalThrees[19], "Milton Huff"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[8], _unidirectionalThrees[1], "Jody Clarke"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[8], _unidirectionalThrees[8], "Elisa Cooper"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[8], _unidirectionalThrees[10], "Grace Owen"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[8], _unidirectionalThrees[11], "Donald Welch"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[8], _unidirectionalThrees[14], "Marian Day"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[8], _unidirectionalThrees[16], "Cory Cortez"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[1], "Chad Rowe"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[2], "Simon Reyes"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[3], "Shari Jensen"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[7], "Ricky Bradley"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[9], "Debra Gibbs"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[9], _unidirectionalThrees[10], "Everett Mckenzie"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[13], "Kirk Graham"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[9], _unidirectionalThrees[15], "Paulette Adkins"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[17], "Raul Holloway"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[9], _unidirectionalThrees[18], "Danielle Ross"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[10], _unidirectionalThrees[0], "Frank Garner"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[10], _unidirectionalThrees[5], "Stella Thompson"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[10], _unidirectionalThrees[7], "Peggy Wagner"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[10], _unidirectionalThrees[8], "Geneva Holmes"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[10], _unidirectionalThrees[9], "Ignacio Black"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[10], _unidirectionalThrees[12], "Phillip Wells"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[10], _unidirectionalThrees[13], "Hubert Lambert"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[10], _unidirectionalThrees[18], "Courtney Gregory"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[11], _unidirectionalThrees[1], "Esther Carter"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[12], _unidirectionalThrees[5], "Thomas Benson"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[12], _unidirectionalThrees[8], "Kara Baldwin"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[12], _unidirectionalThrees[9], "Yvonne Sparks"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[12], _unidirectionalThrees[10], "Darin Mathis"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[12], _unidirectionalThrees[11], "Glenda Castillo"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[12], _unidirectionalThrees[12], "Larry Walters"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[12], _unidirectionalThrees[14], "Meredith Yates"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[12], _unidirectionalThrees[15], "Rosemarie Henry"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[12], _unidirectionalThrees[17], "Nora Leonard"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[13], _unidirectionalThrees[16], "Corey Delgado"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[13], _unidirectionalThrees[17], "Kari Strickland"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[14], _unidirectionalThrees[7], "Joann Stanley"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[14], _unidirectionalThrees[10], "Camille Gordon"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[14], _unidirectionalThrees[13], "Flora Anderson"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[14], _unidirectionalThrees[14], "Wilbur Soto"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[14], _unidirectionalThrees[17], "Shirley Andrews"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[14], _unidirectionalThrees[19], "Marcus Mcguire"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[15], _unidirectionalThrees[0], "Saul Dixon"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[15], _unidirectionalThrees[5], "Cynthia Hart"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[15], _unidirectionalThrees[9], "Elbert Spencer"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[15], _unidirectionalThrees[12], "Darrell Norris"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[15], _unidirectionalThrees[13], "Jamie Kelley"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[15], _unidirectionalThrees[14], "Francis Briggs"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[15], _unidirectionalThrees[15], "Lindsey Morris"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[16], _unidirectionalThrees[1], "James Castro"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[16], _unidirectionalThrees[4], "Carlos Chavez"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[16], _unidirectionalThrees[6], "Janis Valdez"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[16], _unidirectionalThrees[12], "Alfredo Bowen"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[16], _unidirectionalThrees[13], "Viola Torres"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[16], _unidirectionalThrees[14], "Dianna Lowe"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[17], _unidirectionalThrees[2], "Craig Howell"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[17], _unidirectionalThrees[6], "Sandy Curtis"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[17], _unidirectionalThrees[11], "Alonzo Pierce"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[17], _unidirectionalThrees[17], "Albert Harper"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[1], "Frankie Baker"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[4], "Candace Tucker"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[18], _unidirectionalThrees[5], "Willis Christensen"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[6], "Juan Joseph"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[9], "Thelma Sanders"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[10], "Kerry West"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[14], "Sheri Castro"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[15], "Mark Schultz"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[18], _unidirectionalThrees[16], "Priscilla Summers"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[18], _unidirectionalThrees[19], "Allan Valdez"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[19], _unidirectionalThrees[2], "Bill Peters"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[19], _unidirectionalThrees[4], "Cora Stone"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[19], _unidirectionalThrees[5], "Frankie Pope"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[19], _unidirectionalThrees[9], "Christian Young"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[19], _unidirectionalThrees[10], "Shari Brewer"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[19], _unidirectionalThrees[11], "Antonia Wolfe"),
            CreateUnidirectionalJoinOneToThreePayloadFull(
                context, _unidirectionalOnes[19], _unidirectionalThrees[13], "Lawrence Matthews"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[19], _unidirectionalThrees[17], "Van Hubbard"),
            CreateUnidirectionalJoinOneToThreePayloadFull(context, _unidirectionalOnes[19], _unidirectionalThrees[19], "Lindsay Pena")
        ];

    private static UnidirectionalJoinOneToThreePayloadFull CreateUnidirectionalJoinOneToThreePayloadFull(
        ManyToManyContext context,
        UnidirectionalEntityOne one,
        UnidirectionalEntityThree three,
        string payload)
        => CreateInstance(
            context?.Set<UnidirectionalJoinOneToThreePayloadFull>(), (e, p) =>
            {
                e.One = one;
                e.Three = three;
                e.Payload = payload;
            });

    private UnidirectionalJoinOneToTwo[] CreateUnidirectionalJoinOneToTwos(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[2]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[3]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[5]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[6]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[8]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[9]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[11]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[14]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[15]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[16]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[17]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[2]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[6]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[8]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[14]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[16]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[3]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[6]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[9]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[15]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[3], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[3], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[3], _unidirectionalTwos[8]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[3], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[3], _unidirectionalTwos[16]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[5]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[15]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[5], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[5], _unidirectionalTwos[6]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[5], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[5], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[6], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[6], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[6], _unidirectionalTwos[14]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[7], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[7], _unidirectionalTwos[8]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[7], _unidirectionalTwos[16]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[8], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[8], _unidirectionalTwos[9]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[8], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[9], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[9], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[17]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[16]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[15]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[14]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[11]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[9]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[8]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[6]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[5]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[3]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[2]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[0]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[16]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[15]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[11]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[3]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[13], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[13], _unidirectionalTwos[14]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[13], _unidirectionalTwos[9]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[13], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[14], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[14], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[14], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[14], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[15], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[15], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[15], _unidirectionalTwos[5]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[16], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[16], _unidirectionalTwos[11]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[16], _unidirectionalTwos[3]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[17], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[17], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[17], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[18], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinOneToTwo(context, _unidirectionalOnes[18], _unidirectionalTwos[9])
        ];

    private static UnidirectionalJoinOneToTwo CreateUnidirectionalJoinOneToTwo(
        ManyToManyContext context,
        UnidirectionalEntityOne one,
        UnidirectionalEntityTwo two)
        => CreateInstance(
            context?.Set<UnidirectionalJoinOneToTwo>(), (e, p) =>
            {
                e.OneId = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e.TwoId = context?.Entry(two).Property(e => e.Id).CurrentValue ?? two.Id;
            });

    private UnidirectionalJoinThreeToCompositeKeyFull[] CreateUnidirectionalJoinThreeToCompositeKeyFulls(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[0], _unidirectionalCompositeKeys[5]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[1], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[1], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[1], _unidirectionalCompositeKeys[19]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[2], _unidirectionalCompositeKeys[5]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[2], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[2], _unidirectionalCompositeKeys[19]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[4], _unidirectionalCompositeKeys[11]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[4], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[4], _unidirectionalCompositeKeys[17]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[5], _unidirectionalCompositeKeys[5]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[6], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[6], _unidirectionalCompositeKeys[8]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[7], _unidirectionalCompositeKeys[10]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[7], _unidirectionalCompositeKeys[18]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[8], _unidirectionalCompositeKeys[8]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[8], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[9], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[10], _unidirectionalCompositeKeys[6]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[10], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[11], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[11], _unidirectionalCompositeKeys[10]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[11], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[12], _unidirectionalCompositeKeys[5]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[12], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[12], _unidirectionalCompositeKeys[13]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[12], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[13], _unidirectionalCompositeKeys[9]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[13], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[13], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[14], _unidirectionalCompositeKeys[9]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[14], _unidirectionalCompositeKeys[13]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[14], _unidirectionalCompositeKeys[18]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[15], _unidirectionalCompositeKeys[4]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[15], _unidirectionalCompositeKeys[6]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[15], _unidirectionalCompositeKeys[18]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[16], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[16], _unidirectionalCompositeKeys[9]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[17], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[18], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[18], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[18], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[18], _unidirectionalCompositeKeys[19]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[19], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinThreeToCompositeKeyFull(context, _unidirectionalThrees[19], _unidirectionalCompositeKeys[6])
        ];

    private static UnidirectionalJoinThreeToCompositeKeyFull CreateUnidirectionalJoinThreeToCompositeKeyFull(
        ManyToManyContext context,
        UnidirectionalEntityThree three,
        UnidirectionalEntityCompositeKey composite)
        => CreateInstance(
            context?.Set<UnidirectionalJoinThreeToCompositeKeyFull>(), (e, p) =>
            {
                e.Three = three;
                e.Composite = composite;
            });

    private UnidirectionalJoinTwoToThree[] CreateUnidirectionalJoinTwoToThrees(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[0], _unidirectionalThrees[1]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[0], _unidirectionalThrees[2]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[0], _unidirectionalThrees[12]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[0], _unidirectionalThrees[17]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[1], _unidirectionalThrees[0]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[1], _unidirectionalThrees[8]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[1], _unidirectionalThrees[14]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[2], _unidirectionalThrees[10]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[2], _unidirectionalThrees[16]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[3], _unidirectionalThrees[1]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[3], _unidirectionalThrees[4]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[3], _unidirectionalThrees[10]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[4], _unidirectionalThrees[3]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[4], _unidirectionalThrees[4]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[5], _unidirectionalThrees[2]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[5], _unidirectionalThrees[9]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[5], _unidirectionalThrees[15]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[5], _unidirectionalThrees[17]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[6], _unidirectionalThrees[11]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[6], _unidirectionalThrees[14]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[6], _unidirectionalThrees[19]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[7], _unidirectionalThrees[0]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[7], _unidirectionalThrees[2]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[7], _unidirectionalThrees[19]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[8], _unidirectionalThrees[2]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[8], _unidirectionalThrees[12]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[8], _unidirectionalThrees[18]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[9], _unidirectionalThrees[16]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[10], _unidirectionalThrees[5]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[10], _unidirectionalThrees[6]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[10], _unidirectionalThrees[7]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[10], _unidirectionalThrees[12]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[11], _unidirectionalThrees[8]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[12], _unidirectionalThrees[0]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[12], _unidirectionalThrees[10]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[12], _unidirectionalThrees[18]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[13], _unidirectionalThrees[1]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[14], _unidirectionalThrees[16]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[15], _unidirectionalThrees[2]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[15], _unidirectionalThrees[15]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[17], _unidirectionalThrees[0]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[17], _unidirectionalThrees[4]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[17], _unidirectionalThrees[9]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[18], _unidirectionalThrees[4]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[18], _unidirectionalThrees[15]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[18], _unidirectionalThrees[17]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[19], _unidirectionalThrees[5]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[19], _unidirectionalThrees[9]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[19], _unidirectionalThrees[11]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[19], _unidirectionalThrees[15]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[19], _unidirectionalThrees[16]),
            CreateUnidirectionalJoinTwoToThree(context, _unidirectionalTwos[19], _unidirectionalThrees[17])
        ];

    private static UnidirectionalJoinTwoToThree CreateUnidirectionalJoinTwoToThree(
        ManyToManyContext context,
        UnidirectionalEntityTwo two,
        UnidirectionalEntityThree three)
        => CreateInstance(
            context?.Set<UnidirectionalJoinTwoToThree>(), (e, p) =>
            {
                e.Two = two;
                e.Three = three;
            });

    private Dictionary<string, object>[] CreateUnidirectionalEntityOneEntityTwos(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[2]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[0], _unidirectionalTwos[15]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[2]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[9]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[1], _unidirectionalTwos[17]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[9]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[10]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[2], _unidirectionalTwos[15]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[1]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[4]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[6]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[8]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[4], _unidirectionalTwos[13]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[5], _unidirectionalTwos[11]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[6], _unidirectionalTwos[2]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[6], _unidirectionalTwos[15]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[6], _unidirectionalTwos[16]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[7], _unidirectionalTwos[18]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[8], _unidirectionalTwos[8]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[8], _unidirectionalTwos[10]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[9], _unidirectionalTwos[5]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[9], _unidirectionalTwos[16]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[9], _unidirectionalTwos[19]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[16]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[10], _unidirectionalTwos[17]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[5]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[11], _unidirectionalTwos[18]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[6]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[7]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[8]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[12], _unidirectionalTwos[12]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[13], _unidirectionalTwos[3]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[13], _unidirectionalTwos[8]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[13], _unidirectionalTwos[18]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[14], _unidirectionalTwos[9]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[15], _unidirectionalTwos[0]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[15], _unidirectionalTwos[6]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[15], _unidirectionalTwos[18]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[16], _unidirectionalTwos[7]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[16], _unidirectionalTwos[14]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[17], _unidirectionalTwos[3]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[17], _unidirectionalTwos[12]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[17], _unidirectionalTwos[13]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[18], _unidirectionalTwos[3]),
            CreateUnidirectionalEntityOneEntityTwo(context, _unidirectionalOnes[18], _unidirectionalTwos[13])
        ];

    private static Dictionary<string, object> CreateUnidirectionalEntityOneEntityTwo(
        ManyToManyContext context,
        UnidirectionalEntityOne one,
        UnidirectionalEntityTwo two)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("UnidirectionalEntityOneUnidirectionalEntityTwo"), (e, p) =>
            {
                e["UnidirectionalEntityOneId"] = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e["TwoSkipSharedId"] = context?.Entry(two).Property(e => e.Id).CurrentValue ?? two.Id;
            });

    private Dictionary<string, object>[] CreateUnidirectionalJoinOneToThreePayloadFullShareds(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[2], _unidirectionalThrees[0], "Capbrough"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[2], _unidirectionalThrees[1], "East Eastdol"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[2], _unidirectionalThrees[3], "Southingville"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[2], _unidirectionalThrees[8], "Goldbrough"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[3], _unidirectionalThrees[4], "Readingworth"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[3], _unidirectionalThrees[17], "Skillpool"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[4], _unidirectionalThrees[0], "Lawgrad"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[4], _unidirectionalThrees[3], "Kettleham Park"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[4], _unidirectionalThrees[8], "Sayford Park"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[4], _unidirectionalThrees[15], "Hamstead"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[5], _unidirectionalThrees[10], "North Starside"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[5], _unidirectionalThrees[12], "Goldfolk"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[6], _unidirectionalThrees[3], "Winstead"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[7], _unidirectionalThrees[10], "Transworth"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[7], _unidirectionalThrees[17], "Parkpool"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[7], _unidirectionalThrees[18], "Fishham"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[9], _unidirectionalThrees[0], "Passmouth"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[9], _unidirectionalThrees[4], "Valenfield"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[9], _unidirectionalThrees[19], "Passford Park"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[10], _unidirectionalThrees[9], "Chatfield"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[11], _unidirectionalThrees[10], "Hosview"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[11], _unidirectionalThrees[16], "Dodgewich"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[12], _unidirectionalThrees[2], "Skillhampton"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[12], _unidirectionalThrees[13], "Hardcaster"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[12], _unidirectionalThrees[15], "Hollowmouth"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[13], _unidirectionalThrees[5], "Cruxcaster"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[13], _unidirectionalThrees[10], "Elcaster"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[13], _unidirectionalThrees[16], "Clambrough"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[14], _unidirectionalThrees[9], "Millwich"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[14], _unidirectionalThrees[12], "Hapcester"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[15], _unidirectionalThrees[6], "Sanddol Beach"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[15], _unidirectionalThrees[12], "Hamcaster"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[16], _unidirectionalThrees[8], "New Foxbrough"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[16], _unidirectionalThrees[12], "Chatpool"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[17], _unidirectionalThrees[7], "Duckworth"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(context, _unidirectionalOnes[17], _unidirectionalThrees[11], "Snowham"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[17], _unidirectionalThrees[12], "Bannview Island"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[19], _unidirectionalThrees[3], "Rockbrough"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[19], _unidirectionalThrees[4], "Sweetfield"),
            CreateUnidirectionalJoinOneToThreePayloadFullShared(
                context, _unidirectionalOnes[19], _unidirectionalThrees[15], "Bayburgh Hills")
        ];

    private static Dictionary<string, object> CreateUnidirectionalJoinOneToThreePayloadFullShared(
        ManyToManyContext context,
        UnidirectionalEntityOne one,
        UnidirectionalEntityThree three,
        string payload)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared"), (e, p) =>
            {
                e["OneId"] = context?.Entry(one).Property(e => e.Id).CurrentValue ?? one.Id;
                e["ThreeId"] = context?.Entry(three).Property(e => e.Id).CurrentValue ?? three.Id;
                e["Payload"] = payload;
            });

    private Dictionary<string, object>[] CreateUnidirectionalJoinTwoSelfShareds(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[0], _unidirectionalTwos[8]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[0], _unidirectionalTwos[9]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[0], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[0], _unidirectionalTwos[17]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[2], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[2], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[2], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[2], _unidirectionalTwos[17]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[2], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[3], _unidirectionalTwos[10]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[4], _unidirectionalTwos[7]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[5], _unidirectionalTwos[17]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[7], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[7], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[7], _unidirectionalTwos[14]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[7], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[8], _unidirectionalTwos[3]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[8], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[9], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[11], _unidirectionalTwos[12]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[11], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[12], _unidirectionalTwos[13]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[12], _unidirectionalTwos[17]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[12], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[15], _unidirectionalTwos[5]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[16], _unidirectionalTwos[8]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[16], _unidirectionalTwos[18]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[16], _unidirectionalTwos[19]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[17], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[17], _unidirectionalTwos[4]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[17], _unidirectionalTwos[15]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[17], _unidirectionalTwos[16]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[18], _unidirectionalTwos[1]),
            CreateUnidirectionalJoinTwoSelfShared(context, _unidirectionalTwos[19], _unidirectionalTwos[3])
        ];

    private static Dictionary<string, object> CreateUnidirectionalJoinTwoSelfShared(
        ManyToManyContext context,
        UnidirectionalEntityTwo left,
        UnidirectionalEntityTwo right)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("UnidirectionalEntityTwoUnidirectionalEntityTwo"), (e, p) =>
            {
                e["UnidirectionalEntityTwoId"] = context?.Entry(left).Property(e => e.Id).CurrentValue ?? left.Id;
                e["SelfSkipSharedRightId"] = context?.Entry(right).Property(e => e.Id).CurrentValue ?? right.Id;
            });

    private Dictionary<string, object>[] CreateUnidirectionalJoinTwoToCompositeKeyShareds(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[0], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[0], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[0], _unidirectionalCompositeKeys[4]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[1], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[2], _unidirectionalCompositeKeys[5]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[3], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[3], _unidirectionalCompositeKeys[18]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[5], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[5], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[6], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[8], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[8], _unidirectionalCompositeKeys[8]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[9], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[9], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[9], _unidirectionalCompositeKeys[17]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[10], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[10], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[11], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[11], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[11], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[12], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[12], _unidirectionalCompositeKeys[6]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[12], _unidirectionalCompositeKeys[16]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[14], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[15], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[15], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[15], _unidirectionalCompositeKeys[18]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[16], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[16], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[16], _unidirectionalCompositeKeys[13]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[16], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[18], _unidirectionalCompositeKeys[4]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[19], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[19], _unidirectionalCompositeKeys[4]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[19], _unidirectionalCompositeKeys[5]),
            CreateUnidirectionalJoinTwoToCompositeKeyShared(context, _unidirectionalTwos[19], _unidirectionalCompositeKeys[13])
        ];

    private static Dictionary<string, object> CreateUnidirectionalJoinTwoToCompositeKeyShared(
        ManyToManyContext context,
        UnidirectionalEntityTwo two,
        UnidirectionalEntityCompositeKey composite)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo"), (e, p) =>
            {
                e["TwoSkipSharedId"] = context?.Entry(two).Property(e => e.Id).CurrentValue ?? two.Id;
                e["UnidirectionalEntityCompositeKeyKey1"] = context?.Entry(composite).Property(e => e.Key1).CurrentValue ?? composite.Key1;
                e["UnidirectionalEntityCompositeKeyKey2"] = composite.Key2;
                e["UnidirectionalEntityCompositeKeyKey3"] = composite.Key3;
            });

    private Dictionary<string, object>[] CreateUnidirectionalEntityRootEntityThrees(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[0], _unidirectionalRoots[6]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[0], _unidirectionalRoots[7]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[0], _unidirectionalRoots[14]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[1], _unidirectionalRoots[3]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[1], _unidirectionalRoots[15]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[2], _unidirectionalRoots[11]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[2], _unidirectionalRoots[13]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[2], _unidirectionalRoots[19]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[4], _unidirectionalRoots[13]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[4], _unidirectionalRoots[14]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[4], _unidirectionalRoots[15]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[5], _unidirectionalRoots[16]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[6], _unidirectionalRoots[0]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[6], _unidirectionalRoots[5]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[6], _unidirectionalRoots[12]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[6], _unidirectionalRoots[19]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[7], _unidirectionalRoots[9]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[9], _unidirectionalRoots[2]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[9], _unidirectionalRoots[7]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[12], _unidirectionalRoots[4]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[13], _unidirectionalRoots[0]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[13], _unidirectionalRoots[13]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[15], _unidirectionalRoots[4]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[15], _unidirectionalRoots[6]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[16], _unidirectionalRoots[13]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[17], _unidirectionalRoots[5]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[17], _unidirectionalRoots[18]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[18], _unidirectionalRoots[10]),
            CreateUnidirectionalEntityRootEntityThree(context, _unidirectionalThrees[19], _unidirectionalRoots[13])
        ];

    private static Dictionary<string, object> CreateUnidirectionalEntityRootEntityThree(
        ManyToManyContext context,
        UnidirectionalEntityThree three,
        UnidirectionalEntityRoot root)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("UnidirectionalEntityRootUnidirectionalEntityThree"), (e, p) =>
            {
                e["ThreeSkipSharedId"] = context?.Entry(three).Property(e => e.Id).CurrentValue ?? three.Id;
                e["UnidirectionalEntityRootId"] = context?.Entry(root).Property(e => e.Id).CurrentValue ?? root.Id;
            });

    private Dictionary<string, object>[] CreateUnidirectionalEntityRootUnidirectionalEntityBranches(ManyToManyContext context)
    {
        var branches = _unidirectionalRoots.OfType<UnidirectionalEntityBranch>().ToList();
        return
        [
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[0], _unidirectionalRoots[6]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[0], _unidirectionalRoots[7]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[0], _unidirectionalRoots[14]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[1], _unidirectionalRoots[3]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[1], _unidirectionalRoots[15]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[2], _unidirectionalRoots[11]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[2], _unidirectionalRoots[13]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[2], _unidirectionalRoots[19]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[4], _unidirectionalRoots[13]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[4], _unidirectionalRoots[14]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[4], _unidirectionalRoots[15]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[5], _unidirectionalRoots[16]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[6], _unidirectionalRoots[0]),
            CreateUnidirectionalEntityRootUnidirectionalEntityBranch(context, branches[6], _unidirectionalRoots[5])
        ];
    }

    private static Dictionary<string, object> CreateUnidirectionalEntityRootUnidirectionalEntityBranch(
        ManyToManyContext context,
        UnidirectionalEntityBranch branch,
        UnidirectionalEntityRoot root)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("UnidirectionalEntityBranchUnidirectionalEntityRoot"), (e, p) =>
            {
                e["BranchSkipSharedId"] = context?.Entry(branch).Property(e => e.Id).CurrentValue ?? branch.Id;
                e["UnidirectionalEntityRootId"] = context?.Entry(root).Property(e => e.Id).CurrentValue ?? root.Id;
            });

    private Dictionary<string, object>[] CreateUnidirectionalJoinCompositeKeyToRootShareds(ManyToManyContext context)
        =>
        [
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[5], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[8], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[19], _unidirectionalCompositeKeys[0]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[0], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[1], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[3], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[5], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[10], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[17], _unidirectionalCompositeKeys[1]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[3], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[13], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[15], _unidirectionalCompositeKeys[2]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[1], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[2], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[3], _unidirectionalCompositeKeys[3]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[1], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[7], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[15], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[17], _unidirectionalCompositeKeys[7]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[6], _unidirectionalCompositeKeys[8]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[7], _unidirectionalCompositeKeys[8]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[18], _unidirectionalCompositeKeys[8]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[2], _unidirectionalCompositeKeys[9]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[11], _unidirectionalCompositeKeys[9]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[17], _unidirectionalCompositeKeys[9]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[1], _unidirectionalCompositeKeys[10]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[3], _unidirectionalCompositeKeys[10]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[4], _unidirectionalCompositeKeys[10]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[6], _unidirectionalCompositeKeys[11]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[2], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[7], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[13], _unidirectionalCompositeKeys[12]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[3], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[10], _unidirectionalCompositeKeys[14]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[0], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[6], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[14], _unidirectionalCompositeKeys[15]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[0], _unidirectionalCompositeKeys[18]),
            CreateUnidirectionalJoinCompositeKeyToRootShared(context, _unidirectionalRoots[5], _unidirectionalCompositeKeys[19])
        ];

    private static Dictionary<string, object> CreateUnidirectionalJoinCompositeKeyToRootShared(
        ManyToManyContext context,
        UnidirectionalEntityRoot root,
        UnidirectionalEntityCompositeKey composite)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("UnidirectionalEntityCompositeKeyUnidirectionalEntityRoot"), (e, p) =>
            {
                e["RootSkipSharedId"] = context?.Entry(root).Property(e => e.Id).CurrentValue ?? root.Id;
                e["UnidirectionalEntityCompositeKeyKey1"] = context?.Entry(composite).Property(e => e.Key1).CurrentValue ?? composite.Key1;
                e["UnidirectionalEntityCompositeKeyKey2"] = composite.Key2;
                e["UnidirectionalEntityCompositeKeyKey3"] = composite.Key3;
            });

    private static ICollection<TEntity> CreateCollection<TEntity>(bool proxy)
        => proxy ? new ObservableCollection<TEntity>() : new List<TEntity>();

    private static TEntity CreateInstance<TEntity>(DbSet<TEntity> set, Action<TEntity, bool> configureEntity)
        where TEntity : class, new()
    {
        if (set != null)
        {
            return set.CreateInstance(configureEntity);
        }

        var entity = new TEntity();
        configureEntity(entity, false);
        return entity;
    }
}
