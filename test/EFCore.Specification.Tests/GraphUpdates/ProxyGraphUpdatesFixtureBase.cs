// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class ProxyGraphUpdatesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ProxyGraphUpdatesTestBase<TFixture>.ProxyGraphUpdatesFixtureBase, new()
{
    protected ProxyGraphUpdatesTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected abstract bool DoesLazyLoading { get; }
    protected abstract bool DoesChangeTracking { get; }

    public abstract class ProxyGraphUpdatesFixtureBase : SharedStoreFixtureBase<DbContext>
    {
        public readonly Guid RootAK = Guid.NewGuid();

        protected override bool UsePooling
            => false;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).AddInterceptors(new UpdatingIdentityResolutionInterceptor());

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

            modelBuilder.Entity<OptionalSingle2Derived>();
            modelBuilder.Entity<OptionalSingle2MoreDerived>();

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

            modelBuilder.Entity<Person>(
                p =>
                {
                    p.HasKey(tp => tp.Id);
                });

            modelBuilder.Entity<Car>(
                c =>
                {
                    c.HasKey(tc => tc.Id);
                    c.HasOne(tc => tc.Owner)
                        .WithOne(tp => tp.Vehicle)
                        .HasForeignKey<Car>("fk_PersonId")
                        .IsRequired();
                });

            modelBuilder.Entity<RecordCar>();
        }

        protected virtual object CreateFullGraph(DbContext context)
            => context.CreateProxy<Root>(
                e =>
                {
                    e.AlternateId = RootAK;

                    e.RequiredChildren = new ObservableHashSet<Required1>(ReferenceEqualityComparer.Instance)
                    {
                        context.CreateProxy<Required1>(
                            e =>
                            {
                                e.Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<Required2>().CreateProxy(), context.Set<Required2>().CreateProxy()
                                };
                            }),
                        context.CreateProxy<Required1>(
                            e =>
                            {
                                e.Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<Required2>().CreateProxy(), context.Set<Required2>().CreateProxy()
                                };
                            })
                    };

                    e.OptionalChildren = new ObservableHashSet<Optional1>(ReferenceEqualityComparer.Instance)
                    {
                        context.Set<Optional1>().CreateProxy(
                            e =>
                            {
                                e.Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<Optional2>().CreateProxy(), context.Set<Optional2>().CreateProxy()
                                };

                                e.CompositeChildren =
                                    new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);
                            }),
                        context.Set<Optional1>().CreateProxy(
                            e =>
                            {
                                e.Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<Optional2>().CreateProxy(), context.Set<Optional2>().CreateProxy()
                                };

                                e.CompositeChildren =
                                    new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);
                            })
                    };

                    e.RequiredSingle = context.CreateProxy<RequiredSingle1>(
                        e => e.Single = context.Set<RequiredSingle2>().CreateProxy());

                    e.OptionalSingle = context.CreateProxy<OptionalSingle1>(
                        e => e.Single = context.Set<OptionalSingle2>().CreateProxy());

                    e.OptionalSingleDerived = context.CreateProxy<OptionalSingle1Derived>(
                        e => e.Single = context.Set<OptionalSingle2Derived>().CreateProxy());

                    e.OptionalSingleMoreDerived = context.CreateProxy<OptionalSingle1MoreDerived>(
                        e => e.Single = context.Set<OptionalSingle2MoreDerived>().CreateProxy());

                    e.RequiredNonPkSingle = context.CreateProxy<RequiredNonPkSingle1>(
                        e => e.Single = context.Set<RequiredNonPkSingle2>().CreateProxy());

                    e.RequiredNonPkSingleDerived = context.CreateProxy<RequiredNonPkSingle1Derived>(
                        e =>
                        {
                            e.Single = context.Set<RequiredNonPkSingle2Derived>().CreateProxy();
                            e.Root = context.Set<Root>().CreateProxy();
                        });

                    e.RequiredNonPkSingleMoreDerived = context.CreateProxy<RequiredNonPkSingle1MoreDerived>(
                        e =>
                        {
                            e.Single = context.Set<RequiredNonPkSingle2MoreDerived>().CreateProxy();
                            e.Root = context.Set<Root>().CreateProxy();
                            e.DerivedRoot = context.Set<Root>().CreateProxy();
                        });

                    e.RequiredChildrenAk = new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance)
                    {
                        context.Set<RequiredAk1>().CreateProxy(
                            e =>
                            {
                                e.AlternateId = Guid.NewGuid();

                                e.Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<RequiredAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid()),
                                    context.Set<RequiredAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid())
                                };

                                e.CompositeChildren =
                                    new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance)
                                    {
                                        context.Set<RequiredComposite2>().CreateProxy(), context.Set<RequiredComposite2>().CreateProxy()
                                    };
                            }),
                        context.Set<RequiredAk1>().CreateProxy(
                            e =>
                            {
                                e.AlternateId = Guid.NewGuid();

                                e.Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<RequiredAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid()),
                                    context.Set<RequiredAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid())
                                };

                                e.CompositeChildren =
                                    new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance)
                                    {
                                        context.Set<RequiredComposite2>().CreateProxy(), context.Set<RequiredComposite2>().CreateProxy()
                                    };
                            })
                    };

                    e.OptionalChildrenAk = new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance)
                    {
                        context.Set<OptionalAk1>().CreateProxy(
                            e =>
                            {
                                e.AlternateId = Guid.NewGuid();

                                e.Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<OptionalAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid()),
                                    context.Set<OptionalAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid())
                                };
                                e.CompositeChildren =
                                    new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                                    {
                                        context.Set<OptionalComposite2>().CreateProxy(), context.Set<OptionalComposite2>().CreateProxy()
                                    };
                            }),
                        context.Set<OptionalAk1>().CreateProxy(
                            e =>
                            {
                                e.AlternateId = Guid.NewGuid();

                                e.Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                                {
                                    context.Set<OptionalAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid()),
                                    context.Set<OptionalAk2>().CreateProxy(e => e.AlternateId = Guid.NewGuid())
                                };

                                e.CompositeChildren =
                                    new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                                    {
                                        context.Set<OptionalComposite2>().CreateProxy(), context.Set<OptionalComposite2>().CreateProxy()
                                    };
                            })
                    };

                    e.RequiredSingleAk = context.CreateProxy<RequiredSingleAk1>(
                        e =>
                        {
                            e.AlternateId = Guid.NewGuid();
                            e.Single = context.CreateProxy<RequiredSingleAk2>(e => e.AlternateId = Guid.NewGuid());
                            e.SingleComposite = context.CreateProxy<RequiredSingleComposite2>();
                        });

                    e.OptionalSingleAk = context.CreateProxy<OptionalSingleAk1>(
                        e =>
                        {
                            e.AlternateId = Guid.NewGuid();
                            e.Single = context.CreateProxy<OptionalSingleAk2>(e => e.AlternateId = Guid.NewGuid());
                            e.SingleComposite = context.CreateProxy<OptionalSingleComposite2>();
                        });

                    e.OptionalSingleAkDerived = context.CreateProxy<OptionalSingleAk1Derived>(
                        e =>
                        {
                            e.AlternateId = Guid.NewGuid();
                            e.Single = context.CreateProxy<OptionalSingleAk2Derived>(e => e.AlternateId = Guid.NewGuid());
                        });

                    e.OptionalSingleAkMoreDerived = context.CreateProxy<OptionalSingleAk1MoreDerived>(
                        e =>
                        {
                            e.AlternateId = Guid.NewGuid();
                            e.Single = context.CreateProxy<OptionalSingleAk2MoreDerived>(e => e.AlternateId = Guid.NewGuid());
                        });

                    e.RequiredNonPkSingleAk = context.CreateProxy<RequiredNonPkSingleAk1>(
                        e =>
                        {
                            e.AlternateId = Guid.NewGuid();
                            e.Single = context.CreateProxy<RequiredNonPkSingleAk2>(e => e.AlternateId = Guid.NewGuid());
                        });

                    e.RequiredNonPkSingleAkDerived = context.CreateProxy<RequiredNonPkSingleAk1Derived>(
                        e =>
                        {
                            e.AlternateId = Guid.NewGuid();
                            e.Single = context.CreateProxy<RequiredNonPkSingleAk2Derived>(e => e.AlternateId = Guid.NewGuid());
                            e.Root = context.CreateProxy<Root>();
                        });

                    e.RequiredNonPkSingleAkMoreDerived = context.CreateProxy<RequiredNonPkSingleAk1MoreDerived>(
                        e =>
                        {
                            e.AlternateId = Guid.NewGuid();
                            e.Single = context.CreateProxy<RequiredNonPkSingleAk2MoreDerived>(e => e.AlternateId = Guid.NewGuid());
                            e.Root = context.CreateProxy<Root>();
                            e.DerivedRoot = context.CreateProxy<Root>();
                        });

                    e.RequiredCompositeChildren = new ObservableHashSet<RequiredComposite1>(ReferenceEqualityComparer.Instance)
                    {
                        context.Set<RequiredComposite1>().CreateProxy(
                            e =>
                            {
                                e.Id = 1;

                                e.CompositeChildren =
                                    new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance)
                                    {
                                        context.CreateProxy<OptionalOverlapping2>(e => e.Id = 1),
                                        context.CreateProxy<OptionalOverlapping2>(e => e.Id = 2)
                                    };
                            }),
                        context.Set<RequiredComposite1>().CreateProxy(
                            e =>
                            {
                                e.Id = 2;

                                e.CompositeChildren =
                                    new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance)
                                    {
                                        context.CreateProxy<OptionalOverlapping2>(e => e.Id = 3),
                                        context.CreateProxy<OptionalOverlapping2>(e => e.Id = 4)
                                    };
                            })
                    };
                });

        protected override Task SeedAsync(DbContext context)
        {
            var tracker = new KeyValueEntityTracker();

            context.ChangeTracker.TrackGraph(CreateFullGraph(context), e => tracker.TrackEntity(e.Entry));

            context.Add(context.CreateProxy<BadOrder>(e => e.BadCustomer = context.CreateProxy<BadCustomer>()));

            var root = context.CreateProxy<SharedFkRoot>();
            context.Add(root);

            var parent = context.CreateProxy<SharedFkParent>();
            parent.Root = root;
            context.Add(parent);

            var dependent = context.CreateProxy<SharedFkDependant>();
            dependent.Root = root;
            dependent.Parent = parent;
            context.Add(dependent);

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

    private static void Add<T>(IEnumerable<T> collection, T item)
        => ((ICollection<T>)collection).Add(item);

    private static void Remove<T>(IEnumerable<T> collection, T item)
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

    protected Task<Root> LoadRootAsync(DbContext context)
        => context.Set<Root>().SingleAsync(IsTheRoot);

    protected Task<Root> LoadRequiredGraphAsync(DbContext context)
        => QueryRequiredGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredGraph(DbContext context)
        => context.Set<Root>()
            .Include(e => e.RequiredChildren).ThenInclude(e => e.Children)
            .Include(e => e.RequiredSingle).ThenInclude(e => e.Single)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadOptionalGraphAsync(DbContext context)
        => QueryOptionalGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryOptionalGraph(DbContext context)
        => context.Set<Root>()
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
        => context.Set<Root>()
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
        => context.Set<Root>()
            .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.Children)
            .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.RequiredSingleAk).ThenInclude(e => e.Single)
            .Include(e => e.RequiredSingleAk).ThenInclude(e => e.SingleComposite)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadOptionalAkGraphAsync(DbContext context)
        => QueryOptionalAkGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryOptionalAkGraph(DbContext context)
        => context.Set<Root>()
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
        => context.Set<Root>()
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
        => context.Set<Root>()
            .Include(e => e.OptionalChildren).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildren).ThenInclude(e => e.CompositeChildren)
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
            .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
            .OrderBy(e => e.Id);

    protected Task<Root> LoadRequiredCompositeGraphAsync(DbContext context)
        => QueryRequiredCompositeGraph(context)
            .SingleAsync(IsTheRoot);

    protected IOrderedQueryable<Root> QueryRequiredCompositeGraph(DbContext context)
        => context.Set<Root>()
            .Include(e => e.RequiredCompositeChildren).ThenInclude(e => e.CompositeChildren)
            .OrderBy(e => e.Id);

    protected static void AssertEntries(IReadOnlyList<EntityEntry> expectedEntries, IReadOnlyList<EntityEntry> actualEntries)
    {
        var newEntities = new HashSet<object>(actualEntries.Select(ne => ne.Entity));
        var missingEntities = expectedEntries.Select(e => e.Entity).Where(e => !newEntities.Contains(e)).ToList();
        Assert.Equal([], missingEntities);
        Assert.Equal(expectedEntries.Count, actualEntries.Count);
    }

    public class Root
    {
        protected Root()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual IEnumerable<Required1> RequiredChildren { get; set; }
            = new ObservableHashSet<Required1>(ReferenceEqualityComparer.Instance);

        public virtual IEnumerable<Optional1> OptionalChildren { get; set; }
            = new ObservableHashSet<Optional1>(ReferenceEqualityComparer.Instance);

        public virtual RequiredSingle1 RequiredSingle { get; set; }

        public virtual RequiredNonPkSingle1 RequiredNonPkSingle { get; set; }

        public virtual RequiredNonPkSingle1Derived RequiredNonPkSingleDerived { get; set; }

        public virtual RequiredNonPkSingle1MoreDerived RequiredNonPkSingleMoreDerived { get; set; }

        public virtual OptionalSingle1 OptionalSingle { get; set; }

        public virtual OptionalSingle1Derived OptionalSingleDerived { get; set; }

        public virtual OptionalSingle1MoreDerived OptionalSingleMoreDerived { get; set; }

        public virtual IEnumerable<RequiredAk1> RequiredChildrenAk { get; set; }
            = new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance);

        public virtual IEnumerable<OptionalAk1> OptionalChildrenAk { get; set; }
            = new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance);

        public virtual RequiredSingleAk1 RequiredSingleAk { get; set; }

        public virtual RequiredNonPkSingleAk1 RequiredNonPkSingleAk { get; set; }

        public virtual RequiredNonPkSingleAk1Derived RequiredNonPkSingleAkDerived { get; set; }

        public virtual RequiredNonPkSingleAk1MoreDerived RequiredNonPkSingleAkMoreDerived { get; set; }

        public virtual OptionalSingleAk1 OptionalSingleAk { get; set; }

        public virtual OptionalSingleAk1Derived OptionalSingleAkDerived { get; set; }

        public virtual OptionalSingleAk1MoreDerived OptionalSingleAkMoreDerived { get; set; }

        public virtual IEnumerable<RequiredComposite1> RequiredCompositeChildren { get; set; }
            = new ObservableHashSet<RequiredComposite1>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object obj)
        {
            var other = obj as Root;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class Required1
    {
        protected Required1()
        {
        }

        public virtual int Id { get; set; }

        public virtual int ParentId { get; set; }

        public virtual Root Parent { get; set; }

        public virtual IEnumerable<Required2> Children { get; set; }
            = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object obj)
        {
            var other = obj as Required1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class Required1Derived : Required1
    {
        protected Required1Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Required1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class Required1MoreDerived : Required1Derived
    {
        protected Required1MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Required1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class Required2
    {
        protected Required2()
        {
        }

        public virtual int Id { get; set; }

        public virtual int ParentId { get; set; }

        public virtual Required1 Parent { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Required2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class Required2Derived : Required2
    {
        protected Required2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Required2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class Required2MoreDerived : Required2Derived
    {
        protected Required2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Required2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class Optional1
    {
        protected Optional1()
        {
        }

        public virtual int Id { get; set; }

        public virtual int? ParentId { get; set; }

        public virtual Root Parent { get; set; }

        public virtual IEnumerable<Optional2> Children { get; set; }
            = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance);

        public virtual ICollection<OptionalComposite2> CompositeChildren { get; set; }
            = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object obj)
        {
            var other = obj as Optional1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class Optional1Derived : Optional1
    {
        protected Optional1Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Optional1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class Optional1MoreDerived : Optional1Derived
    {
        protected Optional1MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Optional1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class Optional2
    {
        protected Optional2()
        {
        }

        public virtual int Id { get; set; }

        public virtual int? ParentId { get; set; }

        public virtual Optional1 Parent { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Optional2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class Optional2Derived : Optional2
    {
        protected Optional2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Optional2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class Optional2MoreDerived : Optional2Derived
    {
        protected Optional2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as Optional2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredSingle1
    {
        protected RequiredSingle1()
        {
        }

        public virtual int Id { get; set; }

        public virtual Root Root { get; set; }

        public virtual RequiredSingle2 Single { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingle1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredSingle2
    {
        protected RequiredSingle2()
        {
        }

        public virtual int Id { get; set; }

        public virtual RequiredSingle1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingle2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredNonPkSingle1
    {
        protected RequiredNonPkSingle1()
        {
        }

        public virtual int Id { get; set; }

        public virtual int RootId { get; set; }

        public virtual Root Root { get; set; }

        public virtual RequiredNonPkSingle2 Single { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingle1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredNonPkSingle1Derived : RequiredNonPkSingle1
    {
        protected RequiredNonPkSingle1Derived()
        {
        }

        public virtual int DerivedRootId { get; set; }

        public virtual Root DerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredNonPkSingle1MoreDerived : RequiredNonPkSingle1Derived
    {
        protected RequiredNonPkSingle1MoreDerived()
        {
        }

        public virtual int MoreDerivedRootId { get; set; }

        public virtual Root MoreDerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredNonPkSingle2
    {
        protected RequiredNonPkSingle2()
        {
        }

        public virtual int Id { get; set; }

        public virtual int BackId { get; set; }

        public virtual RequiredNonPkSingle1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingle2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredNonPkSingle2Derived : RequiredNonPkSingle2
    {
        protected RequiredNonPkSingle2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredNonPkSingle2MoreDerived : RequiredNonPkSingle2Derived
    {
        protected RequiredNonPkSingle2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingle2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingle1
    {
        protected OptionalSingle1()
        {
        }

        public virtual int Id { get; set; }

        public virtual int? RootId { get; set; }

        public virtual Root Root { get; set; }

        public virtual OptionalSingle2 Single { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingle1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalSingle1Derived : OptionalSingle1
    {
        protected OptionalSingle1Derived()
        {
        }

        public virtual int? DerivedRootId { get; set; }

        public virtual Root DerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingle1MoreDerived : OptionalSingle1Derived
    {
        protected OptionalSingle1MoreDerived()
        {
        }

        public virtual int? MoreDerivedRootId { get; set; }

        public virtual Root MoreDerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingle2
    {
        protected OptionalSingle2()
        {
        }

        public virtual int Id { get; set; }

        public virtual int? BackId { get; set; }

        public virtual OptionalSingle1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingle2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalSingle2Derived : OptionalSingle2
    {
        protected OptionalSingle2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingle2MoreDerived : OptionalSingle2Derived
    {
        protected OptionalSingle2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingle2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredAk1
    {
        protected RequiredAk1()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid ParentId { get; set; }

        public virtual Root Parent { get; set; }

        public virtual IEnumerable<RequiredAk2> Children { get; set; }
            = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance);

        public virtual IEnumerable<RequiredComposite2> CompositeChildren { get; set; }
            = new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object obj)
        {
            var other = obj as RequiredAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredAk1Derived : RequiredAk1
    {
        protected RequiredAk1Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredAk1MoreDerived : RequiredAk1Derived
    {
        protected RequiredAk1MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredAk2
    {
        protected RequiredAk2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid ParentId { get; set; }

        public virtual RequiredAk1 Parent { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredComposite1
    {
        protected RequiredComposite1()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid ParentAlternateId { get; set; }

        public virtual Root Parent { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredComposite1;
            return Id == other?.Id;
        }

        public virtual ICollection<OptionalOverlapping2> CompositeChildren { get; set; }
            = new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance);

        public override int GetHashCode()
            => Id;
    }

    public class OptionalOverlapping2
    {
        protected OptionalOverlapping2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid ParentAlternateId { get; set; }

        public virtual int? ParentId { get; set; }

        public virtual RequiredComposite1 Parent { get; set; }

        public virtual Root Root { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalOverlapping2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredComposite2
    {
        protected RequiredComposite2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid ParentAlternateId { get; set; }

        public virtual int ParentId { get; set; }

        public virtual RequiredAk1 Parent { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredAk2Derived : RequiredAk2
    {
        protected RequiredAk2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredAk2MoreDerived : RequiredAk2Derived
    {
        protected RequiredAk2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalAk1
    {
        protected OptionalAk1()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid? ParentId { get; set; }

        public virtual Root Parent { get; set; }

        public virtual IEnumerable<OptionalAk2> Children { get; set; }
            = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance);

        public virtual ICollection<OptionalComposite2> CompositeChildren { get; set; }
            = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

        public override bool Equals(object obj)
        {
            var other = obj as OptionalAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalAk1Derived : OptionalAk1
    {
        protected OptionalAk1Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalAk1MoreDerived : OptionalAk1Derived
    {
        protected OptionalAk1MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalAk2
    {
        protected OptionalAk2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid? ParentId { get; set; }

        public virtual OptionalAk1 Parent { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalComposite2
    {
        protected OptionalComposite2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid ParentAlternateId { get; set; }

        public virtual int? ParentId { get; set; }

        public virtual OptionalAk1 Parent { get; set; }

        public virtual int? Parent2Id { get; set; }

        public virtual Optional1 Parent2 { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalAk2Derived : OptionalAk2
    {
        protected OptionalAk2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalAk2MoreDerived : OptionalAk2Derived
    {
        protected OptionalAk2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredSingleAk1
    {
        protected RequiredSingleAk1()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid RootId { get; set; }

        public virtual Root Root { get; set; }

        public virtual RequiredSingleAk2 Single { get; set; }

        public virtual RequiredSingleComposite2 SingleComposite { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingleAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredSingleAk2
    {
        protected RequiredSingleAk2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid BackId { get; set; }

        public virtual RequiredSingleAk1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingleAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredSingleComposite2
    {
        protected RequiredSingleComposite2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid BackAlternateId { get; set; }

        public virtual int BackId { get; set; }

        public virtual RequiredSingleAk1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredSingleComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredNonPkSingleAk1
    {
        protected RequiredNonPkSingleAk1()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid RootId { get; set; }

        public virtual Root Root { get; set; }

        public virtual RequiredNonPkSingleAk2 Single { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingleAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredNonPkSingleAk1Derived : RequiredNonPkSingleAk1
    {
        protected RequiredNonPkSingleAk1Derived()
        {
        }

        public virtual Guid DerivedRootId { get; set; }

        public virtual Root DerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredNonPkSingleAk1MoreDerived : RequiredNonPkSingleAk1Derived
    {
        protected RequiredNonPkSingleAk1MoreDerived()
        {
        }

        public virtual Guid MoreDerivedRootId { get; set; }

        public virtual Root MoreDerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredNonPkSingleAk2
    {
        protected RequiredNonPkSingleAk2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid BackId { get; set; }

        public virtual RequiredNonPkSingleAk1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RequiredNonPkSingleAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class RequiredNonPkSingleAk2Derived : RequiredNonPkSingleAk2
    {
        protected RequiredNonPkSingleAk2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class RequiredNonPkSingleAk2MoreDerived : RequiredNonPkSingleAk2Derived
    {
        protected RequiredNonPkSingleAk2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as RequiredNonPkSingleAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingleAk1
    {
        protected OptionalSingleAk1()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid? RootId { get; set; }

        public virtual Root Root { get; set; }

        public virtual OptionalSingleComposite2 SingleComposite { get; set; }

        public virtual OptionalSingleAk2 Single { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingleAk1;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalSingleAk1Derived : OptionalSingleAk1
    {
        protected OptionalSingleAk1Derived()
        {
        }

        public virtual Guid? DerivedRootId { get; set; }

        public virtual Root DerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk1Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingleAk1MoreDerived : OptionalSingleAk1Derived
    {
        protected OptionalSingleAk1MoreDerived()
        {
        }

        public virtual Guid? MoreDerivedRootId { get; set; }

        public virtual Root MoreDerivedRoot { get; set; }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk1MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingleAk2
    {
        protected OptionalSingleAk2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid AlternateId { get; set; }

        public virtual Guid? BackId { get; set; }

        public virtual OptionalSingleAk1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingleAk2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalSingleComposite2
    {
        protected OptionalSingleComposite2()
        {
        }

        public virtual int Id { get; set; }

        public virtual Guid ParentAlternateId { get; set; }

        public virtual int? BackId { get; set; }

        public virtual OptionalSingleAk1 Back { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as OptionalSingleComposite2;
            return Id == other?.Id;
        }

        public override int GetHashCode()
            => Id;
    }

    public class OptionalSingleAk2Derived : OptionalSingleAk2
    {
        protected OptionalSingleAk2Derived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk2Derived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class OptionalSingleAk2MoreDerived : OptionalSingleAk2Derived
    {
        protected OptionalSingleAk2MoreDerived()
        {
        }

        public override bool Equals(object obj)
            => base.Equals(obj as OptionalSingleAk2MoreDerived);

        public override int GetHashCode()
            => base.GetHashCode();
    }

    public class BadCustomer
    {
        protected BadCustomer()
        {
        }

        public virtual int Id { get; set; }

        public virtual int Status { get; set; }

        public virtual ICollection<BadOrder> BadOrders { get; set; }
            = new ObservableHashSet<BadOrder>(ReferenceEqualityComparer.Instance);
    }

    public class BadOrder
    {
        protected BadOrder()
        {
        }

        public virtual int Id { get; set; }

        public virtual int? BadCustomerId { get; set; }

        public virtual BadCustomer BadCustomer { get; set; }
    }

    public class SharedFkRoot
    {
        public virtual long Id { get; set; }
        public virtual ICollection<SharedFkDependant> Dependants { get; set; }
        public virtual ICollection<SharedFkParent> Parents { get; set; }
    }

    public class SharedFkParent
    {
        public virtual long Id { get; set; }
        public virtual long? DependantId { get; set; }
        public virtual long RootId { get; set; }
        public virtual SharedFkRoot Root { get; set; }
        public virtual SharedFkDependant Dependant { get; set; }
    }

    public class SharedFkDependant
    {
        public virtual long Id { get; set; }
        public virtual long RootId { get; set; }
        public virtual SharedFkRoot Root { get; set; }
        public virtual SharedFkParent Parent { get; set; }
    }

    public class Car
    {
        public virtual Guid Id { get; set; }
        public virtual Person Owner { get; set; }
    }

    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual Car Vehicle { get; set; }
    }

    public record RecordBase
    {
        public virtual int Id { get; set; }
    }

    public record RecordCar : RecordBase
    {
        public virtual RecordPerson Owner { get; set; }
        public virtual int? OwnerId { get; set; }
    }

    public record RecordPerson : RecordBase
    {
        public virtual ICollection<RecordCar> Vehicles { get; }
            = new ObservableHashSet<RecordCar>(ReferenceEqualityComparer.Instance);
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
