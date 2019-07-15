// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query
{
    public class OwnedQueryCosmosTest : OwnedQueryTestBase<OwnedQueryCosmosTest.OwnedQueryCosmosFixture>
    {
        public OwnedQueryCosmosTest(OwnedQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Query_with_owned_entity_equality_operator()
        {
            base.Query_with_owned_entity_equality_operator();
        }

        [ConditionalFact(Skip = "Issue#16392")]
        public override void Navigation_rewrite_on_owned_collection()
        {
            base.Navigation_rewrite_on_owned_collection();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""LeafB"") OR ((c[""Discriminator""] = ""LeafA"") OR ((c[""Discriminator""] = ""Branch"") OR (c[""Discriminator""] = ""OwnedPerson""))))");
        }

        [ConditionalFact(Skip = "Issue#16392")]
        public override void Navigation_rewrite_on_owned_collection_with_composition()
        {
            base.Navigation_rewrite_on_owned_collection_with_composition();
        }

        [ConditionalFact(Skip = "Issue#16392")]
        public override void Navigation_rewrite_on_owned_collection_with_composition_complex()
        {
            base.Navigation_rewrite_on_owned_collection_with_composition_complex();
        }

        public override void Navigation_rewrite_on_owned_reference_projecting_entity()
        {
            base.Navigation_rewrite_on_owned_reference_projecting_entity();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""OwnedPerson"", ""Branch"", ""LeafB"", ""LeafA"") AND (c[""PersonAddress""][""Country""][""Name""] = ""USA""))");
        }

        public override void Navigation_rewrite_on_owned_reference_projecting_scalar()
        {
            base.Navigation_rewrite_on_owned_reference_projecting_scalar();

            AssertSql(
                @"SELECT c[""PersonAddress""][""Country""][""Name""]
FROM root c
WHERE (c[""Discriminator""] IN (""OwnedPerson"", ""Branch"", ""LeafB"", ""LeafA"") AND (c[""PersonAddress""][""Country""][""Name""] = ""USA""))");
        }

        public override void Query_for_base_type_loads_all_owned_navs()
        {
            base.Query_for_base_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""OwnedPerson"", ""Branch"", ""LeafB"", ""LeafA"")");
        }

        public override void Query_for_branch_type_loads_all_owned_navs()
        {
            base.Query_for_branch_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Branch"", ""LeafA"")");
        }

        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
            base.Query_for_leaf_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""LeafA"")");
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection()
        {
            base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection()
        {
            base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Project_multiple_owned_navigations()
        {
            base.Project_multiple_owned_navigations();
        }

        [ConditionalFact(Skip = "LeftJoin #12086")]
        public override void Project_multiple_owned_navigations_with_expansion_on_owned_collections()
        {
            base.Project_multiple_owned_navigations_with_expansion_on_owned_collections();
        }

        [ConditionalFact(Skip = "SelectMany #12086")]
        public override void SelectMany_on_owned_collection()
        {
            base.SelectMany_on_owned_collection();
        }

        [ConditionalFact(Skip = "SelectMany #12086")]
        public override void SelectMany_on_owned_reference_followed_by_regular_entity_and_collection()
        {
            base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection();
        }

        [ConditionalFact(Skip = "SelectMany #12086")]
        public override void SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection()
        {
            base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection();
        }

        [ConditionalFact(Skip = "SelectMany #12086")]
        public override void Query_with_owned_entity_equality_method()
        {
            base.Query_with_owned_entity_equality_method();
        }

        [ConditionalFact(Skip = "SelectMany #12086")]
        public override void Query_with_owned_entity_equality_object_method()
        {
            base.Query_with_owned_entity_equality_object_method();
        }

        [ConditionalFact(Skip = "OfType #12086")]
        public override void Query_with_OfType_eagerly_loads_correct_owned_navigations()
        {
            base.Query_with_OfType_eagerly_loads_correct_owned_navigations();
        }

        [ConditionalFact(Skip = "Distinct ordering #16156")]
        public override void Query_when_subquery()
        {
            base.Query_when_subquery();
        }

        [ConditionalFact(Skip = "Count #16146")]
        public override void No_ignored_include_warning_when_implicit_load()
        {
            base.No_ignored_include_warning_when_implicit_load();
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

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
                            new
                            {
                                Id = 1,
                                id = Guid.NewGuid().ToString()
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

                        eb.OwnsMany(
                            p => p.Orders, ob =>
                            {
                                ob.HasKey(o => o.Id);
                                ob.HasData(
                                    new
                                    {
                                        Id = -10,
                                        ClientId = 1
                                    },
                                    new
                                    {
                                        Id = -11,
                                        ClientId = 1
                                    },
                                    new
                                    {
                                        Id = -20,
                                        ClientId = 2
                                    },
                                    new
                                    {
                                        Id = -30,
                                        ClientId = 3
                                    },
                                    new
                                    {
                                        Id = -40,
                                        ClientId = 4
                                    }
                                );
                            });
                    });

                modelBuilder.Entity<Branch>(
                    eb =>
                    {
                        eb.HasData(
                            new
                            {
                                Id = 2,
                                id = Guid.NewGuid().ToString()
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
                            new
                            {
                                Id = 3,
                                id = Guid.NewGuid().ToString()
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
                            new
                            {
                                Id = 4,
                                id = Guid.NewGuid().ToString()
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
                                    new { Id = "H", Name = "Hydrogen", StarId = 1 },
                                    new { Id = "He", Name = "Helium", StarId = 1 });
                            });
                    });
            }
        }
    }
}
