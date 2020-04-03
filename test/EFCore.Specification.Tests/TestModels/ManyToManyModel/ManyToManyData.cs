// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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

        private readonly JoinOneToTwoShared[] _joinOneToTwoShareds;
        private readonly JoinOneToThreePayloadFullShared[] _joinOneToThreePayloadFullShareds;
        private readonly JoinTwoSelfShared[] _joinTwoSelfShareds;
        private readonly JoinTwoToCompositeKeyShared[] _joinTwoToCompositeKeyShareds;
        private readonly JoinThreeToRootShared[] _joinThreeToRootShareds;
        private readonly JoinCompositeKeyToRootShared[] _joinCompositeKeyToRootShareds;

        public ManyToManyData()
        {
            _ones = CreateOnes();
            _twos = CreateTwos();
            _threes = CreateThrees();
            _compositeKeys = CreateCompositeKeys();
            _roots = CreateRoots();

            _joinCompositeKeyToLeaves = CreateJoinCompositeKeyToLeaves();
            _joinOneSelfPayloads = CreateJoinOneSelfPayloads();
            _joinOneToBranches = CreateJoinOneToBranches();
            _joinOneToThreePayloadFulls = CreateJoinOneToThreePayloadFulls();
            _joinOneToTwos = CreateJoinOneToTwos();
            _joinThreeToCompositeKeyFulls = CreateJoinThreeToCompositeKeyFulls();
            _joinTwoToThrees = CreateJoinTwoToThrees();

            _joinOneToTwoShareds = CreateJoinOneToTwoShareds();
            _joinOneToThreePayloadFullShareds = CreateJoinOneToThreePayloadFullShareds();
            _joinTwoSelfShareds = CreateJoinTwoSelfShareds();
            _joinTwoToCompositeKeyShareds = CreateJoinTwoToCompositeKeyShareds();
            _joinThreeToRootShareds = CreateJoinThreeToRootShareds();
            _joinCompositeKeyToRootShareds = CreateJoinCompositeKeyToRootShareds();

            foreach (var basicOne in _ones)
            {
                basicOne.Collection = new List<EntityTwo>();
                basicOne.TwoSkip = new List<EntityTwo>();
                basicOne.ThreeSkipPayloadFull = new List<EntityThree>();
                basicOne.JoinThreePayloadFull = new List<JoinOneToThreePayloadFull>();
                basicOne.TwoSkipShared = new List<EntityTwo>();
                basicOne.ThreeSkipPayloadFullShared = new List<EntityThree>();
                basicOne.JoinThreePayloadFullShared = new List<JoinOneToThreePayloadFullShared>();
                basicOne.SelfSkipPayloadLeft = new List<EntityOne>();
                basicOne.JoinSelfPayloadLeft = new List<JoinOneSelfPayload>();
                basicOne.SelfSkipPayloadRight = new List<EntityOne>();
                basicOne.JoinSelfPayloadRight = new List<JoinOneSelfPayload>();
                basicOne.BranchSkip = new List<EntityBranch>();
            }

            foreach (var basicTwo in _twos)
            {
                basicTwo.Collection = new List<EntityThree>();
                basicTwo.OneSkip = new List<EntityOne>();
                basicTwo.ThreeSkipFull = new List<EntityThree>();
                basicTwo.JoinThreeFull = new List<JoinTwoToThree>();
                basicTwo.OneSkipShared = new List<EntityOne>();
                basicTwo.SelfSkipSharedLeft = new List<EntityTwo>();
                basicTwo.SelfSkipSharedRight = new List<EntityTwo>();
                basicTwo.CompositeKeySkipShared = new List<EntityCompositeKey>();

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
                basicThree.OneSkipPayloadFull = new List<EntityOne>();
                basicThree.JoinOnePayloadFull = new List<JoinOneToThreePayloadFull>();
                basicThree.TwoSkipFull = new List<EntityTwo>();
                basicThree.JoinTwoFull = new List<JoinTwoToThree>();
                basicThree.OneSkipPayloadFullShared = new List<EntityOne>();
                basicThree.JoinOnePayloadFullShared = new List<JoinOneToThreePayloadFullShared>();
                basicThree.CompositeKeySkipFull = new List<EntityCompositeKey>();
                basicThree.JoinCompositeKeyFull = new List<JoinThreeToCompositeKeyFull>();
                basicThree.RootSkipShared = new List<EntityRoot>();

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

            foreach (var compositeKey in _compositeKeys)
            {
                compositeKey.TwoSkipShared = new List<EntityTwo>();
                compositeKey.ThreeSkipFull = new List<EntityThree>();
                compositeKey.JoinThreeFull = new List<JoinThreeToCompositeKeyFull>();
                compositeKey.RootSkipShared = new List<EntityRoot>();
                compositeKey.LeafSkipFull = new List<EntityLeaf>();
                compositeKey.JoinLeafFull = new List<JoinCompositeKeyToLeaf>();
            }

            foreach (var root in _roots)
            {
                root.ThreeSkipShared = new List<EntityThree>();
                root.CompositeKeySkipShared = new List<EntityCompositeKey>();

                if (root is EntityBranch branch)
                {
                    branch.OneSkip = new List<EntityOne>();
                }

                if (root is EntityLeaf leaf)
                {
                    leaf.CompositeKeySkipFull = new List<EntityCompositeKey>();
                    leaf.JoinCompositeKeyFull = new List<JoinCompositeKeyToLeaf>();
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
                var one = _ones.First(o => o.Id == joinEntity.OneId);
                var branch = _roots.OfType<EntityBranch>().First(t => t.Id == joinEntity.BranchId);
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
                var compositeKey = _compositeKeys.First(o => o.Key1 == joinEntity.CompositeId1
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
                var compositeKey = _compositeKeys.First(o => o.Key1 == joinEntity.CompositeId1
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
                var one = _ones.First(o => o.Id == joinEntity.OneId);
                var two = _twos.First(t => t.Id == joinEntity.TwoId);
                one.TwoSkipShared.Add(two);
                two.OneSkipShared.Add(one);
            }

            foreach (var joinEntity in _joinOneToThreePayloadFullShareds)
            {
                var one = _ones.First(o => o.Id == joinEntity.OneId);
                var three = _threes.First(t => t.Id == joinEntity.ThreeId);
                one.ThreeSkipPayloadFullShared.Add(three);
                one.JoinThreePayloadFullShared.Add(joinEntity);
                three.OneSkipPayloadFullShared.Add(one);
                three.JoinOnePayloadFullShared.Add(joinEntity);
            }

            foreach (var joinEntity in _joinTwoSelfShareds)
            {
                var left = _twos.First(o => o.Id == joinEntity.LeftId);
                var right = _twos.First(t => t.Id == joinEntity.RightId);
                left.SelfSkipSharedRight.Add(right);
                right.SelfSkipSharedLeft.Add(left);
            }

            foreach (var joinEntity in _joinTwoToCompositeKeyShareds)
            {
                var compositeKey = _compositeKeys.First(o => o.Key1 == joinEntity.CompositeId1
                    && o.Key2 == joinEntity.CompositeId2
                    && o.Key3 == joinEntity.CompositeId3);
                var two = _twos.First(t => t.Id == joinEntity.TwoId);
                compositeKey.TwoSkipShared.Add(two);
                two.CompositeKeySkipShared.Add(compositeKey);
            }

            foreach (var joinEntity in _joinThreeToRootShareds)
            {
                var three = _threes.First(o => o.Id == joinEntity.ThreeId);
                var root = _roots.First(t => t.Id == joinEntity.RootId);
                three.RootSkipShared.Add(root);
                root.ThreeSkipShared.Add(three);
            }

            foreach (var joinEntity in _joinCompositeKeyToRootShareds)
            {
                var compositeKey = _compositeKeys.First(o => o.Key1 == joinEntity.CompositeId1
                    && o.Key2 == joinEntity.CompositeId2
                    && o.Key3 == joinEntity.CompositeId3);
                var root = _roots.First(t => t.Id == joinEntity.RootId);
                compositeKey.RootSkipShared.Add(root);
                root.CompositeKeySkipShared.Add(compositeKey);
            }
        }

        public IQueryable<TEntity> Set<TEntity>() where TEntity : class
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
            context.Set<EntityOne>().AddRange(CreateOnes());
            context.Set<EntityTwo>().AddRange(CreateTwos());
            context.Set<EntityThree>().AddRange(CreateThrees());
            context.Set<EntityCompositeKey>().AddRange(CreateCompositeKeys());
            context.Set<EntityRoot>().AddRange(CreateRoots());

            context.Set<JoinCompositeKeyToLeaf>().AddRange(CreateJoinCompositeKeyToLeaves());
            context.Set<JoinOneSelfPayload>().AddRange(CreateJoinOneSelfPayloads());
            context.Set<JoinOneToBranch>().AddRange(CreateJoinOneToBranches());
            context.Set<JoinOneToThreePayloadFull>().AddRange(CreateJoinOneToThreePayloadFulls());
            context.Set<JoinOneToTwo>().AddRange(CreateJoinOneToTwos());
            context.Set<JoinThreeToCompositeKeyFull>().AddRange(CreateJoinThreeToCompositeKeyFulls());
            context.Set<JoinTwoToThree>().AddRange(CreateJoinTwoToThrees());

            context.Set<JoinOneToTwoShared>().AddRange(CreateJoinOneToTwoShareds());
            context.Set<JoinOneToThreePayloadFullShared>().AddRange(CreateJoinOneToThreePayloadFullShareds());
            context.Set<JoinTwoSelfShared>().AddRange(CreateJoinTwoSelfShareds());
            context.Set<JoinTwoToCompositeKeyShared>().AddRange(CreateJoinTwoToCompositeKeyShareds());
            context.Set<JoinThreeToRootShared>().AddRange(CreateJoinThreeToRootShareds());
            context.Set<JoinCompositeKeyToRootShared>().AddRange(CreateJoinCompositeKeyToRootShareds());

            context.SaveChanges();
        }

        private static EntityOne[] CreateOnes()
            => new[]
            {
                new EntityOne { Id = 1, Name = "EntityOne 1" },
                new EntityOne { Id = 2, Name = "EntityOne 2" },
                new EntityOne { Id = 3, Name = "EntityOne 3" },
                new EntityOne { Id = 4, Name = "EntityOne 4" },
                new EntityOne { Id = 5, Name = "EntityOne 5" },
                new EntityOne { Id = 6, Name = "EntityOne 6" },
                new EntityOne { Id = 7, Name = "EntityOne 7" },
                new EntityOne { Id = 8, Name = "EntityOne 8" },
                new EntityOne { Id = 9, Name = "EntityOne 9" },
                new EntityOne { Id = 10, Name = "EntityOne 10" },
                new EntityOne { Id = 11, Name = "EntityOne 11" },
                new EntityOne { Id = 12, Name = "EntityOne 12" },
                new EntityOne { Id = 13, Name = "EntityOne 13" },
                new EntityOne { Id = 14, Name = "EntityOne 14" },
                new EntityOne { Id = 15, Name = "EntityOne 15" },
                new EntityOne { Id = 16, Name = "EntityOne 16" },
                new EntityOne { Id = 17, Name = "EntityOne 17" },
                new EntityOne { Id = 18, Name = "EntityOne 18" },
                new EntityOne { Id = 19, Name = "EntityOne 19" },
                new EntityOne { Id = 20, Name = "EntityOne 20" },
            };
        private static EntityTwo[] CreateTwos()
            => new[]
            {
                new EntityTwo { Id = 1, Name = "EntityTwo 1", ReferenceInverseId = null, CollectionInverseId = 1 },
                new EntityTwo { Id = 2, Name = "EntityTwo 2", ReferenceInverseId = null, CollectionInverseId = 1 },
                new EntityTwo { Id = 3, Name = "EntityTwo 3", ReferenceInverseId = null, CollectionInverseId = null },
                new EntityTwo { Id = 4, Name = "EntityTwo 4", ReferenceInverseId = null, CollectionInverseId = 3 },
                new EntityTwo { Id = 5, Name = "EntityTwo 5", ReferenceInverseId = null, CollectionInverseId = 3 },
                new EntityTwo { Id = 6, Name = "EntityTwo 6", ReferenceInverseId = null, CollectionInverseId = 5 },
                new EntityTwo { Id = 7, Name = "EntityTwo 7", ReferenceInverseId = null, CollectionInverseId = 5 },
                new EntityTwo { Id = 8, Name = "EntityTwo 8", ReferenceInverseId = null, CollectionInverseId = 7 },
                new EntityTwo { Id = 9, Name = "EntityTwo 9", ReferenceInverseId = null, CollectionInverseId = 7 },
                new EntityTwo { Id = 10, Name = "EntityTwo 10", ReferenceInverseId = 20, CollectionInverseId = 9 },
                new EntityTwo { Id = 11, Name = "EntityTwo 11", ReferenceInverseId = 18, CollectionInverseId = 9 },
                new EntityTwo { Id = 12, Name = "EntityTwo 12", ReferenceInverseId = 16, CollectionInverseId = 11 },
                new EntityTwo { Id = 13, Name = "EntityTwo 13", ReferenceInverseId = 14, CollectionInverseId = 11 },
                new EntityTwo { Id = 14, Name = "EntityTwo 14", ReferenceInverseId = 12, CollectionInverseId = 13 },
                new EntityTwo { Id = 15, Name = "EntityTwo 15", ReferenceInverseId = 11, CollectionInverseId = 13 },
                new EntityTwo { Id = 16, Name = "EntityTwo 16", ReferenceInverseId = 9, CollectionInverseId = 15 },
                new EntityTwo { Id = 17, Name = "EntityTwo 17", ReferenceInverseId = 7, CollectionInverseId = 15 },
                new EntityTwo { Id = 18, Name = "EntityTwo 18", ReferenceInverseId = 5, CollectionInverseId = 16 },
                new EntityTwo { Id = 19, Name = "EntityTwo 19", ReferenceInverseId = 3, CollectionInverseId = 16 },
                new EntityTwo { Id = 20, Name = "EntityTwo 20", ReferenceInverseId = 1, CollectionInverseId = 17 },
            };
        private static EntityThree[] CreateThrees()
            => new[]
            {
                new EntityThree { Id = 1, Name = "EntityThree 1", ReferenceInverseId = null, CollectionInverseId = null },
                new EntityThree { Id = 2, Name = "EntityThree 2", ReferenceInverseId = 19, CollectionInverseId = 17 },
                new EntityThree { Id = 3, Name = "EntityThree 3", ReferenceInverseId = 2, CollectionInverseId = 16 },
                new EntityThree { Id = 4, Name = "EntityThree 4", ReferenceInverseId = 20, CollectionInverseId = 16 },
                new EntityThree { Id = 5, Name = "EntityThree 5", ReferenceInverseId = 4, CollectionInverseId = 15 },
                new EntityThree { Id = 6, Name = "EntityThree 6", ReferenceInverseId = null, CollectionInverseId = 15 },
                new EntityThree { Id = 7, Name = "EntityThree 7", ReferenceInverseId = 6, CollectionInverseId = 13 },
                new EntityThree { Id = 8, Name = "EntityThree 8", ReferenceInverseId = null, CollectionInverseId = 13 },
                new EntityThree { Id = 9, Name = "EntityThree 9", ReferenceInverseId = 8, CollectionInverseId = 11 },
                new EntityThree { Id = 10, Name = "EntityThree 10", ReferenceInverseId = null, CollectionInverseId = 11 },
                new EntityThree { Id = 11, Name = "EntityThree 11", ReferenceInverseId = 10, CollectionInverseId = 9 },
                new EntityThree { Id = 12, Name = "EntityThree 12", ReferenceInverseId = null, CollectionInverseId = 9 },
                new EntityThree { Id = 13, Name = "EntityThree 13", ReferenceInverseId = 12, CollectionInverseId = 7 },
                new EntityThree { Id = 14, Name = "EntityThree 14", ReferenceInverseId = null, CollectionInverseId = 7 },
                new EntityThree { Id = 15, Name = "EntityThree 15", ReferenceInverseId = 14, CollectionInverseId = 5 },
                new EntityThree { Id = 16, Name = "EntityThree 16", ReferenceInverseId = null, CollectionInverseId = 5 },
                new EntityThree { Id = 17, Name = "EntityThree 17", ReferenceInverseId = 16, CollectionInverseId = 3 },
                new EntityThree { Id = 18, Name = "EntityThree 18", ReferenceInverseId = null, CollectionInverseId = 3 },
                new EntityThree { Id = 19, Name = "EntityThree 19", ReferenceInverseId = 18, CollectionInverseId = 1 },
                new EntityThree { Id = 20, Name = "EntityThree 20", ReferenceInverseId = null, CollectionInverseId = 1 },
            };
        private static EntityCompositeKey[] CreateCompositeKeys()
            => new[]
            {
                new EntityCompositeKey { Key1 = 1, Key2 = "1_1", Key3 = new DateTime(2001, 1, 1), Name = "Composite 1" },
                new EntityCompositeKey { Key1 = 1, Key2 = "1_2", Key3 = new DateTime(2001, 2, 1), Name = "Composite 2" },
                new EntityCompositeKey { Key1 = 3, Key2 = "3_1", Key3 = new DateTime(2003, 1, 1), Name = "Composite 3" },
                new EntityCompositeKey { Key1 = 3, Key2 = "3_2", Key3 = new DateTime(2003, 2, 1), Name = "Composite 4" },
                new EntityCompositeKey { Key1 = 3, Key2 = "3_3", Key3 = new DateTime(2003, 3, 1), Name = "Composite 5" },
                new EntityCompositeKey { Key1 = 6, Key2 = "6_1", Key3 = new DateTime(2006, 1, 1), Name = "Composite 6" },
                new EntityCompositeKey { Key1 = 7, Key2 = "7_1", Key3 = new DateTime(2007, 1, 1), Name = "Composite 7" },
                new EntityCompositeKey { Key1 = 7, Key2 = "7_2", Key3 = new DateTime(2007, 2, 1), Name = "Composite 8" },
                new EntityCompositeKey { Key1 = 8, Key2 = "8_1", Key3 = new DateTime(2008, 1, 1), Name = "Composite 9" },
                new EntityCompositeKey { Key1 = 8, Key2 = "8_2", Key3 = new DateTime(2008, 2, 1), Name = "Composite 10" },
                new EntityCompositeKey { Key1 = 8, Key2 = "8_3", Key3 = new DateTime(2008, 3, 1), Name = "Composite 11" },
                new EntityCompositeKey { Key1 = 8, Key2 = "8_4", Key3 = new DateTime(2008, 4, 1), Name = "Composite 12" },
                new EntityCompositeKey { Key1 = 8, Key2 = "8_5", Key3 = new DateTime(2008, 5, 1), Name = "Composite 13" },
                new EntityCompositeKey { Key1 = 9, Key2 = "9_1", Key3 = new DateTime(2009, 1, 1), Name = "Composite 14" },
                new EntityCompositeKey { Key1 = 9, Key2 = "9_2", Key3 = new DateTime(2009, 2, 1), Name = "Composite 15" },
                new EntityCompositeKey { Key1 = 9, Key2 = "9_3", Key3 = new DateTime(2009, 3, 1), Name = "Composite 16" },
                new EntityCompositeKey { Key1 = 9, Key2 = "9_4", Key3 = new DateTime(2009, 4, 1), Name = "Composite 17" },
                new EntityCompositeKey { Key1 = 9, Key2 = "9_5", Key3 = new DateTime(2009, 5, 1), Name = "Composite 18" },
                new EntityCompositeKey { Key1 = 9, Key2 = "9_6", Key3 = new DateTime(2009, 6, 1), Name = "Composite 19" },
                new EntityCompositeKey { Key1 = 9, Key2 = "9_7", Key3 = new DateTime(2009, 7, 1), Name = "Composite 20" },
            };
        private static EntityRoot[] CreateRoots()
            => new[]
            {
                new EntityRoot { Id = 1, Name = "Root 1" },
                new EntityRoot { Id = 2, Name = "Root 2" },
                new EntityRoot { Id = 3, Name = "Root 3" },
                new EntityRoot { Id = 4, Name = "Root 4" },
                new EntityRoot { Id = 5, Name = "Root 5" },
                new EntityRoot { Id = 6, Name = "Root 6" },
                new EntityRoot { Id = 7, Name = "Root 7" },
                new EntityRoot { Id = 8, Name = "Root 8" },
                new EntityRoot { Id = 9, Name = "Root 9" },
                new EntityRoot { Id = 10, Name = "Root 10" },
                new EntityBranch { Id = 11, Name = "Branch 1", Number = 7 },
                new EntityBranch { Id = 12, Name = "Branch 2", Number = 77 },
                new EntityBranch { Id = 13, Name = "Branch 3", Number = 777 },
                new EntityBranch { Id = 14, Name = "Branch 4", Number = 7777 },
                new EntityBranch { Id = 15, Name = "Branch 5", Number = 77777 },
                new EntityBranch { Id = 16, Name = "Branch 6", Number = 777777 },
                new EntityLeaf { Id = 21, Name = "Leaf 1", Number = 42, IsGreen = true },
                new EntityLeaf { Id = 22, Name = "Leaf 2", Number = 421, IsGreen = true },
                new EntityLeaf { Id = 23, Name = "Leaf 3", Number = 1337, IsGreen = false },
                new EntityLeaf { Id = 24, Name = "Leaf 4", Number = 1729, IsGreen = false },
            };

        private static JoinCompositeKeyToLeaf[] CreateJoinCompositeKeyToLeaves()
            => new[]
            {
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 22, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 24, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 22, CompositeId1 = 3, CompositeId2 = "3_3", CompositeId3 = new DateTime(2003, 3, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 6, CompositeId2 = "6_1", CompositeId3 = new DateTime(2006, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 22, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 24, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 8, CompositeId2 = "8_1", CompositeId3 = new DateTime(2008, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 8, CompositeId2 = "8_2", CompositeId3 = new DateTime(2008, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 22, CompositeId1 = 8, CompositeId2 = "8_3", CompositeId3 = new DateTime(2008, 3, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 8, CompositeId2 = "8_3", CompositeId3 = new DateTime(2008, 3, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 24, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 9, CompositeId2 = "9_1", CompositeId3 = new DateTime(2009, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 22, CompositeId1 = 9, CompositeId2 = "9_1", CompositeId3 = new DateTime(2009, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 9, CompositeId2 = "9_1", CompositeId3 = new DateTime(2009, 1, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 22, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 24, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 9, CompositeId2 = "9_4", CompositeId3 = new DateTime(2009, 4, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 24, CompositeId1 = 9, CompositeId2 = "9_4", CompositeId3 = new DateTime(2009, 4, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 23, CompositeId1 = 9, CompositeId2 = "9_5", CompositeId3 = new DateTime(2009, 5, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 24, CompositeId1 = 9, CompositeId2 = "9_5", CompositeId3 = new DateTime(2009, 5, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 21, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) },
                new JoinCompositeKeyToLeaf { LeafId = 22, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) }
            };
        private static JoinOneSelfPayload[] CreateJoinOneSelfPayloads()
            => new[]
            {
                new JoinOneSelfPayload { LeftId = 3, RightId = 4, Payload = DateTime.Parse("2020-01-11 19:26:36") },
                new JoinOneSelfPayload { LeftId = 3, RightId = 6, Payload = DateTime.Parse("2005-10-03 12:57:54") },
                new JoinOneSelfPayload { LeftId = 3, RightId = 8, Payload = DateTime.Parse("2015-12-20 01:09:24") },
                new JoinOneSelfPayload { LeftId = 3, RightId = 18, Payload = DateTime.Parse("1999-12-26 02:51:57") },
                new JoinOneSelfPayload { LeftId = 3, RightId = 20, Payload = DateTime.Parse("2011-06-15 19:08:00") },
                new JoinOneSelfPayload { LeftId = 5, RightId = 3, Payload = DateTime.Parse("2019-12-08 05:40:16") },
                new JoinOneSelfPayload { LeftId = 5, RightId = 4, Payload = DateTime.Parse("2014-03-09 12:58:26") },
                new JoinOneSelfPayload { LeftId = 6, RightId = 5, Payload = DateTime.Parse("2014-05-15 16:34:38") },
                new JoinOneSelfPayload { LeftId = 6, RightId = 7, Payload = DateTime.Parse("2014-03-08 18:59:49") },
                new JoinOneSelfPayload { LeftId = 6, RightId = 13, Payload = DateTime.Parse("2013-12-10 07:01:53") },
                new JoinOneSelfPayload { LeftId = 7, RightId = 13, Payload = DateTime.Parse("2005-05-31 02:21:16") },
                new JoinOneSelfPayload { LeftId = 8, RightId = 9, Payload = DateTime.Parse("2011-12-31 19:37:25") },
                new JoinOneSelfPayload { LeftId = 8, RightId = 11, Payload = DateTime.Parse("2012-08-02 16:33:07") },
                new JoinOneSelfPayload { LeftId = 8, RightId = 12, Payload = DateTime.Parse("2018-07-19 09:10:12") },
                new JoinOneSelfPayload { LeftId = 10, RightId = 7, Payload = DateTime.Parse("2018-12-28 01:21:23") },
                new JoinOneSelfPayload { LeftId = 13, RightId = 2, Payload = DateTime.Parse("2014-03-22 02:20:06") },
                new JoinOneSelfPayload { LeftId = 13, RightId = 18, Payload = DateTime.Parse("2005-03-21 14:45:37") },
                new JoinOneSelfPayload { LeftId = 14, RightId = 9, Payload = DateTime.Parse("2016-06-26 08:03:32") },
                new JoinOneSelfPayload { LeftId = 15, RightId = 13, Payload = DateTime.Parse("2018-09-18 12:51:22") },
                new JoinOneSelfPayload { LeftId = 16, RightId = 5, Payload = DateTime.Parse("2016-12-17 14:20:25") },
                new JoinOneSelfPayload { LeftId = 16, RightId = 6, Payload = DateTime.Parse("2008-07-30 03:43:17") },
                new JoinOneSelfPayload { LeftId = 17, RightId = 14, Payload = DateTime.Parse("2019-08-01 16:26:31") },
                new JoinOneSelfPayload { LeftId = 19, RightId = 1, Payload = DateTime.Parse("2010-02-19 13:24:07") },
                new JoinOneSelfPayload { LeftId = 19, RightId = 8, Payload = DateTime.Parse("2004-07-28 09:06:02") },
                new JoinOneSelfPayload { LeftId = 19, RightId = 12, Payload = DateTime.Parse("2004-08-21 11:07:20") },
                new JoinOneSelfPayload { LeftId = 20, RightId = 1, Payload = DateTime.Parse("2014-11-21 18:13:02") },
                new JoinOneSelfPayload { LeftId = 20, RightId = 7, Payload = DateTime.Parse("2009-08-24 21:44:46") },
                new JoinOneSelfPayload { LeftId = 20, RightId = 14, Payload = DateTime.Parse("2013-02-18 02:19:19") },
                new JoinOneSelfPayload { LeftId = 20, RightId = 16, Payload = DateTime.Parse("2016-02-05 14:18:12") }
            };
        private static JoinOneToBranch[] CreateJoinOneToBranches()
            => new[]
            {
                new JoinOneToBranch { OneId = 2, BranchId = 16 },
                new JoinOneToBranch { OneId = 2, BranchId = 24 },
                new JoinOneToBranch { OneId = 3, BranchId = 14 },
                new JoinOneToBranch { OneId = 3, BranchId = 16 },
                new JoinOneToBranch { OneId = 3, BranchId = 22 },
                new JoinOneToBranch { OneId = 3, BranchId = 24 },
                new JoinOneToBranch { OneId = 5, BranchId = 13 },
                new JoinOneToBranch { OneId = 6, BranchId = 16 },
                new JoinOneToBranch { OneId = 6, BranchId = 22 },
                new JoinOneToBranch { OneId = 6, BranchId = 23 },
                new JoinOneToBranch { OneId = 8, BranchId = 11 },
                new JoinOneToBranch { OneId = 8, BranchId = 12 },
                new JoinOneToBranch { OneId = 8, BranchId = 13 },
                new JoinOneToBranch { OneId = 9, BranchId = 11 },
                new JoinOneToBranch { OneId = 9, BranchId = 12 },
                new JoinOneToBranch { OneId = 9, BranchId = 14 },
                new JoinOneToBranch { OneId = 9, BranchId = 16 },
                new JoinOneToBranch { OneId = 9, BranchId = 21 },
                new JoinOneToBranch { OneId = 9, BranchId = 24 },
                new JoinOneToBranch { OneId = 10, BranchId = 12 },
                new JoinOneToBranch { OneId = 10, BranchId = 13 },
                new JoinOneToBranch { OneId = 10, BranchId = 14 },
                new JoinOneToBranch { OneId = 10, BranchId = 21 },
                new JoinOneToBranch { OneId = 12, BranchId = 11 },
                new JoinOneToBranch { OneId = 12, BranchId = 12 },
                new JoinOneToBranch { OneId = 12, BranchId = 14 },
                new JoinOneToBranch { OneId = 12, BranchId = 23 },
                new JoinOneToBranch { OneId = 13, BranchId = 15 },
                new JoinOneToBranch { OneId = 14, BranchId = 12 },
                new JoinOneToBranch { OneId = 14, BranchId = 14 },
                new JoinOneToBranch { OneId = 14, BranchId = 16 },
                new JoinOneToBranch { OneId = 14, BranchId = 23 },
                new JoinOneToBranch { OneId = 15, BranchId = 15 },
                new JoinOneToBranch { OneId = 15, BranchId = 16 },
                new JoinOneToBranch { OneId = 15, BranchId = 24 },
                new JoinOneToBranch { OneId = 16, BranchId = 11 },
                new JoinOneToBranch { OneId = 17, BranchId = 11 },
                new JoinOneToBranch { OneId = 17, BranchId = 21 },
                new JoinOneToBranch { OneId = 18, BranchId = 12 },
                new JoinOneToBranch { OneId = 18, BranchId = 15 },
                new JoinOneToBranch { OneId = 18, BranchId = 24 },
                new JoinOneToBranch { OneId = 19, BranchId = 11 },
                new JoinOneToBranch { OneId = 19, BranchId = 12 },
                new JoinOneToBranch { OneId = 19, BranchId = 16 },
                new JoinOneToBranch { OneId = 19, BranchId = 23 },
                new JoinOneToBranch { OneId = 20, BranchId = 21 },
                new JoinOneToBranch { OneId = 20, BranchId = 23 }
            };
        private static JoinOneToThreePayloadFull[] CreateJoinOneToThreePayloadFulls()
            => new[]
            {
                new JoinOneToThreePayloadFull { OneId = 1, ThreeId = 2, Payload = "Ira Watts" },
                new JoinOneToThreePayloadFull { OneId = 1, ThreeId = 6, Payload = "Harold May" },
                new JoinOneToThreePayloadFull { OneId = 1, ThreeId = 9, Payload = "Freda Vaughn" },
                new JoinOneToThreePayloadFull { OneId = 1, ThreeId = 13, Payload = "Pedro Mccarthy" },
                new JoinOneToThreePayloadFull { OneId = 1, ThreeId = 17, Payload = "Elaine Simon" },
                new JoinOneToThreePayloadFull { OneId = 2, ThreeId = 9, Payload = "Melvin Maldonado" },
                new JoinOneToThreePayloadFull { OneId = 2, ThreeId = 11, Payload = "Lora George" },
                new JoinOneToThreePayloadFull { OneId = 2, ThreeId = 13, Payload = "Joey Cohen" },
                new JoinOneToThreePayloadFull { OneId = 2, ThreeId = 14, Payload = "Erik Carroll" },
                new JoinOneToThreePayloadFull { OneId = 2, ThreeId = 16, Payload = "April Rodriguez" },
                new JoinOneToThreePayloadFull { OneId = 3, ThreeId = 5, Payload = "Gerardo Colon" },
                new JoinOneToThreePayloadFull { OneId = 3, ThreeId = 12, Payload = "Alexander Willis" },
                new JoinOneToThreePayloadFull { OneId = 3, ThreeId = 16, Payload = "Laura Wheeler" },
                new JoinOneToThreePayloadFull { OneId = 3, ThreeId = 19, Payload = "Lester Summers" },
                new JoinOneToThreePayloadFull { OneId = 4, ThreeId = 2, Payload = "Raquel Curry" },
                new JoinOneToThreePayloadFull { OneId = 4, ThreeId = 4, Payload = "Steven Fisher" },
                new JoinOneToThreePayloadFull { OneId = 4, ThreeId = 11, Payload = "Casey Williams" },
                new JoinOneToThreePayloadFull { OneId = 4, ThreeId = 13, Payload = "Lauren Clayton" },
                new JoinOneToThreePayloadFull { OneId = 4, ThreeId = 19, Payload = "Maureen Weber" },
                new JoinOneToThreePayloadFull { OneId = 5, ThreeId = 4, Payload = "Joyce Ford" },
                new JoinOneToThreePayloadFull { OneId = 5, ThreeId = 6, Payload = "Willie Mccormick" },
                new JoinOneToThreePayloadFull { OneId = 5, ThreeId = 9, Payload = "Geraldine Jackson" },
                new JoinOneToThreePayloadFull { OneId = 7, ThreeId = 1, Payload = "Victor Aguilar" },
                new JoinOneToThreePayloadFull { OneId = 7, ThreeId = 4, Payload = "Cathy Allen" },
                new JoinOneToThreePayloadFull { OneId = 7, ThreeId = 9, Payload = "Edwin Burke" },
                new JoinOneToThreePayloadFull { OneId = 7, ThreeId = 10, Payload = "Eugene Flores" },
                new JoinOneToThreePayloadFull { OneId = 7, ThreeId = 11, Payload = "Ginger Patton" },
                new JoinOneToThreePayloadFull { OneId = 7, ThreeId = 12, Payload = "Israel Mitchell" },
                new JoinOneToThreePayloadFull { OneId = 7, ThreeId = 18, Payload = "Joy Francis" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 1, Payload = "Orville Parker" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 3, Payload = "Alyssa Mann" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 4, Payload = "Hugh Daniel" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 13, Payload = "Kim Craig" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 14, Payload = "Lucille Moreno" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 17, Payload = "Virgil Drake" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 18, Payload = "Josephine Dawson" },
                new JoinOneToThreePayloadFull { OneId = 8, ThreeId = 20, Payload = "Milton Huff" },
                new JoinOneToThreePayloadFull { OneId = 9, ThreeId = 2, Payload = "Jody Clarke" },
                new JoinOneToThreePayloadFull { OneId = 9, ThreeId = 9, Payload = "Elisa Cooper" },
                new JoinOneToThreePayloadFull { OneId = 9, ThreeId = 11, Payload = "Grace Owen" },
                new JoinOneToThreePayloadFull { OneId = 9, ThreeId = 12, Payload = "Donald Welch" },
                new JoinOneToThreePayloadFull { OneId = 9, ThreeId = 15, Payload = "Marian Day" },
                new JoinOneToThreePayloadFull { OneId = 9, ThreeId = 17, Payload = "Cory Cortez" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 2, Payload = "Chad Rowe" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 3, Payload = "Simon Reyes" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 4, Payload = "Shari Jensen" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 8, Payload = "Ricky Bradley" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 10, Payload = "Debra Gibbs" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 11, Payload = "Everett Mckenzie" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 14, Payload = "Kirk Graham" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 16, Payload = "Paulette Adkins" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 18, Payload = "Raul Holloway" },
                new JoinOneToThreePayloadFull { OneId = 10, ThreeId = 19, Payload = "Danielle Ross" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 1, Payload = "Frank Garner" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 6, Payload = "Stella Thompson" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 8, Payload = "Peggy Wagner" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 9, Payload = "Geneva Holmes" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 10, Payload = "Ignacio Black" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 13, Payload = "Phillip Wells" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 14, Payload = "Hubert Lambert" },
                new JoinOneToThreePayloadFull { OneId = 11, ThreeId = 19, Payload = "Courtney Gregory" },
                new JoinOneToThreePayloadFull { OneId = 12, ThreeId = 2, Payload = "Esther Carter" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 6, Payload = "Thomas Benson" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 9, Payload = "Kara Baldwin" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 10, Payload = "Yvonne Sparks" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 11, Payload = "Darin Mathis" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 12, Payload = "Glenda Castillo" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 13, Payload = "Larry Walters" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 15, Payload = "Meredith Yates" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 16, Payload = "Rosemarie Henry" },
                new JoinOneToThreePayloadFull { OneId = 13, ThreeId = 18, Payload = "Nora Leonard" },
                new JoinOneToThreePayloadFull { OneId = 14, ThreeId = 17, Payload = "Corey Delgado" },
                new JoinOneToThreePayloadFull { OneId = 14, ThreeId = 18, Payload = "Kari Strickland" },
                new JoinOneToThreePayloadFull { OneId = 15, ThreeId = 8, Payload = "Joann Stanley" },
                new JoinOneToThreePayloadFull { OneId = 15, ThreeId = 11, Payload = "Camille Gordon" },
                new JoinOneToThreePayloadFull { OneId = 15, ThreeId = 14, Payload = "Flora Anderson" },
                new JoinOneToThreePayloadFull { OneId = 15, ThreeId = 15, Payload = "Wilbur Soto" },
                new JoinOneToThreePayloadFull { OneId = 15, ThreeId = 18, Payload = "Shirley Andrews" },
                new JoinOneToThreePayloadFull { OneId = 15, ThreeId = 20, Payload = "Marcus Mcguire" },
                new JoinOneToThreePayloadFull { OneId = 16, ThreeId = 1, Payload = "Saul Dixon" },
                new JoinOneToThreePayloadFull { OneId = 16, ThreeId = 6, Payload = "Cynthia Hart" },
                new JoinOneToThreePayloadFull { OneId = 16, ThreeId = 10, Payload = "Elbert Spencer" },
                new JoinOneToThreePayloadFull { OneId = 16, ThreeId = 13, Payload = "Darrell Norris" },
                new JoinOneToThreePayloadFull { OneId = 16, ThreeId = 14, Payload = "Jamie Kelley" },
                new JoinOneToThreePayloadFull { OneId = 16, ThreeId = 15, Payload = "Francis Briggs" },
                new JoinOneToThreePayloadFull { OneId = 16, ThreeId = 16, Payload = "Lindsey Morris" },
                new JoinOneToThreePayloadFull { OneId = 17, ThreeId = 2, Payload = "James Castro" },
                new JoinOneToThreePayloadFull { OneId = 17, ThreeId = 5, Payload = "Carlos Chavez" },
                new JoinOneToThreePayloadFull { OneId = 17, ThreeId = 7, Payload = "Janis Valdez" },
                new JoinOneToThreePayloadFull { OneId = 17, ThreeId = 13, Payload = "Alfredo Bowen" },
                new JoinOneToThreePayloadFull { OneId = 17, ThreeId = 14, Payload = "Viola Torres" },
                new JoinOneToThreePayloadFull { OneId = 17, ThreeId = 15, Payload = "Dianna Lowe" },
                new JoinOneToThreePayloadFull { OneId = 18, ThreeId = 3, Payload = "Craig Howell" },
                new JoinOneToThreePayloadFull { OneId = 18, ThreeId = 7, Payload = "Sandy Curtis" },
                new JoinOneToThreePayloadFull { OneId = 18, ThreeId = 12, Payload = "Alonzo Pierce" },
                new JoinOneToThreePayloadFull { OneId = 18, ThreeId = 18, Payload = "Albert Harper" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 2, Payload = "Frankie Baker" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 5, Payload = "Candace Tucker" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 6, Payload = "Willis Christensen" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 7, Payload = "Juan Joseph" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 10, Payload = "Thelma Sanders" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 11, Payload = "Kerry West" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 15, Payload = "Sheri Castro" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 16, Payload = "Mark Schultz" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 17, Payload = "Priscilla Summers" },
                new JoinOneToThreePayloadFull { OneId = 19, ThreeId = 20, Payload = "Allan Valdez" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 3, Payload = "Bill Peters" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 5, Payload = "Cora Stone" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 6, Payload = "Frankie Pope" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 10, Payload = "Christian Young" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 11, Payload = "Shari Brewer" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 12, Payload = "Antonia Wolfe" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 14, Payload = "Lawrence Matthews" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 18, Payload = "Van Hubbard" },
                new JoinOneToThreePayloadFull { OneId = 20, ThreeId = 20, Payload = "Lindsay Pena" }
            };
        private static JoinOneToTwo[] CreateJoinOneToTwos()
            => new[]
            {
                new JoinOneToTwo { OneId = 1, TwoId = 1},
                new JoinOneToTwo { OneId = 1, TwoId = 2},
                new JoinOneToTwo { OneId = 1, TwoId = 3},
                new JoinOneToTwo { OneId = 1, TwoId = 4},
                new JoinOneToTwo { OneId = 1, TwoId = 5},
                new JoinOneToTwo { OneId = 1, TwoId = 6},
                new JoinOneToTwo { OneId = 1, TwoId = 7},
                new JoinOneToTwo { OneId = 1, TwoId = 8},
                new JoinOneToTwo { OneId = 1, TwoId = 9},
                new JoinOneToTwo { OneId = 1, TwoId = 10},
                new JoinOneToTwo { OneId = 1, TwoId = 11},
                new JoinOneToTwo { OneId = 1, TwoId = 12},
                new JoinOneToTwo { OneId = 1, TwoId = 13},
                new JoinOneToTwo { OneId = 1, TwoId = 14},
                new JoinOneToTwo { OneId = 1, TwoId = 15},
                new JoinOneToTwo { OneId = 1, TwoId = 16},
                new JoinOneToTwo { OneId = 1, TwoId = 17},
                new JoinOneToTwo { OneId = 1, TwoId = 18},
                new JoinOneToTwo { OneId = 1, TwoId = 19},
                new JoinOneToTwo { OneId = 1, TwoId = 20},
                new JoinOneToTwo { OneId = 2, TwoId = 1},
                new JoinOneToTwo { OneId = 2, TwoId = 3},
                new JoinOneToTwo { OneId = 2, TwoId = 5},
                new JoinOneToTwo { OneId = 2, TwoId = 7},
                new JoinOneToTwo { OneId = 2, TwoId = 9},
                new JoinOneToTwo { OneId = 2, TwoId = 11},
                new JoinOneToTwo { OneId = 2, TwoId = 13},
                new JoinOneToTwo { OneId = 2, TwoId = 15},
                new JoinOneToTwo { OneId = 2, TwoId = 17},
                new JoinOneToTwo { OneId = 2, TwoId = 19},
                new JoinOneToTwo { OneId = 3, TwoId = 1},
                new JoinOneToTwo { OneId = 3, TwoId = 4},
                new JoinOneToTwo { OneId = 3, TwoId = 7},
                new JoinOneToTwo { OneId = 3, TwoId = 10},
                new JoinOneToTwo { OneId = 3, TwoId = 13},
                new JoinOneToTwo { OneId = 3, TwoId = 16},
                new JoinOneToTwo { OneId = 3, TwoId = 19},
                new JoinOneToTwo { OneId = 4, TwoId = 1},
                new JoinOneToTwo { OneId = 4, TwoId = 5},
                new JoinOneToTwo { OneId = 4, TwoId = 9},
                new JoinOneToTwo { OneId = 4, TwoId = 13},
                new JoinOneToTwo { OneId = 4, TwoId = 17},
                new JoinOneToTwo { OneId = 5, TwoId = 1},
                new JoinOneToTwo { OneId = 5, TwoId = 6},
                new JoinOneToTwo { OneId = 5, TwoId = 11},
                new JoinOneToTwo { OneId = 5, TwoId = 16},
                new JoinOneToTwo { OneId = 6, TwoId = 1},
                new JoinOneToTwo { OneId = 6, TwoId = 7},
                new JoinOneToTwo { OneId = 6, TwoId = 13},
                new JoinOneToTwo { OneId = 6, TwoId = 19},
                new JoinOneToTwo { OneId = 7, TwoId = 1},
                new JoinOneToTwo { OneId = 7, TwoId = 8},
                new JoinOneToTwo { OneId = 7, TwoId = 15},
                new JoinOneToTwo { OneId = 8, TwoId = 1},
                new JoinOneToTwo { OneId = 8, TwoId = 9},
                new JoinOneToTwo { OneId = 8, TwoId = 17},
                new JoinOneToTwo { OneId = 9, TwoId = 1},
                new JoinOneToTwo { OneId = 9, TwoId = 10},
                new JoinOneToTwo { OneId = 9, TwoId = 19},
                new JoinOneToTwo { OneId = 10, TwoId = 1},
                new JoinOneToTwo { OneId = 10, TwoId = 11},
                new JoinOneToTwo { OneId = 11, TwoId = 20},
                new JoinOneToTwo { OneId = 11, TwoId = 19},
                new JoinOneToTwo { OneId = 11, TwoId = 18},
                new JoinOneToTwo { OneId = 11, TwoId = 17},
                new JoinOneToTwo { OneId = 11, TwoId = 16},
                new JoinOneToTwo { OneId = 11, TwoId = 15},
                new JoinOneToTwo { OneId = 11, TwoId = 14},
                new JoinOneToTwo { OneId = 11, TwoId = 13},
                new JoinOneToTwo { OneId = 11, TwoId = 12},
                new JoinOneToTwo { OneId = 11, TwoId = 11},
                new JoinOneToTwo { OneId = 11, TwoId = 10},
                new JoinOneToTwo { OneId = 11, TwoId = 9},
                new JoinOneToTwo { OneId = 11, TwoId = 8},
                new JoinOneToTwo { OneId = 11, TwoId = 7},
                new JoinOneToTwo { OneId = 11, TwoId = 6},
                new JoinOneToTwo { OneId = 11, TwoId = 5},
                new JoinOneToTwo { OneId = 11, TwoId = 4},
                new JoinOneToTwo { OneId = 11, TwoId = 3},
                new JoinOneToTwo { OneId = 11, TwoId = 2},
                new JoinOneToTwo { OneId = 11, TwoId = 1},
                new JoinOneToTwo { OneId = 12, TwoId = 20},
                new JoinOneToTwo { OneId = 12, TwoId = 17},
                new JoinOneToTwo { OneId = 12, TwoId = 14},
                new JoinOneToTwo { OneId = 12, TwoId = 11},
                new JoinOneToTwo { OneId = 12, TwoId = 8},
                new JoinOneToTwo { OneId = 12, TwoId = 5},
                new JoinOneToTwo { OneId = 12, TwoId = 2},
                new JoinOneToTwo { OneId = 13, TwoId = 20},
                new JoinOneToTwo { OneId = 13, TwoId = 16},
                new JoinOneToTwo { OneId = 13, TwoId = 12},
                new JoinOneToTwo { OneId = 13, TwoId = 8},
                new JoinOneToTwo { OneId = 13, TwoId = 4},
                new JoinOneToTwo { OneId = 14, TwoId = 20},
                new JoinOneToTwo { OneId = 14, TwoId = 15},
                new JoinOneToTwo { OneId = 14, TwoId = 10},
                new JoinOneToTwo { OneId = 14, TwoId = 5},
                new JoinOneToTwo { OneId = 15, TwoId = 20},
                new JoinOneToTwo { OneId = 15, TwoId = 14},
                new JoinOneToTwo { OneId = 15, TwoId = 8},
                new JoinOneToTwo { OneId = 15, TwoId = 2},
                new JoinOneToTwo { OneId = 16, TwoId = 20},
                new JoinOneToTwo { OneId = 16, TwoId = 13},
                new JoinOneToTwo { OneId = 16, TwoId = 6},
                new JoinOneToTwo { OneId = 17, TwoId = 20},
                new JoinOneToTwo { OneId = 17, TwoId = 12},
                new JoinOneToTwo { OneId = 17, TwoId = 4},
                new JoinOneToTwo { OneId = 18, TwoId = 20},
                new JoinOneToTwo { OneId = 18, TwoId = 11},
                new JoinOneToTwo { OneId = 18, TwoId = 2},
                new JoinOneToTwo { OneId = 19, TwoId = 20},
                new JoinOneToTwo { OneId = 19, TwoId = 10}
            };
        private static JoinThreeToCompositeKeyFull[] CreateJoinThreeToCompositeKeyFulls()
            => new[]
            {
                new JoinThreeToCompositeKeyFull { ThreeId = 1, CompositeId1 = 6, CompositeId2 = "6_1", CompositeId3 = new DateTime(2006, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 2, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 2, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 2, CompositeId1 = 9, CompositeId2 = "9_7", CompositeId3 = new DateTime(2009, 7, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 3, CompositeId1 = 6, CompositeId2 = "6_1", CompositeId3 = new DateTime(2006, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 3, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 3, CompositeId1 = 9, CompositeId2 = "9_7", CompositeId3 = new DateTime(2009, 7, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 5, CompositeId1 = 8, CompositeId2 = "8_4", CompositeId3 = new DateTime(2008, 4, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 5, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 5, CompositeId1 = 9, CompositeId2 = "9_5", CompositeId3 = new DateTime(2009, 5, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 6, CompositeId1 = 6, CompositeId2 = "6_1", CompositeId3 = new DateTime(2006, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 7, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 7, CompositeId1 = 8, CompositeId2 = "8_1", CompositeId3 = new DateTime(2008, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 8, CompositeId1 = 8, CompositeId2 = "8_3", CompositeId3 = new DateTime(2008, 3, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 8, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 9, CompositeId1 = 8, CompositeId2 = "8_1", CompositeId3 = new DateTime(2008, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 9, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 10, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 11, CompositeId1 = 7, CompositeId2 = "7_1", CompositeId3 = new DateTime(2007, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 11, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 12, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 12, CompositeId1 = 8, CompositeId2 = "8_3", CompositeId3 = new DateTime(2008, 3, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 12, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 13, CompositeId1 = 6, CompositeId2 = "6_1", CompositeId3 = new DateTime(2006, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 13, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 13, CompositeId1 = 9, CompositeId2 = "9_1", CompositeId3 = new DateTime(2009, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 13, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 14, CompositeId1 = 8, CompositeId2 = "8_2", CompositeId3 = new DateTime(2008, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 14, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 14, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 15, CompositeId1 = 8, CompositeId2 = "8_2", CompositeId3 = new DateTime(2008, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 15, CompositeId1 = 9, CompositeId2 = "9_1", CompositeId3 = new DateTime(2009, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 15, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 16, CompositeId1 = 3, CompositeId2 = "3_3", CompositeId3 = new DateTime(2003, 3, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 16, CompositeId1 = 7, CompositeId2 = "7_1", CompositeId3 = new DateTime(2007, 1, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 16, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 17, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 17, CompositeId1 = 8, CompositeId2 = "8_2", CompositeId3 = new DateTime(2008, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 18, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 19, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 19, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 19, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 19, CompositeId1 = 9, CompositeId2 = "9_7", CompositeId3 = new DateTime(2009, 7, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 20, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinThreeToCompositeKeyFull { ThreeId = 20, CompositeId1 = 7, CompositeId2 = "7_1", CompositeId3 = new DateTime(2007, 1, 1) }
            };
        private static JoinTwoToThree[] CreateJoinTwoToThrees()
            => new[]
            {
                new JoinTwoToThree { TwoId = 1, ThreeId = 2 },
                new JoinTwoToThree { TwoId = 1, ThreeId = 3 },
                new JoinTwoToThree { TwoId = 1, ThreeId = 13 },
                new JoinTwoToThree { TwoId = 1, ThreeId = 18 },
                new JoinTwoToThree { TwoId = 2, ThreeId = 1 },
                new JoinTwoToThree { TwoId = 2, ThreeId = 9 },
                new JoinTwoToThree { TwoId = 2, ThreeId = 15 },
                new JoinTwoToThree { TwoId = 3, ThreeId = 11 },
                new JoinTwoToThree { TwoId = 3, ThreeId = 17 },
                new JoinTwoToThree { TwoId = 4, ThreeId = 2 },
                new JoinTwoToThree { TwoId = 4, ThreeId = 5 },
                new JoinTwoToThree { TwoId = 4, ThreeId = 11 },
                new JoinTwoToThree { TwoId = 5, ThreeId = 4 },
                new JoinTwoToThree { TwoId = 5, ThreeId = 5 },
                new JoinTwoToThree { TwoId = 6, ThreeId = 3 },
                new JoinTwoToThree { TwoId = 6, ThreeId = 10 },
                new JoinTwoToThree { TwoId = 6, ThreeId = 16 },
                new JoinTwoToThree { TwoId = 6, ThreeId = 18 },
                new JoinTwoToThree { TwoId = 7, ThreeId = 12 },
                new JoinTwoToThree { TwoId = 7, ThreeId = 15 },
                new JoinTwoToThree { TwoId = 7, ThreeId = 20 },
                new JoinTwoToThree { TwoId = 8, ThreeId = 1 },
                new JoinTwoToThree { TwoId = 8, ThreeId = 3 },
                new JoinTwoToThree { TwoId = 8, ThreeId = 20 },
                new JoinTwoToThree { TwoId = 9, ThreeId = 3 },
                new JoinTwoToThree { TwoId = 9, ThreeId = 13 },
                new JoinTwoToThree { TwoId = 9, ThreeId = 19 },
                new JoinTwoToThree { TwoId = 10, ThreeId = 17 },
                new JoinTwoToThree { TwoId = 11, ThreeId = 6 },
                new JoinTwoToThree { TwoId = 11, ThreeId = 7 },
                new JoinTwoToThree { TwoId = 11, ThreeId = 8 },
                new JoinTwoToThree { TwoId = 11, ThreeId = 13 },
                new JoinTwoToThree { TwoId = 12, ThreeId = 9 },
                new JoinTwoToThree { TwoId = 13, ThreeId = 1 },
                new JoinTwoToThree { TwoId = 13, ThreeId = 11 },
                new JoinTwoToThree { TwoId = 13, ThreeId = 19 },
                new JoinTwoToThree { TwoId = 14, ThreeId = 2 },
                new JoinTwoToThree { TwoId = 15, ThreeId = 17 },
                new JoinTwoToThree { TwoId = 16, ThreeId = 3 },
                new JoinTwoToThree { TwoId = 16, ThreeId = 16 },
                new JoinTwoToThree { TwoId = 18, ThreeId = 1 },
                new JoinTwoToThree { TwoId = 18, ThreeId = 5 },
                new JoinTwoToThree { TwoId = 18, ThreeId = 10 },
                new JoinTwoToThree { TwoId = 19, ThreeId = 5 },
                new JoinTwoToThree { TwoId = 19, ThreeId = 16 },
                new JoinTwoToThree { TwoId = 19, ThreeId = 18 },
                new JoinTwoToThree { TwoId = 20, ThreeId = 6 },
                new JoinTwoToThree { TwoId = 20, ThreeId = 10 },
                new JoinTwoToThree { TwoId = 20, ThreeId = 12 },
                new JoinTwoToThree { TwoId = 20, ThreeId = 16 },
                new JoinTwoToThree { TwoId = 20, ThreeId = 17 },
                new JoinTwoToThree { TwoId = 20, ThreeId = 18 }
            };

        private static JoinOneToTwoShared[] CreateJoinOneToTwoShareds()
            => new[]
            {
                new JoinOneToTwoShared { OneId = 1, TwoId = 3 },
                new JoinOneToTwoShared { OneId = 1, TwoId = 16 },
                new JoinOneToTwoShared { OneId = 2, TwoId = 3 },
                new JoinOneToTwoShared { OneId = 2, TwoId = 10 },
                new JoinOneToTwoShared { OneId = 2, TwoId = 18 },
                new JoinOneToTwoShared { OneId = 3, TwoId = 10 },
                new JoinOneToTwoShared { OneId = 3, TwoId = 11 },
                new JoinOneToTwoShared { OneId = 3, TwoId = 16 },
                new JoinOneToTwoShared { OneId = 5, TwoId = 2 },
                new JoinOneToTwoShared { OneId = 5, TwoId = 5 },
                new JoinOneToTwoShared { OneId = 5, TwoId = 7 },
                new JoinOneToTwoShared { OneId = 5, TwoId = 9 },
                new JoinOneToTwoShared { OneId = 5, TwoId = 14 },
                new JoinOneToTwoShared { OneId = 6, TwoId = 12 },
                new JoinOneToTwoShared { OneId = 7, TwoId = 3 },
                new JoinOneToTwoShared { OneId = 7, TwoId = 16 },
                new JoinOneToTwoShared { OneId = 7, TwoId = 17 },
                new JoinOneToTwoShared { OneId = 8, TwoId = 19 },
                new JoinOneToTwoShared { OneId = 9, TwoId = 9 },
                new JoinOneToTwoShared { OneId = 9, TwoId = 11 },
                new JoinOneToTwoShared { OneId = 10, TwoId = 6 },
                new JoinOneToTwoShared { OneId = 10, TwoId = 17 },
                new JoinOneToTwoShared { OneId = 10, TwoId = 20 },
                new JoinOneToTwoShared { OneId = 11, TwoId = 17 },
                new JoinOneToTwoShared { OneId = 11, TwoId = 18 },
                new JoinOneToTwoShared { OneId = 12, TwoId = 6 },
                new JoinOneToTwoShared { OneId = 12, TwoId = 19 },
                new JoinOneToTwoShared { OneId = 13, TwoId = 7 },
                new JoinOneToTwoShared { OneId = 13, TwoId = 8 },
                new JoinOneToTwoShared { OneId = 13, TwoId = 9 },
                new JoinOneToTwoShared { OneId = 13, TwoId = 13 },
                new JoinOneToTwoShared { OneId = 14, TwoId = 4 },
                new JoinOneToTwoShared { OneId = 14, TwoId = 9 },
                new JoinOneToTwoShared { OneId = 14, TwoId = 19 },
                new JoinOneToTwoShared { OneId = 15, TwoId = 10 },
                new JoinOneToTwoShared { OneId = 16, TwoId = 1 },
                new JoinOneToTwoShared { OneId = 16, TwoId = 7 },
                new JoinOneToTwoShared { OneId = 16, TwoId = 19 },
                new JoinOneToTwoShared { OneId = 17, TwoId = 8 },
                new JoinOneToTwoShared { OneId = 17, TwoId = 15 },
                new JoinOneToTwoShared { OneId = 18, TwoId = 4 },
                new JoinOneToTwoShared { OneId = 18, TwoId = 13 },
                new JoinOneToTwoShared { OneId = 18, TwoId = 14 },
                new JoinOneToTwoShared { OneId = 19, TwoId = 4 },
                new JoinOneToTwoShared { OneId = 19, TwoId = 14 }
            };
        private static JoinOneToThreePayloadFullShared[] CreateJoinOneToThreePayloadFullShareds()
            => new[]
            {
                new JoinOneToThreePayloadFullShared { OneId = 3, ThreeId = 1, Payload = "Capbrough" },
                new JoinOneToThreePayloadFullShared { OneId = 3, ThreeId = 2, Payload = "East Eastdol" },
                new JoinOneToThreePayloadFullShared { OneId = 3, ThreeId = 4, Payload = "Southingville" },
                new JoinOneToThreePayloadFullShared { OneId = 3, ThreeId = 9, Payload = "Goldbrough" },
                new JoinOneToThreePayloadFullShared { OneId = 4, ThreeId = 5, Payload = "Readingworth" },
                new JoinOneToThreePayloadFullShared { OneId = 4, ThreeId = 18, Payload = "Skillpool" },
                new JoinOneToThreePayloadFullShared { OneId = 5, ThreeId = 1, Payload = "Lawgrad" },
                new JoinOneToThreePayloadFullShared { OneId = 5, ThreeId = 4, Payload = "Kettleham Park" },
                new JoinOneToThreePayloadFullShared { OneId = 5, ThreeId = 9, Payload = "Sayford Park" },
                new JoinOneToThreePayloadFullShared { OneId = 5, ThreeId = 16, Payload = "Hamstead" },
                new JoinOneToThreePayloadFullShared { OneId = 6, ThreeId = 11, Payload = "North Starside" },
                new JoinOneToThreePayloadFullShared { OneId = 6, ThreeId = 13, Payload = "Goldfolk" },
                new JoinOneToThreePayloadFullShared { OneId = 7, ThreeId = 4, Payload = "Winstead" },
                new JoinOneToThreePayloadFullShared { OneId = 8, ThreeId = 11, Payload = "Transworth" },
                new JoinOneToThreePayloadFullShared { OneId = 8, ThreeId = 18, Payload = "Parkpool" },
                new JoinOneToThreePayloadFullShared { OneId = 8, ThreeId = 19, Payload = "Fishham" },
                new JoinOneToThreePayloadFullShared { OneId = 10, ThreeId = 1, Payload = "Passmouth" },
                new JoinOneToThreePayloadFullShared { OneId = 10, ThreeId = 5, Payload = "Valenfield" },
                new JoinOneToThreePayloadFullShared { OneId = 10, ThreeId = 20, Payload = "Passford Park" },
                new JoinOneToThreePayloadFullShared { OneId = 11, ThreeId = 10, Payload = "Chatfield" },
                new JoinOneToThreePayloadFullShared { OneId = 12, ThreeId = 11, Payload = "Hosview" },
                new JoinOneToThreePayloadFullShared { OneId = 12, ThreeId = 17, Payload = "Dodgewich" },
                new JoinOneToThreePayloadFullShared { OneId = 13, ThreeId = 3, Payload = "Skillhampton" },
                new JoinOneToThreePayloadFullShared { OneId = 13, ThreeId = 14, Payload = "Hardcaster" },
                new JoinOneToThreePayloadFullShared { OneId = 13, ThreeId = 16, Payload = "Hollowmouth" },
                new JoinOneToThreePayloadFullShared { OneId = 14, ThreeId = 6, Payload = "Cruxcaster" },
                new JoinOneToThreePayloadFullShared { OneId = 14, ThreeId = 11, Payload = "Elcaster" },
                new JoinOneToThreePayloadFullShared { OneId = 14, ThreeId = 17, Payload = "Clambrough" },
                new JoinOneToThreePayloadFullShared { OneId = 15, ThreeId = 10, Payload = "Millwich" },
                new JoinOneToThreePayloadFullShared { OneId = 15, ThreeId = 13, Payload = "Hapcester" },
                new JoinOneToThreePayloadFullShared { OneId = 16, ThreeId = 7, Payload = "Sanddol Beach" },
                new JoinOneToThreePayloadFullShared { OneId = 16, ThreeId = 13, Payload = "Hamcaster" },
                new JoinOneToThreePayloadFullShared { OneId = 17, ThreeId = 9, Payload = "New Foxbrough" },
                new JoinOneToThreePayloadFullShared { OneId = 17, ThreeId = 13, Payload = "Chatpool" },
                new JoinOneToThreePayloadFullShared { OneId = 18, ThreeId = 8, Payload = "Duckworth" },
                new JoinOneToThreePayloadFullShared { OneId = 18, ThreeId = 12, Payload = "Snowham" },
                new JoinOneToThreePayloadFullShared { OneId = 18, ThreeId = 13, Payload = "Bannview Island" },
                new JoinOneToThreePayloadFullShared { OneId = 20, ThreeId = 4, Payload = "Rockbrough" },
                new JoinOneToThreePayloadFullShared { OneId = 20, ThreeId = 5, Payload = "Sweetfield" },
                new JoinOneToThreePayloadFullShared { OneId = 20, ThreeId = 16, Payload = "Bayburgh Hills" }
            };
        private static JoinTwoSelfShared[] CreateJoinTwoSelfShareds()
            => new[]
            {
                new JoinTwoSelfShared { LeftId = 1, RightId = 9 },
                new JoinTwoSelfShared { LeftId = 1, RightId = 10 },
                new JoinTwoSelfShared { LeftId = 1, RightId = 11 },
                new JoinTwoSelfShared { LeftId = 1, RightId = 18 },
                new JoinTwoSelfShared { LeftId = 3, RightId = 2 },
                new JoinTwoSelfShared { LeftId = 3, RightId = 5 },
                new JoinTwoSelfShared { LeftId = 3, RightId = 8 },
                new JoinTwoSelfShared { LeftId = 3, RightId = 18 },
                new JoinTwoSelfShared { LeftId = 3, RightId = 19 },
                new JoinTwoSelfShared { LeftId = 4, RightId = 11 },
                new JoinTwoSelfShared { LeftId = 5, RightId = 8 },
                new JoinTwoSelfShared { LeftId = 6, RightId = 18 },
                new JoinTwoSelfShared { LeftId = 8, RightId = 2 },
                new JoinTwoSelfShared { LeftId = 8, RightId = 14 },
                new JoinTwoSelfShared { LeftId = 8, RightId = 15 },
                new JoinTwoSelfShared { LeftId = 8, RightId = 20 },
                new JoinTwoSelfShared { LeftId = 9, RightId = 4 },
                new JoinTwoSelfShared { LeftId = 9, RightId = 14 },
                new JoinTwoSelfShared { LeftId = 10, RightId = 5 },
                new JoinTwoSelfShared { LeftId = 12, RightId = 13 },
                new JoinTwoSelfShared { LeftId = 12, RightId = 14 },
                new JoinTwoSelfShared { LeftId = 13, RightId = 14 },
                new JoinTwoSelfShared { LeftId = 13, RightId = 18 },
                new JoinTwoSelfShared { LeftId = 13, RightId = 19 },
                new JoinTwoSelfShared { LeftId = 16, RightId = 6 },
                new JoinTwoSelfShared { LeftId = 17, RightId = 9 },
                new JoinTwoSelfShared { LeftId = 17, RightId = 19 },
                new JoinTwoSelfShared { LeftId = 17, RightId = 20 },
                new JoinTwoSelfShared { LeftId = 18, RightId = 2 },
                new JoinTwoSelfShared { LeftId = 18, RightId = 5 },
                new JoinTwoSelfShared { LeftId = 18, RightId = 16 },
                new JoinTwoSelfShared { LeftId = 18, RightId = 17 },
                new JoinTwoSelfShared { LeftId = 19, RightId = 2 },
                new JoinTwoSelfShared { LeftId = 20, RightId = 4 }
            };
        private static JoinTwoToCompositeKeyShared[] CreateJoinTwoToCompositeKeyShareds()
            => new[]
            {
                new JoinTwoToCompositeKeyShared { TwoId = 1, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 1, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 1, CompositeId1 = 3, CompositeId2 = "3_3", CompositeId3 = new DateTime(2003, 3, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 2, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 3, CompositeId1 = 6, CompositeId2 = "6_1", CompositeId3 = new DateTime(2006, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 4, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 4, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 6, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 6, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 7, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 9, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 9, CompositeId1 = 8, CompositeId2 = "8_1", CompositeId3 = new DateTime(2008, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 10, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 10, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 10, CompositeId1 = 9, CompositeId2 = "9_5", CompositeId3 = new DateTime(2009, 5, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 11, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 11, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 12, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 12, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 12, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 13, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 13, CompositeId1 = 7, CompositeId2 = "7_1", CompositeId3 = new DateTime(2007, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 13, CompositeId1 = 9, CompositeId2 = "9_4", CompositeId3 = new DateTime(2009, 4, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 15, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 16, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 16, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 16, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 17, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 17, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 17, CompositeId1 = 9, CompositeId2 = "9_1", CompositeId3 = new DateTime(2009, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 17, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 19, CompositeId1 = 3, CompositeId2 = "3_3", CompositeId3 = new DateTime(2003, 3, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 20, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 20, CompositeId1 = 3, CompositeId2 = "3_3", CompositeId3 = new DateTime(2003, 3, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 20, CompositeId1 = 6, CompositeId2 = "6_1", CompositeId3 = new DateTime(2006, 1, 1) },
                new JoinTwoToCompositeKeyShared { TwoId = 20, CompositeId1 = 9, CompositeId2 = "9_1", CompositeId3 = new DateTime(2009, 1, 1) }
            };
        private static JoinThreeToRootShared[] CreateJoinThreeToRootShareds()
            => new[]
            {
                new JoinThreeToRootShared { ThreeId = 1, RootId = 7 },
                new JoinThreeToRootShared { ThreeId = 1, RootId = 8 },
                new JoinThreeToRootShared { ThreeId = 1, RootId = 15 },
                new JoinThreeToRootShared { ThreeId = 2, RootId = 4 },
                new JoinThreeToRootShared { ThreeId = 2, RootId = 16 },
                new JoinThreeToRootShared { ThreeId = 3, RootId = 12 },
                new JoinThreeToRootShared { ThreeId = 3, RootId = 14 },
                new JoinThreeToRootShared { ThreeId = 3, RootId = 24 },
                new JoinThreeToRootShared { ThreeId = 5, RootId = 14 },
                new JoinThreeToRootShared { ThreeId = 5, RootId = 15 },
                new JoinThreeToRootShared { ThreeId = 5, RootId = 16 },
                new JoinThreeToRootShared { ThreeId = 6, RootId = 21 },
                new JoinThreeToRootShared { ThreeId = 7, RootId = 1 },
                new JoinThreeToRootShared { ThreeId = 7, RootId = 6 },
                new JoinThreeToRootShared { ThreeId = 7, RootId = 13 },
                new JoinThreeToRootShared { ThreeId = 7, RootId = 24 },
                new JoinThreeToRootShared { ThreeId = 8, RootId = 10 },
                new JoinThreeToRootShared { ThreeId = 10, RootId = 3 },
                new JoinThreeToRootShared { ThreeId = 10, RootId = 8 },
                new JoinThreeToRootShared { ThreeId = 13, RootId = 5 },
                new JoinThreeToRootShared { ThreeId = 14, RootId = 1 },
                new JoinThreeToRootShared { ThreeId = 14, RootId = 14 },
                new JoinThreeToRootShared { ThreeId = 16, RootId = 5 },
                new JoinThreeToRootShared { ThreeId = 16, RootId = 7 },
                new JoinThreeToRootShared { ThreeId = 17, RootId = 14 },
                new JoinThreeToRootShared { ThreeId = 18, RootId = 6 },
                new JoinThreeToRootShared { ThreeId = 18, RootId = 23 },
                new JoinThreeToRootShared { ThreeId = 19, RootId = 11 },
                new JoinThreeToRootShared { ThreeId = 20, RootId = 14 }
            };
        private static JoinCompositeKeyToRootShared[] CreateJoinCompositeKeyToRootShareds()
            => new[]
            {
                new JoinCompositeKeyToRootShared { RootId = 6, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 9, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 24, CompositeId1 = 1, CompositeId2 = "1_1", CompositeId3 = new DateTime(2001, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 1, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 2, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 4, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 6, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 11, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 22, CompositeId1 = 1, CompositeId2 = "1_2", CompositeId3 = new DateTime(2001, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 4, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 14, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 16, CompositeId1 = 3, CompositeId2 = "3_1", CompositeId3 = new DateTime(2003, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 2, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 3, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 4, CompositeId1 = 3, CompositeId2 = "3_2", CompositeId3 = new DateTime(2003, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 2, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 8, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 16, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 22, CompositeId1 = 7, CompositeId2 = "7_2", CompositeId3 = new DateTime(2007, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 7, CompositeId1 = 8, CompositeId2 = "8_1", CompositeId3 = new DateTime(2008, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 8, CompositeId1 = 8, CompositeId2 = "8_1", CompositeId3 = new DateTime(2008, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 23, CompositeId1 = 8, CompositeId2 = "8_1", CompositeId3 = new DateTime(2008, 1, 1) },
                new JoinCompositeKeyToRootShared { RootId = 3, CompositeId1 = 8, CompositeId2 = "8_2", CompositeId3 = new DateTime(2008, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 12, CompositeId1 = 8, CompositeId2 = "8_2", CompositeId3 = new DateTime(2008, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 22, CompositeId1 = 8, CompositeId2 = "8_2", CompositeId3 = new DateTime(2008, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 2, CompositeId1 = 8, CompositeId2 = "8_3", CompositeId3 = new DateTime(2008, 3, 1) },
                new JoinCompositeKeyToRootShared { RootId = 4, CompositeId1 = 8, CompositeId2 = "8_3", CompositeId3 = new DateTime(2008, 3, 1) },
                new JoinCompositeKeyToRootShared { RootId = 5, CompositeId1 = 8, CompositeId2 = "8_3", CompositeId3 = new DateTime(2008, 3, 1) },
                new JoinCompositeKeyToRootShared { RootId = 7, CompositeId1 = 8, CompositeId2 = "8_4", CompositeId3 = new DateTime(2008, 4, 1) },
                new JoinCompositeKeyToRootShared { RootId = 3, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinCompositeKeyToRootShared { RootId = 8, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinCompositeKeyToRootShared { RootId = 14, CompositeId1 = 8, CompositeId2 = "8_5", CompositeId3 = new DateTime(2008, 5, 1) },
                new JoinCompositeKeyToRootShared { RootId = 4, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 11, CompositeId1 = 9, CompositeId2 = "9_2", CompositeId3 = new DateTime(2009, 2, 1) },
                new JoinCompositeKeyToRootShared { RootId = 1, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinCompositeKeyToRootShared { RootId = 7, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinCompositeKeyToRootShared { RootId = 15, CompositeId1 = 9, CompositeId2 = "9_3", CompositeId3 = new DateTime(2009, 3, 1) },
                new JoinCompositeKeyToRootShared { RootId = 1, CompositeId1 = 9, CompositeId2 = "9_6", CompositeId3 = new DateTime(2009, 6, 1) },
                new JoinCompositeKeyToRootShared { RootId = 6, CompositeId1 = 9, CompositeId2 = "9_7", CompositeId3 = new DateTime(2009, 7, 1) }
            };
    }
}
