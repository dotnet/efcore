// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQueryCosmosTest : OwnedQueryTestBase<OwnedQueryCosmosTest.OwnedQueryCosmosFixture>
    {
        public OwnedQueryCosmosTest(OwnedQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Query_loads_reference_nav_automatically_in_projection(bool async)
        {
            return base.Query_loads_reference_nav_automatically_in_projection(async);
        }

        [ConditionalTheory(Skip = "SelectMany #17246")]
        public override Task Query_with_owned_entity_equality_operator(bool async)
        {
            return base.Query_with_owned_entity_equality_operator(async);
        }

        [ConditionalTheory(Skip = "Count #16146")]
        public override async Task Navigation_rewrite_on_owned_collection(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""LeafB"") OR ((c[""Discriminator""] = ""LeafA"") OR ((c[""Discriminator""] = ""Branch"") OR (c[""Discriminator""] = ""OwnedPerson""))))");
        }

        [ConditionalTheory(Skip = "Issue#16926")]
        public override async Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection_with_composition(async);

            AssertSql(" ");
        }

        [ConditionalTheory(Skip = "Issue#16926")]
        public override async Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
        {
            await base.Navigation_rewrite_on_owned_collection_with_composition_complex(async);

            AssertSql(" ");
        }

        public override async Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_projecting_entity(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""OwnedPerson"", ""Branch"", ""LeafB"", ""LeafA"") AND (c[""PersonAddress""][""Country""][""Name""] = ""USA""))");
        }

        public override async Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
        {
            await base.Navigation_rewrite_on_owned_reference_projecting_scalar(async);

            AssertSql(
                @"SELECT c[""PersonAddress""][""Country""][""Name""]
FROM root c
WHERE (c[""Discriminator""] IN (""OwnedPerson"", ""Branch"", ""LeafB"", ""LeafA"") AND (c[""PersonAddress""][""Country""][""Name""] = ""USA""))");
        }

        public override async Task Query_for_base_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_base_type_loads_all_owned_navs(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""OwnedPerson"", ""Branch"", ""LeafB"", ""LeafA"")");
        }

        public override async Task Query_for_branch_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_branch_type_loads_all_owned_navs(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Branch"", ""LeafA"")");
        }

        public override async Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
        {
            await base.Query_for_branch_type_loads_all_owned_navs_tracking(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Branch"", ""LeafA"")");
        }

        public override async Task Query_for_leaf_type_loads_all_owned_navs(bool async)
        {
            await base.Query_for_leaf_type_loads_all_owned_navs(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""LeafA"")");
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
        {
            return base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task
            Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool async)
        {
            return base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
                async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Project_multiple_owned_navigations(bool async)
        {
            return base.Project_multiple_owned_navigations(async);
        }

        [ConditionalTheory(Skip = "LeftJoin #17314")]
        public override Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
        {
            return base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(async);
        }

        [ConditionalTheory(Skip = "SelectMany #17246")]
        public override Task SelectMany_on_owned_collection(bool async)
        {
            return base.SelectMany_on_owned_collection(async);
        }

        [ConditionalTheory(Skip = "SelectMany #17246")]
        public override Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            return base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(async);
        }

        [ConditionalTheory(Skip = "SelectMany #17246")]
        public override Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
        {
            return base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(async);
        }

        [ConditionalTheory(Skip = "SelectMany #17246")]
        public override Task Query_with_owned_entity_equality_method(bool async)
        {
            return base.Query_with_owned_entity_equality_method(async);
        }

        [ConditionalTheory(Skip = "SelectMany #17246")]
        public override Task Query_with_owned_entity_equality_object_method(bool async)
        {
            return base.Query_with_owned_entity_equality_object_method(async);
        }

        [ConditionalTheory(Skip = "OfType #17246")]
        public override Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
        {
            return base.Query_with_OfType_eagerly_loads_correct_owned_navigations(async);
        }

        [ConditionalTheory(Skip = "Distinct ordering #16156")]
        public override Task Query_when_subquery(bool async)
        {
            return base.Query_when_subquery(async);
        }

        [ConditionalTheory(Skip = "Count #16146")]
        public override Task No_ignored_include_warning_when_implicit_load(bool async)
        {
            return base.No_ignored_include_warning_when_implicit_load(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        public class OwnedQueryCosmosFixture : OwnedQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<OwnedPerson>(
                    eb =>
                    {
                        eb.HasData(
                            new { Id = 1, id = Guid.NewGuid().ToString() });

                        eb.OwnsOne(
                            p => p.PersonAddress, ab =>
                            {
                                ab.HasData(
                                    new { OwnedPersonId = 1 }, new { OwnedPersonId = 2 }, new { OwnedPersonId = 3 },
                                    new { OwnedPersonId = 4 });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 1,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 2,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 3,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 4,
                                                PlanetId = 1,
                                                Name = "USA"
                                            });

                                        cb.HasOne(cc => cc.Planet).WithMany().HasForeignKey(ee => ee.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);
                                    });
                            });

                        eb.OwnsMany(
                            p => p.Orders, ob =>
                            {
                                ob.HasKey(o => o.Id);
                                ob.HasData(
                                    new { Id = -10, ClientId = 1 },
                                    new { Id = -11, ClientId = 1 },
                                    new { Id = -20, ClientId = 2 },
                                    new { Id = -30, ClientId = 3 },
                                    new { Id = -40, ClientId = 4 }
                                );
                            });
                    });

                modelBuilder.Entity<Branch>(
                    eb =>
                    {
                        eb.HasData(
                            new { Id = 2, id = Guid.NewGuid().ToString() });

                        eb.OwnsOne(
                            p => p.BranchAddress, ab =>
                            {
                                ab.HasData(
                                    new { BranchId = 2 }, new { BranchId = 3 });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressBranchId = 2,
                                                PlanetId = 1,
                                                Name = "Canada"
                                            },
                                            new
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
                            new { Id = 3, id = Guid.NewGuid().ToString() });

                        eb.OwnsOne(
                            p => p.LeafAAddress, ab =>
                            {
                                ab.HasData(
                                    new { LeafAId = 3 });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
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
                            new { Id = 4, id = Guid.NewGuid().ToString() });

                        eb.OwnsOne(
                            p => p.LeafBAddress, ab =>
                            {
                                ab.HasData(
                                    new { LeafBId = 4 });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
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

                modelBuilder.Entity<Planet>(
                    pb =>
                    {
                        pb.HasData(
                            new
                            {
                                Id = 1,
                                id = Guid.NewGuid().ToString(),
                                StarId = 1
                            });
                    });

                modelBuilder.Entity<Moon>(
                    mb =>
                    {
                        mb.HasData(
                            new
                            {
                                Id = 1,
                                id = Guid.NewGuid().ToString(),
                                PlanetId = 1,
                                Diameter = 3474
                            });
                    });

                modelBuilder.Entity<Star>(
                    sb =>
                    {
                        sb.HasData(
                            new
                            {
                                Id = 1,
                                id = Guid.NewGuid().ToString(),
                                Name = "Sol"
                            });

                        sb.OwnsMany(
                            s => s.Composition, ob =>
                            {
                                ob.HasKey(o => o.Id);
                                ob.HasData(
                                    new
                                    {
                                        Id = "H",
                                        Name = "Hydrogen",
                                        StarId = 1
                                    },
                                    new
                                    {
                                        Id = "He",
                                        Name = "Helium",
                                        StarId = 1
                                    });
                            });
                    });

                modelBuilder.Entity<Barton>(
                    b =>
                    {
                        b.OwnsOne(
                            e => e.Throned, b => b.HasData(
                                new { BartonId = 1, Property = "Property" }));
                        b.HasData(
                            new Barton { Id = 1, Simple = "Simple" });
                    });

                modelBuilder.Entity<Fink>().HasData(
                    new { Id = 1, BartonId = 1 });
            }
        }
    }
}
