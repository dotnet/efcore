// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g == new Gear()));
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Include(t => t.Gear.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToString_guid_property_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Select(
                    ct => new { A = ct.GearNickName, B = ct.Id.ToString() }),
                elementSorter: e => e.B,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.A, a.A);
                    Assert.Equal(e.B.ToLower(), a.B.ToLower());
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_one_to_one_and_one_to_many_self_reference(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(isAsync, ss => ss.Set<Weapon>().Include(w => w.Owner.Weapons)));
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Include(t => t.Gear.Squad),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_one_to_one_and_one_to_one_and_one_to_many(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(isAsync, ss => ss.Set<CogTag>().Include(t => t.Gear.Squad.Members)));
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth.StationedGears),
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth.StationedGears).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_using_alternate_key(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.Weapons).Where(g => g.Nickname == "Marcus"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_include_then_include(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Gear>()
                        .Include(g => g.AssignedCity.BornGears).ThenInclude(g => g.Tag)
                        .Include(g => g.AssignedCity.StationedGears).ThenInclude(g => g.Tag)
                        .Include(g => g.CityOfBirth.BornGears).ThenInclude(g => g.Tag)
                        .Include(g => g.CityOfBirth.StationedGears).ThenInclude(g => g.Tag)
                        .OrderBy(g => g.Nickname)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_navigation_on_derived_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>().Include(o => o.Reports),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_based_Include_navigation_on_derived_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>().Include("Reports"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Included(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<CogTag>(t => t.Gear, "Gear") };

            return AssertIncludeQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>().Include(o => o.Gear)
                      where t.Gear.Nickname == "Marcus"
                      select t,
                ss => from t in ss.Set<CogTag>()
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Join(
                    ss.Set<CogTag>(),
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Join(
                    ss.Set<Gear>(),
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    (t, g) => g).Include(g => g.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_collection1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Join(
                    ss.Set<CogTag>(),
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    (g, t) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_collection2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(o => o.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Join(
                    ss.Set<Gear>(),
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    (t, g) => g).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_where_list_contains_navigation(bool isAsync)
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var query = context.Gears
                    .Include(g => g.Tag)
                    .Where(g => g.Tag != null && tags.Contains(g.Tag.Id));

                var gears = isAsync
                    ? (await query.ToListAsync())
                    : query.ToList();

                Assert.Equal(5, gears.Count);

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_where_list_contains_navigation2(bool isAsync)
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var query = context.Gears
                    .Include(g => g.Tag)
                    .Where(g => g.CityOfBirth.Location != null && tags.Contains(g.Tag.Id));

                var gears = isAsync
                    ? (await query.ToListAsync())
                    : query.ToList();

                Assert.Equal(5, gears.Count);

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Navigation_accessed_twice_outside_and_inside_subquery(bool isAsync)
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var query = context.Gears
                    .Where(g => g.Tag != null && tags.Contains(g.Tag.Id));

                var gears = isAsync
                    ? (await query.ToListAsync())
                    : query.ToList();

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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Join(
                    ss.Set<CogTag>(),
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    (g, t) => g).Include(g => g.CityOfBirth.StationedGears),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.CityOfBirth, "CityOfBirth") };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Join(
                    ss.Set<Gear>().OfType<Officer>(),
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    o => new { SquadId = (int?)o.SquadId, o.Nickname },
                    (t, o) => o).Include(o => o.CityOfBirth),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Join(
                        ss.Set<Gear>().OfType<Officer>().OrderBy(ee => ee.SquadId),
                        t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                        o => new { SquadId = (int?)o.SquadId, o.Nickname },
                        (t, o) => o).OrderBy(ee => ee.FullName).Include(o => o.Reports).OrderBy(oo => oo.HasSoulPatch)
                    .ThenByDescending(oo => oo.Nickname),
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Weapons, "Weapons") };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>().Join(
                    ss.Set<CogTag>(),
                    o => new { SquadId = (int?)o.SquadId, o.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    (o, t) => o).Include(g => g.Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_join_and_inheritance3(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Join(
                    ss.Set<Gear>().OfType<Officer>(),
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    (t, o) => o).Include(o => o.Reports),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_nested_navigation_in_order_by(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Weapon>(w => w.Owner, "Owner") };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Include(w => w.Owner)
                    .Where(w => w.Owner.Nickname != "Paduk")
                    .OrderBy(e => e.Owner.CityOfBirth.Name).ThenBy(e => e.Id),
                ss => ss.Set<Weapon>()
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Rank == MilitaryRank.Sergeant));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_enum_with_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == AmmunitionType.Cartridge));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_enum_with_null_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_enum_with_non_nullable_parameter(bool isAsync)
        {
            var ammunitionType = AmmunitionType.Cartridge;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == ammunitionType));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_nullable_enum_with_nullable_parameter(bool isAsync)
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == ammunitionType));

            ammunitionType = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == ammunitionType));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_and_enum(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g.Rank & MilitaryRank.Corporal) > 0));

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_and_integral(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => ((int)g.Rank & 1) == 1));

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => ((long)g.Rank & 1L) == 1L));

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => ((short)g.Rank & (short)1) == 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_nullable_enum_with_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & AmmunitionType.Cartridge) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_nullable_enum_with_null_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
#pragma warning disable CS0458 // The result of the expression is always 'null'
                ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & null) > 0));
#pragma warning restore CS0458 // The result of the expression is always 'null'
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool isAsync)
        {
            var ammunitionType = AmmunitionType.Cartridge;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & ammunitionType) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool isAsync)
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & ammunitionType) > 0));

            ammunitionType = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & ammunitionType) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_or_enum(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g.Rank | MilitaryRank.Corporal) > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Bitwise_projects_values_in_select(bool isAsync)
        {
            return AssertFirst(
                isAsync,
                ss => ss.Set<Gear>()
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
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(MilitaryRank.Corporal)));

            // Expression
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(MilitaryRank.Corporal | MilitaryRank.Captain)));

            // Casting
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag((MilitaryRank)1)));

            // Casting to nullable
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag((MilitaryRank?)1)));

            // QuerySource
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => MilitaryRank.Corporal.HasFlag(g.Rank)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_enum_has_flag_subquery(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => g.Rank.HasFlag(
                        ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).Select(x => x.Rank).FirstOrDefault())));

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => MilitaryRank.Corporal.HasFlag(
                        ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).Select(x => x.Rank).FirstOrDefault())));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_enum_has_flag_subquery_with_pushdown(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => g.Rank.HasFlag(ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => MilitaryRank.Corporal.HasFlag(
                        ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_enum_has_flag_subquery_client_eval(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => g.Rank.HasFlag(ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).First().Rank)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_enum_has_flag_with_non_nullable_parameter(bool isAsync)
        {
            var parameter = MilitaryRank.Corporal;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(parameter)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_has_flag_with_nullable_parameter(bool isAsync)
        {
            MilitaryRank? parameter = MilitaryRank.Corporal;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(parameter)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_enum_has_flag(bool isAsync)
        {
            return AssertFirst(
                isAsync,
                ss => ss.Set<Gear>()
                    .Where(g => g.Rank.HasFlag(MilitaryRank.Corporal))
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(w => w.Weapons.Count == 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_any_subquery_without_collision(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(w => w.Weapons.Any()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_inverted_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Where(w => w.IsAutomatic)
                    .Select(
                        w => new { w.Id, Manual = !w.IsAutomatic }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_comparison_with_null(bool isAsync)
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(
                        w => new { w.Id, Cartridge = w.AmmunitionType == ammunitionType }),
                elementSorter: e => e.Id);

            ammunitionType = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(
                        w => new { w.Id, Cartridge = w.AmmunitionType == ammunitionType }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_null_parameter(bool isAsync)
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Select(
                        w => new { w.Id, AmmoType = ammunitionType }),
                elementSorter: e => e.Id);

            ammunitionType = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Select(
                        w => new { w.Id, AmmoType = ammunitionType }),
                elementSorter: e => e.Id);

            ammunitionType = AmmunitionType.Shell;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Select(
                        w => new { w.Id, AmmoType = ammunitionType }),
                elementSorter: e => e.Id);

            ammunitionType = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
                    .Select(
                        w => new { w.Id, AmmoType = ammunitionType }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_with_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(
                    w => new { w.Id, Num = w.IsAutomatic ? 1 : 0 }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_with_inverted_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(
                    w => new { w.Id, Num = !w.IsAutomatic ? 1 : 0 }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_with_has_value_not_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>()
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(
                    w => new { w.Id, IsCartridge = w.AmmunitionType == AmmunitionType.Shell && w.SynergyWithId == 1 ? "Yes" : "No" }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_ternary_operation_multiple_conditions_2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(
                    w => new { w.Id, IsCartridge = !w.IsAutomatic && w.SynergyWithId == 1 ? "Yes" : "No" }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_conditions(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(
                    w => new { w.Id, IsCartridge = !w.IsAutomatic && w.SynergyWithId == 1 }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_ternary_operations(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g == null ? null : g.LeaderNickname) == "Marcus" == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => (g.LeaderNickname == null ? (bool?)null : (bool?)g.LeaderNickname.EndsWith("us")) == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => (g.LeaderNickname != null ? (bool?)g.LeaderNickname.EndsWith("us") : (bool?)null) == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization4(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => (null == EF.Property<string>(g, "LeaderNickname") ? (int?)null : g.LeaderNickname.Length) == 5 == (bool?)true),
                ss => ss.Set<Gear>().Where(g => (null == g.LeaderNickname ? (int?)null : g.LeaderNickname.Length) == 5 == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization5(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => (null != g.LeaderNickname ? (int?)(EF.Property<string>(g, "LeaderNickname").Length) : (int?)null)
                        == 5
                        == (bool?)true),
                ss => ss.Set<Gear>().Where(
                    g => (null != g.LeaderNickname ? (int?)(g.LeaderNickname.Length) : (int?)null) == 5 == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_propagation_optimization6(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => (null != g.LeaderNickname ? (int?)EF.Property<string>(g, "LeaderNickname").Length : (int?)null)
                        == 5
                        == (bool?)true),
                ss => ss.Set<Gear>().Where(
                    g => (null != g.LeaderNickname ? (int?)g.LeaderNickname.Length : (int?)null) == 5 == (bool?)true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_optimization7(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Select(g => null != g.LeaderNickname ? g.LeaderNickname + g.LeaderNickname : null));
        }

        [ConditionalTheory(Skip = "issue #3836")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_optimization8(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g != null ? g.LeaderNickname + g.LeaderNickname : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_optimization9(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g != null ? (int?)g.FullName.Length : (int?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative1(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.LeaderNickname != null ? (bool?)(g.Nickname.Length == 5) : (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>()
                      from g2 in ss.Set<Gear>()
                      select g1.LeaderNickname != null ? g2.LeaderNickname : (string)null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>()
                      join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select new { g2.Nickname, Condition = g2 != null ? (bool?)(g2.LeaderNickname != null) : (bool?)null },
                ss => from g1 in ss.Set<Gear>()
                      join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
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
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>()
                      join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select g2 != null ? new Tuple<string, int>(g2.Nickname, 5) : null,
                ss => from g1 in ss.Set<Gear>()
                      join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby Maybe(g2, () => g2.Nickname)
                      select g2 != null ? new Tuple<string, int>(g2.Nickname, 5) : null,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative5(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>()
                      join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby g2.Nickname
                      select g2 != null
                          ? new { g2.Nickname, Five = 5 }
                          : null,
                ss => from g1 in ss.Set<Gear>()
                      join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      orderby Maybe(g2, () => g2.Nickname)
                      select g2 != null
                          ? new { Nickname = Maybe(g2, () => g2.Nickname), Five = 5 }
                          : null,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative6(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(
                    g => null != g.LeaderNickname
                        ? EF.Property<string>(g, "LeaderNickname").Length != EF.Property<string>(g, "LeaderNickname").Length
                        : (bool?)null),
                ss => ss.Set<Gear>().Select(
                    g => null != g.LeaderNickname ? g.LeaderNickname.Length != g.LeaderNickname.Length : (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative7(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => null != g.LeaderNickname ? g.LeaderNickname == g.LeaderNickname : (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_negative8(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Select(t => t.Gear.Squad != null ? t.Gear.AssignedCity.Name : null),
                ss => ss.Set<CogTag>().Select(
                    t => Maybe(t.Gear, () => t.Gear.Squad) != null
                        ? Maybe(t.Gear, () => Maybe(t.Gear.AssignedCity, () => t.Gear.AssignedCity.Name))
                        : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_works_for_navigations_with_composite_keys(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
#pragma warning disable IDE0031 // Use null propagation
                      select t.Gear != null ? t.Gear.Nickname : null,
#pragma warning restore IDE0031 // Use null propagation
                ss => from t in ss.Set<CogTag>()
                      select t.Gear != null ? Maybe(t.Gear, () => t.Gear.Nickname) : null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      select EF.Property<City>(EF.Property<CogTag>(t.Gear, "Tag").Gear, "AssignedCity") != null
                          ? EF.Property<string>(EF.Property<Gear>(t.Gear.Tag, "Gear").AssignedCity, "Name")
                          : null,
                ss => from t in ss.Set<CogTag>()
                      select Maybe(t.Gear, () => Maybe(t.Gear.Tag.Gear, () => t.Gear.Tag.Gear.AssignedCity)) != null
                          ? t.Gear.Tag.Gear.AssignedCity.Name
                          : null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_conditional_with_anonymous_type_and_null_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      orderby g.Nickname
                      select g.LeaderNickname != null
                          ? new { g.HasSoulPatch }
                          : null,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_conditional_with_anonymous_types(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      orderby g.Nickname
                      select g.LeaderNickname != null
                          ? new { Name = g.Nickname }
                          : new { Name = g.FullName },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_conditional_with_anonymous_type(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => from g in ss.Set<Gear>()
                          orderby g.Nickname
                          where (g.LeaderNickname != null
                                  ? new { g.HasSoulPatch }
                                  : null)
                              == null
                          select g.Nickname,
                    assertOrder: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_coalesce_with_anonymous_types(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      orderby g.Nickname
                      select new { Name = g.LeaderNickname } ?? new { Name = g.FullName },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_coalesce_with_anonymous_types(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => from g in ss.Set<Gear>()
                          where (new { Name = g.LeaderNickname } ?? new { Name = g.FullName }) != null
                          select g.Nickname));
        }

        [ConditionalTheory(Skip = "issue #8421")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_anonymous_types(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      from o in ss.Set<Gear>().OfType<Officer>()
                      where new
                      {
                          Name = g.LeaderNickname,
                          Squad = g.LeaderSquadId,
                          Five = 5
                      }
                          == new
                          {
                              Name = o.Nickname,
                              Squad = o.SquadId,
                              Five = 5
                          }
                      select g.Nickname);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_member_access_on_anonymous_type(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where new { Name = g.LeaderNickname, Squad = g.LeaderSquadId }.Name == "Marcus"
                      select g.Nickname);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_anonymous_types_with_uncorrelated_members(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                // ReSharper disable once EqualExpressionComparison
                ss => from g in ss.Set<Gear>()
                      where new { Five = 5 } == new { Five = 5 }
                      select g.Nickname);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where t.Gear.Nickname == "Marcus"
                      select t,
                ss => from t in ss.Set<CogTag>()
                      where Maybe(t.Gear, () => t.Gear.Nickname) == "Marcus"
                      select t);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t1 in ss.Set<CogTag>()
                      from t2 in ss.Set<CogTag>()
                      where t1.Gear.Nickname == t2.Gear.Nickname
                      select new { Tag1 = t1, Tag2 = t2 },
                ss => from t1 in ss.Set<CogTag>()
                      from t2 in ss.Set<CogTag>()
                      where Maybe(t1.Gear, () => t1.Gear.Nickname) == Maybe(t2.Gear, () => t2.Gear.Nickname)
                      select new { Tag1 = t1, Tag2 = t2 },
                elementSorter: e => (e.Tag1.Id, e.Tag2.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Tag1, a.Tag1);
                    AssertEqual(e.Tag2, a.Tag2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t1 in ss.Set<CogTag>()
                      from t2 in ss.Set<CogTag>()
                      where t1.Gear.Nickname == t2.Gear.Nickname
                      select new { Id1 = t1.Id, Id2 = t2.Id },
                ss => from t1 in ss.Set<CogTag>()
                      from t2 in ss.Set<CogTag>()
                      where Maybe(t1.Gear, () => t1.Gear.Nickname) == Maybe(t2.Gear, () => t2.Gear.Nickname)
                      select new { Id1 = t1.Id, Id2 = t2.Id },
                elementSorter: e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool isAsync)
        {
            return AssertFirst(
                isAsync,
                ss => ss.Set<Weapon>().OrderBy(w => w.Id).Select(
                    w => new Weapon { IsAutomatic = (bool?)w.SynergyWith.IsAutomatic ?? false }),
                ss => ss.Set<Weapon>().OrderBy(w => w.Id).Select(
                    w => new Weapon { IsAutomatic = MaybeScalar<bool>(w.SynergyWith, () => w.SynergyWith.IsAutomatic) ?? false }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_first_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.Nickname)
                    .Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).First().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_singleordefault_boolean1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Where(
                    g => g.HasSoulPatch
                        && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().Select(w => w.IsAutomatic)
                            .SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_singleordefault_boolean2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Where(
                    g => g.HasSoulPatch
                        && g.Weapons.Where(w => w.Name.Contains("Lancer")).Select(w => w.IsAutomatic).Distinct()
                            .SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Where(
                    g => g.HasSoulPatch && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_lastordefault_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Nickname)
                    .Where(g => !g.Weapons.Distinct().OrderBy(w => w.Id).LastOrDefault().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_last_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Nickname)
                    .Where(g => !g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Last().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_orderby_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory(Skip = "Issue#17759")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_union_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(
                    g => g.HasSoulPatch && g.Weapons.Union(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_subquery_concat_firstordefault_boolean(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Gear>().Where(
                        g => g.HasSoulPatch && g.Weapons.Concat(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic)))).Message;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_with_count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Gear>().Concat(ss.Set<Gear>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_scalars_with_count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.Nickname).Concat(ss.Set<Gear>().Select(g2 => g2.FullName)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_anonymous_with_count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Gear>()
                    .Select(
                        g => new { Gear = g, Name = g.Nickname })
                    .Concat(
                        ss.Set<Gear>().Select(
                            g2 => new { Gear = g2, Name = g2.FullName })));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_with_scalar_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Concat(ss.Set<Gear>()).Select(g => g.Nickname));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_with_groupings(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().GroupBy(g => g.LeaderNickname).Concat(ss.Set<Gear>().GroupBy(g => g.LeaderNickname)),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) => AssertGrouping(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_navigation_with_concat_and_count(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQueryScalar(
                    isAsync,
                    ss => ss.Set<Gear>().Where(g => !g.HasSoulPatch).Select(g => g.Weapons.Concat(g.Weapons).Count())))).Message;

            Assert.Equal(
                CoreStrings.QueryFailed(
                    @"(MaterializeCollectionNavigation(
    navigation: Navigation: Gear.Weapons,
    subquery: (NavigationExpansionExpression
        Source: DbSet<Weapon>
            .Where(w => EF.Property<string>(g, ""FullName"") != null && EF.Property<string>(g, ""FullName"") == EF.Property<string>(w, ""OwnerFullName""))
        PendingSelector: w => (NavigationTreeExpression
            Value: (EntityReference: Weapon)
            Expression: w)
    )
        .Where(i => EF.Property<string>((NavigationTreeExpression
            Value: (EntityReference: Gear)
            Expression: g), ""FullName"") != null && EF.Property<string>((NavigationTreeExpression
            Value: (EntityReference: Gear)
            Expression: g), ""FullName"") == EF.Property<string>(i, ""OwnerFullName"")))
    .AsQueryable()
    .Concat((MaterializeCollectionNavigation(
        navigation: Navigation: Gear.Weapons,
        subquery: (NavigationExpansionExpression
            Source: DbSet<Weapon>
                .Where(w0 => EF.Property<string>(g, ""FullName"") != null && EF.Property<string>(g, ""FullName"") == EF.Property<string>(w0, ""OwnerFullName""))
            PendingSelector: w0 => (NavigationTreeExpression
                Value: (EntityReference: Weapon)
                Expression: w0)
        )
            .Where(i => EF.Property<string>((NavigationTreeExpression
                Value: (EntityReference: Gear)
                Expression: g), ""FullName"") != null && EF.Property<string>((NavigationTreeExpression
                Value: (EntityReference: Gear)
                Expression: g), ""FullName"") == EF.Property<string>(i, ""OwnerFullName""))))", "NavigationExpandingExpressionVisitor"),
                message, ignoreLineEndingDifferences: true);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_concat_order_by_firstordefault_boolean(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().GroupBy(g => g.LeaderNickname).Concat(ss.Set<Gear>().GroupBy(g => g.LeaderNickname)),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) => AssertGrouping(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Concat_with_collection_navigations(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQueryScalar(
                    isAsync,
                    ss => ss.Set<Gear>().Where(g => g.HasSoulPatch).Select(g => g.Weapons.Union(g.Weapons).Count())))).Message;

            Assert.Equal(
                CoreStrings.QueryFailed(
                    @"(MaterializeCollectionNavigation(
    navigation: Navigation: Gear.Weapons,
    subquery: (NavigationExpansionExpression
        Source: DbSet<Weapon>
            .Where(w => EF.Property<string>(g, ""FullName"") != null && EF.Property<string>(g, ""FullName"") == EF.Property<string>(w, ""OwnerFullName""))
        PendingSelector: w => (NavigationTreeExpression
            Value: (EntityReference: Weapon)
            Expression: w)
    )
        .Where(i => EF.Property<string>((NavigationTreeExpression
            Value: (EntityReference: Gear)
            Expression: g), ""FullName"") != null && EF.Property<string>((NavigationTreeExpression
            Value: (EntityReference: Gear)
            Expression: g), ""FullName"") == EF.Property<string>(i, ""OwnerFullName"")))
    .AsQueryable()
    .Union((MaterializeCollectionNavigation(
        navigation: Navigation: Gear.Weapons,
        subquery: (NavigationExpansionExpression
            Source: DbSet<Weapon>
                .Where(w0 => EF.Property<string>(g, ""FullName"") != null && EF.Property<string>(g, ""FullName"") == EF.Property<string>(w0, ""OwnerFullName""))
            PendingSelector: w0 => (NavigationTreeExpression
                Value: (EntityReference: Weapon)
                Expression: w0)
        )
            .Where(i => EF.Property<string>((NavigationTreeExpression
                Value: (EntityReference: Gear)
                Expression: g), ""FullName"") != null && EF.Property<string>((NavigationTreeExpression
                Value: (EntityReference: Gear)
                Expression: g), ""FullName"") == EF.Property<string>(i, ""OwnerFullName""))))", "NavigationExpandingExpressionVisitor"),
                message, ignoreLineEndingDifferences: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Union_with_collection_navigations(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQueryScalar(
                    isAsync,
                    ss => ss.Set<Gear>().OfType<Officer>().Select(o => o.Reports.Union(o.Reports).Count())))).Message;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_firstordefault(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch).Select(g => g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Client(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => from t in ss.Set<CogTag>()
                          where t.Gear != null && t.Gear.IsMarcus
                          select t));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where t.Gear == null
                      select t);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Null_Reverse(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where null == t.Gear
                      select t);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Equals_Navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t1 in ss.Set<CogTag>()
                      from t2 in ss.Set<CogTag>()
                      where t1.Gear == t2.Gear
                      select new { t1, t2 },
                elementSorter: e => e.t1.Id + " " + e.t2.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.t1, a.t1);
                    AssertEqual(e.t2, e.t2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from ct in ss.Set<CogTag>()
                      where ct.Gear.Nickname == "Marcus"
                      where ct.Gear.CityOfBirthName != "Ephyra"
                      select new { B = ct.Gear.CityOfBirthName },
                ss => from ct in ss.Set<CogTag>()
                      where Maybe(ct.Gear, () => ct.Gear.Nickname) == "Marcus"
                      where Maybe(ct.Gear, () => ct.Gear.CityOfBirthName) != "Ephyra"
                      select new { B = Maybe(ct.Gear, () => ct.Gear.CityOfBirthName) },
                elementSorter: e => e.B);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from ct in ss.Set<CogTag>()
                      where ct.Gear.Nickname == "Marcus"
                      where ct.Gear.CityOfBirthName != "Ephyra"
                      select new { A = ct.Gear, B = ct.Gear.CityOfBirthName },
                ss => from ct in ss.Set<CogTag>()
                      where Maybe(ct.Gear, () => ct.Gear.Nickname) == "Marcus"
                      where Maybe(ct.Gear, () => ct.Gear.CityOfBirthName) != "Ephyra"
                      select new { A = ct.Gear, B = Maybe(ct.Gear, () => ct.Gear.CityOfBirthName) },
                elementSorter: e => e.A.Nickname,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.A, a.A);
                    Assert.Equal(e.B, e.B);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_Composite_Key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>()
                        on new { N = t.GearNickName, S = t.GearSquadId }
                        equals new { N = g.Nickname, S = (int?)g.SquadId } into grouping
                    from g in grouping
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_translated_to_subquery_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join t in ss.Set<CogTag>() on g.FullName equals t.Gear.FullName
                    select new { g.FullName, t.Note },
                ss =>
                    from g in ss.Set<Gear>()
                    join t in ss.Set<CogTag>() on g.FullName equals Maybe(t.Gear, () => t.Gear.FullName)
                    select new { g.FullName, t.Note },
                elementSorter: e => e.FullName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join t in ss.Set<CogTag>().OrderBy(tt => tt.Id) on g.FullName equals t.Gear.FullName
                    select new { g.FullName, t.Note },
                ss =>
                    from g in ss.Set<Gear>()
                    join t in ss.Set<CogTag>().OrderBy(tt => tt.Id) on g.FullName equals Maybe(t.Gear, () => t.Gear.FullName)
                    select new { g.FullName, t.Note },
                elementSorter: e => e.FullName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_order_by_without_skip_or_take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      join w in ss.Set<Weapon>().OrderBy(ww => ww.Name) on g.FullName equals w.OwnerFullName
                      select new { w.Name, g.FullName },
                elementSorter: w => w.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_order_by_without_skip_or_take_nested(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from s in ss.Set<Squad>()
                      join g in ss.Set<Gear>().OrderByDescending(gg => gg.SquadId) on s.Id equals g.SquadId
                      join w in ss.Set<Weapon>().OrderBy(ww => ww.Name) on g.FullName equals w.OwnerFullName
                      select new { w.Name, g.FullName },
                elementSorter: w => w.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_with_inheritance_and_join_include_joined(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => (from t in ss.Set<CogTag>()
                       join g in ss.Set<Gear>().OfType<Officer>() on new { id1 = t.GearSquadId, id2 = t.GearNickName }
                           equals new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                       select g).Include(g => g.Tag),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Tag, "Tag") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_with_inheritance_and_join_include_source(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => (from g in ss.Set<Gear>().OfType<Officer>()
                       join t in ss.Set<CogTag>() on new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                           equals new { id1 = t.GearSquadId, id2 = t.GearNickName }
                       select g).Include(g => g.Tag),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Tag, "Tag") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<City>()
                      where c.Location == "Unknown"
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<City>()
                      where "Unknown" == c.Location
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_parameter_is_used_for_non_unicode_column(bool isAsync)
        {
            var value = "Unknown";

            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<City>()
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

            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<City>()
                      where cities.Contains(c.Location)
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<City>()
                      where c.Location == "Unknown" && c.BornGears.Count(g => g.Nickname == "Paduk") == 1
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname == "Marcus" && g.CityOfBirth.Location == "Jacinto's location"
                      select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<City>()
                      where c.Location.Contains("Jacinto")
                      select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<City>()
                      where (c.Location + "Added").Contains("Add")
                      select c);
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
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                      join g2 in ss.Set<Gear>().Include(g => g.Weapons)
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
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                      join g2 in ss.Set<Gear>().Include(g => g.Weapons)
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
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                      join g2 in ss.Set<Gear>().OfType<Officer>().Include(g => g.Weapons)
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
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                      join g2 in ss.Set<Gear>().Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0029 // Use coalesce expression
                      select g2 != null ? g2 : g1,
#pragma warning restore IDE0029 // Use coalesce expression
                expectedIncludes);
        }

        [ConditionalTheory(Skip = "issue #16899")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Weapons, "Weapons"), new ExpectedInclude<Officer>(g => g.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                      join g2 in ss.Set<Gear>().Include(g => g.Weapons)
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => (bool?)w.IsAutomatic ?? false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Coalesce_operator_in_predicate_with_other_conditions(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Coalesce_operator_in_projection_with_other_conditions(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A." && t.Gear.HasSoulPatch));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Gear.HasSoulPatch),
                ss => ss.Set<CogTag>().Where(t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => !t.Gear.HasSoulPatch),
                ss => ss.Set<CogTag>().Where(t => !MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => !(t.Gear.HasSoulPatch ? true : t.Gear.HasSoulPatch)),
                ss => ss.Set<CogTag>().Where(
                    t => !(MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true
                            ? true
                            : MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch))
                        == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => !(!t.Gear.HasSoulPatch ? false : t.Gear.HasSoulPatch)),
                ss => ss.Set<CogTag>().Where(
                    t => !(MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == false
                            ? false
                            : MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch))
                        == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_conditional_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                // ReSharper disable once RedundantTernaryExpression
                ss => ss.Set<CogTag>().Where(t => t.Gear.HasSoulPatch ? true : false),
                // ReSharper disable once RedundantTernaryExpression
                ss => ss.Set<CogTag>().Where(t => (MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true) ? true : false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_binary_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Gear.HasSoulPatch || t.Note.Contains("Cole")),
                ss => ss.Set<CogTag>().Where(t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true || t.Note.Contains("Cole")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<CogTag>().Select(t => t.Gear.HasSoulPatch && t.Note.Contains("Cole")),
                ss => ss.Set<CogTag>().Select(
                    t => MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) == true && t.Note.Contains("Cole")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_projection(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(t => t.Gear.SquadId));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(
                    t => new { t.Gear.SquadId }),
                elementSorter: e => e.SquadId);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_DTOs(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(
                    t => new Squad { Id = t.Gear.SquadId }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_list_initializers(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note).Select(
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(t => new[] { t.Gear.SquadId }),
                elementSorter: e => e[0]);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_orderby(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Gear.SquadId).Select(t => t));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_groupby(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").GroupBy(t => t.Gear.SquadId),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) => AssertGrouping(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_all(bool isAsync)
        {
            return AssertAll(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A."),
                predicate: t => t.Gear.HasSoulPatch);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_negated_predicate(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Where(t => !t.Gear.HasSoulPatch));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_contains(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A." && ss.Set<Gear>().Select(g => g.SquadId).Contains(t.Gear.SquadId)));
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_skip(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note)
                    .Select(t => ss.Set<Gear>().OrderBy(g => g.Nickname).Skip(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_type_compensation_works_with_take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note)
                    .Select(t => ss.Set<Gear>().OrderBy(g => g.Nickname).Take(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_filtered_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Where(g => g.CityOfBirth.Name == "Ephyra" || g.CityOfBirth.Name == "Hanover")
                    .OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.Where(w => w.Name != "Lancer").ToList()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_filtered_collection_with_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>().OrderBy(g => g.Nickname)
                    .Select(g => g.Reports.Where(r => r.Nickname != "Dom").ToList()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_filtered_collection_works_with_caching(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().OrderBy(t => t.Note).Select(t => ss.Set<Gear>().Where(g => g.Nickname == t.GearNickName)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_predicate_value_equals_condition(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join w in ss.Set<Weapon>()
                        on true equals w.SynergyWithId != null
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_predicate_value(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join w in ss.Set<Weapon>()
                        on g.HasSoulPatch equals true
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_predicate_condition_equals_condition(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join w in ss.Set<Weapon>()
                        on g.FullName != null equals w.SynergyWithId != null
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_predicate_value_equals_condition(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join w in ss.Set<Weapon>()
                        on true equals w.SynergyWithId != null
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_predicate_value(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join w in ss.Set<Weapon>()
                        on g.HasSoulPatch equals true
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_predicate_condition_equals_condition(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join w in ss.Set<Weapon>()
                        on g.FullName != null equals w.SynergyWithId != null
                        into group1
                    from w in group1.DefaultIfEmpty()
                    select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_now(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline != DateTimeOffset.Now
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_utcnow(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline != DateTimeOffset.UtcNow
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_date_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Date > new DateTimeOffset().Date
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_year_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Year == 2
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_month_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Month == 1
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_dayofyear_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.DayOfYear == 2
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_day_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Day == 2
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_hour_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Hour == 10
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_minute_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Minute == 0
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_second_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Second == 0
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_millisecond_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      where m.Timeline.Millisecond == 0
                      select m);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddYears(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.AddYears(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddMonths(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.AddMonths(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddDays(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.AddDays(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddHours(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.AddHours(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddMinutes(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.AddMinutes(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddSeconds(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.AddSeconds(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_DateAdd_AddMilliseconds(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.AddMilliseconds(300));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_milliseconds_parameter_and_constant(bool isAsync)
        {
            var dateTimeOffset = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0));

            // Literal where clause
            var p = Expression.Parameter(typeof(Mission), "i");
            var dynamicWhere = Expression.Lambda<Func<Mission, bool>>(
                Expression.Equal(
                    Expression.Property(p, "Timeline"),
                    Expression.Constant(dateTimeOffset)
                ), p);

            return AssertCount(
                isAsync,
                ss => ss.Set<Mission>().Where(dynamicWhere),
                ss => ss.Set<Mission>().Where(m => m.Timeline == dateTimeOffset));
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(
            bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from w in ss.Set<Weapon>()
                      where w.Id != 50 && !w.Owner.HasSoulPatch
                      select w,
                ss => from w in ss.Set<Weapon>()
                      where w.Id != 50 && MaybeScalar<bool>(w.Owner, () => w.Owner.HasSoulPatch) == false
                      select w);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => (from g in ss.Set<Gear>()
                       where g.Tag.Note != "Foo"
                       select g.HasSoulPatch).Distinct(),
                ss => (from g in ss.Set<Gear>()
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.HasSoulPatch).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => (from g in ss.Set<Gear>()
                       where g.Tag.Note != "Foo"
                       select g.SquadId),
                ss => (from g in ss.Set<Gear>()
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.SquadId));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => (from g in ss.Set<Gear>()
                       where g.Tag.Note != "Foo"
                       select g.HasSoulPatch),
                ss => (from g in ss.Set<Gear>()
                       where Maybe(g.Tag, () => g.Tag.Note) != "Foo"
                       select g.HasSoulPatch));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_with_unflattened_groupjoin_is_evaluated_on_client(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().GroupJoin(
                        ss.Set<CogTag>(),
                        g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                        t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                        (g, t) => g.HasSoulPatch)
                    .Distinct());
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_unflattened_groupjoin_is_evaluated_on_client(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Gear>()
                    .GroupJoin(
                        ss.Set<CogTag>(),
                        g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                        t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                        (g, t) => g));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool isAsync)
        {
            return AssertFirstOrDefault(
                isAsync,
                ss => from s in ss.Set<Squad>()
                      join g in ss.Set<Gear>() on s.Id equals g.SquadId into grouping
                      from g in grouping.DefaultIfEmpty()
                      where s.Name == "Kilo"
                      select s);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from s in ss.Set<Squad>()
                      where !s.Members.Any(m => m.Tag.Note == "Dom's Tag")
                      select s.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_with_optional_navigation_is_translated_to_sql(bool isAsync)
        {
            return AssertAll(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      select g,
                predicate: g => g.Tag.Note != "Foo");
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<CogTag>().GroupJoin(
                    ss.Set<Gear>(),
                    t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                    g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                    (k, r) => r.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_side_equality_with_parameter_works_with_optional_navigations(bool isAsync)
        {
            var prm = "Marcus' Tag";

            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Gear>().Where(g => ClientEquals(g.Tag.Note, prm))));
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

            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(e => ids.Contains(e.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .Where(g => g.HasSoulPatch)
                    .Select(
                        g => new { FullName = EF.Property<string>(g, "FullName") }),
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .Where(g => g.HasSoulPatch)
                    .Select(
                        g => new { g.FullName }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .OrderByDescending(g => g.Nickname)
                    .Include(g => g.CityOfBirth)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Include(g => g.CityOfBirth)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_is_properly_lifted_from_subquery_created_by_include(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Where(g => g.FullName != "Augustus Cole")
                    .Include(g => g.Tag)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Gear>(e => e.Tag, "Tag") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_is_lifted_from_main_from_clause_of_SelectMany(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().OrderBy(g => g.Rank).Include(g => g.Tag)
                      from g2 in ss.Set<Gear>()
                      orderby g1.FullName
                      where g1.HasSoulPatch && !g2.HasSoulPatch
                      select new { Name1 = g1.FullName, Name2 = g2.FullName });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in (from gear in ss.Set<Gear>()
                               from tag in ss.Set<CogTag>()
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
            return AssertQuery(
                isAsync,
                ss =>
                    from g in (from gear in ss.Set<Gear>()
                               join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName
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
            return AssertQuery(
                isAsync,
                ss =>
                    from g in (from gear in ss.Set<Gear>()
                               join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName into grouping
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
            return AssertQuery(
                isAsync,
                ss =>
                    from gear in (from gear in ss.Set<Gear>()
                                  join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName
                                  orderby tag.Note
                                  where tag.GearNickName != "Cole Train"
                                  select gear).AsTracking()
                    join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName
                    orderby gear.Nickname, tag.Id
                    select gear.Nickname,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_created_by_include_gets_lifted_nested(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => from gear in ss.Set<Gear>().OrderBy(g => g.Rank).Where(g => g.Weapons.Any()).Include(g => g.CityOfBirth)
                      where !gear.HasSoulPatch
                      orderby gear.Nickname
                      select gear,
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Gear>(e => e.CityOfBirth, "CityOfBirth") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_is_lifted_from_additional_from_clause(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g1 in ss.Set<Gear>()
                    from g2 in ss.Set<Gear>().OrderBy(g => g.Rank).Include(g => g.Tag)
                    orderby g1.FullName
                    where g1.HasSoulPatch && !g2.HasSoulPatch
                    select new { Name1 = g1.FullName, Name2 = g2.FullName },
                elementSorter: e => (e.Name1, e.Name2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_with_result_operator_is_not_lifted(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Take(2).AsTracking()
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_with_orderby_followed_by_orderBy_is_pushed_down(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Skip(1)
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).Take(999).OrderBy(g => g.FullName)
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).Take(999)
                      orderby g.FullName
                      orderby g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).Take(999)
                      orderby g.FullName, g.Rank
                      select g.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_length_of_string_property(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from w in ss.Set<Weapon>()
                      select new { w.Name, w.Name.Length },
                elementSorter: e => e.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_on_collection_navigation_in_predicate(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => from g in ss.Set<Gear>()
                          where g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Marcus' Lancer"
                          select g.Nickname));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => from g in ss.Set<Gear>()
                          where !g.HasSoulPatch && FavoriteWeapon(EF.Property<List<Weapon>>(g, "Weapons")).Name == "Cole's Gnasher"
                          select g.Nickname,
                    ss => from g in ss.Set<Gear>()
                          where !g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Cole's Gnasher"
                          select g.Nickname));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_on_collection_navigation_in_order_by(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => from g in ss.Set<Gear>()
                          where !g.HasSoulPatch
                          orderby FavoriteWeapon(g.Weapons).Name descending
                          select g.Nickname,
                    assertOrder: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_method_on_collection_navigation_in_additional_from_clause(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => from g in ss.Set<Gear>().OfType<Officer>()
                          from v in Veterans(g.Reports)
                          select new { g = g.Nickname, v = v.Nickname },
                    elementSorter: e => e.g + e.v))).Message;

            Assert.StartsWith(
                CoreStrings.QueryFailed("", "").Substring(0, 35),
                message);
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_method_on_collection_navigation_in_outer_join_key(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => from o in ss.Set<Gear>().OfType<Officer>()
                          join g in ss.Set<Gear>() on FavoriteWeapon(o.Weapons).Name equals FavoriteWeapon(g.Weapons).Name
                          where o.HasSoulPatch
                          select new { o = o.Nickname, g = g.Nickname },
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_access_on_derived_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby ((LocustHorde)f).Name
                      select new { ((LocustHorde)f).Name, ((LocustHorde)f).Eradicated },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_access_on_derived_materialized_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby f.Name
                      select new { f, ((LocustHorde)f).Eradicated },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.f, a.f);
                    Assert.Equal(e.Eradicated, a.Eradicated);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_access_on_derived_entity_using_cast_and_let(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      let horde = (LocustHorde)f
                      orderby horde.Name
                      select new { horde.Name, horde.Eradicated },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Property_access_on_derived_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      let horde = (LocustHorde)f
                      orderby f.Name
                      select new
                      {
                          Name = EF.Property<string>(horde, "Name"),
                          Eradicated = EF.Property<bool>((LocustHorde)f, "Eradicated")
                      },
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      let horde = (LocustHorde)f
                      orderby f.Name
                      select new { horde.Name, Eradicated = (bool)((LocustHorde)f).Eradicated },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_access_on_derived_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby f.Name
                      select new { f.Name, Threat = ((LocustHorde)f).Commander.ThreatLevel },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_access_on_derived_materialized_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby f.Name
                      select new
                      {
                          f,
                          f.Name,
                          Threat = ((LocustHorde)f).Commander.ThreatLevel
                      },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.f, a.f);
                    Assert.Equal(e.Name, a.Name);
                    Assert.Equal(e.Threat, a.Threat);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_access_via_EFProperty_on_derived_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby f.Name
                      select new { f.Name, Threat = EF.Property<LocustCommander>((LocustHorde)f, "Commander").ThreatLevel },
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby f.Name
                      select new { f.Name, Threat = ((LocustHorde)f).Commander.ThreatLevel },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_access_fk_on_derived_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby f.Name
                      select new { f.Name, CommanderName = ((LocustHorde)f).Commander.Name },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_navigation_access_on_derived_entity_using_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      where f is LocustHorde
                      orderby f.Name
                      select new { f.Name, LeadersCount = ((LocustHorde)f).Leaders.Count },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>().Where(f => f is LocustHorde)
                      from l in ((LocustHorde)f).Leaders
                      orderby l.Name
                      select new { f.Name, LeaderName = l.Name },
                elementSorter: e => (e.Name, e.LeaderName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_derived_entity_using_OfType(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<LocustHorde>(e => e.Commander, "Commander"),
                new ExpectedInclude<LocustHorde>(e => e.Leaders, "Leaders")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => from lh in ss.Set<Faction>().OfType<LocustHorde>().Include(h => h.Commander).Include(h => h.Leaders)
                      orderby lh.Name
                      select lh,
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_on_derived_entity_with_cast(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude> { new ExpectedInclude<Faction>(e => e.Capital, "Capital") };

            // TODO: should we disable this scenario? see #14671
            return AssertIncludeQuery(
                isAsync,
                ss => (from f in ss.Set<Faction>()
                       where f is LocustHorde
                       select f).Include(f => f.Capital),
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_on_subquery_doesnt_get_lifted(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from g in (from ig in ss.Set<Gear>()
                                 select ig).Distinct()
                      select g.HasSoulPatch);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from lh in (from f in ss.Set<Faction>()
                                  select f).Cast<LocustHorde>()
                      select lh.Eradicated);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_two_collection_navigations_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>()
                      from g2 in ss.Set<Gear>()
                          // ReSharper disable once PossibleUnintendedReferenceComparison
                      where g1.Weapons == g2.Weapons
                      orderby g1.Nickname
                      select new { Nickname1 = g1.Nickname, Nickname2 = g2.Nickname },
                elementSorter: e => (e.Nickname1, e.Nickname2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_two_collection_navigations_inheritance(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from f in ss.Set<Faction>()
                      from o in ss.Set<Gear>().OfType<Officer>()
                      where f is LocustHorde && o.HasSoulPatch
                      // ReSharper disable once PossibleUnintendedReferenceComparison
                      where ((LocustHorde)f).Commander.DefeatedBy.Weapons == o.Weapons
                      select new { f.Name, o.Nickname },
                ss => from f in ss.Set<Faction>()
                      from o in ss.Set<Gear>().OfType<Officer>()
                      where f is LocustHorde && o.HasSoulPatch
                      where Maybe(
                              ((LocustHorde)f).Commander,
                              () => ((LocustHorde)f).Commander.DefeatedBy)
                          == o
                      select new { f.Name, o.Nickname },
                elementSorter: e => (e.Name, e.Nickname));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_entities_using_Equals_inheritance(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      from o in ss.Set<Gear>().OfType<Officer>()
                      where g.Equals(o)
                      orderby g.Nickname, o.Nickname
                      select new { Nickname1 = g.Nickname, Nickname2 = o.Nickname },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_on_nullable_array_produces_correct_sql(bool isAsync)
        {
            var cities = new[] { "Ephyra", null };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.SquadId < 2 && cities.Contains(g.AssignedCity.Name)),
                ss => ss.Set<Gear>().Where(g => g.SquadId < 2 && cities.Contains(Maybe(g.AssignedCity, () => g.AssignedCity.Name))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_collection_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Gear is Officer && ((Officer)t.Gear).Reports.Count(r => r.Nickname == "Dom") > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_conditional_with_inheritance(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>()
                    .Where(f => f is LocustHorde)
                    .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).CommanderName : null),
                ss => ss.Set<Faction>()
                    .Where(f => f is LocustHorde)
                    .Select(f => ((LocustHorde)f).CommanderName != null ? ((LocustHorde)f).CommanderName : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_conditional_with_inheritance_negative(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Faction>()
                    .Where(f => f is LocustHorde)
                    .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).Eradicated : null),
                ss => ss.Set<Faction>()
                    .Where(f => f is LocustHorde)
                    .Select(f => ((LocustHorde)f).CommanderName != null ? ((LocustHorde)f).Eradicated : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_with_inheritance1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().OfType<LocustHorde>()
                    .Select(
                        h => new { h.Id, Leaders = EF.Property<ICollection<LocustLeader>>(h.Commander.CommandingFaction, "Leaders") }),
                ss => ss.Set<Faction>().OfType<LocustHorde>()
                    .Select(
                        h => new { h.Id, Leaders = (ICollection<LocustLeader>)h.Commander.CommandingFaction.Leaders }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.Leaders, a.Leaders);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_with_inheritance2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().OfType<LocustHorde>()
                    .Select(
                        h => new { h.Id, Gears = EF.Property<ICollection<Gear>>((Officer)h.Commander.DefeatedBy, "Reports") }),
                ss => ss.Set<Faction>().OfType<LocustHorde>()
                    .Select(
                        h => new
                        {
                            h.Id,
                            Gears = Maybe(
                                    h.Commander,
                                    () => Maybe(
                                        h.Commander.DefeatedBy,
                                        () => ((Officer)h.Commander.DefeatedBy).Reports))
                                ?? new List<Gear>()
                        }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.Gears, a.Gears);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_with_inheritance3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>()
                    .Where(f => f is LocustHorde)
                    .Select(
                        f => new
                        {
                            f.Id,
                            Gears = EF.Property<ICollection<Gear>>((Officer)((LocustHorde)f).Commander.DefeatedBy, "Reports")
                        }),
                ss => ss.Set<Faction>()
                    .Where(f => f is LocustHorde)
                    .Select(
                        f => new
                        {
                            f.Id,
                            Gears = Maybe(
                                    ((LocustHorde)f).Commander,
                                    () => Maybe(
                                        ((LocustHorde)f).Commander.DefeatedBy,
                                        () => ((Officer)((LocustHorde)f).Commander.DefeatedBy).Reports))
                                ?? new List<Gear>()
                        }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.Gears, a.Gears);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_string(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Include("DefeatedBy"),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Include("DefeatedBy.Squad"),
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Include("DefeatedBy.Reports.CityOfBirth"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_lambda(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Include(ll => (ll as LocustCommander).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_on_derived_type_using_lambda_with_tracking(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().AsTracking().Include(ll => ((LocustCommander)ll).DefeatedBy),
                new List<IExpectedInclude> { new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy, "DefeatedBy") },
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_derived_type_using_string(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include("Reports"),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_derived_type_using_lambda(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => ((Officer)g).Reports),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => (g as Officer).Reports),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_base_navigation_on_derived_entity(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Officer>(e => e.Tag, "Tag"), new ExpectedInclude<Officer>(e => e.Weapons, "Weapons")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => ((Officer)g).Tag).Include(g => ((Officer)g).Weapons),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ThenInclude_collection_on_derived_after_base_reference(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<CogTag>(e => e.Gear, "Gear"), new ExpectedInclude<Officer>(e => e.Weapons, "Weapons", "Gear")
            };

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().Include(t => t.Gear).ThenInclude(g => (g as Officer).Weapons),
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Faction>().Include(f => (f as LocustHorde).Commander).ThenInclude(c => (c.DefeatedBy as Officer).Reports),
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => ((Officer)g).Reports).ThenInclude(g => ((Officer)g).Reports),
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Faction>().Include(f => ((LocustHorde)f).Leaders).ThenInclude(l => ((LocustCommander)l).DefeatedBy),
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Faction>().Include(f => (((LocustHorde)f).Commander.DefeatedBy as Officer).Reports),
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => ((Officer)g).Reports).ThenInclude(g => g.Squad.Missions),
                expectedIncludes);
        }

        [ConditionalTheory(Skip = "Issue#15312")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_collection_and_invalid_navigation_using_string_throws(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Gear>().Include("Reports.Foo")))).Message;

            Assert.Equal(
                CoreStrings.IncludeBadNavigation("Foo", "Gear"),
                message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_nullable_bool_in_conditional_works(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<CogTag>().Select(
                        cg =>
                            new { Prop = cg.Gear != null ? cg.Gear.HasSoulPatch : false }),
                e => e.Prop);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enum_ToString_is_client_eval(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<Gear>().OrderBy(g => g.SquadId)
                        .ThenBy(g => g.Nickname)
                        .Select(g => g.Rank.ToString()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_naked_navigation_with_ToList(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select g.Weapons.ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => (from g in ss.Set<Gear>()
                       where g.Nickname != "Marcus"
                       orderby g.Nickname
                       select g.Weapons.ToList()).Select(e => e.Count),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_naked_navigation_with_ToArray(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select g.Weapons.ToArray(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_explicit_to_list(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_explicit_to_array(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w).ToArray(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_ordered(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              orderby w.Name descending
                              select w).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projection_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                    AssertCollection(
                        e.Collection,
                        a.Collection,
                        elementSorter: ee => ee.FullName,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            Assert.Equal(ee.Nickname, aa.Nickname);
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projecting_single_property(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select w.Name).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projecting_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select "BFG").ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_basic_projecting_constant_bool(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Marcus"
                      orderby g.Nickname
                      select (from w in g.Weapons
                              where w.IsAutomatic || w.Name != "foo"
                              select true).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_projection_of_collection_thru_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      orderby g.FullName
                      where g.Nickname != "Marcus"
                      select g.Squad.Missions.Where(m => m.MissionId != 17).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_project_anonymous_collection_result(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from s in ss.Set<Squad>()
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
                    AssertCollection(e.Collection, a.Collection, elementSorter: ee => ee.FullName);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from s in ss.Set<Squad>()
                      select (from m in s.Missions
                              where m.MissionId < 42
                              select (from ps in m.Mission.ParticipatingSquads
                                      where ps.SquadId < 7
                                      select ps).ToList()).ToList(),
                elementSorter: e => e.Count(),
                elementAsserter: (e, a) => AssertCollection(
                    e,
                    a,
                    elementSorter: ee => ee.Count(),
                    elementAsserter: (ee, aa) => AssertCollection(ee, aa)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from s in ss.Set<Squad>()
                      select (from m in s.Missions
                              where m.MissionId < 3
                              select (from ps in m.Mission.ParticipatingSquads
                                      where ps.SquadId < 2
                                      select ps).ToList()),
                elementSorter: e => e.Count(),
                elementAsserter: (e, a) => AssertCollection(
                    e,
                    a,
                    elementSorter: ee => ee.Count(),
                    elementAsserter: (ee, aa) => AssertCollection(ee, aa)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from s in ss.Set<Squad>()
                      select (from m in s.Missions
                              where m.MissionId < 42
                              select (from ps in m.Mission.ParticipatingSquads
                                      where ps.SquadId < 7
                                      select ps)).ToList(),
                elementSorter: e => e.Count(),
                elementAsserter: (e, a) => AssertCollection(
                    e,
                    a,
                    elementSorter: ee => ee.Count(),
                    elementAsserter: (ee, aa) => AssertCollection(ee, aa)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_with_custom_ordering(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
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
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        elementSorter: ee => ee.FullName,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            AssertCollection(ee.InnerCollection, aa.InnerCollection);
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_same_collection_projected_multiple_times(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
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
                    AssertCollection(e.First, a.First);
                    AssertCollection(e.Second, a.Second);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_similar_collection_projected_multiple_times(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
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
                    AssertCollection(e.First, a.First);
                    AssertCollection(e.Second, a.Second);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_different_collections_projected(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
                    orderby o.FullName
                    select new
                    {
                        o.Nickname,
                        First = o.Weapons.Where(w => w.IsAutomatic).Select(
                            w => new { w.Name, w.IsAutomatic }).ToArray(),
                        Second = o.Reports.OrderBy(r => r.FullName).Select(
                            r => new { r.Nickname, r.Rank }).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    AssertCollection(e.First, a.First, elementSorter: ee => ee.Name);
                    AssertCollection(e.Second, a.Second, ordered: true);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
                    orderby o.HasSoulPatch descending, o.Tag.Note
                    where o.Reports.Any()
                    select o.FullName,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                    AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(
            bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                    AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(
            bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                    AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_multiple_nested_complex_collections(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                                                                          ww => new { ww.Name, ww.IsAutomatic }).ToList(),
                                                                      InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                                          mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
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
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        elementSorter: ee => ee.FullName,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            AssertCollection(
                                ee.InnerCollection,
                                aa.InnerCollection,
                                elementSorter: eee => eee.Id,
                                elementAsserter: (eee, aaa) =>
                                {
                                    Assert.Equal(eee.Id, aaa.Id);
                                    AssertCollection(
                                        eee.InnerFirst,
                                        aaa.InnerFirst,
                                        elementSorter: eeee => eeee.Name);
                                    AssertCollection(
                                        eee.InnerSecond,
                                        aaa.InnerSecond,
                                        elementSorter: eeee => eeee.Nickname);
                                });
                        });
                    AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                    AssertCollection(e.Collection, a.Collection, elementSorter: ee => (ee.ReportName, ee.OfficerName));
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                    AssertCollection(e.Collection, a.Collection, elementSorter: ee => ee.ReportName);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                                                                  select new { w.Name, r.Nickname }).ToList()
                                           }).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        elementSorter: ee => ee.FullName,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => (eee.Name, eee.Nickname));
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                                                                select new { w.Name, o.Nickname }
                                          }
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        elementSorter: ee => ee.FullName,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => (eee.Name, eee.Nickname));
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_on_select_many(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    from s in ss.Set<Squad>()
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

                    AssertCollection(e.Collection1, a.Collection1);
                    AssertCollection(e.Collection2, a.Collection2);
                });
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_Skip(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Skip(1)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory(Skip = "Issue#16313")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_Take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Take(2)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_Distinct(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Distinct()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_FirstOrDefault(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().OrderBy(s => s.Name)
                    .Select(s => s.Members.OrderBy(m => m.Nickname).Select(m => m.FullName).FirstOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_on_left_join_with_predicate(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    where !g.HasSoulPatch
                    select new { g.Nickname, WeaponNames = g.Weapons.Select(w => w.Name).ToList() },
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    where !MaybeScalar<bool>(g, () => g.HasSoulPatch) == true
                    select new
                    {
                        Nickname = Maybe(g, () => g.Nickname),
                        WeaponNames = g == null ? new List<string>() : g.Weapons.Select(w => w.Name).ToList()
                    },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    AssertCollection(e.WeaponNames, a.WeaponNames);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_on_left_join_with_null_value(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note
                    select g.Weapons.Select(w => w.Name).ToList(),
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note
                    select g != null ? g.Weapons.Select(w => w.Name).ToList() : new List<string>(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_left_join_with_self_reference(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from t in ss.Set<CogTag>()
                    join o in ss.Set<Gear>().OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                    from o in grouping.DefaultIfEmpty()
                    select new { t.Note, ReportNames = o.Reports.Select(r => r.FullName).ToList() },
                ss =>
                    from t in ss.Set<CogTag>()
                    join o in ss.Set<Gear>().OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                    from o in grouping.DefaultIfEmpty()
                    select new { t.Note, ReportNames = o != null ? o.Reports.Select(r => r.FullName).ToList() : new List<string>() },
                elementSorter: e => e.Note,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Note, a.Note);
                    AssertCollection(e.ReportNames, a.ReportNames);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_deeply_nested_left_join(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note, g.Nickname descending
                    select g.Squad.Members.Where(m => m.HasSoulPatch).Select(
                        m => new { m.Nickname, AutomaticWeapons = m.Weapons.Where(w => w.IsAutomatic).ToList() }).ToList(),
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                    from g in grouping.DefaultIfEmpty()
                    orderby t.Note, Maybe(g, () => g.Nickname) descending
                    select
                        g != null
                            ? g.Squad.Members.Where(m => m.HasSoulPatch).OrderBy(m => m.Nickname)
                                .Select(m => new { m.Nickname, AutomaticWeapons = m.Weapons.Where(w => w.IsAutomatic).ToList() }).ToList()
                            : Enumerable.Empty<int>().Select(x => new { Nickname = (string)null, AutomaticWeapons = new List<Weapon>() })
                                .ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(
                    e,
                    a,
                    elementSorter: e => e.Nickname,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Nickname, aa.Nickname);
                        AssertCollection(ee.AutomaticWeapons, aa.AutomaticWeapons);
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().OrderBy(w => w.Name).Select(
                    w => w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(
                        m => new { Weapons = m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id).ToList(), m.Rank }).ToList()),
                ss => ss.Set<Weapon>().OrderBy(w => w.Name).Select(
                    w => w.Owner != null
                        ? w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(
                            m => new { Weapons = m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id).ToList(), m.Rank }).ToList()
                        : Enumerable.Empty<int>().Select(x => new { Weapons = new List<Weapon>(), Rank = default(MilitaryRank) })),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(
                    e, a, ordered: true, elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Rank, aa.Rank);
                        AssertCollection(ee.Weapons, aa.Weapons, ordered: true);
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_complex_scenario1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from r in ss.Set<Gear>()
                    select new
                    {
                        r.FullName,
                        OuterCollection = (from w in r.Weapons
                                           select new
                                           {
                                               w.Id,
                                               InnerCollection = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                   mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                           }).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        elementSorter: ee => ee.Id,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.Id, aa.Id);
                            AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => eee.Nickname);
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_complex_scenario2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                                                                          mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                                                  }).ToList()
                                           }).ToList()
                    },
                elementSorter: e => e.FullName,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        elementSorter: ee => ee.FullName,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            AssertCollection(
                                ee.InnerCollection,
                                aa.InnerCollection,
                                elementSorter: eee => eee.Id,
                                elementAsserter: (eee, aaa) =>
                                {
                                    Assert.Equal(eee.Id, aaa.Id);
                                    AssertCollection(eee.InnerSecond, aaa.InnerSecond, elementSorter: eeee => eeee.Nickname);
                                });
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_funky_orderby_complex_scenario1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from r in ss.Set<Gear>()
                    orderby r.FullName, r.Nickname descending, r.FullName
                    select new
                    {
                        r.FullName,
                        OuterCollection = (from w in r.Weapons
                                           select new
                                           {
                                               w.Id,
                                               InnerCollection = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                   mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                           }).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        elementSorter: ee => ee.Id,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.Id, aa.Id);
                            AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => eee.Nickname);
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collections_with_funky_orderby_complex_scenario2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
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
                                                                          mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                                                  }).ToList()
                                           }).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.FullName, a.FullName);
                    AssertCollection(
                        e.OuterCollection,
                        a.OuterCollection,
                        ordered: true,
                        elementAsserter: (ee, aa) =>
                        {
                            Assert.Equal(ee.FullName, aa.FullName);
                            AssertCollection(
                                ee.InnerCollection,
                                aa.InnerCollection,
                                ordered: true,
                                elementAsserter: (eee, aaa) =>
                                {
                                    Assert.Equal(eee.Id, aaa.Id);
                                    AssertCollection(eee.InnerSecond, aaa.InnerSecond, ordered: true);
                                });
                        });
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_top_level_FirstOrDefault(bool isAsync)
        {
            return AssertFirstOrDefault(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(g => g.Weapons),
                asserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_top_level_Count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.Weapons));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_top_level_Last_with_orderby_on_outer(bool isAsync)
        {
            return AssertLast(
                isAsync,
                ss => ss.Set<Gear>().OrderByDescending(g => g.FullName).Select(g => g.Weapons),
                asserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_top_level_Last_with_order_by_on_inner(bool isAsync)
        {
            return AssertLast(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.FullName).Select(g => g.Weapons.OrderBy(w => w.Name).ToList()),
                asserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_with_group_by_and_last()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.OrderByDescending(g => g.HasSoulPatch).Include(g => g.Weapons).Select(
                    g => new { g.Rank, g }).GroupBy(g => g.Rank).ToList().OrderBy(g => g.Key).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderByDescending(g => g.HasSoulPatch).Include(g => g.Weapons)
                    .Select(
                        g => new { g.Rank, g }).GroupBy(g => g.Rank).ToList().OrderBy(g => g.Key).ToList();

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
                    g => new { g.Rank, g.HasSoulPatch }).ToList().OrderBy(g => g.Key.Rank).ThenBy(g => g.Key.HasSoulPatch).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.Nickname).Include(g => g.Weapons).GroupBy(
                    g => new { g.Rank, g.HasSoulPatch }).ToList().OrderBy(g => g.Key.Rank).ThenBy(g => g.Key.HasSoulPatch).ToList();

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
            return AssertQuery(
                isAsync,
                ss =>
                    from ll in ss.Set<LocustLeader>()
                    join h in ss.Set<Faction>().OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName
                    where h.Eradicated != true
                    select h);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from ll in ss.Set<LocustLeader>()
                    join h in ss.Set<Faction>().OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName into
                        grouping
                    from h in grouping.DefaultIfEmpty()
                    where h.Eradicated != true
                    select h,
                ss =>
                    from ll in ss.Set<LocustLeader>()
                    join h in ss.Set<Faction>().OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName into
                        grouping
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Include(ll => ((LocustCommander)ll).DefeatedBy).ThenInclude(g => g.Weapons)
                    .OrderBy(ll => ((LocustCommander)ll).DefeatedBy.Tag.Note).Take(10),
                ss => ss.Set<LocustLeader>().Take(10),
                expectedIncludes,
                elementSorter: e => e.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_required_navigation_on_derived_type(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Select(ll => ((LocustCommander)ll).HighCommand.Name),
                ss => ss.Set<LocustLeader>().Select(ll => ll is LocustCommander ? ((LocustCommander)ll).HighCommand.Name : null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_required_navigation_on_the_same_type_with_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.CityOfBirth.Name),
                ss => ss.Set<Gear>().Select(g => g.CityOfBirth.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_required_navigation_on_derived_type(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Where(ll => ((LocustCommander)ll).HighCommand.IsOperational),
                ss => ss.Set<LocustLeader>().Where(ll => ll is LocustCommander ? ((LocustCommander)ll).HighCommand.IsOperational : false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_join_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ss.Set<CogTag>()
                                      join g in ss.Set<Gear>() on o.FullName equals g.FullName
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.Collection, a.Collection));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_join_key_inner_and_outer(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ss.Set<CogTag>()
                                      join g in ss.Set<Gear>() on o.FullName equals o.Nickname
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.Collection, a.Collection));
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_group_join_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ss.Set<CogTag>()
                                      join g in ss.Set<Gear>() on o.FullName equals g.FullName into grouping
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.Collection, a.Collection));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from o in ss.Set<Gear>().OfType<Officer>()
                    orderby o.Nickname
                    select new
                    {
                        Collection = (from t in ss.Set<CogTag>()
                                      join g in ss.Set<Gear>() on o.FullName equals g.FullName into grouping
                                      from g in grouping.DefaultIfEmpty()
                                      select t.Note).ToList()
                    },
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.Collection, a.Collection));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_with_concat(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Gear>(g => g.Squad, "Squad"), new ExpectedInclude<Officer>(o => o.Squad, "Squad")
            };

            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertIncludeQuery(
                    isAsync,
                    ss => ss.Set<Gear>().Include(g => g.Squad).Concat(ss.Set<Gear>()),
                    expectedIncludes))).Message;

            Assert.Equal(
                "When performing a set operation, both operands must have the same Include operations.",
                message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Select(
                    t => new { c = !(t.Gear.HasSoulPatch ? true : ((bool?)t.Gear.HasSoulPatch ?? true)) }),
                ss => ss.Set<CogTag>().Select(
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.AssignedCity).ThenByDescending(g => g.Nickname).Select(f => f.FullName),
                ss => ss.Set<Gear>().OrderBy(g => Maybe(g.AssignedCity, () => g.AssignedCity.Name)).ThenByDescending(g => g.Nickname)
                    .Select(f => f.FullName),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_entity_qsre_with_inheritance(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().OfType<LocustCommander>().OrderBy(lc => lc.HighCommand).ThenBy(lc => lc.Name)
                    .Select(lc => lc.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_entity_qsre_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().OrderBy(w => w.Owner).ThenBy(w => w.Id).Select(w => w.Name),
                ss => ss.Set<Weapon>().OrderBy(w => Maybe(w.Owner, () => w.Owner.Nickname))
                    .ThenBy(w => MaybeScalar<int>(w.Owner, () => w.Owner.SquadId))
                    .ThenBy(w => w.Id).Select(w => w.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_entity_qsre_with_other_orderbys(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().OrderBy(w => w.IsAutomatic).ThenByDescending(w => w.Owner).ThenBy(w => w.SynergyWith)
                    .ThenBy(w => w.Name),
                ss => ss.Set<Weapon>()
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
            return AssertQuery(
                isAsync,
                ss => from w1 in ss.Set<Weapon>()
                      join w2 in ss.Set<Weapon>() on w1 equals w2
                      select new { Name1 = w1.Name, Name2 = w2.Name },
                elementSorter: e => e.Name1 + " " + e.Name2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>()
                      join g2 in ss.Set<Gear>() on g1 equals g2
                      select new { GearName1 = g1.FullName, GearName2 = g2.FullName },
                elementSorter: e => (e.GearName1, e.GearName2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inheritance(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      join o in ss.Set<Gear>().OfType<Officer>() on g equals o
                      select new { GearName = g.FullName, OfficerName = o.FullName },
                elementSorter: e => (e.GearName, e.OfficerName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from w1 in ss.Set<Weapon>()
                      join w2 in ss.Set<Weapon>() on w1.SynergyWith equals w2
                      select new { Name1 = w1.Name, Name2 = w2.Name },
                elementSorter: e => (e.Name1, e.Name2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<City>()
                    join g in ss.Set<Gear>() on c equals g.AssignedCity
                    select new { CityName = c.Name, GearNickname = g.Nickname },
                elementSorter: e => (e.CityName, e.GearNickname));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from g in ss.Set<Gear>()
                    join t in ss.Set<CogTag>().Where(tt => tt.Note == "Cole's Tag" || tt.Note == "Dom's Tag") on g equals t.Gear
                    select new { g.Nickname, t.Note },
                elementSorter: e => (e.Nickname, e.Note));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from s in ss.Set<Squad>()
                    join w in ss.Set<Weapon>().Where(ww => ww.IsAutomatic) on s equals w.Owner.Squad
                    select new { SquadName = s.Name, WeaponName = w.Name },
                elementSorter: e => (e.SquadName, e.WeaponName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from s in ss.Set<Squad>()
                    join w in ss.Set<Weapon>() on s equals w.Owner.Squad into grouping
                    from w in grouping.DefaultIfEmpty()
                    select new { SquadName = s.Name, WeaponName = w.Name },
                ss =>
                    from s in ss.Set<Squad>()
                    join w in ss.Set<Weapon>() on s equals Maybe(w.Owner, () => w.Owner.Squad) into grouping
                    from w in grouping.DefaultIfEmpty()
                    select new { SquadName = s.Name, WeaponName = Maybe(w, () => w.Name) },
                elementSorter: e => (e.SquadName, e.WeaponName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_with_complex_key_selector(bool isAsync)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Squad>()
                        .Join(
                            ss.Set<CogTag>().Where(t => t.Note == "Marcus' Tag"), o => true, i => true, (o, i) => new { o, i })
                        .GroupJoin(
                            ss.Set<Gear>(),
                            oo => oo.o.Members.FirstOrDefault(v => v.Tag == oo.i),
                            ii => ii,
                            (k, g) => new
                            {
                                k.o,
                                k.i,
                                value = g.OrderBy(gg => gg.FullName).FirstOrDefault()
                            })
                        .Select(
                            r => new { r.o.Id, TagId = r.i.Id }),
                    elementSorter: e => (e.Id, e.TagId)))).Message;

            Assert.Equal(
                "This query would cause multiple evaluation of a subquery because entity 'Gear' has a composite key. Rewrite your query avoiding the subquery.",
                message);
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

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Streaming_correlated_collection_issue_11403(bool isAsync)
        {
            return AssertFirstOrDefault(
                isAsync,
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.Where(w => !w.IsAutomatic).OrderBy(w => w.Id)),
                asserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_one_value_type_from_empty_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Where(s => s.Name == "Kilo").Select(
                    s => new { s.Name, SquadId = s.Members.Where(m => m.HasSoulPatch).Select(m => m.SquadId).FirstOrDefault() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_one_value_type_converted_to_nullable_from_empty_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Where(s => s.Name == "Kilo").Select(
                    s => new { s.Name, SquadId = s.Members.Where(m => m.HasSoulPatch).Select(m => (int?)m.SquadId).FirstOrDefault() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_one_value_type_with_client_projection_from_empty_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Where(s => s.Name == "Kilo").Select(
                    s => new
                    {
                        s.Name,
                        SquadId = s.Members.Where(m => m.HasSoulPatch).Select(m => ClientFunction(m.SquadId, m.LeaderSquadId)).FirstOrDefault()
                    }),
                elementSorter: s => s.Name);
        }

        private static int ClientFunction(int a, int b) => a + b + 1;

        [ConditionalTheory(Skip = "issue #15864")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Where(s => s.Name == "Kilo")
                    .Where(s => s.Members.Where(m => m.HasSoulPatch).Select(m => m.SquadId).FirstOrDefault() != 0).Select(s => s.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_int(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Select(
                    s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => 42).FirstOrDefault() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_string(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Select(
                    s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => "Foo").FirstOrDefault() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_bool(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Select(
                    s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => true).FirstOrDefault() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_inside_anonymous(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => new { One = 1 }).FirstOrDefault()
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Select(
                    s => new
                    {
                        s.Name,
                        Gear = s.Members.Where(g => g.HasSoulPatch).Select(
                            g => new { True = true, False = false }).FirstOrDefault()
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_order_by_constant(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Squad>().Include(s => s.Members).OrderBy(s => 42).Select(s => s),
                expectedQuery: ss => ss.Set<Squad>(),
                new List<IExpectedInclude> { new ExpectedInclude<Squad>(s => s.Members, "Members") });
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void Include_groupby_constant()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads.Include(s => s.Members).GroupBy(s => 1);
                var result = query.ToList();

                Assert.Single(result);
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderByDescending(s => 1).Select(
                    g => new { g.Nickname, Weapons = g.Weapons.Select(w => w.Name).ToList() }),
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    AssertCollection(e.Weapons, a.Weapons);
                });
        }

        public class MyDTO
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Select(
                    s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => (MyDTO)null).FirstOrDefault() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Squad>().Select(
                    s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => new MyDTO()).FirstOrDefault() }),
                elementSorter: e => e.Name,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Name, a.Name);
                    if (e.Gear == null)
                    {
                        Assert.Null(a.Gear);
                    }
                });
        }

        [ConditionalTheory(Skip = "issue #11567")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_order_by_constant_null_of_non_mapped_type(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Squad>().Include(s => s.Members).OrderBy(s => (MyDTO)null),
                expectedQuery: ss => ss.Set<Squad>(),
                new List<IExpectedInclude> { new ExpectedInclude<Squad>(s => s.Members, "Members") });
        }

        [ConditionalFact(Skip = "issue #11567")]
        public virtual void Include_groupby_constant_null_of_non_mapped_type()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads.Include(s => s.Members).GroupBy(s => (MyDTO)null);
                var result = query.ToList();

                Assert.Single(result);
                var bucket = result[0].ToList();
                Assert.Equal(2, bucket.Count);
                Assert.NotNull(bucket[0].Members);
                Assert.NotNull(bucket[1].Members);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Gear>().OrderByDescending(s => (MyDTO)null).Select(
                        g => new { g.Nickname, Weapons = g.Weapons.Select(w => w.Name).ToList() }),
                    elementSorter: e => e.Nickname,
                    elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.Nickname, a.Nickname);
                        AssertCollection(e.Weapons, a.Weapons);
                    }));
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
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>()
                    .Include(o => o.Reports)
                    .OrderBy(o => o.Weapons.Count).ThenBy(o => o.Nickname),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_complex_OrderBy2(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>()
                    .Include(o => o.Reports)
                    .OrderBy(o => o.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic).ThenBy(o => o.Nickname),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_complex_OrderBy3(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>()
                    .Include(o => o.Reports)
                    .OrderBy(o => o.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()).ThenBy(o => o.Nickname),
                new List<IExpectedInclude> { new ExpectedInclude<Officer>(o => o.Reports, "Reports") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_complex_OrderBy(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>()
                    .OrderBy(o => o.Weapons.Count).ThenBy(g => g.Nickname)
                    .Select(o => o.Reports.Where(g => !g.HasSoulPatch).ToList()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_very_complex_order_by(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>()
                    .OrderBy(
                        o => o.Weapons.Where(
                                w => w.IsAutomatic
                                    == ss.Set<Gear>().Where(g => g.Nickname == "Marcus").Select(g => g.HasSoulPatch)
                                        .FirstOrDefault())
                            .Count()).ThenBy(g => g.Nickname)
                    .Select(o => o.Reports.Where(g => !g.HasSoulPatch).ToList()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_to_derived_type_causes_client_eval(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidCastException>(() => AssertQuery(isAsync, ss => ss.Set<Gear>().Cast<Officer>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_to_derived_type_after_OfType_works(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>().Cast<Officer>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_inside_cast_and_coalesce(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.Weapons.OrderBy(w => w.Id).Select(w => (int?)w.Id).FirstOrDefault() ?? 42));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_outside_cast_and_coalesce(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => (int?)g.Weapons.OrderBy(w => w.Id).Select(w => w.Id).FirstOrDefault() ?? 42));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_pushdown_and_coalesce(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => (int?)g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id ?? 42));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_int_with_pushdown_and_coalesce2(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(
                    g => (int?)g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id ?? g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(
                    g => g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(
                    g => (bool?)g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).FirstOrDefault().IsAutomatic),
                ss => ss.Set<Gear>().Select(g => (bool?)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty_with_pushdown_without_convert_to_nullable1(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).FirstOrDefault().IsAutomatic),
                ss => ss.Set<Gear>().Select(g => false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_boolean_empty_with_pushdown_without_convert_to_nullable2(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).FirstOrDefault().Id),
                ss => ss.Set<Gear>().Select(g => 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean1(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                    .Select(g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().Select(w => w.IsAutomatic).SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean2(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                    .Select(g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Select(w => w.IsAutomatic).Distinct().SingleOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                    .Select(g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_empty1(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                    .Select(g => g.Weapons.Where(w => w.Name == "BFG").Distinct().Select(w => w.IsAutomatic).SingleOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_empty2(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                    .Select(g => g.Weapons.Where(w => w.Name == "BFG").Select(w => w.IsAutomatic).Distinct().SingleOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                    .Select(g => (bool?)g.Weapons.Where(w => w.Name == "BFG").Distinct().SingleOrDefault().IsAutomatic),
                ss => ss.Set<Gear>().Where(g => g.HasSoulPatch).Select(g => (bool?)null),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_subquery_to_base_type_using_typed_ToList(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<City>().Where(c => c.Name == "Ephyra").Select(
                    c => c.StationedGears.Select(
                        g => new Officer
                        {
                            CityOfBirthName = g.CityOfBirthName,
                            FullName = g.FullName,
                            HasSoulPatch = g.HasSoulPatch,
                            LeaderNickname = g.LeaderNickname,
                            LeaderSquadId = g.LeaderSquadId,
                            Nickname = g.Nickname,
                            Rank = g.Rank,
                            SquadId = g.SquadId
                        }).ToList<Gear>()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<City>().Where(c => c.Name == "Ephyra").Select(
                    c => c.StationedGears.OrderByDescending(g => g.Nickname).Select(
                        g => new Officer
                        {
                            CityOfBirthName = g.CityOfBirthName,
                            FullName = g.FullName,
                            HasSoulPatch = g.HasSoulPatch,
                            LeaderNickname = g.LeaderNickname,
                            LeaderSquadId = g.LeaderSquadId,
                            Nickname = g.Nickname,
                            Rank = g.Rank,
                            SquadId = g.SquadId
                        }).ToArray<Gear>()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalTheory(Skip = "Issue#15713")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool isAsync)
        {
            var nicknames = new List<string>();
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      orderby nicknames.Contains(g.Nickname) descending
                      select new { g.Nickname, Weapons = g.Weapons.Select(w => w.Name).ToList() },
                elementSorter: e => e.Nickname,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Nickname, a.Nickname);
                    AssertCollection(e.Weapons, a.Weapons);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.IsAutomatic).OrderBy(w => w.IsAutomatic).ThenBy(w => w.Id),
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.IsAutomatic : false)
                    .ThenBy(w => w != null ? (int?)w.Id : null),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_Like(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => EF.Functions.Like(w.Name, "%Lancer"))
                    .OrderBy(w => EF.Functions.Like(w.Name, "%Lancer")).Select(w => w),
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name.EndsWith("Lancer") : false)
                    .Select(w => w));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_is_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.Name == null).OrderBy(w => w.Name == null).Select(w => w),
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name == null : false).Select(w => w));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_on_string_compare(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0)
                    .OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0).ThenBy(w => w.Id),
                ss => ss.Set<Weapon>().OrderBy(w => w != null ? w.Name.CompareTo("Marcus' Lancer") == 0 : false).ThenBy(w => w.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Double_order_by_binary_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().OrderBy(w => w.Id + 2).OrderBy(w => w.Id + 2).Select(w => new { Binary = w.Id + 2 }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_compare_with_null_conditional_argument(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0).Select(c => c),
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name.CompareTo("Marcus' Lancer") == 0 : false)
                    .Select(c => c));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_compare_with_null_conditional_argument2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => "Marcus' Lancer".CompareTo(w.Name) == 0).Select(w => w),
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? "Marcus' Lancer".CompareTo(w.Name) == 0 : false)
                    .Select(w => w));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_with_null_conditional_argument(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.Name + 5),
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name + 5 : null),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_with_null_conditional_argument2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => string.Concat(w.Name, "Marcus' Lancer")),
                ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? string.Concat(w.Name, "Marcus' Lancer") : null),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "issue #14205")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_on_various_types(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      from m in ss.Set<Mission>()
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
            return AssertQueryScalar(
                isAsync,
                ss => from m in ss.Set<Mission>()
                      select m.Timeline.TimeOfDay);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Average(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Average(gg => gg.SquadId)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Sum(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Sum(gg => gg.SquadId)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Count(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_LongCount(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.LongCount()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Max(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Max(gg => gg.SquadId)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Select_Min(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Min(gg => gg.SquadId)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Nickname).OrderBy(g => g.Key)
                        .Select(
                            g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "issue #16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector(bool isAsync)
        {
            // TODO: convert to AssertIncludeQuery
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
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

        [ConditionalTheory(Skip = "issue #16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_entity_key_with_include_on_that_entity_with_key_in_result_selector_using_EF_Property(bool isAsync)
        {
            // TODO: convert to AssertIncludeQuery
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
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
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).OrderBy(g => g.Key)
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

            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.FirstOrDefault(gg => gg.HasSoulPatch)),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_Cast_to_base(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>().OfType<Officer>().Include(o => o.Weapons).Cast<Gear>(),
                new List<IExpectedInclude> { new ExpectedInclude<Gear>(e => e.Weapons, "Weapons") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_client_method_and_member_access_still_applies_includes(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Include(g => g.Tag)
                    .Select(g => new { g.Nickname, Client(g).FullName }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_projection_of_unmapped_property_still_gets_applied(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Include(g => g.Weapons).Select(g => g.IsMarcus));
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

                Assert.Single(result);

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
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.LeaderNickname != null ? (bool?)(g.Nickname.Length == 5) : (bool?)null)
                    .OrderBy(e => e.HasValue)
                    .ThenBy(e => e.HasValue).Select(e => e));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_projection(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Weapon>().Select(w => w.SynergyWithId.GetValueOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_filter(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.SynergyWithId.GetValueOrDefault() == 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_filter_non_nullable_column(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => ((int?)w.Id).GetValueOrDefault() == 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_on_DateTimeOffset(bool isAsync)
        {
            var defaultValue = default(DateTimeOffset);

            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Mission>().Where(m => ((DateTimeOffset?)m.Timeline).GetValueOrDefault() == defaultValue)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_in_order_by(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().OrderBy(w => w.SynergyWithId.GetValueOrDefault()).ThenBy(w => w.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_with_argument(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.SynergyWithId.GetValueOrDefault(w.Id) == 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetValueOrDefault_with_argument_complex(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Where(w => w.SynergyWithId.GetValueOrDefault(w.Name.Length + 42) > 10),
                ss => ss.Set<Weapon>().Where(w => (w.SynergyWithId == null ? w.Name.Length + 42 : w.SynergyWithId) > 10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_with_complex_predicate_containing_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.FullName != "Dom" && g.Weapons.OrderBy(w => w.Id).FirstOrDefault(w => w.IsAutomatic) != null
                      select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      where g.Nickname != "Dom"
                      let automaticWeapons
                          = g.Weapons
                              .OrderByDescending(w => w.AmmunitionType)
                              .Where(w => w.IsAutomatic)
                      select new { g.Nickname, WeaponName = automaticWeapons.FirstOrDefault().Name },
                ss => from g in ss.Set<Gear>()
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
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note.Substring(0, t.Gear.SquadId) == t.GearNickName),
                ss => ss.Set<CogTag>().Where(t => Maybe(t.Gear, () => t.Note.Substring(0, t.Gear.SquadId)) == t.GearNickName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().Where(t => t.Note.Substring(0, t.Gear.Squad.Name.Length) == t.GearNickName),
                ss => ss.Set<CogTag>().Where(t => Maybe(t.Gear, () => t.Note.Substring(0, t.Gear.Squad.Name.Length)) == t.GearNickName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_with_new_Guid(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where t.Id == new Guid("DF36F493-463F-4123-83F9-6B135DEEB7BA")
                      select t);
        }

        public virtual async Task Filter_with_new_Guid_closure(bool isAsync)
        {
            var guid = "DF36F493-463F-4123-83F9-6B135DEEB7BD";

            await AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where t.Id == new Guid(guid)
                      select t);

            guid = "B39A6FBA-9026-4D69-828E-FD7068673E57";

            await AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where t.Id == new Guid(guid)
                      select t);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OfTypeNav1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Tag.Note != "Foo").OfType<Officer>().Where(o => o.Tag.Note != "Bar"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OfTypeNav2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => g.Tag.Note != "Foo").OfType<Officer>().Where(o => o.AssignedCity.Location != "Bar"),
                ss => ss.Set<Gear>()
                    .Where(g => Maybe(g.Tag, () => g.Tag.Note) != "Foo")
                    .OfType<Officer>()
                    .Where(o => Maybe(o.AssignedCity, () => o.AssignedCity.Location) != "Bar"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OfTypeNav3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Where(g => g.Tag.Note != "Foo")
                    .Join(
                        ss.Set<Weapon>(),
                        g => g.FullName,
                        w => w.OwnerFullName,
                        (o, i) => o)
                    .OfType<Officer>()
                    .Where(o => o.Tag.Note != "Bar"));
        }

        [ConditionalFact(Skip = "Issue #17328")]
        public virtual void Nav_rewrite_Distinct_with_convert()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.Factions.Include(f => ((LocustHorde)f).Commander)
                    .Where(f => f.Capital.Name != "Foo").Select(f => (LocustHorde)f)
                    .Distinct().Where(lh => lh.Commander.Name != "Bar").ToList();
            }
        }

        [ConditionalFact(Skip = "Issue #17328")]
        public virtual void Nav_rewrite_Distinct_with_convert_anonymous()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.Factions.Include(f => ((LocustHorde)f).Commander)
                    .Where(f => f.Capital.Name != "Foo").Select(f => new { horde = (LocustHorde)f })
                    .Distinct().Where(lh => lh.horde.Commander.Name != "Bar").ToList();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nav_rewrite_with_convert1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().Where(f => f.Capital.Name != "Foo").Select(f => ((LocustHorde)f).Commander),
                ss => ss.Set<Faction>().Where(f => Maybe(f.Capital, () => f.Capital.Name) != "Foo")
                    .Select(f => ((LocustHorde)f).Commander));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nav_rewrite_with_convert2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().Where(f => f.Capital.Name != "Foo").Select(f => (LocustHorde)f)
                    .Where(lh => lh.Commander.Name != "Bar"),
                ss => ss.Set<Faction>().Where(f => Maybe(f.Capital, () => f.Capital.Name) != "Foo").Select(f => (LocustHorde)f)
                    .Where(lh => lh.Commander.Name != "Bar"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nav_rewrite_with_convert3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().Where(f => f.Capital.Name != "Foo").Select(f => new { horde = (LocustHorde)f })
                    .Where(x => x.horde.Commander.Name != "Bar"),
                ss => ss.Set<Faction>().Where(f => Maybe(f.Capital, () => f.Capital.Name) != "Foo")
                    .Select(f => new { horde = (LocustHorde)f }).Where(x => x.horde.Commander.Name != "Bar"),
                elementSorter: e => e.horde.Id,
                elementAsserter: (e, a) => AssertEqual(e.horde, a.horde));
        }

        [ConditionalTheory(Skip = "Issue#15260")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_contains_on_navigation_with_composite_keys(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => ss.Set<City>().Any(c => c.BornGears.Contains(g))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_derivied_entity_with_convert_to_parent(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().OfType<LocustHorde>().Select(f => (Faction)f));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_complex_order_by(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Include(g => g.Weapons)
                    .OrderBy(g => g.Weapons.FirstOrDefault(w => w.Name.Contains("Gnasher")).Name)
                    .ThenBy(g => g.Nickname),
                ss => ss.Set<Gear>()
                    .Include(g => g.Weapons)
                    .OrderBy(
                        g => Maybe(
                            g.Weapons.FirstOrDefault(w => w.Name.Contains("Gnasher")),
                            () => g.Weapons.FirstOrDefault(w => w.Name.Contains("Gnasher")).Name))
                    .ThenBy(g => g.Nickname),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Gear>(e => e.Weapons, "Weapons") },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "issue #12603")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Select(g => new { Gear = g }).Take(25)
                    .Select(e => e.Gear.Weapons.OrderBy(w => w.Id).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Bool_projection_from_subquery_treated_appropriately_in_where(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<City>().Where(
                    c => ss.Set<Gear>().OrderBy(g => g.Nickname).ThenBy(g => g.SquadId).FirstOrDefault().HasSoulPatch));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTimeOffset_Contains_Less_than_Greater_than(bool isAsync)
        {
            var dto = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0));
            var start = dto.AddDays(-1);
            var end = dto.AddDays(1);
            var dates = new[] { dto };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Mission>().Where(
                    m => start <= m.Timeline.Date && m.Timeline < end && dates.Contains(m.Timeline)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_interpolated_string_expanded(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Weapon>().Select(
                    w => w.SynergyWithId.HasValue ? $"SynergyWithOwner: {w.SynergyWith.OwnerFullName}" : string.Empty));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_projection_using_coalesce_tracking(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().AsTracking()
                      join g2 in ss.Set<Gear>()
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      select g2 ?? g1,
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_projection_using_conditional_tracking(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g1 in ss.Set<Gear>().AsTracking()
                      join g2 in ss.Set<Gear>()
                          on g1.LeaderNickname equals g2.Nickname into grouping
                      from g2 in grouping.DefaultIfEmpty()
                      select g2 == null ? g1 : g2,
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_nested_with_take_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where t.Gear is Officer
                      select ((Officer)t.Gear).Reports.Take(50),
                ss => from t in ss.Set<CogTag>()
                      where t.Gear is Officer
                      select Maybe(t.Gear, () => ((Officer)t.Gear).Reports.Take(50)),
                elementSorter: e => e?.Count() ?? 0,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_nested_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      where t.Gear is Officer
                      select ((Officer)t.Gear).Reports,
                ss => from t in ss.Set<CogTag>()
                      where t.Gear is Officer
                      select Maybe(t.Gear, () => ((Officer)t.Gear).Reports),
                elementSorter: e => e?.Count ?? 0,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_checks_in_correlated_predicate_are_correctly_translated(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from t in ss.Set<CogTag>()
                      select new
                      {
                          key = t.Id,
                          collection = (from g in ss.Set<Gear>()
                                        where t.GearNickName == g.Nickname
                                            && t.GearSquadId != null
                                            && t.GearSquadId == g.SquadId
                                            && t.GearNickName != null
                                            && t.Note != null
                                            && null != t.Note
                                        select g).ToList()
                      },
                elementSorter: e => e.key,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.key, a.key);
                    AssertCollection(e.collection, a.collection);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(bool isAsync)
        {
            var isAutomatic = true;

            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      from w in g.Weapons.Where(ww => ww.IsAutomatic == isAutomatic).DefaultIfEmpty()
                      select new
                      {
                          g.Nickname,
                          g.FullName,
                          Collection = w != null
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_inner_being_a_subquery_projecting_single_property(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      join inner in (
                          from g2 in ss.Set<Gear>()
                          select g2.Nickname
                      ) on g.Nickname equals inner
                      select g);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      join inner in (
                          from g2 in ss.Set<Gear>()
                          select new { g2.Nickname }
                      ) on g.Nickname equals inner.Nickname
                      select g);
        }

        [ConditionalTheory(Skip = "issue #17475")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_based_on_complex_expression1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().Where(f => f is LocustHorde ? (f as LocustHorde).Commander != null : false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_based_on_complex_expression2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().Where(f => f is LocustHorde).Where(f => ((LocustHorde)f).Commander != null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_based_on_complex_expression3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>().Where(f => f is LocustHorde).Select(f => ((LocustHorde)f).Commander));
        }

        [ConditionalTheory(Skip = "issue #17782")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_based_on_complex_expression4(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from lc1 in ss.Set<Faction>().Select(f => (f is LocustHorde) ? ((LocustHorde)f).Commander : null)
                      from lc2 in ss.Set<LocustLeader>().OfType<LocustCommander>()
                      select (lc1 ?? lc2).DefeatedBy);
        }

        [ConditionalTheory(Skip = "issue #17782")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_based_on_complex_expression5(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from lc1 in ss.Set<Faction>().OfType<LocustHorde>().Select(lh => lh.Commander)
                      join lc2 in ss.Set<LocustLeader>().OfType<LocustCommander>() on true equals true
                      select (lc1 ?? lc2).DefeatedBy);
        }

        [ConditionalTheory(Skip = "issue #17782")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_based_on_complex_expression6(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from lc1 in ss.Set<Faction>().OfType<LocustHorde>().Select(lh => lh.Commander)
                      join lc2 in ss.Set<LocustLeader>().OfType<LocustCommander>() on true equals true
                      select (lc1.Name == "Queen Myrrah" ? lc1 : lc2).DefeatedBy);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_as_operator(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Select(ll => ll as LocustCommander));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetimeoffset_comparison_in_projection(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Mission>().Select(m => m.Timeline > DateTimeOffset.Now));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OfType_in_subquery_works(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Officer>().SelectMany(o => o.Reports.OfType<Officer>().Select(o1 => o1.AssignedCity)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_bool_comparison_is_translated_to_server(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<LocustHorde>().Select(lh => new { IsEradicated = lh.Eradicated == true }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Acessing_reference_navigation_collection_composition_generates_single_query(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(
                    g => new
                    {
                        Weapons = g.Weapons.Select(
                            w => new
                            {
                                w.Id,
                                w.IsAutomatic,
                                w.SynergyWith.Name
                            })
                    }),
                ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(
                    g => new
                    {
                        Weapons = g.Weapons.Select(
                            w => new
                            {
                                w.Id,
                                w.IsAutomatic,
                                Name = Maybe(w.SynergyWith, () => w.SynergyWith.Name)
                            })
                    }),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.Weapons, a.Weapons, elementSorter: ee => ee.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Reference_include_chain_loads_correctly_when_middle_is_null(bool isAsync)
        {
            return AssertIncludeQuery(
                isAsync,
                ss => ss.Set<CogTag>().AsTracking().OrderBy(t => t.Note).Include(t => t.Gear).ThenInclude(g => g.Squad),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<CogTag>(t => t.Gear, "Gear"), new ExpectedInclude<Gear>(t => t.Squad, "Squad", "Gear")
                },
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Accessing_property_of_optional_navigation_in_child_projection_works(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>().OrderBy(e => e.Note).Select(
                    t => new
                    {
                        Items = t.Gear != null
                            ? t.Gear.Weapons.Select(w => new { w.Owner.Nickname }).ToList()
                            : null
                    }),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.Items, a.Items, elementSorter: ee => ee.Nickname));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_navigation_ofType_filter_works(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<City>().Where(c => c.BornGears.OfType<Officer>().Any(o => o.Nickname == "Marcus")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_after_select_with_cast_throws(bool isAsync)
        {
            Assert.Equal(
                "Include has been used on non entity queryable.",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        isAsync,
                        ss => ss.Set<Faction>().Where(f => f is LocustHorde).Select(f => (LocustHorde)f).Include(h => h.Commander))))
                .Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_after_select_with_entity_projection_throws(bool isAsync)
        {
            Assert.Equal(
                "Include has been used on non entity queryable.",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        isAsync,
                        ss => ss.Set<Faction>().Select(f => f.Capital).Include(c => c.BornGears)))).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_after_select_anonymous_projection_throws(bool isAsync)
        {
            Assert.Equal(
                "Include has been used on non entity queryable.",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        isAsync,
                        ss => ss.Set<Faction>().Select(f => new { f }).Include(x => x.f.Capital)))).Message);
        }

        [ConditionalTheory(Skip = "issue #14671")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_after_Select_throws(bool isAsync)
        {
            Assert.Equal(
                "Include has been used on non entity queryable.",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        isAsync,
                        ss => ss.Set<Faction>().Select(f => f).Include(h => h.Capital)))).Message);
        }

        [ConditionalTheory(Skip = "issue #14671")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_after_SelectMany_throws(bool isAsync)
        {
            Assert.Equal(
                "Include has been used on non entity queryable.",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        isAsync,
                        ss => ss.Set<Faction>().SelectMany(f => f.Capital.BornGears).Include(g => g.Squad),
                        ss => ss.Set<Faction>().SelectMany(f => Maybe(f.Capital, () => f.Capital.BornGears) ?? new List<Gear>()))))
                .Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_reusing_parameter_doesnt_declare_duplicate_parameter(bool isAsync)
        {
            var prm = new ComplexParameter { Inner = new ComplexParameterInner { Nickname = "Marcus" } };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Where(g => g.Nickname != prm.Inner.Nickname)
                    .Distinct()
                    .Where(g => g.Nickname != prm.Inner.Nickname)
                    .OrderBy(g => g.FullName),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(bool isAsync)
        {
            var prm = new ComplexParameter { Inner = new ComplexParameterInner { Squad = new Squad { Id = 1 } } };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Where(g => g.Squad == prm.Inner.Squad)
                    .Distinct()
                    .Where(g => g.Squad == prm.Inner.Squad)
                    .OrderBy(g => g.FullName),
                ss => ss.Set<Gear>()
                    .Where(g => g.Squad.Id == prm.Inner.Squad.Id)
                    .Distinct()
                    .Where(g => g.Squad.Id == prm.Inner.Squad.Id)
                    .OrderBy(g => g.FullName),
                assertOrder: true);
        }

        private class ComplexParameter
        {
            public ComplexParameterInner Inner { get; set; }
        }

        private class ComplexParameterInner
        {
            public string Nickname { get; set; }
            public Squad Squad { get; set; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_entity_and_collection_element(bool isAsync)
        {
            // can't use AssertIncludeQuery here, see #18191
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Include(g => g.Squad)
                    .Include(g => g.Weapons)
                    .Select(g => new { gear = g, weapon = g.Weapons.OrderBy(w => w.Id).FirstOrDefault() }),
                elementSorter: e => e.gear.Nickname,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.gear, a.gear);
                    AssertEqual(e.gear.Squad, a.gear.Squad);
                    AssertCollection(e.gear.Weapons, a.gear.Weapons);
                    AssertEqual(e.weapon, a.weapon);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_GroupBy_after_set_operator(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => (from g in ss.Set<Gear>()
                       select new { g.AssignedCity.Name, Count = g.Weapons.Count() }).Concat(
                        from g in ss.Set<Gear>()
                        select new { g.CityOfBirth.Name, Count = g.Weapons.Count() })
                    .GroupBy(x => new { x.Name, x.Count })
                    .Select(
                        g => new
                        {
                            g.Key.Name,
                            g.Key.Count,
                            Sum = g.Sum(xx => xx.Count)
                        }),
                ss => (from g in ss.Set<Gear>()
                       select new { Name = Maybe(g.AssignedCity, () => g.AssignedCity.Name), Count = g.Weapons.Count() }).Concat(
                        from g in ss.Set<Gear>()
                        select new { g.CityOfBirth.Name, Count = g.Weapons.Count() })
                    .GroupBy(x => new { x.Name, x.Count })
                    .Select(
                        g => new
                        {
                            g.Key.Name,
                            g.Key.Count,
                            Sum = g.Sum(xx => xx.Count)
                        }),
                elementSorter: e => (e.Name, e.Count, e.Sum));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_GroupBy_after_set_operator_using_result_selector(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => (from g in ss.Set<Gear>()
                       select new { g.AssignedCity.Name, Count = g.Weapons.Count() }).Concat(
                        from g in ss.Set<Gear>()
                        select new { g.CityOfBirth.Name, Count = g.Weapons.Count() })
                    .GroupBy(
                        x => new { x.Name, x.Count },
                        (k, g) => new
                        {
                            k.Name,
                            k.Count,
                            Sum = g.Sum(xx => xx.Count)
                        }),
                ss => (from g in ss.Set<Gear>()
                       select new { Name = Maybe(g.AssignedCity, () => g.AssignedCity.Name), Count = g.Weapons.Count() }).Concat(
                        from g in ss.Set<Gear>()
                        select new { g.CityOfBirth.Name, Count = g.Weapons.Count() })
                    .GroupBy(
                        x => new { x.Name, x.Count },
                        (k, g) => new
                        {
                            k.Name,
                            k.Count,
                            Sum = g.Sum(xx => xx.Count)
                        }),
                elementSorter: e => (e.Name, e.Count, e.Sum));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_with_GroupBy_with_composite_group_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from g in ss.Set<Gear>()
                      join s in ss.Set<Squad>() on g.SquadId equals s.Id
                      join t in ss.Set<CogTag>() on g.Nickname equals t.GearNickName into grouping
                      from t in grouping.DefaultIfEmpty()
                      group g by new { g.CityOfBirthName, g.HasSoulPatch }
                      into groupby
                      select new { groupby.Key.CityOfBirthName, groupby.Key.HasSoulPatch },
                elementSorter: e => (e.CityOfBirthName, e.HasSoulPatch));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_boolean_grouping_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Select(
                        g => new
                        {
                            g.Nickname,
                            g.CityOfBirthName,
                            g.HasSoulPatch,
                            IsMarcus = g.Nickname == "Marcus"
                        })
                    .GroupBy(
                        g => new
                        {
                            g.CityOfBirthName,
                            g.HasSoulPatch,
                            g.IsMarcus
                        })
                    .Select(
                        x => new
                        {
                            x.Key.CityOfBirthName,
                            x.Key.HasSoulPatch,
                            x.Key.IsMarcus,
                            Count = x.Count()
                        }),
                elementSorter: e => (e.CityOfBirthName, e.HasSoulPatch, e.IsMarcus, e.Count));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_with_boolean_groupin_key_thru_navigation_access(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<CogTag>()
                    .GroupBy(t => new { t.Gear.HasSoulPatch, t.Gear.Squad.Name })
                    .Select(g => new { g.Key.HasSoulPatch, Name = g.Key.Name.ToLower() }),
                ss => ss.Set<CogTag>()
                    .GroupBy(
                        t => new
                        {
                            HasSoulPatch = MaybeScalar<bool>(t.Gear, () => t.Gear.HasSoulPatch) ?? false,
                            Name = Maybe(t.Gear, () => t.Gear.Squad.Name)
                        })
                    .Select(g => new { g.Key.HasSoulPatch, Name = Maybe(g.Key.Name, () => g.Key.Name.ToLower()) }),
                elementSorter: e => (e.HasSoulPatch, e.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>()
                    .Select(
                        g => new
                        {
                            g.Nickname,
                            AssignedCityName = g.AssignedCity.Name,
                            CityOfBirthName = g.CityOfBirth.Name,
                            SquadName = g.Squad.Name
                        })
                    .GroupBy(x => x.CityOfBirthName)
                    .Select(g => g.Key),
                ss => ss.Set<Gear>()
                    .Select(
                        g => new
                        {
                            g.Nickname,
                            AssignedCityName = Maybe(g.AssignedCity, () => g.AssignedCity.Name),
                            CityOfBirthName = Maybe(g.CityOfBirth, () => g.CityOfBirth.Name),
                            SquadName = g.Squad.Name
                        })
                    .GroupBy(x => x.CityOfBirthName)
                    .Select(g => g.Key));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_with_aggregate_max_on_entity_type(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Gear>()
                        .GroupBy(g => g.CityOfBirthName)
                        .Select(g => new { g.Key, Aggregate = g.Max() })));
        }

        [ConditionalTheory(Skip = "issue #18492")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool isAsync)
        {
            var prm = (string)null;

            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().GroupBy(g => g.FullName.StartsWith(prm)).Select(g => g.Key));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_by_with_having_StartsWith_with_null_parameter_as_argument(bool isAsync)
        {
            var prm = (string)null;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().GroupBy(g => g.FullName).Where(g => g.Key.StartsWith(prm)).Select(g => g.Key),
                ss => ss.Set<Gear>().GroupBy(g => g.FullName).Where(g => false).Select(g => g.Key));
        }

        [ConditionalTheory(Skip = "issue #18492")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_StartsWith_with_null_parameter_as_argument(bool isAsync)
        {
            var prm = (string)null;

            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => g.FullName.StartsWith(prm)),
                ss => ss.Set<Gear>().Select(g => false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_parameter_is_not_null(bool isAsync)
        {
            var prm = (string)null;

            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Gear>().Select(g => prm != null),
                ss => ss.Set<Gear>().Select(g => false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_null_parameter_is_not_null(bool isAsync)
        {
            var prm = (string)null;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => prm != null),
                ss => ss.Set<Gear>().Where(g => false));
        }

        [ConditionalTheory(Skip = "issue #18492")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_StartsWith_with_null_parameter_as_argument(bool isAsync)
        {
            var prm = (string)null;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().OrderBy(g => g.FullName.StartsWith(prm)).ThenBy(g => g.Nickname),
                ss => ss.Set<Gear>().OrderBy(g => false).ThenBy(g => g.Nickname),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_with_enum_flags_parameter(bool isAsync)
        {
            MilitaryRank? rank = MilitaryRank.Private;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g.Rank & rank) == rank));

            rank = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g.Rank & rank) == rank));

            rank = MilitaryRank.Corporal;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g.Rank | rank) != rank));

            rank = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Gear>().Where(g => (g.Rank | rank) != rank));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Faction>()
                    .Where(f => f.Capital == ss.Set<Gear>().OrderBy(s => s.Nickname).FirstOrDefault().CityOfBirth));
        }

        protected async Task AssertTranslationFailed(Func<Task> testCode)
        {
            Assert.Contains(
                CoreStrings.TranslationFailed("").Substring(21),
                (await Assert.ThrowsAsync<InvalidOperationException>(testCode)).Message);
        }

        protected GearsOfWarContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
