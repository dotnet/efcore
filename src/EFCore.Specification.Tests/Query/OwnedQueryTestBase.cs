// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : OwnedQueryTestBase<TFixture>.OwnedQueryFixtureBase, new()
    {
        protected OwnedQueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
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
            }
        }

        [Fact]
        public virtual void No_ignored_include_warning_when_implicit_load()
        {
            using (var context = CreateContext())
            {
                var count = context.Set<OwnedPerson>().Count();

                Assert.Equal(4, count);
            }
        }

        [Fact]
        public virtual void Query_for_branch_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<Branch>().ToList();

                Assert.Equal(2, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.All(b => b.BranchAddress != null));
                Assert.True(people.OfType<LeafA>().All(a => a.LeafAAddress != null));
            }
        }

        [Fact]
        public virtual void Query_for_leaf_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<LeafA>().ToList();

                Assert.Equal(1, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.All(b => b.BranchAddress != null));
                Assert.True(people.All(a => a.LeafAAddress != null));
            }
        }

        [Fact]
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

        [Fact]
        public virtual void Query_when_subquery()
        {
            using (var context = CreateContext())
            {
                var people
                    = context.Set<OwnedPerson>()
                        .Distinct()
                        .OrderBy(p => p.Id)
                        .Take(5)
                        .Select(op => new { op })
                        .ToList();

                Assert.Equal(4, people.Count);
                Assert.True(people.All(p => p.op.PersonAddress != null));
                Assert.True(people.Select(p => p.op).OfType<Branch>().All(b => b.BranchAddress != null));
                Assert.True(people.Select(p => p.op).OfType<LeafA>().All(a => a.LeafAAddress != null));
                Assert.True(people.Select(p => p.op).OfType<LeafB>().All(b => b.LeafBAddress != null));
            }
        }

        protected virtual DbContext CreateContext() => Fixture.CreateContext();

        public abstract class OwnedQueryFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "OwnedQueryTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<OwnedPerson>().OwnsOne(p => p.PersonAddress).OwnsOne(a => a.Country);
                modelBuilder.Entity<Branch>().OwnsOne(p => p.BranchAddress).OwnsOne(a => a.Country);
                modelBuilder.Entity<LeafA>().OwnsOne(p => p.LeafAAddress).OwnsOne(a => a.Country);
                modelBuilder.Entity<LeafB>().OwnsOne(p => p.LeafBAddress).OwnsOne(a => a.Country);
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                return base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Throw());
            }

            protected override void Seed(DbContext context)
            {
                context.Set<OwnedPerson>().AddRange(
                    new OwnedPerson
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        }
                    },
                    new Branch
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        },
                        BranchAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Canada" }
                        }
                    },
                    new LeafA
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        },
                        BranchAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Canada" }
                        },
                        LeafAAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Mexico" }
                        }
                    },
                    new LeafB
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        },
                        LeafBAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Panama" }
                        }
                    });

                context.SaveChanges();
            }

            public override DbContext CreateContext()
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
        }

        protected class OwnedPerson
        {
            public int Id { get; set; }
            public OwnedAddress PersonAddress { get; set; }
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
    }
}
