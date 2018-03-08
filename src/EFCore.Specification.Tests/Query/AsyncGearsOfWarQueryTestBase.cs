// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AsyncGearsOfWarQueryTestBase<TFixture> : AsyncQueryTestBase<TFixture>
        where TFixture : GearsOfWarQueryFixtureBase, new()
    {
        protected AsyncGearsOfWarQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected GearsOfWarContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual async Task Entity_equality_empty()
        {
            await AssertQuery<Gear>(
                gs => gs.Where(g => g == new Gear()));
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_one_to_one_and_one_to_many()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons", "Gear"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons", "Gear")
            };

            await AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task ToString_guid_property_projection()
        {
            await AssertQuery<CogTag>(
                ts => ts.Select(ct => new { A = ct.GearNickName, B = ct.Id.ToString() }),
                elementSorter: e => e.B,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.A, a.A);
                    Assert.Equal(e.B.ToLower(), a.B.ToLower());
                });
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Weapon>(w => w.Owner, "Owner"),
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons", "Owner"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons", "Owner")
            };

            await AssertIncludeQuery<Weapon>(
                ws => ws.Include(w => w.Owner.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "Gear"),
                new ExpectedInclude<Officer>(o => o.Squad, "Squad", "Gear")
            };

            await AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear.Squad),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "Gear"),
                new ExpectedInclude<Officer>(o => o.Squad, "Squad", "Gear"),
                new ExpectedInclude<Squad>(s => s.Members, "Members", "Gear.Squad")
            };

            await AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear.Squad.Members),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_circular()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.CityOfBirth.StationedGears),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_circular_with_filter()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.CityOfBirth.StationedGears).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_using_alternate_key()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.Weapons).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_multiple_include_then_include()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.AssignedCity, "AssignedCity"),
                new ExpectedInclude<Officer>(o => o.AssignedCity, "AssignedCity"),
                new ExpectedInclude<City>(c => c.BornGears, "BornGears", "AssignedCity"),
                new ExpectedInclude<Gear>(g => g.Tag, "Tag", "AssignedCity.BornGears"),
                new ExpectedInclude<Officer>(o => o.Tag, "Tag", "AssignedCity.BornGears"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "AssignedCity"),
                new ExpectedInclude<Gear>(g => g.Tag, "Tag", "AssignedCity.StationedGears"),
                new ExpectedInclude<Officer>(o => o.Tag, "Tag", "AssignedCity.StationedGears"),
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.BornGears, "BornGears", "CityOfBirth"),
                new ExpectedInclude<Gear>(g => g.Tag, "Tag", "CityOfBirth.BornGears"),
                new ExpectedInclude<Officer>(o => o.Tag, "Tag", "CityOfBirth.BornGears"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth"),
                new ExpectedInclude<Gear>(g => g.Tag, "Tag", "CityOfBirth.StationedGears"),
                new ExpectedInclude<Officer>(o => o.Tag, "Tag", "CityOfBirth.StationedGears")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.AssignedCity.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.AssignedCity.StationedGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.StationedGears).ThenInclude(g => g.Tag)
                    .OrderBy(g => g.Nickname),
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task Include_navigation_on_derived_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.OfType<Officer>().Include(o => o.Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task String_based_Include_navigation_on_derived_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.OfType<Officer>().Include("Reports"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Select_Where_Navigation_Included()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear")
            };

            await AssertIncludeQuery<CogTag>(
                ts => from t in ts.Include(o => o.Gear)
                      where t.Gear.Nickname == "Marcus"
                      select t,
                ts => from t in ts
                      where Maybe(t.Gear, () => t.Gear.Nickname) == "Marcus"
                      select t,
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_with_join_reference1()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            await AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        (g, t) => g).Include(g => g.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_with_join_reference2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            await AssertIncludeQuery<CogTag, Gear>(
                (ts, gs) =>
                    ts.Join(
                        gs,
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        (t, g) => g).Include(g => g.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_with_join_collection1()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            await AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        (g, t) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_with_join_collection2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            await AssertIncludeQuery<CogTag, Gear>(
                (ts, gs) =>
                    ts.Join(
                        gs,
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        (t, g) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_reference_on_derived_type_using_string()
        {
            await AssertIncludeQuery<LocustLeader>(
                lls => lls.Include("DefeatedBy"),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalFact]
        public virtual async Task Include_reference_on_derived_type_using_string_nested1()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "DefeatedBy"),
            };

            await AssertIncludeQuery<LocustLeader>(
                lls => lls.Include("DefeatedBy.Squad"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_reference_on_derived_type_using_string_nested2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy"),
                new ExpectedInclude<Officer>(o => o.Reports, "Reports", "DefeatedBy"),
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth", "DefeatedBy.Reports"),
            };

            await AssertIncludeQuery<LocustLeader>(
                lls => lls.Include("DefeatedBy.Reports.CityOfBirth"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_reference_on_derived_type_using_lambda()
        {
            await AssertIncludeQuery<LocustLeader>(
                lls => lls.Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalFact]
        public virtual async Task Include_reference_on_derived_type_using_lambda_with_soft_cast()
        {
            await AssertIncludeQuery<LocustLeader>(
                lls => lls.Include(ll => (ll as LocustCommander).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalFact]
        public virtual async Task Include_reference_on_derived_type_using_lambda_with_tracking()
        {
            await AssertIncludeQuery<LocustLeader>(
                lls => lls.AsTracking().Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") },
                entryCount: 7);
        }

        [ConditionalFact]
        public virtual async Task Include_collection_on_derived_type_using_string()
        {
            await AssertIncludeQuery<Gear>(
                gs => gs.Include("Reports"),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalFact]
        public virtual async Task Include_collection_on_derived_type_using_lambda()
        {
            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Reports),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalFact]
        public virtual async Task Include_collection_on_derived_type_using_lambda_with_soft_cast()
        {
            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => (g as Officer).Reports),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalFact]
        public virtual async Task Include_base_navigation_on_derived_entity()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Tag, "Tag"),
                new ExpectedInclude<Officer>(e => e.Weapons, "Weapons")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Tag).Include(g => ((Officer)g).Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task ThenInclude_collection_on_derived_after_base_reference()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(e => e.Gear, "Gear"),
                new ExpectedInclude<Officer>(e => e.Weapons, "Weapons", "Gear")
            };

            await AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear).ThenInclude(g => (g as Officer).Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task ThenInclude_collection_on_derived_after_derived_reference()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Commander, "Commander"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Commander"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Commander.DefeatedBy"),
            };

            await AssertIncludeQuery<Faction>(
                fs => fs.Include(f => (f as LocustHorde).Commander).ThenInclude(c => (c.DefeatedBy as Officer).Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task ThenInclude_collection_on_derived_after_derived_collection()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Reports, "Reports"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Reports"),
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Reports).ThenInclude(g => ((Officer)g).Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task ThenInclude_reference_on_derived_after_derived_collection()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Leaders, "Leaders"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Leaders")
            };

            await AssertIncludeQuery<Faction>(
                fs => fs.Include(f => ((LocustHorde)f).Leaders).ThenInclude(l => ((LocustCommander)l).DefeatedBy),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Multiple_derived_included_on_one_method()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Commander, "Commander"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Commander"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Commander.DefeatedBy" )
            };

            await AssertIncludeQuery<Faction>(
                fs => fs.Include(f => (((LocustHorde)f).Commander.DefeatedBy as Officer).Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Include_on_derived_multi_level()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Reports, "Reports"),
                new ExpectedInclude<Gear>(e => e.Squad, "Squad", "Reports"),
                new ExpectedInclude<Squad>(e => e.Missions, "Missions", "Reports.Squad")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Reports).ThenInclude(g => g.Squad.Missions),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual async Task Projecting_nullable_bool_in_conditional_works()
        {
            await AssertQuery<CogTag>(
                cgs =>
                    cgs.Select(
                        cg =>
                            new
                            {
                                Prop = cg.Gear != null ? cg.Gear.HasSoulPatch : false
                            }),
                e => e.Prop);
        }

#if !Test20
        [ConditionalFact]
        public virtual async Task Enum_ToString_is_client_eval()
        {
            await AssertQuery<Gear>(
                gs =>
                    gs.OrderBy(g => g.SquadId)
                        .ThenBy(g => g.Nickname)
                        .Select(g => g.Rank.ToString()));
        }
#endif

        [ConditionalFact]
        public virtual async Task Correlated_collections_naked_navigation_with_ToList()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select g.Weapons.ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_naked_navigation_with_ToArray()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select g.Weapons.ToArray(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_basic_projection()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_basic_projection_explicit_to_list()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_basic_projection_explicit_to_array()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToArray(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_basic_projection_ordered()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              orderby w.Name descending
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_basic_projection_composite_key()
        {
            await AssertQuery<Gear>(gs =>
                from o in gs.OfType<Officer>()
                where o.Nickname != "Foo"
                select new
                {
                    o.Nickname,
                    Collection = (from r in o.Reports
                                  where !r.HasSoulPatch
                                  select new { r.Nickname, r.FullName }).ToArray()
                },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<dynamic>(elementSorter: ee => ee.FullName)(e.Collection, a.Collection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_basic_projecting_single_property()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w.Name).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<string>(e => e));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_basic_projecting_constant()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select "BFG").ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<string>());
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_projection_of_collection_thru_navigation()
        {
            await AssertQuery<Gear>(
                gs => from g in gs
                      orderby g.FullName
                      where g.Nickname != "Marcus"
                      select g.Squad.Missions.Where(m => m.MissionId != 17).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<SquadMission>(
                    e => e.MissionId + " " + e.SquadId,
                    (e, a) =>
                    {
                        Assert.Equal(e.MissionId, a.MissionId);
                        Assert.Equal(e.SquadId, a.SquadId);
                    }));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_project_anonymous_collection_result()
        {
            await AssertQuery<Squad>(
                ss => from s in ss
                      where s.Id < 20
                      select new
                      {
                          s.Name,
                          Collection = (from m in s.Members
                                        select new { m.FullName, m.Rank }).ToList()
                      },
                elementSorter: e => e.Name,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Name, a.Name);
                    CollectionAsserter<dynamic>(ee => ee.FullName + " " + ee.Rank)(e.Collection, a.Collection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_nested()
        {
            await AssertQuery<Squad>(
                ss => from s in ss
                      select (from m in s.Missions
                              where m.MissionId < 42
                              select (from ps in m.Mission.ParticipatingSquads
                                      where ps.SquadId < 7
                                      select ps).ToList()).ToList(),
                elementSorter: CollectionSorter<object>(),
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter(
                        CollectionSorter<SquadMission>(),
                        CollectionAsserter<SquadMission>(
                            ee => ee.SquadId + " " + ee.MissionId,
                            (ee, aa) =>
                            {
                                Assert.Equal(ee.SquadId, aa.SquadId);
                                Assert.Equal(ee.MissionId, aa.MissionId);
                            }))(e, a);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_nested_mixed_streaming_with_buffer1()
        {
            await AssertQuery<Squad>(
                ss => from s in ss
                      select (from m in s.Missions
                              where m.MissionId < 3
                              select (from ps in m.Mission.ParticipatingSquads
                                      where ps.SquadId < 2
                                      select ps).ToList()),
                elementSorter: CollectionSorter<object>(),
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter(
                        CollectionSorter<SquadMission>(),
                        CollectionAsserter<SquadMission>(
                            ee => ee.SquadId + " " + ee.MissionId,
                            (ee, aa) =>
                            {
                                Assert.Equal(ee.SquadId, aa.SquadId);
                                Assert.Equal(ee.MissionId, aa.MissionId);
                            }))(e, a);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_nested_mixed_streaming_with_buffer2()
        {
            await AssertQuery<Squad>(
                ss => from s in ss
                      select (from m in s.Missions
                              where m.MissionId < 42
                              select (from ps in m.Mission.ParticipatingSquads
                                      where ps.SquadId < 7
                                      select ps)).ToList(),
                elementSorter: CollectionSorter<object>(),
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter(
                        CollectionSorter<SquadMission>(),
                        CollectionAsserter<SquadMission>(
                            ee => ee.SquadId + " " + ee.MissionId,
                            (ee, aa) =>
                            {
                                Assert.Equal(ee.SquadId, aa.SquadId);
                                Assert.Equal(ee.MissionId, aa.MissionId);
                            }))(e, a);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_nested_with_custom_ordering()
        {
            await AssertQuery<Gear>(
                gs => gs
                    .OfType<Officer>()
                    .OrderByDescending(o => o.HasSoulPatch)
                    .Select(o => new
                    {
                        o.FullName,
                        OuterCollection = o.Reports
                            .Where(r => r.FullName != "Foo")
                            .OrderBy(r => r.Rank)
                            .Select(g => new
                            {
                                g.FullName,
                                InnerCollection = g.Weapons
                                    .Where(w => w.Name != "Bar")
                                    .OrderBy(w => w.IsAutomatic).ToList()
                            }).ToList()
                    }),
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<dynamic>(
                        ee => ee.FullName,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            CollectionAsserter<dynamic>(
                                eee => eee.Name,
                                (eee, aaa) => Assert.Equal(eee.Name, aaa.Name))(ee.InnerCollection, aa.InnerCollection);
                        })(e.OuterCollection, a.OuterCollection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_same_collection_projected_multiple_times()
        {
            await AssertQuery<Gear>(
                gs =>
                    from g in gs
                    select new
                    {
                        g.FullName,
                        First = g.Weapons.Where(w1 => w1.IsAutomatic).ToList(),
                        Second = g.Weapons.Where(w2 => w2.IsAutomatic).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<Weapon>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.First, a.First);
                    CollectionAsserter<Weapon>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.Second, a.Second);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_similar_collection_projected_multiple_times()
        {
            await AssertQuery<Gear>(
                gs =>
                    from g in gs
                    orderby g.Rank
                    select new
                    {
                        g.FullName,
                        First = g.Weapons.OrderBy(w1 => w1.OwnerFullName).Where(w1 => w1.IsAutomatic).ToList(),
                        Second = g.Weapons.OrderBy(w2 => w2.IsAutomatic).Where(w2 => !w2.IsAutomatic).ToArray()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<Weapon>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.First, a.First);
                    CollectionAsserter<Weapon>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.Second, a.Second);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_different_collections_projected()
        {
            await AssertQuery<Gear>(
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.FullName
                    select new
                    {
                        o.Nickname,
                        First = o.Weapons.Where(w => w.IsAutomatic).Select(w => new { w.Name, w.IsAutomatic }).ToArray(),
                        Second = o.Reports.OrderBy(r => r.FullName).Select(r => new { r.Nickname, r.Rank }).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<dynamic>()(e.First, a.First);
                    CollectionAsserter<dynamic>()(e.Second, a.Second);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_multiple_nested_complex_collections()
        {
            await AssertQuery<Gear>(
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.HasSoulPatch descending, o.Tag.Note
                    where o.Reports.Any()
                    select new
                    {
                        o.FullName,
                        OuterCollection = (from r in o.Reports
                                           where r.FullName != "Foo"
                                           orderby r.Rank
                                           select new
                                           {
                                               r.FullName,
                                               InnerCollection = (from w in r.Weapons
                                                                  where w.Name != "Bar"
                                                                  orderby w.IsAutomatic
                                                                  select new
                                                                  {
                                                                      w.Id,
                                                                      InnerFirst = w.Owner.Weapons.Select(ww => new { ww.Name, ww.IsAutomatic }).ToList(),
                                                                      InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                                                  }).ToList()
                                           }).ToList(),
                        OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                            orderby www.IsAutomatic, www.Owner.Nickname descending
                                            select www).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);

                    CollectionAsserter<dynamic>(
                        ee => ee.FullName,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            CollectionAsserter<dynamic>(
                                eee => eee.Id,
                                (eee, aaa) =>
                                {
                                    Assert.Equal(eee.Id, aaa.Id);
                                    CollectionAsserter<dynamic>(eeee => eeee.Name)(eee.InnerFirst, aaa.InnerFirst);
                                    CollectionAsserter<dynamic>()(eee.InnerSecond, aaa.InnerSecond);
                                })(ee.InnerCollection, aa.InnerCollection);
                        })(e.OuterCollection, a.OuterCollection);

                    CollectionAsserter<dynamic>(
                        ee => ee.Id,
                        (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.OuterCollection2, a.OuterCollection2);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_inner_subquery_selector_references_outer_qsre()
        {
            await AssertQuery<Gear>(
                gs =>
                    from o in gs.OfType<Officer>()
                    select new
                    {
                        o.FullName,
                        Collection = from r in o.Reports
                                     select new { ReportName = r.FullName, OfficerName = o.FullName }
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<dynamic>(ee => ee.ReportName)(e.Collection, a.Collection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre()
        {
            await AssertQuery<Gear>(
                gs =>
                    from o in gs.OfType<Officer>()
                    select new
                    {
                        o.FullName,
                        Collection = from r in o.Reports
                                     where o.FullName != "Foo"
                                     select new { ReportName = r.FullName }
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<dynamic>(ee => ee.ReportName)(e.Collection, a.Collection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up()
        {
            await AssertQuery<Gear>(
                gs =>
                    from o in gs.OfType<Officer>()
                    select new
                    {
                        o.FullName,
                        OuterCollection = (from r in o.Reports
                                           where r.FullName != "Foo"
                                           select new
                                           {
                                               r.FullName,
                                               InnerCollection = (from w in r.Weapons
                                                                  where w.Name != "Bar"
                                                                  select new
                                                                  {
                                                                      w.Name,
                                                                      r.Nickname
                                                                  }).ToList()
                                           }).ToList(),
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<dynamic>(
                        ee => ee.FullName,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            CollectionAsserter<dynamic>(eee => eee.Name)(ee.InnerCollection, aa.InnerCollection);
                        })(e.OuterCollection, a.OuterCollection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up()
        {
            await AssertQuery<Gear>(
                gs =>
                    from o in gs.OfType<Officer>()
                    select new
                    {
                        o.FullName,
                        OuterCollection = from r in o.Reports
                                          where r.FullName != "Foo"
                                          select new
                                          {
                                              r.FullName,
                                              InnerCollection = from w in r.Weapons
                                                                where w.Name != "Bar"
                                                                select new
                                                                {
                                                                    w.Name,
                                                                    o.Nickname
                                                                }
                                          },
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<dynamic>(
                        ee => ee.FullName,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            CollectionAsserter<dynamic>(eee => eee.Name)(ee.InnerCollection, aa.InnerCollection);
                        })(e.OuterCollection, a.OuterCollection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_on_select_many()
        {
            await AssertQuery<Gear, Squad>(
                (gs, ss) =>
                    from g in gs
                    from s in ss
                    where g.HasSoulPatch
                    orderby g.Nickname, s.Id descending
                    select new
                    {
                        GearNickname = g.Nickname,
                        SquadName = s.Name,
                        Collection1 = from w in g.Weapons
                                      where w.IsAutomatic || w.Name != "foo"
                                      select w,
                        Collection2 = from m in s.Members
                                      where !m.HasSoulPatch
                                      select m
                    },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.GearNickname, e.GearNickname);
                    Assert.Equal(e.SquadName, e.SquadName);

                    CollectionAsserter<Weapon>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.Collection1, a.Collection1);
                    CollectionAsserter<Gear>(ee => ee.Nickname, (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e.Collection2, a.Collection2);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_with_Skip()
        {
            await AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Skip(1)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_with_Take()
        {
            await AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Take(2)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_with_Distinct()
        {
            await AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Distinct()),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_with_FirstOrDefault()
        {
            await AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Select(m => m.FullName).FirstOrDefault()),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname));
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_on_left_join_with_predicate()
        {
            await AssertQuery<CogTag, Gear>(
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    where !g.HasSoulPatch
                    select new { g.Nickname, WeaponNames = g.Weapons.Select(w => w.Name).ToList() },
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    where !MaybeScalar<bool>(g, () => g.HasSoulPatch) == true || g == null
                    select new { Nickname = Maybe(g, () => g.Nickname), WeaponNames = g == null ? new List<string>() : g.Weapons.Select(w => w.Name) },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<string>(ee => ee)(e.WeaponNames, a.WeaponNames);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_on_left_join_with_null_value()
        {
            await AssertQuery<CogTag, Gear>(
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note
                    select g.Weapons.Select(w => w.Name).ToList(),
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note
                    select g != null ? g.Weapons.Select(w => w.Name) : new List<string>(),
                assertOrder: true,
                elementAsserter: (e, a) => CollectionAsserter<string>(ee => ee));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_left_join_with_self_reference()
        {
            await AssertQuery<CogTag, Gear>(
                (ts, gs) =>
                    from t in ts
                    join o in gs.OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                    from o in grouping.DefaultIfEmpty()
                    select new { t.Note, ReportNames = o.Reports.Select(r => r.FullName).ToList() },
                (ts, gs) =>
                    from t in ts
                    join o in gs.OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                    from o in grouping.DefaultIfEmpty()
                    select new { t.Note, ReportNames = o != null ? o.Reports.Select(r => r.FullName) : new List<string>() },
                elementSorter: e => e.Note,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Note, a.Note);
                    CollectionAsserter<string>(ee => ee)(e.ReportNames, a.ReportNames);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_deeply_nested_left_join()
        {
            await AssertQuery<CogTag, Gear>(
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note, g.Nickname descending
                    select g.Squad.Members.Where(m => m.HasSoulPatch).Select(m => new { m.Nickname, AutomaticWeapons = m.Weapons.Where(w => w.IsAutomatic).ToList() }).ToList(),
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note, Maybe(g, () => g.Nickname) descending
                    select g != null ? g.Squad.Members.Where(m => m.HasSoulPatch).OrderBy(m => m.Nickname).Select(m => m.Weapons.Where(w => w.IsAutomatic)) : new List<List<Weapon>>(),
                assertOrder: true,
                elementAsserter: (e, a) =>
                    CollectionAsserter<dynamic>(
                        elementAsserter: (ee, aa) => CollectionAsserter<Weapon>(eee => eee.Id, (eee, aaa) => Assert.Equal(eee.Id, aaa.Id))));
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join()
        {
            await AssertQuery<Weapon>(
                ws => ws.OrderBy(w => w.Name).Select(w => w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(m => new { Weapons = m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id).ToList(), m.Rank }).ToList()),
                ws => ws.OrderBy(w => w.Name).Select(w => w.Owner != null
                    ? w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(m => new Tuple<IEnumerable<Weapon>, MilitaryRank>(m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id), m.Rank))
                    : new List<Tuple<IEnumerable<Weapon>, MilitaryRank>>()),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<dynamic>(
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.Item2, aa.Rank);
                            CollectionAsserter<Weapon>(
                                elementAsserter: (eee, aaa) => Assert.Equal(eee.Id, aaa.Id))(ee.Item1, aa.Weapons);
                        })(e, a);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_complex_scenario1()
        {
            await AssertQuery<Gear>(
                gs =>
                    from r in gs
                    select new
                    {
                        r.FullName,
                        OuterCollection = (from w in r.Weapons
                                           select new
                                           {
                                               w.Id,
                                               InnerCollection = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                           }).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);

                    CollectionAsserter<dynamic>(
                        ee => ee.Id,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.Id, aa.Id);
                            CollectionAsserter<dynamic>(eee => eee.Nickname)(ee.InnerCollection, aa.InnerCollection);
                        })(e.OuterCollection, a.OuterCollection);
                });
        }

        [ConditionalFact]
        public virtual async Task Correlated_collections_complex_scenario2()
        {
            await AssertQuery<Gear>(
                gs =>
                    from o in gs.OfType<Officer>()
                    select new
                    {
                        o.FullName,
                        OuterCollection = (from r in o.Reports
                                           select new
                                           {
                                               r.FullName,
                                               InnerCollection = (from w in r.Weapons
                                                                  select new
                                                                  {
                                                                      w.Id,
                                                                      InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                                                  }).ToList()
                                           }).ToList(),
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);

                    CollectionAsserter<dynamic>(
                        ee => ee.FullName,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            CollectionAsserter<dynamic>(
                                eee => eee.Id,
                                (eee, aaa) =>
                                {
                                    Assert.Equal(eee.Id, aaa.Id);
                                    CollectionAsserter<dynamic>()(eee.InnerSecond, aaa.InnerSecond);
                                })(ee.InnerCollection, aa.InnerCollection);
                        })(e.OuterCollection, a.OuterCollection);
                });
        }

        [ConditionalFact]
        public virtual async Task Outer_parameter_in_join_key()
        {
            await AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from o in gs.OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ts
                                      join g in gs on o.FullName equals g.FullName
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => CollectionAsserter<string>(elementSorter: ee => ee)(e.Collection, a.Collection));
        }

        [ConditionalFact]
        public virtual async Task Outer_parameter_in_group_join_key()
        {
            await AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from o in gs.OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ts
                                      join g in gs on o.FullName equals g.FullName into grouping
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => CollectionAsserter<string>(elementSorter: ee => ee)(e.Collection, a.Collection));
        }

        [ConditionalFact]
        public virtual async Task Outer_parameter_in_group_join_with_DefaultIfEmpty()
        {
            await AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from o in gs.OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ts
                                      join g in gs on o.FullName equals g.FullName into grouping
                                      from g in grouping.DefaultIfEmpty()
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => CollectionAsserter<string>(elementSorter: ee => ee)(e.Collection, a.Collection));
        }

        [ConditionalFact]
        public virtual async Task Include_with_concat()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Squad, "Squad"),
                new ExpectedInclude<Officer>(o => o.Squad, "Squad")
            };

            await AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.Squad).Concat(gs),
                expectedIncludes);
        }
    }
}
