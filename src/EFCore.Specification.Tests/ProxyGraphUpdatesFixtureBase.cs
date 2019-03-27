// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract partial class ProxyGraphUpdatesTestBase<TFixture>
    {
        public abstract class ProxyGraphUpdatesFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "ProxyGraphUpdatesTest";

            public readonly Guid RootAK = Guid.NewGuid();

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
                            .HasForeignKey(
                                e => new
                                {
                                    e.Parent2Id
                                });
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
                                e => new
                                {
                                    e.Id,
                                    e.AlternateId
                                })
                            .HasForeignKey(
                                e => new
                                {
                                    e.ParentId,
                                    e.ParentAlternateId
                                });
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
                                e => new
                                {
                                    e.Id,
                                    e.AlternateId
                                })
                            .HasForeignKey(
                                e => new
                                {
                                    e.ParentId,
                                    e.ParentAlternateId
                                });
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
                                e => new
                                {
                                    e.BackId,
                                    e.BackAlternateId
                                })
                            .HasPrincipalKey<RequiredSingleAk1>(
                                e => new
                                {
                                    e.Id,
                                    e.AlternateId
                                });
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
                                e => new
                                {
                                    e.BackId,
                                    e.ParentAlternateId
                                })
                            .HasPrincipalKey<OptionalSingleAk1>(
                                e => new
                                {
                                    e.Id,
                                    e.AlternateId
                                });
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
                        eb.HasKey(
                            e => new
                            {
                                e.Id,
                                e.ParentAlternateId
                            });

                        eb.HasMany(e => e.CompositeChildren)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey(
                                e => new
                                {
                                    e.Id,
                                    e.ParentAlternateId
                                })
                            .HasForeignKey(
                                e => new
                                {
                                    e.ParentId,
                                    e.ParentAlternateId
                                });
                    });

                modelBuilder.Entity<OptionalOverlaping2>(
                    eb =>
                    {
                        eb.HasKey(
                            e => new
                            {
                                e.Id,
                                e.ParentAlternateId
                            });

                        eb.HasOne(e => e.Root)
                            .WithMany()
                            .HasPrincipalKey(e => e.AlternateId)
                            .HasForeignKey(e => e.ParentAlternateId);
                    });

                modelBuilder.Entity<BadCustomer>();
                modelBuilder.Entity<BadOrder>();
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
                                new RequiredAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                },
                                new RequiredAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                }
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
                                new RequiredAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                },
                                new RequiredAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                }
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
                                new OptionalAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                },
                                new OptionalAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                }
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
                                new OptionalAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                },
                                new OptionalAk2
                                {
                                    AlternateId = Guid.NewGuid()
                                }
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
                        Single = new RequiredSingleAk2
                        {
                            AlternateId = Guid.NewGuid()
                        },
                        SingleComposite = new RequiredSingleComposite2()
                    },
                    OptionalSingleAk = new OptionalSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2
                        {
                            AlternateId = Guid.NewGuid()
                        },
                        SingleComposite = new OptionalSingleComposite2()
                    },
                    OptionalSingleAkDerived = new OptionalSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2Derived
                        {
                            AlternateId = Guid.NewGuid()
                        }
                    },
                    OptionalSingleAkMoreDerived = new OptionalSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2MoreDerived
                        {
                            AlternateId = Guid.NewGuid()
                        }
                    },
                    RequiredNonPkSingleAk = new RequiredNonPkSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2
                        {
                            AlternateId = Guid.NewGuid()
                        }
                    },
                    RequiredNonPkSingleAkDerived = new RequiredNonPkSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2Derived
                        {
                            AlternateId = Guid.NewGuid()
                        },
                        Root = new Root()
                    },
                    RequiredNonPkSingleAkMoreDerived = new RequiredNonPkSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2MoreDerived
                        {
                            AlternateId = Guid.NewGuid()
                        },
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
                                new OptionalOverlaping2
                                {
                                    Id = 1
                                },
                                new OptionalOverlaping2
                                {
                                    Id = 2
                                }
                            }
                        },
                        new RequiredComposite1
                        {
                            Id = 2,
                            CompositeChildren = new ObservableHashSet<OptionalOverlaping2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalOverlaping2
                                {
                                    Id = 3
                                },
                                new OptionalOverlaping2
                                {
                                    Id = 4
                                }
                            }
                        }
                    }
                };

            protected override void Seed(DbContext context)
            {
                var tracker = new KeyValueEntityTracker();

                context.ChangeTracker.TrackGraph(CreateFullGraph(), e => tracker.TrackEntity(e.Entry));

                context.Add(
                    new BadOrder
                    {
                        BadCustomer = new BadCustomer()
                    });

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

        protected Root LoadRoot(DbContext context)
            => context.Set<Root>().Single(IsTheRoot);

        public class Root
        {
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

            public override int GetHashCode() => Id;
        }

        public class Required1
        {
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

            public override int GetHashCode() => Id;
        }

        public class Required1Derived : Required1
        {
            public override bool Equals(object obj) => base.Equals(obj as Required1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class Required1MoreDerived : Required1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Required1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class Required2
        {
            public virtual int Id { get; set; }

            public virtual int ParentId { get; set; }

            public virtual Required1 Parent { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as Required2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class Required2Derived : Required2
        {
            public override bool Equals(object obj) => base.Equals(obj as Required2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class Required2MoreDerived : Required2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Required2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class Optional1
        {
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

            public override int GetHashCode() => Id;
        }

        public class Optional1Derived : Optional1
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class Optional1MoreDerived : Optional1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class Optional2
        {
            public virtual int Id { get; set; }

            public virtual int? ParentId { get; set; }

            public virtual Optional1 Parent { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as Optional2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class Optional2Derived : Optional2
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class Optional2MoreDerived : Optional2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredSingle1
        {
            public virtual int Id { get; set; }

            public virtual Root Root { get; set; }

            public virtual RequiredSingle2 Single { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingle1;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredSingle2
        {
            public virtual int Id { get; set; }

            public virtual RequiredSingle1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingle2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredNonPkSingle1
        {
            public virtual int Id { get; set; }

            public virtual int RootId { get; set; }

            public virtual Root Root { get; set; }

            public virtual RequiredNonPkSingle2 Single { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredNonPkSingle1;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredNonPkSingle1Derived : RequiredNonPkSingle1
        {
            public virtual int DerivedRootId { get; set; }

            public virtual Root DerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredNonPkSingle1MoreDerived : RequiredNonPkSingle1Derived
        {
            public virtual int MoreDerivedRootId { get; set; }

            public virtual Root MoreDerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredNonPkSingle2
        {
            public virtual int Id { get; set; }

            public virtual int BackId { get; set; }

            public virtual RequiredNonPkSingle1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredNonPkSingle2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredNonPkSingle2Derived : RequiredNonPkSingle2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredNonPkSingle2MoreDerived : RequiredNonPkSingle2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingle1
        {
            public virtual int Id { get; set; }

            public virtual int? RootId { get; set; }

            public virtual Root Root { get; set; }

            public virtual OptionalSingle2 Single { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingle1;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class OptionalSingle1Derived : OptionalSingle1
        {
            public virtual int? DerivedRootId { get; set; }

            public virtual Root DerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingle1MoreDerived : OptionalSingle1Derived
        {
            public virtual int? MoreDerivedRootId { get; set; }

            public virtual Root MoreDerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingle2
        {
            public virtual int Id { get; set; }

            public virtual int? BackId { get; set; }

            public virtual OptionalSingle1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingle2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class OptionalSingle2Derived : OptionalSingle2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingle2MoreDerived : OptionalSingle2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredAk1
        {
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

            public override int GetHashCode() => Id;
        }

        public class RequiredAk1Derived : RequiredAk1
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredAk1MoreDerived : RequiredAk1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredAk2
        {
            public virtual int Id { get; set; }

            public virtual Guid AlternateId { get; set; }

            public virtual Guid ParentId { get; set; }

            public virtual RequiredAk1 Parent { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredAk2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredComposite1
        {
            public virtual int Id { get; set; }

            public virtual Guid ParentAlternateId { get; set; }

            public virtual Root Parent { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredComposite1;
                return Id == other?.Id;
            }

            public virtual ICollection<OptionalOverlaping2> CompositeChildren { get; set; }
                = new ObservableHashSet<OptionalOverlaping2>(ReferenceEqualityComparer.Instance);

            public override int GetHashCode() => Id;
        }

        public class OptionalOverlaping2
        {
            public virtual int Id { get; set; }

            public virtual Guid ParentAlternateId { get; set; }

            public virtual int? ParentId { get; set; }

            public virtual RequiredComposite1 Parent { get; set; }

            public virtual Root Root { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalOverlaping2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredComposite2
        {
            public virtual int Id { get; set; }

            public virtual Guid ParentAlternateId { get; set; }

            public virtual int ParentId { get; set; }

            public virtual RequiredAk1 Parent { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredComposite2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredAk2Derived : RequiredAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredAk2MoreDerived : RequiredAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalAk1
        {
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

            public override int GetHashCode() => Id;
        }

        public class OptionalAk1Derived : OptionalAk1
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalAk1MoreDerived : OptionalAk1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalAk2
        {
            public virtual int Id { get; set; }

            public virtual Guid AlternateId { get; set; }

            public virtual Guid? ParentId { get; set; }

            public virtual OptionalAk1 Parent { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalAk2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class OptionalComposite2
        {
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

            public override int GetHashCode() => Id;
        }

        public class OptionalAk2Derived : OptionalAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalAk2MoreDerived : OptionalAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredSingleAk1
        {
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

            public override int GetHashCode() => Id;
        }

        public class RequiredSingleAk2
        {
            public virtual int Id { get; set; }

            public virtual Guid AlternateId { get; set; }

            public virtual Guid BackId { get; set; }

            public virtual RequiredSingleAk1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingleAk2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredSingleComposite2
        {
            public virtual int Id { get; set; }

            public virtual Guid BackAlternateId { get; set; }

            public virtual int BackId { get; set; }

            public virtual RequiredSingleAk1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingleComposite2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredNonPkSingleAk1
        {
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

            public override int GetHashCode() => Id;
        }

        public class RequiredNonPkSingleAk1Derived : RequiredNonPkSingleAk1
        {
            public virtual Guid DerivedRootId { get; set; }

            public virtual Root DerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredNonPkSingleAk1MoreDerived : RequiredNonPkSingleAk1Derived
        {
            public virtual Guid MoreDerivedRootId { get; set; }

            public virtual Root MoreDerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredNonPkSingleAk2
        {
            public virtual int Id { get; set; }

            public virtual Guid AlternateId { get; set; }

            public virtual Guid BackId { get; set; }

            public virtual RequiredNonPkSingleAk1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredNonPkSingleAk2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class RequiredNonPkSingleAk2Derived : RequiredNonPkSingleAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class RequiredNonPkSingleAk2MoreDerived : RequiredNonPkSingleAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingleAk1
        {
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

            public override int GetHashCode() => Id;
        }

        public class OptionalSingleAk1Derived : OptionalSingleAk1
        {
            public virtual Guid? DerivedRootId { get; set; }

            public virtual Root DerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingleAk1MoreDerived : OptionalSingleAk1Derived
        {
            public virtual Guid? MoreDerivedRootId { get; set; }

            public virtual Root MoreDerivedRoot { get; set; }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingleAk2
        {
            public virtual int Id { get; set; }

            public virtual Guid AlternateId { get; set; }

            public virtual Guid? BackId { get; set; }

            public virtual OptionalSingleAk1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingleAk2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class OptionalSingleComposite2
        {
            public virtual int Id { get; set; }

            public virtual Guid ParentAlternateId { get; set; }

            public virtual int? BackId { get; set; }

            public virtual OptionalSingleAk1 Back { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingleComposite2;
                return Id == other?.Id;
            }

            public override int GetHashCode() => Id;
        }

        public class OptionalSingleAk2Derived : OptionalSingleAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class OptionalSingleAk2MoreDerived : OptionalSingleAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        public class BadCustomer
        {
            public virtual int Id { get; set; }

            public virtual int Status { get; set; }

            public virtual ICollection<BadOrder> BadOrders { get; set; }
                = new ObservableHashSet<BadOrder>(ReferenceEqualityComparer.Instance);
        }

        public class BadOrder
        {
            public virtual int Id { get; set; }

            public virtual int? BadCustomerId { get; set; }

            public virtual BadCustomer BadCustomer { get; set; }
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
