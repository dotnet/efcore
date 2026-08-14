// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ManyToManyQueryFixtureBase : SharedStoreFixtureBase<ManyToManyContext>, IQueryFixtureBase
{
    protected override string StoreName
        => "ManyToManyQueryTest";

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    private ManyToManyData _data;

    public ISetSource GetExpectedData()
    {
        if (_data == null)
        {
            using var context = CreateContext();
            _data = new ManyToManyData(context, false);
            context.ChangeTracker.DetectChanges();
            context.ChangeTracker.Clear();
        }

        return _data;
    }

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(EntityOne), e => ((EntityOne)e)?.Id },
        { typeof(EntityTwo), e => ((EntityTwo)e)?.Id },
        { typeof(EntityThree), e => ((EntityThree)e)?.Id },
        { typeof(EntityCompositeKey), e => (((EntityCompositeKey)e)?.Key1, ((EntityCompositeKey)e)?.Key2, ((EntityCompositeKey)e)?.Key3) },
        { typeof(EntityRoot), e => ((EntityRoot)e)?.Id },
        { typeof(EntityBranch), e => ((EntityBranch)e)?.Id },
        { typeof(EntityLeaf), e => ((EntityLeaf)e)?.Id },
        { typeof(EntityBranch2), e => ((EntityBranch2)e)?.Id },
        { typeof(EntityLeaf2), e => ((EntityLeaf2)e)?.Id },
        { typeof(EntityTableSharing1), e => ((EntityTableSharing1)e)?.Id },
        { typeof(EntityTableSharing2), e => ((EntityTableSharing2)e)?.Id },
        { typeof(UnidirectionalEntityOne), e => ((UnidirectionalEntityOne)e)?.Id },
        { typeof(UnidirectionalEntityTwo), e => ((UnidirectionalEntityTwo)e)?.Id },
        { typeof(UnidirectionalEntityThree), e => ((UnidirectionalEntityThree)e)?.Id },
        {
            typeof(UnidirectionalEntityCompositeKey), e => (((UnidirectionalEntityCompositeKey)e)?.Key1,
                ((UnidirectionalEntityCompositeKey)e)?.Key2,
                ((UnidirectionalEntityCompositeKey)e)?.Key3)
        },
        { typeof(UnidirectionalEntityRoot), e => ((UnidirectionalEntityRoot)e)?.Id },
        { typeof(UnidirectionalEntityBranch), e => ((UnidirectionalEntityBranch)e)?.Id },
        { typeof(UnidirectionalEntityLeaf), e => ((UnidirectionalEntityLeaf)e)?.Id },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
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
        {
            typeof(EntityBranch2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (EntityBranch2)e;
                    var aa = (EntityBranch2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Slumber, aa.Slumber);
                }
            }
        },
        {
            typeof(EntityLeaf2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (EntityLeaf2)e;
                    var aa = (EntityLeaf2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Slumber, aa.Slumber);
                    Assert.Equal(ee.IsBrown, aa.IsBrown);
                }
            }
        },
        {
            typeof(EntityTableSharing1), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (EntityTableSharing1)e;
                    var aa = (EntityTableSharing1)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(EntityTableSharing2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (EntityTableSharing2)e;
                    var aa = (EntityTableSharing2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Cucumber, aa.Cucumber);
                }
            }
        },
        {
            typeof(UnidirectionalEntityOne), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (UnidirectionalEntityOne)e;
                    var aa = (UnidirectionalEntityOne)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(UnidirectionalEntityTwo), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (UnidirectionalEntityTwo)e;
                    var aa = (UnidirectionalEntityTwo)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(UnidirectionalEntityThree), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (UnidirectionalEntityThree)e;
                    var aa = (UnidirectionalEntityThree)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(UnidirectionalEntityCompositeKey), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (UnidirectionalEntityCompositeKey)e;
                    var aa = (UnidirectionalEntityCompositeKey)a;

                    Assert.Equal(ee.Key1, aa.Key1);
                    Assert.Equal(ee.Key2, aa.Key2);
                    Assert.Equal(ee.Key3, aa.Key3);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(UnidirectionalEntityRoot), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (UnidirectionalEntityRoot)e;
                    var aa = (UnidirectionalEntityRoot)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(UnidirectionalEntityBranch), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (UnidirectionalEntityBranch)e;
                    var aa = (UnidirectionalEntityBranch)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Number, aa.Number);
                }
            }
        },
        {
            typeof(UnidirectionalEntityLeaf), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (UnidirectionalEntityLeaf)e;
                    var aa = (UnidirectionalEntityLeaf)a;

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
            .WithMany(e => e.OneSkipShared);

        modelBuilder.Entity<EntityRoot>()
            .HasMany(e => e.BranchSkipShared)
            .WithMany(e => e.RootSkipShared);

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
            .UsingEntity<JoinOneToTwo>();

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
            .UsingEntity<JoinOneToBranch>();

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
        modelBuilder.Entity<EntityTwo>()
            .HasMany(e => e.SelfSkipSharedLeft)
            .WithMany(e => e.SelfSkipSharedRight);

        // Nav:2 Payload:No Join:Shared Extra:CompositeKey
        modelBuilder.Entity<EntityTwo>()
            .HasMany(e => e.CompositeKeySkipShared)
            .WithMany(e => e.TwoSkipShared);

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
        modelBuilder.Entity<EntityThree>()
            .HasMany(e => e.RootSkipShared)
            .WithMany(e => e.ThreeSkipShared);

        // Nav:2 Payload:No Join:Shared Extra:Inheritance,CompositeKey
        modelBuilder.Entity<EntityCompositeKey>()
            .HasMany(e => e.RootSkipShared)
            .WithMany(e => e.CompositeKeySkipShared);

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
                    }));

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
            .WithMany();

        modelBuilder.Entity<UnidirectionalEntityBranch>()
            .HasMany<UnidirectionalEntityRoot>()
            .WithMany(e => e.BranchSkipShared);

        // Nav:2 Payload:No Join:Concrete Extra:None
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.TwoSkip)
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneToTwo>();

        // Nav:6 Payload:Yes Join:Concrete Extra:None
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany<UnidirectionalEntityThree>()
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneToThreePayloadFull>(
                r => r.HasOne(x => x.Three).WithMany(e => e.JoinOnePayloadFull),
                l => l.HasOne(x => x.One).WithMany(e => e.JoinThreePayloadFull));

        // Nav:4 Payload:Yes Join:Shared Extra:None
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.ThreeSkipPayloadFullShared)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UnidirectionalJoinOneToThreePayloadFullShared",
                r => r.HasOne<UnidirectionalEntityThree>().WithMany(e => e.JoinOnePayloadFullShared).HasForeignKey("ThreeId"),
                l => l.HasOne<UnidirectionalEntityOne>().WithMany(e => e.JoinThreePayloadFullShared).HasForeignKey("OneId"))
            .IndexerProperty<string>("Payload");

        // Nav:6 Payload:Yes Join:Concrete Extra:Self-Ref
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.SelfSkipPayloadLeft)
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneSelfPayload>(
                l => l.HasOne(x => x.Left).WithMany(x => x.JoinSelfPayloadLeft),
                r => r.HasOne(x => x.Right).WithMany(x => x.JoinSelfPayloadRight));

        // Nav:2 Payload:No Join:Concrete Extra:Inheritance
        modelBuilder.Entity<UnidirectionalEntityOne>()
            .HasMany(e => e.BranchSkip)
            .WithMany()
            .UsingEntity<UnidirectionalJoinOneToBranch>();

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
                l => l.HasOne(x => x.Two).WithMany(e => e.JoinThreeFull));

        // Nav:2 Payload:No Join:Shared Extra:Self-ref
        modelBuilder.Entity<UnidirectionalEntityTwo>()
            .HasMany<UnidirectionalEntityTwo>()
            .WithMany(e => e.SelfSkipSharedRight);

        // Nav:2 Payload:No Join:Shared Extra:CompositeKey
        modelBuilder.Entity<UnidirectionalEntityTwo>()
            .HasMany<UnidirectionalEntityCompositeKey>()
            .WithMany(e => e.TwoSkipShared);

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
                r => r.HasOne(x => x.Three).WithMany(x => x.JoinCompositeKeyFull).IsRequired());

        // Nav:2 Payload:No Join:Shared Extra:Inheritance
        modelBuilder.Entity<UnidirectionalEntityThree>()
            .HasMany<UnidirectionalEntityRoot>()
            .WithMany(e => e.ThreeSkipShared);

        // Nav:2 Payload:No Join:Shared Extra:Inheritance,CompositeKey
        modelBuilder.Entity<UnidirectionalEntityCompositeKey>()
            .HasMany(e => e.RootSkipShared)
            .WithMany();

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
                    }));

        modelBuilder.SharedTypeEntity<ProxyableSharedType>(
            "PST", b =>
            {
                b.IndexerProperty<int>("Id").ValueGeneratedNever();
                b.IndexerProperty<string>("Payload");
            });
    }

    public virtual bool UseGeneratedKeys
        => false;

    protected override Task SeedAsync(ManyToManyContext context)
    {
        new ManyToManyData(context, UseGeneratedKeys);
        return context.SaveChangesAsync();
    }
}
