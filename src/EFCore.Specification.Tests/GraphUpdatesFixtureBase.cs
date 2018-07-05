// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Microsoft.EntityFrameworkCore
{
    public abstract partial class GraphUpdatesTestBase<TFixture>
    {
        public abstract class GraphUpdatesFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "GraphUpdatesChangedTest";
            public readonly Guid RootAK = Guid.NewGuid();
            public virtual bool ForceRestrict => false;
            protected override bool UsePooling => false;

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
                                .HasPrincipalKey(e => new { e.Id, e.AlternateId })
                                .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
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
                                .HasPrincipalKey(e => new { e.Id, e.AlternateId })
                                .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
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
                                .HasForeignKey<RequiredSingleComposite2>(e => new { e.BackId, e.BackAlternateId })
                                .HasPrincipalKey<RequiredSingleAk1>(e => new { e.Id, e.AlternateId });
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
                                .HasForeignKey<OptionalSingleComposite2>(e => new { e.BackId, e.ParentAlternateId })
                                .HasPrincipalKey<OptionalSingleAk1>(e => new { e.Id, e.AlternateId });
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
                            eb.HasKey(e => new { e.Id, e.ParentAlternateId });

                            eb.HasMany(e => e.CompositeChildren)
                                .WithOne(e => e.Parent)
                                .HasPrincipalKey(e => new { e.Id, e.ParentAlternateId })
                                .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
                        });

                modelBuilder.Entity<OptionalOverlaping2>(
                    eb =>
                        {
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
            }

            protected virtual object CreateFullGraph()
                => new Root
                {
                    AlternateId = RootAK,
                    RequiredChildren = new ObservableHashSet<Required1>(ReferenceEqualityComparer.Instance)
                    {
                        new Required1
                        {
                            Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance)
                            {
                                new Required2(),
                                new Required2()
                            }
                        },
                        new Required1
                        {
                            Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance)
                            {
                                new Required2(),
                                new Required2()
                            }
                        }
                    },
                    OptionalChildren = new ObservableHashSet<Optional1>(ReferenceEqualityComparer.Instance)
                    {
                        new Optional1
                        {
                            Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance)
                            {
                                new Optional2(),
                                new Optional2()
                            },
                            CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                        },
                        new Optional1
                        {
                            Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance)
                            {
                                new Optional2(),
                                new Optional2()
                            },
                            CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                        }
                    },
                    RequiredSingle = new RequiredSingle1
                    {
                        Single = new RequiredSingle2()
                    },
                    OptionalSingle = new OptionalSingle1
                    {
                        Single = new OptionalSingle2()
                    },
                    OptionalSingleDerived = new OptionalSingle1Derived
                    {
                        Single = new OptionalSingle2Derived()
                    },
                    OptionalSingleMoreDerived = new OptionalSingle1MoreDerived
                    {
                        Single = new OptionalSingle2MoreDerived()
                    },
                    RequiredNonPkSingle = new RequiredNonPkSingle1
                    {
                        Single = new RequiredNonPkSingle2()
                    },
                    RequiredNonPkSingleDerived = new RequiredNonPkSingle1Derived
                    {
                        Single = new RequiredNonPkSingle2Derived(),
                        Root = new Root()
                    },
                    RequiredNonPkSingleMoreDerived = new RequiredNonPkSingle1MoreDerived
                    {
                        Single = new RequiredNonPkSingle2MoreDerived(),
                        Root = new Root(),
                        DerivedRoot = new Root()
                    },
                    RequiredChildrenAk = new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance)
                    {
                        new RequiredAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredAk2 { AlternateId = Guid.NewGuid() },
                                new RequiredAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredComposite2(),
                                new RequiredComposite2()
                            }
                        },
                        new RequiredAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredAk2 { AlternateId = Guid.NewGuid() },
                                new RequiredAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredComposite2(),
                                new RequiredComposite2()
                            }
                        }
                    },
                    OptionalChildrenAk = new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance)
                    {
                        new OptionalAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalAk2 { AlternateId = Guid.NewGuid() },
                                new OptionalAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalComposite2(),
                                new OptionalComposite2()
                            }
                        },
                        new OptionalAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalAk2 { AlternateId = Guid.NewGuid() },
                                new OptionalAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalComposite2(),
                                new OptionalComposite2()
                            }
                        }
                    },
                    RequiredSingleAk = new RequiredSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredSingleAk2 { AlternateId = Guid.NewGuid() },
                        SingleComposite = new RequiredSingleComposite2()
                    },
                    OptionalSingleAk = new OptionalSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2 { AlternateId = Guid.NewGuid() },
                        SingleComposite = new OptionalSingleComposite2()
                    },
                    OptionalSingleAkDerived = new OptionalSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2Derived { AlternateId = Guid.NewGuid() }
                    },
                    OptionalSingleAkMoreDerived = new OptionalSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2MoreDerived { AlternateId = Guid.NewGuid() }
                    },
                    RequiredNonPkSingleAk = new RequiredNonPkSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2 { AlternateId = Guid.NewGuid() }
                    },
                    RequiredNonPkSingleAkDerived = new RequiredNonPkSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2Derived { AlternateId = Guid.NewGuid() },
                        Root = new Root()
                    },
                    RequiredNonPkSingleAkMoreDerived = new RequiredNonPkSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2MoreDerived { AlternateId = Guid.NewGuid() },
                        Root = new Root(),
                        DerivedRoot = new Root()
                    },
                    RequiredCompositeChildren = new ObservableHashSet<RequiredComposite1>(ReferenceEqualityComparer.Instance)
                    {
                        new RequiredComposite1
                        {
                            Id = 1,
                            CompositeChildren = new ObservableHashSet<OptionalOverlaping2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalOverlaping2 { Id = 1 },
                                new OptionalOverlaping2 { Id = 2 }
                            }
                        },
                        new RequiredComposite1
                        {
                            Id = 2,
                            CompositeChildren = new ObservableHashSet<OptionalOverlaping2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalOverlaping2 { Id = 3 },
                                new OptionalOverlaping2 { Id = 4 }
                            }
                        }
                    }
                };

            protected override void Seed(DbContext context)
            {
                var tracker = new KeyValueEntityTracker();

                context.ChangeTracker.TrackGraph(CreateFullGraph(), e => tracker.TrackEntity(e.Entry));

                context.Add(new BadOrder { BadCustomer = new BadCustomer() });

                context.SaveChanges();
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

        private static void Add<T>(IEnumerable<T> collection, T item) => ((ICollection<T>)collection).Add(item);

        private static void Remove<T>(IEnumerable<T> collection, T item) => ((ICollection<T>)collection).Remove(item);

        [Flags]
        public enum ChangeMechanism
        {
            Dependent = 1,
            Principal = 2,
            Fk = 4
        }

        protected Expression<Func<Root, bool>> IsTheRoot => r => r.AlternateId == Fixture.RootAK;

        protected Root LoadRequiredGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.RequiredChildren).ThenInclude(e => e.Children)
                .Include(e => e.RequiredSingle).ThenInclude(e => e.Single)
                .Single(IsTheRoot);

        protected Root LoadOptionalGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.OptionalChildren).ThenInclude(e => e.Children)
                .Include(e => e.OptionalChildren).ThenInclude(e => e.CompositeChildren)
                .Include(e => e.OptionalSingle).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleDerived).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleMoreDerived).ThenInclude(e => e.Single)
                .Single(IsTheRoot);

        protected Root LoadRequiredNonPkGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.RequiredNonPkSingle).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.DerivedRoot)
                .Single(IsTheRoot);

        protected Root LoadRequiredAkGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.Children)
                .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.CompositeChildren)
                .Include(e => e.RequiredSingleAk).ThenInclude(e => e.Single)
                .Include(e => e.RequiredSingleAk).ThenInclude(e => e.SingleComposite)
                .Single(IsTheRoot);

        protected Root LoadOptionalAkGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
                .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
                .Include(e => e.OptionalSingleAk).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleAk).ThenInclude(e => e.SingleComposite)
                .Include(e => e.OptionalSingleAkDerived).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleAkMoreDerived).ThenInclude(e => e.Single)
                .Single(IsTheRoot);

        protected Root LoadRequiredNonPkAkGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.RequiredNonPkSingleAk).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleAkDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleAkDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.DerivedRoot)
                .Single(IsTheRoot);

        protected Root LoadOptionalOneToManyGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.OptionalChildren).ThenInclude(e => e.Children)
                .Include(e => e.OptionalChildren).ThenInclude(e => e.CompositeChildren)
                .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
                .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
                .Single(IsTheRoot);

        protected Root LoadRequiredCompositeGraph(DbContext context)
            => context.Set<Root>()
                .Include(e => e.RequiredCompositeChildren).ThenInclude(e => e.CompositeChildren)
                .Single(IsTheRoot);

        private static void AssertEntries(IReadOnlyList<EntityEntry> expectedEntries, IReadOnlyList<EntityEntry> actualEntries)
        {
            var newEntities = new HashSet<object>(actualEntries.Select(ne => ne.Entity));
            var missingEntities = expectedEntries.Select(e => e.Entity).Where(e => !newEntities.Contains(e)).ToList();
            Assert.Equal(Array.Empty<object>(), missingEntities);
            Assert.Equal(expectedEntries.Count, actualEntries.Count);
        }

        private static void AssertKeys(Root expected, Root actual)
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
            Assert.Equal(expected.OptionalSingleAkMoreDerived?.Single?.AlternateId, actual.OptionalSingleAkMoreDerived?.Single?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAk?.Single?.AlternateId, actual.RequiredNonPkSingleAk?.Single?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAkDerived?.Single?.AlternateId, actual.RequiredNonPkSingleAkDerived?.Single?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAkMoreDerived?.Single?.AlternateId, actual.RequiredNonPkSingleAkMoreDerived?.Single?.AlternateId);

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

        private static void AssertNavigations(Root root)
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

        private static void AssertPossiblyNullNavigations(Root root)
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
            private IEnumerable<RequiredAk1> _requiredChildrenAk = new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance);
            private IEnumerable<OptionalAk1> _optionalChildrenAk = new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance);
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

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
        }

        protected class Required1Derived : Required1
        {
            public override bool Equals(object obj) => base.Equals(obj as Required1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Required1MoreDerived : Required1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Required1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
        }

        protected class Required2Derived : Required2
        {
            public override bool Equals(object obj) => base.Equals(obj as Required2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Required2MoreDerived : Required2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Required2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Optional1 : NotifyingEntity
        {
            private int _id;
            private int? _parentId;
            private Root _parent;
            private IEnumerable<Optional2> _children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance);
            private ICollection<OptionalComposite2> _compositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

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

            public override int GetHashCode() => _id;
        }

        protected class Optional1Derived : Optional1
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Optional1MoreDerived : Optional1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
        }

        protected class Optional2Derived : Optional2
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Optional2MoreDerived : Optional2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredSingle1 : NotifyingEntity
        {
            private int _id;
            private Root _root;
            private RequiredSingle2 _single;

            public int Id
            {
                get => _id;
                set => SetWithNotify(value, ref _id);
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

            public override int GetHashCode() => _id;
        }

        protected class RequiredSingle2 : NotifyingEntity
        {
            private int _id;
            private RequiredSingle1 _back;

            public int Id
            {
                get => _id;
                set => SetWithNotify(value, ref _id);
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

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
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

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle1Derived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingle2Derived : RequiredNonPkSingle2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingle2MoreDerived : RequiredNonPkSingle2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
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

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle1Derived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingle2 : NotifyingEntity
        {
            private int _id;
            private int? _backId;
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

            public override int GetHashCode() => _id;
        }

        protected class OptionalSingle2Derived : OptionalSingle2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingle2MoreDerived : OptionalSingle2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredAk1 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid _parentId;
            private Root _parent;
            private IEnumerable<RequiredAk2> _children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance);
            private IEnumerable<RequiredComposite2> _compositeChildren = new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance);

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

            public override int GetHashCode() => _id;
        }

        protected class RequiredAk1Derived : RequiredAk1
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredAk1MoreDerived : RequiredAk1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
        }

        protected class RequiredComposite1 : NotifyingEntity
        {
            private int _id;
            private Guid _parentAlternateId;
            private Root _parent;
            private ICollection<OptionalOverlaping2> _compositeChildren = new ObservableHashSet<OptionalOverlaping2>(ReferenceEqualityComparer.Instance);

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

            public ICollection<OptionalOverlaping2> CompositeChildren
            {
                get => _compositeChildren;
                set => SetWithNotify(value, ref _compositeChildren);
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalOverlaping2 : NotifyingEntity
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
                var other = obj as OptionalOverlaping2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
        }

        protected class RequiredAk2Derived : RequiredAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredAk2MoreDerived : RequiredAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalAk1 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid? _parentId;
            private Root _parent;
            private IEnumerable<OptionalAk2> _children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance);
            private ICollection<OptionalComposite2> _compositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance);

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

            public override int GetHashCode() => _id;
        }

        protected class OptionalAk1Derived : OptionalAk1
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalAk1MoreDerived : OptionalAk1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
        }

        protected class OptionalAk2Derived : OptionalAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalAk2MoreDerived : OptionalAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
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

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingleAk2Derived : RequiredNonPkSingleAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingleAk2MoreDerived : RequiredNonPkSingleAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
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

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

            public override int GetHashCode() => _id;
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

            public override int GetHashCode() => _id;
        }

        protected class OptionalSingleAk2Derived : OptionalSingleAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingleAk2MoreDerived : OptionalSingleAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
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

        protected class HiddenAreaTask : TaskWithChoices
        {
        }

        protected abstract class QuestTask : NotifyingEntity
        {
            private int _id;

            public int Id
            {
                get => _id;
                set => SetWithNotify(value, ref _id);
            }
        }

        protected class QuizTask : TaskWithChoices
        {
        }

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

        protected abstract class TaskWithChoices : QuestTask
        {
            private ICollection<TaskChoice> _choices = new ObservableHashSet<TaskChoice>(ReferenceEqualityComparer.Instance);

            public ICollection<TaskChoice> Choices
            {
                get => _choices;
                set => SetWithNotify(value, ref _choices);
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

        protected DbContext CreateContext() => Fixture.CreateContext();

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<DbContext> testOperation,
            Action<DbContext> nestedTestOperation1 = null,
            Action<DbContext> nestedTestOperation2 = null,
            Action<DbContext> nestedTestOperation3 = null)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }
    }
}
