// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedEntityQueryRelationalTestBase : OwnedEntityQueryTestBase
    {
        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected void ClearLog() => TestSqlLoggerFactory.Clear();

        protected void AssertSql(params string[] expected) => TestSqlLoggerFactory.AssertBaseline(expected);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(bool async)
        {
            var contextFactory = await InitializeAsync<Context24777>();

            using (var context = contextFactory.CreateContext())
            {
                var root3 = await context.Roots.Where(e => e.Id == 3).AsSplitQuery().SingleAsync();

                Assert.Equal(2, root3.ModdleA.Leaves.Count);
            }
        }

        protected class Context24777 : DbContext
        {
            public Context24777(DbContextOptions options)
                   : base(options)
            {
            }

            public DbSet<Root24777> Roots { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Root24777>(b =>
                {
                    b.ToTable(nameof(Root24777));
                    b.HasKey(x => x.Id);
                    b.OwnsOne(x => x.ModdleA, ob =>
                    {
                        ob.ToTable(nameof(ModdleA24777));
                        ob.HasKey(x => x.Id);
                        ob.WithOwner().HasForeignKey(e => e.RootId);
                        ob.OwnsMany(x => x.Leaves, oob =>
                        {
                            oob.ToTable(nameof(Leaf24777));
                            oob.HasKey(x => new { ProductCommissionRulesetId = x.ModdleAId, x.UnitThreshold });
                            oob.WithOwner().HasForeignKey(e => e.ModdleAId);
                            oob.HasData(
                                new Leaf24777 { ModdleAId = 1, UnitThreshold = 1 },
                                new Leaf24777 { ModdleAId = 3, UnitThreshold = 1 },
                                new Leaf24777 { ModdleAId = 3, UnitThreshold = 15 });
                        });

                        ob.HasData(
                            new ModdleA24777 { Id = 1, RootId = 1 },
                            new ModdleA24777 { Id = 2, RootId = 2 },
                            new ModdleA24777 { Id = 3, RootId = 3 });
                    });

                    b.OwnsOne(x => x.MiddleB, ob =>
                    {
                        ob.ToTable(nameof(MiddleB24777));
                        ob.HasKey(x => x.Id);
                        ob.WithOwner().HasForeignKey(e => e.RootId);
                        ob.HasData(
                            new MiddleB24777 { Id = 1, RootId = 1, Enabled = true },
                            new MiddleB24777 { Id = 2, RootId = 3, Enabled = true });
                    });

                    b.HasData(
                        new Root24777 { Id = 1 },
                        new Root24777 { Id = 2 },
                        new Root24777 { Id = 3 });
                });
            }
        }

        protected class Root24777
        {
            public int Id { get; init; }
            public ModdleA24777 ModdleA { get; init; }
            public MiddleB24777 MiddleB { get; init; }
        }

        protected class ModdleA24777
        {
            public int Id { get; init; }
            public int RootId { get; init; }
            public List<Leaf24777> Leaves { get; init; }
        }

        protected class MiddleB24777
        {
            public int Id { get; init; }
            public int RootId { get; init; }
            public bool Enabled { get; init; }
        }

        protected class Leaf24777
        {
            public int ModdleAId { get; init; }
            public int UnitThreshold { get; init; }
        }
    }
}
