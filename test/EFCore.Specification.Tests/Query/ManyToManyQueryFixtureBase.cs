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

        public ManyToManyQueryFixtureBase()
        {
            var entitySorters = new Dictionary<Type, Func<object, object>>
            {
                { typeof(EntityOne), e => ((EntityOne)e)?.Id },
                { typeof(EntityTwo), e => ((EntityTwo)e)?.Id },
                { typeof(EntityThree), e => ((EntityThree)e)?.Id },
            }.ToDictionary(e => e.Key, e => (object)e.Value); ;

            var entityAsserters = new Dictionary<Type, Action<object, object>>
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
            }.ToDictionary(e => e.Key, e => (object)e.Value); ;

            QueryAsserter = CreateQueryAsserter(entitySorters, entityAsserters);
        }

        protected virtual QueryAsserter<ManyToManyContext> CreateQueryAsserter(
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters)
            => new QueryAsserter<ManyToManyContext>(
                CreateContext,
                new ManyToManyData(),
                entitySorters,
                entityAsserters);

        public QueryAsserterBase QueryAsserter { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<EntityOne>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<EntityTwo>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<EntityThree>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<EntityCompositeKey>().HasKey(e => new { e.Key1, e.Key2, e.Key3 });
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

            // Nav:2 Payload:No Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToTwo>(
                    r => r.HasOne<EntityTwo>().WithMany().HasForeignKey(e => e.TwoId),
                    l => l.HasOne<EntityOne>().WithMany().HasForeignKey(e => e.OneId))
                .HasKey(e => new { e.OneId, e.TwoId });

            // Nav:6 Payload:Yes Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFull)
                .WithMany(e => e.OneSkipPayloadFull)
                .UsingEntity<JoinOneToThreePayloadFull>(
                    r => r.HasOne(x => x.Three).WithMany(e => e.JoinOnePayloadFull),
                    l => l.HasOne(x => x.One).WithMany(e => e.JoinThreePayloadFull))
                .HasKey(e => new { e.OneId, e.ThreeId });

            // TODO: convert to shared type
            // Nav:2 Payload:No Join:Shared Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkipShared)
                .WithMany(e => e.OneSkipShared)
                .UsingEntity<JoinOneToTwoShared>(
                    r => r.HasOne<EntityTwo>().WithMany().HasForeignKey(e => e.TwoId),
                    l => l.HasOne<EntityOne>().WithMany().HasForeignKey(e => e.OneId))
                .HasKey(e => new { e.OneId, e.TwoId });

            // TODO: convert to shared type
            // Nav:4 Payload:Yes Join:Shared Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFullShared)
                .WithMany(e => e.OneSkipPayloadFullShared)
                .UsingEntity<JoinOneToThreePayloadFullShared>(
                    r => r.HasOne<EntityThree>().WithMany(e => e.JoinOnePayloadFullShared).HasForeignKey(e => e.ThreeId),
                    l => l.HasOne<EntityOne>().WithMany(e => e.JoinThreePayloadFullShared).HasForeignKey(e => e.OneId))
                .HasKey(e => new { e.OneId, e.ThreeId });

            // TODO: convert to shared type
            // Nav:6 Payload:Yes Join:Concrete Extra:Self-Ref
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.SelfSkipPayloadLeft)
                .WithMany(e => e.SelfSkipPayloadRight)
                .UsingEntity<JoinOneSelfPayload>(
                    r => r.HasOne(x => x.Right).WithMany(x => x.JoinSelfPayloadRight).OnDelete(DeleteBehavior.NoAction),
                    l => l.HasOne(x => x.Left).WithMany(x => x.JoinSelfPayloadLeft))
                .HasKey(e => new { e.LeftId, e.RightId });

            // Nav:2 Payload:No Join:Concrete Extra:Inheritance
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.BranchSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToBranch>(
                    r => r.HasOne<EntityBranch>().WithMany().HasForeignKey(e => e.BranchId),
                    l => l.HasOne<EntityOne>().WithMany().HasForeignKey(e => e.OneId))
                .HasKey(e => new { e.BranchId, e.OneId });

            modelBuilder.Entity<EntityTwo>()
                .HasOne(e => e.Reference)
                .WithOne(e => e.ReferenceInverse)
                .HasForeignKey<EntityThree>(e => e.ReferenceInverseId);

            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.Collection)
                .WithOne(e => e.CollectionInverse)
                .HasForeignKey(e => e.CollectionInverseId);

            // Nav:6 Payload:No Join:Concrete Extra:Inheritance
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.ThreeSkipFull)
                .WithMany(e => e.TwoSkipFull)
                .UsingEntity<JoinTwoToThree>(
                    r => r.HasOne(x => x.Three).WithMany(e => e.JoinTwoFull),
                    l => l.HasOne(x => x.Two).WithMany(e => e.JoinThreeFull))
                .HasKey(e => new { e.TwoId, e.ThreeId });

            // Nav:2 Payload:No Join:Shared Extra:Self
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.SelfSkipSharedLeft)
                .WithMany(e => e.SelfSkipSharedRight)
                .UsingEntity<JoinTwoSelfShared>(
                    r => r.HasOne<EntityTwo>().WithMany().OnDelete(DeleteBehavior.NoAction).HasForeignKey(e => e.RightId),
                    l => l.HasOne<EntityTwo>().WithMany().HasForeignKey(e => e.LeftId))
                .HasKey(e => new { e.LeftId, e.RightId });

            // TODO: convert to shared type
            // Nav:2 Payload:No Join:Shared Extra:CompositeKey
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.CompositeKeySkipShared)
                .WithMany(e => e.TwoSkipShared)
                .UsingEntity<JoinTwoToCompositeKeyShared>(
                    r => r.HasOne<EntityCompositeKey>().WithMany().HasForeignKey(e => new { e.CompositeId1, e.CompositeId2, e.CompositeId3 }),
                    l => l.HasOne<EntityTwo>().WithMany().HasForeignKey(e => e.TwoId))
                .HasKey(e => new { e.TwoId, e.CompositeId1, e.CompositeId2, e.CompositeId3 });

            // Nav:6 Payload:No Join:Concrete Extra:CompositeKey
            modelBuilder.Entity<EntityThree>()
                .HasMany(e => e.CompositeKeySkipFull)
                .WithMany(e => e.ThreeSkipFull)
                .UsingEntity<JoinThreeToCompositeKeyFull>(
                    l => l.HasOne(x => x.Composite).WithMany(x => x.JoinThreeFull),
                    r => r.HasOne(x => x.Three).WithMany(x => x.JoinCompositeKeyFull))
                .HasKey(e => new { e.ThreeId, e.CompositeId1, e.CompositeId2, e.CompositeId3 });

            // TODO: convert to shared type
            // Nav:2 Payload:No Join:Shared Extra:Inheritance
            modelBuilder.Entity<EntityThree>()
                .HasMany(e => e.RootSkipShared)
                .WithMany(e => e.ThreeSkipShared)
                .UsingEntity<JoinThreeToRootShared>(
                    r => r.HasOne<EntityRoot>().WithMany().HasForeignKey(e => e.RootId),
                    l => l.HasOne<EntityThree>().WithMany().HasForeignKey(e => e.ThreeId))
                .HasKey(e => new { e.ThreeId, e.RootId });

            // TODO: convert to shared type
            // Nav:2 Payload:No Join:Shared Extra:Inheritance,CompositeKey
            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.RootSkipShared)
                .WithMany(e => e.CompositeKeySkipShared)
                .UsingEntity<JoinCompositeKeyToRootShared>(
                    r => r.HasOne<EntityRoot>().WithMany().HasForeignKey(e => e.RootId),
                    l => l.HasOne<EntityCompositeKey>().WithMany().HasForeignKey(e => new { e.CompositeId1, e.CompositeId2, e.CompositeId3 }))
                .HasKey(e => new { e.CompositeId1, e.CompositeId2, e.CompositeId3, e.RootId });

            // Nav:6 Payload:No Join:Concrete Extra:Inheritance,CompositeKey
            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.LeafSkipFull)
                .WithMany(e => e.CompositeKeySkipFull)
                .UsingEntity<JoinCompositeKeyToLeaf>(
                    r => r.HasOne(x => x.Leaf).WithMany(x => x.JoinCompositeKeyFull),
                    l => l.HasOne(x => x.Composite).WithMany(x => x.JoinLeafFull))
                .HasKey(e => new { e.CompositeId1, e.CompositeId2, e.CompositeId3, e.LeafId });
        }

        protected override void Seed(ManyToManyContext context) => ManyToManyContext.Seed(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder);

        public override ManyToManyContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }
}

