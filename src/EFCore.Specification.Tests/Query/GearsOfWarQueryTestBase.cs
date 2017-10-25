// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable SimplifyConditionalTernaryExpression
// ReSharper disable ArgumentsStyleAnonymousFunction
// ReSharper disable ArgumentsStyleOther
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable EqualExpressionComparison
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable ReplaceWithSingleCallToSingle
// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class GearsOfWarQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : GearsOfWarQueryFixtureBase, new()
    {
        protected GearsOfWarQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void Entity_equality_empty()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g == new Gear()));
        }

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_and_one_to_many()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons", "Gear"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons", "Gear")
            };

            AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void ToString_guid_property_projection()
        {
            AssertQuery<CogTag>(
                ts => ts.Select(ct => new { A = ct.GearNickName, B = ct.Id.ToString() }),
                elementSorter: e => e.B,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.A, a.A);
                        Assert.Equal(e.B.ToLower(), a.B.ToLower());
                    });
        }

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Weapon>(w => w.Owner, "Owner"),
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons", "Owner"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons", "Owner")
            };

            AssertIncludeQuery<Weapon>(
                ws => ws.Include(w => w.Owner.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "Gear"),
                new ExpectedInclude<Officer>(o => o.Squad, "Squad", "Gear")
            };

            AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear.Squad),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "Gear"),
                new ExpectedInclude<Officer>(o => o.Squad, "Squad", "Gear"),
                new ExpectedInclude<Squad>(s => s.Members, "Members", "Gear.Squad")
            };

            AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear.Squad.Members),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_multiple_circular()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.CityOfBirth.StationedGears),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_multiple_circular_with_filter()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.CityOfBirth.StationedGears).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_using_alternate_key()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.Weapons).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_multiple_include_then_include()
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

            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => g.AssignedCity.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.AssignedCity.StationedGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.StationedGears).ThenInclude(g => g.Tag)
                    .OrderBy(g => g.Nickname),
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Include_navigation_on_derived_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            AssertIncludeQuery<Gear>(
                gs => gs.OfType<Officer>().Include(o => o.Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void String_based_Include_navigation_on_derived_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            AssertIncludeQuery<Gear>(
                gs => gs.OfType<Officer>().Include("Reports"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Included()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear")
            };

            AssertIncludeQuery<CogTag>(
                ts => from t in ts.Include(o => o.Gear)
                      where t.Gear.Nickname == "Marcus"
                      select t,
                ts => from t in ts
                      where Maybe(t.Gear, () => t.Gear.Nickname) == "Marcus"
                      select t,
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_join_reference1()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        (g, t) => g).Include(g => g.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_join_reference2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            AssertIncludeQuery<CogTag, Gear>(
                (ts, gs) =>
                    ts.Join(
                        gs,
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        (t, g) => g).Include(g => g.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_join_collection1()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        (g, t) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_join_collection2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            AssertIncludeQuery<CogTag, Gear>(
                (ts, gs) =>
                    ts.Join(
                        gs,
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        (t, g) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_where_list_contains_navigation()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var gears = context.Gears
                    .Include(g => g.Tag)
                    .Where(g => g.Tag != null && tags.Contains(g.Tag.Id))
                    .ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_where_list_contains_navigation2()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var gears = context.Gears
                    .Include(g => g.Tag)
                    .Where(g => g.CityOfBirth.Location != null && tags.Contains(g.Tag.Id))
                    .ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_accessed_twice_outside_and_inside_subquery()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var gears = context.Gears
                    .Where(g => g.Tag != null && tags.Contains(g.Tag.Id))
                    .ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_multi_level()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        (g, t) => g).Include(g => g.CityOfBirth.StationedGears),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_join_and_inheritance1()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    ts.Join(
                        gs.OfType<Officer>(),
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        o => new { SquadId = (int?)o.SquadId, o.Nickname },
                        (t, o) => o).Include(o => o.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_join_and_inheritance2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    gs.OfType<Officer>().Join(
                        ts,
                        o => new { SquadId = (int?)o.SquadId, o.Nickname },
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        (o, t) => o).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_join_and_inheritance3()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    ts.Join(
                        gs.OfType<Officer>(),
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        g => new { SquadId = (int?)g.SquadId, g.Nickname },
                        (t, o) => o).Include(o => o.Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_with_nested_navigation_in_order_by()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Weapon>(w => w.Owner, "Owner")
            };

            AssertIncludeQuery<Weapon>(
                ws => ws
                    .Include(w => w.Owner)
                    .Where(w => w.Owner.Nickname != "Paduk")
                    .OrderBy(e => e.Owner.CityOfBirth.Name).ThenBy(e => e.Id),
                ws => ws
                    .Include(w => w.Owner)
                    .Where(w => Maybe(w.Owner, () => w.Owner.Nickname) != "Paduk")
                    .OrderBy(e => Maybe(e.Owner, () => Maybe(e.Owner.CityOfBirth, () => e.Owner.CityOfBirth.Name)))
                    .ThenBy(e => e.Id),
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Where_enum()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank == MilitaryRank.Sergeant));
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_constant()
        {
            AssertQuery<Weapon>(
                ws => ws.Where(w => w.AmmunitionType == AmmunitionType.Cartridge));
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_null_constant()
        {
            AssertQuery<Weapon>(
                ws => ws.Where(w => w.AmmunitionType == null));
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_non_nullable_parameter()
        {
            var ammunitionType = AmmunitionType.Cartridge;

            AssertQuery<Weapon>(
                ws => ws.Where(w => w.AmmunitionType == ammunitionType));
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_nullable_parameter()
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            AssertQuery<Weapon>(
                ws => ws.Where(w => w.AmmunitionType == ammunitionType));

            ammunitionType = null;

            AssertQuery<Weapon>(
                ws => ws.Where(w => w.AmmunitionType == ammunitionType));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_enum()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (g.Rank & MilitaryRank.Corporal) > 0));

            AssertQuery<Gear>(
                gs => gs.Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_integral()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => ((int)g.Rank & 1) == 1));

            AssertQuery<Gear>(
                gs => gs.Where(g => ((long)g.Rank & 1L) == 1L));

            AssertQuery<Gear>(
                gs => gs.Where(g => ((short)g.Rank & (short)1) == 1));

            AssertQuery<Gear>(
                gs => gs.Where(g => ((char)g.Rank & '\x0001') == '\x0001'));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_constant()
        {
            AssertQuery<Weapon>(
                ws => ws.Where(w => (w.AmmunitionType & AmmunitionType.Cartridge) > 0));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            AssertQuery<Weapon>(
#pragma warning disable CS0458 // The result of the expression is always 'null'
                ws => ws.Where(w => (w.AmmunitionType & null) > 0));
#pragma warning restore CS0458 // The result of the expression is always 'null'
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            var ammunitionType = AmmunitionType.Cartridge;

            AssertQuery<Weapon>(
                ws => ws.Where(w => (w.AmmunitionType & ammunitionType) > 0));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_nullable_parameter()
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            AssertQuery<Weapon>(
                ws => ws.Where(w => (w.AmmunitionType & ammunitionType) > 0));

            ammunitionType = null;

            AssertQuery<Weapon>(
                ws => ws.Where(w => (w.AmmunitionType & ammunitionType) > 0));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_or_enum()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (g.Rank | MilitaryRank.Corporal) > 0));
        }

        [ConditionalFact]
        public virtual void Bitwise_projects_values_in_select()
        {
            AssertSingleResult<Gear>(
                gs => gs
                    .Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal)
                    .Select(
                        b => new
                        {
                            BitwiseTrue = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal,
                            BitwiseFalse = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Sergeant,
                            BitwiseValue = b.Rank & MilitaryRank.Corporal
                        }).First());
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag()
        {
            // Constant
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag(MilitaryRank.Corporal)));

            // Expression
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag(MilitaryRank.Corporal | MilitaryRank.Captain)));

            // Casting
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag((MilitaryRank)1)));

            // Casting to nullable
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag((MilitaryRank?)1)));

            // QuerySource
            AssertQuery<Gear>(
                gs => gs.Where(g => MilitaryRank.Corporal.HasFlag(g.Rank)));
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag_subquery()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag(gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));

            AssertQuery<Gear>(
                gs => gs.Where(g => MilitaryRank.Corporal.HasFlag(gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag_subquery_client_eval()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag(gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).First().Rank)));
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag_with_non_nullable_parameter()
        {
            var parameter = MilitaryRank.Corporal;

            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag(parameter)));
        }

        [ConditionalFact]
        public virtual void Where_has_flag_with_nullable_parameter()
        {
            MilitaryRank? parameter = MilitaryRank.Corporal;

            AssertQuery<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag(parameter)));
        }

        [ConditionalFact]
        public virtual void Select_enum_has_flag()
        {
            AssertSingleResult<Gear>(
                gs => gs.Where(g => g.Rank.HasFlag(MilitaryRank.Corporal))
                    .Select(
                        b => new
                        {
                            hasFlagTrue = b.Rank.HasFlag(MilitaryRank.Corporal),
                            hasFlagFalse = b.Rank.HasFlag(MilitaryRank.Sergeant)
                        }).First());
        }

        [ConditionalFact]
        public virtual void Where_count_subquery_without_collision()
        {
            AssertQuery<Gear>(
                gs => gs.Where(w => w.Weapons.Count == 2));
        }

        [ConditionalFact]
        public virtual void Where_any_subquery_without_collision()
        {
            AssertQuery<Gear>(
                gs => gs.Where(w => w.Weapons.Any()));
        }

        [ConditionalFact]
        public virtual void Select_inverted_boolean()
        {
            AssertQuery<Weapon>(
                ws => ws
                    .Where(w => w.IsAutomatic)
                    .Select(w => new { w.Id, Manual = !w.IsAutomatic }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_comparison_with_null()
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            AssertQuery<Weapon>(
                ws => ws
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(w => new { w.Id, Cartidge = w.AmmunitionType == ammunitionType }),
                elementSorter: e => e.Id);

            ammunitionType = null;

            AssertQuery<Weapon>(
                ws => ws
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(w => new { w.Id, Cartidge = w.AmmunitionType == ammunitionType }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_with_boolean()
        {
            AssertQuery<Weapon>(
                ws => ws.Select(w => new { w.Id, Num = w.IsAutomatic ? 1 : 0 }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_with_inverted_boolean()
        {
            AssertQuery<Weapon>(
                ws => ws.Select(w => new { w.Id, Num = !w.IsAutomatic ? 1 : 0 }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_with_has_value_not_null()
        {
            AssertQuery<Weapon>(
                ws => ws
                    .Where(w => w.AmmunitionType.HasValue && w.AmmunitionType == AmmunitionType.Cartridge)
                    .Select(w => new { w.Id, IsCartidge = w.AmmunitionType.HasValue && w.AmmunitionType.Value == AmmunitionType.Cartridge ? "Yes" : "No" }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_multiple_conditions()
        {
            AssertQuery<Weapon>(
                ws => ws.Select(w => new { w.Id, IsCartidge = w.AmmunitionType == AmmunitionType.Shell && w.SynergyWithId == 1 ? "Yes" : "No" }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_multiple_conditions_2()
        {
            AssertQuery<Weapon>(
                ws => ws.Select(w => new { w.Id, IsCartidge = !w.IsAutomatic && w.SynergyWithId == 1 ? "Yes" : "No" }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_multiple_conditions()
        {
            AssertQuery<Weapon>(
                ws => ws.Select(w => new { w.Id, IsCartidge = !w.IsAutomatic && w.SynergyWithId == 1 }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_nested_ternary_operations()
        {
            AssertQuery<Weapon>(
                ws => ws.Select(w => new { w.Id, IsManualCartidge = !w.IsAutomatic ? w.AmmunitionType == AmmunitionType.Cartridge ? "ManualCartridge" : "Manual" : "Auto" }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization1()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (g == null ? null : g.LeaderNickname) == "Marcus" == (bool?)true));
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization2()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (g.LeaderNickname == null ? (bool?)null : (bool?)g.LeaderNickname.EndsWith("us")) == (bool?)true));
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization3()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (g.LeaderNickname != null ? (bool?)g.LeaderNickname.EndsWith("us") : (bool?)null) == (bool?)true));
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization4()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (null == EF.Property<string>(g, "LeaderNickname") ? (int?)null : g.LeaderNickname.Length) == 5 == (bool?)true),
                gs => gs.Where(g => (null == g.LeaderNickname ? (int?)null : g.LeaderNickname.Length) == 5 == (bool?)true));
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization5()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (null != g.LeaderNickname ? (int?)(EF.Property<string>(g, "LeaderNickname").Length) : (int?)null) == 5 == (bool?)true),
                gs => gs.Where(g => (null != g.LeaderNickname ? (int?)(g.LeaderNickname.Length) : (int?)null) == 5 == (bool?)true));
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization6()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => (null != g.LeaderNickname ? (int?)EF.Property<string>(g, "LeaderNickname").Length : (int?)null) == 5 == (bool?)true),
                gs => gs.Where(g => (null != g.LeaderNickname ? (int?)g.LeaderNickname.Length : (int?)null) == 5 == (bool?)true));
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_optimization7()
        {
            AssertQuery<Gear>(
                gs => gs.Select(g => null != g.LeaderNickname ? g.LeaderNickname + g.LeaderNickname : null));
        }

        [ConditionalFact(Skip = "issue #9201")]
        public virtual void Select_null_propagation_optimization8()
        {
            AssertQuery<Gear>(
                gs => gs.Select(g => g != null ? g.LeaderNickname + g.LeaderNickname : null));
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_optimization9()
        {
            AssertQueryScalar<Gear>(
                gs => gs.Select(g => g != null ? (int?)g.FullName.Length : (int?)null));
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative1()
        {
            AssertQueryScalar<Gear>(
                gs => gs.Select(g => g.LeaderNickname != null ? (bool?)(g.Nickname.Length == 5) : (bool?)null));
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative2()
        {
            AssertQuery<Gear>(
                gs => from g1 in gs
                      from g2 in gs
                      select g1.LeaderNickname != null ? g2.LeaderNickname : (string)null);
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative3()
        {
            AssertQuery<Gear>(
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select new { g2.Nickname, Condition = g2 != null ? (bool?)(g2.LeaderNickname != null) : (bool?)null },
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby Maybe(g2, () => g2.Nickname)
                      select new { Nickname = Maybe(g2, () => g2.Nickname), Condition = g2 != null ? (bool?)(g2.LeaderNickname != null) : (bool?)null },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative4()
        {
            AssertQuery<Gear>(
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select g2 != null ? new Tuple<string, int>(g2.Nickname, 5) : null,
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby Maybe(g2, () => g2.Nickname)
                      select g2 != null ? new Tuple<string, int>(g2.Nickname, 5) : null,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative5()
        {
            AssertQuery<Gear>(
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select g2 != null ? new { g2.Nickname, Five = 5 } : null,
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby Maybe(g2, () => g2.Nickname)
                      select g2 != null ? new { Nickname = Maybe(g2, () => g2.Nickname), Five = 5 } : null,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative6()
        {
            AssertQueryScalar<Gear>(
                gs => gs.Select(g => null != g.LeaderNickname ? EF.Property<string>(g, "LeaderNickname").Length != EF.Property<string>(g, "LeaderNickname").Length : (bool?)null),
                gs => gs.Select(g => null != g.LeaderNickname ? g.LeaderNickname.Length != g.LeaderNickname.Length : (bool?)null));
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative7()
        {
            AssertQueryScalar<Gear>(
                gs => gs.Select(g => null != g.LeaderNickname ? g.LeaderNickname == g.LeaderNickname : (bool?)null));
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative8()
        {
            AssertQuery<CogTag>(
                ts => ts.Select(t => t.Gear.Squad != null ? t.Gear.AssignedCity.Name : null),
                ts => ts.Select(t => Maybe(t.Gear, () => t.Gear.Squad) != null ? Maybe(t.Gear, () => Maybe(t.Gear.AssignedCity, () => t.Gear.AssignedCity.Name)) : null));
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_works_for_navigations_with_composite_keys()
        {
            AssertQuery<CogTag>(
                ts => from t in ts
#pragma warning disable IDE0031 // Use null propagation
                      select t.Gear != null ? t.Gear.Nickname : null,
#pragma warning restore IDE0031 // Use null propagation
                ts => from t in ts
                      select t.Gear != null ? Maybe(t.Gear, () => t.Gear.Nickname) : null);
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_works_for_multiple_navigations_with_composite_keys()
        {
            AssertQuery<CogTag>(
                ts => from t in ts
                      select EF.Property<City>(EF.Property<CogTag>(t.Gear, "Tag").Gear, "AssignedCity") != null
                          ? EF.Property<string>(EF.Property<Gear>(t.Gear.Tag, "Gear").AssignedCity, "Name")
                          : null,
                ts => from t in ts
                      select Maybe(t.Gear, () => Maybe(t.Gear.Tag.Gear, () => t.Gear.Tag.Gear.AssignedCity)) != null
                          ? t.Gear.Tag.Gear.AssignedCity.Name
                          : null);
        }

        [ConditionalFact]
        public virtual void Select_conditional_with_anonymous_type_and_null_constant()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      orderby g.Nickname
                      select g.LeaderNickname != null ? new { g.HasSoulPatch } : null,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_conditional_with_anonymous_types()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      orderby g.Nickname
                      select g.LeaderNickname != null ? new { Name = g.Nickname } : new { Name = g.FullName },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Where_conditional_with_anonymous_type()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      orderby g.Nickname
                      where (g.LeaderNickname != null ? new { g.HasSoulPatch } : null) == null
                      select g.Nickname,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_coalesce_with_anonymous_types()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      orderby g.Nickname
                      // ReSharper disable once ConstantNullCoalescingCondition
                      select new { Name = g.LeaderNickname } ?? new { Name = g.FullName },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Where_coalesce_with_anonymous_types()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                          // ReSharper disable once ConstantNullCoalescingCondition
                      where (new { Name = g.LeaderNickname } ?? new { Name = g.FullName }) != null
                      select g.Nickname);
        }

        [ConditionalFact(Skip = "issue #8421")]
        public virtual void Where_compare_anonymous_types()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            from o in context.Gears.OfType<Officer>()
                            where new { Name = g.LeaderNickname, Squad = g.LeaderSquadId, Five = 5 } == new { Name = o.Nickname, Squad = o.SquadId, Five = 5 }
                            select g.Nickname;

                var result = query.ToList();
                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_member_access_on_anonymous_type()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      where new { Name = g.LeaderNickname, Squad = g.LeaderSquadId }.Name == "Marcus"
                      select g.Nickname);
        }

        [ConditionalFact]
        public virtual void Where_compare_anonymous_types_with_uncorrelated_members()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                          // ReSharper disable once EqualExpressionComparison
                      where new { Five = 5 } == new { Five = 5 }
                      select g.Nickname);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation()
        {
            AssertQuery<CogTag>(
                ts => from t in ts
                      where t.Gear.Nickname == "Marcus"
                      select t,
                ts => from t in ts
                      where Maybe(t.Gear, () => t.Gear.Nickname) == "Marcus"
                      select t);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            AssertQuery<CogTag>(
                ts => from t1 in ts
                      from t2 in ts
                      where t1.Gear.Nickname == t2.Gear.Nickname
                      select new { Tag1 = t1, Tag2 = t2 },
                ts => from t1 in ts
                      from t2 in ts
                      where Maybe(t1.Gear, () => t1.Gear.Nickname) == Maybe(t2.Gear, () => t2.Gear.Nickname)
                      select new { Tag1 = t1, Tag2 = t2 },
                elementSorter: e => e.Tag1.Id + " " + e.Tag2.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.Tag1.Id, a.Tag1.Id);
                        Assert.Equal(e.Tag2.Id, a.Tag2.Id);
                    });
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            AssertQuery<CogTag>(
                ts => from t1 in ts
                      from t2 in ts
                      where t1.Gear.Nickname == t2.Gear.Nickname
                      select new { Id1 = t1.Id, Id2 = t2.Id },
                ts => from t1 in ts
                      from t2 in ts
                      where Maybe(t1.Gear, () => t1.Gear.Nickname) == Maybe(t2.Gear, () => t2.Gear.Nickname)
                      select new { Id1 = t1.Id, Id2 = t2.Id },
                elementSorter: e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            AssertSingleResult<Weapon>(
                ws => ws.OrderBy(w => w.Id).Select(w => new Weapon { IsAutomatic = (bool?)w.SynergyWith.IsAutomatic ?? false }).First(),
                ws => ws.OrderBy(w => w.Id).Select(w => new Weapon { IsAutomatic = MaybeScalar<bool>(w.SynergyWith, () => w.SynergyWith.IsAutomatic) ?? false }).First());
        }

        [ConditionalFact]
        public virtual void Where_subquery_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_firstordefault_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_first_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.OrderBy(g => g.Nickname).Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).First().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_singleordefault_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.OrderBy(g => g.Nickname).Where(g => g.HasSoulPatch && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalFact(Skip = "issue #8582")]
        public virtual void Where_subquery_distinct_lastordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .OrderBy(g => g.Nickname)
                    .Where(g => !g.Weapons.Distinct().OrderBy(w => w.Id).LastOrDefault().IsAutomatic);

                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.Equal("Baird", result[0].Nickname);
                Assert.Equal("Dom", result[1].Nickname);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Paduk", result[3].Nickname);
            }
        }

        [ConditionalFact(Skip = "issue #8582")]
        public virtual void Where_subquery_distinct_last_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .OrderBy(g => g.Nickname)
                    .Where(g => !g.HasSoulPatch && g.Weapons.Distinct().Last().IsAutomatic);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Cole Train", result[0].Nickname);
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_orderby_firstordefault_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalFact]
        public virtual void Where_subquery_union_firstordefault_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Union(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalFact]
        public virtual void Where_subquery_concat_firstordefault_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Concat(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalFact]
        public virtual void Concat_with_count()
        {
            AssertSingleResult<Gear>(
                gs => gs.Concat(gs).Count());
        }

        [ConditionalFact]
        public virtual void Concat_scalars_with_count()
        {
            AssertSingleResult<Gear>(
                gs => gs.Select(g => g.Nickname).Concat(gs.Select(g2 => g2.FullName)).Count());
        }

        [ConditionalFact]
        public virtual void Concat_anonymous_with_count()
        {
            AssertSingleResult<Gear>(
                gs => gs.Select(g => new { Gear = g, Name = g.Nickname })
                    .Concat(gs.Select(g2 => new { Gear = g2, Name = g2.FullName })).Count());
        }

        [ConditionalFact(Skip = "issue #9007")]
        public virtual void Concat_with_scalar_projection()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Concat(context.Gears).Select(g => g.Nickname);
                var result = query.ToList();

                Assert.Equal(10, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Concat_with_groupings()
        {
            AssertQuery<Gear>(
                gs => gs.GroupBy(g => g.LeaderNickname).Concat(gs.GroupBy(g => g.LeaderNickname)),
                elementSorter: GroupingSorter<string, Gear>(),
                elementAsserter: GroupingAsserter<string, Gear>(g => g.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalFact]
        public virtual void Select_navigation_with_concat_and_count()
        {
            AssertQueryScalar<Gear>(
                gs => gs.Where(g => !g.HasSoulPatch).Select(g => g.Weapons.Concat(g.Weapons).Count()));
        }

        [ConditionalFact]
        public virtual void Where_subquery_concat_order_by_firstordefault_boolean()
        {
            AssertQuery<Gear>(
                gs => gs.GroupBy(g => g.LeaderNickname).Concat(gs.GroupBy(g => g.LeaderNickname)),
                elementSorter: GroupingSorter<string, Gear>(),
                elementAsserter: GroupingAsserter<string, Gear>(g => g.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalFact]
        public virtual void Concat_with_collection_navigations()
        {
            AssertQueryScalar<Gear>(
                gs => gs.Where(g => g.HasSoulPatch).Select(g => g.Weapons.Union(g.Weapons).Count()));
        }

        [ConditionalFact]
        public virtual void Union_with_collection_navigations()
        {
            AssertQueryScalar<Gear>(
                gs => gs.OfType<Officer>().Select(o => o.Reports.Union(o.Reports).Count()));
        }

        [ConditionalFact]
        public virtual void Select_subquery_distinct_firstordefault()
        {
            AssertQuery<Gear>(
                gs => gs.Where(g => g.HasSoulPatch).Select(g => g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().Name));
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Client()
        {
            AssertQuery<CogTag>(
                ts => from t in ts
                      where t.Gear != null && t.Gear.IsMarcus
                      select t);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null()
        {
            AssertQuery<CogTag>(
                ts => from t in ts
                      where t.Gear == null
                      select t);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null_Reverse()
        {
            AssertQuery<CogTag>(
                ts => from t in ts
                      where null == t.Gear
                      select t);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Equals_Navigation()
        {
            AssertQuery<CogTag>(
                ts => from t1 in ts
                      from t2 in ts
                      where t1.Gear == t2.Gear
                      select new { t1, t2 },
                elementSorter: e => e.t1.Id + " " + e.t2.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.t1.Id, a.t1.Id);
                        Assert.Equal(e.t2.Id, a.t2.Id);
                    });
        }

        [ConditionalFact]
        public virtual void Singleton_Navigation_With_Member_Access()
        {
            AssertQuery<CogTag>(
                ts => from ct in ts
                      where ct.Gear.Nickname == "Marcus"
                      where ct.Gear.CityOrBirthName != "Ephyra"
                      select new { B = ct.Gear.CityOrBirthName },
                ts => from ct in ts
                      where Maybe(ct.Gear, () => ct.Gear.Nickname) == "Marcus"
                      where Maybe(ct.Gear, () => ct.Gear.CityOrBirthName) != "Ephyra"
                      select new { B = Maybe(ct.Gear, () => ct.Gear.CityOrBirthName) },
                elementSorter: e => e.B);
        }

        [ConditionalFact]
        public virtual void Select_Singleton_Navigation_With_Member_Access()
        {
            AssertQuery<CogTag>(
                ts => from ct in ts
                      where ct.Gear.Nickname == "Marcus"
                      where ct.Gear.CityOrBirthName != "Ephyra"
                      select new { A = ct.Gear, B = ct.Gear.CityOrBirthName },
                ts => from ct in ts
                      where Maybe(ct.Gear, () => ct.Gear.Nickname) == "Marcus"
                      where Maybe(ct.Gear, () => ct.Gear.CityOrBirthName) != "Ephyra"
                      select new { A = ct.Gear, B = Maybe(ct.Gear, () => ct.Gear.CityOrBirthName) },
                elementSorter: e => e.A.Nickname,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.A.Nickname, e.A.Nickname);
                        Assert.Equal(e.B, e.B);
                    });
        }

        [ConditionalFact]
        public virtual void GroupJoin_Composite_Key()
        {
            AssertQuery<CogTag, Gear>(
                (ts, gs) =>
                    from t in ts
                    join g in gs
                        on new { N = t.GearNickName, S = t.GearSquadId }
                        equals new { N = g.Nickname, S = (int?)g.SquadId } into grouping
                    from g in grouping
                    select g);
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_composite_key()
        {
            AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from g in gs
                    join t in ts on g.FullName equals t.Gear.FullName
                    select new { g.FullName, t.Note },
                (gs, ts) =>
                    from g in gs
                    join t in ts on g.FullName equals Maybe(t.Gear, () => t.Gear.FullName)
                    select new { g.FullName, t.Note },
                elementSorter: e => e.FullName);
        }

        [ConditionalFact]
        public virtual void Collection_with_inheritance_and_join_include_joined()
        {
            AssertIncludeQuery<CogTag, Gear>(
                (ts, gs) =>
                    (from t in ts
                     join g in gs.OfType<Officer>() on new { id1 = t.GearSquadId, id2 = t.GearNickName }
                         equals new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                     select g).Include(g => g.Tag),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Tag, "Tag") });
        }

        [ConditionalFact]
        public virtual void Collection_with_inheritance_and_join_include_source()
        {
            AssertIncludeQuery<Gear, CogTag>(
                (gs, ts) =>
                    (from g in gs.OfType<Officer>()
                     join t in ts on new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                         equals new { id1 = t.GearSquadId, id2 = t.GearNickName }
                     select g).Include(g => g.Tag),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Tag, "Tag") });
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literal_is_used_for_non_unicode_column()
        {
            AssertQuery<City>(
                cs => from c in cs
                      where c.Location == "Unknown"
                      select c);
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literal_is_used_for_non_unicode_column_right()
        {
            AssertQuery<City>(
                cs => from c in cs
                      where "Unknown" == c.Location
                      select c);
        }

        [ConditionalFact]
        public virtual void Non_unicode_parameter_is_used_for_non_unicode_column()
        {
            var value = "Unknown";

            AssertQuery<City>(
                cs => from c in cs
                      where c.Location == value
                      select c);
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column()
        {
            var cities = new List<string> { "Unknown", "Jacinto's location", "Ephyra's location" };

            AssertQuery<City>(
                cs => from c in cs
                      where cities.Contains(c.Location)
                      select c);
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery()
        {
            AssertQuery<City>(
                cs => from c in cs
                      where c.Location == "Unknown" && c.BornGears.Count(g => g.Nickname == "Paduk") == 1
                      select c);
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname == "Marcus" && g.CityOfBirth.Location == "Jacinto's location"
                      select g);
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains()
        {
            AssertQuery<City>(
                cs => from c in cs
                      where c.Location.Contains("Jacinto")
                      select c);
        }

        [ConditionalFact(Skip = "Test does not pass. See issue#4978")]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Cities
                            where (c.Location + "Added").Contains("Add")
                            select c;

                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears.Include(g => g.Weapons)
                            join g2 in context.Gears
                                on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select g2 ?? g1;

                var result = query.ToList();

                Assert.Equal("Marcus", result[0].Nickname);
                Assert.Equal(2, result[0].Weapons.Count);
                Assert.Equal("Marcus", result[1].Nickname);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Baird", result[3].Nickname);
                Assert.Equal(0, result[3].Weapons.Count);
                Assert.Equal("Marcus", result[4].Nickname);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears
                            join g2 in context.Gears.Include(g => g.Weapons)
                                on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select g2 ?? g1;

                var result = query.ToList();

                Assert.Equal("Marcus", result[0].Nickname);
                Assert.Equal(2, result[0].Weapons.Count);
                Assert.Equal("Baird", result[1].Nickname);
                Assert.Equal(2, result[1].Weapons.Count);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Marcus", result[3].Nickname);
                Assert.Equal("Marcus", result[4].Nickname);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear>(
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      select g2 ?? g1,
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear>(
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.OfType<Officer>().Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      select g2 ?? g1,
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear>(
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                          // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                      select g2 != null ? g2 : g1,
#pragma warning restore IDE0029 // Use coalesce expression
                expectedIncludes);
        }

        [ConditionalFact(Skip = "issue #9256")]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear>(
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                          // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                      select new { g1, g2, coalesce = g2 ?? g1, conditional = g2 != null ? g2 : g1 },
#pragma warning restore IDE0029 // Use coalesce expression
                expectedIncludes,
                elementSorter: e => e.g1.Nickname + " " + e.g2?.Nickname,
                clientProjections: new List<Func<dynamic, object>> { e => e.g1, e => e.g2, e => e.coalesce, e => e.conditional });
        }

        [ConditionalFact]
        public virtual void Coalesce_operator_in_predicate()
        {
            AssertQuery<Weapon>(
                ws => ws.Where(w => (bool?)w.IsAutomatic ?? false));
        }

        [ConditionalFact]
        public virtual void Coalesce_operator_in_predicate_with_other_conditions()
        {
            AssertQuery<Weapon>(
                ws => ws.Where(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));
        }

        [ConditionalFact]
        public virtual void Coalesce_operator_in_projection_with_other_conditions()
        {
            AssertQueryScalar<Weapon>(
                ws => ws.Select(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A." && t.Gear.HasSoulPatch));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate2()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Gear.HasSoulPatch),
                ts => ts.Where(t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate_negated()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => !t.Gear.HasSoulPatch),
                ts => ts.Where(t => !MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true));
        }

        [ConditionalFact(Skip = "issue #9254")]
        public virtual void Optional_navigation_type_compensation_works_with_predicate_negated_complex1()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => !(t.Gear.HasSoulPatch ? true : t.Gear.HasSoulPatch)));
        }

        [ConditionalFact(Skip = "issue #9254")]
        public virtual void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => !(!t.Gear.HasSoulPatch ? false : t.Gear.HasSoulPatch)));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_conditional_expression()
        {
            AssertQuery<CogTag>(
                // ReSharper disable once RedundantTernaryExpression
                ts => ts.Where(t => t.Gear.HasSoulPatch ? true : false),
                // ReSharper disable once RedundantTernaryExpression
                ts => ts.Where(t => (MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true) ? true : false));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_binary_expression()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Gear.HasSoulPatch || t.Note.Contains("Cole")),
                ts => ts.Where(t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true || t.Note.Contains("Cole")));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_projection()
        {
            AssertQueryScalar<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").Select(t => t.Gear.SquadId));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_projection_into_anonymous_type()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").Select(t => new { t.Gear.SquadId }),
                elementSorter: e => e.SquadId);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_DTOs()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").Select(t => new Squad { Id = t.Gear.SquadId }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_list_initializers()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note).Select(t => new List<int> { t.Gear.SquadId, t.Gear.SquadId + 1, 42 }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_array_initializers()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").Select(t => new[] { t.Gear.SquadId }),
                elementSorter: e => e[0]);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_orderby()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Gear.SquadId));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_groupby()
        {
            AssertQuery<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").GroupBy(t => t.Gear.SquadId),
                elementSorter: GroupingSorter<int, CogTag>(),
                elementAsserter: GroupingAsserter<int, CogTag>(t => t.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_all()
        {
            AssertSingleResult<CogTag>(
                ts => ts.Where(t => t.Note != "K.I.A.").All(t => t.Gear.HasSoulPatch));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_contains()
        {
            AssertQuery<CogTag, Gear>(
                (ts, gs) => ts.Where(t => t.Note != "K.I.A." && gs.Select(g => g.SquadId).Contains(t.Gear.SquadId)));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_skip()
        {
            AssertQuery<CogTag, Gear>(
                (ts, gs) => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note).Select(t => gs.OrderBy(g => g.Nickname).Skip(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_take()
        {
            AssertQuery<CogTag, Gear>(
                (ts, gs) => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note).Select(t => gs.OrderBy(g => g.Nickname).Take(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalFact]
        public virtual void Select_correlated_filtered_collection()
        {
            AssertQuery<Gear>(
                gs => gs
                    .Where(g => g.CityOfBirth.Name == "Ephyra" || g.CityOfBirth.Name == "Hanover")
                    .OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.Where(w => w.Name != "Lancer").ToList()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalFact]
        public virtual void Select_correlated_filtered_collection_with_composite_key()
        {
            AssertQuery<Gear>(
                gs => gs.OfType<Officer>().OrderBy(g => g.Nickname).Select(g => g.Reports.Where(r => r.Nickname != "Dom").ToList()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalFact]
        public virtual void Select_correlated_filtered_collection_works_with_caching()
        {
            AssertQuery<CogTag, Gear>(
                (ts, gs) => ts.OrderBy(t => t.Note).Select(t => gs.Where(g => g.Nickname == t.GearNickName)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(g => g.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalFact]
        public virtual void Join_predicate_value_equals_condition()
        {
            AssertQuery<Gear, Weapon>(
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on true equals w.SynergyWithId != null
                    select g);
        }

        [ConditionalFact]
        public virtual void Join_predicate_value()
        {
            AssertQuery<Gear, Weapon>(
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.HasSoulPatch equals true
                    select g);
        }

        [ConditionalFact]
        public virtual void Join_predicate_condition_equals_condition()
        {
            AssertQuery<Gear, Weapon>(
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.FullName != null equals w.SynergyWithId != null
                    select g);
        }

        [ConditionalFact]
        public virtual void Left_join_predicate_value_equals_condition()
        {
            AssertQuery<Gear, Weapon>(
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on true equals w.SynergyWithId != null
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalFact]
        public virtual void Left_join_predicate_value()
        {
            AssertQuery<Gear, Weapon>(
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.HasSoulPatch equals true
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalFact]
        public virtual void Left_join_predicate_condition_equals_condition()
        {
            AssertQuery<Gear, Weapon>(
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.FullName != null equals w.SynergyWithId != null
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_now()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline != DateTimeOffset.Now
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_utcnow()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline != DateTimeOffset.UtcNow
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_date_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Date > new DateTimeOffset().Date
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_year_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Year == 2
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_month_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Month == 1
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_dayofyear_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.DayOfYear == 2
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_day_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Day == 2
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_hour_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Hour == 10
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_minute_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Minute == 0
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_second_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Second == 0
                      select m);
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_millisecond_component()
        {
            AssertQuery<Mission>(
                ms => from m in ms
                      where m.Timeline.Millisecond == 0
                      select m);
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddYears()
        {
            AssertQueryScalar<Mission>(
                ms => from m in ms
                      select m.Timeline.AddYears(1));
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddMonths()
        {
            AssertQueryScalar<Mission>(
                ms => from m in ms
                      select m.Timeline.AddMonths(1));
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddDays()
        {
            AssertQueryScalar<Mission>(
                ms => from m in ms
                      select m.Timeline.AddDays(1));
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddHours()
        {
            AssertQueryScalar<Mission>(
                ms => from m in ms
                      select m.Timeline.AddHours(1));
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddMinutes()
        {
            AssertQueryScalar<Mission>(
                ms => from m in ms
                      select m.Timeline.AddMinutes(1));
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddSeconds()
        {
            AssertQueryScalar<Mission>(
                ms => from m in ms
                      select m.Timeline.AddSeconds(1));
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddMilliseconds()
        {
            AssertQueryScalar<Mission>(
                ms => from m in ms
                      select m.Timeline.AddMilliseconds(300));
        }

        [ConditionalFact]
        public virtual void Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used()
        {
            AssertQuery<CogTag, Gear>(
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { t.Note, Nickname = g != null ? g.Nickname : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Note);
        }

        private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        {
            // ReSharper disable PossibleMultipleEnumeration
            return source?.Count() == 0 ? new[] { default(TElement) } : source;
            // ReSharper restore PossibleMultipleEnumeration
        }

        [ConditionalFact]
        public virtual void Complex_predicate_with_AndAlso_and_nullable_bool_property()
        {
            AssertQuery<Weapon>(
                ws => from w in ws
                      where w.Id != 50 && !w.Owner.HasSoulPatch
                      select w,
                ws => from w in ws
                      where w.Id != 50 && MaybeScalar<bool>(w.Owner, () => w.Owner.HasSoulPatch) == false
                      select w);
        }

        [ConditionalFact]
        public virtual void Distinct_with_optional_navigation_is_translated_to_sql()
        {
            AssertQueryScalar<Gear>(
                gs => (from g in gs
                       where g.Tag.Note != "Foo"
                       select g.HasSoulPatch).Distinct(),
                gs => (from g in gs
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.HasSoulPatch).Distinct());
        }

        [ConditionalFact]
        public virtual void Sum_with_optional_navigation_is_translated_to_sql()
        {
            AssertSingleResult<Gear>(
                gs => (from g in gs
                       where g.Tag.Note != "Foo"
                       select g.SquadId).Sum(),
                gs => (from g in gs
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.SquadId).Sum());
        }

        [ConditionalFact]
        public virtual void Count_with_optional_navigation_is_translated_to_sql()
        {
            AssertSingleResult<Gear>(
                gs => (from g in gs
                       where g.Tag.Note != "Foo"
                       select g.HasSoulPatch).Count(),
                gs => (from g in gs
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.HasSoulPatch).Count());
        }

        [ConditionalFact]
        public virtual void Distinct_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            AssertQueryScalar<Gear, CogTag>(
                (gs, ts) => gs.GroupJoin(
                        ts,
                        g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                        t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                        (g, t) => g.HasSoulPatch)
                    .Distinct());
        }

        [ConditionalFact]
        public virtual void Count_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            AssertSingleResult<Gear, CogTag>(
                (gs, ts) => gs
                    .GroupJoin(
                        ts,
                        g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                        t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                        (g, t) => g)
                    .Count());
        }

        [ConditionalFact]
        public virtual void FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql()
        {
            AssertSingleResult<Squad, Gear>(
                (ss, gs) =>
                    (from s in ss
                     join g in gs on s.Id equals g.SquadId into grouping
                     from g in grouping.DefaultIfEmpty()
                     where s.Name == "Kilo"
                     select s).FirstOrDefault());
        }

        [ConditionalFact]
        public virtual void Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql()
        {
            AssertQuery<Squad>(
                ss => from s in ss
                      where !s.Members.Any(m => m.Tag.Note == "Dom's Tag")
                      select s.Name);
        }

        [ConditionalFact]
        public virtual void All_with_optional_navigation_is_translated_to_sql()
        {
            AssertSingleResult<Gear>(
                gs => (from g in gs
                       select g).All(g => g.Tag.Note != "Foo"));
        }

        [ConditionalFact]
        public virtual void Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client()
        {
            AssertQueryScalar<CogTag, Gear>(
                (ts, gs) => ts.GroupJoin(
                    gs,
                    t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                    g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                    (k, r) => r.Count()));
        }

        [ConditionalFact]
        public virtual void Client_side_equality_with_parameter_works_with_optional_navigations()
        {
            var prm = "Marcus' Tag";

            AssertQuery<Gear>(
                gs => gs.Where(g => ClientEquals(g.Tag.Note, prm)),
                elementAsserter: (e, a) => Assert.Equal(e.Nickname, a.Nickname));
        }

        private static bool ClientEquals(string first, string second)
        {
            return first == second;
        }

        [ConditionalFact]
        public virtual void Contains_with_local_nullable_guid_list_closure()
        {
            var ids = new List<Guid?>
            {
                Guid.Parse("D2C26679-562B-44D1-AB96-23D1775E0926"),
                Guid.Parse("23CBCF9B-CE14-45CF-AAFA-2C2667EBFDD3"),
                Guid.Parse("AB1B82D7-88DB-42BD-A132-7EEF9AA68AF4")
            };

            AssertQuery<CogTag>(
                ts => ts.Where(e => ids.Contains(e.Id)));
        }

        [ConditionalFact]
        public virtual void Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .Where(g => g.HasSoulPatch)
                    .Select(g => new { FullName = EF.Property<string>(g, "FullName") });

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Damon Baird", result[0].FullName);
                Assert.Equal("Marcus Fenix", result[1].FullName);
            }
        }

        [ConditionalFact]
        public virtual void Order_by_is_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Order_by_then_by_is_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank).ThenByDescending(g => g.Nickname)
                    .Include(g => g.Tag)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .OrderByDescending(g => g.Nickname)
                    .Include(g => g.CityOfBirth)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.FullName)
                    .Include(g => g.CityOfBirth)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Where_is_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .Where(g => g.FullName != "Augustus Cole")
                    .Include(g => g.Tag)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g);

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Dominic Santiago", result[0].FullName);
                Assert.Equal("Dom's Tag", result[0].Tag.Note);
                Assert.Equal("Garron Paduk", result[1].FullName);
                Assert.Equal("Paduk's Tag", result[1].Tag.Note);
            }
        }

        [ConditionalFact]
        public virtual void Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking()
        {
            AssertQuery<Gear>(
                gs => gs
                    .Where(g => g.FullName != "Augustus Cole")
                    .AsNoTracking()
                    .OrderBy(g => g.Rank)
                    .AsTracking()
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Subquery_is_lifted_from_main_from_clause_of_SelectMany()
        {
            using (var ctx = CreateContext())
            {
                var query = from g1 in ctx.Gears.OrderBy(g => g.Rank).Include(g => g.Tag)
                            from g2 in ctx.Gears
                            orderby g1.FullName
                            where g1.HasSoulPatch && !g2.HasSoulPatch
                            select new { Name1 = g1.FullName, Name2 = g2.FullName };

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.True(result.All(r => r.Name1 == "Damon Baird" || r.Name1 == "Marcus Fenix"));
                Assert.True(result.All(r => r.Name2 == "Augustus Cole" || r.Name2 == "Garron Paduk" || r.Name2 == "Dominic Santiago"));
            }
        }

        [ConditionalFact]
        public virtual void Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted()
        {
            AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from g in (from gear in gs
                               from tag in ts
                               where gear.HasSoulPatch
                               orderby tag.Note
                               select gear).AsTracking()
                    orderby g.FullName
                    select g.FullName,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Subquery_containing_join_projecting_main_from_clause_gets_lifted()
        {
            AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from g in (from gear in gs
                               join tag in ts on gear.Nickname equals tag.GearNickName
                               orderby tag.Note
                               select gear).AsTracking()
                    orderby g.Nickname
                    select g.Nickname,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Subquery_containing_left_join_projecting_main_from_clause_gets_lifted()
        {
            AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from g in (from gear in gs
                               join tag in ts on gear.Nickname equals tag.GearNickName into grouping
                               from tag in grouping.DefaultIfEmpty()
                               orderby gear.Rank
                               select gear).AsTracking()
                    orderby g.Nickname
                    select g.Nickname,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Subquery_containing_join_gets_lifted_clashing_names()
        {
            AssertQuery<Gear, CogTag>(
                (gs, ts) =>
                    from gear in (from gear in gs
                                  join tag in ts on gear.Nickname equals tag.GearNickName
                                  orderby tag.Note
                                  where tag.GearNickName != "Cole Train"
                                  select gear).AsTracking()
                    join tag in ts on gear.Nickname equals tag.GearNickName
                    orderby gear.Nickname, tag.Id
                    select gear.Nickname,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Subquery_created_by_include_gets_lifted_nested()
        {
            using (var ctx = CreateContext())
            {
                var query = from gear in ctx.Gears.OrderBy(g => g.Rank).Where(g => g.Weapons.Any()).Include(g => g.CityOfBirth)
                            where !gear.HasSoulPatch
                            orderby gear.Nickname
                            select gear;

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0].FullName);
                Assert.Equal("Hanover", result[0].CityOfBirth.Name);
                Assert.Equal("Dominic Santiago", result[1].FullName);
                Assert.Equal("Ephyra", result[1].CityOfBirth.Name);
                Assert.Equal("Garron Paduk", result[2].FullName);
                Assert.Equal("Unknown", result[2].CityOfBirth.Name);
            }
        }

        [ConditionalFact]
        public virtual void Subquery_is_not_lifted_from_additional_from_clause()
        {
            AssertQuery<Gear>(
                gs =>
                    from g1 in gs
                    from g2 in gs.OrderBy(g => g.Rank).Include(g => g.Tag)
                    orderby g1.FullName
                    where g1.HasSoulPatch && !g2.HasSoulPatch
                    select new { Name1 = g1.FullName, Name2 = g2.FullName },
                elementSorter: e => e.Name1 + " " + e.Name2);
        }

        [ConditionalFact]
        public virtual void Subquery_with_result_operator_is_not_lifted()
        {
            AssertQuery<Gear>(
                gs => from g in gs.Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Take(2).AsTracking()
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_length_of_string_property()
        {
            AssertQuery<Weapon>(
                ws => from w in ws
                      select new { w.Name, w.Name.Length },
                elementSorter: e => e.Name);
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_predicate()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      where g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Marcus' Lancer"
                      select g.Nickname);
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      where !g.HasSoulPatch && FavoriteWeapon(EF.Property<List<Weapon>>(g, "Weapons")).Name == "Cole's Gnasher"
                      select g.Nickname,
                gs => from g in gs
                      where !g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Cole's Gnasher"
                      select g.Nickname);
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_order_by()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      where !g.HasSoulPatch
                      orderby FavoriteWeapon(g.Weapons).Name descending
                      select g.Nickname,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_additional_from_clause()
        {
            AssertQuery<Gear>(
                gs => from g in gs.OfType<Officer>()
                      from v in Veterans(g.Reports)
                      select new { g = g.Nickname, v = v.Nickname },
                elementSorter: e => e.g + e.v);
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_outer_join_key()
        {
            AssertQuery<Gear>(
                gs => from o in gs.OfType<Officer>()
                      join g in gs on FavoriteWeapon(o.Weapons).Name equals FavoriteWeapon(g.Weapons).Name
                      where o.HasSoulPatch
                      select new { o = o.Nickname, g = g.Nickname },
                elementSorter: e => e.o + e.g);
        }

        private static Weapon FavoriteWeapon(IEnumerable<Weapon> weapons)
        {
            return weapons.OrderBy(w => w.Id).FirstOrDefault();
        }

        private static IEnumerable<Gear> Veterans(IEnumerable<Gear> gears)
        {
            return gears.Where(g => g.Nickname == "Marcus" || g.Nickname == "Dom" || g.Nickname == "Cole Train" || g.Nickname == "Baird");
        }

        [ConditionalFact]
        public virtual void Member_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby ((LocustHorde)f).Name
                            select new { ((LocustHorde)f).Name, ((LocustHorde)f).Eradicated };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Member_access_on_derived_materialized_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f, ((LocustHorde)f).Eradicated };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].f.Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].f.Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Member_access_on_derived_entity_using_cast_and_let()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            let horde = (LocustHorde)f
                            orderby horde.Name
                            select new { horde.Name, horde.Eradicated };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Property_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            let horde = (LocustHorde)f
                            orderby f.Name
                            select new { Name = EF.Property<string>(horde, "Name"), Eradicated = EF.Property<bool>((LocustHorde)f, "Eradicated") };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, Threat = ((LocustHorde)f).Commander.ThreatLevel };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal(5, result[0].Threat);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal(0, result[1].Threat);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_access_on_derived_materialized_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f, f.Name, Threat = ((LocustHorde)f).Commander.ThreatLevel };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Locust", result[0].f.Name);
                Assert.Equal(5, result[0].Threat);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal("Swarm", result[1].f.Name);
                Assert.Equal(0, result[1].Threat);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_access_via_EFProperty_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, Threat = EF.Property<LocustCommander>((LocustHorde)f, "Commander").ThreatLevel };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal(5, result[0].Threat);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal(0, result[1].Threat);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_access_fk_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, CommanderName = ((LocustHorde)f).Commander.Name };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Queen Myrrah", result[0].CommanderName);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal("Unknown", result[1].CommanderName);
            }
        }

        [ConditionalFact]
        public virtual void Collection_navigation_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, LeadersCount = ((LocustHorde)f).Leaders.Count };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal(4, result[0].LeadersCount);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal(2, result[1].LeadersCount);
            }
        }

        [ConditionalFact]
        public virtual void Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions.Where(f => f is LocustHorde)
                            from l in ((LocustHorde)f).Leaders
                            orderby l.Name
                            select new { f.Name, LeaderName = l.Name };

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Locust", result[1].Name);
                Assert.Equal("Locust", result[2].Name);
                Assert.Equal("Locust", result[3].Name);
                Assert.Equal("Swarm", result[4].Name);
                Assert.Equal("General Karn", result[0].LeaderName);
                Assert.Equal("General RAAM", result[1].LeaderName);
                Assert.Equal("High Priest Skorge", result[2].LeaderName);
                Assert.Equal("Queen Myrrah", result[3].LeaderName);
                Assert.Equal("The Speaker", result[4].LeaderName);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_OfType()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in ctx.Factions.OfType<LocustHorde>().Include(h => h.Commander).Include(h => h.Leaders)
                            orderby lh.Name
                            select lh;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Queen Myrrah", result[0].Commander.Name);
                Assert.Equal(4, result[0].Leaders.Count);
                Assert.Equal("Unknown", result[1].Commander.Name);
                Assert.Equal(2, result[1].Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_subquery_with_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in (from f in ctx.Factions
                                        where f is LocustHorde
                                        select (LocustHorde)f).Include(h => h.Commander).Include(h => h.Leaders)
                            orderby lh.Name
                            select lh;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Queen Myrrah", result[0].Commander.Name);
                Assert.Equal(4, result[0].Leaders.Count);
                Assert.Equal("Unknown", result[1].Commander.Name);
                Assert.Equal(2, result[1].Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_subquery_with_cast_AsNoTracking()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in (from f in ctx.Factions
                                        where f is LocustHorde
                                        select (LocustHorde)f).AsNoTracking().Include(h => h.Commander).Include(h => h.Leaders)
                            orderby lh.Name
                            select lh;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Queen Myrrah", result[0].Commander.Name);
                Assert.Equal(4, result[0].Leaders.Count);
                Assert.Equal("Unknown", result[1].Commander.Name);
                Assert.Equal(2, result[1].Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in (from f2 in ctx.Factions
                                        where f2 is LocustHorde
                                        select (LocustHorde)f2).Include(h => h.Commander).Include(h => h.Leaders)
                            from f in ctx.Factions.Include(ff => ff.Capital)
                            orderby lh.Name, f.Name
                            select new { lh, f };

                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.Equal("Queen Myrrah", result[0].lh.Commander.Name);
                Assert.Equal(4, result[0].lh.Leaders.Count);
                Assert.Equal("Unknown", result[2].lh.Commander.Name);
                Assert.Equal(2, result[2].lh.Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Distinct_on_subquery_doesnt_get_lifted()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in (from ig in ctx.Gears
                                       select ig).Distinct()
                            select g.HasSoulPatch;

                var result = query.ToList();
                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in (from f in ctx.Factions
                                        select f).Cast<LocustHorde>()
                            select lh.Eradicated;

                var result = query.ToList();
                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Comparing_two_collection_navigations_composite_key()
        {
            using (var ctx = CreateContext())
            {
                var query = from g1 in ctx.Gears
                            from g2 in ctx.Gears
                                // ReSharper disable once PossibleUnintendedReferenceComparison
                            where g1.Weapons == g2.Weapons
                            orderby g1.Nickname
                            select new { Nickname1 = g1.Nickname, Nickname2 = g2.Nickname };

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.Equal(5, result.Count(r => r.Nickname1 == r.Nickname2));
            }
        }

        [ConditionalFact]
        public virtual void Comparing_two_collection_navigations_inheritance()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            from o in ctx.Gears.OfType<Officer>()
                            where f is LocustHorde && o.HasSoulPatch
                            // ReSharper disable once PossibleUnintendedReferenceComparison
                            where ((LocustHorde)f).Commander.DefeatedBy.Weapons == o.Weapons
                            select new { f.Name, o.Nickname };

                var result = query.ToList();
                Assert.Equal(1, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Marcus", result[0].Nickname);
            }
        }

        [ConditionalFact(Skip = "issue #8375")]
        public virtual void Comparing_entities_using_Equals_inheritance()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in ctx.Gears
                            from o in ctx.Gears.OfType<Officer>()
                            where g.Equals(o)
                            orderby g.Nickname
                            select new { Nickname1 = g.Nickname, Nickname2 = o.Nickname };

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal("Baird", result[0].Nickname1);
                Assert.Equal("Baird", result[0].Nickname2);
                Assert.Equal("Marcus", result[1].Nickname1);
                Assert.Equal("Marcus", result[1].Nickname2);
            }
        }

        [ConditionalFact]
        public virtual void Contains_on_nullable_array_produces_correct_sql()
        {
            using (var context = CreateContext())
            {
                var cities = new[] { "Ephyra", null };
                var query = context.Gears.Where(g => g.SquadId < 2 && cities.Contains(g.AssignedCity.Name)).ToList();

                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_with_collection_composite_key()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Gear is Officer && ((Officer)t.Gear).Reports.Count(r => r.Nickname == "Dom") > 0);
                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Marcus' Tag", result[0].Note);
            }
        }

        [ConditionalFact]
        public virtual void Select_null_conditional_with_inheritance()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions
                    .Where(f => f is LocustHorde)
                    .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).CommanderName : null);

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.True(result.Contains("Queen Myrrah"));
                Assert.True(result.Contains("Unknown"));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_conditional_with_inheritance_negative()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions
                    .Where(f => f is LocustHorde)
                    .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).Eradicated : null);

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.True(result.Contains(true));
                Assert.True(result.Contains(false));
            }
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_with_inheritance1()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions.OfType<LocustHorde>()
                    .Select(
                        h => new
                        {
                            h.Id,
                            Leaders = EF.Property<ICollection<LocustLeader>>(h.Commander.CommandingFaction, "Leaders")
                        });

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal(1, result.Count(r => r.Id == 1 && r.Leaders.Count == 4));
                Assert.Equal(1, result.Count(r => r.Id == 2 && r.Leaders.Count == 2));
            }
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_with_inheritance2()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions.OfType<LocustHorde>()
                    .Select(
                        h => new
                        {
                            h.Id,
                            Gears = EF.Property<ICollection<Gear>>((Officer)h.Commander.DefeatedBy, "Reports")
                        });

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal(1, result.Count(r => r.Id == 1 && r.Gears.Count == 3));
                Assert.Equal(1, result.Count(r => r.Id == 2 && r.Gears.Count == 0));
            }
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_with_inheritance3()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions
                    .Where(f => f is LocustHorde)
                    .Select(
                        f => new
                        {
                            f.Id,
                            Gears = EF.Property<ICollection<Gear>>((Officer)((LocustHorde)f).Commander.DefeatedBy, "Reports")
                        });

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal(1, result.Count(r => r.Id == 1 && r.Gears.Count == 3));
                Assert.Equal(1, result.Count(r => r.Id == 2 && r.Gears.Count == 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_reference_on_derived_type_using_string()
        {
            AssertIncludeQuery<LocustLeader>(
                lls => lls.Include("DefeatedBy"),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalFact]
        public virtual void Include_reference_on_derived_type_using_string_nested1()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "DefeatedBy"),
            };

            AssertIncludeQuery<LocustLeader>(
                lls => lls.Include("DefeatedBy.Squad"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_reference_on_derived_type_using_string_nested2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy"),
                new ExpectedInclude<Officer>(o => o.Reports, "Reports", "DefeatedBy"),
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth", "DefeatedBy.Reports"),
            };

            AssertIncludeQuery<LocustLeader>(
                lls => lls.Include("DefeatedBy.Reports.CityOfBirth"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_reference_on_derived_type_using_lambda()
        {
            AssertIncludeQuery<LocustLeader>(
                lls => lls.Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalFact]
        public virtual void Include_reference_on_derived_type_using_lambda_with_soft_cast()
        {
            AssertIncludeQuery<LocustLeader>(
                lls => lls.Include(ll => (ll as LocustCommander).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalFact]
        public virtual void Include_reference_on_derived_type_using_lambda_with_tracking()
        {
            AssertIncludeQuery<LocustLeader>(
                lls => lls.AsTracking().Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") },
                entryCount: 7);
        }

        [ConditionalFact]
        public virtual void Include_collection_on_derived_type_using_string()
        {
            AssertIncludeQuery<Gear>(
                gs => gs.Include("Reports"),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalFact]
        public virtual void Include_collection_on_derived_type_using_lambda()
        {
            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Reports),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalFact]
        public virtual void Include_collection_on_derived_type_using_lambda_with_soft_cast()
        {
            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => (g as Officer).Reports),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalFact]
        public virtual void Include_base_navigation_on_derived_entity()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Tag, "Tag"),
                new ExpectedInclude<Officer>(e => e.Weapons, "Weapons")
            };

            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Tag).Include(g => ((Officer)g).Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void ThenInclude_collection_on_derived_after_base_reference()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(e => e.Gear, "Gear"),
                new ExpectedInclude<Officer>(e => e.Weapons, "Weapons", "Gear")
            };

            AssertIncludeQuery<CogTag>(
                ts => ts.Include(t => t.Gear).ThenInclude(g => (g as Officer).Weapons),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void ThenInclude_collection_on_derived_after_derived_reference()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Commander, "Commander"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Commander"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Commander.DefeatedBy"),
            };

            AssertIncludeQuery<Faction>(
                fs => fs.Include(f => (f as LocustHorde).Commander).ThenInclude(c => (c.DefeatedBy as Officer).Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void ThenInclude_collection_on_derived_after_derived_collection()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Reports, "Reports"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Reports"),
            };

            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Reports).ThenInclude(g => ((Officer)g).Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void ThenInclude_reference_on_derived_after_derived_collection()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Leaders, "Leaders"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Leaders")
            };

            AssertIncludeQuery<Faction>(
                fs => fs.Include(f => ((LocustHorde)f).Leaders).ThenInclude(l => ((LocustCommander)l).DefeatedBy),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Multiple_derived_included_on_one_method()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Commander, "Commander"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Commander"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Commander.DefeatedBy" )
            };

            AssertIncludeQuery<Faction>(
                fs => fs.Include(f => (((LocustHorde)f).Commander.DefeatedBy as Officer).Reports),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_on_derived_multi_level()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Reports, "Reports"),
                new ExpectedInclude<Gear>(e => e.Squad, "Squad", "Reports"),
                new ExpectedInclude<Squad>(e => e.Missions, "Missions", "Reports.Squad")
            };

            AssertIncludeQuery<Gear>(
                gs => gs.Include(g => ((Officer)g).Reports).ThenInclude(g => g.Squad.Missions),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_collection_and_invalid_navigation_using_string_throws()
        {
            Assert.Equal(
                CoreStrings.IncludeBadNavigation("Foo", "Gear"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        {
                            using (var context = CreateContext())
                            {
                                var query = context.Gears
                                    .Include("Reports.Foo")
                                    .ToList();
                            }
                        }).Message);
        }

        [ConditionalFact]
        public virtual void Projecting_nullable_bool_in_conditional_works()
        {
            AssertQuery<CogTag>(
                cgs =>
                    cgs.Select(
                        cg =>
                            new
                            {
                                Prop = cg.Gear != null ? cg.Gear.HasSoulPatch : false
                            }),
                e => e.Prop);
        }

        [ConditionalFact]
        public virtual void Enum_ToString_is_client_eval()
        {
            AssertQuery<Gear>(
                gs =>
                    gs.OrderBy(g => g.SquadId)
                        .ThenBy(g => g.Nickname)
                        .Select(g => g.Rank.ToString()));
        }

        [ConditionalFact]
        public virtual void Correlated_collections_basic_projection()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_basic_projection_explicit_to_list()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_basic_projection_explicit_to_array()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_basic_projection_ordered()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_basic_projection_composite_key()
        {
            AssertQuery<Gear>(gs =>
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
        public virtual void Correlated_collections_basic_projecting_single_property()
        {
            AssertQuery<Gear>(
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w.Name).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<string>());
        }

        [ConditionalFact]
        public virtual void Correlated_collections_basic_projecting_constant()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_projection_of_collection_thru_navigation()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_project_anonymous_collection_result()
        {
            AssertQuery<Squad>(
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
        public virtual void Correlated_collections_nested()
        {
            AssertQuery<Squad>(
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
        public virtual void Correlated_collections_nested_mixed_streaming_with_buffer1()
        {
            AssertQuery<Squad>(
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
        public virtual void Correlated_collections_nested_mixed_streaming_with_buffer2()
        {
            AssertQuery<Squad>(
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
        public virtual void Correlated_collections_nested_with_custom_ordering()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_same_collection_projected_multiple_times()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_similar_collection_projected_multiple_times()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_different_collections_projected()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_multiple_nested_complex_collections()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_inner_subquery_selector_references_outer_qsre()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_inner_subquery_predicate_references_outer_qsre()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_on_select_many()
        {
            AssertQuery<Gear, Squad>(
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
        public virtual void Correlated_collections_with_Skip()
        {
            AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Skip(1)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a);
                });
        }

        [ConditionalFact]
        public virtual void Correlated_collections_with_Take()
        {
            AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Take(2)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a);
                });
        }

        [ConditionalFact]
        public virtual void Correlated_collections_with_Distinct()
        {
            AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Distinct()),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a);
                });
        }

        [ConditionalFact]
        public virtual void Correlated_collections_with_FirstOrDefault()
        {
            AssertQuery<Squad>(
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Select(m => m.FullName).FirstOrDefault()),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname));
                });
        }

        [ConditionalFact]
        public virtual void Correlated_collections_on_left_join_with_predicate()
        {
            AssertQuery<CogTag, Gear>(
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
        public virtual void Correlated_collections_on_left_join_with_null_value()
        {
            AssertQuery<CogTag, Gear>(
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
        public virtual void Correlated_collections_left_join_with_self_reference()
        {
            AssertQuery<CogTag, Gear>(
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
        public virtual void Correlated_collections_deeply_nested_left_join()
        {
            AssertQuery<CogTag, Gear>(
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
        public virtual void Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join()
        {
            AssertQuery<Weapon>(
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
        public virtual void Correlated_collections_complex_scenario1()
        {
            AssertQuery<Gear>(
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
        public virtual void Correlated_collections_complex_scenario2()
        {
            AssertQuery<Gear>(
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

        protected GearsOfWarContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
