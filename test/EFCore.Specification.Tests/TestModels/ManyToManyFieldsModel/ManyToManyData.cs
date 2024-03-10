// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class ManyToManyData : ISetSource
{
    private readonly bool _useGeneratedKeys;
    private readonly EntityOne[] _ones;
    private readonly EntityTwo[] _twos;
    private readonly EntityThree[] _threes;
    private readonly EntityCompositeKey[] _compositeKeys;
    private readonly EntityRoot[] _roots;

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
        context.Set<Dictionary<string, object>>("EntityTwoEntityTwo").AddRange(CreateEntityTwoEntityTwos(context));
        context.Set<Dictionary<string, object>>("EntityCompositeKeyEntityTwo").AddRange(CreateEntityCompositeKeyEntityTwos(context));
        context.Set<Dictionary<string, object>>("EntityRootEntityThree").AddRange(CreateEntityRootEntityThrees(context));
        context.Set<Dictionary<string, object>>("EntityCompositeKeyEntityRoot").AddRange(CreateEntityCompositeKeyEntityRoots(context));
        context.Set<Dictionary<string, object>>("EntityBranchEntityRoot").AddRange(CreateEntityRootEntityBranches(context));
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(EntityOne))
        {
            return (IQueryable<TEntity>)_ones.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityTwo))
        {
            return (IQueryable<TEntity>)_twos.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityThree))
        {
            return (IQueryable<TEntity>)_threes.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityCompositeKey))
        {
            return (IQueryable<TEntity>)_compositeKeys.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityRoot))
        {
            return (IQueryable<TEntity>)_roots.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityBranch))
        {
            return (IQueryable<TEntity>)_roots.OfType<EntityBranch>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityLeaf))
        {
            return (IQueryable<TEntity>)_roots.OfType<EntityLeaf>().AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

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

    private Dictionary<string, object>[] CreateEntityTwoEntityTwos(ManyToManyContext context)
        =>
        [
            CreateEntityTwoEntityTwo(context, _twos[0], _twos[8]),
            CreateEntityTwoEntityTwo(context, _twos[0], _twos[9]),
            CreateEntityTwoEntityTwo(context, _twos[0], _twos[10]),
            CreateEntityTwoEntityTwo(context, _twos[0], _twos[17]),
            CreateEntityTwoEntityTwo(context, _twos[2], _twos[1]),
            CreateEntityTwoEntityTwo(context, _twos[2], _twos[4]),
            CreateEntityTwoEntityTwo(context, _twos[2], _twos[7]),
            CreateEntityTwoEntityTwo(context, _twos[2], _twos[17]),
            CreateEntityTwoEntityTwo(context, _twos[2], _twos[18]),
            CreateEntityTwoEntityTwo(context, _twos[3], _twos[10]),
            CreateEntityTwoEntityTwo(context, _twos[4], _twos[7]),
            CreateEntityTwoEntityTwo(context, _twos[5], _twos[17]),
            CreateEntityTwoEntityTwo(context, _twos[7], _twos[1]),
            CreateEntityTwoEntityTwo(context, _twos[7], _twos[13]),
            CreateEntityTwoEntityTwo(context, _twos[7], _twos[14]),
            CreateEntityTwoEntityTwo(context, _twos[7], _twos[19]),
            CreateEntityTwoEntityTwo(context, _twos[8], _twos[3]),
            CreateEntityTwoEntityTwo(context, _twos[8], _twos[13]),
            CreateEntityTwoEntityTwo(context, _twos[9], _twos[4]),
            CreateEntityTwoEntityTwo(context, _twos[11], _twos[12]),
            CreateEntityTwoEntityTwo(context, _twos[11], _twos[13]),
            CreateEntityTwoEntityTwo(context, _twos[12], _twos[13]),
            CreateEntityTwoEntityTwo(context, _twos[12], _twos[17]),
            CreateEntityTwoEntityTwo(context, _twos[12], _twos[18]),
            CreateEntityTwoEntityTwo(context, _twos[15], _twos[5]),
            CreateEntityTwoEntityTwo(context, _twos[16], _twos[8]),
            CreateEntityTwoEntityTwo(context, _twos[16], _twos[18]),
            CreateEntityTwoEntityTwo(context, _twos[16], _twos[19]),
            CreateEntityTwoEntityTwo(context, _twos[17], _twos[1]),
            CreateEntityTwoEntityTwo(context, _twos[17], _twos[4]),
            CreateEntityTwoEntityTwo(context, _twos[17], _twos[15]),
            CreateEntityTwoEntityTwo(context, _twos[17], _twos[16]),
            CreateEntityTwoEntityTwo(context, _twos[18], _twos[1]),
            CreateEntityTwoEntityTwo(context, _twos[19], _twos[3])
        ];

    private static Dictionary<string, object> CreateEntityTwoEntityTwo(
        ManyToManyContext context,
        EntityTwo left,
        EntityTwo right)
        => CreateInstance(
            context?.Set<Dictionary<string, object>>("EntityTwoEntityTwo"), (e, p) =>
            {
                e["SelfSkipSharedLeftId"] = context?.Entry(left).Property(e => e.Id).CurrentValue ?? left.Id;
                e["SelfSkipSharedRightId"] = context?.Entry(right).Property(e => e.Id).CurrentValue ?? right.Id;
            });

    private Dictionary<string, object>[] CreateEntityCompositeKeyEntityTwos(ManyToManyContext context)
        =>
        [
            CreateEntityCompositeKeyEntityTwo(context, _twos[0], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[0], _compositeKeys[3]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[0], _compositeKeys[4]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[1], _compositeKeys[3]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[2], _compositeKeys[5]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[3], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[3], _compositeKeys[18]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[5], _compositeKeys[2]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[5], _compositeKeys[12]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[6], _compositeKeys[7]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[8], _compositeKeys[2]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[8], _compositeKeys[8]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[9], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[9], _compositeKeys[14]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[9], _compositeKeys[17]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[10], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[10], _compositeKeys[14]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[11], _compositeKeys[7]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[11], _compositeKeys[12]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[11], _compositeKeys[14]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[12], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[12], _compositeKeys[6]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[12], _compositeKeys[16]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[14], _compositeKeys[15]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[15], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[15], _compositeKeys[2]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[15], _compositeKeys[18]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[16], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[16], _compositeKeys[7]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[16], _compositeKeys[13]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[16], _compositeKeys[14]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[18], _compositeKeys[4]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[19], _compositeKeys[2]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[19], _compositeKeys[4]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[19], _compositeKeys[5]),
            CreateEntityCompositeKeyEntityTwo(context, _twos[19], _compositeKeys[13])
        ];

    private static Dictionary<string, object> CreateEntityCompositeKeyEntityTwo(
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
            context?.Set<Dictionary<string, object>>("EntityRootEntityBranch"), (e, p) =>
            {
                e["BranchSkipSharedId"] = context?.Entry(branch).Property(e => e.Id).CurrentValue ?? branch.Id;
                e["RootSkipSharedId"] = context?.Entry(root).Property(e => e.Id).CurrentValue ?? root.Id;
            });

    private Dictionary<string, object>[] CreateEntityCompositeKeyEntityRoots(ManyToManyContext context)
        =>
        [
            CreateEntityCompositeKeyEntityRoot(context, _roots[5], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[8], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[19], _compositeKeys[0]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[0], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[1], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[3], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[5], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[10], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[17], _compositeKeys[1]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[3], _compositeKeys[2]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[13], _compositeKeys[2]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[15], _compositeKeys[2]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[1], _compositeKeys[3]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[2], _compositeKeys[3]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[3], _compositeKeys[3]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[1], _compositeKeys[7]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[7], _compositeKeys[7]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[15], _compositeKeys[7]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[17], _compositeKeys[7]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[6], _compositeKeys[8]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[7], _compositeKeys[8]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[18], _compositeKeys[8]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[2], _compositeKeys[9]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[11], _compositeKeys[9]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[17], _compositeKeys[9]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[1], _compositeKeys[10]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[3], _compositeKeys[10]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[4], _compositeKeys[10]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[6], _compositeKeys[11]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[2], _compositeKeys[12]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[7], _compositeKeys[12]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[13], _compositeKeys[12]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[3], _compositeKeys[14]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[10], _compositeKeys[14]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[0], _compositeKeys[15]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[6], _compositeKeys[15]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[14], _compositeKeys[15]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[0], _compositeKeys[18]),
            CreateEntityCompositeKeyEntityRoot(context, _roots[5], _compositeKeys[19])
        ];

    private static ICollection<TEntity> CreateCollection<TEntity>(bool proxy)
        => proxy ? new ObservableCollection<TEntity>() : new List<TEntity>();

    private static Dictionary<string, object> CreateEntityCompositeKeyEntityRoot(
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
