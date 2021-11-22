// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TemporalManyToManyQuerySqlServerFixture : ManyToManyQueryFixtureBase
    {
        protected override string StoreName { get; } = "TemporalManyToManyQueryTest";

        public DateTime ChangesDate { get; private set; }

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<EntityOne>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<EntityTwo>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<EntityThree>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<EntityCompositeKey>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<EntityRoot>().ToTable(tb => tb.IsTemporal());

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

            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkipShared)
                .WithMany(e => e.OneSkipShared)
                .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

            // Nav:2 Payload:No Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToTwo>()
                .ToTable(tb => tb.IsTemporal());

            // Nav:6 Payload:Yes Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFull)
                .WithMany(e => e.OneSkipPayloadFull)
                .UsingEntity<JoinOneToThreePayloadFull>(
                    r => r.HasOne(x => x.Three).WithMany(e => e.JoinOnePayloadFull),
                    l => l.HasOne(x => x.One).WithMany(e => e.JoinThreePayloadFull))
                .ToTable(tb => tb.IsTemporal());

            // Nav:4 Payload:Yes Join:Shared Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFullShared)
                .WithMany(e => e.OneSkipPayloadFullShared)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinOneToThreePayloadFullShared",
                    r => r.HasOne<EntityThree>().WithMany(e => e.JoinOnePayloadFullShared).HasForeignKey("ThreeId"),
                    l => l.HasOne<EntityOne>().WithMany(e => e.JoinThreePayloadFullShared).HasForeignKey("OneId"))
                .ToTable(tb => tb.IsTemporal())
                .IndexerProperty<string>("Payload");

            // Nav:6 Payload:Yes Join:Concrete Extra:Self-Ref
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.SelfSkipPayloadLeft)
                .WithMany(e => e.SelfSkipPayloadRight)
                .UsingEntity<JoinOneSelfPayload>(
                    l => l.HasOne(x => x.Left).WithMany(x => x.JoinSelfPayloadLeft),
                    r => r.HasOne(x => x.Right).WithMany(x => x.JoinSelfPayloadRight))
                .ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Concrete Extra:Inheritance
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.BranchSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToBranch>()
                .ToTable(tb => tb.IsTemporal());

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
                    l => l.HasOne(x => x.Two).WithMany(e => e.JoinThreeFull))
                .ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Shared Extra:Self-ref
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.SelfSkipSharedLeft)
                .WithMany(e => e.SelfSkipSharedRight)
                .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

            // Nav:2 Payload:No Join:Shared Extra:CompositeKey
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.CompositeKeySkipShared)
                .WithMany(e => e.TwoSkipShared)
                .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

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
                    r => r.HasOne(x => x.Three).WithMany(x => x.JoinCompositeKeyFull).IsRequired())
                .ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Shared Extra:Inheritance
            modelBuilder.Entity<EntityThree>().HasMany(e => e.RootSkipShared).WithMany(e => e.ThreeSkipShared)
                .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

            // Nav:2 Payload:No Join:Shared Extra:Inheritance,CompositeKey
            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.RootSkipShared)
                .WithMany(e => e.CompositeKeySkipShared)
                .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

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
                .ToTable(tb => tb.IsTemporal());

            modelBuilder.SharedTypeEntity<ProxyableSharedType>(
                "PST", b =>
                {
                    b.IndexerProperty<int>("Id").ValueGeneratedNever();
                    b.IndexerProperty<string>("Payload");
                });
        }

        protected override void Seed(ManyToManyContext context)
        {
            base.Seed(context);

            ChangesDate = new DateTime(2010, 1, 1);

            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityThree).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityTwo).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityOne).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityCompositeKey).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityRoot).Select(e => e.Entity));
            context.SaveChanges();

            var tableNames = new List<string>
            {
                "EntityCompositeKeys",
                "EntityOneEntityTwo",
                "EntityOnes",
                "EntityTwos",
                "EntityThrees",
                "EntityRoots",
                "EntityRootEntityThree",
                "JoinCompositeKeyToLeaf",
                "EntityCompositeKeyEntityRoot",
                "JoinOneSelfPayload",
                "JoinOneToBranch",
                "JoinOneToThreePayloadFull",
                "JoinOneToThreePayloadFullShared",
                "JoinOneToTwo",
                "JoinThreeToCompositeKeyFull",
                "EntityTwoEntityTwo",
                "EntityCompositeKeyEntityTwo",
                "JoinTwoToThree",
            };

            foreach (var tableName in tableNames)
            {
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME");

                context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
                context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
                context.Database.ExecuteSqlRaw(
                    $"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");
            }
        }
    }
}
