// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
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

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class GearsOfWarQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : GearsOfWarQueryFixtureBase, new()
    {
        protected GearsOfWarQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_empty(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g == new Gear()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_one_to_one_and_one_to_many(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons", "Gear"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons", "Gear")
            };

            return AssertIncludeQuery<CogTag>(
                isAsync,
                ts => ts.Include(t => t.Gear.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToString_guid_property_projection(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Select(
                    ct => new
                    {
                        A = ct.GearNickName,
                        B = ct.Id.ToString()
                    }),
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
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Weapons.Include(w => w.Owner.Weapons).ToList());

            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_one_to_one_optional_and_one_to_one_required(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "Gear"),
                new ExpectedInclude<Officer>(o => o.Squad, "Squad", "Gear")
            };

            return AssertIncludeQuery<CogTag>(
                isAsync,
                ts => ts.Include(t => t.Gear.Squad),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Tags.Include(t => t.Gear.Squad.Members).ToList());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_circular(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth.StationedGears),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_circular_with_filter(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth.StationedGears).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_using_alternate_key(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => g.Weapons).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_multiple_include_then_include()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Gears.Include(g => g.AssignedCity.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.AssignedCity.StationedGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.StationedGears).ThenInclude(g => g.Tag)
                    .OrderBy(g => g.Nickname).ToList());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_navigation_on_derived_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.OfType<Officer>().Include(o => o.Reports),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_based_Include_navigation_on_derived_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.OfType<Officer>().Include("Reports"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Included(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(t => t.Gear, "Gear")
            };

            return AssertIncludeQuery<CogTag>(
                isAsync,
                ts => from t in ts.Include(o => o.Gear)
                      where t.Gear.Nickname == "Marcus"
                      select t,
                ts => from t in ts
                      where Maybe(t.Gear, () => t.Gear.Nickname) == "Marcus"
                      select t,
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_reference1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new
                        {
                            SquadId = (int?)g.SquadId,
                            g.Nickname
                        },
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        (g, t) => g).Include(g => g.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_reference2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            return AssertIncludeQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    ts.Join(
                        gs,
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        g => new
                        {
                            SquadId = (int?)g.SquadId,
                            g.Nickname
                        },
                        (t, g) => g).Include(g => g.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_collection1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new
                        {
                            SquadId = (int?)g.SquadId,
                            g.Nickname
                        },
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        (g, t) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_collection2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    ts.Join(
                        gs,
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        g => new
                        {
                            SquadId = (int?)g.SquadId,
                            g.Nickname
                        },
                        (t, g) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual void Include_where_list_contains_navigation(bool isAsync)
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

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual void Include_where_list_contains_navigation2(bool isAsync)
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

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual void Navigation_accessed_twice_outside_and_inside_subquery(bool isAsync)
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var gears = context.Gears
                    .Where(g => g.Tag != null && tags.Contains(g.Tag.Id))
                    .ToList();

                Assert.Equal(5, gears.Count);

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_multi_level(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<City>(c => c.StationedGears, "StationedGears", "CityOfBirth")
            };

            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    gs.Join(
                        ts,
                        g => new
                        {
                            SquadId = (int?)g.SquadId,
                            g.Nickname
                        },
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        (g, t) => g).Include(g => g.CityOfBirth.StationedGears),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth")
            };

            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    ts.Join(
                        gs.OfType<Officer>(),
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        o => new
                        {
                            SquadId = (int?)o.SquadId,
                            o.Nickname
                        },
                        (t, o) => o).Include(o => o.CityOfBirth),
                expectedIncludes);
        }

        // issue #12827
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    ts.Join(
                        gs.OfType<Officer>().OrderBy(ee => ee.SquadId),
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        o => new
                        {
                            SquadId = (int?)o.SquadId,
                            o.Nickname
                        },
                        (t, o) => o).OrderBy(ee => ee.FullName).Include(o => o.Reports).OrderBy(oo => oo.HasSoulPatch),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    gs.OfType<Officer>().Join(
                        ts,
                        o => new
                        {
                            SquadId = (int?)o.SquadId,
                            o.Nickname
                        },
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        (o, t) => o).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance3(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(o => o.Reports, "Reports")
            };

            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    ts.Join(
                        gs.OfType<Officer>(),
                        t => new
                        {
                            SquadId = t.GearSquadId,
                            Nickname = t.GearNickName
                        },
                        g => new
                        {
                            SquadId = (int?)g.SquadId,
                            g.Nickname
                        },
                        (t, o) => o).Include(o => o.Reports),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_nested_navigation_in_order_by(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Weapon>(w => w.Owner, "Owner")
            };

            return AssertIncludeQuery<Weapon>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_enum(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank == MilitaryRank.Sergeant));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_enum_with_constant(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.AmmunitionType == AmmunitionType.Cartridge));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_enum_with_null_constant(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.AmmunitionType == null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_enum_with_non_nullable_parameter(bool isAsync)
        {
            var ammunitionType = AmmunitionType.Cartridge;

            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.AmmunitionType == ammunitionType));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_nullable_enum_with_nullable_parameter(bool isAsync)
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            await AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.AmmunitionType == ammunitionType));

            ammunitionType = null;

            await AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.AmmunitionType == ammunitionType));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_and_enum(bool isAsync)
        {
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => (g.Rank & MilitaryRank.Corporal) > 0));

            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_and_integral(bool isAsync)
        {
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => ((int)g.Rank & 1) == 1));

            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => ((long)g.Rank & 1L) == 1L));

            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => ((short)g.Rank & (short)1) == 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_nullable_enum_with_constant(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => (w.AmmunitionType & AmmunitionType.Cartridge) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_nullable_enum_with_null_constant(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
#pragma warning disable CS0458 // The result of the expression is always 'null'
                ws => ws.Where(w => (w.AmmunitionType & null) > 0));
#pragma warning restore CS0458 // The result of the expression is always 'null'
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool isAsync)
        {
            var ammunitionType = AmmunitionType.Cartridge;

            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => (w.AmmunitionType & ammunitionType) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool isAsync)
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            await AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => (w.AmmunitionType & ammunitionType) > 0));

            ammunitionType = null;

            await AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => (w.AmmunitionType & ammunitionType) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_or_enum(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => (g.Rank | MilitaryRank.Corporal) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Bitwise_projects_values_in_select(bool isAsync)
        {
            return AssertFirst<Gear>(
                isAsync,
                gs => gs
                    .Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal)
                    .Select(
                        b => new
                        {
                            BitwiseTrue = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal,
                            BitwiseFalse = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Sergeant,
                            BitwiseValue = b.Rank & MilitaryRank.Corporal
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_enum_has_flag(bool isAsync)
        {
            // Constant
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag(MilitaryRank.Corporal)));

            // Expression
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag(MilitaryRank.Corporal | MilitaryRank.Captain)));

            // Casting
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag((MilitaryRank)1)));

            // Casting to nullable
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag((MilitaryRank?)1)));

            // QuerySource
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => MilitaryRank.Corporal.HasFlag(g.Rank)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_enum_has_flag_subquery(bool isAsync)
        {
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(
                    g => g.Rank.HasFlag(gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).Select(x => x.Rank).FirstOrDefault())));

            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(
                    g => MilitaryRank.Corporal.HasFlag(
                        gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).Select(x => x.Rank).FirstOrDefault())));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_enum_has_flag_subquery_with_pushdown(bool isAsync)
        {
            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag(gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));

            await AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(
                    g => MilitaryRank.Corporal.HasFlag(gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_enum_has_flag_subquery_client_eval(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag(gs.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).First().Rank)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_enum_has_flag_with_non_nullable_parameter(bool isAsync)
        {
            var parameter = MilitaryRank.Corporal;

            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag(parameter)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_has_flag_with_nullable_parameter(bool isAsync)
        {
            MilitaryRank? parameter = MilitaryRank.Corporal;

            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag(parameter)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_enum_has_flag(bool isAsync)
        {
            return AssertFirst<Gear>(
                isAsync,
                gs => gs.Where(g => g.Rank.HasFlag(MilitaryRank.Corporal))
                    .Select(
                        b => new
                        {
                            hasFlagTrue = b.Rank.HasFlag(MilitaryRank.Corporal),
                            hasFlagFalse = b.Rank.HasFlag(MilitaryRank.Sergeant)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_count_subquery_without_collision(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(w => w.Weapons.Count == 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_any_subquery_without_collision(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(w => w.Weapons.Any()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_inverted_boolean(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws
                    .Where(w => w.IsAutomatic)
                    .Select(
                        w => new
                        {
                            w.Id,
                            Manual = !w.IsAutomatic
                        }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_comparison_with_null(bool isAsync)
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            await AssertQuery<Weapon>(
                isAsync,
                ws => ws
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(
                        w => new
                        {
                            w.Id,
                            Cartridge = w.AmmunitionType == ammunitionType
                        }),
                elementSorter: e => e.Id);

            ammunitionType = null;

            await AssertQuery<Weapon>(
                isAsync,
                ws => ws
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(
                        w => new
                        {
                            w.Id,
                            Cartridge = w.AmmunitionType == ammunitionType
                        }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_with_boolean(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(
                    w => new
                    {
                        w.Id,
                        Num = w.IsAutomatic ? 1 : 0
                    }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_with_inverted_boolean(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(
                    w => new
                    {
                        w.Id,
                        Num = !w.IsAutomatic ? 1 : 0
                    }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_with_has_value_not_null(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws
                    .Where(w => w.AmmunitionType.HasValue && w.AmmunitionType == AmmunitionType.Cartridge)
                    .Select(
                        w => new
                        {
                            w.Id,
                            IsCartridge = w.AmmunitionType.HasValue && w.AmmunitionType.Value == AmmunitionType.Cartridge ? "Yes" : "No"
                        }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_multiple_conditions(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(
                    w => new
                    {
                        w.Id,
                        IsCartridge = w.AmmunitionType == AmmunitionType.Shell && w.SynergyWithId == 1 ? "Yes" : "No"
                    }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_multiple_conditions_2(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(
                    w => new
                    {
                        w.Id,
                        IsCartridge = !w.IsAutomatic && w.SynergyWithId == 1 ? "Yes" : "No"
                    }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_conditions(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(
                    w => new
                    {
                        w.Id,
                        IsCartridge = !w.IsAutomatic && w.SynergyWithId == 1
                    }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_ternary_operations(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(
                    w => new
                    {
                        w.Id,
                        IsManualCartridge = !w.IsAutomatic
                            ? w.AmmunitionType == AmmunitionType.Cartridge ? "ManualCartridge" : "Manual"
                            : "Auto"
                    }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization1(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => (g == null ? null : g.LeaderNickname) == "Marcus" == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization2(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => (g.LeaderNickname == null ? (bool?)null : (bool?)g.LeaderNickname.EndsWith("us")) == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization3(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => (g.LeaderNickname != null ? (bool?)g.LeaderNickname.EndsWith("us") : (bool?)null) == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization4(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(
                    g => (null == EF.Property<string>(g, "LeaderNickname") ? (int?)null : g.LeaderNickname.Length) == 5 == (bool?)true),
                gs => gs.Where(g => (null == g.LeaderNickname ? (int?)null : g.LeaderNickname.Length) == 5 == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization5(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(
                    g => (null != g.LeaderNickname ? (int?)(EF.Property<string>(g, "LeaderNickname").Length) : (int?)null) == 5
                                                                                                                           == (bool?)true),
                gs => gs.Where(g => (null != g.LeaderNickname ? (int?)(g.LeaderNickname.Length) : (int?)null) == 5 == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization6(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(
                    g => (null != g.LeaderNickname ? (int?)EF.Property<string>(g, "LeaderNickname").Length : (int?)null) == 5
                                                                                                                         == (bool?)true),
                gs => gs.Where(g => (null != g.LeaderNickname ? (int?)g.LeaderNickname.Length : (int?)null) == 5 == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_optimization7(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Select(g => null != g.LeaderNickname ? g.LeaderNickname + g.LeaderNickname : null));
        }

        [ConditionalTheory(Skip = "issue #9201")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_optimization8(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Select(g => g != null ? g.LeaderNickname + g.LeaderNickname : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_optimization9(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g != null ? (int?)g.FullName.Length : (int?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative1(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.LeaderNickname != null ? (bool?)(g.Nickname.Length == 5) : (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative2(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g1 in gs
                      from g2 in gs
                      select g1.LeaderNickname != null ? g2.LeaderNickname : (string)null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative3(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select new
                      {
                          g2.Nickname,
                          Condition = g2 != null ? (bool?)(g2.LeaderNickname != null) : (bool?)null
                      },
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby Maybe(g2, () => g2.Nickname)
                      select new
                      {
                          Nickname = Maybe(g2, () => g2.Nickname),
                          Condition = g2 != null ? (bool?)(g2.LeaderNickname != null) : (bool?)null
                      },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative4(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative5(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select g2 != null
                          ? new
                          {
                              g2.Nickname,
                              Five = 5
                          }
                          : null,
                gs => from g1 in gs
                      join g2 in gs on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby Maybe(g2, () => g2.Nickname)
                      select g2 != null
                          ? new
                          {
                              Nickname = Maybe(g2, () => g2.Nickname),
                              Five = 5
                          }
                          : null,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative6(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(
                    g => null != g.LeaderNickname
                        ? EF.Property<string>(g, "LeaderNickname").Length != EF.Property<string>(g, "LeaderNickname").Length
                        : (bool?)null),
                gs => gs.Select(g => null != g.LeaderNickname ? g.LeaderNickname.Length != g.LeaderNickname.Length : (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative7(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => null != g.LeaderNickname ? g.LeaderNickname == g.LeaderNickname : (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative8(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Select(t => t.Gear.Squad != null ? t.Gear.AssignedCity.Name : null),
                ts => ts.Select(
                    t => Maybe(t.Gear, () => t.Gear.Squad) != null
                        ? Maybe(t.Gear, () => Maybe(t.Gear.AssignedCity, () => t.Gear.AssignedCity.Name))
                        : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_works_for_navigations_with_composite_keys(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
#pragma warning disable IDE0031 // Use null propagation
                      select t.Gear != null ? t.Gear.Nickname : null,
#pragma warning restore IDE0031 // Use null propagation
                ts => from t in ts
                      select t.Gear != null ? Maybe(t.Gear, () => t.Gear.Nickname) : null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      select EF.Property<City>(EF.Property<CogTag>(t.Gear, "Tag").Gear, "AssignedCity") != null
                          ? EF.Property<string>(EF.Property<Gear>(t.Gear.Tag, "Gear").AssignedCity, "Name")
                          : null,
                ts => from t in ts
                      select Maybe(t.Gear, () => Maybe(t.Gear.Tag.Gear, () => t.Gear.Tag.Gear.AssignedCity)) != null
                          ? t.Gear.Tag.Gear.AssignedCity.Name
                          : null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_conditional_with_anonymous_type_and_null_constant(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      orderby g.Nickname
                      select g.LeaderNickname != null
                          ? new
                          {
                              g.HasSoulPatch
                          }
                          : null,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_conditional_with_anonymous_types(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      orderby g.Nickname
                      select g.LeaderNickname != null
                          ? new
                          {
                              Name = g.Nickname
                          }
                          : new
                          {
                              Name = g.FullName
                          },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_conditional_with_anonymous_type(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      orderby g.Nickname
                      where (g.LeaderNickname != null
                                ? new
                                {
                                    g.HasSoulPatch
                                }
                                : null) == null
                      select g.Nickname,
                assertOrder: true))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Gear>(    source: OrderBy<Gear, string>(        source: DbSet<Gear>,         keySelector: (g) => g.Nickname),     predicate: (g) => g.LeaderNickname != null ? new { HasSoulPatch = g.HasSoulPatch } : null == null)"),
                RemoveNewLines(message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_coalesce_with_anonymous_types(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      orderby g.Nickname
                      // ReSharper disable once ConstantNullCoalescingCondition
                      select new
                      {
                          Name = g.LeaderNickname
                      } ?? new
                      {
                          Name = g.FullName
                      },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_coalesce_with_anonymous_types(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                          // ReSharper disable once ConstantNullCoalescingCondition
                      where (new
                      {
                          Name = g.LeaderNickname
                      } ?? new
                      {
                          Name = g.FullName
                      }) != null
                      select g.Nickname))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Gear>(    source: DbSet<Gear>,     predicate: (g) => new { Name = g.LeaderNickname } ?? new { Name = g.FullName } != null)"),
                RemoveNewLines(message));
        }

        [ConditionalFact(Skip = "issue #8421")]
        public virtual void Where_compare_anonymous_types()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            from o in context.Gears.OfType<Officer>()
                            where new
                            {
                                Name = g.LeaderNickname,
                                Squad = g.LeaderSquadId,
                                Five = 5
                            } == new
                            {
                                Name = o.Nickname,
                                Squad = o.SquadId,
                                Five = 5
                            }
                            select g.Nickname;

                var result = query.ToList();
                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_member_access_on_anonymous_type(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where new
                      {
                          Name = g.LeaderNickname,
                          Squad = g.LeaderSquadId
                      }.Name == "Marcus"
                      select g.Nickname);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_anonymous_types_with_uncorrelated_members(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                          // ReSharper disable once EqualExpressionComparison
                      where new
                      {
                          Five = 5
                      } == new
                      {
                          Five = 5
                      }
                      select g.Nickname);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      where t.Gear.Nickname == "Marcus"
                      select t,
                ts => from t in ts
                      where Maybe(t.Gear, () => t.Gear.Nickname) == "Marcus"
                      select t);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t1 in ts
                      from t2 in ts
                      where t1.Gear.Nickname == t2.Gear.Nickname
                      select new
                      {
                          Tag1 = t1,
                          Tag2 = t2
                      },
                ts => from t1 in ts
                      from t2 in ts
                      where Maybe(t1.Gear, () => t1.Gear.Nickname) == Maybe(t2.Gear, () => t2.Gear.Nickname)
                      select new
                      {
                          Tag1 = t1,
                          Tag2 = t2
                      },
                elementSorter: e => e.Tag1.Id + " " + e.Tag2.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Tag1.Id, a.Tag1.Id);
                    Assert.Equal(e.Tag2.Id, a.Tag2.Id);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t1 in ts
                      from t2 in ts
                      where t1.Gear.Nickname == t2.Gear.Nickname
                      select new
                      {
                          Id1 = t1.Id,
                          Id2 = t2.Id
                      },
                ts => from t1 in ts
                      from t2 in ts
                      where Maybe(t1.Gear, () => t1.Gear.Nickname) == Maybe(t2.Gear, () => t2.Gear.Nickname)
                      select new
                      {
                          Id1 = t1.Id,
                          Id2 = t2.Id
                      },
                elementSorter: e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool isAsync)
        {
            return AssertFirst<Weapon>(
                isAsync,
                ws => ws.OrderBy(w => w.Id).Select(
                    w => new Weapon
                    {
                        IsAutomatic = (bool?)w.SynergyWith.IsAutomatic ?? false
                    }),
                ws => ws.OrderBy(w => w.Id).Select(
                    w => new Weapon
                    {
                        IsAutomatic = MaybeScalar<bool>(w.SynergyWith, () => w.SynergyWith.IsAutomatic) ?? false
                    }));
        }

        [ConditionalTheory(Skip = "issue #14900")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_boolean(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "issue #14900")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory(Skip = "issue #14900")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "issue #14900")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_first_boolean(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OrderBy(g => g.Nickname).Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).First().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_singleordefault_boolean1(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OrderBy(g => g.Nickname).Where(
                    g => g.HasSoulPatch && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().Select(w => w.IsAutomatic)
                             .SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_singleordefault_boolean2(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OrderBy(g => g.Nickname).Where(
                    g => g.HasSoulPatch && g.Weapons.Where(w => w.Name.Contains("Lancer")).Select(w => w.IsAutomatic).Distinct()
                             .SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OrderBy(g => g.Nickname).Where(
                    g => g.HasSoulPatch && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic),
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

        [ConditionalTheory(Skip = "issue #14900")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_orderby_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "issue #14900")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_union_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Union(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_subquery_concat_firstordefault_boolean(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch && g.Weapons.Concat(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic)))).Message;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_with_count(bool isAsync)
        {
            return AssertCount<Gear>(
                isAsync,
                gs => gs.Concat(gs));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_scalars_with_count(bool isAsync)
        {
            return AssertCount<Gear>(
                isAsync,
                gs => gs.Select(g => g.Nickname).Concat(gs.Select(g2 => g2.FullName)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_anonymous_with_count(bool isAsync)
        {
            return AssertCount<Gear>(
                isAsync,
                gs => gs.Select(
                        g => new
                        {
                            Gear = g,
                            Name = g.Nickname
                        })
                    .Concat(
                        gs.Select(
                            g2 => new
                            {
                                Gear = g2,
                                Name = g2.FullName
                            })));
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

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_with_groupings(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.GroupBy(g => g.LeaderNickname).Concat(gs.GroupBy(g => g.LeaderNickname)),
                elementSorter: GroupingSorter<string, Gear>(),
                elementAsserter: GroupingAsserter<string, Gear>(g => g.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_navigation_with_concat_and_count(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => !g.HasSoulPatch).Select(g => g.Weapons.Concat(g.Weapons).Count())))).Message;

            Assert.Equal(
                RemoveNewLines(CoreStrings.QueryFailed("Concat<Weapon>(\n    source1: AsQueryable<Weapon>(MaterializeCollectionNavigation(Navigation: Gear.Weapons (<Weapons>k__BackingField, ICollection<Weapon>) Collection ToDependent Weapon Inverse: Owner, Where<Weapon>(\n        source: NavigationExpansionExpression\n            Source: Where<Weapon>(\n                source: DbSet<Weapon>, \n                predicate: (w) => Property<string>((Unhandled parameter: g), \"FullName\") == Property<string>(w, \"OwnerFullName\"))\n            PendingSelector: (w) => NavigationTreeExpression\n                Value: EntityReferenceWeapon\n                Expression: w\n        , \n        predicate: (i) => Property<string>(NavigationTreeExpression\n            Value: EntityReferenceGear\n            Expression: (Unhandled parameter: g), \"FullName\") == Property<string>(i, \"OwnerFullName\")))), \n    source2: MaterializeCollectionNavigation(Navigation: Gear.Weapons (<Weapons>k__BackingField, ICollection<Weapon>) Collection ToDependent Weapon Inverse: Owner, Where<Weapon>(\n        source: NavigationExpansionExpression\n            Source: Where<Weapon>(\n                source: DbSet<Weapon>, \n                predicate: (w0) => Property<string>((Unhandled parameter: g), \"FullName\") == Property<string>(w0, \"OwnerFullName\"))\n            PendingSelector: (w0) => NavigationTreeExpression\n                Value: EntityReferenceWeapon\n                Expression: w0\n        , \n        predicate: (i) => Property<string>(NavigationTreeExpression\n            Value: EntityReferenceGear\n            Expression: (Unhandled parameter: g), \"FullName\") == Property<string>(i, \"OwnerFullName\"))))", "NavigationExpandingExpressionVisitor")),
                RemoveNewLines(message));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_concat_order_by_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.GroupBy(g => g.LeaderNickname).Concat(gs.GroupBy(g => g.LeaderNickname)),
                elementSorter: GroupingSorter<string, Gear>(),
                elementAsserter: GroupingAsserter<string, Gear>(g => g.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Concat_with_collection_navigations(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch).Select(g => g.Weapons.Union(g.Weapons).Count())))).Message;

            Assert.Equal(
                RemoveNewLines(CoreStrings.QueryFailed("Union<Weapon>(\n    source1: AsQueryable<Weapon>(MaterializeCollectionNavigation(Navigation: Gear.Weapons (<Weapons>k__BackingField, ICollection<Weapon>) Collection ToDependent Weapon Inverse: Owner, Where<Weapon>(\n        source: NavigationExpansionExpression\n            Source: Where<Weapon>(\n                source: DbSet<Weapon>, \n                predicate: (w) => Property<string>((Unhandled parameter: g), \"FullName\") == Property<string>(w, \"OwnerFullName\"))\n            PendingSelector: (w) => NavigationTreeExpression\n                Value: EntityReferenceWeapon\n                Expression: w\n        , \n        predicate: (i) => Property<string>(NavigationTreeExpression\n            Value: EntityReferenceGear\n            Expression: (Unhandled parameter: g), \"FullName\") == Property<string>(i, \"OwnerFullName\")))), \n    source2: MaterializeCollectionNavigation(Navigation: Gear.Weapons (<Weapons>k__BackingField, ICollection<Weapon>) Collection ToDependent Weapon Inverse: Owner, Where<Weapon>(\n        source: NavigationExpansionExpression\n            Source: Where<Weapon>(\n                source: DbSet<Weapon>, \n                predicate: (w0) => Property<string>((Unhandled parameter: g), \"FullName\") == Property<string>(w0, \"OwnerFullName\"))\n            PendingSelector: (w0) => NavigationTreeExpression\n                Value: EntityReferenceWeapon\n                Expression: w0\n        , \n        predicate: (i) => Property<string>(NavigationTreeExpression\n            Value: EntityReferenceGear\n            Expression: (Unhandled parameter: g), \"FullName\") == Property<string>(i, \"OwnerFullName\"))))", "NavigationExpandingExpressionVisitor")),
                RemoveNewLines(message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Union_with_collection_navigations(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.OfType<Officer>().Select(o => o.Reports.Union(o.Reports).Count())))).Message;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_firstordefault(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch).Select(g => g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_Where_Navigation_Client(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      where t.Gear != null && t.Gear.IsMarcus
                      select t))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("Where<TransparentIdentifier<CogTag, Gear>>(    source: LeftJoin<CogTag, Gear, AnonymousObject, TransparentIdentifier<CogTag, Gear>>(        outer: DbSet<CogTag>,         inner: DbSet<Gear>,         outerKeySelector: (c) => new AnonymousObject(new object[]        {             (object)Property<string>(c, \"GearNickName\"),             (object)Property<Nullable<int>>(c, \"GearSquadId\")         }),         innerKeySelector: (g) => new AnonymousObject(new object[]        {             (object)Property<string>(g, \"Nickname\"),             (object)Property<Nullable<int>>(g, \"SquadId\")         }),         resultSelector: (o, i) => new TransparentIdentifier<CogTag, Gear>(            Outer = o,             Inner = i        )),     predicate: (c) => Property<string>(c.Inner, \"Nickname\") != null && c.Inner.IsMarcus)"),
                RemoveNewLines(message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Null(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      where t.Gear == null
                      select t);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Null_Reverse(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      where null == t.Gear
                      select t);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Equals_Navigation(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t1 in ts
                      from t2 in ts
                      where t1.Gear == t2.Gear
                      select new
                      {
                          t1,
                          t2
                      },
                elementSorter: e => e.t1.Id + " " + e.t2.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.t1.Id, a.t1.Id);
                    Assert.Equal(e.t2.Id, a.t2.Id);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from ct in ts
                      where ct.Gear.Nickname == "Marcus"
                      where ct.Gear.CityOrBirthName != "Ephyra"
                      select new
                      {
                          B = ct.Gear.CityOrBirthName
                      },
                ts => from ct in ts
                      where Maybe(ct.Gear, () => ct.Gear.Nickname) == "Marcus"
                      where Maybe(ct.Gear, () => ct.Gear.CityOrBirthName) != "Ephyra"
                      select new
                      {
                          B = Maybe(ct.Gear, () => ct.Gear.CityOrBirthName)
                      },
                elementSorter: e => e.B);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from ct in ts
                      where ct.Gear.Nickname == "Marcus"
                      where ct.Gear.CityOrBirthName != "Ephyra"
                      select new
                      {
                          A = ct.Gear,
                          B = ct.Gear.CityOrBirthName
                      },
                ts => from ct in ts
                      where Maybe(ct.Gear, () => ct.Gear.Nickname) == "Marcus"
                      where Maybe(ct.Gear, () => ct.Gear.CityOrBirthName) != "Ephyra"
                      select new
                      {
                          A = ct.Gear,
                          B = Maybe(ct.Gear, () => ct.Gear.CityOrBirthName)
                      },
                elementSorter: e => e.A.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.A.Nickname, e.A.Nickname);
                    Assert.Equal(e.B, e.B);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_Composite_Key(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    from t in ts
                    join g in gs
                        on new
                        {
                            N = t.GearNickName,
                            S = t.GearSquadId
                        }
                        equals new
                        {
                            N = g.Nickname,
                            S = (int?)g.SquadId
                        } into grouping
                    from g in grouping
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_translated_to_subquery_composite_key(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    from g in gs
                    join t in ts on g.FullName equals t.Gear.FullName
                    select new
                    {
                        g.FullName,
                        t.Note
                    },
                (gs, ts) =>
                    from g in gs
                    join t in ts on g.FullName equals Maybe(t.Gear, () => t.Gear.FullName)
                    select new
                    {
                        g.FullName,
                        t.Note
                    },
                elementSorter: e => e.FullName);
        }

        // issue #12786
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    from g in gs
                    join t in ts.OrderBy(tt => tt.Id) on g.FullName equals t.Gear.FullName
                    select new
                    {
                        g.FullName,
                        t.Note
                    },
                (gs, ts) =>
                    from g in gs
                    join t in ts.OrderBy(tt => tt.Id) on g.FullName equals Maybe(t.Gear, () => t.Gear.FullName)
                    select new
                    {
                        g.FullName,
                        t.Note
                    },
                elementSorter: e => e.FullName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_order_by_without_skip_or_take(bool isAsync)
        {
            return AssertQuery<Gear, Weapon>(
                isAsync,
                (gs, ws) =>
                    from g in gs
                    join w in ws.OrderBy(ww => ww.Name) on g.FullName equals w.OwnerFullName
                    select new { w.Name, g.FullName },
                elementSorter: w => w.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_with_inheritance_and_join_include_joined(bool isAsync)
        {
            return AssertIncludeQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    (from t in ts
                     join g in gs.OfType<Officer>() on new
                     {
                         id1 = t.GearSquadId,
                         id2 = t.GearNickName
                     }
                         equals new
                         {
                             id1 = (int?)g.SquadId,
                             id2 = g.Nickname
                         }
                     select g).Include(g => g.Tag),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Tag, "Tag")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_with_inheritance_and_join_include_source(bool isAsync)
        {
            return AssertIncludeQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    (from g in gs.OfType<Officer>()
                     join t in ts on new
                     {
                         id1 = (int?)g.SquadId,
                         id2 = g.Nickname
                     }
                         equals new
                         {
                             id1 = t.GearSquadId,
                             id2 = t.GearNickName
                         }
                     select g).Include(g => g.Tag),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Tag, "Tag")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool isAsync)
        {
            return AssertQuery<City>(
                isAsync,
                cs => from c in cs
                      where c.Location == "Unknown"
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool isAsync)
        {
            return AssertQuery<City>(
                isAsync,
                cs => from c in cs
                      where "Unknown" == c.Location
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_parameter_is_used_for_non_unicode_column(bool isAsync)
        {
            var value = "Unknown";

            return AssertQuery<City>(
                isAsync,
                cs => from c in cs
                      where c.Location == value
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(bool isAsync)
        {
            var cities = new List<string>
            {
                "Unknown",
                "Jacinto's location",
                "Ephyra's location"
            };

            return AssertQuery<City>(
                isAsync,
                cs => from c in cs
                      where cities.Contains(c.Location)
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool isAsync)
        {
            return AssertQuery<City>(
                isAsync,
                cs => from c in cs
                      where c.Location == "Unknown" && c.BornGears.Count(g => g.Nickname == "Paduk") == 1
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname == "Marcus" && g.CityOfBirth.Location == "Jacinto's location"
                      select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool isAsync)
        {
            return AssertQuery<City>(
                isAsync,
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

                Assert.Equal(new[] { "Marcus", "Marcus", "Marcus", "Marcus", "Baird" }, result.Select(g => g.Nickname));
                Assert.Equal(new[] { 0, 0, 0, 2, 0 }, result.Select(g => g.Weapons.Count));
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

                Assert.Equal(new[] { "Marcus", "Marcus", "Marcus", "Marcus", "Baird" }, result.Select(g => g.Nickname));
                Assert.Equal(new[] { 2, 2, 2, 0, 2 }, result.Select(g => g.Weapons.Count));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      select g2 ?? g1,
                expectedIncludes);
        }

        [ConditionalTheory(Skip = "Issue#16899")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      select new
                      {
                          g1,
                          g2,
                          coalesce = g2 ?? g1
                      },
                expectedIncludes,
                elementSorter: e => e.g1.Nickname,
                clientProjections: new List<Func<dynamic, object>>
                {
                    r => r.g1,
                    r => r.g2,
                    r => r.coalesce
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.OfType<Officer>().Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      select g2 ?? g1,
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
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

        [ConditionalTheory(Skip = "issue #9256")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => from g1 in gs.Include(g => g.Weapons)
                      join g2 in gs.Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                          // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                      select new
                      {
                          g1,
                          g2,
                          coalesce = g2 ?? g1,
                          conditional = g2 != null ? g2 : g1
                      },
#pragma warning restore IDE0029 // Use coalesce expression
                expectedIncludes,
                elementSorter: e => e.g1.Nickname + " " + e.g2?.Nickname,
                clientProjections: new List<Func<dynamic, object>>
                {
                    e => e.g1,
                    e => e.g2,
                    e => e.coalesce,
                    e => e.conditional
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Coalesce_operator_in_predicate(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => (bool?)w.IsAutomatic ?? false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Coalesce_operator_in_predicate_with_other_conditions(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Coalesce_operator_in_projection_with_other_conditions(bool isAsync)
        {
            return AssertQueryScalar<Weapon>(
                isAsync,
                ws => ws.Select(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A." && t.Gear.HasSoulPatch));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate2(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Gear.HasSoulPatch),
                ts => ts.Where(t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => !t.Gear.HasSoulPatch),
                ts => ts.Where(t => !MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true));
        }

        [ConditionalTheory(Skip = "issue #9254")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => !(t.Gear.HasSoulPatch ? true : t.Gear.HasSoulPatch)));
        }

        [ConditionalTheory(Skip = "issue #9254")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => !(!t.Gear.HasSoulPatch ? false : t.Gear.HasSoulPatch)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_conditional_expression(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                // ReSharper disable once RedundantTernaryExpression
                ts => ts.Where(t => t.Gear.HasSoulPatch ? true : false),
                // ReSharper disable once RedundantTernaryExpression
                ts => ts.Where(t => (MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true) ? true : false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_binary_expression(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Gear.HasSoulPatch || t.Note.Contains("Cole")),
                ts => ts.Where(t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true || t.Note.Contains("Cole")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool isAsync)
        {
            return AssertQueryScalar<CogTag>(
                isAsync,
                ts => ts.Select(t => t.Gear.HasSoulPatch && t.Note.Contains("Cole")),
                ts => ts.Select(t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true && t.Note.Contains("Cole")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_projection(bool isAsync)
        {
            return AssertQueryScalar<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").Select(t => t.Gear.SquadId));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").Select(
                    t => new
                    {
                        t.Gear.SquadId
                    }),
                elementSorter: e => e.SquadId);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_DTOs(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").Select(
                    t => new Squad
                    {
                        Id = t.Gear.SquadId
                    }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_list_initializers(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note).Select(
                    t => new List<int>
                    {
                        t.Gear.SquadId,
                        t.Gear.SquadId + 1,
                        42
                    }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_array_initializers(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").Select(t => new[] { t.Gear.SquadId }),
                elementSorter: e => e[0]);

        }
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_orderby(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Gear.SquadId).Select(t => t));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_groupby(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").GroupBy(t => t.Gear.SquadId),
                elementSorter: GroupingSorter<int, CogTag>(),
                elementAsserter: GroupingAsserter<int, CogTag>(t => t.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_all(bool isAsync)
        {
            return AssertAll<CogTag, CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A."),
                predicate: t => t.Gear.HasSoulPatch);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_negated_predicate(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note != "K.I.A.").Where(t => !t.Gear.HasSoulPatch));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_contains(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) => ts.Where(t => t.Note != "K.I.A." && gs.Select(g => g.SquadId).Contains(t.Gear.SquadId)));
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_skip(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note)
                    .Select(t => gs.OrderBy(g => g.Nickname).Skip(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_take(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) => ts.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note)
                    .Select(t => gs.OrderBy(g => g.Nickname).Take(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_filtered_collection(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs
                    .Where(g => g.CityOfBirth.Name == "Ephyra" || g.CityOfBirth.Name == "Hanover")
                    .OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.Where(w => w.Name != "Lancer").ToList()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_filtered_collection_with_composite_key(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OfType<Officer>().OrderBy(g => g.Nickname).Select(g => g.Reports.Where(r => r.Nickname != "Dom").ToList()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_filtered_collection_works_with_caching(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) => ts.OrderBy(t => t.Note).Select(t => gs.Where(g => g.Nickname == t.GearNickName)),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(g => g.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_predicate_value_equals_condition(bool isAsync)
        {
            return AssertQuery<Gear, Weapon>(
                isAsync,
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on true equals w.SynergyWithId != null
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_predicate_value(bool isAsync)
        {
            return AssertQuery<Gear, Weapon>(
                isAsync,
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.HasSoulPatch equals true
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_predicate_condition_equals_condition(bool isAsync)
        {
            return AssertQuery<Gear, Weapon>(
                isAsync,
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.FullName != null equals w.SynergyWithId != null
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_predicate_value_equals_condition(bool isAsync)
        {
            return AssertQuery<Gear, Weapon>(
                isAsync,
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on true equals w.SynergyWithId != null
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_predicate_value(bool isAsync)
        {
            return AssertQuery<Gear, Weapon>(
                isAsync,
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.HasSoulPatch equals true
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_predicate_condition_equals_condition(bool isAsync)
        {
            return AssertQuery<Gear, Weapon>(
                isAsync,
                (gs, ws) =>
                    from g in gs
                    join w in ws
                        on g.FullName != null equals w.SynergyWithId != null
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_now(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline != DateTimeOffset.Now
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_utcnow(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline != DateTimeOffset.UtcNow
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_date_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Date > new DateTimeOffset().Date
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_year_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Year == 2
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_month_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Month == 1
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_dayofyear_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.DayOfYear == 2
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_day_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Day == 2
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_hour_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Hour == 10
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_minute_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Minute == 0
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_second_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Second == 0
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_millisecond_component(bool isAsync)
        {
            return AssertQuery<Mission>(
                isAsync,
                ms => from m in ms
                      where m.Timeline.Millisecond == 0
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddYears(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.AddYears(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddMonths(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.AddMonths(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddDays(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.AddDays(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddHours(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.AddHours(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddMinutes(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.AddMinutes(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddSeconds(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.AddSeconds(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddMilliseconds(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.AddMilliseconds(300));
        }

        [ConditionalFact]
        public virtual void Where_datetimeoffset_milliseconds_parameter_and_constant()
        {
            using (var ctx = CreateContext())
            {
                var dateTimeOffset = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0));

                // Parameter where clause
                Assert.Equal(1, ctx.Missions.Where(m => m.Timeline == dateTimeOffset).Count());

                // Literal where clause
                var p = Expression.Parameter(typeof(Mission), "i");
                var dynamicWhere = Expression.Lambda<Func<Mission, bool>>(
                    Expression.Equal(
                        Expression.Property(p, "Timeline"),
                        Expression.Constant(dateTimeOffset)
                    ), p);

                Assert.Equal(1, ctx.Missions.Where(dynamicWhere).Count());
            }
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(
            bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new
                    {
                        t.Note,
                        Nickname = g != null ? g.Nickname : null
                    },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Note);
        }

        private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        {
            // ReSharper disable PossibleMultipleEnumeration
            return source?.Count() == 0 ? new[] { default(TElement) } : source;
            // ReSharper restore PossibleMultipleEnumeration
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => from w in ws
                      where w.Id != 50 && !w.Owner.HasSoulPatch
                      select w,
                ws => from w in ws
                      where w.Id != 50 && MaybeScalar<bool>(w.Owner, () => w.Owner.HasSoulPatch) == false
                      select w);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => (from g in gs
                       where g.Tag.Note != "Foo"
                       select g.HasSoulPatch).Distinct(),
                gs => (from g in gs
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.HasSoulPatch).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertSum<Gear>(
                isAsync,
                gs => (from g in gs
                       where g.Tag.Note != "Foo"
                       select g.SquadId),
                gs => (from g in gs
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.SquadId));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertCount<Gear>(
                isAsync,
                gs => (from g in gs
                       where g.Tag.Note != "Foo"
                       select g.HasSoulPatch),
                gs => (from g in gs
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.HasSoulPatch));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_with_unflattened_groupjoin_is_evaluated_on_client(bool isAsync)
        {
            return AssertQueryScalar<Gear, CogTag>(
                isAsync,
                (gs, ts) => gs.GroupJoin(
                        ts,
                        g => new
                        {
                            k1 = g.Nickname,
                            k2 = (int?)g.SquadId
                        },
                        t => new
                        {
                            k1 = t.GearNickName,
                            k2 = t.GearSquadId
                        },
                        (g, t) => g.HasSoulPatch)
                    .Distinct());
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_unflattened_groupjoin_is_evaluated_on_client(bool isAsync)
        {
            return AssertCount<Gear, CogTag>(
                isAsync,
                (gs, ts) => gs
                    .GroupJoin(
                        ts,
                        g => new
                        {
                            k1 = g.Nickname,
                            k2 = (int?)g.SquadId
                        },
                        t => new
                        {
                            k1 = t.GearNickName,
                            k2 = t.GearSquadId
                        },
                        (g, t) => g));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool isAsync)
        {
            return AssertFirstOrDefault<Squad, Gear>(
                isAsync,
                (ss, gs) =>
                    from s in ss
                    join g in gs on s.Id equals g.SquadId into grouping
                    from g in grouping.DefaultIfEmpty()
                    where s.Name == "Kilo"
                    select s);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => from s in ss
                      where !s.Members.Any(m => m.Tag.Note == "Dom's Tag")
                      select s.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertAll<Gear, Gear>(
                isAsync,
                gs => from g in gs
                      select g,
                predicate: g => g.Tag.Note != "Foo");
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client(bool isAsync)
        {
            return AssertQueryScalar<CogTag, Gear>(
                isAsync,
                (ts, gs) => ts.GroupJoin(
                    gs,
                    t => new
                    {
                        k1 = t.GearNickName,
                        k2 = t.GearSquadId
                    },
                    g => new
                    {
                        k1 = g.Nickname,
                        k2 = (int?)g.SquadId
                    },
                    (k, r) => r.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_side_equality_with_parameter_works_with_optional_navigations(bool isAsync)
        {
            var prm = "Marcus' Tag";

            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => gs.Where(g => ClientEquals(g.Tag.Note, prm)),
                elementAsserter: (e, a) => Assert.Equal(e.Nickname, a.Nickname)))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("Where<TransparentIdentifier<Gear, CogTag>>(    source: LeftJoin<Gear, CogTag, AnonymousObject, TransparentIdentifier<Gear, CogTag>>(        outer: DbSet<Gear>,         inner: DbSet<CogTag>,         outerKeySelector: (g) => new AnonymousObject(new object[]        {             (object)Property<string>(g, \"Nickname\"),             (object)Property<Nullable<int>>(g, \"SquadId\")         }),         innerKeySelector: (c) => new AnonymousObject(new object[]        {             (object)Property<string>(c, \"GearNickName\"),             (object)Property<Nullable<int>>(c, \"GearSquadId\")         }),         resultSelector: (o, i) => new TransparentIdentifier<Gear, CogTag>(            Outer = o,             Inner = i        )),     predicate: (g) => ClientEquals(        first: g.Inner.Note,         second: (Unhandled parameter: __prm_0)))"),
                RemoveNewLines(message));
        }

        private static bool ClientEquals(string first, string second)
        {
            return first == second;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_nullable_guid_list_closure(bool isAsync)
        {
            var ids = new List<Guid?>
            {
                Guid.Parse("D2C26679-562B-44D1-AB96-23D1775E0926"),
                Guid.Parse("23CBCF9B-CE14-45CF-AAFA-2C2667EBFDD3"),
                Guid.Parse("AB1B82D7-88DB-42BD-A132-7EEF9AA68AF4")
            };

            return AssertQuery<CogTag>(
                isAsync,
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
                    .Select(
                        g => new
                        {
                            FullName = EF.Property<string>(g, "FullName")
                        });

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Damon Baird", result[0].FullName);
                Assert.Equal("Marcus Fenix", result[1].FullName);
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
        public virtual void Subquery_is_lifted_from_main_from_clause_of_SelectMany()
        {
            using (var ctx = CreateContext())
            {
                var query = from g1 in ctx.Gears.OrderBy(g => g.Rank).Include(g => g.Tag)
                            from g2 in ctx.Gears
                            orderby g1.FullName
                            where g1.HasSoulPatch && !g2.HasSoulPatch
                            select new
                            {
                                Name1 = g1.FullName,
                                Name2 = g2.FullName
                            };

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.True(result.All(r => r.Name1 == "Damon Baird" || r.Name1 == "Marcus Fenix"));
                Assert.True(result.All(r => r.Name2 == "Augustus Cole" || r.Name2 == "Garron Paduk" || r.Name2 == "Dominic Santiago"));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_containing_join_projecting_main_from_clause_gets_lifted(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    from g in (from gear in gs
                               join tag in ts on gear.Nickname equals tag.GearNickName
                               orderby tag.Note
                               select gear).AsTracking()
                    orderby g.Nickname
                    select g.Nickname,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_containing_join_gets_lifted_clashing_names(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_is_lifted_from_additional_from_clause(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from g1 in gs
                    from g2 in gs.OrderBy(g => g.Rank).Include(g => g.Tag)
                    orderby g1.FullName
                    where g1.HasSoulPatch && !g2.HasSoulPatch
                    select new
                    {
                        Name1 = g1.FullName,
                        Name2 = g2.FullName
                    },
                elementSorter: e => e.Name1 + " " + e.Name2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_with_result_operator_is_not_lifted(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs.Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Take(2).AsTracking()
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_with_orderby_followed_by_orderBy_is_pushed_down(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs.Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Skip(1)
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs.Where(g => !g.HasSoulPatch).Take(999).OrderBy(g => g.FullName)
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs.Where(g => !g.HasSoulPatch).Take(999)
                      orderby g.FullName
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down3(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs.Where(g => !g.HasSoulPatch).Take(999)
                      orderby g.FullName, g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_length_of_string_property(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => from w in ws
                      select new
                      {
                          w.Name,
                          w.Name.Length
                      },
                elementSorter: e => e.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_method_on_collection_navigation_in_predicate(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Marcus' Lancer"
                      select g.Nickname))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Gear>(    source: DbSet<Gear>,     predicate: (g) => g.HasSoulPatch && FavoriteWeapon(MaterializeCollectionNavigation(Navigation: Gear.Weapons (<Weapons>k__BackingField, ICollection<Weapon>) Collection ToDependent Weapon Inverse: Owner, Where<Weapon>(        source: DbSet<Weapon>,         predicate: (w) => Property<string>(g, \"FullName\") == Property<string>(w, \"OwnerFullName\")))).Name == \"Marcus' Lancer\")"),
                RemoveNewLines(message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where !g.HasSoulPatch && FavoriteWeapon(EF.Property<List<Weapon>>(g, "Weapons")).Name == "Cole's Gnasher"
                      select g.Nickname,
                gs => from g in gs
                      where !g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Cole's Gnasher"
                      select g.Nickname))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Gear>(    source: DbSet<Gear>,     predicate: (g) => !(g.HasSoulPatch) && FavoriteWeapon(MaterializeCollectionNavigation(Navigation: Gear.Weapons (<Weapons>k__BackingField, ICollection<Weapon>) Collection ToDependent Weapon Inverse: Owner, Where<Weapon>(        source: DbSet<Weapon>,         predicate: (w) => Property<string>(g, \"FullName\") == Property<string>(w, \"OwnerFullName\")))).Name == \"Cole's Gnasher\")"),
                RemoveNewLines(message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_method_on_collection_navigation_in_order_by(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where !g.HasSoulPatch
                      orderby FavoriteWeapon(g.Weapons).Name descending
                      select g.Nickname,
                assertOrder: true))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("OrderByDescending<Gear, string>(    source: Where<Gear>(        source: DbSet<Gear>,         predicate: (g) => !(g.HasSoulPatch)),     keySelector: (g) => FavoriteWeapon(MaterializeCollectionNavigation(Navigation: Gear.Weapons (<Weapons>k__BackingField, ICollection<Weapon>) Collection ToDependent Weapon Inverse: Owner, Where<Weapon>(        source: DbSet<Weapon>,         predicate: (w) => Property<string>(g, \"FullName\") == Property<string>(w, \"OwnerFullName\")))).Name)"),
                RemoveNewLines(message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_method_on_collection_navigation_in_additional_from_clause(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                    isAsync,
                    gs => from g in gs.OfType<Officer>()
                          from v in Veterans(g.Reports)
                          select new
                          {
                              g = g.Nickname, v = v.Nickname
                          },
                    elementSorter: e => e.g + e.v))).Message;

                Assert.Equal(
                    CoreStrings.QueryFailed("(g) => Veterans(g.Reports)", "NavigationExpandingExpressionVisitor"),
                    RemoveNewLines(message));
        }

        private string RemoveNewLines(string message)
            => message.Replace("\n", "").Replace("\r", "");

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_method_on_collection_navigation_in_outer_join_key(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => from o in gs.OfType<Officer>()
                      join g in gs on FavoriteWeapon(o.Weapons).Name equals FavoriteWeapon(g.Weapons).Name
                      where o.HasSoulPatch
                      select new
                      {
                          o = o.Nickname,
                          g = g.Nickname
                      },
                elementSorter: e => e.o + e.g))).Message;
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
                            select new
                            {
                                ((LocustHorde)f).Name,
                                ((LocustHorde)f).Eradicated
                            };

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
                            select new
                            {
                                f,
                                ((LocustHorde)f).Eradicated
                            };

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
                            select new
                            {
                                horde.Name,
                                horde.Eradicated
                            };

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
                            select new
                            {
                                Name = EF.Property<string>(horde, "Name"),
                                Eradicated = EF.Property<bool>((LocustHorde)f, "Eradicated")
                            };

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
                            select new
                            {
                                f.Name,
                                Threat = ((LocustHorde)f).Commander.ThreatLevel
                            };

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
                            select new
                            {
                                f,
                                f.Name,
                                Threat = ((LocustHorde)f).Commander.ThreatLevel
                            };

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
                            select new
                            {
                                f.Name,
                                Threat = EF.Property<LocustCommander>((LocustHorde)f, "Commander").ThreatLevel
                            };

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
                            select new
                            {
                                f.Name,
                                CommanderName = ((LocustHorde)f).Commander.Name
                            };

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
                            select new
                            {
                                f.Name,
                                LeadersCount = ((LocustHorde)f).Leaders.Count
                            };

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
                            select new
                            {
                                f.Name,
                                LeaderName = l.Name
                            };

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

        [ConditionalFact(Skip = "Issue#16089")]
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

        [ConditionalFact(Skip = "Issue#16089")]
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

        [ConditionalFact(Skip = "Issue#16089")]
        public virtual void Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in (from f2 in ctx.Factions
                                        where f2 is LocustHorde
                                        select (LocustHorde)f2).Include(h => h.Commander).Include(h => h.Leaders)
                            from f in ctx.Factions.Include(ff => ff.Capital)
                            orderby lh.Name, f.Name
                            select new
                            {
                                lh,
                                f
                            };

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
                            select new
                            {
                                Nickname1 = g1.Nickname,
                                Nickname2 = g2.Nickname
                            };

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
                            select new
                            {
                                f.Name,
                                o.Nickname
                            };

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
                            select new
                            {
                                Nickname1 = g.Nickname,
                                Nickname2 = o.Nickname
                            };

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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_string(bool isAsync)
        {
            return AssertIncludeQuery<LocustLeader>(
                isAsync,
                lls => lls.Include("DefeatedBy"),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_string_nested1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy"),
                new ExpectedInclude<Gear>(g => g.Squad, "Squad", "DefeatedBy")
            };

            return AssertIncludeQuery<LocustLeader>(
                isAsync,
                lls => lls.Include("DefeatedBy.Squad"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_string_nested2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy"),
                new ExpectedInclude<Officer>(o => o.Reports, "Reports", "DefeatedBy"),
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "CityOfBirth", "DefeatedBy.Reports")
            };

            return AssertIncludeQuery<LocustLeader>(
                isAsync,
                lls => lls.Include("DefeatedBy.Reports.CityOfBirth"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_lambda(bool isAsync)
        {
            return AssertIncludeQuery<LocustLeader>(
                isAsync,
                lls => lls.Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool isAsync)
        {
            return AssertIncludeQuery<LocustLeader>(
                isAsync,
                lls => lls.Include(ll => (ll as LocustCommander).DefeatedBy),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_lambda_with_tracking(bool isAsync)
        {
            return AssertIncludeQuery<LocustLeader>(
                isAsync,
                lls => lls.AsTracking().Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy")
                },
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_derived_type_using_string(bool isAsync)
        {
            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include("Reports"),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Reports, "Reports")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_derived_type_using_lambda(bool isAsync)
        {
            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => ((Officer)g).Reports),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Reports, "Reports")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool isAsync)
        {
            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => (g as Officer).Reports),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Reports, "Reports")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_base_navigation_on_derived_entity(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Tag, "Tag"),
                new ExpectedInclude<Officer>(e => e.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => ((Officer)g).Tag).Include(g => ((Officer)g).Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ThenInclude_collection_on_derived_after_base_reference(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(e => e.Gear, "Gear"),
                new ExpectedInclude<Officer>(e => e.Weapons, "Weapons", "Gear")
            };

            return AssertIncludeQuery<CogTag>(
                isAsync,
                ts => ts.Include(t => t.Gear).ThenInclude(g => (g as Officer).Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ThenInclude_collection_on_derived_after_derived_reference(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Commander, "Commander"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Commander"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Commander.DefeatedBy")
            };

            return AssertIncludeQuery<Faction>(
                isAsync,
                fs => fs.Include(f => (f as LocustHorde).Commander).ThenInclude(c => (c.DefeatedBy as Officer).Reports),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ThenInclude_collection_on_derived_after_derived_collection(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Reports, "Reports"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Reports")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => ((Officer)g).Reports).ThenInclude(g => ((Officer)g).Reports),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ThenInclude_reference_on_derived_after_derived_collection(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Leaders, "Leaders"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Leaders")
            };

            return AssertIncludeQuery<Faction>(
                isAsync,
                fs => fs.Include(f => ((LocustHorde)f).Leaders).ThenInclude(l => ((LocustCommander)l).DefeatedBy),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_derived_included_on_one_method(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Commander, "Commander"),
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy", "Commander"),
                new ExpectedInclude<Officer>(e => e.Reports, "Reports", "Commander.DefeatedBy")
            };

            return AssertIncludeQuery<Faction>(
                isAsync,
                fs => fs.Include(f => (((LocustHorde)f).Commander.DefeatedBy as Officer).Reports),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_derived_multi_level(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Reports, "Reports"),
                new ExpectedInclude<Gear>(e => e.Squad, "Squad", "Reports"),
                new ExpectedInclude<Squad>(e => e.Missions, "Missions", "Reports.Squad")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => ((Officer)g).Reports).ThenInclude(g => g.Squad.Missions),
                expectedIncludes);
        }

        [ConditionalFact(Skip = "Issue#15312")]
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_nullable_bool_in_conditional_works(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                cgs =>
                    cgs.Select(
                        cg =>
                            new
                            {
                                Prop = cg.Gear != null ? cg.Gear.HasSoulPatch : false
                            }),
                e => e.Prop);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enum_ToString_is_client_eval(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    gs.OrderBy(g => g.SquadId)
                        .ThenBy(g => g.Nickname)
                        .Select(g => g.Rank.ToString()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_naked_navigation_with_ToList(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select g.Weapons.ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        // issue #12579
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => (from g in gs
                       where g.Nickname != "Marcus"
                       orderby g.Nickname
                       select g.Weapons.ToList()).Select(e => e.Count),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_naked_navigation_with_ToArray(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select g.Weapons.ToArray(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_explicit_to_list(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_explicit_to_array(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToArray(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Weapon>(e => e.Id, (e, a) => Assert.Equal(e.Id, a.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_ordered(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_composite_key(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    where o.Nickname != "Foo"
                    select new
                    {
                        o.Nickname,
                        Collection = (from r in o.Reports
                                      where !r.HasSoulPatch
                                      select new
                                      {
                                          r.Nickname,
                                          r.FullName
                                      }).ToArray()
                    },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<dynamic>(elementSorter: ee => ee.FullName)(e.Collection, a.Collection);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projecting_single_property(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w.Name).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<string>(e => e));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projecting_constant(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select "BFG").ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<string>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projecting_constant_bool(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select true).ToList(),
                assertOrder: true,
                elementAsserter: CollectionAsserter<bool>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_projection_of_collection_thru_navigation(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_project_anonymous_collection_result(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => from s in ss
                      where s.Id < 20
                      select new
                      {
                          s.Name,
                          Collection = (from m in s.Members
                                        select new
                                        {
                                            m.FullName,
                                            m.Rank
                                        }).ToList()
                      },
                elementSorter: e => e.Name,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Name, a.Name);
                    CollectionAsserter<dynamic>(ee => ee.FullName + " " + ee.Rank)(e.Collection, a.Collection);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_with_custom_ordering(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs
                    .OfType<Officer>()
                    .OrderByDescending(o => o.HasSoulPatch)
                    .Select(
                        o => new
                        {
                            o.FullName,
                            OuterCollection = o.Reports
                                .Where(r => r.FullName != "Foo")
                                .OrderBy(r => r.Rank)
                                .Select(
                                    g => new
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_same_collection_projected_multiple_times(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_similar_collection_projected_multiple_times(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_different_collections_projected(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.FullName
                    select new
                    {
                        o.Nickname,
                        First = o.Weapons.Where(w => w.IsAutomatic).Select(
                            w => new
                            {
                                w.Name,
                                w.IsAutomatic
                            }).ToArray(),
                        Second = o.Reports.OrderBy(r => r.FullName).Select(
                            r => new
                            {
                                r.Nickname,
                                r.Rank
                            }).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<dynamic>()(e.First, a.First);
                    CollectionAsserter<dynamic>()(e.Second, a.Second);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.HasSoulPatch descending, o.Tag.Note
                    where o.Reports.Any()
                    select o.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.HasSoulPatch descending, o.Tag.Note
                    where o.Reports.Any()
                    select new
                    {
                        o.FullName,
                        OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                            orderby www.IsAutomatic, www.Owner.Nickname descending
                                            select www).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);

                    CollectionAsserter<dynamic>(
                        ee => ee.Id,
                        (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.OuterCollection2, a.OuterCollection2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.HasSoulPatch descending, o.Tag.Note
                    where o.Reports.Any()
                    select new
                    {
                        o.FullName,
                        OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                            orderby www.IsAutomatic, www.Owner.Nickname descending
                                            orderby www.IsAutomatic, www.Owner.Nickname descending
                                            select www).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);

                    CollectionAsserter<dynamic>(
                        ee => ee.Id,
                        (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.OuterCollection2, a.OuterCollection2);
                });
        }


        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.HasSoulPatch descending, o.Tag.Note
                    where o.Reports.Any()
                    select new
                    {
                        o.FullName,
                        OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                            orderby www.Id descending, www.Owner.Weapons.Count
                                            select www).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);

                    CollectionAsserter<dynamic>(
                        ee => ee.Id,
                        (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.OuterCollection2, a.OuterCollection2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_multiple_nested_complex_collections(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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
                                                                      InnerFirst = w.Owner.Weapons.Select(
                                                                          ww => new
                                                                          {
                                                                              ww.Name,
                                                                              ww.IsAutomatic
                                                                          }).ToList(),
                                                                      InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                                          mm => new
                                                                          {
                                                                              mm.Nickname,
                                                                              mm.HasSoulPatch
                                                                          }).ToList()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    select new
                    {
                        o.FullName,
                        Collection = from r in o.Reports
                                     select new
                                     {
                                         ReportName = r.FullName,
                                         OfficerName = o.FullName
                                     }
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<dynamic>(ee => ee.ReportName)(e.Collection, a.Collection);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    select new
                    {
                        o.FullName,
                        Collection = from r in o.Reports
                                     where o.FullName != "Foo"
                                     select new
                                     {
                                         ReportName = r.FullName
                                     }
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    CollectionAsserter<dynamic>(ee => ee.ReportName)(e.Collection, a.Collection);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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
                                           }).ToList()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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
                                          }
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_on_select_many(bool isAsync)
        {
            return AssertQuery<Gear, Squad>(
                isAsync,
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
                    CollectionAsserter<Gear>(ee => ee.Nickname, (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(
                        e.Collection2, a.Collection2);
                });
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_Skip(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Skip(1)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a));
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_Take(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Take(2)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_Distinct(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Distinct()),
                assertOrder: true,
                elementAsserter: (e, a) =>
                    CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname))(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Select(m => m.FullName).FirstOrDefault()),
                assertOrder: true,
                elementAsserter: (e, a) => CollectionAsserter<Gear>(elementAsserter: (ee, aa) => Assert.Equal(ee.Nickname, aa.Nickname)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_on_left_join_with_predicate(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    where !g.HasSoulPatch
                    select new
                    {
                        g.Nickname,
                        WeaponNames = g.Weapons.Select(w => w.Name).ToList()
                    },
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    where !MaybeScalar<bool>(g, () => g.HasSoulPatch) == true
                    select new
                    {
                        Nickname = Maybe(g, () => g.Nickname),
                        WeaponNames = g == null ? new List<string>() : g.Weapons.Select(w => w.Name)
                    },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<string>(ee => ee)(e.WeaponNames, a.WeaponNames);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_on_left_join_with_null_value(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_left_join_with_self_reference(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    from t in ts
                    join o in gs.OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                    from o in grouping.DefaultIfEmpty()
                    select new
                    {
                        t.Note,
                        ReportNames = o.Reports.Select(r => r.FullName).ToList()
                    },
                (ts, gs) =>
                    from t in ts
                    join o in gs.OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                    from o in grouping.DefaultIfEmpty()
                    select new
                    {
                        t.Note,
                        ReportNames = o != null ? o.Reports.Select(r => r.FullName) : new List<string>()
                    },
                elementSorter: e => e.Note,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Note, a.Note);
                    CollectionAsserter<string>(ee => ee)(e.ReportNames, a.ReportNames);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_deeply_nested_left_join(bool isAsync)
        {
            return AssertQuery<CogTag, Gear>(
                isAsync,
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note, g.Nickname descending
                    select g.Squad.Members.Where(m => m.HasSoulPatch).Select(
                        m => new
                        {
                            m.Nickname,
                            AutomaticWeapons = m.Weapons.Where(w => w.IsAutomatic).ToList()
                        }).ToList(),
                (ts, gs) =>
                    from t in ts
                    join g in gs on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note, Maybe(g, () => g.Nickname) descending
                    select g != null
                        ? g.Squad.Members.Where(m => m.HasSoulPatch).OrderBy(m => m.Nickname)
                            .Select(m => m.Weapons.Where(w => w.IsAutomatic))
                        : new List<List<Weapon>>(),
                assertOrder: true,
                elementAsserter: (e, a) =>
                    CollectionAsserter<dynamic>(
                        elementAsserter: (ee, aa) => CollectionAsserter<Weapon>(
                            eee => eee.Id, (eee, aaa) => Assert.Equal(eee.Id, aaa.Id))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.OrderBy(w => w.Name).Select(
                    w => w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(
                        m => new
                        {
                            Weapons = m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id).ToList(),
                            m.Rank
                        }).ToList()),
                ws => ws.OrderBy(w => w.Name).Select(
                    w => w.Owner != null
                        ? w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(
                            m => new Tuple<IEnumerable<Weapon>, MilitaryRank>(
                                m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id), m.Rank))
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_complex_scenario1(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from r in gs
                    select new
                    {
                        r.FullName,
                        OuterCollection = (from w in r.Weapons
                                           select new
                                           {
                                               w.Id,
                                               InnerCollection = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                   mm => new
                                                   {
                                                       mm.Nickname,
                                                       mm.HasSoulPatch
                                                   }).ToList()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_complex_scenario2(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
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
                                                                      InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                                          mm => new
                                                                          {
                                                                              mm.Nickname,
                                                                              mm.HasSoulPatch
                                                                          }).ToList()
                                                                  }).ToList()
                                           }).ToList()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_funky_orderby_complex_scenario1(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from r in gs
                    orderby r.FullName, r.Nickname descending, r.FullName
                    select new
                    {
                        r.FullName,
                        OuterCollection = (from w in r.Weapons
                                           select new
                                           {
                                               w.Id,
                                               InnerCollection = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                   mm => new
                                                   {
                                                       mm.Nickname,
                                                       mm.HasSoulPatch
                                                   }).ToList()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_funky_orderby_complex_scenario2(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    from o in gs.OfType<Officer>()
                    orderby o.HasSoulPatch, o.LeaderNickname, o.HasSoulPatch descending, o.LeaderNickname descending, o.FullName
                    select new
                    {
                        o.FullName,
                        OuterCollection = (from r in o.Reports
                                           orderby r.FullName, r.HasSoulPatch descending, r.FullName descending
                                           select new
                                           {
                                               r.FullName,
                                               InnerCollection = (from w in r.Weapons
                                                                  orderby w.IsAutomatic, w.Name descending, w.Name, w.IsAutomatic descending
                                                                      , w.IsAutomatic descending
                                                                  select new
                                                                  {
                                                                      w.Id,
                                                                      InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                                          mm => new
                                                                          {
                                                                              mm.Nickname,
                                                                              mm.HasSoulPatch
                                                                          }).ToList()
                                                                  }).ToList()
                                           }).ToList()
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
        public virtual void Correlated_collection_with_top_level_FirstOrDefault()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.OrderBy(g => g.Nickname).Select(g => g.Weapons).FirstOrDefault();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.Nickname).Select(g => g.Weapons).FirstOrDefault();

                Assert.Equal(expected.Count, actual.Count);

                var actualList = actual.ToList();
                var expectedList = expected.ToList();
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expectedList[i].Id, actualList[i].Id);
                    Assert.Equal(expectedList[i].Name, actualList[i].Name);
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_top_level_Count(bool isAsync)
        {
            return AssertCount<Gear>(
                isAsync,
                gs => gs.Select(g => g.Weapons));
        }

        [ConditionalFact]
        public virtual void Correlated_collection_with_top_level_Last_with_orderby_on_outer()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.OrderByDescending(g => g.FullName).Select(g => g.Weapons).Last();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderByDescending(g => g.FullName).Select(g => g.Weapons)
                    .Last();

                Assert.Equal(expected.Count, actual.Count);

                var actualList = actual.ToList();
                var expectedList = expected.ToList();
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expectedList[i].Id, actualList[i].Id);
                    Assert.Equal(expectedList[i].Name, actualList[i].Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Correlated_collection_with_top_level_Last_with_order_by_on_inner()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.OrderBy(g => g.FullName).Select(g => g.Weapons.OrderBy(w => w.Name).ToList()).Last();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.FullName)
                    .Select(g => g.Weapons.OrderBy(w => w.Name).ToList()).Last();

                Assert.Equal(expected.Count, actual.Count);

                var actualList = actual.ToList();
                var expectedList = expected.ToList();
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expectedList[i].Id, actualList[i].Id);
                    Assert.Equal(expectedList[i].Name, actualList[i].Name);
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_and_last()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.OrderByDescending(g => g.HasSoulPatch).Include(g => g.Weapons).Select(
                    g => new
                    {
                        g.Rank,
                        g
                    }).GroupBy(g => g.Rank).ToList().OrderBy(g => g.Key).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderByDescending(g => g.HasSoulPatch).Include(g => g.Weapons)
                    .Select(
                        g => new
                        {
                            g.Rank,
                            g
                        }).GroupBy(g => g.Rank).ToList().OrderBy(g => g.Key).ToList();

                Assert.Equal(expected.Count, actual.Count);

                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    var expectedInners = expected[i].ToList();
                    var actualInners = actual[i].ToList();

                    Assert.Equal(expectedInners.Count, actualInners.Count);
                    for (var j = 0; j < expectedInners.Count; j++)
                    {
                        Assert.Equal(expectedInners[j].g.Rank, actualInners[j].g.Rank);

                        var expectedWeapons = expectedInners[j].g.Weapons.OrderBy(w => w.Id).ToList();
                        var actualWeapons = actualInners[j].g.Weapons.OrderBy(w => w.Id).ToList();

                        Assert.Equal(expectedWeapons.Count, actualWeapons.Count);
                        for (var k = 0; k < expectedWeapons.Count; k++)
                        {
                            Assert.Equal(expectedWeapons[k].Id, actualWeapons[k].Id);
                            Assert.Equal(expectedWeapons[k].Name, actualWeapons[k].Name);
                        }
                    }
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_with_composite_group_key()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.OrderBy(g => g.Nickname).Include(g => g.Weapons).GroupBy(
                    g => new
                    {
                        g.Rank,
                        g.HasSoulPatch
                    }).ToList().OrderBy(g => g.Key.Rank).ThenBy(g => g.Key.HasSoulPatch).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.Nickname).Include(g => g.Weapons).GroupBy(
                    g => new
                    {
                        g.Rank,
                        g.HasSoulPatch
                    }).ToList().OrderBy(g => g.Key.Rank).ThenBy(g => g.Key.HasSoulPatch).ToList();

                Assert.Equal(expected.Count, actual.Count);

                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    var expectedInners = expected[i].ToList();
                    var actualInners = actual[i].ToList();

                    Assert.Equal(expectedInners.Count, actualInners.Count);
                    for (var j = 0; j < expectedInners.Count; j++)
                    {
                        Assert.Equal(expectedInners[j].Rank, actualInners[j].Rank);
                        Assert.Equal(expectedInners[j].HasSoulPatch, actualInners[j].HasSoulPatch);

                        var expectedWeapons = expectedInners[j].Weapons.OrderBy(w => w.Id).ToList();
                        var actualWeapons = actualInners[j].Weapons.OrderBy(w => w.Id).ToList();

                        Assert.Equal(expectedWeapons.Count, actualWeapons.Count);
                        for (var k = 0; k < expectedWeapons.Count; k++)
                        {
                            Assert.Equal(expectedWeapons[k].Id, actualWeapons[k].Id);
                            Assert.Equal(expectedWeapons[k].Name, actualWeapons[k].Name);
                        }
                    }
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_order_by_take()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.Include(g => g.Weapons).OrderBy(g => g.Nickname).Take(3).GroupBy(g => g.HasSoulPatch).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.Nickname).Take(3).GroupBy(g => g.HasSoulPatch)
                    .ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Count(), actual[i].Count());
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_distinct()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.Include(g => g.Weapons).OrderBy(g => g.Nickname).Distinct().GroupBy(g => g.HasSoulPatch).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.Nickname).Distinct()
                    .GroupBy(g => g.HasSoulPatch).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Count(), actual[i].Count());
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(bool isAsync)
        {
            return AssertQuery<LocustLeader, Faction>(
                isAsync,
                (lls, fs) =>
                    from ll in lls
                    join h in fs.OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName
                    where h.Eradicated != true
                    select h);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(bool isAsync)
        {
            return AssertQuery<LocustLeader, Faction>(
                isAsync,
                (lls, fs) =>
                    from ll in lls
                    join h in fs.OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName into grouping
                    from h in grouping.DefaultIfEmpty()
                    where h.Eradicated != true
                    select h,
                (lls, fs) =>
                    from ll in lls
                    join h in fs.OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName into grouping
                    from h in grouping.DefaultIfEmpty()
                    where MaybeScalar(h, () => h.Eradicated) != true
                    select h);
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_collection_group_by_reference()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Gear>()
                    .Include(g => g.Weapons)
                    .GroupBy(g => g.Squad)
                    .ToList();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_derived_type_with_order_by_and_paging(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustCommander>(e => e.DefeatedBy, "DefeatedBy"),
                new ExpectedInclude<Gear>(e => e.Weapons, "Weapons", "DefeatedBy")
            };

            return AssertIncludeQuery<LocustLeader>(
                isAsync,
                lls => lls.Include(ll => ((LocustCommander)ll).DefeatedBy).ThenInclude(g => g.Weapons)
                    .OrderBy(ll => ((LocustCommander)ll).DefeatedBy.Tag.Note).Take(10),
                lls => lls.Take(10),
                expectedIncludes,
                elementSorter: e => e.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_required_navigation_on_derived_type(bool isAsync)
        {
            return AssertQuery<LocustLeader>(
                isAsync,
                lls => lls.Select(ll => ((LocustCommander)ll).HighCommand.Name),
                lls => lls.Select(ll => ll is LocustCommander ? ((LocustCommander)ll).HighCommand.Name : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_required_navigation_on_the_same_type_with_cast(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Select(g => ((Gear)g).CityOfBirth.Name),
                gs => gs.Select(g => g.CityOfBirth.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_required_navigation_on_derived_type(bool isAsync)
        {
            return AssertQuery<LocustLeader>(
                isAsync,
                lls => lls.Where(ll => ((LocustCommander)ll).HighCommand.IsOperational),
                lls => lls.Where(ll => ll is LocustCommander ? ((LocustCommander)ll).HighCommand.IsOperational : false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_join_key(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_join_key_inner_and_outer(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    from o in gs.OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ts
                                      join g in gs on o.FullName equals o.Nickname
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => CollectionAsserter<string>(elementSorter: ee => ee)(e.Collection, a.Collection));
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_group_join_key(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_with_concat(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Squad, "Squad"),
                new ExpectedInclude<Officer>(o => o.Squad, "Squad")
            };

            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => g.Squad).Concat(gs),
                expectedIncludes))).Message;

            Assert.Equal(
                "When performing a set operation, both operands must have the same Include operations.",
                RemoveNewLines(message));
        }

        // issue #12889
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_concat(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => g.Weapons).Concat(gs),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                cts => cts.Select(
                    t => new
                    {
                        c = !(t.Gear.HasSoulPatch ? true : ((bool?)t.Gear.HasSoulPatch ?? true))
                    }),
                cts => cts.Select(
                    t => new
                    {
                        c = !(MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) ?? false
                            ? true
                            : (MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) ?? true))
                    }),
                elementSorter: e => e.c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_entity_qsre(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OrderBy(g => g.AssignedCity).ThenByDescending(g => g.Nickname).Select(f => f.FullName),
                gs => gs.OrderBy(g => Maybe(g.AssignedCity, () => g.AssignedCity.Name)).ThenByDescending(g => g.Nickname)
                    .Select(f => f.FullName),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_entity_qsre_with_inheritance(bool isAsync)
        {
            return AssertQuery<LocustLeader>(
                isAsync,
                lls => lls.OfType<LocustCommander>().OrderBy(lc => lc.HighCommand).ThenBy(lc => lc.Name).Select(lc => lc.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_entity_qsre_composite_key(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.OrderBy(w => w.Owner).ThenBy(w => w.Id).Select(w => w.Name),
                ws => ws.OrderBy(w => Maybe(w.Owner, () => w.Owner.Nickname)).ThenBy(w => MaybeScalar<int>(w.Owner, () => w.Owner.SquadId))
                    .ThenBy(w => w.Id).Select(w => w.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_entity_qsre_with_other_orderbys(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.OrderBy(w => w.IsAutomatic).ThenByDescending(w => w.Owner).ThenBy(w => w.SynergyWith).ThenBy(w => w.Name),
                ws => ws
                    .OrderBy(w => w.IsAutomatic)
                    .ThenByDescending(w => Maybe(w.Owner, () => w.Owner.Nickname))
                    .ThenByDescending(w => MaybeScalar<int>(w.Owner, () => w.Owner.SquadId))
                    .ThenBy(w => MaybeScalar<int>(w.SynergyWith, () => w.SynergyWith.Id))
                    .ThenBy(w => w.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => from w1 in ws
                      join w2 in ws on w1 equals w2
                      select new
                      {
                          Name1 = w1.Name,
                          Name2 = w2.Name
                      },
                elementSorter: e => e.Name1 + " " + e.Name2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_composite_key(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g1 in gs
                      join g2 in gs on g1 equals g2
                      select new
                      {
                          GearName1 = g1.FullName,
                          GearName2 = g2.FullName
                      },
                elementSorter: e => e.GearName1 + " " + e.GearName2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inheritance(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      join o in gs.OfType<Officer>() on g equals o
                      select new
                      {
                          GearName = g.FullName,
                          OfficerName = o.FullName
                      },
                elementSorter: e => e.GearName + " " + e.OfficerName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => from w1 in ws
                      join w2 in ws on w1.SynergyWith equals w2
                      select new
                      {
                          Name1 = w1.Name,
                          Name2 = w2.Name
                      },
                elementSorter: e => e.Name1 + " " + e.Name2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool isAsync)
        {
            return AssertQuery<City, Gear>(
                isAsync,
                (cs, gs) =>
                    from c in cs
                    join g in gs on c equals g.AssignedCity
                    select new
                    {
                        CityName = c.Name,
                        GearNickname = g.Nickname
                    },
                e => e.CityName + " " + e.GearNickname);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool isAsync)
        {
            return AssertQuery<Gear, CogTag>(
                isAsync,
                (gs, ts) =>
                    from g in gs
                    join t in ts.Where(tt => tt.Note == "Cole's Tag" || tt.Note == "Dom's Tag") on g equals t.Gear
                    select new
                    {
                        g.Nickname,
                        t.Note
                    },
                elementSorter: e => e.Nickname + " " + e.Note);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool isAsync)
        {
            return AssertQuery<Squad, Weapon>(
                isAsync,
                (ss, ws) =>
                    from s in ss
                    join w in ws.Where(ww => ww.IsAutomatic) on s equals w.Owner.Squad
                    select new
                    {
                        SquadName = s.Name,
                        WeaponName = w.Name
                    },
                elementSorter: e => e.SquadName + " " + e.WeaponName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool isAsync)
        {
            return AssertQuery<Squad, Weapon>(
                isAsync,
                (ss, ws) =>
                    from s in ss
                    join w in ws on s equals w.Owner.Squad into grouping
                    from w in grouping.DefaultIfEmpty()
                    select new
                    {
                        SquadName = s.Name,
                        WeaponName = w.Name
                    },
                (ss, ws) =>
                    from s in ss
                    join w in ws on s equals Maybe(w.Owner, () => w.Owner.Squad) into grouping
                    from w in grouping.DefaultIfEmpty()
                    select new
                    {
                        SquadName = s.Name,
                        WeaponName = Maybe(w, () => w.Name)
                    },
                elementSorter: e => e.SquadName + " " + e.WeaponName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_with_complex_key_selector(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Squad, CogTag, Gear>(
                isAsync,
                (ss, ts, gs) => ss
                    .Join(
                        ts.Where(t => t.Note == "Marcus' Tag"), o => true, i => true, (o, i) => new
                        {
                            o,
                            i
                        })
                    .GroupJoin(
                        gs,
                        oo => oo.o.Members.FirstOrDefault(v => v.Tag == oo.i),
                        ii => ii,
                        (k, g) => new
                        {
                            k.o,
                            k.i,
                            value = g.OrderBy(gg => gg.FullName).FirstOrDefault()
                        })
                    .Select(
                        r => new
                        {
                            r.o.Id,
                            TagId = r.i.Id
                        }),
                elementSorter: e => e.Id + " " + e.TagId))).Message;

            Assert.Equal(
                "This query would cause multiple evaluation of a subquery because entity 'Gear' has a composite key. Rewrite your query avoiding the subquery.",
                RemoveNewLines(message));
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_on_entity_qsre()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads.Include(s => s.Members).GroupBy(s => s);
                var results = query.ToList();

                foreach (var result in results)
                {
                    foreach (var grouping in result)
                    {
                        Assert.True(grouping.Members.Count > 0);
                    }
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_on_entity_qsre_with_composite_key()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Include(g => g.Weapons).GroupBy(g => g);
                var results = query.ToList();

                foreach (var result in results)
                {
                    foreach (var grouping in result)
                    {
                        Assert.True(grouping.Weapons.Count > 0);
                    }
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_on_entity_navigation()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Include(g => g.Weapons).Where(g => !g.HasSoulPatch).GroupBy(g => g.Squad);
                var results = query.ToList();

                foreach (var result in results)
                {
                    foreach (var grouping in result)
                    {
                        Assert.True(grouping.Weapons.Count > 0);
                    }
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_on_entity_navigation_with_inheritance()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Factions.OfType<LocustHorde>().Include(lh => lh.Leaders).GroupBy(lh => lh.Commander.DefeatedBy);
                var results = query.ToList();

                foreach (var result in results)
                {
                    foreach (var grouping in result)
                    {
                        Assert.True(grouping.Leaders.Count > 0);
                    }
                }
            }
        }

        [ConditionalFact(Skip = "Issue#16314")]
        public virtual void Streaming_correlated_collection_issue_11403()
        {
            Weapon[] expected;
            using (var context = CreateContext())
            {
                expected = context.Gears.OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.Where(w => !w.IsAutomatic).OrderBy(w => w.Id).ToArray())
                    .FirstOrDefault();

                ClearLog();
            }

            using (var context = CreateContext())
            {
                var query = context.Gears
                    .OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.Where(w => !w.IsAutomatic).OrderBy(w => w.Id))
                    .FirstOrDefault()
                    .ToArray();

                Assert.Equal(expected.Length, query.Length);

                for (var i = 0; i < expected.Length; i++)
                {
                    Assert.Equal(expected[i].Id, query[i].Id);
                    Assert.Equal(expected[i].AmmunitionType, query[i].AmmunitionType);
                    Assert.Equal(expected[i].IsAutomatic, query[i].IsAutomatic);
                    Assert.Equal(expected[i].Name, query[i].Name);
                    Assert.Equal(expected[i].Owner, query[i].Owner);
                    Assert.Equal(expected[i].OwnerFullName, query[i].OwnerFullName);
                    Assert.Equal(expected[i].SynergyWith, query[i].SynergyWith);
                    Assert.Equal(expected[i].SynergyWithId, query[i].SynergyWithId);
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_one_value_type_from_empty_collection(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Where(s => s.Name == "Kilo").Select(
                    s => new
                    {
                        s.Name,
                        SquadId = s.Members.Where(m => m.HasSoulPatch).Select(m => m.SquadId).FirstOrDefault()
                    }));
        }

        [ConditionalTheory(Skip = "issue #15864")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Where(s => s.Name == "Kilo")
                    .Where(s => s.Members.Where(m => m.HasSoulPatch).Select(m => m.SquadId).FirstOrDefault() != 0).Select(s => s.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_int(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => 42).FirstOrDefault()
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_string(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => "Foo").FirstOrDefault()
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_bool(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => true).FirstOrDefault()
                    }));
        }

        [ConditionalTheory(Skip = "Issue#17287")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_inside_anonymous(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(
                            g => new
                            {
                                One = 1
                            }).FirstOrDefault()
                    }));
        }

        [ConditionalTheory(Skip = "Issue#17287")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(
                            g => new
                            {
                                True = true,
                                False = false
                            }).FirstOrDefault()
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_order_by_constant(bool isAsync)
        {
            return AssertIncludeQuery<Squad>(
                isAsync,
                ss => ss.Include(s => s.Members).OrderBy(s => 42).Select(s => s),
                expectedQuery: ss => ss,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Squad>(s => s.Members, "Members")
                });
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_groupby_constant()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads.Include(s => s.Members).GroupBy(s => 1);
                var result = query.ToList();

                Assert.Equal(1, result.Count);
                var bucket = result[0].ToList();
                Assert.Equal(2, bucket.Count);
                Assert.NotNull(bucket[0].Members);
                Assert.NotNull(bucket[1].Members);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_order_by_constant(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OrderByDescending(s => 1).Select(
                    g => new
                    {
                        g.Nickname,
                        Weapons = g.Weapons.Select(w => w.Name).ToList()
                    }),
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<string>(ee => ee)(e.Weapons, a.Weapons);
                });
        }

        public class MyDTO
        {
        }

        [ConditionalTheory(Skip = "issue #15862")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => (MyDTO)null).FirstOrDefault()
                    }));
        }

        [ConditionalTheory(Skip = "issue #15862")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool isAsync)
        {
            return AssertQuery<Squad>(
                isAsync,
                ss => ss.Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => new MyDTO()).FirstOrDefault()
                    }),
                elementSorter: e => e.Name,
                elementAsserter: (e, a) => Assert.Equal(e.Name, a.Name));
        }

        [ConditionalTheory(Skip = "issue #11567")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_order_by_constant_null_of_non_mapped_type(bool isAsync)
        {
            return AssertIncludeQuery<Squad>(
                isAsync,
                ss => ss.Include(s => s.Members).OrderBy(s => (MyDTO)null),
                expectedQuery: ss => ss,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Squad>(s => s.Members, "Members")
                });
        }

        [ConditionalFact(Skip = "issue #11567")]
        public virtual void Include_groupby_constant_null_of_non_mapped_type()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads.Include(s => s.Members).GroupBy(s => (MyDTO)null);
                var result = query.ToList();

                Assert.Equal(1, result.Count);
                var bucket = result[0].ToList();
                Assert.Equal(2, bucket.Count);
                Assert.NotNull(bucket[0].Members);
                Assert.NotNull(bucket[1].Members);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Gear>(
                isAsync,
                gs => gs.OrderByDescending(s => (MyDTO)null).Select(
                    g => new
                    {
                        g.Nickname,
                        Weapons = g.Weapons.Select(w => w.Name).ToList()
                    }),
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<string>()(e.Weapons, a.Weapons);
                }))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("OrderByDescending<Gear, MyDTO>(    source: DbSet<Gear>,     keySelector: (g) => null)"),
                RemoveNewLines(message));
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void GroupBy_composite_key_with_Include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Include(o => o.Weapons).GroupBy(
                    o => new
                    {
                        o.Rank,
                        One = 1,
                        o.Nickname
                    });
                var result = query.ToList();

                Assert.Equal(5, result.Count);
                foreach (var bucket in result)
                {
                    foreach (var gear in bucket)
                    {
                        Assert.True(gear.Weapons.Count > 0);
                    }
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_OrderBy_aggregate(bool isAsync)
        {
            return AssertIncludeQuery<Gear>(
                isAsync,
                os => os.OfType<Officer>()
                    .Include(o => o.Reports)
                    .OrderBy(o => o.Weapons.Count),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Reports, "Reports")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_complex_OrderBy2(bool isAsync)
        {
            return AssertIncludeQuery<Gear>(
                isAsync,
                os => os.OfType<Officer>()
                    .Include(o => o.Reports)
                    .OrderBy(o => o.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic).ThenBy(o => o.Nickname),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Reports, "Reports")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_complex_OrderBy3(bool isAsync)
        {
            return AssertIncludeQuery<Gear>(
                isAsync,
                os => os.OfType<Officer>()
                    .Include(o => o.Reports)
                    .OrderBy(o => o.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Officer>(o => o.Reports, "Reports")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_complex_OrderBy(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OfType<Officer>()
                    .OrderBy(o => o.Weapons.Count)
                    .Select(o => o.Reports.Where(g => !g.HasSoulPatch).ToList()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(ee => ee.FullName, (ee, aa) => Assert.Equal(ee.FullName, aa.FullName)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_very_complex_order_by(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OfType<Officer>()
                    .OrderBy(
                        o => o.Weapons.Where(
                                w => w.IsAutomatic == gs.Where(g => g.Nickname == "Marcus").Select(g => g.HasSoulPatch).FirstOrDefault())
                            .Count())
                    .Select(o => o.Reports.Where(g => !g.HasSoulPatch).ToList()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(ee => ee.FullName, (ee, aa) => Assert.Equal(ee.FullName, aa.FullName)));
        }

        [ConditionalFact]
        public virtual void Cast_to_derived_type_causes_client_eval()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidCastException>(
                    () => context.Gears.Cast<Officer>().ToList());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_to_derived_type_after_OfType_works(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.OfType<Officer>().Cast<Officer>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_inside_cast_and_coalesce(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.Weapons.OrderBy(w => w.Id).Select(w => (int?)w.Id).FirstOrDefault() ?? 42));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_outside_cast_and_coalesce(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => (int?)g.Weapons.OrderBy(w => w.Id).Select(w => w.Id).FirstOrDefault() ?? 42));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_pushdown_and_coalesce(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => (int?)g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id ?? 42));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_pushdown_and_coalesce2(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => (int?)g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id ?? g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => (bool?)g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).FirstOrDefault().IsAutomatic),
                gs => gs.Select(g => (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty_with_pushdown_without_convert_to_nullable1(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).FirstOrDefault().IsAutomatic),
                gs => gs.Select(g => false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty_with_pushdown_without_convert_to_nullable2(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).FirstOrDefault().Id),
                gs => gs.Select(g => 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean1(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch).Select(
                    g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().Select(w => w.IsAutomatic).SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean2(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch).Select(
                    g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Select(w => w.IsAutomatic).Distinct().SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch).Select(
                    g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_empty1(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch).Select(
                    g => g.Weapons.Where(w => w.Name == "BFG").Distinct().Select(w => w.IsAutomatic).SingleOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_empty2(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch).Select(
                    g => g.Weapons.Where(w => w.Name == "BFG").Select(w => w.IsAutomatic).Distinct().SingleOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Where(g => g.HasSoulPatch)
                    .Select(g => (bool?)g.Weapons.Where(w => w.Name == "BFG").Distinct().SingleOrDefault().IsAutomatic),
                gs => gs.Where(g => g.HasSoulPatch).Select(g => (bool?)null),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_subquery_to_base_type_using_typed_ToList(bool isAsync)
        {
            return AssertQuery<City>(
                isAsync,
                cs => cs.Where(c => c.Name == "Ephyra").Select(
                    c => c.StationedGears.Select(
                        g => new Officer
                        {
                            CityOrBirthName = g.CityOrBirthName,
                            FullName = g.FullName,
                            HasSoulPatch = g.HasSoulPatch,
                            LeaderNickname = g.LeaderNickname,
                            LeaderSquadId = g.LeaderSquadId,
                            Nickname = g.Nickname,
                            Rank = g.Rank,
                            SquadId = g.SquadId
                        }).ToList<Gear>()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool isAsync)
        {
            return AssertQuery<City>(
                isAsync,
                cs => cs.Where(c => c.Name == "Ephyra").Select(
                    c => c.StationedGears.OrderByDescending(g => g.Nickname).Select(
                        g => new Officer
                        {
                            CityOrBirthName = g.CityOrBirthName,
                            FullName = g.FullName,
                            HasSoulPatch = g.HasSoulPatch,
                            LeaderNickname = g.LeaderNickname,
                            LeaderSquadId = g.LeaderSquadId,
                            Nickname = g.Nickname,
                            Rank = g.Rank,
                            SquadId = g.SquadId
                        }).ToArray<Gear>()),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Gear>(e => e.Nickname, (e, a) => Assert.Equal(e.Nickname, a.Nickname)));
        }

        [ConditionalTheory(Skip = "Issue#15713")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool isAsync)
        {
            var nicknames = new List<string>();
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      orderby nicknames.Contains(g.Nickname) descending
                      select new { g.Nickname, Weapons = g.Weapons.Select(w => w.Name).ToList() },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    CollectionAsserter<string>(ee => ee)(e.Weapons, a.Weapons);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w.IsAutomatic).OrderBy(w => w.IsAutomatic).ThenBy(w => w.Id),
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w != null ? w.IsAutomatic : false).ThenBy(w => w != null ? (int?)w.Id : null),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_Like(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => EF.Functions.Like(w.Name, "%Lancer"))
                    .OrderBy(w => EF.Functions.Like(w.Name, "%Lancer")).Select(w => w),
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name.EndsWith("Lancer") : false).Select(w => w));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_is_null(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w.Name == null).OrderBy(w => w.Name == null).Select(w => w),
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name == null : false).Select(w => w));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_string_compare(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0).OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0).ThenBy(w => w.Id),
                ws => ws.OrderBy(w => w != null ? w.Name.CompareTo("Marcus' Lancer") == 0 : false).ThenBy(w => w.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_binary_expression(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.OrderBy(w => w.Id + 2).OrderBy(w => w.Id + 2).Select(w => new { Binary = w.Id + 2 }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_compare_with_null_conditional_argument(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0).Select(c => c),
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name.CompareTo("Marcus' Lancer") == 0 : false).Select(c => c));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_compare_with_null_conditional_argument2(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => "Marcus' Lancer".CompareTo(w.Name) == 0).Select(w => w),
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w != null ? "Marcus' Lancer".CompareTo(w.Name) == 0 : false).Select(w => w));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_with_null_conditional_argument(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w.Name + 5),
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name + 5 : null),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_with_null_conditional_argument2(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => string.Concat(w.Name, "Marcus' Lancer")),
                ws => ws.Select(w => w.SynergyWith).OrderBy(w => w != null ? string.Concat(w.Name, "Marcus' Lancer") : null),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "issue #14205")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_on_various_types(bool isAsync)
        {
            return AssertQuery<Gear, Mission>(
                isAsync,
                (gs, ms) => from g in gs
                            from m in ms
                            orderby g.Nickname, m.Id
                            select new
                            {
                                HasSoulPatch = string.Concat("HasSoulPatch " + g.HasSoulPatch, " HasSoulPatch"),
                                Rank = string.Concat("Rank " + g.Rank, " Rank"),
                                SquadId = string.Concat("SquadId " + g.SquadId, " SquadId"),
                                Rating = string.Concat("Rating " + m.Rating, " Rating"),
                                Timeline = string.Concat("Timeline " + m.Timeline, " Timeline")
                            },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Time_of_day_datetimeoffset(bool isAsync)
        {
            return AssertQueryScalar<Mission>(
                isAsync,
                ms => from m in ms
                      select m.Timeline.TimeOfDay);
        }

        [ConditionalTheory(Skip = "issue #25249")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Average(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Average(gg => gg.SquadId)));
        }

        [ConditionalTheory(Skip = "issue #25249")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Sum(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Sum(gg => gg.SquadId)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Count(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_LongCount(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Max(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Max(gg => gg.SquadId)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Min(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Min(gg => gg.SquadId)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    gs.Include(g => g.CityOfBirth).GroupBy(g => g.Nickname).OrderBy(g => g.Key)
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count()
                            }),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "issue #12340")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs
                    .GroupBy(g => g.CityOfBirth)
                    .OrderBy(g => g.Key.Name)
                    .Select(g => g.Key)
                    .Include(c => c.BornGears).ThenInclude(g => g.Weapons),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Name, a.Name);

                    Assert.Equal(e.BornGears == null, a.BornGears == null);
                    Assert.Equal(e.BornGears.Count(), a.BornGears.Count());
                });
        }

        [ConditionalTheory(Skip = "issue #12340")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector_using_EF_Property(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs
                    .GroupBy(g => g.CityOfBirth)
                    .OrderBy(g => g.Key.Name)
                    .Select(g => EF.Property<City>(g, "Key"))
                    .Include(c => c.BornGears).ThenInclude(g => g.Weapons),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Name, a.Name);

                    Assert.Equal(e.BornGears == null, a.BornGears == null);
                    Assert.Equal(e.BornGears.Count(), a.BornGears.Count());
                });
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_with_include_with_entity_in_result_selector(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs =>
                    gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).OrderBy(g => g.Key)
                        .Select(
                            g => new
                            {
                                g.Key,
                                c = g.Count(),
                                element = g.OrderBy(gg => gg.Nickname).FirstOrDefault()
                            }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Key, a.Key);
                    Assert.Equal(e.c, a.c);
                    Assert.Equal(e.element.Nickname, a.element.Nickname);
                    Assert.Equal(e.element.CityOfBirth == null, a.element.CityOfBirth == null);
                    if (e.element.CityOfBirth != null)
                    {
                        Assert.Equal(e.element.CityOfBirth.Name, a.element.CityOfBirth.Name);
                    }
                });
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_group_by_and_FirstOrDefault_gets_properly_applied(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(e => e.CityOfBirth, "CityOfBirth"),
                new ExpectedInclude<Officer>(e => e.CityOfBirth, "CityOfBirth")
            };

            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.FirstOrDefault(gg => gg.HasSoulPatch)),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_Cast_to_base(bool isAsync)
        {
            return AssertIncludeQuery<Gear>(
                isAsync,
                gs => gs.OfType<Officer>().Include(o => o.Weapons).Cast<Gear>(),
                new List<IExpectedInclude> { new ExpectedInclude<Gear>(e => e.Weapons, "Weapons") });
        }

        [ConditionalFact]
        public virtual void Include_with_client_method_and_member_access_still_applies_includes()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .Include(g => g.Tag)
                    .Select(g => new { g.Nickname, Client(g).FullName });

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include_with_projection_of_unmapped_property_still_gets_applied()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Include(g => g.Weapons).Select(g => g.IsMarcus);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual Task Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads
                    .Include(s => s.Members)
                    .ThenInclude(g => g.Weapons)
                    .Where(s => s.Name == "Delta")
                    .Select(s => new { s.Name, Client(s).Members });

                var result = query.ToList();

                Assert.Equal(1, result.Count);

                var topLevel = result[0];

                Assert.Equal(4, topLevel.Members.Count);
                Assert.True(topLevel.Members.First().Weapons.Count > 0);
            }

            return Task.CompletedTask;
        }

        public static TEntity Client<TEntity>(TEntity entity) => entity;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(bool isAsync)
        {
            return AssertQueryScalar<Gear>(
                isAsync,
                gs => gs.Select(g => g.LeaderNickname != null ? (bool?)(g.Nickname.Length == 5) : (bool?)null).OrderBy(e => e.HasValue)
                    .ThenBy(e => e.HasValue).Select(e => e));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_projection(bool isAsync)
        {
            return AssertQueryScalar<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWithId.GetValueOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_filter(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.SynergyWithId.GetValueOrDefault() == 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_filter_non_nullable_column(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => ((int?)w.Id).GetValueOrDefault() == 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task GetValueOrDefault_on_DateTimeOffset(bool isAsync)
        {
            var defaultValue = default(DateTimeOffset);

            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery<Mission>(
                isAsync,
                ms => ms.Where(m => ((DateTimeOffset?)m.Timeline).GetValueOrDefault() == defaultValue)))).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => (Nullable<DateTimeOffset>)m.Timeline.GetValueOrDefault() == (Unhandled parameter: __defaultValue_0))"),
                RemoveNewLines(message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_order_by(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.OrderBy(w => w.SynergyWithId.GetValueOrDefault()).ThenBy(w => w.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_with_argument(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.SynergyWithId.GetValueOrDefault(w.Id) == 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_with_argument_complex(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Where(w => w.SynergyWithId.GetValueOrDefault(w.Name.Length + 42) > 10),
                ws => ws.Where(w => (w.SynergyWithId == null ? w.Name.Length + 42 : w.SynergyWithId) > 10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_with_complex_predicate_containing_subquery(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.FullName != "Dom" && g.Weapons.OrderBy(w => w.Id).FirstOrDefault(w => w.IsAutomatic) != null
                      select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => from g in gs
                      where g.Nickname != "Dom"
                      let automaticWeapons
                          = g.Weapons
                              .OrderByDescending(w => w.AmmunitionType)
                              .Where(w => w.IsAutomatic)
                      select new
                      {
                          g.Nickname,
                          WeaponName = automaticWeapons.FirstOrDefault().Name
                      },
                gs => from g in gs
                      where g.Nickname != "Dom"
                      let automaticWeapons
                          = g.Weapons
                              .OrderByDescending(w => w.AmmunitionType)
                              .Where(w => w.IsAutomatic)
                      select new
                      {
                          g.Nickname,
                          WeaponName = Maybe(automaticWeapons.FirstOrDefault(), () => automaticWeapons.FirstOrDefault().Name)
                      },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    Assert.Equal(e.WeaponName, a.WeaponName);
                });
        }

        [ConditionalTheory(Skip = "issue #13721")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(
            bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note.Substring(0, t.Gear.SquadId) == t.GearNickName),
                ts => ts.Where(t => Maybe(t.Gear, () => t.Note.Substring(0, t.Gear.SquadId)) == t.GearNickName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => ts.Where(t => t.Note.Substring(0, t.Gear.Squad.Name.Length) == t.GearNickName),
                ts => ts.Where(t => Maybe(t.Gear, () => t.Note.Substring(0, t.Gear.Squad.Name.Length)) == t.GearNickName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_with_new_Guid(bool isAsync)
        {
            return AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      where t.Id == new Guid("DF36F493-463F-4123-83F9-6B135DEEB7BA")
                      select t);
        }

        public virtual async Task Filter_with_new_Guid_closure(bool isAsync)
        {
            var guid = "DF36F493-463F-4123-83F9-6B135DEEB7BD";

            await AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      where t.Id == new Guid(guid)
                      select t);

            guid = "B39A6FBA-9026-4D69-828E-FD7068673E57";

            await AssertQuery<CogTag>(
                isAsync,
                ts => from t in ts
                      where t.Id == new Guid(guid)
                      select t);
        }

        [ConditionalFact]
        public virtual void OfTypeNav1()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Where(g => g.Tag.Note != "Foo").OfType<Officer>().Where(o => o.Tag.Note != "Bar");
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void OfTypeNav2()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Where(g => g.Tag.Note != "Foo").OfType<Officer>().Where(o => o.AssignedCity.Location != "Bar");
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void OfTypeNav3()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Where(g => g.Tag.Note != "Foo").Join(ctx.Weapons, g => g.FullName, w => w.OwnerFullName, (o, i) => o).OfType<Officer>().Where(o => o.Tag.Note != "Bar");
                var result = query.ToList();
            }
        }

        [ConditionalFact(Skip = "Issue #17234")]
        public virtual void Nav_rewrite_Distinct_with_convert()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.Factions.Include(f => ((LocustHorde)f).Commander)
                    .Where(f => f.Capital.Name != "Foo").Select(f => (LocustHorde)f)
                    .Distinct().Where(lh => lh.Commander.Name != "Bar").ToList();
            }
        }

        [ConditionalFact(Skip = "Issue #17234")]
        public virtual void Nav_rewrite_Distinct_with_convert_anonymous()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.Factions.Include(f => ((LocustHorde)f).Commander)
                    .Where(f => f.Capital.Name != "Foo").Select(f => new { horde = (LocustHorde)f })
                    .Distinct().Where(lh => lh.horde.Commander.Name != "Bar").ToList();
            }
        }

        [ConditionalFact]
        public virtual void Nav_rewrite_with_convert1()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Factions.Where(f => f.Capital.Name != "Foo").Select(f => ((LocustHorde)f).Commander);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Nav_rewrite_with_convert2()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Factions.Where(f => f.Capital.Name != "Foo").Select(f => (LocustHorde)f).Where(lh => lh.Commander.Name != "Bar");
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Nav_rewrite_with_convert3()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Factions.Where(f => f.Capital.Name != "Foo").Select(f => new { horde = (LocustHorde)f }).Where(x => x.horde.Commander.Name != "Bar");
                var result = query.ToList();
            }
        }

        [ConditionalTheory(Skip = "Issue#15260")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_contains_on_navigation_with_composite_keys(bool isAsync)
        {
            return AssertQuery<Gear, City>(
                isAsync,
                (gs, cs) => gs.Where(g => cs.Any(c => c.BornGears.Contains(g))));
        }

        [ConditionalFact]
        public virtual void Project_derivied_entity_with_convert_to_parent()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Factions.OfType<LocustHorde>().Select(f => (Faction)f);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include_with_complex_order_by()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Include(g => g.Weapons).OrderBy(g => g.Weapons.FirstOrDefault(w => w.Name.Contains("Gnasher")).Name).ThenBy(g => g.Nickname);
                var result = query.ToList();
            }
        }

        [ConditionalTheory(Skip = "issue #12603")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(bool isAsync)
        {
            return AssertQuery<Gear>(
                isAsync,
                gs => gs.Select(g => new { Gear = g }).Take(25).Select(e => e.Gear.Weapons.OrderBy(w => w.Id).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Bool_projection_from_subquery_treated_appropriately_in_where(bool isAsync)
        {
            return AssertQuery<City, Gear>(
                isAsync,
                (cs, gs) => cs.Where(c => gs.OrderBy(g => g.Nickname).ThenBy(g => g.SquadId).FirstOrDefault().HasSoulPatch));
        }

        [ConditionalTheory]  // issue #15208
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_Contains_Less_than_Greater_than(bool isAsync)
        {
            var dto = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0));
            var start = dto.AddDays(-1);
            var end = dto.AddDays(1);
            var dates = new DateTimeOffset[] { dto };

            return AssertQuery<Mission>(
                isAsync,
                ms => ms.Where(m => start <= m.Timeline.Date &&
                                    m.Timeline < end &&
                                    dates.Contains(m.Timeline)));
        }

        [ConditionalTheory]  // issue #16724
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_interpolated_string_expanded(bool isAsync)
        {
            return AssertQuery<Weapon>(
                isAsync,
                ws => ws.Select(w => w.SynergyWithId.HasValue ? $"SynergyWithOwner: {w.SynergyWith.OwnerFullName}" : string.Empty));
        }

        protected GearsOfWarContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
