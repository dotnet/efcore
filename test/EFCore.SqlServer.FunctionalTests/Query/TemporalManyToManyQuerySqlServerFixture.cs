﻿// Licensed to the .NET Foundation under one or more agreements.
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

            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkipShared)
                .WithMany(e => e.OneSkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "EntityOneEntityTwo",
                    r => r.HasOne<EntityTwo>().WithMany().HasForeignKey("EntityTwoId"),
                    l => l.HasOne<EntityOne>().WithMany().HasForeignKey("EntityOneId")).ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.TwoSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToTwo>(
                    r => r.HasOne(e => e.Two).WithMany().HasForeignKey(e => e.TwoId),
                    l => l.HasOne(e => e.One).WithMany().HasForeignKey(e => e.OneId)).ToTable(tb => tb.IsTemporal());

            // Nav:6 Payload:Yes Join:Concrete Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFull)
                .WithMany(e => e.OneSkipPayloadFull)
                .UsingEntity<JoinOneToThreePayloadFull>(
                    r => r.HasOne(x => x.Three).WithMany(e => e.JoinOnePayloadFull),
                    l => l.HasOne(x => x.One).WithMany(e => e.JoinThreePayloadFull)).ToTable(tb => tb.IsTemporal());

            // Nav:4 Payload:Yes Join:Shared Extra:None
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.ThreeSkipPayloadFullShared)
                .WithMany(e => e.OneSkipPayloadFullShared)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinOneToThreePayloadFullShared",
                    r => r.HasOne<EntityThree>().WithMany(e => e.JoinOnePayloadFullShared).HasForeignKey("ThreeId"),
                    l => l.HasOne<EntityOne>().WithMany(e => e.JoinThreePayloadFullShared).HasForeignKey("OneId")).ToTable(tb => tb.IsTemporal())
                .IndexerProperty<string>("Payload");

            // Nav:6 Payload:Yes Join:Concrete Extra:Self-Ref
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.SelfSkipPayloadLeft)
                .WithMany(e => e.SelfSkipPayloadRight)
                .UsingEntity<JoinOneSelfPayload>(
                    l => l.HasOne(x => x.Left).WithMany(x => x.JoinSelfPayloadLeft),
                    r => r.HasOne(x => x.Right).WithMany(x => x.JoinSelfPayloadRight)).ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Concrete Extra:Inheritance
            modelBuilder.Entity<EntityOne>()
                .HasMany(e => e.BranchSkip)
                .WithMany(e => e.OneSkip)
                .UsingEntity<JoinOneToBranch>(
                    r => r.HasOne<EntityBranch>().WithMany(),
                    l => l.HasOne<EntityOne>().WithMany()).ToTable(tb => tb.IsTemporal());

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
                    l => l.HasOne(x => x.Two).WithMany(e => e.JoinThreeFull)).ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Shared Extra:Self-ref
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.SelfSkipSharedLeft)
                .WithMany(e => e.SelfSkipSharedRight)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinTwoSelfShared",
                    l => l.HasOne<EntityTwo>().WithMany().HasForeignKey("LeftId"),
                    r => r.HasOne<EntityTwo>().WithMany().HasForeignKey("RightId")).ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Shared Extra:CompositeKey
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityTwo>()
                .HasMany(e => e.CompositeKeySkipShared)
                .WithMany(e => e.TwoSkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinTwoToCompositeKeyShared",
                    r => r.HasOne<EntityCompositeKey>().WithMany().HasForeignKey("CompositeId1", "CompositeId2", "CompositeId3"),
                    l => l.HasOne<EntityTwo>().WithMany().HasForeignKey("TwoId")).ToTable(tb => tb.IsTemporal())
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
                    r => r.HasOne(x => x.Three).WithMany(x => x.JoinCompositeKeyFull).IsRequired()).ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Shared Extra:Inheritance
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityThree>().HasMany(e => e.RootSkipShared).WithMany(e => e.ThreeSkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "EntityRootEntityThree",
                    r => r.HasOne<EntityRoot>().WithMany().HasForeignKey("EntityRootId"),
                    l => l.HasOne<EntityThree>().WithMany().HasForeignKey("EntityThreeId")).ToTable(tb => tb.IsTemporal());

            // Nav:2 Payload:No Join:Shared Extra:Inheritance,CompositeKey
            // TODO: Remove UsingEntity
            modelBuilder.Entity<EntityCompositeKey>()
                .HasMany(e => e.RootSkipShared)
                .WithMany(e => e.CompositeKeySkipShared)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinCompositeKeyToRootShared",
                    r => r.HasOne<EntityRoot>().WithMany().HasForeignKey("RootId"),
                    l => l.HasOne<EntityCompositeKey>().WithMany().HasForeignKey("CompositeId1", "CompositeId2", "CompositeId3")).ToTable(tb => tb.IsTemporal())
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
                        })).ToTable(tb => tb.IsTemporal())
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
        {
            base.Seed(context);

            ChangesDate = new DateTime(2010, 1, 1);

            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityThree).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityTwo).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityOne).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityCompositeKey).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityRoot).Select(e => e.Entity));
            context.SaveChanges();

            var historyTableInfos = new List<(string table, string historyTable)>()
            {
                ("EntityCompositeKeys", "EntityCompositeKeyHistory"),
                ("EntityOneEntityTwo", "EntityOneEntityTwoHistory"),
                ("EntityOnes", "EntityOneHistory"),
                ("EntityTwos", "EntityTwoHistory"),
                ("EntityThrees", "EntityThreeHistory"),
                ("EntityRoots", "EntityRootHistory"),
                ("EntityRootEntityThree", "EntityRootEntityThreeHistory"),

                ("JoinCompositeKeyToLeaf", "JoinCompositeKeyToLeafHistory"),
                ("JoinCompositeKeyToRootShared", "JoinCompositeKeyToRootSharedHistory"),
                ("JoinOneSelfPayload", "JoinOneSelfPayloadHistory"),
                ("JoinOneToBranch", "JoinOneToBranchHistory"),
                ("JoinOneToThreePayloadFull", "JoinOneToThreePayloadFullHistory"),
                ("JoinOneToThreePayloadFullShared", "JoinOneToThreePayloadFullSharedHistory"),
                ("JoinOneToTwo", "JoinOneToTwoHistory"),
                ("JoinThreeToCompositeKeyFull", "JoinThreeToCompositeKeyFullHistory"),
                ("JoinTwoSelfShared", "JoinTwoSelfSharedHistory"),
                ("JoinTwoToCompositeKeyShared", "JoinTwoToCompositeKeySharedHistory"),
                ("JoinTwoToThree", "JoinTwoToThreeHistory"),
            };

            foreach (var historyTableInfo in historyTableInfos)
            {
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] SET (SYSTEM_VERSIONING = OFF)");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] DROP PERIOD FOR SYSTEM_TIME");

                context.Database.ExecuteSqlRaw($"UPDATE [{historyTableInfo.historyTable}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
                context.Database.ExecuteSqlRaw($"UPDATE [{historyTableInfo.historyTable}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{historyTableInfo.historyTable}]))");
            }
        }
    }
}
