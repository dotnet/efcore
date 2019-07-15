// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : OwnedQueryTestBase<TFixture>.OwnedQueryFixtureBase, new()
    {
        protected OwnedQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.ListLoggerFactory.Clear();
        }

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Query_with_owned_entity_equality_operator()
        {
            using (var context = CreateContext())
            {
                var query
                    = (from a in context.Set<LeafA>()
                       from b in context.Set<LeafB>()
                       where a.LeafAAddress == b.LeafBAddress
                       select a).ToList();

                Assert.Equal(0, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_owned_entity_equality_method()
        {
            using (var context = CreateContext())
            {
                var query
                    = (from a in context.Set<LeafA>()
                       from b in context.Set<LeafB>()
                       where a.LeafAAddress.Equals(b.LeafBAddress)
                       select a).ToList();

                Assert.Equal(0, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Query_with_owned_entity_equality_object_method()
        {
            using (var context = CreateContext())
            {
                var query
                    = (from a in context.Set<LeafA>()
                       from b in context.Set<LeafB>()
                       where Equals(a.LeafAAddress, b.LeafBAddress)
                       select a).ToList();

                Assert.Equal(0, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Query_for_base_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<OwnedPerson>().ToList();

                Assert.Equal(4, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.OfType<Branch>().All(b => b.BranchAddress != null));
                Assert.True(people.OfType<LeafA>().All(a => a.LeafAAddress != null));
                Assert.True(people.OfType<LeafB>().All(b => b.LeafBAddress != null));
                //Assert.True(people.All(p => p.Orders.Count == (p.Id == 1 ? 2 : 1)));
                //Assert.True(people.All(p => p.Orders.All(o => o.Client == p)));

                //var branch = people.OfType<Branch>().First();
                //var branchEntry = context.Entry(branch);
                //Assert.True(branchEntry.Collection(b => b.Orders).IsLoaded);
                //Assert.True(branchEntry.Reference(b => b.PersonAddress).IsLoaded);
                //Assert.True(branchEntry.Reference(b => b.BranchAddress).IsLoaded);

                //Assert.True(people.All(p => p.Orders.All(o => context.Entry(o).Reference(oe => oe.Client).IsLoaded)));
            }
        }

        [ConditionalFact(Skip = "Issue#16619")]
        public virtual void No_ignored_include_warning_when_implicit_load()
        {
            using (var context = CreateContext())
            {
                var count = context.Set<OwnedPerson>().Count();

                Assert.Equal(4, count);
            }
        }

        [ConditionalFact]
        public virtual void Query_for_branch_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<Branch>().ToList();

                Assert.Equal(2, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.All(b => b.BranchAddress != null));
                Assert.True(people.OfType<LeafA>().All(a => a.LeafAAddress != null));
                //Assert.True(people.All(p => p.Orders.Count == 1));
            }
        }

        [ConditionalFact]
        public virtual void Query_for_leaf_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<LeafA>().ToList();

                Assert.Equal(1, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.All(b => b.BranchAddress != null));
                Assert.True(people.All(a => a.LeafAAddress != null));
                //Assert.True(people.All(p => p.Orders.Count == 1));
            }
        }

        [ConditionalFact(Skip = "Issue #14935. Cannot eval 'GroupBy([op].Id, _Include(queryContext, [op], new [] {[op.PersonAddress], [op.PersonAddress.Country], [op.BranchAddress], [op.BranchAddress.Country], [op.LeafAAddress], [op.LeafAAddress.Country], [op.LeafBAddress], [op.LeafBAddress.Country]}, (queryContext, entity, included) => { ... }))'")]
        public virtual void Query_when_group_by()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<OwnedPerson>().GroupBy(op => op.Id).ToList();

                Assert.Equal(4, people.Count);
                Assert.True(people.SelectMany(p => p).All(p => p.PersonAddress != null));
                Assert.True(people.SelectMany(p => p).OfType<Branch>().All(b => b.BranchAddress != null));
                Assert.True(people.SelectMany(p => p).OfType<LeafA>().All(a => a.LeafAAddress != null));
                Assert.True(people.SelectMany(p => p).OfType<LeafB>().All(b => b.LeafBAddress != null));
            }
        }

        [ConditionalFact]
        public virtual void Query_when_subquery()
        {
            using (var context = CreateContext())
            {
                var people
                    = context.Set<OwnedPerson>()
                        .Distinct()
                        .OrderBy(p => p.Id)
                        .Take(5)
                        .Select(
                            op => new
                            {
                                op
                            })
                        .ToList();

                Assert.Equal(4, people.Count);
                Assert.True(people.All(p => p.op.PersonAddress != null));
                Assert.True(people.Select(p => p.op).OfType<Branch>().All(b => b.BranchAddress != null));
                Assert.True(people.Select(p => p.op).OfType<LeafA>().All(a => a.LeafAAddress != null));
                Assert.True(people.Select(p => p.op).OfType<LeafB>().All(b => b.LeafBAddress != null));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_projecting_scalar()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>()
                    .Where(p => p.PersonAddress.Country.Name == "USA")
                    .Select(p => p.PersonAddress.Country.Name);

                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r == "USA"));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_projecting_entity()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Name == "USA");
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void Navigation_rewrite_on_owned_collection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Where(p => p.Orders.Count > 0).Select(p => p.Orders);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r.Count > 0));
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void Navigation_rewrite_on_owned_collection_with_composition()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.Orders.Select(o => o.Id != 42).FirstOrDefault());
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void Navigation_rewrite_on_owned_collection_with_composition_complex()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.Orders.Select(o => o.Client.PersonAddress.Country.Name).FirstOrDefault());
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void SelectMany_on_owned_collection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().SelectMany(p => p.Orders);
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Set_throws_for_owned_type()
        {
            using (var ctx = CreateContext())
            {
                Assert.Equal(CoreStrings.InvalidSetTypeWeak(nameof(OwnedAddress)),
                    Assert.Throws<InvalidOperationException>(() => ctx.Set<OwnedAddress>().ToList()).Message);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Planet.Id != 42).Select(p => new { p.Orders });
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void Project_multiple_owned_navigations()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => new { p.Orders, p.PersonAddress, p.PersonAddress.Country.Planet });
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void Project_multiple_owned_navigations_with_expansion_on_owned_collections()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => new { Count = p.Orders.Where(o => o.Client.PersonAddress.Country.Planet.Star.Id != 42).Count(), p.PersonAddress.Country.Planet });
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#16622")]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Planet.Id != 7).Select(p => new { p });
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Id);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r == 1));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Moons);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r.Count > 0));
            }
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public virtual void SelectMany_on_owned_reference_followed_by_regular_entity_and_collection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().SelectMany(p => p.PersonAddress.Country.Planet.Moons);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r.Diameter == 3474));
            }
        }

        [ConditionalFact(Skip = "Issue#16620")]
        public virtual void SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().SelectMany(p => p.PersonAddress.Country.Planet.Star.Composition);
                var result = query.ToList();

                Assert.Equal(8, result.Count);
                Assert.True(result.All(r => r.Name.StartsWith("H")));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Moons.Count);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r == 1));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Star);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r.Name == "Sol"));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Star.Name);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r == "Sol"));
            }
        }

        [ConditionalFact]
        public virtual void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Planet.Star.Name == "Sol")
                    .Select(p => p.PersonAddress.Country.Planet.Star);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.True(result.All(r => r.Name == "Sol"));
            }
        }

        [ConditionalFact]
        public virtual void Query_with_OfType_eagerly_loads_correct_owned_navigations()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Set<OwnedPerson>().OfType<LeafA>();
                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.NotNull(result[0].BranchAddress);
                Assert.NotNull(result[0].LeafAAddress);
                Assert.NotNull(result[0].PersonAddress);
                //Issue#16620
                //Assert.Equal(1, result[0].Orders.Count);
            }
        }

        protected virtual DbContext CreateContext() => Fixture.CreateContext();

        public abstract class OwnedQueryFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "OwnedQueryTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<OwnedPerson>(
                    eb =>
                    {
                        eb.HasData(
                            new OwnedPerson
                            {
                                Id = 1
                            });

                        eb.OwnsOne(
                            p => p.PersonAddress, ab =>
                            {
                                ab.HasData(
                                    new
                                    {
                                        OwnedPersonId = 1
                                    }, new
                                    {
                                        OwnedPersonId = 2
                                    }, new
                                    {
                                        OwnedPersonId = 3
                                    }, new
                                    {
                                        OwnedPersonId = 4
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 1,
                                                PlanetId = 1,
                                                Name = "USA"
                                            }, new
                                            {
                                                OwnedAddressOwnedPersonId = 2,
                                                PlanetId = 1,
                                                Name = "USA"
                                            }, new
                                            {
                                                OwnedAddressOwnedPersonId = 3,
                                                PlanetId = 1,
                                                Name = "USA"
                                            }, new
                                            {
                                                OwnedAddressOwnedPersonId = 4,
                                                PlanetId = 1,
                                                Name = "USA"
                                            });

                                        cb.HasOne(cc => cc.Planet).WithMany().HasForeignKey(ee => ee.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);
                                    });
                            });

                        // TODO: Owned collections are expanded in nav expansion
                        eb.Ignore(p => p.Orders);
                        //eb.OwnsMany(
                        //    p => p.Orders, ob =>
                        //    {
                        //        ob.HasData(
                        //            new
                        //            {
                        //                Id = -10,
                        //                ClientId = 1
                        //            },
                        //            new
                        //            {
                        //                Id = -11,
                        //                ClientId = 1
                        //            },
                        //            new
                        //            {
                        //                Id = -20,
                        //                ClientId = 2
                        //            },
                        //            new
                        //            {
                        //                Id = -30,
                        //                ClientId = 3
                        //            },
                        //            new
                        //            {
                        //                Id = -40,
                        //                ClientId = 4
                        //            }
                        //        );
                        //    });
                    });

                modelBuilder.Entity<Branch>(
                    eb =>
                    {
                        eb.HasData(
                            new Branch
                            {
                                Id = 2
                            });

                        eb.OwnsOne(
                            p => p.BranchAddress, ab =>
                            {
                                ab.HasData(
                                    new
                                    {
                                        BranchId = 2
                                    }, new
                                    {
                                        BranchId = 3
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressBranchId = 2,
                                                PlanetId = 1,
                                                Name = "Canada"
                                            }, new
                                            {
                                                OwnedAddressBranchId = 3,
                                                PlanetId = 1,
                                                Name = "Canada"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<LeafA>(
                    eb =>
                    {
                        eb.HasData(
                            new LeafA
                            {
                                Id = 3
                            });

                        eb.OwnsOne(
                            p => p.LeafAAddress, ab =>
                            {
                                ab.HasData(
                                    new
                                    {
                                        LeafAId = 3
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);

                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressLeafAId = 3,
                                                PlanetId = 1,
                                                Name = "Mexico"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<LeafB>(
                    eb =>
                    {
                        eb.HasData(
                            new LeafB
                            {
                                Id = 4
                            });

                        eb.OwnsOne(
                            p => p.LeafBAddress, ab =>
                            {
                                ab.HasData(
                                    new
                                    {
                                        LeafBId = 4
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);

                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressLeafBId = 4,
                                                PlanetId = 1,
                                                Name = "Panama"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<Planet>(pb => pb.HasData(new Planet { Id = 1, StarId = 1 }));

                modelBuilder.Entity<Moon>(mb => mb.HasData(new Moon { Id = 1, PlanetId = 1, Diameter = 3474 }));

                modelBuilder.Entity<Star>(
                    sb =>
                    {
                        sb.HasData(new Star { Id = 1, Name = "Sol" });
                        // TODO: Owned collections are expanded in nav expansion
                        sb.Ignore(p => p.Composition);
                        //sb.OwnsMany(
                        //    s => s.Composition, ob =>
                        //    {
                        //        ob.HasKey(e => e.Id);
                        //        ob.HasData(
                        //            new { Id = "H", Name = "Hydrogen", StarId = 1 },
                        //            new { Id = "He", Name = "Helium", StarId = 1 });
                        //    });
                    });
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Throw());

            public override PoolableDbContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context;
            }
        }

        protected class OwnedAddress
        {
            public OwnedCountry Country { get; set; }
        }

        protected class OwnedCountry
        {
            public string Name { get; set; }

            public int PlanetId { get; set; }
            public Planet Planet { get; set; }
        }

        protected class OwnedPerson
        {
            public int Id { get; set; }
            public OwnedAddress PersonAddress { get; set; }

            public ICollection<Order> Orders { get; set; }
        }

        protected class Order
        {
            public int Id { get; set; }
            public OwnedPerson Client { get; set; }
        }

        protected class Branch : OwnedPerson
        {
            public OwnedAddress BranchAddress { get; set; }
        }

        protected class LeafA : Branch
        {
            public OwnedAddress LeafAAddress { get; set; }
        }

        protected class LeafB : OwnedPerson
        {
            public OwnedAddress LeafBAddress { get; set; }
        }

        protected class Planet
        {
            public int Id { get; set; }

            public int StarId { get; set; }
            public Star Star { get; set; }

            public List<Moon> Moons { get; set; }
        }

        protected class Moon
        {
            public int Id { get; set; }
            public int Diameter { get; set; }

            public int PlanetId { get; set; }
        }

        protected class Star
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Element> Composition { get; set; }

            public List<Planet> Planets { get; set; }
        }

        protected class Element
        {
            public string Id { get; set; }
            public string Name { get; set; }

            public int StarId { get; set; }
        }
    }
}
