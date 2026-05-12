// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ManyToManyFieldsQueryFixtureBase : SharedStoreFixtureBase<ManyToManyContext>, IQueryFixtureBase
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
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<EntityOne>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<EntityTwo>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<EntityThree>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<JoinOneSelfPayload>(
            b =>
            {
                b.Property(e => e.LeftId);
                b.Property(e => e.RightId);
            });

        modelBuilder.Entity<GeneratedKeysLeft>(
            b =>
            {
                b.Property(e => e.Id);
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<GeneratedKeysRight>(
            b =>
            {
                b.Property(e => e.Id);
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<ImplicitManyToManyA>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<ImplicitManyToManyB>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<JoinOneToBranch>(
            b =>
            {
                b.Property(e => e.EntityOneId);
                b.Property(e => e.EntityBranchId);
            });

        modelBuilder.Entity<JoinTwoToThree>(
            b =>
            {
                b.Property(e => e.ThreeId);
                b.Property(e => e.TwoId);
            });

        modelBuilder.Entity<EntityCompositeKey>().HasKey(
            e => new
            {
                e.Key1,
                e.Key2,
                e.Key3
            });

        modelBuilder.Entity<EntityRoot>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Name);
            });

        modelBuilder.Entity<EntityBranch>(
            b =>
            {
                b.Property(e => e.Number);
            });

        modelBuilder.Entity<EntityLeaf>(
            b =>
            {
                b.Property(e => e.IsGreen);
            });

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

        // Nav:2 Payload:No Join:Concrete Extra:None
        modelBuilder.Entity<EntityOne>()
            .HasMany(e => e.TwoSkip)
            .WithMany(e => e.OneSkip)
            // See issue#25491
            .UsingEntity<JoinOneToTwo>(
                r => r.HasOne<EntityTwo>().WithMany().HasForeignKey(e => e.TwoId),
                l => l.HasOne<EntityOne>().WithMany().HasForeignKey(e => e.OneId));

        modelBuilder.Entity<JoinOneToThreePayloadFull>(
            b =>
            {
                b.Property(e => e.OneId);
                b.Property(e => e.ThreeId);
            });

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
                r => r.HasOne(x => x.Three).WithMany(x => x.JoinCompositeKeyFull).IsRequired(),
                b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.ThreeId);
                });

        // Nav:2 Payload:No Join:Shared Extra:Inheritance
        modelBuilder.Entity<EntityThree>().HasMany(e => e.RootSkipShared).WithMany(e => e.ThreeSkipShared);

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
