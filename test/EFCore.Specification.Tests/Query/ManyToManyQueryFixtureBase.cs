// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ManyToManyQueryFixtureBase : SharedStoreFixtureBase<ManyToManyContext>, IQueryFixtureBase
    {
        protected override string StoreName { get; } = "ManyToManyQueryTest";

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public ISetSource GetExpectedData()
            => new ManyToManyData();

        public IReadOnlyDictionary<Type, object> GetEntitySorters()
            => new Dictionary<Type, Func<object, object>>
            {
                { typeof(EntityOne), e => ((EntityOne)e)?.Id },
                { typeof(EntityTwo), e => ((EntityTwo)e)?.Id },
                { typeof(EntityThree), e => ((EntityThree)e)?.Id },
                {
                    typeof(EntityCompositeKey),
                    e => (((EntityCompositeKey)e)?.Key1, ((EntityCompositeKey)e)?.Key2, ((EntityCompositeKey)e)?.Key3)
                },
                { typeof(EntityRoot), e => ((EntityRoot)e)?.Id },
                { typeof(EntityBranch), e => ((EntityBranch)e)?.Id },
                { typeof(EntityLeaf), e => ((EntityLeaf)e)?.Id },
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> GetEntityAsserters()
            => new Dictionary<Type, Action<object, object>>
            {
                {
                    typeof(EntityOne), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (EntityOne)e;
                            var aa = (EntityOne)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(EntityTwo), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (EntityTwo)e;
                            var aa = (EntityTwo)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(EntityThree), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (EntityThree)e;
                            var aa = (EntityThree)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(EntityCompositeKey), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (EntityCompositeKey)e;
                            var aa = (EntityCompositeKey)a;

                            Assert.Equal(ee.Key1, aa.Key1);
                            Assert.Equal(ee.Key2, aa.Key2);
                            Assert.Equal(ee.Key3, aa.Key3);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(EntityRoot), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (EntityRoot)e;
                            var aa = (EntityRoot)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(EntityBranch), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (EntityBranch)e;
                            var aa = (EntityBranch)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                            Assert.Equal(ee.Number, aa.Number);
                        }
                    }
                },
                {
                    typeof(EntityLeaf), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (EntityLeaf)e;
                            var aa = (EntityLeaf)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                            Assert.Equal(ee.Number, aa.Number);
                            Assert.Equal(ee.IsGreen, aa.IsGreen);
                        }
                    }
                },
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<EntityOne>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<EntityTwo>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<EntityThree>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<EntityCompositeKey>().HasKey(
                e => new
                {
                    e.Key1,
                    e.Key2,
                    e.Key3
                });
            modelBuilder.Entity<EntityRoot>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<EntityBranch>().HasBaseType<EntityRoot>();
            modelBuilder.Entity<EntityLeaf>().HasBaseType<EntityBranch>();

            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.Collection)
                .WithOne(e => e.CollectionInverse)
                .HasForeignKey(e => e.CollectionInverseId);

            modelBuilder.Entity<EntityOne>()
                .HasOne(e => e.Reference)
                .WithOne(e => e.ReferenceInverse)
                .HasForeignKey<EntityTwo>(e => e.ReferenceInverseId);

            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkipShared)
                .WithMany(e => e.OneSkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "EntityOneEntityTwo",
                    r => r.HasOne<EntityTwo>().WithMany().HasForeignKey("EntityTwoId"),
                    l => l.HasOne<EntityOne>().WithMany().HasForeignKey("EntityOneId"));

            // Nav:2 Payload:No Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToTwo>(
                    r => r.HasOne<EntityTwo>().WithMany().HasForeignKey(e => e.TwoId),
                    l => l.HasOne<EntityOne>().WithMany().HasForeignKey(e => e.OneId));

            // Nav:6 Payload:Yes Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFull)
                .WithMany(e => e.OneSkipPayloadFull)
                .UsingEntity<JoinOneToThreePayloadFull>(
                    r => r.HasOne(x => x.Three).WithMany(e => e.JoinOnePayloadFull),
                    l => l.HasOne(x => x.One).WithMany(e => e.JoinThreePayloadFull));

            // Nav:4 Payload:Yes Join:Shared Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFullShared)
                .WithMany(e => e.OneSkipPayloadFullShared)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinOneToThreePayloadFullShared",
                    r => r.HasOne<EntityThree>().WithMany(e => e.JoinOnePayloadFullShared).HasForeignKey("ThreeId"),
                    l => l.HasOne<EntityOne>().WithMany(e => e.JoinThreePayloadFullShared).HasForeignKey("OneId"))
                .IndexerProperty<string>("Payload");

            // Nav:6 Payload:Yes Join:Concrete Extra:Self-Ref
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.SelfSkipPayloadLeft)
                .WithMany(e => e.SelfSkipPayloadRight)
                .UsingEntity<JoinOneSelfPayload>(
                    l => l.HasOne(x => x.Left).WithMany(x => x.JoinSelfPayloadLeft),
                    r => r.HasOne(x => x.Right).WithMany(x => x.JoinSelfPayloadRight));

            // Nav:2 Payload:No Join:Concrete Extra:Inheritance
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.BranchSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToBranch>(
                    r => r.HasOne<EntityBranch>().WithMany(),
                    l => l.HasOne<EntityOne>().WithMany());

            modelBuilder.Entity<EntityTwo>()
                .HasOne(e => e.Reference)
                .WithOne(e => e.ReferenceInverse)
                .HasForeignKey<EntityThree>(e => e.ReferenceInverseId);

            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.Collection)
                .WithOne(e => e.CollectionInverse)
                .HasForeignKey(e => e.CollectionInverseId);

            // Nav:6 Payload:No Join:Concrete Extra:None
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.ThreeSkipFull)
                .WithMany(e => e.TwoSkipFull)
                .UsingEntity<JoinTwoToThree>(
                    r => r.HasOne(x => x.Three).WithMany(e => e.JoinTwoFull),
                    l => l.HasOne(x => x.Two).WithMany(e => e.JoinThreeFull));

            // Nav:2 Payload:No Join:Shared Extra:Self-ref
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.SelfSkipSharedLeft)
                .WithMany(e => e.SelfSkipSharedRight)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinTwoSelfShared",
                    l => l.HasOne<EntityTwo>().WithMany().HasForeignKey("LeftId"),
                    r => r.HasOne<EntityTwo>().WithMany().HasForeignKey("RightId"));

            // Nav:2 Payload:No Join:Shared Extra:CompositeKey
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.CompositeKeySkipShared)
                .WithMany(e => e.TwoSkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinTwoToCompositeKeyShared",
                    r => r.HasOne<EntityCompositeKey>().WithMany().HasForeignKey("CompositeId1", "CompositeId2", "CompositeId3"),
                    l => l.HasOne<EntityTwo>().WithMany().HasForeignKey("TwoId"))
                .HasKey("TwoId", "CompositeId1", "CompositeId2", "CompositeId3");

            // Nav:6 Payload:No Join:Concrete Extra:CompositeKey
            modelBuilder.Entity<EntityThree>()
                .HasMany(e => e.CompositeKeySkipFull)
                .WithMany(e => e.ThreeSkipFull)
                .UsingEntity<JoinThreeToCompositeKeyFull>(
                    l => l.HasOne(x => x.Composite).WithMany(x => x.JoinThreeFull).HasForeignKey(
                        e => new
                        {
                            e.CompositeId1,
                            e.CompositeId2,
                            e.CompositeId3
                        }).IsRequired(),
                    r => r.HasOne(x => x.Three).WithMany(x => x.JoinCompositeKeyFull).IsRequired());

            // Nav:2 Payload:No Join:Shared Extra:Inheritance
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityThree>().HasMany(e => e.RootSkipShared).WithMany(e => e.ThreeSkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "EntityRootEntityThree",
                    r => r.HasOne<EntityRoot>().WithMany().HasForeignKey("EntityRootId"),
                    l => l.HasOne<EntityThree>().WithMany().HasForeignKey("EntityThreeId"));

            // Nav:2 Payload:No Join:Shared Extra:Inheritance,CompositeKey
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.RootSkipShared)
                .WithMany(e => e.CompositeKeySkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinCompositeKeyToRootShared",
                    r => r.HasOne<EntityRoot>().WithMany().HasForeignKey("RootId"),
                    l => l.HasOne<EntityCompositeKey>().WithMany().HasForeignKey("CompositeId1", "CompositeId2", "CompositeId3"))
                .HasKey("CompositeId1", "CompositeId2", "CompositeId3", "RootId");

            // Nav:6 Payload:No Join:Concrete Extra:Inheritance,CompositeKey
            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.LeafSkipFull)
                .WithMany(e => e.CompositeKeySkipFull)
                .UsingEntity<JoinCompositeKeyToLeaf>(
                    r => r.HasOne(x => x.Leaf).WithMany(x => x.JoinCompositeKeyFull),
                    l => l.HasOne(x => x.Composite).WithMany(x => x.JoinLeafFull).HasForeignKey(
                        e => new
                        {
                            e.CompositeId1,
                            e.CompositeId2,
                            e.CompositeId3
                        }))
                .HasKey(
                    e => new
                    {
                        e.CompositeId1,
                        e.CompositeId2,
                        e.CompositeId3,
                        e.LeafId
                    });

            modelBuilder.SharedTypeEntity<ProxyableSharedType>(
                "PST", b =>
                {
                    b.IndexerProperty<int>("Id").ValueGeneratedNever();
                    b.IndexerProperty<string>("Payload");
                });
        }

        protected override void Seed(ManyToManyContext context)
            => ManyToManyContext.Seed(context);
    }
}
