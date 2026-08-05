// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TemporalManyToManyQuerySqlServerFixture : ManyToManyQueryFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "TemporalManyToManyQueryTest";

    public DateTime ChangesDate { get; private set; }

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<EntityTableSharing1>().ToTable("TableSharing");
        modelBuilder.Entity<EntityTableSharing2>(
            b =>
            {
                b.HasOne<EntityTableSharing1>().WithOne().HasForeignKey<EntityTableSharing2>(e => e.Id);
                b.ToTable("TableSharing");
            });

        modelBuilder.Entity<EntityOne>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<EntityTwo>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<EntityThree>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<EntityCompositeKey>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<EntityRoot>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<UnidirectionalEntityOne>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<UnidirectionalEntityTwo>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<UnidirectionalEntityThree>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<UnidirectionalEntityCompositeKey>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<UnidirectionalEntityRoot>().ToTable(tb => tb.IsTemporal());

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
        modelBuilder.Entity<EntityLeaf>().HasBaseType<EntityBranch>();
        modelBuilder.Entity<EntityBranch2>().HasBaseType<EntityRoot>();
        modelBuilder.Entity<EntityLeaf2>().HasBaseType<EntityBranch2>();
        modelBuilder.Entity<EntityTableSharing1>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<EntityTableSharing2>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<UnidirectionalEntityOne>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<UnidirectionalEntityTwo>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<UnidirectionalEntityThree>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<UnidirectionalEntityCompositeKey>().HasKey(
            e => new
            {
                e.Key1,
                e.Key2,
                e.Key3
            });
        modelBuilder.Entity<UnidirectionalEntityRoot>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<UnidirectionalEntityBranch>().HasBaseType<UnidirectionalEntityRoot>();
        modelBuilder.Entity<UnidirectionalEntityLeaf>().HasBaseType<UnidirectionalEntityBranch>();

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

        modelBuilder.Entity<EntityRoot>()
            .HasMany(e => e.BranchSkipShared)
            .WithMany(e => e.RootSkipShared)
            .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

        modelBuilder.Entity<EntityBranch2>()
            .HasMany(e => e.SelfSkipSharedLeft)
            .WithMany(e => e.SelfSkipSharedRight);

        modelBuilder.Entity<EntityBranch2>()
            .HasMany(e => e.Leaf2SkipShared)
            .WithMany(e => e.Branch2SkipShared);

        modelBuilder.Entity<EntityTableSharing1>()
            .HasMany(e => e.TableSharing2Shared)
            .WithMany(e => e.TableSharing1Shared);

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

        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.Collection)
            .WithOne(e => e.CollectionInverse)
            .HasForeignKey(e => e.CollectionInverseId);

        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasOne(e => e.Reference)
            .WithOne(e => e.ReferenceInverse)
            .HasForeignKey<UnidirectionalEntityTwo>(e => e.ReferenceInverseId);

        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.TwoSkipShared)
            .WithMany()
            .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

        modelBuilder.Entity<UnidirectionalEntityBranch>()
            .HasMany<UnidirectionalEntityRoot>()
            .WithMany(e => e.BranchSkipShared)
            .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

        // Nav:2 Payload:No Join:Concrete Extra:None
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.TwoSkip)
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneToTwo>()
            .ToTable(tb => tb.IsTemporal());

        // Nav:6 Payload:Yes Join:Concrete Extra:None
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany<UnidirectionalEntityThree>()
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneToThreePayloadFull>(
                r => r.HasOne(x => x.Three).WithMany(e => e.JoinOnePayloadFull),
                l => l.HasOne(x => x.One).WithMany(e => e.JoinThreePayloadFull))
            .ToTable(tb => tb.IsTemporal());

        // Nav:4 Payload:Yes Join:Shared Extra:None
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.ThreeSkipPayloadFullShared)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UnidirectionalJoinOneToThreePayloadFullShared",
                r => r.HasOne<UnidirectionalEntityThree>().WithMany(e => e.JoinOnePayloadFullShared).HasForeignKey("ThreeId"),
                l => l.HasOne<UnidirectionalEntityOne>().WithMany(e => e.JoinThreePayloadFullShared).HasForeignKey("OneId"))
            .ToTable(tb => tb.IsTemporal())
            .IndexerProperty<string>("Payload");

        // Nav:6 Payload:Yes Join:Concrete Extra:Self-Ref
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.SelfSkipPayloadLeft)
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneSelfPayload>(
                l => l.HasOne(x => x.Left).WithMany(x => x.JoinSelfPayloadLeft),
                r => r.HasOne(x => x.Right).WithMany(x => x.JoinSelfPayloadRight))
            .ToTable(tb => tb.IsTemporal());

        // Nav:2 Payload:No Join:Concrete Extra:Inheritance
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.BranchSkip)
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneToBranch>()
            .ToTable(tb => tb.IsTemporal());

        modelBuilder.Entity<UnidirectionalEntityTwo>()
            .HasOne(e => e.Reference)
            .WithOne(e => e.ReferenceInverse)
            .HasForeignKey<UnidirectionalEntityThree>(e => e.ReferenceInverseId);

        modelBuilder.Entity<UnidirectionalEntityTwo>()
            .HasMany(e => e.Collection)
            .WithOne(e => e.CollectionInverse)
            .HasForeignKey(e => e.CollectionInverseId);

        // Nav:6 Payload:No Join:Concrete Extra:None
        modelBuilder.Entity<UnidirectionalEntityTwo>()
            .HasMany<UnidirectionalEntityThree>()
            .WithMany(e => e.TwoSkipFull)
            .UsingEntity<UnidirectionalJoinTwoToThree>(
                r => r.HasOne(x => x.Three).WithMany(e => e.JoinTwoFull),
                l => l.HasOne(x => x.Two).WithMany(e => e.JoinThreeFull))
            .ToTable(tb => tb.IsTemporal());

        // Nav:2 Payload:No Join:Shared Extra:Self-ref
        modelBuilder.Entity<UnidirectionalEntityTwo>()
            .HasMany<UnidirectionalEntityTwo>()
            .WithMany(e => e.SelfSkipSharedRight)
            .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

        // Nav:2 Payload:No Join:Shared Extra:CompositeKey
        modelBuilder.Entity<UnidirectionalEntityTwo>()
            .HasMany<UnidirectionalEntityCompositeKey>()
            .WithMany(e => e.TwoSkipShared)
            .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

        // Nav:6 Payload:No Join:Concrete Extra:CompositeKey
        modelBuilder.Entity<UnidirectionalEntityThree>()
            .HasMany<UnidirectionalEntityCompositeKey>()
            .WithMany(e => e.ThreeSkipFull)
            .UsingEntity<UnidirectionalJoinThreeToCompositeKeyFull>(
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
        modelBuilder.Entity<UnidirectionalEntityThree>()
            .HasMany<UnidirectionalEntityRoot>()
            .WithMany(e => e.ThreeSkipShared)
            .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

        // Nav:2 Payload:No Join:Shared Extra:Inheritance,CompositeKey
        modelBuilder.Entity<UnidirectionalEntityCompositeKey>()
            .HasMany(e => e.RootSkipShared)
            .WithMany()
            .UsingEntity(t => t.ToTable(tb => tb.IsTemporal()));

        // Nav:6 Payload:No Join:Concrete Extra:Inheritance,CompositeKey
        modelBuilder.Entity<UnidirectionalEntityCompositeKey>()
            .HasMany<UnidirectionalEntityLeaf>()
            .WithMany(e => e.CompositeKeySkipFull)
            .UsingEntity<UnidirectionalJoinCompositeKeyToLeaf>(
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

    protected override async Task SeedAsync(ManyToManyContext context)
    {
        await base.SeedAsync(context);

        ChangesDate = new DateTime(2010, 1, 1);

        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityThree).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityTwo).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityOne).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityCompositeKey).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is EntityRoot).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is UnidirectionalEntityThree).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is UnidirectionalEntityTwo).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is UnidirectionalEntityOne).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is UnidirectionalEntityCompositeKey).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is UnidirectionalEntityRoot).Select(e => e.Entity));
        await context.SaveChangesAsync();

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
            "UnidirectionalEntityCompositeKeys",
            "UnidirectionalEntityOneUnidirectionalEntityTwo",
            "UnidirectionalEntityOnes",
            "UnidirectionalEntityTwos",
            "UnidirectionalEntityThrees",
            "UnidirectionalEntityRoots",
            "UnidirectionalEntityRootUnidirectionalEntityThree",
            "UnidirectionalJoinCompositeKeyToLeaf",
            "UnidirectionalEntityCompositeKeyUnidirectionalEntityRoot",
            "UnidirectionalJoinOneSelfPayload",
            "UnidirectionalJoinOneToBranch",
            "UnidirectionalJoinOneToThreePayloadFull",
            "UnidirectionalJoinOneToThreePayloadFullShared",
            "UnidirectionalJoinOneToTwo",
            "UnidirectionalJoinThreeToCompositeKeyFull",
            "UnidirectionalEntityTwoUnidirectionalEntityTwo",
            "UnidirectionalEntityCompositeKeyUnidirectionalEntityTwo",
            "UnidirectionalJoinTwoToThree",
        };

        foreach (var tableName in tableNames)
        {
            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME");

            await context.Database.ExecuteSqlRawAsync($"UPDATE [{tableName + "History"}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
            await context.Database.ExecuteSqlRawAsync($"UPDATE [{tableName + "History"}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
            await context.Database.ExecuteSqlRawAsync(
                $"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");
        }
    }
}
