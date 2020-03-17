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
        public IReadOnlyList<EntityOne> Ones { get; }
        public IReadOnlyList<EntityTwo> Twos { get; }
        public IReadOnlyList<EntityThree> Threes { get; }
        public IReadOnlyList<EntityCompositeKey> CompositeKeys { get; }
        public IReadOnlyList<EntityRoot> Roots { get; }

        public ManyToManyData()
        {
            Ones = CreateOnes();
            Twos = CreateTwos();
            Threes = CreateThrees();
            CompositeKeys = CreateCompositeKeys();
            Roots = CreateRoots();
        }

        public IQueryable<TEntity> Set<TEntity>() where TEntity : class
        {
            if (typeof(TEntity) == typeof(EntityOne))
            {
                return (IQueryable<TEntity>)Ones.AsQueryable();
            }

            if (typeof(TEntity) == typeof(EntityTwo))
            {
                return (IQueryable<TEntity>)Twos.AsQueryable();
            }

            if (typeof(TEntity) == typeof(EntityThree))
            {
                return (IQueryable<TEntity>)Threes.AsQueryable();
            }

            if (typeof(TEntity) == typeof(EntityCompositeKey))
            {
                return (IQueryable<TEntity>)CompositeKeys.AsQueryable();
            }

            if (typeof(TEntity) == typeof(EntityRoot))
            {
                return (IQueryable<TEntity>)Roots.AsQueryable();
            }

            if (typeof(TEntity) == typeof(EntityBranch))
            {
                return (IQueryable<TEntity>)Roots.OfType<EntityBranch>().AsQueryable();
            }

            if (typeof(TEntity) == typeof(EntityLeaf))
            {
                return (IQueryable<TEntity>)Roots.OfType<EntityLeaf>().AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static IReadOnlyList<EntityOne> CreateOnes()
        {
            var result = new List<EntityOne>
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

            return result;
        }

        public static IReadOnlyList<EntityTwo> CreateTwos()
        {
            var result = new List<EntityTwo>
            {
                new EntityTwo { Id = 1, Name = "EntityTwo 1" },
                new EntityTwo { Id = 2, Name = "EntityTwo 2" },
                new EntityTwo { Id = 3, Name = "EntityTwo 3" },
                new EntityTwo { Id = 4, Name = "EntityTwo 4" },
                new EntityTwo { Id = 5, Name = "EntityTwo 5" },
                new EntityTwo { Id = 6, Name = "EntityTwo 6" },
                new EntityTwo { Id = 7, Name = "EntityTwo 7" },
                new EntityTwo { Id = 8, Name = "EntityTwo 8" },
                new EntityTwo { Id = 9, Name = "EntityTwo 9" },
                new EntityTwo { Id = 10, Name = "EntityTwo 10" },
                new EntityTwo { Id = 11, Name = "EntityTwo 11" },
                new EntityTwo { Id = 12, Name = "EntityTwo 12" },
                new EntityTwo { Id = 13, Name = "EntityTwo 13" },
                new EntityTwo { Id = 14, Name = "EntityTwo 14" },
                new EntityTwo { Id = 15, Name = "EntityTwo 15" },
                new EntityTwo { Id = 16, Name = "EntityTwo 16" },
                new EntityTwo { Id = 17, Name = "EntityTwo 17" },
                new EntityTwo { Id = 18, Name = "EntityTwo 18" },
                new EntityTwo { Id = 19, Name = "EntityTwo 19" },
                new EntityTwo { Id = 20, Name = "EntityTwo 20" },
            };

            return result;
        }

        public static IReadOnlyList<EntityThree> CreateThrees()
        {
            var result = new List<EntityThree>
            {
                new EntityThree { Id = 1, Name = "EntityThree 1" },
                new EntityThree { Id = 2, Name = "EntityThree 2" },
                new EntityThree { Id = 3, Name = "EntityThree 3" },
                new EntityThree { Id = 4, Name = "EntityThree 4" },
                new EntityThree { Id = 5, Name = "EntityThree 5" },
                new EntityThree { Id = 6, Name = "EntityThree 6" },
                new EntityThree { Id = 7, Name = "EntityThree 7" },
                new EntityThree { Id = 8, Name = "EntityThree 8" },
                new EntityThree { Id = 9, Name = "EntityThree 9" },
                new EntityThree { Id = 10, Name = "EntityThree 10" },
                new EntityThree { Id = 11, Name = "EntityThree 11" },
                new EntityThree { Id = 12, Name = "EntityThree 12" },
                new EntityThree { Id = 13, Name = "EntityThree 13" },
                new EntityThree { Id = 14, Name = "EntityThree 14" },
                new EntityThree { Id = 15, Name = "EntityThree 15" },
                new EntityThree { Id = 16, Name = "EntityThree 16" },
                new EntityThree { Id = 17, Name = "EntityThree 17" },
                new EntityThree { Id = 18, Name = "EntityThree 18" },
                new EntityThree { Id = 19, Name = "EntityThree 19" },
                new EntityThree { Id = 20, Name = "EntityThree 20" },
            };

            return result;
        }

        public static IReadOnlyList<EntityCompositeKey> CreateCompositeKeys()
        {
            var result = new List<EntityCompositeKey>
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

            return result;
        }

        public static IReadOnlyList<EntityRoot> CreateRoots()
        {
            var result = new List<EntityRoot>
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
                new EntityLeaf { Id = 17, Name = "Leaf 1", Number = 42, IsGreen = true },
                new EntityLeaf { Id = 18, Name = "Leaf 2", Number = 421, IsGreen = true },
                new EntityLeaf { Id = 19, Name = "Leaf 3", Number = 1337, IsGreen = false },
                new EntityLeaf { Id = 20, Name = "Leaf 4", Number = 1729, IsGreen = false },
            };

            return result;
        }

        private static void CreateRelationship_OneCollection(
            EntityOne one,
            EntityTwo two)
        {
            one.Collection.Add(two);
            two.CollectionInverse = one;
        }

        private static void CreateRelationship_OneReference(
            EntityOne one,
            EntityTwo two)
        {
            one.Reference = two;
            two.ReferenceInverse = one;
        }

        private static void CreateRelationship_OneToOneFullySpecified(
            EntityOne left,
            EntityTwo right)
        {
            left.TwoFullySpecified.Add(right);
            right.OneFullySpecified.Add(left);
        }

        private static void CreateRelationship_OneToThreeFullySpecifiedWithPayload(
            EntityOne left,
            EntityThree right)
        {
            left.ThreeFullySpecifiedWithPayload.Add(right);
            right.OneFullySpecifiedWithPayload.Add(left);
        }

        private static void CreateRelationship_OneToTwoSharedType(
            EntityOne left,
            EntityTwo right)
        {
            left.TwoSharedType.Add(right);
            right.OneSharedType.Add(left);
        }

        private static void CreateRelationship_OneToThreeSharedType(
            EntityOne left,
            EntityThree right)
        {
            left.ThreeSharedType.Add(right);
            right.OneSharedType.Add(left);
        }

        private static void CreateRelationship_OneSelfSharedTypeWithPayload(
            EntityOne left,
            EntityOne right)
        {
            left.SelfSharedTypeRightWithPayload.Add(right);
            right.SelfSharedTypeLeftWithPayload.Add(left);
        }

        private static void CreateRelationship_OneToBranchFullySpecified(
            EntityOne left,
            EntityBranch branch)
        {
            left.BranchFullySpecified.Add(branch);
            branch.OneFullySpecified.Add(left);
        }

        private static void CreateRelationship_TwoCollection(
            EntityTwo two,
            EntityThree three)
        {
            two.Collection.Add(three);
            three.CollectionInverse = two;
        }

        private static void CreateRelationship_TwoReference(
            EntityTwo two,
            EntityThree three)
        {
            two.Reference = three;
            three.ReferenceInverse = two;
        }

        private static void CreateRelationship_TwoToThreeFullySpecified(
            EntityTwo left,
            EntityThree right)
        {
            left.ThreeFullySpecified.Add(right);
            right.TwoFullySpecified.Add(left);
        }

        private static void CreateRelationship_TwoSelfFullySpecified(
            EntityTwo left,
            EntityTwo right)
        {
            left.SelfFullySpecifiedRight.Add(right);
            right.SelfFullySpecifiedLeft.Add(left);
        }

        private static void CreateRelationship_TwoToCompositeSharedType(
            EntityTwo left,
            EntityCompositeKey right)
        {
            left.CompositeSharedType.Add(right);
            right.TwoSharedType.Add(left);
        }

        private static void CreateRelationship_ThreeToCompositeFullySpecified(
            EntityThree left,
            EntityCompositeKey right)
        {
            left.CompositeFullySpecified.Add(right);
            right.ThreeFullySpecified.Add(left);
        }

        private static void CreateRelationship_ThreeToRootSharedType(
            EntityThree left,
            EntityRoot right)
        {
            left.RootSharedType.Add(right);
            right.ThreesSharedType.Add(left);
        }

        private static void CreateRelationship_CompositeKeyToRootSharedType(
            EntityCompositeKey left,
            EntityRoot right)
        {
            left.RootSharedType.Add(right);
            right.CompositeKeySharedType.Add(left);
        }

        private static void CreateRelationship_CompositeKeyToLeafSharedType(
            EntityCompositeKey left,
            EntityLeaf right)
        {
            left.RootSharedType.Add(right);
            right.CompositeKeySharedType.Add(left);
        }

        public static void WireUp(
            IReadOnlyList<EntityOne> ones,
            IReadOnlyList<EntityTwo> twos,
            IReadOnlyList<EntityThree> threes,
            IReadOnlyList<EntityCompositeKey> compositeKeys,
            IReadOnlyList<EntityRoot> roots)
        {
            foreach (var basicOne in ones)
            {
                basicOne.Collection = new List<EntityTwo>();
                basicOne.TwoFullySpecified = new List<EntityTwo>();
                basicOne.ThreeFullySpecifiedWithPayload = new List<EntityThree>();
                basicOne.TwoSharedType = new List<EntityTwo>();
                basicOne.ThreeSharedType = new List<EntityThree>();
                basicOne.SelfSharedTypeLeftWithPayload = new List<EntityOne>();
                basicOne.SelfSharedTypeRightWithPayload = new List<EntityOne>();
                basicOne.BranchFullySpecified = new List<EntityBranch>();
            }

            foreach (var basicTwo in twos)
            {
                basicTwo.Collection = new List<EntityThree>();
                basicTwo.OneFullySpecified = new List<EntityOne>();
                basicTwo.ThreeFullySpecified = new List<EntityThree>();
                basicTwo.OneSharedType = new List<EntityOne>();
                basicTwo.SelfFullySpecifiedLeft = new List<EntityTwo>();
                basicTwo.SelfFullySpecifiedRight = new List<EntityTwo>();
                basicTwo.CompositeSharedType = new List<EntityCompositeKey>();
            }

            foreach (var basicThree in threes)
            {
                basicThree.OneFullySpecifiedWithPayload = new List<EntityOne>();
                basicThree.TwoFullySpecified = new List<EntityTwo>();
                basicThree.OneSharedType = new List<EntityOne>();
                basicThree.CompositeFullySpecified = new List<EntityCompositeKey>();
                basicThree.RootSharedType = new List<EntityRoot>();
            }

            foreach (var compositeKey in compositeKeys)
            {
                compositeKey.TwoSharedType = new List<EntityTwo>();
                compositeKey.ThreeFullySpecified = new List<EntityThree>();
                compositeKey.RootSharedType = new List<EntityRoot>();
                compositeKey.LeafFullySpecified = new List<EntityLeaf>();
            }

            foreach (var root in roots)
            {
                root.ThreesSharedType = new List<EntityThree>();
                root.CompositeKeySharedType = new List<EntityCompositeKey>();
            }

            var branches = roots.OfType<EntityBranch>().ToList();
            foreach (var branch in branches)
            {
                branch.OneFullySpecified = new List<EntityOne>();
            }

            var leaves = branches.OfType<EntityLeaf>().ToList();
            foreach (var leaf in leaves)
            {
                leaf.CompositeKeyFullySpecified = new List<EntityCompositeKey>();
            }

            // ONE

            // Collection
            CreateRelationship_OneCollection(ones[0], twos[0]);
            CreateRelationship_OneCollection(ones[0], twos[1]);
            CreateRelationship_OneCollection(ones[2], twos[3]);
            CreateRelationship_OneCollection(ones[2], twos[4]);
            CreateRelationship_OneCollection(ones[4], twos[5]);
            CreateRelationship_OneCollection(ones[4], twos[6]);
            CreateRelationship_OneCollection(ones[6], twos[7]);
            CreateRelationship_OneCollection(ones[6], twos[8]);
            CreateRelationship_OneCollection(ones[8], twos[9]);
            CreateRelationship_OneCollection(ones[8], twos[10]);
            CreateRelationship_OneCollection(ones[10], twos[11]);
            CreateRelationship_OneCollection(ones[10], twos[12]);
            CreateRelationship_OneCollection(ones[12], twos[13]);
            CreateRelationship_OneCollection(ones[12], twos[14]);
            CreateRelationship_OneCollection(ones[14], twos[15]);
            CreateRelationship_OneCollection(ones[14], twos[16]);
            CreateRelationship_OneCollection(ones[15], twos[17]);
            CreateRelationship_OneCollection(ones[15], twos[18]);
            CreateRelationship_OneCollection(ones[16], twos[19]);

            // Reference
            CreateRelationship_OneReference(ones[0], twos[19]);
            CreateRelationship_OneReference(ones[2], twos[18]);
            CreateRelationship_OneReference(ones[4], twos[17]);
            CreateRelationship_OneReference(ones[6], twos[16]);
            CreateRelationship_OneReference(ones[8], twos[15]);
            CreateRelationship_OneReference(ones[10], twos[14]);
            CreateRelationship_OneReference(ones[11], twos[13]);
            CreateRelationship_OneReference(ones[13], twos[12]);
            CreateRelationship_OneReference(ones[15], twos[11]);
            CreateRelationship_OneReference(ones[17], twos[10]);
            CreateRelationship_OneReference(ones[19], twos[9]);

            // ManyToMany two fully specified
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[1]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[2]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[3]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[4]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[5]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[6]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[7]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[8]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[9]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[10]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[11]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[12]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[13]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[14]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[15]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[16]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[17]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[18]);
            CreateRelationship_OneToOneFullySpecified(ones[0], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[2]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[4]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[6]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[8]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[10]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[12]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[14]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[16]);
            CreateRelationship_OneToOneFullySpecified(ones[1], twos[18]);
            CreateRelationship_OneToOneFullySpecified(ones[2], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[2], twos[3]);
            CreateRelationship_OneToOneFullySpecified(ones[2], twos[6]);
            CreateRelationship_OneToOneFullySpecified(ones[2], twos[9]);
            CreateRelationship_OneToOneFullySpecified(ones[2], twos[12]);
            CreateRelationship_OneToOneFullySpecified(ones[2], twos[15]);
            CreateRelationship_OneToOneFullySpecified(ones[2], twos[18]);
            CreateRelationship_OneToOneFullySpecified(ones[3], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[3], twos[4]);
            CreateRelationship_OneToOneFullySpecified(ones[3], twos[8]);
            CreateRelationship_OneToOneFullySpecified(ones[3], twos[12]);
            CreateRelationship_OneToOneFullySpecified(ones[3], twos[16]);
            CreateRelationship_OneToOneFullySpecified(ones[4], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[4], twos[5]);
            CreateRelationship_OneToOneFullySpecified(ones[4], twos[10]);
            CreateRelationship_OneToOneFullySpecified(ones[4], twos[15]);
            CreateRelationship_OneToOneFullySpecified(ones[5], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[5], twos[6]);
            CreateRelationship_OneToOneFullySpecified(ones[5], twos[12]);
            CreateRelationship_OneToOneFullySpecified(ones[5], twos[18]);
            CreateRelationship_OneToOneFullySpecified(ones[6], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[6], twos[7]);
            CreateRelationship_OneToOneFullySpecified(ones[6], twos[14]);
            CreateRelationship_OneToOneFullySpecified(ones[7], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[7], twos[8]);
            CreateRelationship_OneToOneFullySpecified(ones[7], twos[16]);
            CreateRelationship_OneToOneFullySpecified(ones[8], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[8], twos[9]);
            CreateRelationship_OneToOneFullySpecified(ones[8], twos[18]);
            CreateRelationship_OneToOneFullySpecified(ones[9], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[9], twos[10]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[18]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[17]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[16]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[15]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[14]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[13]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[12]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[11]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[10]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[9]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[8]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[7]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[6]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[5]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[4]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[3]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[2]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[1]);
            CreateRelationship_OneToOneFullySpecified(ones[10], twos[0]);
            CreateRelationship_OneToOneFullySpecified(ones[11], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[11], twos[16]);
            CreateRelationship_OneToOneFullySpecified(ones[11], twos[13]);
            CreateRelationship_OneToOneFullySpecified(ones[11], twos[10]);
            CreateRelationship_OneToOneFullySpecified(ones[11], twos[7]);
            CreateRelationship_OneToOneFullySpecified(ones[11], twos[4]);
            CreateRelationship_OneToOneFullySpecified(ones[11], twos[1]);
            CreateRelationship_OneToOneFullySpecified(ones[12], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[12], twos[15]);
            CreateRelationship_OneToOneFullySpecified(ones[12], twos[11]);
            CreateRelationship_OneToOneFullySpecified(ones[12], twos[7]);
            CreateRelationship_OneToOneFullySpecified(ones[12], twos[3]);
            CreateRelationship_OneToOneFullySpecified(ones[13], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[13], twos[14]);
            CreateRelationship_OneToOneFullySpecified(ones[13], twos[9]);
            CreateRelationship_OneToOneFullySpecified(ones[13], twos[4]);
            CreateRelationship_OneToOneFullySpecified(ones[14], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[14], twos[13]);
            CreateRelationship_OneToOneFullySpecified(ones[14], twos[7]);
            CreateRelationship_OneToOneFullySpecified(ones[14], twos[1]);
            CreateRelationship_OneToOneFullySpecified(ones[15], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[15], twos[12]);
            CreateRelationship_OneToOneFullySpecified(ones[15], twos[5]);
            CreateRelationship_OneToOneFullySpecified(ones[16], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[16], twos[11]);
            CreateRelationship_OneToOneFullySpecified(ones[16], twos[3]);
            CreateRelationship_OneToOneFullySpecified(ones[17], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[17], twos[10]);
            CreateRelationship_OneToOneFullySpecified(ones[17], twos[1]);
            CreateRelationship_OneToOneFullySpecified(ones[18], twos[19]);
            CreateRelationship_OneToOneFullySpecified(ones[18], twos[9]);

            // ManyToMany three fully specified with payload
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[0], threes[1]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[0], threes[5]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[0], threes[8]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[0], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[0], threes[16]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[1], threes[8]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[1], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[1], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[1], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[1], threes[15]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[2], threes[4]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[2], threes[11]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[2], threes[15]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[2], threes[18]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[3], threes[1]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[3], threes[3]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[3], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[3], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[3], threes[18]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[4], threes[3]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[4], threes[5]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[4], threes[8]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[6], threes[0]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[6], threes[3]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[6], threes[8]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[6], threes[9]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[6], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[6], threes[11]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[6], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[0]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[2]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[3]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[16]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[7], threes[19]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[8], threes[1]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[8], threes[8]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[8], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[8], threes[11]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[8], threes[14]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[8], threes[16]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[1]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[2]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[3]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[7]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[9]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[15]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[9], threes[18]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[0]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[5]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[7]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[8]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[9]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[10], threes[18]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[11], threes[1]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[5]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[8]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[9]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[11]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[14]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[15]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[12], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[13], threes[16]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[13], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[14], threes[7]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[14], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[14], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[14], threes[14]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[14], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[14], threes[19]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[15], threes[0]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[15], threes[5]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[15], threes[9]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[15], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[15], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[15], threes[14]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[15], threes[15]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[16], threes[1]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[16], threes[4]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[16], threes[6]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[16], threes[12]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[16], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[16], threes[14]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[17], threes[2]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[17], threes[6]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[17], threes[11]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[17], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[1]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[4]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[5]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[6]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[9]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[14]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[15]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[16]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[18], threes[19]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[2]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[4]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[5]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[9]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[10]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[11]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[13]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[17]);
            CreateRelationship_OneToThreeFullySpecifiedWithPayload(ones[19], threes[19]);

            // ManyToMany two shared type 
            CreateRelationship_OneToTwoSharedType(ones[0], twos[2]);
            CreateRelationship_OneToTwoSharedType(ones[0], twos[15]);
            CreateRelationship_OneToTwoSharedType(ones[1], twos[2]);
            CreateRelationship_OneToTwoSharedType(ones[1], twos[9]);
            CreateRelationship_OneToTwoSharedType(ones[1], twos[17]);
            CreateRelationship_OneToTwoSharedType(ones[2], twos[9]);
            CreateRelationship_OneToTwoSharedType(ones[2], twos[10]);
            CreateRelationship_OneToTwoSharedType(ones[2], twos[15]);
            CreateRelationship_OneToTwoSharedType(ones[4], twos[1]);
            CreateRelationship_OneToTwoSharedType(ones[4], twos[4]);
            CreateRelationship_OneToTwoSharedType(ones[4], twos[6]);
            CreateRelationship_OneToTwoSharedType(ones[4], twos[8]);
            CreateRelationship_OneToTwoSharedType(ones[4], twos[13]);
            CreateRelationship_OneToTwoSharedType(ones[5], twos[11]);
            CreateRelationship_OneToTwoSharedType(ones[6], twos[2]);
            CreateRelationship_OneToTwoSharedType(ones[6], twos[15]);
            CreateRelationship_OneToTwoSharedType(ones[6], twos[16]);
            CreateRelationship_OneToTwoSharedType(ones[7], twos[18]);
            CreateRelationship_OneToTwoSharedType(ones[8], twos[8]);
            CreateRelationship_OneToTwoSharedType(ones[8], twos[10]);
            CreateRelationship_OneToTwoSharedType(ones[9], twos[5]);
            CreateRelationship_OneToTwoSharedType(ones[9], twos[16]);
            CreateRelationship_OneToTwoSharedType(ones[9], twos[19]);
            CreateRelationship_OneToTwoSharedType(ones[10], twos[16]);
            CreateRelationship_OneToTwoSharedType(ones[10], twos[17]);
            CreateRelationship_OneToTwoSharedType(ones[11], twos[5]);
            CreateRelationship_OneToTwoSharedType(ones[11], twos[18]);
            CreateRelationship_OneToTwoSharedType(ones[12], twos[6]);
            CreateRelationship_OneToTwoSharedType(ones[12], twos[7]);
            CreateRelationship_OneToTwoSharedType(ones[12], twos[8]);
            CreateRelationship_OneToTwoSharedType(ones[12], twos[12]);
            CreateRelationship_OneToTwoSharedType(ones[13], twos[3]);
            CreateRelationship_OneToTwoSharedType(ones[13], twos[8]);
            CreateRelationship_OneToTwoSharedType(ones[13], twos[18]);
            CreateRelationship_OneToTwoSharedType(ones[14], twos[9]);
            CreateRelationship_OneToTwoSharedType(ones[15], twos[0]);
            CreateRelationship_OneToTwoSharedType(ones[15], twos[6]);
            CreateRelationship_OneToTwoSharedType(ones[15], twos[18]);
            CreateRelationship_OneToTwoSharedType(ones[16], twos[7]);
            CreateRelationship_OneToTwoSharedType(ones[16], twos[14]);
            CreateRelationship_OneToTwoSharedType(ones[17], twos[3]);
            CreateRelationship_OneToTwoSharedType(ones[17], twos[12]);
            CreateRelationship_OneToTwoSharedType(ones[17], twos[13]);
            CreateRelationship_OneToTwoSharedType(ones[18], twos[3]);
            CreateRelationship_OneToTwoSharedType(ones[18], twos[13]);

            // ManyToMany three shared type 
            CreateRelationship_OneToThreeSharedType(ones[2], threes[0]);
            CreateRelationship_OneToThreeSharedType(ones[2], threes[1]);
            CreateRelationship_OneToThreeSharedType(ones[2], threes[3]);
            CreateRelationship_OneToThreeSharedType(ones[2], threes[8]);
            CreateRelationship_OneToThreeSharedType(ones[3], threes[4]);
            CreateRelationship_OneToThreeSharedType(ones[3], threes[17]);
            CreateRelationship_OneToThreeSharedType(ones[4], threes[0]);
            CreateRelationship_OneToThreeSharedType(ones[4], threes[3]);
            CreateRelationship_OneToThreeSharedType(ones[4], threes[8]);
            CreateRelationship_OneToThreeSharedType(ones[4], threes[15]);
            CreateRelationship_OneToThreeSharedType(ones[5], threes[10]);
            CreateRelationship_OneToThreeSharedType(ones[5], threes[12]);
            CreateRelationship_OneToThreeSharedType(ones[6], threes[3]);
            CreateRelationship_OneToThreeSharedType(ones[7], threes[10]);
            CreateRelationship_OneToThreeSharedType(ones[7], threes[17]);
            CreateRelationship_OneToThreeSharedType(ones[7], threes[18]);
            CreateRelationship_OneToThreeSharedType(ones[9], threes[0]);
            CreateRelationship_OneToThreeSharedType(ones[9], threes[4]);
            CreateRelationship_OneToThreeSharedType(ones[9], threes[19]);
            CreateRelationship_OneToThreeSharedType(ones[10], threes[9]);
            CreateRelationship_OneToThreeSharedType(ones[11], threes[10]);
            CreateRelationship_OneToThreeSharedType(ones[11], threes[16]);
            CreateRelationship_OneToThreeSharedType(ones[12], threes[2]);
            CreateRelationship_OneToThreeSharedType(ones[12], threes[13]);
            CreateRelationship_OneToThreeSharedType(ones[12], threes[15]);
            CreateRelationship_OneToThreeSharedType(ones[13], threes[5]);
            CreateRelationship_OneToThreeSharedType(ones[13], threes[10]);
            CreateRelationship_OneToThreeSharedType(ones[13], threes[16]);
            CreateRelationship_OneToThreeSharedType(ones[14], threes[9]);
            CreateRelationship_OneToThreeSharedType(ones[14], threes[12]);
            CreateRelationship_OneToThreeSharedType(ones[15], threes[6]);
            CreateRelationship_OneToThreeSharedType(ones[15], threes[12]);
            CreateRelationship_OneToThreeSharedType(ones[16], threes[8]);
            CreateRelationship_OneToThreeSharedType(ones[16], threes[12]);
            CreateRelationship_OneToThreeSharedType(ones[17], threes[7]);
            CreateRelationship_OneToThreeSharedType(ones[17], threes[11]);
            CreateRelationship_OneToThreeSharedType(ones[17], threes[12]);
            CreateRelationship_OneToThreeSharedType(ones[19], threes[3]);
            CreateRelationship_OneToThreeSharedType(ones[19], threes[4]);
            CreateRelationship_OneToThreeSharedType(ones[19], threes[15]);

            // ManyToMany self shared type with payload
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[2], ones[3]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[2], ones[5]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[2], ones[7]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[2], ones[17]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[2], ones[19]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[4], ones[2]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[4], ones[3]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[5], ones[4]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[5], ones[6]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[5], ones[12]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[6], ones[12]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[7], ones[8]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[7], ones[10]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[7], ones[11]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[9], ones[6]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[12], ones[1]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[12], ones[17]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[13], ones[8]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[14], ones[12]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[15], ones[4]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[15], ones[5]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[16], ones[13]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[18], ones[0]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[18], ones[7]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[18], ones[11]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[19], ones[0]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[19], ones[6]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[19], ones[13]);
            CreateRelationship_OneSelfSharedTypeWithPayload(ones[19], ones[15]);

            // ManyToMany branch fully specified
            CreateRelationship_OneToBranchFullySpecified(ones[1], branches[5]);
            CreateRelationship_OneToBranchFullySpecified(ones[1], branches[9]);
            CreateRelationship_OneToBranchFullySpecified(ones[2], branches[3]);
            CreateRelationship_OneToBranchFullySpecified(ones[2], branches[5]);
            CreateRelationship_OneToBranchFullySpecified(ones[2], branches[7]);
            CreateRelationship_OneToBranchFullySpecified(ones[2], branches[9]);
            CreateRelationship_OneToBranchFullySpecified(ones[4], branches[2]);
            CreateRelationship_OneToBranchFullySpecified(ones[5], branches[5]);
            CreateRelationship_OneToBranchFullySpecified(ones[5], branches[7]);
            CreateRelationship_OneToBranchFullySpecified(ones[5], branches[8]);
            CreateRelationship_OneToBranchFullySpecified(ones[7], branches[0]);
            CreateRelationship_OneToBranchFullySpecified(ones[7], branches[1]);
            CreateRelationship_OneToBranchFullySpecified(ones[7], branches[2]);
            CreateRelationship_OneToBranchFullySpecified(ones[8], branches[0]);
            CreateRelationship_OneToBranchFullySpecified(ones[8], branches[1]);
            CreateRelationship_OneToBranchFullySpecified(ones[8], branches[3]);
            CreateRelationship_OneToBranchFullySpecified(ones[8], branches[5]);
            CreateRelationship_OneToBranchFullySpecified(ones[8], branches[6]);
            CreateRelationship_OneToBranchFullySpecified(ones[8], branches[9]);
            CreateRelationship_OneToBranchFullySpecified(ones[9], branches[1]);
            CreateRelationship_OneToBranchFullySpecified(ones[9], branches[2]);
            CreateRelationship_OneToBranchFullySpecified(ones[9], branches[3]);
            CreateRelationship_OneToBranchFullySpecified(ones[9], branches[6]);
            CreateRelationship_OneToBranchFullySpecified(ones[11], branches[0]);
            CreateRelationship_OneToBranchFullySpecified(ones[11], branches[1]);
            CreateRelationship_OneToBranchFullySpecified(ones[11], branches[3]);
            CreateRelationship_OneToBranchFullySpecified(ones[11], branches[8]);
            CreateRelationship_OneToBranchFullySpecified(ones[12], branches[4]);
            CreateRelationship_OneToBranchFullySpecified(ones[13], branches[1]);
            CreateRelationship_OneToBranchFullySpecified(ones[13], branches[3]);
            CreateRelationship_OneToBranchFullySpecified(ones[13], branches[5]);
            CreateRelationship_OneToBranchFullySpecified(ones[13], branches[8]);
            CreateRelationship_OneToBranchFullySpecified(ones[14], branches[4]);
            CreateRelationship_OneToBranchFullySpecified(ones[14], branches[5]);
            CreateRelationship_OneToBranchFullySpecified(ones[14], branches[9]);
            CreateRelationship_OneToBranchFullySpecified(ones[15], branches[0]);
            CreateRelationship_OneToBranchFullySpecified(ones[16], branches[0]);
            CreateRelationship_OneToBranchFullySpecified(ones[16], branches[6]);
            CreateRelationship_OneToBranchFullySpecified(ones[17], branches[1]);
            CreateRelationship_OneToBranchFullySpecified(ones[17], branches[4]);
            CreateRelationship_OneToBranchFullySpecified(ones[17], branches[9]);
            CreateRelationship_OneToBranchFullySpecified(ones[18], branches[0]);
            CreateRelationship_OneToBranchFullySpecified(ones[18], branches[1]);
            CreateRelationship_OneToBranchFullySpecified(ones[18], branches[5]);
            CreateRelationship_OneToBranchFullySpecified(ones[18], branches[8]);
            CreateRelationship_OneToBranchFullySpecified(ones[19], branches[6]);
            CreateRelationship_OneToBranchFullySpecified(ones[19], branches[8]);

            // TWO

            // Collection
            CreateRelationship_TwoCollection(twos[0], threes[19]);
            CreateRelationship_TwoCollection(twos[0], threes[18]);
            CreateRelationship_TwoCollection(twos[2], threes[17]);
            CreateRelationship_TwoCollection(twos[2], threes[16]);
            CreateRelationship_TwoCollection(twos[4], threes[15]);
            CreateRelationship_TwoCollection(twos[4], threes[14]);
            CreateRelationship_TwoCollection(twos[6], threes[13]);
            CreateRelationship_TwoCollection(twos[6], threes[12]);
            CreateRelationship_TwoCollection(twos[8], threes[11]);
            CreateRelationship_TwoCollection(twos[8], threes[10]);
            CreateRelationship_TwoCollection(twos[10], threes[9]);
            CreateRelationship_TwoCollection(twos[10], threes[8]);
            CreateRelationship_TwoCollection(twos[12], threes[7]);
            CreateRelationship_TwoCollection(twos[12], threes[6]);
            CreateRelationship_TwoCollection(twos[14], threes[5]);
            CreateRelationship_TwoCollection(twos[14], threes[4]);
            CreateRelationship_TwoCollection(twos[15], threes[3]);
            CreateRelationship_TwoCollection(twos[15], threes[2]);
            CreateRelationship_TwoCollection(twos[16], threes[1]);

            // Reference
            CreateRelationship_TwoReference(twos[1], threes[2]);
            CreateRelationship_TwoReference(twos[3], threes[4]);
            CreateRelationship_TwoReference(twos[5], threes[6]);
            CreateRelationship_TwoReference(twos[7], threes[8]);
            CreateRelationship_TwoReference(twos[9], threes[10]);
            CreateRelationship_TwoReference(twos[11], threes[12]);
            CreateRelationship_TwoReference(twos[13], threes[14]);
            CreateRelationship_TwoReference(twos[15], threes[16]);
            CreateRelationship_TwoReference(twos[17], threes[18]);
            CreateRelationship_TwoReference(twos[18], threes[1]);
            CreateRelationship_TwoReference(twos[19], threes[3]);

            // ManyToMany two fully specified
            CreateRelationship_TwoToThreeFullySpecified(twos[0], threes[1]);
            CreateRelationship_TwoToThreeFullySpecified(twos[0], threes[2]);
            CreateRelationship_TwoToThreeFullySpecified(twos[0], threes[12]);
            CreateRelationship_TwoToThreeFullySpecified(twos[0], threes[17]);
            CreateRelationship_TwoToThreeFullySpecified(twos[1], threes[0]);
            CreateRelationship_TwoToThreeFullySpecified(twos[1], threes[8]);
            CreateRelationship_TwoToThreeFullySpecified(twos[1], threes[14]);
            CreateRelationship_TwoToThreeFullySpecified(twos[2], threes[10]);
            CreateRelationship_TwoToThreeFullySpecified(twos[2], threes[16]);
            CreateRelationship_TwoToThreeFullySpecified(twos[3], threes[1]);
            CreateRelationship_TwoToThreeFullySpecified(twos[3], threes[4]);
            CreateRelationship_TwoToThreeFullySpecified(twos[3], threes[10]);
            CreateRelationship_TwoToThreeFullySpecified(twos[4], threes[3]);
            CreateRelationship_TwoToThreeFullySpecified(twos[4], threes[4]);
            CreateRelationship_TwoToThreeFullySpecified(twos[5], threes[2]);
            CreateRelationship_TwoToThreeFullySpecified(twos[5], threes[9]);
            CreateRelationship_TwoToThreeFullySpecified(twos[5], threes[15]);
            CreateRelationship_TwoToThreeFullySpecified(twos[5], threes[17]);
            CreateRelationship_TwoToThreeFullySpecified(twos[6], threes[11]);
            CreateRelationship_TwoToThreeFullySpecified(twos[6], threes[14]);
            CreateRelationship_TwoToThreeFullySpecified(twos[6], threes[19]);
            CreateRelationship_TwoToThreeFullySpecified(twos[7], threes[0]);
            CreateRelationship_TwoToThreeFullySpecified(twos[7], threes[2]);
            CreateRelationship_TwoToThreeFullySpecified(twos[7], threes[19]);
            CreateRelationship_TwoToThreeFullySpecified(twos[8], threes[2]);
            CreateRelationship_TwoToThreeFullySpecified(twos[8], threes[12]);
            CreateRelationship_TwoToThreeFullySpecified(twos[8], threes[18]);
            CreateRelationship_TwoToThreeFullySpecified(twos[9], threes[16]);
            CreateRelationship_TwoToThreeFullySpecified(twos[10], threes[5]);
            CreateRelationship_TwoToThreeFullySpecified(twos[10], threes[6]);
            CreateRelationship_TwoToThreeFullySpecified(twos[10], threes[7]);
            CreateRelationship_TwoToThreeFullySpecified(twos[10], threes[12]);
            CreateRelationship_TwoToThreeFullySpecified(twos[11], threes[8]);
            CreateRelationship_TwoToThreeFullySpecified(twos[12], threes[0]);
            CreateRelationship_TwoToThreeFullySpecified(twos[12], threes[10]);
            CreateRelationship_TwoToThreeFullySpecified(twos[12], threes[18]);
            CreateRelationship_TwoToThreeFullySpecified(twos[13], threes[1]);
            CreateRelationship_TwoToThreeFullySpecified(twos[14], threes[16]);
            CreateRelationship_TwoToThreeFullySpecified(twos[15], threes[2]);
            CreateRelationship_TwoToThreeFullySpecified(twos[15], threes[15]);
            CreateRelationship_TwoToThreeFullySpecified(twos[17], threes[0]);
            CreateRelationship_TwoToThreeFullySpecified(twos[17], threes[4]);
            CreateRelationship_TwoToThreeFullySpecified(twos[17], threes[9]);
            CreateRelationship_TwoToThreeFullySpecified(twos[18], threes[4]);
            CreateRelationship_TwoToThreeFullySpecified(twos[18], threes[15]);
            CreateRelationship_TwoToThreeFullySpecified(twos[18], threes[17]);
            CreateRelationship_TwoToThreeFullySpecified(twos[19], threes[5]);
            CreateRelationship_TwoToThreeFullySpecified(twos[19], threes[9]);
            CreateRelationship_TwoToThreeFullySpecified(twos[19], threes[11]);
            CreateRelationship_TwoToThreeFullySpecified(twos[19], threes[15]);
            CreateRelationship_TwoToThreeFullySpecified(twos[19], threes[16]);
            CreateRelationship_TwoToThreeFullySpecified(twos[19], threes[17]);

            // ManyToMany self fully specified
            CreateRelationship_TwoSelfFullySpecified(twos[0], twos[8]);
            CreateRelationship_TwoSelfFullySpecified(twos[0], twos[9]);
            CreateRelationship_TwoSelfFullySpecified(twos[0], twos[10]);
            CreateRelationship_TwoSelfFullySpecified(twos[0], twos[17]);
            CreateRelationship_TwoSelfFullySpecified(twos[2], twos[1]);
            CreateRelationship_TwoSelfFullySpecified(twos[2], twos[4]);
            CreateRelationship_TwoSelfFullySpecified(twos[2], twos[7]);
            CreateRelationship_TwoSelfFullySpecified(twos[2], twos[17]);
            CreateRelationship_TwoSelfFullySpecified(twos[2], twos[18]);
            CreateRelationship_TwoSelfFullySpecified(twos[3], twos[10]);
            CreateRelationship_TwoSelfFullySpecified(twos[4], twos[7]);
            CreateRelationship_TwoSelfFullySpecified(twos[5], twos[17]);
            CreateRelationship_TwoSelfFullySpecified(twos[7], twos[1]);
            CreateRelationship_TwoSelfFullySpecified(twos[7], twos[13]);
            CreateRelationship_TwoSelfFullySpecified(twos[7], twos[14]);
            CreateRelationship_TwoSelfFullySpecified(twos[7], twos[19]);
            CreateRelationship_TwoSelfFullySpecified(twos[8], twos[3]);
            CreateRelationship_TwoSelfFullySpecified(twos[8], twos[13]);
            CreateRelationship_TwoSelfFullySpecified(twos[9], twos[4]);
            CreateRelationship_TwoSelfFullySpecified(twos[11], twos[12]);
            CreateRelationship_TwoSelfFullySpecified(twos[11], twos[13]);
            CreateRelationship_TwoSelfFullySpecified(twos[12], twos[13]);
            CreateRelationship_TwoSelfFullySpecified(twos[12], twos[17]);
            CreateRelationship_TwoSelfFullySpecified(twos[12], twos[18]);
            CreateRelationship_TwoSelfFullySpecified(twos[15], twos[5]);
            CreateRelationship_TwoSelfFullySpecified(twos[16], twos[8]);
            CreateRelationship_TwoSelfFullySpecified(twos[16], twos[18]);
            CreateRelationship_TwoSelfFullySpecified(twos[16], twos[19]);
            CreateRelationship_TwoSelfFullySpecified(twos[17], twos[1]);
            CreateRelationship_TwoSelfFullySpecified(twos[17], twos[4]);
            CreateRelationship_TwoSelfFullySpecified(twos[17], twos[15]);
            CreateRelationship_TwoSelfFullySpecified(twos[17], twos[16]);
            CreateRelationship_TwoSelfFullySpecified(twos[18], twos[1]);
            CreateRelationship_TwoSelfFullySpecified(twos[19], twos[3]);

            // ManyToMany composite shared type
            CreateRelationship_TwoToCompositeSharedType(twos[0], compositeKeys[0]);
            CreateRelationship_TwoToCompositeSharedType(twos[0], compositeKeys[3]);
            CreateRelationship_TwoToCompositeSharedType(twos[0], compositeKeys[4]);
            CreateRelationship_TwoToCompositeSharedType(twos[1], compositeKeys[3]);
            CreateRelationship_TwoToCompositeSharedType(twos[2], compositeKeys[5]);
            CreateRelationship_TwoToCompositeSharedType(twos[3], compositeKeys[1]);
            CreateRelationship_TwoToCompositeSharedType(twos[3], compositeKeys[18]);
            CreateRelationship_TwoToCompositeSharedType(twos[5], compositeKeys[2]);
            CreateRelationship_TwoToCompositeSharedType(twos[5], compositeKeys[12]);
            CreateRelationship_TwoToCompositeSharedType(twos[6], compositeKeys[7]);
            CreateRelationship_TwoToCompositeSharedType(twos[8], compositeKeys[2]);
            CreateRelationship_TwoToCompositeSharedType(twos[8], compositeKeys[8]);
            CreateRelationship_TwoToCompositeSharedType(twos[9], compositeKeys[0]);
            CreateRelationship_TwoToCompositeSharedType(twos[9], compositeKeys[14]);
            CreateRelationship_TwoToCompositeSharedType(twos[9], compositeKeys[17]);
            CreateRelationship_TwoToCompositeSharedType(twos[10], compositeKeys[0]);
            CreateRelationship_TwoToCompositeSharedType(twos[10], compositeKeys[14]);
            CreateRelationship_TwoToCompositeSharedType(twos[11], compositeKeys[7]);
            CreateRelationship_TwoToCompositeSharedType(twos[11], compositeKeys[12]);
            CreateRelationship_TwoToCompositeSharedType(twos[11], compositeKeys[14]);
            CreateRelationship_TwoToCompositeSharedType(twos[12], compositeKeys[0]);
            CreateRelationship_TwoToCompositeSharedType(twos[12], compositeKeys[6]);
            CreateRelationship_TwoToCompositeSharedType(twos[12], compositeKeys[16]);
            CreateRelationship_TwoToCompositeSharedType(twos[14], compositeKeys[15]);
            CreateRelationship_TwoToCompositeSharedType(twos[15], compositeKeys[0]);
            CreateRelationship_TwoToCompositeSharedType(twos[15], compositeKeys[2]);
            CreateRelationship_TwoToCompositeSharedType(twos[15], compositeKeys[18]);
            CreateRelationship_TwoToCompositeSharedType(twos[16], compositeKeys[1]);
            CreateRelationship_TwoToCompositeSharedType(twos[16], compositeKeys[7]);
            CreateRelationship_TwoToCompositeSharedType(twos[16], compositeKeys[13]);
            CreateRelationship_TwoToCompositeSharedType(twos[16], compositeKeys[14]);
            CreateRelationship_TwoToCompositeSharedType(twos[18], compositeKeys[4]);
            CreateRelationship_TwoToCompositeSharedType(twos[19], compositeKeys[2]);
            CreateRelationship_TwoToCompositeSharedType(twos[19], compositeKeys[4]);
            CreateRelationship_TwoToCompositeSharedType(twos[19], compositeKeys[5]);
            CreateRelationship_TwoToCompositeSharedType(twos[19], compositeKeys[13]);

            // THREE

            // ManyToMany composite fully specified
            CreateRelationship_ThreeToCompositeFullySpecified(threes[0], compositeKeys[5]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[1], compositeKeys[0]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[1], compositeKeys[14]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[1], compositeKeys[19]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[2], compositeKeys[5]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[2], compositeKeys[14]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[2], compositeKeys[19]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[4], compositeKeys[11]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[4], compositeKeys[12]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[4], compositeKeys[17]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[5], compositeKeys[5]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[6], compositeKeys[3]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[6], compositeKeys[8]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[7], compositeKeys[10]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[7], compositeKeys[18]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[8], compositeKeys[8]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[8], compositeKeys[15]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[9], compositeKeys[15]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[10], compositeKeys[6]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[10], compositeKeys[14]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[11], compositeKeys[7]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[11], compositeKeys[10]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[11], compositeKeys[12]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[12], compositeKeys[5]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[12], compositeKeys[7]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[12], compositeKeys[13]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[12], compositeKeys[14]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[13], compositeKeys[9]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[13], compositeKeys[12]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[13], compositeKeys[15]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[14], compositeKeys[9]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[14], compositeKeys[13]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[14], compositeKeys[18]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[15], compositeKeys[4]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[15], compositeKeys[6]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[15], compositeKeys[18]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[16], compositeKeys[1]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[16], compositeKeys[9]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[17], compositeKeys[3]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[18], compositeKeys[1]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[18], compositeKeys[12]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[18], compositeKeys[14]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[18], compositeKeys[19]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[19], compositeKeys[3]);
            CreateRelationship_ThreeToCompositeFullySpecified(threes[19], compositeKeys[6]);

            // ManyToMany root shared type
            CreateRelationship_ThreeToRootSharedType(threes[0], roots[6]);
            CreateRelationship_ThreeToRootSharedType(threes[0], roots[7]);
            CreateRelationship_ThreeToRootSharedType(threes[0], roots[14]);
            CreateRelationship_ThreeToRootSharedType(threes[1], roots[3]);
            CreateRelationship_ThreeToRootSharedType(threes[1], roots[15]);
            CreateRelationship_ThreeToRootSharedType(threes[2], roots[11]);
            CreateRelationship_ThreeToRootSharedType(threes[2], roots[13]);
            CreateRelationship_ThreeToRootSharedType(threes[2], roots[19]);
            CreateRelationship_ThreeToRootSharedType(threes[4], roots[13]);
            CreateRelationship_ThreeToRootSharedType(threes[4], roots[14]);
            CreateRelationship_ThreeToRootSharedType(threes[4], roots[15]);
            CreateRelationship_ThreeToRootSharedType(threes[5], roots[16]);
            CreateRelationship_ThreeToRootSharedType(threes[6], roots[0]);
            CreateRelationship_ThreeToRootSharedType(threes[6], roots[5]);
            CreateRelationship_ThreeToRootSharedType(threes[6], roots[12]);
            CreateRelationship_ThreeToRootSharedType(threes[6], roots[19]);
            CreateRelationship_ThreeToRootSharedType(threes[7], roots[9]);
            CreateRelationship_ThreeToRootSharedType(threes[9], roots[2]);
            CreateRelationship_ThreeToRootSharedType(threes[9], roots[7]);
            CreateRelationship_ThreeToRootSharedType(threes[12], roots[4]);
            CreateRelationship_ThreeToRootSharedType(threes[13], roots[0]);
            CreateRelationship_ThreeToRootSharedType(threes[13], roots[13]);
            CreateRelationship_ThreeToRootSharedType(threes[15], roots[4]);
            CreateRelationship_ThreeToRootSharedType(threes[15], roots[6]);
            CreateRelationship_ThreeToRootSharedType(threes[16], roots[13]);
            CreateRelationship_ThreeToRootSharedType(threes[17], roots[5]);
            CreateRelationship_ThreeToRootSharedType(threes[17], roots[18]);
            CreateRelationship_ThreeToRootSharedType(threes[18], roots[10]);
            CreateRelationship_ThreeToRootSharedType(threes[19], roots[13]);

            // COMPOSITE KEY

            // ManyToMany root shared type
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[0], roots[5]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[0], roots[8]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[0], roots[19]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[1], roots[0]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[1], roots[1]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[1], roots[3]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[1], roots[5]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[1], roots[10]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[1], roots[17]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[2], roots[3]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[2], roots[13]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[2], roots[15]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[3], roots[1]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[3], roots[2]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[3], roots[3]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[7], roots[1]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[7], roots[7]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[7], roots[15]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[7], roots[17]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[8], roots[6]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[8], roots[7]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[8], roots[18]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[9], roots[2]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[9], roots[11]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[9], roots[17]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[10], roots[1]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[10], roots[3]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[10], roots[4]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[11], roots[6]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[12], roots[2]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[12], roots[7]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[12], roots[13]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[14], roots[3]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[14], roots[10]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[15], roots[0]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[15], roots[6]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[15], roots[14]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[18], roots[0]);
            CreateRelationship_CompositeKeyToRootSharedType(compositeKeys[19], roots[5]);

            // ManyToMany leaves shared type
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[0], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[1], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[1], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[2], leaves[1]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[2], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[3], leaves[3]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[4], leaves[1]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[5], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[7], leaves[1]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[7], leaves[3]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[8], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[9], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[10], leaves[1]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[10], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[12], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[12], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[12], leaves[3]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[13], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[13], leaves[1]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[13], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[14], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[14], leaves[1]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[15], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[15], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[15], leaves[3]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[16], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[16], leaves[3]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[17], leaves[2]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[17], leaves[3]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[18], leaves[0]);
            CreateRelationship_CompositeKeyToLeafSharedType(compositeKeys[18], leaves[1]);
        }
    }
}
