// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class GraphUpdatesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : GraphUpdatesTestBase<TFixture>.GraphUpdatesFixtureBase, new()
{
    protected GraphUpdatesTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    public abstract class GraphUpdatesFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        public readonly Guid RootAK = Guid.NewGuid();

        public virtual bool ForceClientNoAction
            => false;

        public virtual bool NoStoreCascades
            => false;

        public virtual bool HasIdentityResolution
            => false;

        public virtual bool AutoDetectChanges
            => true;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Root>(
                b =>
                {
                    b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                    b.HasMany(e => e.RequiredChildren)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId);

                    b.HasMany(e => e.OptionalChildren)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.RequiredSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<RequiredSingle1>(e => e.Id);

                    b.HasOne(e => e.OptionalSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<OptionalSingle1>(e => e.RootId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.OptionalSingleDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasForeignKey<OptionalSingle1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    b.HasOne(e => e.OptionalSingleMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasForeignKey<OptionalSingle1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    b.HasOne(e => e.RequiredNonPkSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<RequiredNonPkSingle1>(e => e.RootId);

                    b.HasOne(e => e.RequiredNonPkSingleDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasForeignKey<RequiredNonPkSingle1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingleMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasForeignKey<RequiredNonPkSingle1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasMany(e => e.RequiredChildrenAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId);

                    b.HasMany(e => e.OptionalChildrenAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.RequiredSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredSingleAk1>(e => e.RootId);

                    b.HasOne(e => e.OptionalSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1>(e => e.RootId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.OptionalSingleAkDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    b.HasOne(e => e.OptionalSingleAkMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    b.HasOne(e => e.RequiredNonPkSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1>(e => e.RootId);

                    b.HasOne(e => e.RequiredNonPkSingleAkDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingleAkMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasMany(e => e.RequiredCompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentAlternateId);
                });

            modelBuilder.Entity<Required1>()
                .HasMany(e => e.Children)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<Required1Derived>();
            modelBuilder.Entity<Required1MoreDerived>();
            modelBuilder.Entity<Required2Derived>();
            modelBuilder.Entity<Required2MoreDerived>();

            modelBuilder.Entity<Optional1>(
                b =>
                {
                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent2)
                        .HasForeignKey(
                            e => new { e.Parent2Id });
                });

            modelBuilder.Entity<Optional1Derived>();
            modelBuilder.Entity<Optional1MoreDerived>();
            modelBuilder.Entity<Optional2Derived>();
            modelBuilder.Entity<Optional2MoreDerived>();

            modelBuilder.Entity<RequiredSingle1>()
                .HasOne(e => e.Single)
                .WithOne(e => e.Back)
                .HasForeignKey<RequiredSingle2>(e => e.Id);

            modelBuilder.Entity<OptionalSingle1>()
                .HasOne(e => e.Single)
                .WithOne(e => e.Back)
                .HasForeignKey<OptionalSingle2>(e => e.BackId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OptionalSingle2>(
                b =>
                {
                    b.HasDiscriminator(e => e.Disc)
                        .HasValue<OptionalSingle2>(new MyDiscriminator(1))
                        .HasValue<OptionalSingle2Derived>(new MyDiscriminator(2))
                        .HasValue<OptionalSingle2MoreDerived>(new MyDiscriminator(3));

                    b.Property(e => e.Disc)
                        .HasConversion(
                            v => v.Value,
                            v => new MyDiscriminator(v),
                            new ValueComparer<MyDiscriminator>(
                                (l, r) => l.Value == r.Value,
                                v => v.Value.GetHashCode(),
                                v => new MyDiscriminator(v.Value)))
                        .Metadata
                        .SetAfterSaveBehavior(PropertySaveBehavior.Save);
                });

            modelBuilder.Entity<RequiredNonPkSingle1>()
                .HasOne(e => e.Single)
                .WithOne(e => e.Back)
                .HasForeignKey<RequiredNonPkSingle2>(e => e.BackId);

            modelBuilder.Entity<RequiredNonPkSingle2Derived>();
            modelBuilder.Entity<RequiredNonPkSingle2MoreDerived>();

            modelBuilder.Entity<RequiredAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId);

                    b.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(
                            e => new { e.Id, e.AlternateId })
                        .HasForeignKey(
                            e => new { e.ParentId, e.ParentAlternateId });
                });

            modelBuilder.Entity<RequiredAk1Derived>();
            modelBuilder.Entity<RequiredAk1MoreDerived>();

            modelBuilder.Entity<OptionalAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(
                            e => new { e.Id, e.AlternateId })
                        .HasForeignKey(
                            e => new { e.ParentId, e.ParentAlternateId });
                });

            modelBuilder.Entity<OptionalAk1Derived>();
            modelBuilder.Entity<OptionalAk1MoreDerived>();

            modelBuilder.Entity<RequiredSingleAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<RequiredSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<RequiredSingleAk1>(e => e.AlternateId);

                    b.HasOne(e => e.SingleComposite)
                        .WithOne(e => e.Back)
                        .HasForeignKey<RequiredSingleComposite2>(
                            e => new { e.BackId, e.BackAlternateId })
                        .HasPrincipalKey<RequiredSingleAk1>(
                            e => new { e.Id, e.AlternateId });
                });

            modelBuilder.Entity<OptionalSingleAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<OptionalSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<OptionalSingleAk1>(e => e.AlternateId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.SingleComposite)
                        .WithOne(e => e.Back)
                        .HasForeignKey<OptionalSingleComposite2>(
                            e => new { e.BackId, e.ParentAlternateId })
                        .HasPrincipalKey<OptionalSingleAk1>(
                            e => new { e.Id, e.AlternateId });
                });

            modelBuilder.Entity<OptionalSingleAk2Derived>();
            modelBuilder.Entity<OptionalSingleAk2MoreDerived>();

            modelBuilder.Entity<RequiredNonPkSingleAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<RequiredNonPkSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<RequiredNonPkSingleAk1>(e => e.AlternateId);
                });

            modelBuilder.Entity<RequiredAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<RequiredAk2Derived>();
            modelBuilder.Entity<RequiredAk2MoreDerived>();

            modelBuilder.Entity<OptionalAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OptionalAk2Derived>();
            modelBuilder.Entity<OptionalAk2MoreDerived>();

            modelBuilder.Entity<RequiredSingleAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<RequiredNonPkSingleAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<RequiredNonPkSingleAk2Derived>();
            modelBuilder.Entity<RequiredNonPkSingleAk2MoreDerived>();

            modelBuilder.Entity<OptionalSingleAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<RequiredComposite1>(
                eb =>
                {
                    eb.Property(e => e.Id).ValueGeneratedNever();

                    eb.HasKey(
                        e => new { e.Id, e.ParentAlternateId });

                    eb.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(
                            e => new { e.Id, e.ParentAlternateId })
                        .HasForeignKey(
                            e => new { e.ParentId, e.ParentAlternateId });
                });

            modelBuilder.Entity<OptionalOverlapping2>(
                eb =>
                {
                    eb.Property(e => e.Id).ValueGeneratedNever();

                    eb.HasKey(
                        e => new { e.Id, e.ParentAlternateId });

                    eb.HasOne(e => e.Root)
                        .WithMany()
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentAlternateId);
                });

            modelBuilder.Entity<BadCustomer>();
            modelBuilder.Entity<BadOrder>();

            modelBuilder.Entity<QuestTask>();

            modelBuilder.Entity<QuizTask>()
                .HasMany(qt => qt.Choices)
                .WithOne()
                .HasForeignKey(tc => tc.QuestTaskId);

            modelBuilder.Entity<HiddenAreaTask>()
                .HasMany(hat => hat.Choices)
                .WithOne()
                .HasForeignKey(tc => tc.QuestTaskId);

            modelBuilder.Entity<TaskChoice>();
            modelBuilder.Entity<ParentAsAChild>();
            modelBuilder.Entity<ChildAsAParent>();

            modelBuilder.Entity<Poost>();
            modelBuilder.Entity<Bloog>();

            modelBuilder.Entity<Produce>()
                .HasIndex(e => e.BarCode)
                .IsUnique();

            modelBuilder.Entity<SharedFkRoot>(
                builder =>
                {
                    builder.HasMany(x => x.Dependants).WithOne(x => x.Root)
                        .HasForeignKey(x => new { x.RootId })
                        .HasPrincipalKey(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);

                    builder.HasMany(x => x.Parents).WithOne(x => x.Root)
                        .HasForeignKey(x => new { x.RootId })
                        .HasPrincipalKey(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity<SharedFkParent>(
                builder =>
                {
                    builder.HasOne(x => x.Dependant).WithOne(x => x!.Parent).IsRequired(false)
                        .HasForeignKey<SharedFkParent>(x => new { x.RootId, x.DependantId })
                        .HasPrincipalKey<SharedFkDependant>(x => new { x.RootId, x.Id })
                        .OnDelete(DeleteBehavior.ClientSetNull);
                });

            modelBuilder.Entity<SharedFkDependant>();

            modelBuilder.Entity<Owner>();

            modelBuilder.Entity<OwnerWithKeyedCollection>(
                b =>
                {
                    b.Navigation(e => e.Owned).IsRequired();
                    b.Navigation(e => e.OwnedWithKey).IsRequired();

                    b.OwnsMany(
                        e => e.OwnedCollectionPrivateKey,
                        b => b.HasKey("OwnerWithKeyedCollectionId", "PrivateKey"));
                });

            modelBuilder
                .Entity<OwnerWithNonCompositeOwnedCollection>()
                .OwnsMany(e => e.Owned, owned => owned.HasKey("Id"));

            modelBuilder.Entity<OwnerNoKeyGeneration>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();

                    b.OwnsOne(
                        e => e.Owned,
                        b => b.Property("OwnerNoKeyGenerationId").ValueGeneratedNever());
                    b.OwnsMany(
                        e => e.OwnedCollection,
                        b =>
                        {
                            b.Property<int>("OwnedNoKeyGenerationId").ValueGeneratedNever();
                            b.Property("OwnerNoKeyGenerationId").ValueGeneratedNever();
                        });
                });

            modelBuilder.Entity<Provider>().HasData(
                new Provider { Id = "prov1" },
                new Provider { Id = "prov2" });

            modelBuilder.Entity<Partner>().HasData(
                new Partner { Id = "partner1" });

            modelBuilder.Entity<ProviderContract>(
                b =>
                {
                    b.HasOne(p => p.Partner).WithMany().IsRequired().HasForeignKey("PartnerId");
                    b.HasOne<Provider>().WithMany().IsRequired().HasForeignKey("ProviderId");

                    b.HasDiscriminator<string>("ProviderId")
                        .HasValue<ProviderContract1>("prov1")
                        .HasValue<ProviderContract2>("prov2");

                    b.HasKey("PartnerId", "ProviderId");
                });

            modelBuilder.Entity<EventDescriptorZ>(
                b =>
                {
                    b.Property<long>("EntityZId");
                    b.HasOne(e => e.EntityZ).WithMany().HasForeignKey("EntityZId").IsRequired();
                });

            modelBuilder.Entity<City>();

            modelBuilder.Entity<SomethingCategory>().HasData(
                new SomethingCategory { Id = 1, Name = "A" },
                new SomethingCategory { Id = 2, Name = "B" },
                new SomethingCategory { Id = 3, Name = "C" });

            modelBuilder.Entity<Something>().HasOne(s => s.SomethingCategory)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<SomethingOfCategoryA>(
                builder =>
                {
                    builder.Property<int>("CategoryId").IsRequired();

                    builder.HasKey(nameof(SomethingOfCategoryA.SomethingId), "CategoryId");

                    builder.HasOne(d => d.Something)
                        .WithOne(p => p.SomethingOfCategoryA)
                        .HasPrincipalKey<Something>(p => new { p.Id, p.CategoryId })
                        .HasForeignKey<SomethingOfCategoryA>(nameof(SomethingOfCategoryA.SomethingId), "CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    builder.HasOne<SomethingCategory>()
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull);
                });

            modelBuilder.Entity<SomethingOfCategoryB>(
                builder =>
                {
                    builder.Property(e => e.CategoryId).IsRequired();

                    builder.HasKey(e => new { e.SomethingId, e.CategoryId });

                    builder.HasOne(d => d.Something)
                        .WithOne(p => p.SomethingOfCategoryB)
                        .HasPrincipalKey<Something>(p => new { p.Id, p.CategoryId })
                        .HasForeignKey<SomethingOfCategoryB>(socb => new { socb.SomethingId, socb.CategoryId })
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    builder.HasOne(e => e.SomethingCategory)
                        .WithMany()
                        .HasForeignKey(e => e.CategoryId)
                        .OnDelete(DeleteBehavior.ClientSetNull);
                });

            modelBuilder.Entity<Swede>().HasMany(e => e.TurnipSwedes).WithOne(e => e.Swede).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Parsnip>().HasData(new Parsnip { Id = 1 });
            modelBuilder.Entity<Carrot>().HasData(new Carrot { Id = 1, ParsnipId = 1 });
            modelBuilder.Entity<Turnip>().HasData(new Turnip { Id = 1, CarrotsId = 1 });
            modelBuilder.Entity<Swede>().HasData(new Swede { Id = 1, ParsnipId = 1 });
            modelBuilder.Entity<TurnipSwede>().HasData(
                new TurnipSwede
                {
                    Id = 1,
                    SwedesId = 1,
                    TurnipId = 1
                });

            modelBuilder.Entity<FirstLaw>();
            modelBuilder.Entity<Bayaz>();
            modelBuilder.Entity<SecondLaw>();
            modelBuilder.Entity<ThirdLaw>();

            modelBuilder.Entity<SneakyChild>(
                b =>
                {
                    b.HasOne(x => x.Parent).WithMany(x => x.Children).OnDelete(DeleteBehavior.Restrict);
                    b.HasAlternateKey(x => new { x.Id, x.ParentId });
                });

            modelBuilder.Entity<Beetroot2>().HasData(
                new
                {
                    Id = 1,
                    Key = "root-1",
                    Name = "Root One"
                });

            modelBuilder.Entity<Lettuce2>().HasData(
                new
                {
                    Id = 4,
                    Key = "root-1/leaf-1",
                    Name = "Leaf One-One",
                    RootId = 1
                });

            modelBuilder.Entity<Radish2>()
                .HasMany(entity => entity.Entities)
                .WithMany()
                .UsingEntity<RootStructure>();

            modelBuilder.Entity<OwnerRoot>(
                b =>
                {
                    b.OwnsOne(e => e.OptionalSingle).OwnsOne(e => e.Single);
                    b.OwnsOne(e => e.RequiredSingle).OwnsOne(e => e.Single);
                    b.OwnsMany(e => e.OptionalChildren).OwnsMany(e => e.Children);
                    b.OwnsMany(e => e.RequiredChildren).OwnsMany(e => e.Children);
                });

            modelBuilder.Entity<ParentEntity32084>()
                .HasOne(x => x.Child)
                .WithOne()
                .HasForeignKey<ChildBaseEntity32084>(x => x.ParentId);

            modelBuilder.Entity<ChildEntity32084>();

            modelBuilder.Entity<StableParent32084>(
                b =>
                {
                    b.HasOne(x => x.Child).WithOne().HasForeignKey<StableChild32084>(x => x.ParentId);
                    b.Property(e => e.Id).HasValueGenerator<StableGuidGenerator>();
                });

            modelBuilder.Entity<StableChild32084>(
                b =>
                {
                    b.Property(e => e.Id).HasValueGenerator<StableGuidGenerator>();
                });

            modelBuilder.Entity<SneakyUncle32084>(
                b =>
                {
                    b.HasOne(x => x.Brother).WithOne().HasForeignKey<SneakyUncle32084>(x => x.BrotherId);
                    b.Property(e => e.Id).HasValueGenerator<StableGuidGenerator>();
                });

            modelBuilder.Entity<CompositeKeyWith<int>>(
                b =>
                {
                    b.HasKey(
                        e => new
                        {
                            e.TargetId,
                            e.SourceId,
                            e.PrimaryGroup
                        });
                    b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<CompositeKeyWith<bool>>(
                b =>
                {
                    b.HasKey(
                        e => new
                        {
                            e.TargetId,
                            e.SourceId,
                            e.PrimaryGroup
                        });
                    b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<CompositeKeyWith<bool?>>(
                b =>
                {
                    b.HasKey(
                        e => new
                        {
                            e.TargetId,
                            e.SourceId,
                            e.PrimaryGroup
                        });
                    b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<BoolOnlyKey<bool>>(
                b =>
                {
                    b.HasKey(e => e.PrimaryGroup);
                    b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<BoolOnlyKey<bool?>>(
                b =>
                {
                    b.HasKey(e => e.PrimaryGroup);
                    b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
                });
        }

        private class StableGuidGenerator : ValueGenerator<Guid>
        {
            private readonly ConcurrentDictionary<object, Guid> _guids = new(ReferenceEqualityComparer.Instance);

            public override Guid Next(EntityEntry entry)
                => _guids.GetOrAdd(entry.Entity, _ => Guid.NewGuid());

            public override bool GeneratesTemporaryValues
                => false;

            public override bool GeneratesStableValues
                => true;
        }

        protected virtual object CreateFullGraph()
            => new Root
            {
                AlternateId = RootAK,
                RequiredChildren =
                    new ObservableHashSet<Required1>(ReferenceEqualityComparer.Instance)
                    {
                        new() { Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance) { new(), new() } },
                        new() { Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance) { new(), new() } }
                    },
                OptionalChildren =
                    new ObservableHashSet<Optional1>(ReferenceEqualityComparer.Instance)
                    {
                        new()
                        {
                            Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance) { new(), new() },
                            CompositeChildren =
                                new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                        },
                        new()
                        {
                            Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance) { new(), new() },
                            CompositeChildren =
                                new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                        }
                    },
                RequiredSingle = new RequiredSingle1 { Single = new RequiredSingle2() },
                OptionalSingle = new OptionalSingle1 { Single = new OptionalSingle2() },
                OptionalSingleDerived = new OptionalSingle1Derived { Single = new OptionalSingle2Derived() },
                OptionalSingleMoreDerived = new OptionalSingle1MoreDerived { Single = new OptionalSingle2MoreDerived() },
                RequiredNonPkSingle = new RequiredNonPkSingle1 { Single = new RequiredNonPkSingle2() },
                RequiredNonPkSingleDerived =
                    new RequiredNonPkSingle1Derived { Single = new RequiredNonPkSingle2Derived(), Root = new Root() },
                RequiredNonPkSingleMoreDerived =
                    new RequiredNonPkSingle1MoreDerived
                    {
                        Single = new RequiredNonPkSingle2MoreDerived(),
                        Root = new Root(),
                        DerivedRoot = new Root()
                    },
                RequiredChildrenAk =
                    new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance)
                    {
                        new()
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new() { AlternateId = Guid.NewGuid() }, new() { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren =
                                new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance) { new(), new() }
                        },
                        new()
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new() { AlternateId = Guid.NewGuid() }, new() { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren =
                                new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance) { new(), new() }
                        }
                    },
                OptionalChildrenAk =
                    new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance)
                    {
                        new()
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new() { AlternateId = Guid.NewGuid() }, new() { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren =
                                new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance) { new(), new() }
                        },
                        new()
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new() { AlternateId = Guid.NewGuid() }, new() { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren =
                                new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance) { new(), new() }
                        }
                    },
                RequiredSingleAk =
                    new RequiredSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredSingleAk2 { AlternateId = Guid.NewGuid() },
                        SingleComposite = new RequiredSingleComposite2()
                    },
                OptionalSingleAk =
                    new OptionalSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2 { AlternateId = Guid.NewGuid() },
                        SingleComposite = new OptionalSingleComposite2()
                    },
                OptionalSingleAkDerived =
                    new OptionalSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(), Single = new OptionalSingleAk2Derived { AlternateId = Guid.NewGuid() }
                    },
                OptionalSingleAkMoreDerived =
                    new OptionalSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(), Single = new OptionalSingleAk2MoreDerived { AlternateId = Guid.NewGuid() }
                    },
                RequiredNonPkSingleAk =
                    new RequiredNonPkSingleAk1
                    {
                        AlternateId = Guid.NewGuid(), Single = new RequiredNonPkSingleAk2 { AlternateId = Guid.NewGuid() }
                    },
                RequiredNonPkSingleAkDerived =
                    new RequiredNonPkSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2Derived { AlternateId = Guid.NewGuid() },
                        Root = new Root()
                    },
                RequiredNonPkSingleAkMoreDerived =
                    new RequiredNonPkSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2MoreDerived { AlternateId = Guid.NewGuid() },
                        Root = new Root(),
                        DerivedRoot = new Root()
                    },
                RequiredCompositeChildren = new ObservableHashSet<RequiredComposite1>(ReferenceEqualityComparer.Instance)
                {
                    new()
                    {
                        Id = 1,
                        CompositeChildren =
                            new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance)
                            {
                                new() { Id = 1 }, new() { Id = 2 }
                            }
                    },
                    new()
                    {
                        Id = 2,
                        CompositeChildren =
                            new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance)
                            {
                                new() { Id = 3 }, new() { Id = 4 }
                            }
                    }
                }
            };

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var tracker = new KeyValueEntityTracker();

            context.ChangeTracker.TrackGraph(CreateFullGraph(), e => tracker.TrackEntity(e.Entry));

            context.Add(
                new BadOrder { BadCustomer = new BadCustomer() });

            context.Add(
                new ParentAsAChild { ChildAsAParent = new ChildAsAParent() });

            var bloog = new Bloog { Id = 515 };

            context.AddRange(
                new Poost { Id = 516, Bloog = bloog },
                new Poost { Id = 517, Bloog = bloog });

            var root = new SharedFkRoot();
            context.Add(root);

            var parent = new SharedFkParent { Root = root };
            context.Add(parent);

            context.Add(new SharedFkDependant { Root = root, Parent = parent });

            return context.SaveChangesAsync();
        }

        public class KeyValueEntityTracker
        {
            public virtual void TrackEntity(EntityEntry entry)
                => entry.GetInfrastructure()
                    .SetEntityState(DetermineState(entry), true);

            public virtual EntityState DetermineState(EntityEntry entry)
                => entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
        }
    }

    protected static void Add<T>(IEnumerable<T> collection, T item)
        => ((ICollection<T>)collection).Add(item);

    protected static void Remove<T>(IEnumerable<T> collection, T item)
        => ((ICollection<T>)collection).Remove(item);

    [Flags]
    public enum ChangeMechanism
    {
        Dependent = 1,
        Principal = 2,
        Fk = 4
    }

    protected Expression<Func<Root, bool>> IsTheRoot
        => r => r.AlternateId == Fixture.RootAK;

    protected virtual IQueryable<Root> ModifyQueryRoot(IQueryable<Root> query)
        => query;

    protected virtual OwnerRoot CreateOwnerRoot()
        => new()
        {
            OptionalSingle = new() { Name = "OS", Single = new() { Name = "OS2" } },
            RequiredSingle = new() { Name = "RS", Single = new() { Name = "RS2 " } },
            OptionalChildren =
            {
                new() { Name = "OC1" }, new() { Name = "OC2", Children = { new() { Name = "OCC1" }, new() { Name = "OCC2" } } }
            },
            RequiredChildren =
            {
                new() { Name = "RC1", Children = { new() { Name = "RCC1" }, new() { Name = "RCC2" } } }, new() { Name = "RC2" }
            }
        };

    protected Task<Root> LoadRequiredGraphAsync(DbContext context)
        => QueryRequiredGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.RequiredChildren).ThenInclude(e => e.Children)
            .Include(e => e.RequiredSingle).ThenInclude(e => e.Single)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadOptionalGraphAsync(DbContext context)
        => QueryOptionalGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryOptionalGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.OptionalChildren).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildren).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.OptionalSingle).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleDerived).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleMoreDerived).ThenInclude(e => e.Single)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadRequiredNonPkGraphAsync(DbContext context)
        => QueryRequiredNonPkGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredNonPkGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.RequiredNonPkSingle).ThenInclude(e => e.Single)
            .Include(e => e.RequiredNonPkSingleDerived).ThenInclude(e => e.Single)
            .Include(e => e.RequiredNonPkSingleDerived).ThenInclude(e => e.Root)
            .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.Single)
            .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.Root)
            .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.DerivedRoot)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadRequiredAkGraphAsync(DbContext context)
        => QueryRequiredAkGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredAkGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.Children)
            .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.RequiredSingleAk).ThenInclude(e => e.Single)
            .Include(e => e.RequiredSingleAk).ThenInclude(e => e.SingleComposite)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadOptionalAkGraphAsync(DbContext context)
        => QueryOptionalAkGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryOptionalAkGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.OptionalSingleAk).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleAk).ThenInclude(e => e.SingleComposite)
            .Include(e => e.OptionalSingleAkDerived).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleAkMoreDerived).ThenInclude(e => e.Single)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadRequiredNonPkAkGraphAsync(DbContext context)
        => QueryRequiredNonPkAkGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredNonPkAkGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.RequiredNonPkSingleAk).ThenInclude(e => e.Single)
            .Include(e => e.RequiredNonPkSingleAkDerived).ThenInclude(e => e.Single)
            .Include(e => e.RequiredNonPkSingleAkDerived).ThenInclude(e => e.Root)
            .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.Single)
            .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.Root)
            .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.DerivedRoot)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadOptionalOneToManyGraphAsync(DbContext context)
        => QueryOptionalOneToManyGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryOptionalOneToManyGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.OptionalChildren).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildren).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadRequiredCompositeGraphAsync(DbContext context)
        => QueryRequiredCompositeGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredCompositeGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.RequiredCompositeChildren).ThenInclude(e => e.CompositeChildren)
            .OrderBy(e => e.Id);

    protected static void AssertEntries(IReadOnlyList<EntityEntry> expectedEntries, IReadOnlyList<EntityEntry> actualEntries)
    {
        var newEntities = new HashSet<object>(actualEntries.Select(ne => ne.Entity));
        var missingEntities = expectedEntries.Select(e => e.Entity).Where(e => !newEntities.Contains(e)).ToList();
        Assert.Equal([], missingEntities);
        Assert.Equal(expectedEntries.Count, actualEntries.Count);
    }

    protected static void AssertKeys(Root expected, Root actual)
    {
        Assert.Equal(expected.Id, actual.Id);

        Assert.Equal(
            expected.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Id),
            actual.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Id));

        Assert.Equal(
            expected.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()),
            actual.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()));

        Assert.Equal(
            expected.RequiredChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id),
            actual.RequiredChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id));

        Assert.Equal(
            expected.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Id),
            actual.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Id));

        Assert.Equal(
            expected.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()),
            actual.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()));

        Assert.Equal(
            expected.OptionalChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id),
            actual.OptionalChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id));

        Assert.Equal(expected.RequiredSingle?.Id, actual.RequiredSingle?.Id);
        Assert.Equal(expected.OptionalSingle?.Id, actual.OptionalSingle?.Id);
        Assert.Equal(expected.OptionalSingleDerived?.Id, actual.OptionalSingleDerived?.Id);
        Assert.Equal(expected.OptionalSingleMoreDerived?.Id, actual.OptionalSingleMoreDerived?.Id);
        Assert.Equal(expected.RequiredNonPkSingle?.Id, actual.RequiredNonPkSingle?.Id);
        Assert.Equal(expected.RequiredNonPkSingleDerived?.Id, actual.RequiredNonPkSingleDerived?.Id);
        Assert.Equal(expected.RequiredNonPkSingleMoreDerived?.Id, actual.RequiredNonPkSingleMoreDerived?.Id);

        Assert.Equal(expected.RequiredSingle?.Single?.Id, actual.RequiredSingle?.Single?.Id);
        Assert.Equal(expected.OptionalSingle?.Single?.Id, actual.OptionalSingle?.Single?.Id);
        Assert.Equal(expected.OptionalSingleDerived?.Single?.Id, actual.OptionalSingleDerived?.Single?.Id);
        Assert.Equal(expected.OptionalSingleMoreDerived?.Single?.Id, actual.OptionalSingleMoreDerived?.Single?.Id);
        Assert.Equal(expected.RequiredNonPkSingle?.Single?.Id, actual.RequiredNonPkSingle?.Single?.Id);
        Assert.Equal(expected.RequiredNonPkSingleDerived?.Single?.Id, actual.RequiredNonPkSingleDerived?.Single?.Id);
        Assert.Equal(expected.RequiredNonPkSingleMoreDerived?.Single?.Id, actual.RequiredNonPkSingleMoreDerived?.Single?.Id);

        Assert.Equal(expected.AlternateId, actual.AlternateId);

        Assert.Equal(
            expected.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId),
            actual.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId));

        Assert.Equal(
            expected.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()),
            actual.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()));

        Assert.Equal(
            expected.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId),
            actual.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId));

        Assert.Equal(
            expected.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id),
            actual.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id));

        Assert.Equal(
            expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId),
            actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId));

        Assert.Equal(
            expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()),
            actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()));

        Assert.Equal(
            expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count),
            actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count));

        Assert.Equal(
            expected.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId),
            actual.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId));

        Assert.Equal(
            expected.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id),
            actual.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id));

        Assert.Equal(expected.RequiredSingleAk?.AlternateId, actual.RequiredSingleAk?.AlternateId);
        Assert.Equal(expected.OptionalSingleAk?.AlternateId, actual.OptionalSingleAk?.AlternateId);
        Assert.Equal(expected.OptionalSingleAkDerived?.AlternateId, actual.OptionalSingleAkDerived?.AlternateId);
        Assert.Equal(expected.OptionalSingleAkMoreDerived?.AlternateId, actual.OptionalSingleAkMoreDerived?.AlternateId);
        Assert.Equal(expected.RequiredNonPkSingleAk?.AlternateId, actual.RequiredNonPkSingleAk?.AlternateId);
        Assert.Equal(expected.RequiredNonPkSingleAkDerived?.AlternateId, actual.RequiredNonPkSingleAkDerived?.AlternateId);
        Assert.Equal(expected.RequiredNonPkSingleAkMoreDerived?.AlternateId, actual.RequiredNonPkSingleAkMoreDerived?.AlternateId);

        Assert.Equal(expected.RequiredSingleAk?.Single?.AlternateId, actual.RequiredSingleAk?.Single?.AlternateId);
        Assert.Equal(expected.RequiredSingleAk?.SingleComposite?.Id, actual.RequiredSingleAk?.SingleComposite?.Id);
        Assert.Equal(expected.OptionalSingleAk?.Single?.AlternateId, actual.OptionalSingleAk?.Single?.AlternateId);
        Assert.Equal(expected.OptionalSingleAk?.SingleComposite?.Id, actual.OptionalSingleAk?.SingleComposite?.Id);
        Assert.Equal(expected.OptionalSingleAkDerived?.Single?.AlternateId, actual.OptionalSingleAkDerived?.Single?.AlternateId);
        Assert.Equal(
            expected.OptionalSingleAkMoreDerived?.Single?.AlternateId, actual.OptionalSingleAkMoreDerived?.Single?.AlternateId);
        Assert.Equal(expected.RequiredNonPkSingleAk?.Single?.AlternateId, actual.RequiredNonPkSingleAk?.Single?.AlternateId);
        Assert.Equal(
            expected.RequiredNonPkSingleAkDerived?.Single?.AlternateId, actual.RequiredNonPkSingleAkDerived?.Single?.AlternateId);
        Assert.Equal(
            expected.RequiredNonPkSingleAkMoreDerived?.Single?.AlternateId,
            actual.RequiredNonPkSingleAkMoreDerived?.Single?.AlternateId);

        Assert.Equal(
            expected.RequiredCompositeChildren.OrderBy(e => e.Id).Select(
                e => new { e.Id, e.ParentAlternateId }),
            actual.RequiredCompositeChildren.OrderBy(e => e.Id).Select(
                e => new { e.Id, e.ParentAlternateId }));

        Assert.Equal(
            expected.RequiredCompositeChildren.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count),
            actual.RequiredCompositeChildren.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count));

        Assert.Equal(
            expected.RequiredCompositeChildren.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id)
                .Select(
                    e => new { e.Id, e.ParentAlternateId }),
            actual.RequiredCompositeChildren.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id)
                .Select(
                    e => new { e.Id, e.ParentAlternateId }));
    }

    protected static void AssertNavigations(Root root)
    {
        foreach (var child in root.RequiredChildren)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        foreach (var child in root.OptionalChildren)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        if (root.RequiredSingle != null)
        {
            Assert.Same(root, root.RequiredSingle.Root);
            Assert.Same(root.RequiredSingle, root.RequiredSingle.Single.Back);
        }

        if (root.OptionalSingle != null)
        {
            Assert.Same(root, root.OptionalSingle.Root);
            Assert.Same(root, root.OptionalSingleDerived.DerivedRoot);
            Assert.Same(root, root.OptionalSingleMoreDerived.MoreDerivedRoot);
            Assert.Same(root.OptionalSingle, root.OptionalSingle.Single.Back);
            Assert.Same(root.OptionalSingleDerived, root.OptionalSingleDerived.Single.Back);
            Assert.Same(root.OptionalSingleMoreDerived, root.OptionalSingleMoreDerived.Single.Back);
        }

        if (root.RequiredNonPkSingle != null)
        {
            Assert.Same(root, root.RequiredNonPkSingle.Root);
            Assert.Same(root, root.RequiredNonPkSingleDerived.DerivedRoot);
            Assert.Same(root, root.RequiredNonPkSingleMoreDerived.MoreDerivedRoot);
            Assert.Same(root.RequiredNonPkSingle, root.RequiredNonPkSingle.Single.Back);
            Assert.Same(root.RequiredNonPkSingleDerived, root.RequiredNonPkSingleDerived.Single.Back);
            Assert.Same(root.RequiredNonPkSingleMoreDerived, root.RequiredNonPkSingleMoreDerived.Single.Back);
        }

        foreach (var child in root.RequiredChildrenAk)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        foreach (var child in root.OptionalChildrenAk)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        if (root.RequiredSingleAk != null)
        {
            Assert.Same(root, root.RequiredSingleAk.Root);
            Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.Single.Back);
            Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.SingleComposite.Back);
        }

        if (root.OptionalSingleAk != null)
        {
            Assert.Same(root, root.OptionalSingleAk.Root);
            Assert.Same(root, root.OptionalSingleAkDerived.DerivedRoot);
            Assert.Same(root, root.OptionalSingleAkMoreDerived.MoreDerivedRoot);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single.Back);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.SingleComposite.Back);
            Assert.Same(root.OptionalSingleAkDerived, root.OptionalSingleAkDerived.Single.Back);
            Assert.Same(root.OptionalSingleAkMoreDerived, root.OptionalSingleAkMoreDerived.Single.Back);
        }

        if (root.RequiredNonPkSingleAk != null)
        {
            Assert.Same(root, root.RequiredNonPkSingleAk.Root);
            Assert.Same(root, root.RequiredNonPkSingleAkDerived.DerivedRoot);
            Assert.Same(root, root.RequiredNonPkSingleAkMoreDerived.MoreDerivedRoot);
            Assert.Same(root.RequiredNonPkSingleAk, root.RequiredNonPkSingleAk.Single.Back);
            Assert.Same(root.RequiredNonPkSingleAkDerived, root.RequiredNonPkSingleAkDerived.Single.Back);
            Assert.Same(root.RequiredNonPkSingleAkMoreDerived, root.RequiredNonPkSingleAkMoreDerived.Single.Back);
        }
    }

    protected static void AssertPossiblyNullNavigations(Root root)
    {
        foreach (var child in root.RequiredChildren)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        foreach (var child in root.OptionalChildren)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        foreach (var child in root.OptionalChildren)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        if (root.RequiredSingle != null)
        {
            Assert.Same(root, root.RequiredSingle.Root);
            Assert.Same(root.RequiredSingle, root.RequiredSingle.Single.Back);
        }

        if (root.OptionalSingle != null)
        {
            Assert.Same(root, root.OptionalSingle.Root);
            Assert.Same(root.OptionalSingle, root.OptionalSingle.Single.Back);
        }

        if (root.RequiredNonPkSingle != null)
        {
            Assert.Same(root, root.RequiredNonPkSingle.Root);
            Assert.Same(root.RequiredNonPkSingle, root.RequiredNonPkSingle.Single.Back);
        }

        foreach (var child in root.RequiredChildrenAk)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        foreach (var child in root.OptionalChildrenAk)
        {
            Assert.Same(root, child.Parent);
            Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
        }

        if (root.RequiredSingleAk != null)
        {
            Assert.Same(root, root.RequiredSingleAk.Root);
            Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.Single.Back);
            Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.SingleComposite.Back);
        }

        if (root.OptionalSingleAk != null)
        {
            Assert.Same(root, root.OptionalSingleAk.Root);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single.Back);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.SingleComposite.Back);
        }

        if (root.RequiredNonPkSingleAk != null)
        {
            Assert.Same(root, root.RequiredNonPkSingleAk.Root);
            Assert.Same(root.RequiredNonPkSingleAk, root.RequiredNonPkSingleAk.Single.Back);
        }
    }

    protected class Root : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private IEnumerable<Required1> _requiredChildren = new ObservableHashSet<Required1>(ReferenceEqualityComparer.Instance);
        private IEnumerable<Optional1> _optionalChildren = new ObservableHashSet<Optional1>(ReferenceEqualityComparer.Instance);
        private RequiredSingle1 _requiredSingle;
        private RequiredNonPkSingle1 _requiredNonPkSingle;
        private RequiredNonPkSingle1Derived _requiredNonPkSingleDerived;
        private RequiredNonPkSingle1MoreDerived _requiredNonPkSingleMoreDerived;
        private OptionalSingle1 _optionalSingle;
        private OptionalSingle1Derived _optionalSingleDerived;
        private OptionalSingle1MoreDerived _optionalSingleMoreDerived;

        private IEnumerable<RequiredAk1> _requiredChildrenAk =
            new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance);

        private IEnumerable<OptionalAk1> _optionalChildrenAk =
            new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance);

        private RequiredSingleAk1 _requiredSingleAk;
        private RequiredNonPkSingleAk1 _requiredNonPkSingleAk;
        private RequiredNonPkSingleAk1Derived _requiredNonPkSingleAkDerived;
        private RequiredNonPkSingleAk1MoreDerived _requiredNonPkSingleAkMoreDerived;
        private OptionalSingleAk1 _optionalSingleAk;
        private OptionalSingleAk1Derived _optionalSingleAkDerived;
        private OptionalSingleAk1MoreDerived _optionalSingleAkMoreDerived;

        private IEnumerable<RequiredComposite1> _requiredCompositeChildren
            = new ObservableHashSet<RequiredComposite1>(ReferenceEqualityComparer.Instance);

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public IEnumerable<Required1> RequiredChildren
        {
            get => _requiredChildren;
            set => SetWithNotify(value, ref _requiredChildren);
        }

        public IEnumerable<Optional1> OptionalChildren
        {
            get => _optionalChildren;
            set => SetWithNotify(value, ref _optionalChildren);
        }

        public RequiredSingle1 RequiredSingle
        {
            get => _requiredSingle;
            set => SetWithNotify(value, ref _requiredSingle);
        }

        public RequiredNonPkSingle1 RequiredNonPkSingle
        {
            get => _requiredNonPkSingle;
            set => SetWithNotify(value, ref _requiredNonPkSingle);
        }

        public RequiredNonPkSingle1Derived RequiredNonPkSingleDerived
        {
            get => _requiredNonPkSingleDerived;
            set => SetWithNotify(value, ref _requiredNonPkSingleDerived);
        }

        public RequiredNonPkSingle1MoreDerived RequiredNonPkSingleMoreDerived
        {
            get => _requiredNonPkSingleMoreDerived;
            set => SetWithNotify(value, ref _requiredNonPkSingleMoreDerived);
        }

        public OptionalSingle1 OptionalSingle
        {
            get => _optionalSingle;
            set => SetWithNotify(value, ref _optionalSingle);
        }

        public OptionalSingle1Derived OptionalSingleDerived
        {
            get => _optionalSingleDerived;
            set => SetWithNotify(value, ref _optionalSingleDerived);
        }

        public OptionalSingle1MoreDerived OptionalSingleMoreDerived
        {
            get => _optionalSingleMoreDerived;
            set => SetWithNotify(value, ref _optionalSingleMoreDerived);
        }

        public IEnumerable<RequiredAk1> RequiredChildrenAk
        {
            get => _requiredChildrenAk;
            set => SetWithNotify(value, ref _requiredChildrenAk);
        }

        public IEnumerable<OptionalAk1> OptionalChildrenAk
        {
            get => _optionalChildrenAk;
            set => SetWithNotify(value, ref _optionalChildrenAk);
        }

        public RequiredSingleAk1 RequiredSingleAk
        {
            get => _requiredSingleAk;
            set => SetWithNotify(value, ref _requiredSingleAk);
        }

        public RequiredNonPkSingleAk1 RequiredNonPkSingleAk
        {
            get => _requiredNonPkSingleAk;
            set => SetWithNotify(value, ref _requiredNonPkSingleAk);
        }

        public RequiredNonPkSingleAk1Derived RequiredNonPkSingleAkDerived
        {
            get => _requiredNonPkSingleAkDerived;
            set => SetWithNotify(value, ref _requiredNonPkSingleAkDerived);
        }

        public RequiredNonPkSingleAk1MoreDerived RequiredNonPkSingleAkMoreDerived
        {
            get => _requiredNonPkSingleAkMoreDerived;
            set => SetWithNotify(value, ref _requiredNonPkSingleAkMoreDerived);
        }

        public OptionalSingleAk1 OptionalSingleAk
        {
            get => _optionalSingleAk;
            set => SetWithNotify(value, ref _optionalSingleAk);
        }

        public OptionalSingleAk1Derived OptionalSingleAkDerived
        {
            get => _optionalSingleAkDerived;
            set => SetWithNotify(value, ref _optionalSingleAkDerived);
        }

        public OptionalSingleAk1MoreDerived OptionalSingleAkMoreDerived
        {
            get => _optionalSingleAkMoreDerived;
            set => SetWithNotify(value, ref _optionalSingleAkMoreDerived);
        }

        public IEnumerable<RequiredComposite1> RequiredCompositeChildren
        {
            get => _requiredCompositeChildren;
            set => SetWithNotify(value, ref _requiredCompositeChildren);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Root;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class Required1 : NotifyingEntity
    {
        private int _id;
        private int _parentId;
        private Root _parent;
        private IEnumerable<Required2> _children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance);

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public Root Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public IEnumerable<Required2> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Required1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class Required1Derived : Required1
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Required1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Required1MoreDerived : Required1Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Required1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Required2 : NotifyingEntity
    {
        private int _id;
        private int _parentId;
        private Required1 _parent;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public Required1 Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Required2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class Required2Derived : Required2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Required2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Required2MoreDerived : Required2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Required2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional1 : NotifyingEntity
    {
        private int _id;
        private int? _parentId;
        private Root _parent;
        private IEnumerable<Optional2> _children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance);

        private ICollection<OptionalComposite2> _compositeChildren =
            new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public Root Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public IEnumerable<Optional2> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }

        public ICollection<OptionalComposite2> CompositeChildren
        {
            get => _compositeChildren;
            set => SetWithNotify(value, ref _compositeChildren);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Optional1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class Optional1Derived : Optional1
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Optional1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional1MoreDerived : Optional1Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Optional1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional2 : NotifyingEntity
    {
        private int _id;
        private int? _parentId;
        private Optional1 _parent;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public Optional1 Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Optional2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class Optional2Derived : Optional2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Optional2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional2MoreDerived : Optional2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as Optional2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredSingle1 : NotifyingEntity
    {
        private int _id;
        private bool _bool;
        private Root _root;
        private RequiredSingle2 _single;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public bool Bool
        {
            get => _bool;
            set => SetWithNotify(value, ref _bool);
        }

        public Root Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public RequiredSingle2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingle1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredSingle2 : NotifyingEntity
    {
        private int _id;
        private bool _bool;
        private RequiredSingle1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public bool Bool
        {
            get => _bool;
            set => SetWithNotify(value, ref _bool);
        }

        public RequiredSingle1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingle2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredNonPkSingle1 : NotifyingEntity
    {
        private int _id;
        private int _rootId;
        private Root _root;
        private RequiredNonPkSingle2 _single;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int RootId
        {
            get => _rootId;
            set => SetWithNotify(value, ref _rootId);
        }

        public Root Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public RequiredNonPkSingle2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingle1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredNonPkSingle1Derived : RequiredNonPkSingle1
    {
        private int _derivedRootId;
        private Root _derivedRoot;

        public int DerivedRootId
        {
            get => _derivedRootId;
            set => SetWithNotify(value, ref _derivedRootId);
        }

        public Root DerivedRoot
        {
            get => _derivedRoot;
            set => SetWithNotify(value, ref _derivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingle1MoreDerived : RequiredNonPkSingle1Derived
    {
        private int _moreDerivedRootId;
        private Root _moreDerivedRoot;

        public int MoreDerivedRootId
        {
            get => _moreDerivedRootId;
            set => SetWithNotify(value, ref _moreDerivedRootId);
        }

        public Root MoreDerivedRoot
        {
            get => _moreDerivedRoot;
            set => SetWithNotify(value, ref _moreDerivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingle2 : NotifyingEntity
    {
        private int _id;
        private int _backId;
        private RequiredNonPkSingle1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int BackId
        {
            get => _backId;
            set => SetWithNotify(value, ref _backId);
        }

        public RequiredNonPkSingle1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingle2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredNonPkSingle2Derived : RequiredNonPkSingle2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingle2MoreDerived : RequiredNonPkSingle2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle1 : NotifyingEntity
    {
        private int _id;
        private int? _rootId;
        private Root _root;
        private OptionalSingle2 _single;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? RootId
        {
            get => _rootId;
            set => SetWithNotify(value, ref _rootId);
        }

        public Root Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public OptionalSingle2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingle1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalSingle1Derived : OptionalSingle1
    {
        private int? _derivedRootId;
        private Root _derivedRoot;

        public int? DerivedRootId
        {
            get => _derivedRootId;
            set => SetWithNotify(value, ref _derivedRootId);
        }

        public Root DerivedRoot
        {
            get => _derivedRoot;
            set => SetWithNotify(value, ref _derivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle1MoreDerived : OptionalSingle1Derived
    {
        private Root _moreDerivedRoot;
        private int? _moreDerivedRootId;

        public int? MoreDerivedRootId
        {
            get => _moreDerivedRootId;
            set => SetWithNotify(value, ref _moreDerivedRootId);
        }

        public Root MoreDerivedRoot
        {
            get => _moreDerivedRoot;
            set => SetWithNotify(value, ref _moreDerivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle2 : NotifyingEntity
    {
        private int _id;
        private int? _backId;
        private MyDiscriminator _disc;
        private OptionalSingle1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? BackId
        {
            get => _backId;
            set => SetWithNotify(value, ref _backId);
        }

        public MyDiscriminator Disc
        {
            get => _disc;
            set => SetWithNotify(value, ref _disc);
        }

        public OptionalSingle1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingle2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class MyDiscriminator(int value)
    {
        public int Value { get; } = value;

        public override bool Equals(object obj)
            => throw new InvalidOperationException();

        public override int GetHashCode()
            => throw new InvalidOperationException();
    }

    protected class OptionalSingle2Derived : OptionalSingle2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle2MoreDerived : OptionalSingle2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk1 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid _parentId;
        private Root _parent;
        private IEnumerable<RequiredAk2> _children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance);

        private IEnumerable<RequiredComposite2> _compositeChildren =
            new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance);

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public Root Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public IEnumerable<RequiredAk2> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }

        public IEnumerable<RequiredComposite2> CompositeChildren
        {
            get => _compositeChildren;
            set => SetWithNotify(value, ref _compositeChildren);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredAk1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredAk1Derived : RequiredAk1
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk1MoreDerived : RequiredAk1Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid _parentId;
        private RequiredAk1 _parent;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public RequiredAk1 Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredAk2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredComposite1 : NotifyingEntity
    {
        private int _id;
        private Guid _parentAlternateId;
        private Root _parent;

        private ICollection<OptionalOverlapping2> _compositeChildren =
            new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance);

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentAlternateId
        {
            get => _parentAlternateId;
            set => SetWithNotify(value, ref _parentAlternateId);
        }

        public Root Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredComposite1;
            return _id == other?.Id;
        }

        public ICollection<OptionalOverlapping2> CompositeChildren
        {
            get => _compositeChildren;
            set => SetWithNotify(value, ref _compositeChildren);
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalOverlapping2 : NotifyingEntity
    {
        private int _id;
        private Guid _parentAlternateId;
        private int? _parentId;
        private RequiredComposite1 _parent;
        private Root _root;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentAlternateId
        {
            get => _parentAlternateId;
            set => SetWithNotify(value, ref _parentAlternateId);
        }

        public int? ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public RequiredComposite1 Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public Root Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalOverlapping2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredComposite2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private int _parentId;
        private RequiredAk1 _parent;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentAlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public int ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public RequiredAk1 Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredComposite2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredAk2Derived : RequiredAk2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk2MoreDerived : RequiredAk2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk1 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid? _parentId;
        private Root _parent;
        private IEnumerable<OptionalAk2> _children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance);

        private ICollection<OptionalComposite2> _compositeChildren =
            new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid? ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public Root Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public IEnumerable<OptionalAk2> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }

        public ICollection<OptionalComposite2> CompositeChildren
        {
            get => _compositeChildren;
            set => SetWithNotify(value, ref _compositeChildren);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalAk1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalAk1Derived : OptionalAk1
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk1MoreDerived : OptionalAk1Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid? _parentId;
        private OptionalAk1 _parent;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid? ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public OptionalAk1 Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalAk2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalComposite2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private int? _parentId;
        private int? _parent2Id;
        private OptionalAk1 _parent;
        private Optional1 _parent2;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentAlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public int? ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public OptionalAk1 Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }

        public int? Parent2Id
        {
            get => _parent2Id;
            set => SetWithNotify(value, ref _parent2Id);
        }

        public Optional1 Parent2
        {
            get => _parent2;
            set => SetWithNotify(value, ref _parent2);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalComposite2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalAk2Derived : OptionalAk2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk2MoreDerived : OptionalAk2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredSingleAk1 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid _rootId;
        private Root _root;
        private RequiredSingleAk2 _single;
        private RequiredSingleComposite2 _singleComposite;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid RootId
        {
            get => _rootId;
            set => SetWithNotify(value, ref _rootId);
        }

        public Root Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public RequiredSingleAk2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }

        public RequiredSingleComposite2 SingleComposite
        {
            get => _singleComposite;
            set => SetWithNotify(value, ref _singleComposite);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingleAk1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredSingleAk2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid _backId;
        private RequiredSingleAk1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid BackId
        {
            get => _backId;
            set => SetWithNotify(value, ref _backId);
        }

        public RequiredSingleAk1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingleAk2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredSingleComposite2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private int _backId;
        private RequiredSingleAk1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid BackAlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public int BackId
        {
            get => _backId;
            set => SetWithNotify(value, ref _backId);
        }

        public RequiredSingleAk1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingleComposite2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredNonPkSingleAk1 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid _rootId;
        private Root _root;
        private RequiredNonPkSingleAk2 _single;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid RootId
        {
            get => _rootId;
            set => SetWithNotify(value, ref _rootId);
        }

        public Root Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public RequiredNonPkSingleAk2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingleAk1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredNonPkSingleAk1Derived : RequiredNonPkSingleAk1
    {
        private Guid _derivedRootId;
        private Root _derivedRoot;

        public Guid DerivedRootId
        {
            get => _derivedRootId;
            set => SetWithNotify(value, ref _derivedRootId);
        }

        public Root DerivedRoot
        {
            get => _derivedRoot;
            set => SetWithNotify(value, ref _derivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingleAk1MoreDerived : RequiredNonPkSingleAk1Derived
    {
        private Guid _moreDerivedRootId;
        private Root _moreDerivedRoot;

        public Guid MoreDerivedRootId
        {
            get => _moreDerivedRootId;
            set => SetWithNotify(value, ref _moreDerivedRootId);
        }

        public Root MoreDerivedRoot
        {
            get => _moreDerivedRoot;
            set => SetWithNotify(value, ref _moreDerivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingleAk2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid _backId;
        private RequiredNonPkSingleAk1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid BackId
        {
            get => _backId;
            set => SetWithNotify(value, ref _backId);
        }

        public RequiredNonPkSingleAk1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingleAk2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class RequiredNonPkSingleAk2Derived : RequiredNonPkSingleAk2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingleAk2MoreDerived : RequiredNonPkSingleAk2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk1 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid? _rootId;
        private Root _root;
        private OptionalSingleAk2 _single;
        private OptionalSingleComposite2 _singleComposite;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid? RootId
        {
            get => _rootId;
            set => SetWithNotify(value, ref _rootId);
        }

        public Root Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public OptionalSingleComposite2 SingleComposite
        {
            get => _singleComposite;
            set => SetWithNotify(value, ref _singleComposite);
        }

        public OptionalSingleAk2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingleAk1;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalSingleAk1Derived : OptionalSingleAk1
    {
        private Guid? _derivedRootId;
        private Root _derivedRoot;

        public Guid? DerivedRootId
        {
            get => _derivedRootId;
            set => SetWithNotify(value, ref _derivedRootId);
        }

        public Root DerivedRoot
        {
            get => _derivedRoot;
            set => SetWithNotify(value, ref _derivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk1MoreDerived : OptionalSingleAk1Derived
    {
        private Guid? _moreDerivedRootId;
        private Root _moreDerivedRoot;

        public Guid? MoreDerivedRootId
        {
            get => _moreDerivedRootId;
            set => SetWithNotify(value, ref _moreDerivedRootId);
        }

        public Root MoreDerivedRoot
        {
            get => _moreDerivedRoot;
            set => SetWithNotify(value, ref _moreDerivedRoot);
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private Guid? _backId;
        private OptionalSingleAk1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public Guid? BackId
        {
            get => _backId;
            set => SetWithNotify(value, ref _backId);
        }

        public OptionalSingleAk1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingleAk2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalSingleComposite2 : NotifyingEntity
    {
        private int _id;
        private Guid _alternateId;
        private int? _backId;
        private OptionalSingleAk1 _back;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentAlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public int? BackId
        {
            get => _backId;
            set => SetWithNotify(value, ref _backId);
        }

        public OptionalSingleAk1 Back
        {
            get => _back;
            set => SetWithNotify(value, ref _back);
        }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingleComposite2;
            return _id == other?.Id;
        }

        public override int GetHashCode()
            => _id;
    }

    protected class OptionalSingleAk2Derived : OptionalSingleAk2
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk2MoreDerived : OptionalSingleAk2Derived
    {
        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OwnerRoot : NotifyingEntity
    {
        private int _id;
        private ICollection<OwnedRequired1> _requiredChildren = new ObservableHashSet<OwnedRequired1>(ReferenceEqualityComparer.Instance);
        private ICollection<OwnedOptional1> _optionalChildren = new ObservableHashSet<OwnedOptional1>(ReferenceEqualityComparer.Instance);
        private OwnedRequiredSingle1 _requiredSingle;
        private OwnedOptionalSingle1 _optionalSingle;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public OwnedRequiredSingle1 RequiredSingle
        {
            get => _requiredSingle;
            set => SetWithNotify(value, ref _requiredSingle);
        }

        public OwnedOptionalSingle1 OptionalSingle
        {
            get => _optionalSingle;
            set => SetWithNotify(value, ref _optionalSingle);
        }

        public ICollection<OwnedRequired1> RequiredChildren
        {
            get => _requiredChildren;
            set => SetWithNotify(value, ref _requiredChildren);
        }

        public ICollection<OwnedOptional1> OptionalChildren
        {
            get => _optionalChildren;
            set => SetWithNotify(value, ref _optionalChildren);
        }
    }

    protected class OwnedRequired1 : NotifyingEntity
    {
        private ICollection<OwnedRequired2> _children = new ObservableHashSet<OwnedRequired2>(ReferenceEqualityComparer.Instance);
        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public ICollection<OwnedRequired2> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }
    }

    protected class OwnedRequired2 : NotifyingEntity
    {
        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }
    }

    protected class OwnedOptional1 : NotifyingEntity
    {
        private ICollection<OwnedOptional2> _children = new ObservableHashSet<OwnedOptional2>(ReferenceEqualityComparer.Instance);
        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public ICollection<OwnedOptional2> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }
    }

    protected class OwnedOptional2 : NotifyingEntity
    {
        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }
    }

    protected class OwnedRequiredSingle1 : NotifyingEntity
    {
        private OwnedRequiredSingle2 _single;
        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public OwnedRequiredSingle2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }
    }

    protected class OwnedRequiredSingle2 : NotifyingEntity
    {
        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }
    }

    protected class OwnedOptionalSingle1 : NotifyingEntity
    {
        private string _name;
        private OwnedOptionalSingle2 _single;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public OwnedOptionalSingle2 Single
        {
            get => _single;
            set => SetWithNotify(value, ref _single);
        }
    }

    protected class OwnedOptionalSingle2 : NotifyingEntity
    {
        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }
    }

    protected class BadCustomer : NotifyingEntity
    {
        private int _id;
        private int _status;
        private ICollection<BadOrder> _badOrders = new ObservableHashSet<BadOrder>(ReferenceEqualityComparer.Instance);

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int Status
        {
            get => _status;
            set => SetWithNotify(value, ref _status);
        }

        public ICollection<BadOrder> BadOrders
        {
            get => _badOrders;
            set => SetWithNotify(value, ref _badOrders);
        }
    }

    protected class BadOrder : NotifyingEntity
    {
        private int _id;
        private int? _badCustomerId;
        private BadCustomer _badCustomer;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? BadCustomerId
        {
            get => _badCustomerId;
            set => SetWithNotify(value, ref _badCustomerId);
        }

        public BadCustomer BadCustomer
        {
            get => _badCustomer;
            set => SetWithNotify(value, ref _badCustomer);
        }
    }

    protected class HiddenAreaTask : TaskWithChoices;

    protected abstract class QuestTask : NotifyingEntity
    {
        private int _id;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }
    }

    protected class QuizTask : TaskWithChoices;

    protected class TaskChoice : NotifyingEntity
    {
        private int _id;
        private int _questTaskId;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int QuestTaskId
        {
            get => _questTaskId;
            set => SetWithNotify(value, ref _questTaskId);
        }
    }

    protected class ParentAsAChild : NotifyingEntity
    {
        private int _id;
        private int? _childAsAParentId;
        private ChildAsAParent _childAsAParent;

        public bool Filler { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? ChildAsAParentId
        {
            get => _childAsAParentId;
            set => SetWithNotify(value, ref _childAsAParentId);
        }

        public ChildAsAParent ChildAsAParent
        {
            get => _childAsAParent;
            set => SetWithNotify(value, ref _childAsAParent);
        }
    }

    protected class ChildAsAParent : NotifyingEntity
    {
        private int _id;
        private ParentAsAChild _parentAsAChild;

        public bool Filler { get; set; }

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public ParentAsAChild ParentAsAChild
        {
            get => _parentAsAChild;
            set => SetWithNotify(value, ref _parentAsAChild);
        }
    }

    protected abstract class TaskWithChoices : QuestTask
    {
        private ICollection<TaskChoice> _choices = new ObservableHashSet<TaskChoice>(ReferenceEqualityComparer.Instance);

        public ICollection<TaskChoice> Choices
        {
            get => _choices;
            set => SetWithNotify(value, ref _choices);
        }
    }

    protected class Produce : NotifyingEntity
    {
        private Guid _produceId;
        private string _name;
        private int _barCode;

        public Guid ProduceId
        {
            get => _produceId;
            set => SetWithNotify(value, ref _produceId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public int BarCode
        {
            get => _barCode;
            set => SetWithNotify(value, ref _barCode);
        }
    }

    protected class Bloog : NotifyingEntity
    {
        private int _id;
        private IEnumerable<Poost> _poosts = new ObservableHashSet<Poost>(ReferenceEqualityComparer.Instance);

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public IEnumerable<Poost> Poosts
        {
            get => _poosts;
            set => SetWithNotify(value, ref _poosts);
        }
    }

    protected class Poost : NotifyingEntity
    {
        private int _id;
        private int? _bloogId;
        private Bloog _bloog;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? BloogId
        {
            get => _bloogId;
            set => SetWithNotify(value, ref _bloogId);
        }

        public Bloog Bloog
        {
            get => _bloog;
            set => SetWithNotify(value, ref _bloog);
        }
    }

    protected class SharedFkRoot : NotifyingEntity
    {
        private long _id;

        private ICollection<SharedFkDependant> _dependants
            = new ObservableHashSet<SharedFkDependant>(ReferenceEqualityComparer.Instance);

        private ICollection<SharedFkParent> _parents
            = new ObservableHashSet<SharedFkParent>(ReferenceEqualityComparer.Instance);

        public long Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public ICollection<SharedFkDependant> Dependants
        {
            get => _dependants;
            set => SetWithNotify(value, ref _dependants);
        }

        public ICollection<SharedFkParent> Parents
        {
            get => _parents;
            set => SetWithNotify(value, ref _parents);
        }
    }

    protected class SharedFkParent : NotifyingEntity
    {
        private long _id;
        private long? _dependantId;
        private long _rootId;
        private SharedFkRoot _root = null!;
        private SharedFkDependant _dependant;

        public long Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public long? DependantId
        {
            get => _dependantId;
            set => SetWithNotify(value, ref _dependantId);
        }

        public long RootId
        {
            get => _rootId;
            set => SetWithNotify(value, ref _rootId);
        }

        public SharedFkRoot Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public SharedFkDependant Dependant
        {
            get => _dependant;
            set => SetWithNotify(value, ref _dependant);
        }
    }

    protected class SharedFkDependant : NotifyingEntity
    {
        private long _id;
        private long _rootId;
        private SharedFkRoot _root = null!;
        private SharedFkParent _parent = null!;

        public long Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public long RootId
        {
            get => _rootId;
            set => SetWithNotify(value, ref _rootId);
        }

        public SharedFkRoot Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }

        public SharedFkParent Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }
    }

    protected class Owner : NotifyingEntity
    {
        private int _id;
        private Owned _owned;
        private ICollection<Owned> _ownedCollection = new ObservableHashSet<Owned>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Owned Owned
        {
            get => _owned;
            set => SetWithNotify(value, ref _owned);
        }

        public ICollection<Owned> OwnedCollection
        {
            get => _ownedCollection;
            set => SetWithNotify(value, ref _ownedCollection);
        }
    }

    [Owned]
    protected class Owned : NotifyingEntity
    {
        private int _foo;
        private string _bar;

        public int Foo
        {
            get => _foo;
            set => SetWithNotify(value, ref _foo);
        }

        public string Bar
        {
            get => _bar;
            set => SetWithNotify(value, ref _bar);
        }
    }

    protected class OwnerWithKeyedCollection : NotifyingEntity
    {
        private int _id;
        private Owned _owned;
        private OwnedWithKey _ownedWithKey;
        private ICollection<OwnedWithKey> _ownedCollection = new ObservableHashSet<OwnedWithKey>();
        private ICollection<OwnedWithPrivateKey> _ownedCollectionPrivateKey = new ObservableHashSet<OwnedWithPrivateKey>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Owned Owned
        {
            get => _owned;
            set => SetWithNotify(value, ref _owned);
        }

        public OwnedWithKey OwnedWithKey
        {
            get => _ownedWithKey;
            set => SetWithNotify(value, ref _ownedWithKey);
        }

        public ICollection<OwnedWithKey> OwnedCollection
        {
            get => _ownedCollection;
            set => SetWithNotify(value, ref _ownedCollection);
        }

        public ICollection<OwnedWithPrivateKey> OwnedCollectionPrivateKey
        {
            get => _ownedCollectionPrivateKey;
            set => SetWithNotify(value, ref _ownedCollectionPrivateKey);
        }
    }

    [Owned]
    protected class OwnedWithKey : NotifyingEntity
    {
        private int _foo;
        private string _bar;
        private int _ownedWithKeyId;

        public int OwnedWithKeyId
        {
            get => _ownedWithKeyId;
            set => SetWithNotify(value, ref _ownedWithKeyId);
        }

        public int Foo
        {
            get => _foo;
            set => SetWithNotify(value, ref _foo);
        }

        public string Bar
        {
            get => _bar;
            set => SetWithNotify(value, ref _bar);
        }
    }

    [Owned]
    protected class OwnedWithPrivateKey : NotifyingEntity
    {
        private int _foo;
        private string _bar;
        private int _privateKey;

        private int PrivateKey
        {
            get => _privateKey;
            set => SetWithNotify(value, ref _privateKey);
        }

        public int Foo
        {
            get => _foo;
            set => SetWithNotify(value, ref _foo);
        }

        public string Bar
        {
            get => _bar;
            set => SetWithNotify(value, ref _bar);
        }
    }

    protected class OwnerWithNonCompositeOwnedCollection : NotifyingEntity
    {
        private int _id;
        private ICollection<NonCompositeOwnedCollection> _owned = new ObservableHashSet<NonCompositeOwnedCollection>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public ICollection<NonCompositeOwnedCollection> Owned
        {
            get => _owned;
            set => SetWithNotify(value, ref _owned);
        }
    }

    protected class NonCompositeOwnedCollection : NotifyingEntity
    {
        private string _foo;

        public string Foo
        {
            get => _foo;
            set => SetWithNotify(value, ref _foo);
        }
    }

    protected class OwnerNoKeyGeneration : NotifyingEntity
    {
        private int _id;
        private OwnedNoKeyGeneration _owned;
        private ICollection<OwnedNoKeyGeneration> _ownedCollection = new ObservableHashSet<OwnedNoKeyGeneration>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public OwnedNoKeyGeneration Owned
        {
            get => _owned;
            set => SetWithNotify(value, ref _owned);
        }

        public ICollection<OwnedNoKeyGeneration> OwnedCollection
        {
            get => _ownedCollection;
            set => SetWithNotify(value, ref _ownedCollection);
        }
    }

    [Owned]
    protected class OwnedNoKeyGeneration : NotifyingEntity
    {
        private int _foo;
        private string _bar;

        public int Foo
        {
            get => _foo;
            set => SetWithNotify(value, ref _foo);
        }

        public string Bar
        {
            get => _bar;
            set => SetWithNotify(value, ref _bar);
        }
    }

    [PrimaryKey("PartnerId", "ProviderId")]
    protected abstract class ProviderContract : NotifyingEntity
    {
        private Partner _partner;

        public Partner Partner
        {
            get => _partner;
            set => SetWithNotify(value, ref _partner);
        }
    }

    protected class ProviderContract1 : ProviderContract
    {
        private string _details;

        public string Details
        {
            get => _details;
            set => SetWithNotify(value, ref _details);
        }
    }

    protected class ProviderContract2 : ProviderContract
    {
        private string _details;

        public string Details
        {
            get => _details;
            set => SetWithNotify(value, ref _details);
        }
    }

    protected class Partner : NotifyingEntity
    {
        private string _id;

        public string Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }
    }

    protected class Provider : NotifyingEntity
    {
        private string _id;

        public string Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }
    }

    protected class EventDescriptorZ : NotifyingEntity
    {
        private int _id;
        private EntityZ _entityZ;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public EntityZ EntityZ
        {
            get => _entityZ;
            set => SetWithNotify(value, ref _entityZ);
        }
    }

    protected class EntityZ : NotifyingEntity
    {
        private long _id;

        public long Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }
    }

    protected class City : NotifyingEntity
    {
        private int _id;
        private ICollection<College> _colleges = new ObservableHashSet<College>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public ICollection<College> Colleges
        {
            get => _colleges;
            set => SetWithNotify(value, ref _colleges);
        }
    }

    protected class College : NotifyingEntity
    {
        private int _id;
        private int _cityId;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int CityId
        {
            get => _cityId;
            set => SetWithNotify(value, ref _cityId);
        }
    }

    protected class Cruiser : NotifyingEntity
    {
        private int _cruiserId;
        private int _idUserState;
        private AccessState _userState;

        public int CruiserId
        {
            get => _cruiserId;
            set => SetWithNotify(value, ref _cruiserId);
        }

        public int IdUserState
        {
            get => _idUserState;
            set => SetWithNotify(value, ref _idUserState);
        }

        public virtual AccessState UserState
        {
            get => _userState;
            set => SetWithNotify(value, ref _userState);
        }
    }

    protected class AccessState : NotifyingEntity
    {
        private int _accessStateId;
        private ICollection<Cruiser> _users = new ObservableHashSet<Cruiser>();

        public int AccessStateId
        {
            get => _accessStateId;
            set => SetWithNotify(value, ref _accessStateId);
        }

        public virtual ICollection<Cruiser> Users
        {
            get => _users;
            set => SetWithNotify(value, ref _users);
        }
    }

    protected class CruiserWithSentinel : NotifyingEntity
    {
        private int _cruiserWithSentinelId;
        private int _idUserState;
        private AccessStateWithSentinel _userState;

        public int CruiserWithSentinelId
        {
            get => _cruiserWithSentinelId;
            set => SetWithNotify(value, ref _cruiserWithSentinelId);
        }

        public int IdUserState
        {
            get => _idUserState;
            set => SetWithNotify(value, ref _idUserState);
        }

        public virtual AccessStateWithSentinel UserState
        {
            get => _userState;
            set => SetWithNotify(value, ref _userState);
        }
    }

    protected class AccessStateWithSentinel : NotifyingEntity
    {
        private int _accessStateWithSentinelId;
        private ICollection<CruiserWithSentinel> _users = new ObservableHashSet<CruiserWithSentinel>();

        public int AccessStateWithSentinelId
        {
            get => _accessStateWithSentinelId;
            set => SetWithNotify(value, ref _accessStateWithSentinelId);
        }

        public virtual ICollection<CruiserWithSentinel> Users
        {
            get => _users;
            set => SetWithNotify(value, ref _users);
        }
    }

    protected class SomethingCategory : NotifyingEntity
    {
        private int _id;
        private string _name;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }
    }

    protected class Something : NotifyingEntity
    {
        private int _id;
        private int _categoryId;
        private string _name;
        private SomethingCategory _somethingCategory;
        private SomethingOfCategoryA _somethingOfCategoryA;
        private SomethingOfCategoryB _somethingOfCategoryB;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int CategoryId
        {
            get => _categoryId;
            set => SetWithNotify(value, ref _categoryId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public virtual SomethingCategory SomethingCategory
        {
            get => _somethingCategory;
            set => SetWithNotify(value, ref _somethingCategory);
        }

        public virtual SomethingOfCategoryA SomethingOfCategoryA
        {
            get => _somethingOfCategoryA;
            set => SetWithNotify(value, ref _somethingOfCategoryA);
        }

        public virtual SomethingOfCategoryB SomethingOfCategoryB
        {
            get => _somethingOfCategoryB;
            set => SetWithNotify(value, ref _somethingOfCategoryB);
        }
    }

    protected class SomethingOfCategoryA : NotifyingEntity
    {
        private int _somethingId;
        private string _name;
        private Something _something;

        public int SomethingId
        {
            get => _somethingId;
            set => SetWithNotify(value, ref _somethingId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public virtual Something Something
        {
            get => _something;
            set => SetWithNotify(value, ref _something);
        }
    }

    protected class SomethingOfCategoryB : NotifyingEntity
    {
        private int _somethingId;
        private int _categoryId;
        private string _name;
        private SomethingCategory _somethingCategory;
        private Something _something;

        public int SomethingId
        {
            get => _somethingId;
            set => SetWithNotify(value, ref _somethingId);
        }

        public int CategoryId
        {
            get => _categoryId;
            set => SetWithNotify(value, ref _categoryId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public virtual SomethingCategory SomethingCategory
        {
            get => _somethingCategory;
            set => SetWithNotify(value, ref _somethingCategory);
        }

        public virtual Something Something
        {
            get => _something;
            set => SetWithNotify(value, ref _something);
        }
    }

    protected class Parsnip : NotifyingEntity
    {
        private int _id;
        private Carrot _carrot;
        private Swede _swede;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Carrot Carrot
        {
            get => _carrot;
            set => SetWithNotify(value, ref _carrot);
        }

        public Swede Swede
        {
            get => _swede;
            set => SetWithNotify(value, ref _swede);
        }
    }

    protected class Carrot : NotifyingEntity
    {
        private int _id;
        private int _parsnipId;
        private Parsnip _parsnip;
        private ICollection<Turnip> _turnips = new ObservableHashSet<Turnip>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int ParsnipId
        {
            get => _parsnipId;
            set => SetWithNotify(value, ref _parsnipId);
        }

        public Parsnip Parsnip
        {
            get => _parsnip;
            set => SetWithNotify(value, ref _parsnip);
        }

        public ICollection<Turnip> Turnips
        {
            get => _turnips;
            set => SetWithNotify(value, ref _turnips);
        }
    }

    protected class Turnip : NotifyingEntity
    {
        private int _id;
        private int _carrotsId;
        private Carrot _carrot;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int CarrotsId
        {
            get => _carrotsId;
            set => SetWithNotify(value, ref _carrotsId);
        }

        public Carrot Carrot
        {
            get => _carrot;
            set => SetWithNotify(value, ref _carrot);
        }
    }

    protected class Swede : NotifyingEntity
    {
        private int _id;
        private int _parsnipId;
        private Parsnip _parsnip;
        private ICollection<TurnipSwede> _turnipSwede = new ObservableHashSet<TurnipSwede>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int ParsnipId
        {
            get => _parsnipId;
            set => SetWithNotify(value, ref _parsnipId);
        }

        public Parsnip Parsnip
        {
            get => _parsnip;
            set => SetWithNotify(value, ref _parsnip);
        }

        public ICollection<TurnipSwede> TurnipSwedes
        {
            get => _turnipSwede;
            set => SetWithNotify(value, ref _turnipSwede);
        }
    }

    protected class TurnipSwede : NotifyingEntity
    {
        private int _id;
        private int _swedesId;
        private Swede _swede;
        private int _turnipId;
        private Turnip _turnip;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int SwedesId
        {
            get => _swedesId;
            set => SetWithNotify(value, ref _swedesId);
        }

        public Swede Swede
        {
            get => _swede;
            set => SetWithNotify(value, ref _swede);
        }

        public int TurnipId
        {
            get => _turnipId;
            set => SetWithNotify(value, ref _turnipId);
        }

        public Turnip Turnip
        {
            get => _turnip;
            set => SetWithNotify(value, ref _turnip);
        }
    }

    protected class Bayaz : NotifyingEntity
    {
        private int _bayazId;
        private string _bayazName;
        private ICollection<FirstLaw> _firstLaw = new ObservableHashSet<FirstLaw>();

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BayazId
        {
            get => _bayazId;
            set => SetWithNotify(value, ref _bayazId);
        }

        public string BayazName
        {
            get => _bayazName;
            set => SetWithNotify(value, ref _bayazName);
        }

        public virtual ICollection<FirstLaw> FirstLaw
        {
            get => _firstLaw;
            set => SetWithNotify(value, ref _firstLaw);
        }
    }

    protected class FirstLaw : NotifyingEntity
    {
        private int _firstLawId;
        private string _firstLawName;
        private int _bayazId;
        private Bayaz _bayaz = null!;
        private readonly ICollection<SecondLaw> _secondLaw = new ObservableHashSet<SecondLaw>();

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FirstLawId
        {
            get => _firstLawId;
            set => SetWithNotify(value, ref _firstLawId);
        }

        public string FirstLawName
        {
            get => _firstLawName;
            set => SetWithNotify(value, ref _firstLawName);
        }

        public int BayazId
        {
            get => _bayazId;
            set => SetWithNotify(value, ref _bayazId);
        }

        public virtual Bayaz Bayaz
        {
            get => _bayaz;
            set => SetWithNotify(value, ref _bayaz);
        }

        public virtual ICollection<SecondLaw> SecondLaw
            => _secondLaw;
    }

    protected class SecondLaw : NotifyingEntity
    {
        private int _secondLawId;
        private string _secondLawName;
        private int _firstLawId;
        private FirstLaw _firstLaw = null!;
        private readonly ICollection<ThirdLaw> _thirdLaw = new ObservableHashSet<ThirdLaw>();

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SecondLawId
        {
            get => _secondLawId;
            set => SetWithNotify(value, ref _secondLawId);
        }

        public string SecondLawName
        {
            get => _secondLawName;
            set => SetWithNotify(value, ref _secondLawName);
        }

        public int FirstLawId
        {
            get => _firstLawId;
            set => SetWithNotify(value, ref _firstLawId);
        }

        public virtual FirstLaw FirstLaw
        {
            get => _firstLaw;
            set => SetWithNotify(value, ref _firstLaw);
        }

        public virtual ICollection<ThirdLaw> ThirdLaw
            => _thirdLaw;
    }

    protected class ThirdLaw : NotifyingEntity
    {
        private int _thirdLawId;
        private string _thirdLawName;
        private int _secondLawId;
        private SecondLaw _secondLaw = null!;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ThirdLawId
        {
            get => _thirdLawId;
            set => SetWithNotify(value, ref _thirdLawId);
        }

        public string ThirdLawName
        {
            get => _thirdLawName;
            set => SetWithNotify(value, ref _thirdLawName);
        }

        public int SecondLawId
        {
            get => _secondLawId;
            set => SetWithNotify(value, ref _secondLawId);
        }

        public virtual SecondLaw SecondLaw
        {
            get => _secondLaw;
            set => SetWithNotify(value, ref _secondLaw);
        }
    }

    protected class NaiveParent : NotifyingEntity
    {
        private Guid _id;
        private readonly ICollection<SneakyChild> _children = new ObservableHashSet<SneakyChild>();

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public virtual ICollection<SneakyChild> Children
            => _children;
    }

    protected class SneakyChild : NotifyingEntity
    {
        private Guid _id;
        private Guid _parentId;
        private NaiveParent _parent = null!;

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public virtual NaiveParent Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }
    }

    protected abstract class Parsnip2 : NotifyingEntity
    {
        private int _id;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }
    }

    protected class Lettuce2 : Parsnip2
    {
        private Beetroot2 _root;

        public Beetroot2 Root
        {
            get => _root;
            set => SetWithNotify(value, ref _root);
        }
    }

    protected class RootStructure : NotifyingEntity
    {
        private Guid _radish2Id;
        private int _parsnip2Id;

        public Guid Radish2Id
        {
            get => _radish2Id;
            set => SetWithNotify(value, ref _radish2Id);
        }

        public int Parsnip2Id
        {
            get => _parsnip2Id;
            set => SetWithNotify(value, ref _parsnip2Id);
        }
    }

    protected class Radish2 : NotifyingEntity
    {
        private Guid _id;
        private ICollection<Parsnip2> _entities = new ObservableHashSet<Parsnip2>();

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public ICollection<Parsnip2> Entities
        {
            get => _entities;
            set => SetWithNotify(value, ref _entities);
        }
    }

    protected class Beetroot2 : Parsnip2;

    protected class ParentEntity32084 : NotifyingEntity
    {
        private Guid _id;
        private ChildBaseEntity32084 _child;

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public ChildBaseEntity32084 Child
        {
            get => _child;
            set => SetWithNotify(value, ref _child);
        }
    }

    protected abstract class ChildBaseEntity32084 : NotifyingEntity
    {
        private Guid _id;
        private Guid _parentId;

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }
    }

    protected class ChildEntity32084 : ChildBaseEntity32084
    {
        private string _childValue;

        public string ChildValue
        {
            get => _childValue;
            set => SetWithNotify(value, ref _childValue);
        }
    }

    protected class StableParent32084 : NotifyingEntity
    {
        private Guid _id;
        private StableChild32084 _child;

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public StableChild32084 Child
        {
            get => _child;
            set => SetWithNotify(value, ref _child);
        }
    }

    protected class StableChild32084 : NotifyingEntity
    {
        private Guid _id;
        private Guid _parentId;

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }
    }

    protected class SneakyUncle32084 : NotifyingEntity
    {
        private Guid _id;
        private Guid? _brotherId;
        private StableParent32084 _brother;

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public Guid? BrotherId
        {
            get => _brotherId;
            set => SetWithNotify(value, ref _brotherId);
        }

        public StableParent32084 Brother
        {
            get => _brother;
            set => SetWithNotify(value, ref _brother);
        }
    }

    protected class CompositeKeyWith<T> : NotifyingEntity
        where T : new()
    {
        private Guid _targetId;
        private Guid _sourceId;
        private T _primaryGroup;

        public Guid TargetId
        {
            get => _targetId;
            set => SetWithNotify(value, ref _targetId);
        }

        public Guid SourceId
        {
            get => _sourceId;
            set => SetWithNotify(value, ref _sourceId);
        }

        public T PrimaryGroup
        {
            get => _primaryGroup;
            set => SetWithNotify(value, ref _primaryGroup);
        }
    }

    protected class BoolOnlyKey<T> : NotifyingEntity
        where T : new()
    {
        private T _primaryGroup;

        public T PrimaryGroup
        {
            get => _primaryGroup;
            set => SetWithNotify(value, ref _primaryGroup);
        }
    }

    protected class NotifyingEntity : INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
        {
            NotifyChanging(propertyName);
            field = value;
            NotifyChanged(propertyName);
        }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void NotifyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task> nestedTestOperation1 = null,
        Func<DbContext, Task> nestedTestOperation2 = null,
        Func<DbContext, Task> nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }
}
