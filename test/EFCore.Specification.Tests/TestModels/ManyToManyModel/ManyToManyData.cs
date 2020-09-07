// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class ManyToManyData : ISetSource
    {
        private readonly EntityOne[] _ones;
        private readonly EntityTwo[] _twos;
        private readonly EntityThree[] _threes;
        private readonly EntityCompositeKey[] _compositeKeys;
        private readonly EntityRoot[] _roots;

        private readonly JoinCompositeKeyToLeaf[] _joinCompositeKeyToLeaves;
        private readonly JoinOneSelfPayload[] _joinOneSelfPayloads;
        private readonly JoinOneToBranch[] _joinOneToBranches;
        private readonly JoinOneToThreePayloadFull[] _joinOneToThreePayloadFulls;
        private readonly JoinOneToTwo[] _joinOneToTwos;
        private readonly JoinThreeToCompositeKeyFull[] _joinThreeToCompositeKeyFulls;
        private readonly JoinTwoToThree[] _joinTwoToThrees;

        private readonly Dictionary<string, object>[] _joinOneToTwoShareds;
        private readonly Dictionary<string, object>[] _joinOneToThreePayloadFullShareds;
        private readonly Dictionary<string, object>[] _joinTwoSelfShareds;
        private readonly Dictionary<string, object>[] _joinTwoToCompositeKeyShareds;
        private readonly Dictionary<string, object>[] _joinThreeToRootShareds;
        private readonly Dictionary<string, object>[] _joinCompositeKeyToRootShareds;

        public ManyToManyData()
        {
            _ones = CreateOnes(null);
            _twos = CreateTwos(null);
            _threes = CreateThrees(null);
            _compositeKeys = CreateCompositeKeys(null);
            _roots = CreateRoots(null);

            _joinCompositeKeyToLeaves = CreateJoinCompositeKeyToLeaves(null);
            _joinOneSelfPayloads = CreateJoinOneSelfPayloads(null);
            _joinOneToBranches = CreateJoinOneToBranches(null);
            _joinOneToThreePayloadFulls = CreateJoinOneToThreePayloadFulls(null);
            _joinOneToTwos = CreateJoinOneToTwos(null);
            _joinThreeToCompositeKeyFulls = CreateJoinThreeToCompositeKeyFulls(null);
            _joinTwoToThrees = CreateJoinTwoToThrees(null);

            _joinOneToTwoShareds = CreateEntityOneEntityTwos(null);
            _joinOneToThreePayloadFullShareds = CreateJoinOneToThreePayloadFullShareds(null);
            _joinTwoSelfShareds = CreateJoinTwoSelfShareds(null);
            _joinTwoToCompositeKeyShareds = CreateJoinTwoToCompositeKeyShareds(null);
            _joinThreeToRootShareds = CreateEntityRootEntityThrees(null);
            _joinCompositeKeyToRootShareds = CreateJoinCompositeKeyToRootShareds(null);

            foreach (var basicTwo in _twos)
            {
                var collectionOne = _ones.FirstOrDefault(o => o.Id == basicTwo.CollectionInverseId);
                basicTwo.CollectionInverse = collectionOne;
                collectionOne?.Collection.Add(basicTwo);

                var referenceOne = _ones.FirstOrDefault(o => o.Id == basicTwo.ReferenceInverseId);
                basicTwo.ReferenceInverse = referenceOne;
                if (referenceOne != null)
                {
                    referenceOne.Reference = basicTwo;
                }
            }

            foreach (var basicThree in _threes)
            {
                var collectionTwo = _twos.FirstOrDefault(o => o.Id == basicThree.CollectionInverseId);
                basicThree.CollectionInverse = collectionTwo;
                collectionTwo?.Collection.Add(basicThree);

                var referenceTwo = _twos.FirstOrDefault(o => o.Id == basicThree.ReferenceInverseId);
                basicThree.ReferenceInverse = referenceTwo;
                if (referenceTwo != null)
                {
                    referenceTwo.Reference = basicThree;
                }
            }

            // Join entities
            foreach (var joinEntity in _joinOneToTwos)
            {
                var one = _ones.First(o => o.Id == joinEntity.OneId);
                var two = _twos.First(t => t.Id == joinEntity.TwoId);
                one.TwoSkip.Add(two);
                two.OneSkip.Add(one);
            }

            foreach (var joinEntity in _joinOneToThreePayloadFulls)
            {
                var one = _ones.First(o => o.Id == joinEntity.OneId);
                var three = _threes.First(t => t.Id == joinEntity.ThreeId);
                one.ThreeSkipPayloadFull.Add(three);
                one.JoinThreePayloadFull.Add(joinEntity);
                three.OneSkipPayloadFull.Add(one);
                three.JoinOnePayloadFull.Add(joinEntity);
                joinEntity.One = one;
                joinEntity.Three = three;
            }

            foreach (var joinEntity in _joinOneSelfPayloads)
            {
                var left = _ones.First(o => o.Id == joinEntity.LeftId);
                var right = _ones.First(t => t.Id == joinEntity.RightId);
                left.SelfSkipPayloadRight.Add(right);
                left.JoinSelfPayloadRight.Add(joinEntity);
                right.SelfSkipPayloadLeft.Add(left);
                right.JoinSelfPayloadLeft.Add(joinEntity);
                joinEntity.Left = left;
                joinEntity.Right = right;
            }

            foreach (var joinEntity in _joinOneToBranches)
            {
                var one = _ones.First(o => o.Id == joinEntity.EntityOneId);
                var branch = _roots.OfType<EntityBranch>().First(t => t.Id == joinEntity.EntityBranchId);
                one.BranchSkip.Add(branch);
                branch.OneSkip.Add(one);
            }

            foreach (var joinEntity in _joinTwoToThrees)
            {
                var two = _twos.First(o => o.Id == joinEntity.TwoId);
                var three = _threes.First(t => t.Id == joinEntity.ThreeId);
                two.ThreeSkipFull.Add(three);
                two.JoinThreeFull.Add(joinEntity);
                three.TwoSkipFull.Add(two);
                three.JoinTwoFull.Add(joinEntity);
                joinEntity.Two = two;
                joinEntity.Three = three;
            }

            foreach (var joinEntity in _joinThreeToCompositeKeyFulls)
            {
                var compositeKey = _compositeKeys.First(
                    o => o.Key1 == joinEntity.CompositeId1
                        && o.Key2 == joinEntity.CompositeId2
                        && o.Key3 == joinEntity.CompositeId3);
                var three = _threes.First(t => t.Id == joinEntity.ThreeId);
                compositeKey.ThreeSkipFull.Add(three);
                compositeKey.JoinThreeFull.Add(joinEntity);
                three.CompositeKeySkipFull.Add(compositeKey);
                three.JoinCompositeKeyFull.Add(joinEntity);
                joinEntity.Composite = compositeKey;
                joinEntity.Three = three;
            }

            foreach (var joinEntity in _joinCompositeKeyToLeaves)
            {
                var compositeKey = _compositeKeys.First(
                    o => o.Key1 == joinEntity.CompositeId1
                        && o.Key2 == joinEntity.CompositeId2
                        && o.Key3 == joinEntity.CompositeId3);
                var leaf = _roots.OfType<EntityLeaf>().First(t => t.Id == joinEntity.LeafId);
                compositeKey.LeafSkipFull.Add(leaf);
                compositeKey.JoinLeafFull.Add(joinEntity);
                leaf.CompositeKeySkipFull.Add(compositeKey);
                leaf.JoinCompositeKeyFull.Add(joinEntity);
                joinEntity.Composite = compositeKey;
                joinEntity.Leaf = leaf;
            }

            // Shared join entities
            foreach (var joinEntity in _joinOneToTwoShareds)
            {
                var one = _ones.First(o => o.Id == (int)joinEntity["EntityOneId"]);
                var two = _twos.First(t => t.Id == (int)joinEntity["EntityTwoId"]);
                one.TwoSkipShared.Add(two);
                two.OneSkipShared.Add(one);
            }

            foreach (var joinEntity in _joinOneToThreePayloadFullShareds)
            {
                var one = _ones.First(o => o.Id == (int)joinEntity["OneId"]);
                var three = _threes.First(t => t.Id == (int)joinEntity["ThreeId"]);
                one.ThreeSkipPayloadFullShared.Add(three);
                one.JoinThreePayloadFullShared.Add(joinEntity);
                three.OneSkipPayloadFullShared.Add(one);
                three.JoinOnePayloadFullShared.Add(joinEntity);
            }

            foreach (var joinEntity in _joinTwoSelfShareds)
            {
                var left = _twos.First(o => o.Id == (int)joinEntity["LeftId"]);
                var right = _twos.First(t => t.Id == (int)joinEntity["RightId"]);
                left.SelfSkipSharedRight.Add(right);
                right.SelfSkipSharedLeft.Add(left);
            }

            foreach (var joinEntity in _joinTwoToCompositeKeyShareds)
            {
                var compositeKey = _compositeKeys.First(
                    o => o.Key1 == (int)joinEntity["CompositeId1"]
                        && o.Key2 == (string)joinEntity["CompositeId2"]
                        && o.Key3 == (DateTime)joinEntity["CompositeId3"]);
                var two = _twos.First(t => t.Id == (int)joinEntity["TwoId"]);
                compositeKey.TwoSkipShared.Add(two);
                two.CompositeKeySkipShared.Add(compositeKey);
            }

            foreach (var joinEntity in _joinThreeToRootShareds)
            {
                var three = _threes.First(o => o.Id == (int)joinEntity["EntityThreeId"]);
                var root = _roots.First(t => t.Id == (int)joinEntity["EntityRootId"]);
                three.RootSkipShared.Add(root);
                root.ThreeSkipShared.Add(three);
            }

            foreach (var joinEntity in _joinCompositeKeyToRootShareds)
            {
                var compositeKey = _compositeKeys.First(
                    o => o.Key1 == (int)joinEntity["CompositeId1"]
                        && o.Key2 == (string)joinEntity["CompositeId2"]
                        && o.Key3 == (DateTime)joinEntity["CompositeId3"]);
                var root = _roots.First(t => t.Id == (int)joinEntity["RootId"]);
                compositeKey.RootSkipShared.Add(root);
                root.CompositeKeySkipShared.Add(compositeKey);
            }
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

        public static void Seed(ManyToManyContext context)
        {
            context.Set<EntityOne>().AddRange(CreateOnes(context));
            context.Set<EntityTwo>().AddRange(CreateTwos(context));
            context.Set<EntityThree>().AddRange(CreateThrees(context));
            context.Set<EntityCompositeKey>().AddRange(CreateCompositeKeys(context));
            context.Set<EntityRoot>().AddRange(CreateRoots(context));

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
            context.Set<Dictionary<string, object>>("JoinTwoSelfShared").AddRange(CreateJoinTwoSelfShareds(context));
            context.Set<Dictionary<string, object>>("JoinTwoToCompositeKeyShared").AddRange(CreateJoinTwoToCompositeKeyShareds(context));
            context.Set<Dictionary<string, object>>("EntityRootEntityThree").AddRange(CreateEntityRootEntityThrees(context));
            context.Set<Dictionary<string, object>>("JoinCompositeKeyToRootShared").AddRange(CreateJoinCompositeKeyToRootShareds(context));

            context.SaveChanges();
        }

        private static EntityOne[] CreateOnes(ManyToManyContext context)
            => new[]
            {
                CreateEntityOne(context, 1, "EntityOne 1"),
                CreateEntityOne(context, 2, "EntityOne 2"),
                CreateEntityOne(context, 3, "EntityOne 3"),
                CreateEntityOne(context, 4, "EntityOne 4"),
                CreateEntityOne(context, 5, "EntityOne 5"),
                CreateEntityOne(context, 6, "EntityOne 6"),
                CreateEntityOne(context, 7, "EntityOne 7"),
                CreateEntityOne(context, 8, "EntityOne 8"),
                CreateEntityOne(context, 9, "EntityOne 9"),
                CreateEntityOne(context, 10, "EntityOne 10"),
                CreateEntityOne(context, 11, "EntityOne 11"),
                CreateEntityOne(context, 12, "EntityOne 12"),
                CreateEntityOne(context, 13, "EntityOne 13"),
                CreateEntityOne(context, 14, "EntityOne 14"),
                CreateEntityOne(context, 15, "EntityOne 15"),
                CreateEntityOne(context, 16, "EntityOne 16"),
                CreateEntityOne(context, 17, "EntityOne 17"),
                CreateEntityOne(context, 18, "EntityOne 18"),
                CreateEntityOne(context, 19, "EntityOne 19"),
                CreateEntityOne(context, 20, "EntityOne 20"),
            };

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

        private static EntityTwo[] CreateTwos(ManyToManyContext context)
            => new[]
            {
                CreateEntityTwo(context, 1, "EntityTwo 1", null, 1),
                CreateEntityTwo(context, 2, "EntityTwo 2", null, 1),
                CreateEntityTwo(context, 3, "EntityTwo 3", null, null),
                CreateEntityTwo(context, 4, "EntityTwo 4", null, 3),
                CreateEntityTwo(context, 5, "EntityTwo 5", null, 3),
                CreateEntityTwo(context, 6, "EntityTwo 6", null, 5),
                CreateEntityTwo(context, 7, "EntityTwo 7", null, 5),
                CreateEntityTwo(context, 8, "EntityTwo 8", null, 7),
                CreateEntityTwo(context, 9, "EntityTwo 9", null, 7),
                CreateEntityTwo(context, 10, "EntityTwo 10", 20, 9),
                CreateEntityTwo(context, 11, "EntityTwo 11", 18, 9),
                CreateEntityTwo(context, 12, "EntityTwo 12", 16, 11),
                CreateEntityTwo(context, 13, "EntityTwo 13", 14, 11),
                CreateEntityTwo(context, 14, "EntityTwo 14", 12, 13),
                CreateEntityTwo(context, 15, "EntityTwo 15", 11, 13),
                CreateEntityTwo(context, 16, "EntityTwo 16", 9, 15),
                CreateEntityTwo(context, 17, "EntityTwo 17", 7, 15),
                CreateEntityTwo(context, 18, "EntityTwo 18", 5, 16),
                CreateEntityTwo(context, 19, "EntityTwo 19", 3, 16),
                CreateEntityTwo(context, 20, "EntityTwo 20", 1, 17),
            };

        private static EntityTwo CreateEntityTwo(
            ManyToManyContext context,
            int id,
            string name,
            int? referenceInverseId,
            int? collectionInverseId)
            => CreateInstance(
                context?.EntityTwos, (e, p) =>
                {
                    e.Id = id;
                    e.Name = name;
                    e.ReferenceInverseId = referenceInverseId;
                    e.CollectionInverseId = collectionInverseId;
                    e.Collection = CreateCollection<EntityThree>(p);
                    e.OneSkip = CreateCollection<EntityOne>(p);
                    e.ThreeSkipFull = CreateCollection<EntityThree>(p);
                    e.JoinThreeFull = CreateCollection<JoinTwoToThree>(p);
                    e.SelfSkipSharedLeft = CreateCollection<EntityTwo>(p);
                    e.SelfSkipSharedRight = CreateCollection<EntityTwo>(p);
                    e.OneSkipShared = CreateCollection<EntityOne>(p);
                    e.CompositeKeySkipShared = CreateCollection<EntityCompositeKey>(p);
                });

        private static EntityThree[] CreateThrees(ManyToManyContext context)
            => new[]
            {
                CreateEntityThree(context, 1, "EntityThree 1", null, null),
                CreateEntityThree(context, 2, "EntityThree 2", 19, 17),
                CreateEntityThree(context, 3, "EntityThree 3", 2, 16),
                CreateEntityThree(context, 4, "EntityThree 4", 20, 16),
                CreateEntityThree(context, 5, "EntityThree 5", 4, 15),
                CreateEntityThree(context, 6, "EntityThree 6", null, 15),
                CreateEntityThree(context, 7, "EntityThree 7", 6, 13),
                CreateEntityThree(context, 8, "EntityThree 8", null, 13),
                CreateEntityThree(context, 9, "EntityThree 9", 8, 11),
                CreateEntityThree(context, 10, "EntityThree 10", null, 11),
                CreateEntityThree(context, 11, "EntityThree 11", 19, 9),
                CreateEntityThree(context, 12, "EntityThree 12", null, 9),
                CreateEntityThree(context, 13, "EntityThree 13", 12, 7),
                CreateEntityThree(context, 14, "EntityThree 14", null, 7),
                CreateEntityThree(context, 15, "EntityThree 15", 14, 5),
                CreateEntityThree(context, 16, "EntityThree 16", null, 5),
                CreateEntityThree(context, 17, "EntityThree 17", 16, 3),
                CreateEntityThree(context, 18, "EntityThree 18", null, 3),
                CreateEntityThree(context, 19, "EntityThree 19", 18, 1),
                CreateEntityThree(context, 20, "EntityThree 20", null, 1),
            };

        private static EntityThree CreateEntityThree(
            ManyToManyContext context,
            int id,
            string name,
            int? referenceInverseId,
            int? collectionInverseId)
            => CreateInstance(
                context?.EntityThrees, (e, p) =>
                {
                    e.Id = id;
                    e.Name = name;
                    e.ReferenceInverseId = referenceInverseId;
                    e.CollectionInverseId = collectionInverseId;
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

        private static EntityCompositeKey[] CreateCompositeKeys(ManyToManyContext context)
            => new[]
            {
                CreateEntityCompositeKey(context, 1, "1_1", new DateTime(2001, 1, 1), "Composite 1"),
                CreateEntityCompositeKey(context, 1, "1_2", new DateTime(2001, 2, 1), "Composite 2"),
                CreateEntityCompositeKey(context, 3, "3_1", new DateTime(2003, 1, 1), "Composite 3"),
                CreateEntityCompositeKey(context, 3, "3_2", new DateTime(2003, 2, 1), "Composite 4"),
                CreateEntityCompositeKey(context, 3, "3_3", new DateTime(2003, 3, 1), "Composite 5"),
                CreateEntityCompositeKey(context, 6, "6_1", new DateTime(2006, 1, 1), "Composite 6"),
                CreateEntityCompositeKey(context, 7, "7_1", new DateTime(2007, 1, 1), "Composite 7"),
                CreateEntityCompositeKey(context, 7, "7_2", new DateTime(2007, 2, 1), "Composite 8"),
                CreateEntityCompositeKey(context, 8, "8_1", new DateTime(2008, 1, 1), "Composite 9"),
                CreateEntityCompositeKey(context, 8, "8_2", new DateTime(2008, 2, 1), "Composite 10"),
                CreateEntityCompositeKey(context, 8, "8_3", new DateTime(2008, 3, 1), "Composite 11"),
                CreateEntityCompositeKey(context, 8, "8_4", new DateTime(2008, 4, 1), "Composite 12"),
                CreateEntityCompositeKey(context, 8, "8_5", new DateTime(2008, 5, 1), "Composite 13"),
                CreateEntityCompositeKey(context, 9, "9_1", new DateTime(2009, 1, 1), "Composite 14"),
                CreateEntityCompositeKey(context, 9, "9_2", new DateTime(2009, 2, 1), "Composite 15"),
                CreateEntityCompositeKey(context, 9, "9_3", new DateTime(2009, 3, 1), "Composite 16"),
                CreateEntityCompositeKey(context, 9, "9_4", new DateTime(2009, 4, 1), "Composite 17"),
                CreateEntityCompositeKey(context, 9, "9_5", new DateTime(2009, 5, 1), "Composite 18"),
                CreateEntityCompositeKey(context, 9, "9_6", new DateTime(2009, 6, 1), "Composite 19"),
                CreateEntityCompositeKey(context, 9, "9_7", new DateTime(2009, 7, 1), "Composite 20")
            };

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

        private static EntityRoot[] CreateRoots(ManyToManyContext context)
            => new[]
            {
                CreateEntityRoot(context, 1, "Root 1"),
                CreateEntityRoot(context, 2, "Root 2"),
                CreateEntityRoot(context, 3, "Root 3"),
                CreateEntityRoot(context, 4, "Root 4"),
                CreateEntityRoot(context, 5, "Root 5"),
                CreateEntityRoot(context, 6, "Root 6"),
                CreateEntityRoot(context, 7, "Root 7"),
                CreateEntityRoot(context, 8, "Root 8"),
                CreateEntityRoot(context, 9, "Root 9"),
                CreateEntityRoot(context, 10, "Root 10"),
                CreateEntityBranch(context, 11, "Branch 1", 7),
                CreateEntityBranch(context, 12, "Branch 2", 77),
                CreateEntityBranch(context, 13, "Branch 3", 777),
                CreateEntityBranch(context, 14, "Branch 4", 7777),
                CreateEntityBranch(context, 15, "Branch 5", 77777),
                CreateEntityBranch(context, 16, "Branch 6", 777777),
                CreateEntityLeaf(context, 21, "Leaf 1", 42, true),
                CreateEntityLeaf(context, 22, "Leaf 2", 421, true),
                CreateEntityLeaf(context, 23, "Leaf 3", 1337, false),
                CreateEntityLeaf(context, 24, "Leaf 4", 1729, false)
            };

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

        private static JoinCompositeKeyToLeaf[] CreateJoinCompositeKeyToLeaves(ManyToManyContext context)
            => new[]
            {
                CreateJoinCompositeKeyToLeaf(context, 21, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 22, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 24, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 22, 3, "3_3", new DateTime(2003, 3, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 6, "6_1", new DateTime(2006, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 22, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 24, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 8, "8_1", new DateTime(2008, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 8, "8_2", new DateTime(2008, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 22, 8, "8_3", new DateTime(2008, 3, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 8, "8_3", new DateTime(2008, 3, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinCompositeKeyToLeaf(context, 24, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 9, "9_1", new DateTime(2009, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 22, 9, "9_1", new DateTime(2009, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 9, "9_1", new DateTime(2009, 1, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 22, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinCompositeKeyToLeaf(context, 24, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 9, "9_4", new DateTime(2009, 4, 1)),
                CreateJoinCompositeKeyToLeaf(context, 24, 9, "9_4", new DateTime(2009, 4, 1)),
                CreateJoinCompositeKeyToLeaf(context, 23, 9, "9_5", new DateTime(2009, 5, 1)),
                CreateJoinCompositeKeyToLeaf(context, 24, 9, "9_5", new DateTime(2009, 5, 1)),
                CreateJoinCompositeKeyToLeaf(context, 21, 9, "9_6", new DateTime(2009, 6, 1)),
                CreateJoinCompositeKeyToLeaf(context, 22, 9, "9_6", new DateTime(2009, 6, 1))
            };

        private static JoinCompositeKeyToLeaf CreateJoinCompositeKeyToLeaf(
            ManyToManyContext context,
            int leafId,
            int compositeId1,
            string compositeId2,
            DateTime compositeId3)
            => CreateInstance(
                context?.Set<JoinCompositeKeyToLeaf>(), (e, p) =>
                {
                    e.LeafId = leafId;
                    e.CompositeId1 = compositeId1;
                    e.CompositeId2 = compositeId2;
                    e.CompositeId3 = compositeId3;
                });

        private static JoinOneSelfPayload[] CreateJoinOneSelfPayloads(ManyToManyContext context)
            => new[]
            {
                CreateJoinOneSelfPayload(context, 3, 4, DateTime.Parse("2020-01-11 19:26:36")),
                CreateJoinOneSelfPayload(context, 3, 6, DateTime.Parse("2005-10-03 12:57:54")),
                CreateJoinOneSelfPayload(context, 3, 8, DateTime.Parse("2015-12-20 01:09:24")),
                CreateJoinOneSelfPayload(context, 3, 18, DateTime.Parse("1999-12-26 02:51:57")),
                CreateJoinOneSelfPayload(context, 3, 20, DateTime.Parse("2011-06-15 19:08:00")),
                CreateJoinOneSelfPayload(context, 5, 3, DateTime.Parse("2019-12-08 05:40:16")),
                CreateJoinOneSelfPayload(context, 5, 4, DateTime.Parse("2014-03-09 12:58:26")),
                CreateJoinOneSelfPayload(context, 6, 5, DateTime.Parse("2014-05-15 16:34:38")),
                CreateJoinOneSelfPayload(context, 6, 7, DateTime.Parse("2014-03-08 18:59:49")),
                CreateJoinOneSelfPayload(context, 6, 13, DateTime.Parse("2013-12-10 07:01:53")),
                CreateJoinOneSelfPayload(context, 7, 13, DateTime.Parse("2005-05-31 02:21:16")),
                CreateJoinOneSelfPayload(context, 8, 9, DateTime.Parse("2011-12-31 19:37:25")),
                CreateJoinOneSelfPayload(context, 8, 11, DateTime.Parse("2012-08-02 16:33:07")),
                CreateJoinOneSelfPayload(context, 8, 12, DateTime.Parse("2018-07-19 09:10:12")),
                CreateJoinOneSelfPayload(context, 10, 7, DateTime.Parse("2018-12-28 01:21:23")),
                CreateJoinOneSelfPayload(context, 13, 2, DateTime.Parse("2014-03-22 02:20:06")),
                CreateJoinOneSelfPayload(context, 13, 18, DateTime.Parse("2005-03-21 14:45:37")),
                CreateJoinOneSelfPayload(context, 14, 9, DateTime.Parse("2016-06-26 08:03:32")),
                CreateJoinOneSelfPayload(context, 15, 13, DateTime.Parse("2018-09-18 12:51:22")),
                CreateJoinOneSelfPayload(context, 16, 5, DateTime.Parse("2016-12-17 14:20:25")),
                CreateJoinOneSelfPayload(context, 16, 6, DateTime.Parse("2008-07-30 03:43:17")),
                CreateJoinOneSelfPayload(context, 17, 14, DateTime.Parse("2019-08-01 16:26:31")),
                CreateJoinOneSelfPayload(context, 19, 1, DateTime.Parse("2010-02-19 13:24:07")),
                CreateJoinOneSelfPayload(context, 19, 8, DateTime.Parse("2004-07-28 09:06:02")),
                CreateJoinOneSelfPayload(context, 19, 12, DateTime.Parse("2004-08-21 11:07:20")),
                CreateJoinOneSelfPayload(context, 20, 1, DateTime.Parse("2014-11-21 18:13:02")),
                CreateJoinOneSelfPayload(context, 20, 7, DateTime.Parse("2009-08-24 21:44:46")),
                CreateJoinOneSelfPayload(context, 20, 14, DateTime.Parse("2013-02-18 02:19:19")),
                CreateJoinOneSelfPayload(context, 20, 16, DateTime.Parse("2016-02-05 14:18:12"))
            };

        private static JoinOneSelfPayload CreateJoinOneSelfPayload(
            ManyToManyContext context,
            int leftId,
            int rightId,
            DateTime payload)
            => CreateInstance(
                context?.Set<JoinOneSelfPayload>(), (e, p) =>
                {
                    e.LeftId = leftId;
                    e.RightId = rightId;
                    e.Payload = payload;
                });

        private static JoinOneToBranch[] CreateJoinOneToBranches(ManyToManyContext context)
            => new[]
            {
                CreateJoinOneToBranch(context, 2, 16),
                CreateJoinOneToBranch(context, 2, 24),
                CreateJoinOneToBranch(context, 3, 14),
                CreateJoinOneToBranch(context, 3, 16),
                CreateJoinOneToBranch(context, 3, 22),
                CreateJoinOneToBranch(context, 3, 24),
                CreateJoinOneToBranch(context, 5, 13),
                CreateJoinOneToBranch(context, 6, 16),
                CreateJoinOneToBranch(context, 6, 22),
                CreateJoinOneToBranch(context, 6, 23),
                CreateJoinOneToBranch(context, 8, 11),
                CreateJoinOneToBranch(context, 8, 12),
                CreateJoinOneToBranch(context, 8, 13),
                CreateJoinOneToBranch(context, 9, 11),
                CreateJoinOneToBranch(context, 9, 12),
                CreateJoinOneToBranch(context, 9, 14),
                CreateJoinOneToBranch(context, 9, 16),
                CreateJoinOneToBranch(context, 9, 21),
                CreateJoinOneToBranch(context, 9, 24),
                CreateJoinOneToBranch(context, 10, 12),
                CreateJoinOneToBranch(context, 10, 13),
                CreateJoinOneToBranch(context, 10, 14),
                CreateJoinOneToBranch(context, 10, 21),
                CreateJoinOneToBranch(context, 12, 11),
                CreateJoinOneToBranch(context, 12, 12),
                CreateJoinOneToBranch(context, 12, 14),
                CreateJoinOneToBranch(context, 12, 23),
                CreateJoinOneToBranch(context, 13, 15),
                CreateJoinOneToBranch(context, 14, 12),
                CreateJoinOneToBranch(context, 14, 14),
                CreateJoinOneToBranch(context, 14, 16),
                CreateJoinOneToBranch(context, 14, 23),
                CreateJoinOneToBranch(context, 15, 15),
                CreateJoinOneToBranch(context, 15, 16),
                CreateJoinOneToBranch(context, 15, 24),
                CreateJoinOneToBranch(context, 16, 11),
                CreateJoinOneToBranch(context, 17, 11),
                CreateJoinOneToBranch(context, 17, 21),
                CreateJoinOneToBranch(context, 18, 12),
                CreateJoinOneToBranch(context, 18, 15),
                CreateJoinOneToBranch(context, 18, 24),
                CreateJoinOneToBranch(context, 19, 11),
                CreateJoinOneToBranch(context, 19, 12),
                CreateJoinOneToBranch(context, 19, 16),
                CreateJoinOneToBranch(context, 19, 23),
                CreateJoinOneToBranch(context, 20, 21),
                CreateJoinOneToBranch(context, 20, 23)
            };

        private static JoinOneToBranch CreateJoinOneToBranch(
            ManyToManyContext context,
            int oneId,
            int branchId)
            => CreateInstance(
                context?.Set<JoinOneToBranch>(), (e, p) =>
                {
                    e.EntityOneId = oneId;
                    e.EntityBranchId = branchId;
                });

        private static JoinOneToThreePayloadFull[] CreateJoinOneToThreePayloadFulls(ManyToManyContext context)
            => new[]
            {
                CreateJoinOneToThreePayloadFull(context, 1, 2, "Ira Watts"),
                CreateJoinOneToThreePayloadFull(context, 1, 6, "Harold May"),
                CreateJoinOneToThreePayloadFull(context, 1, 9, "Freda Vaughn"),
                CreateJoinOneToThreePayloadFull(context, 1, 13, "Pedro Mccarthy"),
                CreateJoinOneToThreePayloadFull(context, 1, 17, "Elaine Simon"),
                CreateJoinOneToThreePayloadFull(context, 2, 9, "Melvin Maldonado"),
                CreateJoinOneToThreePayloadFull(context, 2, 11, "Lora George"),
                CreateJoinOneToThreePayloadFull(context, 2, 13, "Joey Cohen"),
                CreateJoinOneToThreePayloadFull(context, 2, 14, "Erik Carroll"),
                CreateJoinOneToThreePayloadFull(context, 2, 16, "April Rodriguez"),
                CreateJoinOneToThreePayloadFull(context, 3, 5, "Gerardo Colon"),
                CreateJoinOneToThreePayloadFull(context, 3, 12, "Alexander Willis"),
                CreateJoinOneToThreePayloadFull(context, 3, 16, "Laura Wheeler"),
                CreateJoinOneToThreePayloadFull(context, 3, 19, "Lester Summers"),
                CreateJoinOneToThreePayloadFull(context, 4, 2, "Raquel Curry"),
                CreateJoinOneToThreePayloadFull(context, 4, 4, "Steven Fisher"),
                CreateJoinOneToThreePayloadFull(context, 4, 11, "Casey Williams"),
                CreateJoinOneToThreePayloadFull(context, 4, 13, "Lauren Clayton"),
                CreateJoinOneToThreePayloadFull(context, 4, 19, "Maureen Weber"),
                CreateJoinOneToThreePayloadFull(context, 5, 4, "Joyce Ford"),
                CreateJoinOneToThreePayloadFull(context, 5, 6, "Willie Mccormick"),
                CreateJoinOneToThreePayloadFull(context, 5, 9, "Geraldine Jackson"),
                CreateJoinOneToThreePayloadFull(context, 7, 1, "Victor Aguilar"),
                CreateJoinOneToThreePayloadFull(context, 7, 4, "Cathy Allen"),
                CreateJoinOneToThreePayloadFull(context, 7, 9, "Edwin Burke"),
                CreateJoinOneToThreePayloadFull(context, 7, 10, "Eugene Flores"),
                CreateJoinOneToThreePayloadFull(context, 7, 11, "Ginger Patton"),
                CreateJoinOneToThreePayloadFull(context, 7, 12, "Israel Mitchell"),
                CreateJoinOneToThreePayloadFull(context, 7, 18, "Joy Francis"),
                CreateJoinOneToThreePayloadFull(context, 8, 1, "Orville Parker"),
                CreateJoinOneToThreePayloadFull(context, 8, 3, "Alyssa Mann"),
                CreateJoinOneToThreePayloadFull(context, 8, 4, "Hugh Daniel"),
                CreateJoinOneToThreePayloadFull(context, 8, 13, "Kim Craig"),
                CreateJoinOneToThreePayloadFull(context, 8, 14, "Lucille Moreno"),
                CreateJoinOneToThreePayloadFull(context, 8, 17, "Virgil Drake"),
                CreateJoinOneToThreePayloadFull(context, 8, 18, "Josephine Dawson"),
                CreateJoinOneToThreePayloadFull(context, 8, 20, "Milton Huff"),
                CreateJoinOneToThreePayloadFull(context, 9, 2, "Jody Clarke"),
                CreateJoinOneToThreePayloadFull(context, 9, 9, "Elisa Cooper"),
                CreateJoinOneToThreePayloadFull(context, 9, 11, "Grace Owen"),
                CreateJoinOneToThreePayloadFull(context, 9, 12, "Donald Welch"),
                CreateJoinOneToThreePayloadFull(context, 9, 15, "Marian Day"),
                CreateJoinOneToThreePayloadFull(context, 9, 17, "Cory Cortez"),
                CreateJoinOneToThreePayloadFull(context, 10, 2, "Chad Rowe"),
                CreateJoinOneToThreePayloadFull(context, 10, 3, "Simon Reyes"),
                CreateJoinOneToThreePayloadFull(context, 10, 4, "Shari Jensen"),
                CreateJoinOneToThreePayloadFull(context, 10, 8, "Ricky Bradley"),
                CreateJoinOneToThreePayloadFull(context, 10, 10, "Debra Gibbs"),
                CreateJoinOneToThreePayloadFull(context, 10, 11, "Everett Mckenzie"),
                CreateJoinOneToThreePayloadFull(context, 10, 14, "Kirk Graham"),
                CreateJoinOneToThreePayloadFull(context, 10, 16, "Paulette Adkins"),
                CreateJoinOneToThreePayloadFull(context, 10, 18, "Raul Holloway"),
                CreateJoinOneToThreePayloadFull(context, 10, 19, "Danielle Ross"),
                CreateJoinOneToThreePayloadFull(context, 11, 1, "Frank Garner"),
                CreateJoinOneToThreePayloadFull(context, 11, 6, "Stella Thompson"),
                CreateJoinOneToThreePayloadFull(context, 11, 8, "Peggy Wagner"),
                CreateJoinOneToThreePayloadFull(context, 11, 9, "Geneva Holmes"),
                CreateJoinOneToThreePayloadFull(context, 11, 10, "Ignacio Black"),
                CreateJoinOneToThreePayloadFull(context, 11, 13, "Phillip Wells"),
                CreateJoinOneToThreePayloadFull(context, 11, 14, "Hubert Lambert"),
                CreateJoinOneToThreePayloadFull(context, 11, 19, "Courtney Gregory"),
                CreateJoinOneToThreePayloadFull(context, 12, 2, "Esther Carter"),
                CreateJoinOneToThreePayloadFull(context, 13, 6, "Thomas Benson"),
                CreateJoinOneToThreePayloadFull(context, 13, 9, "Kara Baldwin"),
                CreateJoinOneToThreePayloadFull(context, 13, 10, "Yvonne Sparks"),
                CreateJoinOneToThreePayloadFull(context, 13, 11, "Darin Mathis"),
                CreateJoinOneToThreePayloadFull(context, 13, 12, "Glenda Castillo"),
                CreateJoinOneToThreePayloadFull(context, 13, 13, "Larry Walters"),
                CreateJoinOneToThreePayloadFull(context, 13, 15, "Meredith Yates"),
                CreateJoinOneToThreePayloadFull(context, 13, 16, "Rosemarie Henry"),
                CreateJoinOneToThreePayloadFull(context, 13, 18, "Nora Leonard"),
                CreateJoinOneToThreePayloadFull(context, 14, 17, "Corey Delgado"),
                CreateJoinOneToThreePayloadFull(context, 14, 18, "Kari Strickland"),
                CreateJoinOneToThreePayloadFull(context, 15, 8, "Joann Stanley"),
                CreateJoinOneToThreePayloadFull(context, 15, 11, "Camille Gordon"),
                CreateJoinOneToThreePayloadFull(context, 15, 14, "Flora Anderson"),
                CreateJoinOneToThreePayloadFull(context, 15, 15, "Wilbur Soto"),
                CreateJoinOneToThreePayloadFull(context, 15, 18, "Shirley Andrews"),
                CreateJoinOneToThreePayloadFull(context, 15, 20, "Marcus Mcguire"),
                CreateJoinOneToThreePayloadFull(context, 16, 1, "Saul Dixon"),
                CreateJoinOneToThreePayloadFull(context, 16, 6, "Cynthia Hart"),
                CreateJoinOneToThreePayloadFull(context, 16, 10, "Elbert Spencer"),
                CreateJoinOneToThreePayloadFull(context, 16, 13, "Darrell Norris"),
                CreateJoinOneToThreePayloadFull(context, 16, 14, "Jamie Kelley"),
                CreateJoinOneToThreePayloadFull(context, 16, 15, "Francis Briggs"),
                CreateJoinOneToThreePayloadFull(context, 16, 16, "Lindsey Morris"),
                CreateJoinOneToThreePayloadFull(context, 17, 2, "James Castro"),
                CreateJoinOneToThreePayloadFull(context, 17, 5, "Carlos Chavez"),
                CreateJoinOneToThreePayloadFull(context, 17, 7, "Janis Valdez"),
                CreateJoinOneToThreePayloadFull(context, 17, 13, "Alfredo Bowen"),
                CreateJoinOneToThreePayloadFull(context, 17, 14, "Viola Torres"),
                CreateJoinOneToThreePayloadFull(context, 17, 15, "Dianna Lowe"),
                CreateJoinOneToThreePayloadFull(context, 18, 3, "Craig Howell"),
                CreateJoinOneToThreePayloadFull(context, 18, 7, "Sandy Curtis"),
                CreateJoinOneToThreePayloadFull(context, 18, 12, "Alonzo Pierce"),
                CreateJoinOneToThreePayloadFull(context, 18, 18, "Albert Harper"),
                CreateJoinOneToThreePayloadFull(context, 19, 2, "Frankie Baker"),
                CreateJoinOneToThreePayloadFull(context, 19, 5, "Candace Tucker"),
                CreateJoinOneToThreePayloadFull(context, 19, 6, "Willis Christensen"),
                CreateJoinOneToThreePayloadFull(context, 19, 7, "Juan Joseph"),
                CreateJoinOneToThreePayloadFull(context, 19, 10, "Thelma Sanders"),
                CreateJoinOneToThreePayloadFull(context, 19, 11, "Kerry West"),
                CreateJoinOneToThreePayloadFull(context, 19, 15, "Sheri Castro"),
                CreateJoinOneToThreePayloadFull(context, 19, 16, "Mark Schultz"),
                CreateJoinOneToThreePayloadFull(context, 19, 17, "Priscilla Summers"),
                CreateJoinOneToThreePayloadFull(context, 19, 20, "Allan Valdez"),
                CreateJoinOneToThreePayloadFull(context, 20, 3, "Bill Peters"),
                CreateJoinOneToThreePayloadFull(context, 20, 5, "Cora Stone"),
                CreateJoinOneToThreePayloadFull(context, 20, 6, "Frankie Pope"),
                CreateJoinOneToThreePayloadFull(context, 20, 10, "Christian Young"),
                CreateJoinOneToThreePayloadFull(context, 20, 11, "Shari Brewer"),
                CreateJoinOneToThreePayloadFull(context, 20, 12, "Antonia Wolfe"),
                CreateJoinOneToThreePayloadFull(context, 20, 14, "Lawrence Matthews"),
                CreateJoinOneToThreePayloadFull(context, 20, 18, "Van Hubbard"),
                CreateJoinOneToThreePayloadFull(context, 20, 20, "Lindsay Pena")
            };

        private static JoinOneToThreePayloadFull CreateJoinOneToThreePayloadFull(
            ManyToManyContext context,
            int oneId,
            int threeId,
            string payload)
            => CreateInstance(
                context?.Set<JoinOneToThreePayloadFull>(), (e, p) =>
                {
                    e.OneId = oneId;
                    e.ThreeId = threeId;
                    e.Payload = payload;
                });

        private static JoinOneToTwo[] CreateJoinOneToTwos(ManyToManyContext context)
            => new[]
            {
                CreateJoinOneToTwo(context, 1, 1),
                CreateJoinOneToTwo(context, 1, 2),
                CreateJoinOneToTwo(context, 1, 3),
                CreateJoinOneToTwo(context, 1, 4),
                CreateJoinOneToTwo(context, 1, 5),
                CreateJoinOneToTwo(context, 1, 6),
                CreateJoinOneToTwo(context, 1, 7),
                CreateJoinOneToTwo(context, 1, 8),
                CreateJoinOneToTwo(context, 1, 9),
                CreateJoinOneToTwo(context, 1, 10),
                CreateJoinOneToTwo(context, 1, 11),
                CreateJoinOneToTwo(context, 1, 12),
                CreateJoinOneToTwo(context, 1, 13),
                CreateJoinOneToTwo(context, 1, 14),
                CreateJoinOneToTwo(context, 1, 15),
                CreateJoinOneToTwo(context, 1, 16),
                CreateJoinOneToTwo(context, 1, 17),
                CreateJoinOneToTwo(context, 1, 18),
                CreateJoinOneToTwo(context, 1, 19),
                CreateJoinOneToTwo(context, 1, 20),
                CreateJoinOneToTwo(context, 2, 1),
                CreateJoinOneToTwo(context, 2, 3),
                CreateJoinOneToTwo(context, 2, 5),
                CreateJoinOneToTwo(context, 2, 7),
                CreateJoinOneToTwo(context, 2, 9),
                CreateJoinOneToTwo(context, 2, 11),
                CreateJoinOneToTwo(context, 2, 13),
                CreateJoinOneToTwo(context, 2, 15),
                CreateJoinOneToTwo(context, 2, 17),
                CreateJoinOneToTwo(context, 2, 19),
                CreateJoinOneToTwo(context, 3, 1),
                CreateJoinOneToTwo(context, 3, 4),
                CreateJoinOneToTwo(context, 3, 7),
                CreateJoinOneToTwo(context, 3, 10),
                CreateJoinOneToTwo(context, 3, 13),
                CreateJoinOneToTwo(context, 3, 16),
                CreateJoinOneToTwo(context, 3, 19),
                CreateJoinOneToTwo(context, 4, 1),
                CreateJoinOneToTwo(context, 4, 5),
                CreateJoinOneToTwo(context, 4, 9),
                CreateJoinOneToTwo(context, 4, 13),
                CreateJoinOneToTwo(context, 4, 17),
                CreateJoinOneToTwo(context, 5, 1),
                CreateJoinOneToTwo(context, 5, 6),
                CreateJoinOneToTwo(context, 5, 11),
                CreateJoinOneToTwo(context, 5, 16),
                CreateJoinOneToTwo(context, 6, 1),
                CreateJoinOneToTwo(context, 6, 7),
                CreateJoinOneToTwo(context, 6, 13),
                CreateJoinOneToTwo(context, 6, 19),
                CreateJoinOneToTwo(context, 7, 1),
                CreateJoinOneToTwo(context, 7, 8),
                CreateJoinOneToTwo(context, 7, 15),
                CreateJoinOneToTwo(context, 8, 1),
                CreateJoinOneToTwo(context, 8, 9),
                CreateJoinOneToTwo(context, 8, 17),
                CreateJoinOneToTwo(context, 9, 1),
                CreateJoinOneToTwo(context, 9, 10),
                CreateJoinOneToTwo(context, 9, 19),
                CreateJoinOneToTwo(context, 10, 1),
                CreateJoinOneToTwo(context, 10, 11),
                CreateJoinOneToTwo(context, 11, 20),
                CreateJoinOneToTwo(context, 11, 19),
                CreateJoinOneToTwo(context, 11, 18),
                CreateJoinOneToTwo(context, 11, 17),
                CreateJoinOneToTwo(context, 11, 16),
                CreateJoinOneToTwo(context, 11, 15),
                CreateJoinOneToTwo(context, 11, 14),
                CreateJoinOneToTwo(context, 11, 13),
                CreateJoinOneToTwo(context, 11, 12),
                CreateJoinOneToTwo(context, 11, 11),
                CreateJoinOneToTwo(context, 11, 10),
                CreateJoinOneToTwo(context, 11, 9),
                CreateJoinOneToTwo(context, 11, 8),
                CreateJoinOneToTwo(context, 11, 7),
                CreateJoinOneToTwo(context, 11, 6),
                CreateJoinOneToTwo(context, 11, 5),
                CreateJoinOneToTwo(context, 11, 4),
                CreateJoinOneToTwo(context, 11, 3),
                CreateJoinOneToTwo(context, 11, 2),
                CreateJoinOneToTwo(context, 11, 1),
                CreateJoinOneToTwo(context, 12, 20),
                CreateJoinOneToTwo(context, 12, 17),
                CreateJoinOneToTwo(context, 12, 14),
                CreateJoinOneToTwo(context, 12, 11),
                CreateJoinOneToTwo(context, 12, 8),
                CreateJoinOneToTwo(context, 12, 5),
                CreateJoinOneToTwo(context, 12, 2),
                CreateJoinOneToTwo(context, 13, 20),
                CreateJoinOneToTwo(context, 13, 16),
                CreateJoinOneToTwo(context, 13, 12),
                CreateJoinOneToTwo(context, 13, 8),
                CreateJoinOneToTwo(context, 13, 4),
                CreateJoinOneToTwo(context, 14, 20),
                CreateJoinOneToTwo(context, 14, 15),
                CreateJoinOneToTwo(context, 14, 10),
                CreateJoinOneToTwo(context, 14, 5),
                CreateJoinOneToTwo(context, 15, 20),
                CreateJoinOneToTwo(context, 15, 14),
                CreateJoinOneToTwo(context, 15, 8),
                CreateJoinOneToTwo(context, 15, 2),
                CreateJoinOneToTwo(context, 16, 20),
                CreateJoinOneToTwo(context, 16, 13),
                CreateJoinOneToTwo(context, 16, 6),
                CreateJoinOneToTwo(context, 17, 20),
                CreateJoinOneToTwo(context, 17, 12),
                CreateJoinOneToTwo(context, 17, 4),
                CreateJoinOneToTwo(context, 18, 20),
                CreateJoinOneToTwo(context, 18, 11),
                CreateJoinOneToTwo(context, 18, 2),
                CreateJoinOneToTwo(context, 19, 20),
                CreateJoinOneToTwo(context, 19, 10)
            };

        private static JoinOneToTwo CreateJoinOneToTwo(
            ManyToManyContext context,
            int oneId,
            int twoId)
            => CreateInstance(
                context?.Set<JoinOneToTwo>(), (e, p) =>
                {
                    e.OneId = oneId;
                    e.TwoId = twoId;
                });

        private static JoinThreeToCompositeKeyFull[] CreateJoinThreeToCompositeKeyFulls(ManyToManyContext context)
            => new[]
            {
                CreateJoinThreeToCompositeKeyFull(context, 1, 6, "6_1", new DateTime(2006, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 2, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 2, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 2, 9, "9_7", new DateTime(2009, 7, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 3, 6, "6_1", new DateTime(2006, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 3, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 3, 9, "9_7", new DateTime(2009, 7, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 5, 8, "8_4", new DateTime(2008, 4, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 5, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 5, 9, "9_5", new DateTime(2009, 5, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 6, 6, "6_1", new DateTime(2006, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 7, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 7, 8, "8_1", new DateTime(2008, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 8, 8, "8_3", new DateTime(2008, 3, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 8, 9, "9_6", new DateTime(2009, 6, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 9, 8, "8_1", new DateTime(2008, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 9, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 10, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 11, 7, "7_1", new DateTime(2007, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 11, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 12, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 12, 8, "8_3", new DateTime(2008, 3, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 12, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 13, 6, "6_1", new DateTime(2006, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 13, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 13, 9, "9_1", new DateTime(2009, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 13, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 14, 8, "8_2", new DateTime(2008, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 14, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 14, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 15, 8, "8_2", new DateTime(2008, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 15, 9, "9_1", new DateTime(2009, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 15, 9, "9_6", new DateTime(2009, 6, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 16, 3, "3_3", new DateTime(2003, 3, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 16, 7, "7_1", new DateTime(2007, 1, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 16, 9, "9_6", new DateTime(2009, 6, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 17, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 17, 8, "8_2", new DateTime(2008, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 18, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 19, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 19, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 19, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 19, 9, "9_7", new DateTime(2009, 7, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 20, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinThreeToCompositeKeyFull(context, 20, 7, "7_1", new DateTime(2007, 1, 1))
            };

        private static JoinThreeToCompositeKeyFull CreateJoinThreeToCompositeKeyFull(
            ManyToManyContext context,
            int threeId,
            int compositeId1,
            string compositeId2,
            DateTime compositeId3)
            => CreateInstance(
                context?.Set<JoinThreeToCompositeKeyFull>(), (e, p) =>
                {
                    e.ThreeId = threeId;
                    e.CompositeId1 = compositeId1;
                    e.CompositeId2 = compositeId2;
                    e.CompositeId3 = compositeId3;
                });

        private static JoinTwoToThree[] CreateJoinTwoToThrees(ManyToManyContext context)
            => new[]
            {
                CreateJoinTwoToThree(context, 1, 2),
                CreateJoinTwoToThree(context, 1, 3),
                CreateJoinTwoToThree(context, 1, 13),
                CreateJoinTwoToThree(context, 1, 18),
                CreateJoinTwoToThree(context, 2, 1),
                CreateJoinTwoToThree(context, 2, 9),
                CreateJoinTwoToThree(context, 2, 15),
                CreateJoinTwoToThree(context, 3, 11),
                CreateJoinTwoToThree(context, 3, 17),
                CreateJoinTwoToThree(context, 4, 2),
                CreateJoinTwoToThree(context, 4, 5),
                CreateJoinTwoToThree(context, 4, 11),
                CreateJoinTwoToThree(context, 5, 4),
                CreateJoinTwoToThree(context, 5, 5),
                CreateJoinTwoToThree(context, 6, 3),
                CreateJoinTwoToThree(context, 6, 10),
                CreateJoinTwoToThree(context, 6, 16),
                CreateJoinTwoToThree(context, 6, 18),
                CreateJoinTwoToThree(context, 7, 12),
                CreateJoinTwoToThree(context, 7, 15),
                CreateJoinTwoToThree(context, 7, 20),
                CreateJoinTwoToThree(context, 8, 1),
                CreateJoinTwoToThree(context, 8, 3),
                CreateJoinTwoToThree(context, 8, 20),
                CreateJoinTwoToThree(context, 9, 3),
                CreateJoinTwoToThree(context, 9, 13),
                CreateJoinTwoToThree(context, 9, 19),
                CreateJoinTwoToThree(context, 10, 17),
                CreateJoinTwoToThree(context, 11, 6),
                CreateJoinTwoToThree(context, 11, 7),
                CreateJoinTwoToThree(context, 11, 8),
                CreateJoinTwoToThree(context, 11, 13),
                CreateJoinTwoToThree(context, 12, 9),
                CreateJoinTwoToThree(context, 13, 1),
                CreateJoinTwoToThree(context, 13, 11),
                CreateJoinTwoToThree(context, 13, 19),
                CreateJoinTwoToThree(context, 14, 2),
                CreateJoinTwoToThree(context, 15, 17),
                CreateJoinTwoToThree(context, 16, 3),
                CreateJoinTwoToThree(context, 16, 16),
                CreateJoinTwoToThree(context, 18, 1),
                CreateJoinTwoToThree(context, 18, 5),
                CreateJoinTwoToThree(context, 18, 10),
                CreateJoinTwoToThree(context, 19, 5),
                CreateJoinTwoToThree(context, 19, 16),
                CreateJoinTwoToThree(context, 19, 18),
                CreateJoinTwoToThree(context, 20, 6),
                CreateJoinTwoToThree(context, 20, 10),
                CreateJoinTwoToThree(context, 20, 12),
                CreateJoinTwoToThree(context, 20, 16),
                CreateJoinTwoToThree(context, 20, 17),
                CreateJoinTwoToThree(context, 20, 18)
            };

        private static JoinTwoToThree CreateJoinTwoToThree(
            ManyToManyContext context,
            int twoId,
            int threeId)
            => CreateInstance(
                context?.Set<JoinTwoToThree>(), (e, p) =>
                {
                    e.TwoId = twoId;
                    e.ThreeId = threeId;
                });

        private static Dictionary<string, object>[] CreateEntityOneEntityTwos(ManyToManyContext context)
            => new[]
            {
                CreateEntityOneEntityTwo(context, 1, 3),
                CreateEntityOneEntityTwo(context, 1, 16),
                CreateEntityOneEntityTwo(context, 2, 3),
                CreateEntityOneEntityTwo(context, 2, 10),
                CreateEntityOneEntityTwo(context, 2, 18),
                CreateEntityOneEntityTwo(context, 3, 10),
                CreateEntityOneEntityTwo(context, 3, 11),
                CreateEntityOneEntityTwo(context, 3, 16),
                CreateEntityOneEntityTwo(context, 5, 2),
                CreateEntityOneEntityTwo(context, 5, 5),
                CreateEntityOneEntityTwo(context, 5, 7),
                CreateEntityOneEntityTwo(context, 5, 9),
                CreateEntityOneEntityTwo(context, 5, 14),
                CreateEntityOneEntityTwo(context, 6, 12),
                CreateEntityOneEntityTwo(context, 7, 3),
                CreateEntityOneEntityTwo(context, 7, 16),
                CreateEntityOneEntityTwo(context, 7, 17),
                CreateEntityOneEntityTwo(context, 8, 19),
                CreateEntityOneEntityTwo(context, 9, 9),
                CreateEntityOneEntityTwo(context, 9, 11),
                CreateEntityOneEntityTwo(context, 10, 6),
                CreateEntityOneEntityTwo(context, 10, 17),
                CreateEntityOneEntityTwo(context, 10, 20),
                CreateEntityOneEntityTwo(context, 11, 17),
                CreateEntityOneEntityTwo(context, 11, 18),
                CreateEntityOneEntityTwo(context, 12, 6),
                CreateEntityOneEntityTwo(context, 12, 19),
                CreateEntityOneEntityTwo(context, 13, 7),
                CreateEntityOneEntityTwo(context, 13, 8),
                CreateEntityOneEntityTwo(context, 13, 9),
                CreateEntityOneEntityTwo(context, 13, 13),
                CreateEntityOneEntityTwo(context, 14, 4),
                CreateEntityOneEntityTwo(context, 14, 9),
                CreateEntityOneEntityTwo(context, 14, 19),
                CreateEntityOneEntityTwo(context, 15, 10),
                CreateEntityOneEntityTwo(context, 16, 1),
                CreateEntityOneEntityTwo(context, 16, 7),
                CreateEntityOneEntityTwo(context, 16, 19),
                CreateEntityOneEntityTwo(context, 17, 8),
                CreateEntityOneEntityTwo(context, 17, 15),
                CreateEntityOneEntityTwo(context, 18, 4),
                CreateEntityOneEntityTwo(context, 18, 13),
                CreateEntityOneEntityTwo(context, 18, 14),
                CreateEntityOneEntityTwo(context, 19, 4),
                CreateEntityOneEntityTwo(context, 19, 14)
            };

        private static Dictionary<string, object> CreateEntityOneEntityTwo(
            ManyToManyContext context,
            int oneId,
            int twoId)
            => CreateInstance(
                context?.Set<Dictionary<string, object>>("EntityOneEntityTwo"), (e, p) =>
                {
                    e["EntityOneId"] = oneId;
                    e["EntityTwoId"] = twoId;
                });

        private static Dictionary<string, object>[] CreateJoinOneToThreePayloadFullShareds(ManyToManyContext context)
            => new[]
            {
                CreateJoinOneToThreePayloadFullShared(context, 3, 1, "Capbrough"),
                CreateJoinOneToThreePayloadFullShared(context, 3, 2, "East Eastdol"),
                CreateJoinOneToThreePayloadFullShared(context, 3, 4, "Southingville"),
                CreateJoinOneToThreePayloadFullShared(context, 3, 9, "Goldbrough"),
                CreateJoinOneToThreePayloadFullShared(context, 4, 5, "Readingworth"),
                CreateJoinOneToThreePayloadFullShared(context, 4, 18, "Skillpool"),
                CreateJoinOneToThreePayloadFullShared(context, 5, 1, "Lawgrad"),
                CreateJoinOneToThreePayloadFullShared(context, 5, 4, "Kettleham Park"),
                CreateJoinOneToThreePayloadFullShared(context, 5, 9, "Sayford Park"),
                CreateJoinOneToThreePayloadFullShared(context, 5, 16, "Hamstead"),
                CreateJoinOneToThreePayloadFullShared(context, 6, 11, "North Starside"),
                CreateJoinOneToThreePayloadFullShared(context, 6, 13, "Goldfolk"),
                CreateJoinOneToThreePayloadFullShared(context, 7, 4, "Winstead"),
                CreateJoinOneToThreePayloadFullShared(context, 8, 11, "Transworth"),
                CreateJoinOneToThreePayloadFullShared(context, 8, 18, "Parkpool"),
                CreateJoinOneToThreePayloadFullShared(context, 8, 19, "Fishham"),
                CreateJoinOneToThreePayloadFullShared(context, 10, 1, "Passmouth"),
                CreateJoinOneToThreePayloadFullShared(context, 10, 5, "Valenfield"),
                CreateJoinOneToThreePayloadFullShared(context, 10, 20, "Passford Park"),
                CreateJoinOneToThreePayloadFullShared(context, 11, 10, "Chatfield"),
                CreateJoinOneToThreePayloadFullShared(context, 12, 11, "Hosview"),
                CreateJoinOneToThreePayloadFullShared(context, 12, 17, "Dodgewich"),
                CreateJoinOneToThreePayloadFullShared(context, 13, 3, "Skillhampton"),
                CreateJoinOneToThreePayloadFullShared(context, 13, 14, "Hardcaster"),
                CreateJoinOneToThreePayloadFullShared(context, 13, 16, "Hollowmouth"),
                CreateJoinOneToThreePayloadFullShared(context, 14, 6, "Cruxcaster"),
                CreateJoinOneToThreePayloadFullShared(context, 14, 11, "Elcaster"),
                CreateJoinOneToThreePayloadFullShared(context, 14, 17, "Clambrough"),
                CreateJoinOneToThreePayloadFullShared(context, 15, 10, "Millwich"),
                CreateJoinOneToThreePayloadFullShared(context, 15, 13, "Hapcester"),
                CreateJoinOneToThreePayloadFullShared(context, 16, 7, "Sanddol Beach"),
                CreateJoinOneToThreePayloadFullShared(context, 16, 13, "Hamcaster"),
                CreateJoinOneToThreePayloadFullShared(context, 17, 9, "New Foxbrough"),
                CreateJoinOneToThreePayloadFullShared(context, 17, 13, "Chatpool"),
                CreateJoinOneToThreePayloadFullShared(context, 18, 8, "Duckworth"),
                CreateJoinOneToThreePayloadFullShared(context, 18, 12, "Snowham"),
                CreateJoinOneToThreePayloadFullShared(context, 18, 13, "Bannview Island"),
                CreateJoinOneToThreePayloadFullShared(context, 20, 4, "Rockbrough"),
                CreateJoinOneToThreePayloadFullShared(context, 20, 5, "Sweetfield"),
                CreateJoinOneToThreePayloadFullShared(context, 20, 16, "Bayburgh Hills")
            };

        private static Dictionary<string, object> CreateJoinOneToThreePayloadFullShared(
            ManyToManyContext context,
            int oneId,
            int threeId,
            string payload)
            => CreateInstance(
                context?.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared"), (e, p) =>
                {
                    e["OneId"] = oneId;
                    e["ThreeId"] = threeId;
                    e["Payload"] = payload;
                });

        private static Dictionary<string, object>[] CreateJoinTwoSelfShareds(ManyToManyContext context)
            => new[]
            {
                CreateJoinTwoSelfShared(context, 1, 9),
                CreateJoinTwoSelfShared(context, 1, 10),
                CreateJoinTwoSelfShared(context, 1, 11),
                CreateJoinTwoSelfShared(context, 1, 18),
                CreateJoinTwoSelfShared(context, 3, 2),
                CreateJoinTwoSelfShared(context, 3, 5),
                CreateJoinTwoSelfShared(context, 3, 8),
                CreateJoinTwoSelfShared(context, 3, 18),
                CreateJoinTwoSelfShared(context, 3, 19),
                CreateJoinTwoSelfShared(context, 4, 11),
                CreateJoinTwoSelfShared(context, 5, 8),
                CreateJoinTwoSelfShared(context, 6, 18),
                CreateJoinTwoSelfShared(context, 8, 2),
                CreateJoinTwoSelfShared(context, 8, 14),
                CreateJoinTwoSelfShared(context, 8, 15),
                CreateJoinTwoSelfShared(context, 8, 20),
                CreateJoinTwoSelfShared(context, 9, 4),
                CreateJoinTwoSelfShared(context, 9, 14),
                CreateJoinTwoSelfShared(context, 10, 5),
                CreateJoinTwoSelfShared(context, 12, 13),
                CreateJoinTwoSelfShared(context, 12, 14),
                CreateJoinTwoSelfShared(context, 13, 14),
                CreateJoinTwoSelfShared(context, 13, 18),
                CreateJoinTwoSelfShared(context, 13, 19),
                CreateJoinTwoSelfShared(context, 16, 6),
                CreateJoinTwoSelfShared(context, 17, 9),
                CreateJoinTwoSelfShared(context, 17, 19),
                CreateJoinTwoSelfShared(context, 17, 20),
                CreateJoinTwoSelfShared(context, 18, 2),
                CreateJoinTwoSelfShared(context, 18, 5),
                CreateJoinTwoSelfShared(context, 18, 16),
                CreateJoinTwoSelfShared(context, 18, 17),
                CreateJoinTwoSelfShared(context, 19, 2),
                CreateJoinTwoSelfShared(context, 20, 4)
            };

        private static Dictionary<string, object> CreateJoinTwoSelfShared(
            ManyToManyContext context,
            int leftId,
            int rightId)
            => CreateInstance(
                context?.Set<Dictionary<string, object>>("JoinTwoSelfShared"), (e, p) =>
                {
                    e["LeftId"] = leftId;
                    e["RightId"] = rightId;
                });

        private static Dictionary<string, object>[] CreateJoinTwoToCompositeKeyShareds(ManyToManyContext context)
            => new[]
            {
                CreateJoinTwoToCompositeKeyShared(context, 1, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 1, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 1, 3, "3_3", new DateTime(2003, 3, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 2, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 3, 6, "6_1", new DateTime(2006, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 4, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 4, 9, "9_6", new DateTime(2009, 6, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 6, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 6, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 7, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 9, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 9, 8, "8_1", new DateTime(2008, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 10, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 10, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 10, 9, "9_5", new DateTime(2009, 5, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 11, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 11, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 12, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 12, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 12, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 13, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 13, 7, "7_1", new DateTime(2007, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 13, 9, "9_4", new DateTime(2009, 4, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 15, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 16, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 16, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 16, 9, "9_6", new DateTime(2009, 6, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 17, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 17, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 17, 9, "9_1", new DateTime(2009, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 17, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 19, 3, "3_3", new DateTime(2003, 3, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 20, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 20, 3, "3_3", new DateTime(2003, 3, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 20, 6, "6_1", new DateTime(2006, 1, 1)),
                CreateJoinTwoToCompositeKeyShared(context, 20, 9, "9_1", new DateTime(2009, 1, 1))
            };

        private static Dictionary<string, object> CreateJoinTwoToCompositeKeyShared(
            ManyToManyContext context,
            int twoId,
            int compositeId1,
            string compositeId2,
            DateTime compositeId3)
            => CreateInstance(
                context?.Set<Dictionary<string, object>>("JoinTwoToCompositeKeyShared"), (e, p) =>
                {
                    e["TwoId"] = twoId;
                    e["CompositeId1"] = compositeId1;
                    e["CompositeId2"] = compositeId2;
                    e["CompositeId3"] = compositeId3;
                });

        private static Dictionary<string, object>[] CreateEntityRootEntityThrees(ManyToManyContext context)
            => new[]
            {
                CreateEntityRootEntityThree(context, 1, 7),
                CreateEntityRootEntityThree(context, 1, 8),
                CreateEntityRootEntityThree(context, 1, 15),
                CreateEntityRootEntityThree(context, 2, 4),
                CreateEntityRootEntityThree(context, 2, 16),
                CreateEntityRootEntityThree(context, 3, 12),
                CreateEntityRootEntityThree(context, 3, 14),
                CreateEntityRootEntityThree(context, 3, 24),
                CreateEntityRootEntityThree(context, 5, 14),
                CreateEntityRootEntityThree(context, 5, 15),
                CreateEntityRootEntityThree(context, 5, 16),
                CreateEntityRootEntityThree(context, 6, 21),
                CreateEntityRootEntityThree(context, 7, 1),
                CreateEntityRootEntityThree(context, 7, 6),
                CreateEntityRootEntityThree(context, 7, 13),
                CreateEntityRootEntityThree(context, 7, 24),
                CreateEntityRootEntityThree(context, 8, 10),
                CreateEntityRootEntityThree(context, 10, 3),
                CreateEntityRootEntityThree(context, 10, 8),
                CreateEntityRootEntityThree(context, 13, 5),
                CreateEntityRootEntityThree(context, 14, 1),
                CreateEntityRootEntityThree(context, 14, 14),
                CreateEntityRootEntityThree(context, 16, 5),
                CreateEntityRootEntityThree(context, 16, 7),
                CreateEntityRootEntityThree(context, 17, 14),
                CreateEntityRootEntityThree(context, 18, 6),
                CreateEntityRootEntityThree(context, 18, 23),
                CreateEntityRootEntityThree(context, 19, 11),
                CreateEntityRootEntityThree(context, 20, 14)
            };

        private static Dictionary<string, object> CreateEntityRootEntityThree(
            ManyToManyContext context,
            int threeId,
            int rootId)
            => CreateInstance(
                context?.Set<Dictionary<string, object>>("EntityRootEntityThree"), (e, p) =>
                {
                    e["EntityThreeId"] = threeId;
                    e["EntityRootId"] = rootId;
                });

        private static Dictionary<string, object>[] CreateJoinCompositeKeyToRootShareds(ManyToManyContext context)
            => new[]
            {
                CreateJoinCompositeKeyToRootShared(context, 6, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 9, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 24, 1, "1_1", new DateTime(2001, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 1, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 2, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 4, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 6, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 11, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 22, 1, "1_2", new DateTime(2001, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 4, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 14, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 16, 3, "3_1", new DateTime(2003, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 2, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 3, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 4, 3, "3_2", new DateTime(2003, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 2, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 8, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 16, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 22, 7, "7_2", new DateTime(2007, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 7, 8, "8_1", new DateTime(2008, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 8, 8, "8_1", new DateTime(2008, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 23, 8, "8_1", new DateTime(2008, 1, 1)),
                CreateJoinCompositeKeyToRootShared(context, 3, 8, "8_2", new DateTime(2008, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 12, 8, "8_2", new DateTime(2008, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 22, 8, "8_2", new DateTime(2008, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 2, 8, "8_3", new DateTime(2008, 3, 1)),
                CreateJoinCompositeKeyToRootShared(context, 4, 8, "8_3", new DateTime(2008, 3, 1)),
                CreateJoinCompositeKeyToRootShared(context, 5, 8, "8_3", new DateTime(2008, 3, 1)),
                CreateJoinCompositeKeyToRootShared(context, 7, 8, "8_4", new DateTime(2008, 4, 1)),
                CreateJoinCompositeKeyToRootShared(context, 3, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinCompositeKeyToRootShared(context, 8, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinCompositeKeyToRootShared(context, 14, 8, "8_5", new DateTime(2008, 5, 1)),
                CreateJoinCompositeKeyToRootShared(context, 4, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 11, 9, "9_2", new DateTime(2009, 2, 1)),
                CreateJoinCompositeKeyToRootShared(context, 1, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinCompositeKeyToRootShared(context, 7, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinCompositeKeyToRootShared(context, 15, 9, "9_3", new DateTime(2009, 3, 1)),
                CreateJoinCompositeKeyToRootShared(context, 1, 9, "9_6", new DateTime(2009, 6, 1)),
                CreateJoinCompositeKeyToRootShared(context, 6, 9, "9_7", new DateTime(2009, 7, 1))
            };

        private static ICollection<TEntity> CreateCollection<TEntity>(bool proxy)
            => proxy ? (ICollection<TEntity>)new ObservableCollection<TEntity>() : new List<TEntity>();

        private static Dictionary<string, object> CreateJoinCompositeKeyToRootShared(
            ManyToManyContext context,
            int rootId,
            int compositeId1,
            string compositeId2,
            DateTime compositeId3)
            => CreateInstance(
                context?.Set<Dictionary<string, object>>("JoinCompositeKeyToRootShared"), (e, p) =>
                {
                    e["RootId"] = rootId;
                    e["CompositeId1"] = compositeId1;
                    e["CompositeId2"] = compositeId2;
                    e["CompositeId3"] = compositeId3;
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
}
