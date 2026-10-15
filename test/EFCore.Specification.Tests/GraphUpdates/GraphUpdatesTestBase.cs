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

public abstract partial class GraphUpdatesTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : GraphUpdatesTestBase<TFixture>.GraphUpdatesFixtureBase, new()
{
    protected TFixture Fixture { get; } = fixture;

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
            modelBuilder.Entity<Root>(b =>
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

            modelBuilder.Entity<Optional1>(b =>
            {
                b.HasMany(e => e.Children)
                    .WithOne(e => e.Parent)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(e => e.CompositeChildren)
                    .WithOne(e => e.Parent2)
                    .HasForeignKey(e => new { e.Parent2Id });
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

            modelBuilder.Entity<OptionalSingle2>(b =>
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
                            (l, r) => l!.Value == r!.Value,
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

            modelBuilder.Entity<RequiredAk1>(b =>
            {
                b.Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                b.HasMany(e => e.Children)
                    .WithOne(e => e.Parent)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey(e => e.ParentId);

                b.HasMany(e => e.CompositeChildren)
                    .WithOne(e => e.Parent)
                    .HasPrincipalKey(e => new { e.Id, e.AlternateId })
                    .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
            });

            modelBuilder.Entity<RequiredAk1Derived>();
            modelBuilder.Entity<RequiredAk1MoreDerived>();

            modelBuilder.Entity<OptionalAk1>(b =>
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
                    .HasPrincipalKey(e => new { e.Id, e.AlternateId })
                    .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
            });

            modelBuilder.Entity<OptionalAk1Derived>();
            modelBuilder.Entity<OptionalAk1MoreDerived>();

            modelBuilder.Entity<RequiredSingleAk1>(b =>
            {
                b.Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                b.HasOne(e => e.Single)
                    .WithOne(e => e.Back)
                    .HasForeignKey<RequiredSingleAk2>(e => e.BackId)
                    .HasPrincipalKey<RequiredSingleAk1>(e => e.AlternateId);

                b.HasOne(e => e.SingleComposite)
                    .WithOne(e => e.Back)
                    .HasForeignKey<RequiredSingleComposite2>(e => new { e.BackId, e.BackAlternateId })
                    .HasPrincipalKey<RequiredSingleAk1>(e => new { e.Id, e.AlternateId });
            });

            modelBuilder.Entity<OptionalSingleAk1>(b =>
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
                    .HasForeignKey<OptionalSingleComposite2>(e => new { e.BackId, e.ParentAlternateId })
                    .HasPrincipalKey<OptionalSingleAk1>(e => new { e.Id, e.AlternateId });
            });

            modelBuilder.Entity<OptionalSingleAk2Derived>();
            modelBuilder.Entity<OptionalSingleAk2MoreDerived>();

            modelBuilder.Entity<RequiredNonPkSingleAk1>(b =>
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

            modelBuilder.Entity<RequiredComposite1>(eb =>
            {
                eb.Property(e => e.Id).ValueGeneratedNever();

                eb.HasKey(e => new { e.Id, e.ParentAlternateId });

                eb.HasMany(e => e.CompositeChildren)
                    .WithOne(e => e.Parent)
                    .HasPrincipalKey(e => new { e.Id, e.ParentAlternateId })
                    .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
            });

            modelBuilder.Entity<OptionalOverlapping2>(eb =>
            {
                eb.Property(e => e.Id).ValueGeneratedNever();

                eb.HasKey(e => new { e.Id, e.ParentAlternateId });

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

            modelBuilder.Entity<SharedFkRoot>(builder =>
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

            modelBuilder.Entity<SharedFkParent>(builder => builder.HasOne(x => x.Dependant).WithOne(x => x!.Parent).IsRequired(false)
                .HasForeignKey<SharedFkParent>(x => new { x.RootId, x.DependantId })
                .HasPrincipalKey<SharedFkDependant>(x => new { x.RootId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull));

            modelBuilder.Entity<SharedFkDependant>();

            modelBuilder.Entity<Owner>();

            modelBuilder.Entity<OwnerWithKeyedCollection>(b =>
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

            modelBuilder.Entity<OwnerNoKeyGeneration>(b =>
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

            modelBuilder.Entity<ProviderContract>(b =>
            {
                b.HasOne(p => p.Partner).WithMany().IsRequired().HasForeignKey("PartnerId");
                b.HasOne<Provider>().WithMany().IsRequired().HasForeignKey("ProviderId");

                b.HasDiscriminator<string>("ProviderId")
                    .HasValue<ProviderContract1>("prov1")
                    .HasValue<ProviderContract2>("prov2");

                b.HasKey("PartnerId", "ProviderId");
            });

            modelBuilder.Entity<EventDescriptorZ>(b =>
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

            modelBuilder.Entity<SomethingOfCategoryA>(builder =>
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

            modelBuilder.Entity<SomethingOfCategoryB>(builder =>
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

            modelBuilder.Entity<SneakyChild>(b =>
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

            modelBuilder.Entity<OwnerRoot>(b =>
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

            modelBuilder.Entity<StableParent32084>(b =>
            {
                b.HasOne(x => x.Child).WithOne().HasForeignKey<StableChild32084>(x => x.ParentId);
                b.Property(e => e.Id).HasValueGenerator<StableGuidGenerator>();
            });

            modelBuilder.Entity<StableChild32084>(b => b.Property(e => e.Id).HasValueGenerator<StableGuidGenerator>());

            modelBuilder.Entity<SneakyUncle32084>(b =>
            {
                b.HasOne(x => x.Brother).WithOne().HasForeignKey<SneakyUncle32084>(x => x.BrotherId);
                b.Property(e => e.Id).HasValueGenerator<StableGuidGenerator>();
            });

            modelBuilder.Entity<CompositeKeyWith<int>>(b =>
            {
                b.HasKey(e => new
                {
                    e.TargetId,
                    e.SourceId,
                    e.PrimaryGroup
                });
                b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<CompositeKeyWith<bool>>(b =>
            {
                b.HasKey(e => new
                {
                    e.TargetId,
                    e.SourceId,
                    e.PrimaryGroup
                });
                b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<CompositeKeyWith<bool?>>(b =>
            {
                b.HasKey(e => new
                {
                    e.TargetId,
                    e.SourceId,
                    e.PrimaryGroup
                });
                b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<BoolOnlyKey<bool>>(b =>
            {
                b.HasKey(e => e.PrimaryGroup);
                b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<BoolOnlyKey<bool?>>(b =>
            {
                b.HasKey(e => e.PrimaryGroup);
                b.Property(e => e.PrimaryGroup).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Group37310>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.HasOne(e => e.GroupOwner)
                    .WithMany()
                    .HasForeignKey(e => new { e.Id, e.GroupOwnerId });
            });

            modelBuilder.Entity<GroupMember37310>(b =>
            {
                b.HasKey(e => new { e.GroupId, e.UserId });
                b.HasOne(e => e.User)
                    .WithMany(e => e.Groups)
                    .HasForeignKey(e => e.UserId);
                b.HasOne(e => e.Group)
                    .WithMany(e => e.Members)
                    .HasForeignKey(e => e.GroupId);
            });

            modelBuilder.Entity<User37310>(b => b.Property(e => e.Id).ValueGeneratedNever());

            modelBuilder.Entity<ParentWithClientSetDefault>(b => b.Property(e => e.Id).ValueGeneratedNever());

            modelBuilder.Entity<ChildWithClientSetDefault>(b =>
            {
                b.Property(e => e.ParentId).HasSentinel(667);
                b.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.ClientSetDefault);
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
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2Derived { AlternateId = Guid.NewGuid() }
                    },
                OptionalSingleAkMoreDerived =
                    new OptionalSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2MoreDerived { AlternateId = Guid.NewGuid() }
                    },
                RequiredNonPkSingleAk =
                    new RequiredNonPkSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2 { AlternateId = Guid.NewGuid() }
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
            OptionalSingle = new OwnedOptionalSingle1 { Name = "OS", Single = new OwnedOptionalSingle2 { Name = "OS2" } },
            RequiredSingle = new OwnedRequiredSingle1 { Name = "RS", Single = new OwnedRequiredSingle2 { Name = "RS2 " } },
            OptionalChildren =
            {
                new OwnedOptional1 { Name = "OC1" },
                new OwnedOptional1
                {
                    Name = "OC2", Children = { new OwnedOptional2 { Name = "OCC1" }, new OwnedOptional2 { Name = "OCC2" } }
                }
            },
            RequiredChildren =
            {
                new OwnedRequired1
                {
                    Name = "RC1", Children = { new OwnedRequired2 { Name = "RCC1" }, new OwnedRequired2 { Name = "RCC2" } }
                },
                new OwnedRequired1 { Name = "RC2" }
            }
        };

    protected Task<Root> LoadRequiredGraphAsync(DbContext context)
        => QueryRequiredGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.RequiredChildren).ThenInclude(e => e.Children)
            .Include(e => e.RequiredSingle).ThenInclude(e => e!.Single)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadOptionalGraphAsync(DbContext context)
        => QueryOptionalGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryOptionalGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.OptionalChildren).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildren).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.OptionalSingle!).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleDerived!).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleMoreDerived!).ThenInclude(e => e.Single)
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
            .Include(e => e.RequiredSingleAk).ThenInclude(e => e!.Single)
            .Include(e => e.RequiredSingleAk).ThenInclude(e => e!.SingleComposite)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadOptionalAkGraphAsync(DbContext context)
        => QueryOptionalAkGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryOptionalAkGraph(DbContext context)
        => ModifyQueryRoot(context.Set<Root>())
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.OptionalSingleAk!).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleAk!).ThenInclude(e => e.SingleComposite)
            .Include(e => e.OptionalSingleAkDerived!).ThenInclude(e => e.Single)
            .Include(e => e.OptionalSingleAkMoreDerived!).ThenInclude(e => e.Single)
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
            expected.RequiredCompositeChildren.OrderBy(e => e.Id).Select(e => new { e.Id, e.ParentAlternateId }),
            actual.RequiredCompositeChildren.OrderBy(e => e.Id).Select(e => new { e.Id, e.ParentAlternateId }));

        Assert.Equal(
            expected.RequiredCompositeChildren.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count),
            actual.RequiredCompositeChildren.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count));

        Assert.Equal(
            expected.RequiredCompositeChildren.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id)
                .Select(e => new { e.Id, e.ParentAlternateId }),
            actual.RequiredCompositeChildren.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id)
                .Select(e => new { e.Id, e.ParentAlternateId }));
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
            Assert.Same(root, root.OptionalSingleDerived!.DerivedRoot);
            Assert.Same(root, root.OptionalSingleMoreDerived!.MoreDerivedRoot);
            Assert.Same(root.OptionalSingle, root.OptionalSingle.Single!.Back);
            Assert.Same(root.OptionalSingleDerived, root.OptionalSingleDerived.Single!.Back);
            Assert.Same(root.OptionalSingleMoreDerived, root.OptionalSingleMoreDerived.Single!.Back);
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
            Assert.Same(root, root.OptionalSingleAkDerived!.DerivedRoot);
            Assert.Same(root, root.OptionalSingleAkMoreDerived!.MoreDerivedRoot);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single!.Back);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.SingleComposite!.Back);
            Assert.Same(root.OptionalSingleAkDerived, root.OptionalSingleAkDerived.Single!.Back);
            Assert.Same(root.OptionalSingleAkMoreDerived, root.OptionalSingleAkMoreDerived.Single!.Back);
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
            Assert.Same(root.OptionalSingle, root.OptionalSingle.Single!.Back);
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
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single!.Back);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.SingleComposite!.Back);
        }

        if (root.RequiredNonPkSingleAk != null)
        {
            Assert.Same(root, root.RequiredNonPkSingleAk.Root);
            Assert.Same(root.RequiredNonPkSingleAk, root.RequiredNonPkSingleAk.Single.Back);
        }
    }

    protected class Root : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public IEnumerable<Required1> RequiredChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Required1>(ReferenceEqualityComparer.Instance);

        public IEnumerable<Optional1> OptionalChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Optional1>(ReferenceEqualityComparer.Instance);

        public RequiredSingle1? RequiredSingle
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredNonPkSingle1 RequiredNonPkSingle
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredNonPkSingle1Derived RequiredNonPkSingleDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredNonPkSingle1MoreDerived RequiredNonPkSingleMoreDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public OptionalSingle1? OptionalSingle
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingle1Derived? OptionalSingleDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingle1MoreDerived? OptionalSingleMoreDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public IEnumerable<RequiredAk1> RequiredChildrenAk
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance);

        public IEnumerable<OptionalAk1> OptionalChildrenAk
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance);

        public RequiredSingleAk1? RequiredSingleAk
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredNonPkSingleAk1 RequiredNonPkSingleAk
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredNonPkSingleAk1Derived RequiredNonPkSingleAkDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredNonPkSingleAk1MoreDerived RequiredNonPkSingleAkMoreDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public OptionalSingleAk1? OptionalSingleAk
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingleAk1Derived? OptionalSingleAkDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingleAk1MoreDerived? OptionalSingleAkMoreDerived
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public IEnumerable<RequiredComposite1> RequiredCompositeChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<RequiredComposite1>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object? obj)
        {
            var other = obj as Root;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class Required1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public IEnumerable<Required2> Children
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object? obj)
        {
            var other = obj as Required1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class Required1Derived : Required1
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Required1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Required1MoreDerived : Required1Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Required1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Required2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Required1 Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as Required2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class Required2Derived : Required2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Required2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Required2MoreDerived : Required2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Required2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public IEnumerable<Optional2> Children
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance);

        public ICollection<OptionalComposite2> CompositeChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object? obj)
        {
            var other = obj as Optional1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class Optional1Derived : Optional1
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Optional1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional1MoreDerived : Optional1Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Optional1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Optional1? Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as Optional2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class Optional2Derived : Optional2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Optional2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class Optional2MoreDerived : Optional2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as Optional2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredSingle1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public bool Bool
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Root
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredSingle2 Single
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredSingle1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredSingle2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public bool Bool
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredSingle1 Back
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredSingle2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredNonPkSingle1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int RootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Root
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredNonPkSingle2 Single
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredNonPkSingle1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredNonPkSingle1Derived : RequiredNonPkSingle1
    {
        public int DerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root DerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingle1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingle1MoreDerived : RequiredNonPkSingle1Derived
    {
        public int MoreDerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root MoreDerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingle1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingle2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int BackId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredNonPkSingle1 Back
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredNonPkSingle2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredNonPkSingle2Derived : RequiredNonPkSingle2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingle2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingle2MoreDerived : RequiredNonPkSingle2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingle2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? RootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? Root
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingle2? Single
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalSingle1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalSingle1Derived : OptionalSingle1
    {
        public int? DerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? DerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingle1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle1MoreDerived : OptionalSingle1Derived
    {
        public int? MoreDerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? MoreDerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingle1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? BackId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public MyDiscriminator Disc
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public OptionalSingle1? Back
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalSingle2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class MyDiscriminator(int value)
    {
        public int Value { get; } = value;

        public override bool Equals(object? obj)
            => throw new InvalidOperationException();

        public override int GetHashCode()
            => throw new InvalidOperationException();
    }

    protected class OptionalSingle2Derived : OptionalSingle2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingle2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingle2MoreDerived : OptionalSingle2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingle2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public IEnumerable<RequiredAk2> Children
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance);

        public IEnumerable<RequiredComposite2> CompositeChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredAk1Derived : RequiredAk1
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk1MoreDerived : RequiredAk1Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredAk1 Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredComposite1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentAlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredComposite1;
            return Id == other?.Id;
        }

        public ICollection<OptionalOverlapping2> CompositeChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance);

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalOverlapping2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentAlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredComposite1? Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Root
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalOverlapping2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredComposite2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentAlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredAk1 Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredAk2Derived : RequiredAk2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredAk2MoreDerived : RequiredAk2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid? ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public IEnumerable<OptionalAk2> Children
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance);

        public ICollection<OptionalComposite2> CompositeChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalAk1Derived : OptionalAk1
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk1MoreDerived : OptionalAk1Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid? ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalAk1? Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalComposite2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentAlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalAk1? Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? Parent2Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Optional1? Parent2
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalAk2Derived : OptionalAk2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalAk2MoreDerived : OptionalAk2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredSingleAk1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid RootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Root
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredSingleAk2 Single
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredSingleComposite2 SingleComposite
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredSingleAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredSingleAk2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid BackId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredSingleAk1 Back
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredSingleAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredSingleComposite2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid BackAlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int BackId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredSingleAk1 Back
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredSingleComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredNonPkSingleAk1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid RootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root Root
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public RequiredNonPkSingleAk2 Single
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredNonPkSingleAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredNonPkSingleAk1Derived : RequiredNonPkSingleAk1
    {
        public Guid DerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root DerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingleAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingleAk1MoreDerived : RequiredNonPkSingleAk1Derived
    {
        public Guid MoreDerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root MoreDerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingleAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingleAk2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid BackId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public RequiredNonPkSingleAk1 Back
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public override bool Equals(object? obj)
        {
            var other = obj as RequiredNonPkSingleAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class RequiredNonPkSingleAk2Derived : RequiredNonPkSingleAk2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingleAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class RequiredNonPkSingleAk2MoreDerived : RequiredNonPkSingleAk2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as RequiredNonPkSingleAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk1 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid? RootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? Root
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingleComposite2? SingleComposite
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingleAk2? Single
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalSingleAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalSingleAk1Derived : OptionalSingleAk1
    {
        public Guid? DerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? DerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingleAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk1MoreDerived : OptionalSingleAk1Derived
    {
        public Guid? MoreDerivedRootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Root? MoreDerivedRoot
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingleAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid AlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid? BackId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingleAk1? Back
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalSingleAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalSingleComposite2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentAlternateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? BackId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OptionalSingleAk1? Back
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionalSingleComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    protected class OptionalSingleAk2Derived : OptionalSingleAk2
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingleAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OptionalSingleAk2MoreDerived : OptionalSingleAk2Derived
    {
        public override bool Equals(object? obj)
            => base.Equals(obj as OptionalSingleAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    protected class OwnerRoot : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OwnedRequiredSingle1 RequiredSingle
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public OwnedOptionalSingle1? OptionalSingle
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ICollection<OwnedRequired1> RequiredChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OwnedRequired1>(ReferenceEqualityComparer.Instance);

        public ICollection<OwnedOptional1> OptionalChildren
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OwnedOptional1>(ReferenceEqualityComparer.Instance);
    }

    protected class OwnedRequired1 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public ICollection<OwnedRequired2> Children
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OwnedRequired2>(ReferenceEqualityComparer.Instance);
    }

    protected class OwnedRequired2 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class OwnedOptional1 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public ICollection<OwnedOptional2> Children
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OwnedOptional2>(ReferenceEqualityComparer.Instance);
    }

    protected class OwnedOptional2 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class OwnedRequiredSingle1 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public OwnedRequiredSingle2 Single
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class OwnedRequiredSingle2 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class OwnedOptionalSingle1 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public OwnedOptionalSingle2? Single
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class OwnedOptionalSingle2 : NotifyingEntity
    {
        [Required]
        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class BadCustomer : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int Status
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ICollection<BadOrder> BadOrders
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<BadOrder>(ReferenceEqualityComparer.Instance);
    }

    protected class BadOrder : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? BadCustomerId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public BadCustomer? BadCustomer
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class HiddenAreaTask : TaskWithChoices;

    protected abstract class QuestTask : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class QuizTask : TaskWithChoices;

    protected class TaskChoice : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int QuestTaskId
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class ParentAsAChild : NotifyingEntity
    {
        public bool Filler { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? ChildAsAParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ChildAsAParent? ChildAsAParent
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class ChildAsAParent : NotifyingEntity
    {
        public bool Filler { get; set; }

        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ParentAsAChild? ParentAsAChild
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected abstract class TaskWithChoices : QuestTask
    {
        public ICollection<TaskChoice> Choices
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<TaskChoice>(ReferenceEqualityComparer.Instance);
    }

    protected class Produce : NotifyingEntity
    {
        public Guid ProduceId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public int BarCode
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class Bloog : NotifyingEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public IEnumerable<Poost> Poosts
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Poost>(ReferenceEqualityComparer.Instance);
    }

    protected class Poost : NotifyingEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int? BloogId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Bloog? Bloog
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class SharedFkRoot : NotifyingEntity
    {
        public long Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ICollection<SharedFkDependant> Dependants
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<SharedFkDependant>(ReferenceEqualityComparer.Instance);

        public ICollection<SharedFkParent> Parents
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<SharedFkParent>(ReferenceEqualityComparer.Instance);
    }

    protected class SharedFkParent : NotifyingEntity
    {
        public long Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public long? DependantId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public long RootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public SharedFkRoot Root
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public SharedFkDependant? Dependant
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class SharedFkDependant : NotifyingEntity
    {
        public long Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public long RootId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public SharedFkRoot Root
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public SharedFkParent? Parent
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class Owner : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Owned Owned
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public ICollection<Owned> OwnedCollection
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Owned>();
    }

    [Owned]
    protected class Owned : NotifyingEntity
    {
        public int Foo
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string? Bar
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class OwnerWithKeyedCollection : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Owned Owned
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public OwnedWithKey OwnedWithKey
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public ICollection<OwnedWithKey> OwnedCollection
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OwnedWithKey>();

        public ICollection<OwnedWithPrivateKey> OwnedCollectionPrivateKey
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OwnedWithPrivateKey>();
    }

    [Owned]
    protected class OwnedWithKey : NotifyingEntity
    {
        public int OwnedWithKeyId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int Foo
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string? Bar
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    [Owned]
    protected class OwnedWithPrivateKey : NotifyingEntity
    {
        private int PrivateKey
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int Foo
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string? Bar
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class OwnerWithNonCompositeOwnedCollection : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ICollection<NonCompositeOwnedCollection> Owned
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<NonCompositeOwnedCollection>();
    }

    protected class NonCompositeOwnedCollection : NotifyingEntity
    {
        public string Foo
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class OwnerNoKeyGeneration : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public OwnedNoKeyGeneration Owned
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public ICollection<OwnedNoKeyGeneration> OwnedCollection
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<OwnedNoKeyGeneration>();
    }

    [Owned]
    protected class OwnedNoKeyGeneration : NotifyingEntity
    {
        public int Foo
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string? Bar
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    [PrimaryKey("PartnerId", "ProviderId")]
    protected abstract class ProviderContract : NotifyingEntity
    {
        public Partner Partner
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class ProviderContract1 : ProviderContract
    {
        public string Details
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class ProviderContract2 : ProviderContract
    {
        public string Details
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class Partner : NotifyingEntity
    {
        public string Id
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class Provider : NotifyingEntity
    {
        public string Id
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class EventDescriptorZ : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public EntityZ EntityZ
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class EntityZ : NotifyingEntity
    {
        public long Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class City : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ICollection<College> Colleges
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<College>();
    }

    protected class College : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int CityId
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class Cruiser : NotifyingEntity
    {
        private AccessState _userState = null!;

        public int CruiserId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int IdUserState
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual AccessState UserState
        {
            get => _userState;
            set => SetWithNotify(value, ref _userState);
        }
    }

    protected class AccessState : NotifyingEntity
    {
        private ICollection<Cruiser> _users = new ObservableHashSet<Cruiser>();

        public int AccessStateId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual ICollection<Cruiser> Users
        {
            get => _users;
            set => SetWithNotify(value, ref _users);
        }
    }

    protected class CruiserWithSentinel : NotifyingEntity
    {
        private AccessStateWithSentinel _userState = null!;

        public int CruiserWithSentinelId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int IdUserState
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual AccessStateWithSentinel UserState
        {
            get => _userState;
            set => SetWithNotify(value, ref _userState);
        }
    }

    protected class AccessStateWithSentinel : NotifyingEntity
    {
        private ICollection<CruiserWithSentinel> _users = new ObservableHashSet<CruiserWithSentinel>();

        public int AccessStateWithSentinelId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual ICollection<CruiserWithSentinel> Users
        {
            get => _users;
            set => SetWithNotify(value, ref _users);
        }
    }

    protected class ParentWithClientSetDefault : NotifyingEntity
    {
        private ICollection<ChildWithClientSetDefault> _children = new ObservableHashSet<ChildWithClientSetDefault>();

        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual ICollection<ChildWithClientSetDefault> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }
    }

    protected class ChildWithClientSetDefault : NotifyingEntity
    {
        private ParentWithClientSetDefault _parent = null!;

        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual ParentWithClientSetDefault Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }
    }

    protected class SomethingCategory : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class Something : NotifyingEntity
    {
        private SomethingCategory _somethingCategory = null!;
        private SomethingOfCategoryA _somethingOfCategoryA = null!;
        private SomethingOfCategoryB _somethingOfCategoryB = null!;

        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int CategoryId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

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
        private Something _something = null!;

        public int SomethingId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public virtual Something Something
        {
            get => _something;
            set => SetWithNotify(value, ref _something);
        }
    }

    protected class SomethingOfCategoryB : NotifyingEntity
    {
        private SomethingCategory _somethingCategory = null!;
        private Something _something = null!;

        public int SomethingId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int CategoryId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string Name
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

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
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Carrot? Carrot
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Swede? Swede
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class Carrot : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int ParsnipId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Parsnip Parsnip
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public ICollection<Turnip> Turnips
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Turnip>();
    }

    protected class Turnip : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int CarrotsId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Carrot? Carrot
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class Swede : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int ParsnipId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Parsnip Parsnip
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public ICollection<TurnipSwede> TurnipSwedes
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<TurnipSwede>();
    }

    protected class TurnipSwede : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int SwedesId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Swede? Swede
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int TurnipId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Turnip Turnip
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class Bayaz : NotifyingEntity
    {
        private ICollection<FirstLaw> _firstLaw = new ObservableHashSet<FirstLaw>();

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BayazId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string BayazName
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public virtual ICollection<FirstLaw> FirstLaw
        {
            get => _firstLaw;
            set => SetWithNotify(value, ref _firstLaw);
        }
    }

    protected class FirstLaw : NotifyingEntity
    {
        private Bayaz _bayaz = null!;
        private readonly ICollection<SecondLaw> _secondLaw = new ObservableHashSet<SecondLaw>();

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FirstLawId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string FirstLawName
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public int BayazId
        {
            get;
            set => SetWithNotify(value, ref field);
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
        private FirstLaw _firstLaw = null!;
        private readonly ICollection<ThirdLaw> _thirdLaw = new ObservableHashSet<ThirdLaw>();

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SecondLawId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string SecondLawName
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public int FirstLawId
        {
            get;
            set => SetWithNotify(value, ref field);
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
        private SecondLaw _secondLaw = null!;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ThirdLawId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public string ThirdLawName
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;

        public int SecondLawId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual SecondLaw SecondLaw
        {
            get => _secondLaw;
            set => SetWithNotify(value, ref _secondLaw);
        }
    }

    protected class NaiveParent : NotifyingEntity
    {
        private readonly ICollection<SneakyChild> _children = new ObservableHashSet<SneakyChild>();

        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual ICollection<SneakyChild> Children
            => _children;
    }

    protected class SneakyChild : NotifyingEntity
    {
        private NaiveParent _parent = null!;

        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public virtual NaiveParent Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }
    }

    protected abstract class Parsnip2 : NotifyingEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class Lettuce2 : Parsnip2
    {
        public Beetroot2? Root
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class RootStructure : NotifyingEntity
    {
        public Guid Radish2Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int Parsnip2Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class Radish2 : NotifyingEntity
    {
        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ICollection<Parsnip2> Entities
        {
            get;
            set => SetWithNotify(value, ref field);
        } = new ObservableHashSet<Parsnip2>();
    }

    protected class Beetroot2 : Parsnip2;

    protected class ParentEntity32084 : NotifyingEntity
    {
        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ChildBaseEntity32084 Child
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected abstract class ChildBaseEntity32084 : NotifyingEntity
    {
        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class ChildEntity32084 : ChildBaseEntity32084
    {
        public string ChildValue
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class StableParent32084 : NotifyingEntity
    {
        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public StableChild32084 Child
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class StableChild32084 : NotifyingEntity
    {
        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid ParentId
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class SneakyUncle32084 : NotifyingEntity
    {
        public Guid Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid? BrotherId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public StableParent32084? Brother
        {
            get;
            set => SetWithNotify(value, ref field);
        }
    }

    protected class CompositeKeyWith<T> : NotifyingEntity
        where T : new()
    {
        public Guid TargetId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Guid SourceId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public T PrimaryGroup
        {
            get;
            set => SetWithNotify(value, ref field);
        } = default!;
    }

    protected class BoolOnlyKey<T> : NotifyingEntity
        where T : new()
    {
        public T PrimaryGroup
        {
            get;
            set => SetWithNotify(value, ref field);
        } = default!;
    }

    protected class NotifyingEntity : INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
        {
            NotifyChanging(propertyName);
            field = value;
            NotifyChanged(propertyName);
        }

        public event PropertyChangingEventHandler? PropertyChanging;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void NotifyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task>? nestedTestOperation1 = null,
        Func<DbContext, Task>? nestedTestOperation2 = null,
        Func<DbContext, Task>? nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }
}
