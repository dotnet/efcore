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

            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoFullySpecified)
                .WithMany(e => e.OneFullySpecified)
                .UsingEntity<OneToTwoFullySpecified>(
                    r => r.HasOne(x => x.Two).WithMany(),
                    l => l.HasOne(x => x.One).WithMany())
                .HasKey(e => new { e.OneId, e.TwoId });

            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeFullySpecifiedWithPayload)
                .WithMany(e => e.OneFullySpecifiedWithPayload)
                .UsingEntity<OneToThreeFullySpecifiedWithPayload>(
                    r => r.HasOne(x => x.Three).WithMany(),
                    l => l.HasOne(x => x.One).WithMany())
                .HasKey(e => new { e.OneId, e.ThreeId });

            // TODO: convert to shared type
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSharedType)
                .WithMany(e => e.OneSharedType)
                .UsingEntity<OneToTwoSharedType>(
                    r => r.HasOne(x => x.Two).WithMany(),
                    l => l.HasOne(x => x.One).WithMany())
                .HasKey(e => new { e.OneId, e.TwoId });

            // TODO: convert to shared type
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSharedType)
                .WithMany(e => e.OneSharedType)
                .UsingEntity<OneToThreeSharedType>(
                    r => r.HasOne(x => x.Three).WithMany(),
                    l => l.HasOne(x => x.One).WithMany())
                .HasKey(e => new { e.OneId, e.ThreeId });

            // TODO: convert to shared type
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.SelfSharedTypeLeftWithPayload)
                .WithMany(e => e.SelfSharedTypeRightWithPayload)
                .UsingEntity<OneSelfSharedTypeWithPayload>(
                    r => r.HasOne(x => x.Right).WithMany().OnDelete(DeleteBehavior.NoAction),
                    l => l.HasOne(x => x.Left).WithMany())
                .HasKey(e => new { e.LeftId, e.RightId });

            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.BranchFullySpecified)
                .WithMany(e => e.OneFullySpecified)
                .UsingEntity<OneToBranchFullySpecified>(
                    r => r.HasOne(x => x.Branch).WithMany(),
                    l => l.HasOne(x => x.One).WithMany())
                .HasKey(e => new { e.BranchId, e.OneId });

            modelBuilder.Entity<EntityTwo>()
                .HasOne(e => e.Reference)
                .WithOne(e => e.ReferenceInverse)
                .HasForeignKey<EntityThree>(e => e.ReferenceInverseId);

            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.Collection)
                .WithOne(e => e.CollectionInverse)
                .HasForeignKey(e => e.CollectionInverseId);

            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.ThreeFullySpecified)
                .WithMany(e => e.TwoFullySpecified)
                .UsingEntity<TwoToThreeFullySpecified>(
                    r => r.HasOne(x => x.Three).WithMany(),
                    l => l.HasOne(x => x.Two).WithMany())
                .HasKey(e => new { e.TwoId, e.ThreeId });

            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.SelfFullySpecifiedLeft)
                .WithMany(e => e.SelfFullySpecifiedRight)
                .UsingEntity<TwoSelfFullySpecified>(
                    r => r.HasOne(x => x.Right).WithMany().OnDelete(DeleteBehavior.NoAction),
                    l => l.HasOne(x => x.Left).WithMany())
                .HasKey(e => new { e.LeftId, e.RightId });

            // TODO: convert to shared type
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.CompositeSharedType)
                .WithMany(e => e.TwoSharedType)
                .UsingEntity<TwoToCompositeSharedType>(
                    r => r.HasOne(x => x.Composite).WithMany(),
                    l => l.HasOne(x => x.Two).WithMany())
                .HasKey(e => new { e.TwoId, e.CompositeId1, e.CompositeId2, e.CompositeId3 });

            modelBuilder.Entity<EntityThree>()
                .HasMany(e => e.CompositeFullySpecified)
                .WithMany(e => e.ThreeFullySpecified)
                .UsingEntity<ThreeToCompositeFullySpecified>(
                    l => l.HasOne(x => x.Composite).WithMany(),
                    r => r.HasOne(x => x.Three).WithMany())
                .HasKey(e => new { e.ThreeId, e.CompositeId1, e.CompositeId2, e.CompositeId3 });

            // TODO: convert to shared type
            modelBuilder.Entity<EntityThree>()
                .HasMany(e => e.RootSharedType)
                .WithMany(e => e.ThreesSharedType)
                .UsingEntity<ThreeToRootSharedType>(
                    r => r.HasOne(x => x.Root).WithMany(),
                    l => l.HasOne(x => x.Three).WithMany())
                .HasKey(e => new { e.ThreeId, e.RootId });

            // TODO: convert to shared type
            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.RootSharedType)
                .WithMany(e => e.CompositeKeySharedType)
                .UsingEntity<CompositeToRootSharedType>(
                    r => r.HasOne(x => x.Root).WithMany(),
                    l => l.HasOne(x => x.Composite).WithMany())
                .HasKey(e => new { e.CompositeId1, e.CompositeId2, e.CompositeId3, e.RootId });

            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.LeafFullySpecified)
                .WithMany(e => e.CompositeKeyFullySpecified)
                .UsingEntity<CompositeToLeafFullySpecified>(
                    r => r.HasOne(x => x.Leaf).WithMany(),
                    l => l.HasOne(x => x.Composite).WithMany())
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

